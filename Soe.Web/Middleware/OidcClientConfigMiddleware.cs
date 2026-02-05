using Microsoft.Owin;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace SoftOne.Soe.Web.Middleware
{
    public class OidcClientConfigMiddleware
    {
        OidcClientConfigOptions _options;
        AppFunc _next;
        string _config;

        public OidcClientConfigMiddleware(AppFunc next, OidcClientConfigOptions options = null)
        {
            _next = next;

            if (options == null)
                options = new OidcClientConfigOptions();

            _options = options;

            string template;

            using (var stream = GetType().Assembly.GetManifestResourceStream("SoftOne.Soe.Web.Middleware.Resources.oidc-client-config-template.js"))
            using (var sr = new StreamReader(stream))
            {
                template = sr.ReadToEnd();
            }

            _config = template.Replace("{{authority}}", options.Authority)
                        .Replace("{{clientId}}", options.ClientId)
                        .Replace("{{redirectUri}}", options.RedirectUri)
                        .Replace("{{silentTokenRenewalRedirectPath}}", options.SilentTokenRenewalRedirectPath)
                        .Replace("{{scopes}}", options.Scopes);
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var ctx = new OwinContext(environment);

            if (ctx.Request.Path == _options.EndpointPath)
            {
                ctx.Response.ContentType = "text/javascript";
                await ctx.Response.WriteAsync(_config.Replace("{{postLogoutRedirectUri}}", _options.GetPostLogoutRedirectUri(ctx)));
                return;
            }

            await _next(environment);
        }
    }
}