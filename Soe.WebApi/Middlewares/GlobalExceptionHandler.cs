using SoftOne.Soe.Business.Util.LogCollector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace Soe.WebApi.Middlewares
{
    public class GlobalExceptionHandler : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            // Log the exception (optional)
            LogError(context.Exception);

            // Create a custom response for the error
            var response = context.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                new
                {
                    Message = "An unexpected error occurred. Please try again later or contact support.",
#if DEBUG
                    Error = context.Exception.Message // Include in development only
#endif
                });

            context.Response = response;
        }

        /// <summary>
        /// Log error details to a text file or any other logging mechanism.
        /// </summary>
        private void LogError(Exception ex)
        {
            LogCollector.LogError(ex, "From GlobalExceptionHandler.cs");
        }
    }
}