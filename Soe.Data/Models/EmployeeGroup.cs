using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Data.Util;

namespace SoftOne.Soe.Data
{
    public partial class EmployeeGroup : ICreatedModified, IState
    {
        public string TimeDeviationCausesNames { get; set; }
        public string TimeDeviationCausesRequestNames { get; set; }
        public string DayTypesNames { get; set; }
        public string AttestTransitionNames { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
        public bool ExternalCodesIsLoaded { get; set; }

        public DayOfWeek GetRuleRestTimeWeekStartDayDayOfWeek()
        {
            return this.RuleRestTimeWeekStartDayNumber.HasValue ? (DayOfWeek)this.RuleRestTimeWeekStartDayNumber.Value : DayOfWeek.Monday;
        }

        public DateTime GetRuleRestTimeWeekStartTime()
        {
            return this.RuleRestTimeWeekStartTime ?? CalendarUtility.DATETIME_DEFAULT;
        }
        public DateTime GetRuleRestTimeDayStartTime()
        {
            return this.RuleRestTimeDayStartTime ?? CalendarUtility.DATETIME_DEFAULT.AddHours(12);
        }
    }

    public static partial class EntityExtensions
    {
        #region EmployeeGroup

        public static EmployeeGroupDTO ToDTO(this EmployeeGroup e, bool includeTimeDeviationCause = false, Dictionary<int, string> timeReportTypes = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeTimeDeviationCause)
                {
                    if (!e.TimeDeviationCauseReference.IsLoaded)
                    {
                        e.TimeDeviationCauseReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeGroup.cs e.TimeDeviationCauseReference");
                    }
                    if (e.TimeDeviationCause != null && !e.TimeDeviationCause.TimeCodeReference.IsLoaded)
                    {
                        e.TimeDeviationCause.TimeCodeReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeGroup.cs e.TimeDeviationCause.TimeCodeReference");
                    }
                    if (!e.EmployeeGroupTimeDeviationCauseTimeCode.IsLoaded)
                    {
                        e.EmployeeGroupTimeDeviationCauseTimeCode.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("EmployeeGroup.cs e.EmployeeGroupTimeDeviationCauseTimeCode");
                    }
                    e.EmployeeGroupTimeDeviationCauseTimeCode.ToList().ForEach(i => i.TimeCodeReference.Load());
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            var dto = new EmployeeGroupDTO()
            {
                EmployeeGroupId = e.EmployeeGroupId,
                ActorCompanyId = e.ActorCompanyId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeCodeId = e.TimeCodeId,
                Name = e.Name,
                DeviationAxelStartHours = e.DeviationAxelStartHours,
                DeviationAxelStopHours = e.DeviationAxelStopHours,
                PayrollProductAccountingPrio = e.PayrollProductAccountingPrio,
                InvoiceProductAccountingPrio = e.InvoiceProductAccountingPrio,
                AutogenTimeblocks = e.AutogenTimeblocks,
                AlwaysDiscardBreakEvaluation = e.AlwaysDiscardBreakEvaluation,
                MergeScheduleBreaksOnDay = e.MergeScheduleBreaksOnDay,
                AutogenBreakOnStamping = e.AutogenBreakOnStamping,
                BreakDayMinutesAfterMidnight = e.BreakDayMinutesAfterMidnight,
                KeepStampsTogetherWithinMinutes = e.KeepStampsTogetherWithinMinutes,
                RuleWorkTimeWeek = e.RuleWorkTimeWeek,
                RuleRestTimeDay = e.RuleRestTimeDay,
                RuleRestTimeWeek = e.RuleRestTimeWeek,
                MaxScheduleTimeFullTime = e.MaxScheduleTimeFullTime,
                MinScheduleTimeFullTime = e.MinScheduleTimeFullTime,
                MaxScheduleTimePartTime = e.MaxScheduleTimePartTime,
                MinScheduleTimePartTime = e.MinScheduleTimePartTime,
                MaxScheduleTimeWithoutBreaks = e.MaxScheduleTimeWithoutBreaks,
                QualifyingDayCalculationRule = (TermGroup_QualifyingDayCalculationRule)e.QualifyingDayCalculationRule,
                QualifyingDayCalculationRuleLimitFirstDay = e.QualifyingDayCalculationRuleLimitFirstDay,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                RuleWorkTimeDayMinimum = e.RuleWorkTimeDayMinimum,
                RuleWorkTimeDayMaximumWorkDay = e.RuleWorkTimeDayMaximumWorkDay,
                RuleWorkTimeDayMaximumWeekend = e.RuleWorkTimeDayMaximumWeekend,
                TimeReportType = e.TimeReportType,
                ExtraShiftAsDefault = e.ExtraShiftAsDefault,
            };

            if (includeTimeDeviationCause && e.TimeDeviationCause != null)
            {
                dto.TimeDeviationCause = e.TimeDeviationCause.ToDTO();
                dto.EmployeeGroupTimeDeviationCauseTimeCode = (e.EmployeeGroupTimeDeviationCauseTimeCode != null && e.EmployeeGroupTimeDeviationCauseTimeCode.Count > 0) ? e.EmployeeGroupTimeDeviationCauseTimeCode.ToDTOs().ToList() : new List<EmployeeGroupTimeDeviationCauseTimeCodeDTO>();
            }

            dto.DayTypesNames = e.DayTypesNames;
            dto.TimeDeviationCausesNames = e.TimeDeviationCausesNames;

            if (timeReportTypes != null && timeReportTypes.TryGetValue(dto.TimeReportType, out string timeReportTypeName))
                dto.TimeReportTypeName = timeReportTypeName;

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes;
                dto.ExternalCodesString = e.ExternalCodesString;
            }

