using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Attest/AttestState")]
    public class AttestStateController : SoeApiController
    {
        #region Variables
        private readonly AttestManager am;

        #endregion

        #region Constructor
        public AttestStateController(AttestManager am) {
            this.am = am;
        }
        #endregion


        #region AttestState

        [HttpGet]
        [Route("AttestState/{entity:int}/{module:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetAttestStates(int entity, int module, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetAttestStates(base.ActorCompanyId, (TermGroup_AttestEntity)entity, (SoeModule)module, addEmptyRow).ToDTOs());
        }

        [HttpGet]
        [Route("AttestState/{attestStateId:int}")]
        public IHttpActionResult GetAttestState(int attestStateId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestState(attestStateId, false).ToDTO());
        }

        [HttpGet]
        [Route("AttestState/Initial/{entity:int}")]
        public IHttpActionResult GetInitialAttestState(int entity)
        {
            return Content(HttpStatusCode.OK, am.GetInitialAttestState(base.ActorCompanyId, (TermGroup_AttestEntity)entity).ToDTO());
        }

        [HttpGet]
        [Route("AttestState/GenericList/{entity:int}/{module:int}/{addEmptyRow:bool}/{addMultipleRow:bool}")]
        public IHttpActionResult GetAttestStatesGenericList(int entity, int module, bool addEmptyRow, bool addMultipleRow)
        {
            return Content(HttpStatusCode.OK, am.GetAttestStatesDict(base.ActorCompanyId, (TermGroup_AttestEntity)entity, (SoeModule)module, addEmptyRow, addMultipleRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("AttestState/UserValidAttestStates/{entity:int}/{dateFrom}/{dateTo}/{excludePayrollStates:bool}/{employeeGroupId:int?}")]
        public IHttpActionResult GetUserValidAttestStates(int entity, string dateFrom, string dateTo, bool excludePayrollStates, int? employeeGroupId = null)
        {
            return Content(HttpStatusCode.OK, am.GetUserValidAttestStates((TermGroup_AttestEntity)entity, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), excludePayrollStates, employeeGroupId.HasValue ? employeeGroupId.Value.ToNullable() : null));
        }

        [HttpGet]
        [Route("AttestState/HasHiddenAttestState/{entity:int}")]
        public IHttpActionResult HasHiddenAttestState(int entity)
        {
            return Content(HttpStatusCode.OK, am.HasHiddenAttestState((TermGroup_AttestEntity)entity, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AttestState/HasInitialAttestState/{entity:int}/{module:int}")]
        public IHttpActionResult HasInitialAttestState(int entity, int module)
        {
            return Content(HttpStatusCode.OK, am.HasInitialAttestState((TermGroup_AttestEntity)entity, (SoeModule)module, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestState/")]
        public IHttpActionResult SaveAttestState(AttestStateDTO attestStateDTO)
        {
            return Ok(am.SaveAttestState(attestStateDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("AttestState/{attestStateId:int}")]
        public IHttpActionResult DeleteAttestState(int attestStateId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAttestState(attestStateId));
        }

        #endregion

        #region AttestEntity

        [HttpGet]
        [Route("AttestEntity/GenericList/{addEmptyRow:bool}/{skipUnknown:bool}/{module:int}")]
        public IHttpActionResult GetAttestEntitiesGenericList(bool addEmptyRow, bool skipUnknown, int module)
        {
            return Content(HttpStatusCode.OK, am.GetAttestEntities(addEmptyRow, skipUnknown, (SoeModule)module).ToDictionary().ToSmallGenericTypes());
        }

        #endregion


    }
}
