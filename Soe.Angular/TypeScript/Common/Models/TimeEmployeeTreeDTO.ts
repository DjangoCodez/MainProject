import { ITimeEmployeeTreeDTO, ITimeEmployeeTreeGroupNodeDTO, ITimeEmployeeTreeNodeDTO, ITimeEmployeeTreeSettings, IPayrollCalculationPeriodSumDTO, IPayrollCalculationEmployeePeriodDTO, IAttestStateDTO, System, IAttestEmployeeBreakDTO, IAttestEmployeeDayDTO, IAttestEmployeePeriodDTO, IAttestEmployeeAdditionDeductionDTO, IAttestEmployeeAdditionDeductionTransactionDTO, ISaveAttestEmployeeDayDTO, IAttestEmployeeDayShiftDTO, IAttestEmployeeDayTimeBlockDTO, IAttestEmployeeDayTimeStampDTO, IAttestEmployeeDayTimeCodeTransactionDTO, IAttestEmployeeDayTimeInvoiceTransactionDTO, ITimeCodeDTO, IAttestEmployeeDaySmallDTO, IAttestEmployeesDaySmallDTO, ITimeAttestCalculationFunctionValidationDTO, ITimeUnhandledShiftChangesEmployeeDTO, ITimeUnhandledShiftChangesWeekDTO, ITimeTreeEmployeeWarning, IEmployeeAttestResult, IEmployeesAttestResult } from "../../Scripts/TypeLite.Net4";
import { AttestStateDTO } from "./AttestStateDTO";
import { AttestPayrollTransactionDTO } from "./AttestPayrollTransactionDTO";
import { SoeEntityState, TermGroup_ExpenseType, TermGroup_AttestTreeGrouping, TermGroup_AttestTreeSorting, SoeAttestTreeMode, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, TermGroup_TimeScheduleTemplateBlockType, TermGroup_TimeStampEntryStatus, TimeStampEntryType, TermGroup_TimeCodeRegistrationType, SoeTimeCodeType, SoeTimeTransactionType, TermGroup_TimeReportType, SoeTimeAttestFunctionOption, TermGroup_RecalculateTimeRecordStatus, SoeEmploymentFinalSalaryStatus, SoeTimeAttestInformation, SoeTimeAttestWarning, SoeTimeAttestWarningGroup, TermGroup_Sex, TermGroup_TimeStampEntryOriginType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { ShiftDTO } from "./TimeSchedulePlanningDTOs";
import { ProjectTimeBlockDTO } from "./ProjectDTO";
import { TimeBlockDateDTO } from "./TimeBlockDateDTO";
import { PayrollImportEmployeeTransactionDTO } from "./PayrollImport";
import { TimePeriodDTO } from "./TimePeriodDTO";
import { TimeStampEntryExtendedDTO } from "./TimeStampDTOs";
import { AccountInternalDTO } from "./AccountInternalDTO";

export class TimeEmployeeTreeDTO implements ITimeEmployeeTreeDTO {
    actorCompanyId: number;
    cacheKey: string;
    grouping: TermGroup_AttestTreeGrouping;
    groupNodes: TimeEmployeeTreeGroupNodeDTO[];
    hibernatingText: string;
    mode: SoeAttestTreeMode;
    sorting: TermGroup_AttestTreeSorting;
    startDate: Date;
    stopDate: Date;
    settings: TimeEmployeeTreeSettings;
    timePeriod: TimePeriodDTO;

    // Extensions
    employeeIds: number[];
    ignoreEmploymentStopDate: boolean;

    constructor() {
    }

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }

    public static createInstance(inputTree: any) {
        var tree = new TimeEmployeeTreeDTO();
        angular.extend(tree, inputTree);
        tree.fixDates();
        if (tree.groupNodes) {
            tree.groupNodes = tree.groupNodes.map(groupNode => {
                return TimeEmployeeTreeGroupNodeDTO.loadGroupRecursive(groupNode);
            });
        }
        return tree;
    }

    public filterEmployees(filterText: string) {
        if (filterText && filterText.length > 0)
            filterText = filterText.toLocaleLowerCase();
        _.forEach(this.groupNodes, groupNode => {
            this.filterEmployeesRecursive(groupNode, filterText);
        });
    }

    private filterEmployeesRecursive(groupNode: TimeEmployeeTreeGroupNodeDTO, filterText: string) {
        groupNode.filterEmployees(filterText);
        _.forEach(groupNode.childGroupNodes, childGroupNode => {
            this.filterEmployeesRecursive(childGroupNode, filterText);
        });
    }

    public getVisibleEmployeeIds(): number[] {
        var allEmployeeIds: number[] = [];
        _.forEach(this.groupNodes, groupNode => {
            var employeeIds = groupNode.getVisibleEmployeeIds(true);
            _.forEach(employeeIds, employeeId => {
                allEmployeeIds.push(employeeId);
            });
        });
        return _.uniq(allEmployeeIds);
    }
}

export class TimeEmployeeTreeGroupNodeDTO implements ITimeEmployeeTreeGroupNodeDTO {
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStates: IAttestStateDTO[];
    attestStateSort: number;
    childGroupNodes: TimeEmployeeTreeGroupNodeDTO[];
    definedSort: number;
    code: string;
    employeeNodes: TimeEmployeeTreeNodeDTO[];
    expanded: boolean;
    guid: System.IGuid;
    hasEnded: boolean;
    hasWarningsTime: boolean;
    hasWarningsPayroll: boolean;
    hasWarningsPayrollStopping: boolean;
    id: number;
    isAdditional: boolean;
    name: string;
    timeEmployeePeriods: AttestEmployeePeriodDTO[];
    type: number;
    warningMessageTime: string;
    warningMessagePayroll: string;
    warningMessagePayrollStopping: string;

