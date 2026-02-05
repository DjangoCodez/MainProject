using RestSharp;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Util.ExternalMiddleService
{
    class EMSSuperOfficeConnector
    {
        public static Uri GetUri()
        {
            Uri uri = new Uri("https://ems-superofficetest.azurewebsites.net");
            return uri;
        }
        public static RestRequest CreateRequest(string resource, Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            if (obj != null)
                request.AddJsonBody(obj);

            return request;
        }
        public static ActionResult StartSync(string companyApiKey, int? customerId = null, string countryName = "sweden", int? daysBack = 0)
        {
            try
            {
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("SuperOffice/Sync", Method.Get);
                request.AddParameter("companyApiKey", companyApiKey, ParameterType.QueryString);
                request.AddParameter("customerId", customerId, ParameterType.QueryString);
                request.AddParameter("countryName", countryName, ParameterType.QueryString);
                request.AddParameter("daysBack", daysBack, ParameterType.QueryString);
                var response = client.Execute(request);
                return new ActionResult();
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage(ex.ToString() + " SuperOffice Sync");
                return new ActionResult(ex, "SuperOffice Sync failed");
            }
        }
    }
}
