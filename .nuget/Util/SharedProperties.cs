using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.EdiAdmin.Business.Util
{
    public static class SharedProperties
    {
        private static Dictionary<string, string> drEdiSettingsOld;

        public static ConcurrentDictionary<ApplicationSettingType, string> DrEdiSettings;
        public static Dictionary<string, string> DrEdiSettingsOld
        {
            get
            {
                if (drEdiSettingsOld == null)
                {
                    drEdiSettingsOld = new Dictionary<string, string>();
                    foreach (var item in DrEdiSettings)
                    {
                        drEdiSettingsOld.Add(item.Key.ToString(), item.Value);
                    }
                }

                return drEdiSettingsOld;
            }
        }
        public static DataSet DsStandardMall = new DataSet();
        
        public static bool DoNotBackupFilesToDics = false;
        public static bool DoNotCheckDuplicates { get; set; }

        public static void LogError(Exception ex, string errorMsg, params object[] arg)
        {
            string msg = ex.GetInnerExceptionMessages().JoinToString(Environment.NewLine + "Inner: ");
            LogError(string.Concat(errorMsg, Environment.NewLine, "Exception: ", msg), arg);
        }

        public static void LogError(string errorMsg, params object[] arg)
        {
            string stackTrace = SoftOne.Soe.Util.Exceptions.SoeException.GetStackTrace(10);
            Console.Error.WriteLine(string.Concat(errorMsg, Environment.NewLine, "Stacktrace: ", stackTrace), arg);
        }
    }
}
