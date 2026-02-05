using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class VacationGroup : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public DateTime? LatestVacationYearEnd { get; set; }

        public List<string> ExternalCodes { get; set; }
        public string ExternalCodesString { get; set; }
        public bool ExternalCodesIsLoaded { get; set; }

        public int NbrOfDays(DateTime? date = null)
        {
            // Get number of days for current vacation year (will return 365 or 366 if leap year)
            DateTime fromDate = this.ActualFromDate(date);
            DateTime toDate = fromDate.AddYears(1);
            return (int)(toDate - fromDate).TotalDays;
        }

        public void GetActualDates(out DateTime fromDate, out DateTime toDate, out int nbrOfDaysForVacationYear, out int nbrOfDaysForVacationYearToDate, DateTime? date = null)
        {
            fromDate = this.ActualFromDate(date);
            DateTime vacationYearEndDate = fromDate.AddYears(1).AddDays(-1);

            if (!date.HasValue)
                toDate = vacationYearEndDate;
            else
                toDate = date.Value;

            nbrOfDaysForVacationYearToDate = (int)(toDate - fromDate).TotalDays + 1;
            nbrOfDaysForVacationYear = (int)(fromDate.AddYears(1) - fromDate).TotalDays;
        }
        public DateTime ActualFromDate(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            // Get actual start date of current vacation year (stored on vacation group as 1900-MM-01)
            return new DateTime(date.Value.Year - (this.FromDate.Month <= date.Value.Month ? 0 : 1), this.FromDate.Month, 1);
        }
        public DateTime ActualStopDate(DateTime? date = null)
        {
            return this.ActualFromDate(date).AddYears(1).AddDays(-1);
        }
    }

    public static partial class EntityExtensions
    {
        #region VacationGroup

        public static VacationGroupGridDTO ToGridDTO(this VacationGroup e)
        {
            if (e == null)
                return null;

            return new VacationGroupGridDTO()
            {
                VacationGroupId = e.VacationGroupId,
                ActorCompanyId = e.ActorCompanyId,
                FromDate = e.FromDate,
                Name = e.Name,
                Type = (TermGroup_VacationGroupType)e.Type,
                State = (SoeEntityState)e.State,
                TypeName = e.TypeName,
            };
        }

        public static IEnumerable<VacationGroupGridDTO> ToGridDTOs(this IEnumerable<VacationGroup> l)
        {
            var dtos = new List<VacationGroupGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static VacationGroupDTO ToDTO(this VacationGroup e)
        {
            if (e == null)
                return null;

            VacationGroupDTO dto = new VacationGroupDTO()
            {
                VacationGroupId = e.VacationGroupId,
                ActorCompanyId = e.ActorCompanyId,
                Type = (TermGroup_VacationGroupType)e.Type,
                Name = e.Name,
                FromDate = e.FromDate,
                VacationDaysPaidByLaw = e.VacationDaysPaidByLaw,
                VacationGroupSE = e.VacationGroupSE?.FirstOrDefault()?.ToDTO(),
                LatesVacationYearEnd = e.LatestVacationYearEnd,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (!e.ExternalCodes.IsNullOrEmpty())
            {
                dto.ExternalCodes = e.ExternalCodes;
                dto.ExternalCodesString = e.ExternalCodesString;
            }

            return dto;
        }

        public static IEnumerable<VacationGroupDTO> ToDTOs(this IEnumerable<VacationGroup> l)
        {
            var dtos = new List<VacationGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static VacationGroupSEDTO ToDTO(this VacationGroupSE e)
        {
            if (e == null)
                return null;

            return new VacationGroupSEDTO()
            {
                VacationGroupSEId = e.VacationGroupSEId,
                VacationGroupId = e.VacationGroupId,
                CalculationType = (TermGroup_VacationGroupCalculationType)e.CalculationType,
                UseAdditionalVacationDays = e.UseAdditionalVacationDays,
                NbrOfAdditionalVacationDays = e.NbrOfAdditionalVacationDays,
                AdditionalVacationDaysFromAge1 = e.AdditionalVacationDaysFromAge1,
                AdditionalVacationDays1 = e.AdditionalVacationDays1,
                AdditionalVacationDaysFromAge2 = e.AdditionalVacationDaysFromAge2,
                AdditionalVacationDays2 = e.AdditionalVacationDays2,
                AdditionalVacationDaysFromAge3 = e.AdditionalVacationDaysFromAge3,
                AdditionalVacationDays3 = e.AdditionalVacationDays3,
                VacationHandleRule = (TermGroup_VacationGroupVacationHandleRule)e.VacationHandleRule,
                VacationDaysHandleRule = (TermGroup_VacationGroupVacationDaysHandleRule)e.VacationDaysHandleRule,
                VacationDaysGrossUseFiveDaysPerWeek = e.VacationDaysGrossUseFiveDaysPerWeek,
                RemainingDaysRule = (TermGroup_VacationGroupRemainingDaysRule)e.RemainingDaysRule,
                UseMaxRemainingDays = e.UseMaxRemainingDays,
                MaxRemainingDays = e.MaxRemainingDays,
                RemainingDaysPayoutMonth = e.RemainingDaysPayoutMonth,
                EarningYearAmountFromDate = e.EarningYearAmountFromDate,
                EarningYearVariableAmountFromDate = e.EarningYearVariableAmountFromDate,
                MonthlySalaryFormulaId = e.MonthlySalaryFormulaId,
                HourlySalaryFormulaId = e.HourlySalaryFormulaId,
                VacationDayPercent = e.VacationDayPercent,
                VacationDayAdditionPercent = e.VacationDayAdditionPercent,
                VacationVariablePercent = e.VacationVariablePercent,
                VacationDayPercentPriceTypeId = e.VacationDayPercentPriceTypeId,
                VacationDayAdditionPercentPriceTypeId = e.VacationDayAdditionPercentPriceTypeId,
                VacationVariablePercentPriceTypeId = e.VacationVariablePercentPriceTypeId,
                UseGuaranteeAmount = e.UseGuaranteeAmount,
                GuaranteeAmountAccordingToHandels = e.GuaranteeAmountAccordingToHandels,
                GuaranteeAmountMaxNbrOfDaysRule = (TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule)e.GuaranteeAmountMaxNbrOfDaysRule,
                GuaranteeAmountEmployedNbrOfYears = e.GuaranteeAmountEmployedNbrOfYears,
                GuaranteeAmountPerDayPriceTypeId = e.GuaranteeAmountPerDayPriceTypeId,
                GuaranteeAmountJuvenile = e.GuaranteeAmountJuvenile,
                GuaranteeAmountJuvenileAgeLimit = e.GuaranteeAmountJuvenileAgeLimit,
                GuaranteeAmountJuvenilePerDayPriceTypeId = e.GuaranteeAmountJuvenilePerDayPriceTypeId,
                UseOwnGuaranteeAmount = e.UseOwnGuaranteeAmount,
                OwnGuaranteeAmount = e.OwnGuaranteeAmount,
                VacationAbsenceCalculationRule = (TermGroup_VacationGroupVacationAbsenceCalculationRule)e.VacationAbsenceCalculationRule,
                VacationSalaryPayoutRule = (TermGroup_VacationGroupVacationSalaryPayoutRule)e.VacationSalaryPayoutRule,
                VacationSalaryPayoutDays = e.VacationSalaryPayoutDays,
                VacationSalaryPayoutMonth = e.VacationSalaryPayoutMonth,
                VacationVariablePayoutRule = (TermGroup_VacationGroupVacationSalaryPayoutRule)e.VacationVariablePayoutRule,
                VacationVariablePayoutDays = e.VacationVariablePayoutDays,
                VacationVariablePayoutMonth = e.VacationVariablePayoutMonth,
                YearEndRemainingDaysRule = (TermGroup_VacationGroupYearEndRemainingDaysRule)e.YearEndRemainingDaysRule,
                YearEndOverdueDaysRule = (TermGroup_VacationGroupYearEndOverdueDaysRule)e.YearEndOverdueDaysRule,
                YearEndVacationVariableRule = (TermGroup_VacationGroupYearEndVacationVariableRule)e.YearEndVacationVariableRule,
                ReplacementTimeDeviationCauseId = e.ReplacementTimeDeviationCauseId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                ValueDaysDebitAccountId = e.ValueDaysDebitAccountId,
                ValueDaysCreditAccountId = e.ValueDaysCreditAccountId,
                ValueDaysAccountInternalOnDebit = e.ValueDaysAccountInternalOnDebit,
                ValueDaysAccountInternalOnCredit = e.ValueDaysAccountInternalOnCredit,
                UseEmploymentTaxAcccount = e.UseEmploymentTaxAcccount,
                EmploymentTaxDebitAccountId = e.EmploymentTaxDebitAccountId,
                EmploymentTaxCredidAccountId = e.EmploymentTaxCredidAccountId,
                EmploymentTaxAccountInternalOnDebit = e.EmploymentTaxAccountInternalOnDebit,
                EmploymentTaxAccountInternalOnCredit = e.EmploymentTaxAccountInternalOnCredit,
                UseSupplementChargeAccount = e.UseSupplementChargeAccount,
                SupplementChargeDebitAccountId = e.SupplementChargeDebitAccountId,
                SupplementChargeCreditAccountId = e.SupplementChargeCreditAccountId,
                SupplementChargeAccountInternalOnDebit = e.SupplementChargeAccountInternalOnDebit,
                SupplementChargeAccountInternalOnCredit = e.SupplementChargeAccountInternalOnCredit,
                UseFillUpToVacationDaysPaidByLawRule = e.UseFillUpToVacationDaysPaidByLawRule,
                VacationGroupSEDayTypes = e.VacationGroupSEDayType.ToDTOs().ToList()
            };
        }
        public static IEnumerable<VacationGroupSEDayTypeDTO> ToDTOs(this IEnumerable<VacationGroupSEDayType> l)
        {
            var dtos = new List<VacationGroupSEDayTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (e.State == (int)SoeEntityState.Active)
                        dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }
        public static VacationGroupSEDayTypeDTO ToDTO(this VacationGroupSEDayType e)
        {
            if (e == null)
                return null;

            return new VacationGroupSEDayTypeDTO()
            {
                VacationGroupSEDayTypeId = e.VacationGroupSEDayTypeId,
                DayTypeId = e.DayTypeId,
                VacationGroupSEId = e.VacationGroupSEId,
                Type = (SoeVacationGroupDayType)e.Type,
            };
        }
        public static IEnumerable<VacationGroupSEDTO> ToDTOs(this IEnumerable<VacationGroupSE> l)
        {
            var dtos = new List<VacationGroupSEDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool IsPercentCalculation(this VacationGroupSE e)
        {
            return e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw;

        }

        public static bool IsVacationAdditionCalculation(this VacationGroupSE e)
        {
            return e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition;
        }

        public static bool IsEarningYearIsVacationYear(this VacationGroupSE e)
        {
            return e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition;
        }

        public static bool HasVacationVariablePercentSettings(this VacationGroupSE e)
        {
            return !e.VacationVariablePercentPriceTypeId.IsNullOrEmpty() || !e.VacationVariablePercent.IsNullOrEmpty();
        }

        public static bool UseParagraph26(this VacationGroupSE e)
        {
            if (e == null)
                return false;

            return e.RemainingDaysRule == (int)TermGroup_VacationGroupRemainingDaysRule.SavedAccordingToVacationLaw &&
                   (e.YearEndRemainingDaysRule == (int)TermGroup_VacationGroupYearEndRemainingDaysRule.Over20DaysSaved || e.YearEndRemainingDaysRule == (int)TermGroup_VacationGroupYearEndRemainingDaysRule.Saved) &&
                   e.UseFillUpToVacationDaysPaidByLawRule;

        }

        #endregion

        #region VacationGroupSe

        public static DateTime ActualEarningYearAmountFromDate(this VacationGroupSE e, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            // Get actual start date of current vacation year (stored on vacation group as 1900-MM-01)
            return new DateTime(date.Value.Year - (e.EarningYearAmountFromDate.Month <= date.Value.Month ? 0 : 1), e.EarningYearAmountFromDate.Month, e.EarningYearAmountFromDate.Day);
        }

        public static DateTime ActualEarningYearVariableAmountFromDate(this VacationGroupSE e, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            if (!e.EarningYearVariableAmountFromDate.HasValue)
                return CalendarUtility.DATETIME_DEFAULT;

            // Get actual start date of current vacation year (stored on vacation group as 1900-MM-01)
            return new DateTime(date.Value.Year - (e.EarningYearVariableAmountFromDate.Value.Month <= date.Value.Month ? 0 : 1), e.EarningYearVariableAmountFromDate.Value.Month, e.EarningYearVariableAmountFromDate.Value.Day);

        }

        public static bool IsInvertVacationAddition(this VacationGroupSE e)
        {
            return e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_ABAgreement ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsVacationYear_VacationDayAddition;
        }

        public static bool IsInvertVacationAdditionVariable(this VacationGroupSE e)
        {
            return e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToVacationLaw ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_VacationDayAddition_AccordingToCollectiveAgreement;
        }

        public static bool IsInvertVacationSalary(this VacationGroupSE e)
        {
            return e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToVacationLaw ||
                   e.CalculationType == (int)TermGroup_VacationGroupCalculationType.EarningYearIsBeforeVacationYear_PercentCalculation_AccordingToCollectiveAgreement;
        }

        public static void GetActualEarningYearAmountFromDate(this VacationGroupSE e, out DateTime earningYearAmountFromDate, out DateTime earningYearAmountToDate, out int nbrOfDaysForVacationYear, out int nbrOfDaysForVacationYearToDate, DateTime? date = null)
        {
            earningYearAmountFromDate = e.ActualEarningYearAmountFromDate(date);
            DateTime vacationYearEndDate = earningYearAmountFromDate.AddYears(1).AddDays(-1);

            if (!date.HasValue)
                earningYearAmountToDate = vacationYearEndDate;
            else
                earningYearAmountToDate = date.Value;

            nbrOfDaysForVacationYearToDate = (int)(earningYearAmountToDate - earningYearAmountFromDate).TotalDays + 1;
            nbrOfDaysForVacationYear = (int)(earningYearAmountFromDate.AddYears(1) - earningYearAmountFromDate).TotalDays;
        }

        public static DateTime? GetActualVacationVariablePayoutMonthDate(this VacationGroupSE e, DateTime date)
        {
            if (e.VacationVariablePayoutMonth.HasValue && e.VacationVariablePayoutMonth > 0 && e.VacationVariablePayoutMonth < 13)
            {
                GetActualEarningYearAmountFromDate(e, out DateTime actualEarningYearAmountFromDate, out _, out _, out _, date);
                return new DateTime(actualEarningYearAmountFromDate.Year, e.VacationVariablePayoutMonth.Value, 1);
            }
            return null;
        }

        public static DateTime? GetActualVacationSalaryPayoutMonthDate(this VacationGroupSE e, DateTime date)
        {
            if (e.VacationSalaryPayoutMonth.HasValue && e.VacationSalaryPayoutMonth > 0 && e.VacationSalaryPayoutMonth < 13)
            {
                GetActualEarningYearAmountFromDate(e, out DateTime actualEarningYearAmountFromDate, out _, out _, out _, date);
                return new DateTime(actualEarningYearAmountFromDate.Year, e.VacationSalaryPayoutMonth.Value, 1);
            }
            return null;
        }

        public static void GetActualActualEarningYearVariableAmountFromDate(this VacationGroupSE e, out DateTime earningYearVariableAmountFromDate, out DateTime earningYearAmountToDate, out int nbrOfDaysForVacationYear, out int nbrOfDaysForVacationYearToDate, DateTime? date = null)
        {
            earningYearVariableAmountFromDate = e.ActualEarningYearVariableAmountFromDate(date);
            DateTime vacationYearEndDate = earningYearVariableAmountFromDate.AddYears(1).AddDays(-1);

            if (!date.HasValue)
                earningYearAmountToDate = vacationYearEndDate;
            else
                earningYearAmountToDate = date.Value;

            nbrOfDaysForVacationYearToDate = (int)(earningYearAmountToDate - earningYearVariableAmountFromDate).TotalDays + 1;
            nbrOfDaysForVacationYear = (int)(earningYearVariableAmountFromDate.AddYears(1) - earningYearVariableAmountFromDate).TotalDays;
        }

        #endregion
    }
}
