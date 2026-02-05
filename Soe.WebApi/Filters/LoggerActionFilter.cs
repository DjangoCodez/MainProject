using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace Soe.WebApi.Filters
{
    public class LoggerActionFilter : ActionFilterAttribute
    {
        public LoggerActionFilter()
        {
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            base.OnActionExecuted(actionExecutedContext);

            if (!ShouldLogRequest)
            {
                return;
            }

            if (actionExecutedContext.Request.Method == HttpMethod.Get && actionExecutedContext.ActionContext?.Response?.Content != null)
            {
                try
                {
                    var logger = new LoggerManager(ParameterObject);
                    logger.CreatePersonalDataLogFireAndForget(GetResult(actionExecutedContext.ActionContext.Response.Content), LoggingGuid, TermGroup_PersonalDataActionType.Read, url: GetRequestUrl());
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    SysLogConnector.LogErrorString("OnActionExecutedAsync " + ex.ToString());
                }
            }
        }

        protected object GetResult(HttpContent content)
        {
            if (content is ObjectContent)
            {
                return ((ObjectContent)content).Value;
            }

            return content;
        }

        private bool ShouldLogRequest => HttpContext.Current.GetOwinContext().Environment.ContainsKey("Soe.ParameterObject");

        protected ParameterObject ParameterObject
        {
            get
            {
                if (HttpContext.Current.GetOwinContext().Environment.ContainsKey("Soe.ParameterObject"))
                {
                    return (ParameterObject)HttpContext.Current.GetOwinContext().Environment["Soe.ParameterObject"];
                }
                return null;
            }
        }
        private Guid LoggingGuid
        {
            get
            {
                if (HttpContext.Current.GetOwinContext().Environment.ContainsKey("Soe.LoggingGuid"))
                {
                    return (Guid)HttpContext.Current.GetOwinContext().Environment["Soe.LoggingGuid"];
                }
                return Guid.NewGuid();
            }
        }

        private String GetRequestUrl()
        {
            string url = String.Empty;
            if (HttpContext.Current?.Request?.Url != null)
                url = HttpContext.Current.Request.Url.ToString();
            return url;
        }
    }
}