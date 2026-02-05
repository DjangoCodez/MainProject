using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Soe.Web;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Shared.Cache;
using SoftOne.Soe.Web.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace SoftOne.Soe.Web
{
    public class Global : HttpApplication
    {
        #region Events

        protected void Application_Start(object sender, EventArgs e)
        {
            // Enable TLS 1.2 to enable communication with kestrel
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Setup();
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {
            bool value = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_ENABLEENCRYPTCONNECTIONSTRING));
            if (value)
            {
                //Encrypt connectionstrings
                ProtectWebConfigSection("connectionStrings");
            }
            else
            {
                //Decrypt connectionstrings
                UnprotectWebConfigSection("connectionStrings");
            }
            SettingManager sm = new SettingManager(null);
            bool relaseMode = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE));
            sm.UpdateInsertBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.ReleaseMode, relaseMode, 0, 0, 0);
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_PreRequestHandlerExecute(Object sender, EventArgs e)
        {

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

            if (HttpContext.Current.Request.Url.Scheme.ToLower() == "http")
            {
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.Url.AbsoluteUri.Replace("http://", "https://"));
            }


            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            LogToEventViewer();
            //if we are already on ErrorPage.aspx, do not redirect to it again
            if (HttpContext.Current.Request.Url.AbsolutePath.ToLower().Contains("errorpage.aspx"))
                return;
            Response.Redirect("~/ErrorPage.aspx");
        }



        #endregion

        #region Help-methods

        private void Setup()
        {
            SetupEnv();
            Parallel.Invoke(
                            () => { SetupCache(); },
                            () => { SetupLog4net(); },
                            () => { SetupBundling(); }
                            );
        }

        private void SetupEnv()
        {
            //Setup EF profiler
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
            ConfigurationSetupUtil.Init();
            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogInfo($"Web start from machine {Environment.MachineName} from {HttpContext.Current.Server.MapPath("~")}");
            if (HttpContext.Current?.Server != null)
                ConfigSettings.SetCurrentDirectory(HttpContext.Current.Server.MapPath("~"));

            #region Check temp folders

            //Save ApplicationDirectory in database
            SettingManager sm = new SettingManager(null);
            sm.UpdateInsertStringSetting(SettingMainType.Application, (int)ApplicationSettingType.AppDirectory, HttpContext.Current.Server.MapPath("~").ToString(), 0, 0, 0);

            //bool relaseMode = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE));
            //result = sm.UpdateInsertBoolSetting(SettingMainType.Application, (int)ApplicationSettingType.ReleaseMode, relaseMode, 0, 0);

            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_EMAIL_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_EMAIL_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_PAYMENT_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_PAYMENT_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_EXPORT_SALARY_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_EXCEL_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_EXCEL_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_REPORT_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_FINVOICE_REPORT_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_IMPORT_EDI_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_IMPORT_EDI_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_LOGO_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_LOGO_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_PRICELIST_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_PRICELIST_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_REPORT_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_SIE_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_SIE_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_UPLOADEDFILES_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_UPLOADEDFILES_PHYSICAL);
            if (!Directory.Exists(ConfigSettings.SOE_SERVER_DIR_TEMP_AUTOMASTER_PHYSICAL))
                Directory.CreateDirectory(ConfigSettings.SOE_SERVER_DIR_TEMP_AUTOMASTER_PHYSICAL);

            #endregion
        }

        private void SetupCache()
        {
            var cache = CompDbCache.Instance;
            string redisConn = null;

            Parallel.Invoke(
                () => { var _ = cache.Forms; },
                () => { var _ = cache.Fields; },
                () => { redisConn = cache.RedisCacheConnectionString; }
            );

            HttpContext.Current.Cache.Insert(
                Constants.CACHE_OUTPUT_CACHE_DEPENDENCY,
                DateTime.Now,
                null,
                DateTime.MaxValue,
                TimeSpan.Zero,
                CacheItemPriority.NotRemovable,
                null
            );

            SoeCache.RedisConnectionString = redisConn;
        }

        private void SetupLog4net()
        {
            //Configure log4net to use and monitor the specified .config file
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Server.MapPath("~/Log4net.config")));

            //Clear all Log4net properties
            SysLogManager.ClearLog4netProperties();

            //Get ConnectionString from web.config
            Hierarchy hierarchy = LogManager.GetRepository() as Hierarchy;
            if (hierarchy != null)
            {
                AdoNetAppender adoNetAppender = hierarchy.Root.GetAppender("ADONetAppender") as AdoNetAppender;
                if (adoNetAppender != null)
                {
                    adoNetAppender.ConnectionString = SOESysEntities.GetConnectionString();
                    adoNetAppender.ActivateOptions();
                }
            }
        }

        private void SetupBundling()
        {
            BundleConfig.RegisterBundles();
        }

        private void LogToEventViewer()
        {
            bool value = StringUtility.GetBool(WebConfigUtility.GetSoeConfigurationSetting(Constants.SOE_CONFIGURATION_SETTING_LOGTOEVENTVIEWER));
            if (value)
            {
                string url = "";
                try
                {
                    url = Request.Url.ToString();
                }
                catch (HttpException)
                {
                    url = "[Url not available]";
                }

                //Log to OS Event Viewer
                Exception error = Server.GetLastError().GetBaseException();
                string message = "Error Caught in Application_Error event\n" +
                                    "Error in: " + url +
                                    "\nError Message:" + error.Message.ToString() +
                                    "\nStack Trace:" + error.StackTrace.ToString();
                try
                {
                    //Check if the EventLog exists
                    if (!EventLog.SourceExists(Constants.APPLICATION_NAME))
                    {
                        //Create EventLog
                        EventLog.CreateEventSource(Constants.APPLICATION_NAME, Constants.APPLICATION_NAME);
                    }

                    EventLog.WriteEntry(Constants.APPLICATION_NAME, message, EventLogEntryType.Error);
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }
        }

        private void ProtectWebConfigSection(string sectionName)
        {
            WebConfigUtility.ProtectWebConfigSection(sectionName);
        }

        private void UnprotectWebConfigSection(string sectionName)
        {
            WebConfigUtility.UnprotectWebConfigSection(sectionName);
        }

        #endregion
    }
}
