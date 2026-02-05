import { IHttpService } from "../../../Core/Services/httpservice";
import { Constants } from "../../../Util/Constants";
import { ShiftDTO, TimeScheduleShiftQueueDTO, OrderListDTO, AvailableTimeDTO, ShiftHistoryDTO, TimeScheduleScenarioHeadDTO, TimeScheduleScenarioAccountDTO, TimeScheduleScenarioEmployeeDTO, TimeLeisureCodeSmallDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { IEmployeeSkillDTO, IEmployeePostSkillDTO, ITimeCodeBreakSmallDTO, IEvaluateWorkRulesActionResult, IActionResult, IShiftRequestStatusDTO, IShiftAccountingDTO, IEmployeeRequestDTO, IExtendedAbsenceSettingDTO, IAvailableEmployeesDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TermGroup_TimeScheduleTemplateBlockQueueType, DragShiftAction, SoeScheduleWorkRules, SoeTimeAttestFunctionOption, TermGroup_EmployeeRequestType, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, HandleShiftAction, TermGroup_AssignmentTimeAdjustmentType, TimeSchedulePlanningDisplayMode, TermGroup_TimeScheduleTemplateBlockType, TimeSchedulePlanningMode, TermGroup_TimeScheduleCopyHeadType } from "../../../Util/CommonEnumerations";
import { AttestEmployeeDaySmallDTO, AttestEmployeesDaySmallDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { TimeScheduleTemplateBlockTaskDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { EmployeeRequestDTO } from "../../../Common/Models/EmployeeRequestDTO";
import { TimeScheduleTypeSmallDTO, TimeScheduleTypeFactorSmallDTO } from "../../../Common/Models/TimeScheduleTypeDTO";
import { ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";
import { AccountingSettingsRowDTO } from "../../../Common/Models/AccountingSettingsRowDTO";
import { AccountDimDTO, AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { EmployeeListDTO, EmployeeListSmallDTO } from "../../../Common/Models/EmployeeListDTO";
import { EmployeeAccountDTO } from "../../../Common/Models/EmployeeUserDTO";
import { TimeScheduleTemplateChangeDTO, TimeScheduleTemplateHeadDTO, TimeScheduleTemplateHeadSmallDTO, TimeScheduleTemplatePeriodDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";
import { EmployeeScheduleDTO, EmployeeSchedulePlacementGridViewDTO } from "../../../Common/Models/EmployeeScheduleDTOs";
import { EvaluateWorkRuleResultDTO } from "../../../Common/Models/WorkRuleDTOs";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export interface IScheduleService {

    // GET
    getAbsenceRequest(employeeRequestId: number): ng.IPromise<any>
    getAbsenceRequestHistory(absenceRequestId: number): ng.IPromise<any>
    getAccountDimsForPlanning(onlyDefaultAccounts: boolean, includeAbstractAccounts: boolean, displayMode: TimeSchedulePlanningDisplayMode, filterOnHierarchyHideOnSchedule?: boolean): ng.IPromise<AccountDimSmallDTO[]>
    getAvailableEmployeeIds(dateFrom: Date, dateTo: Date, isTemplate: boolean, preliminary?: boolean): ng.IPromise<number[]>
    getAvailableEmployees(shiftIds: number[], employeeIds: number[], filterOnShiftType: boolean, filterOnAvailability: boolean, filterOnSkills: boolean, filterOnWorkRules: boolean, filterOnMessageGroupId?: number): ng.IPromise<IAvailableEmployeesDTO[]>
    getAvailableTime(employeeId: number, startTime: Date, stopTime: Date): ng.IPromise<AvailableTimeDTO>
    getDefaultEmployeeAccountDim(): ng.IPromise<AccountDimSmallDTO>
    getDefaultEmployeeAccountDimAndSelectableAccounts(employeeId: number, date: Date): ng.IPromise<AccountDimSmallDTO>
    getDefaultEmployeeAccountId(employeeId: number, date?: Date): ng.IPromise<number>
    getEmployeePostSkills(employeePostId: number): ng.IPromise<IEmployeePostSkillDTO[]>
    getEmployeePostTemplateShiftsForDay(employeePostId: number, date: Date, loadYesterdayAlso: boolean, loadTasks: boolean): ng.IPromise<ShiftDTO[]>
    getEmployeeRequestFromDateInterval(employeeId: number, start: Date, stop: Date, requestType: TermGroup_EmployeeRequestType): ng.IPromise<any>
    getEmployeesForPlanning(employeeIds: number[], categoryIds: number[], getHidden: boolean, getInactive: boolean, loadSkills: boolean, loadAvailability: boolean, loadImage: boolean, dateFrom: Date, dateTo: Date, includeSecondaryCategoriesOrAccounts: boolean, displayMode: TimeSchedulePlanningDisplayMode): ng.IPromise<EmployeeListDTO[]>
    getEmployeeSkills(employeeId: number): ng.IPromise<IEmployeeSkillDTO[]>
    getEmployeesSmallForPlanning(dateFrom: Date, dateTo: Date, includeSecondary: boolean, employedInCurrentYear: boolean, employeeIds?: number[]): ng.IPromise<EmployeeListSmallDTO[]>
    getHiddenEmployeeId(): ng.IPromise<number>;
    getVacantEmployeeIds(): ng.IPromise<number[]>;
    getPlacementForEmployee(date: Date, employeeId: number): ng.IPromise<EmployeeSchedulePlacementGridViewDTO>
    getLastPlacementForEmployee(employeeId: number, timeScheduleTemplateHeadId: number): ng.IPromise<EmployeeSchedulePlacementGridViewDTO>
    getHasAttestByEmployeeAccount(date: Date): ng.IPromise<boolean>
    getHasStaffingByEmployeeAccount(date: Date): ng.IPromise<boolean>
    getScenarioHead(timeScheduleScenarioHeadId: number, loadEmployees: boolean, loadAccounts: boolean): ng.IPromise<TimeScheduleScenarioHeadDTO>
    getShift(timeScheduleTemplateBlockId: number, includeBreaks: boolean): ng.IPromise<ShiftDTO>
    getShiftAccounting(timeScheduleTemplateBlockId: number): ng.IPromise<IShiftAccountingDTO>
    getShiftQueue(timeScheduleTemplateBlockId: number): ng.IPromise<TimeScheduleShiftQueueDTO[]>
    getShiftRequestStatus(timeScheduleTemplateBlockId: number): ng.IPromise<IShiftRequestStatusDTO>
    checkIfShiftRequestIsTooEarlyToSend(startTime: Date): ng.IPromise<IEvaluateWorkRulesActionResult>
    getShifts(employeeId: number, dateFrom: Date, dateTo: Date, employeeIds: number[], planningMode: TimeSchedulePlanningMode, displayMode: TimeSchedulePlanningDisplayMode, includeSecondaryCategories: boolean, includeBreaks: boolean, includeGrossNetAndCost: boolean, includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, includeShiftRequest: boolean, includeAbsenceRequest: boolean, checkToIncludeDeliveryAdress: boolean, timeScheduleScenarioHeadId: number, useWeekendSalary: boolean, includeLeisureCodes: boolean): ng.IPromise<ShiftDTO[]>
    getShiftsForDay(employeeId: number, date: Date, blockTypes: number[], includeBreaks: boolean, includeGrossNetAndCost: boolean, link: string, loadQueue: boolean, loadDeviationCause: boolean, loadTasks: boolean, includePreliminary: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<ShiftDTO[]>
    getShiftTypes(loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, setCategoryNames: boolean, loadHierarchyAccounts: boolean): ng.IPromise<ShiftTypeDTO[]>
    getShiftTypeAccountDim(loadAccounts: boolean): ng.IPromise<AccountDimDTO>
    getShiftTypeIdsForUser(employeeId: number, isAdmin: boolean, includeSecondaryCategories: boolean, dateFrom?: Date, dateTo?: Date, blockTypes?: TermGroup_TimeScheduleTemplateBlockType[]): ng.IPromise<number[]>
    getTemplateShiftsForDay(employeeId: number, date: Date, link: string, loadYesterdayAlso: boolean, includeGrossNetAndCost: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, loadTasks: boolean): ng.IPromise<ShiftDTO[]>
    getTimeCodeBreaks(addEmptyRow: boolean): ng.IPromise<ITimeCodeBreakSmallDTO[]>
    getTimeCodeBreaksForEmployee(employeeId: number, date: Date, addEmptyRow: boolean): ng.IPromise<ITimeCodeBreakSmallDTO[]>
    getTimeCodeBreaksForEmployeePost(employeePostId: number, addEmptyRow: boolean): ng.IPromise<ITimeCodeBreakSmallDTO[]>
    getTimeLeisureCodesSmall(): ng.IPromise<TimeLeisureCodeSmallDTO[]>
    getTimeScheduleTemplate(timeScheduleTemplateHeadId: number, loadEmployeeSchedule: boolean, loadAccounts: boolean): ng.IPromise<TimeScheduleTemplateHeadDTO>
    getTimeScheduleTemplateBlockHistory(timeScheduleTemplateBlockId: number): ng.IPromise<ShiftHistoryDTO[]>
    getTimeScheduleTemplateHeadsForEmployee(employeeId: number, dateLimitFrom: Date, dateLimitTo: Date, intersecting: boolean, excludeMultipleAccounts?: boolean, includePublicTemplates?: boolean): ng.IPromise<TimeScheduleTemplateHeadSmallDTO[]>
    getTimeScheduleTemplateHeadForEmployee(dateLimitFrom: Date, dateLimitTo: Date, timeScheduleTemplateHeadId: number): ng.IPromise<any[]>
    getOverlappingTemplates(employeeId: number, date: Date): ng.IPromise<string[]>
    getTimeScheduleTypes(getAll: boolean, onlyActive: boolean, loadFactors: boolean): ng.IPromise<TimeScheduleTypeSmallDTO[]>
    hasMultipleEmployeeAccounts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<boolean>
    matchEmployeesByShiftTypeSkills(shiftTypeId: number): ng.IPromise<number[]>
    hasTimeBreakTemplates(): ng.IPromise<boolean>
    isDayAttested(employeeId: number, date: Date): ng.IPromise<boolean>
    getTimeBlockDateId(employeeId: number, date: Date): ng.IPromise<number>
    getTimeScheduleCopyHeadsDict(type: TermGroup_TimeScheduleCopyHeadType): ng.IPromise<ISmallGenericType[]>;
    getTimeScheduleCopyRowEmployeesDict(timeScheduleCopyHeadId: number): ng.IPromise<ISmallGenericType[]>;

    // POST
    createBreaksFromTemplatesForEmployee(shifts: ShiftDTO[], employeeId: number): ng.IPromise<ShiftDTO[]>
    dragShift(action: DragShiftAction, sourceShiftId: number, targetShiftId: number, start: Date, end: Date, employeeId: number, targetLink: string, updateLinkOnTarget: boolean, timeDeviationCauseId: number, employeeChildId: number, wholeDayAbsence: boolean, skipXEMailOnChanges: boolean, copyTaskWithShift: boolean, isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, includeOnDutyShifts?: boolean, includedOnDutyShiftIds?: number[]): ng.IPromise<IActionResult>
    evaluateAbsenceRequestPlanningAgainstWorkRules(employeeId: number, shifts: ShiftDTO[], rules: SoeScheduleWorkRules[], timeScheduleScenarioHeadId?: number): ng.IPromise<IEvaluateWorkRulesActionResult>
    evaluateDragShiftAgainstWorkRules(action: DragShiftAction, sourceShiftId: number, targetShiftId: number, start: Date, end: Date, employeeId: number, isPersonalScheduleTemplate: boolean, wholeDayAbsence: boolean, rules: SoeScheduleWorkRules[], isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, fromQueue?: boolean, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date): ng.IPromise<IEvaluateWorkRulesActionResult>
    evaluateEmployeePostPlannedShiftsAgainstWorkRules(shifts: ShiftDTO[], rules: SoeScheduleWorkRules[]): ng.IPromise<IEvaluateWorkRulesActionResult>
    evaluatePlannedShiftsAgainstWorkRules(shifts: ShiftDTO[], rules: SoeScheduleWorkRules[], employeeId: number, isPersonalScheduleTemplate: boolean, timeScheduleScenarioHeadId?: number, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date): ng.IPromise<IEvaluateWorkRulesActionResult>
    evaluateSplitShiftAgainstWorkRules(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number, keepShiftsTogether: boolean, isPersonalScheduleTemplate: boolean, timeScheduleScenarioHeadId?: number, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date): ng.IPromise<IEvaluateWorkRulesActionResult>
    evaluateSplitTemplateShiftAgainstWorkRules(sourceShift: ShiftDTO, sourceTemplateHeadId: number, splitTime: Date, employeeId1: number, employeePostId1: number, templateHeadId1: number, employeeId2: number, employeePostId2: number, templateHeadId2: number, keepShiftsTogether: boolean): ng.IPromise<IEvaluateWorkRulesActionResult>
    getShiftsForAbsencePlanning(employeeId: number, shiftId: number, includeLinkedShifts: boolean, getAllshifts: boolean, timeDeviationCauseId: number, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    getAbsenceAffectedShifts(employeeId: number, dateFrom: Date, dateTo: Date, timeDeviationCauseId: number, extendedSettings: IExtendedAbsenceSettingDTO, includeAlreadyAbsence: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    getAbsenceRequestAffectedShiftsFromSelectedDays(employeeId: number, days: Date[], timeDeviationCauseId: number, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    getAbsenceRequestAffectedShifts(request: IEmployeeRequestDTO, extendedSettings: IExtendedAbsenceSettingDTO, shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    getEmployeeAvailability(employeeIds: number[]): ng.IPromise<EmployeeListDTO[]>
    getShiftsIsIncludedInAbsenceRequestWarningMessage(employeeId: number, shifts: ShiftDTO[]): ng.IPromise<any>
    getShiftTasks(shiftIds: number[]): ng.IPromise<TimeScheduleTemplateBlockTaskDTO[]>
    getTimeScheduleTemplateChanges(employeeId: number, timeScheduleTemplateHeadId: number, date: Date, dateFrom: Date, dateTo: Date, shifts: ShiftDTO[]): ng.IPromise<TimeScheduleTemplateChangeDTO[]>;
    createStringFromShifts(shifts: ShiftDTO[]): ng.IPromise<string>
    handleShift(action: HandleShiftAction, timeScheduleTemplateBlockId: number, timeDeviationCauseId: number, employeeId: number, swapTimeScheduleTemplateBlockId: number, preventAutoPermissions: boolean): ng.IPromise<IActionResult>
    performAbsencePlanningAction(employeeRequest: EmployeeRequestDTO, shifts: ShiftDTO[], scheduledAbsence: boolean, skipXEMailOnShiftChanges: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    performAbsenceRequestPlanningAction(employeeRequestId: number, shifts: ShiftDTO[], skipXEMailOnShiftChanges: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    performRestoreAbsenceRequestedShifts(employeeRequestId: number, setRequestAsPending: boolean): ng.IPromise<any>
    applyAttestCalculationFunctionEmployee(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<any>
    applyAttestCalculationFunctionEmployees(items: AttestEmployeesDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<any>
    removeAbsenceInScenario(items: AttestEmployeeDaySmallDTO[], timeScheduleScenarioHeadId: number): ng.IPromise<any>
    saveAbsenceRequest(request: IEmployeeRequestDTO, employeeId: number, requestType: TermGroup_EmployeeRequestType, skipXEMailOnShiftChanges: boolean, isForcedDefinitive: boolean): ng.IPromise<any>
    saveOrderAssignments(employeeId: number, orderId: number, shiftTypeId: number, startTime: Date, stopTime: Date, type: TermGroup_AssignmentTimeAdjustmentType, skipXEMailOnChanges: boolean): ng.IPromise<IActionResult>
    setOrderKeepAsPlanned(orderId: number, keepAsPlanned: boolean): ng.IPromise<IActionResult>
    splitShift(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number, keepShiftsTogether: boolean, isPersonalScheduleTemplate: boolean, skipXEMailOnChanges: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<IActionResult>
    splitTemplateShift(sourceShift: ShiftDTO, sourceTemplateHeadId: number, splitTime: Date, employeeId1: number, employeePostId1: number, templateHeadId1: number, employeeId2: number, employeePostId2: number, templateHeadId2: number, keepShiftsTogether: boolean, skipXEMailOnChanges: boolean): ng.IPromise<IActionResult>
    validateDeviationCausePolicy(request: IEmployeeRequestDTO, employeeId: number, requestType: TermGroup_EmployeeRequestType): ng.IPromise<any>
    unlockDay(items: any[], employeeId: number): ng.IPromise<any>

    // DELETE
    deleteEmployeeRequest(employeeRequestId: number): ng.IPromise<any>
    removeEmployeeFromShiftQueue(type: TermGroup_TimeScheduleTemplateBlockQueueType, timeScheduleTemplateBlockId: number, employeeId: number): ng.IPromise<any>
    removeRecipientFromShiftRequest(timeScheduleTemplateBlockId: number, userId: number): ng.IPromise<IActionResult>
    undoShiftRequest(timeScheduleTemplateBlockId: number): ng.IPromise<IActionResult>
}

export class ScheduleService implements IScheduleService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getAbsenceRequest(employeeRequestId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST + employeeRequestId, false).then(x => {
            if (!x)
                return null;
            else {
                let obj = new EmployeeRequestDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            }
        });
    }

    getAbsenceRequestHistory(absenceRequestId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_HISTORY + absenceRequestId, false);
    }

    getAccountDimsForPlanning(onlyDefaultAccounts: boolean, includeAbstractAccounts: boolean, displayMode: TimeSchedulePlanningDisplayMode, filterOnHierarchyHideOnSchedule?: boolean) {
        var filterOnHierarchyHideOnScheduleFlag: boolean = (filterOnHierarchyHideOnSchedule != undefined && filterOnHierarchyHideOnSchedule != null) ? filterOnHierarchyHideOnSchedule : false;

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_PLANNING_ACCOUNT_DIM + onlyDefaultAccounts + "/" + includeAbstractAccounts + "/" + displayMode + "/" + filterOnHierarchyHideOnScheduleFlag, false).then(x => {
            return x.map(y => {
                var obj = new AccountDimSmallDTO();
                angular.extend(obj, y);

                if (obj.accounts) {
                    obj.accounts = _.sortBy(obj.accounts.map(a => {
                        var aObj = new AccountDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    }), a => a.name);

                    // Copy all accounts as filtered accounts (meaning show all)
                    obj.filteredAccounts = _.sortBy(obj.accounts.map(a => {
                        var aObj = new AccountDTO();
                        angular.extend(aObj, a);
                        return aObj;
                    }), a => a.name);
                }

                obj.selectedAccounts = [];

                return obj;
            });
        });
    }

    getAvailableEmployeeIds(dateFrom: Date, dateTo: Date, isTemplate: boolean, preliminary?: boolean) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        var url = Constants.WEBAPI_TIME_SCHEDULE_AVAILABLE_EMPLOYEE_IDS + dateFromString + "/" + dateToString + "/" + isTemplate;
        if (preliminary !== undefined && preliminary !== null)
            url += "/" + preliminary;

        return this.httpService.get(url, true);
    }

    getAvailableEmployees(shiftIds: number[], employeeIds: number[], filterOnShiftType: boolean, filterOnAvailability: boolean, filterOnSkills: boolean, filterOnWorkRules: boolean, filterOnMessageGroupId?: number) {
        var model = {
            timeScheduleTemplateBlockIds: shiftIds,
            employeeIds: employeeIds,
            filterOnShiftType: filterOnShiftType,
            filterOnAvailability: filterOnAvailability,
            filterOnSkills: filterOnSkills,
            filterOnWorkRules: filterOnWorkRules,
            filterOnMessageGroupId: filterOnMessageGroupId
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_AVAILABLE_EMPLOYEES, model);
    }

    getAvailableTime(employeeId: number, startTime: Date, stopTime: Date): ng.IPromise<AvailableTimeDTO> {
        var startTimeString: string = null;
        if (startTime)
            startTimeString = startTime.toDateTimeString();
        var stopTimeString: string = null;
        if (stopTime)
            stopTimeString = stopTime.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ORDER_AVAILABLE_TIME + employeeId + "/" + startTimeString + "/" + stopTimeString, false).then(x => {
            let obj: AvailableTimeDTO = new AvailableTimeDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getDefaultEmployeeAccountDim(): ng.IPromise<AccountDimSmallDTO> {
        return this.httpService.getCache(Constants.WEBAPI_TIME_SCHEDULE_PLANNING_ACCOUNT_DIM_DEFAULT_EMPLOYEE_ACCOUNT_DIM, null, Constants.CACHE_EXPIRE_LONG).then(x => {
            var obj = new AccountDimSmallDTO();
            angular.extend(obj, x);

            if (obj.accounts) {
                obj.accounts = _.sortBy(obj.accounts.map(a => {
                    var aObj = new AccountDTO();
                    angular.extend(aObj, a);
                    return aObj;
                }), a => a.name);
            }

            return obj;
        });
    }

    getDefaultEmployeeAccountDimAndSelectableAccounts(employeeId: number, date: Date): ng.IPromise<AccountDimSmallDTO> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.getCache(Constants.WEBAPI_TIME_SCHEDULE_PLANNING_ACCOUNT_DIM_DEFAULT_EMPLOYEE_ACCOUNT_DIM_AND_SELECTABLE_ACCOUNTS + employeeId + "/" + dateString, null, Constants.CACHE_EXPIRE_LONG).then(x => {
            var obj = new AccountDimSmallDTO();
            angular.extend(obj, x);

            if (obj.accounts) {
                obj.accounts = _.sortBy(obj.accounts.map(a => {
                    var aObj = new AccountDTO();
                    angular.extend(aObj, a);
                    return aObj;
                }), a => a.name);
            }

            return obj;
        });
    }

    getDefaultEmployeeAccountId(employeeId: number, date?: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_ACCOUNT_DEFAULT + employeeId + "/" + dateString, false);
    }

    getEmployeePostSkills(employeePostId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_EMPLOYEE_POST + employeePostId, false);
    }

    getEmployeePostTemplateShiftsForDay(employeePostId: number, date: Date, loadYesterdayAlso: boolean, loadTasks: boolean) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_TEMPLATE_SHIFT + employeePostId + "/" + dateString + "/" + loadYesterdayAlso + "/" + loadTasks, false).then((x: ShiftDTO[]) => {
            return x.map(y => {
                let shift: ShiftDTO = new ShiftDTO();
                angular.extend(shift, y);
                shift.fixDates();
                return shift;
            });
        });
    }

    getEmployeeRequestFromDateInterval(employeeId: number, start: Date, stop: Date, requestType: TermGroup_EmployeeRequestType) {
        var startString: string = null;
        if (start)
            startString = start.toDateTimeString();
        var stopString: string = null;
        if (stop)
            stopString = stop.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_INTERVAL + employeeId + "/" + startString + "/" + stopString + "/" + requestType, false).then(x => {
            if (!x)
                return null;
            else {
                let obj = new EmployeeRequestDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            }
        });
    }

    getEmployeesForPlanning(employeeIds: number[], categoryIds: number[], getHidden: boolean, getInactive: boolean, loadSkills: boolean, loadAvailability: boolean, loadImage: boolean, dateFrom: Date, dateTo: Date, includeSecondaryCategoriesOrAccounts: boolean, displayMode: TimeSchedulePlanningDisplayMode): ng.IPromise<EmployeeListDTO[]> {
        var employeeIdsString: string = null;
        if (employeeIds && employeeIds.length > 0)
            employeeIdsString = employeeIds.join(',');
        var categoryIdsString: string = null;
        if (categoryIds && categoryIds.length > 0)
            categoryIdsString = categoryIds.join(',');

        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PLANNING + employeeIdsString + "/" + categoryIdsString + "/" + getHidden + "/" + getInactive + "/" + loadSkills + "/" + loadAvailability + "/" + loadImage + "/" + dateFromString + "/" + dateToString + "/" + includeSecondaryCategoriesOrAccounts + "/" + displayMode, false).then(x => {
            return x.map(e => {
                var obj = new EmployeeListDTO();
                angular.extend(obj, e);
                obj.fixDates();
                obj.setTypes();

                return obj;
            });
        });
    }

    getEmployeeSkills(employeeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_EMPLOYEE + employeeId, false);
    }

    getEmployeesSmallForPlanning(dateFrom: Date, dateTo: Date, includeSecondary: boolean, employedInCurrentYear: boolean, employeeIds?: number[]): ng.IPromise<EmployeeListSmallDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        var employeeIdsString: string = null;
        if (employeeIds && employeeIds.length > 0)
            employeeIdsString = employeeIds.join(',');

        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PLANNING + dateFromString + "/" + dateToString + "/" + includeSecondary + "/" + employedInCurrentYear + "/" + employeeIdsString, false).then(x => {
            return x.map(e => {
                var obj = new EmployeeListSmallDTO();
                angular.extend(obj, e);
                obj.employeeNrSort = _.padStart(obj.employeeNr, 50, '0');

                if (obj.accounts) {
                    obj.accounts = obj.accounts.map(a => {
                        let aObj = new EmployeeAccountDTO();
                        angular.extend(aObj, a);
                        aObj.fixDates();

                        if (a.children) {
                            aObj.children = aObj.children.map(c => {
                                let cObj = new EmployeeAccountDTO();
                                angular.extend(cObj, c);
                                cObj.fixDates();
                                return cObj;
                            });
                        }

                        return aObj;
                    });
                }

                return obj;
            });
        });
    }

    getHiddenEmployeeId() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_HIDDEN_EMPLOYEE_ID, true);
    }

    getVacantEmployeeIds() {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_VACANT_EMPLOYEE_IDS, true);
    }

    getPlacementForEmployee(date: Date, employeeId: number): ng.IPromise<EmployeeSchedulePlacementGridViewDTO> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_FOR_EMPLOYEE + dateString + "/" + employeeId, false).then(x => {
            if (x) {
                let obj = new EmployeeSchedulePlacementGridViewDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            } else {
                return null;
            }
        });
    }

    getLastPlacementForEmployee(employeeId: number, timeScheduleTemplateHeadId: number): ng.IPromise<EmployeeSchedulePlacementGridViewDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_LAST + employeeId + "/" + timeScheduleTemplateHeadId, false).then(x => {
            if (x) {
                let obj = new EmployeeSchedulePlacementGridViewDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            } else {
                return null;
            }
        });
    }

    getHasAttestByEmployeeAccount(date: Date): ng.IPromise<boolean> {
        var dateString: string = null;
        if (!date)
            date = CalendarUtility.getDateToday();

        dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE_USER_HAS_ATTEST_BY_EMPLOYEE_ACCOUNT + dateString, false);
    }

    getHasStaffingByEmployeeAccount(date: Date): ng.IPromise<boolean> {
        var dateString: string = null;
        if (!date)
            date = CalendarUtility.getDateToday();

        dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_ROLE_USER_HAS_STAFFING_BY_EMPLOYEE_ACCOUNT + dateString, false);
    }

    getScenarioHead(timeScheduleScenarioHeadId: number, loadEmployees: boolean, loadAccounts: boolean): ng.IPromise<TimeScheduleScenarioHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD + timeScheduleScenarioHeadId + "/" + loadEmployees + "/" + loadAccounts, false).then(x => {
            let obj = new TimeScheduleScenarioHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();

            if (obj.accounts) {
                obj.accounts = obj.accounts.map(a => {
                    let aObj = new TimeScheduleScenarioAccountDTO();
                    angular.extend(aObj, a);
                    return aObj;
                });
            } else {
                obj.accounts = [];
            }

            if (obj.employees) {
                obj.employees = obj.employees.map(e => {
                    let eObj = new TimeScheduleScenarioEmployeeDTO();
                    angular.extend(eObj, e);
                    return eObj;
                });
            } else {
                obj.employees = [];
            }

            return obj;
        });
    }

    getShift(timeScheduleTemplateBlockId: number, includeBreaks: boolean): ng.IPromise<ShiftDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT + timeScheduleTemplateBlockId + "/" + includeBreaks, false).then(x => {
            if (x) {
                var obj: ShiftDTO = new ShiftDTO(x.type);
                angular.extend(obj, x);
                obj.fixDates();

                if (obj.order) {
                    let order: OrderListDTO = new OrderListDTO();
                    angular.extend(order, obj.order);
                    order.fixDates();
                    order.fixColors();
                    order.fixCategories();
                    obj.order = order;
                }
                return obj;
            } else {
                return x;
            }
        });
    }

    getShiftAccounting(timeScheduleTemplateBlockId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_ACCOUNTING + timeScheduleTemplateBlockId, false);
    }

    getShiftQueue(timeScheduleTemplateBlockId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_QUEUE + timeScheduleTemplateBlockId, false).then(x => {
            return x.map(y => {
                let obj: TimeScheduleShiftQueueDTO = new TimeScheduleShiftQueueDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getShiftRequestStatus(timeScheduleTemplateBlockId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_REQUEST_STATUS + timeScheduleTemplateBlockId, false);
    }

    checkIfShiftRequestIsTooEarlyToSend(startTime: Date): ng.IPromise<IEvaluateWorkRulesActionResult> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_REQUEST_CHECK_IF_TOO_EARLY_TO_SEND + startTime.toDateTimeString(), false);
    }

    getShifts(employeeId: number, dateFrom: Date, dateTo: Date, employeeIds: number[], planningMode: TimeSchedulePlanningMode, displayMode: TimeSchedulePlanningDisplayMode, includeSecondaryCategories: boolean, includeBreaks: boolean, includeGrossNetAndCost: boolean, includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, includeShiftRequest: boolean, includeAbsenceRequest: boolean, checkToIncludeDeliveryAdress: boolean, timeScheduleScenarioHeadId: number, useWeekendSalary: boolean, includeLeisureCodes: boolean) {
        var model = {
            employeeId: employeeId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds,
            planningMode: planningMode,
            displayMode: displayMode,
            includeSecondaryCategories: includeSecondaryCategories,
            includeBreaks: includeBreaks,
            includeGrossNetAndCost: includeGrossNetAndCost,
            includePreliminary: includePreliminary,
            includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost,
            includeShiftRequest: includeShiftRequest,
            includeAbsenceRequest: includeAbsenceRequest,
            checkToIncludeDeliveryAdress: checkToIncludeDeliveryAdress,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includeHolidaySalary: useWeekendSalary,
            includeLeisureCodes: includeLeisureCodes
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_SEARCH, model).then((x: ShiftDTO[]) => {
            return x.map(y => {
                var obj: ShiftDTO = new ShiftDTO(y.type);
                angular.extend(obj, y);
                obj.fixDates();

                if (obj.order) {
                    let order: OrderListDTO = new OrderListDTO();
                    angular.extend(order, obj.order);
                    order.fixDates();
                    order.fixColors();
                    order.fixCategories();
                    obj.order = order;
                }
                return obj;
            });
        });
    }

    getShiftsForDay(employeeId: number, date: Date, blockTypes: number[], includeBreaks: boolean, includeGrossNetAndCost: boolean, link: string, loadQueue: boolean, loadDeviationCause: boolean, loadTasks: boolean, includePreliminary: boolean, timeScheduleScenarioHeadId: number = null) {
        if (!employeeId)
            return this.$q.when([]);

        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        var blockTypesString: string = null;
        if (blockTypes)
            blockTypesString = blockTypes.join();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT + employeeId + "/" + dateString + "/" + blockTypesString + "/" + includeBreaks + "/" + includeGrossNetAndCost + "/" + link + "/" + loadQueue + "/" + loadDeviationCause + "/" + loadTasks + "/" + includePreliminary + "/" + (timeScheduleScenarioHeadId || '0'), false).then((x: ShiftDTO[]) => {
            return x.map(y => {
                let shift: ShiftDTO = new ShiftDTO();
                angular.extend(shift, y);
                shift.fixDates();
                return shift;
            });
        });
    }

    getShiftTypes(loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, setCategoryNames: boolean, loadHierarchyAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE + "?loadAccounts=" + loadAccounts + "&loadSkills=" + loadSkills + "&loadEmployeeStatisticsTargets=" + loadEmployeeStatisticsTargets + "&setTimeScheduleTemplateBlockTypeName=" + setEmployeeStatisticsTargetsTypeName + "&setCategoryNames=" + setCategoryNames + "&loadHierarchyAccounts=" + loadHierarchyAccounts, true, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(y => {
                var obj = new ShiftTypeDTO();
                angular.extend(obj, y);

                var aObj = new AccountingSettingsRowDTO(0);
                angular.extend(aObj, obj.accountingSettings);
                obj.accountingSettings = aObj;

                return obj;
            });
        });
    }

    getShiftTypeAccountDim(loadAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_DIM_SHIFT_TYPE + loadAccounts, false);
    }

    getShiftTypeIdsForUser(employeeId: number, isAdmin: boolean, includeSecondaryCategories: boolean, dateFrom: Date = null, dateTo: Date = null, blockTypes: TermGroup_TimeScheduleTemplateBlockType[] = []) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.getCache(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE_GET_SHIFT_TYPE_IDS_FOR_USER + "?employeeId=" + employeeId + "&isAdmin=" + isAdmin + "&includeSecondaryCategories=" + includeSecondaryCategories + "&dateFromString=" + dateFromString + "&dateToString=" + dateToString + "&blockTypes=" + blockTypes.join(','), null, Constants.CACHE_EXPIRE_LONG);
    }

    getTemplateShiftsForDay(employeeId: number, date: Date, link: string, loadYesterdayAlso: boolean, includeGrossNetAndCost: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, loadTasks: boolean) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TEMPLATE_SHIFT + employeeId + "/" + dateString + "/" + link + "/" + loadYesterdayAlso + "/" + includeGrossNetAndCost + "/" + includeEmploymentTaxAndSupplementChargeCost + "/" + loadTasks, false).then((x: ShiftDTO[]) => {
            return x.map(y => {
                let shift: ShiftDTO = new ShiftDTO();
                angular.extend(shift, y);
                shift.fixDates();
                return shift;
            });
        });
    }

    getTimeCodeBreaks(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE_BREAK + addEmptyRow, false);
    }

    getTimeCodeBreaksForEmployee(employeeId: number, date: Date, addEmptyRow: boolean) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        if (employeeId) {
            return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_CODE_BREAK + employeeId + "/" + dateString + "/" + addEmptyRow, false);
        } else {
            var deferral = this.$q.defer<any>();
            deferral.resolve([]);
            return deferral.promise;
        }
    }

    getTimeCodeBreaksForEmployeePost(employeePostId: number, addEmptyRow: boolean) {
        if (employeePostId) {
            return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_CODE_BREAK_EMPLOYEE_POST + employeePostId + "/" + addEmptyRow, false);
        } else {
            var deferral = this.$q.defer<any>();
            deferral.resolve([]);
            return deferral.promise;
        }
    }

    getTimeLeisureCodesSmall(): ng.IPromise<TimeLeisureCodeSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_LEISURE_CODE_SMALL, false);
    }

    getTimeScheduleTemplate(timeScheduleTemplateHeadId: number, loadEmployeeSchedule: boolean, loadAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE + timeScheduleTemplateHeadId + "/" + loadEmployeeSchedule + "/" + loadAccounts, false).then(x => {
            if (x) {
                let obj = new TimeScheduleTemplateHeadDTO();
                angular.extend(obj, x);
                obj.fixDates();

                obj.timeScheduleTemplatePeriods = obj.timeScheduleTemplatePeriods.map(p => {
                    var pObj = new TimeScheduleTemplatePeriodDTO();
                    angular.extend(pObj, p);
                    pObj.fixDates();
                    return pObj;
                });

                obj.employeeSchedules = obj.employeeSchedules.map(e => {
                    var eObj = new EmployeeScheduleDTO();
                    angular.extend(eObj, e);
                    eObj.fixDates();
                    return eObj;
                });

                return obj;
            } else
                return null;
        });
    }

    getTimeScheduleTemplateBlockHistory(timeScheduleTemplateBlockId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_BLOCK_HISTORY + timeScheduleTemplateBlockId, false).then(x => {
            return x.map(y => {
                var obj = new ShiftHistoryDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeScheduleTemplateHeadsForEmployee(employeeId: number, dateLimitFrom: Date, dateLimitTo: Date, intersecting: boolean, excludeMultipleAccounts: boolean = false, includePublicTemplates: boolean = false) {
        var dateLimitFromString: string = null;
        if (dateLimitFrom)
            dateLimitFromString = dateLimitFrom.toDateTimeString();
        var dateLimitToString: string = null;
        if (dateLimitTo)
            dateLimitToString = dateLimitTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD + employeeId + "/" + dateLimitFromString + "/" + dateLimitToString + "/" + intersecting + "/" + excludeMultipleAccounts + "/" + includePublicTemplates, false).then(x => {
            return x.map(t => {
                var obj = new TimeScheduleTemplateHeadSmallDTO();
                angular.extend(obj, t);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeScheduleTemplateHeadForEmployee(dateLimitFrom: Date, dateLimitTo: Date, timeScheduleTemplateHeadId: number) {
        var dateLimitFromString: string = null;
        if (dateLimitFrom)
            dateLimitFromString = dateLimitFrom.toDateTimeString();
        var dateLimitToString: string = null;
        if (dateLimitTo)
            dateLimitToString = dateLimitTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD + dateLimitFromString + "/" + dateLimitToString + "/" + timeScheduleTemplateHeadId, false);
    }

    getOverlappingTemplates(employeeId: number, date: Date): ng.IPromise<string[]> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_OVERLAPPING + employeeId + "/" + dateString, false);
    }

    getTimeScheduleTypes(getAll: boolean, onlyActive: boolean, loadFactors: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TYPE + "?getAll=" + getAll + "&onlyActive=" + onlyActive + "&loadFactors=" + loadFactors, true, Constants.WEBAPI_ACCEPT_DTO).then(x => {
            return x.map(t => {
                var obj = new TimeScheduleTypeSmallDTO();
                angular.extend(obj, t);

                obj.factors = obj.factors.map(f => {
                    var fObj = new TimeScheduleTypeFactorSmallDTO();
                    angular.extend(fObj, f);
                    fObj.fixDates();
                    return fObj;
                });

                return obj;
            });
        });
    }

    hasMultipleEmployeeAccounts(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<boolean> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_ACCOUNT_HAS_MULTIPLE + employeeId + "/" + dateFromString + "/" + dateToString, null, Constants.CACHE_EXPIRE_LONG);
    }

    matchEmployeesByShiftTypeSkills(shiftTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_MATCH_EMPLOYEES + shiftTypeId, true);
    }

    hasTimeBreakTemplates(): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_BREAK_TEMPLATE_HAS_TEMPLATES, false);
    }

    isDayAttested(employeeId: number, date: Date): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_IS_DAY_ATTESTED + employeeId + "/" + date.toDateTimeString(), false);
    }

    getTimeBlockDateId(employeeId: number, date: Date): ng.IPromise<number> {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_BLOCK_DATE_ID + employeeId + "/" + dateString, false);
    }

    getTimeScheduleCopyHeadsDict(type: TermGroup_TimeScheduleCopyHeadType): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_COPY_HEAD_DICT + type, false);
    }

    getTimeScheduleCopyRowEmployeesDict(timeScheduleCopyHeadId: number): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_COPY_ROW_EMPLOYEE_DICT + timeScheduleCopyHeadId, false);
    }

    // POST

    createBreaksFromTemplatesForEmployee(shifts: ShiftDTO[], employeeId: number): ng.IPromise<ShiftDTO[]> {
        const model = {
            shifts: shifts,
            employeeId: employeeId,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_BREAK_TEMPLATE_CREATE_BREAKS_FOR_EMPLOYEE, model).then(x => {
            return x.map(y => {
                const obj = new ShiftDTO(y.type);
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    dragShift(action: DragShiftAction, sourceShiftId: number, targetShiftId: number, start: Date, end: Date, employeeId: number, targetLink: string, updateLinkOnTarget: boolean, timeDeviationCauseId: number, employeeChildId: number, wholeDayAbsence: boolean, skipXEMailOnChanges: boolean, copytaskWithShift: boolean, isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, includeOnDutyShifts?: boolean, includedOnDutyShiftIds?: number[]) {
        var model = {
            action: action,
            sourceShiftId: sourceShiftId,
            targetShiftId: targetShiftId,
            start: start,
            end: end,
            employeeId: employeeId,
            targetLink: targetLink,
            updateLinkOnTarget: updateLinkOnTarget,
            timeDeviationCauseId: timeDeviationCauseId,
            employeeChildId: employeeChildId,
            wholeDayAbsence: wholeDayAbsence,
            skipXEMailOnChanges: skipXEMailOnChanges,
            copytaskWithShift: copytaskWithShift,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            standbyCycleWeek: standbyCycleWeek,
            standbyCycleDateFrom: standbyCycleDateFrom,
            standbyCycleDateTo: standbyCycleDateTo,
            isStandByView: isStandByView,
            includeOnDutyShifts: includeOnDutyShifts,
            includedOnDutyShiftIds: includedOnDutyShiftIds,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_DRAG, model);
    }

    evaluateAbsenceRequestPlanningAgainstWorkRules(employeeId: number, shifts: ShiftDTO[], rules: SoeScheduleWorkRules[], timeScheduleScenarioHeadId?: number) {
        var model = {
            employeeId: employeeId,
            shifts: shifts,
            rules: rules,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_WORK_RULES, model);
    }

    evaluateDragShiftAgainstWorkRules(action: DragShiftAction, sourceShiftId: number, targetShiftId: number, start: Date, end: Date, employeeId: number, isPersonalScheduleTemplate: boolean, wholeDayAbsence: boolean, rules: SoeScheduleWorkRules[], isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, fromQueue?: boolean, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date) {
        var model = {
            action: action,
            sourceShiftId: sourceShiftId,
            targetShiftId: targetShiftId,
            start: start,
            end: end,
            employeeId: employeeId,
            isPersonalScheduleTemplate: isPersonalScheduleTemplate,
            wholeDayAbsence: wholeDayAbsence,
            rules: rules,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            standbyCycleWeek: standbyCycleWeek,
            standbyCycleDateFrom: standbyCycleDateFrom,
            standbyCycleDateTo: standbyCycleDateTo,
            isStandByView: isStandByView,
            fromQueue: fromQueue,
            planningPeriodStartDate: planningPeriodStartDate,
            planningPeriodStopDate: planningPeriodStopDate
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_DRAG, model);
    }

    evaluateEmployeePostPlannedShiftsAgainstWorkRules(shifts: ShiftDTO[], rules: SoeScheduleWorkRules[]) {
        var model = {
            shifts: shifts,
            rules: rules
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_EMPLOYEE_POST_PLANNED, model);
    }

    evaluatePlannedShiftsAgainstWorkRules(shifts: ShiftDTO[], rules: SoeScheduleWorkRules[], employeeId: number, isPersonalScheduleTemplate: boolean, timeScheduleScenarioHeadId?: number, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date) {
        _.forEach(_.filter(shifts, s => s.isOrder), shift => {
            shift.order.unfixCategories();
        });

        let model = {
            shifts: shifts,
            rules: rules,
            employeeId: employeeId,
            isPersonalScheduleTemplate: isPersonalScheduleTemplate,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            planningPeriodStartDate: planningPeriodStartDate,
            planningPeriodStopDate: planningPeriodStopDate
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_PLANNED, model).then(x => {
            _.forEach(_.filter(shifts, s => s.isOrder), shift => {
                shift.order.fixCategories();
            });
            return x;
        });
    }

    evaluateSplitShiftAgainstWorkRules(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number, keepShiftsTogether: boolean, isPersonalScheduleTemplate: boolean, timeScheduleScenarioHeadId?: number, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date) {
        if (shift.isOrder)
            shift.order.unfixCategories();

        var model = {
            shift: shift,
            splitTime: splitTime,
            employeeId1: employeeId1,
            employeeId2: employeeId2,
            keepShiftsTogether: keepShiftsTogether,
            isPersonalScheduleTemplate: isPersonalScheduleTemplate,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            planningPeriodStartDate: planningPeriodStartDate,
            planningPeriodStopDate: planningPeriodStopDate
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_SPLIT, model).then(x => {
            if (shift.isOrder)
                shift.order.fixCategories();
            return x;
        });
    }

    evaluateSplitTemplateShiftAgainstWorkRules(sourceShift: ShiftDTO, sourceTemplateHeadId: number, splitTime: Date, employeeId1: number, employeePostId1: number, templateHeadId1: number, employeeId2: number, employeePostId2: number, templateHeadId2: number, keepShiftsTogether: boolean) {
        if (sourceShift.isOrder)
            sourceShift.order.unfixCategories();

        var model = {
            sourceShift: sourceShift,
            sourceTemplateHeadId: sourceTemplateHeadId,
            splitTime: splitTime,
            employeeId1: employeeId1,
            employeePostId1: employeePostId1,
            templateHeadId1: templateHeadId1,
            employeeId2: employeeId2,
            employeePostId2: employeePostId2,
            templateHeadId2: templateHeadId2,
            keepShiftsTogether: keepShiftsTogether,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_TEMPLATE_SPLIT, model).then(x => {
            if (sourceShift.isOrder)
                sourceShift.order.fixCategories();
            return x;
        });
    }

    getShiftsForAbsencePlanning(employeeId: number, shiftId: number, includeLinkedShifts: boolean, getAllshifts: boolean, timeDeviationCauseId: number, timeScheduleScenarioHeadId?: number) {
        var model = {
            employeeId: employeeId,
            shiftId: shiftId,
            includeLinkedShifts: includeLinkedShifts,
            getAllshifts: getAllshifts,
            timeDeviationCauseId: timeDeviationCauseId,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_SHIFTS, model);
    }


    getAbsenceAffectedShifts(employeeId: number, dateFrom: Date, dateTo: Date, timeDeviationCauseId: number, extendedSettings: IExtendedAbsenceSettingDTO, includeAlreadyAbsence: boolean, timeScheduleScenarioHeadId?: number) {
        var model = {
            employeeId: employeeId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            timeDeviationCauseId: timeDeviationCauseId,
            extendedSettings: extendedSettings,
            includeAlreadyAbsence: includeAlreadyAbsence,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_AFFECTED_SHIFTS, model);
    }

    getAbsenceRequestAffectedShiftsFromSelectedDays(employeeId: number, days: Date[], timeDeviationCauseId: number, timeScheduleScenarioHeadId?: number) {
        var model = {
            employeeId: employeeId,
            selectedDays: days,
            timeDeviationCauseId: timeDeviationCauseId,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_SELECTED_DAYS, model);
    }
    getAbsenceRequestAffectedShifts(request: IEmployeeRequestDTO, extendedSettings: IExtendedAbsenceSettingDTO, shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus, timeScheduleScenarioHeadId?: number) {
        var model = {
            request: request,
            extendedSettings: extendedSettings,
            shiftUserStatus: shiftUserStatus,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_AFFECTED_SHIFTS, model);
    }

    getEmployeeAvailability(employeeIds: number[]): ng.IPromise<EmployeeListDTO[]> {
        var model = {
            numbers: employeeIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_PLANNING_EMPLOYEE_AVAILABILITY, model).then(x => {
            return x.map(y => {
                let obj = new EmployeeListDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getShiftsIsIncludedInAbsenceRequestWarningMessage(employeeId: number, shifts: ShiftDTO[]) {
        var model = {
            employeeId: employeeId,
            shifts: shifts
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_CHECK_SHIFTS_INCLUDED_IN_ABSENCEREQUEST, model);
    }

    getShiftTasks(shiftIds: number[]) {
        var model = {
            numbers: shiftIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_SHIFT_TASKS, model);
    }

    getTimeScheduleTemplateChanges(employeeId: number, timeScheduleTemplateHeadId: number, date: Date, dateFrom: Date, dateTo: Date, shifts: ShiftDTO[]): ng.IPromise<TimeScheduleTemplateChangeDTO[]> {
        var model = {
            employeeId: employeeId,
            timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
            date: date,
            dateFrom: dateFrom,
            dateTo: dateTo,
            shifts: shifts
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_CHANGES, model).then(x => {
            return x.map(t => {
                var obj = new TimeScheduleTemplateChangeDTO();
                angular.extend(obj, t);
                obj.fixDates();

                if (obj.workRulesResults) {
                    obj.workRulesResults = obj.workRulesResults.map(w => {
                        let wObj = new EvaluateWorkRuleResultDTO();
                        angular.extend(wObj, w);
                        wObj.fixDates();
                        return wObj;
                    })
                }

                return obj;
            });
        });
    }

    createStringFromShifts(shifts: ShiftDTO[]): ng.IPromise<string> {
        var model = {
            shifts: shifts
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_CREATE_STRING_FROM_SHIFTS, model);
    }

    handleShift(action: HandleShiftAction, timeScheduleTemplateBlockId: number, timeDeviationCauseId: number, employeeId: number, swapTimeScheduleTemplateBlockId: number, preventAutoPermissions: boolean) {
        var model = {
            action: action,
            timeScheduleTemplateBlockId: timeScheduleTemplateBlockId,
            timeDeviationCauseId: timeDeviationCauseId,
            employeeId: employeeId,
            swapTimeScheduleTemplateBlockId: swapTimeScheduleTemplateBlockId,
            preventAutoPermissions: preventAutoPermissions
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_HANDLE, model);
    }

    performAbsencePlanningAction(employeeRequest: EmployeeRequestDTO, shifts: ShiftDTO[], scheduledAbsence: boolean, skipXEMailOnShiftChanges: boolean, timeScheduleScenarioHeadId?: number) {
        var model = {
            employeeRequest: employeeRequest,
            shifts: shifts,
            scheduledAbsence: scheduledAbsence,
            skipXEMailOnShiftChanges: skipXEMailOnShiftChanges,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_PERFORM_PLANNING, model);
    }

    performAbsenceRequestPlanningAction(employeeRequestId: number, shifts: ShiftDTO[], skipXEMailOnShiftChanges: boolean, timeScheduleScenarioHeadId?: number) {
        var model = {
            employeeRequestId: employeeRequestId,
            shifts: shifts,
            skipXEMailOnShiftChanges: skipXEMailOnShiftChanges,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_PERFORM_PLANNING, model);
    }

    performRestoreAbsenceRequestedShifts(employeeRequestId: number, setRequestAsPending: boolean) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_HISTORY_PERFORM_RESTORE + employeeRequestId + "/" + setRequestAsPending, false);
    }

    applyAttestCalculationFunctionEmployee(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<any> {
        var model = { items: items, option: option };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_EMPLOYEE, model);
    }

    applyAttestCalculationFunctionEmployees(items: AttestEmployeesDaySmallDTO[], option: SoeTimeAttestFunctionOption): ng.IPromise<any> {
        var model = { items: items, option: option };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_CALCULATIONFUNCTION_EMPLOYEES, model);
    }

    removeAbsenceInScenario(items: AttestEmployeeDaySmallDTO[], timeScheduleScenarioHeadId: number): ng.IPromise<any> {
        var model = { items: items, timeScheduleScenarioHeadId: timeScheduleScenarioHeadId };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_ABSENCE_REMOVE, model);
    }

    saveAbsenceRequest(request: IEmployeeRequestDTO, employeeId: number, requestType: TermGroup_EmployeeRequestType, skipXEMailOnShiftChanges: boolean, isForcedDefinitive: boolean) {
        var model = {
            request: request,
            employeeId: employeeId,
            requestType: requestType,
            skipXEMailOnShiftChanges: skipXEMailOnShiftChanges,
            isForcedDefinitive: isForcedDefinitive,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST, model);
    }

    saveOrderAssignments(employeeId: number, orderId: number, shiftTypeId: number, startTime: Date, stopTime: Date, type: TermGroup_AssignmentTimeAdjustmentType, skipXEMailOnChanges: boolean): ng.IPromise<IActionResult> {
        var model = {
            employeeId: employeeId,
            orderId: orderId,
            shiftTypeId: shiftTypeId,
            startTime: startTime,
            stopTime: stopTime,
            assignmentTimeAdjustmentType: type,
            skipXEMailOnChanges: skipXEMailOnChanges
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ORDER_ASSIGNMENTS, model);
    }

    setOrderKeepAsPlanned(orderId: number, keepAsPlanned: boolean): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_SET_KEEP_AS_PLANNED + orderId + "/" + keepAsPlanned, null);
    }

    splitShift(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number, keepShiftsTogether: boolean, isPersonalScheduleTemplate: boolean, skipXEMailOnChanges: boolean, timeScheduleScenarioHeadId?: number) {
        shift.startTime = shift.actualStartTime;
        shift.stopTime = shift.actualStopTime;

        if (shift.isOrder)
            shift.order.unfixCategories();

        var model = {
            shift: shift,
            splitTime: splitTime,
            employeeId1: employeeId1,
            employeeId2: employeeId2,
            keepShiftsTogether: keepShiftsTogether,
            isPersonalScheduleTemplate: isPersonalScheduleTemplate,
            skipXEMailOnChanges: skipXEMailOnChanges,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_SPLIT, model).then(x => {
            if (shift.isOrder)
                shift.order.fixCategories();
            return x;
        });
    }

    splitTemplateShift(sourceShift: ShiftDTO, sourceTemplateHeadId: number, splitTime: Date, employeeId1: number, employeePostId1: number, templateHeadId1: number, employeeId2: number, employeePostId2: number, templateHeadId2: number, keepShiftsTogether: boolean, skipXEMailOnChanges: boolean) {
        sourceShift.startTime = sourceShift.actualStartTime;
        sourceShift.stopTime = sourceShift.actualStopTime;

        if (sourceShift.isOrder)
            sourceShift.order.unfixCategories();

        var model = {
            sourceShift: sourceShift,
            sourceTemplateHeadId: sourceTemplateHeadId,
            splitTime: splitTime,
            employeeId1: employeeId1,
            employeePostId1: employeePostId1,
            templateHeadId1: templateHeadId1,
            employeeId2: employeeId2,
            employeePostId2: employeePostId2,
            templateHeadId2: templateHeadId2,
            keepShiftsTogether: keepShiftsTogether,
            skipXEMailOnChanges: skipXEMailOnChanges
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_SPLIT_TEMPLATE, model).then(x => {
            if (sourceShift.isOrder)
                sourceShift.order.fixCategories();
            return x;
        });
    }

    validateDeviationCausePolicy(request: IEmployeeRequestDTO, employeeId: number, requestType: TermGroup_EmployeeRequestType) {
        var model = {
            request: request,
            employeeId: employeeId,
            requestType: requestType
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST_VALIDATE_DEVIATIONCAUSE_POLICY, model);
    }

    unlockDay(items: any[], employeeId: number): ng.IPromise<any> {
        var model = {
            items: items,
            employeeId: employeeId
        };
        return this.httpService.post(Constants.WEBAPI_TIME_ATTEST_UNLOCKDAY, model);
    }

    // DELETE

    deleteEmployeeRequest(employeeRequestId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST + employeeRequestId);
    }

    removeEmployeeFromShiftQueue(type: TermGroup_TimeScheduleTemplateBlockQueueType, timeScheduleTemplateBlockId: number, employeeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_QUEUE + type + "/" + timeScheduleTemplateBlockId + "/" + employeeId);
    }

    removeRecipientFromShiftRequest(timeScheduleTemplateBlockId: number, userId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_REQUEST + timeScheduleTemplateBlockId + "/" + userId);
    }

    undoShiftRequest(timeScheduleTemplateBlockId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_REQUEST + timeScheduleTemplateBlockId);
    }
}
