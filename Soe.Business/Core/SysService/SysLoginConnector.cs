using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysLoginConnector : SysConnectorBase
    {
        public SysLoginConnector(ParameterObject parameterObject) : base(parameterObject)
        {
        }

        public static List<SysServerDTO> GetSysServers()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysServerDTO>> response = client.Execute<List<SysServerDTO>>(CreateRequest("SysServer/SysServer", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysServerDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysServers");
            }

            return new List<SysServerDTO>();
        }

    }
}
