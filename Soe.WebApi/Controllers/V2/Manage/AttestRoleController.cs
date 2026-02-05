using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;


namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Attest/AttestRole")]
    public class AttestRoleV2Controller : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;

        #endregion

        #region Constructor

        public AttestRoleV2Controller(AttestManager am)
        {
            this.am = am;
        }

        #endregion


        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAttestRoles(int module, bool includeInactive)
        {
            SoeModule soeModule = (SoeModule)module;
            return Content(HttpStatusCode.OK, am.GetAttestRoles(base.ActorCompanyId, soeModule, includeInactive: includeInactive, loadExternalCode: true).ToDTOs());
        }

    }
}