    //Extensions
    totalCount: number;
    preview: boolean;
    payrollEmployeePeriods: PayrollCalculationEmployeePeriodDTO[];
    filteredPayrollEmployeePeriods: PayrollCalculationEmployeePeriodDTO[];
    filteredTimeEmployeePeriods: AttestEmployeePeriodDTO[];

    //Current
    currentWarningMessageTime: string;
    currentWarningMessagePayroll: string;
    currentWarningMessagePayrollStopping: string;
    currentHasWarningsTime: boolean;
    currentHasWarningsPayroll: boolean;
    currentHasWarningsPayrollStopping: boolean;
    currentAttestStateName: string;
    currentAttestStateColor: string;

    public static createInstance(inputGroupNode: any) {
        return this.loadGroupRecursive(inputGroupNode);
    }

    public static loadGroupRecursive(inputGroupNode: any): TimeEmployeeTreeGroupNodeDTO {
        let groupNode = new TimeEmployeeTreeGroupNodeDTO();
        angular.extend(groupNode, inputGroupNode);
        if (groupNode.employeeNodes) {
            groupNode.employeeNodes = groupNode.employeeNodes.map(e => {
                let employeeObj = new TimeEmployeeTreeNodeDTO();
                angular.extend(employeeObj, e);
                employeeObj.fixDates();
                return employeeObj;
            });
        }

        if (groupNode.childGroupNodes) {
            groupNode.childGroupNodes = groupNode.childGroupNodes.map(childGroupNode => {
                return this.loadGroupRecursive(childGroupNode);
            });
        }

        groupNode.setDefaultWarnings();
        groupNode.setDefaultAttestState();

        return groupNode;
    }

    public setDefaultWarnings() {
        this.currentWarningMessageTime = this.warningMessageTime;
        this.currentWarningMessagePayroll = this.warningMessagePayroll;
        this.currentWarningMessagePayrollStopping = this.warningMessagePayrollStopping;

        this.currentHasWarningsTime = this.hasWarningsTime;
        this.currentHasWarningsPayroll = this.hasWarningsPayroll;
        this.currentHasWarningsPayrollStopping = this.hasWarningsPayrollStopping;
    }

    public setCurrentWarnings(warningMessageTime: string[], warningMessagePayroll: string[], warningMessagePayrollStopping: string[]) {
        this.currentWarningMessageTime = this.getWarningMessage(warningMessageTime);
        this.currentWarningMessagePayroll = this.getWarningMessage(warningMessagePayroll);
        this.currentWarningMessagePayrollStopping = this.getWarningMessage(warningMessagePayrollStopping);

        this.currentHasWarningsTime = this.currentWarningMessageTime.length > 1;
        this.currentHasWarningsPayroll = this.currentWarningMessagePayroll.length > 1;
        this.currentHasWarningsPayrollStopping = this.currentWarningMessagePayrollStopping.length > 1;
    }

    private getWarningMessage(warningMessages: string[]): string {
        var message = '';
        _.forEach(warningMessages, warningMessage => {
            if (message.length > 1)
                message += ',';
            message += warningMessage;
        });
        return message;
    }

    public setDefaultAttestState() {
        this.currentAttestStateColor = this.attestStateColor;
        this.currentAttestStateName = this.attestStateName;
    }

    public setAttestState(color: string, name: string) {
        this.currentAttestStateColor = color;
        this.currentAttestStateName = name;
    }

    public getVisibleEmployeeNodes(checkChildren: boolean = false): TimeEmployeeTreeNodeDTO[] {
        var employees = _.filter(this.employeeNodes, e => e.visible);
        if (checkChildren) {
            _.forEach(this.childGroupNodes, childGroupNode => {
                _.forEach(childGroupNode.getVisibleEmployeeNodes(true), employeeNode => {
                    employees.push(employeeNode);
                });
            });
        }
        return employees;
    }

    public getVisibleEmployeeIds(checkChildren: boolean = false): number[] {
        var ids: number[] = [];
        _.forEach(this.getVisibleEmployeeNodes(checkChildren), (employeeNode: TimeEmployeeTreeNodeDTO) => {
            ids.push(employeeNode.employeeId);
        });
        return _.uniq(ids);
    }

    public filterEmployees(filterText: string) {
        _.forEach(this.employeeNodes, employeeNode => {
            employeeNode.filter(filterText);
        });
    }

    public hasVisibleEmployees(checkChildren: boolean = false): boolean {
        var visibleEmployeeIds = this.getVisibleEmployeeIds(true);
        return visibleEmployeeIds && visibleEmployeeIds.length > 0;
    }

    public doShowGroup(filterText: string): boolean {
        if (!filterText || filterText.length == 0)
            return true;
        var employeeIds = this.getVisibleEmployeeIds(true);
        return employeeIds && employeeIds.length > 0;
    }

    public isAnyEmployeeStamping() {
        return _.filter(this.employeeNodes, e => e.isStamping).length > 0;
    }
}

export class TimeEmployeeTreeNodeDTO implements ITimeEmployeeTreeNodeDTO {
    additionalOnAccountIds: number[];
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStates: IAttestStateDTO[];
    attestStateSort: number;
    autogenTimeblocks: boolean;
    disbursementAccountNr: string;
    disbursementAccountNrIsMissing: boolean;
    disbursementMethod: number;
    disbursementMethodIsCash: boolean;
    disbursementMethodIsUnknown: boolean;
    disbursementMethodName: string;
    employeeEndDate: Date;
    employeeFirstName: string;
    employeeGroupId: number;
    employeeId: number;
    employeeLastName: string;
    employeeName: string;
    employeeNr: string;
    employeeNrAndName: string;
    employeeSex: TermGroup_Sex;
    finalSalaryStatus: SoeEmploymentFinalSalaryStatus;
    groupId: number;
    guid: System.IGuid;
    hasEnded: boolean;
    hasWarningsTime: boolean;
    hasWarningsPayroll: boolean;
    hasWarningsPayrollStopping: boolean;
    hibernatingText: string;
    isStamping: boolean;
    isAdditional: boolean;
    socialSec: string;
    taxSettingsAreMissing: boolean;
    timeReportType: TermGroup_TimeReportType;
    tooltip: string;
    userId: number;
    visible: boolean;
    warningMessageTime: string;
    warningMessagePayroll: string;
    warningMessagePayrollStopping: string;

