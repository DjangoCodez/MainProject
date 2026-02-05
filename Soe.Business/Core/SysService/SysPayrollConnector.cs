using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysPayrollConnector : SysConnectorBase
    {
        #region Ctor
        public SysPayrollConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion
       
        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<SysPayrollPriceViewDTO> GetSysPayrollPriceViews()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysPayrollPriceViewDTO>> response = client.Execute<List<SysPayrollPriceViewDTO>>(CreateRequest("System/Payroll/SysPayrollPriceView", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysPayrollPriceViewDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysPayrollPriceViewDTO");
            }

            return new List<SysPayrollPriceViewDTO>();
        }    

    }
}
