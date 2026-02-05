import { IHttpService } from "../../Core/Services/HttpService";
import { ISmallGenericType, ITimeCodeDTO, ITimeCodeGridDTO, IEmployeeTimeCodeDTO, IProjectTimeBlockSaveDTO, IActionResult, IApplyAbsenceDTO, IAttestPayrollTransactionDTO, ITimeCodeAdditionDeductionDTO, IAccountingPrioDTO, ITimeDeviationCauseDTO, ITimeHibernatingAbsenceHeadDTO, IPeriodCalculationResultDTO } from "../../Scripts/TypeLite.Net4";
import { EmployeeEarnedHolidayDTO } from "../../Common/Models/EmployeeEarnedHolidayDTO";
import { TermGroup_AttestTreeGrouping, TermGroup_AttestTreeSorting, SoeTimeCodeType, TermGroup_TimePeriodType, TermGroup_AttestEntity, SoeTimeAttestFunctionOption, TermGroup_InvoiceProductVatType, SoeTimeBlockClientChange, ProductAccountType, SoeReportTemplateType, TimeTerminalSettingType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { ProjectSmallDTO } from "../../Common/Models/ProjectDTO";
import { TimeTerminalDTO } from "../../Common/Models/TimeTerminalDTO";
import { TimeEmployeeTreeDTO, TimeEmployeeTreeSettings, AttestEmployeeDayDTO, AttestEmployeePeriodDTO, AttestEmployeeDayTimeStampDTO, AttestEmployeeDayTimeBlockDTO, AttestEmployeeDayTimeCodeTransactionDTO, AttestEmployeeAdditionDeductionDTO, AttestEmployeeAdditionDeductionTransactionDTO, AttestEmployeeDaySmallDTO, AttestEmployeesDaySmallDTO, TimeAttestCalculationFunctionValidationDTO, TimeEmployeeTreeGroupNodeDTO, TimeUnhandledShiftChangesEmployeeDTO } from "../../Common/Models/TimeEmployeeTreeDTO";
import { AccountSmallDTO } from "../../Common/Models/AccountDTO";
import { TimeStampAdditionDTO, TimeStampEntryDTO } from "../../Common/Models/TimeStampDTOs";
import { AttestPayrollTransactionDTO } from "../../Common/Models/AttestPayrollTransactionDTO";
import { TimeDeviationCauseDTO, TimeDeviationCauseGridDTO } from "../../Common/Models/TimeDeviationCauseDTOs";
import { ValidateDeviationChangeResult } from "../../Common/Models/AttestEmployeeListDTO";
import { TimeCodeBaseDTO, TimeCodeAbsenceDTO, TimeCodeAdditionDeductionDTO, TimeCodeBreakDTO, TimeCodeMaterialDTO, TimeCodeWorkDTO, TimeCodeSaveDTO, TimeCodeRuleDTO, TimeCodeBreakTimeCodeDeviationCauseDTO, TimeCodeBreakGroupGridDTO } from "../../Common/Models/TimeCode";
import { AttestTransitionLogDTO } from "../../Common/Models/AttestTransitionLogDTO";
import { TimeSalaryExportDTO, TimeSalaryExportSelectionDTO, TimeSalaryExportSelectionEmployeeDTO, TimeSalaryExportSelectionGroupDTO } from "../../Common/Models/TimeSalaryExportDTOs";
import { SmallGenericType } from "../../Common/Models/SmallGenericType";
import { TimeCalendarPeriodDTO, TimeCalendarPeriodPayrollProductDTO } from "../../Common/Models/TimeCalendarDTOs";
import { TimeAbsenceRuleHeadDTO, TimeAbsenceRuleRowDTO } from "../../Common/Models/TimeAbsenceRuleHeadDTO";
import { TimePeriodDTO } from "../../Common/Models/TimePeriodDTO";
import { TimePeriodHeadGridDTO, TimePeriodHeadDTO } from "../../Common/Models/TimePeriodHeadDTO";
import { PayrollProductDTO, PayrollProductSettingDTO, PayrollProductPriceFormulaDTO, PayrollProductPriceTypeDTO, PayrollProductPriceTypePeriodDTO } from "../../Common/Models/ProductDTOs";
import { TimeRuleGridDTO, TimeRuleEditDTO, TimeRuleExportImportDTO, TimeRuleImportedDetailsDTO } from "../../Common/Models/TimeRuleDTOs";
import { FormulaWidget } from "../../Common/Models/FormulaBuilderDTOs";
import { TimeAccumulatorDTO, TimeAccumulatorGridDTO, TimeAccumulatorItem } from "../../Common/Models/TimeAccumulatorDTOs";
import { AccountDimSmallDTO } from "../../Common/Models/AccountDimDTO";
import { StringKeyValue } from "../../Common/Models/StringKeyValue";
import { UserAgentClientInfoDTO } from "../../Common/Models/UserAgentClientInfoDTO";
import { TimeAbsenceDetailDTO } from "../../Common/Models/TimeAbsenceDetailDTO";
import { PlanningPeriodHead } from "../../Common/Models/TimeSchedulePlanningDTOs";
import { MessageGroupDTO } from "../../Common/Models/MessageDTOs";
import { AccountingSettingsRowDTO } from "../../Common/Models/AccountingSettingsRowDTO";

export interface ITimeService {

    // GET
    createTimeStampsAccourdingToSchedule(timeScheduleTemplatePeriodId: number, date: Date, employeeId: number, employeeGroupId: number): ng.IPromise<TimeStampEntryDTO[]>
    getAbsenceDetails(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TimeAbsenceDetailDTO[]>
    getAccountsSmall(accountDimId: number, accountYearId: number, useCache?: boolean, ignoreHierarchyOnly?: boolean): ng.IPromise<AccountSmallDTO[]>
    getAttestTransitionLogs(timeBlockDateId: number, employeeId: number, timePayrollTransactionId: number): ng.IPromise<AttestTransitionLogDTO[]>;
    getHasAttestByEmployeeAccount(date: Date): ng.IPromise<boolean>;
    getEmployeesForTimeAttestTree(filterIds: any, dateFrom: Date, dateTo: Date): ng.IPromise<any>
    getEmployeeVacationPeriod(employeeId: any, dateFrom: Date, dateTo: Date): ng.IPromise<any>
    getEmployeeForUser(): ng.IPromise<IEmployeeTimeCodeDTO>
    getExportedSalaries(): ng.IPromise<TimeSalaryExportDTO[]>
    getTimeSalaryExportSelection(dateFrom: Date, dateTo: Date, accountDimId: number): ng.IPromise<TimeSalaryExportSelectionDTO>
    getInvoiceProductsDict(invoiceProductVatType: TermGroup_InvoiceProductVatType, addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>
    getPayrollProducts(useCache: boolean): ng.IPromise<any>;
    getPayrollProductsDict(addEmptyRow: boolean, concatNumberAndName: boolean, useCache: boolean): ng.IPromise<SmallGenericType[]>
    getPayrollProductAccount(type: ProductAccountType, employeeId: number, productId: number, projectId: number, customerId: number, getInternalAccounts: boolean, date: Date, useCache?: boolean): ng.IPromise<IAccountingPrioDTO>
    getTerminalGroupNames(): ng.IPromise<string[]>;
    getHasAnyTerminalSpecifiedBoolSetting(settingType: TimeTerminalSettingType): ng.IPromise<boolean>;
    getHasAnyTerminalSpecifiedIntSetting(settingType: TimeTerminalSettingType, allowZero: boolean): ng.IPromise<boolean>;
    getAnyTerminalSpecifiedIntSetting(settingType: TimeTerminalSettingType): ng.IPromise<number>;
    getTimeAttestEmployeeDays(gridName: string, employeeId: number, startDate: Date, stopDate: Date, hasDayFilter: boolean, includeProjectTimeBlocks: boolean, includeShifts: boolean, includeTimeStamps: boolean, includeTimeBlocks: boolean, includeTimeCodeTransactions: boolean, includeTimeInvoiceTransactions: boolean, doNotShowDaysOutsideEmployeeAccount: boolean, filterAccountIds: number[], cacheKeyToUse: string): ng.IPromise<AttestEmployeeDayDTO[]>
    getAdditionDeductions(employeeId: number, startDate: Date, stopDate: Date, timePeriodId: number, isMySelf: boolean): ng.IPromise<AttestEmployeeAdditionDeductionDTO[]>
    getTimeAttestFunctionOptionDescription(option: number): ng.IPromise<any>
    getAttestTreeMessageGroups(): ng.IPromise<MessageGroupDTO[]>
    getTimeRules(): ng.IPromise<TimeRuleGridDTO[]>
    getTimeRule(timeRuleId: number): ng.IPromise<TimeRuleEditDTO>
    getTimeAbsenceRules(): ng.IPromise<any>
    getTimeAbsenceRule(timeAbsenceRuleHeadId: number): ng.IPromise<any>
    getTimeAbsenceRuleRows(timeAbsenceRuleHeadId: number): ng.IPromise<any>
    getTimeAccumulators(onlyActive: boolean): ng.IPromise<any>
    getTimeAccumulatorsDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>
    getTimeAccumulatorsForEmployee(employeeId: number, startDate: Date, stopDate: Date, addSourceIds: boolean, calculateDay: boolean, calculatePeriod: boolean, calculatePlanningPeriod: boolean, calculateYear: boolean, calculateAccToday: boolean, calculateAccTodayValue: boolean): ng.IPromise<TimeAccumulatorItem[]>
    getTimeAccumulator(timeAccumulatorId: number, onlyActive: boolean, loadEmployeeGroups: boolean, loadTimeWorkReductionEarning: boolean): ng.IPromise<TimeAccumulatorDTO>;
    getTimeCalendarPeriods(employeeId: number, fromDate: Date, toDate: Date, sysPayrollTypeLevel1?: number, sysPayrollTypeLevel2?: number, sysPayrollTypeLevel3?: number, sysPayrollTypeLevel4?: number): ng.IPromise<TimeCalendarPeriodDTO[]>
    getTimeCodes(timeCodeType: SoeTimeCodeType, onlyActive: boolean, loadPayrollProducts: boolean): ng.IPromise<ITimeCodeDTO[]>
    getTimeCodesDict(timeCodeType: SoeTimeCodeType, addEmptyRow: boolean, concatCodeAndName: boolean, includeType?: boolean): ng.IPromise<ISmallGenericType[]>
    getTimeCodesGrid(timeCodeType: SoeTimeCodeType, onlyActive: boolean, loadPayrollProdcuts: boolean): ng.IPromise<ITimeCodeGridDTO[]>
    getTimeCode(timeCodeType: SoeTimeCodeType, timeCodeId: number, loadInvoiceProducts?: boolean, loadPayrollProducts?: boolean, loadTimeCodeDeviationCauses?: boolean, loadEmployeeGroups?: boolean): ng.IPromise<any>
    getTimeCodeBreakGroups(): ng.IPromise<TimeCodeBreakGroupGridDTO[]>
    getTimeCodeBreakGroup(timeCodeBreakGroupId: number): ng.IPromise<any>
    getTimeDeviationCauses(): ng.IPromise<any>
    getTimeDeviationCausesGrid(): ng.IPromise<TimeDeviationCauseGridDTO[]>
    getTimeDeviationCause(timeDeviationCauseId: number): ng.IPromise<TimeDeviationCauseDTO>
    getTimeDeviationCausesByEmployeeGroupDict(employeeGroupId: number, addEmptyRow: boolean, removeAbsence: boolean): ng.IPromise<any>
    getTimeDeviationCausesByEmployeeGroupGrid(employeeGroupId: number, addEmptyRow: boolean, removeAbsence: boolean): ng.IPromise<TimeDeviationCauseGridDTO[]>
    getTimeDeviationCausesDict(addEmptyRow: boolean, removeAbsence: boolean): ng.IPromise<any>
    getTimeDeviationCauseRequestsDict(employeeGroupId: number, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getTimeDeviationCauseStandardIdFromPrio(employeeId: number, date: Date): ng.IPromise<number>
    getTimeMonthlyReportDataURL(employeeId: number, startDate: Date, stopDate: Date, reportId: number): ng.IPromise<any>;
    getTimePeriodHeadsDict(type: TermGroup_TimePeriodType, addEmptyRow: boolean, accountId?: number): ng.IPromise<ISmallGenericType[]>
    getTimePeriodHeadsForGrid(type: TermGroup_TimePeriodType, loadTypeName: boolean, loadAccountNames: boolean, loadChildNames: boolean): ng.IPromise<TimePeriodHeadGridDTO[]>
    getTimePeriodHead(timePeriodHeadId: number): ng.IPromise<TimePeriodHeadDTO>
    getTimePeriods(timePeriodHeadId: number): ng.IPromise<TimePeriodDTO[]>
    getPeriodsForCalculation(type: TermGroup_TimePeriodType, dateFrom: Date, dateTo: Date, includePeriodsWithoutChildren: boolean): ng.IPromise<IPeriodCalculationResultDTO>
    getTimePeriod(timePeriodHeadId: number, date: Date, loadTimePeriodHead: boolean): ng.IPromise<TimePeriodDTO>
    getPlanningPeriodHeadWithPeriods(timePeriodHeadId: number, date: Date): ng.IPromise<PlanningPeriodHead>
    getTimeRuleImportedDetails(timeRuleId: number, loadDetails: boolean): ng.IPromise<TimeRuleImportedDetailsDTO>;
    getTimeStamp(timeStampEntryId: number): ng.IPromise<TimeStampEntryDTO>
    getTimeStampAdditions(isMySelf: boolean): ng.IPromise<TimeStampAdditionDTO[]>
    getTimeStampUserAgentClientInfo(timeStampEntryId: number): ng.IPromise<UserAgentClientInfoDTO>
    getTimeTerminals(type: number, onlyActive: boolean, onlyRegistered: boolean, onlySynchronized: boolean, loadSettings: boolean, loadCompanies: boolean, loadTypeNames: boolean, ignoreLimitToAccount: boolean): ng.IPromise<TimeTerminalDTO[]>
    getTimeTerminal(timeTerminalId: number): ng.IPromise<TimeTerminalDTO>
    getTimeTerminalAccountDim(timeTerminalId: number, dimNr: number): ng.IPromise<AccountDimSmallDTO>
    getTimeZones(): ng.IPromise<StringKeyValue[]>;
    getUserValidAttestStates(entity: TermGroup_AttestEntity, dateFrom: Date, dateTo: Date, excludePayrollStates: boolean, employeeGroupId?: number)
    getYears(yearsBack: number): ng.IPromise<any>
    getHolidays(year: number, onlyRedDays: boolean, onlyHistorical: boolean, addEmptyRow: boolean): ng.IPromise<any>
    getProjectsForTimeSheetEmployees(employeeIds: number[]): ng.IPromise<ProjectSmallDTO[]>;
    getEmployeesForProjectTimeCode(addEmptyRow: boolean, getHidden: boolean, addNoReplacementEmployee: boolean, includeEmployeeId?: number): ng.IPromise<IEmployeeTimeCodeDTO[]>;
    isEmployeeCurrentUser(employeeId: number): ng.IPromise<boolean>
    getUsesWeekendSalary(): ng.IPromise<boolean>
    getTimeHibernatingAbsence(employeeId: number, employmentId: number): ng.IPromise<ITimeHibernatingAbsenceHeadDTO>
  
    // POST
    getTimeAttestTree(grouping: TermGroup_AttestTreeGrouping, sorting: TermGroup_AttestTreeSorting, startDate: Date, stopDate: Date, timePeriodId: number, settings: TimeEmployeeTreeSettings): ng.IPromise<TimeEmployeeTreeDTO>
    getTimeAttestTreeWarnings(tree: TimeEmployeeTreeDTO, startDate: Date, stopDate: Date, timePeriodId: number, employeeIds: number[], doShowOnlyWithWarnings: boolean, flushCache: boolean): ng.IPromise<TimeEmployeeTreeDTO>
    refreshTimeAttestTree(tree: TimeEmployeeTreeDTO, dateFrom: Date, dateTo: Date, timePeriodId: number, settings: TimeEmployeeTreeSettings): ng.IPromise<TimeEmployeeTreeDTO>
    refreshTimeAttestTreeGroupNode(tree: TimeEmployeeTreeDTO, groupNode: TimeEmployeeTreeGroupNodeDTO): ng.IPromise<TimeEmployeeTreeGroupNodeDTO>
    recalculateUnhandledShiftChangesEmployees(unhandledEmployees: TimeUnhandledShiftChangesEmployeeDTO[], doRecalculateShifts: boolean, doRecalculateExtraShifts: boolean): ng.IPromise<IActionResult>
    getTimeMonthlyReportMultipleEmployeesDataURL(employeeIds: number[], startDate: Date, stopDate: Date, reportId: number, reportTemplateType: SoeReportTemplateType): ng.IPromise<any>
    loadEarnedHolidaysContent(holidayId: number, yearId: number, loadSuggestions: boolean, employeeEarnedHolidays: EmployeeEarnedHolidayDTO[]): ng.IPromise<EmployeeEarnedHolidayDTO[]>
    saveEarnedHolidayTransactions(holidayId: number, employeeIds: number[], yearId: number): ng.IPromise<any>
    saveTimePeriodHead(timePeriodHead: TimePeriodHeadDTO, removePeriodLinks: boolean): ng.IPromise<IActionResult>
    saveTimeCodeBreakGroup(breakGroup: any): ng.IPromise<any>
    saveAttestForEmployees(currentEmployeeId: number, employeeIds: number[], attestStateToId: number, startDate?: Date, stopDate?: Date): ng.IPromise<any>
    saveAttestForEmployeeValidation(items: any[], employeeId: number, attestStateToId: number, isMySelf: boolean): ng.IPromise<any>
    saveAttestForEmployee(items: any[], employeeId: number, attestStateToId: number, isMySelf: boolean): ng.IPromise<any>
    saveAttestForTransactionsValidation(items: any, attestStateId: number, isMySelf: boolean): ng.IPromise<any>
    saveAttestForTransactions(items: any, attestStateId: number, isMySelf: boolean): ng.IPromise<any>
    saveAttestForAdditionDeductionsValidation(transactionItems: any, employeeId: number, attestStateId: number, isMySelf: boolean): ng.IPromise<any>
    unlockDay(items: any[], employeeId: number): ng.IPromise<any>
    saveTimeStampEntries(entries: AttestEmployeeDayTimeStampDTO[], date: Date, discardBreakEvaluation: boolean, employeeId: number): ng.IPromise<IActionResult>
    validateDeviationChange(employeeId: number, timeBlockId: number, timeBlockGuidId: string, timeBlocks: AttestEmployeeDayTimeBlockDTO[], timeScheduleTemplatePeriodId: number, date: Date, startTime: Date, stopTime: Date, clientChange: SoeTimeBlockClientChange, onlyUseInTimeTerminal: boolean, timeDeviationCauseId: number, employeeChildId: number, comment: string, accountSetting?: AccountingSettingsRowDTO): ng.IPromise<ValidateDeviationChangeResult>
    saveGeneratedDeviations(timeBlocks: AttestEmployeeDayTimeBlockDTO[], timeCodeTransactions: AttestEmployeeDayTimeCodeTransactionDTO[], timePayrollTransactions: IAttestPayrollTransactionDTO[], applyAbsences: IApplyAbsenceDTO[], timeBlockDateId: number, timeScheduleTemplatePeriodId: number, employeeId: number, payrollImportEmployeeTransactionIds: number[]): ng.IPromise<IActionResult>
    recalculateTimeAccumulators(timeAccumulatorIds: number[]): ng.IPromise<any>
    getTimeAttestEmployeePeriods(dateFrom: Date, dateTo: Date, grouping: TermGroup_AttestTreeGrouping, groupId: number, visibleEmployeeIds: number[], isAdditional: boolean, includeAdditionalEmployees: boolean, doNotShowDaysOutsideEmployeeAccount: boolean, cacheKeyToUse: string, flushCache: boolean, timePeriodId?: number): ng.IPromise<AttestEmployeePeriodDTO[]>
    getTimeAttestEmployeePeriodsPreview(tree: TimeEmployeeTreeDTO, groupNode: TimeEmployeeTreeGroupNodeDTO): ng.IPromise<AttestEmployeePeriodDTO[]>
    applyAttestCalculationFunctionEmployee(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<any>
    applyAttestCalculationFunctionEmployees(items: AttestEmployeesDaySmallDTO[], option: SoeTimeAttestFunctionOption, timeScheduleScenarioHeadId: number): ng.IPromise<any>
    applyCalculationFunctionValidation(employeeId: number, items: any[], option: SoeTimeAttestFunctionOption): ng.IPromise<any>
    runAutoAttest(employeeIds: number[], dateFrom: Date, dateTo: Date): ng.IPromise<any>
    saveTimeAbsenceDetailRatios(employeeId: number, timeAbsenceDetails: TimeAbsenceDetailDTO[]): ng.IPromise<any>;
    saveTimeAbsenceRule(timeAbsenceRule: TimeAbsenceRuleHeadDTO): ng.IPromise<IActionResult>
    saveTimeCode(timeCode: TimeCodeBaseDTO | TimeCodeAbsenceDTO | ITimeCodeAdditionDeductionDTO | TimeCodeBreakDTO | TimeCodeMaterialDTO | TimeCodeWorkDTO): ng.IPromise<IActionResult>
    updateTimeCodesState(dict: any): ng.IPromise<any>
    validateSaveProjectTimeBlocks(items: any): ng.IPromise<any>
    saveProjectTimeBlocks(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO[]): ng.IPromise<any>;
    saveTimeTerminal(terminal: TimeTerminalDTO): ng.IPromise<IActionResult>;
    reverseTransactionsValidation(employeeId: number, dates: Date[]): ng.IPromise<any>
    reverseTransactions(employeeId: number, dates: Date[], timeDeviationCauseId?: number, timePeriodId?: number, employeeChildId?: number): ng.IPromise<any>
    searchTimeStampEntries(employeeIds: number[], dateFrom: Date, dateTo: Date): ng.IPromise<any>;
    saveAdjustedTimeStampEntries(items: TimeStampEntryDTO[]): ng.IPromise<any>;
    sendEmailToPayrollAdministrator(timeSalaryExportId: number): ng.IPromise<IActionResult>;
    sendPayrollToSftp(timeSalaryExportId: number): ng.IPromise<IActionResult>
    validateExportSalary(employeeIds: number[], startDate: Date, stopDate: Date): ng.IPromise<IActionResult>
    exportSalary(employeeIds: number[], startDate: Date, stopDate: Date, exportTarget: number, lockPeriod: boolean, isPreliminary: boolean): ng.IPromise<IActionResult>
    updateTimeRuleState(dict: any): ng.IPromise<any>;
    exportTimeRules(timeRuleIds: number[]): ng.IPromise<any>;
    importTimeRulesMatch(model: TimeRuleExportImportDTO): ng.IPromise<TimeRuleExportImportDTO>;
    importTimeRulesSave(model: TimeRuleExportImportDTO): ng.IPromise<IActionResult>;
    validateTimeRuleStructure(widgets: FormulaWidget[]): ng.IPromise<IActionResult>;
    saveTimeAccumulator(timeAccumulator: TimeAccumulatorDTO): ng.IPromise<IActionResult>;
    saveTimeRule(timeRule: TimeRuleEditDTO): ng.IPromise<IActionResult>;
    saveTimeDeviationCauses(timeDeviationCauses: ITimeDeviationCauseDTO): ng.IPromise<any>;
    saveTimeHibernatingAbsence(timeHibernatingAbsence: ITimeHibernatingAbsenceHeadDTO): ng.IPromise<IActionResult>;
    createTransactionsForPlannedPeriodCalculation(employeeId: number, timePeriodId: number): ng.IPromise<any>
    getCalculationsFromPeriod(employeeIds: number[], periodId: number): ng.IPromise<IPeriodCalculationResultDTO[]>;

    // DELETE
    deleteEarnedHolidayTransactions(holidayId: number, employeeIds: number[], yearId: number): ng.IPromise<any>
    deleteTimeAccumulator(timeAccumulatorId: number): ng.IPromise<IActionResult>
    deleteTimeAbsenceRule(timeAbsenceRuleId: number): ng.IPromise<IActionResult>
    deleteTimeCode(timeCodeId: number): ng.IPromise<IActionResult>
    deleteTimeCodeBreakGroup(timeCodeBreakGroupId: number): ng.IPromise<any>
    deleteTimePeriodHead(timePeriodHeadId: number, removePeriodLinks: boolean): ng.IPromise<IActionResult>
    deleteTimeRule(timeRuleId: number): ng.IPromise<any>
    deleteTimeSalaryExport(timeSalaryExportId: number): ng.IPromise<IActionResult>
    deleteTimeDeviationCause(timeDeviationCauses: ITimeDeviationCauseDTO): ng.IPromise<any>;
}

export class TimeService implements ITimeService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET   

    createTimeStampsAccourdingToSchedule(timeScheduleTemplatePeriodId: number, date: Date, employeeId: number, employeeGroupId: number): ng.IPromise<TimeStampEntryDTO[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_STAMPS_ACCOURDING_TO_SCHEDULE + timeScheduleTemplatePeriodId + "/" + dateString + "/" + employeeId + "/" + employeeGroupId, false).then(x => {
            return x.map(y => {
                let obj = new TimeStampEntryDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getAbsenceDetails(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TimeAbsenceDetailDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ABSENCEDETAILS + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                let obj = new TimeAbsenceDetailDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getAccountsSmall(accountDimId: number, accountYearId: number, useCache: boolean = true, ignoreHierarchyOnly: boolean = false): ng.IPromise<AccountSmallDTO[]> {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT + "?accountDimId=" + accountDimId + "&accountYearId=" + accountYearId + "&ignoreHierarchyOnly=" + ignoreHierarchyOnly, Constants.WEBAPI_ACCEPT_SMALL_DTO, Constants.CACHE_EXPIRE_MEDIUM, !useCache).then(x => {
            return x.map(y => {
                var obj = new AccountSmallDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getAttestTransitionLogs(timeBlockDateId: number, employeeId: number, timePayrollTransactionId: number): ng.IPromise<AttestTransitionLogDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_ATTEST_ATTEST_TRANSITION_LOGS + timeBlockDateId + "/" + employeeId + "/" + timePayrollTransactionId, false).then(x => {
            return x.map(y => {
                let obj = new AttestTransitionLogDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getHasAttestByEmployeeAccount(date: Date): ng.IPromise<boolean> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE_USER_HAS_ATTEST_BY_EMPLOYEE_ACCOUNT + dateString, false);
    }

    getEmployeesForTimeAttestTree(filterIds: any, dateFrom: Date, dateTo: Date) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_ATTEST_TREE + filterIds + "/" + dateFromString + "/" + dateToString, true);
    }

    getEmployeeVacationPeriod(employeeId: any, dateFrom: Date, dateTo: Date) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_VACATION_EMPLOYEEVACATIONPERIOD + employeeId + "/" + dateFromString + "/" + dateToString, false);
    }

    getEmployeeForUser() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_FOR_USER, false);
    }

    getExportedSalaries(): ng.IPromise<TimeSalaryExportDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT, false).then(x => {
            return x.map(e => {
                var obj = new TimeSalaryExportDTO();
                angular.extend(obj, e);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeSalaryExportSelection(dateFrom: Date, dateTo: Date, accountDimId: number): ng.IPromise<TimeSalaryExportSelectionDTO> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT_SELECTION + dateFromString + "/" + dateToString + "/" + accountDimId, false).then(x => {
            var obj = new TimeSalaryExportSelectionDTO();
            angular.extend(obj, x);
            obj.fixDates();

            if (obj.timeSalaryExportSelectionGroups) {
                obj.timeSalaryExportSelectionGroups = obj.timeSalaryExportSelectionGroups.map(g => {
                    let gObj = new TimeSalaryExportSelectionGroupDTO();
                    angular.extend(gObj, g);
                    gObj.selected = true;
                    gObj.sortBy = 'employeeNrSort';
                    gObj.sortByReverse = false;

                    if (gObj.timeSalaryExportSelectionEmployees) {
                        gObj.timeSalaryExportSelectionEmployees = gObj.timeSalaryExportSelectionEmployees.map(e => {
                            let eObj = new TimeSalaryExportSelectionEmployeeDTO();
                            angular.extend(eObj, e);
                            eObj.selected = true;
                            eObj.visible = true;
                            return eObj;
                        });
                    }
                    return gObj;
                });
            }

            return obj;
        });
    }

    getInvoiceProductsDict(invoiceProductVatType: TermGroup_InvoiceProductVatType, addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_INVOICE_PRODUCTS + invoiceProductVatType + "/" + addEmptyRow, useCache);
    }

    getPayrollProducts(useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG, !useCache).then(x => {
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

                        return sObj;
                    })
                } else {
                    obj.settings = [];
                }

                return obj;
            });
        });
    }

    getPayrollProductsDict(addEmptyRow: boolean, concatNumberAndName: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getPayrollProductAccount(type: ProductAccountType, employeeId: number, productId: number, projectId: number, customerId: number, getInternalAccounts: boolean, date: Date, useCache: boolean = true): ng.IPromise<IAccountingPrioDTO> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_PAYROLL_PAYROLL_PRODUCT_ACCOUNT + type + "/" + employeeId + "/" + productId + "/" + projectId + "/" + customerId + "/" + getInternalAccounts + "/" + dateString, useCache);
    }

    getTerminalGroupNames(): ng.IPromise<string[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL_GROUP_NAMES, false);
    }

    getHasAnyTerminalSpecifiedBoolSetting(settingType: TimeTerminalSettingType): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL_HAS_ANY_TERMINAL_SPECIFIED_BOOL_SETTING + settingType, false);
    }

    getHasAnyTerminalSpecifiedIntSetting(settingType: TimeTerminalSettingType, allowZero: boolean): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL_HAS_ANY_TERMINAL_SPECIFIED_INT_SETTING + settingType + "/" + allowZero, false);
    }

    getAnyTerminalSpecifiedIntSetting(settingType: TimeTerminalSettingType): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL_GET_ANY_TERMINAL_SPECIFIED_INT_SETTING + settingType, false);
    }

    getTimeAttestEmployeeDays(gridName: string, employeeId: number, startDate: Date, stopDate: Date, hasDayFilter: boolean, includeProjectTimeBlocks: boolean, includeShifts: boolean, includeTimeStamps: boolean, includeTimeBlocks: boolean, includeTimeCodeTransactions: boolean, includeTimeInvoiceTransactions: boolean, doNotShowDaysOutsideEmployeeAccount: boolean, filterAccountIds: number[], cacheKeyToUse: string): ng.IPromise<AttestEmployeeDayDTO[]> {
        var startDateString: string = null;
        if (startDate)
            startDateString = startDate.toDateTimeString();
        var stopDateString: string = null;
        if (stopDate)
            stopDateString = stopDate.toDateTimeString();
        var filterAccountIdsString: string = null;
        if (filterAccountIds)
            filterAccountIdsString = filterAccountIds.join(',');

        var url = Constants.WEBAPI_TIME_ATTEST_EMPLOYEE_DAYS + gridName + "/" + employeeId + "/" + startDateString + "/" + stopDateString + "/" + hasDayFilter + "/" + includeProjectTimeBlocks + "/" + includeShifts + "/" + includeTimeStamps + "/" + includeTimeBlocks + "/" + includeTimeCodeTransactions + "/" + includeTimeInvoiceTransactions + "/" + doNotShowDaysOutsideEmployeeAccount + "/" + filterAccountIdsString + "/" + cacheKeyToUse;
        return this.httpService.get(url, false).then(x => {
            return x.map(e => {
                var obj = new AttestEmployeeDayDTO();
                angular.extend(obj, e);
                obj.fixDates();
                obj.setTypes();

                return obj;
            });
        });
    }

    getAdditionDeductions(employeeId: number, startDate: Date, stopDate: Date, timePeriodId: number, isMySelf = false): ng.IPromise<any[]> {
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
                        cObj.fixDates();
                        return cObj;
                    });
                } else {
                    obj.transactions = [];
                }

                return obj;
            });
        });
    }

    getTimeAttestFunctionOptionDescription(option: number): ng.IPromise<any[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_DESCRIPTION + option, false);
    }

    getAttestTreeMessageGroups(): ng.IPromise<MessageGroupDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_ATTEST_MESSAGEGROUP, false);
    }

    getTimeRules(): ng.IPromise<TimeRuleGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_RULE, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimeRuleGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeRule(timeRuleId: number): ng.IPromise<TimeRuleEditDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_RULE + timeRuleId, false).then(x => {
            if (!x)
                return null;

            let obj = new TimeRuleEditDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();

            return obj;
        });
    }

    getTimeAbsenceRules(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ABSENCERULES, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getTimeAbsenceRule(timeAbsenceRuleHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ABSENCERULE + timeAbsenceRuleHeadId, false).then(x => {
            let obj = new TimeAbsenceRuleHeadDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getTimeAbsenceRuleRows(timeAbsenceRuleHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ABSENCERULEROWS + timeAbsenceRuleHeadId, false).then(x => {
            if (x) {
                x = x.map(e => {
                    let rows = new TimeAbsenceRuleRowDTO();
                    angular.extend(rows, e);
                    return rows;
                });
            }
            return x;
        });
    }

    getTimeAccumulators(onlyActive: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR + "?onlyActive=" + onlyActive, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimeAccumulatorGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeAccumulatorsDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR + "?addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeAccumulatorsForEmployee(employeeId: number, startDate: Date, stopDate: Date, addSourceIds: boolean, calculateDay: boolean, calculatePeriod: boolean, calculatePlanningPeriod: boolean, calculateYear: boolean, calculateAccToday: boolean, calculateAccTodayValue: boolean): ng.IPromise<TimeAccumulatorItem[]> {
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

    getTimeAccumulator(timeAccumulatorId: number, onlyActive: boolean, loadEmployeeGroups: boolean, loadTimeWorkReductionEarning: boolean): ng.IPromise<TimeAccumulatorDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR + timeAccumulatorId + "/" + onlyActive + "/" + loadEmployeeGroups + "/" + loadTimeWorkReductionEarning, false).then((x: TimeAccumulatorDTO) => {
            let obj = new TimeAccumulatorDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getTimeCalendarPeriods(employeeId: number, fromDate: Date, toDate: Date, sysPayrollTypeLevel1?: number, sysPayrollTypeLevel2?: number, sysPayrollTypeLevel3?: number, sysPayrollTypeLevel4?: number): ng.IPromise<TimeCalendarPeriodDTO[]> {
        var fromDateString: string = null;
        if (fromDate)
            fromDateString = fromDate.toDateTimeString();
        var toDateString: string = null;
        if (toDate)
            toDateString = toDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CALENDAR_PERIOD + employeeId + "/" + fromDateString + "/" + toDateString + "/" + (sysPayrollTypeLevel1 || 0) + "/" + (sysPayrollTypeLevel2 || 0) + "/" + (sysPayrollTypeLevel3 || 0) + "/" + (sysPayrollTypeLevel4 || 0), false).then(x => {
            return x.map(y => {
                let obj: TimeCalendarPeriodDTO = new TimeCalendarPeriodDTO();
                angular.extend(obj, y);
                obj.fixDates();

                if (obj.payrollProducts) {
                    obj.payrollProducts = obj.payrollProducts.map(p => {
                        let pObj = new TimeCalendarPeriodPayrollProductDTO();
                        angular.extend(pObj, p);
                        return pObj;
                    });
                } else {
                    obj.payrollProducts = [];
                }

                return obj;
            });
        });
    }

    getTimeCodes(timeCodeType: SoeTimeCodeType, onlyActive: boolean, loadPayrollProducts: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + "?timeCodeType=" + timeCodeType + "&onlyActive=" + onlyActive + "&loadPayrollProducts=" + loadPayrollProducts, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getTimeCodesDict(timeCodeType: SoeTimeCodeType, addEmptyRow: boolean, concatCodeAndName: boolean, includeType: boolean = false) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + "?timeCodeType=" + timeCodeType + "&addEmptyRow=" + addEmptyRow + "&concatCodeAndName=" + concatCodeAndName + "&includeType=" + includeType, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeCodesGrid(timeCodeType: SoeTimeCodeType, onlyActive: boolean, loadPayrollProducts: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + "?timeCodeType=" + timeCodeType + "&onlyActive=" + onlyActive + "&loadPayrollProducts=" + loadPayrollProducts, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getTimeCode(timeCodeType: SoeTimeCodeType, timeCodeId: number, loadInvoiceProducts: boolean = false, loadPayrollProducts: boolean = false, loadTimeCodeDeviationCauses: boolean = false, loadEmployeeGroups: boolean = false) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE + timeCodeType + "/" + timeCodeId + "/" + loadInvoiceProducts + "/" + loadPayrollProducts + "/" + loadTimeCodeDeviationCauses + "/" + loadEmployeeGroups, false).then(x => {
            let obj;
            switch (x.type) {
                case SoeTimeCodeType.Absense:
                    obj = new TimeCodeAbsenceDTO();
                    break;
                case SoeTimeCodeType.AdditionDeduction:
                    obj = new TimeCodeAdditionDeductionDTO();
                    break;
                case SoeTimeCodeType.Break:
                    obj = new TimeCodeBreakDTO();
                    break;
                case SoeTimeCodeType.Material:
                    obj = new TimeCodeMaterialDTO();
                    break;
                case SoeTimeCodeType.Work:
                    obj = new TimeCodeWorkDTO();
                    break;
            }

            angular.extend(obj, x);

            if (x.type === SoeTimeCodeType.Break) {
                obj.fixDates();
                if (x.timeCodeRules) {
                    obj.timeCodeRules = _.map(x.timeCodeRules, (y: TimeCodeRuleDTO) => {
                        let rObj = new TimeCodeRuleDTO(y.type, y.value);
                        return rObj;
                    });
                }

                if (loadTimeCodeDeviationCauses) {
                    obj.timeCodeDeviationCauses = _.map(x.timeCodeDeviationCauses, y => {
                        let dObj = new TimeCodeBreakTimeCodeDeviationCauseDTO();
                        angular.extend(dObj, y);
                        return dObj;
                    });
                }
            }
            if (x.type === SoeTimeCodeType.Absense) {
                obj.fixDates();
            }
            if (x.type === SoeTimeCodeType.Work) {
                obj.fixDates();
            }

            return obj;
        });
    }

    getTimeCodeBreakGroups() {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE_BREAK_GROUPS, false);
    }

    getTimeCodeBreakGroup(timeCodeBreakGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE_BREAK_GROUP + timeCodeBreakGroupId, false);
    }

    getTimeDeviationCauses() {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getTimeDeviationCausesGrid(): ng.IPromise<TimeDeviationCauseGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_GRID, false).then(x => {
            return x.map(y => {
                let obj = new TimeDeviationCauseGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeDeviationCausesByEmployeeGroupDict(employeeGroupId: number, addEmptyRow: boolean, removeAbsence: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?addEmptyRow=" + addEmptyRow + "&removeAbsence=" + removeAbsence + "&employeeGroupId=" + employeeGroupId, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeDeviationCausesByEmployeeGroupGrid(employeeGroupId: number, addEmptyRow: boolean, removeAbsence: boolean): ng.IPromise<TimeDeviationCauseGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?addEmptyRow=" + addEmptyRow + "&removeAbsence=" + removeAbsence + "&employeeGroupId=" + employeeGroupId, true, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimeDeviationCauseGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeDeviationCause(timeDeviationCauseId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + timeDeviationCauseId, false).then(x => {
            let obj = new TimeDeviationCauseDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getTimeDeviationCausesDict(addEmptyRow: boolean, removeAbsence: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "?addEmptyRow=" + addEmptyRow + "&removeAbsence=" + removeAbsence, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeDeviationCauseRequestsDict(employeeGroupId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_REQUESTS + "?employeeGroupId=" + employeeGroupId + "&addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeDeviationCauseStandardIdFromPrio(employeeId: number, date: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE_STANDARD_ID_FROM_PRIO + employeeId + "/" + dateString, false);
    }

    getTimeMonthlyReportDataURL(employeeId: number, startDate: Date, stopDate: Date, reportId: number) {
        var startDateString: string = null;
        if (startDate)
            startDateString = startDate.toDateTimeString();
        var stopDateString: string = null;
        if (stopDate)
            stopDateString = stopDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_REPORT_TIMEMONTHLYREPORTURL + employeeId + "/" + startDateString + "/" + stopDateString + "/" + reportId + "/", false);
    }

    getTimePeriodHeadsDict(type: TermGroup_TimePeriodType, addEmptyRow: boolean, accountId: number = null) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + "?type=" + type + "&addEmptyRow=" + addEmptyRow + (accountId ? "&accountId=" + accountId : ""), false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimePeriodHeadsForGrid(type: TermGroup_TimePeriodType, loadTypeNames: boolean, loadAccountNames: boolean, loadChildNames: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + "?type=" + type + "&loadTypeNames=" + loadTypeNames + "&loadAccountNames=" + loadAccountNames + "&loadChildNames=" + loadChildNames, false, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimePeriodHeadGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimePeriodHead(timePeriodHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + timePeriodHeadId, false).then(x => {
            let obj = new TimePeriodHeadDTO();
            angular.extend(obj, x);

            if (obj.timePeriods) {
                obj.timePeriods = _.orderBy(x.timePeriods.map(p => {
                    let pObj = new TimePeriodDTO();
                    angular.extend(pObj, p);
                    pObj.fixDates();
                    return pObj;
                }), ['stopDate'], ['desc']);
            } else {
                obj.timePeriods = [];
            }

            return obj;
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
    getPeriodsForCalculation(type: TermGroup_TimePeriodType, dateFrom: Date, dateTo: Date, getPeriodsForCalculation: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_FOR_CALCULATION + type + "/" + dateFrom.toDateTimeString() + "/" + dateTo.toDateTimeString() + "/" + getPeriodsForCalculation, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getTimePeriod(timePeriodHeadId: number, date: Date, loadTimePeriodHead: boolean): ng.IPromise<TimePeriodDTO> {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_PERIOD + timePeriodHeadId + "/" + dateString + "/" + loadTimePeriodHead, false, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            if (x) {
                let obj = new TimePeriodDTO();
                angular.extend(obj, x);
                obj.fixDates();
                obj.setTypes();
                return obj;
            } else {
                return null;
            }
        });
    }

    getPlanningPeriodHeadWithPeriods(timePeriodHeadId: number, date: Date): ng.IPromise<PlanningPeriodHead> {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_PLANNING_PERIOD_HEAD_WITH_PERIODS + timePeriodHeadId + "/" + dateString, false).then(x => {
            if (x) {
                let obj = new PlanningPeriodHead();
                angular.extend(obj, x);
                obj.setTypes();
                return obj;
            } else {
                return null;
            }
        });
    }

    getTimeRuleImportedDetails(timeRuleId: number, loadDetails: boolean): ng.IPromise<TimeRuleImportedDetailsDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_RULE_IMPORTED_DETAILS + timeRuleId + "/" + loadDetails, false).then(x => {
            let obj = new TimeRuleImportedDetailsDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.json = JSON.parse(obj.json);
            obj.originalJson = JSON.parse(obj.originalJson);
            return obj;
        });
    }

    getTimeStamp(timeStampEntryId: number): ng.IPromise<TimeStampEntryDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_STAMP + timeStampEntryId, false).then(x => {
            let obj = new TimeStampEntryDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getTimeStampAdditions(isMySelf: boolean): ng.IPromise<TimeStampAdditionDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_STAMP_ADDITION + isMySelf, false).then(x => {
            return x.map(y => {
                let obj = new TimeStampAdditionDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeStampUserAgentClientInfo(timeStampEntryId: number): ng.IPromise<UserAgentClientInfoDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_STAMP_USER_AGENT_CLIENT_INFO + timeStampEntryId, false).then(x => {
            let obj = new UserAgentClientInfoDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getTimeTerminals(type: number, onlyActive: boolean, onlyRegistered: boolean, onlySynchronized: boolean, loadSettings: boolean, loadCompanies: boolean, loadTypeNames: boolean, ignoreLimitToAccount: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL + "?type=" + type + "&onlyActive=" + onlyActive + "&onlyRegistered=" + onlyRegistered + "&onlySynchronized=" + onlySynchronized + "&loadSettings=" + loadSettings + "&loadCompanies=" + loadCompanies + "&loadTypeNames=" + loadTypeNames + "&ignoreLimitToAccount=" + ignoreLimitToAccount, false).then(x => {
            return x.map(y => {
                let obj = new TimeTerminalDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getTimeTerminal(timeTerminalId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL + timeTerminalId, false).then(x => {
            let obj = new TimeTerminalDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getTimeTerminalAccountDim(timeTerminalId: number, dimNr: number): ng.IPromise<AccountDimSmallDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL_ACCOUNT_DIM + timeTerminalId + "/" + dimNr, true);
    }

    getTimeZones(): ng.IPromise<StringKeyValue[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL_TIME_ZONES, true);
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

    getYears(yearsBack: number) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_CALENDAR_YEARS + yearsBack, false);
    }

    getHolidays(year: number, onlyRedDays: boolean, onlyHistorical: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_CALENDAR_HOLIDAYS + year + "/" + onlyRedDays + "/" + onlyHistorical + "/" + addEmptyRow, false);
    }

    getProjectsForTimeSheetEmployees(employeeIds: number[]) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_PROJECTSFORTIMESHEET_EMPLOYEES + "?empIds=" + employeeIds.join(','), false);
    }

    getEmployeesForProjectTimeCode(addEmptyRow: boolean, getHidden: boolean, addNoReplacementEmployee: boolean, includeEmployeeId?: number): ng.IPromise<IEmployeeTimeCodeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PROJECTTIMECODE + addEmptyRow + "/" + getHidden + "/" + addNoReplacementEmployee + "/" + includeEmployeeId, false);
    }

    isEmployeeCurrentUser(employeeId: number): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_IS_EMPLOYEE_CURRENT_USER + employeeId, false);
    }

    getUsesWeekendSalary() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_USESWEEKENDSALARY, false);
    }

    getTimeHibernatingAbsence(employeeId: number, employmentId: number): ng.IPromise<ITimeHibernatingAbsenceHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_HIBERNATING_ABSCENCE + employeeId + "/" + employmentId, false);
    }    
    // POST

    getTimeAttestTree(grouping: TermGroup_AttestTreeGrouping, sorting: TermGroup_AttestTreeSorting, startDate: Date, stopDate: Date, timePeriodId: number, settings: TimeEmployeeTreeSettings): ng.IPromise<TimeEmployeeTreeDTO> {
        var model = { grouping, sorting, startDate, stopDate, timePeriodId, settings };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TREE, model).then(t => {
            return TimeEmployeeTreeDTO.createInstance(t);
        });
    }

    getTimeAttestTreeWarnings(tree: TimeEmployeeTreeDTO, startDate: Date, stopDate: Date, timePeriodId: number, employeeIds: number[], doShowOnlyWithWarnings: boolean, flushCache: boolean): ng.IPromise<TimeEmployeeTreeDTO> {
        var model = { tree, startDate, stopDate, timePeriodId, employeeIds, doShowOnlyWithWarnings, flushCache };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TREE_WARNINGS, model).then(t => {
            return TimeEmployeeTreeDTO.createInstance(t);
        });
    }

    refreshTimeAttestTree(tree: TimeEmployeeTreeDTO, startDate: Date, stopDate: Date, timePeriodId: number, settings: TimeEmployeeTreeSettings): ng.IPromise<TimeEmployeeTreeDTO> {
        var model = { tree, startDate, stopDate, timePeriodId, settings };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TREE_REFRESH, model).then(t => {
            return TimeEmployeeTreeDTO.createInstance(t);
        });
    }

    refreshTimeAttestTreeGroupNode(tree: TimeEmployeeTreeDTO, groupNode: TimeEmployeeTreeGroupNodeDTO): ng.IPromise<TimeEmployeeTreeGroupNodeDTO> {
        var model = { tree, groupNode };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TREE_REFRESH_GROUP, model).then(g => {
            return TimeEmployeeTreeGroupNodeDTO.createInstance(g);
        });
    }

    recalculateUnhandledShiftChangesEmployees(unhandledEmployees: TimeUnhandledShiftChangesEmployeeDTO[], doRecalculateShifts: boolean, doRecalculateExtraShifts: boolean): ng.IPromise<IActionResult> {
        var model = { unhandledEmployees, doRecalculateShifts, doRecalculateExtraShifts };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_UNHANDLEDSHIFTCHANGES_RECALCULATE, model);
    }

    getTimeMonthlyReportMultipleEmployeesDataURL(employeeIds: number[], startDate: Date, stopDate: Date, reportId: number, reportTemplateType: SoeReportTemplateType) {
        var model = {
            employeeIds: employeeIds,
            dateFrom: startDate,
            dateTo: stopDate,
            reportId: reportId,
            reportTemplateType: reportTemplateType
        }

        return this.httpService.post(Constants.WEBAPI_REPORT_TIMEMONTHLYREPORTURL, model);
    }

    loadEarnedHolidaysContent(holidayId: number, yearId: number, loadSuggestions: boolean, employeeEarnedHolidays: EmployeeEarnedHolidayDTO[]) {
        var model = {
            holidayId: holidayId,
            loadSuggestions: loadSuggestions,
            employeeEarnedHolidays: employeeEarnedHolidays,
            year: yearId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_EARNED_HOLIDAY_LOAD, model);
    }

    saveEarnedHolidayTransactions(holidayId: number, employeeIds: number[], yearId: number) {
        var model = {
            holidayId: holidayId,
            employeeIds: employeeIds,
            year: yearId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_EARNED_HOLIDAY_CREATETRANSACTIONS, model);
    }

    saveTimePeriodHead(timePeriodHead: TimePeriodHeadDTO, removePeriodLinks: boolean) {
        let model = {
            timePeriodHead: timePeriodHead,
            removePeriodLinks: removePeriodLinks
        }
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD, model);
    }

    saveTimeCodeBreakGroup(breakGroup: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_CODE_BREAK_GROUP, breakGroup);
    }

    saveAttestForEmployees(currentEmployeeId: number, employeeIds: number[], attestStateToId: number, startDate?: Date, stopDate?: Date) {
        var model = {
            currentEmployeeId: currentEmployeeId,
            employeeIds: employeeIds,
            attestStateToId: attestStateToId,
            startDate: startDate,
            stopDate: stopDate,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEES, model);
    }

    saveAttestForEmployeeValidation(items: any[], employeeId: number, attestStateToId: number, isMySelf: boolean) {
        var model = {
            items: items,
            employeeId: employeeId,
            attestStateToId: attestStateToId,
            isMySelf: isMySelf,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEE_VALIDATION, model);
    }

    saveAttestForEmployee(items: any[], employeeId: number, attestStateToId: number, isMySelf: boolean) {
        var model = {
            items: items,
            employeeId: employeeId,
            attestStateToId: attestStateToId,
            isMySelf: isMySelf
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEE, model);
    }

    saveAttestForTransactionsValidation(items: any, attestStateToId: number, isMySelf: boolean) {
        var model = {
            items: items,
            attestStateToId: attestStateToId,
            isMySelf: isMySelf,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TRANSACTIONS_VALIDATION, model);
    }

    saveAttestForTransactions(items: any, attestStateToId: number, isMySelf: boolean) {
        var model = {
            items: items,
            attestStateToId: attestStateToId,
            isMySelf: isMySelf
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TRANSACTIONS, model);
    }

    saveAttestForAdditionDeductionsValidation(transactionItems: any, employeeId: number, attestStateToId: number, isMySelf: boolean) {
        var model = {
            transactionItems: transactionItems,
            employeeId: employeeId,
            attestStateToId: attestStateToId,
            isMySelf: isMySelf,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEE_ADDITION_DEDUCTION_VALIDATION, model);
    }

    unlockDay(items: any[], employeeId: number): ng.IPromise<any> {
        var model = {
            items: items,
            employeeId: employeeId
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_UNLOCKDAY, model);
    }

    saveTimeStampEntries(entries: AttestEmployeeDayTimeStampDTO[], date: Date, discardBreakEvaluation: boolean, employeeId: number) {
        var model = {
            entries: entries,
            date: date,
            discardBreakEvaluation: discardBreakEvaluation,
            employeeId: employeeId
        };

        return this.httpService.post(Constants. WEBAPI_TIME_ATTEST_EMPLOYEE_TIMESTAMP, model);
    }


    validateDeviationChange(employeeId: number, timeBlockId: number, timeBlockGuidId: string, timeBlocks: AttestEmployeeDayTimeBlockDTO[], timeScheduleTemplatePeriodId: number, date: Date, startTime: Date, stopTime: Date, clientChange: SoeTimeBlockClientChange, onlyUseInTimeTerminal: boolean, timeDeviationCauseId: number, employeeChildId: number, comment: string, accountSetting: AccountingSettingsRowDTO = null) {
        var model = {
            employeeId: employeeId,
            timeBlockId: timeBlockId,
            timeBlockGuidId: timeBlockGuidId,
            timeBlocks: timeBlocks,
            timeScheduleTemplatePeriodId: timeScheduleTemplatePeriodId,
            date: date,
            startTime: startTime,
            stopTime: stopTime,
            clientChange: clientChange,
            onlyUseInTimeTerminal: onlyUseInTimeTerminal,
            timeDeviationCauseId: timeDeviationCauseId,
            employeeChildId: employeeChildId,
            comment: comment,
            accountSetting: accountSetting,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_VALIDATE_DEVIATION_CHANGE, model).then(x => {
            let obj = new ValidateDeviationChangeResult();
            angular.extend(obj, x);

            if (obj.generatedTimeBlocks) {
                obj.generatedTimeBlocks = obj.generatedTimeBlocks.map(tb => {
                    let tbObj = new AttestEmployeeDayTimeBlockDTO();
                    angular.extend(tbObj, tb);
                    tbObj.fixDates();
                    return tbObj;
                });
            } else {
                obj.generatedTimeBlocks = [];
            }

            if (obj.generatedTimeCodeTransactions) {
                obj.generatedTimeCodeTransactions = obj.generatedTimeCodeTransactions.map(tc => {
                    let tcObj = new AttestEmployeeDayTimeCodeTransactionDTO();
                    angular.extend(tcObj, tc);
                    tcObj.fixDates();
                    return tcObj;
                });
            } else {
                obj.generatedTimeCodeTransactions = [];
            }

            if (obj.generatedTimePayrollTransactions) {
                obj.generatedTimePayrollTransactions = obj.generatedTimePayrollTransactions.map(tp => {
                    let tpObj = new AttestPayrollTransactionDTO();
                    angular.extend(tpObj, tp);
                    tpObj.fixDates();
                    return tpObj;
                });
            } else {
                obj.generatedTimePayrollTransactions = [];
            }

            if (obj.timeDeviationCauses) {
                obj.timeDeviationCauses = obj.timeDeviationCauses.map(tb => {
                    let tbObj = new TimeDeviationCauseGridDTO();
                    angular.extend(tbObj, tb);
                    return tbObj;
                });
            } else {
                obj.timeDeviationCauses = [];
            }

            return obj;
        });
    }

    saveGeneratedDeviations(timeBlocks: AttestEmployeeDayTimeBlockDTO[], timeCodeTransactions: AttestEmployeeDayTimeCodeTransactionDTO[], timePayrollTransactions: IAttestPayrollTransactionDTO[], applyAbsences: IApplyAbsenceDTO[], timeBlockDateId: number, timeScheduleTemplatePeriodId: number, employeeId: number, payrollImportEmployeeTransactionIds: number[]): ng.IPromise<IActionResult> {
        var model = {
            timeBlocks: timeBlocks,
            timeCodeTransactions: timeCodeTransactions,
            timePayrollTransactions: timePayrollTransactions,
            applyAbsences: applyAbsences,
            timeBlockDateId: timeBlockDateId,
            timeScheduleTemplatePeriodId: timeScheduleTemplatePeriodId,
            employeeId: employeeId,
            payrollImportEmployeeTransactionIds: payrollImportEmployeeTransactionIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_SAVE_GENERATED_DEVIATIONS, model);
    }

    recalculateTimeAccumulators(timeAccumulatorIds: number[]) {
        var model = {
            numbers: timeAccumulatorIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR_RECALCULATE, model);
    }

    applyAttestCalculationFunctionEmployee(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<any> {
        var model = { items: items, option: option };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_EMPLOYEE, model);
    }

    applyAttestCalculationFunctionEmployees(items: AttestEmployeesDaySmallDTO[], option: SoeTimeAttestFunctionOption, timeScheduleScenarioHeadId: number): ng.IPromise<any> {
        var model = { items: items, option: option, timeScheduleScenarioHeadId: timeScheduleScenarioHeadId };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_EMPLOYEES, model);
    }

    applyCalculationFunctionValidation(employeeId: number, items: any[], option: SoeTimeAttestFunctionOption): ng.IPromise<TimeAttestCalculationFunctionValidationDTO> {
        var model = { employeeId: employeeId, items: items, option: option };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_VALIDATION, model);
    }

    getTimeAttestEmployeePeriods(dateFrom: Date, dateTo: Date, grouping: TermGroup_AttestTreeGrouping, groupId: number, visibleEmployeeIds: number[], isAdditional: boolean, includeAdditionalEmployees: boolean, doNotShowDaysOutsideEmployeeAccount: boolean, cacheKeyToUse: string, flushCache: boolean, timePeriodId?: number): ng.IPromise<AttestEmployeePeriodDTO[]> {
        var model = { dateFrom, dateTo, timePeriodId, grouping, groupId, visibleEmployeeIds, isAdditional, includeAdditionalEmployees, doNotShowDaysOutsideEmployeeAccount, cacheKeyToUse, flushCache };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEE_PERIODS, model).then(x => {
            return x.map(e => {
                var obj = new AttestEmployeePeriodDTO();
                angular.extend(obj, e);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeAttestEmployeePeriodsPreview(tree: TimeEmployeeTreeDTO, groupNode: TimeEmployeeTreeGroupNodeDTO): ng.IPromise<AttestEmployeePeriodDTO[]> {
        var model = { tree, groupNode };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_EMPLOYEE_PERIODS_PREVIEW, model).then(x => {
            return x.map(e => {
                var obj = new AttestEmployeePeriodDTO();
                angular.extend(obj, e);
                obj.fixDates();
                return obj;
            });
        });
    }

    runAutoAttest(employeeIds: number[], startDate: Date, stopDate: Date): ng.IPromise<any> {
        var model = { employeeIds, startDate, stopDate };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_TREE_AUTOATTEST, model);
    }

    saveTimeAbsenceDetailRatios(employeeId: number, timeAbsenceDetails: TimeAbsenceDetailDTO[]): ng.IPromise<IActionResult> {
        var model = { employeeId, timeAbsenceDetails };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_ABSENCEDETAILS_RATIO, model);
    }

    saveTimeAbsenceRule(timeAbsenceRule: TimeAbsenceRuleHeadDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_ABSENCERULE, timeAbsenceRule);
    }
    saveTimeDeviationCauses(timeDeviationCauses: ITimeDeviationCauseDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE, timeDeviationCauses);
    }

    saveTimeCode(timeCode: TimeCodeBaseDTO | TimeCodeAbsenceDTO | ITimeCodeAdditionDeductionDTO | TimeCodeBreakDTO | TimeCodeMaterialDTO | TimeCodeWorkDTO): ng.IPromise<IActionResult> {
        var model: TimeCodeSaveDTO = new TimeCodeSaveDTO();

        // Base fields        
        model.timeCodeId = timeCode.timeCodeId;
        model.code = timeCode.code;
        model.name = timeCode.name;
        model.type = timeCode.type;
        model.registrationType = timeCode.registrationType;
        model.payed = timeCode.payed;
        model.minutesByConstantRules = timeCode.minutesByConstantRules;
        model.description = timeCode.description;
        model.roundingType = timeCode.roundingType;
        model.roundingValue = timeCode.roundingValue;
        model.roundingTimeCodeId = timeCode.roundingTimeCodeId;
        model.roundingInterruptionTimeCodeId = timeCode.roundingInterruptionTimeCodeId;
        model.roundingGroupKey = timeCode.roundingGroupKey;
        model.roundStartTime = timeCode.roundStartTime;
        model.classification = timeCode.classification;
        model.state = timeCode.state;

        if (timeCode.factorBasedOnWorkPercentage)
            model.factorBasedOnWorkPercentage = timeCode.factorBasedOnWorkPercentage;
        if (timeCode.invoiceProducts)
            model.invoiceProducts = <any>timeCode.invoiceProducts;
        if (timeCode.payrollProducts)
            model.payrollProducts = <any>timeCode.payrollProducts;

        // Type specific fields
        switch (timeCode.type) {
            case SoeTimeCodeType.Absense:
                model.adjustQuantityByBreakTime = (<TimeCodeAbsenceDTO>timeCode).adjustQuantityByBreakTime;
                model.adjustQuantityTimeCodeId = (<TimeCodeAbsenceDTO>timeCode).adjustQuantityTimeCodeId;
                model.adjustQuantityTimeScheduleTypeId = (<TimeCodeAbsenceDTO>timeCode).adjustQuantityTimeScheduleTypeId;
                model.timeCodeRuleType = (<TimeCodeAbsenceDTO>timeCode).timeCodeRuleType;
                model.timeCodeRuleValue = (<TimeCodeAbsenceDTO>timeCode).timeCodeRuleValue;
                model.timeCodeRuleTime = (<TimeCodeAbsenceDTO>timeCode).timeCodeRuleTime;
                model.kontekId = (<TimeCodeAbsenceDTO>timeCode).kontekId;
                model.isAbsence = (<TimeCodeAbsenceDTO>timeCode).isAbsence;
                break;
            case SoeTimeCodeType.AdditionDeduction:
                model.expenseType = (<TimeCodeAdditionDeductionDTO>timeCode).expenseType;
                model.comment = (<TimeCodeAdditionDeductionDTO>timeCode).comment;
                model.stopAtDateStart = (<TimeCodeAdditionDeductionDTO>timeCode).stopAtDateStart;
                model.stopAtDateStop = (<TimeCodeAdditionDeductionDTO>timeCode).stopAtDateStop;
                model.stopAtPrice = (<TimeCodeAdditionDeductionDTO>timeCode).stopAtPrice;
                model.stopAtVat = (<TimeCodeAdditionDeductionDTO>timeCode).stopAtVat;
                model.stopAtAccounting = (<TimeCodeAdditionDeductionDTO>timeCode).stopAtAccounting;
                model.stopAtComment = (<TimeCodeAdditionDeductionDTO>timeCode).stopAtComment;
                model.commentMandatory = (<TimeCodeAdditionDeductionDTO>timeCode).commentMandatory;
                model.hideForEmployee = (<TimeCodeAdditionDeductionDTO>timeCode).hideForEmployee;
                model.showInTerminal = (<TimeCodeAdditionDeductionDTO>timeCode).showInTerminal;
                model.fixedQuantity = (<TimeCodeAdditionDeductionDTO>timeCode).fixedQuantity;
                break;
            case SoeTimeCodeType.Break:
                model.minMinutes = (<TimeCodeBreakDTO>timeCode).minMinutes;
                model.maxMinutes = (<TimeCodeBreakDTO>timeCode).maxMinutes;
                model.defaultMinutes = (<TimeCodeBreakDTO>timeCode).defaultMinutes;
                model.startType = (<TimeCodeBreakDTO>timeCode).startType;
                model.stopType = (<TimeCodeBreakDTO>timeCode).stopType;
                model.startTimeMinutes = (<TimeCodeBreakDTO>timeCode).startTimeMinutes;
                model.stopTimeMinutes = (<TimeCodeBreakDTO>timeCode).stopTimeMinutes;
                model.startTime = (<TimeCodeBreakDTO>timeCode).startTime;
                model.template = (<TimeCodeBreakDTO>timeCode).template;
                model.timeCodeBreakGroupId = (<TimeCodeBreakDTO>timeCode).timeCodeBreakGroupId;
                model.timeCodeRules = (<TimeCodeBreakDTO>timeCode).timeCodeRules;
                model.employeeGroupIds = (<TimeCodeBreakDTO>timeCode).employeeGroupIds;
                model.timeCodeDeviationCauses = (<TimeCodeBreakDTO>timeCode).timeCodeDeviationCauses;
                break;
            case SoeTimeCodeType.Material:
                model.note = (<TimeCodeMaterialDTO>timeCode).note;
                break;
            case SoeTimeCodeType.Work:
                model.adjustQuantityByBreakTime = (<TimeCodeWorkDTO>timeCode).adjustQuantityByBreakTime;
                model.adjustQuantityTimeCodeId = (<TimeCodeWorkDTO>timeCode).adjustQuantityTimeCodeId;
                model.adjustQuantityTimeScheduleTypeId = (<TimeCodeWorkDTO>timeCode).adjustQuantityTimeScheduleTypeId;
                model.timeCodeRuleType = (<TimeCodeWorkDTO>timeCode).timeCodeRuleType;
                model.timeCodeRuleValue = (<TimeCodeWorkDTO>timeCode).timeCodeRuleValue;
                model.timeCodeRuleTime = (<TimeCodeWorkDTO>timeCode).timeCodeRuleTime;
                model.isWorkOutsideSchedule = (<TimeCodeWorkDTO>timeCode).isWorkOutsideSchedule;
                break;
        }
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_CODE, model);
    }

    updateTimeCodesState(dict: any) {
        var model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_CODE_UPDATESTATE, model);
    }

    validateSaveProjectTimeBlocks(items: any) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_VALIDATEPROJECTTIMEBLOCKSAVEDTO, items);
    }

    saveProjectTimeBlocks(projectTimeBlockSaveDTOs: IProjectTimeBlockSaveDTO[]) {
        return this.httpService.post(Constants.WEBAPI_CORE_PROJECT_PROJECTTIMEBLOCKSAVEDTO, projectTimeBlockSaveDTOs);
    }

    saveTimeTerminal(terminal: TimeTerminalDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_TERMINAL, terminal);
    }

    reverseTransactionsValidation(employeeId: number, dates: Date[]) {
        var model = {
            employeeId: employeeId,
            dates: dates,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_PAYROLL_TRANSACTION_REVERSE_VALIDATION, model);
    }

    reverseTransactions(employeeId: number, dates: Date[], timeDeviationCauseId?: number, timePeriodId?: number, employeeChildId?: number) {
        var model = {
            employeeId: employeeId,
            employeeChildId: employeeChildId,
            timePeriodId: timePeriodId,
            timeDeviationCauseId: timeDeviationCauseId,
            dates: dates,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_PAYROLL_TRANSACTION_REVERSE, model);
    }

    searchTimeStampEntries(employeeIds: number[], dateFrom: Date, dateTo: Date) {
        var model = {
            employeeIds: employeeIds,
            dateFrom: dateFrom,
            dateTo: dateTo,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_STAMP_SEARCH, model);
    }

    saveAdjustedTimeStampEntries(items: TimeStampEntryDTO[]) {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_STAMP_SAVE, items);
    }

    sendEmailToPayrollAdministrator(timeSalaryExportId: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT_SEND_EMAIL + timeSalaryExportId, null);
    }

    sendPayrollToSftp(timeSalaryExportId: number): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT_SEND_SFTP + timeSalaryExportId, null);
    }

    validateExportSalary(employeeIds: number[], startDate: Date, stopDate: Date): ng.IPromise<IActionResult> {
        var model = {
            employeeIds: employeeIds,
            startDate: startDate,
            stopDate: stopDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT_VALIDATE, model);
    }

    exportSalary(employeeIds: number[], startDate: Date, stopDate: Date, exportTarget: number, lockPeriod: boolean, isPreliminary: boolean): ng.IPromise<IActionResult> {
        var model = {
            employeeIds: employeeIds,
            startDate: startDate,
            stopDate: stopDate,
            exportTarget: exportTarget,
            lockPeriod: lockPeriod,
            isPreliminary: isPreliminary,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT, model);
    }

    updateTimeRuleState(dict: any) {
        var model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_RULE_UPDATE_STATE, model);
    }

    exportTimeRules(timeRuleIds: number[]): ng.IPromise<any> {
        var model = {
            numbers: timeRuleIds
        }
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_RULE_EXPORT, model);
    }

    importTimeRulesMatch(model: TimeRuleExportImportDTO): ng.IPromise<TimeRuleExportImportDTO> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_RULE_IMPORT_MATCH, model);
    }

    importTimeRulesSave(model: TimeRuleExportImportDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_RULE_IMPORT_SAVE, model);
    }

    validateTimeRuleStructure(widgets: FormulaWidget[]): ng.IPromise<IActionResult> {
        var model = {
            widgets: widgets
        };

        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_RULE_VALIDATE_STRUCTURE, model);
    }

    saveTimeAccumulator(timeAccumulator: TimeAccumulatorDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR, timeAccumulator);
    }

    saveTimeRule(timeRule: TimeRuleEditDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_RULE, timeRule);
    }

    saveTimeHibernatingAbsence(timeHibernatingAbsence: ITimeHibernatingAbsenceHeadDTO): ng.IPromise<IActionResult> {
        var model = {
            timeHibernatingAbsenceHead: timeHibernatingAbsence
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_HIBERNATING_ABSCENCE, model);
    }

    createTransactionsForPlannedPeriodCalculation(employeeId: number, timePeriodId: number) {
        var model = {
            employeeId: employeeId, timePeriodId: timePeriodId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_EMPLOYEES_CREATETRANSACTIONFORPLANNEDPERIODCALCULATION, model);
    }

    getCalculationsFromPeriod(employeeIds: number[], timeperiodId: number) {
        var model = {
            employeeIds: employeeIds,
            timePeriodId: timeperiodId
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_EMPLOYEES_GETCALCULATIONFROMPERIOD, model);
    }

    // DELETE
    deleteEarnedHolidayTransactions(holidayId: number, employeeIds: number[], yearId: number) {
        var model = {
            holidayId: holidayId,
            employeeIds: employeeIds,
            year: yearId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_EARNED_HOLIDAY_DELETETRANSACTIONS, model);
    }

    deleteTimeAccumulator(timeAccumulatorId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_ACCUMULATOR + timeAccumulatorId);
    }

    deleteTimeAbsenceRule(timeAbsenceRuleId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_ABSENCERULE + timeAbsenceRuleId);
    }

    deleteTimeCode(timeCodeId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_CODE + timeCodeId);
    }

    deleteTimeCodeBreakGroup(timeCodeBreakGroupId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_CODE_BREAK_GROUP + timeCodeBreakGroupId);
    }

    deleteTimePeriodHead(timePeriodHeadId: number, removePeriodLinks: boolean) {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_PERIOD_HEAD + timePeriodHeadId + "/" + removePeriodLinks);
    }

    deleteTimeRule(timeRuleId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_RULE + timeRuleId);
    }

    deleteTimeSalaryExport(timeSalaryExportId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_TIME_TIME_SALARY_EXPORT + timeSalaryExportId);
    }
    deleteTimeDeviationCause(timeDeviationCauses: ITimeDeviationCauseDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_DEVIATION_CAUSE + "/Delete", timeDeviationCauses);
    }
}