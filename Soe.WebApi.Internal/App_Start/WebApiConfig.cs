using Soe.Api.Internal;
using Soe.WebApi.Internal.Filters;
using Soe.WebApi.Internal.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Soe.Api.Internal
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            NinjectConfig.Configure();

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Filters.Add(new LoggerActionFilter());
            config.Filters.Add(new GlobalExceptionHandler());
            // Per-request EF scope handler
            config.MessageHandlers.Add(new SoftOne.Soe.Business.Util.RequestScopeHandler());
        }
    }
}
