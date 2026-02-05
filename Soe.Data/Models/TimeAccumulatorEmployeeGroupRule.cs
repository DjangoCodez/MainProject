using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Text;


namespace SoftOne.Soe.Data
{
    public partial class TimeAccumulatorEmployeeGroupRule : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        public static TimeAccumulatorEmployeeGroupRuleDTO ToDTO(this TimeAccumulatorEmployeeGroupRule e)
        {
            if (e == null)
                return null;

            return new TimeAccumulatorEmployeeGroupRuleDTO()
            {
                EmployeeGroupId = e.EmployeeGroupId,
                Type = (TermGroup_AccumulatorTimePeriodType)e.Type,
                MinMinutes = e.MinMinutes,
                MinTimeCodeId = e.MinTimeCodeId,
                MaxMinutes = e.MaxMinutes,
                MaxTimeCodeId = e.MaxTimeCodeId,
                ShowOnPayrollSlip = e.ShowOnPayrollSlip,
                MinMinutesWarning = e.MinMinutesWarning,
                MaxMinutesWarning = e.MaxMinutesWarning,
                ScheduledJobHeadId = e.ScheduledJobHeadId,
                TimeAccumulatorEmployeeGroupRuleId = e.TimeAccumulatorEmployeeGroupRuleId,
                TimeAccumulatorId = e.TimeAccumulatorId,
                ThresholdMinutes = e.ThresholdMinutes,
                State = (SoeEntityState)e.State,
            };
        }

        public static List<TimeAccumulatorEmployeeGroupRuleDTO> ToDTOs(this IEnumerable<TimeAccumulatorEmployeeGroupRule> l)
        {
            var dtos = new List<TimeAccumulatorEmployeeGroupRuleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static string GetRuleBoundaries(this TimeAccumulatorEmployeeGroupRule e, string typeName, int? defaultMinutes = null)
        {
            if (e == null)
                return string.Empty;

            int? minMinutes = e.MinMinutes ?? defaultMinutes;
            int? maxMinutes = e.MaxMinutes ?? defaultMinutes;
            if (!minMinutes.HasValue || !maxMinutes.HasValue)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            if (minMinutes == maxMinutes)
                sb.Append($"{typeName} {CalendarUtility.GetHoursAndMinutesString(minMinutes.Value)}");
            else
                sb.Append($"{typeName} {CalendarUtility.GetHoursAndMinutesString(minMinutes.Value)} - {CalendarUtility.GetHoursAndMinutesString(maxMinutes.Value)}");

            if (e.MinMinutesWarning.HasValue || e.MaxMinutesWarning.HasValue)
            {
                sb.Append(" (");
                if (e.MinMinutesWarning.HasValue)
                    sb.Append(CalendarUtility.GetHoursAndMinutesString(e.MinMinutesWarning.Value));
                sb.Append(" - ");
                if (e.MaxMinutesWarning.HasValue)
                    sb.Append(CalendarUtility.GetHoursAndMinutesString(e.MaxMinutesWarning.Value));
                sb.Append(")");
            }

            return sb.ToString();
        }
    }
}
