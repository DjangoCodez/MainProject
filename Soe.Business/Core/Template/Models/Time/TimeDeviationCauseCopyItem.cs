namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeDeviationCauseCopyItem
    {
        public int TimeDeviationCauseId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExtCode { get; set; }
        public string ImageSource { get; set; }
        public int EmployeeRequestPolicyNbrOfDaysBefore { get; set; }
        public bool EmployeeRequestPolicyNbrOfDaysBeforeCanOverride { get; set; }
        public int AttachZeroDaysNbrOfDaysBefore { get; set; }
        public int AttachZeroDaysNbrOfDaysAfter { get; set; }
        public bool ChangeDeviationCauseAccordingToPlannedAbsence { get; set; }
        public int ChangeCauseOutsideOfPlannedAbsence { get; set; }
        public int ChangeCauseInsideOfPlannedAbsence { get; set; }
        public int AdjustTimeOutsideOfPlannedAbsence { get; set; }
        public int AdjustTimeInsideOfPlannedAbsence { get; set; }
        public bool AllowGapToPlannedAbsence { get; set; }
        public bool ShowZeroDaysInAbsencePlanning { get; set; }
        public bool IsVacation { get; set; }
        public bool Payed { get; set; }
        public bool NotChargeable { get; set; }
        public bool OnlyWholeDay { get; set; }
        public bool SpecifyChild { get; set; }
        public bool ExcludeFromPresenceWorkRules { get; set; }
        public bool ExcludeFromScheduleWorkRules { get; set; }
        public bool ValidForHibernating { get; set; }
        public bool ValidForStandby { get; set; }
        public bool CandidateForOvertime { get; set; }
        public bool MandatoryNote { get; set; }
        public bool MandatoryTime { get; set; }
        public int State { get; set; }
        public int? TimeCodeId { get; set; }
        public bool CalculateAsOtherTimeInSales { get; set; }
    }
}
