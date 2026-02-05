using Soe.WebApi.Filters;
using Soe.WebApi.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Soe.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Filters.Add(new LoggerActionFilter());
            config.Filters.Add(new CleanerActionFilter());
            config.Filters.Add(new GlobalExceptionHandler());

            // Per-request EF scope handler
            config.MessageHandlers.Add(new SoftOne.Soe.Business.Util.RequestScopeHandler());

        }
    }
}
