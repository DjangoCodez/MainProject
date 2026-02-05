import { IHttpService } from "../../Core/Services/HttpService";
import { IPayrollPriceTypeAndFormulaDTO, IAccountingSettingsRowDTO, IActionResult, ISmallGenericType, IEmployeeSmallDTO, ISelectablePayrollTypeDTO, ITimeWorkAccountDTO, IVacationYearEndResultDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_TimePeriodType, TermGroup_AttestEntity, TermGroup_AttestTreeGrouping, TermGroup_AttestTreeSorting, TermGroup_SysPayrollPrice, TermGroup_VacationYearEndHeadContentType, TermGroup_Currency, TermGroup_TimeTreeWarningFilter } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { PayrollProductGridDTO, PayrollProductDTO, PayrollProductSettingDTO, PayrollProductPriceFormulaDTO, PayrollProductPriceTypeDTO, PayrollProductPriceTypePeriodDTO, ProductSmallDTO } from "../../Common/Models/ProductDTOs";
import { PayrollGroupDTO, PayrollGroupSmallDTO, PayrollGroupPriceTypeDTO, PayrollGroupPriceTypePeriodDTO, PayrollGroupPriceFormulaDTO, PayrollPriceFormulaResultDTO, PayrollGroupVacationGroupDTO, PayrollGroupGridDTO, PayrollGroupPayrollProductDTO, PayrollGroupReportDTO, PayrollGroupSettingDTO, PayrollGroupAccountsDTO, PayrollPriceFormulaDTO } from "../../Common/Models/PayrollGroupDTOs";
import { TimeEmployeeTreeDTO, TimeEmployeeTreeSettings, AttestEmployeeAdditionDeductionDTO, AttestEmployeeAdditionDeductionTransactionDTO, PayrollCalculationEmployeePeriodDTO } from "../../Common/Models/TimeEmployeeTreeDTO";
import { PayrollPriceTypeDTO, PayrollPriceTypeGridDTO, PayrollPriceTypePeriodDTO } from "../../Common/Models/PayrollPriceTypeDTOs";
import { PayrollCalculationProductDTO } from "../../Common/Models/PayrollCalculationProductDTO";
import { SysPayrollPriceDTO, SysPayrollPriceIntervalDTO } from "../../Common/Models/SysPayrollPriceDTO";
import { TimeSalaryPaymentExportEmployeeDTO, TimeSalaryPaymentExportGridDTO } from "../../Common/Models/TimeSalaryExportDTOs";
import { TimePeriodHeadDTO, TimePeriodHeadGridDTO } from "../../Common/Models/TimePeriodHeadDTO";
import { RetroactivePayrollDTO, RetroactivePayrollOutcomeDTO, RetroactivePayrollEmployeeDTO } from "../../Common/Models/RetroactivePayroll";
import { FixedPayrollRowDTO } from "../../Common/Models/FixedPayrollRowDTO";
import { AttestPayrollTransactionDTO } from "../../Common/Models/AttestPayrollTransactionDTO";
import { TimePeriodDTO } from "../../Common/Models/TimePeriodDTO";
import { AccountingSettingsRowDTO } from "../../Common/Models/AccountingSettingsRowDTO";
import { MassRegistrationGridDTO, MassRegistrationTemplateHeadDTO, MassRegistrationTemplateRowDTO } from "../../Common/Models/MassRegistrationDTOs";
import { TimeAccumulatorItem } from "../../Common/Models/TimeAccumulatorDTOs";
import { PayrollImportEmployeeDTO, PayrollImportEmployeeScheduleDTO, PayrollImportEmployeeTransactionDTO, PayrollImportEmployeeTransactionLinkDTO, PayrollImportHeadDTO, PayrollStartValueHeadDTO, PayrollStartValueRowDTO } from "../../Common/Models/PayrollImport";
import { SmallGenericType } from "../../Common/Models/SmallGenericType";
import { DateChart } from "../../Common/Models/ChartDTOs";
import { PayrollLevelDTO } from "../../Common/Models/PayrollLevelDTO";
import { TimeWorkAccountYearEmployeeDTO } from "../../Common/Models/EmployeeUserDTO";
import { ExtraFieldRecordDTO } from "../../Common/Models/ExtraFieldDTO";


export interface IPayrollService {

