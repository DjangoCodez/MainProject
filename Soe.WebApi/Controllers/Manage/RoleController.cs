using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/Role")]
    public class RoleController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;
        private readonly CompanyManager cm;
        private readonly RoleManager rm;
        private readonly SettingManager sm;
        private readonly UserManager um;

        #endregion

        #region Constructor

        public RoleController(AttestManager am, CompanyManager cm, RoleManager rm, SettingManager sm, UserManager um)
        {
            this.am = am;
            this.cm = cm;
            this.rm = rm;
            this.sm = sm;
            this.um = um;
        }

        #endregion

        #region AttestRole

        [HttpGet]
        [Route("AttestRole/Meeting/")]
        public IHttpActionResult GetAttestRolesForMeeting()
        {
            return Content(HttpStatusCode.OK, am.GetAttestRolesDict(base.ActorCompanyId, SoeModule.Time, false, onlyHumanResourcesPrivacy: true).ToSmallGenericTypes());
        }

        #endregion

        #region Role

        [HttpGet]
        [Route("Roles/{actorCompanyId:int}")]
        public IHttpActionResult GetRoles(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, rm.GetRolesByCompany(actorCompanyId, loadExternalCode: true).ToGridDTOs());
        }
        [HttpGet]
        [Route("GetAllRoles/{actorCompanyId:int}")]
        public IHttpActionResult GetAllRoles(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, rm.GetAllRolesByCompany(actorCompanyId, loadExternalCode: true).ToGridDTOs());
        }
        [HttpPost]
        [Route("Role/UpdateState")]
        public IHttpActionResult UpdateRoleState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.UpdateRolesState(model.Dict));
        }
        [HttpGet]
        [Route("ForEdit/{roleId:int}")]
        public IHttpActionResult GetRole(int roleId)
        {
            return Content(HttpStatusCode.OK, rm.GetRoleDiscardState(roleId, loadExternalCode: true).ToEditDTO());
        }

        [HttpGet]
        [Route("{addEmptyRow:bool}/{addEmptyRowAsAll:bool}")]
        public IHttpActionResult GetRolesDict(bool addEmptyRow, bool addEmptyRowAsAll)
        {
            return Content(HttpStatusCode.OK, rm.GetRolesByCompanyDict(base.ActorCompanyId, addEmptyRow, addEmptyRowAsAll).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ByUser/{actorCompanyId:int}")]
        public IHttpActionResult GetRolesByUser(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, rm.GetRolesByUser(base.UserId, actorCompanyId).ToDictionary(k => k.RoleId, v => v.Name).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("UserRoles/{userId:int}/{ignoreDate:bool}")]
        public IHttpActionResult GetUserRoles(int userId, bool ignoreDate)
        {
            return Content(HttpStatusCode.OK, um.GetUserRolesDTO(userId, ignoreDate));
        }

        [HttpGet]
        [Route("CompanyRoles/{isAdmin:bool}/{userId:int}")]
        public IHttpActionResult GetCompanyRoles(bool isAdmin, int userId)
        {
            return Content(HttpStatusCode.OK, cm.GetCompanyRolesDTO(isAdmin, userId, base.LicenseId));
        }

        [HttpGet]
        [Route("Roles/StartPages")]
        public IHttpActionResult GetStartPages()
        {
            return Content(HttpStatusCode.OK, sm.GetFavoriteItemOptionsDict(true).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveRole(RoleEditDTO roleInput)
        {
            return Ok(rm.SaveRole(roleInput, base.ActorCompanyId, base.UserId));
        }

        [HttpDelete]
        [Route("{roleId:int}")]
        public IHttpActionResult DeleteRole(int roleId)
        {
            return Content(HttpStatusCode.OK, rm.DeleteRole(roleId, base.ActorCompanyId));
        }
        [HttpGet]
        [Route("VerifyRoleHasUsers/{roleId:int}")]
        public IHttpActionResult VerifyRoleHasUsers(int roleId)
        {
            return Content(HttpStatusCode.OK, rm.VerifyRoleHasUsers(roleId));
        }

        #endregion
    }
}