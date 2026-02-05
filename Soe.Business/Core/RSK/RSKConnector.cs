using Banker.Shared.DTO;
using Banker.Shared.Types;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using RestSharp;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Common.KeyVault;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.DTO.RSK;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SoftOne.Soe.Business.Core.RSK
{
    public class RSKConnector
    {
        private static readonly string baseUrlTestProDibas = "https://test.webapi.prodibas.se";
        private static readonly string baseUrlProDibas = "https://webapi.vvsinfo.se";
        private static readonly string industryKey = "VVS";

        private static string apiKey;
        private static string companyIdentifier;

        private static readonly SysLogManager sl = new SysLogManager(null);

        //private string _token;
        static RSKConnector()
        {
        }

        #region Rest base

        private static string GetAPIToken()
        {
            return ConnectorBase.GetAccessToken();
        }

        private static RestClient CreateAPIClient(bool testMode)
        {
            return new GoRestClient(testMode ? baseUrlTestProDibas : baseUrlProDibas);
        }

        public static RestRequest CreateAPIRequest(KeyVaultSettings vaultsettings, string resource, Method method, bool testMode)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            apiKey = apiKey ?? KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "RSK-ApiKey", vaultsettings.StoreLocation, vaultsettings.TenantId);
            companyIdentifier = companyIdentifier ?? KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "RSK-CompanyIdentifier", vaultsettings.StoreLocation, vaultsettings.TenantId);

            request.AddHeader("X-API-KEY", apiKey); 
            request.AddHeader("X-MANUFACTURER-IDENTIFIER", companyIdentifier);
            request.AddHeader("X-SYSTEM", industryKey);
            return request;

        }

        #endregion

        #region Get product

        /*public static RSKProductDTO GetPlumbingProductFromRSK(SettingManager settingManager, string productId)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest("v1/products/items?number=" + productId + "&include=manufacturer&include=productGroup&include=baseInfo&include=nameInfo&etimVersion=1", Method.Get, testMode);
            request.RequestFormat = DataFormat.Json;

            var response = client.Execute<ActionResult>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var products = JsonConvert.DeserializeObject<RSKProductDTO[]>(response.Content);
                return products.FirstOrDefault();
            }
            else
            { 
                return null; 
            }    
        }*/

        #endregion

        #region Get products from group

        public static List<RSKProductDTO> CreateGetProductsByProductGroupRequest(SettingManager settingManager, KeyVaultSettings vaultsettings, string productGroupIdentifier, int retryCount = 3)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest(vaultsettings, "v1/tasks/products/items", Method.Post, testMode);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new {
                Type = "Search",
                SearchParams = new
                {
                    OutputSettings = new {
                        Include = new[] { "baseInfo", "productGroup", "nameInfo", "manufacturer", "uris", "extNumbers" },
                        EtimVersion = 1
                    },
                    SearchCriteria = new
                    {
                        ActiveStatus = "All",
                        ProductGroupIdentifier = productGroupIdentifier,
                    },
                },
            });

            var response = client.Execute<ActionResult>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var batchResponse = JsonConvert.DeserializeObject<RSKBatchResponse>(response.Content);
                if(batchResponse.Id != null)
                {
                    var dtos = GetProductsByProductGroupResponse(settingManager, vaultsettings, batchResponse.Id);
                    if(dtos == null && retryCount > 0)
                    {
                        // Retrival failed, retry search
                        return CreateGetProductsByProductGroupRequest(settingManager, vaultsettings, productGroupIdentifier, retryCount--);
                    }
                    else
                    {
                        // Retrival complete, returning result
                        return dtos;
                    }
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }
        }

        public static List<RSKProductDTO> GetProductsByProductGroupResponse(SettingManager settingManager, KeyVaultSettings vaultsettings, string id)
        {
            List<RSKProductDTO> dtos = new List<RSKProductDTO>();

            var response = GetProductsByProductGroupResponse(settingManager, vaultsettings, id, 1);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var rskResponse = JsonConvert.DeserializeObject<RSKBatchResponse>(response.Content);
                if(rskResponse.Status == "Done")
                {
                    dtos.AddRange(rskResponse.SearchResult.ProductItems);

                    int page = 2;
                    int totalCount = rskResponse.SearchResult.PaginationMetadata.TotalCount - dtos.Count;
                    while (totalCount > 0)
                    {
                        var additionalResponse = GetProductsByProductGroupResponse(settingManager, vaultsettings, id, page);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK) { 
                            var additionalRskResponse = JsonConvert.DeserializeObject<RSKBatchResponse>(additionalResponse.Content);

                            dtos.AddRange(additionalRskResponse.SearchResult.ProductItems);
                            totalCount = totalCount - additionalRskResponse.SearchResult.PaginationMetadata.PageSize;
                        }
                        else
                        {
                            break;
                        }
                        page++;
                    }
                }
                else if(rskResponse.Status == "Error")
                {
                    sl.AddSysLogInfoMessage(Environment.MachineName, "RSK Update Product Names Job", "Response failed for get request for batch " + id + " calling for new search. (" + rskResponse.ErrorMessage + ")");
                    return null;
                }
                else if(rskResponse.Status == "Pending")
                {
                    sl.AddSysLogInfoMessage(Environment.MachineName, "RSK Update Product Names Job", "Response pending for get request for batch " + id + " calling for retry (proposed retry: " + rskResponse.RecommendedRecheckAfterSeconds + " sek)");
                    Thread.Sleep(1000);
                    return GetProductsByProductGroupResponse(settingManager, vaultsettings, id);
                }
                return dtos;
            }
            else
            {
                return null;
            }
        }

        private static RestResponse GetProductsByProductGroupResponse(SettingManager settingManager, KeyVaultSettings vaultsettings, string id, int page)
        {
            bool testMode = GetTestMode(settingManager);
            var client = CreateAPIClient(testMode);

            var request = CreateAPIRequest(vaultsettings, "v1/tasks/products/items/" + id + "?PageNumber=" + page + "&PageSize=1000", Method.Get, testMode);
            request.RequestFormat = DataFormat.Json;
            sl.AddSysLogInfoMessage(Environment.MachineName, "RSK Update Product Names Job", "Executing get request for id " + id + " page: " + page);
            return client.Execute<ActionResult>(request);
        }

        #endregion

        #region Product groups

        public static List<RSKProductGroupDTO> GetPlumbingProductGroupsFromRSK(SettingManager settingManager, KeyVaultSettings vaultsettings)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest(vaultsettings, "v1/productGroups?productGroupVersion=0", Method.Get, testMode);
            request.RequestFormat = DataFormat.Json;

            var response = client.Execute<ActionResult>(request);
            return JsonConvert.DeserializeObject<List<RSKProductGroupDTO>>(response.Content);
        }

        #endregion

        #region Private methods

        public static bool GetTestMode(SettingManager settingManager)
        {
            bool testMode = settingManager.isTest() || settingManager.isDev();
#if DEBUG
            testMode = true;
#endif

            return testMode;
        }

        #endregion
    }
}