    //Extensions
    employeeGroupName: string;
    payrollGroupId: number;
    payrollGroupName: string;
    vacationGroupId: number;
    vacationGroupName: string;
    employmentPercent: number;
    note: string;
    showNote: boolean;
    finalSalaryEndDate: Date;
    finalSalaryEndDateApplied: Date;
    finalSalaryAppliedTimePeriodId: number;
    taxRate: number;
    adjustmentValue: number;
    adjustmentType: number;
    currentEmploymentTypeString: string;
    currentEmploymentDateFromString: string;
    currentEmploymentDateToString: string;
    applyEmploymentTaxMinimumRule: boolean;
    taxTableInfo: string;

    constructor() {
        this.visible = true;
    }

    public fixDates() {
        this.employeeEndDate = CalendarUtility.convertToDate(this.employeeEndDate);
    }

    public filter(filterText: string) {
        this.visible = (!filterText || filterText.length === 0 || this.employeeNrAndName.toLocaleLowerCase().includes(filterText));
    }

    public getWarningMessagesTime(): string[] {
        return this.splitWarningMessage(this.hasWarningsTime, this.warningMessageTime);
    }
    public getWarningMessagesPayroll(): string[] {
        return this.splitWarningMessage(this.hasWarningsPayroll, this.warningMessagePayroll);
    }
    public getWarningMessagesPayrollStopping(): string[] {
        return this.splitWarningMessage(this.hasWarningsPayrollStopping, this.warningMessagePayrollStopping);
    }
    private splitWarningMessage(hasWarning: boolean, warningMessage: string): string[] {
        var messages: string[] = [];
        if (hasWarning && warningMessage) {
            _.forEach(warningMessage.split(','), warning => {
                warning = warning.trim();
                if (messages.indexOf(warning) < 0)
                    messages.push(warning);
            });
        }
        return messages;
    }
}

export class TimeTreeEmployeeWarning implements ITimeTreeEmployeeWarning {
    employeeId: number;
    isStopping: boolean;
    key: number;
    message: string;
    warningGroup: SoeTimeAttestWarningGroup;
}

export class TimeEmployeeTreeSettings implements ITimeEmployeeTreeSettings {
    cacheKeyToUse: string;
    doNotShowAttested: boolean;
    doNotShowCalculated: boolean;
    doNotShowDaysOutsideEmployeeAccount: boolean;
    doNotShowWithoutTransactions: boolean;
    doRefreshFinalSalaryStatus: boolean;
    doShowOnlyShiftSwaps: boolean;
    excludeDuplicateEmployees: boolean;
    filterAttestStateIds: number[];
    filterEmployeeAuthModelIds: number[];
    filterEmployeeIds: number[];
    filterMessageGroupId: number;
    includeAdditionalEmployees: boolean;
    includeEmptyGroups: boolean;
    includeEnded: boolean;
    isProjectAttest: boolean;
    searchPattern: string;
    showOnlyAppliedFinalSalary: boolean;
    showOnlyApplyFinalSalary: boolean;
}

