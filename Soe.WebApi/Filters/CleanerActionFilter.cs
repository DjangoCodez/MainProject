using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Soe.WebApi.Filters
{
    public class CleanerActionFilter : LoggerActionFilter
    {
        public CleanerActionFilter()
        {
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            base.OnActionExecuted(actionExecutedContext);
            object model = null;

            if (ParameterObject != null && !ParameterObject.SupportUserId.HasValue && actionExecutedContext.ActionContext?.Response?.Content != null)
            {
                try
                {
                    model = GetResult(actionExecutedContext.ActionContext.Response.Content);
                    if (CleanerUtil.IsValidForCleanup(model))
                        CleanerUtil.CleanObject(model);
                }
                catch
                {
                    // Make sure request doesn't die
                    // TODO: Handle in some way to make sure logging doesn't stop
                }
            }

            await Task.CompletedTask;
        }
    }
}