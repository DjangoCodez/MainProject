using Soe.Sys.Common.DTO;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System;

namespace SoftOne.Soe.Business.Core
{
    public class ClientManagementManager : ManagerBase
    {
        #region Constructor
        public ClientManagementManager(ParameterObject parameterObject) : base(parameterObject)
        {
        }
        #endregion

        public ActionResult CreateMultiCompanyConnectionRequest()
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                if (!FeatureManager.HasRolePermission(Feature.ClientManagement_Clients, Permission.Readonly, base.RoleId, base.ActorCompanyId, base.LicenseId))
                {
                    actionResult.Success = false;
                    actionResult.ErrorMessage = base.GetText(6502, "Du saknar behörighet att initiera en anslutning till kundföretaget.");
                    actionResult.ErrorNumber = (int)ActionResultSave.ClientManagementNoConnectionCreationPermission;

                    return actionResult;
                }

                var companyConnection = new MultiCompanyConnectionRequestDTO();
                /* Because new sys nuget dosent match this
                companyConnection.MCActorCompanyId = base.ActorCompanyId;
                companyConnection.MCSysCompDbId = CompDbCache.Instance.SysCompDbId;
                companyConnection.MCLicenseId = base.LicenseId;
                companyConnection.MCActorCompanyGuid = base.GetCompanyFromCache(base.ActorCompanyId)?.CompanyGuid ?? null;
                */
                companyConnection.CreatedBy = base.GetUserDetails();
                var compDbId = CompDbCache.Instance.SysCompDbId;

                actionResult = SysMultiCompanyConnector.RegisterConnection(base.parameterObject.CompanyGuid.Value, compDbId, companyConnection);
            }
            catch 
            {
                actionResult.Success = false;
            }
            return actionResult;
        }

        public List<TargetCompanyDTO> GetTargetCompanies(Guid companyGuid)
        {
            var compDbId = CompDbCache.Instance.SysCompDbId;
            return SysMultiCompanyConnector.GetTargetCompanies(companyGuid, compDbId);
        }

        public ActionResult GetCompanyRequestStatus(Guid companyGuid, int connectionRequestId)
        {
            return SysMultiCompanyConnector.GetConnectionRequestStatus(companyGuid, connectionRequestId);
        }
    }
}
