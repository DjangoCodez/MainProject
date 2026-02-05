import { IIncomingDeliveryHeadDTO, IShiftTypeSkillDTO, IStaffingNeedsHeadDTO, ITimeScheduleEventForPlanningDTO, ISmallGenericType, ITimeScheduleTaskDTO, IScheduleCycleRuleTypeDTO, IScheduleCycleDTO, IAnnualScheduledTimeSummary, IEvaluateWorkRulesActionResult, IActionResult, IEvaluateAllWorkRulesActionResult, ISoeProgressInfo, IEmployeeGridDTO, ISkillDTO, IReportDataSelectionDTO, IAnnualLeaveBalance } from "../../Scripts/TypeLite.Net4";
import { IHttpService } from "../../Core/Services/HttpService";
import { IncomingDeliveryHeadDTO, StaffingNeedsHeadDTO, TimeScheduleTaskDTO, TimeScheduleTaskTypeDTO, StaffingNeedsTaskDTO, StaffingStatisticsInterval, StaffingStatisticsIntervalRow, StaffingStatisticsIntervalValue, PreAnalysisInformation, TimeScheduleTaskTypeGridDTO, TimeScheduleTaskGridDTO, TimeScheduleTaskGeneratedNeedDTO, IncomingDeliveryRowDTO } from "../../Common/Models/StaffingNeedsDTOs";
import { ShiftDTO, TimeSchedulePlanningMonthDetailDTO, OrderListDTO, OrderShiftDTO, TimeScheduleScenarioHeadDTO, TimeScheduleScenarioAccountDTO, TimeScheduleScenarioEmployeeDTO, PreviewActivateScenarioDTO, ActivateScenarioDTO, CreateTemplateFromScenarioDTO, PreviewCreateTemplateFromScenarioDTO, ShiftPeriodDTO, EmployeePeriodTimeSummary, TimeScheduleEmployeePeriodDetailDTO, AutomaticAllocationResultDTO } from "../../Common/Models/TimeSchedulePlanningDTOs";
import { TimeBreakTemplateGridDTO } from "../../Common/Models/TimeBreakTemplate";
import { ScheduleCycleDTO, ScheduleCycleRuleTypeDTO } from "../../Common/Models/ScheduleCycle";
import { TimeCodeBreakGroupGridDTO } from "../../Common/Models/TimeCode";
import { TimeScheduleTemplateBlockSlim, TimeScheduleTemplateGroupDTO, TimeScheduleTemplateGroupGridDTO, TimeScheduleTemplateHeadDTO, TimeScheduleTemplateHeadSmallDTO, TimeScheduleTemplateHeadsRangeDTO, TimeScheduleTemplatePeriodSmallDTO } from "../../Common/Models/timescheduletemplatedtos";
import { DayTypeDTO } from "../../Common/Models/DayTypeDTO";
import { DayOfWeek } from "../../Util/Enumerations";
import { TimeScheduledTimeSummaryType, TermGroup_StaffingNeedsHeadStatus, StaffingNeedsHeadType, SoeStaffingNeedType, DragShiftAction, SoeScheduleWorkRules, TimeSchedulePlanningDisplayMode, TermGroup_StaffingNeedHeadsFilterType, TermGroup_TimeSchedulePlanningFollowUpCalculationType, SoeEmployeePostStatus, TermGroup_TemplateScheduleActivateFunctions, SoeRecalculateTimeHeadAction, SoeTimeScheduleEmployeePeriodDetailType, TermGroup_AnnualLeaveGroupType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { ShiftTypeDTO, ShiftTypeGridDTO } from "../../Common/Models/ShiftTypeDTO";
import { HolidaySmallDTO } from "../../Common/Models/HolidayDTO";
import { EmployeeListDTO, EmployeeListEmploymentDTO } from "../../Common/Models/EmployeeListDTO";
import { ContactAddressItemDTO } from "../../Common/Models/ContactAddressDTOs";
import { ActivateScheduleControlDTO, ActivateScheduleGridDTO } from "../../Common/Models/EmployeeScheduleDTOs";
import { EmployeePostDTO } from "../../Common/Models/EmployeePostDTO";
import { TimeScheduleTypeDTO, TimeScheduleTypeFactorDTO } from "../../Common/Models/TimeScheduleTypeDTO";
import { SmallGenericType } from "../../Common/Models/SmallGenericType";
import { RecalculateTimeHeadDTO, RecalculateTimeRecordDTO } from "../../Common/Models/RecalculateTimeDTOs";
import { Guid } from "../../Util/StringUtility";
import { EmployeeRequestDTO } from "../../Common/Models/EmployeeRequestDTO";
import { EmployeeAccountDTO, ValidatePossibleDeleteOfEmployeeAccountDTO } from "../../Common/Models/EmployeeUserDTO";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { IMessagingService } from "../../Core/Services/MessagingService";

export interface IScheduleService {

    // GET
    employeeHasSkill(employeeId: number, shiftTypeId: number, date: Date): ng.IPromise<boolean>
    employeePostHasSkill(employeePostId: number, shiftTypeId: number, date: Date): ng.IPromise<boolean>
    getAnnualLeaveShiftLength(date: Date, employeeId: number): ng.IPromise<number>
    getAbsenceRequests(employeeId: number, loadPreliminary: boolean, loadDefinitive: boolean): ng.IPromise<EmployeeRequestDTO[]>
    getAnnualScheduledTimeSummaryForEmployee(employeeId: number, dateFrom: Date, dateTo: Date, type: TimeScheduledTimeSummaryType): ng.IPromise<number>
    updateAnnualScheduledTimeSummaryForEmployee(employeeId: number, dateFrom: Date, dateTo: Date, returnResult: boolean): ng.IPromise<number>
    getAnnualWorkTime(employeeId: number, dateFrom: Date, dateTo: Date, timePeriodHeadId: number): ng.IPromise<number>
    getEmployeePosts(onlyActive: boolean, loadRelations: boolean): ng.IPromise<any>
    getEmployeePostsDict(onlyActive: boolean, addEmptyRow: boolean): ng.IPromise<any>
    getEmployeePost(employeePostId: number): ng.IPromise<EmployeePostDTO>
    getEmployeePostStatus(employeePostId: number): ng.IPromise<any>
    getEmployeeContactInfo(employeeId: number): ng.IPromise<ContactAddressItemDTO[]>
    getEmployeePostsForPlanning(employeePostIds: number[], dateFrom: Date, dateTo: Date): ng.IPromise<EmployeeListDTO[]>
    getEmployeesForDefToFromPrelShift(defToPrel: boolean, dateFrom: Date, dateTo: Date, employeeId: number, employeeIds: number[]): ng.IPromise<EmployeeListDTO[]>
    getEmploymentsForCreatingEmployeePosts(selectedDate: Date): ng.IPromise<any>
    getIncomingDeliveries(loadRows: boolean, loadAccounts: boolean, useCache: boolean): ng.IPromise<IIncomingDeliveryHeadDTO[]>
    getIncomingDeliveriesForInterval(dateFrom: Date, dateTo: Date, ids: number[], useCache: boolean): ng.IPromise<IncomingDeliveryHeadDTO[]>
    getIncomingDeliveriesGrid(useCache: boolean): ng.IPromise<IIncomingDeliveryHeadDTO[]>
    getIncomingDelivery(incomingDeliveryHeadId: number, loadRows: boolean, loadAccounts: boolean, loadExcludedDates: boolean, loadAccountHierarchyAccount: boolean): ng.IPromise<any>
    getIncomingDeliveryRows(incomingDeliveryHeadId: number, loadAccounts: boolean): ng.IPromise<any>
    getIncomingDeliveryTypes(useCache: boolean): ng.IPromise<any>
    getIncomingDeliveryType(incomingDeliveryTypeId: number): ng.IPromise<any>
    getPermittedEmployeeIds(): ng.IPromise<number[]>
    getRecalculateTimeHeads(recalculateAction: SoeRecalculateTimeHeadAction, loadRecords: boolean, showHistory: boolean, setExtensionNames: boolean, dateFrom?: Date, dateTo?: Date, limitNbrOfHeads?: number): ng.IPromise<RecalculateTimeHeadDTO[]>;
    getRecalculateTimeHead(recalculateTimeHeadId: number, loadRecords: boolean, setExtensionNames: boolean): ng.IPromise<RecalculateTimeHeadDTO>;
    getRecalculateTimeHeadId(recalculateGuid: Guid): ng.IPromise<number>;
    getScheduleTypes(getAll: boolean, onlyActive: boolean, loadFactors: boolean, loadTimeDeviationCauses: boolean): ng.IPromise<any>
    getScheduleType(scheduleTypeId: number, loadFactors: boolean): ng.IPromise<TimeScheduleTypeDTO>
    validateBreakChange(employeeId: number, timeScheduleTemplateBlockId: number, timeScheduleTemplatePeriodId: number, timeCodeBreakId: number, dateFrom: Date, breakLength: number, isTemplate: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<any>
    getShiftTypesGrid(loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, setCategoryNames: boolean, setAccountingString: boolean, setSkillNames: boolean, setTimeScheduleTypeName: boolean, useCache: boolean): ng.IPromise<ShiftTypeGridDTO[]>
    getShiftTypesDict(addEmptyRow: boolean): ng.IPromise<any>
    getShiftType(shiftTypeId: number, loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, loadCategories: boolean, loadHierarchyAccounts: boolean): ng.IPromise<any>
    getShiftTypeLinks(): ng.IPromise<any>
    getSkills(useCache: boolean): ng.IPromise<ISkillDTO[]>
    getShiftTypeSkills(shiftTypeId: number): ng.IPromise<IShiftTypeSkillDTO[]>
    getSkill(skillId: number): ng.IPromise<any>
    getSkillTypes(): ng.IPromise<any>
    getSkillTypesDict(addEmptyRow: boolean): ng.IPromise<any>
    getSkillType(skillTypeId: number): ng.IPromise<any>
    getStaffingNeedsHeadsForUser(type: StaffingNeedsHeadType, status: TermGroup_StaffingNeedsHeadStatus, loadRows: boolean, loadPeriods: boolean): ng.IPromise<IStaffingNeedsHeadDTO[]>
    getStaffingNeedsHead(staffingNeedsHeadId: number, loadRows: boolean, loadPeriods: boolean): ng.IPromise<StaffingNeedsHeadDTO>
    getStaffingNeedsLocations(): ng.IPromise<any>
    getStaffingNeedsLocation(locationId: number): ng.IPromise<any>
    getStaffingNeedsLocationGroups(): ng.IPromise<any>
    getStaffingNeedsLocationGroupsDict(addEmptyRow: boolean, includeAccountName: boolean): ng.IPromise<any>
    getStaffingNeedsLocationGroup(locationGroupId: number): ng.IPromise<any>
    getStaffingNeedsRules(): ng.IPromise<any>
    getStaffingNeedsRule(ruleId: number): ng.IPromise<any>
    getStaffingNeedsUnscheduledTaskDates(shiftTypeIds: number[], dateFrom: Date, dateTo: Date, type: SoeStaffingNeedType): ng.IPromise<Date[]>
    getStaffingNeedsUnscheduledTasks(shiftTypeIds: number[], dateFrom: Date, dateTo: Date, type: SoeStaffingNeedType): ng.IPromise<StaffingNeedsTaskDTO[]>
    getTimeScheduleTaskGeneratedNeeds(timeScheduleTaskId: number): ng.IPromise<TimeScheduleTaskGeneratedNeedDTO[]>
    getTimeBreakTemplates(): ng.IPromise<TimeBreakTemplateGridDTO[]>
    createBreaksFromTemplatesForEmployees(date: Date, employeeIds: number[], timeScheduleScenarioHeadId?: number): ng.IPromise<ShiftDTO[]>
    getScheduleCyclesDict(addEmptyRow: boolean): ng.IPromise<any>
    getScheduleCycle(scheduleCycleId: number): ng.IPromise<ScheduleCycleDTO>
    getScheduleCycles(): ng.IPromise<ScheduleCycleDTO[]>
    getScheduleCycleRuleTypesDict(): ng.IPromise<any>
    getScheduleCycleRuleType(scheduleCycleRuleTypeId: number): ng.IPromise<ScheduleCycleRuleTypeDTO>
    getScheduleCycleRuleTypes(): ng.IPromise<ScheduleCycleRuleTypeDTO[]>
    getTimeCodeBreakGroups(): ng.IPromise<TimeCodeBreakGroupGridDTO[]>
    getTimeScheduleEmployeePeriodId(employeeId: number, date: Date): ng.IPromise<number>
    getTimeScheduleEvents(useCache: boolean): ng.IPromise<any>
    getTimeScheduleEventDatesForPlanning(dateFrom: Date, dateTo: Date): ng.IPromise<Date[]>
    getTimeScheduleEventsForPlanning(date: Date): ng.IPromise<ITimeScheduleEventForPlanningDTO[]>
    getTimeScheduleEventsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>
    getTimeScheduleEvent(timeScheduleEventId: number): ng.IPromise<any>
    getTimeScheduleTasks(useCache: boolean): ng.IPromise<ITimeScheduleTaskDTO[]>
    getTimeScheduleTasksDict(): ng.IPromise<any>
    getTimeScheduleTasksForFrequency(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getTimeScheduleTasksForInterval(dateFrom: Date, dateTo: Date, ids: number[], useCache: boolean): ng.IPromise<TimeScheduleTaskDTO[]>
    getTimeScheduleTasksGrid(useCache: boolean): ng.IPromise<TimeScheduleTaskGridDTO[]>
    getTimeScheduleTask(timeScheduleTaskId: number, loadAccounts: boolean, loadExcludedDates: boolean, loadAccountHierarchyAccount: boolean): ng.IPromise<TimeScheduleTaskDTO>
    getTimeScheduleTaskTypesDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>
    getTimeScheduleTaskTypesGrid(useCache: boolean): ng.IPromise<TimeScheduleTaskTypeGridDTO[]>
    getTimeScheduleTaskType(timeScheduleTaskTypeId: number): ng.IPromise<TimeScheduleTaskTypeDTO>
    getTimeScheduleTemplateGroups(useCache: boolean): ng.IPromise<TimeScheduleTemplateGroupDTO[]>
    getTimeScheduleTemplateGroupsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]>
    getTimeScheduleTemplateGroupsGrid(useCache: boolean): ng.IPromise<TimeScheduleTemplateGroupGridDTO[]>
    getTimeScheduleTemplateGroup(timeScheduleTemplateGroupId: number, loadRows: boolean, loadEmployees: boolean, setNextStartDateOnRows: boolean, setEmployeeInfo: boolean): ng.IPromise<TimeScheduleTemplateGroupDTO>
    getTimeScheduleTemplateGroupRowNextStartDate(startDate: Date, stopDate: Date, recurrencePattern: string): ng.IPromise<Date>
    getTimeScheduleTemplateHead(timeScheduleTemplateHeadId: number): ng.IPromise<TimeScheduleTemplateHeadDTO>
    getTimeScheduleTemplateHeadSmall(timeScheduleTemplateHeadId: number): ng.IPromise<TimeScheduleTemplateHeadSmallDTO>
    getTimeScheduleTemplateHeads(): ng.IPromise<TimeScheduleTemplateHeadDTO[]>
    getTimeScheduleTemplateHeadsForActivate(): ng.IPromise<TimeScheduleTemplateHeadSmallDTO[]>
    getTimeScheduleTemplateHeadsRange(timeScheduleTemplateGroupId: number, visibleDateFrom: Date, visibleDateTo: Date): ng.IPromise<TimeScheduleTemplateHeadsRangeDTO>
    getTimeScheduleTemplateHeadsRangeForEmployee(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TimeScheduleTemplateHeadsRangeDTO>
    getTimeScheduleTemplatePeriodsForActivate(timeScheduleTemplateHeadId: number): ng.IPromise<TimeScheduleTemplatePeriodSmallDTO[]>
    getTimeScheduleTypesDict(getAll: boolean, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getUnscheduledOrder(orderId: number): ng.IPromise<OrderListDTO>;
    getOrderShifts(orderId: number): ng.IPromise<OrderShiftDTO[]>;
    getDayTypesAndWeekdays(): ng.IPromise<any>
    getDayType(dayTypeId: number): ng.IPromise<any>
    getDayTypes(): ng.IPromise<DayTypeDTO[]>
    getDayTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>
    getDaysOfWeekDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]>
    getSysHolidayTypes(): ng.IPromise<any>
    getHolidays(): ng.IPromise<any>
    getHolidaysSmall(dateFrom: Date, dateTo: Date): ng.IPromise<HolidaySmallDTO[]>
    getHoliday(holiDayId: number): ng.IPromise<any>
    getHalfDayTypesDict(addEmptyRow: boolean): ng.IPromise<any>
    getHalfDays(): ng.IPromise<any>
    getRecurrenceDescription(pattern: string): ng.IPromise<any>
    hasEmployeeSchedule(employeeId: number, date: Date): ng.IPromise<boolean>
    getWorkRuleBypassLog(allItemsSelection: number): ng.IPromise<any>
    getScenarioEmployeeIds(timeScheduleScenarioHeadId: number): ng.IPromise<number[]>
    previewActivateScenario(timeScheduleScenarioHeadId: number, preliminaryDateFrom?: Date): ng.IPromise<PreviewActivateScenarioDTO[]>
    getActivateScenarioEmployeeStatus(timeScheduleScenarioHeadId: number): ng.IPromise<PreviewActivateScenarioDTO[]>
    previewCreateTemplateFromScenario(timeScheduleScenarioHeadId: number, dateFrom: Date, weekInCycle: number, dateTo?: Date): ng.IPromise<PreviewCreateTemplateFromScenarioDTO[]>
    hasEmployeeTemplatesOfTypeSubstituteShifts(): ng.IPromise<boolean>
    getEmployeeTemplatesOfTypeSubstituteShifts(): ng.IPromise<SmallGenericType[]>

    // POST
    activateScenario(model: ActivateScenarioDTO): ng.IPromise<any>
    createTemplateFromScenario(model: CreateTemplateFromScenarioDTO): ng.IPromise<any>
    assignTimeScheduleTemplateToEmployee(timeScheduleTemplateHeadId: number, employeeId: number, startDate: Date): ng.IPromise<any>
    removeEmployeeFromTimeScheduleTemplate(timeScheduleTemplateHeadId: number): ng.IPromise<any>
    saveEmployeePost(employeePost: any): ng.IPromise<any>
    saveScheduleCycleRuleType(scheduleCycleRuleType: IScheduleCycleRuleTypeDTO): ng.IPromise<any>
    saveScheduleCycle(scheduleCycle: IScheduleCycleDTO): ng.IPromise<any>
    getAnnualScheduledTimeSummary(employeeIds: number[], dateFrom: Date, dateTo: Date, planningPeriodHeadId?: number): ng.IPromise<IAnnualScheduledTimeSummary[]>
    getEmployeePeriodTimeSummary(employeeIds: number[], dateFrom: Date, dateTo: Date, planningPeriodHeadId?: number): ng.IPromise<EmployeePeriodTimeSummary[]>
    saveIncomingDelivery(incomingDeliveryHead: IncomingDeliveryHeadDTO): ng.IPromise<any>
    saveIncomingDeliveryType(incomingDeliveryType: any): ng.IPromise<any>
    dragShifts(action: DragShiftAction, sourceShiftIds: number[], offsetDays: number, targetEmployeeId: number, skipXEMailOnChanges: boolean, copyTaskWithShift: boolean, isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, includeOnDutyShifts?: boolean, includedOnDutyShiftIds?: number[]): ng.IPromise<IActionResult>
    saveEvaluateAllWorkRulesByPass(result: IEvaluateWorkRulesActionResult, employeeId: number): ng.IPromise<any>
    evaluateAllWorkRules(shifts: ShiftDTO[], employeeIds: number[], start: Date, end: Date, isPersonalScheduleTemplate: boolean, rules: SoeScheduleWorkRules[], timeScheduleScenarioHeadId?: number, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date): ng.IPromise<IEvaluateAllWorkRulesActionResult>
    evaluateDragShiftsAgainstWorkRules(action: DragShiftAction, sourceShiftIds: number[], offsetDays: number, employeeId: number, isPersonalScheduleTemplate: boolean, rules: SoeScheduleWorkRules[], isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date): ng.IPromise<IEvaluateWorkRulesActionResult>
    saveScheduleType(scheduleType: any): ng.IPromise<any>
    updateScheduleTypesState(accounts: any): ng.IPromise<any>
    updateEmployeePostsState(posts: any): ng.IPromise<any>
    saveShiftType(shiftType: any): ng.IPromise<any>
    saveDayType(dayType: any): ng.IPromise<any>
    saveShiftTypeLinks(shiftTypeLinks: any): ng.IPromise<any>
    getUnscheduledOrders(categoryIds: number[], dateTo?: Date): ng.IPromise<OrderListDTO[]>;
    getUnscheduledOrdersByIds(orderIds: number[]): ng.IPromise<OrderListDTO[]>;
    getTemplateShifts(dateFrom: Date, dateTo: Date, loadYesterdayAlso: boolean, employeeIds: number[], includeGrossNetAndCost: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, loadTasks: boolean, useWeekendSalary: boolean): ng.IPromise<any>
    dragTemplateShift(action: DragShiftAction, sourceShiftId: number, sourceTemplateHeadId: number, sourceDate: Date, targetShiftId: number, targetTemplateHeadId: number, start: Date, end: Date, employeeId: number, employeePostId: number, targetLink: string, updateLinkOnTarget: boolean, copyTaskWithShift: boolean): ng.IPromise<IActionResult>
    dragTemplateShifts(action: DragShiftAction, sourceShiftIds: number[], sourceTemplateHeadId: number, firstSourceDate: Date, offsetDays: number, firstTargetDate: Date, targetEmployeeId: number, targetEmployeePostId: number, targetTemplateHeadId: number, copyTaskWithShift: boolean): ng.IPromise<IActionResult>
    getEmployeePostTemplateShifts(dateFrom: Date, dateTo: Date, employeePostIds: number[], loadTasks: boolean): ng.IPromise<any>
    getScenarioHeadsDict(validAccountIds: number[], addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>
    getScenarioHead(timeScheduleScenarioHeadId: number, loadEmployees: boolean, loadAccounts: boolean): ng.IPromise<TimeScheduleScenarioHeadDTO>
    evaluateDragTemplateShiftAgainstWorkRules(action: DragShiftAction, sourceShiftId: number, sourceTemplateHeadId: number, sourceDate: Date, targetShiftId: number, targetTemplateHeadId: number, start: Date, end: Date, employeeId: number, employeePostId: number, rules: SoeScheduleWorkRules[]): ng.IPromise<IEvaluateWorkRulesActionResult>
    evaluateDragTemplateShiftsAgainstWorkRules(action: DragShiftAction, sourceShiftIds: number[], sourceTemplateHeadId: number, firstSourceDate: Date, offsetDays: number, employeeId: number, employeePostId: number, targetTemplateHeadId: number, firstTargetDate: Date, rules: SoeScheduleWorkRules[]): ng.IPromise<IEvaluateWorkRulesActionResult>
    saveShifts(source: string, shifts: ShiftDTO[], updateBreaks: boolean, skipXEMailOnChanges: boolean, adjustTasks: boolean, minutesMoved: number, timeScheduleScenarioHeadId?: number): ng.IPromise<IActionResult>
    getShiftsGrossNetAndCost(employeeId: number, dateFrom: Date, dateTo: Date, employeeIds: number[], includeSecondaryCategories: boolean, includeBreaks: boolean, includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, timeScheduleScenarioHeadId?: number, useWeekendSalary?: boolean): ng.IPromise<any>
    getTemplateShiftsGrossNetAndCost(dateFrom: Date, dateTo: Date, employeeIds: number[], includeEmploymentTaxAndSupplementChargeCost: boolean, useWeekendSalary: boolean): ng.IPromise<any>
    getShiftPeriods(dateFrom: Date, dateTo: Date, employeeId: number, displayMode: TimeSchedulePlanningDisplayMode, blockTypes: number[], employeeIds: number[], shiftTypeIds: number[], deviationCauseIds: number[], includeGrossNetAndCost: boolean, includeToolTip: boolean, includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, preliminary?: boolean, timeScheduleScenarioHeadId?: number, useWeekendSalary?: boolean): ng.IPromise<any>
    getShiftPeriodsGrossNetAndCost(dateFrom: Date, dateTo: Date, employeeId: number, blockTypes: number[], employeeIds: number[], shiftTypeIds: number[], deviationCauseIds: number[], includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, preliminary?: boolean, timeScheduleScenarioHeadId?: number, useWeekendSalary?: boolean): ng.IPromise<any>
    getShiftPeriodDetails(date: Date, employeeId: number, blockTypes: number[], employeeIds: number[], shiftTypeIds: number[], deviationCauseIds: number[], includePreliminary: boolean, preliminary?: boolean, timeScheduleScenarioHeadId?: number): ng.IPromise<TimeSchedulePlanningMonthDetailDTO>
    getCyclePlannedMinutes(date: Date, employeeIds: number[]): ng.IPromise<any>
    saveSkill(skill: any): ng.IPromise<any>
    saveSkillType(skillType: any): ng.IPromise<any>
    updateSkillTypesState(accounts: any): ng.IPromise<any>
    saveHoliday(holiday: any): ng.IPromise<any>
    saveStaffingNeedsHead(head: StaffingNeedsHeadDTO): ng.IPromise<any>
    saveStaffingNeedsLocation(location: any): ng.IPromise<any>
    saveStaffingNeedsLocationGroup(locationGroup: any, shiftTypeIds: number[]): ng.IPromise<any>
    saveStaffingNeedsRule(rule: any): ng.IPromise<any>
    saveDefToFromPrelShift(prelToDef: boolean, dateFrom: Date, dateTo: Date, employeeIds: number[], includeScheduleShifts: boolean, includeStandbyShifts: boolean): ng.IPromise<any>
    saveTimeBreakTemplates(breakTemplates: TimeBreakTemplateGridDTO[]): ng.IPromise<IActionResult>
    validateTimeBreakTemplates(breakTemplates: TimeBreakTemplateGridDTO[]): ng.IPromise<TimeBreakTemplateGridDTO[]>
    saveTimeScheduleScenarioHead(scenarioHead: TimeScheduleScenarioHeadDTO, timeScheduleScenarioHeadId: number, includeAbsence: boolean, dateFunction: boolean): ng.IPromise<IActionResult>
    saveTimeScheduleTask(timeScheduleTask: TimeScheduleTaskDTO): ng.IPromise<any>
    saveTimeScheduleTaskType(timeScheduleTaskType: TimeScheduleTaskTypeDTO): ng.IPromise<any>
    saveTimeScheduleTemplate(templateHead: TimeScheduleTemplateHeadDTO, blocks: TimeScheduleTemplateBlockSlim[]): ng.IPromise<IActionResult>;
    saveTimeScheduleTemplateGroup(timeScheduleTemplateGroup: TimeScheduleTemplateGroupDTO): ng.IPromise<IActionResult>
    saveTimeScheduleTemplateHead(employeeId: number, shifts: ShiftDTO[], timeScheduleTemplateHeadId: number, dayNumberFrom: number, dayNumberTo, currentDate: Date, activateDayNumber: number, activateDates: Date[], skipXEMailOnChanges: boolean): ng.IPromise<any>
    saveTimeScheduleTemplateAndPlacement(saveTemplate: boolean, savePlacement: boolean, control: ActivateScheduleControlDTO, shifts: ShiftDTO[], timeScheduleTemplateHeadId: number, templateNoOfDays: number, templateStartDate, templateStopDate: Date, firstMondayOfCycle: Date, placementDateFrom: Date, placementDateTo: Date, currentDate: Date, simpleSchedule: boolean, startOnFirstDayOfWeek: boolean, preliminary: boolean, locked: boolean, employeeId: number, copyFromTimeScheduleTemplateHeadId?: number, useAccountingFromSourceSchedule?: boolean): ng.IPromise<any>
    updateTimeScheduleTemplateHeadsState(dict: any): ng.IPromise<any>;
    saveTimeScheduleEvent(timeScheduleEvent: any): ng.IPromise<any>
    getEmployeesWithSubstituteShifts(employeeIds: number[], dates: Date[]): ng.IPromise<any>
    getTimeEmploymentContractShortSubstituteUrl(employeeIds: number[], dates: Date[], printedFromSchedulePlanning: boolean): ng.IPromise<string>
    sendTimeEmploymentContractShortSubstituteForConfirmation(employeeIds: number[], dates: Date[], savePrintout: boolean): ng.IPromise<any>
    printEmploymentContractFromTemplate(employeeId: number, employeeTemplateId: number, substituteDates: Date[]): ng.IPromise<IActionResult>;
    getStaffingNeedsHeadfromIncomingDeliveryHead(interval: number, incomingDeliveryHeadDTOs: IncomingDeliveryHeadDTO[], name: string, date: Date, dayTypeId: number, dayOfWeek: number): ng.IPromise<StaffingNeedsHeadDTO>
    getStaffingNeedsHeadfromTimeScheduleTask(interval: number, incomingDeliveryHeadDTOs: TimeScheduleTaskDTO[], name: string, date: Date, dayTypeId: number, dayOfWeek: number): ng.IPromise<StaffingNeedsHeadDTO>
    createStaffingNeedsHeadsFromTasks(interval: number, name: string, date: Date, dayTypeId: number, dayOfWeek: number, wholeWeek: boolean, includeStaffingNeedsChartData: boolean, intervalDateFrom: Date, intervalDateTo: Date, dayOfWeeks: DayOfWeek[], adjustPercent: number, currentDate: Date, timeScheduleTaskId: number): ng.IPromise<any>
    generateStaffingNeedsHeads(needFilterType: TermGroup_StaffingNeedHeadsFilterType, dayTypeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<StaffingNeedsHeadDTO[]>
    generateStaffingNeedsHeadsForInterval(needFilterType: TermGroup_StaffingNeedHeadsFilterType, dateFrom: Date, dateTo: Date, calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType, calculateNeed: boolean, calculateNeedFrequency: boolean, calculateNeedRowFrequency: boolean, calculateBudget: boolean, calculateForecast: boolean, calculateTemplateSchedule: boolean, calculateSchedule: boolean, calculateTime: boolean, calculateTemplateScheduleForEmployeePost: boolean, accountDimId: number, accountId: number, employeeIds: number[], employeePostIds: number[], timeScheduleScenarioHeadId: number, includeEmpTaxAndSuppCharge: boolean, shiftTypeIds: number[], forceWeekView: boolean): ng.IPromise<StaffingStatisticsInterval[]>
    recalculateStaffingNeedsSummary(row: StaffingStatisticsIntervalRow): ng.IPromise<StaffingStatisticsIntervalRow>
    createShiftsFromStaffingNeeds(needFilterType: TermGroup_StaffingNeedHeadsFilterType, dayTypeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<StaffingNeedsHeadDTO>
    createEmptyScheduleForEmployeePost(employeePostId: number, fromDate: Date): ng.IPromise<any>
    createEmptyScheduleForEmployeePosts(employeePostIds: number[], fromDate: Date): ng.IPromise<any>
    createScheduleFromEmployeePost(employeePostId: number, fromDate: Date): ng.IPromise<any>
    createScheduleFromEmployeePosts(employeePostIds: number[], fromDate: Date): ng.IPromise<any>
    createScheduleFromEmployeePostsAsync(employeePostIds: number[], fromDate: Date): ng.IPromise<ISoeProgressInfo>
    getPreAnalysisInformation(employeePostId: number, fromDate: Date): ng.IPromise<PreAnalysisInformation>
    assignTaskToEmployee(employeeId: number, date: Date, taskDTOs: StaffingNeedsTaskDTO[], skipXEMailOnShiftChanges: boolean): ng.IPromise<any>
    evaluateAssignTaskToEmployeeAgainstWorkRules(destinationEmployeeId: number, destinationDate: Date, taskDTOs: StaffingNeedsTaskDTO[], rules: SoeScheduleWorkRules[]): ng.IPromise<IEvaluateWorkRulesActionResult>
    assignTemplateShiftTask(tasks: StaffingNeedsTaskDTO[], date: Date, timeScheduleTemplateHeadId: number): ng.IPromise<ShiftDTO[]>
    createEmployeePostsFromEmployments(ids: number[], dateFrom: Date): ng.IPromise<IActionResult>
    changeStatusForEmployeePost(employeePostId: number, status: SoeEmployeePostStatus): ng.IPromise<any>
    copySchedule(sourceEmployeeId: number, sourceDateEnd: Date, targetEmployeeId: number, targetDateStart: Date, targetDateEnd: Date, useAccountingFromSourceSchedule: boolean): ng.IPromise<IActionResult>
    getPlacementsForGrid(onlyLatest: boolean, addEmptyPlacement: boolean, employeeIds: number[], dateFrom?: Date, dateTo?: Date): ng.IPromise<ActivateScheduleGridDTO[]>
    isPlacementsUnchanged(items: ActivateScheduleGridDTO[], placementStopDate: Date): ng.IPromise<IActionResult>
    controlActivation(employeeId: number, employeeScheduleStartDate: Date, employeeScheduleStopDate: Date, startDate?: Date, stopDate?: Date, isDelete?: boolean): ng.IPromise<ActivateScheduleControlDTO>
    controlActivations(items: ActivateScheduleGridDTO[], startDate?: Date, stopDate?: Date, isDelete?: boolean): ng.IPromise<ActivateScheduleControlDTO>;
    activateSchedule(control: ActivateScheduleControlDTO, items: ActivateScheduleGridDTO[], func: TermGroup_TemplateScheduleActivateFunctions, timeScheduleTemplateHeadId: number, timeScheduleTemplatePeriodId: number, startDate: Date, stopDate: Date, preliminary: boolean): ng.IPromise<IActionResult>
    deleteEmployeeSchedule(control: ActivateScheduleControlDTO, item: ActivateScheduleGridDTO): ng.IPromise<IActionResult>
    getEmployeeAccounts(employeeIds: number[], dateFrom: Date, dateTo: Date): ng.IPromise<EmployeeAccountDTO[]>;
    getOngoingTimeScheduleTemplateHeads(dict: any): ng.IPromise<TimeScheduleTemplateHeadSmallDTO[]>;
    setStopDateOnTimeScheduleTemplateHeads(dict: any): ng.IPromise<IActionResult>;
    exportShiftsToExcel(shifts: ShiftDTO[], employees: EmployeeListDTO[], dates: Date[], selections: IReportDataSelectionDTO[]): ng.IPromise<IActionResult>;
    validatePossibleDeleteOfEmployeeAccount(model: ValidatePossibleDeleteOfEmployeeAccountDTO): ng.IPromise<IActionResult>;
    saveTimeScheduleEmployeePeriodDetail(shift: ShiftDTO): ng.IPromise<IActionResult>;
    deleteTimeScheduleEmployeePeriodDetail(timeScheduleEmployeePeriodDetailIds: number[]): ng.IPromise<IActionResult>;
    setRecalculateTimeHeadToProcessed(recalculateTimeHeadId: number): ng.IPromise<IActionResult>;
    allocateLeisureDays(startDate: Date, stopDate: Date, employeeIds: number[]): ng.IPromise<AutomaticAllocationResultDTO>;
    deleteLeisureDays(startDate: Date, stopDate: Date, employeeIds: number[]): ng.IPromise<IActionResult>;
    createAnnualLeaveShift(date: Date, employeeId: number): ng.IPromise<IActionResult>;
    getAnnualLeaveBalance(date: Date, employeeIds: number[]): ng.IPromise<IAnnualLeaveBalance[]>;
    recalculateAnnualLeaveBalance(date: Date, employeeIds: number[], previousYear: boolean): ng.IPromise<IAnnualLeaveBalance[]>;

    // DELETE
    cancelRecalculateTimeHead(recalculateTimeHeadId: number): ng.IPromise<IActionResult>;
    cancelRecalculateTimeRecord(recalculateTimeRecordId: number): ng.IPromise<IActionResult>;
    deleteEmployeePost(employeePostId: number): ng.IPromise<IActionResult>
    deleteEmployeePosts(employeePostIds: number[]): ng.IPromise<IActionResult>
    deleteHoliday(holidayId: number): ng.IPromise<any>
    deleteGeneratedNeeds(staffingNeedRowPeriodIds: number[]): ng.IPromise<IActionResult>
    deleteIncomingDelivery(incomingDeliveryHeadId: number): ng.IPromise<any>
    deleteIncomingDeliveryType(incomingDeliveryTypeId: number): ng.IPromise<any>
    deleteScenarioHead(timeScheduleScenarioHeadId: number): ng.IPromise<IActionResult>
    deleteScheduleCycleRuleType(scheduleCycleRuleTypeId: number): ng.IPromise<any>
    deleteScheduleCycle(scheduleCycleId: number): ng.IPromise<any>
    deleteScheduleFromEmployeePost(employeePostId: number): ng.IPromise<any>
    deleteScheduleFromEmployeePosts(employeePostIds: number[]): ng.IPromise<any>
    deleteScheduleType(scheduleTypeId: number): ng.IPromise<any>
    deleteShifts(shiftIds: number[], skipXEMailOnChanges: boolean, timeScheduleScenarioHeadId?: number, includedOnDutyShiftIds?: number[]): ng.IPromise<any>
    deleteShiftType(shiftTypeId: number): ng.IPromise<IActionResult>
    deleteShiftTypes(shiftTypeIds: number[]): ng.IPromise<IActionResult>
    deleteSkill(skillId: number): ng.IPromise<any>
    deleteDayType(dayTypeId: number): ng.IPromise<any>
    deleteSkillType(skillTypeId: number): ng.IPromise<any>
    deleteStaffingNeedsHead(staffingNeedsHeadId: number): ng.IPromise<any>
    deleteStaffingNeedsLocation(locationId: number): ng.IPromise<any>
    deleteStaffingNeedsLocationGroup(locationGroupId: number): ng.IPromise<any>
    deleteStaffingNeedsRule(ruleId: number): ng.IPromise<any>
    deleteTimeScheduleEvent(timeScheduleEventId: number): ng.IPromise<any>
    deleteTimeScheduleScenarioHead(timeScheduleScenarioHeadId: number): ng.IPromise<IActionResult>
    deleteTimeScheduleTask(timeScheduleTaskId: number): ng.IPromise<any>
    deleteTimeScheduleTaskType(timeScheduleTaskTypeId: number): ng.IPromise<any>
    deleteTimeScheduleTemplateGroup(timeScheduleTemplateGroupId: number): ng.IPromise<IActionResult>
    deleteTimeScheduleTemplateHead(timeScheduleTemplateHeadId: number): ng.IPromise<any>
    deleteAnnualLeaveShift(timeScheduleTemplateBlockId: number): ng.IPromise<IActionResult>;
}

export class ScheduleService implements IScheduleService {

    //@ngInject
    constructor(private httpService: IHttpService, private messagingService: IMessagingService, private $q: ng.IQService) { }

    // GET

    employeeHasSkill(employeeId: number, shiftTypeId: number, date: Date) {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_EMPLOYEE + employeeId + "/" + shiftTypeId + "/" + dateString, false);
    }

    employeePostHasSkill(employeePostId: number, shiftTypeId: number, date: Date) {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_EMPLOYEE_POST + employeePostId + "/" + shiftTypeId + "/" + dateString, false);
    }

    getAnnualLeaveShiftLength(date: Date, employeeId: number): ng.IPromise<number> {
        let dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_LEAVE_SHIFT_LENGTH + dateString + "/" + employeeId, false);
    }

    getAbsenceRequests(employeeId: number, loadPreliminary: boolean, loadDefinitive: boolean): ng.IPromise<EmployeeRequestDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ABSENCE_REQUEST + employeeId + "/" + loadPreliminary + "/" + loadDefinitive, false).then(x => {
            return x.map(y => {
                let obj = new EmployeeRequestDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getAnnualScheduledTimeSummaryForEmployee(employeeId: number, dateFrom: Date, dateTo: Date, type: TimeScheduledTimeSummaryType) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_SCHEDULED_TIME + employeeId + "/" + dateFromString + "/" + dateToString + "/" + type, false);
    }

    updateAnnualScheduledTimeSummaryForEmployee(employeeId: number, dateFrom: Date, dateTo: Date, returnResult: boolean) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_SCHEDULED_TIME_UPDATE + employeeId + "/" + dateFromString + "/" + dateToString + "/" + returnResult, false);
    }

    getAnnualWorkTime(employeeId: number, dateFrom: Date, dateTo: Date, timePeriodHeadId: number) {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_WORK_TIME + employeeId + "/" + dateFromString + "/" + dateToString + "/" + timePeriodHeadId, false);
    }

    getEmployeePosts(active: boolean, loadRelations: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST + "?active=" + active + "&loadRelations=" + loadRelations, false);
    }

    getEmployeePostsDict(active: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST + "?active=" + active + "&addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getEmployeePost(employeePostId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST + employeePostId, false).then(x => {
            let obj = new EmployeePostDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.isLocked = (obj.status === SoeEmployeePostStatus.Locked);
            return obj;
        });
    }

    getEmployeePostStatus(employeePostId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_STATUS + employeePostId, false);
    }

    getEmploymentsForCreatingEmployeePosts(selectedDate: Date) {
        var dateString: string = null;
        if (selectedDate)
            dateString = selectedDate.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_EMPLOYMENTS + dateString, false);
    }

    getEmployeeContactInfo(employeeId: number): ng.IPromise<ContactAddressItemDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_CONTACT_INFO + employeeId, false).then(x => {
            return x.map(y => {
                let obj = new ContactAddressItemDTO();
                angular.extend(obj, y);
                return obj;
            })
        });
    }

    getEmployeePostsForPlanning(employeePostIds: number[], dateFrom: Date, dateTo: Date) {
        var employeePostIdsString: string = null;
        if (employeePostIds && employeePostIds.length > 0)
            employeePostIdsString = employeePostIds.join(',');

        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_PLANNING_EMPLOYEE_POSTS + employeePostIdsString + "/" + dateFromString + "/" + dateToString, false).then(x => {
            return x.map(y => {
                var obj = new EmployeeListDTO();
                angular.extend(obj, y);
                obj.employeeNrSort = _.padStart(obj.employeeNr, 50, '0');
                obj.fixDates();
                obj.isVisible = true;

                if (obj.accounts) {
                    obj.accounts = obj.accounts.map(e => {
                        let eObj = new EmployeeAccountDTO();
                        angular.extend(eObj, e);
                        eObj.fixDates();
                        return eObj;
                    });
                } else {
                    obj.accounts = [];
                }

                if (obj.employments) {
                    obj.employments = obj.employments.map(e => {
                        let eObj = new EmployeeListEmploymentDTO();
                        angular.extend(eObj, e);
                        eObj.fixDates();
                        return eObj;
                    });
                } else {
                    obj.employments = [];
                }

                if (obj.templateSchedules) {
                    obj.templateSchedules = obj.templateSchedules.map(t => {
                        let tObj = new TimeScheduleTemplateHeadSmallDTO;
                        angular.extend(tObj, t);
                        tObj.fixDates();
                        return tObj;
                    });
                } else {
                    obj.templateSchedules = [];
                }
                return obj;
            });
        });
    }

    getEmployeesForDefToFromPrelShift(prelToDef: boolean, dateFrom: Date, dateTo: Date, employeeId: number, employeeIds: number[]): ng.IPromise<EmployeeListDTO[]> {
        const model = {
            prelToDef: prelToDef,
            employeeId: employeeId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_DEF_TO_FROM_PREL_SHIFT, model).then(x => {
            return x.map(y => {
                var obj = new EmployeeListDTO();
                angular.extend(obj, y);
                obj.isVisible = true;
                return obj;
            });
        });
    }

    getIncomingDeliveries(loadRows: boolean, loadAccounts: boolean, useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY + loadRows + "/" + loadAccounts + "/" + false, useCache);
    }

    getIncomingDeliveriesForInterval(dateFrom: Date, dateTo: Date, ids: number[], useCache: boolean): ng.IPromise<IncomingDeliveryHeadDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        let url: string = Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY_FOR_INTERVAL + "?dateFrom=" + dateFromString + "&dateTo=" + dateToString;
        if (ids)
            url += "&ids=" + ids.join(',');

        return this.httpService.get(url, useCache).then(x => {
            return x.map(y => {
                let obj = new IncomingDeliveryHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        });
    }

    getIncomingDeliveriesGrid(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY + false + "/" + false + "/" + true, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getIncomingDelivery(incomingDeliveryHeadId: number, loadRows: boolean, loadAccounts: boolean, loadExcludedDates: boolean, loadAccountHierarchyAccount: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY + incomingDeliveryHeadId + "/" + loadRows + "/" + loadAccounts + "/" + loadExcludedDates + "/" + loadAccountHierarchyAccount, false).then(x => {
            let obj = new IncomingDeliveryHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getIncomingDeliveryRows(incomingDeliveryHeadId: number, loadAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY_ROW + incomingDeliveryHeadId + "/" + loadAccounts, false).then(x => {
            return x.map(y => {
                let obj = new IncomingDeliveryRowDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getIncomingDeliveryTypes(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY_TYPE, useCache);
    }

    getIncomingDeliveryType(incomingDeliveryTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY_TYPE + incomingDeliveryTypeId, false);
    }

    getPermittedEmployeeIds(): ng.IPromise<number[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_GRID + "?showInactive=true&showEnded=true&showNotStarted=true", false, Constants.WEBAPI_ACCEPT_SMALL_DTO).then((x: IEmployeeGridDTO[]) => {
            return _.map(x, y => y.employeeId);
        });
    }

    getRecalculateTimeHeads(recalculateAction: SoeRecalculateTimeHeadAction, loadRecords: boolean, showHistory: boolean, setExtensionNames: boolean, dateFrom?: Date, dateTo?: Date, limitNbrOfHeads?: number): ng.IPromise<RecalculateTimeHeadDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_RECALCULATE_TIME_HEAD + recalculateAction + "/" + loadRecords + "/" + showHistory + "/" + setExtensionNames + "/" + dateFromString + "/" + dateToString + "/" + limitNbrOfHeads || '0', false).then(x => {
            return x.map(h => {
                var obj = new RecalculateTimeHeadDTO();
                angular.extend(obj, h);
                obj.fixDates();

                obj.records = obj.records.map(r => {
                    let rObj = new RecalculateTimeRecordDTO();
                    angular.extend(rObj, r);
                    rObj.fixDates();
                    return rObj;
                });

                return obj;
            });
        });
    }

    getRecalculateTimeHead(recalculateTimeHeadId: number, loadRecords: boolean, setExtensionNames: boolean): ng.IPromise<RecalculateTimeHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_RECALCULATE_TIME_HEAD + recalculateTimeHeadId + "/" + loadRecords + "/" + setExtensionNames, false).then(x => {
            var obj = new RecalculateTimeHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();

            if (obj.records) {
                obj.records = obj.records.map(r => {
                    let rObj = new RecalculateTimeRecordDTO();
                    angular.extend(rObj, r);
                    rObj.fixDates();
                    return rObj;
                });
            } else {
                obj.records = [];
            }

            return obj;
        });
    }

    getRecalculateTimeHeadId(recalculateGuid: Guid): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_RECALCULATE_TIME_HEAD + recalculateGuid, false);
    }

    getScheduleTypes(getAll: boolean, onlyActive: boolean, loadFactors: boolean, loadTimeDeviationCauses: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCHEDULE_TYPE + getAll + "/" + onlyActive + "/" + loadFactors + "/" + loadTimeDeviationCauses, false);
    }

    getScheduleType(scheduleTypeId: number, loadFactors: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCHEDULE_TYPE + scheduleTypeId + "/" + loadFactors, false).then(x => {
            let obj = new TimeScheduleTypeDTO();
            angular.extend(obj, x);
            obj.factors = [];
            if (x.factors) {
                obj.factors = _.map(x.factors, f => {
                    let fObj = new TimeScheduleTypeFactorDTO();
                    angular.extend(fObj, f);
                    fObj.fixDates();
                    return fObj;
                });
            }
            return obj;
        });
    }

    validateBreakChange(employeeId: number, timeScheduleTemplateBlockId: number, timeScheduleTemplatePeriodId: number, timeCodeBreakId: number, dateFrom: Date, breakLength: number, isTemplate: boolean, timeScheduleScenarioHeadId: number = null) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_VALIDATE_BREAK_CHANGE + employeeId + "/" + timeScheduleTemplateBlockId + "/" + timeScheduleTemplatePeriodId + "/" + timeCodeBreakId + "/" + dateFromString + "/" + breakLength + "/" + isTemplate + "/" + (timeScheduleScenarioHeadId || '0'), false);
    }

    getShiftTypesGrid(loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, setCategoryNames: boolean, setAccountingString: boolean, setSkillNames: boolean, setTimeScheduleTypeName: boolean, useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE + "?loadAccounts=" + loadAccounts + "&loadSkills=" + loadSkills + "&loadEmployeeStatisticsTargets=" + loadEmployeeStatisticsTargets + "&setTimeScheduleTemplateBlockTypeName=" + setEmployeeStatisticsTargetsTypeName + "&setCategoryNames=" + setCategoryNames + "&setAccountingString=" + setAccountingString + "&setSkillNames=" + setSkillNames + "&setTimeScheduleTypeName=" + setTimeScheduleTypeName, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new ShiftTypeGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getShiftTypesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE + "?addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getShiftType(shiftTypeId: number, loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, loadCategories: boolean, loadHierarchyAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE + shiftTypeId + "/" + loadAccounts + "/" + loadSkills + "/" + loadEmployeeStatisticsTargets + "/" + setEmployeeStatisticsTargetsTypeName + "/" + loadCategories + "/" + loadHierarchyAccounts, false).then(x => {
            if (x) {
                let shiftType: ShiftTypeDTO = new ShiftTypeDTO();
                angular.extend(shiftType, x);
                shiftType.fixColors();
                return shiftType;
            } else {
                return x;
            }
        });
    }

    getShiftTypeLinks() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE_LINKS, false);
    }

    getSkills(useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_SCHEDULE_SKILL, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getShiftTypeSkills(shiftTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_SHIFT_TYPE + shiftTypeId, false);
    }

    getSkill(skillId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL + skillId, false);
    }

    getSkillTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_TYPE, false);
    }

    getSkillTypesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_TYPE + "?addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getSkillType(skillTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SKILL_TYPE + skillTypeId, false);
    }

    getStaffingNeedsHeadsForUser(type: StaffingNeedsHeadType, status: TermGroup_StaffingNeedsHeadStatus, loadRows: boolean, loadPeriods: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEADS_FOR_USER + type + "/" + status + "/" + loadRows + "/" + loadPeriods, false);
    }

    getStaffingNeedsHead(staffingNeedsHeadId: number, loadRows: boolean, loadPeriods: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD + staffingNeedsHeadId + "/" + loadRows + "/" + loadPeriods, false);
    }

    getStaffingNeedsLocations() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION, false);
    }

    getStaffingNeedsLocation(locationId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION + locationId, false);
    }

    getStaffingNeedsLocationGroups() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION_GROUP, false);
    }

    getStaffingNeedsLocationGroupsDict(addEmptyRow: boolean, includeAccountName: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION_GROUP + "?addEmptyRow=" + addEmptyRow + "&includeAccountName=" + includeAccountName, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getStaffingNeedsLocationGroup(locationGroupId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION_GROUP + locationGroupId, false);
    }

    getStaffingNeedsRules() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_RULE, false);
    }

    getStaffingNeedsRule(ruleId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_RULE + ruleId, false);
    }

    getStaffingNeedsUnscheduledTaskDates(shiftTypeIds: number[], dateFrom: Date, dateTo: Date, type: SoeStaffingNeedType) {
        const model = {
            dateFrom: dateFrom,
            dateTo: dateTo,
            shiftTypeIds: shiftTypeIds,
            type: type,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_UNSCHEDULED_TASK_DATES, model).then(x => {
            return x.map(y => {
                return CalendarUtility.convertToDate(y);
            });
        });
    }

    getStaffingNeedsUnscheduledTasks(shiftTypeIds: number[], dateFrom: Date, dateTo: Date, type: SoeStaffingNeedType): ng.IPromise<StaffingNeedsTaskDTO[]> {
        let model = {
            dateFrom: dateFrom,
            dateTo: dateTo,
            shiftTypeIds: shiftTypeIds,
            type: type,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_UNSCHEDULED_TASKS, model).then(x => {
            return x.map(y => {
                let obj = new StaffingNeedsTaskDTO(y.type);
                angular.extend(obj, y);
                obj.fixDates(false);
                obj.fixColors();
                return obj;
            })
        });
    }

    getTimeScheduleTaskGeneratedNeeds(timeScheduleTaskId: number): ng.IPromise<TimeScheduleTaskGeneratedNeedDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_TASK_GENERATED_NEEDS + timeScheduleTaskId, false).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTaskGeneratedNeedDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getDayTypesAndWeekdays() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE_AND_WEEKDAY, false);
    }

    getDayTypes(): ng.IPromise<DayTypeDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE, false).then(x => {
            return x.map(y => {
                let obj = new DayTypeDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }
    getDayType(dayTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE + dayTypeId, false);
    }

    getDayTypesDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE + "?addEmptyRow=" + addEmptyRow, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }
    getDaysOfWeekDict(addEmptyRow: boolean): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_DAY_WEEK + "?addEmptyRow=" + addEmptyRow, false);
    }

    getHolidays() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY, false);
    }

    getHolidaysSmall(dateFrom: Date, dateTo: Date): ng.IPromise<HolidaySmallDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY_SMALL + dateFromString + "/" + dateToString, false).then((x: HolidaySmallDTO[]) => {
            return x.map(y => {
                let obj = new HolidaySmallDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getSysHolidayTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY_SYS_HOLIDAY_TYPES, true);
    }

    getHoliday(holidayId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY + holidayId, false);
    }

    getHalfDayTypesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HALFDAY_TYPE_DICT + addEmptyRow, false);
    }

    getHalfDays() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HALFDAY, false);
    }

    getTimeBreakTemplates(): ng.IPromise<TimeBreakTemplateGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_BREAK_TEMPLATE, false).then(x => {
            return x.map(y => {
                let obj = new TimeBreakTemplateGridDTO;
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    createBreaksFromTemplatesForEmployees(date: Date, employeeIds: number[], timeScheduleScenarioHeadId?: number): ng.IPromise<ShiftDTO[]> {
        const model = {
            date: date,
            employeeIds: employeeIds,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_BREAK_TEMPLATE_CREATE_BREAKS_FOR_EMPLOYEES, model);
    }

    getScheduleCycleRuleType(scheduleCycleRuleTypeId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE_RULE_TYPE + scheduleCycleRuleTypeId, false);
    }

    getScheduleCycleRuleTypes() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE_RULE_TYPE, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getScheduleCycleRuleTypesDict() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE_RULE_TYPE, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getScheduleCyclesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE + "?addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getScheduleCycle(scheduleCycleId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE + scheduleCycleId, false);
    }

    getScheduleCycles() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getTimeCodeBreakGroups() {
        return this.httpService.get(Constants.WEBAPI_TIME_TIME_TIME_CODE_BREAK_GROUPS, false);
    }

    getTimeScheduleEmployeePeriodId(employeeId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_PERIOD_ID + employeeId + "/" + dateString, false);
    }

    getTimeScheduleEvents(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EVENT, useCache);
    }

    getTimeScheduleEventDatesForPlanning(dateFrom: Date, dateTo: Date) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EVENT_DATES_FOR_PLANNING + dateFromString + "/" + dateToString, false);
    }

    getTimeScheduleEventsForPlanning(date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EVENTS_FOR_PLANNING + dateString, false);
    }

    getTimeScheduleEventsDict(addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EVENT + "?addEmptyRow=" + addEmptyRow, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeScheduleEvent(timeScheduleEventId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_EVENT + timeScheduleEventId, false);
    }

    getTimeScheduleTasks(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASK, useCache);
    }

    getTimeScheduleTasksForFrequency(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASK_FOR_FREQUENCY + addEmptyRow, false);
    }

    getTimeScheduleTasksForInterval(dateFrom: Date, dateTo: Date, ids: number[], useCache: boolean): ng.IPromise<TimeScheduleTaskDTO[]> {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        var url: string = Constants.WEBAPI_TIME_SCHEDULE_TASK_FOR_INTERVAL + "?dateFrom=" + dateFromString + "&dateTo=" + dateToString;
        if (ids)
            url += "&ids=" + ids.join(',');

        return this.httpService.get(url, useCache).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTaskDTO();
                angular.extend(obj, y);
                obj.setTypes();
                return obj;
            });
        });
    }

    getTimeScheduleTasksGrid(useCache: boolean): ng.IPromise<TimeScheduleTaskGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASK, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTaskGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeScheduleTasksDict() {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASK, false, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeScheduleTask(timeScheduleTaskId: number, loadAccounts: boolean, loadExcludedDates: boolean, loadAccountHierarchyAccount: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASK + timeScheduleTaskId + "/" + loadAccounts + "/" + loadExcludedDates + "/" + loadAccountHierarchyAccount, false).then(x => {
            let obj = new TimeScheduleTaskDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getTimeScheduleTaskTypesDict(addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASKTYPE + "?addEmptyRow=" + addEmptyRow, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeScheduleTaskTypesGrid(useCache: boolean): ng.IPromise<TimeScheduleTaskTypeGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASKTYPE, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTaskTypeGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeScheduleTaskType(timeScheduleTaskTypeId: number): ng.IPromise<TimeScheduleTaskTypeDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TASKTYPE + timeScheduleTaskTypeId, false).then(x => {
            let obj = new TimeScheduleTaskTypeDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    getTimeScheduleTypesDict(getAll: boolean, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TYPE + "?getAll=" + getAll + "&addEmptyRow=" + addEmptyRow, true, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeScheduleTemplateGroups(useCache: boolean): ng.IPromise<TimeScheduleTemplateGroupDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP, useCache);
    }

    getTimeScheduleTemplateGroupsDict(addEmptyRow: boolean, useCache: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP + "?addEmptyRow=" + addEmptyRow, useCache, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getTimeScheduleTemplateGroupsGrid(useCache: boolean): ng.IPromise<TimeScheduleTemplateGroupGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP, useCache, Constants.WEBAPI_ACCEPT_GRID_DTO).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTemplateGroupGridDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getTimeScheduleTemplateGroup(timeScheduleTemplateGroupId: number, loadRows: boolean, loadEmployees: boolean, setNextStartDateOnRows: boolean, setEmployeeInfo: boolean): ng.IPromise<TimeScheduleTemplateGroupDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP + timeScheduleTemplateGroupId + "/" + loadRows + "/" + loadEmployees + "/" + setNextStartDateOnRows + "/" + setEmployeeInfo, false).then(x => {
            let obj = new TimeScheduleTemplateGroupDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getTimeScheduleTemplateGroupRowNextStartDate(startDate: Date, stopDate: Date, recurrencePattern: string): ng.IPromise<Date> {
        let startDateString: string = null;
        if (startDate)
            startDateString = startDate.toDateTimeString();

        let stopDateString: string = null;
        if (stopDate)
            stopDateString = stopDate.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP_ROW_NEXT_START_DATE + startDateString + "/" + stopDateString + "/" + recurrencePattern, false).then(x => {
            return CalendarUtility.convertToDate(x);
        });
    }

    getTimeScheduleTemplateHead(timeScheduleTemplateHeadId: number): ng.IPromise<TimeScheduleTemplateHeadDTO> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD + timeScheduleTemplateHeadId, false).then(x => {
            let obj = new TimeScheduleTemplateHeadDTO();
            angular.extend(obj, x);
            obj.fixDates();
            obj.setTypes();
            return obj;
        });
    }

    getTimeScheduleTemplateHeadSmall(timeScheduleTemplateHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD + timeScheduleTemplateHeadId, false, Constants.WEBAPI_ACCEPT_SMALL_DTO).then(x => {
            let obj = new TimeScheduleTemplateHeadSmallDTO();
            angular.extend(obj, x);
            obj.fixDates();
            return obj;
        });
    }

    getTimeScheduleTemplateHeads(): ng.IPromise<TimeScheduleTemplateHeadDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD, false).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTemplateHeadDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeScheduleTemplateHeadsForActivate(): ng.IPromise<TimeScheduleTemplateHeadSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_ACTIVATE, false).then((x: TimeScheduleTemplateHeadSmallDTO[]) => {
            return x.map(y => {
                let obj = new TimeScheduleTemplateHeadSmallDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getTimeScheduleTemplateHeadsRange(timeScheduleTemplateGroupId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TimeScheduleTemplateHeadsRangeDTO> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();

        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP_HEAD_RANGE + timeScheduleTemplateGroupId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            let obj = new TimeScheduleTemplateHeadsRangeDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getTimeScheduleTemplateHeadsRangeForEmployee(employeeId: number, dateFrom: Date, dateTo: Date): ng.IPromise<TimeScheduleTemplateHeadsRangeDTO> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();

        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP_HEAD_RANGE_FOR_EMPLOYEE + employeeId + "/" + dateFromString + "/" + dateToString, false).then(x => {
            let obj = new TimeScheduleTemplateHeadsRangeDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    getTimeScheduleTemplatePeriodsForActivate(timeScheduleTemplateHeadId: number): ng.IPromise<TimeScheduleTemplatePeriodSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_PERIOD_ACTIVATE + timeScheduleTemplateHeadId, false).then((x: TimeScheduleTemplatePeriodSmallDTO[]) => {
            return x.map(y => {
                let obj = new TimeScheduleTemplatePeriodSmallDTO();
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    getUnscheduledOrder(orderId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ORDER_UNSCHEDULED + orderId, false).then((x: OrderListDTO) => {
            if (x) {
                let order: OrderListDTO = new OrderListDTO();
                angular.extend(order, x);
                order.fixDates();
                order.fixColors();
                order.fixCategories();
                return order;
            } else {
                return x;
            }
        });
    }

    getOrderShifts(orderId: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_ORDER_SHIFTS + orderId, false).then((x: OrderShiftDTO[]) => {
            return x.map(y => {
                let shift: OrderShiftDTO = new OrderShiftDTO();
                angular.extend(shift, y);
                shift.fixDates();
                return shift;
            });
        });
    }

    getRecurrenceDescription(pattern: string) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_RECURRENCE_DESCRIPTION + pattern, false);
    }

    hasEmployeeSchedule(employeeId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_HAS_EMPLOYEE_SCHEDULE + employeeId + "/" + dateString, false);
    }

    getWorkRuleBypassLog(allItemsSelection: number) {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_WORKRULEBYPASSLOG + allItemsSelection, false);
    }

    getScenarioEmployeeIds(timeScheduleScenarioHeadId: number): ng.IPromise<number[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_EMPLOYEE + timeScheduleScenarioHeadId, false);
    }

    previewActivateScenario(timeScheduleScenarioHeadId: number, preliminaryDateFrom?: Date): ng.IPromise<PreviewActivateScenarioDTO[]> {
        let preliminaryDateFromString: string = null;
        if (preliminaryDateFrom)
            preliminaryDateFromString = preliminaryDateFrom.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD_ACTIVATE_PREVIEW + timeScheduleScenarioHeadId + "/" + preliminaryDateFromString, false).then(x => {
            return x.map(y => {
                let obj: PreviewActivateScenarioDTO = new PreviewActivateScenarioDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getActivateScenarioEmployeeStatus(timeScheduleScenarioHeadId: number): ng.IPromise<PreviewActivateScenarioDTO[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD_ACTIVATE_STATUS + timeScheduleScenarioHeadId, false).then(x => {
            return x.map(y => {
                let obj: PreviewActivateScenarioDTO = new PreviewActivateScenarioDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    previewCreateTemplateFromScenario(timeScheduleScenarioHeadId: number, dateFrom: Date, weekInCycle: number, dateTo?: Date): ng.IPromise<PreviewCreateTemplateFromScenarioDTO[]> {
        let dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();

        let dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();

        return this.httpService.get(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD_CREATE_TEMPLATE_PREVIEW + timeScheduleScenarioHeadId + "/" + dateFromString + "/" + weekInCycle + "/" + dateToString, false).then(x => {
            return x.map(y => {
                let obj: PreviewCreateTemplateFromScenarioDTO = new PreviewCreateTemplateFromScenarioDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    hasEmployeeTemplatesOfTypeSubstituteShifts(): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE_HAS_EMPLOYEE_TEMPLATES_OF_TYPE_SUBSTITUTE_SHIFTS, true);
    }

    getEmployeeTemplatesOfTypeSubstituteShifts(): ng.IPromise<SmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_TEMPLATE_OF_TYPE_SUBSTITUTE_SHIFTS, true);
    }

    // POST

    activateScenario(model: ActivateScenarioDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD_ACTIVATE, model);
    }

    createTemplateFromScenario(model: CreateTemplateFromScenarioDTO): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD_CREATE_TEMPLATE, model);
    }

    assignTimeScheduleTemplateToEmployee(timeScheduleTemplateHeadId: number, employeeId: number, startDate: Date) {
        var startDateString: string = null;
        if (startDate)
            startDateString = startDate.toDateTimeString();

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_ASSIGNTIMESCHEDULETEMPLATEHEADTOEMPLOYEE + timeScheduleTemplateHeadId + "/" + employeeId + "/" + startDateString, null);
    }

    removeEmployeeFromTimeScheduleTemplate(timeScheduleTemplateHeadId: number) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_REMOVE_EMPLOYEE + timeScheduleTemplateHeadId, null);
    }

    saveEmployeePost(employeePost: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST, employeePost);
    }

    saveScheduleCycleRuleType(scheduleCycleRuleType: IScheduleCycleRuleTypeDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE_RULE_TYPE, scheduleCycleRuleType);
    }

    saveScheduleCycle(scheduleCycle: IScheduleCycleDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE, scheduleCycle);
    }

    getAnnualScheduledTimeSummary(employeeIds: number[], dateFrom: Date, dateTo: Date, planningPeriodHeadId: number = null) {
        const model = {
            employeeIds: employeeIds,
            dateFrom: dateFrom,
            dateTo: dateTo,
            timePeriodHeadId: planningPeriodHeadId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_SCHEDULED_TIME, model);
    }

    getEmployeePeriodTimeSummary(employeeIds: number[], dateFrom: Date, dateTo: Date, planningPeriodHeadId: number = null): ng.IPromise<EmployeePeriodTimeSummary[]> {
        const model = {
            employeeIds: employeeIds,
            dateFrom: dateFrom,
            dateTo: dateTo,
            timePeriodHeadId: planningPeriodHeadId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_PERIOD_TIME_SUMMARY, model);
    }

    getEmployeesWithSubstituteShifts(employeeIds: number[], dates: Date[]) {
        const model = {
            employeeIds: employeeIds,
            dates: dates,
            printedFromSchedulePlanning: true,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_EMPLOYEES_WITH_SUBSTITUTE_SHIFTS, model);
    }

    getTimeEmploymentContractShortSubstituteUrl(employeeIds: number[], dates: Date[], printedFromSchedulePlanning: boolean) {
        const model = {
            employeeIds: employeeIds,
            dates: dates,
            printedFromSchedulePlanning: printedFromSchedulePlanning,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TIME_EMPLOYMENT_CONTRACT_SHORT_SUBSTITUTE_URL, model);
    }

    sendTimeEmploymentContractShortSubstituteForConfirmation(employeeIds: number[], dates: Date[], savePrintout: boolean) {
        const model = {
            employeeIds: employeeIds,
            dates: dates,
            printedFromSchedulePlanning: true,
            savePrintout: savePrintout
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_SEND_TIME_EMPLOYMENT_CONTRACT_SHORT_SUBSTITUTE_FOR_CONFIRMATION, model);
    }

    printEmploymentContractFromTemplate(employeeId: number, employeeTemplateId: number, substituteDates: Date[]): ng.IPromise<IActionResult> {
        let model = {
            employeeId: employeeId,
            employeeTemplateId: employeeTemplateId,
            substituteDates: substituteDates
        }

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_PRINT_EMPLOYMENT_CONTRACT_FROM_TEMPLATE, model);
    }

    saveIncomingDelivery(incomingDeliveryHead: IncomingDeliveryHeadDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY, incomingDeliveryHead);
    }

    saveIncomingDeliveryType(incomingDeliveryType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY_TYPE, incomingDeliveryType);
    }

    dragShifts(action: DragShiftAction, sourceShiftIds: number[], offsetDays: number, targetEmployeeId: number, skipXEMailOnChanges: boolean, copytaskWithShift: boolean, isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, includeOnDutyShifts?: boolean, includedOnDutyShiftIds?: number[]) {
        const model = {
            action: action,
            sourceShiftIds: sourceShiftIds,
            offsetDays: offsetDays,
            targetEmployeeId: targetEmployeeId,
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

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_DRAG_MULTIPLE, model);
    }

    saveEvaluateAllWorkRulesByPass(result: IEvaluateWorkRulesActionResult, employeeId: number) {
        const model = {
            result: result,
            employeeId: employeeId,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_SAVE_BYPASS, model);
    }

    evaluateAllWorkRules(shifts: ShiftDTO[], employeeIds: number[], startDate: Date, stopDate: Date, isPersonalScheduleTemplate: boolean, rules: SoeScheduleWorkRules[], timeScheduleScenarioHeadId?: number, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date) {
        const model = {
            shifts: shifts,
            employeeIds: employeeIds,
            startDate: startDate,
            stopDate: stopDate,
            isPersonalScheduleTemplate: isPersonalScheduleTemplate,
            rules: rules,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            planningPeriodStartDate: planningPeriodStartDate,
            planningPeriodStopDate: planningPeriodStopDate
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_ALL, model);
    }

    evaluateDragShiftsAgainstWorkRules(action: DragShiftAction, sourceShiftIds: number[], offsetDays: number, employeeId: number, isPersonalScheduleTemplate: boolean, rules: SoeScheduleWorkRules[], isStandByView: boolean, timeScheduleScenarioHeadId?: number, standbyCycleWeek?: number, standbyCycleDateFrom?: Date, standbyCycleDateTo?: Date, planningPeriodStartDate?: Date, planningPeriodStopDate?: Date) {
        const model = {
            action: action,
            sourceShiftIds: sourceShiftIds,
            offsetDays: offsetDays,
            employeeId: employeeId,
            isPersonalScheduleTemplate: isPersonalScheduleTemplate,
            rules: rules,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            standbyCycleWeek: standbyCycleWeek,
            standbyCycleDateFrom: standbyCycleDateFrom,
            standbyCycleDateTo: standbyCycleDateTo,
            isStandByView: isStandByView,
            planningPeriodStartDate: planningPeriodStartDate,
            planningPeriodStopDate: planningPeriodStopDate
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_DRAG_MULTIPLE, model);
    }

    saveScheduleType(scheduleType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCHEDULE_TYPE, scheduleType);
    }

    updateScheduleTypesState(dict: any) {
        const model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCHEDULE_TYPE_UPDATE_STATE, model);
    }

    updateEmployeePostsState(posts: any) {
        const model = { dict: posts };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_UPDATE_STATE, model);
    }

    saveShiftType(shiftType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE, shiftType);
    }
    saveDayType(dayType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE, dayType);
    }
    saveShiftTypeLinks(shiftTypeLinks: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE_LINKS, shiftTypeLinks);
    }

    getUnscheduledOrders(categoryIds: number[], dateTo?: Date) {
        const model = {
            categoryIds: categoryIds,
            dateTo: dateTo
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ORDER_UNSCHEDULED, model).then((x: OrderListDTO[]) => {
            return x.map(y => {
                let order: OrderListDTO = new OrderListDTO();
                angular.extend(order, y);
                order.fixDates();
                order.fixColors();
                order.fixCategories();
                return order;
            });
        });
    }

    getUnscheduledOrdersByIds(orderIds: number[]) {
        const model = {
            orderIds: orderIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ORDER_UNSCHEDULED_BY_IDS, model).then((x: OrderListDTO[]) => {
            return x.map(y => {
                let order: OrderListDTO = new OrderListDTO();
                angular.extend(order, y);
                order.fixDates();
                order.fixColors();
                order.fixCategories();
                return order;
            });
        });
    }

    getTemplateShifts(dateFrom: Date, dateTo: Date, loadYesterdayAlso: boolean, employeeIds: number[], includeGrossNetAndCost: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, loadTasks: boolean, useWeekendSalary: boolean) {
        const model = {
            dateFrom: dateFrom,
            dateTo: dateTo,
            loadYesterdayAlso: loadYesterdayAlso,
            employeeIds: employeeIds,
            includeGrossNetAndCost: includeGrossNetAndCost,
            includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost,
            loadTasks: loadTasks,
            userWeekendSalary: useWeekendSalary
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TEMPLATE_SHIFT_SEARCH, model).then((x: ShiftDTO[]) => {
            return x.map(y => {
                var obj: ShiftDTO = new ShiftDTO(y.type);
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    dragTemplateShift(action: DragShiftAction, sourceShiftId: number, sourceTemplateHeadId: number, sourceDate: Date, targetShiftId: number, targetTemplateHeadId: number, start: Date, end: Date, employeeId: number, employeePostId: number, targetLink: string, updateLinkOnTarget: boolean, copyTaskWithShift: boolean) {
        const model = {
            action: action,
            sourceShiftId: sourceShiftId,
            sourceTemplateHeadId: sourceTemplateHeadId,
            targetShiftId: targetShiftId,
            targetTemplateHeadId: targetTemplateHeadId,
            sourceDate: sourceDate,
            start: start,
            end: end,
            employeeId: employeeId,
            employeePostId: employeePostId,
            targetLink: targetLink,
            updateLinkOnTarget: updateLinkOnTarget,
            copyTaskWithShift: copyTaskWithShift,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TEMPLATE_SHIFT_DRAG, model);
    }

    dragTemplateShifts(action: DragShiftAction, sourceShiftIds: number[], sourceTemplateHeadId: number, firstSourceDate: Date, offsetDays: number, firstTargetDate: Date, targetEmployeeId: number, targetEmployeePostId: number, targetTemplateHeadId: number, copyTaskWithShift: boolean) {
        const model = {
            action: action,
            sourceShiftIds: sourceShiftIds,
            sourceTemplateHeadId: sourceTemplateHeadId,
            firstSourceDate: firstSourceDate,
            offsetDays: offsetDays,
            firstTargetDate: firstTargetDate,
            targetEmployeeId: targetEmployeeId,
            targetEmployeePostId: targetEmployeePostId,
            targetTemplateHeadId: targetTemplateHeadId,
            copyTaskWithShift: copyTaskWithShift,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TEMPLATE_SHIFT_DRAG_MULTIPLE, model);
    }

    getEmployeePostTemplateShifts(dateFrom: Date, dateTo: Date, employeePostIds: number[], loadTasks: boolean) {
        const model = {
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeePostIds: employeePostIds,
            loadTasks: loadTasks
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_TEMPLATE_SHIFT_SEARCH, model).then((x: ShiftDTO[]) => {
            return x.map(y => {
                var obj: ShiftDTO = new ShiftDTO(y.type);
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getScenarioHeadsDict(validAccountIds: number[], addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        const model = {
            validAccountIds: validAccountIds,
            addEmptyRow: addEmptyRow
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD_DICT, model);
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

    evaluateDragTemplateShiftAgainstWorkRules(action: DragShiftAction, sourceShiftId: number, sourceTemplateHeadId: number, sourceDate: Date, targetShiftId: number, targetTemplateHeadId: number, start: Date, end: Date, employeeId: number, employeePostId: number, rules: SoeScheduleWorkRules[]) {
        const model = {
            action: action,
            sourceShiftId: sourceShiftId,
            sourceTemplateHeadId: sourceTemplateHeadId,
            targetShiftId: targetShiftId,
            targetTemplateHeadId: targetTemplateHeadId,
            sourceDate: sourceDate,
            start: start,
            end: end,
            employeeId: employeeId,
            employeePostId: employeePostId,
            rules: rules
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_TEMPLATE_DRAG, model);
    }

    evaluateDragTemplateShiftsAgainstWorkRules(action: DragShiftAction, sourceShiftIds: number[], sourceTemplateHeadId: number, firstSourceDate: Date, offsetDays: number, employeeId: number, employeePostId: number, targetTemplateHeadId: number, firstTargetDate: Date, rules: SoeScheduleWorkRules[]) {
        const model = {
            action: action,
            sourceShiftIds: sourceShiftIds,
            sourceTemplateHeadId: sourceTemplateHeadId,
            firstSourceDate: firstSourceDate,
            offsetDays: offsetDays,
            employeeId: employeeId,
            employeePostId: employeePostId,
            targetTemplateHeadId: targetTemplateHeadId,
            firstTargetDate: firstTargetDate,
            rules: rules
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVALUATE_WORK_RULE_TEMPLATE_DRAG_MULTIPLE, model);
    }

    private _saveShiftsGuids: string[] = [];
    saveShifts(source: string, shifts: ShiftDTO[], updateBreaks: boolean, skipXEMailOnChanges: boolean, adjustTasks: boolean, minutesMoved: number, timeScheduleScenarioHeadId?: number): ng.IPromise<IActionResult> {
        let deferral = this.$q.defer<IActionResult>();

        if (_.includes(this._saveShiftsGuids, source)) {
            let message = "Duplicate calls to saveShifts detected and prevented!";
            console.error(message);
            deferral.reject({ error: Constants.SERVICE_ERROR_DUPLICATE_CALLS, message: message });
        } else {
            this._saveShiftsGuids.push(source);

            _.forEach(_.filter(shifts, s => s.isOrder), shift => {
                shift.order.unfixCategories();
            });

            const model = {
                source: source,
                shifts: shifts,
                updateBreaks: updateBreaks,
                skipXEMailOnChanges: skipXEMailOnChanges,
                adjustTasks: adjustTasks,
                minutesMoved: minutesMoved,
                timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            };

            this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT, model).then(result => {
                _.forEach(_.filter(shifts, s => s.isOrder), shift => {
                    shift.order.fixCategories();
                });
                deferral.resolve(result);
            });
        }

        return deferral.promise;
    }

    getShiftsGrossNetAndCost(employeeId: number, dateFrom: Date, dateTo: Date, employeeIds: number[], includeSecondaryCategories: boolean, includeBreaks: boolean, includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, timeScheduleScenarioHeadId?: number, useWeekendSalary?: boolean) {
        const model = {
            employeeId: employeeId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds,
            includeSecondaryCategories: includeSecondaryCategories,
            includeBreaks: includeBreaks,
            includePreliminary: includePreliminary,
            includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includeHolidaySalary: useWeekendSalary
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_GROSS_NET_AND_COST, model);
    }

    getTemplateShiftsGrossNetAndCost(dateFrom: Date, dateTo: Date, employeeIds: number[], includeEmploymentTaxAndSupplementChargeCost: boolean, useWeekendSalary: boolean) {
        const model = {
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds,
            includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost,
            useWeekendSalary: useWeekendSalary
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TEMPLATE_SHIFT_GROSS_NET_AND_COST, model);
    }

    getShiftPeriods(dateFrom: Date, dateTo: Date, employeeId: number, displayMode: TimeSchedulePlanningDisplayMode, blockTypes: number[], employeeIds: number[], shiftTypeIds: number[], deviationCauseIds: number[], includeGrossNetAndCost: boolean, includeToolTip: boolean, includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, preliminary?: boolean, timeScheduleScenarioHeadId?: number, useWeekendSalary?: boolean) {
        const model = {
            employeeId: employeeId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds,
            shiftTypeIds: shiftTypeIds,
            deviationCauseIds: deviationCauseIds,
            displayMode: displayMode,
            blockTypes: blockTypes,
            includeGrossNetAndCost: includeGrossNetAndCost,
            includePreliminary: includePreliminary,
            includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includeHolidaySalary: useWeekendSalary
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_PERIOD_SEARCH, model).then(x => {
            return x.map(s => {
                let obj = new ShiftPeriodDTO;
                angular.extend(obj, s);
                obj.fixDates();
                return obj;
            });
        });
    }

    getShiftPeriodsGrossNetAndCost(dateFrom: Date, dateTo: Date, employeeId: number, blockTypes: number[], employeeIds: number[], shiftTypeIds: number[], deviationCauseIds: number[], includePreliminary: boolean, includeEmploymentTaxAndSupplementChargeCost: boolean, preliminary?: boolean, timeScheduleScenarioHeadId?: number, useWeekendSalary?: boolean) {
        const model = {
            employeeId: employeeId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            employeeIds: employeeIds,
            shiftTypeIds: shiftTypeIds,
            deviationCauseIds: deviationCauseIds,
            blockTypes: blockTypes,
            includePreliminary: includePreliminary,
            includeEmploymentTaxAndSupplementChargeCost: includeEmploymentTaxAndSupplementChargeCost,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includeHolidaySalary: useWeekendSalary
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_PERIOD_GROSS_NET_AND_COST, model);
    }

    getShiftPeriodDetails(date: Date, employeeId: number, blockTypes: number[], employeeIds: number[], shiftTypeIds: number[], deviationCauseIds: number[], includePreliminary: boolean, preliminary?: boolean, timeScheduleScenarioHeadId?: number) {
        const model = {
            employeeId: employeeId,
            date: date,
            employeeIds: employeeIds,
            shiftTypeIds: shiftTypeIds,
            deviationCauseIds: deviationCauseIds,
            blockTypes: blockTypes,
            includePreliminary: includePreliminary,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_PERIOD_DETAIL, model);
    }

    getCyclePlannedMinutes(date: Date, employeeIds: number[]) {
        const model = {
            date: date,
            employeeIds: employeeIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_CYCLE_PLANNED_MINUTES, model);
    }

    saveSkill(skill: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SKILL, skill);
    }

    saveSkillType(skillType: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SKILL_TYPE, skillType);
    }

    updateSkillTypesState(dict: any) {
        const model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SKILL_TYPE_UPDATE_STATE, model);
    }

    saveHoliday(holiday: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY, holiday);
    }

    saveStaffingNeedsHead(head: StaffingNeedsHeadDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD, head);
    }

    saveStaffingNeedsLocation(location: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION, location);
    }

    saveStaffingNeedsLocationGroup(locationGroup: any, shiftTypeIds: number[]) {
        const model = { dto: locationGroup, shiftTypeIds: shiftTypeIds };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION_GROUP, model);
    }

    saveStaffingNeedsRule(rule: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_RULE, rule);
    }

    saveDefToFromPrelShift(prelToDef: boolean, dateFrom: Date, dateTo: Date, employeeIds: number[], includeScheduleShifts: boolean, includeStandbyShifts: boolean) {
        const model = {
            prelToDef: prelToDef,
            employeeId: 0,
            dateFrom: dateFrom.beginningOfDay(),
            dateTo: dateTo.beginningOfDay(),
            employeeIds: employeeIds,
            includeScheduleShifts: includeScheduleShifts,
            includeStandbyShifts: includeStandbyShifts
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_DEF_TO_FROM_PREL_SHIFT, model);
    }

    saveTimeBreakTemplates(breakTemplates: TimeBreakTemplateGridDTO[]) {
        const model = {
            breakTemplates: breakTemplates,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_BREAK_TEMPLATE, model);
    }

    validateTimeBreakTemplates(breakTemplates: TimeBreakTemplateGridDTO[]): ng.IPromise<TimeBreakTemplateGridDTO[]> {
        const model = {
            breakTemplates: breakTemplates,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_BREAK_TEMPLATE_VALIDATE, model).then(x => {
            return x.map(y => {
                var obj = new TimeBreakTemplateGridDTO;
                angular.extend(obj, y);
                return obj;
            });
        });
    }

    saveTimeScheduleScenarioHead(scenarioHead: TimeScheduleScenarioHeadDTO, timeScheduleScenarioHeadId: number, includeAbsence: boolean, dateFunction: boolean): ng.IPromise<IActionResult> {
        const model = {
            scenarioHead: scenarioHead,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includeAbsence: includeAbsence,
            dateFunction: dateFunction
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD, model);
    }

    saveTimeScheduleTask(timeScheduleTask: TimeScheduleTaskDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TASK, timeScheduleTask);
    }

    saveTimeScheduleTaskType(timeScheduleTaskType: TimeScheduleTaskTypeDTO) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TASKTYPE, timeScheduleTaskType);
    }

    saveTimeScheduleTemplate(templateHead: TimeScheduleTemplateHeadDTO, blocks: TimeScheduleTemplateBlockSlim[]): ng.IPromise<IActionResult> {
        let templateBlocks: any[] = [];
        _.forEach(blocks, block => {
            let templateBlock = {
                timeScheduleTemplatePeriodId: block.timeScheduleTemplatePeriodId,
                timeCodeId: block.timeCodeId,
                dayNumber: block.dayNumber,
                startTime: block.startTime,
                stopTime: block.stopTime,
                shiftTypeId: block.shiftTypeId,
                break1TimeCodeId: block.break1TimeCodeId,
                break2TimeCodeId: block.break2TimeCodeId,
                break3TimeCodeId: block.break3TimeCodeId,
                break4TimeCodeId: block.break4TimeCodeId,
            };
            templateBlocks.push(templateBlock);
        });

        const model = {
            head: templateHead,
            blocks: templateBlocks
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_SAVE, model);
    }

    saveTimeScheduleTemplateGroup(timeScheduleTemplateGroup: TimeScheduleTemplateGroupDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP, timeScheduleTemplateGroup);
    }

    saveTimeScheduleTemplateHead(employeeId: number, shifts: ShiftDTO[], timeScheduleTemplateHeadId: number, dayNumberFrom: number, dayNumberTo, currentDate: Date, activateDayNumber: number, activateDates: Date[], skipXEMailOnChanges: boolean) {
        const model = {
            shifts: shifts,
            employeeId: employeeId,
            timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
            dayNumberFrom: dayNumberFrom,
            dayNumberTo: dayNumberTo,
            currentDate: currentDate,
            activateDayNumber: activateDayNumber,
            activateDates: activateDates,
            skipXEMailOnChanges: skipXEMailOnChanges
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_SAVE_TEMPLATE, model);
    }

    saveTimeScheduleTemplateAndPlacement(saveTemplate: boolean, savePlacement: boolean, control: ActivateScheduleControlDTO, shifts: ShiftDTO[], timeScheduleTemplateHeadId: number, templateNoOfDays: number, templateStartDate, templateStopDate: Date, firstMondayOfCycle: Date, placementDateFrom: Date, placementDateTo: Date, currentDate: Date, simpleSchedule: boolean, startOnFirstDayOfWeek: boolean, preliminary: boolean, locked: boolean, employeeId: number, copyFromTimeScheduleTemplateHeadId?: number, useAccountingFromSourceSchedule?: boolean) {
        const model = {
            control: control,
            saveTemplate: saveTemplate,
            savePlacement: savePlacement,
            shifts: shifts,
            timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
            templateNoOfDays: templateNoOfDays,
            templateStartDate: templateStartDate,
            templateStopDate: templateStopDate,
            firstMondayOfCycle: firstMondayOfCycle,
            placementDateFrom: placementDateFrom,
            placementDateTo: placementDateTo,
            currentDate: currentDate,
            simpleSchedule: simpleSchedule,
            startOnFirstDayOfWeek: startOnFirstDayOfWeek,
            preliminary: preliminary,
            locked: locked,
            employeeId: employeeId,
            copyFromTimeScheduleTemplateHeadId: copyFromTimeScheduleTemplateHeadId,
            useAccountingFromSourceSchedule: useAccountingFromSourceSchedule
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_SAVE_TEMPLATE_AND_PLACEMENT, model);
    }

    updateTimeScheduleTemplateHeadsState(dict: any): ng.IPromise<any> {
        const model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_UPDATE_STATE, model);
    }

    saveTimeScheduleEvent(timeScheduleEvent: any) {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EVENT, timeScheduleEvent);
    }

    getStaffingNeedsHeadfromIncomingDeliveryHead(interval: number, incomingDeliveryHeadDTOs: IncomingDeliveryHeadDTO[], name: string, date: Date, dayTypeId: number, dayOfWeek: number) {
        const model = {
            interval: interval,
            incomingDeliveryHeadDTOs: incomingDeliveryHeadDTOs,
            name: name,
            date: date,
            dayTypeId: dayTypeId,
            dayOfWeek: dayOfWeek
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD_TIME_SCHEDULE_TASK, model);
    }

    getStaffingNeedsHeadfromTimeScheduleTask(interval: number, timeScheduleTaskDTOs: TimeScheduleTaskDTO[], name: string, date: Date, dayTypeId: number, dayOfWeek: number) {
        const model = {
            interval: interval,
            timeScheduleTaskDTOs: timeScheduleTaskDTOs,
            name: name,
            date: date,
            dayTypeId: dayTypeId,
            dayOfWeek: dayOfWeek
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD_INCOMING_DELIVERY_HEAD, model);
    }

    createStaffingNeedsHeadsFromTasks(interval: number, name: string, date: Date, dayTypeId: number, dayOfWeek: number, wholeWeek: boolean, includeStaffingNeedsChartData: boolean, intervalDateFrom: Date, intervalDateTo: Date, dayOfWeeks: number[], adjustPercent: number, currentDate: Date, timeScheduleTaskId: number) {
        const model = {
            interval: interval,
            name: name,
            date: date,
            dayTypeId: dayTypeId,
            dayOfWeek: dayOfWeek,
            wholeWeek: wholeWeek,
            includeStaffingNeedsChartData: includeStaffingNeedsChartData,
            intervalDateFrom: intervalDateFrom,
            intervalDateTo: intervalDateTo,
            dayOfWeeks: dayOfWeeks,
            adjustPercent: adjustPercent,
            currentDate: currentDate,
            staffingNeedsFrequencyTimeScheduleTaskId: timeScheduleTaskId
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEADS_FROM_TASKS, model);
    }

    generateStaffingNeedsHeads(needFilterType: TermGroup_StaffingNeedHeadsFilterType, dayTypeId: number, dateFrom: Date, dateTo: Date) {
        const model = {
            needFilterType: needFilterType,
            dayTypeId: dayTypeId,
            dateFrom: dateFrom,
            dateTo: dateTo
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD_GENERATE_HEADS, model);
    }

    generateStaffingNeedsHeadsForInterval(needFilterType: TermGroup_StaffingNeedHeadsFilterType, dateFrom: Date, dateTo: Date, calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType, calculateNeed: boolean, calculateNeedFrequency: boolean, calculateNeedRowFrequency: boolean, calculateBudget: boolean, calculateForecast: boolean, calculateTemplateSchedule: boolean, calculateSchedule: boolean, calculateTime: boolean, calculateTemplateScheduleForEmployeePost: boolean, accountDimId: number, accountId: number, employeeIds: number[], employeePostIds: number[], timeScheduleScenarioHeadId: number, includeEmpTaxAndSuppCharge: boolean, shiftTypeIds: number[], forceWeekView: boolean) {
        const model = {
            needFilterType: needFilterType,
            dateFrom: dateFrom,
            dateTo: dateTo,
            calculationType: calculationType,
            calculateNeed: calculateNeed,
            calculateNeedFrequency: calculateNeedFrequency,
            calculateNeedRowFrequency: calculateNeedRowFrequency,
            calculateBudget: calculateBudget,
            calculateForecast: calculateForecast,
            calculateTemplateSchedule: calculateTemplateSchedule,
            calculateSchedule: calculateSchedule,
            calculateTemplateScheduleForEmployeePost: calculateTemplateScheduleForEmployeePost,
            calculateTime: calculateTime,
            accountDimId: accountDimId,
            accountId: accountId,
            employeeIds: employeeIds,
            employeePostIds: employeePostIds,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includeEmpTaxAndSuppCharge: includeEmpTaxAndSuppCharge,
            shiftTypeIds: shiftTypeIds,
            forceWeekView: forceWeekView
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD_GENERATE_HEADS_FOR_INTERVAL, model).then((x: StaffingStatisticsInterval[]) => {
            return x.map(d => {
                let obj = new StaffingStatisticsInterval();
                angular.extend(obj, d);
                obj.fixDates();

                obj.rows = obj.rows.map(r => {
                    let rObj = new StaffingStatisticsIntervalRow();
                    angular.extend(rObj, r);

                    let bObj = new StaffingStatisticsIntervalValue();
                    angular.extend(bObj, rObj.budget);
                    let tsObj = new StaffingStatisticsIntervalValue();
                    angular.extend(tsObj, rObj.templateSchedule);
                    let sObj = new StaffingStatisticsIntervalValue();
                    angular.extend(sObj, rObj.schedule);
                    let stObj = new StaffingStatisticsIntervalValue();
                    angular.extend(stObj, rObj.scheduleAndTime);
                    let tObj = new StaffingStatisticsIntervalValue();
                    angular.extend(tObj, rObj.time);

                    return rObj;
                });

                return obj;
            });
        });
    }

    recalculateStaffingNeedsSummary(row: StaffingStatisticsIntervalRow): ng.IPromise<StaffingStatisticsIntervalRow> {
        const model = {
            row: row,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD_RECALCULATE_SUMMARY, model).then((x: StaffingStatisticsIntervalRow) => {
            let obj = new StaffingStatisticsIntervalRow();
            angular.extend(obj, x);
            return obj;
        });
    }

    createShiftsFromStaffingNeeds(needFilterType: TermGroup_StaffingNeedHeadsFilterType, dayTypeId: number, dateFrom: Date, dateTo: Date) {
        const model = {
            needFilterType: needFilterType,
            dayTypeId: dayTypeId,
            dateFrom: dateFrom,
            dateTo: dateTo
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD_CREATE_SHIFTS, model);
    }

    createEmptyScheduleForEmployeePost(employeePostId: number, fromDate: Date) {
        const model = {
            employeePostId: employeePostId,
            fromDate: fromDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_CREATE_EMPTY_SCHEDULE_FOR_EMPLOYEE_POST, model);
    }

    createEmptyScheduleForEmployeePosts(employeePostIds: number[], fromDate: Date) {
        const model = {
            employeePostIds: employeePostIds,
            fromDate: fromDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_CREATE_EMPTY_SCHEDULE_FOR_EMPLOYEE_POSTS, model);
    }

    createScheduleFromEmployeePost(employeePostId: number, fromDate: Date) {
        const model = {
            employeePostId: employeePostId,
            fromDate: fromDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_CREATE_FROM_EMPLOYEE_POST, model);
    }

    getPreAnalysisInformation(employeePostId: number, fromDate: Date) {
        const model = {
            employeePostId: employeePostId,
            fromDate: fromDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_GET_PREANALYSIS_INFORMATION, model).then((x: PreAnalysisInformation) => {
            let obj = new PreAnalysisInformation();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    createScheduleFromEmployeePosts(employeePostIds: number[], fromDate: Date) {
        const model = {
            employeePostIds: employeePostIds,
            fromDate: fromDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_CREATE_FROM_EMPLOYEE_POSTS, model);
    }

    createScheduleFromEmployeePostsAsync(employeePostIds: number[], fromDate: Date) {
        const model = {
            employeePostIds: employeePostIds,
            fromDate: fromDate,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_CREATE_FROM_EMPLOYEE_POSTS_ASYNC, model);
    }

    deleteScheduleFromEmployeePosts(employeePostIds: number[]) {

        const model = {
            Numbers: employeePostIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_DELETE_FROM_EMPLOYEE_POSTS, model);
    }

    deleteEmployeePosts(employeePostIds: number[]): ng.IPromise<IActionResult> {
        const model = {
            Numbers: employeePostIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_DELETE_MULTIPLE, model);
    }

    deleteGeneratedNeeds(staffingNeedRowPeriodIds: number[]) {

        const model = {
            Numbers: staffingNeedRowPeriodIds
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_DELETE_GENERATED_NEEDS, model);
    }
    assignTaskToEmployee(employeeId: number, date: Date, taskDTOs: StaffingNeedsTaskDTO[], skipXEMailOnShiftChanges: boolean) {
        const model = {
            employeeId: employeeId,
            date: date,
            taskDTOs: taskDTOs,
            skipXEMailOnShiftChanges: skipXEMailOnShiftChanges
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_SHIFT_TASKS_ASSIGN_TO_EMPLOYEE, model);
    }

    evaluateAssignTaskToEmployeeAgainstWorkRules(destinationEmployeeId: number, destinationDate: Date, taskDTOs: StaffingNeedsTaskDTO[], rules: SoeScheduleWorkRules[]) {
        const model = {
            destinationEmployeeId: destinationEmployeeId,
            destinationDate: destinationDate,
            taskDTOs: taskDTOs,
            rules: rules
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_SHIFT_TASKS_ASSIGN_TO_EMPLOYEE_EVALUATE_WORKRULES, model);
    }

    assignTemplateShiftTask(tasks: StaffingNeedsTaskDTO[], date: Date, timeScheduleTemplateHeadId: number) {
        if (!timeScheduleTemplateHeadId)
            timeScheduleTemplateHeadId = 0;

        const model = {
            tasks: tasks,
            date: date,
            timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_SHIFT_TASKS_ASSIGNTEMPLATESHIFTTASKS, model);
    }

    createEmployeePostsFromEmployments(ids: number[], dateFrom: Date) {
        const model = {
            numbers: ids,
            fromDate: dateFrom
        };
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_FROM_EMPLOYMENTS, model);
    }

    changeStatusForEmployeePost(employeePostId: number, status: SoeEmployeePostStatus) {
        const model = {
            employeePostId: employeePostId,
            status: status,
        };

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST_FROM_EMPLOYMENTS_CHANGESTATUS, model);
    }

    copySchedule(sourceEmployeeId: number, sourceDateEnd: Date, targetEmployeeId: number, targetDateStart: Date, targetDateEnd: Date, useAccountingFromSourceSchedule: boolean) {
        const model = {
            sourceEmployeeId: sourceEmployeeId,
            sourceDateEnd: sourceDateEnd,
            targetEmployeeId: targetEmployeeId,
            targetDateStart: targetDateStart,
            targetDateEnd: targetDateEnd,
            useAccountingFromSourceSchedule: useAccountingFromSourceSchedule
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_COPY_SCHEDULE, model);
    }

    getPlacementsForGrid(onlyLatest: boolean, addEmptyPlacement: boolean, employeeIds: number[], dateFrom?: Date, dateTo?: Date): ng.IPromise<ActivateScheduleGridDTO[]> {
        const model = {
            onlyLatest: onlyLatest,
            addEmptyPlacement: addEmptyPlacement,
            employeeIds: employeeIds,
            dateFrom: dateFrom,
            dateTo: dateTo,
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_FOR_ACTIVATE_GRID, model).then(x => {
            return x.map(y => {
                let obj = new ActivateScheduleGridDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    isPlacementsUnchanged(items: ActivateScheduleGridDTO[], placementStopDate: Date): ng.IPromise<IActionResult> {
        const model = {
            items: items,
            placementStopDate: placementStopDate
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_IS_PLACEMENTS_UNCHANGED, model);
    }

    controlActivation(employeeId: number, employeeScheduleStartDate: Date, employeeScheduleStopDate: Date, startDate?: Date, stopDate?: Date, isDelete?: boolean): ng.IPromise<ActivateScheduleControlDTO> {
        const model = {
            employeeId: employeeId,
            employeeScheduleStartDate: employeeScheduleStartDate,
            employeeScheduleStopDate: employeeScheduleStopDate,
            startDate: startDate,
            stopDate: stopDate,
            isDelete: isDelete,
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_CONTROLACTIVATION, model).then(x => {
            let obj = new ActivateScheduleControlDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    controlActivations(items: ActivateScheduleGridDTO[], startDate?: Date, stopDate?: Date, isDelete?: boolean): ng.IPromise<ActivateScheduleControlDTO> {
        const model = {
            items: items,
            startDate: startDate,
            stopDate: stopDate,
            isDelete: isDelete,
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_CONTROLACTIVATIONS, model).then(x => {
            let obj = new ActivateScheduleControlDTO();
            angular.extend(obj, x);
            return obj;
        });
    }

    activateSchedule(control: ActivateScheduleControlDTO, items: ActivateScheduleGridDTO[], func: TermGroup_TemplateScheduleActivateFunctions, timeScheduleTemplateHeadId: number, timeScheduleTemplatePeriodId: number, startDate: Date, stopDate: Date, preliminary: boolean): ng.IPromise<IActionResult> {
        const model = {
            control: control,
            items: items,
            function: func,
            timeScheduleTemplateHeadId: timeScheduleTemplateHeadId,
            timeScheduleTemplatePeriodId: timeScheduleTemplatePeriodId,
            startDate: startDate,
            stopDate: stopDate,
            preliminary: preliminary,
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_ACTIVATE, model);
    }

    deleteEmployeeSchedule(control: ActivateScheduleControlDTO, item: ActivateScheduleGridDTO): ng.IPromise<IActionResult> {
        const model = {
            control: control,
            item: item,
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_SCHEDULE_DELETE, model);
    }

    getEmployeeAccounts(employeeIds: number[], dateFrom: Date, dateTo: Date): ng.IPromise<EmployeeAccountDTO[]> {
        const model = {
            employeeIds: employeeIds,
            dateFrom: dateFrom,
            dateTo: dateTo
        }

        return this.httpService.post(Constants.WEBAPI_TIME_EMPLOYEE_EMPLOYEE_ACCOUNT, model).then(x => {
            return x.map(y => {
                let obj = new EmployeeAccountDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    getOngoingTimeScheduleTemplateHeads(dict: any): ng.IPromise<TimeScheduleTemplateHeadSmallDTO[]> {
        let model = {
            dict: dict
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_GET_ONGOING, model).then(x => {
            return x.map(y => {
                let obj = new TimeScheduleTemplateHeadSmallDTO();
                angular.extend(obj, y);
                obj.fixDates();
                return obj;
            });
        });
    }

    setStopDateOnTimeScheduleTemplateHeads(dict: any): ng.IPromise<IActionResult> {
        let model = {
            dict: dict
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD_SET_STOP_DATE, model);
    }

    exportShiftsToExcel(shifts: ShiftDTO[], employees: EmployeeListDTO[], dates: Date[], selections: IReportDataSelectionDTO[]): ng.IPromise<IActionResult> {
        let model = {
            shifts: shifts,
            employees: employees,
            dates: dates,
            selections: selections
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_EXPORT_TO_EXCEL, model);
    }

    validatePossibleDeleteOfEmployeeAccount(model: ValidatePossibleDeleteOfEmployeeAccountDTO): ng.IPromise<IActionResult> {
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_VALIDATE_POSSIBLE_DELETE_OF_EMPLOYEE_ACCOUNT, model);
    }

    saveTimeScheduleEmployeePeriodDetail(shift: ShiftDTO): ng.IPromise<IActionResult> {
        let model: TimeScheduleEmployeePeriodDetailDTO = new TimeScheduleEmployeePeriodDetailDTO();
        model.timeScheduleEmployeePeriodDetailId = shift.timeScheduleEmployeePeriodDetailId;
        model.type = SoeTimeScheduleEmployeePeriodDetailType.Unknown;
        model.timeScheduleScenarioHeadId = shift.timeScheduleScenarioHeadId;
        model.timeLeisureCodeId = shift.timeLeisureCodeId;
        model.employeeId = shift.employeeId;
        model.date = shift.startTime;

        if (shift.timeLeisureCodeId)
            model.type = SoeTimeScheduleEmployeePeriodDetailType.LeisureCode;

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_EMPLOYEE_PERIOD_DETAIL, model);
    }

    deleteTimeScheduleEmployeePeriodDetail(timeScheduleEmployeePeriodDetailIds: number[]): ng.IPromise<IActionResult> {
        let model = {
            numbers: timeScheduleEmployeePeriodDetailIds
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_EMPLOYEE_PERIOD_DETAIL_DELETE, model);
    }

    setRecalculateTimeHeadToProcessed(recalculateTimeHeadId: number): ng.IPromise<IActionResult> {
        let model = {
            id: recalculateTimeHeadId
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_RECALCULATE_TIME_HEAD_SET_TO_PROCESSED, model);
    }

    allocateLeisureDays(startDate: Date, stopDate: Date, employeeIds: number[]): ng.IPromise<AutomaticAllocationResultDTO> {
        let model = {
            startDate: startDate,
            stopDate: stopDate,
            employeeIds: employeeIds
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_LEISURE_CODE_ALLOCATE_LEISURE_DAYS, model).then(x => {
            let obj = new AutomaticAllocationResultDTO();
            angular.extend(obj, x);
            obj.setTypes();
            return obj;
        });
    }

    deleteLeisureDays(startDate: Date, stopDate: Date, employeeIds: number[]): ng.IPromise<IActionResult> {
        let model = {
            startDate: startDate,
            stopDate: stopDate,
            employeeIds: employeeIds
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_TIME_LEISURE_CODE_ALLOCATE_LEISURE_DAYS_DELETE, model);
    }

    createAnnualLeaveShift(date: Date, employeeId: number): ng.IPromise<IActionResult> {
        let model = {
            date: date,
            employeeId: employeeId
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_LEAVE_SHIFT, model);
    }

    getAnnualLeaveBalance(date: Date, employeeIds: number[]): ng.IPromise<IAnnualLeaveBalance[]> {
        let model = {
            date: date,
            employeeIds: employeeIds
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_LEAVE_BALANCE, model);
    }

    recalculateAnnualLeaveBalance(date: Date, employeeIds: number[], previousYear: boolean): ng.IPromise<IAnnualLeaveBalance[]> {
        let model = {
            date: date,
            employeeIds: employeeIds,
            previousYear: previousYear
        }
        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_LEAVE_BALANCE_RECALCULATE, model);
    }

    // DELETE

    cancelRecalculateTimeHead(recalculateTimeHeadId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_RECALCULATE_TIME_HEAD + recalculateTimeHeadId);
    }

    cancelRecalculateTimeRecord(recalculateTimeRecordId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_RECALCULATE_TIME_RECORD + recalculateTimeRecordId);
    }

    deleteEmployeePost(employeePostId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_EMPLOYEE_POST + employeePostId);
    }

    deleteHoliday(holidayId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_HOLIDAY + holidayId);
    }

    deleteIncomingDelivery(incomingDeliveryHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY + incomingDeliveryHeadId);
    }

    deleteIncomingDeliveryType(incomingDeliveryTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_INCOMING_DELIVERY_TYPE + incomingDeliveryTypeId);
    }

    deleteScenarioHead(timeScheduleScenarioHeadId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD + timeScheduleScenarioHeadId);
    }

    deleteScheduleCycleRuleType(scheduleCycleRuleTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE_RULE_TYPE + scheduleCycleRuleTypeId);
    }

    deleteScheduleCycle(scheduleCycleId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_CYCLE + scheduleCycleId);
    }

    deleteScheduleFromEmployeePost(employeePostId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_DELETE_FROM_EMPLOYEE_POST + employeePostId);
    }

    deleteScheduleType(scheduleTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SCHEDULE_TYPE + scheduleTypeId);
    }

    deleteShifts(shiftIds: number[], skipXEMailOnChanges: boolean, timeScheduleScenarioHeadId?: number, includedOnDutyShiftIds?: number[]) {
        const model = {
            shiftIds: shiftIds,
            skipXEMailOnChanges: skipXEMailOnChanges,
            timeScheduleScenarioHeadId: timeScheduleScenarioHeadId,
            includedOnDutyShiftIds: includedOnDutyShiftIds
        }

        return this.httpService.post(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_DELETE, model);
    }

    deleteShiftType(shiftTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE + shiftTypeId);
    }

    deleteShiftTypes(shiftTypeIds: number[]) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SHIFT_TYPE + shiftTypeIds.join(','));
    }

    deleteSkill(skillId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SKILL + skillId);
    }
    deleteDayType(dayTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_DAY_TYPE + dayTypeId);
    }
    deleteSkillType(skillTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SKILL_TYPE + skillTypeId);
    }

    deleteStaffingNeedsHead(staffingNeedsHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_HEAD + staffingNeedsHeadId);
    }

    deleteStaffingNeedsLocation(locationId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION + locationId);
    }

    deleteStaffingNeedsLocationGroup(locationGroupId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_LOCATION_GROUP + locationGroupId);
    }

    deleteStaffingNeedsRule(ruleId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_STAFFING_NEEDS_RULE + ruleId);
    }

    deleteTimeScheduleEvent(timeScheduleEventId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_EVENT + timeScheduleEventId);
    }

    deleteTimeScheduleScenarioHead(timeScheduleScenarioHeadId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_SCENARIO_HEAD + timeScheduleScenarioHeadId);
    }

    deleteTimeScheduleTask(timeScheduleTaskId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_TASK + timeScheduleTaskId);
    }

    deleteTimeScheduleTaskType(timeScheduleTaskTypeId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_TASKTYPE + timeScheduleTaskTypeId);
    }

    deleteTimeScheduleTemplateGroup(timeScheduleTemplateGroupId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_GROUP + timeScheduleTemplateGroupId);
    }

    deleteTimeScheduleTemplateHead(timeScheduleTemplateHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_TIME_SCHEDULE_TEMPLATE_HEAD + timeScheduleTemplateHeadId);
    }

    deleteAnnualLeaveShift(timeScheduleTemplateBlockId: number): ng.IPromise<IActionResult> {
        return this.httpService.delete(Constants.WEBAPI_TIME_SCHEDULE_ANNUAL_LEAVE_SHIFT + timeScheduleTemplateBlockId);
    }
}
