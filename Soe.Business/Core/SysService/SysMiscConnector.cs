using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysMiscConnector : SysConnectorBase
    {
        #region Ctor
        public SysMiscConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Dictionary<string, string> GetConnectApiKey()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<Dictionary<string, string>> response = client.Execute<Dictionary<string, string>>(CreateRequest("System/Misc/ConnectApiKeys", Method.Get, null));
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetConnectApiKey");
                return null;
            }

        }
    }
}
