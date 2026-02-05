using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class GetTimeScheduleTemplateInputDTO : TimeEngineInputDTO
    {
        public int TemplateHeadId { get; set; }
        public bool LoadEmployeeSchedule { get; set; }
        public bool LoadAccounts { get; set; }
        public GetTimeScheduleTemplateInputDTO(int templateHeadId, bool loadEmployeeSchedule, bool loadAccounts) : base()
        {
            this.TemplateHeadId = templateHeadId;
            this.LoadEmployeeSchedule = loadEmployeeSchedule;
            this.LoadAccounts = loadAccounts;
        }
    }
    public class GetSequentialScheduleInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public int EmployeeId { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public bool IncludeStandBy { get; set; }
        public GetSequentialScheduleInputDTO(DateTime date, int timeScheduleTemplatePeriodId, int employeeId, bool includeStandby = false)
        {
            this.Date = date;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            this.EmployeeId = employeeId;
            this.IncludeStandBy = includeStandby;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }
    public class SaveTimeScheduleTemplateStaffingInputDTO : TimeEngineInputDTO
    {
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int NoOfDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? FirstMondayOfCycle { get; set; }
        public DateTime CurrentDate { get; set; }
        public bool SimpleSchedule { get; set; }
        public int EmployeeId { get; set; }
        public int? EmployeePostId { get; set; }
        public int? CopyFromTimeScheduleTemplateHeadId { get; set; }
        public bool UseAccountingFromSourceSchedule { get; set; }
        public bool IsPersonalTemplate
        {
            get
            {
                if (this.EmployeePostId.HasValue && this.EmployeePostId.Value != 0)
                    return false;
                else
                    return true;
            }
        }
        public bool StartOnFirstDayOfWeek { get; set; }
        public bool FlexForceSchedule
        {
            get
            {
                return false;
            }
        }
        public bool Locked { get; set; }
        public SaveTimeScheduleTemplateStaffingInputDTO(List<TimeSchedulePlanningDayDTO> shifts, int timeScheduleTemplateHeadId, int noOfDays, DateTime startDate, DateTime? stopDate, DateTime? firstMondayOfCycle, DateTime currentDate, bool simpleSchedule, bool startOnFirstDayOfWeek, bool locked, int employeeId, int? employeePostId = null, int? copyFromTimeScheduleTemplateHeadId = null, bool useAccountingFromSourceSchedule = true) : base()
        {
            this.Shifts = shifts;
            this.TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId;
            this.NoOfDays = noOfDays;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.FirstMondayOfCycle = firstMondayOfCycle;
            this.CurrentDate = currentDate;
            this.SimpleSchedule = simpleSchedule;
            this.StartOnFirstDayOfWeek = startOnFirstDayOfWeek;
            this.Locked = locked;
            this.EmployeeId = employeeId;
            this.EmployeePostId = employeePostId;
            this.CopyFromTimeScheduleTemplateHeadId = copyFromTimeScheduleTemplateHeadId;
            this.UseAccountingFromSourceSchedule = useAccountingFromSourceSchedule;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.StartDate, this.StopDate);
        }
        public void GetNameAndDescription(Employee employee, out string name, out string description)
        {
            name = "";
            description = "";

            if (employee != null)
                name = String.Format("{0} {1}", employee.NameOrNumber, StartDate.ToShortDateString());
        }
    }
    public class SaveTimeScheduleTemplateInputDTO : TimeEngineInputDTO
    {
        public TimeScheduleTemplateHead TemplateHead { get; set; }
        public List<TimeScheduleTemplateBlockDTO> TemplateBlockItems { get; set; }
        public SaveTimeScheduleTemplateInputDTO(TimeScheduleTemplateHead templateHead, List<TimeScheduleTemplateBlockDTO> templateBlockItems) : base()
        {
            this.TemplateHead = templateHead;
            this.TemplateBlockItems = templateBlockItems;
        }
        public override int? GetIdCount()
        {
            return this.TemplateBlockItems?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return this.TemplateBlockItems?.Select(i => i.Date).Distinct().Count();
        }
    }
    public class UpdateTimeScheduleTemplateStaffingInputDTO : TimeEngineInputDTO
    {
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public int EmployeeId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int DayNumberFrom { get; set; }
        public int DayNumberTo { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<DateTime> ActivateDates { get; set; }
        public int? ActivateDayNumber { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public UpdateTimeScheduleTemplateStaffingInputDTO(List<TimeSchedulePlanningDayDTO> shifts, int employeeId, int timeScheduleTemplateHeadId, int dayNumberFrom, int dayNumberTo, DateTime currentDate, List<DateTime> activateDates, int? activateDayNumber, bool skipXEMailOnChanges = false) : base() 
        {
            this.Shifts = shifts;
            this.EmployeeId = employeeId;
            this.TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId;
            this.DayNumberFrom = dayNumberFrom;
            this.DayNumberTo = dayNumberTo;
            this.CurrentDate = currentDate;
            this.ActivateDates = activateDates;
            this.ActivateDayNumber = activateDayNumber;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
        }
        public override int? GetIdCount()
        {
            return this.Shifts?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return this.DayNumberTo - this.DayNumberFrom;
        }
    }
    public class SaveShiftPrelDefInputDTO : TimeEngineInputDTO
    {
        public List<EmployeeDatesDTO> EmployeeDates { get; set; }
        public bool IncludeScheduleShifts { get; set; }
        public bool IncludeStandbyShifts { get; set; }
        public SaveShiftPrelDefInputDTO(List<EmployeeDatesDTO> employeeDates, bool includeScheduleShifts, bool includeStandbyShifts) : base() 
        {
            this.EmployeeDates = employeeDates;
            this.IncludeScheduleShifts = includeScheduleShifts;
            this.IncludeStandbyShifts = includeStandbyShifts;
        }
        public override int? GetIdCount()
        {
            return this.EmployeeDates?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return this.EmployeeDates?.Count();
        }
    }
    public class CopyScheduleInputDTO : TimeEngineInputDTO
    {
        public int SourceEmployeeId { get; set; }
        public int TargetEmployeeId { get; set; }
        public DateTime? SourceDateStop { get; set; }
        public DateTime TargetDateStart { get; set; }
        public DateTime? TargetDateStop { get; set; }
        public bool UseAccountingFromSourceSchedule { get; set; }
        public bool CreateTimeBlocksAndTransactionsAsync { get; set; }
        public CopyScheduleInputDTO(int sourceEmployeeId, int targetEmployeeId, DateTime? sourceDateStop, DateTime targetDateStart, DateTime? targetDateStop, bool useAccountingFromSourceSchedule, bool createTimeBlocksAndTransactionsAsync)
        {
            this.SourceEmployeeId = sourceEmployeeId;
            this.TargetEmployeeId = targetEmployeeId;
            this.SourceDateStop = sourceDateStop;
            this.TargetDateStart = targetDateStart;
            this.TargetDateStop = targetDateStop;
            this.UseAccountingFromSourceSchedule = useAccountingFromSourceSchedule;
            this.CreateTimeBlocksAndTransactionsAsync = createTimeBlocksAndTransactionsAsync;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.TargetDateStart, this.TargetDateStop);
        }
        public bool IsDateValidForShortenSchedule(DateTime date)
        {
            //Date must be after SourceDateStop
            return this.SourceDateStop.HasValue && this.SourceDateStop.Value < date;
        }
        public bool IsDateValidForCopySchedule(DateTime date)
        {
            //Date must be before TargetDateStop (or null)
            return !this.TargetDateStop.HasValue || this.TargetDateStop.Value >= date;
        }
    }
    public class DeleteTimeBlocksAndTransactions : TimeEngineInputDTO
    {
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }
        public List<int> EmployeeIds { get { return this.Items.GetEmployeeIds(); } }
        public string DateInterval { get { return this.Items.GetDateInterval(); } }
        public DeleteTimeBlocksAndTransactions() : base() { }
        public override int? GetIdCount()
        {
            return this.Items.GetNrOfEmployees();
        }
        public override int? GetIntervalCount()
        {
            return this.Items.GetNrOfDates();
        }
    }
    public class DeleteTimeScheduleTemplateInputDTO : TimeEngineInputDTO
    {
        public int TemplateHeadId { get; set; }
        public DeleteTimeScheduleTemplateInputDTO(int templateHeadId) : base() 
        {
            this.TemplateHeadId = templateHeadId;
        }
    }
    public class RemoveEmployeeFromTimeScheduleTemplateInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleTemplateHeadId { get; set; }
        public RemoveEmployeeFromTimeScheduleTemplateInputDTO(int timeScheduleTemplateHeadId) : base() 
        {
            this.TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId;
        }
    }
    public class RemoveAbsenceInScenarioInputDTO : TimeEngineInputDTO
    {
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }
        public int TimeScheduleScenarioHeadId { get; set; }
        public List<int> EmployeeIds { get { return this.Items.GetEmployeeIds(); } }
        public string DateInterval { get { return this.Items.GetDateInterval(); } }
        public RemoveAbsenceInScenarioInputDTO(List<AttestEmployeeDaySmallDTO> inputItems, int timeScheduleScenarioHeadId) : base() 
        {
            this.Items = inputItems;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
        }
        public override int? GetIdCount()
        {
            return this.Items.GetNrOfEmployees();
        }
        public override int? GetIntervalCount()
        {
            return this.Items.GetNrOfDates();
        }
    }
    public class SetTemplateBreakInfoInputDTO : TimeEngineInputDTO
    {
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.StartDate, this.StopDate);
        }
    }
    public class SaveEmployeeSchedulePlacementInputDTO : TimeEngineInputDTO
    {
        public ActivateScheduleControlDTO Control { get; set; }
        public List<SaveEmployeeSchedulePlacementItem> Items { get; set; }
        public bool UseBulk { get; set; }
        public SaveEmployeeSchedulePlacementInputDTO(ActivateScheduleControlDTO control, List<SaveEmployeeSchedulePlacementItem> items, bool useBulk = false) : base()
        {
            this.Control = control ?? new ActivateScheduleControlDTO();
            this.Items = items;
            this.UseBulk = useBulk;
        }
        public override int? GetIdCount()
        {
            return Items?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return Items?.Sum(i => i.TotalDays);
        }
    }
    public class SaveEmployeeSchedulePlacementFromJobInputDTO : TimeEngineInputDTO
    {
        public int RecalculateTimeHeadId { get; set; }
        public SaveEmployeeSchedulePlacementFromJobInputDTO(int recalculateTimeHeadId) : base()
        {
            this.RecalculateTimeHeadId = recalculateTimeHeadId;
        }
    }
    public class SaveEmployeeSchedulePlacementStaffingInputDTO : TimeEngineInputDTO
    {
        public ActivateScheduleControlDTO Control { get; set; }
        public SaveEmployeeSchedulePlacementItem Item { get; set; }
        public SaveEmployeeSchedulePlacementStaffingInputDTO(ActivateScheduleControlDTO control, SaveEmployeeSchedulePlacementItem item)
        {
            this.Control = control;
            this.Item = item;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return Item?.TotalDays;
        }
    }
    public class DeleteEmployeeSchedulePlacementInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int EmployeeScheduleId { get; set; }
        public ActivateScheduleControlDTO Control { get; set; }
        public DeleteEmployeeSchedulePlacementInputDTO(ActivateScheduleGridDTO item, ActivateScheduleControlDTO control) : base()
        {
            this.EmployeeId = item.EmployeeId;
            this.EmployeeScheduleId = item.EmployeeScheduleId;
            this.Control = control;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class ControlEmployeeSchedulePlacementInputDTO : TimeEngineInputDTO
    {
        public List<ActivateScheduleGridDTO> Items { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool IsDelete { get; set; }
        public ControlEmployeeSchedulePlacementInputDTO(List<ActivateScheduleGridDTO> items, DateTime? startDate, DateTime? stopDate, bool isDelete)
        {
            this.Items = items;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.IsDelete = isDelete;
        }
    }
    public class GetEmployeeRequestsInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int? EmployeeRequestId { get; set; }
        public List<TermGroup_EmployeeRequestType> RequestTypes { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool IgnoreState { get; set; }
        public GetEmployeeRequestsInputDTO(int employeeId, int? employeeRequestId, List<TermGroup_EmployeeRequestType> requestTypes, DateTime? dateFrom = null, DateTime? dateTo = null, bool ignoreState = false) : base()
        {
            this.EmployeeRequestId = employeeRequestId;
            this.EmployeeId = employeeId;
            this.RequestTypes = requestTypes;
            this.DateFrom = dateFrom;
            this.DateTo = dateTo;
            this.IgnoreState = ignoreState;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.DateFrom, this.DateTo);
        }
    }
    public class LoadEmployeeRequestInputDTO : TimeEngineInputDTO
    {
        public int? EmployeeRequestId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public TermGroup_EmployeeRequestType RequestType { get; set; }
        public LoadEmployeeRequestInputDTO(int? employeeRequestId, int employeeId, DateTime? start, DateTime? stop, TermGroup_EmployeeRequestType type = TermGroup_EmployeeRequestType.Undefined) : base()
        {
            this.EmployeeRequestId = employeeRequestId;
            this.Start = start;
            this.Stop = stop;
            this.RequestType = type;
            this.EmployeeId = employeeId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.Start, this.Stop);
        }
    }
    public class SaveEmployeeRequestInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public EmployeeRequest EmployeeRequest { get; set; }
        public TermGroup_EmployeeRequestType RequestType { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public bool IsForcedDefinitive { get; set; }
        public SaveEmployeeRequestInputDTO(EmployeeRequest employeeRequest, int employeeId, TermGroup_EmployeeRequestType requestType, bool skipXEMailOnChanges, bool isForcedDefinitive) : base()
        {
            this.EmployeeRequest = employeeRequest;
            this.EmployeeId = employeeId;
            this.RequestType = requestType;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.IsForcedDefinitive = isForcedDefinitive;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.EmployeeRequest?.Start, this.EmployeeRequest?.Stop);
        }
    }
    public class SaveOrDeleteEmployeeRequestInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public List<EmployeeRequestDTO> DeletedEmployeeRequests { get; set; }
        public List<EmployeeRequestDTO> NewOrEditedEmployeeRequests { get; set; }
        public SaveOrDeleteEmployeeRequestInputDTO(int employeeId, List<EmployeeRequestDTO> deletedEmployeeRequests, List<EmployeeRequestDTO> editedOrNewRequests)
        {
            this.EmployeeId = employeeId;
            this.DeletedEmployeeRequests = deletedEmployeeRequests;
            this.NewOrEditedEmployeeRequests = editedOrNewRequests;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class DeleteEmployeeRequestInputDTO : TimeEngineInputDTO
    {
        public int EmployeeRequestId { get; set; }
        public DeleteEmployeeRequestInputDTO(int employeeRequestId) : base()
        {
            this.EmployeeRequestId = employeeRequestId;
        }
    }
    public class GetAvailableEmployeesInputDTO : TimeEngineInputDTO
    {
        public List<int> TimeScheduleTemplateBlockIds { get; set; }
        public List<int> EmployeeIds { get; set; }
        public bool FilterOnShiftType { get; set; }
        public bool FilterOnAvailability { get; set; }
        public bool FilterOnSkills { get; set; }
        public bool FilterOnWorkRules { get; set; }
        public int? FilterOnMessageGroupId { get; set; }
        public bool UseExistingScheduleBlocks { get; set; }
        public List<TimeScheduleTemplateBlockDTO> TimeScheduleTemplateBlockDTOs { get; set; }
        public bool GetHidden { get; set; }
        public bool GetVacant { get; set; }
        public GetAvailableEmployeesInputDTO(List<int> timeScheduleTemplateBlockIds, List<int> employeeIds, bool filterOnShiftType, bool filterOnAvailability, bool filterOnSkills, bool filterOnWorkRules, int? filterOnMessageGroupId, bool useExistingScheduleBlocks, List<TimeScheduleTemplateBlockDTO> timeScheduleTemplateBlockDTOs, bool getHidden, bool getVacant) : base()
        {
            this.TimeScheduleTemplateBlockIds = timeScheduleTemplateBlockIds;
            this.EmployeeIds = employeeIds;
            this.FilterOnShiftType = filterOnShiftType;
            this.FilterOnAvailability = filterOnAvailability;
            this.FilterOnSkills = filterOnSkills;
            this.FilterOnWorkRules = filterOnWorkRules;
            this.FilterOnMessageGroupId = filterOnMessageGroupId;
            this.UseExistingScheduleBlocks = useExistingScheduleBlocks;
            this.TimeScheduleTemplateBlockDTOs = timeScheduleTemplateBlockDTOs;
            this.GetHidden = getHidden;
            this.GetVacant = getVacant;
        }
        public override int? GetIdCount()
        {
            return EmployeeIds?.Count();
        }
    }
    public class GetAvailableTimeInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public GetAvailableTimeInputDTO(int employeeId, DateTime startTime, DateTime stopTime)
        {
            this.EmployeeId = employeeId;
            this.StartTime = startTime;
            this.StopTime = stopTime;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.StartTime, this.StopTime);
        }
    }
    public class InitiateScheduleSwapInputDTO : TimeEngineInputDTO
    {
        public int InitiatorEmployeeId { get; set; }
        public DateTime InitiatorShiftDate { get; set; }
        public List<int> InitiatorShiftIds { get; set; }
        public int SwapWithEmployeeId { get; set; }
        public DateTime SwapShiftDate { get; set; }
        public List<int> SwapWithShiftIds { get; set; }
        public string Comment { get; set; }
        public InitiateScheduleSwapInputDTO(int initiatorEmployeeId, DateTime initiatorShiftDate, List<int> initiatorShiftIds, int swapWithEmployeeId, DateTime swapShiftDate, List<int> swapWithShiftIds, string comment)
        {
            this.InitiatorEmployeeId = initiatorEmployeeId;
            this.InitiatorShiftDate = initiatorShiftDate;
            this.InitiatorShiftIds = initiatorShiftIds;
            this.SwapWithEmployeeId = swapWithEmployeeId;
            this.SwapShiftDate = swapShiftDate;
            this.SwapWithShiftIds = swapWithShiftIds;
            this.Comment = comment;
        }
    }

    public class ApproveScheduleSwapInputDTO : TimeEngineInputDTO
    {
        public int UserId { get; set; }
        public int TimeScheduleSwapRequestId { get; set; }
        public bool Approved { get; set; }
        public string Comment { get; set; }
        public ApproveScheduleSwapInputDTO(int userId, int timeScheduleSwapRequestId, bool approved, string comment)
        {
            this.UserId = userId;
            this.TimeScheduleSwapRequestId = timeScheduleSwapRequestId;
            this.Approved = approved;
            this.Comment = comment;
        }
    }

    public class SaveTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public string Source { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public bool UpdateBreaks { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public bool AdjustTasks { get; set; }
        public int MinutesMoved { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public SaveTimeScheduleShiftInputDTO(string source, List<TimeSchedulePlanningDayDTO> shifts, bool updateBreaks, bool skipXEMailOnChanges, bool adjustTasks, int minutesMoved, int? timeScheduleScenarioHeadId)
        {
            this.Source = source;
            this.Shifts = shifts;
            this.UpdateBreaks = updateBreaks;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.AdjustTasks = adjustTasks;
            this.MinutesMoved = minutesMoved;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
        }
    }
    public class DeleteTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public List<int> TimeScheduleTemplateBlockIds { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public List<int> IncludedOnDutyShiftIds { get; set; }
        public DeleteTimeScheduleShiftInputDTO(List<int> timeScheduleTemplateBlockIds, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, List<int> includedOnDutyShiftIds)
        {
            this.TimeScheduleTemplateBlockIds = timeScheduleTemplateBlockIds;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.IncludedOnDutyShiftIds = includedOnDutyShiftIds;
        }
        public override int? GetIntervalCount()
        {
            return TimeScheduleTemplateBlockIds?.Count();
        }
    }
    public class HandleTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public HandleShiftAction Action { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int EmployeeId { get; set; }
        public int SwapTimeScheduleTemplateBlockId { get; set; }
        public int RoleId { get; set; }
        public bool PreventAutoPermissions { get; set; }
        public bool KeepShiftsTogether { get; set; }
        public HandleTimeScheduleShiftInputDTO(HandleShiftAction action, int timeScheduleTemplateBlockId, int timeDeviationCauseId, int employeeId, int swapTimeScheduleTemplateBlockId, int roleId, bool preventAutoPermissions)
        {
            this.Action = action;
            this.TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId;
            this.TimeDeviationCauseId = timeDeviationCauseId;
            this.EmployeeId = employeeId;
            this.SwapTimeScheduleTemplateBlockId = swapTimeScheduleTemplateBlockId;
            this.RoleId = roleId;
            this.PreventAutoPermissions = preventAutoPermissions;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class SplitTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public TimeSchedulePlanningDayDTO Shift { get; set; }
        public DateTime SplitTime { get; set; }
        public int EmployeeId1 { get; set; }
        public int EmployeeId2 { get; set; }
        public bool KeepShiftsTogether { get; set; }
        public bool IsPersonalScheduleTemplate { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public DateTime? PlanningPeriodStartDate { get; set; }
        public DateTime? PlanningPeriodStopDate { get; set; }
        public SplitTimeScheduleShiftInputDTO(TimeSchedulePlanningDayDTO shift, DateTime splitTime, int employeeId1, int employeeId2, bool keepShiftsTogether, bool isPersonalScheduleTemplate, bool skipXEMailOnChanges, int? timeScheduleScenarioHeadId, DateTime? planningPeriodStartDate = null, DateTime? planningPeriodStopDate = null)
        {
            this.Shift = shift;
            this.SplitTime = splitTime;
            this.EmployeeId1 = employeeId1;
            this.EmployeeId2 = employeeId2;
            this.KeepShiftsTogether = keepShiftsTogether;
            this.IsPersonalScheduleTemplate = isPersonalScheduleTemplate;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.PlanningPeriodStartDate = planningPeriodStartDate;
            this.PlanningPeriodStopDate = planningPeriodStopDate;
        }
    }
    public class SplitTemplateTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public TimeSchedulePlanningDayDTO SourceShift { get; set; }
        public int SourceTemplateHeadId { get; set; }
        public DateTime SplitTime { get; set; }
        public int? EmployeeId1 { get; set; }
        public int? EmployeeId2 { get; set; }
        public int? EmployeePostId1 { get; set; }
        public int? EmployeePostId2 { get; set; }
        public int TemplateHeadId1 { get; set; }
        public int TemplateHeadId2 { get; set; }
        public bool KeepShiftsTogether { get; set; }
        public SplitTemplateTimeScheduleShiftInputDTO(TimeSchedulePlanningDayDTO sourceShift, int sourceTemplateHeadId, DateTime splitTime, int? employeeId1, int? employeePostId1, int templateHeadId1, int? employeeId2, int? employeePostId2, int templateHeadId2, bool keepShiftsTogether)
        {
            this.SourceShift = sourceShift;
            this.SourceTemplateHeadId = sourceTemplateHeadId;
            this.SplitTime = splitTime;
            this.EmployeeId1 = employeeId1;
            this.EmployeeId2 = employeeId2;
            this.EmployeePostId1 = employeePostId1;
            this.EmployeePostId2 = employeePostId2;
            this.KeepShiftsTogether = keepShiftsTogether;
            this.TemplateHeadId1 = templateHeadId1;
            this.TemplateHeadId2 = templateHeadId2;
        }
    }
    public class DragTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public int EmployeeId { get; set; }
        public bool KeepSourceShiftsTogether { get; set; }
        public bool KeepTargetShiftsTogether { get; set; }
        public bool UpdateLinkOnTarget { get; set; }
        public Guid? TargetLink { get; set; }
        public int SourceShiftId { get; set; }
        public int TargetShiftId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int TimeDeviationCauseId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? MessageId { get; set; }
        public bool WholeDayAbsence { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public bool CopyTaskWithShift { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public bool IncludeOnDutyShifts { get; set; }
        public List<int> IncludedOnDutyShiftIds { get; set; }
        public DragTimeScheduleShiftInputDTO(DragShiftAction action, int sourceShiftId, int targetShiftId, DateTime start, DateTime end, int employeeId, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, Guid? targetLink, bool updateLinkOnTarget, int timeDeviationCauseId, int? employeeChildId, bool wholeDayAbsence, int? messageId, bool skipXEMailOnChanges, bool copyTaskWithShift, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, bool includeOnDutyShifts, List<int> includedOnDutyShiftIds) : base() 
        {
            this.Action = action;
            this.SourceShiftId = sourceShiftId;
            this.TargetShiftId = targetShiftId;
            this.Start = start;
            this.End = end;
            this.EmployeeId = employeeId;
            this.KeepSourceShiftsTogether = keepSourceShiftsTogether;
            this.KeepTargetShiftsTogether = keepTargetShiftsTogether;
            this.TargetLink = targetLink;
            this.UpdateLinkOnTarget = updateLinkOnTarget;
            this.TimeDeviationCauseId = timeDeviationCauseId;
            this.EmployeeChildId = employeeChildId;
            this.MessageId = messageId;
            this.WholeDayAbsence = wholeDayAbsence;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.CopyTaskWithShift = copyTaskWithShift;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.StandbyCycleWeek = standbyCycleWeek;
            this.StandbyCycleDateFrom = standbyCycleDateFrom;
            this.StandbyCycleDateTo = standbyCycleDateTo;
            this.IsStandByView = isStandByView;
            this.IncludeOnDutyShifts = includeOnDutyShifts;
            this.IncludedOnDutyShiftIds = includedOnDutyShiftIds;
        }
    }
    public class DragTemplateTimeScheduleShiftInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public int? TargetEmployeeId { get; set; }
        public int? TargetEmployeePostId { get; set; }
        public bool KeepSourceShiftsTogether { get; set; }
        public bool KeepTargetShiftsTogether { get; set; }
        public bool UpdateLinkOnTarget { get; set; }
        public Guid? TargetLink { get; set; }
        public int SourceShiftId { get; set; }
        public int TargetShiftId { get; set; }
        public int SourceTemplateHeadId { get; set; }
        public int TargetTemplateHeadId { get; set; }
        public DateTime SourceDate { get; set; }
        public DateTime TargetStart { get; set; }
        public DateTime TargetEnd { get; set; }
        public bool CopyTaskWithShift { get; set; }
        public DragTemplateTimeScheduleShiftInputDTO(DragShiftAction action, int sourceShiftId, int sourceTemplateHeadId, DateTime sourceDate, int targetShiftId, int targetTemplateHeadId, DateTime targetStart, DateTime targetEnd, int? targetEmployeeId, int? targetEmployeePostId, bool keepSourceShiftsTogether, bool keepTargetShiftsTogether, Guid? targetLink, bool updateLinkOnTarget, bool copyTaskWithShift)
        {
            this.Action = action;
            this.SourceShiftId = sourceShiftId;
            this.TargetShiftId = targetShiftId;
            this.SourceTemplateHeadId = sourceTemplateHeadId;
            this.TargetTemplateHeadId = targetTemplateHeadId;
            this.SourceDate = sourceDate;
            this.TargetStart = targetStart;
            this.TargetEnd = targetEnd;
            this.TargetEmployeeId = targetEmployeeId.HasValue && targetEmployeeId.Value != 0 ? targetEmployeeId : null;
            this.TargetEmployeePostId = targetEmployeePostId.HasValue && targetEmployeePostId.Value != 0 ? targetEmployeePostId : null;
            this.KeepSourceShiftsTogether = keepSourceShiftsTogether;
            this.KeepTargetShiftsTogether = keepTargetShiftsTogether;
            this.TargetLink = targetLink;
            this.UpdateLinkOnTarget = updateLinkOnTarget;
            this.CopyTaskWithShift = copyTaskWithShift;
        }
    }
    public class DragTimeScheduleShiftMultipelInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public bool LinkWithExistingShiftsIfPossible { get; set; }
        public List<int> SourceShiftIds { get; set; }
        public int OffsetDays { get; set; }
        public int DestinationEmployeeId { get; set; }
        public bool SkipXEMailOnChanges { get; set; }
        public bool CopyTaskWithShift { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public int? StandbyCycleWeek { get; set; }
        public DateTime? StandbyCycleDateFrom { get; set; }
        public DateTime? StandbyCycleDateTo { get; set; }
        public bool IsStandByView { get; set; }
        public bool IncludeOnDutyShifts { get; set; }
        public List<int> IncludedOnDutyShiftIds { get; set; }
        public DragTimeScheduleShiftMultipelInputDTO(DragShiftAction action, List<int> sourceShiftIds, int offsetDays, int destinationEmployeeId, bool linkWithExistingShiftsIfPossible, bool skipXEMailOnChanges, bool copyTaskWithShift, int? timeScheduleScenarioHeadId, int? standbyCycleWeek, DateTime? standbyCycleDateFrom, DateTime? standbyCycleDateTo, bool isStandByView, bool includeOnDutyShifts, List<int> includedOnDutyShiftIds) 
        {
            this.Action = action;
            this.SourceShiftIds = sourceShiftIds;
            this.OffsetDays = offsetDays;
            this.DestinationEmployeeId = destinationEmployeeId;
            this.LinkWithExistingShiftsIfPossible = linkWithExistingShiftsIfPossible;
            this.SkipXEMailOnChanges = skipXEMailOnChanges;
            this.CopyTaskWithShift = copyTaskWithShift;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId.ToNullable();
            this.StandbyCycleWeek = standbyCycleWeek;
            this.StandbyCycleDateFrom = standbyCycleDateFrom;
            this.StandbyCycleDateTo = standbyCycleDateTo;
            this.IsStandByView = isStandByView;
            this.IncludeOnDutyShifts = includeOnDutyShifts;
            this.IncludedOnDutyShiftIds = includedOnDutyShiftIds;
        }
        public override int? GetIntervalCount()
        {
            return SourceShiftIds?.Count();
        }
    }
    public class DragTemplateTimeScheduleShiftMultipelInputDTO : TimeEngineInputDTO
    {
        public DragShiftAction Action { get; set; }
        public int SourceTemplateHeadId { get; set; }
        public int TargetTemplateHeadId { get; set; }
        public bool LinkWithExistingShiftsIfPossible { get; set; }
        public List<int> SourceShiftIds { get; set; }
        public DateTime FirstSourceDate { get; set; }
        public int OffsetDays { get; set; }
        public DateTime FirstTargetDate { get; set; }
        public int? TargetEmployeeId { get; set; }
        public int? TargetEmployeePostId { get; set; }
        public bool CopyTaskWithShift { get; set; }
        public DragTemplateTimeScheduleShiftMultipelInputDTO(DragShiftAction action, List<int> sourceShiftIds, int sourceTemplateHeadId, DateTime firstSourceDate, int offsetDays, DateTime firstTargetDate, int? targetEmployeeId, int? targetEmployeePostId, int targetTemplateHeadId, bool linkWithExistingShiftsIfPossible, bool copyTaskWithShift) 
        {
            this.Action = action;
            this.FirstTargetDate = firstTargetDate;
            this.SourceShiftIds = sourceShiftIds;
            this.SourceTemplateHeadId = sourceTemplateHeadId;
            this.TargetTemplateHeadId = targetTemplateHeadId;
            this.FirstSourceDate = firstSourceDate;
            this.OffsetDays = offsetDays;
            this.TargetEmployeeId = targetEmployeeId.HasValue && targetEmployeeId.Value != 0 ? targetEmployeeId : null;
            this.TargetEmployeePostId = targetEmployeePostId.HasValue && targetEmployeePostId.Value != 0 ? targetEmployeePostId : null;
            this.LinkWithExistingShiftsIfPossible = linkWithExistingShiftsIfPossible;
            this.CopyTaskWithShift = copyTaskWithShift;
        }
        public override int? GetIntervalCount()
        {
            return SourceShiftIds?.Count();
        }
    }
    public class AssignTaskToEmployeeInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public int EmployeeId { get; set; }
        public List<StaffingNeedsTaskDTO> TaskDTOs { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public AssignTaskToEmployeeInputDTO(int employeeId, DateTime date, List<StaffingNeedsTaskDTO> taskDTOs, bool skipXEMailOnShiftChanges) 
        {
            this.EmployeeId = employeeId;
            this.Date = date;
            this.TaskDTOs = taskDTOs;
            this.SkipXEMailOnShiftChanges = skipXEMailOnShiftChanges;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return TaskDTOs?.Count();
        }
    }
    public class AssignTemplateShiftTaskInputDTO : TimeEngineInputDTO
    {
        public List<StaffingNeedsTaskDTO> Tasks { get; set; }
        public DateTime Date { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public int? EmployeeId { get; set; }
        public int? EmployeePostId { get; set; }
        public AssignTemplateShiftTaskInputDTO(List<StaffingNeedsTaskDTO> tasks, DateTime date, int timeScheduleTemplateHeadId) 
        {
            this.Tasks = tasks;
            this.Date = date;
            this.TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return Tasks?.Count();
        }
    }
    public class AssignTimeScheduleTemplateToEmployeeInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleTemplateHeadId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public AssignTimeScheduleTemplateToEmployeeInputDTO(int timeScheduleTemplateHeadId, int employeeId, DateTime startDate) : base() 
        {
            this.TimeScheduleTemplateHeadId = timeScheduleTemplateHeadId;
            this.EmployeeId = employeeId;
            this.StartDate = startDate;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class RemoveEmployeeFromShiftQueueInputDTO : TimeEngineInputDTO
    {
        public TermGroup_TimeScheduleTemplateBlockQueueType Type { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int EmployeeId { get; set; }
        public RemoveEmployeeFromShiftQueueInputDTO(TermGroup_TimeScheduleTemplateBlockQueueType type, int timeScheduleTemplateBlockId, int employeeId) 
        {
            this.Type = type;
            this.TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId;
            this.EmployeeId = employeeId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class ActivateScenarioInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public DateTime? PreliminaryDateFrom { get; set; }
        public bool SendMessage { get; set; }
        public List<ActivateScenarioRowDTO> Rows { get; set; }
        public ActivateScenarioInputDTO(ActivateScenarioDTO input) : base()
        { 
            this.TimeScheduleScenarioHeadId = input.TimeScheduleScenarioHeadId;
            this.PreliminaryDateFrom = input.PreliminaryDateFrom;
            this.Rows = input.Rows;
            this.SendMessage = input.SendMessage;
        }
    }
    public class CreateTemplateFromScenarioInputDTO : TimeEngineInputDTO
    {
        public int TimeScheduleScenarioHeadId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int WeekInCycle { get; set; }
        public CreateTemplateFromScenarioInputDTO(CreateTemplateFromScenarioDTO input) : base() 
        {
            this.TimeScheduleScenarioHeadId = input.TimeScheduleScenarioHeadId;
            this.DateFrom = input.DateFrom;
            this.DateTo = input.DateTo;
            this.WeekInCycle = input.WeekInCycle;
        }
    }
    public class SaveTimeScheduleScenarioHeadInputDTO : TimeEngineInputDTO
    {
        public int? TimeScheduleScenarioHeadId { get; set; }
        public bool IncludeAbsence { get; set; }
        public int DateFunction { get; set; }
        public TimeScheduleScenarioHeadDTO ScenarioHeadInput { get; set; }
        public SaveTimeScheduleScenarioHeadInputDTO(TimeScheduleScenarioHeadDTO scenarioHeadInput, int? timeScheduleScenarioHeadId, bool includeAbsence, int dateFunction) : base() 
        {
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
            this.IncludeAbsence = includeAbsence;
            this.DateFunction = dateFunction;
            this.ScenarioHeadInput = scenarioHeadInput;
        }
    }
    public class PerformAbsenceRequestPlanningActionInputDTO : TimeEngineInputDTO
    {
        public int EmployeeRequestId { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public PerformAbsenceRequestPlanningActionInputDTO(int employeeRequestId, IEnumerable<TimeSchedulePlanningDayDTO> shifts, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId) : base()
        {
            this.EmployeeRequestId = employeeRequestId;
            this.SkipXEMailOnShiftChanges = skipXEMailOnShiftChanges;
            this.Shifts = shifts.ToList();
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return Shifts?.Count();
        }
    }
    public class PerformRestoreAbsenceRequestedShiftsInputDTO : TimeEngineInputDTO
    {
        public int EmployeeRequestId { get; set; }
        public bool SetRequestAsPending { get; set; }
        public PerformRestoreAbsenceRequestedShiftsInputDTO(int employeeRequestId, bool setRequestAsPending) : base()
        {
            this.EmployeeRequestId = employeeRequestId;
            this.SetRequestAsPending = setRequestAsPending;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }
    public class GenerateAndSaveAbsenceFromStaffingInputDTO : TimeEngineInputDTO
    {
        public EmployeeRequestDTO EmployeeRequest { get; set; }
        public int EmployeeId
        {
            get
            {
                return this.EmployeeRequest?.EmployeeId ?? 0;
            }
        }
        public int TimeDeviationCauseId
        {
            get
            {
                return this.EmployeeRequest?.TimeDeviationCauseId ?? 0;
            }
        }
        public int? EmployeeChildId
        {
            get
            {
                return this.EmployeeRequest?.EmployeeChildId;
            }
        }
        public string Comment
        {
            get
            {
                return this.EmployeeRequest?.Comment;
            }
        }
        public bool ScheduledAbsence { get; set; }
        public bool SkipXEMailOnShiftChanges { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public GenerateAndSaveAbsenceFromStaffingInputDTO(EmployeeRequestDTO employeeRequest, List<TimeSchedulePlanningDayDTO> Shifts, bool scheduledAbsence, bool skipXEMailOnShiftChanges, int? timeScheduleScenarioHeadId) : base()
        {
            this.EmployeeRequest = employeeRequest;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
            this.ScheduledAbsence = scheduledAbsence;
            this.SkipXEMailOnShiftChanges = skipXEMailOnShiftChanges;
            this.Shifts = Shifts;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return Shifts?.Count();
        }
    }
    public class GetBreaksForScheduleBlockInputDTO : TimeEngineInputDTO
    {
        public TimeScheduleTemplateBlock ScheduleBlock { get; set; }
        public GetBreaksForScheduleBlockInputDTO(TimeScheduleTemplateBlock scheduleBlock)
        {
            this.ScheduleBlock = scheduleBlock;
        }
    }
    public class HasEmployeeValidTimeCodeBreakInputDTO : TimeEngineInputDTO
    {
        public DateTime Date { get; set; }
        public int TimeCodeId { get; set; }
        public int EmployeeId { get; set; }
        public HasEmployeeValidTimeCodeBreakInputDTO(DateTime date, int timeCodeId, int employeeId) : base()
        {
            this.Date = date;
            this.TimeCodeId = timeCodeId;
            this.EmployeeId = employeeId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }
    public class ValidateBreakChangeInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int TimeCodeBreakId { get; set; }
        public DateTime StartTime { get; set; }
        public int BreakLength { get; set; }
        public bool IsTemplate { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public ValidateBreakChangeInputDTO(int employeeId, int timeScheduleTemplateBlockId, int timeScheduleTemplatePeriodId, int timeCodeBreakId, DateTime startTime, int breakLength, bool isTemplate, int? timeScheduleScenarioHeadId) : base()
        {
            this.EmployeeId = employeeId;
            this.TimeScheduleTemplateBlockId = timeScheduleTemplateBlockId;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            this.TimeCodeBreakId = timeCodeBreakId;
            this.StartTime = startTime;
            this.BreakLength = breakLength;
            this.IsTemplate = isTemplate;
            this.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
    }

    #endregion

    #region Output

    public class GetTimeScheduleTemplateOutputDTO : TimeEngineOutputDTO
    {
        public GetTimeScheduleTemplateOutputDTO() { }
        public TimeScheduleTemplateHead TemplateHead { get; set; }
    }
    public class GetTimeScheduleOutputDTO : TimeEngineOutputDTO
    {
        public GetTimeScheduleOutputDTO(){ }
        public List<TimeScheduleTemplatePeriod> TemplatePeriods { get; set; }
    }
    public class GetSequentialScheduleOutputDTO : TimeEngineOutputDTO
    {
        public GetSequentialScheduleOutputDTO() { }
        public TimeScheduleTemplatePeriod TemplatePeriod { get; set; }
    }
    public class SaveTimeScheduleTemplateOutputDTO : TimeEngineOutputDTO { }
    public class SaveTimeScheduleTemplateStaffingOutputDTO : TimeEngineOutputDTO { }
    public class SaveTimeScheduleOutputDTO : TimeEngineOutputDTO
    {
        public List<int> AutogenTimeBlockDateIds { get; set; }
        public List<int> StampingTimeBlockDateIds { get; set; }
        public SaveTimeScheduleOutputDTO() { }
    }
    public class UpdateTimeScheduleTemplateStaffingOutputDTO : TimeEngineOutputDTO
    {
        public List<int> StampingTimeBlockDateIds { get; set; }
        public UpdateTimeScheduleTemplateStaffingOutputDTO()
        {
            this.StampingTimeBlockDateIds = new List<int>();
        }
    }
    public class SaveShiftPrelDefOutputDTO : TimeEngineOutputDTO { }
    public class CopyScheduleOutputDTO : TimeEngineOutputDTO { }
    public class DeleteTimeScheduleTemplateOutputDTO : TimeEngineOutputDTO { }
    public class RemoveEmployeeFromTimeScheduleTemplateOutputDTO : TimeEngineOutputDTO { }
    public class RemoveAbsenceInScenarioOutputDTO : TimeEngineOutputDTO { }
    public class SaveEmployeeSchedulePlacementOutputDTO : TimeEngineOutputDTO { }
    public class SaveEmployeeSchedulePlacementFromJobOutputDTO : TimeEngineOutputDTO { }
    public class SaveEmployeeSchedulePlacementStaffingOutputDTO : TimeEngineOutputDTO { }
    public class DeleteEmployeeSchedulePlacementOutputDTO : TimeEngineOutputDTO { }
    public class ControlEmployeeSchedulePlacementOutputDTO : TimeEngineOutputDTO 
    {
        public ActivateScheduleControlDTO Control { get; set; }
        public ControlEmployeeSchedulePlacementOutputDTO()
        {
            this.Control = new ActivateScheduleControlDTO();
        }
    }
    public class GetEmployeeRequestsOutputDTO : TimeEngineOutputDTO
    {
        public List<EmployeeRequest> EmployeeRequests { get; set; }
        public GetEmployeeRequestsOutputDTO()
        {
            this.EmployeeRequests = new List<EmployeeRequest>();
        }
    }
    public class LoadEmployeeRequestOutputDTO : TimeEngineOutputDTO
    {
        public EmployeeRequest EmployeeRequest { get; set; }
        public LoadEmployeeRequestOutputDTO() : base()
        {
            this.EmployeeRequest = new EmployeeRequest();
        }
    }
    public class SaveEmployeeRequestOutputDTO : TimeEngineOutputDTO { }
    public class SaveOrDeleteEmployeeRequestOutputDTO : TimeEngineOutputDTO { }
    public class DeleteEmployeeRequestOutputDTO : TimeEngineOutputDTO { }
    public class GetAvailableEmployeesOutputDTO : TimeEngineOutputDTO
    {
        public List<AvailableEmployeesDTO> AvailableEmployees { get; set; }
        public GetAvailableEmployeesOutputDTO()
        {
            this.AvailableEmployees = new List<AvailableEmployeesDTO>();
        }
    }
    public class GetAvailableTimeOutputDTO : TimeEngineOutputDTO
    {
        public int ScheduledMinutes { get; set; }   // Schedule
        public int PlannedMinutes { get; set; }     // Assignments
        public int BookedMinutes { get; set; }      // Bookings
        public int AvailableMinutes { get; set; }   // Available
        public GetAvailableTimeOutputDTO() { }
    }
    public class InitiateScheduleSwapOutputDTO : TimeEngineOutputDTO { }
    public class ApproveScheduleSwapOutputDTO : TimeEngineOutputDTO { }
    public class SaveTimeScheduleShiftOutputDTO : TimeEngineOutputDTO
    {
        public List<int> StampingTimeBlockDateIds { get; set; }
        public SaveTimeScheduleShiftOutputDTO() : base() 
        {
            this.StampingTimeBlockDateIds = new List<int>();
        }
    }
    public class DeleteTimeScheduleShiftOutputDTO : TimeEngineOutputDTO { }
    public class HandleTimeScheduleShiftOutputDTO : TimeEngineOutputDTO { }
    public class SplitTimeScheduleShiftOutputDTO : TimeEngineOutputDTO { }
    public class DragTimeScheduleShiftOutputDTO : TimeEngineOutputDTO { }
    public class DragTimeScheduleShiftMultipelOutputDTO : TimeEngineOutputDTO { }
    public class SplitTemplateTimeScheduleShiftOutputDTO : TimeEngineOutputDTO { }
    public class DragTemplateTimeScheduleShiftOutputDTO : TimeEngineOutputDTO { }
    public class RemoveEmployeeFromShiftQueueOutputDTO : TimeEngineOutputDTO { }
    public class AssignTaskToEmployeeOutputDTO : TimeEngineOutputDTO { }
    public class AssignTemplatShiftTaskOutputDTO : TimeEngineOutputDTO
    {
        public List<TimeSchedulePlanningDayDTO> Shifts { get; set; }
        public AssignTemplatShiftTaskOutputDTO() { }
    }
    public class AssignTimeScheduleTemplateToEmployeeOutputDTO : TimeEngineOutputDTO { }
    public class ActivateScenarioOutputDTO : TimeEngineOutputDTO { }
    public class SaveTimeScheduleScenarioHeadOutputDTO : TimeEngineOutputDTO { }
    public class PerformAbsenceRequestPlanningActionOutputDTO : TimeEngineOutputDTO { }
    public class PerformRestoreAbsenceRequestedShiftsOutputDTO : TimeEngineOutputDTO { }
    public class GenerateAbsenceFromStaffingOutputDTO : TimeEngineOutputDTO { }
    public class GetBreaksForScheduleBlockOutputDTO : TimeEngineOutputDTO
    {
        public GetBreaksForScheduleBlockOutputDTO() { }
        public List<TimeScheduleTemplateBlock> ScheduleBlockBreaks { get; set; }
    }
    public class HasEmployeeValidTimeCodeBreakOutputDTO : TimeEngineOutputDTO { }
    public class ValidateBreakChangeOutputDTO : TimeEngineOutputDTO
    {
        public ValidateBreakChangeOutputDTO() : base() { }
        public ValidateBreakChangeResult ValidationResult { get; set; }
    }

    #endregion
}
