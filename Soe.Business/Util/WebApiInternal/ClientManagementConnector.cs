using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.DTO.ClientManagement;
using System;

namespace SoftOne.Soe.Business.Util.WebApiInternal
{
    public class ClientManagementConnector : ConnectorBase
    {
        public MultiCompanyApiResponseDTO ProcessMultiCompanyRequest(string url, MultiCompanyApiRequestDTO request)
        {
            try
            {
                var options = new RestClientOptions(url);
                var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                var apiRequest = CreateRequest("Internal/Client/Management/Process", Method.Post, request);
                apiRequest.AddJsonBody(request);
                var response = client.Execute(apiRequest);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<MultiCompanyApiResponseDTO>(response.Content);
                }
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("ClientManagementConnector ProcessMultiCompanyRequest failed " + ex.ToString());
            }
            return null;
        }
    }
}
