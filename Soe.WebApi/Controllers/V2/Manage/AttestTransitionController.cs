using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;


namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Attest/AttestTransition")]
    public class AttestTransitionController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;

        #endregion

        #region Constructor

        public AttestTransitionController(AttestManager am)
        {
            this.am = am;
        }

        #endregion

        #region AttestTransition

        [HttpGet]
        [Route("{entity:int}/{module:int}/{setEntityName:bool}")]
        public IHttpActionResult GetAttestTransitions(TermGroup_AttestEntity entity, SoeModule module, bool setEntityName)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransitions(entity, module, true, base.ActorCompanyId, setEntityName).ToDTOs(true));
        }

        [HttpGet]
        [Route("Grid/{entity:int}/{module:int}/{setEntityName:bool}")]
        public IHttpActionResult GetAttestTransitionsGrid(TermGroup_AttestEntity entity, SoeModule module, bool setEntityName)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransitions(entity, module, true, base.ActorCompanyId, setEntityName).ToGridDTOs());
        }

        [HttpGet]
        [Route("{attestTransitionId:int}")]
        public IHttpActionResult GetAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransition(attestTransitionId).ToDTO(false));
        }

        [HttpGet]
        [Route("User/{entity:int}")]
        public IHttpActionResult GetUserAttestTransitions(TermGroup_AttestEntity entity, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransitionsForAttestRoleUser(entity, base.ActorCompanyId, base.UserId, dateFrom, dateTo).ToDTOs(true));
        }

        [HttpGet]
        [Route("Dict/{entity:int}/{module:int}/{loadAttestRole:bool}")]
        public IHttpActionResult GetAttestTransitionsDict (TermGroup_AttestEntity entity, SoeModule module, bool loadAttestRole)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransitionsDict(base.ActorCompanyId, entity, module, loadAttestRole).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveAttestTransition(AttestTransitionDTO attestTransitionDTO)
        {
            return Ok(am.SaveAttestTransition(attestTransitionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{attestTransitionId:int}")]
        public IHttpActionResult DeleteAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAttestTransition(attestTransitionId));
        }

        #endregion
    }
}