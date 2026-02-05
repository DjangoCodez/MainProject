using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Soe.Api.Internal.Middlewares;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Web.Middleware;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.Cache;
using System;
using System.Linq;
using System.Web.Http;

[assembly: OwinStartup("Owin.WebApiInternal", typeof(Soe.Api.Internal.Startup))]

namespace Soe.Api.Internal
{
    public class Startup
    {
        protected const string THREAD = "API.System";

        public void Configuration(IAppBuilder app)
        {
            ConfigurationSetupUtil.Init();

            SOEAuthorizeAttribute.SigningCertificateName = SoftOneIdUtil.GetCertificateName(CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test);
            app.UseForwardedHeaders();
            app.Use<CorrelationIdMiddleware>();
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            var config = new HttpConfiguration();
            // Disables the default IIS authentication since we are now using token based for the API
            config.SuppressHostPrincipal();

            app.Use((ctx, next) =>
            {
                if (ctx?.Request?.Uri?.AbsoluteUri != null && ctx.Request.Uri.AbsoluteUri.ToLower().Contains("token="))
                {
                    var token = ctx.Request.Headers.Get("SOEAuthToken");

                    if (!string.IsNullOrEmpty(token))
                    {
                        var qs = System.Web.HttpUtility.ParseQueryString(ctx.Request.Uri.Query);

                        // Read a parameter from the QueryString object.
                        string value1 = qs["token"];

                        if (string.IsNullOrEmpty(value1))
                            // Write a value into the QueryString object.
                            qs["token"] = token;
                    }
                }

                if (ctx != null && !ctx.Request.Path.ToString().StartsWith("/translation/", StringComparison.OrdinalIgnoreCase))
                {
                    WebApiInternalParamObject webApiInternalParamObject = new WebApiInternalParamObject();

                    if (ctx.Request.Headers.GetValues("SOEAuthToken") != null && ctx.Request.Headers.GetValues("SOEAuthToken").Any())
                        webApiInternalParamObject.Token = ctx.Request.Headers.GetValues("SOEAuthToken").FirstOrDefault();
                    else
                        webApiInternalParamObject.Token = string.Empty;

                    ctx.Environment["Soe.WebApiInternalParamObject"] = webApiInternalParamObject;
                }

                SoeCache.RedisConnectionString = CompDbCache.Instance.RedisCacheConnectionString;
                // Set up logging correlation ID for LoggerManager
                if (ctx == null)
                    ctx.Environment.Add("Soe.LoggingGuid", Guid.NewGuid());
                return next();
            });

            app.UseWebApi(config);
        }
    }
}