export class AttestEmployeePeriodDTO implements IAttestEmployeePeriodDTO {
    absenceTime: System.ITimeSpan;
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStates: IAttestStateDTO[];
    attestStateSort: number;
    employeeId: number;
    employeeInfo: string;
    employeeNrAndName: string;
    employeeName: string;
    employeeNr: string;
    employeeSex: TermGroup_Sex;
    hasAbsenceTime: boolean;
    hasExpense: boolean;
    hasInformations: boolean;
    hasInvalidTimeStamps: boolean;
    hasOvertime: boolean;
    hasSameAttestState: boolean;
    hasPayrollImports: boolean;
    hasTimeStampsWithoutTransactions: boolean;
    hasScheduleWithoutTransactions: boolean;
    hasShiftSwaps: boolean;
    hasStandbyTime: boolean;
    hasTransactions: boolean;
    hasWarnings: boolean;
    hasWorkedInsideSchedule: boolean;
    hasWorkedOutsideSchedule: boolean;
    informations: SoeTimeAttestInformation[];
    presenceBreakMinutes: number;
    presenceBreakTime: System.ITimeSpan;
    presenceBreakTimeInfo: string;
    presenceDays: number;
    presenceTime: System.ITimeSpan;
    presenceTimeInfo: string;
    presencePayedTime: System.ITimeSpan;
    presencePayedTimeInfo: string;
    scheduleBreakMinutes: number;
    scheduleBreakTime: System.ITimeSpan;
    scheduleBreakTimeInfo: string;
    scheduleDays: number;
    scheduleTime: System.ITimeSpan;
    scheduleTimeInfo: string;
    standbyTime: System.ITimeSpan;
    startDate: Date;
    stopDate: Date;
    sumAdditionDeductionAmount: number;
    sumAdditionDeductionRows: number;
    sumAdditionDeductionTime: System.ITimeSpan;
    sumAdditionDeductionTimeText: string;
    sumExpenseAmount: number;
    sumExpenseRows: number;
    sumGrossSalaryAbsence: System.ITimeSpan;
    sumGrossSalaryAbsenceLeaveOfAbsence: System.ITimeSpan;
    sumGrossSalaryAbsenceLeaveOfAbsenceText: string;
    sumGrossSalaryAbsenceParentalLeave: System.ITimeSpan;
    sumGrossSalaryAbsenceParentalLeaveText: string;
    sumGrossSalaryAbsenceSick: System.ITimeSpan;
    sumGrossSalaryAbsenceSickText: string;
    sumGrossSalaryAbsenceTemporaryParentalLeave: System.ITimeSpan;
    sumGrossSalaryAbsenceTemporaryParentalLeaveText: string;
    sumGrossSalaryAbsenceText: string;
    sumGrossSalaryAbsenceVacation: System.ITimeSpan;
    sumGrossSalaryAbsenceVacationText: string;
    sumGrossSalaryAdditionalTime: System.ITimeSpan;
    sumGrossSalaryAdditionalTimeText: string;
    sumGrossSalaryDuty: System.ITimeSpan;
    sumGrossSalaryDutyText: string;
    sumGrossSalaryOBAddition: System.ITimeSpan;
    sumGrossSalaryOBAddition100: System.ITimeSpan;
    sumGrossSalaryOBAddition100Text: string;
    sumGrossSalaryOBAddition113: System.ITimeSpan;
    sumGrossSalaryOBAddition113Text: string;
    sumGrossSalaryOBAddition40: System.ITimeSpan;
    sumGrossSalaryOBAddition40Text: string;
    sumGrossSalaryOBAddition50: System.ITimeSpan;
    sumGrossSalaryOBAddition50Text: string;
    sumGrossSalaryOBAddition57: System.ITimeSpan;
    sumGrossSalaryOBAddition57Text: string;
    sumGrossSalaryOBAddition70: System.ITimeSpan;
    sumGrossSalaryOBAddition70Text: string;
    sumGrossSalaryOBAddition79: System.ITimeSpan;
    sumGrossSalaryOBAddition79Text: string;
    sumGrossSalaryOBAdditionText: string;
    sumGrossSalaryOvertime: System.ITimeSpan;
    sumGrossSalaryOvertime100: System.ITimeSpan;
    sumGrossSalaryOvertime100Text: string;
    sumGrossSalaryOvertime50: System.ITimeSpan;
    sumGrossSalaryOvertime50Text: string;
    sumGrossSalaryOvertime70: System.ITimeSpan;
    sumGrossSalaryOvertime70Text: string;
    sumGrossSalaryOvertimeText: string;
    sumGrossSalaryWeekendSalary: System.ITimeSpan;
    sumGrossSalaryWeekendSalaryText: string;
    sumInvoicedTime: System.ITimeSpan;
    sumInvoicedTimeText: string;
    sumTimeAccumulator: System.ITimeSpan;
    sumTimeAccumulatorOverTime: System.ITimeSpan;
    sumTimeAccumulatorOverTimeText: string;
    sumTimeAccumulatorText: string;
    sumTimeWorkedScheduledTime: System.ITimeSpan;
    sumTimeWorkedScheduledTimeText: string;
    unhandledEmployee: TimeUnhandledShiftChangesEmployeeDTO;
    uniqueId: string;
    warnings: SoeTimeAttestWarning[];

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class AttestEmployeeDayDTO implements IAttestEmployeeDayDTO {
    absenceTime: System.ITimeSpan;
    alwaysDiscardBreakEvaluation: boolean;
    attestPayrollTransactions: AttestPayrollTransactionDTO[];
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStates: AttestStateDTO[];
    attestStateSort: number;
    autogenTimeblocks: boolean;
    containsDuplicateTimeBlocks: boolean;
    date: Date;
    hasPayrollImports: boolean;
    day: number;
    dayName: string;
    dayNumber: number;
    dayOfWeekNr: number;
    dayTypeId: number;
    dayTypeName: string;
    discardedBreakEvaluation: boolean;
    earningTimeAccumulatorId?: number;
    employeeGroupId: number;
    employeeId: number;
    employeeScheduleId: number;
    hasAbsenceTime: boolean;
    hasExpense: boolean;
    hasComment: boolean;
    hasDeviations: boolean;
    hasInformations: boolean;
    hasInvalidTimeStamps: boolean;
    hasNoneInitialTransactions: boolean;
    hasOvertime: boolean;
    hasPayrollImportEmployeeTransactions: boolean;
    hasPayrollImportWarnings: boolean;
    hasPayrollScheduleTransactions: boolean;
    hasPayrollTransactions: boolean;
    hasPeriodDiscardedBreakEvaluation: boolean;
    hasPeriodOvertime: boolean;
    hasPeriodTimeScheduleTypeFactorMinutes: boolean;
    hasPeriodTimeWorkReduction: boolean;
    hasSameAttestState: boolean;
    hasScheduledPlacement: boolean;
    hasScheduleWithoutTransactions: boolean;
    hasShiftSwaps: boolean;
    hasStandbyTime: boolean;
    hasTimeStampEntries: boolean;
    hasTimeStampsWithoutTransactions: boolean;
    hasTransactions: boolean;
    hasTimeWorkReduction: boolean;
    hasUnhandledExtraShiftChanges: boolean;
    hasUnhandledShiftChanges: boolean;
    hasWarnings: boolean;
    hasWorkedInsideSchedule: boolean;
    hasWorkedOutsideSchedule: boolean;
    holidayAndDayTypeName: string;
    holidayId: number;
    holidayName: string;
    isAbsenceDay: boolean;
    isAttested: boolean;
    isCompletelyAdditional: boolean;
    isGeneratingTransactions: boolean;
    isHoliday: boolean;
    informations: SoeTimeAttestInformation[];
    isInfoVisible: boolean;
    isNotScheduleTime: boolean;
    isPartlyAdditional: boolean;
    isPrel: boolean;
    isPreliminary: string;
    isReadonly: boolean;
    isScheduleChangedFromTemplate: boolean;
    isScheduleZeroDay: boolean;
    isSelected: boolean;
    isTemplateScheduleZeroDay: boolean;
    isVisible: boolean;
    isWholedayAbsence: boolean;
    manuallyAdjusted: boolean;
    noOfScheduleBreaks: number;
    occupiedTime: System.ITimeSpan;
    payrollAddedTimeMinutes: number;
    payrollImportEmployeeTransactions: PayrollImportEmployeeTransactionDTO[];
    payrollInconvinientWorkingHoursMinutes: number;
    payrollInconvinientWorkingHoursScaledMinutes: number;
    payrollOverTimeMinutes: number;
    payrollWorkMinutes: number;
    presenceBreakItems: IAttestEmployeeBreakDTO[];
    presenceBreakMinutes: number;
    presenceBreakTime: System.ITimeSpan;
    presenceInsideScheduleTime: System.ITimeSpan;
    presenceOutsideScheduleTime: System.ITimeSpan;
    presencePayedTime: System.ITimeSpan;
    presenceStartTime: Date;
    presenceStopTime: Date;
    presenceTime: System.ITimeSpan;
    projectTimeBlocks: ProjectTimeBlockDTO[];
    recalculateTimeRecordId: number;
    recalculateTimeRecordStatus: TermGroup_RecalculateTimeRecordStatus;
    scheduleBreak1Minutes: number;
    scheduleBreak1Start: Date;
    scheduleBreak2Minutes: number;
    scheduleBreak2Start: Date;
    scheduleBreak3Minutes: number;
    scheduleBreak3Start: Date;
    scheduleBreak4Minutes: number;
    scheduleBreak4Start: Date;
    scheduleBreakMinutes: number;
    scheduleBreakTime: System.ITimeSpan;
    scheduleStartTime: Date;
    scheduleStopTime: Date;
    scheduleTime: System.ITimeSpan;
    shiftUserStatuses: TermGroup_TimeScheduleTemplateBlockShiftUserStatus[];
    standbyTime: System.ITimeSpan;
    sumExpenseAmount: number;
    sumExpenseRows: number;
    sumGrossSalaryAbsence: System.ITimeSpan;
    sumGrossSalaryAbsenceLeaveOfAbsence: System.ITimeSpan;
    sumGrossSalaryAbsenceParentalLeave: System.ITimeSpan;
    sumGrossSalaryAbsenceSick: System.ITimeSpan;
    sumGrossSalaryAbsenceTemporaryParentalLeave: System.ITimeSpan;
    sumGrossSalaryAbsenceText: string;
    sumGrossSalaryAbsenceVacation: System.ITimeSpan;
    sumGrossSalaryAdditionalTime: System.ITimeSpan;
    sumGrossSalaryDuty: System.ITimeSpan;
    sumGrossSalaryOBAddition: System.ITimeSpan;
    sumGrossSalaryOBAddition100: System.ITimeSpan;
    sumGrossSalaryOBAddition113: System.ITimeSpan;
    sumGrossSalaryOBAddition40: System.ITimeSpan;
    sumGrossSalaryOBAddition50: System.ITimeSpan;
    sumGrossSalaryOBAddition57: System.ITimeSpan;
    sumGrossSalaryOBAddition70: System.ITimeSpan;
    sumGrossSalaryOBAddition79: System.ITimeSpan;
    sumGrossSalaryOvertime: System.ITimeSpan;
    sumGrossSalaryOvertime100: System.ITimeSpan;
    sumGrossSalaryOvertime50: System.ITimeSpan;
    sumGrossSalaryOvertime70: System.ITimeSpan;
    sumGrossSalaryWeekendSalary: System.ITimeSpan;
    sumInvoicedTime: System.ITimeSpan;
    sumTimeAccumulator: System.ITimeSpan;
    sumTimeAccumulatorOverTime: System.ITimeSpan;
    sumTimeWorkedScheduledTime: System.ITimeSpan;
    templateScheduleBreak1Minutes: number;
    templateScheduleBreak1Start: Date;
    templateScheduleBreak2Minutes: number;
    templateScheduleBreak2Start: Date;
    templateScheduleBreak3Minutes: number;
    templateScheduleBreak3Start: Date;
    templateScheduleBreak4Minutes: number;
    templateScheduleBreak4Start: Date;
    templateScheduleBreakMinutes: number;
    templateScheduleBreakTime: System.ITimeSpan;
    templateScheduleStartTime: Date;
    templateScheduleStopTime: Date;
    templateScheduleTime: System.ITimeSpan;
    timeBlockDateId: number;
    timeBlockDateStampingStatus: number;
    timeBlockDateStatus: number;
    timeBlocks: AttestEmployeeDayTimeBlockDTO[];
    timeCodeTransactions: AttestEmployeeDayTimeCodeTransactionDTO[];
    timeDeviationCauseNames: string;
    timeInvoiceTransactions: AttestEmployeeDayTimeInvoiceTransactionDTO[];
    timeReportType: number;
    timeScheduleTemplateHeadId: number;
    timeScheduleTemplateHeadName: string;
    timeScheduleTemplatePeriodId: number;
    timeScheduleTypeFactorMinutes: number;
    timeStampEntrys: AttestEmployeeDayTimeStampDTO[];
    unhandledEmployee: TimeUnhandledShiftChangesEmployeeDTO;
    uniqueId: string;
    warnings: SoeTimeAttestWarning[];
    weekInfo: string;
    weekNr: number;
    weekNrMonday: string;
    wholedayAbsenseTimeDeviationCauseFromTimeBlock: number;

    // Extensions
    isMyTime: boolean;
    hasAttestStates: boolean;
    shifts: ShiftDTO[];
    standbyShifts: ShiftDTO[];
    additionalStatusIconValue: string;
    additionalStatusIconMessage: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
        this.presenceStartTime = CalendarUtility.convertToDate(this.presenceStartTime);
        this.presenceStopTime = CalendarUtility.convertToDate(this.presenceStopTime);
        this.scheduleBreak1Start = CalendarUtility.convertToDate(this.scheduleBreak1Start);
        this.scheduleBreak2Start = CalendarUtility.convertToDate(this.scheduleBreak2Start);
        this.scheduleBreak3Start = CalendarUtility.convertToDate(this.scheduleBreak3Start);
        this.scheduleBreak4Start = CalendarUtility.convertToDate(this.scheduleBreak4Start);
        this.scheduleStartTime = CalendarUtility.convertToDate(this.scheduleStartTime);
        this.scheduleStopTime = CalendarUtility.convertToDate(this.scheduleStopTime);
        this.templateScheduleBreak1Start = CalendarUtility.convertToDate(this.templateScheduleBreak1Start);
        this.templateScheduleBreak2Start = CalendarUtility.convertToDate(this.templateScheduleBreak2Start);
        this.templateScheduleBreak3Start = CalendarUtility.convertToDate(this.templateScheduleBreak3Start);
        this.templateScheduleBreak4Start = CalendarUtility.convertToDate(this.templateScheduleBreak4Start);
        this.templateScheduleStartTime = CalendarUtility.convertToDate(this.templateScheduleStartTime);
        this.templateScheduleStopTime = CalendarUtility.convertToDate(this.templateScheduleStopTime);
    }

