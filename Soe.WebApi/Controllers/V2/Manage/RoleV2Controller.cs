using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Role")]
    public class RoleV2Controller : SoeApiController
    {
        #region Variables

        private readonly RoleManager rm;

        #endregion

        #region Constructor

        public RoleV2Controller(RoleManager rm)
        {
            this.rm = rm;
        }

        #endregion

        #region Role

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetRoles(bool loadExternalCode)
        {
            return Content(HttpStatusCode.OK, rm.GetRolesByCompany(base.ActorCompanyId, loadExternalCode).ToDTOs());
        }

        [HttpGet]
        [Route("ByCompanyAsDict/{addEmptyRow:bool}/{addEmptyRowAsAll:bool}")]
        public IHttpActionResult ByCompanyAsDict(bool addEmptyRow, bool addEmptyRowAsAll)
        {
            return Content(HttpStatusCode.OK, rm.GetRolesByCompanyDict(base.ActorCompanyId, addEmptyRow, addEmptyRowAsAll).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ByUserAsDict/{actorCompanyId:int}")]
        public IHttpActionResult ByUserAsDict(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, rm.GetRolesByUser(base.UserId, actorCompanyId).ToDictionary(k => k.RoleId, v => v.Name).ToSmallGenericTypes());
        }

        #endregion
    }
}