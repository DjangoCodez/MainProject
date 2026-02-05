import { IVacationGroupDTO, IVacationGroupSEDTO, IVacationGroupSEDayTypeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TermGroup_VacationGroupType, TermGroup_VacationGroupCalculationType, TermGroup_VacationGroupVacationHandleRule, TermGroup_VacationGroupVacationDaysHandleRule, TermGroup_VacationGroupRemainingDaysRule, TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule, TermGroup_VacationGroupVacationAbsenceCalculationRule, TermGroup_VacationGroupVacationSalaryPayoutRule, TermGroup_VacationGroupYearEndRemainingDaysRule, TermGroup_VacationGroupYearEndOverdueDaysRule, TermGroup_VacationGroupYearEndVacationVariableRule, SoeEntityState, SoeVacationGroupDayType } from "../../Util/CommonEnumerations";

export class VacationGroupDTO implements IVacationGroupDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    externalCodes: string[];
    externalCodesString: string;
    fromDate: Date;
    latesVacationYearEnd: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    realDateFrom: Date;
    realDateTo: Date;
    state: SoeEntityState;
    type: TermGroup_VacationGroupType;
    typeName: string;
    vacationDaysPaidByLaw: number;
    vacationGroupId: number;
    vacationGroupSE: VacationGroupSEDTO;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.latesVacationYearEnd = CalendarUtility.convertToDate(this.latesVacationYearEnd);
    }

    public setTypes() {
        let obj = new VacationGroupSEDTO();
        angular.extend(obj, this.vacationGroupSE);
        obj.fixDates();
        this.vacationGroupSE = obj;
    }
}

export class VacationGroupSEDTO implements IVacationGroupSEDTO {
    additionalVacationDays1: number;
    additionalVacationDays2: number;
    additionalVacationDays3: number;
    additionalVacationDaysFromAge1: number;
    additionalVacationDaysFromAge2: number;
    additionalVacationDaysFromAge3: number;
    calculationType: TermGroup_VacationGroupCalculationType;
    created: Date;
    createdBy: string;
    earningYearAmountFromDate: Date;
    earningYearVariableAmountFromDate: Date;
    employmentTaxAccountInternalOnCredit: boolean;
    employmentTaxAccountInternalOnDebit: boolean;
    employmentTaxCredidAccountId: number;
    employmentTaxDebitAccountId: number;
    guaranteeAmountAccordingToHandels: boolean;
    guaranteeAmountEmployedNbrOfYears: number;
    guaranteeAmountJuvenile: boolean;
    guaranteeAmountJuvenileAgeLimit: number;
    guaranteeAmountJuvenilePerDayPriceTypeId: number;
    guaranteeAmountMaxNbrOfDaysRule: TermGroup_VacationGroupGuaranteeAmountMaxNbrOfDaysRule;
    guaranteeAmountPerDayPriceTypeId: number;
    hourlySalaryFormulaId: number;
    maxRemainingDays: number;
    modified: Date;
    modifiedBy: string;
    monthlySalaryFormulaId: number;
    nbrOfAdditionalVacationDays: number;
    ownGuaranteeAmount: number;
    remainingDaysPayoutMonth: number;
    remainingDaysRule: TermGroup_VacationGroupRemainingDaysRule;
    replacementTimeDeviationCauseId: number;
    showHours: boolean;
    supplementChargeAccountInternalOnCredit: boolean;
    supplementChargeAccountInternalOnDebit: boolean;
    supplementChargeCreditAccountId: number;
    supplementChargeDebitAccountId: number;
    useAdditionalVacationDays: boolean;
    useEmploymentTaxAcccount: boolean;
    useFillUpToVacationDaysPaidByLawRule: boolean;
    useGuaranteeAmount: boolean;
    useMaxRemainingDays: boolean;
    useOwnGuaranteeAmount: boolean;
    useSupplementChargeAccount: boolean;
    vacationAbsenceCalculationRule: TermGroup_VacationGroupVacationAbsenceCalculationRule;
    vacationDayAdditionPercent: number;
    vacationDayAdditionPercentPriceTypeId: number;
    vacationDayPercent: number;
    vacationDayPercentPriceTypeId: number;
    vacationDaysGrossUseFiveDaysPerWeek: boolean;
    vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule;
    vacationGroupId: number;
    vacationGroupSEId: number;
    vacationGroupSEDayTypes: VacationGroupSEDayTypeDTO[];
    vacationHandleRule: TermGroup_VacationGroupVacationHandleRule;
    vacationSalaryPayoutDays: number;
    vacationSalaryPayoutMonth: number;
    vacationSalaryPayoutRule: TermGroup_VacationGroupVacationSalaryPayoutRule;
    vacationVariablePayoutDays: number;
    vacationVariablePayoutMonth: number;
    vacationVariablePayoutRule: TermGroup_VacationGroupVacationSalaryPayoutRule;
    vacationVariablePercent: number;
    vacationVariablePercentPriceTypeId: number;
    valueDaysAccountInternalOnCredit: boolean;
    valueDaysAccountInternalOnDebit: boolean;
    valueDaysCreditAccountId: number;
    valueDaysDebitAccountId: number;
    yearEndOverdueDaysRule: TermGroup_VacationGroupYearEndOverdueDaysRule;
    yearEndRemainingDaysRule: TermGroup_VacationGroupYearEndRemainingDaysRule;
    yearEndVacationVariableRule: TermGroup_VacationGroupYearEndVacationVariableRule;

    public fixDates() {
        this.earningYearAmountFromDate = CalendarUtility.convertToDate(this.earningYearAmountFromDate);
        this.earningYearVariableAmountFromDate = CalendarUtility.convertToDate(this.earningYearVariableAmountFromDate);
    }

}
export class VacationGroupSEDayTypeDTO implements IVacationGroupSEDayTypeDTO {
    vacationGroupSEDayTypeId: number;
    dayTypeId: number;
    vacationGroupSEId: number;
    type: SoeVacationGroupDayType;
}
