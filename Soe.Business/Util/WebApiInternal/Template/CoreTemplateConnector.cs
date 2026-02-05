using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SoftOne.Soe.Business.Util.WebApiInternal.Template
{
    public class CoreTemplateConnector : ConnectorBase
    {
        public List<TemplateCompanyItem> GetTemplateCompanyItems(int sysCompDbId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    RestClientOptions options = new RestClientOptions()
                    {
                        Timeout = TimeSpan.FromMilliseconds(1000),
                        BaseUrl = new Uri(url)
                    };
                    var client = new GoRestClient(options);
                    var request = CreateRequest("Internal/Template/Core/TemplateCompanyItems", Method.Get, null);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<TemplateCompanyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<TemplateCompanyItem>();
        }
        public LicenseCopyItem GetLicenseCopyItem(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/LicenseCopyItem", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<LicenseCopyItem>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return null;
        }

        public List<ImportCopyItem> GetImportCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/ImportCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ImportCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ImportCopyItem>();
        }

        public List<CompanyFieldSettingCopyItem> GetCompanyFieldSettingCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/CompanyFieldSettingCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<CompanyFieldSettingCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<CompanyFieldSettingCopyItem>();
        }

        public List<CompanySettingCopyItem> GetCompanySettingCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/CompanySettingCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<CompanySettingCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<CompanySettingCopyItem>();
        }


        public List<RoleAndFeatureCopyItem> GetRoleAndFeatureCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            if (!string.IsNullOrEmpty(url))
            {
                var client = new GoRestClient(url);
                var request = CreateRequest("Internal/Template/Core/RoleAndFeatureCopyItems", Method.Get, null);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<RoleAndFeatureCopyItem>>(response.Content);
                }
            }

            return new List<RoleAndFeatureCopyItem>();
        }

        public List<CompanyAndFeatureCopyItem> GetCompanyAndFeatureCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            if (!string.IsNullOrEmpty(url))
            {
                var client = new GoRestClient(url);
                var request = CreateRequest("Internal/Template/Core/CompanyAndFeatureCopyItems", Method.Get, null);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<CompanyAndFeatureCopyItem>>(response.Content);
                }
            }

            return new List<CompanyAndFeatureCopyItem>();
        }

        public List<UserCopyItem> GetUserCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            if (!string.IsNullOrEmpty(url))
            {
                var client = new GoRestClient(url);
                var request = CreateRequest("Internal/Template/Core/UserCopyItems", Method.Get, null);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<UserCopyItem>>(response.Content);
                }
            }

            return new List<UserCopyItem>();
        }

        public List<ReportCopyItem> GetReportCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/ReportCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ReportCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ReportCopyItem>();
        }

        public List<ReportTemplateCopyItem> GetReportTemplateCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/ReportTemplateCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ReportTemplateCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ReportTemplateCopyItem>();
        }

        public List<ExternalCodeCopyItem> GetExternalCodeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Core/ExternalCodeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ExternalCodeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ExternalCodeCopyItem>();
        }
    }
}
