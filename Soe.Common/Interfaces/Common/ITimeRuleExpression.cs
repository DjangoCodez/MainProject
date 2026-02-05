namespace SoftOne.Soe.Common.Interfaces.Common
{
    public interface ITimeRuleExpression
    {
        int TimeRuleExpressionId { get; set; }
        int TimeRuleId { get; set; }
        bool IsStart { get; set; }
    }
}
