using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/SchedulePlanning")]
    public class SchedulePlanningController : SoeApiController
    {
        #region Variables

        private readonly TimeEngineManager tem;
        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public SchedulePlanningController(TimeEngineManager tem, TimeScheduleManager tsm)
        {
            this.tem = tem;
            this.tsm = tsm;
        }

        #endregion

        #region Shift

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

            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetTimeScheduleShifts(base.ActorCompanyId, base.UserId, base.RoleId, employeeId, dateTime, dateTime, types, includeBreaks, includeGrossNetAndCost, link != "null" ? new Guid(link) : (Guid?)null, 0, loadQueue, loadDeviationCause, loadTasks, includePreliminary, timeScheduleScenarioHeadId != 0 ? timeScheduleScenarioHeadId : (int?)null, setSwapShiftInfo: true);
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            AddBreaksToShifts(shifts, dtos);

            return Content(HttpStatusCode.OK, dtos);
        }

        [HttpGet]
        [Route("LinkedShifts/{timeScheduleTemplateBlockId:int}")]
        public IHttpActionResult GetLinkedShifts(int timeScheduleTemplateBlockId)
        {
            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetLinkedTimeScheduleTemplateBlocks(null, base.ActorCompanyId, timeScheduleTemplateBlockId, true).ToTimeSchedulePlanningDayDTOs();
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            AddBreaksToShifts(shifts, dtos);

            return Content(HttpStatusCode.OK, dtos);
        }

        [HttpPost]
        [Route("Shift/Search")]
        public IHttpActionResult GetShifts(GetShiftsModel model)
        {
            List<TimeSchedulePlanningDayDTO> shifts = tsm.GetTimeSchedulePlanningShifts_ByProcedure(base.ActorCompanyId, base.UserId, model.EmployeeId, base.RoleId, model.DateFrom, model.DateTo, model.EmployeeIds, model.PlanningMode, model.DisplayMode, model.IncludeSecondaryCategories, model.IncludeBreaks, model.IncludeGrossNetAndCost, model.IncludePreliminary, model.IncludeEmploymentTaxAndSupplementChargeCost, model.IncludeShiftRequest, model.IncludeAbsenceRequest, model.CheckToIncludeDeliveryAdress, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, setSwapShiftInfo: true, includeHolidaySalary: model.IncludeHolidaySalary, includeOnDuty: true, includeLeisureCodes: model.IncludeLeisureCodes);
            List<ShiftDTO> dtos = shifts.ToShiftDTOs();
            AddBreaksToShifts(shifts, dtos);

            return Content(HttpStatusCode.OK, dtos);
        }

        private void AddBreaksToShifts(List<TimeSchedulePlanningDayDTO> shifts, List<ShiftDTO> dtos)
        {
            // Extract break information from TimeSchedulePlanningDayDTO and add them to breaks list as ShiftBreakDTOs

            shifts.GroupBy(s => s.TimeScheduleEmployeePeriodId).ToList().ForEach(g =>
            {
                List<TimeSchedulePlanningDayDTO> employeePeriodShifts = g.ToList();
                List<ShiftDTO> periodDtos = dtos.Where(d => d.TimeScheduleEmployeePeriodId == g.Key).ToList();

                foreach (TimeSchedulePlanningDayDTO shift in employeePeriodShifts)
                {
                    AddBreakToPeriodDto(shift, periodDtos, 1);
                    AddBreakToPeriodDto(shift, periodDtos, 2);
                    AddBreakToPeriodDto(shift, periodDtos, 3);
                    AddBreakToPeriodDto(shift, periodDtos, 4);
                }
            });
        }

        private void AddBreakToPeriodDto(TimeSchedulePlanningDayDTO shift, List<ShiftDTO> periodDtos, int breakNumber)
        {
            int breakId = 0;
            int timeCodeId = 0;
            DateTime startTime = DateTime.MinValue;
            int minutes = 0;
            Guid? link = null;
            bool isPreliminary = false;

            switch (breakNumber)
            {
                case 1:
                    breakId = shift.Break1Id;
                    timeCodeId = shift.Break1TimeCodeId;
                    startTime = shift.Break1StartTime;
                    minutes = shift.Break1Minutes;
                    link = shift.Break1Link;
                    isPreliminary = shift.Break1IsPreliminary;
                    break;
                case 2:
                    breakId = shift.Break2Id;
                    timeCodeId = shift.Break2TimeCodeId;
                    startTime = shift.Break2StartTime;
                    minutes = shift.Break2Minutes;
                    link = shift.Break2Link;
                    isPreliminary = shift.Break2IsPreliminary;
                    break;
                case 3:
                    breakId = shift.Break3Id;
                    timeCodeId = shift.Break3TimeCodeId;
                    startTime = shift.Break3StartTime;
                    minutes = shift.Break3Minutes;
                    link = shift.Break3Link;
                    isPreliminary = shift.Break3IsPreliminary;
                    break;
                case 4:
                    breakId = shift.Break4Id;
                    timeCodeId = shift.Break4TimeCodeId;
                    startTime = shift.Break4StartTime;
                    minutes = shift.Break4Minutes;
                    link = shift.Break4Link;
                    isPreliminary = shift.Break4IsPreliminary;
                    break;
                default:
                    return;
            }

            if (breakId != 0)
            {
                ShiftDTO periodDto = periodDtos.FirstOrDefault(p => startTime > p.StartTime && startTime <= p.StopTime);
                if (periodDto != null)
                {
                    if (!periodDto.Breaks.Any(b => b.BreakId == breakId))
                    {
                        periodDto.Breaks.Add(new ShiftBreakDTO()
                        {
                            BreakId = breakId,
                            TimeCodeId = timeCodeId,
                            StartTime = startTime,
                            BelongsToPreviousDay = shift.BelongsToPreviousDay,
                            BelongsToNextDay = shift.BelongsToNextDay,
                            Minutes = minutes,
                            Link = link,
                            IsPreliminary = isPreliminary,
                        });
                    }
                }
            }
        }

        [HttpPost]
        [Route("Shift")]
        public IHttpActionResult SaveShifts(SaveShiftsModelV2 model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                List<TimeSchedulePlanningDayDTO> shifts = model.Shifts.ToTimeSchedulePlanningDayDTOs();
                return Content(HttpStatusCode.OK, tem.SaveTimeScheduleShift(model.Source, shifts, model.UpdateBreaks, model.SkipXEMailOnChanges, model.AdjustTasks, model.MinutesMoved, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
            }
        }

        [HttpPost]
        [Route("Shift/DeleteShifts")]
        public IHttpActionResult DeleteShifts(DeleteShiftsModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteTimeScheduleShifts(model.ShiftIds, model.SkipXEMailOnChanges, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.IncludedOnDutyShiftIds));
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
        [Route("Shift/Split")]
        public IHttpActionResult SplitShift(SplitShiftModelV2 model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.SplitTimeScheduleShift(model.Shift.ToTimeSchedulePlanningDayDTO(), model.SplitTime, model.EmployeeId1, model.EmployeeId2, model.KeepShiftsTogether, model.IsPersonalScheduleTemplate, model.SkipXEMailOnChanges, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null));
        }

        #endregion

        #region Shift accounting

        [HttpGet]
        [Route("ShiftAccounting/{timeScheduleTemplateBlockIds}")]
        public IHttpActionResult GetShiftAccountingRows(string timeScheduleTemplateBlockIds)
        {
            List<int> timeScheduleTemplateBlockIdList = StringUtility.SplitNumericList(timeScheduleTemplateBlockIds, true);
            return Content(HttpStatusCode.OK, tsm.GetShiftAccountingRows(base.ActorCompanyId, timeScheduleTemplateBlockIdList));
        }

        #endregion

        #region Shift history

        [HttpGet]
        [Route("ShiftHistory/{timeScheduleTemplateBlockIds}")]
        public IHttpActionResult GetShiftHistory(string timeScheduleTemplateBlockIds)
        {
            List<int> timeScheduleTemplateBlockIdList = StringUtility.SplitNumericList(timeScheduleTemplateBlockIds, true);
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTemplateBlockHistoryChanges(base.ActorCompanyId, timeScheduleTemplateBlockIdList));
        }

        #endregion

        #region Shift request

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

        #region Work rules

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
        [Route("EvaluateWorkRule/DragMultiple")]
        public IHttpActionResult EvaluateDragShiftsAgainstWorkRules(EvaluateWorkRulesDragMultipleModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateDragShiftMultipelAgainstWorkRules(model.Action, model.SourceShiftIds, model.OffsetDays, model.EmployeeId, model.IsPersonalScheduleTemplate, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.StandbyCycleWeek, model.StandbyCycleDateFrom, model.StandbyCycleDateTo, model.IsStandByView, model.Rules, model.PlanningPeriodStartDate, model.PlanningPeriodStopDate));
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
        [Route("EvaluateWorkRule/Split")]
        public IHttpActionResult EvaluateSplitShiftAgainstWorkRules(EvaluateWorkRulesSplitModelV2 model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.EvaluateSplitShiftAgainstWorkRules(model.Shift.ToTimeSchedulePlanningDayDTO(), model.SplitTime, model.EmployeeId1, model.EmployeeId2, model.KeepShiftsTogether, model.IsPersonalScheduleTemplate, model.TimeScheduleScenarioHeadId.HasValue && model.TimeScheduleScenarioHeadId.Value != 0 ? model.TimeScheduleScenarioHeadId.Value : (int?)null, model.PlanningPeriodStartDate, model.PlanningPeriodStopDate));
        }

        #endregion        
    }
}