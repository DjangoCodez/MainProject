using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class SysHolidayConnector : SysConnectorBase
    {
        #region Ctor
        public SysHolidayConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion
       
        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static List<SysHolidayDTO> GetSysHolidayDTOs()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysHolidayDTO>> response = client.Execute<List<SysHolidayDTO>>(CreateRequest("System/Holiday/SysHolidays", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysHolidayDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysHolidayDTOs");
            }

            return new List<SysHolidayDTO>();
        }

        public static List<SysHolidayDTO> GetSysHolidayDTOs(int sysHolidayTypeId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysHolidayDTO>> response = client.Execute<List<SysHolidayDTO>>(CreateRequest("System/Holiday/SysHolidays/" + sysHolidayTypeId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysHolidayDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysHolidayDTOs");
            }

            return new List<SysHolidayDTO>();
        }

        public static SysHolidayDTO GetSysHolidayDTO(DateTime date, int sysCountryId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                Dictionary<string, object> parameterDict = new Dictionary<string, object>();
                parameterDict.Add("date", date);
                parameterDict.Add("sysCountryId", sysCountryId);
                RestResponse<SysHolidayDTO> response = client.Execute<SysHolidayDTO>(CreateRequest("System/Holiday/SysHoliday/", Method.Get, null, parameterDict));
                return JsonConvert.DeserializeObject<SysHolidayDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysHolidayDTO");
            }

            return new SysHolidayDTO();
        }

        public static List<SysHolidayTypeDTO> GetSysHolidayTypeDTOs()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysHolidayDTO>> response = client.Execute<List<SysHolidayDTO>>(CreateRequest("System/Holiday/SysHolidayTypes/", Method.Get, null));
                var dtos = JsonConvert.DeserializeObject<List<SysHolidayTypeDTO>>(response.Content);
                if (dtos != null)
                    return dtos;
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysHolidayDTOs");
            }

            return new List<SysHolidayTypeDTO>();
        }
    }
}
