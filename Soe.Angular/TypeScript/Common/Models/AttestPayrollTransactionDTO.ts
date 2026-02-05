import { IAttestPayrollTransactionDTO, IAccountDimDTO, IAccountingSettingsRowDTO, IAccountDTO, IAttestTransitionLogDTO } from "../../Scripts/TypeLite.Net4";
import { SoeTimePayrollScheduleTransactionType, TermGroup_TimeCodeRegistrationType, SoeTimeCodeType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { AccountingSettingsRowDTO } from "./AccountingSettingsRowDTO";

export class AttestPayrollTransactionDTO implements IAttestPayrollTransactionDTO {
    absenceIntervalNr: number;
    accountDims: IAccountDimDTO[];
    accountingDescription: string;
    accountingLongString: string;
    accountingSettings: AccountingSettingsRowDTO[];
    accountingShortString: string;
    accountInternalIds: number[];
    accountInternals: IAccountDTO[];
    accountStd: IAccountDTO;
    accountStdId: number;
    addedDateFrom: Date;
    addedDateTo: Date;
    allTimePayrollTransactionIds: number[];
    amount: number;
    amountCurrency: number;
    amountEntCurrency: number;
    attestItemUniqueId: string;
    attestStateColor: string;
    attestStateId: number;
    attestStateInitial: boolean;
    attestStateName: string;
    attestStateSort: number;
    attestTransitionLogs: IAttestTransitionLogDTO[];
    calenderDayFactor: number;
    comment: string;
    commentGrouping: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    date: Date;
    employeeChildId: number;
    employeeChildName: string;
    employeeId: number;
    employeeVehicleId: number;
    formula: string;
    formulaExtracted: string;
    formulaNames: string;
    formulaOrigin: string;
    formulaPlain: string;
    guidId: string;
    guidIdTimeBlock: string;
    guidIdTimeCodeTransaction: string;
    hasAttestState: boolean;
    hasComment: boolean;
    hasInfo: boolean;
    hasSameAttestState: boolean;
    includedInPayrollProductChain: boolean;
    invoiceQuantity: number;
    isAbsence: boolean;
    isAbsenceAbsenceTime: boolean;
    isAdded: boolean;
    isAddedOrFixed: boolean;
    isAdditionOrDeduction: boolean;
    isAverageCalculated: boolean;
    isBelowEmploymentTaxLimitRuleFromPreviousPeriods: boolean;
    isBelowEmploymentTaxLimitRuleHidden: boolean;
    isCentRounding: boolean;
    isDistributed: boolean;
    isEmployeeVehicle: boolean;
    isEmploymentTaxAndHidden: boolean;
    isExported: boolean;
    isFixed: boolean;
    isModified: boolean;
    isPayrollProductChainMainParent: boolean;
    isPayrollStartValue: boolean;
    isPreliminary: boolean;
    isPresence: boolean;
    isPresenceWorkOutsideScheduleTime: boolean;
    isQuantityOrFixed: boolean;
    isQuantityRounding: boolean;
    isRetroactive: boolean;
    isReversed: boolean;
    isRounding: boolean;
    isScheduleTransaction: boolean;
    isSelectDisabled: boolean;
    isSelected: boolean;
    isSpecifiedUnitPrice: boolean;
    isUnionFee: boolean;
    isVacationFiveDaysPerWeek: boolean;
    isVacationReplacement: boolean;
    isVacationYearEnd: boolean;
    manuallyAdded: boolean;
    noOfAbsenceAbsenceTime: number;
    noOfPresenceWorkOutsideScheduleTime: number;
    parentId: number;
    parentGuidId: string;
    payrollCalculationPerformed: boolean;
    payrollCalculationProductUniqueId: string;
    payrollImportEmployeeTransactionId: number;
    payrollPriceFormulaId: number;
    payrollPriceTypeId: number;
    payrollProductExport: boolean;
    payrollProductFactor: number;
    payrollProductId: number;
    payrollProductName: string;
    payrollProductNumber: string;
    payrollProductPayed: boolean;
    payrollProductShortName: string;
    payrollProductString: string;
    payrollProductSysPayrollTypeLevel1: number;
    payrollProductSysPayrollTypeLevel2: number;
    payrollProductSysPayrollTypeLevel3: number;
    payrollProductSysPayrollTypeLevel4: number;
    payrollProductUseInPayroll: boolean;
    payrollStartValueRowId: number;
    quantity: number;
    quantityCalendarDays: number;
    quantityDays: number;
    quantityString: string;
    quantityWorkDays: number;
    retroactivePayrollOutcomeId: number;
    retroTransactionType: string;
    reversedDate: Date;
    scheduleTransactionType: SoeTimePayrollScheduleTransactionType;
    showEdit: boolean;
    startTime: Date;
    startTimeString: string;
    stopTime: Date;
    stopTimeString: string;
    sysPayrollTypeLevel1: number;
    sysPayrollTypeLevel2: number;
    sysPayrollTypeLevel3: number;
    sysPayrollTypeLevel4: number;
    timeBlockDateId: number;
    timeBlockId: number;
    timeCodeRegistrationType: TermGroup_TimeCodeRegistrationType;
    timeCodeTransactionId: number;
    timeCodeType: SoeTimeCodeType;
    timePayrollTransactionId: number;
    timePeriodId: number;
    timePeriodName: string;
    timeUnit: number;
    transactionSysPayrollTypeLevel1: number;
    transactionSysPayrollTypeLevel2: number;
    transactionSysPayrollTypeLevel3: number;
    transactionSysPayrollTypeLevel4: number;
    unionFeeId: number;
    unitPrice: number;
    unitPriceCurrency: number;
    unitPriceEntCurrency: number;
    unitPriceGrouping: number;
    unitPricePayrollSlipGrouping: number;
    updateChildren: boolean;
    vacationYearEndRowId: number;
    vatAmount: number;
    vatAmountCurrency: number;
    vatAmountEntCurrency: number;

    //Extensions
    isTimeZero: boolean;

    public fixDates() {
        this.addedDateFrom = CalendarUtility.convertToDate(this.addedDateFrom);
        this.addedDateTo = CalendarUtility.convertToDate(this.addedDateTo);
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.date = CalendarUtility.convertToDate(this.date);
        this.reversedDate = CalendarUtility.convertToDate(this.reversedDate);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
        this.isTimeZero = CalendarUtility.isTimeZero(this.startTime) && CalendarUtility.isTimeZero(this.stopTime);
    }

    get quantityTimeFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.quantity);
    }
    set quantityTimeFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.quantity = CalendarUtility.timeSpanToMinutes(span);
    }    

    public static getChain(allTransactions: IAttestPayrollTransactionDTO[], parentTransaction: IAttestPayrollTransactionDTO, chainedTransactions: IAttestPayrollTransactionDTO[]) {
        if (!chainedTransactions)
            chainedTransactions = [];

        if (chainedTransactions.length === 0)
            chainedTransactions.push(parentTransaction);

        var childTransaction = _.find(allTransactions, t => t.parentGuidId && t.parentGuidId === parentTransaction.guidId);
        if (childTransaction) {
            chainedTransactions.push(childTransaction);
            this.getChain(allTransactions, childTransaction, chainedTransactions);
        }
    }

    constructor() {
    }
}