            return dto;
        }

        public static EmployeeGroupDTO ToDTONew(
            this EmployeeGroup e,
            Dictionary<int, string> timeReportTypes = null)
        {
            if (e == null)
                return null;

            var dto = new EmployeeGroupDTO()
            {
                EmployeeGroupId = e.EmployeeGroupId,
                ActorCompanyId = e.ActorCompanyId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeCodeId = e.TimeCodeId,
                Name = e.Name,
                DeviationAxelStartHours = e.DeviationAxelStartHours,
                DeviationAxelStopHours = e.DeviationAxelStopHours,
                PayrollProductAccountingPrio = e.PayrollProductAccountingPrio,
                InvoiceProductAccountingPrio = e.InvoiceProductAccountingPrio,
                AutogenTimeblocks = e.AutogenTimeblocks,
                AlwaysDiscardBreakEvaluation = e.AlwaysDiscardBreakEvaluation,
                MergeScheduleBreaksOnDay = e.MergeScheduleBreaksOnDay,
                AutogenBreakOnStamping = e.AutogenBreakOnStamping,
                BreakDayMinutesAfterMidnight = e.BreakDayMinutesAfterMidnight,
                KeepStampsTogetherWithinMinutes = e.KeepStampsTogetherWithinMinutes,
                RuleWorkTimeWeek = e.RuleWorkTimeWeek,
                RuleRestTimeDay = e.RuleRestTimeDay,
                RuleRestTimeWeek = e.RuleRestTimeWeek,
                MaxScheduleTimeFullTime = e.MaxScheduleTimeFullTime,
                MinScheduleTimeFullTime = e.MinScheduleTimeFullTime,
                MaxScheduleTimePartTime = e.MaxScheduleTimePartTime,
                MinScheduleTimePartTime = e.MinScheduleTimePartTime,
                MaxScheduleTimeWithoutBreaks = e.MaxScheduleTimeWithoutBreaks,
                QualifyingDayCalculationRule = (TermGroup_QualifyingDayCalculationRule)e.QualifyingDayCalculationRule,
                QualifyingDayCalculationRuleLimitFirstDay = e.QualifyingDayCalculationRuleLimitFirstDay,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                RuleWorkTimeDayMinimum = e.RuleWorkTimeDayMinimum,
                RuleWorkTimeDayMaximumWorkDay = e.RuleWorkTimeDayMaximumWorkDay,
                RuleWorkTimeDayMaximumWeekend = e.RuleWorkTimeDayMaximumWeekend,
                TimeReportType = e.TimeReportType,
                ExtraShiftAsDefault = e.ExtraShiftAsDefault,
                AlsoAttestAdditionsFromTime = e.AlsoAttestAdditionsFromTime,
                BreakRoundingUp = e.BreakRoundingUp,
                BreakRoundingDown = e.BreakRoundingDown,
                RuleRestDayIncludePresence = e.RuleRestDayIncludePresence,
                RuleRestWeekIncludePresence = e.RuleRestWeekIncludePresence,
                RuleScheduleFreeWeekendsMinimumYear = e.RuleScheduleFreeWeekendsMinimumYear,
                RuleScheduledDaysMaximumWeek = e.RuleScheduledDaysMaximumWeek,
                RuleRestTimeWeekStartTime = e.RuleRestTimeWeekStartTime,
                RuleRestTimeDayStartTime = e.RuleRestTimeDayStartTime,
                RuleRestTimeWeekStartDayNumber = e.RuleRestTimeWeekStartDayNumber,
                NotifyChangeOfDeviations = e.NotifyChangeOfDeviations,
                CandidateForOvertimeOnZeroDayExcluded = e.CandidateForOvertimeOnZeroDayExcluded,
                AutoGenTimeAndBreakForProject = e.AutoGenTimeAndBreakForProject,
                ReminderAttestStateId = e.ReminderAttestStateId,
                ReminderNoOfDays = e.ReminderNoOfDays,
                ReminderPeriodType = e.ReminderPeriodType,
                DefaultDim1CostAccountId = e.DefaultDim1CostAccountId,
                DefaultDim2CostAccountId = e.DefaultDim2CostAccountId,
                DefaultDim3CostAccountId = e.DefaultDim3CostAccountId,
                DefaultDim4CostAccountId = e.DefaultDim4CostAccountId,
                DefaultDim5CostAccountId = e.DefaultDim5CostAccountId,
                DefaultDim6CostAccountId = e.DefaultDim6CostAccountId,
                DefaultDim1IncomeAccountId = e.DefaultDim1IncomeAccountId,
                DefaultDim2IncomeAccountId = e.DefaultDim2IncomeAccountId,
                DefaultDim3IncomeAccountId = e.DefaultDim3IncomeAccountId,
                DefaultDim4IncomeAccountId = e.DefaultDim4IncomeAccountId,
                DefaultDim5IncomeAccountId = e.DefaultDim5IncomeAccountId,
                DefaultDim6IncomeAccountId = e.DefaultDim6IncomeAccountId,
                AllowShiftsWithoutAccount = e.AllowShiftsWithoutAccount,
                TimeWorkReductionCalculationRule = (TermGroup_TimeWorkReductionCalculationRule)e.TimeWorkReductionCalculationRule,
                SwapShiftToShorterText = e.SwapShiftToShorterText,
                SwapShiftToLongerText = e.SwapShiftToLongerText
            };

            if (e.TimeDeviationCause != null)
            {
                dto.TimeDeviationCause = e.TimeDeviationCause.ToDTO();
            }
            if (e.EmployeeGroupTimeDeviationCauseTimeCode != null)
            {
                dto.EmployeeGroupTimeDeviationCauseTimeCode = e.EmployeeGroupTimeDeviationCauseTimeCode?.ToDTOs().ToList();
            }
            if (e.EmployeeGroupDayType != null)
            {
                dto.EmployeeGroupDayType = e.EmployeeGroupDayType.Where(t => t.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }
            if (e.DayType != null)
            {
                dto.DayTypeIds = e.DayType.Select(dt => dt.DayTypeId).ToList();
            }
            if (e.TimeAccumulatorEmployeeGroupRule != null)
            {
                dto.TimeAccumulatorEmployeeGroupRules = e.TimeAccumulatorEmployeeGroupRule.Where(t => t.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }
            if (e.EmployeeGroupTimeDeviationCauseRequest != null)
            {
                dto.TimeDeviationCauseRequestIds = e.EmployeeGroupTimeDeviationCauseRequest.Select(r => r.TimeDeviationCauseId).ToList();
            }
            if (e.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement != null)
            {
                dto.TimeDeviationCauseAbsenceAnnouncementIds = e.EmployeeGroupTimeDeviationCauseAbsenceAnnouncement.Select(r => r.TimeDeviationCauseId).ToList();
            }
            if (e.TimeCodes != null)
            {
                dto.TimeCodeIds = e.TimeCodes.Select(tc => tc.TimeCodeId).ToList();
            }
            if (e.EmployeeGroupTimeDeviationCause != null)
            {
                dto.TimeDeviationCauses = e.EmployeeGroupTimeDeviationCause.Where(t => t.State == (int)SoeEntityState.Active).ToDTOs().ToList();
            }
            if (e.EmployeeGroupRuleWorkTimePeriod != null)
            {
                dto.RuleWorkTimePeriods = e.EmployeeGroupRuleWorkTimePeriod.ToDTOs().ToList();
            }
            var stampRounding = e.TimeStampRounding.FirstOrDefault(); //OK?
            if (stampRounding != null)
            {
                dto.RoundInNeg = stampRounding.RoundInNeg;
                dto.RoundInPos = stampRounding.RoundInPos;
                dto.RoundOutNeg = stampRounding.RoundOutNeg;
                dto.RoundOutPos = stampRounding.RoundOutPos;
            }
            if (e.AttestTransition != null)
            {
                dto.AttestTransition = e.AttestTransition.ToDTOs().ToList();
            }

            if (timeReportTypes != null && timeReportTypes.TryGetValue(dto.TimeReportType, out string timeReportTypeName))
                dto.TimeReportTypeName = timeReportTypeName;

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes;
                dto.ExternalCodesString = e.ExternalCodesString;
            }

            return dto;
        }

        public static EmployeeGroupSmallDTO ToSmallDTO(this EmployeeGroup e)
        {
            if (e == null)
                return null;

            return new EmployeeGroupSmallDTO()
            {
                EmployeeGroupId = e.EmployeeGroupId,
                Name = e.Name,
                RuleWorkTimeWeek = e.RuleWorkTimeWeek,
                AutogenTimeblocks = e.AutogenTimeblocks,
            };
        }

        public static IEnumerable<EmployeeGroupDTO> ToDTOs(this IEnumerable<EmployeeGroup> l, bool includeTimeDeviationCause = false, Dictionary<int, string> timeReportTypes = null)
        {
            var dtos = new List<EmployeeGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeTimeDeviationCause, timeReportTypes));
                }
            }
            return dtos;
        }

        public static IEnumerable<EmployeeGroupSmallDTO> ToSmallDTOs(this IEnumerable<EmployeeGroup> l)
        {
            var dtos = new List<EmployeeGroupSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        #region EmployeeGroupTimeDeviationCauseTimeCodeDTO

        public static EmployeeGroupTimeDeviationCauseTimeCodeDTO ToDTO(this EmployeeGroupTimeDeviationCauseTimeCode e)
        {
            if (e == null)
                return null;

            EmployeeGroupTimeDeviationCauseTimeCodeDTO dto = new EmployeeGroupTimeDeviationCauseTimeCodeDTO()
            {
                EmployeeGroupId = e.EmployeeGroupId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeCodeId = e.TimeCodeId
            };

            if (e.TimeCode != null)
                dto.TimeCode = e.TimeCode.ToDTO();

            return dto;
        }

        public static IEnumerable<EmployeeGroupTimeDeviationCauseTimeCodeDTO> ToDTOs(this IEnumerable<EmployeeGroupTimeDeviationCauseTimeCode> l)
        {
            var dtos = new List<EmployeeGroupTimeDeviationCauseTimeCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region EmployeeGroupDayTypeDTO
        public static EmployeeGroupDayTypeDTO ToDTO(this EmployeeGroupDayType e)
        {
            if (e == null)
                return null;

            EmployeeGroupDayTypeDTO dto = new EmployeeGroupDayTypeDTO()
            {
                EmployeeGroupDayTypeId = e.EmployeeGroupDayTypeId,
                DayTypeId = e.DayTypeId,
                EmployeeGroupId = e.EmployeeGroupId,
                IsHolidaySalary = e.IsHolidaySalary,
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupDayTypeDTO> ToDTOs(this IEnumerable<EmployeeGroupDayType> l)
        {
            var dtos = new List<EmployeeGroupDayTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region EmployeeGroupTimeDeviationCauseDTO

        public static EmployeeGroupTimeDeviationCauseDTO ToDTO(this EmployeeGroupTimeDeviationCause e)
        {
            if (e == null)
                return null;

            EmployeeGroupTimeDeviationCauseDTO dto = new EmployeeGroupTimeDeviationCauseDTO()
            {
                EmployeeGroupTimeDeviationCauseId = e.EmployeeGroupTimeDeviationCauseId,
                EmployeeGroupId = e.EmployeeGroupId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                UseInTimeTerminal = e.UseInTimeTerminal,

            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupTimeDeviationCauseDTO> ToDTOs(this IEnumerable<EmployeeGroupTimeDeviationCause> l)
        {
            var dtos = new List<EmployeeGroupTimeDeviationCauseDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region AttestTransitionDTO
        public static EmployeeGroupAttestTransitionDTO ToDTO(this AttestTransition e)
        {
            if (e == null)
                return null;

            EmployeeGroupAttestTransitionDTO dto = new EmployeeGroupAttestTransitionDTO()
            {
                AttestTransitionId = e.AttestTransitionId,
                Entity = e.AttestStateFrom.Entity
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupAttestTransitionDTO> ToDTOs(this IEnumerable<AttestTransition> l)
        {
            var dtos = new List<EmployeeGroupAttestTransitionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region EmployeeGroupRuleWorkTimePeriodDTO
        public static EmployeeGroupRuleWorkTimePeriodDTO ToDTO(this EmployeeGroupRuleWorkTimePeriod e)
        {
            if (e == null)
                return null;

            EmployeeGroupRuleWorkTimePeriodDTO dto = new EmployeeGroupRuleWorkTimePeriodDTO()
            {
                EmployeeGroupRuleWorkTimePeriodId = e.EmployeeGroupRuleWorkTimePeriodId,
                EmployeeGroupId = e.EmployeeGroupId,
                TimePeriodId = e.TimePeriodId,
                RuleWorkTime = e.RuleWorkTime,
            };

            return dto;
        }

        public static IEnumerable<EmployeeGroupRuleWorkTimePeriodDTO> ToDTOs(this IEnumerable<EmployeeGroupRuleWorkTimePeriod> l)
        {
            var dtos = new List<EmployeeGroupRuleWorkTimePeriodDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        public static Dictionary<int, string> ToDictionary(this IEnumerable<EmployeeGroup> l)
        {
            var dict = new Dictionary<int, string>();

            foreach (var e in l)
            {
                if (!dict.ContainsKey(e.EmployeeGroupId))
                    dict.Add(e.EmployeeGroupId, e.Name);
            }

            return dict;
        }

        public static bool IsTimeReportTypeERP(this EmployeeGroup e)
        {
            return e?.TimeReportType == (int)TermGroup_TimeReportType.ERP;
        }

        public static bool IsTimeReportTypeTimeImport(this EmployeeGroup e)
        {
            return e?.TimeReportType == (int)TermGroup_TimeReportType.TimeImport;
        }

        public static bool UseQualifyingDayCalculationRuleWorkTimeWeekPlusExtraShifts(this EmployeeGroup e)
        {
            return e?.QualifyingDayCalculationRule == (int)TermGroup_QualifyingDayCalculationRule.UseWorkTimeWeekPlusExtraShifts;
        }

        public static EmployeeGroupGridDTO ToGridDTO(this EmployeeGroup e, Dictionary<int, string> timeReportTypes = null)
        {
            if (e == null)
                return null;

            var dto = new EmployeeGroupGridDTO()
            {
                EmployeeGroupId = e.EmployeeGroupId,
                Name = e.Name,
                TimeDeviationCausesNames = e.TimeDeviationCausesNames,
                DayTypesNames = e.DayTypesNames,
                TimeReportType = e.TimeReportType,
                State = (SoeEntityState)e.State
            };

            if (timeReportTypes != null && timeReportTypes.TryGetValue(dto.TimeReportType, out string timeReportTypeName))
                dto.TimeReportTypeName = timeReportTypeName;

            return dto;
        }

        public static IEnumerable<EmployeeGroupGridDTO> ToGridDTOs(this IEnumerable<EmployeeGroup> l, Dictionary<int, string> timeReportTypes = null)
        {
            var dtos = new List<EmployeeGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(timeReportTypes));
                }
            }
            return dtos;
        }

        #endregion

        #region EmployeeGroupAccountStd

        public static EmployeeGroupAccountStd GetEmployeeGroupAccount(this IEnumerable<EmployeeGroupAccountStd> l, EmployeeGroupAccountType type)
        {
            return l?.FirstOrDefault(i => i.Type == (int)type);
        }
        #endregion
    }
}
