using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class TimeRuleCopyItem
    {
        public int TimeCodeId { get; set; }
        public int TimeRuleId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public int RuleStartDirection { get; set; }
        public int RuleStopDirection { get; set; }
        public decimal Factor { get; set; }
        public bool BelongsToGroup { get; set; }
        public bool IsInconvenientWorkHours { get; set; }
        public int? TimeCodeMaxLength { get; set; }
        public bool TimeCodeMaxPerDay { get; set; }
        public int Sort { get; set; }
        public bool Internal { get; set; }
        public int? StandardMinutes { get; set; }
        public bool BreakIfAnyFailed { get; set; }
        public bool AdjustStartToTimeBlockStart { get; set; }
        public List<TimeRuleRowCopyItem> TimeRuleRows { get; set; } = new List<TimeRuleRowCopyItem>();
        public List<TimeRuleExpressionCopyItem> TimeRuleExpressions { get; set; } = new List<TimeRuleExpressionCopyItem>();
    }

    public class TimeRuleRowCopyItem
    {
        public int TimeDeviationCauseId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public int? DayTypeId { get; set; }
    }

    public class TimeRuleExpressionCopyItem
    {
        public bool IsStart { get; set; }
        public List<TimeRuleOperandCopyItem> Operands { get; set; } = new List<TimeRuleOperandCopyItem>();
    }

    public class TimeRuleOperandCopyItem
    {
        public int OperatorType { get; set; }
        public int? LeftValueType { get; set; }
        public int? RightValueType { get; set; }
        public int Minutes { get; set; }
        public int? ComparisonOperator { get; set; }
        public int OrderNbr { get; set; }
        public int? LeftValueId { get; set; }
        public int? RightValueId { get; set; }
    }
}
