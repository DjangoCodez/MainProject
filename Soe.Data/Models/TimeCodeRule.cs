using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeCodeRule : ITimeCodeRule, ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeCodeRule

        public static TimeCodeRuleDTO ToDTO(this TimeCodeRule e)
        {
            if (e == null)
                return null;

            return new TimeCodeRuleDTO()
            {
                Type = e.Type,
                Value = e.Value,
                Time = e.Time,
            };
        }

        public static IEnumerable<TimeCodeRuleDTO> ToDTOs(this IEnumerable<TimeCodeRule> l)
        {
            var dtos = new List<TimeCodeRuleDTO>();
            if (l != null)
            {
                foreach (var e in l.Where(i => i.State == (int)SoeEntityState.Active))
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeCodeRule GetTimeCodeRule(this TimeCode e, TermGroup_TimeCodeRuleType type)
        {
            return e?.TimeCodeRule?.FirstOrDefault(i => i.Type == (int)type && i.State == (int)SoeEntityState.Active);
        }

        public static bool IsBreakRule(this ITimeCodeRule e)
        {
            switch (e.Type)
            {
                case (int)TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart:
                case (int)TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop:
                case (int)TermGroup_TimeCodeRuleType.TimeCodeLessThanMin:
                case (int)TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd:
                case (int)TermGroup_TimeCodeRuleType.TimeCodeStd:
                case (int)TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax:
                case (int)TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsQuantityRule(this ITimeCodeRule e)
        {
            switch (e.Type)
            {
                case (int)TermGroup_TimeCodeRuleType.AdjustQuantityOnTime:
                case (int)TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsModified(this ITimeCodeRule e, int value, DateTime? time = null)
        {
            return e.Value != value || e.Time != time;
        }

        public static bool IsValid(this ITimeCodeRule e)
        {
            if (e.Type == (int)TermGroup_TimeCodeRuleType.AdjustQuantityOnTime)
                return e.Value >= 0 && e.Time.HasValue;
            else if (e.Type == (int)TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay)
                return e.Value >= 0;
            else if (e.Value > 0)
                return true;
            return false;
        }

        #endregion
    }
}
