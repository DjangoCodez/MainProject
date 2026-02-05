using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web;

namespace SoftOne.Soe.Business.Util
{
    public static class UseForwardedHeadersExtension
    {
        private const string ForwardedHeadersAdded = "ForwardedHeadersAdded";

        /// https://stackoverflow.com/questions/66382772/net-framework-equivalent-of-iapplicationbuilder-useforwardedheaders
        /// <summary>
        /// Checks for the presence of <c>X-Forwarded-For</c> and <c>X-Forwarded-Proto</c> headers, and if present updates the properties of the request with those headers' details.
        /// </summary>
        /// <remarks>
        /// This extension method is needed for operating our website on an HTTP connection behind a proxy which handles SSL hand-off. Such a proxy adds the <c>X-Forwarded-For</c>
        /// and <c>X-Forwarded-Proto</c> headers to indicate the nature of the client's connection.
        /// </remarks>
        public static IAppBuilder UseForwardedHeaders(this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            // No need to add more than one instance of this middleware to the pipeline.
            if (!app.Properties.ContainsKey(ForwardedHeadersAdded))
            {
                app.Properties[ForwardedHeadersAdded] = true;

                app.Use(async (context, next) =>
                {
                    try
                    {
                        var request = context.Request;

                        if (string.Equals(request.Headers["X-Forwarded-Proto"], Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                        {
                            request.Scheme = Uri.UriSchemeHttps;

                            // Assuming you want to use Microsoft.AspNetCore.Http.HttpContext
                            var httpContext = context;

                            httpContext.Request.Scheme = Uri.UriSchemeHttps;
                            httpContext.Request.Host = new HostString(request.Headers.ContainsKey("X-Forwarded-Host")
                                ? request.Headers["X-Forwarded-Host"]
                                : request.Host.Value);
                            //    httpContext.Request.PathBase = new PathString("");  // Update as needed
                        }


                        await next.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogCollector.LogCollector.LogError(ex);
                        // Log exception for diagnostics
                        // Decide what to do: ignore, re-throw, return a specific response, etc.
                        // For example, you could return a 408 Timeout status
                        //context.Response.Headers.Add("Connection", ex.ToString());
                        context.Response.StatusCode = 408;
                        context.Response.ReasonPhrase = "Request Timeout";
                        return;
                    }
                });
            }

            return app;
        }
    }
}