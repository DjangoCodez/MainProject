using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using System;
using System.Configuration;
using System.Web;
using System.Web.Http;

namespace Soe.Api.Internal
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ConfigurationSetupUtil.Init();
            if (HttpContext.Current?.Server != null)
                ConfigSettings.SetCurrentDirectory(HttpContext.Current.Server.MapPath("~"));
            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogInfo($"ApiInternal start from machine {Environment.MachineName} from  {HttpContext.Current.Server.MapPath("~")}");
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;

            var formatters = GlobalConfiguration.Configuration.Formatters;
            var jsonFormatter = formatters.JsonFormatter;
            var settings = jsonFormatter.SerializerSettings;
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Converters.Add(new ReportDataSelectionDTOJsonConverter());
        }
    }
}
