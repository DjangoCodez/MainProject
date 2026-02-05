using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class ScheduleCycleCopyItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int NbrOfWeeks { get; set; }
        public List<ScheduleCycleRuleCopyItem> ScheduleCycleRules { get; set; } = new List<ScheduleCycleRuleCopyItem>();
    }

    public class ScheduleCycleRuleCopyItem
    {
        public int MaxOccurrences { get; set; }
        public int MinOccurrences { get; set; }
        public ScheduleCycleRuleTypeCopyItem ScheduleCycleRuleType { get; set; } = new ScheduleCycleRuleTypeCopyItem();
    }

    public class ScheduleCycleRuleTypeCopyItem
    {
        public string Name { get; set; }
        public string DayOfWeeks { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
    }
}
