using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;

namespace Soe.WebApi.Internal.Filters
{
    public class LoggerActionFilter : ActionFilterAttribute
    {
        public LoggerActionFilter()
        {
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            base.OnActionExecuted(actionExecutedContext);

            try
            {
                var model = GetResult(actionExecutedContext.ActionContext?.Response?.Content);
                if (CleanerUtil.IsValidForCleanup(model))
                    CleanerUtil.CleanObject(model);
            }
            catch
            {
                // Make sure request doesn't die
                // TODO: Handle in some way to make sure logging doesn't stop
            }

            if (!ShouldLogRequest)
            {
                return;
            }

            if (actionExecutedContext.Request.Method == HttpMethod.Get)
            {
                var logger = new LoggerManager(ParameterObject);
                await logger.CreatePersonalDataLog(GetResult(actionExecutedContext.ActionContext.Response.Content), LoggingGuid, TermGroup_PersonalDataActionType.Read, url: GetRequestUrl());
            }
        }

        private object GetResult(HttpContent content)
        {
            if (content != null && content is ObjectContent)
            {
                return ((ObjectContent)content).Value;
            }

            return content;
        }

        private bool ShouldLogRequest => HttpContext.Current.GetOwinContext().Environment.ContainsKey("Soe.ParameterObject");

        private ParameterObject ParameterObject
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
                HttpContext.Current.Request.Url.ToString();
            return url;
        }
    }
}