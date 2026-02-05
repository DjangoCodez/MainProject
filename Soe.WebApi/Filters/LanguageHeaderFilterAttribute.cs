using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http.Filters;

namespace Soe.WebApi.Filters
{
    public class LanguageHeaderFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            base.OnActionExecuting(actionContext);

            var y = actionContext.Request.Headers.AcceptLanguage.OrderBy(x => x.Quality).FirstOrDefault();

            string language = string.Empty;

            if (y != null)
                language = y.Value;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(language);
        }
    }
}