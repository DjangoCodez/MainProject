using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Status.Shared;
using SoftOne.Status.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SoftOne.Soe.Business.Core.Status
{
    public static class SoftOneStatusConnector
    {
        private static RestClient CreateClient(int? maxTimeout = null)
        {
            var options = maxTimeout == null ? new RestClientOptions(GetBaseUri()) : new RestClientOptions(GetBaseUri()) { Timeout = TimeSpan.FromMilliseconds(maxTimeout.Value) };
            
            var client = new GoRestClient(options);

            return client;
        }
        private static RestRequest CreateRequest(string resource, Method method, object obj = null)
        {
            var request = new RestRequest(resource, method)
            {
                RequestFormat = DataFormat.Json
            };

            if (obj != null)
                request.AddJsonBody(obj);

            return request;
        }

        private static T Deserialize<T>(RestResponse response)
        {
            if (response?.Content == null)
                return default;

            return JsonConvert.DeserializeObject<T>(response.Content);
        }
        private static List<T> DeserializeList<T>(RestResponse response)
        {
            if (response?.Content == null)
                return new List<T>();

            return JsonConvert.DeserializeObject<List<T>>(response.Content);
        }

        private static void LogError(Exception ex, string method, string message = "")
        {
            message = $"{Environment.MachineName} {method}. {message}. {ex.ToString()}";

            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogError(message.Length <= 3200 ? message : message.Substring(0, 3200));
        }

        #region Url

        private static Uri GetBaseUri() => new Uri("http://softonestatus.azurewebsites.net/");
        public static string GetApiInternal(int sysCompDbId)
        {
            try
            {
                var client = CreateClient(1000);
                var request = CreateRequest("Api/StatusService/ApiInternal", Method.Get, null)
                    .AddQueryParameter("sysCompDbId", sysCompDbId.ToString());
                var response = client.Execute(request);
                return Deserialize<string>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return string.Empty;
            }
        }

        public static string GetEvoUrl()
        {
            var key = "EvoUrl";
            var backupKey = "EvoUrlBackup";
            var url = BusinessMemoryCache<string>.Get(key);

            if (url != null)
                return url;

            try
            {
                var client = CreateClient(4000);
                var request = CreateRequest("Api/StatusService/Evo", Method.Get, null);
                var response = client.Execute(request);
                url = JsonConvert.DeserializeObject<string>(response.Content);

                if (!string.IsNullOrEmpty(url))
                {
                    BusinessMemoryCache<string>.Set(key, url, 60);
                    BusinessMemoryCache<string>.Set(backupKey, url, 60 * 60 * 2);
                }

                return url;
            }
            catch (Exception ex)
            {
                url = BusinessMemoryCache<string>.Get(backupKey);

                if (string.IsNullOrEmpty(url) && !ConfigurationSetupUtil.IsTestBasedOnMachine())
                    url = "https://app.softone.se/";

                if (url != null)
                    BusinessMemoryCache<string>.Set(key, url, 30);

                if (url == null)
                {
                    LogError(ex, MethodBase.GetCurrentMethod().Name);
                    return string.Empty;
                }

                return url;
            }
        }

        public static string GetBridgeUrl(bool isTest)
        {
            try
            {
                var client = CreateClient(1000);
                var request = CreateRequest("Api/StatusService/Bridge", Method.Get, null)
                    .AddQueryParameter("isTest", isTest);
                var response = client.Execute(request);
                return Deserialize<string>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return string.Empty;
            }
        }

        #endregion

        #region Status

        public static string GetDefaultSysServiceUrl(int syscompDbId, bool isTest, bool isDev)
        {
            try
            {
                if (syscompDbId == 0)
                    throw new Exception("syscompDbId == 0");

                var client = CreateClient(1000);
                var request = CreateRequest("Api/StatusService/SysService/default/", Method.Get, null)
                    .AddQueryParameter("syscompDbId", syscompDbId.ToString());
                var response = client.Execute(request);
                return Deserialize<string>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return GetSysServiceUrl(isTest, isDev);
            }
        }

        public static string GetSysServiceUrl(bool isTest, bool isDev)
        {
            try
            {
                var client = CreateClient(1000);
                var request = CreateRequest("Api/StatusService/SysService", Method.Get, null)
                    .AddQueryParameter("isTest", isTest.ToString())
                    .AddQueryParameter("isDev", isDev.ToString());
                var response = client.Execute(request);
                return Deserialize<string>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                if (isDev)
                    return "https://devsys.softone.se/";
                else if (isTest)
                    return "https://stagesys.softone.se/";
                else
                    return "https://sys.softone.se/";
            }
        }

        public static bool? IsCrGenServerAlive(string url)
        {
            DateTime start = DateTime.UtcNow;
            int timeOut = 2000;
            RestResponse response = null;

            try
            {
                var client = CreateClient(timeOut);
                var request = CreateRequest("api/StatusService/CrGenServerAlive", Method.Get, null)
                    .AddQueryParameter("url", url);
                response = client.Execute(request);
                return Deserialize<bool>(response);
            }
            catch (Exception ex)
            {
                if (DateTime.UtcNow < (start.AddMilliseconds(timeOut - 100)))
                    LogError(ex, $"{MethodBase.GetCurrentMethod().Name}{response?.ResponseUri?.ToString() ?? string.Empty}", $"CrGenurl: {url}");
                return null;
            }
        }

        public static bool IsServerLive(string domain, bool webOnly = true)
        {
            DateTime start = DateTime.UtcNow;
            int timeOut = 1000;

            try
            {
                var client = CreateClient(timeOut);
                var request = CreateRequest("api/StatusService/SiteAlive", Method.Get, null)
                    .AddQueryParameter("domain", domain)
                    .AddQueryParameter("webOnly", webOnly.ToString());
                var response = client.Execute(request);
                return Deserialize<bool>(response);
            }
            catch (Exception ex)
            {
                if (DateTime.UtcNow < start.AddMilliseconds(timeOut))
                    LogError(ex, MethodBase.GetCurrentMethod().Name, domain);
                return true;
            }
        }

        public static List<StatusServiceGroupUpTimeDTO> GetStatusServiceGroupUpTimes(DateTime dateFrom, DateTime dateTo, int? statusServiceGroupId = null)
        {
            try
            {
                var client = CreateClient(1000);
                var request = CreateRequest("api/StatusResult/UpTimes", Method.Get, null)
                    .AddParameter("dateFrom", dateFrom, ParameterType.QueryString)
                    .AddParameter("dateTo", dateTo, ParameterType.QueryString)
                    .AddParameter("statusServiceGroupId", statusServiceGroupId, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusServiceGroupUpTimeDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusServiceGroupUpTimeDTO>();
            }
        }

        public static List<StatusServiceTypeDTO> GetStatusServiceTypes()
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("Api/StatusService/StatusServiceType", Method.Get, null);
                var response = client.Execute(request);
                return DeserializeList<StatusServiceTypeDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusServiceTypeDTO>();
            }
        }

        public static List<StatusServiceTypeDTO> GetStatusServiceTypes(ServiceType serviceType)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("Api/StatusService/StatusServiceType/servicetype", Method.Get, null)
                    .AddParameter("serviceType", serviceType, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusServiceTypeDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusServiceTypeDTO>();
            }
        }

        public static List<StatusServiceGroupDTO> GetStatusServiceGroups()
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("Api/StatusService/StatusServiceGroups", Method.Get, null);
                var response = client.Execute(request);
                return DeserializeList<StatusServiceGroupDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusServiceGroupDTO>();
            }
        }

        public static List<StatusServiceDTO> GetStatusServices()
        {
            try
            {
                var client = CreateClient(5000);
                var request = CreateRequest("Api/StatusService/StatusService", Method.Get, null);
                var response = client.Execute(request);
                return DeserializeList<StatusServiceDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusServiceDTO>();
            }
        }

        public static List<StatusServerDTO> GetStatusServers()
        {
            try
            {
                var client = CreateClient(5000);
                var request = CreateRequest("Api/StatusService/StatusServer", Method.Get, null);
                var response = client.Execute(request);
                return DeserializeList<StatusServerDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusServerDTO>();
            }
        }

        public static List<StatusEventDTO> GetStatusEventDTOs(DateTime from, DateTime to)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/StatusEvent/StatusEvent/interval", Method.Get, null)
                    .AddParameter("from", from, ParameterType.QueryString)
                    .AddParameter("to", to, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusEventDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusEventDTO>();
            }
        }

        public static List<StatusResultAggregatedDTO> GetStatusResultAggregates(int statusServiceTypeId, DateTime from, DateTime to)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/StatusResult/StatusResultAggregates", Method.Get, null)
                    .AddParameter("statusServiceTypeId", statusServiceTypeId, ParameterType.QueryString)
                    .AddParameter("from", from, ParameterType.QueryString)
                    .AddParameter("to", to, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusResultAggregatedDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusResultAggregatedDTO>();
            }
        }

        public static List<StatusResultAggregatedDTO> GetStatusResultAggregatesFromServerId(int statusServerId, DateTime from, DateTime to)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/StatusResult/StatusResultAggregates/server", Method.Get, null)
                    .AddParameter("statusServerId", statusServerId, ParameterType.QueryString)
                    .AddParameter("from", from, ParameterType.QueryString)
                    .AddParameter("to", to, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusResultAggregatedDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusResultAggregatedDTO>();
            }
        }

        public static List<StatusResultAggregatedDTO> GetStatusResultAggregatesFromTestCaseId(int testCaseId, DateTime from, DateTime to)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/Testcase/StatusResultAggregates", Method.Get, null)
                    .AddParameter("testCaseId", testCaseId, ParameterType.QueryString)
                    .AddParameter("from", from, ParameterType.QueryString)
                    .AddParameter("to", to, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusResultAggregatedDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusResultAggregatedDTO>();
            }
        }

        public static List<StatusResultAggregatedDTO> GetStatusResultAggregatesFromGroupId(int serviceGroupId, DateTime from, DateTime to)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/StatusResult/StatusResultAggregates/Group", Method.Get, null)
                    .AddParameter("serviceGroupId", serviceGroupId, ParameterType.QueryString)
                    .AddParameter("from", from, ParameterType.QueryString)
                    .AddParameter("to", to, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusResultAggregatedDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusResultAggregatedDTO>();
            }
        }

        public static List<StatusResultAggregatedDTO> GetStatusResultAggregates(DateTime from, DateTime to)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/StatusResult/StatusResultAggregates/Prio", Method.Get, null)
                    .AddParameter("from", from, ParameterType.QueryString)
                    .AddParameter("to", to, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<StatusResultAggregatedDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<StatusResultAggregatedDTO>();
            }
        }

        #endregion

        #region SysCompDBDTO

        public static List<SysCompDBDTO> GetAllSysCompDBTOs()
        {
            try
            {
                var client = CreateClient(5000);
                var request = CreateRequest("Api/StatusService/SysService/AllSysCompDbDTOs", Method.Get, null);
                var response = client.Execute(request);
                return DeserializeList<SysCompDBDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<SysCompDBDTO>();
            }
        }

        #endregion

        #region SysServer

        public static List<SysServerDTO> GetSysServersFromSysCompDb(int sysCompDBId)
        {
            try
            {
                var client = new GoRestClient(GetBaseUri());
                var request = CreateRequest("api/StatusService/SysServices/SysCompDb", Method.Get, null)
                    .AddQueryParameter("sysCompDBId", sysCompDBId.ToString());
                var response = client.Execute(request);
                return DeserializeList<SysServerDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name, sysCompDBId.ToString());
                return new List<SysServerDTO>();
            }
        }

        #endregion

        #region TestCase

        public static TestCaseDTO GetTestCase(int testCaseId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/testcase/testcase/{testCaseId}", Method.Get, null);
                var response = client.Execute(request);
                return Deserialize<TestCaseDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        public static List<TestCaseDTO> GetTestCases(TestCaseType type)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/testcase/testcases", Method.Get, null)
                    .AddParameter("testCaseType", type, ParameterType.QueryString);
                var response = client.Execute(request);
                return DeserializeList<TestCaseDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseDTO>();
            }
        }

        public static ActionResult SaveTestCase(TestCaseDTO testCase)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/testcase/testcase", Method.Post, testCase);
                var response = client.Execute(request);
                return Deserialize<ActionResult>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new ActionResult(ex);
            }
        }

        public static TestCaseGroupDTO GetTestCaseGroup(int testCaseGroupId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/testcase/testcasegroup/{testCaseGroupId}", Method.Get, null);
                var response = client.Execute(request);
                return Deserialize<TestCaseGroupDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        public static List<TestCaseGroupDTO> GetTestCaseGroups()
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/testcase/testcasegroups", Method.Get, null);
                var response = client.Execute(request);
                return DeserializeList<TestCaseGroupDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseGroupDTO>();
            }
        }

        public static ActionResult SaveTestCaseGroup(TestCaseGroupDTO testCaseGroup)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest("api/testcase/testcasegroup", Method.Post, testCaseGroup);
                var response = client.Execute(request);
                return Deserialize<ActionResult>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new ActionResult(ex);
            }
        }

        public static Guid? RunTestCaseGroup(int testCaseGroupId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/Testcase/TestCaseGroup/Run/{testCaseGroupId}", Method.Post);
                var response = client.Execute(request);
                return Deserialize<Guid>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        public static ActionResult ScheduleTestGroupsNow(List<int> testCaseGroupIds)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/Testcase/TestCaseGroup/ScheduleNow", Method.Post, testCaseGroupIds);
                var response = client.Execute(request);
                return Deserialize<ActionResult>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new ActionResult(ex);
            }
        }

        public static TestCaseTracking GetTestCaseTracking(Guid trackingGuid)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/TestCaseTracking/{trackingGuid}", Method.Get);
                var response = client.Execute(request);
                return Deserialize<TestCaseTracking>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return null;
            }
        }

        public static List<TestCaseResultDTO> GetTestCaseResultsByTestCaseId(int testCaseId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/TestCaseResultsByTestCaseId/{testCaseId}", Method.Get);
                var response = client.Execute(request);
                return DeserializeList<TestCaseResultDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseResultDTO>();
            }
        }

        public static List<TestCaseResultDTO> GetTestCaseResultsByTestCaseGroupId(int testCaseGroupId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/TestCaseResultsByTestCaseGroupId/{testCaseGroupId}", Method.Get);
                var response = client.Execute(request);
                return DeserializeList<TestCaseResultDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseResultDTO>();
            }
        }

        public static List<TestCaseGroupResultDTO> GetTestCaseGroupResults(int testCaseGroupId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/TestCaseGroupResults/{testCaseGroupId}", Method.Get);
                var response = client.Execute(request);
                return DeserializeList<TestCaseGroupResultDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseGroupResultDTO>();
            }
        }

        public static List<TestCaseGroupOverviewDTO> GetTestCaseGroupOverview()
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/GetTestCaseGroupOverivew", Method.Get);
                var response = client.Execute(request);
                return DeserializeList<TestCaseGroupOverviewDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseGroupOverviewDTO>();
            }
        }

        public static List<TestCaseGroupOverviewDTO> GetTestCaseGroupOverviewByGroup(int testCaseGroupId)
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/GetTestCaseGroupOverviewByGroup/" + testCaseGroupId, Method.Get);
                var response = client.Execute(request);
                return DeserializeList<TestCaseGroupOverviewDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseGroupOverviewDTO>();
            }
        }

        public static List<TestCaseSettingTypeDTO> GetTestCaseSettings()
        {
            try
            {
                var client = CreateClient();
                var request = CreateRequest($"api/TestCase/Settings/All", Method.Get);
                var response = client.Execute(request);
                return DeserializeList<TestCaseSettingTypeDTO>(response);
            }
            catch (Exception ex)
            {
                LogError(ex, MethodBase.GetCurrentMethod().Name);
                return new List<TestCaseSettingTypeDTO>();
            }
        }

        #endregion
    }
}
