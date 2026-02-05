using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting.Matrix;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Time
{
    [RoutePrefix("Time/Schedule")]
    public class ScheduleController : SoeApiController
    {
        private readonly bool useSkillsPoc = false;

        #region Variables

        private readonly AccountManager am;
        private readonly AnnualLeaveManager alm;
        private readonly CalendarManager cm;
        private readonly CommunicationManager ccm;
        private readonly EmployeeManager em;
        private readonly ReportManager rm;
        private readonly TimeCodeManager tcm;
        private readonly TimeDeviationCauseManager tdcm;
        private readonly TimeEngineManager tem;
        private readonly TimeMatrixDataManager tmdm;
        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public ScheduleController(AccountManager am, AnnualLeaveManager alm, CalendarManager cm, CommunicationManager ccm, EmployeeManager em, ReportManager rm, TimeCodeManager tcm, TimeDeviationCauseManager tdcm, TimeEngineManager tem, TimeMatrixDataManager tmdm, TimeScheduleManager tsm)
        {
            this.am = am;
            this.alm = alm;
            this.cm = cm;
            this.ccm = ccm;
            this.em = em;
            this.rm = rm;
            this.tcm = tcm;
            this.tdcm = tdcm;
            this.tem = tem;
            this.tmdm = tmdm;
            this.tsm = tsm;
        }

        #endregion

        #region Absence/Absencerequests

        #region Get shifts

        [HttpPost]
        [Route("Absence/AffectedShifts")]
        public IHttpActionResult GetAbsenceAffectedShifts(GetAbsenceAffectedShiftsModel model)
        {
            ExtendedAbsenceSetting extendedAbsenceSettings = null;
            if (model.ExtendedSettings != null)
                extendedAbsenceSettings = model.ExtendedSettings.FromDTO();
            return Content(HttpStatusCode.OK, tsm.GetAbsenceAffectedShifts(base.ActorCompanyId, base.UserId, model.EmployeeId, model.TimeScheduleScenarioHeadId.ToNullable(), model.DateFrom, model.DateTo, model.TimeDeviationCauseId, extendedAbsenceSettings, model.IncludeAlreadyAbsence));

        }

        [HttpPost]
        [Route("Absence/SelectedDays")]
        public IHttpActionResult GetAbsenceAffectedShiftsFromSelectedDays(GetAbsenceAffectedShiftsModel model)
        {

            return Content(HttpStatusCode.OK, tsm.GetAbsenceAffectedShifts(base.ActorCompanyId, base.UserId, model.EmployeeId, model.TimeScheduleScenarioHeadId.ToNullable(), model.TimeDeviationCauseId, model.SelectedDays));

        }

        [HttpPost]
        [Route("Absence/Shifts")]
        public IHttpActionResult GetAbsenceAffectedShiftsFromShift(GetAbsenceAffectedShiftsModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetShifts(base.ActorCompanyId, model.EmployeeId, model.ShiftId, model.TimeScheduleScenarioHeadId.ToNullable(), model.IncludeLinkedShifts, model.GetAllshifts, model.TimeDeviationCauseId));
        }

        [HttpPost]
        [Route("Absencerequest/AffectedShifts")]
        public IHttpActionResult GetAbsenceRequestAffectedShifts(GetAbsenceRequestAffectedShiftsModel model)
        {
            model.Request.Start = CalendarUtility.GetBeginningOfDay(model.Request.Start);
            model.Request.Stop = CalendarUtility.GetEndOfDay(model.Request.Stop);
            return Content(HttpStatusCode.OK, tsm.GetAbsenceRequestAffectedShifts(base.ActorCompanyId, base.UserId, model.TimeScheduleScenarioHeadId.ToNullable(), model.Request, model.ShiftUserStatus, model.ExtendedSettings.FromDTO()));

        }

        #endregion

        [HttpPost]
        [Route("Absence/PerformPlanning")]
        public IHttpActionResult PerformAbsencePlanningAction(PerformAbsencePlanningActionModel model)
        {
            return Content(HttpStatusCode.OK, tem.GenerateAndSaveAbsenceFromStaffing(model.EmployeeRequest, model.Shifts, model.ScheduledAbsence, model.SkipXEMailOnShiftChanges, model.TimeScheduleScenarioHeadId.ToNullable()));
        }

        [HttpPost]
        [Route("Absence/CheckShiftsIncludedInAbsenceRequestWarningMessage")]
        public IHttpActionResult GetShiftsIsIncludedInAbsenceRequestWarningMessage(CheckShiftsIncludedInAbsenceRequestModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftsIsIncludedInAbsenceRequestWarningMessage(model.EmployeeId, model.Shifts));
        }

        [HttpPost]
        [Route("Scenario/Absence/Remove")]
        public IHttpActionResult RemoveAbsenceInScenario(RemoveAbsenceInScenarioModel model)
        {
            return Content(HttpStatusCode.OK, tem.RemoveAbsenceInScenario(model.Items, model.TimeScheduleScenarioHeadId));
        }

        [HttpGet]
        [Route("Absencerequest/{employeeId:int}/{loadPreliminary:bool}/{loadDefinitive:bool}")]
        public IHttpActionResult GetAbsenceRequests(int employeeId, bool loadPreliminary, bool loadDefinitive)
        {
            EmployeeRequestStatus status;
            if (loadPreliminary && loadDefinitive)
                status = EmployeeRequestStatus.PreliminaryAndDefinitive;
            else
                status = loadPreliminary ? EmployeeRequestStatus.Preliminary : EmployeeRequestStatus.Definitive;

            switch (status)
            {
                case EmployeeRequestStatus.Preliminary:
                    return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).Where(e => e.Status != TermGroup_EmployeeRequestStatus.Definate));
                case EmployeeRequestStatus.Definitive:
                    return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).Where(e => e.Status == TermGroup_EmployeeRequestStatus.Definate));
                case EmployeeRequestStatus.PreliminaryAndDefinitive:
                    return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }));
                default:
                    return Content(HttpStatusCode.OK, new List<EmployeeRequestDTO>());
            }
        }

        [HttpGet]
        [Route("Absencerequest/{employeeRequestId:int}")]
        public IHttpActionResult GetAbsenceRequest(int employeeRequestId)
        {
            return Content(HttpStatusCode.OK, tem.LoadEmployeeRequest(employeeRequestId).ToDTO(true, true));

        }

        [HttpGet]
        [Route("Absencerequest/Interval/{employeeId:int}/{start}/{stop}/{requestType:int}")]
        public IHttpActionResult GetEmployeeRequestFromDateInterval(int employeeId, string start, string stop, TermGroup_EmployeeRequestType requestType)
        {
            return Content(HttpStatusCode.OK, tem.LoadEmployeeRequest(employeeId, BuildDateTimeFromString(start, true).Value, BuildDateTimeFromString(stop, true).Value, requestType).ToDTO(true, true));

        }

        [HttpPost]
        [Route("Absencerequest/PerformPlanning")]
        public IHttpActionResult PerformAbsenceRequestPlanningAction(PerformAbsenceRequestPlanningActionModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.PerformAbsenceRequestPlanningAction(model.EmployeeRequestId, model.Shifts, model.SkipXEMailOnShiftChanges, model.TimeScheduleScenarioHeadId.ToNullable()));
        }

        [HttpPost]
        [Route("Absencerequest")]
        public IHttpActionResult SaveEmployeeRequest(SaveAbsenceRequestModel model)
        {
            model.Request.ActorCompanyId = base.ActorCompanyId;
            return Content(HttpStatusCode.OK, tem.SaveEmployeeRequest(model.Request.FromDTO(), model.EmployeeId, model.RequestType, model.SkipXEMailOnShiftChanges, model.IsForcedDefinitive));
        }

        [HttpPost]
        [Route("Absencerequest/ValidateDeviationCausePolicy")]
        public IHttpActionResult ValidateTimeDeviationCausePolicy(SaveAbsenceRequestModel model)
        {
            model.Request.ActorCompanyId = base.ActorCompanyId;
            return Content(HttpStatusCode.OK, tdcm.ValidateTimeDeviationCausePolicy(base.ActorCompanyId, model.Request));
        }

        [HttpPost]
        [Route("Absencerequest/History/PerformRestore/{employeeRequestId:int}/{setRequestAsPending:bool}")]
        public IHttpActionResult PerformRestoreAbsenceRequestedShifts(int employeeRequestId, bool setRequestAsPending)
        {
            return Content(HttpStatusCode.OK, tem.PerformRestoreAbsenceRequestedShifts(employeeRequestId, setRequestAsPending));
        }

        [HttpPost]
        [Route("Absencerequest/WorkRules")]
        public IHttpActionResult EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(EvaluateAbsenceRequestPlanningAgainstWorkRules model)
        {
            return Content(HttpStatusCode.OK, tem.EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(model.Shifts, model.EmployeeId, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.Rules));
        }

        [HttpDelete]
        [Route("Absencerequest/{employeeRequestId:int}")]
        public IHttpActionResult DeleteEmployeeRequest(int employeeRequestId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteEmployeeRequest(employeeRequestId));
        }

        [HttpGet]
        [Route("Absencerequest/History/{absenceRequestId:int}")]
        public IHttpActionResult GetAbsenceRequestHistory(int absenceRequestId)
        {
            return Content(HttpStatusCode.OK, tsm.GetAbsenceRequestHistory(base.ActorCompanyId, absenceRequestId));
        }

        #endregion

        #region AccountDim

        [HttpGet]
        [Route("Planning/AccountDim/{onlyDefaultAccounts:bool}/{includeAbstractAccounts:bool}/{displayMode:int}/{filterOnHierarchyHideOnSchedule:bool}")]
        public IHttpActionResult GetAccountDimsForPlanning(bool onlyDefaultAccounts, bool includeAbstractAccounts, TimeSchedulePlanningDisplayMode displayMode, bool filterOnHierarchyHideOnSchedule = false)
        {
            return Content(HttpStatusCode.OK, am.GetAccountDimsForPlanning(base.ActorCompanyId, base.UserId, onlyDefaultAccounts: onlyDefaultAccounts, useEmployeeAccountIfNoAttestRole: true, includeAbstractAccounts: includeAbstractAccounts, isMobile: true, displayMode: displayMode, filterOnHierarchyHideOnSchedule: filterOnHierarchyHideOnSchedule));
        }

        [HttpGet]
        [Route("Planning/AccountDim/DefaultEmployeeAccountDim")]
        public IHttpActionResult GetDefaultEmployeeAccountDim()
        {
            return Content(HttpStatusCode.OK, am.GetDefaultEmployeeAccountDim(base.ActorCompanyId).ToSmallDTO(includeAccounts: true));
        }

        [HttpGet]
        [Route("Planning/AccountDim/DefaultEmployeeAccountDimAndSelectableAccounts/{employeeId:int}/{date}")]
        public IHttpActionResult GetDefaultEmployeeAccountDimAndSelectableAccounts(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, am.GetDefaultEmployeeAccountDimAndSelectableAccounts(base.ActorCompanyId, base.UserId, employeeId, BuildDateTimeFromString(date, true).Value));
        }

        #endregion

        #region Annual leave

        [HttpGet]
        [Route("AnnualLeaveShift/Length/{dateString}/{employeeId:int}")]
        public IHttpActionResult GetAnnualLeaveShiftLength(string dateString, int employeeId)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveShiftLengthForEmployee(BuildDateTimeFromString(dateString, true).Value, employeeId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AnnualLeaveShift")]
        public IHttpActionResult CreateAnnualLeaveShift(CreateAnnualLeaveShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, alm.CreateAnnualLeaveShift(model.Date, model.EmployeeId, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("AnnualLeaveShift/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult DeleteAnnualLeaveShift(int timeScheduleTemplateBlockId)
        {
            return Content(HttpStatusCode.OK, alm.DeleteAnnualLeaveShift(timeScheduleTemplateBlockId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AnnualLeave/CalculateTransactions")]
        public IHttpActionResult CalculateAnnualLeaveTransactions(AnnualLeaveCalculationModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, alm.CalculateAnnualLeaveTransactions(base.ActorCompanyId, model.EmployeeIds, model.DateFrom, model.DateTo));
        }

        [HttpPost]
        [Route("AnnualLeave/Balance")]
        public IHttpActionResult GetAnnualLeaveBalance(GetAnnualLeaveBalanceModel model)
        {
            return Content(HttpStatusCode.OK, alm.GetAnnualLeaveBalance(model.Date, model.EmployeeIds, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AnnualLeave/Balance/Recalculate")]
        public IHttpActionResult RecalculateAnnualLeaveBalance(GetAnnualLeaveBalanceModel model)
        {
            return Content(HttpStatusCode.OK, alm.RecalculateAnnualLeaveBalance(model.Date, model.EmployeeIds, base.ActorCompanyId, model.PreviousYear));
        }

        #endregion

        #region Annual scheduled time

        [HttpGet]
        [Route("AnnualScheduledTime/{employeeId:int}/{dateFrom}/{dateTo}/{type:int}")]
        public IHttpActionResult GetAnnualScheduledTimeSummaryForEmployee(int employeeId, string dateFrom, string dateTo, TimeScheduledTimeSummaryType type)
        {
            return Content(HttpStatusCode.OK, tsm.GetScheduledTimeSummaryTotalWithinEmployments(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, type));
        }

        [HttpGet]
        [Route("AnnualScheduledTime/Update/{employeeId:int}/{dateFrom}/{dateTo}/{returnResult:bool}")]
        public IHttpActionResult UpdateScheduledTimeSummary(int employeeId, string dateFrom, string dateTo, bool returnResult)
        {
            return Content(HttpStatusCode.OK, tsm.UpdateScheduledTimeSummary(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, returnResult));
        }

        [HttpPost]
        [Route("AnnualScheduledTime/")]
        public IHttpActionResult GetAnnualScheduledTimeSummary(GetAnnualScheduledTimeSummaryModel model)
        {
            return Content(HttpStatusCode.OK, em.GetScheduledTimeSummary(base.ActorCompanyId, model.EmployeeIds, model.DateFrom, model.DateTo, model.TimePeriodHeadId));
        }

        #endregion

        #region EmployeePeriodTimeSummary

        [HttpPost]
        [Route("EmployeePeriodTimeSummary/")]
        public IHttpActionResult GetEmployeePeriodTimeSummary(GetEmployeePeriodTimeSummary model)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeePeriodTimeSummaries(base.ActorCompanyId, model.EmployeeIds, model.DateFrom, model.DateTo, model.TimePeriodHeadId ?? 0));
        }

        #endregion

        #region Annual work time

        [HttpGet]
        [Route("AnnualWorkTime/{employeeId:int}/{dateFrom}/{dateTo}/{timePeriodHeadId:int}")]
        public IHttpActionResult GetAnnualWorkTime(int employeeId, string dateFrom, string dateTo, int timePeriodHeadId)
        {
            return Content(HttpStatusCode.OK, em.GetAnnualWorkTimeMinutes(BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, employeeId, base.ActorCompanyId, null, null, timePeriodHeadId));
        }

        #endregion

        #region DayWeek

        [HttpGet]
        [Route("DayWeek")]
        public IHttpActionResult GetDaysOfWeekDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetDaysOfWeekDict(addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region DayType

        [HttpGet]
        [Route("DayType")]
        public IHttpActionResult GetDayTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, cm.GetDayTypesByCompanyDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, cm.GetDayTypesByCompany(base.ActorCompanyId).ToDTOs());
        }


        [HttpGet]
        [Route("DayType/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetDayTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesByCompanyDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("DayType/{dayTypeId:int}")]
        public IHttpActionResult GetDayType(int dayTypeId)
        {
            return Content(HttpStatusCode.OK, cm.GetDayType(dayTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("DayType")]
        public IHttpActionResult SaveDayType(DayTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.SaveDayType(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("DayType/{dayTypeId:int}")]
        public IHttpActionResult DeleteDayType(int dayTypeId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteDayType(dayTypeId));
        }

        [HttpGet]
        [Route("DayTypeAndWeekday")]
        public IHttpActionResult GetDayTypesAndWeekdays()
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesAndWeekdays(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("HalfDay")]
        public IHttpActionResult GetHalfdays()
        {
            return Content(HttpStatusCode.OK, cm.GetTimeHalfdays(base.ActorCompanyId, true).ToDTOs(true));
        }

        [HttpGet]
        [Route("HalfDayTypeDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetHalfDayTypeDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetTimeHalfdayTypesDict(addEmptyRow));
        }

        #endregion

        #region Employee

        [HttpGet]
        [Route("AvailableEmployeeIds/{dateFrom}/{dateTo}/{isTemplate:bool}")]
        public IHttpActionResult GetAvailableEmployeeIds(string dateFrom, string dateTo, bool isTemplate)
        {
            return Content(HttpStatusCode.OK, tsm.GetAvailableEmployeeIds(base.ActorCompanyId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, null, isTemplate));
        }

        [HttpGet]
        [Route("AvailableEmployeeIds/{dateFrom}/{dateTo}/{isTemplate:bool}/{preliminary:bool}")]
        public IHttpActionResult GetAvailableEmployeeIds(string dateFrom, string dateTo, bool isTemplate, bool preliminary)
        {
            return Content(HttpStatusCode.OK, tsm.GetAvailableEmployeeIds(base.ActorCompanyId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, preliminary, isTemplate));
        }

        [HttpGet]
        [Route("HasEmployeeSchedule/{employeeId:int}/{dateString}")]
        public IHttpActionResult HasEmployeeSchedule(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, tsm.HasEmployeeSchedule(employeeId, BuildDateTimeFromString(dateString, true).Value));
        }

        [HttpPost]
        [Route("GetCyclePlannedMinutes/")]
        public IHttpActionResult GetCyclePlannedMinutes(GetCycleWorkTimeMinutesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.GetCyclePlannedMinutes(null, model.Date, model.EmployeeIds));
        }

        [HttpPost]
        [Route("EmployeesForDefToFromPrelShift/")]
        public IHttpActionResult GetEmployeesForDefToFromPrelShift(DefToFromPrelShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.GetEmployeesForDefToFromPrelShiftDialog(model.PrelToDef, model.DateFrom, model.DateTo, base.ActorCompanyId, model.EmployeeId, base.RoleId, base.UserId, model.EmployeeIds));
        }

        [HttpPost]
        [Route("AvailableEmployees/")]
        public IHttpActionResult GetAvailableEmployees(GetAvailableEmployeesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.GetAvailableEmployees(model.TimeScheduleTemplateBlockIds, model.EmployeeIds, model.FilterOnShiftType, model.FilterOnAvailability, model.FilterOnSkills, model.FilterOnWorkRules, model.FilterOnMessageGroupId, true, null, false, false));
        }

        #endregion

        #region EmployeePost

        [HttpGet]
        [Route("EmployeePost/")]
        public IHttpActionResult GetEmployeePosts(HttpRequestMessage message)
        {
            bool? active = message.GetNullableBoolValueFromQS("active");

            if (Request.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetEmployeePostsDict(base.ActorCompanyId, active, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetEmployeePosts(base.ActorCompanyId, active, message.GetBoolValueFromQS("loadRelations"), true, true).ToDTOs(true));
        }

        [HttpGet]
        [Route("EmployeePost/{employeePostId:int}")]
        public IHttpActionResult GetEmployeePost(int employeePostId)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeePost(employeePostId, true, true, true, true).ToDTO(true));
        }

        [HttpGet]
        [Route("EmployeePost/Employments/{selectedDate}")]
        public IHttpActionResult GetEmployments(string selectedDate)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmploymentsForCreatingEmployeePosts(BuildDateTimeFromString(selectedDate, true).Value, base.ActorCompanyId, base.UserId, base.RoleId).ToGridDTOs(true, false));
        }

        [HttpGet]
        [Route("EmployeePost/TemplateShift/{employeePostId:int}/{date}/{loadYesterdayAlso:bool}/{loadTasks:bool}")]
        public IHttpActionResult GetEmployeePostTemplateShiftsForDay(int employeePostId, string date, bool loadYesterdayAlso, bool loadTasks)
        {
            DateTime dateTime = BuildDateTimeFromString(date, true).Value;

            return Content(HttpStatusCode.OK, tsm.GetTemplateShiftsForEmployeePost(base.ActorCompanyId, base.RoleId, base.UserId, dateTime, dateTime, loadYesterdayAlso, new List<int> { employeePostId }, loadTasks));
        }

        [HttpGet]
        [Route("EmployeePost/Status/{employeePostId:int}")]
        public IHttpActionResult GetEmployeePostStatus(int employeePostId)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeePostStatus(employeePostId));
        }

        [HttpPost]
        [Route("EmployeePost")]
        public IHttpActionResult SaveEmployeePost(EmployeePostDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveEmployeePost(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("EmployeePost/UpdateState")]
        public IHttpActionResult UpdateEmployeePostState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.UpdateEmployeePostsState(model.Dict));
        }

        [HttpPost]
        [Route("EmployeePost/CreateEmployeePostsFromEmployments")]
        public IHttpActionResult CreateEmployeePostsFromEmployments(CreateEmployeePostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateEmployeePostsFromEmployments(base.ActorCompanyId, model.Numbers, model.FromDate));
        }

        [HttpPost]
        [Route("EmployeePost/ChangeStatus")]
        public IHttpActionResult ChangeEmployeePostStatus(EmployeePostChangeStatusModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.ChangeEmployeePostStatus(model.EmployeePostId, model.Status));
        }

        [HttpPost]
        [Route("EmployeePost/TemplateShift/Search")]
        public IHttpActionResult GetTemplateShiftsForEmployeePost(GetEmployeePostShiftsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTemplateShiftsForEmployeePost(base.ActorCompanyId, base.RoleId, base.UserId, model.DateFrom, model.DateTo, false, model.EmployeePostIds, model.LoadTasks));
        }

        [HttpPost]
        [Route("EmployeePost/DeleteMultiple")]
        public IHttpActionResult DeleteEmployeePosts(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.DeleteEmployeePosts(model.Numbers));
        }

        [HttpDelete]
        [Route("EmployeePost/{employeePostId:int}")]
        public IHttpActionResult DeleteEmployeePost(int employeePostId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteEmployeePost(employeePostId));
        }

        #endregion

        #region EmployeeRequest (Availability)

        [HttpGet]
        [Route("EmployeeRequest/{employeeId:int}/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetEmployeeRequests(int employeeId, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.InterestRequest, TermGroup_EmployeeRequestType.NonInterestRequest }, BuildDateTimeFromString(dateFromString, true), BuildDateTimeFromString(dateToString, true)));
        }

        [HttpPost]
        [Route("EmployeeRequest")]
        public IHttpActionResult SaveEmployeeRequests(SaveEmployeeRequestModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveEmployeeRequest(model.EmployeeId, model.DeletedEmployeeRequests, model.EditedOrNewRequests));
        }

        #endregion

        #region EmployeeSchedule

        [HttpGet]
        [Route("EmployeeSchedule/GetEmployeeScheduleForEmployee/{dateString}/{employeeId:int}")]
        public IHttpActionResult GetEmployeeScheduleForEmployee(string dateString, int employeeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetPlacementForEmployee(BuildDateTimeFromString(dateString, true).Value, employeeId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("EmployeeSchedule/GetLastEmployeeScheduleForEmployee/{employeeId:int}/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult GetLastEmployeeScheduleForEmployee(int employeeId, int timeScheduleTemplateHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetLastPlacementForEmployee(employeeId, base.ActorCompanyId, timeScheduleTemplateHeadId != 0 ? timeScheduleTemplateHeadId : (int?)null).ToDTO());
        }

        [HttpPost]
        [Route("EmployeeSchedule/ForActivateGrid")]
        public IHttpActionResult GetEmployeeScheduleForActivateGrid(GetEmployeeScheduleForActivateGridModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetPlacementsForGrid(base.RoleId, model.OnlyLatest, model.AddEmptyPlacement, model.EmployeeIds, dateFrom: model.DateFrom, dateTo: model.DateTo));
        }

        [HttpPost]
        [Route("EmployeeSchedule/ValidateShortenEmployment")]
        public IHttpActionResult ValidateShortenEmployment(HasEmployeeOverlappingPlacementModel model)
        {
            return Content(HttpStatusCode.OK, tsm.ValidateShortenEmployment(model.EmployeeId, model.OldDateFrom, model.OldDateTo, model.NewDateFrom, model.NewDateTo, !model.ApplyFinalSalary, model.ChangedEmployment, model.EmployeePlacements, model.ScheduledEmployeePlacements, model.Employments));
        }

        [Route("EmployeeSchedule/ControlActivations")]
        public IHttpActionResult ControlActivations(ControlActivationsModel model)
        {
            return Content(HttpStatusCode.OK, tem.ControlEmployeeSchedulePlacements(model.Items, model.StartDate, model.StopDate, model.IsDelete));
        }

        [Route("EmployeeSchedule/ControlActivation")]
        public IHttpActionResult ControlActivation(ControlActivationModel model)
        {
            return Content(HttpStatusCode.OK, tem.ControlEmployeeSchedulePlacement(model.EmployeeId, model.EmployeeScheduleStartDate, model.EmployeeScheduleStopDate, model.StartDate, model.StopDate, model.IsDelete));
        }

        [HttpPost]
        [Route("EmployeeSchedule/Activate")]
        public IHttpActionResult ActivateSchedule(SaveEmployeeScheduleModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveEmployeeSchedulePlacement(model.Control, model.Items, model.Function, model.StartDate, model.StopDate, model.TimeScheduleTemplateHeadId, model.TimeScheduleTemplatePeriodId, model.Preliminary));
        }

        [HttpPost]
        [Route("EmployeeSchedule/Delete")]
        public IHttpActionResult DeletePlacement(DeletePlacementModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteEmployeeSchedulePlacement(model.Item, model.Control));
        }

        #endregion

        #region Holiday

        [HttpGet]
        [Route("Holiday")]
        public IHttpActionResult GetHolidays()
        {
            return Content(HttpStatusCode.OK, cm.GetHolidaysByCompany(base.ActorCompanyId, loadDayType: true).ToGridDTOs());
        }

        [HttpGet]
        [Route("Holiday/Small/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetHolidaysSmall(string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, cm.GetHolidaysByCompanySmall(base.ActorCompanyId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value));
        }

        [HttpGet]
        [Route("Holiday/SysHolidayTypes")]
        public IHttpActionResult GetHolidayTypes()
        {
            return Content(HttpStatusCode.OK, cm.GetSysHolidayTypeDTOs());
        }

        [HttpGet]
        [Route("Holiday/SysHolidayTypes/Dict")]
        public IHttpActionResult GetHolidayTypesDict()
        {
            return Content(HttpStatusCode.OK, cm.GetSysHolidayTypeDTOs().ToDictionary(x => x.SysHolidayTypeId, x => x.Name).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Holiday/{holidayId:int}")]
        public IHttpActionResult GetHoliday(int holidayId)
        {
            return Content(HttpStatusCode.OK, cm.GetHoliday(holidayId, base.ActorCompanyId).ToDTO(true));
        }

        [HttpPost]
        [Route("Holiday")]
        public IHttpActionResult SaveHoliday(HolidayDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.AddHoliday(model, base.ActorCompanyId, model.DayTypeId));
        }

        [HttpDelete]
        [Route("Holiday/{holidayId:int}")]
        public IHttpActionResult DeleteHoliday(int holidayId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteHoliday(holidayId, base.ActorCompanyId));
        }

        #endregion

        #region IncomingDelivery

        [HttpGet]
        [Route("IncomingDelivery/{loadRows:bool}/{loadAccounts:bool}/{loadAccountHierarchyAccount:bool}")]
        public IHttpActionResult GetIncomingDeliveries(bool loadRows, bool loadAccounts, bool loadAccountHierarchyAccount)
        {
            if (Request.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveries(base.ActorCompanyId, true, false, false, true).ToGridDTOs());

            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveries(base.ActorCompanyId, true, loadRows, loadAccounts, loadAccountHierarchyAccount).ToDTOs(loadRows, loadAccounts));
        }

        [HttpGet]
        [Route("IncomingDelivery/GetIncomingDeliveriesForInterval")]
        public IHttpActionResult GetIncomingDeliveriesForInterval(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveries(base.ActorCompanyId, message.GetDateValueFromQS("dateFrom").Value, message.GetDateValueFromQS("dateTo").Value, message.GetIntListValueFromQS("ids"), loadAccounting: true, loadDeliveryType: true).ToDTOs(true, true));
        }

        [HttpGet]
        [Route("IncomingDelivery/{incomingDeliveryHeadId:int}/{loadRows:bool}/{loadAccounts:bool}/{loadExcludedDates:bool}/{loadAccountHierarchyAccount:bool}")]
        public IHttpActionResult GetIncomingDelivery(int incomingDeliveryHeadId, bool loadRows, bool loadAccounts, bool loadExcludedDates, bool loadAccountHierarchyAccount)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDelivery(incomingDeliveryHeadId, base.ActorCompanyId, loadAccounts, loadRows, false, false, loadExcludedDates, loadAccountHierarchyAccount).ToDTO(loadRows, loadAccounts));
        }

        [HttpPost]
        [Route("IncomingDelivery")]
        public IHttpActionResult SaveIncomingDelivery(IncomingDeliveryHeadDTO incomingDeliveryHeadDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveIncomingDelivery(incomingDeliveryHeadDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("IncomingDelivery/{incomingDeliveryHeadId:int}")]
        public IHttpActionResult DeleteIncomingDelivery(int incomingDeliveryHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteIncomingDelivery(incomingDeliveryHeadId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("IncomingDeliveryRow/{incomingDeliveryHeadId:int}/{loadAccounts:bool}")]
        public IHttpActionResult GetIncomingDeliveryRows(int incomingDeliveryHeadId, bool loadAccounts)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryRows(incomingDeliveryHeadId, loadAccounts).ToDTOs(loadAccounts));
        }

        #endregion

        #region IncomingDeliveryType

        [HttpGet]
        [Route("IncomingDeliveryType/")]
        public IHttpActionResult GetIncomingDeliveryTypes(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryTypes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("IncomingDeliveryType/{incomingDeliveryTypeId:int}")]
        public IHttpActionResult GetIncomingDeliveryType(int incomingDeliveryTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryType(incomingDeliveryTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("IncomingDeliveryType")]
        public IHttpActionResult SaveIncomingDeliveryType(IncomingDeliveryTypeDTO incomingDeliveryTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveIncomingDeliveryType(incomingDeliveryTypeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("IncomingDeliveryType/{incomingDeliveryTypeId:int}")]
        public IHttpActionResult DeleteIncomingDeliveryType(int incomingDeliveryTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteIncomingDeliveryType(incomingDeliveryTypeId, base.ActorCompanyId));
        }

        #endregion

        #region Order planning

        [HttpGet]
        [Route("Order/Unscheduled/{orderId:int}")]
        public IHttpActionResult GetUnscheduledOrder(int orderId)
        {
            return Content(HttpStatusCode.OK, tsm.GetUnscheduledOrder(base.ActorCompanyId, orderId, true, tsm.includeDeliveryAddressOnOrder(base.UserId, base.ActorCompanyId)));
        }

        [HttpGet]
        [Route("Order/Shifts/{orderId:int}")]
        public IHttpActionResult GetOrderShifts(int orderId)
        {
            return Content(HttpStatusCode.OK, tsm.GetOrderShifts(base.ActorCompanyId, orderId));
        }

        [HttpGet]
        [Route("Order/AvailableTime/{employeeId:int}/{startTime}/{stopTime}")]
        public IHttpActionResult GetAvailableTime(int employeeId, string startTime, string stopTime)
        {
            return Content(HttpStatusCode.OK, tem.GetAvailableTime(employeeId, BuildDateTimeFromString(startTime, false).Value, BuildDateTimeFromString(stopTime, false).Value));
        }

        [HttpPost]
        [Route("Order/Unscheduled")]
        public IHttpActionResult GetUnscheduledOrders(GetUnscheduledOrdersModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetUnscheduledOrders(base.ActorCompanyId, model.DateTo, model.CategoryIds, true, tsm.includeDeliveryAddressOnOrder(base.UserId, base.ActorCompanyId)));
        }

        [HttpPost]
        [Route("Order/Unscheduled/ByIds")]
        public IHttpActionResult GetUnscheduledOrdersByIds(GetUnscheduledOrdersModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetUnscheduledOrders(base.ActorCompanyId, model.OrderIds, true, tsm.includeDeliveryAddressOnOrder(base.UserId, base.ActorCompanyId)));
        }

        [HttpPost]
        [Route("Order/Assignments")]
        public IHttpActionResult SaveOrderAssignments(SaveOrderAssignmentsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveOrderAssignments(model.EmployeeId, model.OrderId, model.ShiftTypeId, model.StartTime, model.StopTime, model.AssignmentTimeAdjustmentType, model.SkipXEMailOnChanges));
        }

        #endregion

        #region RecalculateTimeHead

        [HttpGet]
        [Route("RecalculateTimeHead/{recalculateAction:int}/{loadRecords:bool}/{showHistory:bool}/{setExtensionNames:bool}/{dateFromString}/{dateToString}/{limitNbrOfHeads:int}")]
        public IHttpActionResult GetRecalculateTimeHeads(SoeRecalculateTimeHeadAction recalculateAction, bool loadRecords, bool showHistory, bool setExtensionNames, string dateFromString, string dateToString, int limitNbrOfHeads)
        {
            return Content(HttpStatusCode.OK, tsm.GetRecalculateTimeHeads(base.ActorCompanyId, base.UserId, base.RoleId, recalculateAction, loadRecords, showHistory, setExtensionNames, base.BuildDateTimeFromString(dateFromString, true), base.BuildDateTimeFromString(dateToString, true), limitNbrOfHeads != 0 ? limitNbrOfHeads : (int?)null).ToDTOs());
        }

        [HttpGet]
        [Route("RecalculateTimeHead/{recalculateTimeHeadId:int}/{loadRecords:bool}/{setExtensionNames:bool}")]
        public IHttpActionResult GetRecalculateTimeHead(int recalculateTimeHeadId, bool loadRecords, bool setExtensionNames)
        {
            return Content(HttpStatusCode.OK, tsm.GetRecalculateTimeHead(base.ActorCompanyId, recalculateTimeHeadId, loadRecords, setExtensionNames).ToDTO());
        }

        [HttpGet]
        [Route("RecalculateTimeHead/{key}")]
        public IHttpActionResult GetRecalculateTimeHead(string key)
        {
            Guid recalculateGuid = Guid.Parse(key);
            return Content(HttpStatusCode.OK, tsm.GetRecalculateTimeHeadId(base.ActorCompanyId, base.UserId, recalculateGuid));
        }

        [HttpPost]
        [Route("RecalculateTimeHead/SetToProcessed")]
        public IHttpActionResult SetRecalculateTimeHeadToProcessed(IntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SetRecalculateTimeHeadToProcessed(base.ActorCompanyId, base.UserId, base.ParameterObject.IsSupportLoggedIn, model.Id));
        }

        [HttpDelete]
        [Route("RecalculateTimeHead/{recalculateTimeHeadId:int}")]
        public IHttpActionResult CancelRecalculateTimeHead(int recalculateTimeHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.CancelRecalculateTimeHead(base.ActorCompanyId, base.UserId, base.ParameterObject.IsSupportLoggedIn, recalculateTimeHeadId));
        }

        #endregion

        #region RecalculateTimeRecord

        [HttpDelete]
        [Route("RecalculateTimeRecord/{recalculateTimeRecordId:int}")]
        public IHttpActionResult CancelRecalculateTimeRecord(int recalculateTimeRecordId)
        {
            return Content(HttpStatusCode.OK, tsm.CancelRecalculateTimeRecord(base.ActorCompanyId, base.UserId, base.ParameterObject.IsSupportLoggedIn, recalculateTimeRecordId));
        }

        #endregion

        #region Recurrence

        [HttpGet]
        [Route("Recurrence/Description/{pattern}")]
        public IHttpActionResult GetRecurrenceDescription(string pattern)
        {
            return Content(HttpStatusCode.OK, DailyRecurrencePatternDTO.GetRecurrenceDescription(pattern, base.GetTermGroupContent(TermGroup.RecurrencePattern), 1, cm.GetSysHolidayTypeDTOs()));
        }

        #endregion

        #region Schedule

        [HttpPost]
        [Route("CopySchedule")]
        public IHttpActionResult CopySchedule(CopyScheduleModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.CopySchedule(model.SourceEmployeeId, model.TargetEmployeeId, model.SourceDateEnd, model.TargetDateStart, model.TargetDateEnd, model.UseAccountingFromSourceSchedule, false));
        }

        #endregion

        #region ScheduleCycle

        [HttpGet]
        [Route("ScheduleCycle")]
        public IHttpActionResult GetScheduleCycles(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetScheduleCyclesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetScheduleCycles(base.ActorCompanyId).ToDTOs());
        }


        [HttpGet]
        [Route("ScheduleCycle/{scheduleCycleId:int}")]
        public IHttpActionResult GetScheduleCycle(int scheduleCycleId)
        {
            return Content(HttpStatusCode.OK, tsm.GetScheduleCycleWithRules(scheduleCycleId).ToDTO());
        }

        [HttpPost]
        [Route("ScheduleCycle")]
        public IHttpActionResult SaveScheduleCycle(ScheduleCycleDTO scheduleCycle)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveScheduleCycle(scheduleCycle, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ScheduleCycle/{scheduleCycleId:int}")]
        public IHttpActionResult DeleteScheduleCycle(int scheduleCycleId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteScheduleCycle(scheduleCycleId));
        }

        #endregion

        #region ScheduleCycleRuleType

        [HttpGet]
        [Route("ScheduleCycleRuleType/")]
        public IHttpActionResult GetScheduleCycleRuleTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_DTO) || message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetScheduleCycleRuleTypes(base.ActorCompanyId).ToDTOs());

            return Content(HttpStatusCode.OK, tsm.GetScheduleCycleRuleTypesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
        }


        [HttpGet]
        [Route("ScheduleCycleRuleType/{scheduleCycleRuleTypeId:int}")]
        public IHttpActionResult GetScheduleCycleRuleType(int scheduleCycleRuleTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetScheduleCycleRuleType(scheduleCycleRuleTypeId).ToDTO());
        }

        [HttpPost]
        [Route("ScheduleCycleRuleType")]
        public IHttpActionResult SaveScheduleCycleRuleType(ScheduleCycleRuleTypeDTO ruleType)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveScheduleCycleRuleType(ruleType, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ScheduleCycleRuleType/{scheduleCycleRuleTypeId:int}")]
        public IHttpActionResult DeleteScheduleCycleRuleType(int scheduleCycleRuleTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteScheduleCycleRuleType(scheduleCycleRuleTypeId));
        }

        #endregion

        #region ScheduleSwap

        [HttpPost]
        [Route("ScheduleSwap/Initiate")]
        public IHttpActionResult InitiateScheduleSwap(InitiateScheduleSwapModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.InitiateScheduleSwap(model.InitiatorEmployeeId, model.InitiatorShiftDate, model.InitiatorShiftIds, model.SwapWithEmployeeId, model.SwapShiftDate, model.SwapWithShiftIds, model.Comment));
        }

        #endregion

        #region ScheduleTypes

        [HttpGet]
        [Route("ScheduleType/{getAll:bool}/{onlyActive:bool}/{loadFactors:bool}/{loadTimeDeviationCauses:bool}")]
        public IHttpActionResult GetScheduleTypes(bool getAll, bool onlyActive, bool loadFactors, bool loadTimeDeviationCauses)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypes(base.ActorCompanyId, getAll: getAll, onlyActive: onlyActive, loadFactors: loadFactors, loadTimeDeviationCauses: loadTimeDeviationCauses).ToDTOs(loadFactors));
        }

        [HttpGet]
        [Route("ScheduleType/{timeScheduleTypeId:int}/{loadFactors:bool}")]
        public IHttpActionResult GetScheduleType(int timeScheduleTypeId, bool loadFactors)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleType(timeScheduleTypeId, loadFactors).ToDTO(loadFactors));
        }

        [HttpPost]
        [Route("ScheduleType")]
        public IHttpActionResult SaveScheduleType(TimeScheduleTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleType(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ScheduleType/UpdateState")]
        public IHttpActionResult UpdateScheduleTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.UpdateTimeScheduleTypesState(model.Dict));
        }

        [HttpDelete]
        [Route("ScheduleType/{timeScheduleTypeId:int}")]
        public IHttpActionResult DeleteScheduleType(int timeScheduleTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleType(timeScheduleTypeId));
        }

        #endregion

        #region Shifts

        [HttpGet]
        [Route("Shift/{employeeId:int}/{date}/{blockTypes}/{includeBreaks:bool}/{includeGrossNetAndCost:bool}/{link}/{loadQueue:bool}/{loadDeviationCause:bool}/{loadTasks:bool}/{includePreliminary:bool}/{timeScheduleScenarioHeadId:int}")]
        public IHttpActionResult GetShiftsForDay(int employeeId, string date, string blockTypes, bool includeBreaks, bool includeGrossNetAndCost, string link, bool loadQueue, bool loadDeviationCause, bool loadTasks, bool includePreliminary, int timeScheduleScenarioHeadId)
        {
            List<TermGroup_TimeScheduleTemplateBlockType> types = new List<TermGroup_TimeScheduleTemplateBlockType>();
            if (blockTypes != "null")
            {
                List<int> typeIds = StringUtility.SplitNumericList(blockTypes, true, false);
                foreach (int id in typeIds)
                {
                    types.Add((TermGroup_TimeScheduleTemplateBlockType)id);
                }
            }

            DateTime dateTime = BuildDateTimeFromString(date, true).Value;

            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleShifts(base.ActorCompanyId, base.UserId, base.RoleId, employeeId, dateTime, dateTime, types, includeBreaks, includeGrossNetAndCost, link != "null" ? new Guid(link) : (Guid?)null, 0, loadQueue, loadDeviationCause, loadTasks, includePreliminary, timeScheduleScenarioHeadId != 0 ? timeScheduleScenarioHeadId : (int?)null, setSwapShiftInfo: true));
        }

        [HttpGet]
        [Route("Shift/{timeScheduleTemplateBlockId:int}/{includeBreaks:bool}")]
        public IHttpActionResult GetShift(int timeScheduleTemplateBlockId, bool includeBreaks)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleShift(timeScheduleTemplateBlockId, base.ActorCompanyId, base.UserId, includeBreaks));
        }

        [HttpGet]
        [Route("Shift/ValidateBreakChange/{employeeId:int}/{timeScheduleTemplateBlockId:int}/{timeScheduleTemplatePeriodId:int}/{timeCodeBreakId:int}/{dateFrom}/{breakLength:int}/{isTemplate:bool}/{timeScheduleScenarioHeadId:int}")]
        public IHttpActionResult ValidateBreakChange(int employeeId, int timeScheduleTemplateBlockId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, string dateFrom, int breakLength, bool isTemplate, int timeScheduleScenarioHeadId)
        {
            return Content(HttpStatusCode.OK, tem.ValidateBreakChange(employeeId, timeScheduleTemplateBlockId, timeScheduleTemplatePeriodId, timeCodeBreakId, BuildDateTimeFromString(dateFrom, false).Value, breakLength, isTemplate, timeScheduleScenarioHeadId != 0 ? timeScheduleScenarioHeadId : (int?)null));
        }

        [HttpGet]
        [Route("Shift/Queue/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult GetShiftQueue(int timeScheduleTemplateBlockId)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftQueue(timeScheduleTemplateBlockId, TermGroup_TimeScheduleTemplateBlockQueueType.Unspecified, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Shift/Search")]
        public IHttpActionResult GetShifts(GetShiftsModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningShifts_ByProcedure(base.ActorCompanyId, base.UserId, model.EmployeeId, base.RoleId, model.DateFrom, model.DateTo, model.EmployeeIds, model.PlanningMode, model.DisplayMode, model.IncludeSecondaryCategories, model.IncludeBreaks, model.IncludeGrossNetAndCost, model.IncludePreliminary, model.IncludeEmploymentTaxAndSupplementChargeCost, model.IncludeShiftRequest, model.IncludeAbsenceRequest, model.CheckToIncludeDeliveryAdress, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, setSwapShiftInfo: true, includeHolidaySalary: model.IncludeHolidaySalary, includeOnDuty: true, includeLeisureCodes: model.IncludeLeisureCodes));
        }

        [HttpPost]
        [Route("Shift")]
        public IHttpActionResult SaveShifts(SaveShiftsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveTimeScheduleShift(model.Source, model.Shifts, model.UpdateBreaks, model.SkipXEMailOnChanges, model.AdjustTasks, model.MinutesMoved, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
        }

        [HttpPost]
        [Route("Shift/Drag")]
        public IHttpActionResult DragShift(DragShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.DragTimeScheduleShift(model.Action, model.SourceShiftId, model.TargetShiftId, model.Start, model.End, model.EmployeeId, true, true, model.TargetLink, model.UpdateLinkOnTarget, model.TimeDeviationCauseId, model.EmployeeChildId, model.WholeDayAbsence, null, model.SkipXEMailOnChanges, model.CopyTaskWithShift, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.StandbyCycleWeek, model.StandbyCycleDateFrom, model.StandbyCycleDateTo, model.IsStandByView, model.IncludeOnDutyShifts, model.IncludedOnDutyShiftIds));
        }

        [HttpPost]
        [Route("Shift/DragMultiple")]
        public IHttpActionResult DragShifts(DragShiftsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.DragTimeScheduleShiftMultipel(model.Action, model.SourceShiftIds, model.OffsetDays, model.TargetEmployeeId, true, model.SkipXEMailOnChanges, model.CopyTaskWithShift, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.StandbyCycleWeek, model.StandbyCycleDateFrom, model.StandbyCycleDateTo, model.IsStandByView, model.IncludeOnDutyShifts, model.IncludedOnDutyShiftIds));
        }

        [HttpPost]
        [Route("Shift/Handle")]
        public IHttpActionResult HandleShift(HandleShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.HandleTimeScheduleShift(model.Action, model.TimeScheduleTemplateBlockId, model.TimeDeviationCauseId, model.EmployeeId, model.SwapTimeScheduleTemplateBlockId, base.RoleId, model.PreventAutoPermissions));
        }

        [HttpPost]
        [Route("ShiftPeriod/Search")]
        public IHttpActionResult GetShiftPeriods(GetShiftPeriodsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningPeriods_ByProcedure(base.ActorCompanyId, base.UserId, base.RoleId, model.DateFrom, model.DateTo, model.EmployeeId, model.DisplayMode, model.BlockTypes, model.EmployeeIds, model.ShiftTypeIds, null, null, model.DeviationCauseIds, model.IncludeGrossNetAndCost, false, model.IncludePreliminary, model.IncludeEmploymentTaxAndSupplementChargeCost, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, includeHolidaySalary: model.IncludeHolidaySalary));
        }

        [HttpPost]
        [Route("Shift/GrossNetAndCost/")]
        public IHttpActionResult GetShiftsGrossNetAndCost(GetGrossNetCostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningShiftsGrossNetAndCost(base.ActorCompanyId, base.UserId, model.EmployeeId, base.RoleId, model.DateFrom, model.DateTo, model.EmployeeIds, model.IncludeSecondaryCategories, model.IncludeBreaks, model.IncludePreliminary, model.IncludeEmploymentTaxAndSupplementChargeCost, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
        }

        [HttpPost]
        [Route("ShiftPeriod/GrossNetAndCost/")]
        public IHttpActionResult GetShiftPeriodsGrossNetAndCost(GetShiftPeriodsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningPeriodsGrossNetAndCost(base.ActorCompanyId, base.UserId, base.RoleId, model.DateFrom, model.DateTo, model.EmployeeId, model.BlockTypes, model.EmployeeIds, model.ShiftTypeIds, null, null, model.DeviationCauseIds, model.IncludePreliminary, model.IncludeEmploymentTaxAndSupplementChargeCost, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
        }

        [HttpPost]
        [Route("ShiftPeriod/Detail")]
        public IHttpActionResult GetShiftPeriodDetails(GetShiftPeriodDetailsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningPeriodDetails(base.ActorCompanyId, base.UserId, base.RoleId, model.Date, model.EmployeeId, model.BlockTypes, model.EmployeeIds, model.ShiftTypeIds, null, null, model.DeviationCauseIds, model.IncludePreliminary, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
        }

        [HttpPost]
        [Route("Shift/Split")]
        public IHttpActionResult SplitShift(SplitShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SplitTimeScheduleShift(model.Shift, model.SplitTime, model.EmployeeId1, model.EmployeeId2, model.KeepShiftsTogether, model.IsPersonalScheduleTemplate, model.SkipXEMailOnChanges, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
        }

        [HttpPost]
        [Route("Shift/Template/Split")]
        public IHttpActionResult SplitTemplateShift(SplitTemplateShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SplitTemplateTimeScheduleShift(model.SourceShift, model.SourceTemplateHeadId, model.SplitTime, model.EmployeeId1, model.EmployeePostId1, model.TemplateHeadId1, model.EmployeeId2, model.EmployeePostId2, model.TemplateHeadId2, model.KeepShiftsTogether));
        }

        [HttpPost]
        [Route("Shift/DefToFromPrelShift")]
        public IHttpActionResult SaveDefToPrelShift(DefToFromPrelShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK,
                    model.PrelToDef ?
                    tem.SaveShiftPrelToDef(model.EmployeeIds, model.DateFrom, model.DateTo, model.IncludeScheduleShifts, model.includeStandbyShifts) :
                    tem.SaveShiftDefToPrel(model.EmployeeIds, model.DateFrom, model.DateTo, model.IncludeScheduleShifts, model.includeStandbyShifts));
        }

        [HttpPost]
        [Route("Shift/EmployeesWithSubstituteShifts")]
        public IHttpActionResult GetEmployeesWithSubstituteShifts(PrintTimeEmploymentContractShortSubstituteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetEmployeesWithSubstituteShifts(base.ActorCompanyId, model.EmployeeIds, model.Dates, true, true));
        }

        [HttpPost]
        [Route("Shift/TimeEmploymentContractShortSubstituteUrl")]
        public IHttpActionResult GetTimeEmploymentContractShortSubstituteUrl(PrintTimeEmploymentContractShortSubstituteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.GetTimeEmploymentContractShortSubstitutePrintUrl(base.ActorCompanyId, model.EmployeeIds, this.UserId, this.RoleId, model.Dates, model.PrintedFromScheduleplanning));
        }

        [HttpPost]
        [Route("Shift/SendTimeEmploymentContractShortSubstituteForConfirmation")]
        public IHttpActionResult SendTimeEmploymentContractShortSubstituteForConfirmation(PrintTimeEmploymentContractShortSubstituteModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, ccm.SendTimeEmploymentContractShortSubstituteForConfirmation(base.ActorCompanyId, model.EmployeeIds, this.UserId, this.RoleId, model.Dates, model.PrintedFromScheduleplanning, model.SavePrintout));
        }

        [HttpPost]
        [Route("Shift/ExportToExcel")]
        public IHttpActionResult ExportShiftsToExcel(ExportShiftsToExcelModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tmdm.GetMatrixResultFromPlanningDay(base.ActorCompanyId, model.Shifts, model.Employees, model.Dates, model.Selections.ToList()));
        }

        [HttpPost]
        [Route("Shift/CreateEmptyScheduleForEmployeePost")]
        public IHttpActionResult CreateEmptyScheduleForEmployeePost(CreateTimeSchedulePlanningDayDTOsFromEmployeePostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateEmptyScheduleForEmployeePost(base.ActorCompanyId, model.EmployeePostId, model.FromDate));
        }

        [HttpPost]
        [Route("Shift/CreateEmptyScheduleForEmployeePosts")]
        public IHttpActionResult CreateEmptyScheduleForEmployeePosts(CreateTimeSchedulePlanningDayDTOsFromEmployeePostsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateEmptyScheduleForEmployeePosts(base.ActorCompanyId, model.EmployeePostIds, model.FromDate));
        }

        [HttpPost]
        [Route("Shift/CreateScheduleFromEmployeePost")]
        public IHttpActionResult CreateScheduleFromEmployeePost(CreateTimeSchedulePlanningDayDTOsFromEmployeePostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateScheduleFromEmployeePost(base.ActorCompanyId, model.EmployeePostId, model.FromDate));
        }

        [HttpPost]
        [Route("Shift/GetPreAnalysisInformation")]
        public IHttpActionResult GetPreAnalysisInformation(CreateTimeSchedulePlanningDayDTOsFromEmployeePostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                SoeProgressInfo info = new SoeProgressInfo(Guid.NewGuid(), SoeProgressInfoType.ScheduleEmployeePost, base.ActorCompanyId);
                return Content(HttpStatusCode.OK, tsm.GetPreAnalysisInformation(base.ActorCompanyId, model.EmployeePostId, model.FromDate, ref info));
            }
        }

        [HttpPost]
        [Route("Shift/CreateScheduleFromEmployeePosts")]
        public IHttpActionResult CreateScheduleFromEmployeePost(CreateTimeSchedulePlanningDayDTOsFromEmployeePostsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            SoeProgressInfo info = new SoeProgressInfo(Guid.NewGuid(), SoeProgressInfoType.ScheduleEmployeePost, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, tsm.CreateScheduleFromEmployeePosts(base.ActorCompanyId, model.EmployeePostIds, model.FromDate, ref info));
        }

        [HttpPost]
        [Route("Shift/CreateScheduleFromEmployeePostsAsync")]
        public IHttpActionResult CreateScheduleFromEmployeePostAsync(CreateTimeSchedulePlanningDayDTOsFromEmployeePostsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                int actorCompanyId = base.ActorCompanyId;
                int userId = base.UserId;
                int roleId = base.RoleId;
                Guid guid = Guid.NewGuid();
                var culture = Thread.CurrentThread.CurrentCulture;
                var workingThread = new Thread(() => CreateScheduleFromEmployeePosts(culture, actorCompanyId, roleId, userId, model.EmployeePostIds, model.FromDate, guid));
                workingThread.Start();
                return Content(HttpStatusCode.OK, new SoeProgressInfo(guid));
            }
        }

        private void CreateScheduleFromEmployeePosts(CultureInfo cultureInfo, int actorCompanyId, int roleId, int userId, List<int> employeePostIds, DateTime fromDate, Guid threadGuid)
        {
            SetLanguage(cultureInfo);

            var useAccountHierarchy = am.UseAccountHierarchyOnCompanyFromCache(actorCompanyId);
            var accountIds = useAccountHierarchy ? am.GetAccountIdsFromHierarchyByUserSetting(actorCompanyId, roleId, userId, fromDate) : null;
            var id = accountIds.IsNullOrEmpty() ? 0 : accountIds.Sum(s => s);

            SoeProgressInfo oldInfo = monitor.GetInfo(SoeProgressInfoType.ScheduleEmployeePost, actorCompanyId, id);
            SoeProgressInfo info = monitor.RegisterNewProgressProcess(threadGuid, SoeProgressInfoType.ScheduleEmployeePost, actorCompanyId, id);

            if (oldInfo != null && !oldInfo.Abort && !oldInfo.Error && !oldInfo.Done)
            {
                info.Message = base.GetText(11652, "Bearbetning pågår redan, startad: ") + CalendarUtility.ToShortDateTimeString(oldInfo.Created);
                info.ErrorMessage = base.GetText(11652, "Bearbetning pågår redan, startad: ") + CalendarUtility.ToShortDateTimeString(oldInfo.Created);
                info.Abort = true;
                info.Error = true;
                Thread.Sleep(3000);
                return;
            }
            tsm.CreateScheduleFromEmployeePosts(actorCompanyId, employeePostIds, fromDate, ref info, monitor, threadGuid);
        }

        [HttpPost]
        [Route("Shift/DeleteShifts")]
        public IHttpActionResult DeleteShifts(DeleteShiftsModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteTimeScheduleShifts(model.ShiftIds, model.SkipXEMailOnChanges, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.IncludedOnDutyShiftIds));
        }

        [HttpDelete]
        [Route("Shift/Queue/{type:int}/{timeScheduleTemplateBlockId:int}/{employeeId:int}")]
        public IHttpActionResult RemoveEmployeeFromShiftQueue(TermGroup_TimeScheduleTemplateBlockQueueType type, int timeScheduleTemplateBlockId, int employeeId)
        {
            return Content(HttpStatusCode.OK, tem.RemoveEmployeeFromShiftQueue(type, timeScheduleTemplateBlockId, employeeId));
        }

        [HttpDelete]
        [Route("Shift/DeleteScheduleFromEmployeePost/{employeePostId:int}")]
        public IHttpActionResult DeleteScheduleFromEmployeePost(int employeePostId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteShiftsFromEmployeePost(base.ActorCompanyId, employeePostId));
        }

        [HttpPost]
        [Route("Shift/DeleteScheduleFromEmployeePosts/")]
        public IHttpActionResult DeleteScheduleFromEmployeePost(ListIntModel employeePostIds)
        {
            List<int> employeeIds = employeePostIds.Numbers;
            return Content(HttpStatusCode.OK, tsm.DeleteShiftsFromEmployeePosts(base.ActorCompanyId, employeeIds));
        }


        [HttpPost]
        [Route("Shift/ValidatePossibleDeleteOfEmployeeAccount")]
        public IHttpActionResult ValidatePossibleDeleteOfEmployeeAccountModel(ValidatePossibleDeleteOfEmployeeAccountDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                foreach (ValidatePossibleDeleteOfEmployeeAccountRowDTO row in model.Rows)
                {
                    ActionResult result = tsm.ValidatePossibleDeleteOfEmployeeAccount(row.DateFrom, row.DateTo, row.EmployeeAccountId, model.EmployeeId);

                    if (!result.Success)
                        return Content(HttpStatusCode.OK, result);
                }

                return Content(HttpStatusCode.OK, new ActionResult() { InfoMessage = "Validate for Deletion" });
            }
        }

        #endregion

        #region ShiftAccounting

        [HttpGet]
        [Route("ShiftAccounting/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult GetShiftAccounting(int timeScheduleTemplateBlockId)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftAccounting(timeScheduleTemplateBlockId, base.ActorCompanyId));
        }

        #endregion

        #region ShiftRequestStatus

        [HttpGet]
        [Route("ShiftRequest/Status/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult GetShiftRequestStatus(int timeScheduleTemplateBlockId)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftRequestStatus(timeScheduleTemplateBlockId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("ShiftRequest/CheckIfTooEarlyToSend/{startTime}")]
        public IHttpActionResult CheckIfTooEarlyToSend(string startTime)
        {
            return Content(HttpStatusCode.OK, tsm.ShiftRequestCheckIfTooEarlyToSend(BuildDateTimeFromString(startTime, false).Value, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ShiftRequest/{timeScheduleTemplateBlockId:int}/{userId:int}")]
        public IHttpActionResult RemoveRecipientFromShiftRequest(int timeScheduleTemplateBlockId, int userId)
        {
            return Content(HttpStatusCode.OK, tsm.RemoveRecipientFromShiftRequest(timeScheduleTemplateBlockId, base.ActorCompanyId, userId));
        }

        [HttpDelete]
        [Route("ShiftRequest/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult UndoShiftRequest(int timeScheduleTemplateBlockId)
        {
            return Content(HttpStatusCode.OK, tsm.UndoShiftRequest(timeScheduleTemplateBlockId, base.ActorCompanyId));
        }

        #endregion

        #region ShiftTasks

        [HttpPost]
        [Route("ShiftTask/")]
        public IHttpActionResult GetShiftTasks(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftTasks(base.ActorCompanyId, model.Numbers).ToDTOs());
        }

        [HttpPost]
        [Route("ShiftTask/AssignToEmployee")]
        public IHttpActionResult AssignTaskToEmployee(AssignTaskToEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.AssignTaskToEmployee(model.EmployeeId, model.Date, model.TaskDTOs, model.SkipXEMailOnShiftChanges));
        }

        [HttpPost]
        [Route("ShiftTask/AssignToEmployee/EvaluateWorkRule")]
        public IHttpActionResult EvaluateAssignTaskToEmployeeAgainstWorkRules(EvaluateAssignTaskToEmployeeAgainstWorkRulesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateAssignTaskToEmployeeAgainstWorkRules(model.DestinationEmployeeId, model.DestinationDate, model.TaskDTOs, model.Rules));
        }

        [HttpPost]
        [Route("ShiftTask/AssignTemplateShiftTask")]
        public IHttpActionResult AssignTemplateShiftTask(AssignTaskToEmployeePostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.AssignTemplateShiftTask(model.Tasks, model.Date, model.TimeScheduleTemplateHeadId));
        }

        #endregion

        #region TemplateShift

        [HttpGet]
        [Route("TemplateShift/{employeeId:int}/{date}/{link}/{loadYesterdayAlso:bool}/{includeGrossNetAndCost:bool}/{includeEmploymentTaxAndSupplementChargeCost:bool}/{loadTasks:bool}")]
        public IHttpActionResult GetTemplateShiftsForDay(int employeeId, string date, string link, bool loadYesterdayAlso, bool includeGrossNetAndCost, bool includeEmploymentTaxAndSupplementChargeCost, bool loadTasks)
        {
            DateTime dateTime = BuildDateTimeFromString(date, true).Value;

            return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningDaysFromTemplate(base.ActorCompanyId, base.RoleId, base.UserId, dateTime, dateTime, link != "null" ? new Guid(link) : (Guid?)null, new List<int> { employeeId }, includeGrossNetAndCost: includeGrossNetAndCost, includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost, loadYesterdayAlso: loadYesterdayAlso, loadTasksAndDelivery: loadTasks, loadAccounts: false, doNotCheckHoliday: true));
        }

        [HttpPost]
        [Route("TemplateShift/Search")]
        public IHttpActionResult GetTemplateShifts(GetShiftsModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningDaysFromTemplate(base.ActorCompanyId, base.RoleId, base.UserId, model.DateFrom, model.DateTo, null, model.EmployeeIds, includeGrossNetAndCost: model.IncludeGrossNetAndCost, includeEmploymentTaxAndSupplementChargeCost: model.IncludeEmploymentTaxAndSupplementChargeCost, loadYesterdayAlso: model.LoadYesterdayAlso, loadTasksAndDelivery: model.LoadTasks, doNotCheckHoliday: true));
        }

        [HttpPost]
        [Route("TemplateShift/GrossNetAndCost/")]
        public IHttpActionResult GetTemplateShiftsGrossNetAndCost(GetGrossNetCostModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningTemplateShiftsGrossNetAndCost(base.ActorCompanyId, base.RoleId, base.UserId, model.DateFrom, model.DateTo, model.EmployeeIds, model.IncludeEmploymentTaxAndSupplementChargeCost, true));
        }

        [HttpPost]
        [Route("TemplateShift/Drag")]
        public IHttpActionResult DragTemplateShift(DragTemplateShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.DragTemplateTimeScheduleShift(model.Action, model.SourceShiftId, model.SourceTemplateHeadId, model.SourceDate, model.TargetShiftId, model.TargetTemplateHeadId, model.Start, model.End, model.EmployeeId, model.EmployeePostId, true, true, model.TargetLink, model.UpdateLinkOnTarget, model.CopyTaskWithShift));
        }

        [HttpPost]
        [Route("TemplateShift/DragMultiple")]
        public IHttpActionResult DragTemplateShifts(DragTemplateShiftsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.DragTemplateTimeScheduleShiftMultipel(model.Action, model.SourceShiftIds, model.SourceTemplateHeadId, model.FirstSourceDate, model.OffsetDays, model.FirstTargetDate, model.TargetEmployeeId, model.TargetEmployeePostId, model.TargetTemplateHeadId, true, model.CopyTaskWithShift));
        }

        #endregion

        #region ShiftType

        [HttpGet]
        [Route("ShiftType/")]
        public IHttpActionResult GetShiftTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_DTO) || message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
            {
                bool loadAccountInternals = false;
                bool loadAccounts = message.GetBoolValueFromQS("loadAccounts");
                bool loadSkills = message.GetBoolValueFromQS("loadSkills");
                bool loadEmployeeStatisticsTargets = message.GetBoolValueFromQS("loadEmployeeStatisticsTargets");
                bool setTimeScheduleTemplateBlockTypeName = message.GetBoolValueFromQS("setTimeScheduleTemplateBlockTypeName");
                bool setCategoryNames = message.GetBoolValueFromQS("setCategoryNames");
                bool setAccountingString = message.GetBoolValueFromQS("setAccountingString");
                bool setSkillNames = message.GetBoolValueFromQS("setSkillNames");
                bool setTimeScheduleTypeName = message.GetBoolValueFromQS("setTimeScheduleTypeName");
                bool loadHierarchyAccounts = message.GetBoolValueFromQS("loadHierarchyAccounts");

                if (message.HasAcceptValue(HttpExtensions.ACCEPT_DTO))
                    return Content(HttpStatusCode.OK, tsm.GetShiftTypes(base.ActorCompanyId, loadAccountInternals, loadAccounts, loadSkills, loadEmployeeStatisticsTargets, setTimeScheduleTemplateBlockTypeName, setCategoryNames, setAccountingString, setSkillNames, setTimeScheduleTypeName, loadHierarchyAccounts).ToDTOs(includeSkills: loadSkills, includeEmployeeStatisticsTargets: loadEmployeeStatisticsTargets, includeAccountingSettings: loadAccounts));
                else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                    return Content(HttpStatusCode.OK, tsm.GetShiftTypes(base.ActorCompanyId, loadAccountInternals, loadAccounts, loadSkills, loadEmployeeStatisticsTargets, setTimeScheduleTemplateBlockTypeName, setCategoryNames, setAccountingString, setSkillNames, setTimeScheduleTypeName, loadHierarchyAccounts).ToGridDTOs());
            }

            return Content(HttpStatusCode.OK, tsm.GetShiftTypesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ShiftType/GetShiftTypesForUsersCategories/")]
        public IHttpActionResult GetShiftTypesForUsersCategories(HttpRequestMessage message)
        {
            int employeeId = message.GetIntValueFromQS("employeeId");
            bool isAdmin = message.GetBoolValueFromQS("isAdmin");
            List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>();
            List<int> typeIds = message.GetIntListValueFromQS("blockTypes");
            foreach (int id in typeIds)
            {
                blockTypes.Add((TermGroup_TimeScheduleTemplateBlockType)id);
            }

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetShiftTypesForUsersCategories(base.ActorCompanyId, base.UserId, employeeId, isAdmin, true, blockTypes, null).ToGridDTOs());

            return Content(HttpStatusCode.OK, tsm.GetShiftTypesForUsersCategories(base.ActorCompanyId, base.UserId, employeeId, isAdmin, true, blockTypes, null).ToDTOs());
        }

        [HttpGet]
        [Route("ShiftType/GetShiftTypeIdsForUser/")]
        public IHttpActionResult GetShiftTypeIdsForUser(HttpRequestMessage message)
        {
            int employeeId = message.GetIntValueFromQS("employeeId");
            bool isAdmin = message.GetBoolValueFromQS("isAdmin");
            bool includeSecondaryCategories = message.GetBoolValueFromQS("includeSecondaryCategories");
            DateTime? dateFrom = message.GetDateValueFromQS("dateFromString");
            DateTime? dateTo = message.GetDateValueFromQS("dateToString");

            List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>();
            List<int> typeIds = message.GetIntListValueFromQS("blockTypes");
            foreach (int id in typeIds)
            {
                blockTypes.Add((TermGroup_TimeScheduleTemplateBlockType)id);
            }

            return Content(HttpStatusCode.OK, tsm.GetShiftTypeIdsForUser(null, base.ActorCompanyId, base.RoleId, base.UserId, employeeId, isAdmin, dateFrom, dateTo, includeSecondaryCategories, blockTypes));
        }

        [HttpGet]
        [Route("ShiftType/{shiftTypeId:int}/{loadAccounts:bool}/{loadSkills:bool}/{loadEmployeeStatisticsTargets:bool}/{setEmployeeStatisticsTargetsTypeName:bool}/{loadCategories:bool}/{loadHierarchyAccounts:bool}")]
        public IHttpActionResult GetShiftType(int shiftTypeId, bool loadAccounts, bool loadSkills, bool loadEmployeeStatisticsTargets, bool setEmployeeStatisticsTargetsTypeName, bool loadCategories, bool loadHierarchyAccounts)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftType(shiftTypeId, loadAccounts, loadSkills, loadEmployeeStatisticsTargets, setEmployeeStatisticsTargetsTypeName, loadCategories, loadHierarchyAccounts: loadHierarchyAccounts).ToDTO(false, loadSkills, loadEmployeeStatisticsTargets, loadAccounts, false, loadCategories: loadCategories));
        }

        [HttpPost]
        [Route("ShiftType")]
        public IHttpActionResult SaveShiftType(ShiftTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveShiftType(model, null, null, null, null, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ShiftType/{shiftTypeId:int}")]
        public IHttpActionResult DeleteShiftType(int shiftTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteShiftType(shiftTypeId, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ShiftType/{shiftTypeIds}")]
        public IHttpActionResult DeleteShiftTypes(string shiftTypeIds)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteShiftTypes(StringUtility.SplitNumericList(shiftTypeIds), base.ActorCompanyId));
        }

        #endregion

        #region ShiftTypeLink

        [HttpGet]
        [Route("ShiftTypeLink/")]
        public IHttpActionResult GetShiftTypeLink(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftTypeLinkDTOs(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ShiftTypeLink")]
        public IHttpActionResult SaveShiftTypeLinks(List<ShiftTypeLinkDTO> models)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveShiftTypeLinks(models, base.ActorCompanyId));
        }

        #endregion

        #region Skill

        [HttpGet]
        [Route("Skill")]
        public IHttpActionResult GetSkills(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetSkillsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetSkills(base.ActorCompanyId, true).ToDTOs());
        }

        [HttpGet]
        [Route("Skill/Employee/{employeeId:int}")]
        public IHttpActionResult GetEmployeeSkills(int employeeId)
        {
            if (useSkillsPoc)
                return Content(HttpStatusCode.OK, tsm.GetEmployeeSkillsDTOs(employeeId));
            else
                return Content(HttpStatusCode.OK, tsm.GetEmployeeSkills(employeeId).ToDTOs(true));
        }

        [HttpGet]
        [Route("Skill/Employee/{employeeId:int}/{shiftTypeId:int}/{date}")]
        public IHttpActionResult GetEmployeeHasSkill(int employeeId, int shiftTypeId, string date)
        {
            return Content(HttpStatusCode.OK, tsm.EmployeeHasShiftTypeSkills(employeeId, shiftTypeId, BuildDateTimeFromString(date, true).Value));
        }

        [HttpGet]
        [Route("Skill/EmployeePost/{employeePostId:int}")]
        public IHttpActionResult GetEmployeePostSkills(int employeePostId)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeePostSkills(employeePostId).ToDTOs(true));
        }

        [HttpGet]
        [Route("Skill/EmployeePost/{employeePostId:int}/{shiftTypeId:int}/{date}")]
        public IHttpActionResult GetEmployeePostHasSkill(int employeePostId, int shiftTypeId, string date)
        {
            return Content(HttpStatusCode.OK, tsm.EmployeePostHasShiftTypeSkills(employeePostId, shiftTypeId, BuildDateTimeFromString(date, true).Value));
        }

        [HttpGet]
        [Route("Skill/MatchEmployees/{shiftTypeId:int}")]
        public IHttpActionResult MatchEmployeesByShiftTypeSkills(int shiftTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.MatchEmployeesByShiftTypeSkills(shiftTypeId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Skill/ShiftType/{shiftTypeId:int}")]
        public IHttpActionResult GetShiftTypeSkills(int shiftTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftTypeSkills(shiftTypeId).ToDTOs());
        }

        [HttpGet]
        [Route("Skill/{skillId:int}")]
        public IHttpActionResult GetSkill(int skillId)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkill(skillId).ToDTO());
        }

        [HttpPost]
        [Route("Skill")]
        public IHttpActionResult SaveSkill(SkillDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveSkill(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Skill/{skillId:int}")]
        public IHttpActionResult DeleteSkill(int skillId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteSkill(skillId));
        }

        #endregion

        #region SkillType

        [HttpGet]
        [Route("SkillType")]
        public IHttpActionResult GetSkillTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetSkillTypesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetSkillTypes(base.ActorCompanyId, null).ToGridDTOs());
        }

        [HttpGet]
        [Route("SkillType/{skillTypeId:int}")]
        public IHttpActionResult GetSkillType(int skillTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkillType(skillTypeId).ToDTO());
        }

        [HttpPost]
        [Route("SkillType")]
        public IHttpActionResult SaveSkillType(SkillTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveSkillType(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SkillType/UpdateState")]
        public IHttpActionResult UpdateSkillTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.UpdateSkillTypesState(model.Dict));
        }

        [HttpDelete]
        [Route("SkillType/{skillTypeId:int}")]
        public IHttpActionResult DeleteSkillType(int skillTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteSkillType(skillTypeId));
        }

        #endregion

        #region StaffingNeeds

        [HttpPost]
        [Route("StaffingNeeds/UnscheduledTasks/")]
        public IHttpActionResult GetUnscheduledStaffingNeedsTasks(UnscheduledTasksModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetUnscheduledStaffingNeedsTasks(base.ActorCompanyId, model.ShiftTypeIds, model.DateFrom, model.DateTo, model.Type));
        }

        [HttpPost]
        [Route("StaffingNeeds/UnscheduledTaskDates/")]
        public IHttpActionResult GetUnscheduledStaffingNeedsTaskDates(UnscheduledTasksModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetUnscheduledStaffingNeedsTaskDates(base.ActorCompanyId, model.ShiftTypeIds, model.DateFrom, model.DateTo, model.Type));
        }

        [HttpGet]
        [Route("StaffingNeeds/GetTimeScheduleTaskGeneratedNeeds/{timeScheduleTaskId:int}")]
        public IHttpActionResult GetTimeScheduleTaskGeneratedNeeds(int timeScheduleTaskId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskGeneratedNeeds(timeScheduleTaskId));
        }

        [HttpPost]
        [Route("StaffingNeeds/DeleteGeneratedNeeds/")]
        public IHttpActionResult DeleteGeneratedNeeds(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.DeleteGeneratedNeeds(model.Numbers));
        }

        #endregion

        #region StaffingNeedsHead

        [HttpGet]
        [Route("StaffingNeedsHead/GetStaffingNeedsHeadsForUser/{type:int}/{status:int}/{loadRows:bool}/{loadPeriods:bool}")]
        public IHttpActionResult GetStaffingNeedsHeadsForUser(int type, int status, bool loadRows, bool loadPeriods)
        {
            TermGroup_StaffingNeedsHeadStatus? stat = null;
            if (status > 0)
                stat = (TermGroup_StaffingNeedsHeadStatus)status;

            List<StaffingNeedsHead> heads = tsm.GetStaffingNeedsHeadsForUser(base.ActorCompanyId, loadRows, loadPeriods, false, (StaffingNeedsHeadType)type, stat);
            List<StaffingNeedsHead> filteredHeads = heads.Where(i => i.Weekday == null).ToList();
            foreach (var headsGroupedByWeekday in heads.Where(i => i.Weekday != null).GroupBy(g => g.Weekday))
            {
                filteredHeads.Add(headsGroupedByWeekday.OrderByDescending(o => o.Created).FirstOrDefault());
            }

            return Content(HttpStatusCode.OK, filteredHeads.ToDTOs(true, true, false, false));
        }

        [HttpPost]
        [Route("StaffingNeedsHead/GenerateStaffingNeedsHeadsForInterval/")]
        public IHttpActionResult GenerateStaffingNeedsHeadsForInterval(GenerateStaffingNeedsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GenerateStaffingNeedsHeadsForInterval(model.NeedFilterType, model.CalculationType, model.DateFrom, model.DateTo, model.AccountDimId, model.AccountId, model.CalculateNeed, model.CalculateNeedFrequency, model.CalculateNeedRowFrequency, model.CalculateBudget, model.CalculateForecast, model.CalculateTemplateSchedule, model.CalculateSchedule, model.CalculateTime, model.CalculateTemplateScheduleForEmployeePost, model.EmployeeIds, model.EmployeePostIds, true, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.IncludeEmpTaxAndSuppCharge, shiftTypeIds: model.ShiftTypeIds, annualLeaveGroups: null, forceWeekView: model.ForceWeekView));
        }

        [HttpPost]
        [Route("StaffingNeedsHead/RecalculateStaffingNeedsSummary/")]
        public IHttpActionResult RecalculateStaffingNeedsSummary(RecalculateStaffingNeedsSummaryModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                StaffingStatisticsIntervalRow row = model.Row.ConvertToStaffingStatisticsIntervalRow();
                return Content(HttpStatusCode.OK, tsm.RecalculateStaffingNeedsSummary(row));
            }
        }

        [HttpPost]
        [Route("StaffingNeedsHead/GenerateStaffingNeedsHeads/")]
        public IHttpActionResult GenerateStaffingNeedsHeads(GenerateStaffingNeedsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GenerateStaffingNeedsHeads(model.NeedFilterType, StaffingNeedsHeadType.NeedsPlanning, model.DateFrom, model.DateTo, buildMountain: true));
        }

        [HttpGet]
        [Route("StaffingNeedsHead/{staffingNeedsHeadId:int}/{loadRows:bool}/{loadPeriods:bool}")]
        public IHttpActionResult GetStaffingNeedsHead(int staffingNeedsHeadId, bool loadRows, bool loadPeriods)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsHead(staffingNeedsHeadId, base.ActorCompanyId, true, loadRows, loadPeriods, false).ToDTO(loadRows, loadPeriods, false, false));
        }

        [HttpPost]
        [Route("StaffingNeedsHead/")]
        public IHttpActionResult SaveStaffingNeedsHead(StaffingNeedsHeadDTO dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsHead(dto, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("StaffingNeedsHead/IncomingDeliveryHead/")]
        public IHttpActionResult GetStaffingNeedsHeadFromIncomingDeliveryHead(CreateStaffingNeedsHeadModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateStaffingNeedsHeadDTO(model.Interval, model.IncomingDeliveryHeadDTOs, model.Name, model.Date, model.DayTypeId, (DayOfWeek?)model.DayOfWeek));
        }

        [HttpPost]
        [Route("StaffingNeedsHead/TimeScheduleTask/")]
        public IHttpActionResult GetStaffingNeedsHeadFromTimeScheduleTask(CreateStaffingNeedsHeadModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateStaffingNeedsHeadDTO(model.Interval, model.TimeScheduleTaskDTOs, model.Name, model.Date, model.DayTypeId, (DayOfWeek?)model.DayOfWeek));
        }

        [HttpPost]
        [Route("StaffingNeedsHead/Tasks/")]
        public IHttpActionResult CreateStaffingNeedsHeads(CreateStaffingNeedsHeadModel model)
        {
            ActionResult result = new ActionResult();

            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            try
            {
                SettingManager sm = new SettingManager(null);
                var useAccountHierarchy = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.UseAccountHierarchy, 0, base.ActorCompanyId, 0);
                List<int> validAccountIds = !useAccountHierarchy ? null : am.GetAccountIdsFromHierarchyByUserSetting(base.ActorCompanyId, base.RoleId, base.UserId, DateTime.Today, DateTime.Today, null);

                if (useAccountHierarchy && validAccountIds.Count != 1)
                    return Ok(new ActionResult("Invalid level"));

                var timeScheduleTasks = tsm.GetTimeScheduleTasks(base.ActorCompanyId);
                int? shiftTypeId = null;
                List<StaffingNeedsLocation> locations = null;
                string locationIds = string.Empty;

                if (!model.WholeWeek)
                    model.Name = $"{model.DayOfWeek} + {model.Date}";
                else
                    model.Name = $"week ";

                if (timeScheduleTasks != null)
                {
                    if (model.StaffingNeedsFrequencyTimeScheduleTaskId != 0)
                    {
                        var isStaffingNeedsFrequencyTask = timeScheduleTasks.FirstOrDefault(w => w.TimeScheduleTaskId == model.StaffingNeedsFrequencyTimeScheduleTaskId);
                        shiftTypeId = isStaffingNeedsFrequencyTask.ShiftTypeId;
                    }
                    else
                    {
                        var isStaffingNeedsFrequencyTask = timeScheduleTasks.FirstOrDefault(w => w.IsStaffingNeedsFrequency);
                        if (isStaffingNeedsFrequencyTask != null)
                            shiftTypeId = isStaffingNeedsFrequencyTask.ShiftTypeId;
                    }
                }

                if (model.StaffingNeedsFrequencyTimeScheduleTaskId != 0)
                {
                    locations = tsm.GetStaffingNeedsLocationsFromTimeScheduleTask(model.StaffingNeedsFrequencyTimeScheduleTaskId, base.ActorCompanyId);
                    if (locations != null)
                        locationIds = locations.Select(s => s.StaffingNeedsLocationId).JoinToString("#");
                }
                else
                {
                    var tasks = tsm.GetTimeScheduleTasks(base.ActorCompanyId);
                    if (tasks.Count(c => c.IsStaffingNeedsFrequency) == 1)
                    {
                        locations = tsm.GetStaffingNeedsLocationsFromTimeScheduleTask(tasks.First(c => c.IsStaffingNeedsFrequency).TimeScheduleTaskId, base.ActorCompanyId);
                        if (locations != null)
                            locationIds = locations.Select(s => s.StaffingNeedsLocationId).JoinToString("#");
                    }
                    else
                    {
                        result.Success = false;
                        result.ErrorMessage = "Select Schedule task";
                    }
                }

                if (result.Success)
                {
                    List<StaffingNeedsAnalysisChartData> staffingNeedsAnalysisChartData = new List<StaffingNeedsAnalysisChartData>();

                    int interval = model.Interval != 0 ? model.Interval : tsm.GetStaffingNeedsIntervalMinutes(base.ActorCompanyId);

                    if (model.IncomingDeliveryHeadDTOs != null || model.TimeScheduleTaskDTOs != null)
                    {
                        return Content(HttpStatusCode.OK, tsm.CreateStaffingNeedsHeadDTO(base.ActorCompanyId, interval, null, model.TimeScheduleTaskDTOs, model.IncomingDeliveryHeadDTOs, model.Name + "_" + locationIds, model.Date > DateTime.Now.AddYears(-10) ? model.Date : CalendarUtility.DATETIME_DEFAULT, model.DayTypeId, (DayOfWeek?)model.DayOfWeek, model.WholeWeek, model.AdjustPercent, shiftTypeId: shiftTypeId, timeScheduleTaskId: (model.StaffingNeedsFrequencyTimeScheduleTaskId != 0 ? model.StaffingNeedsFrequencyTimeScheduleTaskId : (int?)null)));
                    }
                    else
                    {
                        if (!model.WholeWeek)
                        {
                            if (model.Date != null)
                                staffingNeedsAnalysisChartData = tsm.GetStaffingNeedsChartData(base.ActorCompanyId, model.IntervalDateFrom, model.IntervalDateTo, model.DayOfWeeks, model.Date, interval, (int)TermGroup_StaffingNeedsAnalysisRefType.Unknown, true, 2, null, null, model.AdjustPercent, locations);

                            var dto = tsm.CreateStaffingNeedsHeadDTO(base.ActorCompanyId, interval, staffingNeedsAnalysisChartData.Where(s => s.Value > 0).ToList(), null, null, model.Name + "_" + locationIds, model.Date > DateTime.Now.AddYears(-10) ? model.Date : CalendarUtility.DATETIME_DEFAULT, model.DayTypeId, (DayOfWeek?)model.DayOfWeek, model.WholeWeek, model.AdjustPercent, shiftTypeId: shiftTypeId, timeScheduleTaskId: (model.StaffingNeedsFrequencyTimeScheduleTaskId != 0 ? model.StaffingNeedsFrequencyTimeScheduleTaskId : (int?)null));
                            var old = tsm.GetStaffingNeedsHead(StaffingNeedsHeadType.NeedsPlanning, (DayOfWeek?)model.DayOfWeek, model.Name + "_" + locationIds, base.ActorCompanyId, false, false, false, null);

                            //temp fix remove later
                            if (old == null)
                            {
                                var olds = tsm.GetStaffingNeedsHeads(base.ActorCompanyId, StaffingNeedsHeadType.NeedsPlanning, null);
                                foreach (var item in olds.Where(f => f.Name.Contains("_" + locationIds) || f.Name.EndsWith("_") && f.Date == model.Date && f.FromDate == dto.FromDate).ToList())
                                {
                                    tsm.DeleteStaffingNeedsHead(item.StaffingNeedsHeadId, base.ActorCompanyId);
                                }


                                foreach (var item in olds.Where(f => f.Date == model.Date && f.FromDate == dto.FromDate && f.Created < new DateTime(2018, 11, 28)).ToList())
                                {
                                    tsm.DeleteStaffingNeedsHead(item.StaffingNeedsHeadId, base.ActorCompanyId);
                                }
                            }

                            if (old != null)
                                tsm.DeleteStaffingNeedsHead(old.StaffingNeedsHeadId, base.ActorCompanyId);

                            if (useAccountHierarchy)
                                dto.AccountId = validAccountIds.First();

                            result = tsm.SaveStaffingNeedsHead(dto, base.ActorCompanyId);
                        }
                        else
                        {
                            int day = 1;

                            if (model.CurrentDate < DateTime.Now.AddYears(-100))
                                return Content(HttpStatusCode.OK, new ActionResult(false));

                            var startDate = model.CurrentDate;
                            DateTime currentDate = model.CurrentDate;
                            currentDate = CalendarUtility.AdjustDateToBeginningOfWeek(currentDate);
                            List<StaffingNeedsHeadDTO> staffingNeedsHeadDTOs = new List<StaffingNeedsHeadDTO>();
                            var timeBreakTemplates = tsm.GetTimeBreakTemplates(base.ActorCompanyId).ToDTOs().ToList();

                            while (day <= 7)
                            {
                                List<int> dayOfWeeks = new List<int>() { (int)currentDate.DayOfWeek };

                                staffingNeedsAnalysisChartData = tsm.GetStaffingNeedsChartData(base.ActorCompanyId, model.IntervalDateFrom, model.IntervalDateTo, dayOfWeeks, currentDate, interval, (int)TermGroup_StaffingNeedsAnalysisRefType.Unknown, true, 2, null, null, model.AdjustPercent, locations);
                                var dto = tsm.CreateStaffingNeedsHeadDTO(base.ActorCompanyId, interval, staffingNeedsAnalysisChartData.Where(s => s.Value > 0).ToList(), null, null, model.Name + "_" + day.ToString() + "_" + locationIds, null, 0, currentDate.DayOfWeek, model.WholeWeek, model.AdjustPercent, timeScheduleTaskId: (model.StaffingNeedsFrequencyTimeScheduleTaskId != 0 ? model.StaffingNeedsFrequencyTimeScheduleTaskId : (int?)null), timeBreakTemplates: timeBreakTemplates);
                                dto.FromDate = startDate;
                                staffingNeedsHeadDTOs.Add(dto);
                                var old = tsm.GetStaffingNeedsHead(StaffingNeedsHeadType.NeedsPlanning, currentDate.DayOfWeek, model.Name + "_" + day.ToString() + "_" + locationIds, base.ActorCompanyId, false, false, false, dto.FromDate);

                                //temp fix remove later
                                if (old == null)
                                {
                                    var olds = tsm.GetStaffingNeedsHeads(base.ActorCompanyId, StaffingNeedsHeadType.NeedsPlanning, null);
                                    foreach (var item in olds.Where(f => f.Name.Contains("_" + day.ToString() + "_" + locationIds) && f.Weekday == (int)currentDate.DayOfWeek && f.FromDate == dto.FromDate).ToList())
                                    {
                                        tsm.DeleteStaffingNeedsHead(item.StaffingNeedsHeadId, base.ActorCompanyId);
                                    }

                                    foreach (var item in olds.Where(f => f.Weekday == (int)currentDate.DayOfWeek && f.FromDate == dto.FromDate && f.Created < new DateTime(2018, 11, 28)).ToList())
                                    {
                                        tsm.DeleteStaffingNeedsHead(item.StaffingNeedsHeadId, base.ActorCompanyId);
                                    }
                                }

                                if (old != null)
                                    tsm.DeleteStaffingNeedsHead(old.StaffingNeedsHeadId, base.ActorCompanyId);

                                if (useAccountHierarchy)
                                    dto.AccountId = validAccountIds.First();

                                result = tsm.SaveStaffingNeedsHead(dto, base.ActorCompanyId);
                                day++;
                                currentDate = currentDate.AddDays(1);
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                result = new ActionResult(ex, "CreateStaffingNeedsHeads");
            }

            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("StaffingNeedsHead/CreateShifts/")]
        public IHttpActionResult CreateShiftsFromStaffingNeeds(GenerateStaffingNeedsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, tsm.CreateShiftsFromStaffingNeeds(model.DateFrom, model.DateTo));
            }
        }

        [HttpDelete]
        [Route("StaffingNeedsHead/{staffingNeedsHeadId:int}")]
        public IHttpActionResult DeleteStaffingNeedsHead(int staffingNeedsHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsHead(staffingNeedsHeadId, base.ActorCompanyId));
        }

        #endregion

        #region StaffingNeedsLocationGroup

        [HttpGet]
        [Route("StaffingNeedsLocationGroup")]
        public IHttpActionResult GetStaffingNeedsLocationGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocationGroupsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("includeAccountName")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocationGroups(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("StaffingNeedsLocationGroup/{locationGroupId:int}")]
        public IHttpActionResult GetStaffingNeedsLocationGroup(int locationGroupId)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocationGroup(locationGroupId).ToDTO(true));
        }

        [HttpPost]
        [Route("StaffingNeedsLocationGroup")]
        public IHttpActionResult SaveStaffingNeedsLocationGroup(SaveStaffingNeedsLocationGroupModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsLocationGroup(model.Dto, model.ShiftTypeIds, base.ActorCompanyId));

        }

        [HttpDelete]
        [Route("StaffingNeedsLocationGroup/{locationGroupId:int}")]
        public IHttpActionResult DeleteStaffingNeedsLocationGroup(int locationGroupId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsLocationGroup(locationGroupId));
        }

        #endregion

        #region StaffingNeedsLocation

        [HttpGet]
        [Route("StaffingNeedsLocation")]
        public IHttpActionResult GetStaffingNeedsLocations()
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocations(base.ActorCompanyId, null).ToGridDTOs(true, true));
        }

        [HttpGet]
        [Route("StaffingNeedsLocation/{locationId:int}")]
        public IHttpActionResult GetStaffingNeedsLocation(int locationId)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocation(locationId).ToDTO());
        }

        [HttpPost]
        [Route("StaffingNeedsLocation")]
        public IHttpActionResult SaveStaffingNeedsLocation(StaffingNeedsLocationDTO dto)
        {
            return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsLocation(dto));

        }

        [HttpDelete]
        [Route("StaffingNeedsLocation/{locationId:int}")]
        public IHttpActionResult DeleteStaffingNeedsLocation(int locationId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsLocation(locationId));
        }

        #endregion

        #region StaffingNeedsRule

        [HttpGet]
        [Route("StaffingNeedsRule")]
        public IHttpActionResult GetStaffingNeedsRules()
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsRules(base.ActorCompanyId).ToGridDTOs(true).ToList());
        }

        [HttpGet]
        [Route("StaffingNeedsRule/{ruleId:int}")]
        public IHttpActionResult GetStaffingNeedsRule(int ruleId)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsRule(ruleId).ToDTO(true));
        }

        [HttpPost]
        [Route("StaffingNeedsRule")]
        public IHttpActionResult SaveStaffingNeedsRule(StaffingNeedsRuleDTO dto)
        {
            return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsRule(dto));

        }

        [HttpDelete]
        [Route("StaffingNeedsRule/{ruleId:int}")]
        public IHttpActionResult DeleteStaffingNeedsRule(int ruleId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsRule(ruleId));
        }

        #endregion

        #region TimeBreakTemplate

        [HttpGet]
        [Route("TimeBreakTemplate/")]
        public IHttpActionResult GetTimeBreakTemplates()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeBreakTemplateGrids(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("TimeBreakTemplate/HasTimeBreakTemplates")]
        public IHttpActionResult HasTimeBreakTemplates()
        {
            return Content(HttpStatusCode.OK, tsm.HasTimeBreakTemplates(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeBreakTemplate")]
        public IHttpActionResult SaveTimeBreakTemplates(TimeBreakTemplatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeBreakTemplates(model.BreakTemplates));
        }

        [HttpPost]
        [Route("TimeBreakTemplate/Validate")]
        public IHttpActionResult ValidateSaveTimeBreakTeamplates(TimeBreakTemplatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.ValidateBreakTemplates(model.BreakTemplates));
        }

        [HttpPost]
        [Route("TimeBreakTemplate/CreateBreaksForEmployee")]
        public IHttpActionResult CreateBreaksFromTemplatesForEmployee(CreateBreaksFromTemplatesForEmployeeModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateBreaksFromTemplatesForEmployee(model.Shifts, model.EmployeeId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeBreakTemplate/CreateBreaksForEmployees")]
        public IHttpActionResult CreateBreaksFromTemplatesForEmployees(CreateBreaksFromTemplatesForEmployeesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateBreaksFromTemplatesForEmployees(model.Date, model.EmployeeIds, base.ActorCompanyId, model.TimeScheduleScenarioHeadId));
        }

        #endregion

        #region TimeCodeBreak

        [HttpGet]
        [Route("TimeCodeBreak/{employeeId:int}/{date}/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeCodeBreaksForEmployee(int employeeId, string date, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreaksForEmployee(base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true), addEmptyRow).ToSmallBreakDTOs());
        }

        [HttpGet]
        [Route("TimeCodeBreak/EmployeePost/{employeePostId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeCodeBreaksForEmployeePost(int employeePostId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreaksForEmployeePost(base.ActorCompanyId, employeePostId, addEmptyRow).ToSmallBreakDTOs());
        }

        #endregion

        #region TimeLeisureCode

        [HttpGet]
        [Route("TimeLeisureCode/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeLeisureCodesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeLeisureCodesDict(base.ActorCompanyId, addEmptyRow));
        }

        [HttpGet]
        [Route("TimeLeisureCode/Small")]
        public IHttpActionResult GetTimeLeisureCodesSmall()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeLeisureCodes(base.ActorCompanyId).ToSmallDTOs());
        }

        [HttpPost]
        [Route("TimeLeisureCode/AllocateLeisureDays")]
        public IHttpActionResult AllocateLeisureDays(AllocateLeisureDaysModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.AllocateLeisureDays(base.ActorCompanyId, model.EmployeeIds, model.StartDate, model.StopDate));
        }

        [HttpPost]
        [Route("TimeLeisureCode/AllocateLeisureDays/Delete")]
        public IHttpActionResult AllocateLeisureDaysDelete(AllocateLeisureDaysModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.AllocateLeisureDaysDelete(base.ActorCompanyId, model.EmployeeIds, model.StartDate, model.StopDate));
        }

        #endregion

        #region TimeScheduleCopy

        [HttpGet]
        [Route("TimeScheduleCopyHead/Dict/{type:int}")]
        public IHttpActionResult GetTimeScheduleCopyHeadsDict(TermGroup_TimeScheduleCopyHeadType type)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleCopyHeadsDict(base.ActorCompanyId, type).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeScheduleCopyRow/Employee/Dict/{timeScheduleCopyHeadId:int}")]
        public IHttpActionResult GetTimeScheduleCopyRowEmployeesDict(int timeScheduleCopyHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleCopyRowEmployeesDict(base.ActorCompanyId, timeScheduleCopyHeadId).ToSmallGenericTypes());
        }

        #endregion

        #region TimeScheduleEmployeePeriod

        [HttpGet]
        [Route("TimeScheduleEmployeePeriodId/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetTimeScheduleEmployeePeriodId(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEmployeePeriodId(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true).Value));
        }

        #endregion

        #region TimeScheduleEmployeePeriodDetail

        [HttpPost]
        [Route("TimeScheduleEmployeePeriodDetail")]
        public IHttpActionResult SaveTimeScheduleEmployeePeriodDetail(TimeScheduleEmployeePeriodDetailDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleEmployeePeriodDetail(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeScheduleEmployeePeriodDetail/Delete")]
        public IHttpActionResult DeleteTimeScheduleEmployeePeriodDetail(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleEmployeePeriodDetail(model.Numbers, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleEvent

        [HttpGet]
        [Route("TimeScheduleEvent/")]
        public IHttpActionResult GetTimeScheduleEvents(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEvents(base.ActorCompanyId).ToDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEventsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEvents(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleEvent/GetTimeScheduleEventDatesForPlanning/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetTimeScheduleEventDatesForPlanning(string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEventDatesForPlanning(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value));
        }

        [HttpGet]
        [Route("TimeScheduleEvent/GetTimeScheduleEventsForPlanning/{date}")]
        public IHttpActionResult GetTimeScheduleEventsForPlanning(string date)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEventsForPlanning(base.ActorCompanyId, base.UserId, BuildDateTimeFromString(date, true).Value));
        }

        [HttpGet]
        [Route("TimeScheduleEvent/{timeScheduleEventId:int}")]
        public IHttpActionResult GetTimeScheduleEvent(int timeScheduleEventId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEvent(timeScheduleEventId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("TimeScheduleEvent")]
        public IHttpActionResult SaveTimeScheduleEvent(TimeScheduleEventDTO timeScheduleEvent)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleEvent(timeScheduleEvent, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleEvent/{timeScheduleEventId:int}")]
        public IHttpActionResult DeleteTimeScheduleEvent(int timeScheduleEventId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleEvent(timeScheduleEventId, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleScenarioEmployee

        [HttpGet]
        [Route("TimeScheduleScenarioEmployee/{timeScheduleScenarioHeadId:int}")]
        public IHttpActionResult GetTimeScheduleScenarioEmployeeIds(int timeScheduleScenarioHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleScenarioEmployeeIds(timeScheduleScenarioHeadId, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleScenarioHead

        [HttpGet]
        [Route("TimeScheduleScenarioHead/{timeScheduleScenarioHeadId:int}/{loadEmployees:bool}/{loadAccounts:bool}")]
        public IHttpActionResult GetTimeScheduleScenarioHead(int timeScheduleScenarioHeadId, bool loadEmployees, bool loadAccounts)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleScenarioHead(timeScheduleScenarioHeadId, base.ActorCompanyId, loadEmployees, loadAccounts).ToDTO());
        }

        [HttpGet]
        [Route("TimeScheduleScenarioHead/Activate/Preview/{timeScheduleScenarioHeadId:int}/{preliminaryDateFromString}")]
        public IHttpActionResult PreviewActivateScenario(int timeScheduleScenarioHeadId, string preliminaryDateFromString)
        {
            return Content(HttpStatusCode.OK, tsm.PreviewActivateScenario(timeScheduleScenarioHeadId, base.ActorCompanyId, base.RoleId, base.UserId, BuildDateTimeFromString(preliminaryDateFromString, true)));
        }

        [HttpGet]
        [Route("TimeScheduleScenarioHead/Activate/Status/{timeScheduleScenarioHeadId:int}")]
        public IHttpActionResult GetActivateScenarioEmployeeStatus(int timeScheduleScenarioHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetActivateScenarioEmployeeStatus(timeScheduleScenarioHeadId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeScheduleScenarioHead/Activate/")]
        public IHttpActionResult ActivateScenario(ActivateScenarioDTO model)
        {
            if (IsDuplicateRequest(model.Key, model?.Rows?.Count ?? 5, model?.Rows?.Count ?? 5))
                return Content(HttpStatusCode.Unauthorized, new ActionResult(false));

            return Content(HttpStatusCode.OK, tem.ActivateScenario(model));
        }

        [HttpGet]
        [Route("TimeScheduleScenarioHead/CreateTemplate/Preview/{timeScheduleScenarioHeadId:int}/{dateFromString}/{weekInCycle:int}/{dateToString}")]
        public IHttpActionResult PreviewCreateTemplateFromScenario(int timeScheduleScenarioHeadId, string dateFromString, int weekInCycle, string dateToString)
        {
            return Content(HttpStatusCode.OK, tsm.PreviewCreateTemplateFromScenario(timeScheduleScenarioHeadId, base.ActorCompanyId, base.RoleId, base.UserId, BuildDateTimeFromString(dateFromString, true).Value, weekInCycle, BuildDateTimeFromString(dateToString, true)));
        }

        [HttpPost]
        [Route("TimeScheduleScenarioHead/CreateTemplate/")]
        public IHttpActionResult CreateTemplateFromScenario(CreateTemplateFromScenarioDTO model)
        {
            return Content(HttpStatusCode.OK, tem.CreateTemplateFromScenario(model));
        }

        [HttpPost]
        [Route("TimeScheduleScenarioHead/Dict")]
        public IHttpActionResult GetTimeScheduleScenarioHeadsDict(GetTimeScheduleScenarioHeadsModel model)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleScenarioHeadsDict(base.ActorCompanyId, model.ValidAccountIds, model.AddEmptyRow).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("TimeScheduleScenarioHead")]
        public IHttpActionResult SaveTimeScheduleScenarioHead(SaveTimeScheduleScenarioHeadModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveTimeScheduleScenarioHead(model.ScenarioHead, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.IncludeAbsence, model.DateFunction));
        }

        [HttpDelete]
        [Route("TimeScheduleScenarioHead/{timeScheduleScenarioHead:int}")]
        public IHttpActionResult DeleteTimeScheduleScenarioHead(int timeScheduleScenarioHead)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleScenarioHead(timeScheduleScenarioHead, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleTask

        [HttpGet]
        [Route("TimeScheduleTask/")]
        public IHttpActionResult GetTimeScheduleTasks(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasksDict(base.ActorCompanyId, true).ToSmallGenericTypes());

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasks(base.ActorCompanyId, true, true, true, false).ToGridDTOs());

            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasks(base.ActorCompanyId).ToDTOs(false));
        }

        [HttpGet]
        [Route("TimeScheduleTask/Frequency/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeScheduleTasksForFrequency(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasksForFrequency(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeScheduleTask/GetTimeScheduleTasksForInterval")]
        public IHttpActionResult GetTimeScheduleTasksForInterval(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasks(base.ActorCompanyId, message.GetDateValueFromQS("dateFrom").Value, message.GetDateValueFromQS("dateTo").Value, message.GetIntListValueFromQS("ids"), loadAccounting: true).ToDTOs(true));
        }

        [HttpGet]
        [Route("TimeScheduleTask/{timeScheduleTaskId:int}/{loadAccounts:bool}/{loadExcludedDates:bool}/{loadAccountHierarchyAccount:bool}")]
        public IHttpActionResult GetTimeScheduleTask(int timeScheduleTaskId, bool loadAccounts, bool loadExcludedDates, bool loadAccountHierarchyAccount)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTask(timeScheduleTaskId, base.ActorCompanyId, loadAccounts: loadAccounts, loadExcludedDates: loadExcludedDates, loadAccountHierarchyAccount: loadAccountHierarchyAccount, onlyActive: false).ToDTO(loadAccounts));
        }

        [HttpPost]
        [Route("TimeScheduleTask")]
        public IHttpActionResult SaveTimeScheduleTask(TimeScheduleTaskDTO timeScheduleTask)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleTask(timeScheduleTask, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleTask/{timeScheduleTaskId:int}")]
        public IHttpActionResult DeleteTimeScheduleTask(int timeScheduleTaskId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleTask(timeScheduleTaskId, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleTaskType

        [HttpGet]
        [Route("TimeScheduleTaskType/")]
        public IHttpActionResult GetTimeScheduleTaskTypes(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskTypes(base.ActorCompanyId).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskTypesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskTypes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTaskTypeGrid/")]
        public IHttpActionResult GetTimeScheduleTaskTypesGrid()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskTypes(base.ActorCompanyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTaskType/{timeScheduleTaskTypeId:int}")]
        public IHttpActionResult GetTimeScheduleTaskType(int timeScheduleTaskTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskType(timeScheduleTaskTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("TimeScheduleTaskType")]
        public IHttpActionResult SaveTimeScheduleTaskType(TimeScheduleTaskTypeDTO timeScheduleTaskType)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleTaskType(timeScheduleTaskType, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleTaskType/{timeScheduleTaskTypeId:int}")]
        public IHttpActionResult DeleteTimeScheduleTaskType(int timeScheduleTaskTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleTaskType(timeScheduleTaskTypeId, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleTypes

        [HttpGet]
        [Route("TimeScheduleType")]
        public IHttpActionResult GetTimeScheduleTypes(HttpRequestMessage message)
        {
            bool getAll = message.GetBoolValueFromQS("getAll");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypesDict(base.ActorCompanyId, getAll, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            bool loadFactors = message.GetBoolValueFromQS("loadFactors");
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypes(base.ActorCompanyId, getAll: getAll, onlyActive: message.GetBoolValueFromQS("onlyActive"), loadFactors: loadFactors).ToSmallDTOs(loadFactors));
        }

        #endregion

        #region TimeScheduleTemplate

        [HttpGet]
        [Route("TimeScheduleTemplate/{timeScheduleTemplateHeadId:int}/{loadEmployeeSchedule:bool}/{loadAccounts:bool}")]
        public TimeScheduleTemplateHeadDTO GetTimeScheduleTemplate(int timeScheduleTemplateHeadId, bool loadEmployeeSchedule, bool loadAccounts)
        {
            return tem.GetTimeScheduleTemplate(timeScheduleTemplateHeadId, loadEmployeeSchedule, loadAccounts).ToDTO(true, true, loadEmployeeSchedule, loadAccounts, false, false);
        }

        [HttpPost]
        [Route("TimeScheduleTemplate/SaveTimeScheduleTemplate/")]
        public IHttpActionResult SaveTimeScheduleTemplate(SaveTimeScheduleTemplateModel model)
        {
            return Content(HttpStatusCode.OK, tem.SaveTimeScheduleTemplate(model.Head.FromDTO(), model.Blocks));
        }

        #endregion

        #region TimeScheduleTemplateGroup

        [HttpGet]
        [Route("TimeScheduleTemplateGroup/")]
        public IHttpActionResult GetTimeScheduleTemplateGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateGroups(base.ActorCompanyId, true, true).ToGridDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateGroupsDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateGroups(base.ActorCompanyId, false, false).ToDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTemplateGroup/Employee/{employeeId:int}/{loadGroup:bool}/{loadRows:bool}")]
        public IHttpActionResult GetTimeScheduleTemplateGroupsForEmployee(int employeeId, bool loadGroup, bool loadRows)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateGroupsForEmployee(base.ActorCompanyId, employeeId, loadGroup, loadRows).ToDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTemplateGroup/{timeScheduleTemplateGroupId:int}/{loadRows:bool}/{loadEmployees:bool}/{setNextStartDateOnRows:bool}/{setEmployeeInfo:bool}")]
        public IHttpActionResult GetTimeScheduleTemplateGroup(int timeScheduleTemplateGroupId, bool loadRows, bool loadEmployees, bool setNextStartDateOnRows, bool setEmployeeInfo)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateGroup(timeScheduleTemplateGroupId, base.ActorCompanyId, loadRows, loadEmployees, setNextStartDateOnRows, setEmployeeInfo).ToDTO());
        }

        [HttpGet]
        [Route("TimeScheduleTemplateGroup/Row/NextStartDate/{start}/{stop}/{recurrencePattern}")]
        public IHttpActionResult GetTimeScheduleTemplateGroupRowNextStartDate(string start, string stop, string recurrencePattern)
        {
            DateTime startDate = BuildDateTimeFromString(start, true).Value;
            DateTime? stopDate = BuildDateTimeFromString(stop, true);

            //DateTime visibleDateFrom = DateTime.Today;
            //if (visibleDateFrom < startDate)
            //    visibleDateFrom = startDate;

            DateTime visibleDateFrom = startDate;

            DateTime visibleDateTo = visibleDateFrom.AddYears(1);
            if (stopDate.HasValue && visibleDateTo > stopDate.Value)
                visibleDateTo = stopDate.Value;

            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateGroupRowNextStartDate(startDate, recurrencePattern, visibleDateFrom, visibleDateTo));
        }

        [HttpGet]
        [Route("TimeScheduleTemplateGroup/Head/Range/{timeScheduleTemplateGroupId:int}/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetTimeScheduleTemplateHeadsRange(int timeScheduleTemplateGroupId, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHeadsRange(timeScheduleTemplateGroupId, base.ActorCompanyId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value));
        }

        [HttpGet]
        [Route("TimeScheduleTemplateGroup/Head/Range/Employee/{employeeId:int}/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetTimeScheduleTemplateHeadsRangeForEmployee(int employeeId, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHeadsRangeForEmployee(employeeId, base.ActorCompanyId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateGroup")]
        public IHttpActionResult SaveTimeScheduleTemplateGroup(TimeScheduleTemplateGroupDTO timeScheduleTemplateGroup)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleTemplateGroup(timeScheduleTemplateGroup, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleTemplateGroup/{timeScheduleTemplateGroupId:int}")]
        public IHttpActionResult DeleteTimeScheduleTemplateGroup(int timeScheduleTemplateGroupId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleTemplateGroup(timeScheduleTemplateGroupId, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleTemplateHead

        [HttpGet]
        [Route("TimeScheduleTemplateHead/")]
        public IHttpActionResult GetTimeScheduleTemplateHeads()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHeadsIncludingEmployeeNames(base.ActorCompanyId, false, false, true).ToDTOs(false, false, false, false, true, true));
        }

        [HttpGet]
        [Route("TimeScheduleTemplateHead/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult GetTimeScheduleTemplateHead(int timeScheduleTemplateHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHead(timeScheduleTemplateHeadId, base.ActorCompanyId, true).ToSmallDTO());
        }

        [HttpGet]
        [Route("TimeScheduleTemplateHead/{employeeId:int}/{dateLimitFromString}/{dateLimitToString}/{intersecting:bool}/{excludeMultipleAccounts:bool}/{includePublicTemplates:bool}")]
        public IHttpActionResult GetTimeScheduleTemplateHeadsForEmployee(int employeeId, string dateLimitFromString, string dateLimitToString, bool intersecting, bool excludeMultipleAccounts, bool includePublicTemplates)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHeadsForEmployee(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateLimitFromString, true), BuildDateTimeFromString(dateLimitToString, true), intersecting, excludeMultipleAccounts: excludeMultipleAccounts, includePublicTemplates: includePublicTemplates).ToSmallDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTemplateHead/Overlapping/{employeeId:int}/{dateString}")]
        public IHttpActionResult GetOverlappingTemplates(int employeeId, string dateString)
        {
            return Content(HttpStatusCode.OK, tsm.GetOverlappingTemplates(base.ActorCompanyId, employeeId, BuildDateTimeFromString(dateString, true).Value));
        }

        [HttpGet]
        [Route("TimeScheduleTemplateHead/{dateLimitFromString}/{dateLimitToString}/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult GetTimeScheduleTemplateHeadForEmployee(string dateLimitFromString, string dateLimitToString, int timeScheduleTemplateHeadId)
        {
            int currentEmployeeId = em.GetEmployeeIdForUser(base.UserId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, tsm.GetTimeSchedulePlanningTemplate(base.ActorCompanyId, base.RoleId, base.UserId, currentEmployeeId, timeScheduleTemplateHeadId, BuildDateTimeFromString(dateLimitFromString, true).Value, BuildDateTimeFromString(dateLimitToString, true).Value));
        }

        [HttpGet]
        [Route("TimeScheduleTemplateHead/Activate/")]
        public IHttpActionResult GetTimeScheduleTemplateHeadsForActivate()
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateHeads(base.ActorCompanyId, false, false, true, true).ToSmallDTOs());
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/SaveTimeScheduleTemplate/")]
        public IHttpActionResult SaveTimeScheduleTemplate(SaveTimeScheduleTemplateAndPlacementModel model)
        {
            return Content(HttpStatusCode.OK, tem.UpdateTimeScheduleTemplateStaffing(model.Shifts, model.EmployeeId, model.TimeScheduleTemplateHeadId, model.DayNumberFrom, model.DayNumberTo, model.CurrentDate, model.ActivateDates, model.ActivateDayNumber, model.SkipXEMailOnChanges));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/SaveTimeScheduleTemplateAndPlacement/")]
        public IHttpActionResult SaveTimeScheduleTemplateAndPlacement(SaveTimeScheduleTemplateAndPlacementModel model)
        {
            if (IsDuplicateRequest(model.Key, 30, 30))
                return Content(HttpStatusCode.Unauthorized, new ActionResult(false));

            return Content(HttpStatusCode.OK, tem.SaveTimeScheduleTemplateAndPlacement(model.SaveTemplate, model.SavePlacement, model.Control, model.Shifts, model.TimeScheduleTemplateHeadId, model.TemplateNoOfDays, model.TemplateStartDate, model.TemplateStopDate, model.FirstMondayOfCycle, model.PlacementDateFrom, model.PlacementDateTo, model.CurrentDate, model.EmployeeId, null, model.CopyFromTimeScheduleTemplateHeadId, model.SimpleSchedule, model.StartOnFirstDayOfWeek, model.Preliminary, model.Locked, false, model.UseAccountingFromSourceSchedule));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/AssignTimeScheduleTemplateToEmployee/{timeScheduleTemplateHeadId:int}/{employeeId:int}/{startDate}")]
        public IHttpActionResult AssignTimeScheduleTemplateToEmployee(int timeScheduleTemplateHeadId, int employeeId, string startDate)
        {
            return Content(HttpStatusCode.OK, tem.AssignTimeScheduleTemplateToEmployee(timeScheduleTemplateHeadId, employeeId, BuildDateTimeFromString(startDate, true).Value));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/RemoveEmployee/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult RemoveEmployeeFromTimeScheduleTemplate(int timeScheduleTemplateHeadId)
        {
            return Content(HttpStatusCode.OK, tem.RemoveEmployeeFromTimeScheduleTemplate(timeScheduleTemplateHeadId));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/UpdateState")]
        public IHttpActionResult UpdateTimeScheduleTemplateHeadState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.UpdateTimeScheduleTemplateHeadsState(model.Dict, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/GetOngoing")]
        public IHttpActionResult GetOngoingTimeScheduleTemplateHeads(DictIntDateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetOngoingTimeScheduleTemplateHeads(base.ActorCompanyId, model.Dict));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateHead/SetStopDate")]
        public IHttpActionResult SetStopDateOnTimeScheduleTemplateHeads(DictIntDateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SetStopDateOnTimeScheduleTemplateHeads(base.ActorCompanyId, model.Dict));
        }

        [HttpDelete]
        [Route("TimeScheduleTemplateHead/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult DeleteTimeScheduleTemplate(int timeScheduleTemplateHeadId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteTimeScheduleTemplate(timeScheduleTemplateHeadId));
        }

        #endregion

        #region TimeScheduleTemplatePeriod

        [HttpGet]
        [Route("TimeScheduleTemplatePeriod/Activate/{timeScheduleTemplateHeadId:int}")]
        public IHttpActionResult GetTimeScheduleTemplatePeriodsForActivate(int timeScheduleTemplateHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplatePeriods(timeScheduleTemplateHeadId, false).ToSmallDTOs());
        }

        #endregion

        #region TimeScheduleTemplateBlock

        [HttpGet]
        [Route("TimeScheduleTemplateBlockHistory/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult GetTimeScheduleTemplateBlockHistory(int timeScheduleTemplateBlockId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateBlockHistory(base.ActorCompanyId, timeScheduleTemplateBlockId));
        }

        [HttpPost]
        [Route("TimeScheduleTemplateChanges/")]
        public IHttpActionResult GetTimeScheduleTemplateChanges(GetTimeScheduleTemplateChanges model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateChanges(base.ActorCompanyId, model.EmployeeId, model.TimeScheduleTemplateHeadId, model.Date, model.DateFrom, model.DateTo, model.Shifts));
        }

        [HttpPost]
        [Route("CreateStringFromShifts/")]
        public IHttpActionResult CreateStringFromShifts(CreateStringFromShiftsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.CreateStringFromShifts(base.ActorCompanyId, model.Shifts));
        }

        #endregion

        #region Work rules

        [HttpPost]
        [Route("EvaluateWorkRule/SaveEvaluateAllWorkRulesByPass/")]
        public IHttpActionResult SaveEvaluateAllWorkRulesByPass(SaveEvaluateAllWorkRulesByPassModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SaveEvaluateAllWorkRulesByPass(model.Result, model.EmployeeId));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/All")]
        public IHttpActionResult EvaluateAllWorkRules(EvaluateAllWorkRulesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateAllWorkRules(model.Shifts, model.EmployeeIds, model.StartDate, model.StopDate, model.IsPersonalScheduleTemplate, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.Rules, model.PlanningPeriodStartDate, model.PlanningPeriodStopDate));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/Drag")]
        public IHttpActionResult EvaluateDragShiftAgainstWorkRules(EvaluateWorkRulesDragModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateDragShiftAgainstWorkRules(model.Action, model.SourceShiftId, model.TargetShiftId, model.Start, model.End, model.EmployeeId, model.IsPersonalScheduleTemplate, model.WholeDayAbsence, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.StandbyCycleWeek, model.StandbyCycleDateFrom, model.StandbyCycleDateTo, model.IsStandByView, model.Rules, true, true, model.FromQueue ?? false, model.PlanningPeriodStartDate, model.PlanningPeriodStopDate));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/Template/Drag")]
        public IHttpActionResult EvaluateDragTemplateShiftAgainstWorkRules(EvaluateWorkRulesDragTemplateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateDragTemplateShiftAgainstWorkRules(model.Action, model.SourceShiftId, model.SourceTemplateHeadId, model.SourceDate, model.TargetShiftId, model.TargetTemplateHeadId, model.Start, model.End, model.EmployeeId, model.EmployeePostId, model.Rules, true, true));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/DragMultiple")]
        public IHttpActionResult EvaluateDragShiftsAgainstWorkRules(EvaluateWorkRulesDragMultipleModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateDragShiftMultipelAgainstWorkRules(model.Action, model.SourceShiftIds, model.OffsetDays, model.EmployeeId, model.IsPersonalScheduleTemplate, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.StandbyCycleWeek, model.StandbyCycleDateFrom, model.StandbyCycleDateTo, model.IsStandByView, model.Rules, model.PlanningPeriodStartDate, model.PlanningPeriodStopDate));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/Template/DragMultiple")]
        public IHttpActionResult EvaluateDragTemplateShiftsAgainstWorkRules(EvaluateWorkRulesDragTemplateMultipleModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateDragTemplateShiftMultipelAgainstWorkRules(model.Action, model.SourceShiftIds, model.SourceTemplateHeadId, model.FirstSourceDate, model.OffsetDays, model.EmployeeId, model.EmployeePostId, model.TargetTemplateHeadId, model.FirstTargetDate, model.Rules));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/Planned")]
        public IHttpActionResult EvaluatePlannedShiftsAgainstWorkRules(EvaluateWorkRulesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluatePlannedShiftsAgainstWorkRules(model.Shifts, model.IsPersonalScheduleTemplate, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, rules: model.Rules, planningPeriodStartDate: model.PlanningPeriodStartDate, planningPeriodStopDate: model.PlanningPeriodStopDate));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/EmployeePost/Planned")]
        public IHttpActionResult EvaluateEmployeePostPlannedShiftsAgainstWorkRules(EvaluateWorkRulesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateEmployeePostPlannedShiftsAgainstWorkRules(model.Shifts, model.Rules));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/Split")]
        public IHttpActionResult EvaluateSplitShiftAgainstWorkRules(EvaluateWorkRulesSplitModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateSplitShiftAgainstWorkRules(model.Shift, model.SplitTime, model.EmployeeId1, model.EmployeeId2, model.KeepShiftsTogether, model.IsPersonalScheduleTemplate, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.PlanningPeriodStartDate, model.PlanningPeriodStopDate));
        }

        [HttpPost]
        [Route("EvaluateWorkRule/Template/Split")]
        public IHttpActionResult EvaluateSplitTemplateShiftAgainstWorkRules(SplitTemplateShiftModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateSplitTemplateShiftAgainstWorkRules(model.SourceShift, model.SourceTemplateHeadId, model.SplitTime, model.EmployeeId1, model.EmployeePostId1, model.TemplateHeadId1, model.EmployeeId2, model.EmployeePostId2, model.TemplateHeadId2, model.KeepShiftsTogether));
        }

        [HttpGet]
        [Route("WorkRuleBypassLog/{dateSelection:int}")]
        public IHttpActionResult GetWorkRuleBypassLog(TermGroup_ChangeStatusGridAllItemsSelection dateSelection)
        {
            return Content(HttpStatusCode.OK, tsm.GetWorkRulebypassLog(base.ActorCompanyId, base.UserId, base.RoleId, dateSelection, true));
        }

        [HttpGet]
        [Route("IsDayAttested/{employeeId:int}/{date}")]
        public IHttpActionResult IsDayAttested(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, tem.IsDayAttested(employeeId, BuildDateTimeFromString(date, true).Value));
        }

        #endregion

        #region Weekend Salary
        [HttpGet]
        [Route("UsesWeekendSalary")]
        public IHttpActionResult UsesWeekendSalary()
        {
            return Content(HttpStatusCode.OK, tsm.UsesWeekendSalary(base.ActorCompanyId));
        }

        #endregion
    }
}