using System.Configuration;
using System.ComponentModel;
using SoftOne.Soe.Business.Util;

namespace SoftOne.Soe.Web
{
	public class SoeConfigurationSettings : ConfigurationSection
	{
		public SoeConfigurationSettings(string logToEventViewer, string enablePrintEntityFrameworkInfo)
		{
			this.LogToEventViewer = logToEventViewer;
			this.EnablePrintEntityFrameworkInfo = enablePrintEntityFrameworkInfo;
		}

		public SoeConfigurationSettings()
		{}

		[ConfigurationProperty("logToEventViewer", DefaultValue = "false"), Description("Logs all errors to OS Event Viewer if true")]
		public string LogToEventViewer
		{
			get { return this["logToEventViewer"] as string; }
			set { this["logToEventViewer"] = value; }
		}

		[ConfigurationProperty("enablePrintEntityFrameworkInfo", DefaultValue = "false"), Description("Prints information about SOEs EDM model if true")]
		public string EnablePrintEntityFrameworkInfo
		{
			get { return this["enablePrintEntityFrameworkInfo"] as string; }
			set { this["enablePrintEntityFrameworkInfo"] = value; }
		}

        [ConfigurationProperty("printEntityFrameworkPath", DefaultValue = "c:\temp"), Description("Prints information about SOEs EDM model to given path if enablePrintEntityFrameworkInfo is true")]
		public string PrintEntityFrameworkPath
		{
            get
            {
                string path = this["printEntityFrameworkPath"] as string;
                if (!path.EndsWith(@"/") || !path.EndsWith(@"\"))
                    path += @"\";
                return path;
            }
			set { this["printEntityFrameworkPath"] = value; }
		}

        //[ConfigurationProperty("enableEncryptConnectionString", DefaultValue = "false"), Description("Encrypts connection string at Application startup (if not already encrypted) if true")]
        //public string EnableEncryptConnectionString
        //{
        //    get { return this["enableEncryptConnectionString"] as string; }
        //    set { this["enableEncryptConnectionString"] = value; }
        //}

        [ConfigurationProperty("releaseMode", DefaultValue = "false"), Description("Enable release mode")]
        public string ReleaseMode
        {
            get { return this["releaseMode"] as string; }
            set { this["releaseMode"] = value; }
        }

        [ConfigurationProperty("enableUserSession", DefaultValue = "false"), Description("Enable UserSessions to be logged")]
        public string EnableUserSession
        {
            get { return this["enableUserSession"] as string; }
            set { this["enableUserSession"] = value; }
        }

        [ConfigurationProperty("scriptVersion", DefaultValue = "1.0.0.0"), Description("controls cache termination")]
        public string ScriptVersion
        {
            get { return this["scriptVersion"] as string; }
            set { this["scriptVersion"] = value; }
        }

        [ConfigurationProperty("noOfSysNewsItems", DefaultValue = "10"), Description("No of items visible from SysNews feed")]
        public string NoOfSysNewsItems
        {
            get { return this["noOfSysNewsItems"] as string; }
            set { this["NoOfSysNewsItems"] = value; }
        }
	}
}
