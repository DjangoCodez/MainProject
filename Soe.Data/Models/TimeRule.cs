using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeRule : ICreatedModified, IState
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; }
        public string TimeCodeName { get; set; }
        public List<int> TimeDeviationCauseIds { get; set; }
        public string TimeDeviationCauseNames { get; set; }
        public List<int> DayTypeIds { get; set; }
        public string DayTypeNames { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public string EmployeeGroupNames { get; set; }
        public List<int> TimeScheduleTypeIds { get; set; }
        public string TimeScheduleTypeNames { get; set; }

        //Extensions
        public int PayrollProductId { get; set; }
        public string PayrollProductName { get; set; }
        public decimal PayrollProductFactor { get; set; }
        public int NrOfTimeCodePayrollProducts { get; set; }
        public string Information { get; set; }
        public string PayrollProductExternalCode { get; set; }
        public bool HasFailed { get; set; }
        public bool UseStandardMinutes
        {
            get
            {
                return (this.StandardMinutes.HasValue && this.StandardMinutes.Value > 0) || this.UseMaxAsStandard; //Remove second part when ugyl solution is removed
            }
        }
        public bool UseBreakIfAnyFailed
        {
            get
            {
                return this.BreakIfAnyFailed || this.Contains3Stars; //Remove second part when ugyl solution is removed
            }
        }
        public bool UseAdjustStartToTimeBlockStart
        {
            get
            {
                return this.AdjustStartToTimeBlockStart || this.Contains3Plus; //Remove second part when ugyl solution is removed
            }
        }

        //Temp-properties. Remove when ugly solutions is removed
        public bool UseMaxAsStandard
        {
            get
            {
                return this.Contains3Brackets && this.TimeCodeMaxLength.HasValue;
            }
        }
        public bool Contains3Brackets
        {
            get
            {
                return !this.Description.IsNullOrEmpty() && this.Description.Contains("###");
            }
        }
        public bool Contains3Stars
        {
            get
            {
                return !this.Description.IsNullOrEmpty() && this.Description.Contains("***");
            }
        }
        public bool Contains3Plus
        {
            get
            {
                return !this.Description.IsNullOrEmpty() && this.Description.Contains("+++");
            }
        }

        //Used by wizard
        public Dictionary<int, string> TimeDeviationCausesDict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> DayTypesDict { get; set; } = new Dictionary<int, string>();
        public List<TimeCode> UsedTimeCodesByRule { get; set; } = new List<TimeCode>();
    }

    public partial class TimeRuleExpression : ITimeRuleExpression
    {

    }

    public partial class TimeRuleOperand : ITimeRuleOperand
    {

    }

    public partial class TimeRuleGroup : IState
    {

    }

    public partial class TimeRuleGroup
    {
        public int ActorCompanyId { get; set; }
        public string SourceName { get; set; }
        public int SourceId { get; set; }
        public List<int> TimeDeviationCauseIds { get; set; }
        public string TimeDeviationCauseNames { get; set; }
        public List<int> DayTypeIds { get; set; }
        public string DayTypeNames { get; set; }
        public List<int> EmployeeGroupIds { get; set; }
        public string EmployeeGroupNames { get; set; }
        public List<int> TimeScheduleTypeIds { get; set; }
        public string TimeScheduleTypeNames { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeRule

        public static IEnumerable<TimeRuleIwhDTO> ToIwhDTOs(this IEnumerable<TimeRule> l)
        {
            var dtos = new List<TimeRuleIwhDTO>();
            if (l != null)
            {
                foreach (var e in l.Where(e => e.IsInconvenientWorkHours && e.State == (int)SoeEntityState.Active))
                {
                    var dto = e.ToIwhDTO();
                    if (dto != null)
                        dtos.Add(dto);
                }
            }
            return dtos;
        }

        public static TimeRuleIwhDTO ToIwhDTO(this TimeRule e)
        {
            if (e == null)
                return null;

            if (!e.TryGetIWhTimes(out DateTime startTime, out DateTime stopTime, out string length))
                return null;

            return new TimeRuleIwhDTO()
            {
                ActorCompanyId = e.ActorCompanyId,
                TimeRuleId = e.TimeRuleId,
                Name = e.Name,
                RuleStartDate = e.StartDate,
                RuleStopDate = e.StopDate,
                TimeDeviationCauseIds = e.GetTimeDeviationCauses(),
                DayTypeIds = e.GetDayTypes(),
                EmployeeGroupIds = e.GetEmployeeGroups(),
                TimeScheduleTypeIds = e.GetTimeScheduleTypes(),
                PayrollProductId = e.PayrollProductId,
                PayrollProductName = e.PayrollProductName,
                PayrollProductFactor = e.PayrollProductFactor,
                NrOfTimeCodePayrollProducts = e.NrOfTimeCodePayrollProducts,
                Information = e.Information,
                PayrollProductExternalCode = e.PayrollProductExternalCode,
                StartTime = startTime,
                StopTime = stopTime,
                Length = length,
            };
        }

        public static TimeRuleEditDTO ToEditDTO(this TimeRule e)
        {
            if (e == null)
                return null;

            return new TimeRuleEditDTO()
            {
                TimeRuleId = e.TimeRuleId,
                Name = e.Name,
                Description = e.Description,
                Sort = e.Sort,
                IsInconvenientWorkHours = e.IsInconvenientWorkHours,
                IsStandby = e.IsStandby,
                Type = (SoeTimeRuleType)e.Type,
                RuleStartDirection = e.RuleStartDirection,
                RuleStopDirection = e.RuleStopDirection,
                Factor = e.Factor,
                TimeCodeId = e.TimeCodeId,
                TimeCodeMaxLength = e.TimeCodeMaxLength,
                TimeCodeMaxPerDay = e.TimeCodeMaxPerDay,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Imported = e.Imported,
                StandardMinutes = e.StandardMinutes,
                BreakIfAnyFailed = e.BreakIfAnyFailed,
                AdjustStartToTimeBlockStart = e.AdjustStartToTimeBlockStart,
                InconvenientWorkHourRule = e.ToEditIwhDTO(),
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                EmployeeGroupIds = e.EmployeeGroupIds ?? new List<int>(),
                TimeScheduleTypeIds = e.TimeScheduleTypeIds ?? new List<int>(),
                TimeDeviationCauseIds = e.TimeDeviationCauseIds ?? new List<int>(),
                DayTypeIds = e.DayTypeIds ?? new List<int>(),
                TimeRuleExpressions = e.TimeRuleExpression?.ToDTOs().ToList() ?? new List<TimeRuleExpressionDTO>(),
            };
        }

        public static TimeRuleEditIwhDTO ToEditIwhDTO(this TimeRule e)
        {
            if (!e.TryGetIWhTimes(out DateTime startTime, out DateTime stopTime, out string length))
                return null;

            return new TimeRuleEditIwhDTO()
            {
                PayrollProductName = e.PayrollProductName,
                PayrollProductFactor = e.PayrollProductFactor,
                Information = e.Information,
                StartTime = startTime,
                StopTime = stopTime,
                Length = length,
            };
        }

        public static TimeRuleDTO ToDTO(this TimeRuleEditDTO e)
        {
            if (e == null)
                return null;

            return new TimeRuleDTO()
            {
                TimeRuleId = e.TimeRuleId,
                TimeCodeId = e.TimeCodeId,
                Type = e.Type,
                Name = e.Name,
                Description = e.Description,
                Sort = e.Sort,
                RuleStartDirection = e.RuleStartDirection,
                RuleStopDirection = e.RuleStopDirection,
                TimeCodeMaxLength = e.TimeCodeMaxLength,
                TimeCodeMaxPerDay = e.TimeCodeMaxPerDay,
                IsInconvenientWorkHours = e.IsInconvenientWorkHours,
                IsStandby = e.IsStandby,
                Factor = e.Factor,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                Imported = e.Imported,
                StandardMinutes = e.StandardMinutes,
                BreakIfAnyFailed = e.BreakIfAnyFailed,
                AdjustStartToTimeBlockStart = e.AdjustStartToTimeBlockStart,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = e.State,
                EmployeeGroupIds = e.EmployeeGroupIds ?? new List<int>(),
                TimeScheduleTypeIds = e.TimeScheduleTypeIds ?? new List<int>(),
                TimeDeviationCauseIds = e.TimeDeviationCauseIds ?? new List<int>(),
                DayTypeIds = e.DayTypeIds ?? new List<int>(),
                TimeRuleExpressions = e.TimeRuleExpressions ?? new List<TimeRuleExpressionDTO>(),
            };
        }

        public static List<TimeRule> SortRules(this IEnumerable<TimeRule> l)
        {
            return l?.OrderBy(tr => tr.Sort).ToList() ?? new List<TimeRule>();
        }

        public static List<TimeRule> Filter(this IEnumerable<TimeRule> l, bool? active = null, DateTime? date = null, int timeDeviationCauseId = 0, int? dayTypeId = null, int? employeeGroupId = null, int? timeScheduleTypeId = null, bool doFilterTimeScheduleTypeId = true)
        {
            return l?
                .FilterActive(active)
                .FilterDate(date)
                .FilterRelations(timeDeviationCauseId, dayTypeId, employeeGroupId, timeScheduleTypeId, doFilterTimeScheduleTypeId)
                .ToList()
                ?? new List<TimeRule>();
        }

        public static IEnumerable<TimeRule> FilterActive(this IEnumerable<TimeRule> l, bool? active)
        {
            if (l.Any() && active.HasValue)
            {
                if (active == true)
                    l = l.Where(tr => tr.State == (int)SoeEntityState.Active).ToList();
                else
                    l = l.Where(tr => tr.State == (int)SoeEntityState.Inactive).ToList();
            }
            return l;
        }

        public static IEnumerable<TimeRule> FilterDate(this IEnumerable<TimeRule> l, DateTime? date)
        {
            if (l.Any() && date.HasValue)
            {
                l = (from tr in l
                     where (!tr.StartDate.HasValue || tr.StartDate.Value <= date.Value) &&
                     (!tr.StopDate.HasValue || tr.StopDate.Value >= date.Value)
                     select tr).ToList();
            }
            return l;
        }

        public static IEnumerable<TimeRule> FilterRelations(this IEnumerable<TimeRule> l, int timeDeviationCauseId, int? dayTypeId, int? employeeGroupId, int? timeScheduleTypeId, bool doFilterTimeScheduleTypeId)
        {
            if (l.Any() && (timeDeviationCauseId != 0 || dayTypeId.HasValue || employeeGroupId.HasValue || timeScheduleTypeId.HasValue))
            {
                l = (from tr in l
                     where tr.ContainsRow(timeDeviationCauseId, dayTypeId, employeeGroupId, timeScheduleTypeId, doFilterTimeScheduleTypeId)
                     select tr).ToList();
            }
            return l;
        }

        public static IEnumerable<TimeRule> SeparateRulesByType(this IEnumerable<TimeRule> l, SoeTimeRuleType type)
        {
            return l?.Where(tr => tr.Type == (int)type) ?? new List<TimeRule>();
        }

        public static IEnumerable<TimeRule> SeparateRulesByStandby(this IEnumerable<TimeRule> l, bool isStandby)
        {
            return l?.Where(tr => tr.IsStandby == isStandby) ?? new List<TimeRule>();
        }

        public static IEnumerable<TimeRule> SeparateRulesByStage(this IEnumerable<TimeRule> l, bool? isBalanceRuleStage, bool isStandByStage, bool doSeparateRulesByStage = true)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeRule>();

            if (!doSeparateRulesByStage)
                return l.ToList();

            List<TimeRule> timeRulesByType = new List<TimeRule>();

            foreach (TimeRule e in l.SeparateRulesByStandby(isStandByStage))
            {
                if (isBalanceRuleStage.HasValue)
                {
                    if (isBalanceRuleStage.Value && e.ContainsBalanceTimeRuleExpressions())
                        timeRulesByType.Add(e);
                    else if (!isBalanceRuleStage.Value && !e.ContainsBalanceTimeRuleExpressions())
                        timeRulesByType.Add(e);
                }
                else
                {
                    timeRulesByType.Add(e);
                }
            }

            return timeRulesByType;
        }

        public static IEnumerable<TimeRule> SeparateRulesByTimeDeviationCause(this IEnumerable<TimeRule> l, int timeDeviationCauseId)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeRule>();

            return l.Where(tr => tr.ContainsTimeDeviationCause(timeDeviationCauseId));
        }

        public static IEnumerable<TimeRule> SeparateRulesByTimeScheduleType(this IEnumerable<TimeRule> l, TimeBlock timeBlock, bool useMultipleScheduleTypes, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeRule>();
            if (timeBlock == null)
                return l;

            List<int?> timeScheduleTypeIds = new List<int?>();

            if (useMultipleScheduleTypes)
            {
                if (timeBlock.CalculatedTimeScheduleTypeId.HasValue)
                    timeScheduleTypeIds.Add(timeBlock.CalculatedTimeScheduleTypeId);
                if (timeBlock.CalculatedTimeScheduleTypeIdFromShift.HasValue)
                    timeScheduleTypeIds.Add(timeBlock.CalculatedTimeScheduleTypeIdFromShift.Value);
                if (timeBlock.CalculatedTimeScheduleTypeIdFromShiftType.HasValue)
                    timeScheduleTypeIds.Add(timeBlock.CalculatedTimeScheduleTypeIdFromShiftType.Value);
            }            
            if (timeScheduleTypeIds.IsNullOrEmpty())
                timeScheduleTypeIds.Add(timeBlock.CalculatedTimeScheduleTypeId);

            if (!timeBlock.CalculatedTimeScheduleTypeIdsFromEmployee.IsNullOrEmpty())
                timeScheduleTypeIds.AddRange(timeBlock.CalculatedTimeScheduleTypeIdsFromEmployee.Cast<int?>());
            if (!timeBlock.CalculatedTimeScheduleTypeIdsFromTimeStamp.IsNullOrEmpty())
                timeScheduleTypeIds.AddRange(timeBlock.CalculatedTimeScheduleTypeIdsFromTimeStamp.Cast<int?>());
            if (!timeBlock.CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes.IsNullOrEmpty())
                timeScheduleTypeIds.AddRange(timeBlock.CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes.Cast<int?>());

            int?[] timeScheduleTypeIdsParam = timeScheduleTypeIds.Distinct().ToArray();

            List<TimeRule> timeRules = new List<TimeRule>();
            foreach (TimeRule e in l)
            {
                if (IsTimeRuleValidAgainstTimeScheduleTypes(e, timeScheduleTypeIdsIsNotScheduleTime, timeScheduleTypeIdsParam))
                    timeRules.Add(e);
            }

            return timeRules;
        }

        public static bool ContainsOvertimeOperand(this TimeRule e)
        {
            if (e != null)
            {
                foreach (TimeRuleExpression expression in e.GetExpressions())
                {
                    if (expression.ContainsOperand(SoeTimeRuleValueType.SchedulePlusOvertimeInOvertimePeriod, SoeTimeRuleValueType.FulltimeWeek))
                        return true;
                }
            }
            return false;
        }

        public static bool IsInconvenientWorkHours(this IEnumerable<TimeRule> l, int timeRuleId)
        {
            return timeRuleId != 0 && (l?.FirstOrDefault(i => i.TimeRuleId == timeRuleId)?.IsInconvenientWorkHours ?? false);
        }

        public static bool HasStandby(this IEnumerable<TimeRule> l)
        {
            return l?.Any(i => i.IsStandby) ?? false;
        }

        private static bool TryGetIWhTimes(this TimeRule e, out DateTime startTime, out DateTime stopTime, out string length)
        {
            startTime = CalendarUtility.DATETIME_DEFAULT;
            stopTime = CalendarUtility.DATETIME_DEFAULT;
            length = string.Empty;
            if (e == null || !e.IsInconvenientWorkHours)
                return false;

            TimeRuleExpression startExpression = e.GetStartExpression();
            TimeRuleOperand startOperandClock = startExpression?.GetOperand(SoeTimeRuleOperatorType.TimeRuleOperatorClock);
            if (startOperandClock == null || startExpression.ContainsOperand(SoeTimeRuleOperatorType.TimeRuleOperatorNot, SoeTimeRuleValueType.ScheduleLeft, SoeTimeRuleValueType.ScheduleAndBreakLeft, SoeTimeRuleValueType.PresenceWithinSchedule) || startExpression.ContainsOperand(SoeTimeRuleOperatorType.TimeRuleOperatorScheduleOut))
                return false;

            TimeRuleExpression stopExpression = e.GetStopExpression();
            TimeRuleOperand stopOperandClock = stopExpression?.GetOperand(SoeTimeRuleOperatorType.TimeRuleOperatorClock);
            if (stopOperandClock == null || stopExpression.ContainsOperand(SoeTimeRuleOperatorType.TimeRuleOperatorNot, SoeTimeRuleValueType.ScheduleLeft, SoeTimeRuleValueType.ScheduleAndBreakLeft, SoeTimeRuleValueType.PresenceWithinSchedule) || stopExpression.ContainsOperand(SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn))
                return false;

            startTime = CalendarUtility.GetDateFromMinutes(startOperandClock.Minutes, true);
            stopTime = CalendarUtility.GetDateFromMinutes(stopOperandClock.Minutes, true);
            length = CalendarUtility.FormatTimeSpan(stopTime.Subtract(startTime), true, false);
            return true;
        }

        #endregion

        #region TimeRuleRow

        public static IEnumerable<TimeRuleRowDTO> ToDTOs(this IEnumerable<TimeRuleRow> l)
        {
            return l?.Select(e => e.ToDTO()).ToList() ?? new List<TimeRuleRowDTO>();
        }

        public static TimeRuleRowDTO ToDTO(this TimeRuleRow e)
        {
            if (e == null)
                return null;

            return new TimeRuleRowDTO()
            {
                TimeRuleRowId = e.TimeRuleRowId,
                TimeRuleId = e.TimeRuleId,
                ActorCompanyId = e.ActorCompanyId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                DayTypeId = e.DayTypeId,
                EmployeeGroupId = e.EmployeeGroupId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
            };
        }

        public static List<int> GetTimeDeviationCauses(this TimeRule e)
        {
            return e?.TimeRuleRow.Select(i => i.TimeDeviationCauseId).Distinct().ToList() ?? new List<int>();
        }

        public static List<int?> GetDayTypes(this TimeRule e)
        {
            return e?.TimeRuleRow.Select(i => i.DayTypeId).Distinct().ToList() ?? new List<int?>();
        }

        public static List<int?> GetEmployeeGroups(this TimeRule e)
        {
            return e?.TimeRuleRow.Select(i => i.EmployeeGroupId).Distinct().ToList() ?? new List<int?>();
        }

        public static List<int?> GetTimeScheduleTypes(this TimeRule e)
        {
            return e?.TimeRuleRow.Select(i => i.TimeScheduleTypeId).Distinct().ToList() ?? new List<int?>();
        }

        public static TimeScheduleType GetAllTimeScheduleType(this TimeRule e)
        {
            return e?.TimeRuleRow.Where(r => r.TimeScheduleType != null && r.TimeScheduleType.IsAll).Select(i => i.TimeScheduleType).FirstOrDefault();
        }

        public static IQueryable<TimeRule> FilterActive(this IQueryable<TimeRule> l, bool? active)
        {
            if (active.HasValue)
            {
                if (active == true)
                    l = l.Where(tr => tr.State == (int)SoeEntityState.Active);
                else
                    l = l.Where(tr => tr.State == (int)SoeEntityState.Inactive);
            }
            else
                l = l.Where(tr => tr.State < (int)SoeEntityState.Deleted);

            return l;
        }

        public static bool IsTimeRuleValidAgainstTimeScheduleTypes(this TimeRule e, List<int> timeScheduleTypeIdsIsNotScheduleTime, params int?[] timeScheduleTypeIds)
        {
            foreach (int? timeScheduleTypeId in timeScheduleTypeIds.Distinct())
            {
                if (e.ContainsTimeScheduleType(timeScheduleTypeId) && (!timeScheduleTypeId.HasValue || timeScheduleTypeIdsIsNotScheduleTime == null || !timeScheduleTypeIdsIsNotScheduleTime.Contains(timeScheduleTypeId.Value)))
                    return true;
            }

            return false;
        }

        public static bool HasTimeRuleRows(this TimeRule e)
        {
            return e != null && !e.TimeRuleRow.IsNullOrEmpty();
        }

        public static bool ContainsBalanceTimeRuleExpressions(this TimeRule e)
        {
            return e?.TimeRuleExpression?.Any(op => op.TimeRuleOperand.Any(o => o.OperatorType == (int)SoeTimeRuleOperatorType.TimeRuleOperatorBalance)) ?? false;
        }

        public static bool ContainsRow(this TimeRule e, int timeDeviationCauseId, int? dayTypeId, int? employeeGroupId, int? timeScheduleTypeId, bool doFilterTimeScheduleTypeId = true)
        {
            return
                e.HasTimeRuleRows() &&
                e.ContainsTimeDeviationCause(timeDeviationCauseId) &&
                e.ContainsDayType(dayTypeId) &&
                e.ContainsEmployeeGroup(employeeGroupId) &&
                (!doFilterTimeScheduleTypeId || e.ContainsTimeScheduleType(timeScheduleTypeId));
        }

        public static bool ContainsTimeDeviationCause(this TimeRule e, int timeDeviationCauseId)
        {
            if (timeDeviationCauseId == 0)
                return true;

            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r => r.TimeDeviationCauseId == timeDeviationCauseId);
        }

        public static bool ContainsDayType(this TimeRule e, int? dayTypeId)
        {
            dayTypeId = dayTypeId.ToNullable();
            if (!dayTypeId.HasValue)
                return true;

            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r => !r.DayTypeId.HasValue || r.DayTypeId == dayTypeId);
        }

        public static bool ContainsEmployeeGroup(this TimeRule e, int? employeeGroupId)
        {
            employeeGroupId = employeeGroupId.ToNullable();
            if (!employeeGroupId.HasValue)
                return true;

            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r => !r.EmployeeGroupId.HasValue || r.EmployeeGroupId == employeeGroupId);
        }

        public static bool ContainsTimeScheduleType(this TimeRule e, int? timeScheduleTypeId)
        {
            timeScheduleTypeId = timeScheduleTypeId.ToNullable();

            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r =>
                    (r.TimeScheduleType != null && r.TimeScheduleType.IsAll) //All
                    ||
                    (!timeScheduleTypeId.HasValue && !r.TimeScheduleTypeId.HasValue) //None
                    ||
                    (timeScheduleTypeId.HasValue && timeScheduleTypeId.Value == r.TimeScheduleTypeId) //Specific
                    );
        }

        public static bool ContainsAllEmployeeGroups(this TimeRule e)
        {
            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r => !r.EmployeeGroupId.HasValue);
        }

        public static bool ContainsAllDayTypes(this TimeRule e)
        {
            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r => !r.DayTypeId.HasValue);
        }

        public static bool ContainsAllTimeScheduleTypes(this TimeRule e)
        {
            return
                e.HasTimeRuleRows() &&
                e.TimeRuleRow.Any(r => (r.TimeScheduleType != null && r.TimeScheduleType.IsAll)); //All
        }

        public static bool ContainsOperand(this TimeRule e, SoeTimeRuleOperatorType operand, params SoeTimeRuleValueType[] leftValueTypes)
        {
            return e?.TimeRuleExpression.Any(expression => expression.ContainsOperand(operand, leftValueTypes)) ?? false;
        }

        #endregion

        #region TimeRuleExpression

        public static IEnumerable<TimeRuleExpressionDTO> ToDTOs(this IEnumerable<TimeRuleExpression> l)
        {
            return l?.Select(e => e.ToDTO()) ?? new List<TimeRuleExpressionDTO>();
        }

        public static TimeRuleExpressionDTO ToDTO(this TimeRuleExpression e)
        {
            if (e == null)
                return null;

            return new TimeRuleExpressionDTO()
            {
                TimeRuleExpressionId = e.TimeRuleExpressionId,
                TimeRuleId = e.TimeRuleId,
                IsStart = e.IsStart,
                TimeRuleOperands = e.TimeRuleOperand?.ToDTOs().ToList() ?? new List<TimeRuleOperandDTO>()
            };
        }

        public static List<TimeRuleExpression> GetExpressions(this TimeRule e)
        {
            return e?.TimeRuleExpression?.ToList() ?? new List<TimeRuleExpression>();
        }

        public static TimeRuleExpression GetStartExpression(this TimeRule e)
        {
            return e?.TimeRuleExpression?.FirstOrDefault(i => i.IsStart);
        }

        public static TimeRuleExpression GetStopExpression(this TimeRule e)
        {
            return e?.TimeRuleExpression?.FirstOrDefault(i => !i.IsStart && (i.TimeRuleOperandRecursive == null || !i.TimeRuleOperandRecursive.Any()));
        }

        public static TimeRuleExpressionDTO GetStartExpression(this TimeRuleEditDTO e)
        {
            return e?.TimeRuleExpressions?.FirstOrDefault(i => i.IsStart);
        }

        public static TimeRuleExpressionDTO GetStopExpression(this TimeRuleEditDTO e)
        {
            return e?.TimeRuleExpressions?.FirstOrDefault(i => !i.IsStart);
        }

        public static bool ContainsOperand(this TimeRuleExpression e, SoeTimeRuleOperatorType operand, params SoeTimeRuleValueType[] leftValueTypes)
        {
            return e?.GetOperand(operand, leftValueTypes) != null;
        }

        public static bool ContainsOperand(this TimeRuleExpression e, params SoeTimeRuleValueType[] leftValueTypes)
        {
            return !leftValueTypes.IsNullOrEmpty() && (e?.TimeRuleOperand?.Any(o => o.LeftValueType.HasValue && leftValueTypes.Contains((SoeTimeRuleValueType)o.LeftValueType.Value)) ?? false);
        }

        #endregion

        #region TimeRuleOperand

        public static IEnumerable<TimeRuleOperandDTO> ToDTOs(this IEnumerable<TimeRuleOperand> l)
        {
            return l?.Select(e => e.ToDTO()) ?? new List<TimeRuleOperandDTO>();
        }

        public static TimeRuleOperandDTO ToDTO(this TimeRuleOperand e)
        {
            if (e == null)
                return null;

            return new TimeRuleOperandDTO()
            {
                TimeRuleOperandId = e.TimeRuleOperandId,
                TimeRuleExpressionId = e.TimeRuleExpressionId,
                TimeRuleExpressionRecursiveId = e.TimeRuleExpressionRecursiveId,
                OperatorType = (SoeTimeRuleOperatorType)e.OperatorType,
                LeftValueType = (SoeTimeRuleValueType)(e.LeftValueType ?? 0),
                LeftValueId = e.LeftValueId,
                RightValueType = (SoeTimeRuleValueType)(e.RightValueType ?? 0),
                RightValueId = e.RightValueId,
                Minutes = e.Minutes,
                ComparisonOperator = (SoeTimeRuleComparisonOperator)(e.ComparisonOperator.HasValue ? e.ComparisonOperator : 0),
                OrderNbr = e.OrderNbr
            };
        }

        public static TimeRuleOperand GetOperand(this TimeRuleExpression e, SoeTimeRuleOperatorType operand, params SoeTimeRuleValueType[] leftValueTypes)
        {
            if (!leftValueTypes.IsNullOrEmpty())
                return e.TimeRuleOperand.FirstOrDefault(o => o.OperatorType == (int)operand && o.LeftValueType.HasValue && leftValueTypes.Contains((SoeTimeRuleValueType)o.LeftValueType.Value));
            else
                return e.TimeRuleOperand.FirstOrDefault(o => o.OperatorType == (int)operand);
        }

        public static bool IsLeftValueTimeCode(this TimeRuleOperand e)
        {
            return e?.LeftValueType == (int)SoeTimeRuleValueType.TimeCodeLeft;
        }

        public static bool IsRightValueTimeCode(this TimeRuleOperand e)
        {
            return e?.RightValueType == (int)SoeTimeRuleValueType.TimeCodeRight;
        }

        public static string GetScheduleMinutesMessage(this TimeRuleOperand e)
        {
            if (e.Minutes == 0)
                return string.Empty;
            return e.ComparisonOperator == (int)SoeTimeRuleComparisonOperator.TimeRuleComparisonClockNegative ? "-" : "+" + e.Minutes;
        }

        public static string GetScheduleMinutesMessage(this TimeRuleOperandDTO e)
        {
            if (e.Minutes == 0)
                return string.Empty;
            return e.ComparisonOperator == SoeTimeRuleComparisonOperator.TimeRuleComparisonClockNegative ? "-" : "+" + e.Minutes;
        }

        public static bool IsClockPrevDay(this ITimeRuleOperand e)
        {
            return e.Minutes < 0;
        }

        public static bool IsClockNextDay(this ITimeRuleOperand e)
        {
            return e.Minutes >= 1440;
        }

        #endregion

        #region TimeRule BalanceRuleSetting

        public static IEnumerable<BalanceRuleSettingDTO> ToDTOs(this IEnumerable<BalanceRuleSetting> l)
        {
            return l?.Select(e => e.ToDTO()) ?? new List<BalanceRuleSettingDTO>();
        }

        public static BalanceRuleSettingDTO ToDTO(this BalanceRuleSetting e)
        {
            if (e == null)
                return null;

            return new BalanceRuleSettingDTO()
            {
                BalanceRuleSettingId = e.BalanceRuleSettingId,
                TimeRuleGroupId = e.TimeRuleGroupId,
                TimeCodeId = e.TimeCodeId,
                ReplacementTimeCodeId = e.ReplacementTimeCodeId
            };
        }

        #endregion
    }
}
