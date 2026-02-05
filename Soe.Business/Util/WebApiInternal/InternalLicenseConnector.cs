using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using static Soe.Sys.Common.Enumerations;

namespace SoftOne.Soe.Business.Util.WebApiInternal
{
    public class InternalLicenseConnector : ConnectorBase
    {
        public List<string> GetApiInternals()
        {
            var compDBs = SysCompanyConnector.GetSysCompDBDTOs();

            if (compDBs != null)
                return compDBs.Where(w => w.Type == SysCompDBType.Production).Select(s => s.ApiUrl.ToLower().Replace("apix", "apiinternal")).ToList();
            else
                return new List<string>();
        }
        public List<SysXEArticleDTO> GetGoArticles()
        {
            try
            {
                var options = new RestClientOptions(GetApiInternals().First());
                var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                var request = CreateRequest("GoArticles", Method.Get, null);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<List<SysXEArticleDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("Connector PrintMigratedReportDTO failed " + ex.ToString());
                return null;
            }
        }

        public List<SysXEArticleDTO> GetGoArticles(string orgNr)
        {
            List<SysXEArticleDTO> dtos = new List<SysXEArticleDTO>();

            try
            {
                var urls = GetApiInternals();

                foreach (var url in urls)
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("GoArticles/OrgNr", Method.Get, null);
                    request.AddParameter("orgNr", orgNr, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        dtos.AddRange(JsonConvert.DeserializeObject<List<SysXEArticleDTO>>(response.Content));
                    }
                }
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("Connector GetGoArticles failed " + ex.ToString());
            }

            return dtos;
        }

        public List<LicenseArticleDTO> GetGoArticlesFromLicenseId(int licenseId)
        {
            List<LicenseArticleDTO> dtos = new List<LicenseArticleDTO>();

            try
            {
                var urls = GetApiInternals();

                foreach (var url in urls)
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("LicenseArticles", Method.Get, null);
                    request.AddParameter("licenseId", licenseId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        dtos.AddRange(JsonConvert.DeserializeObject<List<LicenseArticleDTO>>(response.Content));
                    }
                }
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("Connector GetGoArticles failed " + ex.ToString());
            }

            return dtos;
        }

        public List<LicenseArticleDTO> GetGoArticlesFromOrgNr(string orgNr)
        {
            List<LicenseArticleDTO> dtos = new List<LicenseArticleDTO>();

            try
            {
                var urls = GetApiInternals();

                foreach (var url in urls)
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("LicenseArticles/OrgNr", Method.Get, null);
                    request.AddParameter("orgNr", orgNr, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content))
                    {
                        dtos.AddRange(JsonConvert.DeserializeObject<List<LicenseArticleDTO>>(response.Content));
                    }
                }
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("Connector GetGoArticles failed " + ex.ToString());
            }

            return dtos;
        }

    }

}
