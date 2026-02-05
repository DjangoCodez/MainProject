using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Business.Util
{
    #region Enums
    public enum RuleOperatorType
    {
        addition = 1,
        subtract = 2,
        multiplication = 3,
        division = 4,
    }
    public enum ComplexRuleType
    {
        none = 1,
        leftSide = 2,
        rightSide = 3,
        both = 4,
    }
    #endregion

    #region Rules

    public class Rule
    {
        #region Variables

        public ComplexRuleType RuleComposition { get; set; }
        public RuleOperator RuleOperator { get; set; }

        public Rule LeftHandSideRule { get; set; }
        public Rule RightHandSideRule { get; set; }

        public RuleValue LeftHandSideValue { get; set; }
        public RuleValue RightHandSideValue { get; set; }

        public bool IsCondition { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// (rule(s))op(rule(s)) ex : ((1+2)/((1*2)*(10-2)))
        /// Rules are recursive
        /// </summary>
        /// <param name="leftHand"></param>
        /// <param name="ruleOperator"></param>
        /// <param name="rightHand"></param>
        public Rule(Rule leftHand, RuleOperatorType ruleOperator, Rule rightHand)
        {
            IsCondition = false;
            RuleOperator = new RuleOperator(ruleOperator);
            RuleComposition = ComplexRuleType.both;
            LeftHandSideRule = leftHand;
            RightHandSideRule = rightHand;
        }

        /// <summary>
        /// (rule(s))op(value) ex : ((1+2)/2)
        /// Rules are recursive
        /// </summary>
        /// <param name="leftHand"></param>
        /// <param name="ruleOperator"></param>
        /// <param name="rightHand"></param>
        public Rule(RuleValue leftHand, RuleOperatorType ruleOperator, Rule rightHand)
        {
            IsCondition = false;
            RuleOperator = new RuleOperator(ruleOperator);
            RuleComposition = ComplexRuleType.rightSide;
            LeftHandSideValue = leftHand;
            RightHandSideRule = rightHand;
        }

        /// <summary>
        /// (value)op(rule(s)) ex : (2/(1+2))
        /// Rules are recursive
        /// </summary>
        /// <param name="leftHand"></param>
        /// <param name="ruleOperator"></param>
        /// <param name="rightHand"></param>
        public Rule(Rule leftHand, RuleOperatorType ruleOperator, RuleValue rightHand)
        {
            IsCondition = false;
            RuleOperator = new RuleOperator(ruleOperator);
            RuleComposition = ComplexRuleType.leftSide;
            LeftHandSideRule = leftHand;
            RightHandSideValue = rightHand;
        }

        /// <summary>
        /// (value)op(value) ex : (1+2)
        /// </summary>
        /// <param name="leftHand"></param>
        /// <param name="ruleOperator"></param>
        /// <param name="rightHand"></param>
        public Rule(RuleValue leftHand, RuleOperatorType ruleOperator, RuleValue rightHand)
        {
            IsCondition = false;
            RuleOperator = new RuleOperator(ruleOperator);
            RuleComposition = ComplexRuleType.none;
            LeftHandSideValue = leftHand;
            RightHandSideValue = rightHand;
        }

        #endregion

        #region Public methods

        public RuleValue ApplyRule()
        {
            switch (RuleComposition)
            {
                case ComplexRuleType.leftSide:
                    LeftHandSideValue = LeftHandSideRule.ApplyRule();
                    break;
                case ComplexRuleType.rightSide:
                    RightHandSideValue = RightHandSideRule.ApplyRule();
                    break;
                case ComplexRuleType.both:
                    LeftHandSideValue = LeftHandSideRule.ApplyRule();
                    RightHandSideValue = RightHandSideRule.ApplyRule();
                    break;
            }
            return RuleOperator.ApplyOperator(LeftHandSideValue, RightHandSideValue);
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool asFormula)
        {
            string result = string.Empty;
            switch ((int)RuleComposition)
            {
                case (int)ComplexRuleType.both:
                    result = " ( " + LeftHandSideRule.ToString(asFormula) + " " + RuleOperator.ToString() + " " + RightHandSideRule.ToString(asFormula) + " ) ";
                    break;
                case (int)ComplexRuleType.leftSide:
                    result = " ( " + LeftHandSideRule.ToString(asFormula) + " " + RuleOperator.ToString() + " " + RightHandSideValue.ToString(asFormula) + " ) ";
                    break;
                case (int)ComplexRuleType.rightSide:
                    result = " ( " + LeftHandSideValue.ToString(asFormula) + " " + RuleOperator.ToString() + " " + RightHandSideRule.ToString(asFormula) + " ) ";
                    break;
                case (int)ComplexRuleType.none:
                    result = " ( " + LeftHandSideValue.ToString(asFormula) + " " + RuleOperator.ToString() + " " + RightHandSideValue.ToString(asFormula) + " ) ";
                    break;
            }
            return result;
        }

        #endregion
    }

    #endregion

    #region Operator

    public class RuleOperator
    {
        #region Variables

        public RuleOperatorType OperatorType { get; set; }
        public string DisplayName { get; set; }

        #endregion

        #region Ctor

        public RuleOperator(RuleOperatorType type)
        {
            OperatorType = type;
            SetDisplayName();
        }

        #endregion

        #region Public methods

        public void SetDisplayName()
        {
            switch (OperatorType)
            {
                case RuleOperatorType.addition:
                    DisplayName = @"+";
                    break;
                case RuleOperatorType.multiplication:
                    DisplayName = @"*";
                    break;
                case RuleOperatorType.division:
                    DisplayName = @"/";
                    break;
                case RuleOperatorType.subtract:
                    DisplayName = @"-";
                    break;
            }
        }

        public RuleValue ApplyOperator(RuleValue lValue, RuleValue rValue)
        {
            RuleValue result = new RuleValue(0, "resultat");
            switch (OperatorType)
            {
                case RuleOperatorType.addition:
                    result.Value = lValue.Value + rValue.Value;
                    break;
                case RuleOperatorType.multiplication:
                    result.Value = rValue.Value * lValue.Value;
                    break;
                case RuleOperatorType.division:
                    result.Value = lValue.Value / rValue.Value;
                    break;
                case RuleOperatorType.subtract:
                    result.Value = lValue.Value - rValue.Value;
                    break;
            }

            return result;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }

    #endregion

    #region Value

    public class RuleValue
    {
        #region Variables

        public string DefaultText { get; set; }
        public decimal Value { get; set; }

        #endregion

        #region Ctor

        public RuleValue(decimal value)
        {
            Value = value;
        }

        public RuleValue(decimal value, string name)
        {
            Value = value;
            DefaultText = name;
        }

        #endregion

        #region Public methods

        public string ToString(bool asFormula)
        {
            if (asFormula)
            {
                return DefaultText;
            }
            return ToString();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion
    }

    #endregion

    public class RuleResult
    {
        #region Variables

        public int CompanyWholesellerPriceListId { get; set; }
        public int SysProductId { get; set; }
        public decimal Value { get; set; }
        public decimal NetPrice { get; set; }
        public PriceRuleValueType Type { get; set; }
        public string Formula { get; set; }
        public int SysWholesellerId { get; set; }
        public bool NetPriceFromNetPriceList { get; set; }

        #endregion

        #region Ctor

        public RuleResult() { }

        public RuleResult(decimal value)
        {
            Value = value;
        }

        public RuleResult(decimal value, decimal netPrice)
        {
            Value = value;
            NetPrice = netPrice;
            Formula = string.Empty;
        }

        public RuleResult(decimal value, decimal netPrice, PriceRuleValueType type)
        {
            Value = value;
            NetPrice = netPrice;
            Type = type;
        }

        #endregion
    }
}
