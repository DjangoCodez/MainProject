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
    [RoutePrefix("Manage/Registry")]
    public class RegistryController : SoeApiController
    {
        #region Variables

        private readonly ActorManager am;
        private readonly CalendarManager cm;
        private readonly ChecklistManager clm;
        private readonly CommunicationManager com;
        private readonly GeneralManager gm;
        private readonly ScheduledJobManager sjm;
        private readonly UserManager um;

        #endregion

        #region Constructor

        public RegistryController(ActorManager am, CalendarManager cm, ChecklistManager clm, CommunicationManager com, GeneralManager gm, ScheduledJobManager sjm, UserManager um)
        {
            this.am = am;
            this.cm = cm;
            this.clm = clm;
            this.com = com;
            this.gm = gm;
            this.sjm = sjm;
            this.um = um;
        }

        #endregion

        #region Checklists

        [HttpGet]
        [Route("Checklists/ChecklistHeads/")]
        public IHttpActionResult GetChecklistHeads()
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistHeads(base.ActorCompanyId, true).ToGridDTOs());
        }

        [HttpGet]
        [Route("Checklists/ChecklistHead/{checklistHeadId:int}/{loadRows:bool}")]
        public IHttpActionResult GetChecklistHead(int checklistHeadId, bool loadRows)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistHead(checklistHeadId, base.ActorCompanyId, loadRows).ToDTO(true));
        }

        [HttpGet]
        [Route("Checklists/MultipleChoiceAnswerHeads")]
        public IHttpActionResult GetMultipleChoiceAnswerHeads()
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistMultipleChoiceHeads(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("Checklists/MultipleChoiceAnswerRows/{answerHeadId}")]
        public IHttpActionResult GetMultipleChoiceAnswerRows(int answerHeadId)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistMultipleChoiceRows(answerHeadId).ToDTOs());
        }

        [HttpPost]
        [Route("Checklists/ChecklistHead/")]
        public IHttpActionResult SaveChecklistHead(ChecklistHeadDTO checklistHeadDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, clm.SaveChecklistHead(checklistHeadDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Checklists/MultipleChoiceAnswerHeads/")]
        public IHttpActionResult SaveMultipleChoiceAnswerHead(MultipleChoiceAnswerModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, clm.SaveMultipleChoiceQuestion(model.AnswerHead, model.AnswerRows, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Checklists/ChecklistHead/{checklistHeadId:int}")]
        public IHttpActionResult DeleteChecklistHead(int checklistHeadId)
        {
            return Content(HttpStatusCode.OK, clm.DeleteChecklistHead(checklistHeadId, base.ActorCompanyId));
        }

        #endregion

        #region CompanyExternalCode

        [HttpGet]
        [Route("CompanyExternalCode/")]
        public IHttpActionResult GetCompanyExternalCodes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, am.GetCompanyExternalCodesForGrid(base.ActorCompanyId));

            return Content(HttpStatusCode.OK, am.GetCompanyExternalCodes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("CompanyExternalCode/{companyExternalCodeId:int}")]
        public IHttpActionResult GetCompanyExternalCode(int companyExternalCodeId)
        {
            return Content(HttpStatusCode.OK, am.GetCompanyExternalCode(companyExternalCodeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("CompanyExternalCode")]
        public IHttpActionResult SaveCompanyExternalCode(CompanyExternalCodeDTO companyExternalCodeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveCompanyExternalCode(companyExternalCodeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("CompanyExternalCode/{companyExternalCodeId:int}")]
        public IHttpActionResult DeleteCompanyExternalCode(int companyExternalCodeId)
        {
            return Content(HttpStatusCode.OK, am.DeleteCompanyExternalCode(companyExternalCodeId, base.ActorCompanyId));
        }

        #endregion

        #region CompanyInformation

        [HttpGet]
        [Route("CompanyInformation")]
        public IHttpActionResult GetCompanyInformations(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, gm.GetCompanyInformations(base.ActorCompanyId, false, false, true).ToGridDTOs());

            return Content(HttpStatusCode.OK, gm.GetCompanyInformations(base.ActorCompanyId, true, true, false).ToDTOs(false));
        }

        [HttpGet]
        [Route("CompanyInformation/{informationId:int}")]
        public IHttpActionResult GetCompanyInformation(int informationId)
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyInformation(informationId, base.ActorCompanyId, true, true).ToDTO(true));
        }


        [HttpGet]
        [Route("CompanyInformation/Folders")]
        public IHttpActionResult GetCompanyInformationFolders()
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyInformationFolders(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("CompanyInformation/HasConfirmations/{informationId:int}")]
        public IHttpActionResult CompanyInformationHasConfirmations(int informationId)
        {
            return Content(HttpStatusCode.OK, gm.CompanyInformationHasConfirmations(informationId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("CompanyInformation/RecipientInfo/{informationId:int}")]
        public IHttpActionResult GetCompanyInformationRecipientInfo(int informationId)
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyInformationRecipientInfo(informationId, base.ActorCompanyId, base.RoleId, base.UserId, true));
        }

        [HttpPost]
        [Route("CompanyInformation")]
        public IHttpActionResult SaveCompanyInformation(InformationDTO model)
        {
            return Content(HttpStatusCode.OK, gm.SaveCompanyInformation(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("CompanyInformation/DeleteMultiple")]
        public IHttpActionResult DeleteCompanyInformations(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, gm.DeleteCompanyInformation(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("CompanyInformation/DeleteNotificationSent/{informationId:int}")]
        public IHttpActionResult DeleteCompanyInformationNotificationSent(int informationId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteCompanyInformationNotificationSent(informationId, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("CompanyInformation/{informationId:int}")]
        public IHttpActionResult DeleteCompanyInformation(int informationId)
        {
            List<int> informationIds = new List<int>();
            informationIds.Add(informationId);

            return Content(HttpStatusCode.OK, gm.DeleteCompanyInformation(informationIds, base.ActorCompanyId));
        }

        #endregion

        #region MessageGroups

        [HttpGet]
        [Route("MessageGroup/")]
        public IHttpActionResult GetMessageGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, com.GetMessageGroupsDict(base.ActorCompanyId, base.UserId));
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, com.GetMessageGroupsForGrid(base.ActorCompanyId, base.UserId).ToGridDTOs());

            return Content(HttpStatusCode.OK, com.GetMessageGroups(base.ActorCompanyId, base.UserId));
        }

        [HttpGet]
        [Route("MessageGroup/UsersByAccount/{accountId:int}")]
        public IHttpActionResult GetMessageGroupUsersByAccount(int accountId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByAccounts(base.ActorCompanyId, accountId.ObjToList()).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("MessageGroup/UsersByCategory/{categoryId:int}")]
        public IHttpActionResult GetMessageGroupUsersByCategory(int categoryId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByCategory(base.ActorCompanyId, base.RoleId, base.UserId, categoryId).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("MessageGroup/UsersByEmployeeGroup/{employeeGroupId:int}")]
        public IHttpActionResult GetMessageGroupUsersByEmployeeGroup(int employeeGroupId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByEmployeeGroup(base.ActorCompanyId, base.RoleId, base.UserId, employeeGroupId).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("MessageGroup/UsersByRole/{roleId:int}")]
        public IHttpActionResult GetMessageGroupUsersByRole(int roleId)
        {
            return Content(HttpStatusCode.OK, um.GetUsersByRole(base.ActorCompanyId, roleId, base.UserId).ToDict(false, false, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("MessageGroup/{messageGroupId:int}")]
        public IHttpActionResult GetMessageGroup(int messageGroupId)
        {
            return Content(HttpStatusCode.OK, com.GetMessageGroup(messageGroupId).ToDTO(true));
        }

        [HttpPost]
        [Route("MessageGroup")]
        public IHttpActionResult SaveMessageGroup(MessageGroupDTO messageGroupDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, com.SaveMessageGroup(messageGroupDTO));
        }

        [HttpDelete]
        [Route("MessageGroup/{messageGroupId:int}")]
        public IHttpActionResult DeleteMessageGroup(int messageGroupId)
        {
            return Content(HttpStatusCode.OK, com.DeleteMessageGroup(messageGroupId));
        }

        #endregion

        #region OpeningHours

        [HttpGet]
        [Route("OpeningHours/")]
        public IHttpActionResult GetOpeningHours(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, cm.GetOpeningHoursDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("includeDateInName")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, cm.GetOpeningHoursForCompany(base.ActorCompanyId, message.GetDateValueFromQS("fromDate"), message.GetDateValueFromQS("toDate")).ToDTOs());
        }

        [HttpGet]
        [Route("OpeningHours/{openingHoursId:int}")]
        public IHttpActionResult GetOpeningHour(int openingHoursId)
        {
            return Content(HttpStatusCode.OK, cm.GetOpeningHours(openingHoursId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("OpeningHours")]
        public IHttpActionResult SaveOpeningHours(OpeningHoursDTO openingHoursDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.SaveOpeningHours(openingHoursDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("OpeningHours/{openingHoursId:int}")]
        public IHttpActionResult DeleteOpeningHours(int openingHoursId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteOpeningHours(openingHoursId, base.ActorCompanyId));
        }

        #endregion

        #region ScheduledJobHead

        [HttpGet]
        [Route("ScheduledJobHead/")]
        public IHttpActionResult GetScheduledJobHeads(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, sjm.GetScheduledJobHeadsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("includeSharedOnLicense")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, sjm.GetScheduledJobHeads(base.ActorCompanyId).ToGridDTOs());

            return Content(HttpStatusCode.OK, sjm.GetScheduledJobHeads(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("ScheduledJobHead/{scheduledJobHeadId:int}/{loadRows:bool}/{loadLogs:bool}/{loadSettings:bool}/{loadSettingOptions:bool}/{setRecurrenceIntervalText:bool}/{setTimeIntervalText:bool}")]
        public IHttpActionResult GetScheduledJobHead(int scheduledJobHeadId, bool loadRows, bool loadLogs, bool loadSettings, bool loadSettingOptions, bool setRecurrenceIntervalText, bool setTimeIntervalText)
        {
            return Content(HttpStatusCode.OK, sjm.GetScheduledJobHead(scheduledJobHeadId, base.ActorCompanyId, loadRows, loadLogs, loadSettings, loadSettingOptions, setRecurrenceIntervalText, setTimeIntervalText).ToDTO());
        }

        [HttpPost]
        [Route("ScheduledJobHead")]
        public IHttpActionResult SaveScheduledJobHead(ScheduledJobHeadDTO headDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sjm.SaveScheduledJobHead(headDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ScheduledJobHead/{scheduledJobHeadId:int}")]
        public IHttpActionResult DeleteScheduledJobHead(int scheduledJobHeadId)
        {
            return Content(HttpStatusCode.OK, sjm.DeleteScheduledJobHead(scheduledJobHeadId, base.ActorCompanyId));
        }

        #endregion

        #region ScheduledJobLog

        [HttpGet]
        [Route("ScheduledJobLog/{scheduledJobHeadId:int}/{setLogLevelName:bool}/{setStatusName:bool}")]
        public IHttpActionResult GetScheduledJobLogs(int scheduledJobHeadId, bool setLogLevelName, bool setStatusName)
        {
            return Content(HttpStatusCode.OK, sjm.GetScheduledJobLogs(scheduledJobHeadId, base.ActorCompanyId, setLogLevelName, setStatusName).ToDTOs());
        }

        #endregion

        #region ScheduledJobSetting

        [HttpGet]
        [Route("ScheduledJobSetting/GetScheduledJobSettingOptions/{settingType:int}")]
        public IHttpActionResult GetScheduledJobSettingOptions(int settingType)
        {
            return Content(HttpStatusCode.OK, sjm.GetScheduledJobSettingOptions(settingType, base.ActorCompanyId));
        }


        #endregion
    }
}