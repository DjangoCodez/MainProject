using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.Template.Models.Billing;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.WebApiInternal.Template
{
    public class BillingTemplateConnector : ConnectorBase
    {
        public List<InvoiceProductCopyItem> GetInvoiceProductCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/InvoiceProductCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<InvoiceProductCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<InvoiceProductCopyItem>();
        }

        public List<ProductUnitCopyItem> GetProductUnitCopyItems(int sysCompDbId, int actorCompanyId)
        {

           var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/ProductUnitCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);
                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ProductUnitCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ProductUnitCopyItem>();
        }

        public List<PriceListCopyItem> GetPriceListCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/PriceListCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<PriceListCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<PriceListCopyItem>();
        }

        public List<SupplierAgreementCopyItem> GetSupplierAgreementCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/SupplierAgreementCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<SupplierAgreementCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<SupplierAgreementCopyItem>();
        }


        public List<ChecklistCopyItem> GetChecklistCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/ChecklistCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ChecklistCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ChecklistCopyItem>();
        }

        public List<EmailTemplateCopyItem> GetEmailTemplateCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/EmailTemplateCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<EmailTemplateCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<EmailTemplateCopyItem>();
        }

        public List<CompanyWholesellerPriceListCopyItem> GetCompanyWholesellerPriceListCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/CompanyWholesellerPriceListCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<CompanyWholesellerPriceListCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<CompanyWholesellerPriceListCopyItem>();
        }

        public PriceRuleCopyItem GetPriceRuleCopyItem(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/PriceRuleCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<PriceRuleCopyItem>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new PriceRuleCopyItem();
        }

        public ProjectSettingsCopyItem GetProjectSettingsCopyItem(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Billing/ProjectSettingsCopyItem", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<ProjectSettingsCopyItem>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new ProjectSettingsCopyItem();
        }

        
    }
}
