using Microsoft.Owin;
using Owin;
using Soe.WebApi.Middlewares;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Security;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Web.Middleware;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.Cache;
using System;
using System.Net;
using System.Web;
using System.Web.Http;

[assembly: OwinStartup("Owin.WebApi", typeof(Soe.WebApi.Startup))]
namespace Soe.WebApi
{
    public class Startup
    {
        protected const string THREAD = "WebAPI";

        public void Configuration(IAppBuilder app)
        {
            ConfigurationSetupUtil.Init();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogInfo($"Api start from machine {Environment.MachineName} from  {HttpContext.Current.Server.MapPath("~")}");
            app.UseForwardedHeaders();
            app.Use<CorrelationIdMiddleware>();

            app.ConfigureAuthentication(SOESysEntities.GetConnectionString(), KeyVaultSettingsHelper.GetKeyVaultSettings());
            //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            if (HttpContext.Current?.Server != null)
                ConfigSettings.SetCurrentDirectory(HttpContext.Current.Server.MapPath("~"));

            var config = new HttpConfiguration();
            // Disables the default IIS authentication since we are now using token based for the API
            config.SuppressHostPrincipal();

            app.Use((ctx, next) =>
            {
                //Will only load the cache if it doesnt exist
                if (!TimeOutOwinHelper.TryToSlideTimeoutForward(ctx))
                    ctx.Authentication.SignOut("Cookies");

                // Set up logging correlation ID for LoggerManager
                ctx.Environment.Add("Soe.LoggingGuid", Guid.NewGuid());
                return next();
            });

            app.UseSoeParameters();
            app.Use<LoggerMiddleware>();
            app.UseWebApi(config);


            SoeCache.RedisConnectionString = CompDbCache.Instance.RedisCacheConnectionString;
            SOEAuthorizeAttribute.SigningCertificateName = SoftOneIdUtil.GetCertificateName(CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test);
        }
    }
}