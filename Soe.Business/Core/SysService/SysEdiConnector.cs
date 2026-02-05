using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Soe.Edi.Common.DTO;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using static Soe.Edi.Common.Enumerations;

namespace SoftOne.Soe.Business.Core.SysService
{

    public class SysEdiConnector:SysConnectorBase
   {

        #region Ctor
        public SysEdiConnector(ParameterObject parameterObject) : base(parameterObject) { }

        #endregion

        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region EdiImport

        public static ActionResult ImportEdiFromFtp()
        {
            ActionResult result = new ActionResult();

            try
            {                
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 3600 * 1000);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest("Edi/ImportFromFtp", Method.Get, null));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
                LogError(ex, "ImportEdiFromFtp");
                return result;
            }
        }

        public static ActionResult ImportEdiMessageHeads()
        {
            ActionResult result = new ActionResult();

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 3600 * 1000);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest("Edi/ImportEdiMessageHeads", Method.Get, null));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
                LogError(ex, "ImportEdiMessageHeads");
                return result;
            }
        }

        public static ActionResult RunFlow()
        {
            ActionResult result = new ActionResult();

            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri, 3600 * 1000);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest("Edi/RunFlow", Method.Get, null));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
                LogError(ex, "RunFlow");
                return result;
            }
        }

        #endregion

        #region SysEdiMessageRaw

        public static List<SysEdiMessageRawDTO> GetSysEdiMessageRawDTOs()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysEdiMessageRawDTO>> response = client.Execute<List<SysEdiMessageRawDTO>>(CreateRequest("Edi/SysEdiMessageRaw", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysEdiMessageRawDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysEdiMessageRawDTOs");
            }

            return new List<SysEdiMessageRawDTO>();
        }

        public static SysEdiMessageRawDTO GetSysEdiMessageRawDTO(int sysEdiMessageRawId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysEdiMessageRawDTO> response = client.Execute<SysEdiMessageRawDTO>(CreateRequest("Edi/SysEdiMessageRaw/" + sysEdiMessageRawId.ToString(), Method.Get, null));

                return response.Data;

                //JsonSerializerSettings ss = new JsonSerializerSettings();
                //ss.MissingMemberHandling = MissingMemberHandling.Error;
                //var hej = JsonConvert.DeserializeObject<SysEdiMessageRawDTO>(response.Content, ss);
                //var hej3 = JsonConvert.DeserializeObject<SysEdiMessageRawDTO>(response.Content);

                //var hej2 = JsonConvert.DeserializeObject<SysEdiMessageRawDTO2>(response.Content, ss);

                //return hej;

            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysEdiMessageRawDTO");
            }

            return new SysEdiMessageRawDTO();
        }

        #endregion

        #region SysEdiMessageHead

        public static List<SysEdiMessageHeadDTO> GetSysEdiMessageHeadDTOs()
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysEdiMessageHeadDTO>> response = client.Execute<List<SysEdiMessageHeadDTO>>(CreateRequest("Edi/SysEdiMessageHead", Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysEdiMessageHeadDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSysEdiMessageHeadDTOs");
            }

            return new List<SysEdiMessageHeadDTO>();
        }

        public static SysEdiMessageHeadDTO GetSysEdiMessageHeadDTO(int sysEdiMessageHeadId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysEdiMessageHeadDTO> response = client.Execute<SysEdiMessageHeadDTO>(CreateRequest("Edi/SysEdiMessageHead/" + sysEdiMessageHeadId.ToString(), Method.Get, null));
                return JsonConvert.DeserializeObject<SysEdiMessageHeadDTO>(response.Content);
            }
            catch (Exception ex) { LogError(ex, "GetSysEdiMessageHeadDTO"); }

            return new SysEdiMessageHeadDTO();
        }

        public static ActionResult GetSysEdiMessageHeadMessage(int sysEdiMessageHeadId)
        {
            var result = new ActionResult(false);
            var head = GetSysEdiMessageHeadDTO(sysEdiMessageHeadId);

            if (head != null)
            {
                return new ActionResult { Success = true, StringValue = head.XDocument };
            }

            return new ActionResult(false);
        }

        public static List<SysEdiMessageHeadGridDTO> GetSysEdiMessageHeadGridDTOs(SysEdiMessageHeadStatus status, int take, bool missingSysCompanyId)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse <List<SysEdiMessageHeadGridDTO>> response = client.Execute<List<SysEdiMessageHeadGridDTO>>(CreateRequest("Edi/SysEdiMessageGridHead/" + status + "/" + take + "/" + missingSysCompanyId, Method.Get, null));
                return JsonConvert.DeserializeObject<List<SysEdiMessageHeadGridDTO>>(response.Content);
            }
            catch (Exception ex) { LogError(ex, "GetSysEdiMessageHeadGridDTOs"); }

            return new List<SysEdiMessageHeadGridDTO>();
        }

        public static ActionResult SaveSysEdiMessageHeadDTO(SysEdiMessageHeadDTO dto)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<SysEdiMessageHeadDTO> response = client.Execute<SysEdiMessageHeadDTO>(CreateRequest("Edi/SysEdiMessageHead/", Method.Post, dto));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SaveSysEdiMessageHeadDTO");
                return new ActionResult(ex);
            }
        }

        public static List<SysEdiMessageHeadGridDTO> GetSysEdiMessagesGridDTOs(SysEdiMessageFilterDTO filter)
        {
            try
            {
                var client = GetRestClientWithNewtonsoftJson(selectedUri);
                RestResponse<List<SysEdiMessageHeadGridDTO>> response = client.Execute<List<SysEdiMessageHeadGridDTO>>(CreateRequest("Edi/SysEdiMessagesGrid", Method.Post, filter));
                return JsonConvert.DeserializeObject<List<SysEdiMessageHeadGridDTO>>(response.Content);
            }
            catch (Exception ex) { LogError(ex, "GetSysEdiMessageHeadGridDTOs"); }

            return new List<SysEdiMessageHeadGridDTO>();
        }
        #endregion
    }
}
