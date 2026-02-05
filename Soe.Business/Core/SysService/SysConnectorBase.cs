using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysConnectorBase : ManagerBase
    {
        protected static readonly string preselectedUri;
        protected static readonly string token;
        protected static readonly Random random;

        #region Constructor

        public SysConnectorBase(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        static SysConnectorBase()
        {
            random = new Random();

            // 1 in 50 chance to try renewing when fetching preselected URI
            preselectedUri = FixSysServiceUrl(ConfigurationSetupUtil.GetSysServiceUrl(random.Next(0, 50) == 25));

#if DEBUG
            // Hardcoded for local development use
            // selectedUri = "http://localhost:24978";
#endif

            // If initial preselectedUri is null or empty, retry up to 2 times with delay
            if (string.IsNullOrEmpty(preselectedUri))
            {
                Thread.Sleep(1000);
                preselectedUri = FixSysServiceUrl(ConfigurationSetupUtil.GetSysServiceUrl());

                if (string.IsNullOrEmpty(preselectedUri))
                {
                    Thread.Sleep(1000);
                    preselectedUri = FixSysServiceUrl(ConfigurationSetupUtil.GetSysServiceUrl());
                }
            }

            // Hardcoded token; consider generating securely in production
            token = "e8d7bf57fd1b44a684689cfce813f783";
        }

        protected static string selectedUri
        {
            get
            {
                try
                {
                    const string key = "selectedSysUriFromBase";
                    const string backupKey = "Backup" + key;

                    // Try to get the URI from in-memory cache first
                    var fromCache = BusinessMemoryCache<string>.Get(key);
                    if (!string.IsNullOrEmpty(fromCache))
                        return fromCache;

                    // Try to get a fresh URI from config/service discovery
                    var newSelectedUri = FixSysServiceUrl(ConfigurationSetupUtil.GetSysServiceUrl(tryToRenew: true));

                    if (!string.IsNullOrEmpty(newSelectedUri))
                    {
                        BusinessMemoryCache<string>.Set(key, newSelectedUri, 60 * 5); // 5 min memory cache
                        EvoDistributionCacheConnector.UpsertCachedValue(backupKey, newSelectedUri, TimeSpan.FromHours(5)); // 5 hour distributed cache
                        return newSelectedUri;
                    }

                    // Try to retrieve a previously saved URI from distributed backup cache
                    newSelectedUri = FixSysServiceUrl(EvoDistributionCacheConnector.GetCachedValue<string>(backupKey));
                    if (!string.IsNullOrEmpty(newSelectedUri))
                    {
                        BusinessMemoryCache<string>.Set(key, newSelectedUri, 60); // Short cache to retry soon
                        return newSelectedUri;
                    }

                    // Fall back to static preselected URI
                    if (!string.IsNullOrEmpty(preselectedUri))
                    {
                        LogCollector.LogWithTrace($"Unable to fetch a sysServerUrl, fallback to preselectedUri  {preselectedUri}", LogLevel.Warning);
                        BusinessMemoryCache<string>.Set(key, preselectedUri, 60);
                        return preselectedUri;
                    }

                    LogCollector.LogError($"selectedUri is null in SysConnectorBase");

                }
                catch (Exception ex)
                {
                    LogCollector.LogError(ex, $"preselectedUri {preselectedUri}");
                }
                return null;
            }
        }

        /// <summary>
        /// Ensures that the URI ends with a forward slash (if not null).
        /// </summary>
        private static string FixSysServiceUrl(string uri)
        {
            return !string.IsNullOrEmpty(uri) && !uri.EndsWith("/") ? uri + "/" : uri;
        }



        public static void init()
        {
            //Set up cache in constructor
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
            request.AddHeader("Token", token);
            return request;

        }

        public static RestClient GetRestClient(string uri, int maxTimeOut = 0)
        {
            RestClientOptions options = new RestClientOptions(new Uri(uri));
            options.Timeout = TimeSpan.FromMilliseconds(maxTimeOut);
            var client = new GoRestClient(options);
            return client;
        }

        public static RestClient GetRestClientWithNewtonsoftJson(string uri, int maxTimeOut = 0)
        {
            RestClientOptions options = new RestClientOptions(new Uri(uri));
            if (maxTimeOut > 0)
                options.Timeout = TimeSpan.FromMilliseconds(maxTimeOut);
            var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
            return client;
        }

        public static void LogErrorString(string error)
        {
            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogError(error);

        }

        protected static void LogError(Exception exception, string message)
        {
            LogErrorString(message + " exception: " + exception.ToString());
        }

    }
}