    // GET
    evaluateFormulaGivenEmployment(date: Date, employmentId: number, productId: number, payrollGroupPriceFormulaId?: number, payrollProductPriceFormulaId?: number, payrollPriceFormulaId?: number, inputValue?: number): ng.IPromise<PayrollPriceFormulaResultDTO>;
    getAllEmployees(): ng.IPromise<any>;
    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean): ng.IPromise<ISmallGenericType[]>;
    getEmployee(employeeId: number, dateFrom: Date, dateTo: Date, includeEmployments: boolean, includeEmployeeGroup: boolean, includePayrollGroup: boolean, includeVacationGroup: boolean, includeEmployeeTax: boolean, taxYear?: number): ng.IPromise<any>;
    getEmployeeForPayrollCalculation(employeeId: number, timePeriod: TimePeriodDTO): ng.IPromise<any>;
    getEmployeesForPayrollCalculationTree(filterIds: any, timePeriodId: number): ng.IPromise<any>;
    getEmployeesWithPayrollExport(timePeriodId: number, payrollGroupId: number): ng.IPromise<IEmployeeSmallDTO[]>;
    getEmployeeVacationPeriod(employeeId: any, timePeriodId: number): ng.IPromise<any>;
    getEmployeeTimePeriod(employeeId: any, timePeriodId: number): ng.IPromise<any>;
    getMassRegistrationsGrid(useCache: boolean): ng.IPromise<MassRegistrationGridDTO[]>;
    getMassRegistrations(loadRows: boolean): ng.IPromise<MassRegistrationTemplateHeadDTO[]>;
    getMassRegistration(massRegistrationTemplateHeadId: number): ng.IPromise<MassRegistrationTemplateHeadDTO>;
    getPayrollGroup(payrollGroupId: number, includePriceTypes: boolean, includePriceFormulas: boolean, includeSettings: boolean, includePayrollGroupReports: boolean, includeTimePeriod: boolean, includeAccounts: boolean, includePayrollGroupVacationGroup: boolean, includePayrollGroupPayrollProduct: boolean): ng.IPromise<PayrollGroupDTO>
    getPayrollGroups(): ng.IPromise<PayrollGroupDTO[]>;
    getPayrollGroupsGrid(useCache: boolean): ng.IPromise<PayrollGroupGridDTO[]>;
    getPayrollGroupsSmall(addEmptyRow: boolean, useCache: boolean): ng.IPromise<PayrollGroupSmallDTO[]>;
    getPayrollGroupAccountDates(sysCountryId: number): ng.IPromise<Date[]>;
    getPayrollGroupPriceFormulas(payrollGroupId: number, showOnEmployee: boolean): ng.IPromise<PayrollGroupPriceFormulaDTO[]>;
    getPayrollGroupPriceTypes(payrollGroupId: number, showOnEmployee: boolean): ng.IPromise<PayrollGroupPriceTypeDTO[]>;
    getPayrollGroupVacationGroups(payrollGroupId: number, loadVacationGroupSE: boolean): ng.IPromise<PayrollGroupVacationGroupDTO[]>;
    getPayrollLevels(): ng.IPromise<PayrollLevelDTO[]>;
    getPayrollLevel(payrollLevelId: number): ng.IPromise<any>;
    getPayrollImportHeads(includeFile: boolean, includeEmployees: boolean, includeScheduleAndTransactionInfo: boolean, setStatuses: boolean): ng.IPromise<PayrollImportHeadDTO[]>;
    getPayrollImportEmployees(payrollImportHeadId: number, includeSchedule: boolean, includeTransactions: boolean, includeLinks: boolean, setStatuses: boolean): ng.IPromise<PayrollImportEmployeeDTO[]>;
    getPayrollImportEmployeeSchedules(payrollImportEmployeeId: number): ng.IPromise<PayrollImportEmployeeScheduleDTO[]>;
    getPayrollImportEmployeeTransactions(payrollImportEmployeeId: number, setStatuses: boolean): ng.IPromise<PayrollImportEmployeeTransactionDTO[]>;
    getPayrollImportEmployeeTransactionLinks(payrollImportEmployeeTransactionId: number): ng.IPromise<PayrollImportEmployeeTransactionLinkDTO[]>;
    getPayrollProducts(useCache: boolean): ng.IPromise<PayrollProductDTO[]>;
    getPayrollProductsForAddedTransactionDialog(useCache: boolean): ng.IPromise<PayrollProductDTO[]>;
    getPayrollProductsDict(addEmptyRow: boolean, concatNumberAndName: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>;
    getPayrollProductsSmall(useCache: boolean): ng.IPromise<ProductSmallDTO[]>;
    getPayrollProductsGrid(useCache: boolean): ng.IPromise<PayrollProductGridDTO[]>;
    getPayrollProductChildren(excludeProductId: number): ng.IPromise<ISmallGenericType[]>;
    getPayrollProduct(productId: number, useCache?: boolean): ng.IPromise<PayrollProductDTO>;
    getPayrollProductReportPrintUrl(productIds: any[], reportId: number, sysReportTemplateId: number): ng.IPromise<any>;
    getPayrollPriceTypesAndFormulas(): ng.IPromise<IPayrollPriceTypeAndFormulaDTO[]>;
    getPayrollPriceFormulas(showInactive?: boolean): ng.IPromise<PayrollPriceFormulaDTO[]>;
    getPayrollPriceFormulasDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getPayrollPriceFormula(payrollPriceFormulaId: number): ng.IPromise<any>;
    getPayrollPriceTypesForFormulaBuilder(): ng.IPromise<any>;
    getFixedValuesForFormulaBuilder(): ng.IPromise<any>;
    getPayrollPriceFormulasForFormulaBuilder(excludedFormulaId: number): ng.IPromise<any>;
    getPayrollPriceTypes(): ng.IPromise<PayrollPriceTypeDTO[]>;
    getPayrollPriceTypesDict(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getPayrollPriceTypesGrid(includePeriods: boolean): ng.IPromise<PayrollPriceTypeGridDTO[]>;
    getPayrollPriceType(payrollPriceTypeId: number, includePeriods: boolean): ng.IPromise<any>;
    getTimePeriodHeads(type: TermGroup_TimePeriodType, loadTypeNames: boolean, loadTimePeriods: boolean): ng.IPromise<TimePeriodHeadDTO[]>;
    getTimePeriodHeadsDict(type: TermGroup_TimePeriodType, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getTimePeriodHeadsForGrid(type: TermGroup_TimePeriodType, loadTypeName: boolean, loadAccountNames: boolean): ng.IPromise<TimePeriodHeadGridDTO[]>
    getTimePeriods(timePeriodHeadId: number): ng.IPromise<TimePeriodDTO[]>;
    getTimePeriodsDict(timePeriodHeadId: number, addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>;
    getTimePayrollTransactionAccountStd(timePayrollTransactionId: number): ng.IPromise<any>;
    getTimeAccumulatorsForEmployee(employeeId: number, startDate: Date, stopDate: Date, addSourceIds: boolean, calculateDay: boolean, calculatePeriod: boolean, calculatePlanningPeriod: boolean, calculateYear: boolean, calculateAccToday: boolean, calculateAccTodayValue: boolean): ng.IPromise<any>;
    getTimeSalaryPaymentExports(): ng.IPromise<TimeSalaryPaymentExportGridDTO[]>;
    getUserValidAttestStates(entity: TermGroup_AttestEntity, dateFrom: Date, dateTo: Date, excludePayrollStates: boolean, employeeGroupId?: number): ng.IPromise<any>;
    getUnionFees(): ng.IPromise<any>;
    getUnionFee(unionFeeId): ng.IPromise<any>;
    getVacationGroupsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getVacationGroupEndDates(vacationGroupIds: number[]): ng.IPromise<any>;
    getVacationYearEnds(): ng.IPromise<any>;
    getEarningYearIsVacationYearVacationDays(vacationGroupId: number, employeeId: number, date: Date, dateFrom?: Date, dateTo?: Date): ng.IPromise<number>;
    getVacationDaysPaidByLaw(employeeId: number, date: Date): ng.IPromise<number>;
    getAccountProvisionBase(timePeriodId: number): ng.IPromise<any>;
    getAccountProvisionBaseColumns(timePeriodId: number): ng.IPromise<any>;
    getAccountProvisionTransactions(timePeriodId: number): ng.IPromise<any>;
    getPayrollStartValueHeads(includeRows: boolean, includePayrollProduct: boolean): ng.IPromise<PayrollStartValueHeadDTO[]>;
    getPayrollStartValueHead(payrollStartValueHeadId: number, includeRows: boolean, includePayrollProduct: boolean, includeTransaction: boolean): ng.IPromise<PayrollStartValueHeadDTO>;
    getPayrollStartValueRows(payrollStartValueHeadId: number, employeeId: number, includeAppellation: boolean, includePayrollProduct: boolean, includeTransaction: boolean): ng.IPromise<PayrollStartValueRowDTO[]>;
    getPayrollStartValueRowTransactions(employeeId: number, payrollStartValueRowId: number): ng.IPromise<any>;
    getPayrollCalculationProducts(timePeriodId: number, employeeId: number, showAllTransactions: boolean): ng.IPromise<PayrollCalculationProductDTO[]>;
    getAttestTransitionLogs(timeBlockDateId: number, employeeId: number, timePayrollTransactionId: number): ng.IPromise<any>;
    getUnhandledPayrollTransactions(employeeId: number, startDate: Date, stopDate: Date, isBackwards: boolean): ng.IPromise<any>;
    getEvaluatedFormulaGivenEmployee(date: Date, employeeId: number, productId: number): ng.IPromise<any>;
    getEmployeeTimePeriodProductSetting(payrollProductId: number, employeeId: number, timePeriodId: number): ng.IPromise<any>;
    getFixedPayrollRows(employeeId: number, timePeriodId: number): ng.IPromise<FixedPayrollRowDTO[]>;
    getPayrollSlipDataStorageId(employeeId: number, timePeriodId: number): ng.IPromise<any>;
    getPayrollSlipURL(employeeId: number, timePeriodId: number, reportId: number): ng.IPromise<any>;
    getPayrollWarnings(employeeId: number, employeeTimePeriodId: number, showDeleted: boolean): ng.IPromise<any>;
    getPayrollWarningsGroup(employeeIds: number[], timePeriodId: number, showDeleted: boolean): ng.IPromise<any>;
    getRetroactivePayrolls(useCache: boolean): ng.IPromise<any>;
    getRetroactivePayrollsForEmployee(timePeriodId: number, employeeId: number): ng.IPromise<RetroactivePayrollDTO[]>;
    getRetroactivePayroll(retroactivePayrollId: number): ng.IPromise<any>;
    getRetroactivePayrollAccounts(retroactivePayrollId: number): ng.IPromise<any>;
    getRetroactivePayrollReviewEmployees(retroactivePayrollId: number): ng.IPromise<any>;
    getRetroactivePayrollOutcomeForEmployee(retroactivePayrollId: number, employeeId: number): ng.IPromise<any>;
    getRetroactivePayrollOutcomeTransactions(employeeId: number, retroactivePayrollOutcomeId: number): ng.IPromise<any>;
    getRetroactivePayrollBasisForOutcome(employeeId: number, retroactivePayrollOutcomeId: number): ng.IPromise<any>;
    getRetroactivePayrollBasisForTransaction(employeeId: number, retroactivePayrollOutcomeId: number, retroactiveTimePayrollTransactionId: number, retroactiveTimePayrollScheduleTransactionId: number): ng.IPromise<any>;
    priceTypesExistsInPayrollGroup(payrollGroupId: number, priceTypeIds: number[]): ng.IPromise<boolean>;
    getAdditionDeductions(employeeId: number, startDate: Date, stopDate: Date, timePeriodId: number): ng.IPromise<AttestEmployeeAdditionDeductionDTO[]>
    getEmploymentTaxBasisBeforeGivenPeriod(timePeriodId: number, employeeId: number): ng.IPromise<number>;
    getSysPayrollPrices(sysCountryId: number, sysPayrollPrices: TermGroup_SysPayrollPrice[], setName: boolean, setTypeName: boolean, setAmountTypeName: boolean, includeIntervals: boolean, onlyLatest: boolean, date: Date): ng.IPromise<SysPayrollPriceDTO[]>;
    getSysPayrollTypes(): ng.IPromise<ISelectablePayrollTypeDTO[]>;
    getSalaryHistoryForEmployee(timePeriodId: number, employeeId: number): ng.IPromise<DateChart>;
    getAverageSalaryCostForEmployees(timePeriodId: number, employeeIds: number[]): ng.IPromise<DateChart>;
    getTimeWorkAccounts(): ng.IPromise<ITimeWorkAccountDTO[]>;
    getEmployeeTimeWorkAccount(employeeId: number, loadAccount: boolean): ng.IPromise<any>;
    getTimeWorkAccountYearEmployee(employeeId: number): ng.IPromise<TimeWorkAccountYearEmployeeDTO[]>;
    getVacationYearEndResult(vacationYearEndHeadId: number) : ng.IPromise<IVacationYearEndResultDTO>;
   
    // POST
    getRetroactivePayrollEmployees(retroactivePayrollId: number, timePeriodId: number, ignoreEmploymentStopDate: boolean, filterEmployeeIds: number[]): ng.IPromise<any>;
    filterRetroactivePayrollEmployeesOnAccount(employees: RetroactivePayrollEmployeeDTO[], accountId: number);
    saveEmployeeNote(note: string, employeeId: number): ng.IPromise<any>;
    saveMassRegistration(massRegistration: MassRegistrationTemplateHeadDTO): ng.IPromise<IActionResult>;
    createMassRegistrationTransactions(massRegistration: MassRegistrationTemplateHeadDTO): ng.IPromise<IActionResult>;
    exportMassRegistration(head: MassRegistrationTemplateHeadDTO): ng.IPromise<any>;
    savePayrollGroup(payrollGroup: PayrollGroupDTO): ng.IPromise<IActionResult>;
    savePayrollImportEmployeeSchedule(schedule: PayrollImportEmployeeScheduleDTO): ng.IPromise<IActionResult>;
    savePayrollImportEmployeeTransaction(trans: PayrollImportEmployeeTransactionDTO): ng.IPromise<IActionResult>;
    setPayrollImportEmployeeTransactionAsProcessed(payrollImportEmployeeTransactionId: number): ng.IPromise<IActionResult>;
    savePayrollWarnings(model: any[]): ng.IPromise<any>;
    validatePayrollImport(payrollImportHeadId: number, payrollImportEmployeeIds: number[]): ng.IPromise<IActionResult>;
    payrollImportExecute(payrollImportHeadId: number, payrollImportEmployeeIds: number[]): ng.IPromise<IActionResult>;
    payrollImportExecuteRollback(payrollImportHeadId: number, payrollImportEmployeeIds: number[], rollbackOutcomeForAllEmployees: boolean): ng.IPromise<IActionResult>;
    payrollImportExecuteRollbackFile(payrollImportHeadId: number, payrollImportEmployeeIds: number[], rollbackFileContentForAllEmployees: boolean): ng.IPromise<IActionResult>;
    savePayrollPriceFormula(payrollPriceFormula: any): ng.IPromise<any>;
    savePayrollPriceType(payrollPriceType: any): ng.IPromise<any>;
    savePayrollProduct(product: PayrollProductDTO): ng.IPromise<IActionResult>;
    savePayrollLevel(payrollLevel: PayrollLevelDTO): ng.IPromise<IActionResult>;
    saveVacationYearEnd(contentType: TermGroup_VacationYearEndHeadContentType, contentTypeIds: number[], date: Date): ng.IPromise<IVacationYearEndResultDTO>;
    validateVacationYearEnd(date: Date, vacationGroupIds: number[], employeeIds: number[]): ng.IPromise<IActionResult>;
    evaluateFormula(formula: string, identifiers: string[]): ng.IPromise<any>;
    saveUnionFee(unionFee: any): ng.IPromise<any>;
    saveAccountProvisionTransactions(transactions: any[]): ng.IPromise<any>;
    changeAttestStateAccountProvisionTransactions(transactions: any[]): ng.IPromise<any>;
    saveAccountProvisionBase(timePeriodAccountValues: any[]): ng.IPromise<any>;
    lockAccountProvisionBase(timePeriodId: number): ng.IPromise<any>;
    unlockAccountProvisionBase(timePeriodId: number): ng.IPromise<any>;
    savePayrollStartValues(startValueRows: any[], payrollStartValueHeadId: number): ng.IPromise<any>;
    saveTransactionsForPayrollStartValue(payrollStartValueHeadId: number, employeeId?: number): ng.IPromise<any>;
    getPayrollCalculationTree(grouping: TermGroup_AttestTreeGrouping, sorting: TermGroup_AttestTreeSorting, timePeriodId: number, settings: TimeEmployeeTreeSettings): ng.IPromise<any>;
    getPayrollCalculationTreeWarnings(tree: TimeEmployeeTreeDTO, timePeriodId: number, employeeIds: number[], warningFilter: TermGroup_TimeTreeWarningFilter, flushCache: boolean): ng.IPromise<any>;
    refreshPayrollCalculationTree(tree: TimeEmployeeTreeDTO, timePeriodId: number, settings: TimeEmployeeTreeSettings): ng.IPromise<any>;
    getPayrollCalculationEmployeePeriods(timePeriodId: number, grouping: TermGroup_AttestTreeGrouping, groupId: number, visibleEmployeeIds: number[], ignoreEmploymentStopDate: boolean, cacheKeyToUse: string): ng.IPromise<PayrollCalculationEmployeePeriodDTO[]>;
    getPayrollCalculationPeriodSum(payrollCalculationProducts: PayrollCalculationProductDTO[]): ng.IPromise<any>;
    recalculatePayrollPeriod(employeeId: number, timePeriodId: number, includeScheduleTransactions: boolean, ignoreEmploymentStopDate: boolean): ng.IPromise<any>;
    recalculatePayrollPeriodForEmployees(key: string, employeeIds: number[], timePeriodId: number, includeScheduleTransactions: boolean, ignoreEmploymentStopDate: boolean): ng.IPromise<any>;
    recalculateAccounting(employeeIds: number[], timePeriodId: number): ng.IPromise<any>;
    recalculateExportedEmploymentTaxForEmployees(employeeIds: number[], timePeriodId: number): ng.IPromise<any>;
    getRecalculatePayrollPeriodForEmployeesResult(key: string): ng.IPromise<any>;
    lockPayrollPeriod(employeeId: number, timePeriodId: number): ng.IPromise<any>;
    lockPayrollPeriodForEmployees(employeeIds: number[], timePeriodId: number): ng.IPromise<any>;
    unLockPayrollPeriod(employeeId: number, timePeriodId: number): ng.IPromise<any>;
    createFinalSalary(employeeId: number, timePeriodId: number, createReport: boolean): ng.IPromise<any>;
    createFinalSalaries(employeeIds: number[], timePeriodId: number, createReport: boolean): ng.IPromise<any>;
    deleteFinalSalaries(employeeIds: number[], timePeriodId: number): ng.IPromise<any>;
    deleteFinalSalary(employeeId: number, timePeriodId: number): ng.IPromise<any>;
    unLockPayrollPeriodForEmployees(employeeIds: number[], timePeriodId: number): ng.IPromise<any>;
    assignPayrollTransactionsToTimePeriod(transactions: AttestPayrollTransactionDTO[], scheduleTransactions: AttestPayrollTransactionDTO[], timePeriod: TimePeriodDTO, periodType: any, employeeId: number): ng.IPromise<any>;
    saveAddedTransaction(transaction: AttestPayrollTransactionDTO, accountingSettings: IAccountingSettingsRowDTO[], employeeId: number, timePeriodId: number, ignoreEmploymentHasEnded: boolean): ng.IPromise<any>;
    saveAttestForTransactionsValidation(items: any, attestStateId: number, isMySelf: boolean): ng.IPromise<any>;
    saveAttestForTransactions(items: any, attestStateId: number, isMySelf: boolean): ng.IPromise<any>;
    saveAttestForEmployees(currentEmployeeId: number, employeeIds: number[], attestStateToId: number, timePeriodId: number, isPayrollAttest: boolean): ng.IPromise<any>;
    saveEmployeeTimePeriodProductSetting(employeeId: number, timePeriodId: number, setting: any): ng.IPromise<any>;
    saveFixedPayrollRows(rows: FixedPayrollRowDTO[], employeeId: number): ng.IPromise<any>;
    updatePayrollProductsState(dict: any): ng.IPromise<IActionResult>;
    saveRetroactivePayroll(retroactivePayroll: RetroactivePayrollDTO): ng.IPromise<any>;
    saveRetroactivePayrollOutcomes(retroactivePayrollId: number, employeeId: number, retroactivePayrollOutcomes: RetroactivePayrollOutcomeDTO[]): ng.IPromise<any>;
    calculateRetroactivePayroll(retroactivePayroll: RetroactivePayrollDTO, includeAlreadyCalculated: boolean, filterEmployeeIds?: number[]): ng.IPromise<any>;
    deleteRetroactivePayrollOutcomes(retroactivePayroll: RetroactivePayrollDTO): ng.IPromise<any>;
    createRetroactivePayrollTransactions(retroactivePayroll: RetroactivePayrollDTO, filterEmployeeIds?: number[]): ng.IPromise<any>;
    deleteRetroactivePayrollTransactions(retroactivePayroll: RetroactivePayrollDTO, filterEmployeeIds?: number[]): ng.IPromise<any>;
    exportSalaryPayment(timePeriodHeadId: number, timePeriodId: number, employeeIds: number[], salarySpecificationPublishDate?: Date, debitDate?: Date): ng.IPromise<IActionResult>;
    exportSalaryPaymentExtendedSelection(basedOnTimeSalarPaymentExportId: number, currencyDate: Date, currencyRate: number, currency: TermGroup_Currency): ng.IPromise<IActionResult>;
    runPayrollControll(employeeId: number[], timePeriodId: number): ng.IPromise<any>;

    exportSalaryPaymentWarnings(timePeriodId: number, employeeIds: number[]): ng.IPromise<string>;
    setSalarySpecificationPublishDate(timeSalaryPaymentExportId: number, salarySpecificationPublishDate: Date): ng.IPromise<any>;

    // DELETE
    deleteMassRegistration(massRegistrationTemplateHeadId: number, deleteTransactions: boolean): ng.IPromise<IActionResult>;
    deletePayrollGroup(payrollGroupId: number): ng.IPromise<IActionResult>;
    deletePayrollLevel(payrollLevelId: number): ng.IPromise<IActionResult>;
    deletePayrollImportEmployeeSchedule(payrollImportEmployeeScheduleId: number): ng.IPromise<IActionResult>;
    deletePayrollImportEmployeeTransaction(payrollImportEmployeeTransactionId: number): ng.IPromise<IActionResult>;
    deletePayrollPriceFormula(payrollPriceFormulaId: number): ng.IPromise<any>;
    deletePayrollPriceType(payrollPriceTypeId: number): ng.IPromise<any>;
    deletePayrollProduct(productId: number): ng.IPromise<IActionResult>;
    deleteVacationYearEnd(vacationYearEndHeadId: number): ng.IPromise<any>;
    deleteUnionFee(unionFeeId: number): ng.IPromise<any>;
    deletePayrollStartValueHead(payrollStartValueHeadId: number): ng.IPromise<any>;
    deleteTransactionsForPayrollStartValue(payrollStartValueHeadId: number, employeeId?: number): ng.IPromise<any>;
    deleteTimePayrollTransaction(timePayrollTransactionId: number, deleteChilds: boolean): ng.IPromise<any>;
    deleteEmployeeTimePeriodProductSetting(employeeTimePeriodProductSettingId: number): ng.IPromise<any>;
    deleteRetroactivePayroll(retroactivePayrollId: number): ng.IPromise<any>;
    deleteSalaryPaymentExport(timeSalaryPaymentExportId: number): ng.IPromise<IActionResult>;
    clearPayrollCalculation(employeeId: number, timePeriodId: number): ng.IPromise<IActionResult>;
}

export class PayrollService implements IPayrollService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    evaluateFormulaGivenEmployment(date: Date, employmentId: number, productId: number, payrollGroupPriceFormulaId?: number, payrollProductPriceFormulaId?: number, payrollPriceFormulaId?: number, inputValue?: number): ng.IPromise<PayrollPriceFormulaResultDTO> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA_EVALUATE_FORMULA_GIVEN_EMPLOYMENT + dateString + "/" + employmentId + "/" + productId + "/" + (payrollGroupPriceFormulaId ? payrollGroupPriceFormulaId : 0) + "/" + (payrollProductPriceFormulaId ? payrollProductPriceFormulaId : 0) + "/" + (payrollPriceFormulaId ? payrollPriceFormulaId : 0) + "/" + (inputValue ? inputValue : 0), false);
    }

    getAllEmployees() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_ALL_EMPLOYEES, false);
    }

    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName + "&getHidden=" + getHidden + "&orderByName=" + orderByName, true);
    }

    getEmployee(employeeId: number, dateFrom: Date, dateTo: Date, includeEmployments: boolean, includeEmployeeGroup: boolean, includePayrollGroup: boolean, includeVacationGroup: boolean, includeEmployeeTax: boolean, taxYear?: number) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE + employeeId + "/" + dateFromString + "/" + dateToString + "/" + includeEmployments + "/" + includeEmployeeGroup + "/" + includePayrollGroup + "/" + includeVacationGroup + "/" + includeEmployeeTax + "/" + taxYear, false);
    }

    getEmployeeForPayrollCalculation(employeeId: number, timePeriod: TimePeriodDTO): ng.IPromise<any> {
        var taxYear = timePeriod.paymentDate?.year() ?? 0;
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_CALCULATION + employeeId + "/" + timePeriod.timePeriodId + "/" + timePeriod.startDate.toDateTimeString() + "/" + timePeriod.stopDate.toDateTimeString() + "/" + taxYear, false);
    }

    getEmployeesForPayrollCalculationTree(filterIds: any, timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_TREE + filterIds + "/" + timePeriodId, true);
    }

    getEmployeesWithPayrollExport(timePeriodId: number, payrollGroupId: number): ng.IPromise<IEmployeeSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PAYROLL_EXPORT + timePeriodId + "/" + payrollGroupId, true);
    }

    getEmployeeVacationPeriod(employeeId: any, timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_EMPLOYEEVACATIONPERIOD + employeeId + "/" + timePeriodId, false);
    }

    getEmployeeTimePeriod(employeeId: number, timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TIME_PERIOD + employeeId + "/" + timePeriodId, false);
    }

    getMassRegistrationsGrid(useCache: boolean): ng.IPromise<MassRegistrationGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new MassRegistrationGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getMassRegistrations(loadRows: boolean): ng.IPromise<MassRegistrationTemplateHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION + "?loadRows=" + loadRows, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj = new MassRegistrationTemplateHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getMassRegistration(massRegistrationTemplateHeadId: number): ng.IPromise<MassRegistrationTemplateHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION + massRegistrationTemplateHeadId, false).then(x => {
            let obj = new MassRegistrationTemplateHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();

            if (x.rows) {
                obj.rows = x.rows.map(r => {
                    let rObj = new MassRegistrationTemplateRowDTO();
                    angular.extend(rObj, r);
                    rObj.fixDates();
                    rObj.setTypes();
                    return rObj;
                });
            } else {
                obj.rows = [];
            }
            return obj;
        });
    }

    getPayrollGroup(payrollGroupId: number, includePriceTypes: boolean, includePriceFormulas: boolean, includeSettings: boolean, includePayrollGroupReports: boolean, includeTimePeriod: boolean, includeAccounts: boolean, includePayrollGroupVacationGroup: boolean, includePayrollGroupPayrollProduct: boolean): ng.IPromise<PayrollGroupDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP + payrollGroupId + "/" + includePriceTypes + "/" + includePriceFormulas + "/" + includeSettings + "/" + includePayrollGroupReports + "/" + includeTimePeriod + "/" + includeAccounts + "/" + includePayrollGroupVacationGroup + "/" + includePayrollGroupPayrollProduct, false).then(x => {
            var obj: PayrollGroupDTO = new PayrollGroupDTO();
            angular.extend(obj, x);

            if (x.accounts) {
                obj.accounts = x.accounts.map(a => {
                    let aObj = new PayrollGroupAccountsDTO();
                    angular.extend(aObj, a);
                    return aObj;
                });
            } else {
                obj.accounts = [];
            }

            if (x.priceTypes) {
                obj.priceTypes = x.priceTypes.map(p => {
                    let pObj = new PayrollGroupPriceTypeDTO();
                    angular.extend(pObj, p);

                    if (p.periods) {
                        pObj.periods = p.periods.map(pr => {
                            let prObj = new PayrollGroupPriceTypePeriodDTO();
                            angular.extend(prObj, pr);
                            prObj.fixDates();
                            return prObj;
                        });
                    } else {
                        pObj.periods = [];
                    }

                    return pObj;
                });
            } else {
                obj.priceTypes = [];
            }

            if (x.priceFormulas) {
                obj.priceFormulas = x.priceFormulas.map(p => {
                    let pObj = new PayrollGroupPriceFormulaDTO();
                    angular.extend(pObj, p);
                    pObj.fixDates();
                    return pObj;
                });
            } else {
                obj.priceFormulas = [];
            }

            if (x.payrollProducts) {
                obj.payrollProducts = x.payrollProducts.map(p => {
                    let pObj = new PayrollGroupPayrollProductDTO();
                    angular.extend(pObj, p);
                    return pObj;
                });
            } else {
                obj.payrollProducts = [];
            }

            if (x.reports) {
                obj.reports = x.reports.map(p => {
                    let pObj = new PayrollGroupReportDTO();
                    angular.extend(pObj, p);
                    return pObj;
                });
            } else {
                obj.reports = [];
            }

            if (x.settings) {
                obj.settings = x.settings.map(p => {
                    let pObj = new PayrollGroupSettingDTO(p.type, p.dataType);
                    angular.extend(pObj, p);
                    pObj.fixDates();
                    return pObj;
                });
            } else {
                obj.settings = [];
            }

            if (x.vacations) {
                obj.vacations = x.vacations.map(p => {
                    let pObj = new PayrollGroupVacationGroupDTO();
                    angular.extend(pObj, p);
                    return pObj;
                });
            } else {
                obj.vacations = [];
            }

            return obj;
        });
    }
    getPayrollLevels() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_LEVEL, false);
    }
    getPayrollLevel(payrollLevelId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_LEVEL + payrollLevelId, false);
    }

    getPayrollGroups() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP, false);
    }

    getPayrollGroupsGrid(useCache: boolean): ng.IPromise<PayrollGroupGridDTO[]> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then((x: PayrollGroupGridDTO[]) => {
            return x.map(y => {
                var obj: PayrollGroupGridDTO = new PayrollGroupGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getPayrollGroupsSmall(addEmptyRow: boolean, useCache: boolean): ng.IPromise<PayrollGroupSmallDTO[]> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then((x: PayrollGroupSmallDTO[]) => {
            return x.map(y => {
                var pgObj: PayrollGroupSmallDTO = new PayrollGroupSmallDTO();
                angular.extend(pgObj, y);

                if (y.priceTypes) {
                    pgObj.priceTypes = y.priceTypes.map(pt => {
                        var ptObj = new PayrollGroupPriceTypeDTO();
                        angular.extend(ptObj, pt);

                        ptObj.periods = ptObj.periods.map(p => {
                            var pObj = new PayrollGroupPriceTypePeriodDTO();
                            angular.extend(pObj, p);
                            pObj.fixDates();

                            return pObj;
                        });

                        return ptObj;
                    });
                }

                return pgObj;
            });
        });
    }

    getPayrollGroupAccountDates(sysCountryId: number): ng.IPromise<Date[]> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP_ACCOUNT_DATES + sysCountryId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getPayrollGroupPriceFormulas(payrollGroupId: number, showOnEmployee: boolean): ng.IPromise<PayrollGroupPriceFormulaDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP_PRICE_FORMULA + payrollGroupId + "/" + showOnEmployee, false);
    }

    getPayrollGroupPriceTypes(payrollGroupId: number, showOnEmployee: boolean): ng.IPromise<PayrollGroupPriceTypeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP_PRICE_TYPE + payrollGroupId + "/" + showOnEmployee, false).then(x => {
            return x.map(y => {
                const obj = new PayrollGroupPriceTypeDTO();
                angular.extend(obj, y);
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollGroupVacationGroups(payrollGroupId: number, loadVacationGroupSE: boolean): ng.IPromise<PayrollGroupVacationGroupDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP_VACATION_GROUP + payrollGroupId + "/" + loadVacationGroupSE, false);
    }

    getPayrollImportHeads(includeFile: boolean, includeEmployees: boolean, includeScheduleAndTransactionInfo: boolean, setStatuses: boolean): ng.IPromise<PayrollImportHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_HEAD + includeFile + "/" + includeEmployees + "/" + includeScheduleAndTransactionInfo + "/" + setStatuses, false).then(x => {
            return x.map(y => {
                let obj = new PayrollImportHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollImportEmployees(payrollImportHeadId: number, includeSchedule: boolean, includeTransactions: boolean, includeLinks: boolean, setStatuses: boolean): ng.IPromise<PayrollImportEmployeeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE + payrollImportHeadId + "/" + includeSchedule + "/" + includeTransactions + "/" + includeLinks + "/" + setStatuses, false).then(x => {
            return x.map(y => {
                let obj = new PayrollImportEmployeeDTO();
                angular.extend(obj, y);
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollImportEmployeeSchedules(payrollImportEmployeeId: number): ng.IPromise<PayrollImportEmployeeScheduleDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_SCHEDULE + payrollImportEmployeeId, false).then(x => {
            return x.map(y => {
                let obj = new PayrollImportEmployeeScheduleDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getPayrollImportEmployeeTransactions(payrollImportEmployeeId: number, setStatuses: boolean): ng.IPromise<PayrollImportEmployeeTransactionDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_TRANSACTION + payrollImportEmployeeId + "/" + setStatuses, false).then(x => {
            return x.map(y => {
                let obj = new PayrollImportEmployeeTransactionDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollImportEmployeeTransactionLinks(payrollImportEmployeeTransactionId: number): ng.IPromise<PayrollImportEmployeeTransactionLinkDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_TRANSACTION_LINK + payrollImportEmployeeTransactionId, false).then(x => {
            return x.map(y => {
                let obj = new PayrollImportEmployeeTransactionLinkDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }
    getPayrollProductsForAddedTransactionDialog(useCache: boolean) {
        return this.getPayrollProducts(useCache, true);
    }

    getPayrollProducts(useCache: boolean, checkValidForAddedTransactionDialog: boolean = false) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT + "?checkValidForAddedTransactionDialog=" + checkValidForAddedTransactionDialog, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            return x.map(y => {
                let obj = new PayrollProductDTO();
                angular.extend(obj, y);

                if (y.settings) {
                    obj.settings = y.settings.map(s => {
                        let sObj = new PayrollProductSettingDTO();
                        angular.extend(sObj, s);

                        if (s.priceFormulas) {
                            sObj.priceFormulas = s.priceFormulas.map(f => {
                                let fObj = new PayrollProductPriceFormulaDTO();
                                angular.extend(fObj, f);
                                fObj.fixDates();
                                return fObj;
                            });
                        } else {
                            sObj.priceFormulas = [];
                        }

                        if (s.priceTypes) {
                            sObj.priceTypes = s.priceTypes.map(t => {
                                let tObj = new PayrollProductPriceTypeDTO();
                                angular.extend(tObj, t);

                                if (t.periods) {
                                    tObj.periods = t.periods.map(p => {
                                        let pObj = new PayrollProductPriceTypePeriodDTO();
                                        angular.extend(pObj, p);
                                        pObj.fixDates();
                                        return pObj;
                                    });
                                } else {
                                    tObj.periods = [];
                                }

                                return tObj;
                            });
                        } else {
                            sObj.priceFormulas = [];
                        }

                        if (s.accountingSettings) {
                            sObj.accountingSettings = s.accountingSettings.map(a => {
                                let aObj = new AccountingSettingsRowDTO(0);
                                angular.extend(aObj, a);
                                return aObj;
                            });
                        } else {
                            sObj.accountingSettings = [];
                        }

                        return sObj;
                    })
                } else {
                    obj.settings = [];
                }

                return obj;
            });
        });
    }

    getPayrollProductsGrid(useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            return x.map(y => {
                let obj = new PayrollProductGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getPayrollProductChildren(excludeProductId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT_CHILDREN + excludeProductId, false);
    }

    getPayrollPriceTypesAndFormulas(): ng.IPromise<IPayrollPriceTypeAndFormulaDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT_PRICETYPES_AND_FORMULAS, false);
    }

    getPayrollProduct(productId: number, useCache: boolean = true): ng.IPromise<PayrollProductDTO> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT + productId, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
            let obj = new PayrollProductDTO();
            angular.extend(obj, x);

            if (x.settings) {
                obj.settings = x.settings.map(s => {
                    let sObj = new PayrollProductSettingDTO;
                    angular.extend(sObj, s);

                    if (s.priceFormulas) {
                        sObj.priceFormulas = s.priceFormulas.map(f => {
                            let fObj = new PayrollProductPriceFormulaDTO;
                            angular.extend(fObj, f);
                            fObj.fixDates();
                            return fObj;
                        });
                    } else {
                        sObj.priceFormulas = [];
                    }

                    if (s.priceTypes) {
                        sObj.priceTypes = s.priceTypes.map(t => {
                            let tObj = new PayrollProductPriceTypeDTO;
                            angular.extend(tObj, t);

                            if (t.periods) {
                                tObj.periods = t.periods.map(p => {
                                    let pObj = new PayrollProductPriceTypePeriodDTO;
                                    angular.extend(pObj, p);
                                    pObj.fixDates();
                                    return pObj;
                                });
                            } else {
                                tObj.periods = [];
                            }

                            return tObj;
                        });
                    } else {
                        sObj.priceTypes = [];
                    }

                    if (s.accountingSettings) {
                        sObj.accountingSettings = s.accountingSettings.map(a => {
                            let aObj = new AccountingSettingsRowDTO(0);
                            angular.extend(aObj, a);
                            return aObj;
                        });
                    } else {
                        sObj.accountingSettings = [];
                    }

                    if (s.extraFields) {
                        sObj.extraFields = s.extraFields.map(e => {
                            let eObj = new ExtraFieldRecordDTO();
                            angular.extend(eObj, e);
                            return eObj;
                        });
                    } else {
                        sObj.extraFields = [];
                    }

                    return sObj;
                });
            } else {
                obj.settings = [];
            }

            return obj;
        });
    }

    getPayrollProductsDict(addEmptyRow: boolean, concatNumberAndName: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getPayrollProductsSmall(useCache: boolean): ng.IPromise<ProductSmallDTO[]> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT_SMALL, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_SHORT, !useCache).then(x => {
            return x.map(y => {
                let obj = new ProductSmallDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getPayrollProductReportPrintUrl(productIds: any[], reportId: number, sysReportTemplateId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_PRINT_PAYROLL_PRODUCT_LIST_PRINT_URL + reportId + "/" + sysReportTemplateId + "/?productIds=" + productIds.join(','), false);
    }

    getPayrollPriceFormulas(showInactive?: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA + "?showInactive=" + showInactive, false);
    }

    getPayrollPriceFormulasDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getPayrollPriceFormula(payrollPriceFormulaId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA + payrollPriceFormulaId, false);
    }

    getPayrollPriceTypesForFormulaBuilder() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA_PAYROLL_PRICE_TYPE, false);
    }

    getFixedValuesForFormulaBuilder() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA_FIXED_VALUE, false);
    }

    getPayrollPriceFormulasForFormulaBuilder(excludedFormulaId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA_PAYROLL_PRICE_FORMULA + excludedFormulaId, false);
    }

    getPayrollPriceTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE, false).then(x => {
            return x.map(y => {
                let obj = new PayrollPriceTypeDTO();
                angular.extend(obj, y);
                obj.fixDates();

                if (y.periods) {
                    obj.periods = y.periods.map(p => {
                        let pObj = new PayrollPriceTypePeriodDTO();
                        angular.extend(pObj, p);
                        pObj.fixDates();
                        return pObj;
                    })
                } else {
                    obj.periods = [];
                }

                return obj;
            });
        });
    }

    getPayrollPriceTypesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getPayrollPriceTypesGrid(includePeriods: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE + "?includePeriods=" + includePeriods, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new PayrollPriceTypeGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getPayrollPriceType(payrollPriceTypeId: number, includePeriods: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE + payrollPriceTypeId + "/" + includePeriods, false);
    }

    getTimePeriodHeads(type: TermGroup_TimePeriodType, loadTypeNames: boolean, loadTimePeriods: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + "?type=" + type + "&loadTypeNames=" + loadTypeNames + "&loadTimePeriods=" + loadTimePeriods, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj = new TimePeriodHeadDTO();
                angular.extend(obj, y);

                if (y.timePeriods) {
                    obj.timePeriods = obj.timePeriods.map(p => {
                        let pObj = new TimePeriodDTO();
                        angular.extend(pObj, p);
                        pObj.fixDates();
                        return pObj;
                    });
                } else {
                    obj.timePeriods = [];
                }

                return obj;
            });
        });
    }

    getTimePeriodHeadsDict(type: TermGroup_TimePeriodType, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + "?type=" + type + "&addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimePeriodHeadsForGrid(type: TermGroup_TimePeriodType, loadTypeNames: boolean, loadAccountNames: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + "?type=" + type + "&loadTypeNames=" + loadTypeNames + "&loadAccountNames=" + loadAccountNames, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimePeriodHeadGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimePeriods(timePeriodHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD + "?timePeriodHeadId=" + timePeriodHeadId, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                let obj = new TimePeriodDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimePeriodsDict(timePeriodHeadId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD + "?timePeriodHeadId=" + timePeriodHeadId + "&addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimePayrollTransactionAccountStd(timePayrollTransactionId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PAYROLL_TRANSACTION_ACCOUNTSTD + timePayrollTransactionId, false);
    }

    getTimeAccumulatorsForEmployee(employeeId: number, startDate: Date, stopDate: Date, addSourceIds: boolean, calculateDay: boolean, calculatePeriod: boolean, calculatePlanningPeriod: boolean, calculateYear: boolean, calculateAccToday: boolean, calculateAccTodayValue: boolean): ng.IPromise<any> {
        var startDateString: string = null;
        if (startDate)
            startDateString = startDate.toDateTimeString();
        var stopDateString: string = null;
        if (stopDate)
            stopDateString = stopDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR + employeeId + "/" + startDateString + "/" + stopDateString + "/" + addSourceIds + "/" + calculateDay + "/" + calculatePeriod + "/" + calculatePlanningPeriod + "/" + calculateYear + "/" + calculateAccToday + "/" + calculateAccTodayValue, false).then(x => {
            return x.map(y => {
                let obj: TimeAccumulatorItem = new TimeAccumulatorItem();
                angular.extend(obj, y);
                obj.createRules();
                return obj;
            });
        });
    }

    getTimeSalaryPaymentExports(): ng.IPromise<TimeSalaryPaymentExportGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYMENT, false).then(x => {
            return x.map(p => {
                var obj = new TimeSalaryPaymentExportGridDTO();
                angular.extend(obj, p);
                obj.fixDates();

                if (p.employees) {
                    obj.employees = obj.employees.map(e => {
                        let eObj = new TimeSalaryPaymentExportEmployeeDTO();
                        angular.extend(eObj, e);
                        return eObj;
                    });
                } else {
                    obj.employees = [];
                }

                return obj;
            });
        });
    }

    getUserValidAttestStates(entity: TermGroup_AttestEntity, dateFrom: Date, dateTo: Date, excludePayrollStates: boolean, employeeGroupId?: number) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        if (!employeeGroupId)
            employeeGroupId = 0;
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_USER_VALID + entity + "/" + dateFromString + "/" + dateToString + "/" + excludePayrollStates + "/" + employeeGroupId, false);
    }

    getUnionFees() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_UNION_FEE, false);
    }

    getUnionFee(unionFeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_UNION_FEE + unionFeeId, false);
    }

    getVacationGroupsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_GROUP + addEmptyRow, false);
    }

    getVacationGroupEndDates(vacationGroupIds: number[]) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_GROUP_END_DATE + "?vacationGroupIds=" + vacationGroupIds.join(','), true);
    }

    getVacationYearEnds() {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_YEAR_END, false);
    }

    getEarningYearIsVacationYearVacationDays(vacationGroupId: number, employeeId: number, date: Date, dateFrom?: Date, dateTo?: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_GET_EARNING_YEAR_IS_VACATION_YEAR_VACATION_DAYS + vacationGroupId + "/" + employeeId + "/" + dateString + "/" + dateFromString + "/" + dateToString, false);
    }

    getVacationDaysPaidByLaw(employeeId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_GET_VACATION_DAYS_PAID_BY_LAW + employeeId + "/" + dateString, false);
    }

    getAccountProvisionBase(timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_BASE + timePeriodId, false);
    }

    getAccountProvisionBaseColumns(timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_BASE_COLUMNS + timePeriodId, false);
    }

    getAccountProvisionTransactions(timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_TRANSACTION + timePeriodId, false);
    }

    getPayrollStartValueHeads(includeRows: boolean, includePayrollProduct: boolean): ng.IPromise<PayrollStartValueHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_HEAD + includeRows + "/" + includePayrollProduct, false).then(x => {
            return x.map(y => {
                let obj = new PayrollStartValueHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getPayrollStartValueHead(payrollStartValueHeadId: number, includeRows: boolean, includePayrollProduct: boolean, includeTransaction: boolean): ng.IPromise<PayrollStartValueHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_HEAD + payrollStartValueHeadId + "/" + includeRows + "/" + includePayrollProduct + "/" + includeTransaction, false).then(x => {
            let obj = new PayrollStartValueHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getPayrollStartValueRows(payrollStartValueHeadId: number, employeeId: number, includeAppellation: boolean, includePayrollProduct: boolean, includeTransaction: boolean): ng.IPromise<PayrollStartValueRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_ROWS + payrollStartValueHeadId + "/" + employeeId + "/" + includeAppellation + "/" + includePayrollProduct + "/" + includeTransaction, false).then(x => {
            return x.map(y => {
                let obj = new PayrollStartValueRowDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getPayrollStartValueRowTransactions(employeeId: number, payrollStartValueRowId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_ROW_TRANSACTIONS + employeeId + "/" + payrollStartValueRowId, false);
    }

    getPayrollCalculationProducts(timePeriodId: number, employeeId: number, showAllTransactions: boolean): ng.IPromise<PayrollCalculationProductDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PRODUCTS + timePeriodId + "/" + employeeId + "/" + showAllTransactions, false).then(x => {
            return x.map(y => {
                let obj = new PayrollCalculationProductDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getAttestTransitionLogs(timeBlockDateId: number, employeeId: number, timePayrollTransactionId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_ATTEST_ATTEST_TRANSITION_LOGS + timeBlockDateId + "/" + employeeId + "/" + timePayrollTransactionId, false);
    }

    getUnhandledPayrollTransactions(employeeId: number, startDate: Date, stopDate: Date, isBackwards: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_GET_UNHANDLED_PAYROLL_TRANSACTIONS + employeeId + "/" + startDate.toDateTimeString() + "/" + stopDate.toDateTimeString() + "/" + isBackwards, false);
    }

    getEvaluatedFormulaGivenEmployee(date: Date, employeeId: number, productId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA_EVALUATE_FORMULA_GIVEN_EMPLOYEE + date.toDateTimeString() + "/" + employeeId + "/" + productId, false);
    }

    getEmployeeTimePeriodProductSetting(payrollProductId: number, employeeId: number, timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_TIME_PERIOD_PRODUCT_SETTING + employeeId + "/" + timePeriodId + "/" + payrollProductId, false);
    }

    getFixedPayrollRows(employeeId: number, timePeriodId: number): ng.IPromise<FixedPayrollRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_FIXED_PAYROLL_ROW + employeeId + "/" + timePeriodId + "/", false).then(x => {
            return x.map(y => {
                let obj = new FixedPayrollRowDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getPayrollSlipDataStorageId(employeeId: number, timePeriodId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PAYROLL_SLIP_DATA_STORAGE_ID + employeeId + "/" + timePeriodId + "/", false);
    }

    getPayrollSlipURL(employeeId: number, timePeriodId: number, reportId: number) {
        return this.httpService.get(Constants.WEBAPI_REPORT_PAYROLLSLIPURL + employeeId + "/" + timePeriodId + "/" + reportId + "/", false);
    }
    getPayrollWarnings(employeeId: number, employeeTimePeriodId: number, showDeleted: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PAYROLL_WARNINGS + employeeId + "/" + employeeTimePeriodId + "/" + showDeleted, false);
    }
    getPayrollWarningsGroup(employeeIds: number[], timePeriodId: number, showDeleted: boolean) {
        var model = {
            timePeriodId: timePeriodId,
            employeeIds: employeeIds,
            showDeleted: showDeleted,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PAYROLL_WARNINGS_GROUP, model);
    }
    getRetroactivePayrolls(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getRetroactivePayrollsForEmployee(timePeriodId: number, employeeId: number): ng.IPromise<RetroactivePayrollDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_EMPLOYEE + timePeriodId + "/" + employeeId, false).then(x => {
            return x.map(y => {
                let obj = new RetroactivePayrollDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getRetroactivePayroll(retroactivePayrollId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE + retroactivePayrollId + "/", false);
    }

    getRetroactivePayrollAccounts(retroactivePayrollId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVEACCOUNT + retroactivePayrollId + "/", false);
    }

    getRetroactivePayrollReviewEmployees(retroactivePayrollId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_REVIEW_EMPLOYEES + retroactivePayrollId + "/", false);
    }

    getRetroactivePayrollOutcomeForEmployee(retroactivePayrollId: number, employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_OUTCOME_EMPLOYEE + retroactivePayrollId + "/" + employeeId + "/", false);
    }

    getRetroactivePayrollOutcomeTransactions(employeeId: number, retroactivePayrollOutcomeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_OUTCOME_TRANSACTION + employeeId + "/" + retroactivePayrollOutcomeId + "/", false);
    }

    getRetroactivePayrollBasisForOutcome(employeeId: number, retroactivePayrollOutcomeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_BASIS_OUTCOME + employeeId + "/" + retroactivePayrollOutcomeId + "/", false);
    }

    getRetroactivePayrollBasisForTransaction(employeeId: number, retroactivePayrollOutcomeId: number, retroactiveTimePayrollTransactionId: number, retroactiveTimePayrollScheduleTransactionId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_BASIS_OUTCOME_TRANSACTION + employeeId + "/" + retroactivePayrollOutcomeId + "/" + retroactiveTimePayrollTransactionId + "/" + retroactiveTimePayrollScheduleTransactionId + "/", false);
    }

    priceTypesExistsInPayrollGroup(payrollGroupId: number, priceTypeIds: number[]): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP_PRICE_TYPES_EXISTS + payrollGroupId + "/" + priceTypeIds.join(','), false);
    }

    getAdditionDeductions(employeeId: number, startDate: Date, stopDate: Date, timePeriodId: number, isMySelf: boolean = false): ng.IPromise<any[]> {
        var startDateString: string = null;
        if (startDate)
            startDateString = startDate.toDateTimeString();
        var stopDateString: string = null;
        if (stopDate)
            stopDateString = stopDate.toDateTimeString();

        var url = Constants.WEBAPI_TIME_ATTEST_EMPLOYEE_ADDITION_DEDUCTION + employeeId + "/" + startDateString + "/" + stopDateString + "/" + timePeriodId + "/" + isMySelf;
        return this.httpService.get(url, false).then(x => {
            return x.map(e => {
                var obj = new AttestEmployeeAdditionDeductionDTO();
                angular.extend(obj, e);
                obj.fixDates();

                if (obj.transactions) {
                    obj.transactions = obj.transactions.map(c => {
                        let cObj = new AttestEmployeeAdditionDeductionTransactionDTO();
                        angular.extend(cObj, c);
                        return cObj;
                    });
                } else {
                    obj.transactions = [];
                }

                return obj;
            });
        });
    }

    getEmploymentTaxBasisBeforeGivenPeriod(timePeriodId: number, employeeId: number) {

        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYMENT_TAX_BASIS_BEFORE_GIVEN_PERIOD + timePeriodId + "/" + employeeId, false);
    }

    getSysPayrollPrices(sysCountryId: number, sysPayrollPrices: TermGroup_SysPayrollPrice[], setName: boolean, setTypeName: boolean, setAmountTypeName: boolean, includeIntervals: boolean, onlyLatest: boolean, date: Date): ng.IPromise<SysPayrollPriceDTO[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_SYSTEM_SYS_PAYROLL_PRICE + sysCountryId + "/" + sysPayrollPrices.join(',') + "/" + setName + "/" + setTypeName + "/" + setAmountTypeName + "/" + includeIntervals + "/" + onlyLatest + "/" + dateString, false).then(x => {
            return x.map(y => {
                let obj = new SysPayrollPriceDTO();
                angular.extend(obj, y);

                if (y.intervals) {
                    obj.intervals = y.intervals.map(i => {
                        let iObj = new SysPayrollPriceIntervalDTO();
                        angular.extend(iObj, i);
                        return iObj;
                    });
                } else {
                    obj.intervals = [];
                }
                return obj;
            });
        });
    }

    public getSysPayrollTypes(): ng.IPromise<ISelectablePayrollTypeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_REPORT_DATA_PAYROLLTYPES, true);
    }

    public getSalaryHistoryForEmployee(timePeriodId: number, employeeId: number): ng.IPromise<DateChart> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_CHART_SALARY_HISTORY + timePeriodId + "/" + employeeId, false).then(x => {
            let obj = new DateChart();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    public getAverageSalaryCostForEmployees(timePeriodId: number, employeeIds: number[]): ng.IPromise<DateChart> {
        var model = {
            timePeriodId: timePeriodId,
            employeeIds: employeeIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEES_CHART_AVERAGE_SALARY_COST, model).then(x => {
            let obj = new DateChart();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    public getTimeWorkAccounts(): ng.IPromise<ITimeWorkAccountDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_TIME_WORK_ACCOUNT , false);
    }

    public getEmployeeTimeWorkAccount(employeeId: number, loadAccount: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_EMPLOYEE_TIME_WORK_ACCOUNT + employeeId + "/" + loadAccount, false);
    }

    public getTimeWorkAccountYearEmployee(employeeId: number): ng.IPromise<TimeWorkAccountYearEmployeeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_TIME_WORK_ACCOUNT_YEAR_EMPLOYEE + employeeId, false);
    }

    public getVacationYearEndResult(vacationYearEndHeadId: number): ng.IPromise<IVacationYearEndResultDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_YEAR_END_RESULT + vacationYearEndHeadId, false);
    }
    
    // POST

    getRetroactivePayrollEmployees(retroactivePayrollId: number, timePeriodId: number, ignoreEmploymentStopDate: boolean, filterEmployeeIds: number[] = null) {
        var model = { retroactivePayrollId: retroactivePayrollId, timePeriodId: timePeriodId, ignoreEmploymentStopDate: ignoreEmploymentStopDate, filterEmployeeIds: filterEmployeeIds }
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVEEMPLOYEE, model);
    }

    filterRetroactivePayrollEmployeesOnAccount(employees: RetroactivePayrollEmployeeDTO[], accountId: number) {
        var model = { accountOrCategoryId: accountId, employees: employees };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVEEMPLOYEE_FILTER, model);
    }

    saveEmployeeNote(note: string, employeeId: number) {
        var model = { note: note, employeeId: employeeId };
        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_NOTE, model);
    }

    saveMassRegistration(massRegistration: MassRegistrationTemplateHeadDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION, massRegistration);
    }

    createMassRegistrationTransactions(massRegistration: MassRegistrationTemplateHeadDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION_CREATE_TRANSACTIONS, massRegistration);
    }

    exportMassRegistration(head: MassRegistrationTemplateHeadDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION_EXPORT, head);
    }

    savePayrollLevel(payrollLevel: PayrollLevelDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_LEVEL, payrollLevel);
    }
    savePayrollGroup(payrollGroup: PayrollGroupDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP, payrollGroup);
    }

    savePayrollImportEmployeeSchedule(schedule: PayrollImportEmployeeScheduleDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_SCHEDULE, schedule);
    }

    savePayrollImportEmployeeTransaction(trans: PayrollImportEmployeeTransactionDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_TRANSACTION, trans);
    }

    setPayrollImportEmployeeTransactionAsProcessed(payrollImportEmployeeTransactionId: number): ng.IPromise<IActionResult> {
        let model = {
            id: payrollImportEmployeeTransactionId
        }

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_TRANSACTION_SET_AS_PROCESSED, model);
    }
    savePayrollWarnings(model: any[]) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PAYROLL_WARNINGS_SAVE, model);
    }
    validatePayrollImport(payrollImportHeadId: number, payrollImportEmployeeIds: number[]): ng.IPromise<IActionResult> {
        let model = {
            payrollImportHeadId: payrollImportHeadId,
            payrollImportEmployeeIds: payrollImportEmployeeIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_HEAD_VALIDATE, model);
    }

    payrollImportExecute(payrollImportHeadId: number, payrollImportEmployeeIds: number[]): ng.IPromise<IActionResult> {
        let model = {
            payrollImportHeadId: payrollImportHeadId,
            payrollImportEmployeeIds: payrollImportEmployeeIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_HEAD_EXECUTE, model);
    }

    payrollImportExecuteRollback(payrollImportHeadId: number, payrollImportEmployeeIds: number[], rollbackOutcomeForAllEmployees: boolean): ng.IPromise<IActionResult> {
        let model = {
            payrollImportHeadId: payrollImportHeadId,
            payrollImportEmployeeIds: payrollImportEmployeeIds,
            rollbackOutcomeForAllEmployees: rollbackOutcomeForAllEmployees,
        }

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_HEAD_EXECUTE_ROLLBACK, model);
    }

    payrollImportExecuteRollbackFile(payrollImportHeadId: number, payrollImportEmployeeIds: number[], rollbackFileContentForAllEmployees: boolean): ng.IPromise<IActionResult> {
        let model = {
            payrollImportHeadId: payrollImportHeadId,
            payrollImportEmployeeIds: payrollImportEmployeeIds,
            rollbackFileContentForAllEmployees: rollbackFileContentForAllEmployees,
        }

        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_HEAD_EXECUTE_ROLLBACK_FILE, model);
    }

    savePayrollPriceFormula(payrollPriceFormula: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA, payrollPriceFormula);
    }

    savePayrollPriceType(payrollPriceType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE, payrollPriceType);
    }

    savePayrollProduct(product: PayrollProductDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT, product);
    }

    saveVacationYearEnd(contentType: TermGroup_VacationYearEndHeadContentType, contentTypeIds: number[], date: Date) {
        let model = {
            contentType: contentType,
            contentTypeIds: contentTypeIds,
            date: date
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_VACATION_YEAR_END, model);
    }

    validateVacationYearEnd(date: Date, vacationGroupIds: number[], employeeIds: number[]) {
        let model = {
            date: date,
            vacationGroupIds: vacationGroupIds,
            employeeIds: employeeIds
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_VACATION_YEAR_END_VALIDATE, model);
    }

    evaluateFormula(formula: string, identifiers: string[]) {
        var model = { formula: formula, identifiers: identifiers };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA_EVALUATE_FORMULA, model);
    }

    saveUnionFee(unionFee: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_UNION_FEE, unionFee);
    }

    saveAccountProvisionTransactions(transactions: any[]) {
        var model = { transactions: transactions };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_TRANSACTION_UPDATE, model);
    }

    saveAccountProvisionBase(timePeriodAccountValues: any[]) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_BASE, timePeriodAccountValues);
    }

    lockAccountProvisionBase(timePeriodId: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_BASE_LOCK + timePeriodId, null);
    }

    unlockAccountProvisionBase(timePeriodId: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_BASE_UNLOCK + timePeriodId, null);
    }

    changeAttestStateAccountProvisionTransactions(transactions: any[]) {
        var model = { transactions: transactions };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_ACCOUNT_PROVISION_TRANSACTION_ATTEST, model);
    }

    savePayrollStartValues(startValueRows: any[], payrollStartValueHeadId: number) {
        var model = { startValueRows: startValueRows, payrollStartValueHeadId: payrollStartValueHeadId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_ROWS_UPDATE, model);
    }

    saveTransactionsForPayrollStartValue(payrollStartValueHeadId: number, employeeId?: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_ROW_TRANSACTIONS + payrollStartValueHeadId + "/" + employeeId, null);
    }

    getPayrollCalculationTree(grouping: TermGroup_AttestTreeGrouping, sorting: TermGroup_AttestTreeSorting, timePeriodId: number, settings: TimeEmployeeTreeSettings) {
        var model = { grouping, sorting, timePeriodId, settings };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_TREE, model).then(t => {
            return TimeEmployeeTreeDTO.createInstance(t);
        });
    }

    getPayrollCalculationTreeWarnings(tree: TimeEmployeeTreeDTO, timePeriodId: number, employeeIds: number[], warningFilter: TermGroup_TimeTreeWarningFilter, flushCache: boolean) {
        var model = { tree, timePeriodId, employeeIds, warningFilter, flushCache };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_TREE_WARNINGS, model).then(t => {
            return TimeEmployeeTreeDTO.createInstance(t);
        });
    }

    refreshPayrollCalculationTree(tree: TimeEmployeeTreeDTO, timePeriodId: number, settings: TimeEmployeeTreeSettings) {
        var model = { tree, timePeriodId, settings };        
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_REFRESH_TREE, model).then(t => {
            return TimeEmployeeTreeDTO.createInstance(t);
        });
    }

    getPayrollCalculationEmployeePeriods(timePeriodId: number, grouping: TermGroup_AttestTreeGrouping, groupId: number, visibleEmployeeIds: number[], ignoreEmploymentStopDate: boolean, cacheKeyToUse: string): ng.IPromise<PayrollCalculationEmployeePeriodDTO[]> {
        var model = { timePeriodId, grouping, groupId, visibleEmployeeIds, cacheKeyToUse, ignoreEmploymentStopDate };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_PERIODS, model).then(x => {
            return x.map(y => {
                let obj = new PayrollCalculationEmployeePeriodDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setSums();
                return obj;
            });
        });
    }

    getPayrollCalculationPeriodSum(payrollCalculationProducts: PayrollCalculationProductDTO[]) {
        var model = { payrollCalculationProducts };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PERIOD_SUM, model);
    }

    recalculatePayrollPeriod(employeeId: number, timePeriodId: number, includeScheduleTransactions: boolean, ignoreEmploymentStopDate: boolean) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_RECALCULATE_PAYROLL_PERIOD + employeeId + "/" + timePeriodId + "/" + includeScheduleTransactions + "/" + ignoreEmploymentStopDate, null);
    }

    recalculatePayrollPeriodForEmployees(key: string, employeeIds: number[], timePeriodId: number, includeScheduleTransactions: boolean, ignoreEmploymentStopDate: boolean) {
        var model = { key: key, employeeIds: employeeIds, timePeriodId: timePeriodId, includeScheduleTransactions: includeScheduleTransactions, ignoreEmploymentStopDate: ignoreEmploymentStopDate };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEES_RECALCULATE_PAYROLL_PERIOD, model);
    }

    recalculateAccounting(employeeIds: number[], timePeriodId: number) {
        var model = { employeeIds: employeeIds, timePeriodId: timePeriodId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_RECALCULATE_ACCOUNTING, model);
    }

    recalculateExportedEmploymentTaxForEmployees(employeeIds: number[], timePeriodId: number) {
        var model = { employeeIds: employeeIds, timePeriodId: timePeriodId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEES_RECALCULATE_EXPORTED_EMPLOYMENT_TAX, model);
    }

    getRecalculatePayrollPeriodForEmployeesResult(key: string) {
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEES_RECALCULATE_PAYROLL_PERIOD_RESULT + key, null);
    }

    lockPayrollPeriod(employeeId: number, timePeriodId: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_LOCK_PAYROLL_PERIOD + employeeId + "/" + timePeriodId, null);
    }

    lockPayrollPeriodForEmployees(employeeIds: number[], timePeriodId: number) {
        var model = { employeeIds: employeeIds, timePeriodId: timePeriodId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEES_LOCK_PAYROLL_PERIOD, model);
    }

    unLockPayrollPeriod(employeeId: number, timePeriodId: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_UNLOCK_PAYROLL_PERIOD + employeeId + "/" + timePeriodId, null);
    }

    unLockPayrollPeriodForEmployees(employeeIds: number[], timePeriodId: number) {
        var model = { employeeIds: employeeIds, timePeriodId: timePeriodId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEES_UNLOCK_PAYROLL_PERIOD, model);
    }

    createFinalSalary(employeeId: number, timePeriodId: number, createReport: boolean) {
        var model = { employeeId: employeeId, timePeriodId: timePeriodId, createReport: createReport };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_CREATE_FINAL_SALARY, model);
    }

    createFinalSalaries(employeeIds: number[], timePeriodId: number, createReport: boolean) {
        var model = { employeeIds: employeeIds, timePeriodId: timePeriodId, createReport: createReport };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_CREATE_FINAL_SALARIES, model);
    }

    deleteFinalSalaries(employeeIds: number[], timePeriodId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_DELETE_FINAL_SALARIES + employeeIds.join(',') + "/" + timePeriodId);
    }
    deleteFinalSalary(employeeId: number, timePeriodId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_DELETE_FINAL_SALARY + employeeId + "/" + timePeriodId);
    }
   
    assignPayrollTransactionsToTimePeriod(transactions: AttestPayrollTransactionDTO[], scheduleTransactions: AttestPayrollTransactionDTO[], timePeriod: TimePeriodDTO, periodType: any, employeeId: number) {
        var model = { transactions: transactions, scheduleTransactions: scheduleTransactions, timePeriod: timePeriod, periodType: periodType, employeeId: employeeId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_ASSIGN_PAYROLL_TRANSACTIONS_TO_TIME_PERIOD, model);
    }

    saveAddedTransaction(transaction: AttestPayrollTransactionDTO, accountingSettings: IAccountingSettingsRowDTO[], employeeId: number, timePeriodId: number, ignoreEmploymentHasEnded: boolean) {
        var model = { transaction: transaction, accountingSettings: accountingSettings, employeeId: employeeId, timePeriodId: timePeriodId, ignoreEmploymentHasEnded: ignoreEmploymentHasEnded };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_ADDED_TRANSACTION, model);
    }

    saveAttestForTransactionsValidation(items: any, attestStateToId: number, isMySelf: boolean) {
        var model = { items: items, attestStateToId: attestStateToId, isMySelf: isMySelf };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TRANSACTIONS_VALIDATION, model);
    }

    saveAttestForTransactions(items: any, attestStateToId: number, isMySelf: boolean) {
        var model = { items: items, attestStateToId: attestStateToId, isMySelf: isMySelf };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TRANSACTIONS, model);
    }

    saveAttestForEmployees(currentEmployeeId: number, employeeIds: number[], attestStateToId: number, timePeriodId: number, isPayrollAttest: boolean) {
        var model = { currentEmployeeId: currentEmployeeId, employeeIds: employeeIds, attestStateToId: attestStateToId, timePeriodId: timePeriodId, isPayrollAttest: isPayrollAttest };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEES, model);
    }

    saveEmployeeTimePeriodProductSetting(employeeId: number, timePeriodId: number, setting: any) {
        var model = { setting: setting, employeeId: employeeId, timePeriodId: timePeriodId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_TIME_PERIOD_PRODUCT_SETTING, model);
    }

    saveFixedPayrollRows(rows: FixedPayrollRowDTO[], employeeId: number) {
        var model = { rows: rows, employeeId: employeeId };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_FIXED_PAYROLL_ROW, model);
    }

    updatePayrollProductsState(dict: any) {
        var model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT_UPDATE_STATE, model);
    }

    saveRetroactivePayroll(retroactivePayroll: RetroactivePayrollDTO) {
        var model = {
            retroactivePayroll: retroactivePayroll,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_SAVE, model);
    }

    saveRetroactivePayrollOutcomes(retroactivePayrollId: number, employeeId: number, retroactivePayrollOutcomes: RetroactivePayrollOutcomeDTO[]) {
        var model = {
            retroactivePayrollId: retroactivePayrollId,
            employeeId: employeeId,
            retroactivePayrollOutcomeDTOs: retroactivePayrollOutcomes
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_OUTCOME_EMPLOYEE_UPDATE, model);
    }

    calculateRetroactivePayroll(retroactivePayroll: RetroactivePayrollDTO, includeAlreadyCalculated: boolean, filterEmployeeIds?: number[]) {
        var model = {
            retroactivePayroll: retroactivePayroll,
            filterEmployeeIds: filterEmployeeIds,
            includeAlreadyCalculated: includeAlreadyCalculated,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_CALCULATE, model);
    }

    deleteRetroactivePayrollOutcomes(retroactivePayroll: RetroactivePayrollDTO): ng.IPromise<any> {
        var model = {
            retroactivePayroll: retroactivePayroll,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_DELETEOUTCOMES, model);
    }

    createRetroactivePayrollTransactions(retroactivePayroll: RetroactivePayrollDTO, filterEmployeeIds?: number[]) {
        var model = {
            retroactivePayroll: retroactivePayroll,
            filterEmployeeIds: filterEmployeeIds,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_CREATETRANSACTIONS, model);
    }

    deleteRetroactivePayrollTransactions(retroactivePayroll: RetroactivePayrollDTO, filterEmployeeIds?: number[]) {
        var model = {
            retroactivePayroll: retroactivePayroll,
            filterEmployeeIds: filterEmployeeIds,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_DELETETRANSACTIONS, model);
    }

    exportSalaryPayment(timePeriodHeadId: number, timePeriodId: number, employeeIds: number[], salarySpecificationPublishDate?: Date, debitDate?: Date): ng.IPromise<IActionResult> {

        var model = {
            timePeriodHeadId: timePeriodHeadId,
            timePeriodId: timePeriodId,
            employeeIds: employeeIds,
            publishDate: salarySpecificationPublishDate,
            debitDate: debitDate
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYMENT, model);
    }

    exportSalaryPaymentWarnings(timePeriodId: number, employeeIds: number[]): ng.IPromise<string> {
        var model = {
            timePeriodId: timePeriodId,
            employeeIds: employeeIds
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYMENT_WARNINGS, model);
    }

    setSalarySpecificationPublishDate(timeSalaryPaymentExportId: number, salarySpecificationPublishDate: Date): ng.IPromise<any> {
        var salarySpecificationPublishDateString: string = null;
        if (salarySpecificationPublishDate)
            salarySpecificationPublishDateString = salarySpecificationPublishDate.toDateTimeString();
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYMENT_SALARY_SPECIFICATION_PUBLISH_DATE + timeSalaryPaymentExportId + "/" + salarySpecificationPublishDateString, false);
    }
    exportSalaryPaymentExtendedSelection(basedOnTimeSalarPaymentExportId: number, currencyDate: Date, currencyRate: number, currency: TermGroup_Currency): ng.IPromise<IActionResult> {
        var model = {
            basedOnTimeSalarPaymentExportId: basedOnTimeSalarPaymentExportId,
            currencyDate: currencyDate,
            currencyRate: currencyRate,
            currency: currency,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYMENTEXTENDED, model);
    }

    runPayrollControll(employeeIds: number[], timePeriodId: number) {

        var model = {
            employeeIds: employeeIds,
            timePeriodId: timePeriodId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_PAYROLL_WARNINGS_RUN, model);
    }

    // DELETE

    deleteMassRegistration(massRegistrationTemplateHeadId: number, deleteTransactions: boolean): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION + massRegistrationTemplateHeadId + "/" + deleteTransactions);
    }

    deletePayrollGroup(payrollGroupId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_GROUP + payrollGroupId);
    }

    deletePayrollLevel(payrollLevelId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_LEVEL + payrollLevelId);
    }
    deletePayrollImportEmployeeSchedule(payrollImportEmployeeScheduleId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_SCHEDULE + payrollImportEmployeeScheduleId);
    }

    deletePayrollImportEmployeeTransaction(payrollImportEmployeeTransactionId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_EMPLOYEE_TRANSACTION + payrollImportEmployeeTransactionId);
    }

    deletePayrollPriceFormula(payrollPriceFormulaId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_FORMULA + payrollPriceFormulaId);
    }

    deletePayrollPriceType(payrollPriceTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRICE_TYPE + payrollPriceTypeId);
    }

    deletePayrollProduct(productId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT + productId);
    }

    deleteVacationYearEnd(vacationYearEndHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_VACATION_YEAR_END + vacationYearEndHeadId);
    }

    deleteUnionFee(unionFeeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_UNION_FEE + unionFeeId);
    }

    deletePayrollStartValueHead(payrollStartValueHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_HEAD + payrollStartValueHeadId);
    }

    deleteTransactionsForPayrollStartValue(payrollStartValueHeadId: number, employeeId?: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_START_VALUE_ROW_TRANSACTIONS + payrollStartValueHeadId + "/" + employeeId);
    }

    deleteTimePayrollTransaction(timePayrollTransactionId: number, deleteChilds: boolean) {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_PAYROLL_TRANSACTION + timePayrollTransactionId + "/" + deleteChilds);
    }

    deleteEmployeeTimePeriodProductSetting(employeeTimePeriodProductSettingId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_EMPLOYEE_TIME_PERIOD_PRODUCT_SETTING + employeeTimePeriodProductSettingId);
    }

    deleteRetroactivePayroll(retroactivePayrollId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_RETROACTIVE_DELETE + retroactivePayrollId);
    }

    deleteSalaryPaymentExport(timeSalaryPaymentExportId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYMENT + timeSalaryPaymentExportId);
    }
    clearPayrollCalculation(employeeId: number, timePeriodId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_PAYROLL_PAYROLLCALCULACTION_CLEAR_PAYROLL_CALCULATION + employeeId + "/" + timePeriodId);
    }
}
