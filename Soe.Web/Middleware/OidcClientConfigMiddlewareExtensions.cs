using SoftOne.Soe.Web.Middleware;

namespace Owin
{
    public static class OidcClientConfigMiddlewareExtensions
    {
        public static IAppBuilder UseOidcClientConfig(this IAppBuilder app, OidcClientConfigOptions options = null) {

            app.Use<OidcClientConfigMiddleware>(options);
            return app;
        }
    }
}