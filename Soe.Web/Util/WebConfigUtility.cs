using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace SoftOne.Soe.Web.Util
{
    public class WebConfigUtility
    {
        /// <summary>
        /// Opens the web.config file
        /// </summary>
        /// <returns>The web-application configuration file</returns>
        public static Configuration GetWebConfig()
        {
            Configuration config = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
                config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            return config;
        }

        public static Configuration GetLog4netConfig()
        {
            Configuration config = null;
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
                config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath + "Log4net.config");
            return config;
        }

        /// <summary>
        /// Returns the specified ConfigurationSection object if it not is locked
        /// </summary>
        /// <param name="sectionName">The section to get</param>
        public static ConfigurationSection GetWebConfigSection(string sectionName)
        {
            Configuration config = GetWebConfig();
            return GetSection(config, sectionName);
        }

        /// <summary>
        /// Returns the specified ConfigurationSection object if it not is locked
        /// </summary>
        /// <param name="sectionName">The section to get</param>
        public static ConfigurationSection GetLog4netSection(string sectionName)
        {
            Configuration config = GetLog4netConfig();
            return GetSection(config, sectionName);
        }

        /// <summary>
        /// Returns the specified ConfigurationSection object if it not is locked
        /// </summary>
        /// <param name="config">The Configuration file to get section from</param>
        /// <param name="sectionName">The section to get</param>
        public static ConfigurationSection GetSection(Configuration config, string sectionName)
        {
            if (config != null)
            {
                ConfigurationSection section = config.GetSection(sectionName);
                if (section != null && !section.SectionInformation.IsLocked)
                {
                    return section;
                }
            }

            return null;
        }

        /// <summary>
        /// Protects (encrypt) a configuration section
        /// </summary>
        /// <param name="sectionName">The section to protect</param>
        /// <returns>True if the section is protected, otherwise false</returns>
        public static bool ProtectWebConfigSection(string sectionName)
        {
            Configuration config = GetWebConfig();
            return ProtectSection(config, sectionName);
        }

        /// <summary>
        /// Protects (encrypt) a configuration section
        /// </summary>
        /// <param name="config">The Configuration file to protect section in</param>
        /// <param name="sectionName">The section to protect</param>
        /// <returns>True if the section is protected, otherwise false</returns>
        public static bool ProtectSection(Configuration config, string sectionName)
        {
            if (config != null)
            {
                ConfigurationSection section = GetSection(config, sectionName);
                if (section != null)
                {
                    if (section.SectionInformation.IsProtected)
                    {
                        return true;
                    }
                    else
                    {
                        section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                        config.Save();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Unprotects (decrypt) a web configuration section
        /// </summary>
        /// <param name="sectionName">The section to unprotect</param>
        /// <returns>True if the section is unprotected, otherwise false</returns>
        public static bool UnprotectWebConfigSection(string sectionName)
        {
            Configuration config = GetWebConfig();
            return UnprotectSection(config, sectionName);
        }

        /// <summary>
        /// Unprotects (decrypt) a configuration section
        /// </summary>
        /// <param name="config">The Configuration file to unprotect section in</param>
        /// <param name="sectionName">The section to unprotect</param>
        /// <returns>True if the section is unprotected, otherwise false</returns>
        public static bool UnprotectSection(Configuration config, string sectionName)
        {
            if (config != null)
            {
                ConfigurationSection section = GetSection(config, sectionName);
                if (section != null)
                {
                    if (!section.SectionInformation.IsProtected)
                    {
                        return true;
                    }
                    else
                    {
                        section.SectionInformation.UnprotectSection();
                        config.Save();
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetConnectionString(string connectionString)
        {
            ConnectionStringSettings conStrSetting = ConfigurationManager.ConnectionStrings[connectionString];
            if (conStrSetting != null)
                return conStrSetting.ConnectionString;
            return String.Empty;
        }

        public static string GetSoeConfigurationSetting(string soeConfigurationSetting)
        {
            string value = "";
            SoeConfigurationSettings section = GetWebConfigSection(Constants.SOE_CONFIGURATION_SETTINGS) as SoeConfigurationSettings;
            if (section != null)
            {
                if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_LOGTOEVENTVIEWER)
                    value = section.LogToEventViewer;
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_ENABLEPRINTENTITYFRAMEWORKINFO)
                    value = section.EnablePrintEntityFrameworkInfo;
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_PRINTENTITYFRAMEWORKPATH)
                    value = section.PrintEntityFrameworkPath;
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_ENABLEENCRYPTCONNECTIONSTRING)
                    value = section.ReleaseMode;//section.EnableEncryptConnectionString;
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_RELEASEMODE)
                {
                    if (CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Live || CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Beta)
                        value = "true";
                    else
                        value = "false";
                }
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_ENABLEUSERSESSION)
                    value = section.EnableUserSession;
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_SCRIPTVERSION)
                    value = section.ScriptVersion;
                else if (soeConfigurationSetting == Constants.SOE_CONFIGURATION_SETTING_NOOFSYSNEWSITEMS)
                    value = section.NoOfSysNewsItems;
            }

            return value;
        }
    }
}
