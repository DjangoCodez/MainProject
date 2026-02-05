using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysWholesellerConnector : SysConnectorBase
    {
        #region Ctor
        public SysWholesellerConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<SysWholesellerDTO> GetSysWholesellerDTOs()
        {
            try
            {
                var client = new GoRestClient(selectedUri);
                RestResponse<List<SysWholesellerDTO>> response = client.Execute<List<SysWholesellerDTO>>(CreateRequest("System/Product/SysWholeseller", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysWholesellerDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysWholesellerDTOs");
            }

            return new List<SysWholesellerDTO>();
        }

        public static SysWholesellerDTO GetSysWholesellerDTO(int sysWholesellerId)
        {
            try
            {
                var client = new GoRestClient(selectedUri);
                RestResponse<SysWholesellerDTO> response = client.Execute<SysWholesellerDTO>(CreateRequest("System/Product/SysWholeseller/" + sysWholesellerId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<SysWholesellerDTO>(response.Content);
            }
            catch (Exception ex) { LogError(ex, "GetSysWholesellerDTO"); }

            return new SysWholesellerDTO();
        }

        public static ActionResult SaveSysWholesellerDTO(SysWholesellerDTO sysWholesellerDTO)
        {
            try
            {
                var client = new GoRestClient(selectedUri);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest("System/Product/SysWholeseller", Method.Post, sysWholesellerDTO));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex) { LogError(ex, "GetSysWholesellerDTO"); }

            return new ActionResult();
        }

    }
}
