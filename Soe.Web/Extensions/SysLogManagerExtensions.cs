using SoftOne.Soe.Business.Core;
using System;
using System.Web;

namespace SoftOne.Soe.Web
{
    public static class SysLogManagerExtensions
    {
        public static void LogInfo(this SysLogManager slm, string message)
        {
            slm.AddSysLogInfoMessage(Environment.MachineName, "", message);
        }
        public static void LogInfo<T>(this SysLogManager slm, Exception ex)
        {
            var log = log4net.LogManager.GetLogger(typeof(T));
            slm.AddSysLog(ex, log4net.Core.Level.Info, log);
        }
        public static void LogWarning<T>(this SysLogManager slm, Exception ex)
        {
            var log = log4net.LogManager.GetLogger(typeof (T));
            slm.AddSysLog(ex, log4net.Core.Level.Warn, log);
        }
        public static void LogError<T>(this SysLogManager slm, Exception ex)
        {
            var log = log4net.LogManager.GetLogger(typeof(T));
            slm.AddSysLog(ex, log4net.Core.Level.Error, log);
        }
    }
}