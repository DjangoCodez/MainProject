using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Jwt;
using Owin;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Security;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Web.Middleware;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Web.Middleware;
using SoftOne.Soe.Web.Security;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup("Owin.Web", typeof(SoftOne.Soe.Web.Startup))]
namespace SoftOne.Soe.Web
{
    public class Startup
    {
        private static readonly object lockObject = new object();

        public static OidcClientConfigOptions oidcClientConfigOption { get; set; }
        public static JwtBearerAuthenticationOptions jwtBearerAuthenticationOptions { get; set; }
        public static CookieAuthenticationOptions cookieAuthenticationOptions { get; set; }
        public void Configuration(IAppBuilder app)
        {
            app.UseForwardedHeaders();
            lock (lockObject)
            {
                //ConfigureJwtBearerAuthentication(app);
               //ConfigureCookieAuthentication(app);

                app.ConfigureAuthentication(SOESysEntities.GetConnectionString(), KeyVaultSettingsHelper.GetKeyVaultSettings());

            }

            app.Use<CorrelationIdMiddleware>();
            app.Use((ctx, next) =>
            {
                var path = ctx.Request.Path.ToString();
                if (!ctx.Authentication.User.Identity.IsAuthenticated && (path == "/" || path.EndsWith(".aspx")) &&
                    !path.Equals("/login.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/default.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/cssjs/scripts.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/cssjs/style.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/ajax/getSysTerm.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/SoftOneStatus.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/Unauthorized.aspx", StringComparison.OrdinalIgnoreCase) &&
                    !path.Equals("/silentsignin.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Response.Redirect("/login.aspx");
                    //ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return Task.FromResult(0);
                }
                return next();
            });

        //    ConfigureOidcAuthentication(app);

        }
        private void ConfigureCookieAuthentication(IAppBuilder app)
        {
            if (cookieAuthenticationOptions == null)
            {
                cookieAuthenticationOptions = new CookieAuthenticationOptions
                {
                    AuthenticationType = "Cookies",
                    AuthenticationMode = AuthenticationMode.Active,
                    CookieHttpOnly = true,
                    SlidingExpiration = true,
                    ExpireTimeSpan = TimeSpan.FromMinutes(240),
                    LoginPath = new PathString("/login.aspx")
                };
                app.UseCookieAuthentication(cookieAuthenticationOptions);
            }
        }

        private void ConfigureOidcAuthentication(IAppBuilder app)
        {
            if (oidcClientConfigOption == null)
            {
                oidcClientConfigOption = new Middleware.OidcClientConfigOptions
                {
                    Authority = SoftOneIdConnector.GetUri().EnsureTrailingSlash(),
                    ClientId = "Testing_Cookie",
                    //  RedirectUri = GetRedirectUri(),
                    GetPostLogoutRedirectUri = GetPostLogoutRedirect,                   
                    Scopes = "openid profile SoftOne"
            };
                app.UseOidcClientConfig(oidcClientConfigOption);
            }
        }

        private void ConfigureJwtBearerAuthentication(IAppBuilder app)
        {
            if (jwtBearerAuthenticationOptions == null)
            {
                jwtBearerAuthenticationOptions = new JwtBearerAuthenticationOptions()
                {
                    AuthenticationMode = AuthenticationMode.Passive,
                    AuthenticationType = "JWT",
                    AllowedAudiences = new[] { SoftOneIdUtil.Scopes },
                    IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[] {
                    new OpenIdConnectSecurityKeyProvider(SoftOneIdConnector.GetUri().RemoveTrailingSlash() + "/.well-known/openid-configuration") }
                };
                app.UseJwtBearerAuthentication(jwtBearerAuthenticationOptions);
            }
        }

        private string GetPostLogoutRedirect(IOwinContext ctx)
        {
            return ctx.Request.Uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped) + "/";
        }
    }

}
