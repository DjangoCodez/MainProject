using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/AbsenceRequest")]
    public class AbsenceRequestController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;
        private readonly TimeEngineManager tem;
        private readonly TimeDeviationCauseManager tdcm;
        private readonly SettingManager sm;
        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public AbsenceRequestController(TimeScheduleManager tsm, TimeEngineManager tem, TimeDeviationCauseManager tdcm, SettingManager sm, EmployeeManager em)
        {
            this.tsm = tsm;
            this.tem = tem;
            this.tdcm = tdcm;
            this.sm = sm;
            this.em = em;
        }

        #endregion

        #region AbsenceRequest new
        [HttpGet]
        [Route("Get/{employeeRequestId:int}")]
        public IHttpActionResult GetAbsenceRequest(int employeeRequestId)
        {
            return Content(HttpStatusCode.OK, tem.LoadEmployeeRequest(employeeRequestId).ToDTO(true, true));

        }

        [HttpGet]
        [Route("GetGrid/{employeeRequestId:int?}")]
        public IHttpActionResult GetAbsenceRequestGrid(int employeeId, bool loadPreliminary, bool loadDefinitive, int? employeeRequestId = null)
        {
            EmployeeRequestStatus status;
            if (loadPreliminary && loadDefinitive)
                status = EmployeeRequestStatus.PreliminaryAndDefinitive;
            else if (!loadPreliminary && !loadDefinitive)
                status = EmployeeRequestStatus.None;
            else
                status = loadPreliminary ? EmployeeRequestStatus.Preliminary : EmployeeRequestStatus.Definitive;

            switch (status)
            {
                case EmployeeRequestStatus.Preliminary:
                    return Content(HttpStatusCode.OK, tem.GetEmployeeRequests(employeeId, employeeRequestId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).ToGridDTOs().Where(e => e.Status != TermGroup_EmployeeRequestStatus.Definate));
                case EmployeeRequestStatus.Definitive:
                    return Content(HttpStatusCode.OK, tem.GetEmployeeRequests(employeeId, employeeRequestId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).ToGridDTOs().Where(e => e.Status == TermGroup_EmployeeRequestStatus.Definate));
                case EmployeeRequestStatus.PreliminaryAndDefinitive:
                    return Content(HttpStatusCode.OK, tem.GetEmployeeRequests(employeeId, employeeRequestId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).ToGridDTOs());
                default:
                    return Content(HttpStatusCode.OK, new List<EmployeeRequestGridDTO>());
            }
        }

        [HttpPost]
        [Route("Save")]
        public IHttpActionResult SaveAbsenceRequest(SaveAbsenceRequestModel model)
        {
            model.Request.ActorCompanyId = base.ActorCompanyId;
            return Content(HttpStatusCode.OK, tem.SaveEmployeeRequest(model.Request.FromDTO(), model.EmployeeId, model.RequestType, model.SkipXEMailOnShiftChanges, model.IsForcedDefinitive));
        }

        [HttpDelete]
        [Route("Delete/{employeeRequestId:int}")]
        public IHttpActionResult DeleteAbsenceRequest(int employeeRequestId)
        {
            return Content(HttpStatusCode.OK, tem.DeleteEmployeeRequest(employeeRequestId));
        }

        [HttpGet]
        [Route("Absencerequest/History/{absenceRequestId:int}")]
        public IHttpActionResult GetAbsenceRequestHistory(int absenceRequestId)
        {
            return Content(HttpStatusCode.OK, tsm.GetAbsenceRequestHistory(base.ActorCompanyId, absenceRequestId));
        }

        [HttpPost]
        [Route("Absencerequest/AffectedShifts")]
        public IHttpActionResult GetAbsenceRequestAffectedShifts(GetAbsenceRequestAffectedShiftsModel model)
        {
            model.Request.Start = CalendarUtility.GetBeginningOfDay(model.Request.Start);
            model.Request.Stop = CalendarUtility.GetEndOfDay(model.Request.Stop);
            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetAbsenceRequestAffectedShifts(
                base.ActorCompanyId,
                base.UserId,
                model.TimeScheduleScenarioHeadId.ToNullable(),
                model.Request,
                model.ShiftUserStatus,
                model.ExtendedSettings.FromDTO()
            ).ToList();
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            return Content
                (HttpStatusCode.OK, 
                dtos
            );

        }

        #endregion
        #region Absence
        [HttpPost]
        [Route("Absence/Shifts")]
        public IHttpActionResult GetAbsenceAffectedShiftsFromShift(GetAbsenceAffectedShiftsModel model)
        {
            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetShifts(
                base.ActorCompanyId,
                model.EmployeeId,
                model.ShiftId,
                model.TimeScheduleScenarioHeadId.ToNullable(),
                model.IncludeLinkedShifts,
                model.GetAllshifts,
                model.TimeDeviationCauseId
            );
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            return Content(
                HttpStatusCode.OK, 
                dtos
            );
        }

        // Used in AbsenceRequest when not pending
        [HttpPost]
        [Route("Absence/AffectedShifts")]
        public IHttpActionResult GetAbsenceAffectedShifts(GetAbsenceAffectedShiftsModel model)
        {
            ExtendedAbsenceSetting extendedAbsenceSettings = null;
            if (model.ExtendedSettings != null)
                extendedAbsenceSettings = model.ExtendedSettings.FromDTO();
            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetAbsenceAffectedShifts(
                base.ActorCompanyId, 
                base.UserId, 
                model.EmployeeId, 
                model.TimeScheduleScenarioHeadId.ToNullable(), 
                model.DateFrom, 
                model.DateTo, 
                model.TimeDeviationCauseId, 
                extendedAbsenceSettings, 
                model.IncludeAlreadyAbsence
                ).ToList();
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            return Content(HttpStatusCode.OK, dtos);

        }

        [HttpPost]
        [Route("Absence/ShiftsForQuickAbsence")]
        public IHttpActionResult GetShiftsForQuickAbsence(GetShiftsForQuickAbsenceModel model)
        {
            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetShiftsForAbsence(
                base.ActorCompanyId, 
                model.EmployeeId, 
                model.ShiftIds, 
                model.IncludeLinkedShifts, 
                model.TimeScheduleScenarioHeadId.ToNullable()
            );
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            return Content(HttpStatusCode.OK, dtos);
        }

        [HttpPost]
        [Route("Absence/PerformPlanning")]
        public IHttpActionResult PerformAbsencePlanningAction(PerformAbsencePlanningActionModelV2 model)
        {
            List<TimeSchedulePlanningDayDTO> shifts = model.Shifts.ToTimeSchedulePlanningDayDTOs();
            return Content(HttpStatusCode.OK, tem.GenerateAndSaveAbsenceFromStaffing(model.EmployeeRequest, shifts, model.ScheduledAbsence, model.SkipXEMailOnShiftChanges, model.TimeScheduleScenarioHeadId.ToNullable()));        
        }

        [HttpPost]
        [Route("Absence/WorkRules")]
        public IHttpActionResult EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(EvaluateAbsenceRequestPlanningAgainstWorkRulesV2 model)
        {
            var shifts = model.Shifts.ToTimeSchedulePlanningDayDTOs();
            return Content(HttpStatusCode.OK, tem.EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(shifts, model.EmployeeId, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.Rules));
        }
        #endregion

        #region Employee
        [HttpGet]
        [Route("Employee/{dateFrom}/{dateTo}/{mandatoryEmployeeId:int}/{excludeCurrentUserEmployee:bool}/{timeScheduleScenarioHeadId:int}")]
        public IHttpActionResult GetEmployeesForAbsencePlanning(string dateFrom, string dateTo, int mandatoryEmployeeId, bool excludeCurrentUserEmployee, int timeScheduleScenarioHeadId)
        {
            List<int> employeeIds = null;
            bool useHidden = sm.GetBoolSetting(SettingMainType.Company, (int)CompanySettingType.TimeUseStaffing, base.UserId, base.ActorCompanyId, 0);
            List<EmployeeListDTO> employees = new List<EmployeeListDTO>();
            if (timeScheduleScenarioHeadId > 0)
            {
                int hiddenEmployeeId = em.GetHiddenEmployeeId(base.ActorCompanyId);

                var scenarioHead = tsm.GetTimeScheduleScenarioHead(
                    timeScheduleScenarioHeadId, 
                    base.ActorCompanyId, 
                    true, 
                    false
                ).ToDTO();

                if (scenarioHead == null)
                    return Content(HttpStatusCode.OK, employees);

                employeeIds = scenarioHead.Employees?.Select(x => x.EmployeeId).ToList() ?? new List<int>();

                if (!employeeIds.Contains(hiddenEmployeeId))
                    useHidden = false;
            }

            var tempEmployees = em.GetEmployeeList(
                base.ActorCompanyId, 
                base.RoleId, 
                base.UserId, 
                employeeIds, 
                null, 
                useHidden, 
                false, 
                false, 
                false, 
                false, 
                false, 
                false, 
                BuildDateTimeFromString(dateFrom, true), 
                BuildDateTimeFromString(dateTo, true), 
                true, 
                true, 
                true, 
                mandatoryEmployeeId, 
                excludeCurrentUserEmployee: excludeCurrentUserEmployee
            );

            var hiddenEmployee = tempEmployees.FirstOrDefault(x => x.Hidden);
            if (hiddenEmployee != null)
            {
                employees.Add(hiddenEmployee);
                tempEmployees.Remove(hiddenEmployee);
            }
            var noReplacementEmployee = tempEmployees.FirstOrDefault(x => x.EmployeeId == Constants.NO_REPLACEMENT_EMPLOYEEID);
            if (noReplacementEmployee != null)
            {
                employees.Add(noReplacementEmployee);
                tempEmployees.Remove(noReplacementEmployee);
            }

            employees.AddRange(tempEmployees);

            return Content(HttpStatusCode.OK, employees);
        }
        #endregion

        #region AbsenceRequestOLD

        //[HttpPost]
        //[Route("Absence/PerformPlanning")]
        //public IHttpActionResult PerformAbsencePlanningAction(PerformAbsencePlanningActionModel model)
        //{
        //    return Content(HttpStatusCode.OK, tem.GenerateAndSaveAbsenceFromStaffing(model.EmployeeRequest, model.Shifts, model.ScheduledAbsence, model.SkipXEMailOnShiftChanges, model.TimeScheduleScenarioHeadId.ToNullable()));
        //}

        //[HttpPost]
        //[Route("Absence/CheckShiftsIncludedInAbsenceRequestWarningMessage")]
        //public IHttpActionResult GetShiftsIsIncludedInAbsenceRequestWarningMessage(CheckShiftsIncludedInAbsenceRequestModel model)
        //{
        //    return Content(HttpStatusCode.OK, tsm.GetShiftsIsIncludedInAbsenceRequestWarningMessage(model.EmployeeId, model.Shifts));
        //}

        //[HttpPost]
        //[Route("Scenario/Absence/Remove")]
        //public IHttpActionResult RemoveAbsenceInScenario(RemoveAbsenceInScenarioModel model)
        //{
        //    return Content(HttpStatusCode.OK, tem.RemoveAbsenceInScenario(model.Items, model.TimeScheduleScenarioHeadId));
        //}

        //[HttpGet]
        //[Route("Absencerequest/{employeeId:int}/{loadPreliminary:bool}/{loadDefinitive:bool}")]
        //public IHttpActionResult GetAbsenceRequests(int employeeId, bool loadPreliminary, bool loadDefinitive)
        //{
        //    EmployeeRequestStatus status;
        //    if (loadPreliminary && loadDefinitive)
        //        status = EmployeeRequestStatus.PreliminaryAndDefinitive;
        //    else
        //        status = loadPreliminary ? EmployeeRequestStatus.Preliminary : EmployeeRequestStatus.Definitive;

        //    switch (status)
        //    {
        //        case EmployeeRequestStatus.Preliminary:
        //            return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).Where(e => e.Status != TermGroup_EmployeeRequestStatus.Definate));
        //        case EmployeeRequestStatus.Definitive:
        //            return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }).Where(e => e.Status == TermGroup_EmployeeRequestStatus.Definate));
        //        case EmployeeRequestStatus.PreliminaryAndDefinitive:
        //            return Content(HttpStatusCode.OK, tem.GetEmployeeRequestsDTOs(employeeId, new List<TermGroup_EmployeeRequestType>() { TermGroup_EmployeeRequestType.AbsenceRequest }));
        //        default:
        //            return Content(HttpStatusCode.OK, new List<EmployeeRequestDTO>());
        //    }
        //}

        //[HttpGet]
        //[Route("Absencerequest/{employeeRequestId:int}")]
        //public IHttpActionResult GetAbsenceRequest(int employeeRequestId)
        //{
        //    return Content(HttpStatusCode.OK, tem.LoadEmployeeRequest(employeeRequestId).ToDTO(true, true));

        //}

        //[HttpGet]
        //[Route("Absencerequest/Interval/{employeeId:int}/{start}/{stop}/{requestType:int}")]
        //public IHttpActionResult GetEmployeeRequestFromDateInterval(int employeeId, string start, string stop, TermGroup_EmployeeRequestType requestType)
        //{
        //    return Content(HttpStatusCode.OK, tem.LoadEmployeeRequest(employeeId, BuildDateTimeFromString(start, true).Value, BuildDateTimeFromString(stop, true).Value, requestType).ToDTO(true, true));

        //}

        //[HttpPost]
        //[Route("Absencerequest/PerformPlanning")]
        //public IHttpActionResult PerformAbsenceRequestPlanningAction(PerformAbsenceRequestPlanningActionModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return Error(HttpStatusCode.BadRequest, ModelState, null, null);
        //    else
        //        return Content(HttpStatusCode.OK, tem.PerformAbsenceRequestPlanningAction(model.EmployeeRequestId, model.Shifts, model.SkipXEMailOnShiftChanges, model.TimeScheduleScenarioHeadId.ToNullable()));
        //}

        //[HttpPost]
        //[Route("Absencerequest")]
        //public IHttpActionResult SaveEmployeeRequest(SaveAbsenceRequestModel model)
        //{
        //    model.Request.ActorCompanyId = base.ActorCompanyId;
        //    return Content(HttpStatusCode.OK, tem.SaveEmployeeRequest(model.Request.FromDTO(), model.EmployeeId, model.RequestType, model.SkipXEMailOnShiftChanges, model.IsForcedDefinitive));
        //}

        //[HttpPost]
        //[Route("Absencerequest/ValidateDeviationCausePolicy")]
        //public IHttpActionResult ValidateTimeDeviationCausePolicy(SaveAbsenceRequestModel model)
        //{
        //    model.Request.ActorCompanyId = base.ActorCompanyId;
        //    return Content(HttpStatusCode.OK, tdcm.ValidateTimeDeviationCausePolicy(base.ActorCompanyId, model.Request));
        //}

        //[HttpPost]
        //[Route("Absencerequest/History/PerformRestore/{employeeRequestId:int}/{setRequestAsPending:bool}")]
        //public IHttpActionResult PerformRestoreAbsenceRequestedShifts(int employeeRequestId, bool setRequestAsPending)
        //{
        //    return Content(HttpStatusCode.OK, tem.PerformRestoreAbsenceRequestedShifts(employeeRequestId, setRequestAsPending));
        //}

        //[HttpPost]
        //[Route("Absencerequest/WorkRules")]
        //public IHttpActionResult EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(EvaluateAbsenceRequestPlanningAgainstWorkRules model)
        //{
        //    return Content(HttpStatusCode.OK, tem.EvaluateAbsenceRequestPlannedShiftsAgainstWorkRules(model.Shifts, model.EmployeeId, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.Rules));
        //}

        //[HttpDelete]
        //[Route("Absencerequest/{employeeRequestId:int}")]
        //public IHttpActionResult DeleteEmployeeRequest(int employeeRequestId)
        //{
        //    return Content(HttpStatusCode.OK, tem.DeleteEmployeeRequest(employeeRequestId));
        //}

        //[HttpGet]
        //[Route("Absencerequest/History/{absenceRequestId:int}")]
        //public IHttpActionResult GetAbsenceRequestHistory(int absenceRequestId)
        //{
        //    return Content(HttpStatusCode.OK, tsm.GetAbsenceRequestHistory(base.ActorCompanyId, absenceRequestId));
        //}
        #endregion


    }
}