    public setTypes() {
        if (this.shifts) {
            this.shifts = this.shifts.map(s => {
                let sObj = new ShiftDTO();
                angular.extend(sObj, s);
                sObj.fixDates();
                sObj.fixColors();
                return sObj;
            });
        } else {
            this.shifts = [];
        }

        if (this.standbyShifts) {
            this.standbyShifts = this.standbyShifts.map(sb => {
                let sbObj = new ShiftDTO();
                angular.extend(sbObj, sb);
                sbObj.fixDates();
                sbObj.fixColors();
                return sbObj;
            });
        } else {
            this.standbyShifts = [];
        }

        if (this.timeStampEntrys) {
            this.timeStampEntrys = this.timeStampEntrys.map(s => {
                let sObj = new AttestEmployeeDayTimeStampDTO();
                angular.extend(sObj, s);
                sObj.fixDates();
                sObj.setTypes();
                return sObj;
            });
        } else {
            this.timeStampEntrys = [];
        }

        if (this.projectTimeBlocks) {
            this.projectTimeBlocks = this.projectTimeBlocks.map(b => {
                let bObj = new ProjectTimeBlockDTO();
                angular.extend(bObj, b);
                bObj.fixDates();
                return bObj;
            });
        } else {
            this.projectTimeBlocks = [];
        }

        if (this.timeBlocks) {
            this.timeBlocks = this.timeBlocks.map(b => {
                let bObj = new AttestEmployeeDayTimeBlockDTO();
                angular.extend(bObj, b);
                bObj.fixDates();
                return bObj;
            });
        } else {
            this.timeBlocks = [];
        }

        if (this.attestPayrollTransactions) {
            this.attestPayrollTransactions = this.attestPayrollTransactions.map(p => {
                let pObj = new AttestPayrollTransactionDTO();
                angular.extend(pObj, p);
                pObj.fixDates();
                return pObj;
            });
        } else {
            this.attestPayrollTransactions = [];
        }

        if (this.timeCodeTransactions) {
            this.timeCodeTransactions = this.timeCodeTransactions.map(c => {
                let cObj = new AttestEmployeeDayTimeCodeTransactionDTO();
                angular.extend(cObj, c);
                cObj.fixDates();
                return cObj;
            });
        } else {
            this.timeCodeTransactions = [];
        }

        if (this.payrollImportEmployeeTransactions) {
            this.payrollImportEmployeeTransactions = this.payrollImportEmployeeTransactions.map(t => {
                let tObj = new PayrollImportEmployeeTransactionDTO();
                angular.extend(tObj, t);
                tObj.fixDates();
                tObj.setTypes();
                return tObj;
            });
        } else {
            this.payrollImportEmployeeTransactions = [];
        }
    }

