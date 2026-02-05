using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeSchedulePlanningMonthDTO
    {
        // Date
        public DateTime Date { get; set; }
        public string DayDescription { get; set; }

        // Number of shifts
        public int Open { get; set; }
        public int Assigned { get; set; }
        public int Wanted { get; set; }
        public int Unwanted { get; set; }
        public int AbsenceRequested { get; set; }
        public int AbsenceApproved { get; set; }
        public int Preliminary { get; set; }

        // ToolTips
        public string OpenToolTip { get; set; }
        public string AssignedToolTip { get; set; }
        public string WantedToolTip { get; set; }
        public string UnwantedToolTip { get; set; }
        public string AbsenceRequestedToolTip { get; set; }
        public string AbsenceApprovedToolTip { get; set; }
        public string PreliminaryToolTip { get; set; }

        // Summaries
        public int PlannedMinutes { get; set; }
        public int WorkTimeMinutes { get; set; }

        // Net-/gross time and cost
        public int GrossTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalCostIncEmpTaxAndSuppCharge { get; set; }
        public bool UseWeekendSalary { get; set; }

        //Currently only used from Mobile webservice
        public bool IsWholeDayAbsence { get; set; }
        public bool IsPartTimeAbsence { get; set; }
        public DateTime AssignedScheduleIn { get; set; }
        public DateTime AssignedScheduleOut { get; set; }
        public bool HasDescription { get; set; }
    }

    public class TimeSchedulePlanningMonthSmallDTO
    {
        // Date
        public DateTime Date { get; set; }

        // Number of shifts
        public int Open { get; set; }

        public int Unwanted { get; set; }
        public int AbsenceRequested { get; set; }


        // Summaries
        public int PlannedMinutes { get; set; }

        public bool IsWholeDayAbsence { get; set; }
        public bool IsPartTimeAbsence { get; set; }
        public DateTime AssignedScheduleIn { get; set; }
        public DateTime AssignedScheduleOut { get; set; }
        public bool HasDescription { get; set; }
        public bool HasOnDuty { get; set; }
        public bool HasTypeOrder { get; set; }
        public string TypeOrderColor { get; set; }
    }

    public class TimeSchedulePlanningMonthDetailDTO
    {
        // Date
        public DateTime Date { get; set; }

        // Shifts
        public List<TimeSchedulePlanningMonthDetailShiftDTO> Open { get; set; }
        public List<TimeSchedulePlanningMonthDetailShiftDTO> Assigned { get; set; }
        public List<TimeSchedulePlanningMonthDetailShiftDTO> Wanted { get; set; }
        public List<TimeSchedulePlanningMonthDetailShiftDTO> Unwanted { get; set; }
        public List<TimeSchedulePlanningMonthDetailShiftDTO> AbsenceRequested { get; set; }
        public List<TimeSchedulePlanningMonthDetailShiftDTO> AbsenceApproved { get; set; }
        public List<TimeSchedulePlanningMonthDetailShiftDTO> Preliminary { get; set; }
    }

    public class TimeSchedulePlanningMonthDetailShiftDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string ShiftTimeRange { get; set; }
        public string ShiftTypeName { get; set; }
        public string DeviationCauseName { get; set; }
        public bool IsPreliminary { get; set; }
        public string Queue { get; set; }

    }

    public class TimeSchedulePlanningDayDTO
    {
        [TsIgnore]
        public string UniqueId { get; set; }

        // General
        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }
        public int? TimeScheduleTemplateHeadId { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int TempTimeScheduleTemplateBlockId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int TimeScheduleEmployeePeriodId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }

        [TsIgnore]
        public DateTime ActualDate
        {
            get
            {
                if (this.BelongsToPreviousDay)
                    return this.StartTime.Date.AddDays(-1);
                else if (this.BelongsToNextDay)
                    return this.StartTime.Date.AddDays(1);
                else
                    return this.StartTime.Date;
            }
        }

        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime AbsenceStartTime { get; set; }
        public DateTime AbsenceStopTime { get; set; }
        public int WeekNr { get; set; }
        public bool BelongsToPreviousDay { get; set; }
        public bool BelongsToNextDay { get; set; }

        public int TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeCode { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public bool TimeScheduleTypeIsNotScheduleTime { get; set; }
        public List<TimeScheduleTypeFactorSmallDTO> TimeScheduleTypeFactors { get; set; }
        public int ShiftTypeTimeScheduleTypeId { get; set; }
        public string ShiftTypeTimeScheduleTypeCode { get; set; }
        public string ShiftTypeTimeScheduleTypeName { get; set; }

        public int? UserId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeInfo { get; set; }
        public bool IsHiddenEmployee { get; set; }
        public bool IsVacant { get; set; }
        public bool HasMultipleEmployeeAccountsOnDate { get; set; }
        public string Description { get; set; }
        public DayOfWeek DayName { get; set; }
        public bool IsLended { get; set; }

        // Net-/gross time and cost
        public int NetTime { get; set; }
        public int GrossTime { get; set; }
        [TsIgnore]
        public decimal GrossTimeDecimal { get; set; }
        public TimeSpan BreakTime { get; set; }
        public TimeSpan IwhTime { get; set; }
        public TimeSpan GrossNetDiff { get; set; }
        public decimal CostPerHour { get; set; }
        public decimal EmploymentTaxCost { get; set; }
        public decimal SupplementChargeCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalCostIncEmpTaxAndSuppCharge { get; set; }

        // Breaks
        public int Break1Id { get; set; }
        public int Break1TimeCodeId { get; set; }
        public DateTime Break1StartTime { get; set; }
        public int Break1Minutes { get; set; }
        [TsIgnore]
        public Guid? Break1Link { get; set; }
        public bool Break1IsPreliminary { get; set; }
        public int Break2Id { get; set; }
        public int Break2TimeCodeId { get; set; }
        public DateTime Break2StartTime { get; set; }
        public int Break2Minutes { get; set; }
        [TsIgnore]
        public Guid? Break2Link { get; set; }
        public bool Break2IsPreliminary { get; set; }
        public int Break3Id { get; set; }
        public int Break3TimeCodeId { get; set; }
        public DateTime Break3StartTime { get; set; }
        public int Break3Minutes { get; set; }
        [TsIgnore]
        public Guid? Break3Link { get; set; }
        public bool Break3IsPreliminary { get; set; }
        public int Break4Id { get; set; }
        public int Break4TimeCodeId { get; set; }
        public DateTime Break4StartTime { get; set; }
        public int Break4Minutes { get; set; }
        [TsIgnore]
        public Guid? Break4Link { get; set; }
        public bool Break4IsPreliminary { get; set; }

        // Breaks - only serverside
        [TsIgnore]
        public bool HandleBreaks { get; set; }          // Breaks from source will either be copied or moved to destination
        [TsIgnore]
        public bool MoveBreaksWithShift { get; set; }   // Assign the breaks to destination - change employee
        [TsIgnore]
        public bool CopyBreaksWithShift { get; set; }   // Create a new copy of the breakblock to destination
        [TsIgnore]
        public List<int> BreaksToHandle { get; set; }

        [TsIgnore]
        public int TotalBreakMinutes
        {
            get
            {
                return this.Break1Minutes + this.Break2Minutes + this.Break3Minutes + this.Break4Minutes;
            }
        }
        // TimeCode
        public int TimeCodeId { get; set; }

        // TimeDeviationCause
        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockAbsenceType AbsenceType { get; set; }

        // EmployeeChild
        public int? EmployeeChildId { get; set; }
        public string EmployeeChildName { get; set; }

        // Shift
        public int ShiftTypeId { get; set; }
        public string ShiftTypeCode { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeDesc { get; set; }
        public string ShiftTypeColor { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftStatus ShiftStatus { get; set; }
        public string ShiftStatusName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus ShiftUserStatus { get; set; }
        public string ShiftUserStatusName { get; set; }

        public bool ExtraShift { get; set; }
        public bool SubstituteShift { get; set; }

        // Staffing
        public bool IsPreliminary { get; set; }
        public int NbrOfWantedInQueue { get; set; }
        public bool IamInQueue { get; set; }
        public int ApprovalTypeId { get; set; }
        public int AbsenceRequestShiftPlanningAction { get; set; }
        public bool HasShiftRequest { get; set; }
        public XEMailAnswerType ShiftRequestAnswerType { get; set; }
        [TsIgnore]
        public bool HasShiftRequestAnswer
        {
            get
            {
                return this.ShiftRequestAnswerType != XEMailAnswerType.None;
            }
        }
        public Guid CalculationGuid { get; set; }
        public Guid PeriodGuid { get; set; }
        public bool HasSwapRequest { get; set; }
        public string SwapShiftInfo { get; set; }

        [TsIgnore]
        public string TaskKey
        {
            get
            {
                return $"t{TimeScheduleTaskId}#d{IncomingDeliveryRowId}";
            }
        }

        // Template schedule
        public int NbrOfWeeks { get; set; }
        public int OriginalBlockId { get; set; }

        // Accounts
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
        public List<int> AccountIds { get; set; }

        public List<int> GetAccountIdsIncludingAccountIdOnBlock()
        {
            List<int> resultAccountIds = new List<int>();

            if (AccountIds != null)
                resultAccountIds.AddRange(AccountIds);

            if (AccountId.HasValue)
                resultAccountIds.Add(AccountId.Value);

            return resultAccountIds;
        }

        // Placement
        public int DayNumber { get; set; }
        [TsIgnore]
        public Guid? Link { get; set; }

        // AbsenceRequest
        public bool IsAbsenceRequest { get; set; }

        // LeisureCode
        public int? TimeScheduleEmployeePeriodDetailId { get; set; }
        public int? TimeLeisureCodeId { get; set; }


        // Staffing needs
        public int? StaffingNeedsRowId { get; set; }
        public int? StaffingNeedsRowPeriodId { get; set; }
        [TsIgnore]
        public string StaffingNeedsOrigin { get; set; }
        [TsIgnore]
        public DayOfWeek? StaffingNeedsWeekday { get; set; }
        [TsIgnore]
        public int? StaffingNeedsDayTypeId { get; set; }
        [TsIgnore]
        public DateTime? StaffingNeedsDate { get; set; }

        // EmployeePost
        public int? EmployeePostId { get; set; }

        // TimeScheduleTask
        public int? TimeScheduleTaskId { get; set; }

        // IncomingDeliveryRow
        public int? IncomingDeliveryRowId { get; set; }

        // WorkRules
        public int? SourceTimeScheduleTemplateBlockId { get; set; }

        // Order planning
        [TsIgnore]
        public OrderListDTO Order { get; set; }
        public int? PlannedTime { get; set; }

        // Flags
        public bool IsDeleted { get; set; }

        // Extensions for Angular
        // Guids can not be used in Angular, so we make string properties of them
        public string Break1LinkStr { get; set; }
        public string Break2LinkStr { get; set; }
        public string Break3LinkStr { get; set; }
        public string Break4LinkStr { get; set; }
        public string LinkStr { get; set; }

        //Tasks
        public List<TimeScheduleTemplateBlockTaskDTO> Tasks { get; set; }

        //Report
        [TsIgnore]
        public List<DateTime> ValidIntervalStartTimes { get; set; }

        #region Methods

        public bool IsZeroShift()
        {
            return this.StartTime == this.StopTime;
        }
        public bool IsWholeDay()
        {
            return this.StartTime.TimeOfDay == new TimeSpan(0, 0, 0) &&
                   this.StopTime.TimeOfDay == new TimeSpan(23, 59, 0);
        }

        public bool IsSchedule()
        {
            return this.Type == TermGroup_TimeScheduleTemplateBlockType.Schedule;
        }

        // A shift with a connected order
        public bool IsOrder()
        {
            return this.Type == TermGroup_TimeScheduleTemplateBlockType.Order && this.Order != null;
        }

        public bool IsBooking()
        {
            return this.Type == TermGroup_TimeScheduleTemplateBlockType.Booking;
        }

        public bool IsStandby()
        {
            return this.Type == TermGroup_TimeScheduleTemplateBlockType.Standby;
        }

        public bool IsOnDuty()
        {
            return this.Type == TermGroup_TimeScheduleTemplateBlockType.OnDuty;
        }

        [TsIgnore]
        public bool ChangesAffectsRestore
        {
            get { return this.IsSchedule() || this.IsStandby(); }
        }
        public bool IsOverMidnight()
        {
            return StartTime.Date != StopTime.Date;
        }

        #region Breaks

        public List<BreakDTO> GetBreaks(bool ignoreZeroBreaks = false)
        {
            var breaks = new List<BreakDTO>();

            if (this.Break1Minutes > 0 || ignoreZeroBreaks)
            {
                breaks.Add(new BreakDTO
                {
                    Id = this.Break1Id,
                    TimeCodeId = this.Break1TimeCodeId,
                    StartTime = this.Break1StartTime,
                    BreakMinutes = this.Break1Minutes,
                    Link = this.Break1Link,
                    IsPreliminary = this.Break1IsPreliminary
                });
            }

            if (this.Break2Minutes > 0 || ignoreZeroBreaks)
            {
                breaks.Add(new BreakDTO
                {
                    Id = this.Break2Id,
                    TimeCodeId = this.Break2TimeCodeId,
                    StartTime = this.Break2StartTime,
                    BreakMinutes = this.Break2Minutes,
                    Link = this.Break2Link,
                    IsPreliminary = this.Break2IsPreliminary
                });
            }

            if (this.Break3Minutes > 0 || ignoreZeroBreaks)
            {
                breaks.Add(new BreakDTO
                {
                    Id = this.Break3Id,
                    TimeCodeId = this.Break3TimeCodeId,
                    StartTime = this.Break3StartTime,
                    BreakMinutes = this.Break3Minutes,
                    Link = this.Break3Link,
                    IsPreliminary = this.Break3IsPreliminary
                });
            }

            if (this.Break4Minutes > 0 || ignoreZeroBreaks)
            {
                breaks.Add(new BreakDTO
                {
                    Id = this.Break4Id,
                    TimeCodeId = this.Break4TimeCodeId,
                    StartTime = this.Break4StartTime,
                    BreakMinutes = this.Break4Minutes,
                    Link = this.Break4Link,
                    IsPreliminary = this.Break4IsPreliminary
                });
            }

            return breaks.OrderBy(i => i.StartTime).ToList();
        }

        public void AddNewBreaks(List<BreakDTO> breakItems, Guid newLink, DateTime? newDate = null)
        {
            foreach (var breakItem in breakItems)
            {
                this.AddBreak(breakItem, true, newLink, newDate);
            }
        }

        public void AddBreaks(List<BreakDTO> breakItems)
        {
            foreach (var breakItem in breakItems)
            {
                this.AddBreak(breakItem, false, breakItem.Link, null);
            }
        }

        public void AddBreak(BreakDTO breakDTO, bool createNew, Guid? newLink, DateTime? newDate)
        {
            if (this.Break1TimeCodeId == 0)
            {
                this.Break1Id = createNew ? 0 : breakDTO.Id;
                this.Break1TimeCodeId = breakDTO.TimeCodeId;
                this.Break1StartTime = newDate.HasValue ? CalendarUtility.MergeDateAndTime(newDate.Value, breakDTO.StartTime).AddDays(breakDTO.OffsetDaysOnCopy) : breakDTO.StartTime;
                this.Break1Minutes = breakDTO.BreakMinutes;
                this.Break1Link = newLink;
                this.Break1IsPreliminary = breakDTO.IsPreliminary;
            }
            else if (this.Break2TimeCodeId == 0)
            {
                this.Break2Id = createNew ? 0 : breakDTO.Id;
                this.Break2TimeCodeId = breakDTO.TimeCodeId;
                this.Break2StartTime = newDate.HasValue ? CalendarUtility.MergeDateAndTime(newDate.Value, breakDTO.StartTime).AddDays(breakDTO.OffsetDaysOnCopy) : breakDTO.StartTime;
                this.Break2Minutes = breakDTO.BreakMinutes;
                this.Break2Link = newLink;
                this.Break2IsPreliminary = breakDTO.IsPreliminary;
            }
            else if (this.Break3TimeCodeId == 0)
            {
                this.Break3Id = createNew ? 0 : breakDTO.Id;
                this.Break3TimeCodeId = breakDTO.TimeCodeId;
                this.Break3StartTime = newDate.HasValue ? CalendarUtility.MergeDateAndTime(newDate.Value, breakDTO.StartTime).AddDays(breakDTO.OffsetDaysOnCopy) : breakDTO.StartTime;
                this.Break3Minutes = breakDTO.BreakMinutes;
                this.Break3Link = newLink;
                this.Break3IsPreliminary = breakDTO.IsPreliminary;
            }
            else if (this.Break4TimeCodeId == 0)
            {
                this.Break4Id = createNew ? 0 : breakDTO.Id;
                this.Break4TimeCodeId = breakDTO.TimeCodeId;
                this.Break4StartTime = newDate.HasValue ? CalendarUtility.MergeDateAndTime(newDate.Value, breakDTO.StartTime).AddDays(breakDTO.OffsetDaysOnCopy) : breakDTO.StartTime;
                this.Break4Minutes = breakDTO.BreakMinutes;
                this.Break4Link = newLink;
                this.Break4IsPreliminary = breakDTO.IsPreliminary;
            }
        }

        public void SetBreakData(int breakNr, int timeCodeId, DateTime date, DateTime startTime, int length)
        {
            startTime = CalendarUtility.GetDateTime(date, startTime);

            switch (breakNr)
            {
                case 1:
                    this.Break1TimeCodeId = timeCodeId;
                    this.Break1StartTime = startTime;
                    this.Break1Minutes = length;
                    break;
                case 2:
                    this.Break2TimeCodeId = timeCodeId;
                    this.Break2StartTime = startTime;
                    this.Break2Minutes = length;
                    break;
                case 3:
                    this.Break3TimeCodeId = timeCodeId;
                    this.Break3StartTime = startTime;
                    this.Break3Minutes = length;
                    break;
                case 4:
                    this.Break4TimeCodeId = timeCodeId;
                    this.Break4StartTime = startTime;
                    this.Break4Minutes = length;
                    break;
            }
        }

        public void ClearBreaks(List<BreakDTO> breaksItems)
        {
            foreach (var item in breaksItems)
            {
                this.ClearBreak(item);
            }
        }

        public void ClearBreak(BreakDTO breakDTO)
        {
            if (breakDTO.Id != 0)
            {
                if (this.Break1Id == breakDTO.Id)
                {
                    this.Break1Id = 0;
                    this.Break1TimeCodeId = 0;
                    this.Break1Minutes = 0;
                }
                else if (this.Break2Id == breakDTO.Id)
                {
                    this.Break2Id = 0;
                    this.Break2TimeCodeId = 0;
                    this.Break2Minutes = 0;
                }
                else if (this.Break3Id == breakDTO.Id)
                {
                    this.Break3Id = 0;
                    this.Break3TimeCodeId = 0;
                    this.Break3Minutes = 0;
                }
                else if (this.Break4Id == breakDTO.Id)
                {
                    this.Break4Id = 0;
                    this.Break4TimeCodeId = 0;
                    this.Break4Minutes = 0;
                }
            }
        }

        public int GetBreakTimeWithinShift(DateTime? shiftStart = null, DateTime? shiftEnd = null)
        {
            if (!shiftStart.HasValue)
                shiftStart = this.StartTime;
            if (!shiftEnd.HasValue)
                shiftEnd = this.StopTime;

            int breakMinutes = 0;

            DateTime scheduleStart = shiftStart.Value;
            DateTime scheduleStop = shiftEnd.Value;

            //Dont check break1Id - we need to handle unsaved breaks

            //Get break 1 time in schedule
            DateTime break1Start = this.Break1StartTime;
            DateTime break1Stop = this.Break1StartTime.AddMinutes(this.Break1Minutes);
            if (break1Start != break1Stop)
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break1Start, break1Stop).TotalMinutes;

            //Get break 2 time in schedule
            DateTime break2Start = this.Break2StartTime;
            DateTime break2Stop = this.Break2StartTime.AddMinutes(this.Break2Minutes);
            if (break2Start != break2Stop)
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break2Start, break2Stop).TotalMinutes;

            //Get break 3 time in schedule
            DateTime break3Start = this.Break3StartTime;
            DateTime break3Stop = this.Break3StartTime.AddMinutes(this.Break3Minutes);
            if (break3Start != break3Stop)
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break3Start, break3Stop).TotalMinutes;

            //Get break 4 time in schedule
            DateTime break4Start = this.Break4StartTime;
            DateTime break4Stop = this.Break4StartTime.AddMinutes(this.Break4Minutes);
            if (break4Start != break4Stop)
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break4Start, break4Stop).TotalMinutes;

            return breakMinutes;
        }

        public int GetBreakMinutesInInterval(DateTime start, DateTime stop)
        {
            int breakMinutes = 0;

            //Add total schedule time (included breaks)
            DateTime scheduleStart = CalendarUtility.GetScheduleTime(start);
            DateTime scheduleStop = CalendarUtility.GetScheduleTime(stop, start.Date, stop.Date);

            //Get break 1 time in schedule
            if (this.Break1Id != 0)
            {
                DateTime break1Start = this.Break1StartTime;
                DateTime break1Stop = this.Break1StartTime.AddMinutes(this.Break1Minutes);
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break1Start, break1Stop).TotalMinutes;
            }

            if (this.Break2Id != 0)
            {
                //Get break 2 time in schedule
                DateTime break2Start = this.Break2StartTime;
                DateTime break2Stop = this.Break2StartTime.AddMinutes(this.Break2Minutes);
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break2Start, break2Stop).TotalMinutes;
            }

            if (this.Break3Id != 0)
            {
                //Get break 3 time in schedule
                DateTime break3Start = this.Break3StartTime;
                DateTime break3Stop = this.Break3StartTime.AddMinutes(this.Break3Minutes);
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break3Start, break3Stop).TotalMinutes;
            }

            if (this.Break4Id != 0)
            {
                //Get break 4 time in schedule
                DateTime break4Start = this.Break4StartTime;
                DateTime break4Stop = this.Break4StartTime.AddMinutes(this.Break4Minutes);
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, break4Start, break4Stop).TotalMinutes;
            }

            return breakMinutes;
        }

        public int GetBreaksTotalMinutesThatStartInInterval(DateTime start, DateTime stop, ref List<int> alreadyUsedBreaks)
        {
            int breakMinutes = 0;

            //Add total schedule time (included breaks)
            DateTime scheduleStart = start;
            DateTime scheduleStop = stop;

            //Get break 1 time in schedule
            if (this.Break1Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break1Id) && scheduleStart <= this.Break1StartTime && this.Break1StartTime < scheduleStop)
            {
                //if break starts in interval, get its minutes, if this.BreakStartTime = scheduleStop, dont include its minutes
                breakMinutes += this.Break1Minutes;
                alreadyUsedBreaks.Add(this.Break1Id);
            }

            if (this.Break2Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break2Id) && scheduleStart <= this.Break2StartTime && this.Break2StartTime < scheduleStop)
            {
                //if break starts in interval, get its minutes, if this.BreakStartTime = scheduleStop, dont include its minutes
                breakMinutes += this.Break2Minutes;
                alreadyUsedBreaks.Add(this.Break2Id);
            }

            if (this.Break3Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break3Id) && scheduleStart <= this.Break3StartTime && this.Break3StartTime < scheduleStop)
            {
                //if break starts in interval, get its minutes, if this.BreakStartTime = scheduleStop, dont include its minutes
                breakMinutes += this.Break3Minutes;
                alreadyUsedBreaks.Add(this.Break3Id);
            }

            if (this.Break4Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break4Id) && scheduleStart <= this.Break4StartTime && this.Break4StartTime < scheduleStop)
            {
                //if break starts in interval, get its minutes, if this.BreakStartTime = scheduleStop, dont include its minutes
                breakMinutes += this.Break4Minutes;
                alreadyUsedBreaks.Add(this.Break4Id);
            }

            return breakMinutes;
        }

        public int GetBreaksTotalMinutesThatEndInInterval(DateTime start, DateTime stop, ref List<int> alreadyUsedBreaks)
        {
            int breakMinutes = 0;

            //Add total schedule time (included breaks)
            DateTime scheduleStart = start;
            DateTime scheduleStop = stop;

            //Get break 1 time in schedule
            if (this.Break1Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break1Id))
            {
                DateTime break1Start = this.Break1StartTime;
                DateTime break1Stop = this.Break1StartTime.AddMinutes(this.Break1Minutes);

                //if break stops in interval, get its minutes, if break1Stop = scheduleStart, dont include its minutes
                if (scheduleStart < break1Stop && break1Stop <= scheduleStop)
                {
                    breakMinutes += this.Break1Minutes;
                    alreadyUsedBreaks.Add(this.Break1Id);
                }
            }

            if (this.Break2Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break2Id))
            {
                DateTime break2Start = this.Break2StartTime;
                DateTime break2Stop = this.Break2StartTime.AddMinutes(this.Break2Minutes);

                //if break stops in interval, get its minutes, if break1Stop = scheduleStart, dont include its minutes
                if (scheduleStart < break2Stop && break2Stop <= scheduleStop)
                {
                    breakMinutes += this.Break2Minutes;
                    alreadyUsedBreaks.Add(this.Break2Id);
                }
            }

            if (this.Break3Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break3Id))
            {
                DateTime break3Start = this.Break3StartTime;
                DateTime break3Stop = this.Break3StartTime.AddMinutes(this.Break3Minutes);

                //if break stops in interval, get its minutes, if break1Stop = scheduleStart, dont include its minutes
                if (scheduleStart < break3Stop && break3Stop <= scheduleStop)
                {
                    breakMinutes += this.Break3Minutes;
                    alreadyUsedBreaks.Add(this.Break3Id);
                }
            }

            if (this.Break4Id != 0 && !alreadyUsedBreaks.Any(x => x == this.Break4Id))
            {
                DateTime break4Start = this.Break4StartTime;
                DateTime break4Stop = this.Break4StartTime.AddMinutes(this.Break4Minutes);

                //if break stops in interval, get its minutes, if break1Stop = scheduleStart, dont include its minutes
                if (scheduleStart < break4Stop && break4Stop <= scheduleStop)
                {
                    breakMinutes += this.Break4Minutes;
                    alreadyUsedBreaks.Add(this.Break4Id);
                }
            }

            return breakMinutes;
        }

        public void CopyShiftInfo(List<BreakDTO> breaks)
        {
            foreach (var item in breaks)
            {
                item.IsPreliminary = this.IsPreliminary;
                item.AccountId = this.AccountId;
            }
        }

        public List<BreakDTO> GetMyBreaks()
        {
            return this.GetMyBreaks(this.GetBreaks());
        }

        public List<BreakDTO> GetMyBreaks(List<BreakDTO> breakitems)
        {
            return this.GetOverlappedBreaks(breakitems, true);
        }

        public List<BreakDTO> GetOverlappedBreaks(List<BreakDTO> breakitems, bool includeBreaksThatOnlyStartInScheduleBlock = false)
        {
            List<BreakDTO> overlappedBreaks = new List<BreakDTO>();

            foreach (var breakitem in breakitems)
            {
                if (breakitem.StartTime >= this.StartTime && breakitem.StopTime <= this.StopTime)
                    overlappedBreaks.Add(breakitem);
                else if (includeBreaksThatOnlyStartInScheduleBlock && breakitem.StartTime >= this.StartTime && breakitem.StartTime < this.StopTime)
                    overlappedBreaks.Add(breakitem);
            }

            return overlappedBreaks;
        }

        #endregion

        #region TimeScheduleTypeFactor

        public int GetTimeScheduleTypeFactorsWithinShift()
        {
            if (this.TimeScheduleTypeFactors == null || this.TimeScheduleTypeFactors.Count == 0)
                return 0;

            TimeSpan factorTime = new TimeSpan();

            double deltaDays = (this.StartTime.Date - CalendarUtility.DATETIME_DEFAULT).TotalDays;

            foreach (var factor in this.TimeScheduleTypeFactors)
            {
                // Set factor times to actual times for current shift (stored as 1900-01-01)
                factor.FromTime = factor.FromTime.AddDays(deltaDays);
                factor.ToTime = factor.ToTime.AddDays(deltaDays);
                double factorMinutes = 0;

                if (CalendarUtility.IsNewOverlappedByCurrent(factor.FromTime, factor.ToTime, this.StartTime, this.StopTime))
                {
                    // Factor is completely overlapped by a presence shift
                    factorMinutes = (factor.ToTime - factor.FromTime).TotalMinutes;
                    factorMinutes -= GetBreakTimeWithinShift(factor.FromTime, factor.ToTime);
                }
                else if (CalendarUtility.IsNewOverlappingCurrentStart(factor.FromTime, factor.ToTime, this.StartTime, this.StopTime))
                {
                    // Factor end intersects with a presence shift
                    factorMinutes = (factor.ToTime - this.StartTime).TotalMinutes;
                    factorMinutes -= GetBreakTimeWithinShift(this.StartTime, factor.ToTime);
                }
                else if (CalendarUtility.IsNewOverlappingCurrentStop(factor.FromTime, factor.ToTime, this.StartTime, this.StopTime))
                {
                    // Factor start intersects with a presence shift
                    factorMinutes = (this.StopTime - factor.FromTime).TotalMinutes;
                    factorMinutes -= GetBreakTimeWithinShift(factor.FromTime, this.StopTime);
                }

                factorTime = factorTime.Add(TimeSpan.FromMinutes(factorMinutes * (double)(factor.Factor - 1)));
            }

            return (int)factorTime.TotalMinutes;
        }

        #endregion

        #region Copy

        public TimeSchedulePlanningDayDTO Copy()
        {
            var newShift = new TimeSchedulePlanningDayDTO();
            newShift.CopyFrom(this);
            return newShift;
        }

        public TimeSchedulePlanningDayDTO CopyAsNew(bool resetBreaks = false)
        {
            var newShift = new TimeSchedulePlanningDayDTO();
            newShift.CopyFrom(this);
            newShift.UniqueId = Guid.NewGuid().ToString();
            newShift.Link = Guid.NewGuid();
            newShift.ClearIds();

            if (resetBreaks)
                newShift.ResetBreaks();

            return newShift;
        }

        public TimeSchedulePlanningDayDTO CopyAsNew(int? targetEmployeeId, int? targetEmployeePostId, DateTime date, DateTime cycleStartOffset, int timeScheduleTemplateHeadNbrOfDays, Guid? newLink)
        {
            TimeSchedulePlanningDayDTO newShift = this.CopyAsNew(true);
            newShift.TimeScheduleTemplateHeadId = 0;
            newShift.TimeScheduleTemplatePeriodId = 0;
            newShift.TimeScheduleEmployeePeriodId = 0;
            newShift.EmployeeId = targetEmployeeId ?? 0;
            newShift.EmployeePostId = targetEmployeePostId;
            newShift.Link = newLink;
            newShift.ResetBreaks();

            newShift.StartTime = CalendarUtility.MergeDateAndTime(date, this.StartTime);
            if (this.BelongsToPreviousDay)
                newShift.StartTime = newShift.StartTime.AddDays(1);
            else if (this.BelongsToNextDay)
                newShift.StartTime = newShift.StartTime.AddDays(-1);
            newShift.StopTime = newShift.StartTime.Add(this.StopTime - this.StartTime);

            newShift.SetTemplateDayNumber(cycleStartOffset, timeScheduleTemplateHeadNbrOfDays);
            newShift.WeekNr = CalendarUtility.GetWeekNr(newShift.ActualDate);
            return newShift;
        }

        public void ResetBreaks()
        {
            this.Break1Id = 0;
            this.Break1TimeCodeId = 0;
            this.Break1StartTime = CalendarUtility.DATETIME_DEFAULT;
            this.Break1Minutes = 0;
            this.Break1Link = null;
            this.Break1IsPreliminary = false;

            this.Break2Id = 0;
            this.Break2TimeCodeId = 0;
            this.Break2StartTime = CalendarUtility.DATETIME_DEFAULT;
            this.Break2Minutes = 0;
            this.Break2Link = null;
            this.Break2IsPreliminary = false;

            this.Break3Id = 0;
            this.Break3TimeCodeId = 0;
            this.Break3StartTime = CalendarUtility.DATETIME_DEFAULT;
            this.Break3Minutes = 0;
            this.Break3Link = null;
            this.Break3IsPreliminary = false;

            this.Break4Id = 0;
            this.Break4TimeCodeId = 0;
            this.Break4StartTime = CalendarUtility.DATETIME_DEFAULT;
            this.Break4Minutes = 0;
            this.Break4Link = null;
            this.Break4IsPreliminary = false;
        }

        public void CopyBreaks(TimeSchedulePlanningDayDTO source)
        {
            if (source == null)
                return;

            this.Break1Id = source.Break1Id;
            this.Break1TimeCodeId = source.Break1TimeCodeId;
            this.Break1StartTime = source.Break1StartTime;
            this.Break1Minutes = source.Break1Minutes;
            this.Break1Link = source.Break1Link;
            this.Break1IsPreliminary = source.Break1IsPreliminary;

            this.Break2Id = source.Break2Id;
            this.Break2TimeCodeId = source.Break2TimeCodeId;
            this.Break2StartTime = source.Break2StartTime;
            this.Break2Minutes = source.Break2Minutes;
            this.Break2Link = source.Break2Link;
            this.Break2IsPreliminary = source.Break2IsPreliminary;

            this.Break3Id = source.Break3Id;
            this.Break3TimeCodeId = source.Break3TimeCodeId;
            this.Break3StartTime = source.Break3StartTime;
            this.Break3Minutes = source.Break3Minutes;
            this.Break3Link = source.Break3Link;
            this.Break3IsPreliminary = source.Break3IsPreliminary;

            this.Break4Id = source.Break4Id;
            this.Break4TimeCodeId = source.Break4TimeCodeId;
            this.Break4StartTime = source.Break4StartTime;
            this.Break4Minutes = source.Break4Minutes;
            this.Break4Link = source.Break4Link;
            this.Break4IsPreliminary = source.Break4IsPreliminary;
        }

        public void CopyFrom(TimeSchedulePlanningDayDTO shift)
        {
            if (shift != null)
            {
                this.UniqueId = shift.UniqueId;
                this.Type = shift.Type;
                this.TimeScheduleTemplateBlockId = shift.TimeScheduleTemplateBlockId;
                this.TimeScheduleTemplateHeadId = shift.TimeScheduleTemplateHeadId;
                this.TimeScheduleTemplatePeriodId = shift.TimeScheduleTemplatePeriodId;
                this.TimeScheduleEmployeePeriodId = shift.TimeScheduleEmployeePeriodId;
                this.TimeDeviationCauseId = shift.TimeDeviationCauseId;
                this.TimeDeviationCauseName = shift.TimeDeviationCauseName;
                this.EmployeeChildId = shift.EmployeeChildId;
                this.TimeCodeId = shift.TimeCodeId;
                this.TimeScheduleTypeId = shift.TimeScheduleTypeId;
                this.TimeScheduleTypeCode = shift.TimeScheduleTypeCode;
                this.TimeScheduleTypeName = shift.TimeScheduleTypeName;
                this.TimeScheduleTypeIsNotScheduleTime = shift.TimeScheduleTypeIsNotScheduleTime;
                this.TimeScheduleTypeFactors = shift.TimeScheduleTypeFactors;
                this.ShiftTypeTimeScheduleTypeId = shift.ShiftTypeTimeScheduleTypeId;
                this.ShiftTypeTimeScheduleTypeCode = shift.ShiftTypeTimeScheduleTypeCode;
                this.ShiftTypeTimeScheduleTypeName = shift.ShiftTypeTimeScheduleTypeName;
                this.ShiftTypeId = shift.ShiftTypeId;
                this.ShiftTypeName = shift.ShiftTypeName;
                this.ShiftTypeDesc = shift.ShiftTypeDesc;
                this.ShiftTypeColor = shift.ShiftTypeColor;
                this.EmployeeId = shift.EmployeeId;
                this.UserId = shift.UserId;
                this.EmployeeName = shift.EmployeeName;
                this.EmployeeInfo = shift.EmployeeInfo;
                this.IsHiddenEmployee = shift.IsHiddenEmployee;
                this.IsVacant = shift.IsVacant;
                this.StartTime = shift.StartTime;
                this.StopTime = shift.StopTime;
                this.WeekNr = shift.WeekNr;
                this.BelongsToPreviousDay = shift.BelongsToPreviousDay;
                this.BelongsToNextDay = shift.BelongsToNextDay;
                this.Description = shift.Description;
                this.ShiftStatus = shift.ShiftStatus;
                this.ShiftUserStatus = shift.ShiftUserStatus;
                this.ExtraShift = shift.ExtraShift;
                this.SubstituteShift = shift.SubstituteShift;
                this.IsPreliminary = shift.IsPreliminary;
                this.NbrOfWantedInQueue = shift.NbrOfWantedInQueue;
                this.IamInQueue = shift.IamInQueue;
                this.DayNumber = shift.DayNumber;
                this.NbrOfWeeks = shift.NbrOfWeeks;
                this.Link = shift.Link;
                this.AccountId = shift.AccountId;
                this.ShiftStatusName = shift.ShiftStatusName;
                this.ShiftUserStatusName = shift.ShiftUserStatusName;

                // Staffing needs
                this.StaffingNeedsRowId = shift.StaffingNeedsRowId;
                this.StaffingNeedsRowPeriodId = shift.StaffingNeedsRowPeriodId;
                this.StaffingNeedsOrigin = shift.StaffingNeedsOrigin;
                this.StaffingNeedsWeekday = shift.StaffingNeedsWeekday;
                this.StaffingNeedsDayTypeId = shift.StaffingNeedsDayTypeId;
                this.StaffingNeedsDate = shift.StaffingNeedsDate;

                // Order planning
                this.Order = shift.Order;
            }

        }

        #endregion

        #region Template

        public void SetTemplateDayNumber(DateTime offset, int timeScheduleTemplateHeadNbrOfDays)
        {
            this.DayNumber = Convert.ToInt32((this.ActualDate - offset.Date).TotalDays + 1);
            while (this.DayNumber > timeScheduleTemplateHeadNbrOfDays)
            {
                this.DayNumber -= timeScheduleTemplateHeadNbrOfDays;
            }
        }

        #endregion

        #region Misc

        public void SetAsDeleted()
        {
            this.ClearTime();
        }

        public void SetAsNewActivatedShift()
        {
            this.ClearIds(true);
        }

        public void ClearIdsAndTime()
        {
            this.ClearIds();
            this.ClearTime();
        }

        public void ClearScenarioId()
        {
            this.TimeScheduleScenarioHeadId = null;
        }

        public void ClearIds(bool clearScenario = false)
        {
            this.TimeScheduleTemplateBlockId = 0;
            this.Break1Id = 0;
            this.Break2Id = 0;
            this.Break3Id = 0;
            this.Break4Id = 0;

            if (clearScenario)
                this.ClearScenarioId();

        }

        public void ClearTime()
        {
            this.StartTime = CalendarUtility.GetBeginningOfDay(this.StartTime);
            this.StopTime = this.StartTime;
        }

        public void ClearTasks()
        {
            if (this.Tasks != null)
            {
                foreach (var task in this.Tasks)
                {
                    task.TimeScheduleTemplateBlockId = null;
                    task.TimeScheduleTemplateBlockTaskId = 0;
                }
            }
        }

        public void RemoveBreaksNotInCollection(List<int> validBreaks)
        {
            var invalidBreaks = this.GetBreaks().Where(x => !validBreaks.Contains(x.Id)).ToList();
            this.ClearBreaks(invalidBreaks);
        }

        public void ChangeDate(DateTime date)
        {
            //May not be midnight-safe if starttime is after midnight
            DateTime originalShiftStartTime = this.StartTime;
            TimeSpan duration = this.StopTime.Subtract(this.StartTime);
            this.StartTime = CalendarUtility.GetDateTime(date, this.StartTime);
            this.StopTime = this.StartTime.Add(duration);

            // Change date on breaks and also calculate delta days between shift start and break start to handle breaks starting after midnight
            if (this.Break1StartTime.Year != CalendarUtility.DATETIME_0VALUE.Year && this.Break1StartTime.Year != CalendarUtility.DATETIME_DEFAULT.Year)
            {
                int deltaDaysBreak1 = CalendarUtility.GetDateTime(this.Break1StartTime, CalendarUtility.DATETIME_DEFAULT).Subtract(CalendarUtility.GetDateTime(originalShiftStartTime, CalendarUtility.DATETIME_DEFAULT)).Days;
                this.Break1StartTime = CalendarUtility.GetDateTime(date.AddDays(deltaDaysBreak1), this.Break1StartTime);
            }
            if (this.Break2StartTime.Year != CalendarUtility.DATETIME_0VALUE.Year && this.Break2StartTime.Year != CalendarUtility.DATETIME_DEFAULT.Year)
            {
                int deltaDaysBreak2 = CalendarUtility.GetDateTime(this.Break2StartTime, CalendarUtility.DATETIME_DEFAULT).Subtract(CalendarUtility.GetDateTime(originalShiftStartTime, CalendarUtility.DATETIME_DEFAULT)).Days;
                this.Break2StartTime = CalendarUtility.GetDateTime(date.AddDays(deltaDaysBreak2), this.Break2StartTime);
            }
            if (this.Break3StartTime.Year != CalendarUtility.DATETIME_0VALUE.Year && this.Break3StartTime.Year != CalendarUtility.DATETIME_DEFAULT.Year)
            {
                int deltaDaysBreak3 = CalendarUtility.GetDateTime(this.Break3StartTime, CalendarUtility.DATETIME_DEFAULT).Subtract(CalendarUtility.GetDateTime(originalShiftStartTime, CalendarUtility.DATETIME_DEFAULT)).Days;
                this.Break3StartTime = CalendarUtility.GetDateTime(date.AddDays(deltaDaysBreak3), this.Break3StartTime);
            }
            if (this.Break4StartTime.Year != CalendarUtility.DATETIME_0VALUE.Year && this.Break4StartTime.Year != CalendarUtility.DATETIME_DEFAULT.Year)
            {
                int deltaDaysBreak4 = CalendarUtility.GetDateTime(this.Break4StartTime, CalendarUtility.DATETIME_DEFAULT).Subtract(CalendarUtility.GetDateTime(originalShiftStartTime, CalendarUtility.DATETIME_DEFAULT)).Days;
                this.Break4StartTime = CalendarUtility.GetDateTime(date.AddDays(deltaDaysBreak4), this.Break4StartTime);
            }
        }

        public void SetNewLinks(List<Tuple<string, string>> linkMappings)
        {
            if (this.Link.HasValue)
            {
                var shiftLinkMapping = linkMappings.FirstOrDefault(x => x.Item1 == this.Link.Value.ToString());
                if (shiftLinkMapping != null)
                    this.Link = new Guid(shiftLinkMapping.Item2);
            }
            if (this.Break1Link.HasValue)
            {
                var shiftLinkMapping = linkMappings.FirstOrDefault(x => x.Item1 == this.Break1Link.Value.ToString());
                if (shiftLinkMapping != null)
                    this.Break1Link = new Guid(shiftLinkMapping.Item2);
            }
            if (this.Break2Link.HasValue)
            {
                var shiftLinkMapping = linkMappings.FirstOrDefault(x => x.Item1 == this.Break2Link.Value.ToString());
                if (shiftLinkMapping != null)
                    this.Break2Link = new Guid(shiftLinkMapping.Item2);
            }
            if (this.Break3Link.HasValue)
            {
                var shiftLinkMapping = linkMappings.FirstOrDefault(x => x.Item1 == this.Break3Link.Value.ToString());
                if (shiftLinkMapping != null)
                    this.Break3Link = new Guid(shiftLinkMapping.Item2);
            }
            if (this.Break4Link.HasValue)
            {
                var shiftLinkMapping = linkMappings.FirstOrDefault(x => x.Item1 == this.Break4Link.Value.ToString());
                if (shiftLinkMapping != null)
                    this.Break4Link = new Guid(shiftLinkMapping.Item2);
            }
        }

        public static TimeSchedulePlanningDayDTO CreateZeroShift(int employeeId, DateTime date, int? timeScheduleScenarioHeadId)
        {
            TimeSchedulePlanningDayDTO dayDTO = new TimeSchedulePlanningDayDTO();
            dayDTO.Type = TermGroup_TimeScheduleTemplateBlockType.Schedule;
            dayDTO.EmployeeId = employeeId;
            dayDTO.StartTime = date.Date;
            dayDTO.StopTime = date.Date;
            dayDTO.TimeScheduleScenarioHeadId = timeScheduleScenarioHeadId;
            dayDTO.WeekNr = CalendarUtility.GetWeekNr(date);

            dayDTO.ShiftStatus = TermGroup_TimeScheduleTemplateBlockShiftStatus.Assigned;
            dayDTO.ShiftUserStatus = TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested;

            return dayDTO;
        }
        #endregion

        #endregion

        #region Debug

        [TsIgnore]
        public string DebugInfo
        {
            get { return ($"Employee: {this.EmployeeId}, Date: {this.ActualDate.ToShortDateString()}, Time: {this.StartTime.ToShortTimeString() + " - " + this.StopTime.ToShortTimeString()}"); }
        }

        #endregion
    }

    public class TimeSchedulePlanningAggregatedDayDTO
    {
        public TimeSchedulePlanningAggregatedDayDTO(List<TimeSchedulePlanningDayDTO> dayDTOs, List<int> shiftTypeIdsFilter = null)
        {
            if (dayDTOs == null)
                dayDTOs = new List<TimeSchedulePlanningDayDTO>();

            if (shiftTypeIdsFilter != null && shiftTypeIdsFilter.Count > 0)
                dayDTOs = dayDTOs.Where(i => shiftTypeIdsFilter.Contains(i.ShiftTypeId)).ToList();

            this.DayDTOs = dayDTOs;
            this.shiftTypes = new List<ShiftTypeDTO>();

            ParseDayDTOs();
        }

        private void ParseDayDTOs()
        {
            if (!HasDayDTOs)
                return;

            int counter = 0;
            foreach (var dayDTO in DayDTOs)
            {
                //ShiftType
                this.shiftTypes.Add(new ShiftTypeDTO()
                {
                    TimeScheduleTemplateBlockType = dayDTO.Type,
                    ShiftTypeId = dayDTO.ShiftTypeId,
                    Name = dayDTO.ShiftStatusName,
                    Description = !String.IsNullOrEmpty(dayDTO.Description) ? dayDTO.Description : dayDTO.ShiftTypeDesc,
                    Color = dayDTO.ShiftTypeColor,
                    StartTime = dayDTO.StartTime,
                    StopTime = dayDTO.StopTime,
                });

                //Schedule
                this.scheduleTime += dayDTO.StopTime.Subtract(dayDTO.StartTime);
                if (counter == 0 || this.ScheduleStartTime > dayDTO.StartTime)
                    this.scheduleStartTime = dayDTO.StartTime;
                if (counter == 0 || this.ScheduleStopTime < dayDTO.StopTime)
                    this.scheduleStopTime = dayDTO.StopTime;

                //GrossNetCost
                this.grossTime = this.grossTime.Add(TimeSpan.FromMinutes(dayDTO.GrossTime));
                this.netTime = this.netTime.Add(TimeSpan.FromMinutes(dayDTO.NetTime));
                this.breakTime = this.breakTime.Add(dayDTO.BreakTime);
                this.iwhTime = this.iwhTime.Add(dayDTO.IwhTime);
                this.grossNetDiff = this.grossNetDiff.Add(dayDTO.GrossNetDiff);
                this.costPerHour += dayDTO.CostPerHour;
                this.totalCost += dayDTO.TotalCost;

                counter++;
            }

            this.isScheduleZeroDay = this.ScheduleTime.TotalMinutes == 0;

            this.scheduleBreakTime += this.Break1Minutes;
            this.scheduleBreakTime += this.Break2Minutes;
            this.scheduleBreakTime += this.Break3Minutes;
            this.scheduleBreakTime += this.Break4Minutes;
        }

        // TimeSchedulePlanningDayDTO
        public List<TimeSchedulePlanningDayDTO> DayDTOs { get; set; }
        private bool HasDayDTOs
        {
            get
            {
                return !DayDTOs.IsNullOrEmpty();
            }
        }

        // General
        public int TimeScheduleEmployeePeriodId { get; set; }
        public int? UserId
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().UserId : 0;
            }
        }
        public int EmployeeId
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().EmployeeId : 0;
            }
        }
        public string EmployeeName
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().EmployeeName : String.Empty;
            }
        }
        public string EmployeeInfo
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().EmployeeInfo : String.Empty;
            }
        }
        public string Description
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Description : String.Empty;
            }
        }
        public int WeekNr
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().WeekNr : 0;
            }
        }
        public DateTime StartTime
        {
            get
            {
                return HasDayDTOs ? DayDTOs.OrderBy(i => i.StartTime).FirstOrDefault().StartTime : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public DateTime StopTime
        {
            get
            {
                return HasDayDTOs ? DayDTOs.OrderByDescending(i => i.StopTime).FirstOrDefault().StopTime : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public DateTime Date
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().ActualDate : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public bool IsPartTimeAbsence
        {
            get
            {
                return HasDayDTOs && DayDTOs.IsPartTimeAbsence();
            }
        }
        public bool IsWholeDayAbsence
        {
            get
            {
                return HasDayDTOs && DayDTOs.IsWholeDayAbsence();
            }
        }

        public bool IamInQueue
        {
            get
            {
                return HasDayDTOs && DayDTOs.IamInQueue();
            }
        }

        public bool UnWantedShiftsExists
        {
            get
            {
                return HasDayDTOs && DayDTOs.UnWantedShiftsExists();
            }
        }

        public bool WantedShiftsExists
        {
            get
            {
                return HasDayDTOs && DayDTOs.WantedShiftsExists();
            }
        }

        public bool AbsenceRequestedShiftsExists
        {
            get
            {
                return HasDayDTOs && DayDTOs.AbsenceRequestedShiftsExists();
            }
        }

        public List<IGrouping<Guid?, TimeSchedulePlanningDayDTO>> LinkedShifts
        {
            get
            {
                return HasDayDTOs ? DayDTOs.GetLinkedShifts() : new List<IGrouping<Guid?, TimeSchedulePlanningDayDTO>>();
            }
        }

        // Net-/gross time and cost
        private TimeSpan netTime;
        public TimeSpan NetTime
        {
            get
            {
                return netTime;
            }
        }
        private TimeSpan grossTime;
        public TimeSpan GrossTime
        {
            get
            {
                return grossTime;
            }
        }
        private TimeSpan breakTime;
        public TimeSpan BreakTime
        {
            get
            {
                return breakTime;
            }
        }
        private TimeSpan iwhTime;
        public TimeSpan IwhTime
        {
            get
            {
                return iwhTime;
            }
        }
        private TimeSpan grossNetDiff;
        public TimeSpan GrossNetDiff
        {
            get
            {
                return grossNetDiff;
            }
        }
        private decimal costPerHour;
        public decimal CostPerHour
        {
            get
            {
                return costPerHour;
            }
        }
        private decimal totalCost;
        public decimal TotalCost
        {
            get
            {
                return totalCost;
            }
        }

        // Breaks
        public int Break1Id
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break1Id : 0;
            }
        }
        public int Break1TimeCodeId
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break1TimeCodeId : 0;
            }
        }
        public DateTime Break1StartTime
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break1StartTime : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public int Break1Minutes
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break1Minutes : 0;
            }
        }
        public Guid? Break1Link
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break1Link : (Guid?)null;
            }
        }
        public int Break2Id
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break2Id : 0;
            }
        }
        public int Break2TimeCodeId
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break2TimeCodeId : 0;
            }
        }
        public DateTime Break2StartTime
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break2StartTime : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public int Break2Minutes
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break2Minutes : 0;
            }
        }
        public Guid? Break2Link
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break2Link : (Guid?)null;
            }
        }
        public int Break3Id
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break3Id : 0;
            }
        }
        public int Break3TimeCodeId
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break3TimeCodeId : 0;
            }
        }
        public DateTime Break3StartTime
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break3StartTime : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public int Break3Minutes
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break3Minutes : 0;
            }
        }
        public Guid? Break3Link
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break3Link : (Guid?)null;
            }
        }
        public int Break4Id
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break4Id : 0;
            }
        }
        public int Break4TimeCodeId
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break4TimeCodeId : 0;
            }
        }
        public DateTime Break4StartTime
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break4StartTime : CalendarUtility.DATETIME_DEFAULT;
            }
        }
        public int Break4Minutes
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break4Minutes : 0;
            }
        }
        public Guid? Break4Link
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().Break4Link : (Guid?)null;
            }
        }

        // Breaks - only serverside
        // ...

        // TimeCode
        // ...

        // TimeDeviationCause
        // ...

        // Staffing
        // ...

        // Shift
        private readonly List<ShiftTypeDTO> shiftTypes = null;
        public List<ShiftTypeDTO> ShiftTypes
        {
            get
            {
                return shiftTypes;
            }
        }

        // Template schedule
        public int? NbrOfWeeks
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().NbrOfWeeks : 0;
            }
        }

        // Placement
        public int DayNumber
        {
            get
            {
                return HasDayDTOs ? DayDTOs.First().DayNumber : 0;
            }
        }

        //Schedule
        private DateTime scheduleStartTime;
        public DateTime ScheduleStartTime
        {
            get
            {
                return scheduleStartTime;
            }
        }
        private DateTime scheduleStopTime;
        public DateTime ScheduleStopTime
        {
            get
            {
                return scheduleStopTime;
            }
        }
        private TimeSpan scheduleTime;
        public TimeSpan ScheduleTime
        {
            get
            {
                return scheduleTime;
            }
        }
        private int scheduleBreakTime;
        public int ScheduleBreakTime
        {
            get
            {
                return scheduleBreakTime;
            }
        }
        private bool isScheduleZeroDay;
        public bool IsScheduleZeroDay
        {
            get
            {
                return isScheduleZeroDay;
            }
        }

        public bool HasDescription
        {
            get
            {
                return HasDayDTOs && DayDTOs.HasDescription();
            }
        }

        public bool HasOnDuty
        {
            get
            {
                return HasDayDTOs && DayDTOs.HasOnDuty();
            }
        }

        //ShiftRequest
        public bool HasShiftRequest
        {
            get
            {
                return HasDayDTOs && DayDTOs.HasShiftRequest();
            }
        }

        public bool HasShiftRequestAnswer
        {
            get
            {
                return HasDayDTOs && DayDTOs.HasShiftRequestAnswer();
            }
        }
    }

    public class TimeScheduleShiftQueueDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public DateTime Date { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeAgeDays { get; set; }
        public int EmploymentDays { get; set; }
        public int Sort { get; set; }
        public SoeEntityState State { get; set; }
        public bool? WantExtraShifts { get; set; }
    }

    public class ShiftAccountingDTO
    {
        public int? Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public string Dim2DimName { get; set; }
        public int? Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public string Dim3DimName { get; set; }
        public int? Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public string Dim4DimName { get; set; }
        public int? Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public string Dim5DimName { get; set; }
        public int? Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
        public string Dim6DimName { get; set; }
    }

    [TSInclude]
    public class ShiftAccountingRowDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }
        public string IdentityName { get; set; }
        public int? Dim2Id { get; set; }
        public string Dim2Nr { get; set; }
        public string Dim2Name { get; set; }
        public int? Dim3Id { get; set; }
        public string Dim3Nr { get; set; }
        public string Dim3Name { get; set; }
        public int? Dim4Id { get; set; }
        public string Dim4Nr { get; set; }
        public string Dim4Name { get; set; }
        public int? Dim5Id { get; set; }
        public string Dim5Nr { get; set; }
        public string Dim5Name { get; set; }
        public int? Dim6Id { get; set; }
        public string Dim6Nr { get; set; }
        public string Dim6Name { get; set; }
    }

    [TSInclude]
    public class ShiftRequestStatusDTO
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string Text { get; set; }
        public DateTime SentDate { get; set; }
        public List<ShiftRequestStatusRecipientDTO> Recipients { get; set; }
    }

    [TSInclude]
    public class ShiftRequestStatusRecipientDTO
    {
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? AnswerDate { get; set; }
        public XEMailAnswerType AnswerType { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class ShiftRequestStatusForReportDTO
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime? SentDate { get; set; }
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? ReadDate { get; set; }
        public DateTime? AnswerDate { get; set; }
        public XEMailAnswerType AnswerType { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime? ShiftStartTime { get; set; }
        public DateTime? ShiftStopTime { get; set; }
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public DateTime? ShiftCreated { get; set; }
        public string ShiftCreatedBy { get; set; }
        public DateTime? ShiftModified { get; set; }
        public string ShiftModifiedBy { get; set; }
        public SoeEntityState ShiftState { get; set; }
        public string ShiftAccountNr { get; set; }
        public string ShiftAccountName { get; set; }
    }

    public class TimeScheduleTemplateChangeDTO
    {
        public DateTime Date { get; set; }
        public string DayTypeName { get; set; }
        public bool HasAbsence { get; set; }
        public bool HasInvalidDayType { get; set; }
        public bool HasManualChanges { get; set; }
        public string ShiftsBeforeUpdate { get; set; }
        public List<EvaluateWorkRuleResultDTO> WorkRulesResults { get; set; }
        public string Warnings { get; set; }
        public bool HasWarnings
        {
            get { return !String.IsNullOrEmpty(this.Warnings); }
        }
    }

    public class TimeScheduleTemplateChangeDiffDTO
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string ShiftTypeName { get; set; }
        public string ScheduleTypeName { get; set; }
        public string DeviationCauseName { get; set; }
        public bool IsPreliminary { get; set; }
        public bool IsOnDuty { get; set; }
    }

    public class AnnualScheduledTimeSummary
    {
        public int EmployeeId { get; set; }
        public int AnnualScheduledTimeMinutes { get; set; }
        public int AnnualWorkTimeMinutes { get; set; }
    }

    public class AnnualLeaveBalance
    {
        public int EmployeeId { get; set; }
        public decimal AnnualLeaveBalanceDays { get; set; }
        public int AnnualLeaveBalanceMinutes { get; set; }
    }

    public class AnnualLeaveTransactionEarned
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Minutes { get; set; }
        public int Days { get; set; }
        public int Level { get; set; }
        public int AccumulatedMinutes { get; set; }
        public TermGroup_AnnualLeaveGroupType Type { get; set; }
    }

    public class GetEmployeePeriodTimeSummary
    {
        public List<int> EmployeeIds { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int? TimePeriodHeadId { get; set; }
    }

    public class EmployeePeriodTimeSummary
    {
        public int EmployeeId { get; set; }
        public int? ParentTimePeriodId { get; set; }
        public int ParentScheduledTimeMinutes { get; set; }
        public int ParentWorkedTimeMinutes { get; set; }
        public int ParentRuleWorkedTimeMinutes { get; set; }
        public int ParentPeriodBalanceTimeMinutes { get; set; }

        public int? ChildTimePeriodId { get; set; }
        public int ChildScheduledTimeMinutes { get; set; }
        public int ChildWorkedTimeMinutes { get; set; }
        public int ChildRuleWorkedTimeMinutes { get; set; }
        public int ChildPeriodBalanceTimeMinutes { get; set; }
        public int ParentPayrollPeriodBalanceTimeMinutes { get; set; }
        public int ChildPayrollPeriodBalanceTimeMinutes { get; set; }
        public int ParentPayrollRuleWorkedTimeMinutes { get; set; }
        public int ChildPayrollRuleWorkedTimeMinutes { get; set; }

        public string Summery
        {
            get
            {
                return $"Parent Schedule {decimal.Round(decimal.Divide(ParentWorkedTimeMinutes, 60), 2)} Work {decimal.Round(decimal.Divide(ParentScheduledTimeMinutes, 60), 2)} Rule {decimal.Round(decimal.Divide(ParentRuleWorkedTimeMinutes, 60), 2)} Balance {decimal.Round(decimal.Divide(ParentPeriodBalanceTimeMinutes, 60), 2)} " +
                    $"Child Schedule {decimal.Round(decimal.Divide(ChildWorkedTimeMinutes, 60), 2)} Work {decimal.Round(decimal.Divide(ChildScheduledTimeMinutes, 60), 2)} Rule {decimal.Round(decimal.Divide(ChildRuleWorkedTimeMinutes, 60), 2)} {decimal.Round(decimal.Divide(ChildPeriodBalanceTimeMinutes, 60), 2)}";
            }
        }
    }

    [LogAttribute]
    [TSInclude]
    public class EmployeeListDTO
    {
        [LogEmployeeId]
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeNrSort { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public TermGroup_Sex Sex { get; set; }
        [TSIgnore]
        public string ImageSource { get; set; }
        [TSIgnore]
        public byte[] Image { get; set; }

        public int AnnualWorkTimeMinutes { get; set; }
        public int AnnualScheduledTimeMinutes { get; set; }
        public int ChildRuleWorkedTimeMinutes { get; set; }
        public int ChildPeriodBalanceTimeMinutes { get; set; }
        public int ParentWorkedTimeMinutes { get; set; }
        public int ParentScheduledTimeMinutes { get; set; }
        public int ParentRuleWorkedTimeMinutes { get; set; }
        public int ParentPeriodBalanceTimeMinutes { get; set; }


        public bool Active { get; set; }
        public bool Hidden { get; set; }
        public bool Vacant { get; set; }
        public bool IsSelected { get; set; }

        // Used on client side and Excel export
        public bool IsGroupHeader { get; set; }
        public string GroupName { get; set; }

        // Employment
        public List<EmployeeListEmploymentDTO> Employments { get; set; }

        // Accounts
        public List<EmployeeAccountDTO> Accounts { get; set; }

        // Categories
        public List<CompanyCategoryRecordDTO> CategoryRecords { get; set; }

        // Skills
        public List<EmployeeSkillDTO> EmployeeSkills { get; set; }

        // TemplateSchedules
        public List<TimeScheduleTemplateHeadSmallDTO> TemplateSchedules { get; set; }

        // EmployeeSchedules
        public List<DateRangeDTO> EmployeeSchedules { get; set; }

        // Absence
        public List<DateRangeDTO> AbsenceRequest { get; set; }
        public List<DateRangeDTO> AbsenceApproved { get; set; }
        public bool HasAbsenceRequest { get; set; }
        public bool HasAbsenceApproved { get; set; }

        // Availability
        public DateTime? CurrentDate { get; set; }
        public List<EmployeeListAvailabilityDTO> Available { get; set; }
        public List<EmployeeListAvailabilityDTO> Unavailable { get; set; }
        [TsIgnore]
        [TSIgnore]
        public bool IsAvailable { get; set; }
        [TsIgnore]
        [TSIgnore]
        public bool IsPartlyAvailable { get; set; }
        [TsIgnore]
        [TSIgnore]
        public bool IsUnavailable { get; set; }
        [TsIgnore]
        [TSIgnore]
        public bool IsPartlyUnavailable { get; set; }
        [TsIgnore]
        [TSIgnore]
        public bool IsMixedAvailable { get; set; }
        [TsIgnore]
        [TSIgnore]
        public int AvailabilitySort { get; set; }

        // Staffing needs
        public int EmployeePostId { get; set; }
        public string Description { get; set; }
        public SoeEmployeePostStatus EmployeePostStatus { get; set; }

        public string Name
        {
            get { return this.FirstName + " " + this.LastName; }
        }

        public string HibernatingText { get; set; }

        // Get employment within specified period
        [TSInclude]
        public EmployeeListEmploymentDTO GetEmployment(DateTime dateFrom, DateTime dateTo)
        {
            EmployeeListEmploymentDTO result = null;

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                result = EmployeeListEmploymentDTO.GetEmployment(Employments, date);
                if (result != null)
                    break;

                date = date.AddDays(1);
            }

            return result;
        }

        public bool HasEmployment(DateTime date)
        {
            return EmployeeListEmploymentDTO.HasEmployment(Employments, date);
        }

        public bool HasAvailabilityCommentInRange(DateTime dateFrom, DateTime dateTo)
        {
            var availability = GetAvailableInRange(dateFrom, dateTo);
            availability.AddRange(GetUnavailableInRange(dateFrom, dateTo));

            return availability.Any(x => !string.IsNullOrEmpty(x.Comment));
        }

        public List<EmployeeListAvailabilityDTO> GetAvailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return Available.Where(x => CalendarUtility.IsDatesOverlapping(dateFrom.Date, dateTo.Date, x.Start.Date, x.Stop.Date)).ToList();
        }

        public bool IsAvailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return GetAvailableInRange(dateFrom, dateTo).Any();
        }

        public bool IsFullyAvailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            if (IsUnavailableInRange(dateFrom, dateTo))
                return false;

            List<EmployeeListAvailabilityDTO> availableInRange = GetAvailableInRange(dateFrom, dateTo);
            foreach (EmployeeListAvailabilityDTO range in availableInRange)
            {
                if (CalendarUtility.IsCurrentOverlappedByNew(range.Start, range.Stop, dateFrom, dateTo))
                    return true;
            }

            return false;
        }

        public bool IsPartlyAvailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return IsAvailableInRange(dateFrom, dateTo) && !IsFullyAvailableInRange(dateFrom, dateTo);

        }

        public List<EmployeeListAvailabilityDTO> GetUnavailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return Unavailable.Where(x => CalendarUtility.IsDatesOverlapping(dateFrom.Date, dateTo.Date, x.Start.Date, x.Stop.Date)).ToList();
        }

        public bool IsUnavailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return GetUnavailableInRange(dateFrom, dateTo).Any();
        }

        public bool IsFullyUnavailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            if (IsAvailableInRange(dateFrom, dateTo))
                return false;

            List<EmployeeListAvailabilityDTO> unavailableInRange = GetUnavailableInRange(dateFrom, dateTo);
            foreach (EmployeeListAvailabilityDTO range in unavailableInRange)
            {
                if (CalendarUtility.IsCurrentOverlappedByNew(range.Start, range.Stop, dateFrom, dateTo))
                    return true;
            }

            return false;
        }

        public bool IsPartlyUnavailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return this.IsUnavailableInRange(dateFrom, dateTo) && !IsFullyUnavailableInRange(dateFrom, dateTo);
        }

        public bool IsMixedAvailableInRange(DateTime dateFrom, DateTime dateTo)
        {
            return this.IsPartlyAvailableInRange(dateFrom, dateTo) && IsPartlyUnavailableInRange(dateFrom, dateTo);
        }

        public bool HasAvailabilityInRange(DateTime dateFrom, DateTime dateTo)
        {
            return (this.GetAvailableInRange(dateFrom, dateTo).Any() || GetUnavailableInRange(dateFrom, dateTo).Any());
        }
    }

    [TSInclude]
    public class EmployeeListEmploymentDTO
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool IsSecondaryEmployment { get; set; }
        public int WorkTimeWeekMinutes { get; set; }
        public decimal Percent { get; set; }
        public int MinScheduleTime { get; set; }
        public int MaxScheduleTime { get; set; }
        public int? EmployeeGroupId { get; set; }
        public string EmployeeGroupName { get; set; }
        public int BreakDayMinutesAfterMidnight { get; set; }
        public bool AllowShiftsWithoutAccount { get; set; }
        public bool IsTemporaryPrimary { get; set; }
        public bool ExtraShiftAsDefault { get; set; }
        public int? AnnualLeaveGroupId { get; set; }

        public static EmployeeListEmploymentDTO GetEmployment(List<EmployeeListEmploymentDTO> l, DateTime date)
        {
            if (l == null)
                return null;

            return (from e in l
                    where (!e.DateFrom.HasValue || CalendarUtility.GetBeginningOfDay(e.DateFrom.Value) <= date) &&
                    (!e.DateTo.HasValue || CalendarUtility.GetEndOfDay(e.DateTo.Value) >= date)
                    orderby e.DateFrom descending
                    select e).FirstOrDefault();
        }

        public static bool HasEmployment(List<EmployeeListEmploymentDTO> l, DateTime date)
        {
            if (l == null)
                return false;

            return (from e in l
                    where (!e.DateFrom.HasValue || CalendarUtility.GetBeginningOfDay(e.DateFrom.Value) <= date) &&
                    (!e.DateTo.HasValue || CalendarUtility.GetEndOfDay(e.DateTo.Value) >= date)
                    select e).Any();
        }
    }

    [TSInclude]
    public class EmployeeListAvailabilityDTO
    {
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public string Comment { get; set; }
        public int Minutes
        {
            get
            {
                return (int)this.Stop.Subtract(this.Start).TotalMinutes;
            }
        }

        public EmployeeListAvailabilityDTO()
        {
        }

        public EmployeeListAvailabilityDTO(DateTime start, DateTime stop, string comment = null)
        {
            Start = start;
            Stop = stop;
            Comment = comment;
        }
    }

    public class EmployeeListSmallDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeNrSort { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Hidden { get; set; }
        public bool Vacant { get; set; }
        public int? UserId { get; set; }

        // Accounts
        public List<EmployeeAccountDTO> Accounts { get; set; }

        public string Name
        {
            get { return this.FirstName + " " + this.LastName; }
        }
    }

    public class TemplateScheduleEmployeeDTO
    {
        #region Propertys

        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeNrSort { get; set; }
        public string Name { get; set; }
        public string CurrentTemplate { get; set; }
        public int CurrentTemplateNbrOfWeeks { get; set; }
        public DateTime? TemplateStartDate { get; set; }
        public DateTime? TemplateStopDate { get; set; }
        public int NbrOfWeeks { get; set; }
        public int CopyFromEmployeeId { get; set; }
        public int CopyFromTemplateHeadId { get; set; }
        public bool IsRunning { get; set; }
        public bool? ResultSuccess { get; set; }
        public string ResultError { get; set; }
        public bool IsSelected { get; set; }

        #endregion
    }

    public class OrderListDTO
    {
        public int OrderId { get; set; }
        public int? OrderNr { get; set; }

        public int CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }

        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }

        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }

        public int? Priority { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedStopDate { get; set; }
        public int EstimatedTime { get; set; }
        public int RemainingTime { get; set; }
        public bool KeepAsPlanned { get; set; }
        public string WorkingDescription { get; set; }
        public string InternalDescription { get; set; }

        [TsIgnore]
        public Dictionary<int, string> Categories { get; set; }
        public string DeliveryAddress { get; set; }
        [TsIgnore]
        public int? DeliveryAddressId { get; set; }
        [TsIgnore]
        public string InvoiceHeadText { get; set; }

    }

    public class OrderShiftDTO
    {
        public int TimeScheduleTemplateBlockId { get; set; }

        // Grid values
        public DateTime? Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string EmployeeName { get; set; }
        public string ShiftTypeName { get; set; }

        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }

    }

    public class BreakDTO
    {
        public int Id { get; set; }
        public int TimeCodeId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime
        {
            get
            {
                return this.StartTime.AddMinutes(this.BreakMinutes);
            }
        }
        public int BreakMinutes { get; set; }
        public Guid? Link { get; set; }
        public bool IsPreliminary { get; set; }
        public int? AccountId { get; set; } //Not always set
        public int OffsetDaysOnCopy { get; set; }
    }

    public class PlannedAbsenceIntervalDTO
    {
        public PlannedAbsenceIntervalDTO() { TimeScheduleTemplateBlocks = new List<TimeScheduleTemplateBlockDTO>(); }
        public int TimeDeviationCauseId { get; set; }
        public DateTime ActualStartTime { get; set; }
        public DateTime ActualStopTime { get; set; }
        public List<TimeScheduleTemplateBlockDTO> TimeScheduleTemplateBlocks { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        //internal
        public bool Delete { get; set; }
    }

    [TSInclude]
    public class ShiftDTO
    {
        // Only used n new Angular
        public TermGroup_TimeScheduleTemplateBlockType Type { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int TimeScheduleEmployeePeriodId { get; set; }

        public int TimeCodeId { get; set; }
        public string Description { get; set; }

        // Dates
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public DateTime AbsenceStartTime { get; set; }
        public DateTime AbsenceStopTime { get; set; }
        public bool BelongsToPreviousDay { get; set; }
        public bool BelongsToNextDay { get; set; }

        // Schedule type
        public int TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeCode { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public bool TimeScheduleTypeIsNotScheduleTime { get; set; }
        public List<TimeScheduleTypeFactorSmallDTO> TimeScheduleTypeFactors { get; set; }
        public int ShiftTypeTimeScheduleTypeId { get; set; }
        public string ShiftTypeTimeScheduleTypeCode { get; set; }
        public string ShiftTypeTimeScheduleTypeName { get; set; }

        // Employee
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        //public int? EmployeePostId { get; set; }
        //public int? EmployeeChildId { get; set; }

        // Time and cost
        public int GrossTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalCostIncEmpTaxAndSuppCharge { get; set; }

        // Breaks

        public List<ShiftBreakDTO> Breaks { get; set; }

        // Deviation cause
        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }

        // Shift type
        public int ShiftTypeId { get; set; }
        public string ShiftTypeCode { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeDesc { get; set; }
        public string ShiftTypeColor { get; set; }

        // Statuses
        public TermGroup_TimeScheduleTemplateBlockShiftStatus ShiftStatus { get; set; }
        //public string ShiftStatusName { get; set; }
        public TermGroup_TimeScheduleTemplateBlockShiftUserStatus ShiftUserStatus { get; set; }
        //public string ShiftUserStatusName { get; set; }

        // Flags
        public bool IsPreliminary { get; set; }
        public bool ExtraShift { get; set; }
        public bool SubstituteShift { get; set; }
        public bool IsDeleted { get; set; }

        // Accounting
        public int? AccountId { get; set; }
        public string AccountName { get; set; }
        //public bool HasMultipleEmployeeAccountsOnDate { get; set; }

        // Queue
        public int NbrOfWantedInQueue { get; set; }
        //public bool IamInQueue { get; set; }

        // Swap request
        //public bool HasSwapRequest { get; set; }
        //public string SwapShiftInfo { get; set; }

        // Shift request
        //public bool HasShiftRequest { get; set; }
        //public XEMailAnswerType ShiftRequestAnswerType { get; set; }
        public int ApprovalTypeId { get; set; }

        // Template
        public int? TimeScheduleTemplateHeadId { get; set; }
        //public int? TimeScheduleTemplatePeriodId { get; set; }
        public int NbrOfWeeks { get; set; }
        public int DayNumber { get; set; }
        public int OriginalBlockId { get; set; }

        // Link
        public Guid? Link { get; set; }

        // Absence request
        public bool IsAbsenceRequest { get; set; }

        // LeisureCode
        public int? TimeLeisureCodeId { get; set; }

        // Staffing needs
        //public int? StaffingNeedsRowId { get; set; }

        // Order planning
        //public int TimeScheduleEmployeePeriodId { get; set; }
        //public OrderListDTO Order { get; set; }
        //public int? PlannedTime { get; set; }

        // Scenario
        //public int? TimeScheduleScenarioHeadId { get; set; }

        // Extensions
        //public int Index { get; set; }
        //public string Label1 { get; set; }
        //public string Label2 { get; set; }
        //public string ToolTip { get; set; }
        //public string AvailabilityToolTip { get; set; }
        //public DateTime Date { get; set; }
        //public DateTime ActualDateOnLoad { get; set; }
        //public DateTime ActualStartTime { get; set; }
        //public DateTime ActualStopTime { get; set; }
        //public int Duration { get; set; }
        //public DateTime ActualStartTimeDuringMove { get; set; }
        //public DateTime ActualStopTimeDuringMove { get; set; }
        //public bool IsBreak { get; set; }
        //public bool IsVisible { get; set; }
        //public bool IsReadOnly { get; set; }
        //public bool IsLended { get; set; }
        //public bool IsOtherAccount { get; set; }
        //public bool Selected { get; set; }
        //public bool Highlighted { get; set; }
        //public TermGroup_TimeSchedulePlanningShiftStyle ShiftStyle { get; set; }
        //public List<TimeScheduleTemplateBlockTaskDTO> Tasks { get; set; }
        //public List<ShiftTypeDTO> ShiftTypes { get; set; }
        //public int TimeScheduleEmployeePeriodDetailId { get; set; }
        //public int TimeLeisureCodeId { get; set; }
    }

    [TSInclude]
    public class ShiftBreakDTO
    {
        public int BreakId { get; set; }
        public int TimeCodeId { get; set; }
        public DateTime? StartTime { get; set; }
        public bool BelongsToPreviousDay { get; set; }
        public bool BelongsToNextDay { get; set; }
        public int Minutes { get; set; }
        public Guid? Link { get; set; }
        public bool IsPreliminary { get; set; }
    }
}
