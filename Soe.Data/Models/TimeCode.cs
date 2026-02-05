using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeCode : ICreatedModified, IState
    {
        public bool IsRegistrationTypeQuantity
        {
            get { return this.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Quantity; }
        }

        public bool IsRegistrationTypeTime
        {
            get
            {
                if (this.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Unknown && (this.Type == (int)SoeTimeCodeType.Work || this.Type == (int)SoeTimeCodeType.Absense))
                    return true;

                return this.RegistrationType == (int)TermGroup_TimeCodeRegistrationType.Time;
            }
        }
        public bool HasInvoiceProducts { get; set; }
    }

    public static partial class EntityExtensions
    {
        public static void SetBaseProperties(this ITimeCodeDTO dto, TimeCode e)
        {
            if (dto == null || e == null)
                return;

            dto.ActorCompanyId = e.ActorCompanyId;
            dto.TimeCodeId = e.TimeCodeId;
            dto.Type = (SoeTimeCodeType)e.Type;
            dto.RegistrationType = (TermGroup_TimeCodeRegistrationType)e.RegistrationType;
            dto.Classification = (TermGroup_TimeCodeClassification)e.Classification;
            dto.Code = e.Code;
            dto.Name = e.Name;
            dto.Description = e.Description;
            dto.RoundingType = (TermGroup_TimeCodeRoundingType)e.RoundingType;
            dto.RoundingValue = e.RoundingValue;
            dto.RoundingTimeCodeId = e.RoundingTimeCodeId;
            dto.RoundingInterruptionTimeCodeId = e.RoundingInterruptionTimeCodeId;
            dto.RoundingGroupKey = e.RoundingGroupKey;
            dto.RoundStartTime = e.RoundStartTime;
            dto.MinutesByConstantRules = e.MinutesByConstantRules;
            dto.FactorBasedOnWorkPercentage = e.FactorBasedOnWorkPercentage;
            dto.Payed = e.Payed;            
            dto.Created = e.Created;
            dto.CreatedBy = e.CreatedBy;
            dto.Modified = e.Modified;
            dto.ModifiedBy = e.ModifiedBy;
            dto.State = (SoeEntityState)e.State;
        }

        public static void SetProductProperties(this TimeCodeBaseDTO dto, TimeCode e)
        {
            if (dto == null || e == null)
                return;

            if (e.TimeCodeInvoiceProduct.IsLoaded)
            {
                dto.InvoiceProducts = e.TimeCodeInvoiceProduct?.ToDTOs() ?? new List<TimeCodeInvoiceProductDTO>();
                DataProjectLogCollector.LogLoadedEntityInExtension("TimeCode.cs dto.InvoiceProducts ");
            }
            if (e.TimeCodePayrollProduct.IsLoaded)
            {
                dto.PayrollProducts = e.TimeCodePayrollProduct?.ToDTOs() ?? new List<TimeCodePayrollProductDTO>();
                DataProjectLogCollector.LogLoadedEntityInExtension("TimeCode.cs e.PayrollProducts");
            }
        }

        public static void SetTimeCodeRuleQuantityProperties(this TimeCodeBaseDTO dto, TimeCode e)
        {
            if (dto == null)
                return;

            TimeCodeRule timeCodeRule = e?.TimeCodeRule?.FirstOrDefault(r => r.IsQuantityRule() && r.State == (int)SoeEntityState.Active);
            if (timeCodeRule != null)
            {
                dto.TimeCodeRuleType = timeCodeRule.Type;
                dto.TimeCodeRuleValue = timeCodeRule.Value;
                dto.TimeCodeRuleTime = timeCodeRule.Time;
            }
        }

        public static void SetAdjustQuantityProperties(this ITimeCodeAdjustQuantity dto, TimeCode e)
        {
            if (dto == null || e == null)
                return;

            dto.AdjustQuantityByBreakTime = (TermGroup_AdjustQuantityByBreakTime)e.AdjustQuantityByBreakTime;
            dto.AdjustQuantityTimeCodeId = e.AdjustQuantityTimeCodeId;
            dto.AdjustQuantityTimeScheduleTypeId = e.AdjustQuantityTimeScheduleTypeId;
        }

        public static IEnumerable<TimeCodeGridDTO> ToGridDTOs(this IEnumerable<TimeCode> l, bool includePayrollProducts, List<GenericType> yesNoTerms, List<GenericType> classifications)
        {
            var dtos = new List<TimeCodeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(includePayrollProducts, yesNoTerms, classifications));
                }
            }
            return dtos;
        }

        public static TimeCodeGridDTO ToGridDTO(this TimeCode e, bool includePayrollProducts, List<GenericType> yesNoTerms, List<GenericType> classifications)
        {
            if (e == null)
                return null;

            TimeCodeGridDTO dto = new TimeCodeGridDTO()
            {
                TimeCodeId = e.TimeCodeId,
                ActorCompanyId = e.ActorCompanyId,
                Code = e.Code,
                Name = e.Name,
                Description = e.Description,
                ClassificationText = classifications?.FirstOrDefault(x => x.Id == e.Classification)?.Name ?? string.Empty,
                State = (SoeEntityState)e.State
            };

            if (includePayrollProducts)
                dto.PayrollProductNames = e.TimeCodePayrollProduct.Where(p => p?.PayrollProduct != null).Select(i => i.PayrollProduct.Name).ToCommaSeparated(addWhiteSpace: true);

            if (e.Type == (int)SoeTimeCodeType.Break && e is TimeCodeBreak)
            {
                TimeCodeBreak timeCodeBreak = e as TimeCodeBreak;
                if (timeCodeBreak.Template)
                    dto.TemplateText = yesNoTerms?.FirstOrDefault(x => x.Id == (int)TermGroup_YesNo.Yes)?.Name ?? string.Empty;
                else
                    dto.TemplateText = yesNoTerms?.FirstOrDefault(x => x.Id == (int)TermGroup_YesNo.No)?.Name ?? string.Empty;

                dto.TimeCodeBreakGroupName = timeCodeBreak.TimeCodeBreakGroup?.Name ?? string.Empty;
                if (timeCodeBreak.EmployeeGroupsForBreak != null && timeCodeBreak.EmployeeGroupsForBreak.Count > 0)
                    dto.TimeCodeBreakEmployeeGroupNames = StringUtility.GetCommaSeparatedString(timeCodeBreak.EmployeeGroupsForBreak.Select(i => i.Name).ToList(), addWhiteSpace: true);
            }

            return dto;
        }

        public static IEnumerable<TimeCodeDTO> ToDTOs(this IEnumerable<TimeCode> l, bool includePayrollProducts, bool orderByName = false)
        {
            var dtos = new List<TimeCodeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePayrollProducts));
                }
            }
            return orderByName ? dtos.OrderBy(x => x.Name).ToList() : dtos;
        }

        public static TimeCodeDTO ToDTO(this TimeCode e, bool includePayrollProducts = false)
        {
            if (e == null)
                return null;

            TimeCodeDTO dto = new TimeCodeDTO();
            dto.SetBaseProperties(e);

            dto.TimeCodeRules = e.TimeCodeRule?.ToDTOs();
            dto.CompanyName = e.Company?.Name ?? string.Empty;
            if (includePayrollProducts)
            {
                dto.PayrollProducts = new List<PayrollProductGridDTO>();
                foreach (var timeCodePayrollProduct in e.TimeCodePayrollProduct)
                {
                    if (timeCodePayrollProduct == null || timeCodePayrollProduct.PayrollProduct == null)
                        continue;

                    dto.PayrollProducts.Add(timeCodePayrollProduct.PayrollProduct.ToGridDTO());
                }
                dto.PayrollProductNames = StringUtility.GetCommaSeparatedString(dto.PayrollProducts.Select(i => i.Name).ToList(), addWhiteSpace: true);
            }

            // Set type specific properties
            switch ((SoeTimeCodeType)e.Type)
            {
                case SoeTimeCodeType.Absense:
                    #region Absense

                    TimeCodeAbsense timeCodeAbsense = (e as TimeCodeAbsense);
                    if (timeCodeAbsense != null)
                    {
                        dto.KontekId = timeCodeAbsense.KontekId;
                        dto.IsAbsence = timeCodeAbsense.IsAbsence;
                    }

                    #endregion
                    break;
                case SoeTimeCodeType.Break:
                    #region Break

                    TimeCodeBreak timeCodeBreak = (e as TimeCodeBreak);
                    if (timeCodeBreak != null)
                    {
                        dto.MinMinutes = timeCodeBreak.MinMinutes;
                        dto.MaxMinutes = timeCodeBreak.MaxMinutes;
                        dto.DefaultMinutes = timeCodeBreak.DefaultMinutes;
                        dto.StartType = timeCodeBreak.StartType;
                        dto.StopType = timeCodeBreak.StopType;
                        dto.StartTime = timeCodeBreak.StartTime;
                        dto.StartTimeMinutes = timeCodeBreak.StartTimeMinutes;
                        dto.StopTimeMinutes = timeCodeBreak.StopTimeMinutes;
                        dto.Template = timeCodeBreak.Template;
                        dto.TimeCodeBreakGroupId = timeCodeBreak.TimeCodeBreakGroup?.TimeCodeBreakGroupId ?? 0;
                        dto.TimeCodeBreakGroupName = timeCodeBreak.TimeCodeBreakGroup?.Name ?? string.Empty;
                        if (timeCodeBreak.EmployeeGroupsForBreak != null && timeCodeBreak.EmployeeGroupsForBreak.Count > 0)
                            dto.TimeCodeBreakEmployeeGroupNames = StringUtility.GetCommaSeparatedString(timeCodeBreak.EmployeeGroupsForBreak.Select(i => i.Name).ToList(), addWhiteSpace: true);
                    }

                    #endregion
                    break;
                case SoeTimeCodeType.Material:
                    #region Material

                    TimeCodeMaterial timeCodeMaterial = (e as TimeCodeMaterial);
                    if (timeCodeMaterial != null)
                        dto.Note = timeCodeMaterial.Note;

                    #endregion
                    break;
                case SoeTimeCodeType.Work:
                    #region Work

                    TimeCodeWork timeCodeWork = (e as TimeCodeWork);
                    if (timeCodeWork != null)
                        dto.IsWorkOutsideSchedule = timeCodeWork.IsWorkOutsideSchedule;

                    #endregion
                    break;
            }

            return dto;
        }

        public static TimeCode FromDTO(this TimeCodeDTO dto)
        {
            if (dto == null)
                return null;

            TimeCode e = new TimeCode()
            {
                TimeCodeId = dto.TimeCodeId,
                ActorCompanyId = dto.ActorCompanyId,
                Type = (int)dto.Type,
                RegistrationType = (int)dto.RegistrationType,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Payed = dto.Payed,
                RoundingType = (int)dto.RoundingType,
                RoundingValue = dto.RoundingValue,
                RoundingTimeCodeId = dto.RoundingTimeCodeId,
                RoundingInterruptionTimeCodeId = dto.RoundingInterruptionTimeCodeId,
                RoundingGroupKey = dto.RoundingGroupKey,
                RoundStartTime = dto.RoundStartTime,
                MinutesByConstantRules = dto.MinutesByConstantRules,
                FactorBasedOnWorkPercentage = dto.FactorBasedOnWorkPercentage,
                Classification = (int)dto.Classification,
                Created = dto.Created,
                CreatedBy = dto.CreatedBy,
                Modified = dto.Modified,
                ModifiedBy = dto.ModifiedBy,
                State = (int)dto.State
            };

            return e;
        }

        public static IEnumerable<TimeCode> FromDTOs(this IEnumerable<TimeCodeDTO> l)
        {
            var dtos = new List<TimeCode>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.FromDTO());
                }
            }
            return dtos;
        }

        public static TimeCodeWorkDTO ToWorkDTO(this TimeCode e)
        {
            TimeCodeWork timeCodeWork = (e as TimeCodeWork);
            if (timeCodeWork == null)
                return null;

            TimeCodeWorkDTO dto = new TimeCodeWorkDTO
            {
                IsWorkOutsideSchedule = timeCodeWork.IsWorkOutsideSchedule,
            };
            dto.SetBaseProperties(e);
            dto.SetProductProperties(e);
            dto.SetAdjustQuantityProperties(e);
            dto.SetTimeCodeRuleQuantityProperties(e);

            return dto;
        }

        public static TimeCodeAbsenceDTO ToAbsenceDTO(this TimeCode e)
        {
            TimeCodeAbsense timeCodeAbsense = (e as TimeCodeAbsense);
            if (timeCodeAbsense == null)
                return null;

            TimeCodeAbsenceDTO dto = new TimeCodeAbsenceDTO
            {
                KontekId = timeCodeAbsense.KontekId,
                IsAbsence = timeCodeAbsense.IsAbsence,
            };
            dto.SetBaseProperties(e);
            dto.SetProductProperties(e);
            dto.SetAdjustQuantityProperties(e);
            dto.SetTimeCodeRuleQuantityProperties(e);

            return dto;
        }

        public static TimeCodeMaterialDTO ToMaterialDTO(this TimeCode e)
        {
            TimeCodeMaterial timeCodeMaterial = (e as TimeCodeMaterial);
            if (timeCodeMaterial == null)
                return null;

            TimeCodeMaterialDTO dto = new TimeCodeMaterialDTO
            {
                Note = timeCodeMaterial.Note,
            };
            dto.SetBaseProperties(e);
            dto.SetProductProperties(e);

            return dto;
        }

        public static List<TimeCodeAdditionDeductionDTO> ToAdditionDeductionDTOs(this IEnumerable<TimeCodeAdditionDeduction> l, bool onlyShowInTerminal = false)
        {
            List<TimeCodeAdditionDeductionDTO> dtos = new List<TimeCodeAdditionDeductionDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (onlyShowInTerminal && !e.ShowInTerminal)
                        continue;

                    dtos.Add(e.ToAdditionDeductionDTO());
                }
            }
            return dtos;
        }

        public static TimeCodeAdditionDeductionDTO ToAdditionDeductionDTO(this TimeCode e)
        {
            TimeCodeAdditionDeduction timeCodeAdditionDeduction = (e as TimeCodeAdditionDeduction);
            if (timeCodeAdditionDeduction == null)
                return null;

            TimeCodeAdditionDeductionDTO dto = new TimeCodeAdditionDeductionDTO
            {
                ExpenseType = (TermGroup_ExpenseType)timeCodeAdditionDeduction.ExpenseType,
                Comment = timeCodeAdditionDeduction.Comment,
                MinutesByConstantRule = timeCodeAdditionDeduction.MinutesByConstantRules,
                StopAtDateStart = timeCodeAdditionDeduction.StopAtDateStart,
                StopAtDateStop = timeCodeAdditionDeduction.StopAtDateStop,
                StopAtPrice = timeCodeAdditionDeduction.StopAtPrice,
                StopAtVat = timeCodeAdditionDeduction.StopAtVat,
                StopAtAccounting = timeCodeAdditionDeduction.StopAtAccounting,
                StopAtComment = timeCodeAdditionDeduction.StopAtComment,
                CommentMandatory = timeCodeAdditionDeduction.CommentMandatory,
                RegistrationType = (TermGroup_TimeCodeRegistrationType)timeCodeAdditionDeduction.RegistrationType,
                HasInvoiceProducts = timeCodeAdditionDeduction.HasInvoiceProducts,
                HideForEmployee = timeCodeAdditionDeduction.HideForEmployee,
                ShowInTerminal = timeCodeAdditionDeduction.ShowInTerminal,
                FixedQuantity = timeCodeAdditionDeduction.FixedQuantity,
            };
            dto.SetBaseProperties(e);
            dto.SetProductProperties(e);

            return dto;
        }

        public static List<TimeCodeBreakDTO> ToBreakDTOs(this List<TimeCode> l)
        {
            var dtos = new List<TimeCodeBreakDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToBreakDTO());
                }
            }
            return dtos;
        }

        public static TimeCodeBreakDTO ToBreakDTO(this TimeCode e)
        {
            TimeCodeBreak timeCodeBreak = (e as TimeCodeBreak);
            if (timeCodeBreak == null)
                return null;

            TimeCodeBreakDTO dto = new TimeCodeBreakDTO
            {
                MinMinutes = timeCodeBreak.MinMinutes,
                MaxMinutes = timeCodeBreak.MaxMinutes,
                DefaultMinutes = timeCodeBreak.DefaultMinutes,
                StartType = timeCodeBreak.StartType,
                StopType = timeCodeBreak.StopType,
                StartTime = timeCodeBreak.StartTime,
                StartTimeMinutes = timeCodeBreak.StartTimeMinutes,
                StopTimeMinutes = timeCodeBreak.StopTimeMinutes,
                Template = timeCodeBreak.Template,
                TimeCodeBreakGroupId = timeCodeBreak.TimeCodeBreakGroupId,
            };
            dto.SetBaseProperties(e);
            dto.SetProductProperties(e);

            if (e.TimeCodeRule.IsLoaded)
                dto.TimeCodeRules = e.TimeCodeRule.ToDTOs().ToList();
            if (timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses.IsLoaded)
                dto.TimeCodeDeviationCauses = timeCodeBreak.TimeCodeBreakTimeCodeDeviationCauses.ToDTOs();
            if (timeCodeBreak.EmployeeGroupsForBreak.IsLoaded)
                dto.EmployeeGroupIds = timeCodeBreak.EmployeeGroupsForBreak?.Select(eg => eg.EmployeeGroupId).ToList() ?? new List<int>();

            return dto;
        }

        public static IEnumerable<TimeCodeBreakSmallDTO> ToSmallBreakDTOs(this IEnumerable<TimeCode> l)
        {
            var dtos = new List<TimeCodeBreakSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallBreakDTO());
                }
            }
            return dtos.OrderBy(i => i.StartTimeMinutes).ToList();
        }

        public static TimeCodeBreakSmallDTO ToSmallBreakDTO(this TimeCode e)
        {
            if (e == null)
                return null;

            TimeCodeBreakSmallDTO dto = new TimeCodeBreakSmallDTO()
            {
                TimeCodeId = e.TimeCodeId,
                Code = e.Code,
                Name = e.Name,
            };

            TimeCodeBreak timeCodeBreak = (e as TimeCodeBreak);
            if (timeCodeBreak != null)
            {
                dto.DefaultMinutes = timeCodeBreak.DefaultMinutes;
                dto.StartTime = timeCodeBreak.StartTime;
                dto.StartTimeMinutes = timeCodeBreak.StartTimeMinutes;
                dto.StopTimeMinutes = timeCodeBreak.StopTimeMinutes;
            }

            return dto;
        }

        public static IQueryable<TimeCode> FilterActive(this IQueryable<TimeCode> l, bool? active)
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

        public static void CopyFrom(this TimeCode timeCode, TimeCode source)
        {
            if (source == null || timeCode == null)
                return;

            timeCode.Type = source.Type;
            timeCode.Code = source.Code;
            timeCode.Name = source.Name;
            timeCode.Payed = source.Payed;
            timeCode.Description = source.Description;
            timeCode.RoundingType = source.RoundingType;
            timeCode.RoundingValue = source.RoundingValue;
            timeCode.RoundStartTime = source.RoundStartTime;
            timeCode.RoundingTimeCodeId = source.RoundingTimeCodeId;
            timeCode.RoundingInterruptionTimeCodeId = source.RoundingInterruptionTimeCodeId;
            timeCode.RoundingGroupKey = source.RoundingGroupKey;
            timeCode.MinutesByConstantRules = source.MinutesByConstantRules;
            timeCode.FactorBasedOnWorkPercentage = source.FactorBasedOnWorkPercentage;
            timeCode.RegistrationType = source.RegistrationType;
            timeCode.Classification = source.Classification;
            timeCode.State = source.State;
        }

        public static bool IsExpense(this TimeCode timeCode)
        {
            if (timeCode is TimeCodeAdditionDeduction timeCodeAdditionDeduction)
                return timeCodeAdditionDeduction.ExpenseType == (int)TermGroup_ExpenseType.Expense;
            return false;
        }

        public static bool IsWork(this TimeCode e)
        {
            return PayrollRulesUtil.IsWork((SoeTimeCodeType)e.Type);
        }

        public static bool IsAbsence(this TimeCode e)
        {
            return PayrollRulesUtil.IsAbsence((SoeTimeCodeType)e.Type);
        }

        public static bool IsBreak(this TimeCode e)
        {
            return PayrollRulesUtil.IsBreak((SoeTimeCodeType)e.Type);
        }

        public static bool IsAdditionAndDeduction(this TimeCode e)
        {
            return PayrollRulesUtil.IsAdditionAndDeduction((SoeTimeCodeType)e.Type);
        }

        public static bool IsMaterial(this TimeCode e)
        {
            return PayrollRulesUtil.IsMaterial((SoeTimeCodeType)e.Type);
        }

        public static bool UseRoundingWholeDay(this TimeCode e)
        {
            return e.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundDownWholeDay || e.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundUpWholeDay;
        }

        public static int RoundTimeCode(this TimeCode e, int minutes)
        {
            return e.RoundTimeCode(false, minutes, null, out _);
        }

        public static int RoundTimeCodeWholeDay(this TimeCode e, int minutes, int? adjustRestAfter, out TermGroup_TimeCodeRoundingType roundingType)
        {
            return e.RoundTimeCode(true, minutes, adjustRestAfter, out roundingType);
        }

        private static int RoundTimeCode(this TimeCode e, bool doRoundWholeDay, int minutes, int? adjustRestAfter, out TermGroup_TimeCodeRoundingType roundingType)
        {
            roundingType = TermGroup_TimeCodeRoundingType.None;
            if (e != null && e.IsValidRounding(doRoundWholeDay))
            {
                roundingType = (TermGroup_TimeCodeRoundingType)e.RoundingType;

                int rest = minutes % e.RoundingValue;
                if (adjustRestAfter.HasValue)
                    minutes = adjustRestAfter.Value;

                if (rest != 0)
                {
                    switch (roundingType)
                    {
                        case TermGroup_TimeCodeRoundingType.RoundDown:
                        case TermGroup_TimeCodeRoundingType.RoundDownWholeDay:
                            minutes -= rest;
                            break;
                        case TermGroup_TimeCodeRoundingType.RoundUp:
                        case TermGroup_TimeCodeRoundingType.RoundUpWholeDay:
                            minutes += (e.RoundingValue - rest);
                            break;
                        default:
                            roundingType = TermGroup_TimeCodeRoundingType.None;
                            break;
                    }
                }
            }

            return minutes;
        }

        public static bool IsValidRounding(this TimeCode e, bool roundWholeDay)
        {
            if (e.RoundingValue <= 0)
                return false;

            return roundWholeDay
                    ?
                    (e.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundDownWholeDay || e.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundUpWholeDay)
                    :
                    (e.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundDown || e.RoundingType == (int)TermGroup_TimeCodeRoundingType.RoundUp);
        }
    }
}
