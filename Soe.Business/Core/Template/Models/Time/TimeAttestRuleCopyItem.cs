using SoftOne.Soe.Common.Util;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeAttestRuleCopyItem
    {

        public int AttestRuleId { get; set; }
        public SoeModule Module { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? DayTypeId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public List<TimeAttestRuleRowCopyItem> AttestRuleRows { get; set; } = new List<TimeAttestRuleRowCopyItem>();
        public List<EmployeeGroupCopyItem> EmployeeGroups { get; set; } = new List<EmployeeGroupCopyItem>();
    }

    public class TimeAttestRuleRowCopyItem
    {
        public int LeftValueType { get; set; }
        public int LeftValueId { get; set; }
        public int ComparisonOperator { get; set; }
        public int RightValueType { get; set; }
        public int RightValueId { get; set; }
        public int Minutes { get; set; }
    }

}
