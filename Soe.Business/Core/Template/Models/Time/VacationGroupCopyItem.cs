using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class VacationGroupCopyItem
    {
        public int VacationGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public VacationGroupSECopyItem VacationGroupSE { get; set; }
        public int Type { get; set; }
        public DateTime FromDate { get; set; }
        public int? VacationDaysPaidByLaw { get; set; }
    }

    public class VacationGroupSECopyItem
    {
        public int VacationGroupSEId { get; set; }
        public int VacationGroupId { get; set; }

        public TermGroup_VacationGroupCalculationType CalculationType { get; set; }

        public bool UseAdditionalVacationDays { get; set; }
        public int NbrOfAdditionalVacationDays { get; set; }
        public int? AdditionalVacationDaysFromAge1 { get; set; }
        public int? AdditionalVacationDays1 { get; set; }
        public int? AdditionalVacationDaysFromAge2 { get; set; }
        public int? AdditionalVacationDays2 { get; set; }
        public int? AdditionalVacationDaysFromAge3 { get; set; }
        public int? AdditionalVacationDays3 { get; set; }

        public TermGroup_VacationGroupVacationHandleRule VacationHandleRule { get; set; }
        public TermGroup_VacationGroupVacationDaysHandleRule VacationDaysHandleRule { get; set; }
        public bool VacationDaysGrossUseFiveDaysPerWeek { get; set; }

        public TermGroup_VacationGroupRemainingDaysRule RemainingDaysRule { get; set; }
        public bool UseMaxRemainingDays { get; set; }
        public int? MaxRemainingDays { get; set; }
        public int? RemainingDaysPayoutMonth { get; set; }

        public DateTime EarningYearAmountFromDate { get; set; }
        public DateTime? EarningYearVariableAmountFromDate { get; set; }

        public int? MonthlySalaryFormulaId { get; set; }
        public int? HourlySalaryFormulaId { get; set; }

        public decimal? VacationDayPercent { get; set; }
        public decimal? VacationDayAdditionPercent { get; set; }
        public decimal? VacationVariablePercent { get; set; }
        public int? VacationDayPercentPriceTypeId { get; set; }
        public int? VacationDayAdditionPercentPriceTypeId { get; set; }
        public int? VacationVariablePercentPriceTypeId { get; set; }

        public bool UseGuaranteeAmount { get; set; }
        public bool GuaranteeAmountAccordingToHandels { get; set; }
        public TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule GuaranteeAmountMaxNbrOfDaysRule { get; set; }
        public int? GuaranteeAmountEmployedNbrOfYears { get; set; }
        public int? GuaranteeAmountPerDayPriceTypeId { get; set; }
        public bool GuaranteeAmountJuvenile { get; set; }
        public int? GuaranteeAmountJuvenileAgeLimit { get; set; }
        public int? GuaranteeAmountJuvenilePerDayPriceTypeId { get; set; }

        public bool UseFillUpToVacationDaysPaidByLawRule { get; set; }
        public bool UseOwnGuaranteeAmount { get; set; }
        public decimal? OwnGuaranteeAmount { get; set; }

        public TermGroup_VacationGroupVacationAbsenceCalculationRule VacationAbsenceCalculationRule { get; set; }

        public TermGroup_VacationGroupVacationSalaryPayoutRule VacationSalaryPayoutRule { get; set; }
        public int? VacationSalaryPayoutDays { get; set; }
        public int? VacationSalaryPayoutMonth { get; set; }

        public TermGroup_VacationGroupVacationSalaryPayoutRule VacationVariablePayoutRule { get; set; }
        public int? VacationVariablePayoutDays { get; set; }
        public int? VacationVariablePayoutMonth { get; set; }

        public TermGroup_VacationGroupYearEndRemainingDaysRule YearEndRemainingDaysRule { get; set; }
        public TermGroup_VacationGroupYearEndOverdueDaysRule YearEndOverdueDaysRule { get; set; }
        public TermGroup_VacationGroupYearEndVacationVariableRule YearEndVacationVariableRule { get; set; }

        public int? ReplacementTimeDeviationCauseId { get; set; }

        public int? ValueDaysDebitAccountId { get; set; }
        public int? ValueDaysCreditAccountId { get; set; }
        public bool ValueDaysAccountInternalOnDebit { get; set; }
        public bool ValueDaysAccountInternalOnCredit { get; set; }
        public bool UseEmploymentTaxAcccount { get; set; }
        public int? EmploymentTaxDebitAccountId { get; set; }
        public int? EmploymentTaxCredidAccountId { get; set; }
        public bool EmploymentTaxAccountInternalOnDebit { get; set; }
        public bool EmploymentTaxAccountInternalOnCredit { get; set; }
        public bool UseSupplementChargeAccount { get; set; }
        public int? SupplementChargeDebitAccountId { get; set; }
        public int? SupplementChargeCreditAccountId { get; set; }
        public bool SupplementChargeAccountInternalOnDebit { get; set; }
        public bool SupplementChargeAccountInternalOnCredit { get; set; }
    }
}
