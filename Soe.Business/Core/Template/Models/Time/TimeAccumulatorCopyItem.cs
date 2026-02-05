using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeAccumulatorCopyItem
    {
        public int TimeAccumulatorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool ShowInTimeReports { get; set; }
        public int Type { get; set; }
        public bool FinalSalary { get; set; }
        public bool UseTimeWorkAccount { get; set; }
        public bool UseTimeWorkReductionWithdrawal { get; set; }
        public int? TimeCodeId { get; set; }
        public TimeWorkReductionEarningCopyItem TimeWorkReductionEarning { get; set; }

        public List<TimeAccumulatorEmployeeGroupRuleCopyItem> EmployeeGroupRules { get; set; }
        public List<TimeAccumulatorTimeCodeCopyItem> TimeCodes { get; set; }
        public List<TimeAccumulatorPayrollProductCopyItem> PayrollProducts { get; set; }
        public List<TimeAccumulatorInvoiceProductCopyItem> InvoiceProducts { get; set; }
    }

    public class TimeWorkReductionEarningCopyItem
    {
        public int TimeWorkReductionEarningId { get; set; }
        public int MinutesWeight { get; set; }
        public int PeriodType { get; set; }

        public List<TimeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem> TimeAccumulatorTimeWorkReductionEarningEmployeeGroup { get; set; }
    }

    public class TimeAccumulatorTimeWorkReductionEarningEmployeeGroupCopyItem
    {
        public int TimeAccumulatorTimeWorkReductionEarningEmployeeGroupId { get; set; }
        public int TimeWorkReductionEarningId { get; set; }
        public int EmployeeGroupId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class TimeAccumulatorEmployeeGroupRuleCopyItem
    {
        public int EmployeeGroupId { get; set; }
        public int Type { get; set; }
        public int? MinMinutes { get; set; }
        public int? MaxMinutes { get; set; }
        public int? MaxMinutesWarning { get; set; }
        public int? MinMinutesWarning { get; set; }
        public int? MinTimeCodeId { get; set; }
        public int? MaxTimeCodeId { get; set; }
        public bool ShowOnPayrollSlip { get; set; }
        public int? ThresholdMinutes { get; set; }
    }

    public class TimeAccumulatorTimeCodeCopyItem
    {
        public int TimeCodeId { get; set; }
        public decimal Factor { get; set; }
        public bool IsHeadTimeCode { get; set; }
        public bool ImportDefault { get; set; }
    }

    public class TimeAccumulatorPayrollProductCopyItem
    {
        public int PayrollProductId { get; set; }
        public decimal Factor { get; set; }
    }

    public class TimeAccumulatorInvoiceProductCopyItem
    {
        public int InvoiceProductId { get; set; }
        public decimal Factor { get; set; }
    }
}
