using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class EmployeeGroupCopyItem
    {
        public int EmployeeGroupId { get; set; }
        public int? TimeCodeId { get; set; }
        public string Name { get; set; }
        public int DeviationAxelStartHours { get; set; }
        public int DeviationAxelStopHours { get; set; }
        public string PayrollProductAccountingPrio { get; set; }
        public string InvoiceProductAccountingPrio { get; set; }
        public bool AutogenTimeblocks { get; set; }
        public bool AutogenBreakOnStamping { get; set; }
        public bool MergeScheduleBreaksOnDay { get; set; }
        public int BreakDayMinutesAfterMidnight { get; set; }
        public int KeepStampsTogetherWithinMinutes { get; set; }
        public int RuleWorkTimeWeek { get; set; }
        public int RuleRestTimeDay { get; set; }
        public int RuleRestTimeWeek { get; set; }
        public bool AlwaysDiscardBreakEvaluation { get; set; }
        public int? ReminderAttestStateId { get; set; }
        public int? ReminderNoOfDays { get; set; }
        public int? ReminderPeriodType { get; set; }
        public int RuleWorkTimeYear2014 { get; set; }
        public int RuleWorkTimeYear2015 { get; set; }
        public int RuleWorkTimeYear2016 { get; set; }
        public int RuleWorkTimeYear2017 { get; set; }
        public int RuleWorkTimeYear2018 { get; set; }
        public int RuleWorkTimeYear2019 { get; set; }
        public int RuleWorkTimeYear2020 { get; set; }
        public int RuleWorkTimeYear2021 { get; set; }
        public int MaxScheduleTimeFullTime { get; set; }
        public int MinScheduleTimeFullTime { get; set; }
        public int MaxScheduleTimePartTime { get; set; }
        public int MinScheduleTimePartTime { get; set; }
        public int MaxScheduleTimeWithoutBreaks { get; set; }
        public int RuleWorkTimeDayMinimum { get; set; }
        public int RuleWorkTimeDayMaximumWorkDay { get; set; }
        public int RuleWorkTimeDayMaximumWeekend { get; set; }
        public int TimeReportType { get; set; }
        public int QualifyingDayCalculationRule { get; set; }
        public bool QualifyingDayCalculationRuleLimitFirstDay { get; set; }
        public int TimeWorkReductionCalculationRule { get; set; }
        public bool AutoGenTimeAndBreakForProject { get; set; }
        public int BreakRoundingUp { get; set; }
        public int BreakRoundingDown { get; set; }
        public bool NotifyChangeOfDeviations { get; set; }
        public bool RuleRestDayIncludePresence { get; set; }
        public bool RuleRestWeekIncludePresence { get; set; }
        public bool AllowShiftsWithoutAccount { get; set; }
        public bool AlsoAttestAdditionsFromTime { get; set; }
        public int RuleScheduleFreeWeekendsMinimumYear { get; set; }
        public int RuleScheduledDaysMaximumWeek { get; set; }
        public bool CandidateForOvertimeOnZeroDayExcluded { get; set; }
        public int? RuleRestTimeWeekStartDayNumber { get; set; }
        public DateTime? RuleRestTimeDayStartTime { get; set; }
        public bool ExtraShiftAsDefault { get; set; }
        public int State { get; set; }
        public int? DefaultTimeDeviationCauseId { get; set; }
        public string ExternalCodesString { get; set; }
        public string SwapShiftToShorterText { get; set; }
        public string SwapShiftToLongerText { get; set; }

        public List<DayTypeCopyItem> DayTypeCopyItems { get; set; } = new List<DayTypeCopyItem>();
        public List<EmployeeGroupTimeCodeBreakCopyItem> TimeCodeBreakCopyItems { get; set; } = new List<EmployeeGroupTimeCodeBreakCopyItem>();
        public List<EmployeeGroupTimeCodeCopyItem> TimeCodeCopyItems { get; set; } = new List<EmployeeGroupTimeCodeCopyItem>();
        public List<EmployeeGroupAttestTransitionCopyItem> AttestTransitionCopyItems { get; set; } = new List<EmployeeGroupAttestTransitionCopyItem>();
        public List<EmployeeGroupTimeStampRoundingCopyItem> TimeStampRoundingCopyItems { get; set; } = new List<EmployeeGroupTimeStampRoundingCopyItem>();
        public List<EmployeeGroupTimeDeviationCauseCopyItem> TimeDeviationCauseCopyItems { get; set; } = new List<EmployeeGroupTimeDeviationCauseCopyItem>();
        public List<EmployeeGroupTimeDeviationCauseTimeCodeCopyItem> TimeDeviationCauseTimeCodeCopyItems { get; set; } = new List<EmployeeGroupTimeDeviationCauseTimeCodeCopyItem>();
        public List<EmployeeGroupTimeDeviationCauseAbsenceAnnouncementCopyItem> TimeDeviationCauseAbsenceAnnouncementCopyItems { get; set; } = new List<EmployeeGroupTimeDeviationCauseAbsenceAnnouncementCopyItem>();
        public List<EmployeeGroupTimeDeviationCauseRequestCopyItem> TimeDeviationCauseCopyItemsRequest { get; set; } = new List<EmployeeGroupTimeDeviationCauseRequestCopyItem>();
    }

    public class EmployeeGroupTimeDeviationCauseCopyItem
    {
        public int TimeDeviationCauseId { get; set; }
        public bool UseInTimeTerminal { get; set; }
    }

    public class EmployeeGroupTimeDeviationCauseTimeCodeCopyItem
    {
        public int TimeDeviationCauseId { get; set; }
        public int TimeCodeId { get; set; }
    }

    public class EmployeeGroupTimeDeviationCauseAbsenceAnnouncementCopyItem
    {
        public int TimeDeviationCauseId { get; set; }
    }

    public class EmployeeGroupTimeDeviationCauseRequestCopyItem
    {
        public int TimeDeviationCauseId { get; set; }
    }

    public class EmployeeGroupTimeCodeBreakCopyItem
    {
        public int TimeCodeId { get; set; }
    }

    public class EmployeeGroupTimeCodeCopyItem
    {
        public int TimeCodeId { get; set; }
    }

    public class EmployeeGroupAttestTransitionCopyItem
    {
        public int AttestTransitionId { get; set; }
    }

    public class EmployeeGroupTimeStampRoundingCopyItem
    {
        public int RoundInNeg { get; set; }
        public int RoundInPos { get; set; }
        public int RoundOutNeg { get; set; }
        public int RoundOutPos { get; set; }
    }
}
