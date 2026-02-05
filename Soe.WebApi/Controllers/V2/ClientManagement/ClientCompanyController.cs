using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.ClientManagement;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ClientManagement;
using SoftOne.Soe.Common.Util;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.ClientManagement
{
    [RoutePrefix("V2/Shared/ClientCompany")]
    public class ClientCompanyController : SoeApiController
    {
        #region Variables

        private readonly ClientCompanyManager ccm;
        private readonly FeatureManager fm;

        #endregion

        #region Constructor
        public ClientCompanyController(ClientCompanyManager ccm, FeatureManager fm)
        {
            this.ccm = ccm;
            this.fm = fm;
        }
        #endregion

        #region ConnectionRequest
        [HttpGet]
        [Route("ConnectionRequest/{code}")]
        public IHttpActionResult GetConnectionRequest(string code)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users_ServiceUsers_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));

            return Content(HttpStatusCode.OK, ccm.GetMultiCompanyConnectionRequest(base.CompanyGuid.Value, code));
        }


        [HttpPost]
        [Route("ConnectionRequest/Accept")]
        public IHttpActionResult SaveServiceUser(ServiceUserDTO dto)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users_ServiceUsers_Edit, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));

            return Content(HttpStatusCode.OK, ccm.AcceptMultiCompanyRequest(base.CompanyGuid.Value, base.ActorCompanyId, base.LicenseId, dto));
        }


        #endregion
        #region ServiceUser
        [HttpGet]
        [Route("ServiceUser/Grid/{userId:int}")]
        public IHttpActionResult GetServiceUserGrid(int userId)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users_ServiceUsers, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));

            var serviceUsers = ccm.GetServiceUsers(base.CompanyGuid.Value, base.ActorCompanyId);
            if (userId > 0)
                serviceUsers = serviceUsers.Where(su => su.UserId == userId).ToList();

            return Content(HttpStatusCode.OK, serviceUsers);
        }

        [HttpGet]
        [Route("ServiceUser/{serviceUserId:int}")]
        public IHttpActionResult GetServiceUser(int serviceUserId)
        {
            if (!fm.HasRolePermission(Feature.Manage_Users_ServiceUsers, Permission.Modify, base.RoleId, base.ActorCompanyId, base.LicenseId) || base.CompanyGuid is null)
                return Content(HttpStatusCode.Forbidden, new ActionResult(false));

            return Content(HttpStatusCode.OK, ccm.GetServiceUsers(base.CompanyGuid.Value, ActorCompanyId).FirstOrDefault(s => s.UserId == serviceUserId));
        }
        #endregion
    }
}