    public setAdditionalStatus(partlyMessage: string, completelyMessage: string) {
        if (this.isPartlyAdditional) {
            this.additionalStatusIconValue = 'fal fa-map-marker-exclamation';
            this.additionalStatusIconMessage = partlyMessage;
        }
        else if (this.isCompletelyAdditional) {
            this.additionalStatusIconValue = 'fas fa-map-marker-exclamation';
            this.additionalStatusIconMessage = completelyMessage;
        }
    }

    public clearPresence() {
        this.presenceBreakItems = [];
        this.presenceBreakMinutes = 0;
        this.presenceBreakTime = null;
        this.presencePayedTime = null;
        this.presenceStartTime = null;
        this.presenceStopTime = null;
        this.presenceTime = null;
        this.attestPayrollTransactions = null;
        this.attestStates = undefined;
        this.hasAttestStates = false;
        this.attestStateColor = null;
        this.attestStateName = null;
        this.hasNoneInitialTransactions = false;
    }
}

export class AttestEmployeeAdditionDeductionDTO implements IAttestEmployeeAdditionDeductionDTO {
    accounting: string;
    amount: number;
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    comment: string;
    customerInvoiceId: number;
    customerInvoiceRowId: number;
    customerName: string;
    customerNumber: string;
    expenseRowId: number;
    hasFiles: boolean;
    invoiceNumber: string;
    isAttested: boolean;
    isReadOnly: boolean;
    isSpecifiedUnitPrice: boolean;
    priceListInclVat: boolean;
    projectId: number;
    projectName: string;
    projectNumber: string;
    quantity: number;
    quantityText: string;
    registrationType: TermGroup_TimeCodeRegistrationType;
    start: Date;
    state: SoeEntityState;
    stop: Date;
    timeCodeComment: string;
    timeCodeCommentMandatory: boolean;
    timeCodeExpenseType: TermGroup_ExpenseType;
    timeCodeId: number;
    timeCodeName: string;
    timeCodeStopAtAccounting: boolean;
    timeCodeStopAtComment: boolean;
    timeCodeStopAtDateStart: boolean;
    timeCodeStopAtDateStop: boolean;
    timeCodeStopAtPrice: boolean;
    timeCodeStopAtVat: boolean;
    timeCodeTransactionId: number;
    timeCodeType: SoeTimeCodeType;
    timePeriodId: number;
    transactions: IAttestEmployeeAdditionDeductionTransactionDTO[];
    unitPrice: number;
    vatAmount: number;

