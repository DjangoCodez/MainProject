using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.Template.Models;
using SoftOne.Soe.Business.Core.Template.Models.Core;
using SoftOne.Soe.Business.Core.Template.Models.Economy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using ZXing;

namespace SoftOne.Soe.Business.Util.WebApiInternal.Template
{
    public class EconomyTemplateConnector : ConnectorBase
    {
        public List<AccountDimCopyItem> GetAccountDimCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/AccountDimCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AccountDimCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AccountDimCopyItem>();
        }

        public List<AccountStdCopyItem> GetAccountStdCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/AccountStdCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AccountStdCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AccountStdCopyItem>();
        }

        public List<AccountInternalCopyItem> GetAccountInternalCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {

                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/AccountInternalCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AccountInternalCopyItem>>(response.Content);
                    }

                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AccountInternalCopyItem>();
        }

        public List<AccountYearCopyItem> GetAccountYearCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/AccountYearCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AccountYearCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AccountYearCopyItem>();
        }

        public List<VoucherSeriesTypeCopyItem> GetVoucherSeriesTypeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/VoucherSeriesTypeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<VoucherSeriesTypeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<VoucherSeriesTypeCopyItem>();
        }

        public List<ResidualCodeCopyItem> GetResidualCodesCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/ResidualCodeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<ResidualCodeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<ResidualCodeCopyItem>();
        }

        

        public List<VoucherTemplatesCopyItem> GetVoucherTemplatesCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/VoucherTemplatesCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<VoucherTemplatesCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<VoucherTemplatesCopyItem>();
        }

        public List<PaymentMethodCopyItem> GetPaymentMethodCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<PaymentMethodCopyItem> paymentMethodCopyItems = new List<PaymentMethodCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/PaymentMethodCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        paymentMethodCopyItems = JsonConvert.DeserializeObject<List<PaymentMethodCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return paymentMethodCopyItems;
        }
        public List<GrossProfitCodeCopyItem> GetGrossProfitCodeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<GrossProfitCodeCopyItem> grossProfitCodeCopyItems = new List<GrossProfitCodeCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/GrossProfitCodeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        grossProfitCodeCopyItems = JsonConvert.DeserializeObject<List<GrossProfitCodeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return grossProfitCodeCopyItems;
        }

        public List<InventoryWriteOffTemplateCopyItem> GetInventoryWriteOffTemplateCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<InventoryWriteOffTemplateCopyItem> InventoryWriteOffTemplateCopyItems = new List<InventoryWriteOffTemplateCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/InventoryWriteOffTemplateCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        InventoryWriteOffTemplateCopyItems = JsonConvert.DeserializeObject<List<InventoryWriteOffTemplateCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return InventoryWriteOffTemplateCopyItems;
        }

        public List<InventoryWriteOffMethodCopyItem> GetInventoryWriteOffMethodCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<InventoryWriteOffMethodCopyItem> InventoryWriteOffMethodCopyItems = new List<InventoryWriteOffMethodCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/InventoryWriteOffMethodCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        InventoryWriteOffMethodCopyItems = JsonConvert.DeserializeObject<List<InventoryWriteOffMethodCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return InventoryWriteOffMethodCopyItems;
        }

        public List<PaymentConditionCopyItem> GetPaymentConditionCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<PaymentConditionCopyItem> paymentConditionCopyItems = new List<PaymentConditionCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/PaymentConditionCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        paymentConditionCopyItems = JsonConvert.DeserializeObject<List<PaymentConditionCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return paymentConditionCopyItems;
        }

        public List<VatCodeCopyItem> GetVatCodeCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<VatCodeCopyItem> vatCodeCopyItems = new List<VatCodeCopyItem>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Economy/VatCodeCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        vatCodeCopyItems = JsonConvert.DeserializeObject<List<VatCodeCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return vatCodeCopyItems;
        }

        public IList<T> GetCopyItems<T>(int sysCompDbId, int actorCompanyId, string resource, Dictionary<string, string> parameters)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);
            List<T> items = new List<T>();

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest(resource, Method.Get, null);
                    foreach(var param in parameters)
                    {
                        request.AddParameter(param.Key, param.Value, ParameterType.QueryString);
                    }
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        items = JsonConvert.DeserializeObject<List<T>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return items;
        }

    }
}
