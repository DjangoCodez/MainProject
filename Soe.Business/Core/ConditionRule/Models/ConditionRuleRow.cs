using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Core.ConditionRule.Models
{
    public class ConditionRuleDTO
    {
        public int ConditionRuleId { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ConditionRuleTriggerContainer Triggers { get; set; }
        public ConditionRuleContainer Conditions { get; set; }
    }

    public class ConditionRuleContainer
    {
        public List<ConditionRuleRow> Rows { get; set; } = new List<ConditionRuleRow>();
    }

    public class ConditionRuleRow
    {
        public List<ConditionRuleRowValue> Values { get; set; } = new List<ConditionRuleRowValue>();
        public int Sort { get; set; }
        public string Expression { get; set; }
        public ConditionRuleRowType RowType { get; set; }
        public SoeEntityType EntityType { get; set; }

    }

    public enum ConditionRuleRowType
    {
        Unknown = 0,
        Operator = 1, // "&&", "||" "(" ")" ">" "<" etc.
        Contains = 11,
        ContainsAll = 12,
        DoNotContainsAny = 14,
        DoNotContainsAll = 15,
    }

    public class ConditionRuleRowValue
    {
        public string Value { get; set; }
        public int? RecordId { get; set; }
    }

    public class ConditionRuleTriggerContainer
    {
        public List<ConditionRuleTrigger> Triggers { get; set; } = new List<ConditionRuleTrigger>();
    }

    public class ConditionRuleTrigger
    {
        public ConditionRuleTriggerType TriggerType { get; set; }
    }

    public enum ConditionRuleTriggerType
    {
        Unknown = 0,
        TimeRule = 1,
    }

    public static class ConditionRuleExtensions
    {
        public static string GetExpression(this ConditionRuleRow row)
        {
            if (row.RowType == ConditionRuleRowType.Operator)
            {
                return row.Expression;
            }
            else
            {
                return string.Join(" ", row.Values.Select(x => x.Value));
            }
        }

        public static string ConcatenateExpression(this ConditionRuleContainer cotainer)
        {
            // Sort be sort order
            var rows = cotainer.Rows.OrderBy(r => r.Sort).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var item in rows)
            {
                switch (item.RowType)
                {
                    case ConditionRuleRowType.Operator:
                        sb.Append(item.Expression);
                        break;
                    case ConditionRuleRowType.Contains:
                        sb.Append("Contains(" + string.Join(",", item.Values.Select(x => x.Value)) + ")");
                        break;
                    case ConditionRuleRowType.ContainsAll:
                        sb.Append("ContainsAll(" + string.Join(",", item.Values.Select(x => x.Value)) + ")");
                        break;
                    case ConditionRuleRowType.DoNotContainsAny:
                        sb.Append("!Contains(" + string.Join(",", item.Values.Select(x => x.Value)) + ")");
                        break;
                    case ConditionRuleRowType.DoNotContainsAll:
                        sb.Append("!ContainsAll(" + string.Join(",", item.Values.Select(x => x.Value)) + ")");
                        break;
                    default:
                        break;
                }
            }

            return sb.ToString();
        }

        public static ConditionRuleContainer CreateConditionRuleContainer(string expression)
        {
            var container = new ConditionRuleContainer();
            var regex = new Regex(@"(Contains\((.*?)\))|(ContainsAll\((.*?)\))|(!Contains\((.*?)\))|(!ContainsAll\((.*?)\))|(\(|\)|&&|\|\|)");

            var matches = regex.Matches(expression);

            foreach (Match match in matches)
            {
                var row = CreateConditionRuleRow(match.Value);
                container.Rows.Add(row);
            }

            return container;
        }

        private static ConditionRuleRow CreateConditionRuleRow(string match)
        {
            var row = new ConditionRuleRow();

            if (match.StartsWith("Contains("))
            {
                row.RowType = ConditionRuleRowType.Contains;
                row.Values.AddRange(ExtractValues(match, "Contains"));
            }
            else if (match.StartsWith("ContainsAll("))
            {
                row.RowType = ConditionRuleRowType.ContainsAll;
                row.Values.AddRange(ExtractValues(match, "ContainsAll"));
            }
            else if (match.StartsWith("!Contains("))
            {
                row.RowType = ConditionRuleRowType.DoNotContainsAny;
                row.Values.AddRange(ExtractValues(match, "!Contains"));
            }
            else if (match.StartsWith("!ContainsAll("))
            {
                row.RowType = ConditionRuleRowType.DoNotContainsAll;
                row.Values.AddRange(ExtractValues(match, "!ContainsAll"));
            }
            else if (match == "&&" || match == "||" || match == "(" || match == ")")
            {
                row.RowType = ConditionRuleRowType.Operator;
                row.Expression = match;
            }

            return row;
        }

        public static bool EvaluateCondition(this ConditionRuleContainer container, string value)
        {
           return  new ConditionEvaluator(container).Evaluate(value);
        }

        private static IEnumerable<ConditionRuleRowValue> ExtractValues(string match, string prefix)
        {
            var startIndex = prefix.Length + 1; // Length of prefix plus opening parenthesis
            var endIndex = match.Length - 1; // Closing parenthesis
            var valuesString = match.Substring(startIndex, endIndex - startIndex);
            var values = valuesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(v => new ConditionRuleRowValue { Value = v.Trim() });
            return values;
        }
    }

    public class ConditionEvaluator
    {
        private readonly ConditionRuleContainer _container;
        private readonly Stack<bool> _results;
        private readonly Stack<string> _operators;

        public ConditionEvaluator(ConditionRuleContainer container)
        {
            _container = container;
            _results = new Stack<bool>();
            _operators = new Stack<string>();
        }

        public bool Evaluate(string value)
        {
            var values = value.Split(',')
                                          .Select(v => v.Trim())
                                          .ToHashSet();

            foreach (var row in _container.Rows.OrderBy(r => r.Sort))
            {
                EvaluateRow(row, values);

                if (_operators.Count > 0 && _results.Count > 1)
                {
                    EvaluateLogicalOperation();
                }
            }

            return _results.Count > 0 ? _results.Pop() : false;
        }

        public void EvaluateRow(ConditionRuleRow row, HashSet<string> values)
        {
            switch (row.RowType)
            {
                case ConditionRuleRowType.Operator:
                    _operators.Push(row.Expression);
                    break;
                case ConditionRuleRowType.Contains:
                    _results.Push(row.Values.Any(v => values.Contains(v.Value)));
                    break;
                case ConditionRuleRowType.ContainsAll:
                    _results.Push(row.Values.All(v => values.Contains(v.Value)));
                    break;
                case ConditionRuleRowType.DoNotContainsAny:
                    _results.Push(row.Values.All(v => !values.Contains(v.Value)));
                    break;
                case ConditionRuleRowType.DoNotContainsAll:
                    _results.Push(row.Values.Any(v => !values.Contains(v.Value)));
                    break;
            }
        }

        public void EvaluateLogicalOperation()
        {
            var op = _operators.Pop();
            var right = _results.Pop();
            var left = _results.Pop();

            bool result;
            switch (op)
            {
                case "&&":
                    result = left && right;
                    break;
                case "||":
                    result = left || right;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown operator: {op}");
            }

            _results.Push(result);
        }
    }
}