    public fixDates() {
        this.start = CalendarUtility.convertToDate(this.start);
        this.stop = CalendarUtility.convertToDate(this.stop);
    }
}

export class AttestEmployeeAdditionDeductionTransactionDTO implements IAttestEmployeeAdditionDeductionTransactionDTO {
    accountingString: string;
    amount: number;
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    comment: string;
    date: Date;
    isAttested: boolean;
    isReadOnly: boolean;
    productId: number;
    productName: string;
    quantity: number;
    quantityText: string;
    transactionId: number;
    transactionType: SoeTimeTransactionType;
    unitPrice: number;
    vatAmount: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class AttestEmployeeDayShiftDTO implements IAttestEmployeeDayShiftDTO {
    accountId: number;
    break1Id: number;
    break1Minutes: number;
    break1StartTime: Date;
    break1TimeCode: string;
    break1TimeCodeId: number;
    break2Id: number;
    break2Minutes: number;
    break2StartTime: Date;
    break2TimeCode: string;
    break2TimeCodeId: number;
    break3Id: number;
    break3Minutes: number;
    break3StartTime: Date;
    break3TimeCode: string;
    break3TimeCodeId: number;
    break4Id: number;
    break4Minutes: number;
    break4StartTime: Date;
    break4TimeCode: string;
    break4TimeCodeId: number;
    description: string;
    employeeId: number;
    link: string;
    shiftTypeId: number;
    shiftTypeColor: string;
    shiftTypeDescription: string;
    shiftTypeName: string;
    startTime: Date;
    stopTime: Date;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    timeScheduleTemplateBlockId: number;
    timeScheduleTypeId: number;
    timeScheduleTypeName: string;
    type: TermGroup_TimeScheduleTemplateBlockType;

    public fixDates() {
        this.break1StartTime = CalendarUtility.convertToDate(this.break1StartTime);
        this.break2StartTime = CalendarUtility.convertToDate(this.break2StartTime);
        this.break3StartTime = CalendarUtility.convertToDate(this.break3StartTime);
        this.break4StartTime = CalendarUtility.convertToDate(this.break4StartTime);
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }
}

export class AttestEmployeeDayTimeBlockDTO implements IAttestEmployeeDayTimeBlockDTO {
    accountId: number;
    deviationAccounts: AccountInternalDTO[];
    comment: string;
    employeeChildId: number;
    employeeId: number;
    guidId: string;
    isAbsence: boolean;
    isBreak: boolean;
    isGeneratedFromBreak: boolean;
    isOutsideScheduleNotOvertime: boolean;
    isOvertime: boolean;
    isPreliminary: boolean;
    isPresence: boolean;
    isReadonlyLeft: boolean;
    isReadonlyRight: boolean;
    manuallyAdjusted: boolean;
    shiftTypeId: number;
    startTime: Date;
    stopTime: Date;
    timeBlockDateId: number;
    timeBlockId: number;
    timeCodes: ITimeCodeDTO[];
    timeDeviationCauseName: string;
    timeDeviationCauseStartId: number;
    timeDeviationCauseStopId: number;
    timeScheduleTemplateBlockBreakId: number;
    timeScheduleTemplatePeriodId: number;
    timeScheduleTypeId: number;

    // Extensions
    startTimeDuringMove: Date;
    stopTimeDuringMove: Date;
    toolTip: string;

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
    }

    public getStartTimeIfMoved() {
        return (this.startTimeDuringMove ? this.startTimeDuringMove : this.startTime);
    }

    public getStopTimeIfMoved() {
        return (this.stopTimeDuringMove ? this.stopTimeDuringMove : this.stopTime);
    }

    public getBlockLength() {
        return this.stopTime.diffMinutes(this.startTime);
    }

    public getBlockLengthDuringMove() {
        return this.getStopTimeIfMoved().diffMinutes(this.getStartTimeIfMoved());
    }
}

export class AttestEmployeeDayTimeStampDTO implements IAttestEmployeeDayTimeStampDTO {
    accountId: number;
    autoStampOut: boolean;
    employeeChildId: number;
    employeeId: number;
    employeeManuallyAdjusted: boolean;
    extended: TimeStampEntryExtendedDTO[];
    isBreak: boolean;
    isDistanceWork: boolean;
    isPaidBreak: boolean;
    manuallyAdjusted: boolean;
    note: string;
    originType: TermGroup_TimeStampEntryOriginType;
    shiftTypeId: number;
    status: TermGroup_TimeStampEntryStatus;
    time: Date;
    timeBlockDateId: number;
    timeDeviationCauseId: number;
    timeScheduleTemplatePeriodId: number;
    timeScheduleTypeId: number;
    timeScheduleTypeName: string;
    timeStampEntryId: number;
    timeTerminalAccountId: number;
    timeTerminalId: number;
    type: TimeStampEntryType;

    // Extensions
    tmpId: number; // Temp id for new entries
    isReadonly: boolean;
    isModified: boolean;
    timeTerminalAccountName: string;
    specifyChild: boolean;
    accountId2: number;
    accountId2FromExtendedId: number;

    public get identifier(): number {
        return this.timeStampEntryId ? this.timeStampEntryId : this.tmpId;
    }

    public get hasAccounts(): boolean {
        return this.extended && this.extended.filter(e => e.accountId).length > 0;
    }

    public get hasTimeCodes(): boolean {
        return this.extended && this.extended.filter(e => e.timeCodeId).length > 0;
    }

    public get hasTimeScheduleTypes(): boolean {
        return this.extended && this.extended.filter(e => e.timeScheduleTypeId).length > 0;
    }

    public fixDates() {
        this.time = CalendarUtility.convertToDate(this.time);
    }

    public setTypes() {
        if (this.extended) {
            this.extended = this.extended.map(x => {
                let obj = new TimeStampEntryExtendedDTO();
                angular.extend(obj, x);
                return obj;
            });
        } else {
            this.extended = [];
        }
    }
}

