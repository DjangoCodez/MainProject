using Newtonsoft.Json;
using RestSharp;
using Soe.Sys.Common.DTO;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.SysService
{
    public class MultiCompanyDTO // Temp before update of sys nuget
    {
        public int SysMultiCompanyMappingId { get; set; }
        public int MCSysCompanyId { get; set; }
        public string MCName { get; set; }
        public int? MCLicenseId { get; set; }
        public string MCLicenseNr { get; set; }
        public string MCLicenseName { get; set; }
        public int TCUserId { get; set; }
        public int TCRoleId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class SysMultiCompanyConnector : SysConnectorBase
    {
        #region Properties and Fields
        // Create a logger for use in this class
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string URI_PREFIX = "System/MultiCompany";
        private const string URI_PREFIX_TARGET = "System/TargetCompany";

        #endregion

        #region Constructor

        public SysMultiCompanyConnector(ParameterObject parameterObject) : base(parameterObject)
        {
        }
        #endregion

        public static List<TargetCompanyDTO> GetTargetCompanies(Guid companyGuid, int? compDbId)
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                RestResponse<List<TargetCompanyDTO>> response = client.Execute<List<TargetCompanyDTO>>(CreateRequest($"{URI_PREFIX}/{companyGuid}/TargetCompanies?compDbId={compDbId}", Method.Get, null));

                return JsonConvert.DeserializeObject<List<TargetCompanyDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysMultiCompanyConnector:GetTargetCompanies");
            }

            return new List<TargetCompanyDTO>();
        }

        public static ActionResult RegisterConnection(Guid companyGuid, int? compDbId, MultiCompanyConnectionRequestDTO registerRequest)
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest($"{URI_PREFIX}/{companyGuid}/ConnectionRequest?compDbId={compDbId}", Method.Post, registerRequest));

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysMultiCompanyConnector:RegisterConnectionCode");
                return new ActionResult(ex);
            }
        }

        public static ActionResult GetConnectionRequestStatus(Guid companyGuid, int connectionRequestId)
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest($"{URI_PREFIX}/{companyGuid}/ConnectionRequest/{connectionRequestId}/Status", Method.Get));

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysMultiCompanyConnector:GetConnectionRequestStatus");
                return new ActionResult(ex);
            }
        }
        public static ActionResult AcceptConnection(Guid targetCompanyGuid, int? compDbId, MultiCompanyConnectionAcceptDTO connectionAcceptRequest)
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(CreateRequest($"{URI_PREFIX_TARGET}/{targetCompanyGuid}/ConnectionRequest/Accept?compDbId={compDbId}", Method.Post, connectionAcceptRequest));
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysMultiCompanyConnector:AcceptConnectionCode");
                return new ActionResult(ex);
            }
        }

        public static List<MultiCompanyDTO> GetLinkedMultiCompanies(Guid companyGuid, int? compDbId)
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                var response = client.Execute<List<MultiCompanyDTO>>(CreateRequest($"{URI_PREFIX_TARGET}/{companyGuid}/MultiCompanies?compDbId={compDbId}", Method.Get, null));
                return JsonConvert.DeserializeObject<List<MultiCompanyDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysMultiCompanyConnector:GetLinkedMultiCompanies");
            }

            return new List<MultiCompanyDTO>();
        }
        public static CompanyConnectionRequestDTO GetConnectionRequest(Guid companyGuid, string code)
        {
            try
            {
                var options = new RestClientOptions(selectedUri);
                var client = new GoRestClient(options);
                var response = client.Execute<CompanyConnectionRequestDTO>(CreateRequest($"{URI_PREFIX_TARGET}/{companyGuid}/ConnectionRequest?code={code}", Method.Get, null));
                return JsonConvert.DeserializeObject<CompanyConnectionRequestDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SysMultiCompanyConnector:ConnectionRequestIsValid");
            }

            return null;
        }
    }
}
