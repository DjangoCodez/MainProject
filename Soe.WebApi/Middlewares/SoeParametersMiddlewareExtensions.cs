using Soe.WebApi.Middlewares;

namespace Owin
{
    public static class SoeParametersMiddlewareExtensions
    {
        public static IAppBuilder UseSoeParameters(this IAppBuilder app)
        {

            app.Use<SoeParametersMiddleware>();
            return app;
        }
    }
}