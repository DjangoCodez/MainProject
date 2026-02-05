using RestSharp;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;

namespace SoftOne.Soe.Business.Util.LogCollector
{
    public static class LogCollector
    {
        public static Uri GetURI()
        {
            var url = CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Live ? "https://logcollectors1.azurewebsites.net/" : "https://logcollectors1Test.azurewebsites.net/";
            Uri uri = new Uri(url);
            return uri;
        }

        public static RestRequest CreateRequest(string resource, Method method, object obj = null, Dictionary<string, object> parameterDict = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            if (obj != null)
                request.AddJsonBody(obj);

            if (parameterDict != null)
            {
                foreach (var parameter in parameterDict)
                    request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);
            }

            request.AddHeader("Token", "e8d7bf57fd1b44a684689cfce813f783");
            return request;
        }

        public static void LogError(Exception ex, string messagePrefix = null)
        {
            LogModel model = new LogModel()
            {
                Source = "SOE",
                Message = Environment.MachineName + " " + (messagePrefix != null ? ($"{messagePrefix}. {ex?.Message}") : ex?.Message),
                Stack = ex?.StackTrace,
                LogLevel = LogLevel.Error
            };

#if DEBUG
            new Core.SysLogManager(null).AddSysLog(ex, log4net.Core.Level.Error);
#endif

            LogErrorModel(model);
        }

        public static void LogError(string message, int cachedMinutes = 2)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string machineName = Environment.MachineName;
            if (!message.Contains(machineName))
                message = machineName + " " + message;

            var hashed = CryptographyUtility.GetMd5Hash(message);
            string key = "LogError#" + hashed;
            var cacheMessage = MemoryCache.Default.Get(key);

            if (cacheMessage != null)
                return;

            CacheItem item = new CacheItem("LogError#" + hashed, hashed);
            MemoryCache.Default.Add(item, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(cachedMinutes) });

            LogModel model = new LogModel()
            {
                Source = "SOE",
                Message = message,
                Stack = "",
                LogLevel = LogLevel.Error
            };

            LogErrorModel(model);
        }

        public static void LogWithTrace(string info = "", LogLevel logLevel = LogLevel.Information, [CallerMemberName] string callerName = "")
        {
            try
            {
                var model = new LogModel()
                {
                    Source = $"SOE:{Environment.MachineName}",
                    Message = callerName + " " + info,
                    Stack = Environment.StackTrace,
                    LogLevel = logLevel
                };
                LogErrorModel(model);
            }
            catch
            {
                // Intentionally ignored, safe to continue
                // NOSONAR
            }
        }

        public static void LogInfo(string message, int cachedMinutes = 2)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string machineName = Environment.MachineName;
            if (!message.Contains(machineName))
                message = machineName + " " + message;

            var hashed = CryptographyUtility.GetMd5Hash(message);
            string key = "LogInfo#" + hashed;
            var cacheMessage = MemoryCache.Default.Get(key);

            if (cacheMessage != null)
                return;

            CacheItem item = new CacheItem("LogError#" + hashed, hashed);
            MemoryCache.Default.Add(item, new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(cachedMinutes) });

            LogModel model = new LogModel()
            {
                Source = "SOE",
                Message = message,
                Stack = "",
                LogLevel = LogLevel.Information
            };

            LogErrorModel(model);
        }

        public static void LogErrorModel(LogModel logModel)
        {
            try
            {
                new GoRestClient(GetURI()).Execute(CreateRequest("Log/LogModel", Method.Post, logModel, null));
            }
            catch
            {
            }
        }
    }

    public class LogModel
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string Stack { get; set; }
        public LogLevel LogLevel { get; set; }
    }

    public enum LogLevel
    {
        Unknown = 0,
        Information = 1,
        Warning = 2,
        Error = 3,
    }
}
