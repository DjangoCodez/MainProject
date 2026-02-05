using System;
using System.Web;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business;

namespace SoftOne.Soe.Web
{
    public class SoeHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.Error += new System.EventHandler(OnError);
            context.BeginRequest += new EventHandler(OnBeginRequest);
            context.EndRequest += new EventHandler(OnEndRequest);
        }

        public void Dispose()
        {
        }

        public void OnError(object obj, EventArgs args)
        {
            //Log Error
            SysLogManager slm = new SysLogManager(null);
            HttpContext ctx = HttpContext.Current;
            Exception ex = ctx.Server.GetLastError();
            if (ex != null)
            {
                var setAsInfo = ex.ToString().Contains("Server cannot append header after HTTP headers have been sent");
                slm.AddSysLog(ex, setAsInfo ? log4net.Core.Level.Info : log4net.Core.Level.Error);
            }

            //Cancel the bubbling up of the error
            //ctx.Server.ClearError();
        }

        public void OnBeginRequest(object sender, EventArgs e)
        {
            //Init EF ObjectContext for Sys and Comp at each AspNet request (filter certain requests)
            string path = HttpContext.Current?.Request?.Path;
            if (!string.IsNullOrEmpty(path) && HttpContext.Current != null && !path.StartsWith("/cssjs/") && !path.StartsWith("/img/") && !path.ToLower().Contains("getSysTerm.aspx") && path.ToLower().IndexOf(".aspx") >= 0)
            {
                SysEntitiesProvider.CreateOnBeginRequest();
                CompEntitiesProvider.CreateOnBeginRequest();
            }
        }

        public void OnEndRequest(object sender, EventArgs e)
        {
            //Dispose EF ObjectContext for Sys and Comp at each AspNet request (filter certain requests)
            string path = HttpContext.Current?.Request?.Path;
            if (!string.IsNullOrEmpty(path) && HttpContext.Current != null && !path.StartsWith("/cssjs/") && !path.StartsWith("/img/") && !path.ToLower().Contains("getSysTerm.aspx") && path.ToLower().IndexOf(".aspx") >= 0)
            {
                SysEntitiesProvider.DisposeOnRequestEnd();
                CompEntitiesProvider.DisposeOnRequestEnd();
            }
        }
    }
}
