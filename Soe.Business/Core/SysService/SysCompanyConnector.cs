using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysCompanyConnector : SysConnectorBase
    {
        #region Ctor
        public SysCompanyConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<SysCompanyDTO> GetSysCompanyDTOs()
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                RestResponse<List<SysCompanyDTO>> response = client.Execute<List<SysCompanyDTO>>(CreateRequest("SysCompany/SysCompany", Method.Get, null));

                return JsonConvert.DeserializeObject<List<SysCompanyDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysCompanyDTOs");
            }

            return new List<SysCompanyDTO>();
        }

        public static SysCompanyDTO GetSysCompanyDTO(string companyApiKey, int sysCompDBId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompanyDTO> response = client.Execute<SysCompanyDTO>(CreateRequest("SysCompany/SysCompany/" + companyApiKey + "/" + sysCompDBId, Method.Get, null));
                return JsonConvert.DeserializeObject<SysCompanyDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysCompanyDTO");
            }

            return new SysCompanyDTO();
        }

        public static SysCompanyDTO GetSysCompanyDTO(int sysCompanyId, bool includeSettings, bool includeBankAccounts, bool includeUniqueValues)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                var request = CreateRequest("SysCompany/SysCompany/" + sysCompanyId, Method.Get, null);
                if (includeSettings)
                {
                    request.AddParameter("includeSettings", includeSettings, ParameterType.QueryString);
                }
                if (includeBankAccounts)
                {
                    request.AddParameter("includeBankAccounts", includeBankAccounts, ParameterType.QueryString);
                }
                if (includeUniqueValues)
                {
                    request.AddParameter("includeUniqueValues", includeUniqueValues, ParameterType.QueryString);
                }
                RestResponse<SysCompanyDTO> response = client.Execute<SysCompanyDTO>(request);

                return JsonConvert.DeserializeObject<SysCompanyDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysCompanyDTO");
            }

            return new SysCompanyDTO();
        }

        public static ActionResult SaveSysCompanyDTO(SysCompanyDTO sysCompanyDTO)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompanyDTO> response = client.Execute<SysCompanyDTO>(CreateRequest("SysCompany/SysCompany/", Method.Post, sysCompanyDTO));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SaveSysCompanyDTO");
            }

            return new ActionResult();
        }

        public static ActionResult SaveSysCompanyDTOs(List<SysCompanyDTO> sysCompanyDTOs)
        {
            ActionResult result = new ActionResult();
            result.Success = true;
            string content = string.Empty;
            string resp = string.Empty;

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest("SysCompany/SysCompanies/", Method.Post, sysCompanyDTOs));
                content = response.Content;
                resp = response.ToString();
                result = JsonConvert.DeserializeObject<ActionResult>(response.Content);
                result.ErrorMessage = resp + content;
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, resp + content);
                result = new ActionResult();
                result.Success = false;
                result.ErrorMessage = ex.ToString() + resp + content;
                return result;
            }
        }

        public static List<SysCompanyDTO> SearchSysCompanies(SearchSysCompanyDTO filter, bool forceUniqueMatch)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                var request = CreateRequest("SysCompany/SysCompanies/Search/", Method.Post, filter);

                if (forceUniqueMatch)
                {
                    request.AddParameter("forceUniqueMatch", forceUniqueMatch, ParameterType.QueryString);
                }

                var response = client.Execute<List<SysCompanyDTO>>(request);
                return JsonConvert.DeserializeObject<List<SysCompanyDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SearchSysCompanyDTO");
            }
            return new List<SysCompanyDTO>();
        }

        public static List<SysCompDBDTO> GetSysCompDBDTOs()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysCompDBDTO>> response = client.Execute<List<SysCompDBDTO>>(CreateRequest("SysCompany/SysCompDB", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysCompDBDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysCompDBDTOs");
            }

            return new List<SysCompDBDTO>();
        }

        public static SysCompDBDTO GetSysCompDBDTO(int sysCompDBId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompDBDTO> response = client.Execute<SysCompDBDTO>(CreateRequest("SysCompany/SysCompDB/" + sysCompDBId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<SysCompDBDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysCompDBDTO");
            }

            return new SysCompDBDTO();
        }

        public static string GetWebUri(int sysCompDBId)
        {
            string key = $"GetWebUri#{sysCompDBId}";
            string url = BusinessMemoryCache<string>.Get(key);

            if (!string.IsNullOrEmpty(url))
                return url;

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompDBDTO> response = client.Execute<SysCompDBDTO>(CreateRequest("SysCompany/Connector/Web/sysCompDBId/" + sysCompDBId.ToString(), Method.Get, null));
                url = JsonConvert.DeserializeObject<string>(response.Content);

                if (!string.IsNullOrEmpty(url))
                {
                    BusinessMemoryCache<string>.Set(key + "backup", url, 5 * 60 * 60);
                    BusinessMemoryCache<string>.Set(key, url, 10 * 60);
                }

                return url;
            }
            catch (Exception ex)
            {
                url = BusinessMemoryCache<string>.Get(key + "backup");

                if (!string.IsNullOrEmpty(url))
                    return url;
                LogError(ex, "GetWebUri");
            }

            return string.Empty;
        }

        public static string GetApix(int sysCompDBId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompDBDTO> response = client.Execute<SysCompDBDTO>(CreateRequest("SysCompany/Connector/Apix/sysCompDBId/" + sysCompDBId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<string>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetApix");
            }

            return string.Empty;
        }

        public static string GetApiInternal(int sysCompDBId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompDBDTO> response = client.Execute<SysCompDBDTO>(CreateRequest("SysCompany/Connector/ApiInternal/sysCompDBId/" + sysCompDBId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<string>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetApiInternal");
            }

            return string.Empty;
        }

        public static List<SysCompServerDTO> GetSysCompServerDTOs()
        {
            string content = string.Empty;
            string resp = string.Empty;

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysCompServerDTO>> response = client.Execute<List<SysCompServerDTO>>(CreateRequest("SysCompany/SysCompServer", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysCompServerDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogErrorString(ex.ToString() + resp + content);
            }

            return new List<SysCompServerDTO>();
        }

        public static SysCompServerDTO GetSysCompServerDTO(int sysCompServerId)
        {
            string content = string.Empty;
            string resp = string.Empty;

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysCompServerDTO> response = client.Execute<SysCompServerDTO>(CreateRequest("System/SysCompany/SysCompServer/" + sysCompServerId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<SysCompServerDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogErrorString(ex.ToString() + resp + content);
            }

            return new SysCompServerDTO();
        }

        public static void LogError(Exception ex)
        {
            SysServiceManager ssm = new SysServiceManager(null);
            ssm.LogError(ex.ToString());
        }


    }
}