export class AttestEmployeeDayTimeCodeTransactionDTO implements IAttestEmployeeDayTimeCodeTransactionDTO {
    guidId: string;
    guidIdTimeBlock: string;
    isTimeCodeAbsenceTime: boolean;
    isTimeCodePresenceOutsideScheduleTime: boolean;
    quantity: number;
    quantityString: string;
    startTime: Date;
    stopTime: Date;
    timeBlockId: number;
    timeCodeId: number;
    timeCodeName: string;
    timeCodeRegistrationType: TermGroup_TimeCodeRegistrationType;
    timeCodeTransactionId: number;
    timeCodeType: SoeTimeCodeType;
    timeRuleId: number;
    timeRuleName: string;
    timeRuleSort: number;

    //Extensions
    isTimeZero: boolean;

    public fixDates() {
        this.startTime = CalendarUtility.convertToDate(this.startTime);
        this.stopTime = CalendarUtility.convertToDate(this.stopTime);
        this.isTimeZero = CalendarUtility.isTimeZero(this.startTime) && CalendarUtility.isTimeZero(this.stopTime);
    }
}

export class AttestEmployeeDayTimeInvoiceTransactionDTO implements IAttestEmployeeDayTimeInvoiceTransactionDTO {
    guidId: string;
}

export class PayrollCalculationPeriodSumDTO implements IPayrollCalculationPeriodSumDTO {
    gross: number;
    benefitInvertExcluded: number;
    tax: number;
    compensation: number;
    deduction: number;
    employmentTaxDebit: number;
    employmentTaxCredit: number;
    supplementChargeDebit: number;
    supplementChargeCredit: number;
    net: number;
    transactionNet: number;
    isTaxMissing: boolean;
    isEmploymentTaxMissing: boolean;
    hasEmploymentTaxDiff: boolean;
    hasSupplementChargeDiff: boolean;
    isNetSalaryMissing: boolean;
    isNetSalaryNegative: boolean;
    isGrossSalaryNegative: boolean;
    hasNetSalaryDiff: boolean;
    hasWarning: boolean;
}

export class PayrollCalculationEmployeePeriodDTO implements IPayrollCalculationEmployeePeriodDTO {
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStates: IAttestStateDTO[];
    attestStateSort: number;
    createdOrModified: Date;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeeNrAndName: string;
    hasAttestStates: boolean;
    periodSum: IPayrollCalculationPeriodSumDTO;
    timePeriodId: number;

    //Extensions
    attestStateOtherNames: string;
    periodSumGross: number;
    periodSumBenefitInvertExcluded: number;
    periodSumTax: number;
    periodSumCompensation: number;
    periodSumDeduction: number;
    periodSumEmploymentTax: number;
    periodSumNet: number;
    periodWarnings: string;

    public fixDates() {
        this.createdOrModified = CalendarUtility.convertToDate(this.createdOrModified);
    }

    public setSums() {
        this.periodSumGross = this.periodSum.gross;
        this.periodSumBenefitInvertExcluded = this.periodSum.benefitInvertExcluded;
        this.periodSumTax = this.periodSum.tax;
        this.periodSumCompensation = this.periodSum.compensation;
        this.periodSumDeduction = this.periodSum.deduction;
        this.periodSumEmploymentTax = this.periodSum.employmentTaxDebit;
        this.periodSumNet = this.periodSum.net;
    }
}

export class SaveAttestEmployeeDayDTO implements ISaveAttestEmployeeDayDTO {
    date: Date;
    originalUniqueId: string;
    timeBlockDateId: number;
}

export class TimeUnhandledShiftChangesEmployeeDTO implements ITimeUnhandledShiftChangesEmployeeDTO {
    employeeId: number;
    hasDays: boolean;
    hasExtraShiftDays: boolean;
    hasShiftDays: boolean;
    weeks: TimeUnhandledShiftChangesWeekDTO[];
}

export class TimeUnhandledShiftChangesWeekDTO implements ITimeUnhandledShiftChangesWeekDTO {
    dateFrom: Date;
    dateTo: Date;
    extraShiftDays: TimeBlockDateDTO[];
    hasDays: boolean;
    hasExtraShiftDays: boolean;
    hasShiftDays: boolean;
    shiftDays: TimeBlockDateDTO[];
    weekNr: number;

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
    }
}

export class AttestEmployeeDaySmallDTO implements IAttestEmployeeDaySmallDTO {
    date: Date;
    employeeId: number;
    timeBlockDateId: number;
    timeScheduleTemplatePeriodId: number;
}

export class AttestEmployeesDaySmallDTO implements IAttestEmployeesDaySmallDTO {
    dateFrom: Date;
    dateTo: Date;
    employeeId: number;
}

export class TimeAttestCalculationFunctionValidationDTO implements ITimeAttestCalculationFunctionValidationDTO {
    applySilent: boolean;
    canOverride: boolean;
    message: string;
    option: SoeTimeAttestFunctionOption;
    success: boolean;
    title: string;
    validItems: AttestEmployeeDaySmallDTO[];
}

export class TimeAttestCalculationFunctionDTO {
    employeeId: number;
    employeeNr: string
    employeeName: string;
    status: string;
    isProcessed: boolean;
    resultError: string;
    resultSuccess: boolean;

    public get numberAndName(): string {
        return this.employeeNr !== '0' ? "{0}, {1}".format(this.employeeNr, this.employeeName) : this.employeeName;
    }
}

export class EmployeesAttestResult implements IEmployeesAttestResult {
    attestStateToId: number;
    employeeResults: EmployeeAttestResult[];
    success: boolean;
}

export class EmployeeAttestResult implements IEmployeeAttestResult {
    employeeId: number;
    numberAndName: string;
    status: string;
    success: boolean;
    noOfTranscationsAttested: number;
    noOfTranscationsFailed: number;
    noOfDaysFailed: number;
    noOfDaysWithStampingErrors: number;
    datesFailedString: string;
}
