using SoftOne.Soe.Business;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Web;

namespace Soe.WebServices.External
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            ConfigurationSetupUtil.Init();
            SysServiceManager ssm = new SysServiceManager(null);
            if (HttpContext.Current?.Server != null)
                ConfigSettings.SetCurrentDirectory(HttpContext.Current.Server.MapPath("~"));
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                LogCollector.LogError(ex, "From wsx Application_Error");
                Server.ClearError();
            }
            var url = ConfigurationSetupUtil.GetCurrentUrl();
            Response.Redirect(new Uri(url).EnsureTrailingSlash() + "ErrorPage.aspx");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.Path.Equals("/Pulse", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write(PulseManager.Pulse());
                HttpContext.Current.Response.End();
                return;
            }

            // Initialize EF contexts per request
            SysEntitiesProvider.CreateOnBeginRequest();
            CompEntitiesProvider.CreateOnBeginRequest();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            // Dispose EF contexts per request
            SysEntitiesProvider.DisposeOnRequestEnd();
            CompEntitiesProvider.DisposeOnRequestEnd();
        }
    }
}