using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.Template.Models.Attest;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.WebApiInternal.Template
{
    public class AttestTemplateConnector : ConnectorBase
    {
        public List<CategoryCopyItem> GetCategoryCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Attest/CategoryCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<CategoryCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<CategoryCopyItem>();
        }

        public List<AttestRoleCopyItem> GetAttestRoleCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Attest/AttestRoleCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AttestRoleCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AttestRoleCopyItem>();
        }

        public List<AttestStateCopyItem> GetAttestStateCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Attest/AttestStateCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AttestStateCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AttestStateCopyItem>();
        }


        public List<AttestTransitionCopyItem> GetAttestTransitionCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Attest/AttestTransitionCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AttestTransitionCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AttestTransitionCopyItem>();
        }


        public List<AttestWorkFlowTemplateHeadCopyItem> GetAttestWorkFlowTemplateHeadCopyItems(int sysCompDbId, int actorCompanyId)
        {
            var url = ConfigurationSetupUtil.GetUrlFromSysCompDbId(sysCompDbId);

            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    var client = new GoRestClient(url);
                    var request = CreateRequest("Internal/Template/Attest/AttestWorkFlowTemplateHeadCopyItems", Method.Get, null);
                    request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                    RestResponse response = client.Execute(request);

                    if (!string.IsNullOrEmpty(response.Content) && response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return JsonConvert.DeserializeObject<List<AttestWorkFlowTemplateHeadCopyItem>>(response.Content);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogCollector.LogError(ex);
                }
            }

            return new List<AttestWorkFlowTemplateHeadCopyItem>();
        }

    }
}
