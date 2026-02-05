using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/Attest")]
    public class AttestController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;

        #endregion

        #region Constructor

        public AttestController(AttestManager am)
        {
            this.am = am;
        }

        #endregion

        #region Role

        [HttpGet]
        [Route("AttestRole/")]
        public IHttpActionResult GetAttestRoles(HttpRequestMessage message)
        {
            SoeModule module = (SoeModule)message.GetIntValueFromQS("module");
            bool includeInactive = message.GetBoolValueFromQS("includeInactive");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, am.GetAttestRoles(base.ActorCompanyId, module, includeInactive: includeInactive, loadExternalCode: true).ToGridDTOs(base.GetTermGroupContent(TermGroup.YesNo)));
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, am.GetAttestRolesDict(base.ActorCompanyId, module, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, am.GetAttestRoles(base.ActorCompanyId, module, includeInactive: includeInactive, loadExternalCode: true).ToDTOs());
        }

        [HttpGet]
        [Route("AttestRole/{attestRoleId:int}")]
        public IHttpActionResult GetAttestRole(int attestRoleId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestRole(attestRoleId, base.ActorCompanyId, loadTransitions: true, loadExternalCode: true, loadCategories: true, loadAttestRoleMapping: true).ToDTO());
        }

        [HttpPost]
        [Route("AttestRole/")]
        public IHttpActionResult SaveAttestRole(AttestRoleDTO attestRoleDTO)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestRole(attestRoleDTO, base.ActorCompanyId, attestRoleDTO.TransitionIds));
        }

        [HttpPost]
        [Route("AttestRole/UpdateState/")]
        public IHttpActionResult UpdateAttestRoleState(UpdateAttestRoleModel attestRoleState)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.UpdateAttestRoleState(attestRoleState.Dict, base.ActorCompanyId, attestRoleState.Module));
        }


        [HttpDelete]
        [Route("AttestRole/{attestRoleId:int}")]
        public IHttpActionResult DeleteAttestRole(int attestRoleId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAttestRole(attestRoleId, base.ActorCompanyId));
        }

        #endregion

        #region Rule

        [HttpGet]
        [Route("AttestRule/")]
        public IHttpActionResult GetAttestRuleHeads(HttpRequestMessage message)
        {
            int module = message.GetIntValueFromQS("module");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, am.GetAttestRuleHeads((SoeModule)module, base.ActorCompanyId, false, loadEmployeeGroups: true).ToGridDTOs());

            bool onlyActive = message.GetBoolValueFromQS("onlyActive");
            bool loadRows = message.GetBoolValueFromQS("loadRows");
            bool loadEmployeeGroups = message.GetBoolValueFromQS("loadEmployeeGroups");

            return Content(HttpStatusCode.OK, am.GetAttestRuleHeads((SoeModule)module, base.ActorCompanyId, onlyActive, loadEmployeeGroups, loadRows).ToDTOs(loadRows, loadEmployeeGroups, true));
        }

        [HttpGet]
        [Route("AttestRule/{attestRuleHeadId:int}/{loadEmployeeGroups:bool}/{loadRows:bool}")]
        public IHttpActionResult GetAttestRuleHead(int attestRuleHeadId, bool loadEmployeeGroups, bool loadRows)
        {
            return Content(HttpStatusCode.OK, am.GetAttestRuleHead(attestRuleHeadId, loadEmployeeGroups).ToDTO(loadRows, loadEmployeeGroups, false));
        }

        [HttpPost]
        [Route("AttestRule/")]
        public IHttpActionResult SaveAttestRule(AttestRuleHeadDTO attestRuleInput)
        {
            return Ok(am.SaveAttestRuleHead(attestRuleInput, attestRuleInput.AttestRuleRows, ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestRuleState/")]
        public IHttpActionResult ChangeAttestRuleState(List<AttestRuleHeadDTO> attestRulesInput)
        {
            return Ok(am.ChangeAttestRuleStates(attestRulesInput));
        }

        [HttpDelete]
        [Route("AttestRule/{attestRuleHeadId:int}")]
        public IHttpActionResult DeleteAttestRuleHead(int attestRuleHeadId)
        {
            return Ok(am.DeleteAttestRuleHead(attestRuleHeadId));
        }

        #endregion

        #region AttestRoleUser

        [HttpGet]
        [Route("AttestRoleUser/HasAttestRoles/{dateFromString}/{dateToString}")]
        public IHttpActionResult HasAttestRoleUsers(string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, am.HasAttestRoleUsersForUser(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateFromString, true), BuildDateTimeFromString(dateToString, true)));
        }

        [HttpGet]
        [Route("AttestRoleUser/HasAttestByEmployeeAccount/{dateString}")]
        public IHttpActionResult HasAttestByEmployeeAccount(string dateString)
        {
            return Content(HttpStatusCode.OK, am.HasAttestByEmployeeAccount(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateString, true).Value));
        }

        [HttpGet]
        [Route("AttestRoleUser/HasStaffingByEmployeeAccount/{dateString}")]
        public IHttpActionResult HasStaffingByEmployeeAccount(string dateString)
        {
            return Content(HttpStatusCode.OK, am.HasStaffingByEmployeeAccount(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateString, true).Value));
        }

        [HttpGet]
        [Route("AttestRoleUser/HasAllowToAddOtherEmployeeAccounts/{dateString}")]
        public IHttpActionResult HasAllowToAddOtherEmployeeAccounts(string dateString)
        {
            return Content(HttpStatusCode.OK, am.HasAllowToAddOtherEmployeeAccounts(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateString, true).Value));
        }

        #endregion

        #region AttestTransition

        [HttpGet]
        [Route("AttestTransition/{entity:int}/{module:int}/{setEntityName:bool}")]
        public IHttpActionResult GetAttestTransitions(TermGroup_AttestEntity entity, SoeModule module, bool setEntityName)
        {
            var attestTransitions = am.GetAttestTransitions(entity, module, true, base.ActorCompanyId, setEntityName);
            if (Request.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Ok(attestTransitions.ToGridDTOs());
            return Ok(attestTransitions.ToDTOs(true));
        }

        [HttpGet]
        [Route("AttestTransition/{attestTransitionId:int}")]
        public IHttpActionResult GetAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestTransition(attestTransitionId).ToDTO(false));
        }

        [HttpGet]
        [Route("AttestTransition/User/{entity:int}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetUserAttestTransitions(TermGroup_AttestEntity entity, string dateFrom, string dateTo)
        {

            return Content(HttpStatusCode.OK, am.GetAttestTransitionsForAttestRoleUser(entity, base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true)).ToDTOs(true));
        }

        [HttpPost]
        [Route("AttestTransition/")]
        public IHttpActionResult SaveAttestTransition(AttestTransitionDTO attestTransitionDTO)
        {
            return Ok(am.SaveAttestTransition(attestTransitionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("AttestTransition/{attestTransitionId:int}")]
        public IHttpActionResult DeleteAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAttestTransition(attestTransitionId));
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
        public IHttpActionResult GetUserValidAttestStates(int entity, string dateFrom, string dateTo, bool excludePayrollStates, int? employeeGroupId)
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

        #region Attest work flow

        #region AttestWorkFlowTemplate

        [HttpGet]
        [Route("AttestWorkFlowTemplate/List/{entity:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplates(int entity)
        {
            return Ok(am.GetAttestWorkFlowTemplateHeads(base.ActorCompanyId, (TermGroup_AttestEntity)entity).ToGridDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlowTemplate/HasTemplates/{entity:int}")]
        public IHttpActionResult HasAttestWorkFlowTemplates(int entity)
        {
            return Ok(am.HasAttestWorkFlowTemplateHeads(base.ActorCompanyId, (TermGroup_AttestEntity)entity));
        }

        [HttpGet]
        [Route("AttestWorkFlowTemplate/{attestWorkFlowTemplateId:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHead(int attestWorkFlowTemplateId)
        {
            return Ok(am.GetAttestWorkFlowTemplateHead(attestWorkFlowTemplateId).ToDTO());
        }

        [HttpPost]
        [Route("AttestWorkFlowTemplate/")]
        public IHttpActionResult SaveAttestWorkFlowTemplateHead(AttestWorkFlowTemplateHeadDTO attestWorkFlowTemplateInput)
        {
            return Ok(am.SaveAttestWorkFlowTemplateHead(attestWorkFlowTemplateInput, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("AttestWorkFlowTemplate/{attestWorkFlowTemplateId:int}")]
        public IHttpActionResult DeleteAttestWorkFlowTemplateHead(int attestWorkFlowTemplateId)
        {
            return Ok(am.DeleteAttestWorkFlowTemplateHead(attestWorkFlowTemplateId));
        }

        [HttpGet]
        [Route("AttestWorkFlowTemplate/AttestWorkFlowTemplateRows/{attestWorkFlowTemplateId:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateRows(int attestWorkFlowTemplateId)
        {
            return Ok(am.GetAttestWorkFlowTemplateRows(attestWorkFlowTemplateId).ToDTOs(true));
        }

        [HttpPost]
        [Route("AttestWorkFlowTemplate/AttestWorkFlowTemplateRows/{attestWorkFlowTemplateId:int}")]
        public IHttpActionResult SaveAttestWorkFlowTemplateRows(List<AttestWorkFlowTemplateRowDTO> attestWorkFlowTemplateRows, int attestWorkFlowTemplateId)
        {
            return Ok(am.SaveAttestWorkFlowTemplateRows(attestWorkFlowTemplateRows, attestWorkFlowTemplateId));
        }

        #endregion

        #region AttestWorkFlow

        [HttpGet]
        [Route("AttestWorkFlow/AttestWorkFlowHead/{attestWorkFlowHeadId:int}/{loadRows:bool}")]
        public IHttpActionResult GetAttestWorkFlowRowsFromRecordId(int attestWorkFlowHeadId, bool loadRows)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowHead(attestWorkFlowHeadId, loadRows).ToDTO(false, false));
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestWorkFlowRows/FromRecordId/{entity:int}/{recordId:int}")]
        public IHttpActionResult GetAttestWorkFlowRowsFromRecordId(int entity, int recordId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowRowsFromRecordId((SoeEntityType)entity, recordId));
        }

        [HttpGet]
        [Route("AttestWorkFlow/GetDocumentSigningStatus/{entity:int}/{recordId:int}")]
        public IHttpActionResult GetDocumentSigningStatus(int entity, int recordId)
        {
            return Content(HttpStatusCode.OK, am.GetDocumentSigningStatus((SoeEntityType)entity, recordId, base.UserId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/InitiateDocumentSigning")]
        public IHttpActionResult InitiateDocumentSigning(AttestWorkFlowHeadDTO head)
        {
            return Content(HttpStatusCode.OK, am.InitiateDocumentSigning(head, base.ActorCompanyId, base.UserId, base.LicenseId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/SaveDocumentSigningAnswer")]
        public IHttpActionResult SaveDocumentSigningAnswer(SaveDocumentSigningAnswerModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveDocumentSigningAnswer(model.AttestWorkFlowRowId, model.SigneeStatus, model.Comment, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/CancelDocumentSigning")]
        public IHttpActionResult CancelDocumentSigning(CancelDocumentSigningModel model)
        {
            return Content(HttpStatusCode.OK, am.CancelDocumentSigning(model.AttestWorkFlowHeadId, model.Comment, base.UserId, base.ActorCompanyId));
        }

        #endregion

        #region Users

        [HttpGet]
        [Route("AttestWorkFlow/Users/ByAttestRoleMapping/{attestTransitionId:int}")]
        public IHttpActionResult GetUsersByAttestRoleMapping(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetUsersByAttestRoleMapping(attestTransitionId).ToSmallDTOs());
        }

        #endregion

        #endregion
    }
}