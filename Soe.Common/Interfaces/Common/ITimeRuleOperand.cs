namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface ITimeRuleOperand
    {
        int TimeRuleOperandId { get; set; }
        int TimeRuleExpressionId { get; set; }
        int? TimeRuleExpressionRecursiveId { get; set; }
        int? LeftValueId { get; set; }
        int? RightValueId { get; set; }
        int Minutes { get; set; }
        int OrderNbr { get; set; }
    }
}
