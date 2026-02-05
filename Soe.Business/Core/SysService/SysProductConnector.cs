using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using ExternalProductType = SoftOne.Soe.Common.Util.ExternalProductType;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysProductConnector : SysConnectorBase
    {
        #region Ctor
        public SysProductConnector(ParameterObject parameterObject) : base(parameterObject)  { }

        #endregion  

        public static List<ExternalProductSmallDTO> ProductSearch(TermGroup_Country country, int fetchsize, string search, List<int> sysPriceListHeadIds = null)
        {
            List<ExternalProductSmallDTO> sysProductSmallDTOs = new List<ExternalProductSmallDTO>();

            SysProductSearchDTO sysProductSearchDTO = new SysProductSearchDTO()
            {
                SysCountryId = (int)country,
                Token = token,
                Fetchsize = fetchsize,
                Search = search,
                SysPriceListHeadIds = sysPriceListHeadIds,

            };

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<ExternalProductSmallDTO>> response = client.Execute<List<ExternalProductSmallDTO>>(CreateRequest("system/product/sysProductsmallsearch/{sysProductSearchDTO}", Method.Post, sysProductSearchDTO));
                return JsonConvert.DeserializeObject<List<ExternalProductSmallDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "ProductSearch");
            }

            return sysProductSmallDTOs;
        }

        public static List<ExternalProductSmallDTO> ProductAzureSearch(TermGroup_Country country, ExternalProductType externalProductType, int fetchsize, string number, string name, string productGroupIdentifier, string text, List<int> sysPriceListHeadIds = null)
        {
            List<ExternalProductSmallDTO> sysProductSmallDTOs = new List<ExternalProductSmallDTO>();

            if (number == null)
                number = string.Empty;
            if (name == null) 
                name = string.Empty;

            var sysProductSearchDTO = new SysProductSearchDTO
            {
                SysCountryId = (int)country,
                Token = token,
                Fetchsize = fetchsize,
                Number = number,
                SysPriceListHeadIds = sysPriceListHeadIds,
                ExternalProductType = externalProductType,
                Name = name,
                Search = $"{number}+{name}",
                ProductGroupIdentifier = productGroupIdentifier,
                Text = text,
            };

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                var request = CreateRequest("system/product/SysProductAzureSearch/", Method.Post, sysProductSearchDTO);
                RestResponse<List<ExternalProductSmallDTO>> response = client.Execute<List<ExternalProductSmallDTO>>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    LogErrorString($"SysProductConnect.ProductAzureSearch Error: {response.StatusCode} Content: {response.Content} ");
                }
                else
                {
                    return JsonConvert.DeserializeObject<List<ExternalProductSmallDTO>>(response.Content);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "ProductAzureSearch");
            }

            return sysProductSmallDTOs;
        }

        public static ActionResult PopulateAzureSearch()
        {
            ActionResult result = new ActionResult();

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 3600 * 1000 * 8 );
                var request = CreateRequest("system/product/SysProductAzurePopulate/", Method.Get, null);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(request);
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError || response.Content == null)
                {
                    result.ErrorMessage = $"SysProductConnect.PopulateAzureSearch Error: {response.StatusCode}, Message: {response.ErrorMessage}, Content: {response.Content}";
                    result.Success = false;
                    LogErrorString(result.ErrorMessage);
                }
                else
                {
                    return JsonConvert.DeserializeObject<ActionResult>(response.Content);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "PopulateAzureSearch");
                result.Success = false;
                result.ErrorMessage = ex.ToString();
            }

            return result;
        }

        public static List<SysPriceListDTO> GetPriceListDTOsForProducts(TermGroup_Country country, List<ExternalProductSmallDTO> externalProductSmallDTOs)
        {

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<ExternalProductSmallDTO>> response = client.Execute<List<ExternalProductSmallDTO>>(CreateRequest("system/product/SysPriceListDTO/{externalProductSmallDTOs}", Method.Post, externalProductSmallDTOs));
                return JsonConvert.DeserializeObject<List<SysPriceListDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetPriceListDTOsForProducts");
            }

            return new List<SysPriceListDTO>();
        }

        public static List<SysWholesellerDTO> GetSysWholesellerDTOs(TermGroup_Country country)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<ExternalProductSmallDTO>> response = client.Execute<List<ExternalProductSmallDTO>>(CreateRequest("system/product/SysWholeseller/"+((int)country).ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysWholesellerDTO>>(response.Content);
            }
            catch (Exception ex) { LogError(ex, "GetSysWholesellerDTOs"); }

            return new List<SysWholesellerDTO>();
        }

    }
}
