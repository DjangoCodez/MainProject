using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    public class EmployeeAccumulatorDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }

        public int AccumulatorId { get; set; }
        public string AccumulatorName { get; set; }
        public decimal AccumulatorAccTodayValue { get; set; }
        public string AccumulatorAccTodayDates { get; set; }
        public decimal AccumulatorPeriodValue { get; set; }
        public string AccumulatorPeriodDates { get; set; }
        public decimal? AccumulatorAmount { get; set; }
        public int? AccumulatorRuleMinMinutes { get; set; }
        public int? AccumulatorRuleMinWarningMinutes { get; set; }
        public int? AccumulatorRuleMaxMinutes { get; set; }
        public int? AccumulatorRuleMaxWarningMinutes { get; set; }
        public int? AccumulatorDiff { get; set; }
        public SoeTimeAccumulatorComparison AccumulatorStatus { get; set; }
        public string AccumulatorStatusName { get; set; }
        public bool AccumulatorShowError { get; set; }
        public bool AccumulatorShowWarning { get; set; }

        public int? OwnLimitMin { get; set; }
        public int? OwnLimitMax { get; set; }
        public int? OwnLimitDiff { get; set; }
        public SoeTimeAccumulatorComparison OwnLimitStatus { get; set; }
        public string OwnLimitStatusName { get; set; }
        public bool OwnLimitShowError { get; set; }
    }
}
