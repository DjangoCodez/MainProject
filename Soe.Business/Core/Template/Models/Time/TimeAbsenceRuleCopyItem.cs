using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeAbsenceRuleCopyItem
    {
        public int TimeAbsenceRuleId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? TimeCodeId { get; set; }
        public List<TimeAbsenceRuleRowCopyItem> TimeAbsenceRuleRows { get; set; } = new List<TimeAbsenceRuleRowCopyItem>();
        public List<TimeAbsenceRuleHeadEmployeeGroupCopyItem> TimeAbsenceRuleHeadEmployeeGroupCopyItems { get; set; } = new List<TimeAbsenceRuleHeadEmployeeGroupCopyItem>();
    }

    public class TimeAbsenceRuleRowCopyItem
    {
        public bool HasMultiplePayrollProducts { get; set; }
        public int Type { get; set; }
        public int Scope { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }
        public int? PayrollProductId { get; set; }
        public List<TimeAbsenceRuleRowPayrollProductsCopyItem> TimeAbsenceRuleRowPayrollProducts { get; set; } = new List<TimeAbsenceRuleRowPayrollProductsCopyItem>();
    }

    public class TimeAbsenceRuleRowPayrollProductsCopyItem
    {
        public int SourcePayrollProductId { get; set; }
        public int? TargetPayrollProductId { get; set; }
    }

    public class TimeAbsenceRuleHeadEmployeeGroupCopyItem
    {
        public int EmployeeGroupId { get; set; }
    }
}
