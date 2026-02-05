using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class SaveEvaluateAllWorkRulesByPassInputDTO : TimeEngineInputDTO
    {
        public EvaluateWorkRulesActionResult Result { get; set; }
        public int EmployeeId { get; set; }
        public SaveEvaluateAllWorkRulesByPassInputDTO(EvaluateWorkRulesActionResult result, int employeeId) : base()
        {
            this.Result = result;
            this.EmployeeId = employeeId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class EvaluateAllWorkRulesInputDTO : TimeEngineInputDTO
    {
        public List<TimeSchedulePlanningDayDTO> PlannedShifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public List<int> EmployeeIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }

        public EvaluateAllWorkRulesInputDTO(List<TimeSchedulePlanningDayDTO> plannedShifts, List<int> employeeIds, DateTime startDate, DateTime stopDate, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules = null, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null) : base()
        {
            this.PlannedShifts = plannedShifts;
            this.EmployeeIds = employeeIds;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.IsPersonalScheduleTemplate = isPersonalScheduleTemplate;
            this.Rules = rules;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.PlanningPeriodStartDate = planningPeriodStartDate;
            this.PlanningPeriodStopDate = planningPeriodStopDate;
        }
        public override int? GetIdCount()
        {
            return EmployeeIds?.Count();
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(StartDate, StopDate);
        }
    }
    public class EvaluatePlannedShiftsAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public List<TimeSchedulePlanningDayDTO> PlannedShifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public List<SoeScheduleWorkRules> RulesToSkip { get; set; }
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public List<DateTime> Dates { get; set; }
        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }

        public EvaluatePlannedShiftsAgainstWorkRulesInputDTO(List<TimeSchedulePlanningDayDTO> plannedShifts, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, List<DateTime> dates = null, List<SoeScheduleWorkRules> rules = null, List<SoeScheduleWorkRules> rulesToSkip = null, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null) : base()
        {
            this.PlannedShifts = plannedShifts;
            this.IsPersonalScheduleTemplate = isPersonalScheduleTemplate;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.Rules = rules;
            this.RulesToSkip = rulesToSkip;
            this.Dates = dates;
            this.PlanningPeriodStartDate = planningPeriodStartDate;
            this.PlanningPeriodStopDate = planningPeriodStopDate;
        }
        public override int? GetIdCount()
        {
            return PlannedShifts?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return PlannedShifts?.Select(i => i.ActualDate).Distinct().Count();
        }
    }
    public class EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public List<TimeSchedulePlanningDayDTO> PlannedShifts { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public int EmployeeId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesInputDTO(List<TimeSchedulePlanningDayDTO> plannedShifts, int employeeId, int? timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules = null) : base()
        {
            this.PlannedShifts = plannedShifts;
            this.EmployeeId = employeeId;
            this.Rules = rules;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return PlannedShifts?.Select(i => i.ActualDate).Distinct().Count();
        }
    }
    public class EvaluateDragShiftAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public int SourceShiftId { get; set; }
        public int TargetShiftId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DragShiftAction Action { get; set; }
        public int DestinationEmployeeId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public bool IsPersonalScheduleTemplate { get; set; }
        public bool KeepSourceShiftsTogether { get; set; }
        public bool KeepTargetShiftsTogether { get; set; }
        public bool WholeDayAbsence { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public bool FromQueue { get; set; }
        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
        public EvaluateDragShiftAgainstWorkRulesInputDTO(DragShiftAction action, int sourceShiftId, int targetShiftId, DateTime start, DateTime end, int destinationEmployeeId, bool isPersonalScheduleTemplate, bool wholeDayAbsence, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, List<SoeScheduleWorkRules> rules = null, bool keepSourceShiftsTogether = true, bool keepTargetShiftsTogether = true, bool fromQueue = false, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null) : base()
        {
            this.Action = action;
            this.SourceShiftId = sourceShiftId;
            this.TargetShiftId = targetShiftId;
            this.Start = start;
            this.End = end;
            this.DestinationEmployeeId = destinationEmployeeId;
            this.IsPersonalScheduleTemplate = isPersonalScheduleTemplate;
            this.Rules = rules;
            this.KeepSourceShiftsTogether = keepSourceShiftsTogether;
            this.KeepTargetShiftsTogether = keepTargetShiftsTogether;
            this.WholeDayAbsence = wholeDayAbsence;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.StandbyCycleWeek = standbyCycleWeek;
            this.StandbyCycleDateFrom = standbyCycleDateFrom;
            this.StandbyCycleDateTo = standbyCycleDateTo;
            this.IsStandByView = isStandByView;
            this.FromQueue = fromQueue;
            this.PlanningPeriodStartDate = planningPeriodStartDate;
            this.PlanningPeriodStopDate = planningPeriodStopDate;
        }
    }

    public class EvaluateScheduleSwapAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleSwapRequestId { get; set; }
        public EvaluateScheduleSwapAgainstWorkRulesInputDTO(int timeScheduleSwapRequestId) : base()
        {
            this.TimeScheduleSwapRequestId = timeScheduleSwapRequestId;
        }
    }
    public class EvaluateDragTemplateShiftAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public int SourceShiftId { get; set; }
        public int TargetShiftId { get; set; }
        public int SourceTemplateHeadId { get; set; }
        public int TargetTemplateHeadId { get; set; }
        public DateTime SourceDate { get; set; }
        public DateTime TargetStart { get; set; }
        public DateTime TargetEnd { get; set; }
        public int? TargetEmployeeId { get; set; }
        public int? TargetEmployeePostId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public bool KeepSourceShiftsTogether { get; set; }
        public bool KeepTargetShiftsTogether { get; set; }
        public EvaluateDragTemplateShiftAgainstWorkRulesInputDTO(DragShiftAction action, int sourceShiftId, int sourceTemplateHeadId, DateTime sourceDate, int targetShiftId, int targetTemplateHeadId, DateTime targetStart, DateTime targetEnd, int? destinationEmployeeId, int? destinationEmployeePostId, List<SoeScheduleWorkRules> rules = null, bool keepSourceShiftsTogether = true, bool keepTargetShiftsTogether = true) : base()
        {
            this.Action = action;
            this.SourceShiftId = sourceShiftId;
            this.TargetShiftId = targetShiftId;
            this.SourceTemplateHeadId = sourceTemplateHeadId;
            this.TargetTemplateHeadId = targetTemplateHeadId;
            this.SourceDate = sourceDate;
            this.TargetStart = targetStart;
            this.TargetEnd = targetEnd;
            this.TargetEmployeeId = destinationEmployeeId.HasValue && destinationEmployeeId.Value != 0 ? destinationEmployeeId : null;
            this.TargetEmployeePostId = destinationEmployeePostId.HasValue && destinationEmployeePostId.Value != 0 ? destinationEmployeePostId : null;
            this.Rules = rules;
            this.KeepSourceShiftsTogether = keepSourceShiftsTogether;
            this.KeepTargetShiftsTogether = keepTargetShiftsTogether;
        }
    }
    public class EvaluateDragShiftMultipelAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public List<int> SourceShiftIds { get; set; }
        public int OffsetDays { get; set; }
        public int DestinationEmployeeId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public bool IsPersonalScheduleTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
        public EvaluateDragShiftMultipelAgainstWorkRulesInputDTO(DragShiftAction action, List<int> sourceShiftIds, int offsetDays, int destinationEmployeeId, bool isPersonalScheduleTemplate, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, List<SoeScheduleWorkRules> rules = null, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null) : base()
        {
            this.Action = action;
            this.SourceShiftIds = sourceShiftIds;
            this.OffsetDays = offsetDays;
            this.DestinationEmployeeId = destinationEmployeeId;
            this.IsPersonalScheduleTemplate = isPersonalScheduleTemplate;
            this.Rules = rules;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.StandbyCycleWeek = standbyCycleWeek;
            this.StandbyCycleDateFrom = standbyCycleDateFrom;
            this.StandbyCycleDateTo = standbyCycleDateTo;
            this.IsStandByView = isStandByView;
            this.PlanningPeriodStartDate = planningPeriodStartDate;
            this.PlanningPeriodStopDate = planningPeriodStopDate;
        }
        public override int? GetIntervalCount()
        {
            return SourceShiftIds?.Count();
        }
    }
    public class EvaluateDragTemplateShiftMultipelAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public List<int> SourceShiftIds { get; set; }
        public int SourceTemplateHeadId { get; set; }
        public int TargetTemplateHeadId { get; set; }
        public int OffsetDays { get; set; }
        public int? TargetEmployeeId { get; set; }
        public int? TargetEmployeePostId { get; set; }
        public DateTime FirstSourceDate { get; set; }
        public DateTime FirstTargetDate { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public EvaluateDragTemplateShiftMultipelAgainstWorkRulesInputDTO(DragShiftAction action, List<int> sourceShiftIds, int sourceTemplateHeadId, DateTime firstSourceDate, int offsetDays, int? targetEmployeeId, int? targetEmployeePostId, int targetTemplateHeadId, DateTime firstTargetDate, List<SoeScheduleWorkRules> rules = null) : base()
        {
            this.Action = action;
            this.SourceShiftIds = sourceShiftIds;
            this.SourceTemplateHeadId = sourceTemplateHeadId;
            this.TargetTemplateHeadId = targetTemplateHeadId;
            this.OffsetDays = offsetDays;
            this.TargetEmployeeId = targetEmployeeId.HasValue && targetEmployeeId.Value != 0 ? targetEmployeeId : null;
            this.TargetEmployeePostId = targetEmployeePostId.HasValue && targetEmployeePostId.Value != 0 ? targetEmployeePostId : null;
            this.FirstSourceDate = firstSourceDate;
            this.FirstTargetDate = firstTargetDate;
            this.Rules = rules;
        }
    }
    public class EvaluateAssignTaskToEmployeeAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public DateTime DestinationDate { get; set; }
        public int DestinationEmployeeId { get; set; }
        public List<StaffingNeedsTaskDTO> TaskDTOs { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public EvaluateAssignTaskToEmployeeAgainstWorkRulesInputDTO(int destinationEmployeeId, DateTime destinationDate, List<StaffingNeedsTaskDTO> taskDTOs, List<SoeScheduleWorkRules> rules) : base()
        {
            this.DestinationEmployeeId = destinationEmployeeId;
            this.DestinationDate = destinationDate;
            this.TaskDTOs = taskDTOs;
            this.Rules = rules;
        }
    }
    public class EvaluateActivateScenarioAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public DateTime? PreliminaryDateFrom { get; set; }
        public EvaluateActivateScenarioAgainstWorkRulesInputDTO(int timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules, DateTime? preliminaryDateFrom) : base()
        {
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
            this.Rules = rules;
            this.PreliminaryDateFrom = preliminaryDateFrom;
        }
    }
    public class EvaluateScenarioToTemplateAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public List<SoeScheduleWorkRules> Rules { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public int numberOfMovedDays { get; set; }
        public EvaluateScenarioToTemplateAgainstWorkRulesInputDTO(int timeScheduleScenarioHeadId, List<SoeScheduleWorkRules> rules, List<TimeSchedulePlanningDayDTO> shifts, int numberOfMovedDays) : base()
        {
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
            this.Rules = rules;
            this.Shifts = shifts ?? new List<TimeSchedulePlanningDayDTO>();
            this.numberOfMovedDays = numberOfMovedDays;
        }
    }
    public class EvaluateDeviationsAgainstWorkRulesInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public int EmployeeId { get; set; }
        public EvaluateDeviationsAgainstWorkRulesInputDTO(int employeeId, DateTime date) : base()
        {
            this.EmployeeId = employeeId;
            this.Date = date;
        }
    }
    public class IsDayAttestedInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }

        public IsDayAttestedInputDTO(int employeeId, DateTime date)
        {
            this.EmployeeId = employeeId;
            this.Date = date;
        }
    }

    #endregion

    #region Output

    public class EvaluateAllWorkRulesOutputDTO : TimeEngineOutputDTO
    {
        public EvaluateAllWorkRulesActionResult EvaluateWorkRulesResult { get; set; }
        public EvaluateAllWorkRulesOutputDTO() : base()
        {
            this.EvaluateWorkRulesResult = new EvaluateAllWorkRulesActionResult();
        }
    }
    public class EvaluateScheduleWorkRulesOutputDTO : TimeEngineOutputDTO
    {
        public EvaluateWorkRulesActionResult EvaluateWorkRulesResult { get; set; }
        public EvaluateScheduleWorkRulesOutputDTO() : base()
        {
            this.EvaluateWorkRulesResult = new EvaluateWorkRulesActionResult();
        }
    }
    public class SaveEvaluateAllWorkRulesByPassOutputDTO : TimeEngineOutputDTO
    {
        public EvaluateWorkRulesActionResult EvaluateWorkRulesResult { get; set; }
        public SaveEvaluateAllWorkRulesByPassOutputDTO() : base()
        {
            this.EvaluateWorkRulesResult = new EvaluateWorkRulesActionResult();
        }
    }
    public class EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesOutputDTO : TimeEngineOutputDTO
    {
        public EvaluateWorkRulesActionResult EvaluateWorkRulesResult { get; set; }
        public EvaluateAbsenceRequestPlannedShiftsAgainstWorkRulesOutputDTO() : base()
        {
            this.EvaluateWorkRulesResult = new EvaluateWorkRulesActionResult();
        }
    }
    public class EvaluateScenarioAgainstWorkRulesOutputDTO : TimeEngineOutputDTO
    {
        public EvaluateWorkRulesActionResult EvaluateWorkRulesResult { get; set; }
        public EvaluateScenarioAgainstWorkRulesOutputDTO() : base()
        {
            this.EvaluateWorkRulesResult = new EvaluateWorkRulesActionResult();
        }
    }
    public class EvaluateDeviationsAgainstWorkRulesOutputDTO : TimeEngineOutputDTO
    {
        public EvaluateDeviationsAgainstWorkRules EvaluateDeviationsAgainstWorkRulesResult { get; set; }
        public EvaluateDeviationsAgainstWorkRulesOutputDTO() : base()
        {
            this.EvaluateDeviationsAgainstWorkRulesResult = new EvaluateDeviationsAgainstWorkRules();
        }
    }
    public class IsDayAttestedOutputDTO : TimeEngineOutputDTO
    {
        public bool IsDayAttestedResult { get; set; }
        public IsDayAttestedOutputDTO() : base()
        {
        }
    }

    #endregion
}
