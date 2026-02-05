import angular from "angular";
import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ScheduleHandler } from "./ScheduleHandler";
import { TemplateHelper } from "./TemplateHelper";
import { PlanningEditModes, PlanningTabs, SchedulePlanningFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, PlanningStatusFilterItems, SOEMessageBoxSize, TemplateScheduleModes, AbsenceRequestViewMode, AbsenceRequestParentMode, EmployeeAvailabilitySortOrder, DayOfWeek, AbsenceRequestGuiMode, PlanningEmployeeListSortBy, PlanningOrderListSortBy, StaffingNeedsFunctions } from "../../../Util/Enumerations";
import { ISmallGenericType, ITimeScheduleTypeSmallDTO, ITimeCodeBreakSmallDTO, IActionResult, IAvailableEmployeesDTO, IAnnualLeaveBalance } from "../../../Scripts/TypeLite.Net4";
import { EmployeeListDTO, EmployeeListEmploymentDTO, EmployeeRightListDTO } from "../../../Common/Models/EmployeeListDTO";
import { AccountDimSmallDTO, AccountDimDTO } from "../../../Common/Models/AccountDimDTO";
import { ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";
import { EvaluateAllWorkRulesResultDTO } from "../../../Common/Models/WorkRuleDTOs";
import { IncomingDeliveryRowDTO, StaffingNeedsHeadDTO, StaffingNeedsRowDTO, StaffingNeedsRowPeriodDTO, StaffingNeedsTaskDTO, StaffingStatisticsInterval, StaffingStatisticsIntervalRow, TimeScheduleTaskDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { ShiftDTO, SlotDTO, ShiftPeriodDTO, TimeScheduleEventForPlanningDTO, ShiftBreakDTO, TemplateScheduleEmployeeDTO, OrderListDTO, TimeSchedulePlanningSettingsDTO, TimeScheduleScenarioHeadDTO, TimeScheduleScenarioAccountDTO, PlanningPeriodHead, PlanningPeriod, TimeLeisureCodeSmallDTO, AutomaticAllocationResultDTO } from "../../../Common/Models/TimeSchedulePlanningDTOs";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IReportService } from "../../../Core/Services/ReportService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { ITimeService } from "../../Time/TimeService";
import { IContextMenuHandler } from "../../../Core/Handlers/ContextMenuHandler";
import { IContextMenuHandlerFactory } from "../../../Core/Handlers/ContextMenuHandlerFactory";
import { TimeScheduleTemplateHeadSmallDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { TimeScheduleTypeSmallDTO } from "../../../Common/Models/TimeScheduleTypeDTO";
import { GraphicsUtility } from "../../../Util/GraphicsUtility";
import { SelectableInformationController } from "./Dialogs/SelectableInformation/SelectableInformationController";
import { EditController as EmployeeEditController } from "../../Employee/Employees/EditController";
import { EditController as EmployeePostEditController } from "../EmployeePosts/EditController";
import { EditController as TimeScheduleTasksEditController } from "../TimeScheduleTasks/EditController";
import { EditController as IncomingDeliveriesEditController } from "../IncomingDeliveries/EditController";
import { DeleteDeliveryController } from "./Dialogs/DeleteDelivery/DeleteDeliveryController";
import { DeleteTaskController } from "./Dialogs/DeleteTask/DeleteTaskController";
import { ClipboardController } from "./Dialogs/Clipboard/ClipboardController";
import { Guid } from "../../../Util/StringUtility";
import { CalendarDetailsController } from "./Dialogs/CalendarDetails/CalendarDetailsController";
import { ScheduleEventsController } from "./Dialogs/ScheduleEvents/ScheduleEventsController";
import { EditShiftController } from "../../../Shared/Time/Schedule/Planning/Dialogs/EditShift/EditShiftController";
import { EditBookingController } from "../../../Shared/Time/Schedule/Planning/Dialogs/EditBooking/EditBookingController";
import { EditAssignmentController } from "../../../Shared/Time/Schedule/Planning/Dialogs/EditAssignment/EditAssignmentController";
import { ShiftHistoryController } from "../../../Shared/Time/Schedule/Planning/Dialogs/ShiftHistory/ShiftHistoryController";
import { SelectBreakTimeCodeController } from "./Dialogs/SelectBreakTimeCode/SelectBreakTimeCodeController";
import { ShiftRequestStatusController } from "./Dialogs/ShiftRequestStatus/ShiftRequestStatusController";
import { DeleteShiftController } from "./Dialogs/DeleteShift/DeleteShiftController";
import { SplitShiftController } from "../../../Shared/Time/Schedule/Planning/Dialogs/SplitShift/SplitShiftController";
import { DragShiftController } from "./Dialogs/DragShift/DragShiftController";
import { DropEmployeeController } from "./Dialogs/DropEmployee/DropEmployeeController";
import { AssignEmployeePostController } from "./Dialogs/AssignEmployeePost/AssignEmployeePostController";
import { AssignTaskController } from "./Dialogs/AssignTask/AssignTaskController";
import { AnnualSummaryController } from "./Dialogs/AnnualSummary/AnnualSummaryController";
import { TemplateScheduleController } from "./Dialogs/TemplateSchedule/TemplateScheduleController";
import { DefToFromPrelShiftController } from "./Dialogs/DefToFromPrelShift/DefToFromPrelShiftController";
import { CopyScheduleController } from "./Dialogs/CopySchedule/CopyScheduleController";
import { CreateTemplateBreaksController } from "./Dialogs/CreateTemplateBreaks/CreateTemplateBreaksController";
import { PrintEmploymentCertificateController } from "./Dialogs/PrintEmploymentCertificate/PrintEmploymentCertificateController";
import { EditController as MessageEditController } from "../../../Core/RightMenu/MessageMenu/EditController";
import { EditController as AbsenceRequestsEditController } from "../../../Shared/Time/Schedule/Absencerequests/EditController";
import { TermGroup_TimeSchedulePlanningVisibleDays, TermGroup_TimeSchedulePlanningShiftStyle, TermGroup_TimeSchedulePlanningDayViewGroupBy, TermGroup_TimeSchedulePlanningDayViewSortBy, TermGroup_TimeSchedulePlanningScheduleViewGroupBy, TermGroup_TimeSchedulePlanningScheduleViewSortBy, TermGroup_TimeSchedulePlanningFollowUpCalculationType, TimeSchedulePlanningMode, TimeSchedulePlanningDisplayMode, Feature, SoeEmployeePostStatus, CompanySettingType, SettingMainType, SoeReportTemplateType, UserSettingType, TermGroup, SoeCategoryType, TermGroup_StaffingNeedHeadsFilterType, TermGroup_TimeScheduleTemplateBlockType, TermGroup_ShiftHistoryType, SoeScheduleWorkRules, DragShiftAction, SoeValidateBreakChangeError, TimeScheduledTimeSummaryType, SoeStaffingNeedType, TermGroup_MessageType, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, TermGroup_TimeSchedulePlanningBreakVisibility, TermGroup_OrderPlanningShiftInfo, TermGroup_AssignmentTimeAdjustmentType, SoeTimeAttestFunctionOption, TermGroup_TimeSchedulePlanningViews, XEMailType, SoeModule, SoeStaffingNeedsTaskType, TermGroup_StaffingNeedsHeadStatus, TermGroup_StaffingNeedsDayViewSortBy, TermGroup_StaffingNeedsDayViewGroupBy, TermGroup_StaffingNeedsScheduleViewGroupBy, TermGroup_ReportExportType, UserSelectionType, TermGroup_TimeScheduleTemplateBlockShiftStatus, TermGroup_EmployeeSelectionAccountingType, TermGroup_TimeScheduleTemplateBlockAbsenceType, TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditController as EditOrderController } from "../../../Shared/Billing/Orders/EditController";
import { GridController as ActivateScheduleGridController } from "../Activate/GridController";
import { HolidaySmallDTO } from "../../../Common/Models/HolidayDTO";
import { CreateTemplateSchedulesController } from "./Dialogs/CreateTemplateSchedules/CreateTemplateSchedulesController";
import { EmployeeAccountDTO } from "../../../Common/Models/EmployeeUserDTO";
import { SelectReportController } from "../../../Common/Dialogs/SelectReport/SelectReportController";
import { TimeAttestCalculationFunctionDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { TimeAttestCalculationController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/Calculation/TimeAttestCalculationController";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { AdjustFollowUpDataController } from "./Dialogs/AdjustFollowUpData/AdjustFollowUpDataController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CreateScenarioHeadController } from "./Dialogs/CreateScenarioHead/CreateScenarioHeadController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ActivateScenarioController } from "./Dialogs/ActivateScenario/ActivateScenarioController";
import { PreAnalysisInformationController } from "./Dialogs/PreAnalysisInformation/PreAnalysisInformationController";
import { ModalUtility } from "../../../Util/ModalUtility";
import { EditEmployeeAvailabilityDialogController } from "../../../Common/Dialogs/EditEmployeeAvailability/EditEmployeeAvailabilityDialogController";
import { DateRangeDTO } from "../../../Common/Models/DateRangeDTO";
import { BoolSelectionDTO, IdListSelectionDTO, TextSelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { SelectionCollection } from "../../../Core/RightMenu/ReportMenu/SelectionCollection";
import { CreateNeedController } from "./Dialogs/CreateNeed/CreateNeedController";
import { UserSelectionDTO } from "../../../Common/Models/UserSelectionDTOs";
import { CreateTemplateFromScenarioController } from "./Dialogs/CreateTemplateFromScenario/CreateTemplateFromScenarioController";
import { GeneratedNeedsDialogController } from "../TimeScheduleTasks/Dialogs/GeneratedNeedsDialogController";
import { SelectTemplateScheduleController } from "./Dialogs/SelectTemplateSchedule/SelectTemplateScheduleController";
import { NumberUtility } from "../../../Util/NumberUtility";
import { EmployeePeriodTimeSummaryController } from "./Dialogs/EmployeePeriodTimeSummary/EmployeePeriodTimeSummaryController";
import { EditLeisureCodeController } from "./Dialogs/EditLeisureCode/EditLeisureCodeController";
import { DeleteLeisureCodeController } from "./Dialogs/DeleteLeisureCode/DeleteLeisureCodeController";
import { AllocateLeisureCodesController } from "./Dialogs/AllocateLeisureCodes/AllocateLeisureCodesController";
import { CreateAbsenceController } from "./Dialogs/CreateAbsence/CreateAbsenceController";
import { DeleteAbsenceController } from "./Dialogs/DeleteAbsence/DeleteAbsenceController";

declare var XLSX: any;
declare var saveAs: any;

export class EditController extends EditControllerBase {

    private scheduleHandler: ScheduleHandler;
    private templateHelper: TemplateHelper;

    public shiftHeight: number = 38;
    public shiftHeightCompressed: number = 20;
    public shiftMargin: number = 2;
    public shiftMarginCompressed: number = 0;
    private staffingNeedsHeight = 18;
    private fromScenarioEvaluate: boolean = false;

    // Init parameters
    private employeeId: number = 0;
    private employeeGroupId: number = 0;
    private sortableOptions;

    // Data
    public scenarioHead: TimeScheduleScenarioHeadDTO;
    public timeScheduleScenarioHeadId: number;
    private keepScenarioHeadId = false;
    private scenarioEmployeeIds: number[] = [];
    private scenarioDays: number = 0;
    public planningPeriodHead: PlanningPeriodHead;
    public planningPeriodChild: PlanningPeriod;
    public currentPlanningPeriod: TimePeriodDTO;
    private currentPlanningPeriodInRange = false;
    private currentPlanningPeriodChildInRange = false;
    public currentPlanningPeriodChildInRangeExact = false;
    public get hasPlanningPeriodHeadButNoChild(): boolean {
        return this.planningPeriodHead && !this.planningPeriodChild;
    }
    private currentYearFrom: number = 0;
    private currentYearTo: number = 0;
    public dates: DateDay[] = [];
    private dayOfWeeks: number[] = [1, 2, 3, 4, 5, 6, 0];
    public shifts: ShiftDTO[] = [];
    public allShiftsMap: Map<string, ShiftDTO> = new Map();
    public visibleShifts: ShiftDTO[] = [];
    public visibleShiftsMap: Map<string, ShiftDTO> = new Map();
    public visibleEmployeeIdsSet: Set<number> = new Set();

    public get nonZeroShifts(): ShiftDTO[] {
        return this.shifts.filter(s => !s.isZeroShift);
    }

    public employeeShiftsMap: Map<number, Map<number, ShiftDTO[]>>; //by employeeId then by dates time value
    private cutCopiedShifts: ShiftDTO[] = [];
    private isCut: boolean;
    public periods: ShiftPeriodDTO[];
    private nbrOfPeriodShifts: number = 0; se
    private nbrOfVisiblePeriodShifts: number = 0;
    public hiddenEmployeeId: number = 0;
    private vacantEmployeeIds: number[] = [];
    private validAccountIds: number[] = [];

    public allTasks: StaffingNeedsTaskDTO[] = [];
    private tasks: TimeScheduleTaskDTO[] = [];
    private filteredTasks: TimeScheduleTaskDTO[] = [];
    private deliveries: IncomingDeliveryRowDTO[] = [];

    private allHeads: StaffingNeedsHeadDTO[] = [];
    public heads: StaffingNeedsHeadDTO[] = [];
    public originalHead: StaffingNeedsHeadDTO;

    // Both permissions
    private calendarViewReadPermission: boolean = false;
    private calendarViewModifyPermission: boolean = false;
    private dayViewReadPermission: boolean = false;
    private dayViewModifyPermission: boolean = false;
    private scheduleViewReadPermission: boolean = false;
    private scheduleViewModifyPermission: boolean = false;
    private templateDayViewReadPermission: boolean = false;
    private templateDayViewModifyPermission: boolean = false;
    private templateScheduleViewReadPermission: boolean = false;
    private templateScheduleViewModifyPermission: boolean = false;
    private employeePostDayViewReadPermission: boolean = false;
    private employeePostDayViewModifyPermission: boolean = false;
    private employeePostScheduleViewReadPermission: boolean = false;
    private employeePostScheduleViewModifyPermission: boolean = false;
    private standbyDayViewReadPermission: boolean = false;
    private standbyDayViewModifyPermission: boolean = false;
    private standbyScheduleViewReadPermission: boolean = false;
    private standbyScheduleViewModifyPermission: boolean = false;
    private tasksAndDeliveriesDayViewReadPermission: boolean = false;
    private tasksAndDeliveriesDayViewModifyPermission: boolean = false;
    private tasksAndDeliveriesScheduleViewReadPermission: boolean = false;
    private tasksAndDeliveriesScheduleViewModifyPermission: boolean = false;
    private staffingNeedsDayViewReadPermission: boolean = false;
    private staffingNeedsDayViewModifyPermission: boolean = false;
    private staffingNeedsScheduleViewReadPermission: boolean = false;
    private staffingNeedsScheduleViewModifyPermission: boolean = false;

    private get calendarViewPermission(): boolean {
        return this.calendarViewReadPermission || this.calendarViewModifyPermission;
    }

    private get dayViewPermission(): boolean {
        return this.dayViewReadPermission || this.dayViewModifyPermission;
    }

    private get scheduleViewPermission(): boolean {
        return this.scheduleViewReadPermission || this.scheduleViewModifyPermission;
    }

    private get templateDayViewPermission(): boolean {
        return this.templateDayViewReadPermission || this.templateDayViewModifyPermission;
    }

    private get templateScheduleViewPermission(): boolean {
        return this.templateScheduleViewReadPermission || this.templateScheduleViewModifyPermission;
    }

    private get employeePostDayViewPermission(): boolean {
        return this.employeePostDayViewReadPermission || this.employeePostDayViewModifyPermission;
    }

    private get employeePostScheduleViewPermission(): boolean {
        return this.employeePostScheduleViewReadPermission || this.employeePostScheduleViewModifyPermission;
    }

    private get standbyDayViewPermission(): boolean {
        return this.standbyDayViewReadPermission || this.standbyDayViewModifyPermission;
    }

    private get standbyScheduleViewPermission(): boolean {
        return this.standbyScheduleViewReadPermission || this.standbyScheduleViewModifyPermission;
    }

    private get tasksAndDeliveriesDayViewPermission(): boolean {
        return this.tasksAndDeliveriesDayViewReadPermission || this.tasksAndDeliveriesDayViewModifyPermission;
    }

    private get tasksAndDeliveriesScheduleViewPermission(): boolean {
        return this.tasksAndDeliveriesScheduleViewReadPermission || this.tasksAndDeliveriesScheduleViewModifyPermission;
    }

    private get staffingNeedsDayViewPermission(): boolean {
        return this.staffingNeedsDayViewReadPermission || this.staffingNeedsDayViewModifyPermission;
    }

    private get staffingNeedsScheduleViewPermission(): boolean {
        return this.staffingNeedsScheduleViewReadPermission || this.staffingNeedsScheduleViewModifyPermission;
    }

    public get hasCurrentViewModifyPermission(): boolean {
        switch (this.viewDefinition) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                return this.calendarViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.Day:
                return this.dayViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.Schedule:
                return this.scheduleViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
                return this.templateDayViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
                return this.templateScheduleViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
                return this.employeePostDayViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
                return this.employeePostScheduleViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
                return this.scenarioDayViewPermission;
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
                return this.scenarioScheduleViewPermission;
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
                return this.standbyDayViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
                return this.standbyScheduleViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
                return this.tasksAndDeliveriesDayViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
                return this.tasksAndDeliveriesScheduleViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                return this.staffingNeedsDayViewModifyPermission;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                return this.staffingNeedsScheduleViewModifyPermission;
        }
    }

    // Read only permissions
    private noViewPermission = false;
    private viewPermissionsLoaded = false;
    private bookingReadPermission = false;
    private standbyShiftsReadPermission = false;
    private onDutyShiftsReadPermission = false;
    public showQueuePermission = false;
    private showTotalCostPermission = false;
    private showUnscheduledTasksPermission = false;
    private showStaffingNeedsPermission = false;
    public showBudgetPermission = false;
    public showForecastPermission = false;
    private showOrdersOnMapPermission = false;
    private reportPermission = false;
    private viewAvailabilityPermission = false;

    // Modify permissions
    private seeOtherEmployeesShiftsPermission = false;
    private savePublicSelectionPermission = false;
    private scenarioDayViewPermission = false;
    private scenarioScheduleViewPermission = false;
    public activeScheduleEditHiddenPermission = false;
    public templateScheduleEditHiddenPermission = false;
    public standbyEditHiddenPermission = false;
    private bookingModifyPermission = false;
    private standbyShiftsModifyPermission = false;
    private onDutyShiftsModifyPermission = false;
    private attestPermission = false;
    private preliminaryPermission = false;
    private copySchedulePermission = false;
    private placementPermission = false;
    private showNeedsPermission = false;
    private showDashboardPermission = false;
    public adjustKPIsPermission = false;
    private restoreToSchedulePermission = false;
    private editAvailabilityPermission = false;

    // Company settings
    public useAccountHierarchy = false;
    private defaultEmployeeAccountDimId = 0;
    private useVacant = false;
    public defaultTimeCodeId = 0;
    private skillCantBeOverridden = false;
    private showSummaryInCalendarView = false;
    public dayViewStartTime = 0;   // Minutes from midnight
    public dayViewEndTime = 0;     // Minutes from midnight
    private originalDayViewStartTime = 0;
    private originalDayViewEndTime = 0;
    public dayViewMinorTickLength = 0;
    public clockRounding = 0;
    private shiftTypeMandatory = false;
    private allowHolesWithoutBreaks = false;
    private keepShiftsTogether = false;
    private sendXEMailOnChange = false;
    private possibleToSkipWorkRules = false;
    private dayScheduleReportId = 0;
    private weekScheduleReportId = 0;
    private dayTemplateScheduleReportId = 0;
    private weekTemplateScheduleReportId = 0;
    private dayEmployeePostTemplateScheduleReportId = 0;
    private weekEmployeePostTemplateScheduleReportId = 0;
    private dayScenarioScheduleReportId = 0;
    private weekScenarioScheduleReportId = 0;
    private employmentContractShortSubstituteReportId = 0;
    private employmentContractShortSubstituteReportName = '';
    private tasksAndDeliveriesDayReportId = 0;
    private tasksAndDeliveriesWeekReportId = 0;
    private hasEmployeeTemplates = false;
    private maxNbrOfBreaks = 1;
    private useTemplateScheduleStopDate = false;
    private placementDefaultPreliminary = false;
    private placementHidePreliminary = false;
    private calculatePlanningPeriodScheduledTime = false;
    public calculatePlanningPeriodScheduledTimeUseAveragingPeriod = false;
    private planningPeriodColors: string[] = ["da1e28", "24a148", "0565c9"]; // @soe-color-semantic-error, @soe-color-semantic-success, @soe-color-semantic-information
    public get planningPeriodColorOver(): string {
        return `#${this.planningPeriodColors[0]}`;
    }
    public get planningPeriodColorEqual(): string {
        return `#${this.planningPeriodColors[1]}`;
    }
    public get planningPeriodColorUnder(): string {
        return `#${this.planningPeriodColors[2]}`;
    }
    private showGrossTimeSetting = false;
    public showExtraShift = false;
    public showSubstitute = false;
    public useMultipleScheduleTypes = false;
    private orderPlanningIgnoreScheduledBreaksOnAssignment = false;
    private useShiftRequestPreventTooEarly = false;
    private inactivateLending = false;
    private useLeisureCodes = false;
    private useAnnualLeave = false;
    private extraShiftAsDefaultOnHidden = false;
    private dragDropMoveAsDefault = false;

    // User settings
    public selectableInformationSettings: TimeSchedulePlanningSettingsDTO;
    private accountHierarchyId: string;
    private allAccountsSelected: boolean = false;
    private isDefaultAccountDimLevel: boolean = false;
    private userAccountId: number;
    private defaultView: TermGroup_TimeSchedulePlanningViews = TermGroup_TimeSchedulePlanningViews.Schedule;
    private defaultInterval: number = TermGroup_TimeSchedulePlanningVisibleDays.Week;
    private defaultShiftStyle: TermGroup_TimeSchedulePlanningShiftStyle = TermGroup_TimeSchedulePlanningShiftStyle.Detailed;
    private dayViewDefaultGroupBy: TermGroup_TimeSchedulePlanningDayViewGroupBy = TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee;
    private dayViewDefaultSortBy: TermGroup_TimeSchedulePlanningDayViewSortBy = TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime;
    private scheduleViewDefaultGroupBy: TermGroup_TimeSchedulePlanningScheduleViewGroupBy = TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee;
    private scheduleViewDefaultSortBy: TermGroup_TimeSchedulePlanningScheduleViewSortBy = TermGroup_TimeSchedulePlanningScheduleViewSortBy.Firstname;
    private tadDayViewDefaultGroupBy: TermGroup_StaffingNeedsDayViewGroupBy = TermGroup_StaffingNeedsDayViewGroupBy.None;
    private tadDayViewDefaultSortBy: TermGroup_StaffingNeedsDayViewSortBy = TermGroup_StaffingNeedsDayViewSortBy.StartTime;
    private tadScheduleViewDefaultGroupBy: TermGroup_StaffingNeedsScheduleViewGroupBy = TermGroup_StaffingNeedsScheduleViewGroupBy.None;
    private orderPlanningShiftInfoTopRight: TermGroup_OrderPlanningShiftInfo = TermGroup_OrderPlanningShiftInfo.NoInfo;
    private orderPlanningShiftInfoBottomLeft: TermGroup_OrderPlanningShiftInfo = TermGroup_OrderPlanningShiftInfo.NoInfo;
    private orderPlanningShiftInfoBottomRight: TermGroup_OrderPlanningShiftInfo = TermGroup_OrderPlanningShiftInfo.NoInfo;

    private hasStaffingByEmployeeAccount: boolean = false;

    private startWeek: number = 0;
    private calendarViewCountByEmployee: boolean = false;
    private defaultShowEmployeeList: boolean = false;
    private disableCheckBreakTimesWarning: boolean = false;
    private disableBreaksWithinHolesWarning: boolean = false;
    private disableSaveAndActivateCheck: boolean = false;
    private autoSaveAndActivate: boolean = false;
    private disableAutoLoad: boolean = true;
    private disableTemplateScheduleWarning: boolean = false;
    private doNotShowTemplateScheduleWarningAgain: boolean = false;
    private setInitialHiddenEmployeeFilter: boolean = false;
    private firstLoadHasOccurred: boolean = false;

    private showShiftTypeSum: boolean = false;
    public staffingNeedsDayViewShowDiagram: boolean = false;
    public staffingNeedsDayViewShowDetailedSummary: boolean = false;
    public staffingNeedsScheduleViewShowDetailedSummary: boolean = false;

    // Lookups
    private visibleDays: ISmallGenericType[];
    private categories: any[] = [];
    private showSecondaryAccounts: boolean = false;
    private showSecondaryCategories: boolean = false;
    public allEmployees: EmployeeListDTO[] = [];
    public employedEmployees: EmployeeListDTO[] = [];
    private employees: any[] = [];
    private employeeList: EmployeeRightListDTO[] = [];
    private employeeGroups: any[] = [];
    private nbrOfVisibleEmployees: number = 0;
    private inactiveEmployeeIds: number[] = [];
    private permittedEmployeeIds: number[] = [];
    private accountDims: AccountDimSmallDTO[] = [];
    private shiftTypeAccountDim: AccountDimDTO;
    private allShiftTypes: ShiftTypeDTO[] = [];
    private shiftTypes: any[] = [];
    private shiftTypeIds: number[];
    private timeScheduleTypes: ITimeScheduleTypeSmallDTO[];
    private statuses: any[];
    private deviationCauses: any[];
    private blockTypes: any[];
    private taskTypes: any[];
    private timeScheduleTaskTypes: any[] = [];
    private shiftStyles: any[];
    private breakTimeCodes: ITimeCodeBreakSmallDTO[];
    private leisureCodes: TimeLeisureCodeSmallDTO[];
    private absenceTypes: ISmallGenericType[];
    private workRuleViolations: EvaluateAllWorkRulesResultDTO[];
    public unscheduledTasks: StaffingNeedsTaskDTO[] = [];
    private unscheduledTaskDates: Date[] = [];
    public orderList: OrderListDTO[] = [];
    private allUnscheduledOrders: OrderListDTO[] = [];
    private unscheduledOrderDates: Date[] = [];
    private currentUnscheduledOrderDates: Date[] = [];
    private notCurrentUnscheduledOrderDates: Date[] = [];
    private scheduleEventDates: Date[] = [];
    private scheduledTimeEmployeeIds: number[] = [];
    private scheduledTimeEmployeeIdsCount: number = 0;
    private annualLeaveBalanceEmployeeIds: number[] = [];
    private annualLeaveBalanceEmployeeIdsCount: number = 0;
    public followUpCalculationTypes: ISmallGenericType[];
    private holidays: HolidaySmallDTO[] = [];
    private scenarioHeads: ISmallGenericType[] = [];
    private intervals: any[];
    private weekdays: any[] = [{ dayOfWeek: 1 }, { dayOfWeek: 2 }, { dayOfWeek: 3 }, { dayOfWeek: 4 }, { dayOfWeek: 5 }, { dayOfWeek: 6 }, { dayOfWeek: 0 }];
    private headStatuses: any[];
    private needsFilterTypes: ISmallGenericType[];
    private frequencyTasks: ISmallGenericType[];

    // Tools
    protected buttonGroups = new Array<ToolBarButtonGroup>();
    private functions: any[] = [];
    // TODO: Implement permission on this
    public editMode: PlanningEditModes = PlanningEditModes.Shifts;

    // Filters
    private showFilters: boolean = false;
    private minutesToTimeSpanFilter: any;
    private amountFilter: any;

    private selectedCategories: any[] = [];
    private selectedEmployees: any[] = [];
    private selectedEmployeeGroups: any[] = [];
    private selectedShiftTypes: any[] = [];
    private selectedStatuses: any[] = [];
    private selectedDeviationCauses: any[] = [];
    private selectedBlockTypes: any[] = [];
    private selectedTaskTypes: any[] = [];
    private selectedTimeScheduleTaskTypes: any[] = [];
    private selectedTasks: any[] = [];
    private selectedDeliveries: any[] = [];
    private freeTextFilter: string;
    private subsetOfShiftsLoaded: boolean = false;
    private filteredButNotLoaded: boolean = false;

    private userSelectionType: UserSelectionType;
    private userSelections: SelectionCollection;
    private selectedUserSelection: UserSelectionDTO;
    private selectedUserSelectionId: number;
    private delayFilterByUserSelection: boolean = true;

    // Employee list
    private employeeListItemUrl: string;
    private showEmployeeListFilters: boolean = false;
    private employeeListFilterOnSelectedShift: boolean = false;
    private employeeListHideAssignedToPost: boolean = false;
    private employeeListFilterOnSelectedEmployeePost: boolean = false;
    private filterEmployeeListOnSkills: boolean = false;
    private filterEmployeeListOnPercent: boolean = false;
    private filterEmployeeListOnPercentDiff: number = 0;
    private employeeListFreeTextFilter: string = '';
    private employeeListFilterEmployeeIds: number[] = [];
    private employeeListFilterShiftIds: number[] = [];
    private employeeListFilterEmployeePostId: number;
    private filteringEmployees: boolean = false;
    private isEmployeeListFiltered: boolean = false;

    // Order list
    private unscheduledOrderListItemUrl: string;
    private showOrderListFilters: boolean = false;
    private orderListFilterOnShiftType: number = 0;
    private orderListFreeTextFilter: string = '';
    private orderListShowFutureOrders: boolean = false;
    private get isOrderListFiltered(): boolean {
        return (this.orderListFilterOnShiftType !== 0 || this.orderListFreeTextFilter.length > 0);
    }
    private isOrderOverdue(order: OrderListDTO): boolean {
        return order.plannedStartDate && order.plannedStartDate.isBeforeOnDay(this.dateFrom);
    }
    private getNbrOfOrdersWithNoPlannedStopDate(): number {
        return this.orderList.filter(o => !o.plannedStopDate).length;
    }
    private getNbrOfOrdersWithNoPlannedStartDate(): number {
        return this.orderList.filter(o => !o.plannedStartDate).length;
    }

    // Selectable information
    private get loadingSelectableInformation(): boolean {
        return this.loadingCycleTimes || this.loadingGrossNetAndCost || this.loadingStaffingNeed || this.loadingAvailability || this.loadingPlanningPeriodSummary;
    }

    // Grouping and sorting
    public dayViewGroupBy: TermGroup_TimeSchedulePlanningDayViewGroupBy = TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee;
    private dayViewSortBy: TermGroup_TimeSchedulePlanningDayViewSortBy = TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname;
    public scheduleViewGroupBy: TermGroup_TimeSchedulePlanningScheduleViewGroupBy = TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee;
    private scheduleViewSortBy: TermGroup_TimeSchedulePlanningScheduleViewSortBy = TermGroup_TimeSchedulePlanningScheduleViewSortBy.Firstname;

    public tadDayViewGroupBy: TermGroup_StaffingNeedsDayViewGroupBy = TermGroup_StaffingNeedsDayViewGroupBy.None;
    private tadDayViewSortBy: TermGroup_StaffingNeedsDayViewSortBy = TermGroup_StaffingNeedsDayViewSortBy.Name;
    public tadScheduleViewGroupBy: TermGroup_StaffingNeedsScheduleViewGroupBy = TermGroup_StaffingNeedsScheduleViewGroupBy.None;

    public get isGrouped(): boolean {
        return this.isGroupedByAccount || this.isGroupedByCategory || this.isGroupedByShiftType;
    }
    public get isGroupedByAccount(): boolean {
        return (this.isCommonDayView && this.dayViewGroupBy > 10) || (this.isCommonScheduleView && this.scheduleViewGroupBy > 10);
    }
    public get isGroupedByCategory(): boolean {
        return (this.isCommonDayView && this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Category) || (this.isCommonScheduleView && this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Category);
    }
    public get isGroupedByShiftType(): boolean {
        return (this.isCommonDayView && this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType) || (this.isCommonScheduleView && this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.ShiftType);
    }

    private employeeListSortBy: PlanningEmployeeListSortBy = PlanningEmployeeListSortBy.Firstname;
    public orderListSortBy: PlanningOrderListSortBy = PlanningOrderListSortBy.Priority;

    public shiftStyle: TermGroup_TimeSchedulePlanningShiftStyle = TermGroup_TimeSchedulePlanningShiftStyle.Detailed;
    public get isCompressedStyle(): boolean {
        return this.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.ActualTimeCompressed ||
            this.shiftStyle === TermGroup_TimeSchedulePlanningShiftStyle.DetailedCompressed ||
            this.isCommonDayView;
    }

    // Summaries
    private staffingNeedData: StaffingStatisticsInterval[] = [];
    public staffingNeedOriginalSummaryRow: StaffingStatisticsIntervalRow;
    public staffingNeedSum: any[] = [];
    public plannedMinutesSum: any[] = [];
    public factorMinutesSum: any[] = [];
    public grossMinutesSum: any[] = [];
    public totalCostSum: any[] = [];
    public timeShifts: {
        time: number,
        nbrOfShifts: number,
        groupedShifts: { groupName: string, nbrOfShifts: number }[]
    }[] = [];
    public totalCostIncEmpTaxAndSupplementChargeSum: any[] = [];

    // Flags
    private tabActivated = false;
    public showAllEmployees = false;
    private employeesWantsExtraShifts = false;
    private showSkills = false;
    private showEmployeeList = false;
    private showWorkRuleViolations = false;
    private showUnscheduledTasks = false;
    private showOrderList = false;
    private showDashboard = false;
    private dateToChanged = false;
    public nbrOfColumnsChanged = false;
    private loadingEmployees = false;
    private employeesLoaded = false;
    private employeePostsLoaded = false;
    private loadingShifts = false;
    private loadShiftsSilent = false;
    private loadingCycleTimes = false;
    public loadingGrossNetAndCost = false;
    private grossNetAndCostLoaded = false;
    private loadingStaffingNeed = false;
    private delayLoadStaffingNeed = false;
    private loadingAvailability = false;
    private loadPlanningPeriodSummary = false;
    private loadingPlanningPeriodSummary = false;
    private loadAnnualLeaveBalance = false;
    private loadingAnnualLeaveBalance = false;
    private recalculateEmployeeWorkTimes = false;
    private reloadShiftsForSpecifiedEmployeeIds: number[] = [];
    private evaluateAllWorkRulesAfterLoadingShifts = false;
    private loadingUnscheduledTasksAndDeliveries = false;
    private loadingUnscheduledOrders = false;

    // Terms
    public terms: { [index: string]: string; };
    private selectedViewLabel: string;
    private shiftDefined: string;
    private shiftUndefined: string;
    private shiftsDefined: string;
    private shiftsUndefined: string;
    private bookingDefined: string;
    private bookingUndefined: string;
    private bookingsDefined: string;
    private bookingsUndefined: string;

    // Properties
    private baseUrl: string;

    private get planningTab(): PlanningTabs {
        if (soeConfig.type == 'staffingneeds')
            return PlanningTabs.StaffingNeeds;
        else if (soeConfig.type == 'planning')
            return PlanningTabs.SchedulePlanning;

        return PlanningTabs.SchedulePlanning;
    }

    private planningMode: TimeSchedulePlanningMode = TimeSchedulePlanningMode.SchedulePlanning;
    private displayMode: TimeSchedulePlanningDisplayMode = TimeSchedulePlanningDisplayMode.Admin;
    private _viewDefinition: TermGroup_TimeSchedulePlanningViews = TermGroup_TimeSchedulePlanningViews.Schedule;
    public get viewDefinition(): TermGroup_TimeSchedulePlanningViews {
        return this._viewDefinition;
    }
    public set viewDefinition(viewDef: TermGroup_TimeSchedulePlanningViews) {
        var fromViewDef = this._viewDefinition;

        viewDef = parseInt(<any>viewDef, 10);

        this._viewDefinition = viewDef;
        this.viewDefinitionChanged(fromViewDef, this._viewDefinition);
    }

    public get isSchedulePlanningMode(): boolean {
        return this.planningMode === TimeSchedulePlanningMode.SchedulePlanning;
    }

    public get isOrderPlanningMode(): boolean {
        return this.planningMode === TimeSchedulePlanningMode.OrderPlanning;
    }

    private get isAdmin(): boolean {
        return this.displayMode === TimeSchedulePlanningDisplayMode.Admin;
    }

    private get isUser(): boolean {
        return this.displayMode === TimeSchedulePlanningDisplayMode.User;
    }

    public get isCalendarView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Calendar;
    }

    public get isDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Day;
    }

    public get isScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.Schedule;
    }

    public get isTemplateDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateDay;
    }

    public get isTemplateScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TemplateSchedule;
    }

    public get isTemplateView(): boolean {
        return this.isTemplateDayView || this.isTemplateScheduleView;
    }

    public get isEmployeePostDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay;
    }

    public get isEmployeePostScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule;
    }

    public get isEmployeePostView(): boolean {
        return this.isEmployeePostDayView || this.isEmployeePostScheduleView;
    }

    public get isScenarioDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioDay;
    }

    public get isScenarioScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;
    }

    public get isScenarioCompleteView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.ScenarioComplete;
    }

    public get isScenarioView(): boolean {
        return this.isScenarioDayView || this.isScenarioScheduleView || this.isScenarioCompleteView;
    }

    public get isStandbyDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbyDay;
    }

    public get isStandbyScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StandbySchedule;
    }

    public get isStandbyView(): boolean {
        return this.isStandbyDayView || this.isStandbyScheduleView;
    }

    public get isTasksAndDeliveriesDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay;
    }

    public get isTasksAndDeliveriesScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule;
    }

    public get isTasksAndDeliveriesView(): boolean {
        return this.isTasksAndDeliveriesDayView || this.isTasksAndDeliveriesScheduleView;
    }

    public get isStaffingNeedsDayView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay;
    }

    public get isStaffingNeedsScheduleView(): boolean {
        return this.viewDefinition === TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule;
    }

    public get isStaffingNeedsView(): boolean {
        return this.isStaffingNeedsDayView || this.isStaffingNeedsScheduleView;
    }

    public get isCommonDayView(): boolean {
        return this.isDayView || this.isTemplateDayView || this.isEmployeePostDayView || this.isScenarioDayView || this.isStandbyDayView || this.isTasksAndDeliveriesDayView || this.isStaffingNeedsDayView;
    }

    public get isCommonScheduleView(): boolean {
        return this.isScheduleView || this.isTemplateScheduleView || this.isEmployeePostScheduleView || this.isScenarioScheduleView || this.isScenarioCompleteView || this.isStandbyScheduleView || this.isTasksAndDeliveriesScheduleView || this.isStaffingNeedsScheduleView;
    }

    public get isHiddenEmployeeReadOnly(): boolean {
        return ((this.isDayView && !this.activeScheduleEditHiddenPermission) ||
            (this.isScheduleView && !this.activeScheduleEditHiddenPermission) ||
            (this.isTemplateView && !this.templateScheduleEditHiddenPermission) ||
            (this.isStandbyView && !this.standbyEditHiddenPermission));
    }

    private get isFollowUpOnHours(): boolean {
        return this.selectableInformationSettings.followUpCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours;
    }

    private get isFollowUpOnPercent(): boolean {
        return this.selectableInformationSettings.followUpCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent;
    }

    private get hasCutOrCopiedShifts(): boolean {
        return this.cutCopiedShifts.length > 0;
    }

    private getSelectedCutOrCopiedShifts(): ShiftDTO[] {
        return this.cutCopiedShifts.filter(s => s['selectedForPaste']);
    }

    private get hasSelectedCutOrCopiedShifts(): boolean {
        return this.getSelectedCutOrCopiedShifts().length > 0;
    }

    private hasUnscheduledTasks(date: Date): boolean {
        return CalendarUtility.includesDate(this.unscheduledTaskDates, date);
    }

    private hasScheduleEvents(date: Date): boolean {
        return CalendarUtility.includesDate(this.scheduleEventDates, date);
    }

    private hasHoliday(date: Date): boolean {
        return CalendarUtility.includesDate(this.holidays.map(h => h.date), date);
    }

    private getHolidayName(date: Date): string {
        const holiday = this.holidays.find(h => h.date.isSameDayAs(date));
        return holiday ? holiday.name : '';
    }

    private getHolidayDescription(date: Date): string {
        const holiday = this.holidays.find(h => h.date.isSameDayAs(date));
        if (!holiday)
            return '';

        return holiday.description ? holiday.description : holiday.name;
    }

    public getDateDay(date: Date): DateDay {
        return this.dates.find(d => d.date.isSameDayAs(date));
    }

    public isToday(date: Date): boolean {
        return CalendarUtility.getDateToday().isSameDayAs(date);
    }

    public isSaturday(date: Date): boolean {
        // Saturday
        if (date.getDay() === 6)
            return true;

        // Holiday not red day
        const holiday = this.holidays.find(h => h.date.isSameDayAs(date) && !h.isRedDay);
        if (holiday)
            return true;

        return false;
    }

    public isSunday(date: Date): boolean {
        // Sunday
        if (date.getDay() === 0)
            return true;

        // Holiday red day
        const holiday = this.holidays.find(h => h.date.isSameDayAs(date) && h.isRedDay);
        if (holiday)
            return true;

        return false;
    }

    private forceNoLoadData: boolean = false;
    private _dateFrom: Date;
    public get dateFrom(): Date {
        return this._dateFrom;
    }
    public set dateFrom(date: Date) {
        if (this.isCalendarView && !date.isBeginningOfWeek())
            date = date.beginningOfWeek();

        if (this._dateFrom && date && this._dateFrom.isSameHourAs(date))
            return;

        this._dateFrom = date;

        if (this.showWorkRuleViolations)
            this.toggleShowWorkRuleViolations(true);

        if (this.selectedVisibleDays !== TermGroup_TimeSchedulePlanningVisibleDays.Year)
            this.setDateRange(!this.forceNoLoadData && (!this.isCommonScheduleView || this.selectedVisibleDays !== TermGroup_TimeSchedulePlanningVisibleDays.Custom));
    }

    private _dateTo: Date;
    public get dateTo(): Date {
        return this._dateTo;
    }
    public set dateTo(date: Date) {
        if (date && !this.isCommonDayView)
            date = date.endOfDay();

        this._dateTo = date;

        let days = this.isCommonDayView ? 1 : date.beginningOfDay().diffDays(this.dateFrom.beginningOfDay()) + 1;
        if (days !== this.nbrOfVisibleDays)
            this.selectedVisibleDays = days;

        if (this.selectedVisibleDays == TermGroup_TimeSchedulePlanningVisibleDays.Custom)
            this._dateTo = this._dateTo.endOfDay();

        this.dateToChanged = !this.forceNoLoadData;

        if (this.isCommonScheduleView && this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Custom)
            this.setDateRange(!this.forceNoLoadData);
    }

    // Calendar
    private get nbrOfVisibleWeeks(): number {
        if (this.isCalendarView)
            return 6;
        else
            return this.nbrOfVisibleDays / 7;
    }

    private get weeks(): number[] {
        let wks: number[] = [];
        this.dates.forEach(date => {
            if (!wks.includes(date.date.week()))
                wks.push(date.date.week());
        });

        return wks;
    }

    private getDaysInWeek(week: number): DateDay[] {
        let days: DateDay[] = [];
        this.dates.forEach(date => {
            if (date.date.week() === week)
                days.push(date);
        });

        return days;
    }

    // Day
    public get startHour(): number {
        let hour = this.dayViewStartTime / 60;
        if (this.isBecomingDST)
            hour--;
        else if (this.isLeavingDST)
            hour++;

        return hour;
    }
    private get endHour(): number {
        let hour = this.dayViewEndTime / 60;
        if (this.isBecomingDST)
            hour--;
        else if (this.isLeavingDST)
            hour++;

        return hour;
    }
    private get nbrOfVisibleHours(): number {
        return this.endHour - this.startHour;
    }
    public get hourParts(): number {
        return this.isCommonDayView ? 60 / this.dayViewMinorTickLength : 1
    }

    private get isBecomingDST(): boolean {
        return !this.dateFrom.beginningOfDay().isDST() && this.dateFrom.beginningOfDay().addMinutes(this.dayViewStartTime).isDST();
    }

    private get isLeavingDST(): boolean {
        return this.dateFrom.beginningOfDay().isDST() && !this.dateFrom.beginningOfDay().addMinutes(this.dayViewStartTime).isDST();
    }

    // Schedule
    private _selectedVisibleDays: TermGroup_TimeSchedulePlanningVisibleDays;
    private get selectedVisibleDays(): TermGroup_TimeSchedulePlanningVisibleDays {
        return this._selectedVisibleDays;
    }
    private set selectedVisibleDays(days: TermGroup_TimeSchedulePlanningVisibleDays) {
        if (this._selectedVisibleDays === days)
            return;

        // If switching from year, set start to first day of current week
        if (this._selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Year && days !== TermGroup_TimeSchedulePlanningVisibleDays.Year)
            this.dateFrom = new Date().beginningOfWeek();

        this._selectedVisibleDays = days;

        this.setDateRange(!this.forceNoLoadData && days !== TermGroup_TimeSchedulePlanningVisibleDays.Custom);
    }
    public get nbrOfVisibleDays(): number {
        if (this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Custom)
            return this.dateTo.beginningOfDay().diffDays(this.dateFrom.beginningOfDay()) + 1;
        else
            return <number>this.selectedVisibleDays;
    }

    private dateColumnWidth = 100;

    private staffingNeedsSelection: TermGroup_StaffingNeedHeadsFilterType = TermGroup_StaffingNeedHeadsFilterType.ActualNeed;

    public staffingNeedsTotalSum = 0;
    public staffingNeedsFilteredSum = 0;
    public staffingNeedsShiftTypeSum: any[] = [];
    public staffingNeedsNeedSum = 0;

    // Planning chart
    public showPlanningAgChart = false;
    private planningAgChartData: any;

    // Planning follow up table
    public showPlanningFollowUpTable = false;
    public showPlanningFollowUpTableRows = false;
    public planningFollowUpTableData: any[] = [];

    // Staffing needs chart
    public showStaffingNeedsAgChart = true;

    // Dashboard
    private dashboardInitialized = false;
    private gaugeSalesValue = 0;
    public gaugeSalesLabel: string;
    private gaugeSalesThresholds: any = {};
    private gaugeHoursValue = 0;
    public gaugeHoursLabel: string;
    private gaugeHoursThresholds: any = {};
    private gaugeCostValue = 0;
    public gaugeCostLabel: string;
    private gaugeCostThresholds: any = {};
    private gaugeSalaryPercentValue = 0;
    public gaugeSalaryPercentLabel: string;
    private gaugeSalaryPercentThresholds: any = {};
    private gaugeLPATValue = 0;
    public gaugeLPATLabel: string;
    private gaugeLPATThresholds: any = {};
    private gaugeFPATValue = 0;
    public gaugeFPATLabel: string;
    private gaugeFPATThresholds: any = {};
    private gaugeBPATValue = 0;
    public gaugeBPATLabel: string;
    private gaugeBPATThresholds: any = {};

    // Context menus
    private scheduleViewContextMenuHandler: IContextMenuHandler;
    private employeeContextMenuHandler: IContextMenuHandler;
    private employeeListContextMenuHandler: IContextMenuHandler;
    private taskViewContextMenuHandler: IContextMenuHandler;

    // Misc
    private modalInstance: any;
    private autoCloseModalDelay = 4000;
    private editShiftModal: any;

    //@ngInject
    constructor(
        $uibModal,
        private $window,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private reportService: IReportService,
        private reportDataService: IReportDataService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private timeService: ITimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService,
        private $timeout: ng.ITimeoutService,
        private $interval: ng.IIntervalService) {
        super("Time.Schedule.Planning",
            (soeConfig.planningMode === 'order' ? Feature.Billing_Order_Planning : Feature.Time_Schedule_SchedulePlanning),
            $uibModal,
            translationService,
            messagingService,
            coreService,
            notificationService,
            urlHelperService);
        this.baseUrl = urlHelperService.getGlobalUrl('');
        this.employeeListItemUrl = urlHelperService.getGlobalUrl('Time/Schedule/Planning/Views/employeeListItem.html');
        this.unscheduledOrderListItemUrl = urlHelperService.getGlobalUrl('Time/Schedule/Planning/Views/unscheduledOrderListItem.html');

        this.selectableInformationSettings = new TimeSchedulePlanningSettingsDTO(true);

        // Init parameters
        this.employeeId = soeConfig.employeeId;
        this.employeeGroupId = soeConfig.employeeGroupId;
        if (soeConfig.planningMode === 'order') {
            this.planningMode = TimeSchedulePlanningMode.OrderPlanning;
            this.showOrderList = true;
        }

        this.modalInstance = $uibModal;
        this.setupWatchers();

        if (soeConfig.startDate) {
            // Date specified in query string
            this.dateFrom = CalendarUtility.convertToDate(soeConfig.startDate, "YYYYMMDD");
        } else {
            // Set today as default start date
            this.dateFrom = new Date().beginningOfDay();
        }

        this.minutesToTimeSpanFilter = $filter("minutesToTimeSpan");
        this.amountFilter = $filter("amount");

        this.scheduleHandler = new ScheduleHandler(this, this.$filter, this.$timeout, this.$interval, this.$q, this.$scope, this.$compile);
        this.templateHelper = new TemplateHelper(this, this.$q, this.$timeout, this.translationService, this.notificationService, this.scheduleService, this.sharedScheduleService);

        this.setupEvents();
    }

    // SETUP

    private setupEvents() {
        this.messagingService.subscribe(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, employeeIds => {
            this.reloadShiftsForSpecifiedEmployeeIds = employeeIds;
            this.loadData('EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE');
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE_BY_SHIFT_ID, shiftId => {
            let shift = this.scheduleHandler.getShiftById(shiftId);
            if (shift)
                this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
            this.loadData('EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE_BY_SHIFT_ID');
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_RELOAD_TEMPLATE_SCHEDULES, employeeIds => {
            // Reload employee, to get correct information about the templates
            this.reloadEmployees(employeeIds, false).then(() => {
                // Reload template for employee
                this.reloadShiftsForSpecifiedEmployeeIds = employeeIds;
                this.loadData('EVENT_RELOAD_TEMPLATE_SCHEDULES');
            });
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SAVE_SHIFTS, (data: any) => {
            if (this.isTemplateView || this.isEmployeePostView) {
                if (data.needToCalculateDayNumber) {
                    let template = this.templateHelper.getTemplateSchedules(data.employeeIdentifier).find(t => t.timeScheduleTemplateHeadId === data.timeScheduleTemplateHeadId);
                    if (template) {
                        data.shifts.forEach(shift => {
                            this.templateHelper.setDayNumberFromTemplate(shift, template);
                        });
                        data.activateDayNumber = data.shifts[0].dayNumber;
                    }
                }
                this.saveTemplateShifts(data.employeeIdentifier, data.timeScheduleTemplateHeadId, data.shifts, data.activateDayNumber, data.activateDates ? data.activateDates : null).then(success => {
                    if (success && this.editShiftModal)
                        this.editShiftModal.close();
                });
            } else {
                this.saveShifts(data.guid, data.shifts, true, false, false, 0, [data.employeeIdentifier]);
            }
            this.disableBreaksWithinHolesWarning = data.disableBreaksWithinHolesWarning;
            if (this.isTemplateView) {
                this.disableSaveAndActivateCheck = data.disableSaveAndActivateCheck;
                this.autoSaveAndActivate = data.autoSaveAndActivate;
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_UPDATE_ANNUAL_SCHEDULED_TIME, employeeId => {
            this.updateAnnualScheduledTime(employeeId);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_EDIT_ORDER, shift => {
            this.openEditOrder(shift, null, null);
        }, this.$scope);

        this.$scope.$on('onTabActivated', (e, a) => {
            if (a !== this.guid || this.tabActivated)
                return;

            this.tabActivated = true;
            this.loadLookups();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employeeList, (newValue, oldValue) => {
            this.$timeout(() => {
                this.scheduleHandler.enableDragAndDropOfEmployees();
            }, 200);
        });
        this.$scope.$watch(() => this.showEmployeeList, (newValue, oldValue) => {
            if (newValue === true) {
                this.$timeout(() => {
                    this.scheduleHandler.enableDragAndDropOfEmployees();
                }, 200);
            }
        });
        this.$scope.$watch(() => this.unscheduledTasks, () => {
            this.$timeout(() => {
                this.scheduleHandler.enableDragAndDropOfTasks();
            }, 200);
        });
        this.$scope.$watch(() => this.selectedUserSelectionId, (newValue, oldValue) => {
            if (oldValue && oldValue > 0 && newValue === 0) {
                this.clearFilters(false, false, true);
            }
        });
    }

    private loadLookups() {
        this.$q.all([
            this.loadTerms(),
            this.loadReadOnlyPermissions(),
            this.loadModifyPermissions()]).then(() => {
                if (!this.validateViewPermissions())
                    return;

                this.startWork("core.loading");
                this.loadCompanySettings().then(() => {
                    this.$q.all([
                        this.getEmploymentContractShortSubstituteReport(),
                        this.loadUserSettings(),
                        this.loadUserAndCompanysSettings()]).then(() => {
                            let queue = [];
                            queue.push(this.loadHasEmployeeTemplates());
                            queue.push(this.loadVisibleDays());
                            queue.push(this.loadIntervals());
                            if (this.useAccountHierarchy)
                                queue.push(this.loadAccountsByUserFromHierarchy());
                            else
                                queue.push(this.loadCategories(false));
                            queue.push(this.loadAccountDims(false));
                            queue.push(this.loadShiftTypeAccountDim());
                            queue.push(this.loadShiftTypes());
                            queue.push(this.loadUserShiftTypes(false));
                            queue.push(this.loadTimeScheduleTypes());
                            queue.push(this.loadDeviationCauses());
                            queue.push(this.loadShiftStyles());
                            queue.push(this.loadBreakTimeCodes());
                            if (this.useLeisureCodes)
                                queue.push(this.loadLeisureCodes());
                            if (this.useAnnualLeave)
                                queue.push(this.loadAbsenceTypes());
                            queue.push(this.loadHeadStatuses());
                            queue.push(this.loadStaffingNeedsFilterTypes());
                            queue.push(this.loadTimeScheduleTaskTypes());
                            queue.push(this.loadFrequencyTasks());
                            this.$q.all(queue).then(() => {
                                this.setupBlockTypesFilter();
                                this.setShiftTypes();
                                this.setupToolBar();
                                this.setupContextMenus();
                                this.setupTaskTypes();
                                this.setupWeekdays();

                                if (this.employeesLoaded) {
                                    this.delayFilterByUserSelection = false;
                                    if (this.selectedUserSelection)
                                        this.filterByUserSelection();
                                }

                                if (this.unscheduledTaskDates.length === 0)
                                    this.loadUnscheduledTasksAndDeliveriesDates();
                                if (this.allUnscheduledOrders.length === 0)
                                    this.loadAllUnscheduledOrders();
                                if (this.scheduleEventDates.length === 0)
                                    this.loadScheduleEventDates();
                                if (this.holidays.length === 0)
                                    this.loadHolidays();
                            })
                        });
                });
            });
    }

    private validateViewPermissions(): boolean {
        this.viewPermissionsLoaded = true;

        if (!this.calendarViewPermission &&
            !this.dayViewPermission &&
            !this.scheduleViewPermission &&
            !this.templateDayViewPermission &&
            !this.templateScheduleViewPermission &&
            !this.employeePostDayViewPermission &&
            !this.employeePostScheduleViewPermission &&
            !this.scenarioDayViewPermission &&
            !this.scenarioScheduleViewPermission &&
            !this.standbyDayViewPermission &&
            !this.standbyScheduleViewPermission) {

            // No permission for any view
            this.noViewPermission = true;
            return false;
        }

        return true
    }

    private setupToolBar() {

        // Functions
        if (this.isSchedulePlanningMode) {
            // Tasks and deliveries
            this.functions.push({ id: StaffingNeedsFunctions.PrintTasksAndDeliveries, name: this.terms["time.schedule.staffingneeds.planning.functions.printtasksanddeliveries"], icon: "fal fa-print", hidden: () => { return !this.isTasksAndDeliveriesView || !this.reportPermission }, disabled: () => { return !this.visibleTasks } });

            // Staffing needs
            this.functions.push({ id: StaffingNeedsFunctions.AddNeed, name: this.terms["time.schedule.staffingneeds.planning.functions.add"], icon: "fal fa-fw fa-plus", hidden: () => { return !this.isStaffingNeedsView } });
            this.functions.push({ id: StaffingNeedsFunctions.ReloadNeed, name: this.terms["time.schedule.staffingneeds.planning.functions.reload"], icon: "fal fa-fw fa-sync", hidden: () => { return !this.isStaffingNeedsView }, disabled: () => { return !this.heads } });

            // Schedule planning
            this.functions.push({ id: SchedulePlanningFunctions.NewTemplates, name: this.terms["time.schedule.planning.contextmenu.newtemplate"], icon: "fal fa-fw fa-plus", hidden: () => { return !this.showFunctionNewTemplate; } });

            if (this.placementPermission) {
                this.functions.push({ hidden: () => { return !this.showFunctionNewTemplate; } });
                this.functions.push({ id: SchedulePlanningFunctions.OpenActivateSchedule, name: this.terms["time.schedule.planning.contextmenu.activate"], icon: "fal fa-fw fa-calendar-check", hidden: () => { return !this.showFunctionNewTemplate; } });
            }

            this.functions.push({ hidden: () => { return !this.showFunctionAddShift || !this.showFunctionNewTemplate; } });
            this.functions.push({ id: SchedulePlanningFunctions.AddShift, name: this.terms["time.schedule.planning.editshift.addnew"], icon: "fal fa-fw fa-plus", hidden: () => { return !this.showFunctionAddShift; } });

            this.functions.push({ hidden: () => { return !this.showFunctionEditBreaks && !this.showFunctionEditShifts && !this.showFunctionEditTemplateBreaks; } });
            this.functions.push({ id: SchedulePlanningFunctions.EditBreaks, name: this.terms["time.schedule.planning.editmode.breaks"], icon: "fal fa-fw fa-mug-hot", hidden: () => { return !this.showFunctionEditBreaks; } });
            this.functions.push({ id: SchedulePlanningFunctions.EditShifts, name: this.terms["time.schedule.planning.editmode.shifts"], icon: "fal fa-fw fa-calendar", hidden: () => { return !this.showFunctionEditShifts; } });
            this.functions.push({ id: SchedulePlanningFunctions.EditTemplateBreaks, name: this.terms["time.schedule.planning.editmode.templatebreaks"], icon: "fal fa-fw fa-mug-hot", hidden: () => { return !this.showFunctionEditTemplateBreaks; } });

            this.functions.push({ hidden: () => { return !this.showFunctionEmployeeList } });
            this.functions.push({ id: SchedulePlanningFunctions.ShowEmployeeList, name: this.terms["time.schedule.planning.showemployeelist"], icon: "fal fa-fw fa-user", hidden: () => { return !this.showFunctionEmployeeList || this.showEmployeeList; } });
            this.functions.push({ id: SchedulePlanningFunctions.HideEmployeeList, name: this.terms["time.schedule.planning.hideemployeelist"], icon: "fal fa-fw fa-user-times", hidden: () => { return !this.showFunctionEmployeeList || !this.showEmployeeList; } });

            if (this.scenarioDayViewPermission || this.scenarioScheduleViewPermission) {
                this.functions.push({ hidden: () => { return !this.showFunctionAddScenario && !this.showFunctionDeleteScenario && !this.showFunctionActivateScenario && !this.showFunctionCreateTemplateFromScenario } });
                this.functions.push({ id: SchedulePlanningFunctions.AddScenario, name: this.terms["time.schedule.planning.scenario.new"], icon: "fal fa-fw fa-calendar-plus", hidden: () => { return !this.showFunctionAddScenario; } });
                this.functions.push({ id: SchedulePlanningFunctions.DeleteScenario, name: this.terms["time.schedule.planning.scenario.delete"], icon: "fal fa-fw fa-calendar-times iconDelete", hidden: () => { return !this.showFunctionDeleteScenario; } });
                this.functions.push({ id: SchedulePlanningFunctions.ActivateScenario, name: this.terms["time.schedule.planning.scenario.activate"], icon: "fal fa-fw fa-calendar-check", hidden: () => { return !this.showFunctionActivateScenario; } });
                this.functions.push({ id: SchedulePlanningFunctions.CreateTemplateFromScenario, name: this.terms["time.schedule.planning.scenario.createtemplate"], icon: "fal fa-fw fa-calendar-plus", hidden: () => { return !this.showFunctionCreateTemplateFromScenario; } });
            }
        }

        if (this.isOrderPlanningMode) {
            this.functions.push({ hidden: () => { return !this.showFunctionAddOrder; } });
            this.functions.push({ id: SchedulePlanningFunctions.AddOrder, name: this.terms["time.schedule.planning.contextmenu.neworder"], icon: "fal fa-fw fa-plus", hidden: () => { return !this.showFunctionAddOrder; } });
        }

        if (this.showDashboardPermission) {
            this.functions.push({ hidden: () => { return !this.showFunctionDashboard } });
            this.functions.push({ id: SchedulePlanningFunctions.ShowDashboard, name: this.terms["time.schedule.planning.showdashboard"], icon: "fal fa-fw fa-tachometer-alt", hidden: () => { return !this.showFunctionDashboard || this.showDashboard; } });
            this.functions.push({ id: SchedulePlanningFunctions.HideDashboard, name: this.terms["time.schedule.planning.hidedashboard"], icon: "fal fa-fw fa-tachometer-alt", hidden: () => { return !this.showFunctionDashboard || !this.showDashboard; } });
        }

        if (this.preliminaryPermission) {
            this.functions.push({ hidden: () => { return !this.showFunctionPrelDef } });
            this.functions.push({ id: SchedulePlanningFunctions.PrelToDef, name: this.terms["time.schedule.planning.buttonfunctions.preltodef"], icon: "fal fa-fw fa-calendar-check", hidden: () => { return !this.showFunctionPrelDef; } });
            this.functions.push({ id: SchedulePlanningFunctions.DefToPrel, name: this.terms["time.schedule.planning.buttonfunctions.deftoprel"], icon: "fal fa-fw fa-calendar", hidden: () => { return !this.showFunctionPrelDef; } });
        }

        if (this.useLeisureCodes) {
            this.functions.push({ hidden: () => { return !this.showFunctionAllocateLeisureCodes } });
            this.functions.push({ id: SchedulePlanningFunctions.AllocateLeisureCodes, name: this.terms["time.schedule.planning.buttonfunctions.allocateleisurecodes"], icon: "fal fa-fw fa-calendar-plus", hidden: () => { return !this.showFunctionAllocateLeisureCodes; } });
            this.functions.push({ id: SchedulePlanningFunctions.DeleteLeisureCodes, name: this.terms["time.schedule.planning.buttonfunctions.deleteleisurecodes"], icon: "fal fa-fw fa-calendar-minus iconDelete", hidden: () => { return !this.showFunctionAllocateLeisureCodes; } });
        }

        if (this.useAnnualLeave) {
            this.functions.push({ hidden: () => { return !this.showFunctionRecalculateAnnualLeaveBalances } });
            this.functions.push({ id: SchedulePlanningFunctions.RecalculateAnnualLeaveBalances, name: this.terms["time.schedule.planning.annualleave.balance.recalculate"], icon: "fal fa-fw fa-balance-scale", hidden: () => { return !this.showFunctionRecalculateAnnualLeaveBalances; } });
        }

        if (this.isSchedulePlanningMode) {
            if (this.copySchedulePermission) {
                this.functions.push({ hidden: () => { return !this.showFunctionCopySchedule; } });
                this.functions.push({ id: SchedulePlanningFunctions.CopySchedule, name: this.terms["time.schedule.planning.copyschedule.title"], icon: "fal fa-fw fa-clone", hidden: () => { return !this.showFunctionCopySchedule; } });
            }
            if (this.restoreToSchedulePermission) {
                this.functions.push({ hidden: () => { return !this.showFunctionRestoreToSchedule && !this.showFunctionRemoveAbsence; } });
                this.functions.push({ id: SchedulePlanningFunctions.RestoreToSchedule, name: this.terms["time.schedule.planning.editshift.functions.restoretoschedule"], icon: "fal fa-fw fa-undo warningColor", hidden: () => { return !this.showFunctionRestoreToSchedule; } });
                this.functions.push({ id: SchedulePlanningFunctions.RemoveAbsenceInScenario, name: this.terms["time.schedule.planning.editshift.functions.removeabsence"], icon: "fal fa-fw fa-undo warningColor", hidden: () => { return !this.showFunctionRemoveAbsence; } });
            }
            if (this.reportPermission) {
                this.functions.push({ hidden: () => { return !this.showFunctionPrintSchedule && !this.showFunctionPrintTemplateSchedule && !this.showFunctionPrintEmployeePostTemplateSchedule && !this.showFunctionPrintScenarioSchedule && !this.showFunctionPrintEmploymentContract && !this.showFunctionExportToExcel; } });
                this.functions.push({ id: SchedulePlanningFunctions.PrintSchedule, name: this.terms["time.schedule.planning.buttonfunctions.printschedule"], icon: "fal fa-fw fa-print", hidden: () => { return !this.showFunctionPrintSchedule; } });
                this.functions.push({ id: SchedulePlanningFunctions.PrintTemplateSchedule, name: this.terms["time.schedule.planning.buttonfunctions.printtemplateschedule"], icon: "fal fa-fw fa-print", hidden: () => { return !this.showFunctionPrintTemplateSchedule; } });
                this.functions.push({ id: SchedulePlanningFunctions.PrintEmployeePostTemplateSchedule, name: this.terms["time.schedule.planning.buttonfunctions.printemployeeposttemplateschedule"], icon: "fal fa-fw fa-print", hidden: () => { return !this.showFunctionPrintEmployeePostTemplateSchedule; } });
                this.functions.push({ id: SchedulePlanningFunctions.PrintScenarioSchedule, name: this.terms["time.schedule.planning.buttonfunctions.printscenarioschedule"], icon: "fal fa-fw fa-print", hidden: () => { return !this.showFunctionPrintScenarioSchedule; } });
                this.functions.push({ id: SchedulePlanningFunctions.PrintEmploymentCertificate, name: this.terms["time.schedule.planning.employee.contextmenu.printemploymentcertificate"], icon: "fal fa-fw fa-print", hidden: () => { return !this.showFunctionPrintEmploymentContract; } });
                this.functions.push({ id: SchedulePlanningFunctions.SendEmploymentCertificate, name: this.terms["time.schedule.planning.employee.contextmenu.sendemploymentcertificate"], icon: "fal fa-fw fa-envelope", hidden: () => { return !this.showFunctionPrintEmploymentContract; } });
                this.functions.push({ id: SchedulePlanningFunctions.ExportToExcel, name: this.terms["core.exportexcel"], icon: "fal fa-fw fa-file-excel", hidden: () => { return !this.showFunctionExportToExcel; } });
            }

            this.functions.push({ hidden: () => { return !this.showFunctionEvaluateAllWorkRules && !this.showFunctionsForEmployeePostSchedule; } });
            this.functions.push({ id: SchedulePlanningFunctions.EvaluateAllWorkRules, name: this.terms["time.schedule.planning.evaluateworkrules"], icon: "fal fa-fw fa-user-clock", hidden: () => { return !this.showFunctionEvaluateAllWorkRules; } });
            this.functions.push({ id: SchedulePlanningFunctions.CreateEmptyScheduleForEmployeePosts, name: this.terms["time.schedule.planning.createemptyscheduleforemployeeposts"], icon: "fal fa-fw fa-calendar", hidden: () => { return !this.showFunctionsForEmployeePostSchedule; } });
            this.functions.push({ id: SchedulePlanningFunctions.RegenerateScheduleForEmployeePost, name: this.terms["time.schedule.planning.generatescheduleforemployeeposts"], icon: "fal fa-fw fa-calendar-plus", hidden: () => { return !this.showFunctionsForEmployeePostSchedule; } });
            this.functions.push({ id: SchedulePlanningFunctions.DeleteScheduleForEmployeePost, name: this.terms["time.schedule.planning.deletescheduleforemployeeposts"], icon: "fal fa-fw fa-calendar-times iconDelete", hidden: () => { return !this.showFunctionsForEmployeePostSchedule; } });

            if (this.showUnscheduledTasksPermission) {
                this.functions.push({ hidden: () => { return !this.showFunctionUnscheduledTasks; } });
                this.functions.push({ id: SchedulePlanningFunctions.ShowUnscheduledTasks, name: this.terms["time.schedule.planning.showunscheduledtasks"], icon: "fal fa-fw fa-calendar", hidden: () => { return !this.showFunctionUnscheduledTasks || this.showUnscheduledTasks } });
                this.functions.push({ id: SchedulePlanningFunctions.HideUnscheduledTasks, name: this.terms["time.schedule.planning.hideunscheduledtasks"], icon: "fal fa-fw fa-calendar-times", hidden: () => { return !this.showFunctionUnscheduledTasks || !this.showUnscheduledTasks; } });
            }
        }

        if (!this.isTasksAndDeliveriesView && !this.isStaffingNeedsView && !this.hasEmployeesLoaded && !this.loadingEmployees)
            this.loadEmployees();
    }

    private get showFunctionNewTemplate(): boolean {
        return (this.isTemplateView && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionAddShift(): boolean {
        return ((this.isDayView || this.isScheduleView || (this.isScenarioView && this.scenarioHead)) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionEditBreaks(): boolean {
        return (this.editMode !== PlanningEditModes.Breaks && this.isCommonDayView && this.hasCurrentViewModifyPermission && !this.isStandbyDayView && !this.isTasksAndDeliveriesDayView && !this.isStaffingNeedsDayView);
    }

    private get showFunctionEditTemplateBreaks(): boolean {
        return (this.editMode !== PlanningEditModes.TemplateBreaks && (this.isDayView || this.isScenarioDayView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionEditShifts(): boolean {
        return (this.editMode !== PlanningEditModes.Shifts && (this.isDayView || this.isTemplateDayView || this.isScenarioDayView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionEmployeeList(): boolean {
        return (this.isSchedulePlanningMode && (this.isDayView || this.isScheduleView || this.isEmployeePostDayView || this.isEmployeePostScheduleView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionAddOrder(): boolean {
        return (this.isOrderPlanningMode && (this.isDayView || this.isScheduleView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionDashboard(): boolean {
        return (this.isDayView || this.isScheduleView || this.isTemplateView);
    }

    private get showFunctionPrelDef(): boolean {
        return ((this.isDayView || this.isScheduleView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionAllocateLeisureCodes(): boolean {
        return (this.isScheduleView && this.hasCurrentViewModifyPermission && this.visibleEmployees.filter(e => !e.hidden).length > 0);
    }

    private get showFunctionRecalculateAnnualLeaveBalances(): boolean {
        return ((this.isScheduleView || this.isDayView) && this.hasCurrentViewModifyPermission && this.selectableInformationSettings.showAnnualLeaveBalance && this.visibleEmployees.filter(e => !e.hidden).length > 0);
    }

    private get showFunctionCopySchedule(): boolean {
        return (this.isSchedulePlanningMode && (this.isDayView || this.isScheduleView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionRestoreToSchedule(): boolean {
        return (this.isSchedulePlanningMode && (this.isDayView || this.isScheduleView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionRemoveAbsence(): boolean {
        return this.hasCurrentViewModifyPermission && this.existingScenarioSelected;
    }

    private get showFunctionPrintSchedule(): boolean {
        return ((this.isDayView || this.isScheduleView) && this.shifts.length > 0);
    }

    private get showFunctionPrintTemplateSchedule(): boolean {
        return (this.isTemplateView && this.shifts.length > 0);
    }

    private get showFunctionPrintEmployeePostTemplateSchedule(): boolean {
        return (this.isEmployeePostView && this.shifts.length > 0);
    }

    private get showFunctionPrintScenarioSchedule(): boolean {
        return (this.isScenarioView && this.shifts.length > 0);
    }

    private get showFunctionPrintEmploymentContract(): boolean {
        return ((this.employmentContractShortSubstituteReportId || this.hasEmployeeTemplates) && (this.isDayView || this.isScheduleView) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionExportToExcel(): boolean {
        return ((this.isDayView || this.isScheduleView || this.isStandbyView) && this.visibleShifts.length > 0);
    }

    private get showFunctionUnscheduledTasks(): boolean {
        return (!this.isScenarioView && !this.isCalendarView && !this.isStandbyView && !this.isTasksAndDeliveriesView && !this.isStaffingNeedsView && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionEvaluateAllWorkRules(): boolean {
        return ((this.isScheduleView || this.isTemplateScheduleView || ((this.isScenarioScheduleView || this.isScenarioCompleteView) && this.timeScheduleScenarioHeadId)) && this.hasCurrentViewModifyPermission);
    }

    private get showFunctionsForEmployeePostSchedule(): boolean {
        return this.isEmployeePostView && this.getFilteredEmployeePostIds().length > 0;
    }

    private get existingScenarioSelected(): boolean {
        return this.scenarioHead && !!this.timeScheduleScenarioHeadId;
    }

    private get showFunctionAddScenario(): boolean {
        return (((this.isDayView || this.isTemplateDayView || this.isScenarioDayView) && this.scenarioDayViewPermission) || ((this.isScheduleView || this.isTemplateScheduleView || this.isScenarioScheduleView || this.isScenarioCompleteView) && this.scenarioScheduleViewPermission));
    }

    private get showFunctionDeleteScenario(): boolean {
        return this.hasCurrentViewModifyPermission && this.existingScenarioSelected;
    }

    private get showFunctionActivateScenario(): boolean {
        return this.placementPermission && this.existingScenarioSelected;
    }

    private get showFunctionCreateTemplateFromScenario(): boolean {
        return ((this.isScenarioDayView && this.templateDayViewModifyPermission) || ((this.isScenarioScheduleView || this.isScenarioCompleteView) && this.templateScheduleViewModifyPermission)) && this.existingScenarioSelected;
    }

    private setupContextMenus() {
        this.scheduleViewContextMenuHandler = this.contextMenuHandlerFactory.create();
        this.employeeContextMenuHandler = this.contextMenuHandlerFactory.create();
        this.employeeListContextMenuHandler = this.contextMenuHandlerFactory.create();
        this.taskViewContextMenuHandler = this.contextMenuHandlerFactory.create();
    }

    private getSlotContextMenuOptions(employeeId: number, formattedDate: string): any[] {
        // Context menu for empty slot

        // Get clicked slot
        let slot = this.createSlot(formattedDate, employeeId);
        this.setSlotReadOnly(slot);

        return this.createScheduleViewContextMenuOptions(null, slot);
    }

    private getTaskSlotContextMenuOptions(type: SoeStaffingNeedsTaskType, formattedDate: string): any[] {
        // Context menu for empty slot

        // Get clicked slot
        let slot = this.createSlot(formattedDate);

        return this.createTaskViewContextMenuOptions(type, null, slot);
    }

    private getShiftContextMenuOptions(id: string): any[] {
        // Context menu for shift/leisure code

        let shift: ShiftDTO;
        if (id?.startsWith("lc_")) {
            // Get clicked leisure code
            shift = this.scheduleHandler.getLeisureCodeById(id);
        } else if (id?.startsWith("ar_")) {
            // Get clicked absence request
            let parts = id.split('_');
            shift = this.scheduleHandler.getShiftById(parseInt(parts[1], 10), true, true);
        } else {
            // Get clicked shift
            shift = this.scheduleHandler.getShiftById(parseInt(id, 10), false, true);
        }

        return this.createScheduleViewContextMenuOptions(shift, null);
    }

    private getEmployeeContextMenuOptions(id: number): any[] {
        // Context menu for employee

        if (id) {
            // Get clicked employee
            let employee: EmployeeListDTO = this.isEmployeePostView ? this.getEmployeePostById(id) : this.getEmployeeById(id);

            return this.createEmployeeContextMenuOptions(employee);
        }
        return [] as any;
    }

    private getEmployeeListContextMenuOptions(employee: EmployeeRightListDTO): any[] {
        // Context menu for employee list

        return this.createEmployeeListContextMenuOptions(employee);
    }

    private getTaskContextMenuOptions(taskId: string, formattedDate: string): any[] {
        // Context menu for task

        // Get clicked slot
        let slot = new SlotDTO();
        slot.startTime = formattedDate.parsePipedDateTime();
        slot.stopTime = this.isCommonDayView ? slot.startTime.addMinutes(60 / this.hourParts) : slot.startTime.endOfDay();

        // Get clicked task
        let task: StaffingNeedsTaskDTO = this.scheduleHandler.getTaskById(taskId);

        return this.createTaskViewContextMenuOptions(task.type, task, slot);
    }

    private createScheduleViewContextMenuOptions(shift: ShiftDTO, slot: SlotDTO): any[] {
        if (!this.scheduleViewContextMenuHandler || (this.isOrderPlanningMode && shift && !shift.isOrder && !shift.isBooking))
            return [];

        // If right clicking on a shift that is not selected, unselect all other
        if (!shift || !shift.selected)
            this.scheduleHandler.clearSelectedShifts();

        // Make sure linked shifts are selected
        if (shift)
            this.scheduleHandler.selectShift(shift);
        let selectedShifts = this.scheduleHandler.getSelectedShifts();

        // If right clicking a shift that does not belong to same employee as already selected shift,
        // it has not yet been unselected. Clear all selected shifts and reselect the current one.
        if (selectedShifts.length > 1 && shift) {
            for (let s of selectedShifts) {
                if (s.employeeId !== shift.employeeId) {
                    this.scheduleHandler.clearSelectedShifts();
                    this.scheduleHandler.selectShift(shift);
                    selectedShifts = this.scheduleHandler.getSelectedShifts();
                    break;
                }
            }
        }

        // If right clicking an empty slot, previously selected shifts will not be unselected
        if (selectedShifts.length > 0 && !shift && slot) {
            this.scheduleHandler.clearSelectedShifts();
            selectedShifts = [];
        }

        // Get info from slot (or shift)
        let date: Date;
        let employeeId: number;
        let slotFromDate: Date;
        let slotToDate: Date;
        let slotValidRange: boolean = true;
        if (slot) {
            employeeId = slot.employeeId;
            date = slot.startTime.date();

            let slots = this.scheduleHandler.getSelectedSlots();
            if (slots.length > 1) {
                // Check for holes, only support multiple slots if selected in range
                let prevDate: Date;
                slots.forEach(s => {
                    if (prevDate && !prevDate.addDays(1).isSameDayAs(s.startTime))
                        slotValidRange = false;
                    prevDate = s.startTime.date();
                });
                if (slotValidRange) {
                    slotFromDate = slots[0].startTime.date();
                    slotToDate = _.last(slots).startTime.date();
                }
            }
        } else if (shift) {
            date = shift.startTime;
            employeeId = shift.employeeId;
        }

        let selectedShiftsIsAbsenceRequest = selectedShifts.filter(s => s.isAbsenceRequest).length > 0;
        let selectedShiftsIsNotSchedule = selectedShifts.filter(s => !s.isSchedule).length > 0;
        let selectedShiftsAllStandby = selectedShifts.filter(s => !s.isStandby).length === 0;
        let onlyOneShiftSelected = selectedShifts.length <= 1;
        let isAbsence = shift?.isAbsence;
        let isAbsenceRequest = shift?.isAbsenceRequest;
        let isBooking = shift?.isBooking;
        let isLeisureCode = shift?.isLeisureCode;
        let isAnnualLeave = shift?.isAnnualLeave;
        let isReadOnly = (shift?.isReadOnly || slot?.isReadOnly || (employeeId === this.hiddenEmployeeId && this.isHiddenEmployeeReadOnly));
        let employeeHasAnnualLeaveGroup = this.hasAnnualLeaveGroupById(employeeId);

        // If clicked shift does not exist in collection of selected shifts, unselect all selected shifts
        if (shift) {
            if ((!isLeisureCode && !selectedShifts.map(s => s.timeScheduleTemplateBlockId).includes(shift.timeScheduleTemplateBlockId)) ||
                (isLeisureCode && !selectedShifts.map(s => s.timeScheduleEmployeePeriodDetailId).includes(shift.timeScheduleEmployeePeriodDetailId))) {
                this.scheduleHandler.clearSelectedShifts();
                selectedShifts = [];
            }
        }

        // Check if selected slot is a NoTemplate-slot
        let noTemplateSlot = false;
        if (slot && this.isSchedulePlanningMode && (this.isTemplateView || this.isEmployeePostView)) {
            let employee = this.isEmployeePostView ? this.getEmployeePostById(slot.employeeId) : this.getEmployeeById(slot.employeeId);
            if (employee && !employee.hasTemplateSchedule(slot.startTime.date()))
                noTemplateSlot = true;
        }

        let multipleDays = _.uniq(selectedShifts.map(s => s.actualStartDate.toFormattedDate())).length > 1;

        this.scheduleViewContextMenuHandler.clearContextMenuItems();

        if (this.isAdmin && !this.isStandbyView) {
            if (this.isSchedulePlanningMode && !isReadOnly) {
                // Add shift only in schedule planning
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.newshift"], 'fa-plus', ($itemScope, $event, modelValue) => { this.editShift(null, date, employeeId, false, false); }, () => { return !noTemplateSlot; });
            }

            if (shift?.isSchedule && !isLeisureCode && !isAnnualLeave) {
                // Edit shift
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.editshift"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.editShift(shift, null, null, false, false); }, () => { return shift.isSchedule; });

                // Delete shift
                if (!isReadOnly && !this.isOrderPlanningMode) {
                    // Can't delete shifts in order planning
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.deleteshift"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                        if (selectedShifts.length > 0)
                            this.deleteShifts(selectedShifts);
                        else
                            this.deleteShift(shift);
                    }, () => { return shift.isSchedule && !isAbsenceRequest; });
                }
            }
        }

        // Order planning
        if (this.isOrderPlanningMode && !isReadOnly) {
            if (shift?.isOrder) {
                // Edit assignment
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.editassignment.edit"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.openEditAssignment(shift); }, () => { return true; });

                // Delete assignment
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.editassignment.delete"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                    if (selectedShifts.length > 0)
                        this.deleteShifts(selectedShifts);
                    else
                        this.deleteShift(shift);
                }, () => { return true; });
            }

            // Add order
            this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.neworder"], 'fa-plus', ($itemScope, $event, modelValue) => { this.openEditOrder(null, date, employeeId); }, () => { return true; });

            // Edit order
            if (shift?.isOrder)
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.editorder"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.openEditOrder(shift, date, employeeId); }, () => { return shift.order; });
        }

        // Booking
        if (this.bookingModifyPermission && !isReadOnly && !this.isEmployeePostView && !this.isStandbyView) {
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();

            // Add booking
            this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.newbooking"], 'fa-plus', ($itemScope, $event, modelValue) => { this.editBooking(null, date, employeeId); }, () => { return true; });

            if (shift?.isBooking) {
                // Edit booking
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.editbooking"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.editBooking(shift, null, null); }, () => { return shift.isBooking; });

                // Delete booking
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.deletebooking"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                    if (selectedShifts.length > 0)
                        this.deleteShifts(selectedShifts);
                    else
                        this.deleteShift(shift);
                }, () => { return shift.isBooking; });
            }
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();
        }

        // Standby
        if (this.standbyShiftsModifyPermission && !isReadOnly && (this.isDayView || this.isScheduleView || this.isTemplateView || this.isStandbyView)) {
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();

            if (!this.isStandbyView) {
                // Add stand by shift
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.newstandbyshift"], 'fa-plus', ($itemScope, $event, modelValue) => { this.editShift(null, date, employeeId, true, false); }, () => { return true; });
            }

            if (shift?.isStandby) {
                // Edit stand by shift
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.editstandbyshift"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.editShift(shift, null, null, shift.isStandby, shift.isOnDuty); }, () => { return shift.isStandby; });

                if (!this.isStandbyView) {
                    // Delete stand by shift
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.deletestandbyshift"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                        if (selectedShifts.length > 0)
                            this.deleteShifts(selectedShifts);
                        else
                            this.deleteShift(shift);
                    }, () => { return shift.isStandby; });
                }
            }
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();
        }

        // OnDuty
        if (this.onDutyShiftsModifyPermission && !isReadOnly && (this.isDayView || this.isScheduleView || this.isTemplateView)) {
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();

            // Add on duty shift
            this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.newondutyshift"], 'fa-plus', ($itemScope, $event, modelValue) => { this.editShift(null, date, employeeId, false, true); }, () => { return true; });

            if (shift?.isOnDuty) {
                // Edit on duty shift
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.editondutyshift"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.editShift(shift, null, null, shift.isStandby, shift.isOnDuty); }, () => { return shift.isOnDuty; });

                // Delete on duty shift
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.deleteondutyshift"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                    if (selectedShifts.length > 0)
                        this.deleteShifts(selectedShifts);
                    else
                        this.deleteShift(shift);
                }, () => { return shift.isOnDuty; });
            }
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();
        }

        // Leisure codes
        if (this.useLeisureCodes && this.leisureCodes.length > 0 && !isReadOnly && (this.isDayView || this.isScheduleView) && employeeId !== this.hiddenEmployeeId) {
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();

            // Check if leisure code already exists on current day
            const dayContainsLeisureCode = this.shifts.filter(s => s.employeeId === employeeId && s.isLeisureCode && s.startTime.isSameDayAs(date)).length > 0;

            // Add leisure code
            if (!dayContainsLeisureCode)
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.newleisurecode"], 'fa-plus', ($itemScope, $event, modelValue) => { this.editLeisureCode(null, date, employeeId); }, () => { return true; });

            if (isLeisureCode) {
                // Edit leisure code
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.editleisurecode"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.editLeisureCode(shift, null, null); }, () => { return isLeisureCode; });

                // Delete leisure code
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.deleteleisurecode"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                    if (selectedShifts.length > 0)
                        this.deleteLeisureCodes(selectedShifts);
                    else
                        this.deleteLeisureCode(shift);
                }, () => { return isLeisureCode; });
            }
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();
        }

        if (this.isAdmin) {
            // Edit availability
            if (this.isSchedulePlanningMode && (this.isScheduleView || this.isDayView) && (this.viewAvailabilityPermission || this.editAvailabilityPermission)) {
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.editAvailabilityPermission ? this.terms["common.dashboard.myschedule.availability.edit"] : this.terms["common.dashboard.myschedule.availability.show"], 'fa-calendar-check', ($itemScope, $event, modelValue) => {
                    if (!slotValidRange) {
                        this.translationService.translateMany(["time.schedule.planning.slot.invalidrange.title", "time.schedule.planning.slot.invalidrange.message"]).then(terms => {
                            this.notificationService.showDialogEx(terms["time.schedule.planning.slot.invalidrange.title"], terms["time.schedule.planning.slot.invalidrange.message"], SOEMessageBoxImage.Forbidden);
                        });
                    } else {
                        this.editAvailability(slotFromDate || date, slotToDate || date, employeeId);
                    }
                }, () => { return true; });
            }

            // Change employee
            if (!isReadOnly && !this.isEmployeePostView && selectedShifts.length > 0 && (!this.isStandbyView || (shift?.isStandby)) && !isLeisureCode && !isAnnualLeave) {
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.changeemployee"], 'fa-people-arrows', ($itemScope, $event, modelValue) => { this.changeEmployee(); }, () => { return !multipleDays && !selectedShiftsIsAbsenceRequest && (!selectedShiftsIsNotSchedule || selectedShiftsAllStandby); });
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();
            }

            // Cut, copy, paste only in schedule planning
            if (this.isSchedulePlanningMode && !this.isEmployeePostView && !this.isStandbyView && !isReadOnly && !isAbsence && !isAbsenceRequest && !(shift?.isOnDuty) && !isLeisureCode && !isAnnualLeave) {
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();

                // Cut
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.cut"], 'fa-cut', ($itemScope, $event, modelValue) => { this.isCut = true; this.cutOrCopyShifts(); }, () => { return selectedShifts.length > 0 && !selectedShiftsIsAbsenceRequest && !selectedShiftsIsNotSchedule; });

                // Copy
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.copy"], 'fa-clone', ($itemScope, $event, modelValue) => { this.isCut = false; this.cutOrCopyShifts(); }, () => { return selectedShifts.length > 0 && !selectedShiftsIsAbsenceRequest && !selectedShiftsIsNotSchedule; });

                // Paste
                let selectedCopiedShifts: number = this.getSelectedCutOrCopiedShifts().length;
                let allCopiedShifts: number = this.cutCopiedShifts.length;
                let msg = this.terms["time.schedule.planning.contextmenu.paste"];
                if (selectedCopiedShifts === allCopiedShifts)
                    msg = msg.format(selectedCopiedShifts.toString());
                else
                    msg = msg.format("{0} {1} {2}".format(selectedCopiedShifts.toString(), this.terms["common.of"], allCopiedShifts.toString()));

                this.scheduleViewContextMenuHandler.addContextMenuItem(msg, 'fa-paste', ($itemScope, $event, modelValue) => { this.pasteShift(employeeId, date); }, () => { return this.hasSelectedCutOrCopiedShifts && !noTemplateSlot; });

                // Clipboard
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.showclipboard"], 'fa-clipboard', ($itemScope, $event, modelValue) => { this.showClipboard(); }, () => { return this.hasCutOrCopiedShifts; });

                this.scheduleViewContextMenuHandler.addContextMenuSeparator();
            }
        }

        if (shift && !shift.isReadOnly && (!this.isStandbyView || shift.isStandby) && !isLeisureCode && !isAnnualLeave) {
            // Split shift
            this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.splitshift"].format(isBooking ? this.bookingUndefined : this.shiftUndefined), 'fa-cut', ($itemScope, $event, modelValue) => { this.splitShift(shift); }, () => { return onlyOneShiftSelected; });

            // Link shifts
            this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.linkshift"].format(isBooking ? this.bookingUndefined : this.shiftUndefined), 'fa-link', ($itemScope, $event, modelValue) => { this.linkShifts(selectedShifts); }, () => { return selectedShifts.filter(s => s.link).length > 1 && !selectedShiftsIsAbsenceRequest && _.uniqBy(selectedShifts, s => s.link).map(s => s.link).length > 1; });

            // Unlink shifts
            this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.unlinkshift"].format(isBooking ? this.bookingUndefined : this.shiftUndefined), 'fa-unlink', ($itemScope, $event, modelValue) => { this.unlinkShifts(selectedShifts); }, () => { return selectedShifts.filter(s => s.link).length > 1 && !selectedShiftsIsAbsenceRequest && _.uniqBy(selectedShifts, s => s.link).map(s => s.link).length === 1; });
        }

        if (this.isAdmin) {
            if (!this.isTemplateView && !this.isEmployeePostView && shift && !isAbsenceRequest && !isReadOnly && !shift.isOnDuty && !isLeisureCode) {
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();

                // Shift request not in template view
                if (!this.isScenarioView && !this.isStandbyView && !isAbsence && !isBooking) {
                    if (selectedShifts.length > 0 && _.first(_.sortBy(selectedShifts.map(s => s.actualStopTime))).isSameOrAfterOnMinute(new Date))
                        this.scheduleViewContextMenuHandler.addContextMenuItem(this.isOrderPlanningMode ? this.terms["time.schedule.planning.contextmenu.sendassignmentrequest"] : this.terms["time.schedule.planning.contextmenu.sendshiftrequest"], 'fa-envelope', ($itemScope, $event, modelValue) => { this.sendShiftRequest(shift); }, () => { return _.uniqBy(selectedShifts, s => s.link).map(s => s.link).length === 1; });
                }

                // Absence not for hidden employee or in template schedule view and only on schedule shifts
                if (!this.isStandbyView && shift.employeeId !== this.hiddenEmployeeId && !isAbsence && !isBooking)
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.absence"], 'fa-medkit errorColor', ($itemScope, $event, modelValue) => { this.absence(shift); }, () => { return true });

                // Show history
                if (!this.isScenarioView && (!this.isStandbyView || shift.isStandby) && !isAnnualLeave) {
                    this.scheduleViewContextMenuHandler.addContextMenuSeparator();
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.history"], 'fa-history', ($itemScope, $event, modelValue) => { this.showHistory(shift); }, () => { return onlyOneShiftSelected; });
                }
            }

            // Non standard absence
            if (!this.isStandbyView && employeeHasAnnualLeaveGroup) {
                if (!shift)
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.absence"], 'fa-medkit errorColor', ($itemScope, $event, modelValue) => { this.createAbsence(date, employeeId); }, () => { return true });

                if (shift && shift.timeDeviationCauseId && shift.absenceType !== TermGroup_TimeScheduleTemplateBlockAbsenceType.Standard)
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.deleteabsence"], 'fa-times errorColor', ($itemScope, $event, modelValue) => { this.deleteAbsence(shift); }, () => { return true });
            }

            // Template commands only in template view
            if (this.isTemplateView && !isAbsenceRequest && (this.templateScheduleEditHiddenPermission || employeeId !== this.hiddenEmployeeId)) {
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();

                // New template
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.newtemplate"], 'fa-plus', ($itemScope, $event, modelValue) => { this.newTemplate(slot ? slot : shift); }, () => { return !!slot || !!shift; });

                // Edit template
                this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.edittemplate"], 'fa-pencil', ($itemScope, $event, modelValue) => { this.editTemplate(slot ? slot : shift); }, () => { return !!slot || !!shift; });

                // Activate template
                if (this.placementPermission)
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.activate"], 'fa-calendar-check', ($itemScope, $event, modelValue) => { this.activateTemplate(slot ? slot : shift); }, () => { return !!slot || !!shift; });
            }

            // Print
            if ((this.isScheduleView || this.isTemplateScheduleView) && this.reportPermission && employeeId) {
                this.scheduleViewContextMenuHandler.addContextMenuSeparator();
                if (this.isScheduleView) {
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.printschedule"], 'fa-print', ($itemScope, $event, modelValue) => {
                        let employee = this.getEmployeeById(employeeId);
                        if (employee)
                            this.printScheduleForEmployees([employee.employeeId], [SoeReportTemplateType.TimeEmployeeSchedule, SoeReportTemplateType.TimeEmployeeLineSchedule, SoeReportTemplateType.TimeEmployeeScheduleSmallReport]);
                    }, () => { return !!slot || !!shift; });
                } else if (this.isTemplateScheduleView) {
                    this.scheduleViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.printtemplateschedule"], 'fa-print', ($itemScope, $event, modelValue) => {
                        let employee = this.getEmployeeById(employeeId);
                        if (employee)
                            this.printScheduleForEmployees([employee.employeeId], [SoeReportTemplateType.TimeEmployeeTemplateSchedule]);
                    }, () => { return !!slot || !!shift; });
                }
            }
        }

        if (CoreUtility.isSupportAdmin) {
            this.scheduleViewContextMenuHandler.addContextMenuSeparator();
            this.scheduleViewContextMenuHandler.addContextMenuItem("Debug (console.log)", 'fa-debug', ($itemScope, $event, modelValue) => { console.log(shift); }, () => { return !!shift; });
        }

        return this.scheduleViewContextMenuHandler.getContextMenuOptions();
    }

    private createEmployeeContextMenuOptions(employee: EmployeeListDTO): any[] {
        if (!this.employeeContextMenuHandler)
            return [];

        this.employeeContextMenuHandler.clearContextMenuItems();

        if (employee) {
            // Employee
            if (!this.isStandbyView && this.hasCurrentViewModifyPermission)
                this.employeeContextMenuHandler.addContextMenuItem(this.isEmployeePostView ? this.terms["time.schedule.planning.employee.contextmenu.editemployeepost"] : this.terms["time.schedule.planning.employee.contextmenu.editemployee"], 'fa-pencil iconEdit', ($itemScope, $event, modelValue) => { this.isEmployeePostView ? this.editEmployeePost(employee) : this.editEmployee(employee); }, () => { return true; });
            if (!this.isEmployeePostView)
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.showcontactinfo"], 'fa-address-card', ($itemScope, $event, modelValue) => { this.showContactInfo(employee.employeeId); }, () => { return true; });

            // Edit availability
            if (this.isSchedulePlanningMode && (this.isScheduleView || this.isDayView) && (this.viewAvailabilityPermission || this.editAvailabilityPermission) && this.hasCurrentViewModifyPermission) {
                this.employeeContextMenuHandler.addContextMenuSeparator();
                this.employeeContextMenuHandler.addContextMenuItem(this.editAvailabilityPermission ? this.terms["common.dashboard.myschedule.availability.edit"] : this.terms["common.dashboard.myschedule.availability.show"], 'fa-calendar-check', ($itemScope, $event, modelValue) => {
                    this.editAvailability(this.dateFrom, this.dateTo, employee.employeeId);
                }, () => { return true; });
            }

            // Copy schedule
            if (this.isSchedulePlanningMode && (this.isDayView || this.isScheduleView) && this.copySchedulePermission && this.hasCurrentViewModifyPermission) {
                this.employeeContextMenuHandler.addContextMenuSeparator();
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.copyschedule.title"], 'fa-clone', ($itemScope, $event, modelValue) => { this.openCopySchedule(employee.employeeId); }, () => { return true; });
            }

            // Print
            if (this.reportPermission && employee.employeeId) {
                this.employeeContextMenuHandler.addContextMenuSeparator();

                if (this.isDayView || this.isScheduleView)
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.printschedule"], 'fa-print', ($itemScope, $event, modelValue) => { this.printScheduleForEmployees([employee.employeeId], [SoeReportTemplateType.TimeEmployeeSchedule, SoeReportTemplateType.TimeEmployeeLineSchedule, SoeReportTemplateType.TimeEmployeeScheduleSmallReport]); }, () => { return true; });

                if (this.isTemplateView)
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.contextmenu.printtemplateschedule"], 'fa-print', ($itemScope, $event, modelValue) => { this.printScheduleForEmployees([employee.employeeId], [SoeReportTemplateType.TimeEmployeeTemplateSchedule]); }, () => { return true; });

                if ((this.isScheduleView || this.isTemplateScheduleView) && (this.employmentContractShortSubstituteReportId || this.hasEmployeeTemplates)) {
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.printemploymentcertificate"], 'fa-print', ($itemScope, $event, modelValue) => { this.printEmploymentCertificateForEmployee(employee); }, () => { return true; });
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.sendemploymentcertificate"], 'fa-envelope', ($itemScope, $event, modelValue) => { this.sendEmploymentCertificateForEmployee(employee); }, () => { return true; });
                }
            }

            // Employee post
            if (this.isEmployeePostView && this.hasCurrentViewModifyPermission) {
                const hasShifts: boolean = this.shifts.filter(s => employee.employeePostId === s.employeePostId).length > 0;
                const hasTemplate: boolean = employee.hasTemplateSchedules;

                if (!hasTemplate)
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.createemptyscheduleforemployeepost"], 'fa-calendar', ($itemScope, $event, modelValue) => { this.createEmptyScheduleForEmployeePost(employee); }, () => { return !hasShifts; });
                if (!hasShifts)
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.generatescheduleforemployeepost"], 'fa-calendar-plus', ($itemScope, $event, modelValue) => { this.generateScheduleForEmployeePost(employee); }, () => { return !hasShifts; });
                if (hasShifts)
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.regeneratescheduleforemployeepost"], 'fa-sync', ($itemScope, $event, modelValue) => { this.generateScheduleForEmployeePost(employee); }, () => { return hasShifts && employee.employeePostStatus !== SoeEmployeePostStatus.Locked; });
                if (hasShifts || hasTemplate)
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.deletescheduleforemployeepost"], 'fa-calendar-times iconDelete', ($itemScope, $event, modelValue) => { this.deleteScheduleForEmployeePost(employee, true); }, () => { return employee.employeePostStatus !== SoeEmployeePostStatus.Locked; });
                this.employeeContextMenuHandler.addContextMenuSeparator();
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.preanalyseemployeepost"], 'fa-chart-network', ($itemScope, $event, modelValue) => { this.preAnalyseEmployeePost(employee); }, () => { return true; });

                if (employee.employeeId) {
                    this.employeeContextMenuHandler.addContextMenuSeparator();
                    this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.removeemployeefromemployeepost"], 'fa-user-times iconDelete', ($itemScope, $event, modelValue) => { this.removeEmployeeFromEmployeePost(employee); }, () => { return true; });
                }

                const canLock: boolean = hasShifts && employee.employeePostStatus !== SoeEmployeePostStatus.Locked;
                const canUnlock: boolean = employee.employeePostStatus === SoeEmployeePostStatus.Locked;
                if (canLock || canUnlock) {
                    this.employeeContextMenuHandler.addContextMenuSeparator();
                    if (canLock)
                        this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.lockscheduleforemployeepost"], 'fa-lock-alt', ($itemScope, $event, modelValue) => { this.changeStatusForEmployeePost(employee, SoeEmployeePostStatus.Locked); }, () => { return canLock; });
                    else if (canUnlock)
                        this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.unlockscheduleforemployeepost"], 'fa-unlock-alt', ($itemScope, $event, modelValue) => { this.changeStatusForEmployeePost(employee, SoeEmployeePostStatus.None); }, () => { return canUnlock; });
                }
            }

            this.employeeContextMenuHandler.addContextMenuSeparator();
            if (this.isEmployeePostView)
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.reloademployeepost"], 'fa-sync', ($itemScope, $event, modelValue) => { this.reloadEmployeePosts([employee.employeePostId], true); }, () => { return true; });
            else
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.employee.contextmenu.reloademployee"], 'fa-sync', ($itemScope, $event, modelValue) => { this.reloadEmployees([employee.employeeId], true); }, () => { return true; });

            if (this.useAnnualLeave) {
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.annualleave.balance.recalculate"], 'fa-balance-scale', ($itemScope, $event, modelValue) => { this.recalculateAnnualLeaveBalanceForEmployees([employee.employeeId], true, false); }, () => { return true; });
                this.employeeContextMenuHandler.addContextMenuItem(this.terms["time.schedule.planning.annualleave.balance.recalculate.prevyear"], 'fa-balance-scale', ($itemScope, $event, modelValue) => { this.recalculateAnnualLeaveBalanceForEmployees([employee.employeeId], true, true); }, () => { return true; });
            }

            if (CoreUtility.isSupportAdmin) {
                this.employeeContextMenuHandler.addContextMenuSeparator();
                this.employeeContextMenuHandler.addContextMenuItem("Debug (console.log)", 'fa-debug', ($itemScope, $event, modelValue) => { console.log(employee); }, () => { return true; });
            }
        }

        return this.employeeContextMenuHandler.getContextMenuOptions();
    }

    private createEmployeeListContextMenuOptions(employee: EmployeeRightListDTO) {
        if (!this.employeeListContextMenuHandler)
            return [];

        this.employeeListContextMenuHandler.clearContextMenuItems();

        if (employee) {
            if (CoreUtility.isSupportAdmin) {
                this.employeeListContextMenuHandler.addContextMenuItem("Debug (console.log)", 'fa-debug', ($itemScope, $event, modelValue) => { console.log(employee); }, () => { return true; });
            }
        }

        return this.employeeListContextMenuHandler.getContextMenuOptions();
    }

    private createTaskViewContextMenuOptions(type: SoeStaffingNeedsTaskType, task: StaffingNeedsTaskDTO, slot: SlotDTO): any[] {
        let selectedTasks = this.scheduleHandler.getSelectedTasks();

        // If clicked task does not exist in collection of selected task, unselect all selected tasks
        if (task && !selectedTasks.map(s => s.taskId).includes(task.taskId)) {
            this.scheduleHandler.clearSelectedTasks();
            selectedTasks = [];
        }

        if (task && selectedTasks.length === 0)
            this.scheduleHandler.selectTask(task);

        this.taskViewContextMenuHandler.clearContextMenuItems();

        // Add task/delivery
        this.taskViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.staffingneeds.contextmenu.newtask"], 'fa-plus', ($itemScope, $event, modelValue) => {
            this.openEditTask(null, slot);
        }, () => { return true; });
        this.taskViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.staffingneeds.contextmenu.newdelivery"], 'fa-plus', ($itemScope, $event, modelValue) => {
            this.openEditDelivery(null, slot)
        }, () => { return true; });

        // Edit task/delivery
        if (task)
            this.taskViewContextMenuHandler.addContextMenuItem(task.isDelivery ? this.terms["time.schedule.staffingneeds.contextmenu.editdelivery"] : this.terms["time.schedule.staffingneeds.contextmenu.edittask"], 'fa-pencil', ($itemScope, $event, modelValue) => {
                if (task.isDelivery)
                    this.openEditDelivery(task, null)
                else if (task.isTask)
                    this.openEditTask(task, null);
            }, () => { return true; });

        // Delete task/delivery
        if (task) {
            this.taskViewContextMenuHandler.addContextMenuItem(task.isDelivery ? this.terms["time.schedule.staffingneeds.contextmenu.deletedelivery"] : this.terms["time.schedule.staffingneeds.contextmenu.deletetask"], 'fa-times errorColor', ($itemScope, $event, modelValue) => {
                if (task.isDelivery)
                    this.deleteDelivery(task);
                else
                    this.deleteTask(task);
            }, () => { return true; });
        }

        let hasStaffingNeedsPermission = (this.staffingNeedsDayViewPermission || this.staffingNeedsScheduleViewPermission);
        if (hasStaffingNeedsPermission && task && task.isTask && task.isStaffingNeedsFrequency) {
            this.taskViewContextMenuHandler.addContextMenuSeparator();
            this.taskViewContextMenuHandler.addContextMenuItem(this.terms["time.schedule.timescheduletask.generatedneed.show"], 'fa-poll-people', ($itemScope, $event, modelValue) => {
                this.openGeneratedNeedsDialog(task.id, slot.startTime.date());
            }, () => { return true; });
        }

        return this.taskViewContextMenuHandler.getContextMenuOptions();
    }

    private setupTaskTypes() {
        this.taskTypes = [];
        this.taskTypes.push({ id: SoeStaffingNeedsTaskType.Task, label: this.terms["time.schedule.staffingneeds.tasktype.tasks"] });
        this.taskTypes.push({ id: SoeStaffingNeedsTaskType.Delivery, label: this.terms["time.schedule.staffingneeds.tasktype.deliveries"] });
    }

    private setupWeekdays() {
        this.weekdays.forEach(weekday => {
            weekday['name'] = CalendarUtility.getDayName(weekday['dayOfWeek']).toUpperCaseFirstLetter();
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.chart",
            "core.donotshowagain",
            "core.exportexcel",
            "core.hide",
            "core.info",
            "core.notspecified",
            "core.others",
            "core.pieces.short",
            "core.reload_data",
            "core.rounded",
            "core.selection",
            "core.show",
            "core.table",
            "core.time.day",
            "core.time.days",
            "core.time.minutes",
            "core.time.short.hour",
            "core.transfer",
            "core.warning",
            "common.absence",
            "common.chart.nodata",
            "common.contactaddresses.issecret",
            "common.dashboard.myschedule.availability.edit",
            "common.dashboard.myschedule.availability.hascomment",
            "common.dashboard.myschedule.availability.show",
            "common.date",
            "common.filtered",
            "common.inactive",
            "common.initialvalue",
            "common.of",
            "common.quantity",
            "common.rows",
            "common.sum",
            "common.total",
            "common.week",
            "common.weekshort",
            "common.swapshift.approveinmobileinfo",
            "error.default_error",
            "time.schedule.planning.blocktype.schedule",
            "time.schedule.planning.blocktype.booking",
            "time.schedule.planning.blocktype.order",
            "time.schedule.planning.blocktype.standby",
            "time.schedule.planning.blocktype.onduty",
            "time.schedule.planning.viewdefinition.group.schedule",
            "time.schedule.planning.viewdefinition.group.template",
            "time.schedule.planning.viewdefinition.group.scenario",
            "time.schedule.planning.viewdefinition.group.standby",
            "time.schedule.planning.viewdefinition.calendar",
            "time.schedule.planning.viewdefinition.day",
            "time.schedule.planning.viewdefinition.schedule",
            "time.schedule.planning.viewdefinition.templateday",
            "time.schedule.planning.viewdefinition.templateschedule",
            "time.schedule.planning.viewdefinition.complete",
            "time.schedule.planning.shiftstatus.open",
            "time.schedule.planning.shiftstatus.assigned",
            "time.schedule.planning.shiftstatus.accepted",
            "time.schedule.planning.shiftstatus.wanted",
            "time.schedule.planning.shiftstatus.unwanted",
            "time.schedule.planning.shiftstatus.absencerequested",
            "time.schedule.planning.shiftstatus.absenceapproved",
            "time.schedule.planning.shiftstatus.preliminary",
            "time.schedule.planning.shiftstatus.hideabsencerequested",
            "time.schedule.planning.shiftstatus.hideabsenceapproved",
            "time.schedule.planning.shiftstatus.hidepreliminary",
            "time.schedule.planning.bookingdefined",
            "time.schedule.planning.bookingundefined",
            "time.schedule.planning.bookingsdefined",
            "time.schedule.planning.bookingsundefined",
            "time.schedule.planning.breakprefix",
            "time.schedule.planning.breaklabel",
            "time.schedule.planning.wholedaylabel",
            "time.schedule.planning.scheduletime",
            "time.schedule.planning.scheduletypefactortime",
            "time.schedule.planning.worktimeweek",
            "time.schedule.planning.nettime",
            "time.schedule.planning.grosstime",
            "time.schedule.planning.cost",
            "time.schedule.planning.grossnetcost.loading",
            "time.schedule.planning.cycletime.total",
            "time.schedule.planning.cycletime.average",
            "time.schedule.planning.noemployment",
            "time.schedule.planning.firstdayoftemplate",
            "time.schedule.planning.lastdayoftemplate",
            "time.schedule.planning.repeatingday",
            "time.schedule.planning.templateschedule",
            "time.schedule.planning.templateschedules",
            "time.schedule.planning.notemplateschedule",
            "time.schedule.planning.hasemployeeschedule",
            "time.schedule.planning.thisshift",
            "time.schedule.planning.thisorder",
            "time.schedule.planning.thisbooking",
            "time.schedule.planning.todaysschedule",
            "time.schedule.planning.todaysorders",
            "time.schedule.planning.todaysbookings",
            "time.schedule.planning.contextmenu.newshift",
            "time.schedule.planning.contextmenu.editshift",
            "time.schedule.planning.contextmenu.deleteshift",
            "time.schedule.planning.contextmenu.neworder",
            "time.schedule.planning.contextmenu.editorder",
            "time.schedule.planning.contextmenu.newbooking",
            "time.schedule.planning.contextmenu.editbooking",
            "time.schedule.planning.contextmenu.deletebooking",
            "time.schedule.planning.contextmenu.newstandbyshift",
            "time.schedule.planning.contextmenu.editstandbyshift",
            "time.schedule.planning.contextmenu.deletestandbyshift",
            "time.schedule.planning.contextmenu.newondutyshift",
            "time.schedule.planning.contextmenu.editondutyshift",
            "time.schedule.planning.contextmenu.deleteondutyshift",
            "time.schedule.planning.contextmenu.newleisurecode",
            "time.schedule.planning.contextmenu.editleisurecode",
            "time.schedule.planning.contextmenu.deleteleisurecode",
            "time.schedule.planning.contextmenu.changeemployee",
            "time.schedule.planning.contextmenu.cut",
            "time.schedule.planning.contextmenu.copy",
            "time.schedule.planning.contextmenu.paste",
            "time.schedule.planning.contextmenu.showclipboard",
            "time.schedule.planning.contextmenu.splitshift",
            "time.schedule.planning.contextmenu.linkshift",
            "time.schedule.planning.contextmenu.unlinkshift",
            "time.schedule.planning.contextmenu.absence",
            "time.schedule.planning.contextmenu.deleteabsence",
            "time.schedule.planning.contextmenu.history",
            "time.schedule.planning.contextmenu.newtemplate",
            "time.schedule.planning.contextmenu.edittemplate",
            "time.schedule.planning.contextmenu.activate",
            "time.schedule.planning.contextmenu.printschedule",
            "time.schedule.planning.contextmenu.printtemplateschedule",
            "time.schedule.planning.editmode.breaks",
            "time.schedule.planning.editmode.templatebreaks",
            "time.schedule.planning.editmode.shifts",
            "time.schedule.planning.buttonfunctions.allocateleisurecodes",
            "time.schedule.planning.buttonfunctions.deleteleisurecodes",
            "time.schedule.planning.buttonfunctions.deftoprel",
            "time.schedule.planning.buttonfunctions.preltodef",
            "time.schedule.planning.buttonfunctions.printschedule",
            "time.schedule.planning.buttonfunctions.printtemplateschedule",
            "time.schedule.planning.buttonfunctions.printemployeeposttemplateschedule",
            "time.schedule.planning.buttonfunctions.printscenarioschedule",
            "time.schedule.planning.copyschedule.title",
            "time.schedule.planning.showemployeelist",
            "time.schedule.planning.hideemployeelist",
            "time.schedule.planning.showdashboard",
            "time.schedule.planning.hidedashboard",
            "time.schedule.planning.evaluateworkrules",
            "time.schedule.planning.showunscheduledtasks",
            "time.schedule.planning.hideunscheduledtasks",
            "time.schedule.planning.scenario.new",
            "time.schedule.planning.scenario.delete",
            "time.schedule.planning.scenario.activate",
            "time.schedule.planning.scenario.createtemplate",
            "time.schedule.planning.scenario.noselected",
            "time.schedule.planning.scenario.outside",
            "time.schedule.planning.availability",
            "time.schedule.planning.available",
            "time.schedule.planning.unavailable",
            "time.schedule.planning.islended",
            "time.schedule.planning.isotheraccount",
            "time.schedule.planning.hasshiftrequest",
            "time.schedule.planning.editshift.extrashift",
            "time.schedule.planning.editshift.substitute",
            "time.schedule.planning.employee.contextmenu.editemployee",
            "time.schedule.planning.employee.contextmenu.editemployeepost",
            "time.schedule.planning.employee.contextmenu.showcontactinfo",
            "time.schedule.planning.employee.contextmenu.printemploymentcertificate",
            "time.schedule.planning.employee.contextmenu.sendemploymentcertificate",
            "time.schedule.planning.employee.contextmenu.generatescheduleforemployeepost",
            "time.schedule.planning.employee.contextmenu.lockscheduleforemployeepost",
            "time.schedule.planning.employee.contextmenu.unlockscheduleforemployeepost",
            "time.schedule.planning.employee.contextmenu.createemptyscheduleforemployeepost",
            "time.schedule.planning.employee.contextmenu.regeneratescheduleforemployeepost",
            "time.schedule.planning.employee.contextmenu.deletescheduleforemployeepost",
            "time.schedule.planning.employee.contextmenu.preanalyseemployeepost",
            "time.schedule.planning.employee.contextmenu.removeemployeefromemployeepost",
            "time.schedule.planning.employee.contextmenu.reloademployee",
            "time.schedule.planning.employee.contextmenu.reloademployeepost",
            "time.schedule.planning.createemptyscheduleforemployeeposts",
            "time.schedule.planning.generatescheduleforemployeeposts",
            "time.schedule.planning.deletescheduleforemployeeposts",
            "time.schedule.planning.showbudget",
            "time.schedule.planning.budget",
            "time.schedule.planning.annualsummarytooltip",
            "time.schedule.planning.annualsummary.loading",
            "time.schedule.planning.annualleave.balance",
            "time.schedule.planning.annualleave.balance.calculating",
            "time.schedule.planning.annualleave.balance.loading",
            "time.schedule.planning.annualleave.balance.recalculate",
            "time.schedule.planning.annualleave.balance.recalculate.prevyear",
            "time.schedule.planning.employeeperiodtimesummary.opensummaryerror.message",
            "time.schedule.planning.selectableinformation.followup.budget",
            "time.schedule.planning.selectableinformation.followup.forecast",
            "time.schedule.planning.selectableinformation.followup.templateschedule",
            "time.schedule.planning.selectableinformation.followup.templatescheduleforemployeepost",
            "time.schedule.planning.selectableinformation.followup.schedule",
            "time.schedule.planning.selectableinformation.followup.time",
            "time.schedule.planning.selectableinformation.followup.calculationtype",
            "time.schedule.planning.selectableinformation.followup.loading",
            "time.schedule.planning.selectableinformation.followup.delayloading",
            "time.schedule.planning.followuptable.budget",
            "time.schedule.planning.followuptable.forecast",
            "time.schedule.planning.followuptable.templateschedule",
            "time.schedule.planning.followuptable.schedule",
            "time.schedule.planning.followuptable.scheduleandtime",
            "time.schedule.planning.followuptable.time",
            "time.schedule.planning.followuptable.exportname",
            "time.schedule.planning.followuptable.adjust",
            "time.schedule.planning.loadscheduleprogress.load",
            "time.schedule.planning.loadscheduleprogress.process",
            "time.schedule.planning.loadscheduleprogress.render",
            "time.schedule.planning.editshift.functions.restoretoschedule",
            "time.schedule.planning.editshift.functions.removeabsence",
            "time.schedule.planning.noemployeesforshifts",
            "time.schedule.staffingneeds.contextmenu.deletedelivery",
            "time.schedule.staffingneeds.contextmenu.deletetask",
            "time.schedule.staffingneeds.contextmenu.editdelivery",
            "time.schedule.staffingneeds.contextmenu.edittask",
            "time.schedule.staffingneeds.contextmenu.newdelivery",
            "time.schedule.staffingneeds.contextmenu.newtask",
            "time.schedule.staffingneeds.viewdefinition.employeeposts",
            "time.schedule.staffingneeds.viewdefinition.planning",
            "time.schedule.staffingneeds.viewdefinition.tasksanddeliveries",
            "time.schedule.staffingneeds.planning.functions.add",
            "time.schedule.staffingneeds.planning.functions.reload",
            "time.schedule.staffingneeds.planning.functions.printtasksanddeliveries",
            "time.schedule.staffingneeds.planning.need",
            "time.schedule.staffingneeds.planning.needfrequency",
            "time.schedule.staffingneeds.planning.hideshifttypesum",
            "time.schedule.staffingneeds.planning.showshifttypesum",
            "time.schedule.staffingneeds.tasktype.deliveries",
            "time.schedule.staffingneeds.tasktype.tasks",
            "time.schedule.templategroup.templategroup",
            "time.schedule.timescheduletask.generatedneed.show",
            "time.schedule.timescheduletask.task",
            "time.schedule.incomingdelivery.incomingdelivery",
            "common.dailyrecurrencepattern.patterntitle"
        ];

        if (this.isSchedulePlanningMode) {
            keys.push("time.schedule.planning");
            keys.push("time.schedule.planning.shiftdefined");
            keys.push("time.schedule.planning.shiftundefined");
            keys.push("time.schedule.planning.shiftsdefined");
            keys.push("time.schedule.planning.shiftsundefined");
            keys.push("time.schedule.planning.editshift.addnew");
            keys.push("time.schedule.planning.contextmenu.sendshiftrequest");
        } else if (this.isOrderPlanningMode) {
            keys.push("common.categories");
            keys.push("common.customer");
            keys.push("common.customer.customer.orderproject");
            keys.push("common.order");
            keys.push("common.ordershifttype");
            keys.push("common.priority");
            keys.push("time.schedule.planning.orderplanning");
            keys.push("time.schedule.planning.assignmentdefined");
            keys.push("time.schedule.planning.assignmentundefined");
            keys.push("time.schedule.planning.assignmentsdefined");
            keys.push("time.schedule.planning.assignmentsundefined");
            keys.push("time.schedule.planning.editassignment.addnew");
            keys.push("time.schedule.planning.editassignment.edit");
            keys.push("time.schedule.planning.editassignment.delete");
            keys.push("time.schedule.planning.contextmenu.sendassignmentrequest");
            keys.push("time.schedule.planning.orderlist.plannedstartdate");
            keys.push("time.schedule.planning.orderlist.plannedstopdate");
            keys.push("time.schedule.planning.orderlist.estimatedtime");
            keys.push("time.schedule.planning.orderlist.remainingtime");
            keys.push("time.schedule.planning.orderlist.deliveryaddress");
            keys.push("time.schedule.planning.orderstatus");
        }

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            if (this.isSchedulePlanningMode) {
                this.shiftDefined = this.terms["time.schedule.planning.shiftdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.shiftundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.shiftsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.shiftsundefined"];
            } else if (this.isOrderPlanningMode) {
                this.shiftDefined = this.terms["time.schedule.planning.assignmentdefined"];
                this.shiftUndefined = this.terms["time.schedule.planning.assignmentundefined"];
                this.shiftsDefined = this.terms["time.schedule.planning.assignmentsdefined"];
                this.shiftsUndefined = this.terms["time.schedule.planning.assignmentsundefined"];
            }
            this.bookingDefined = this.terms["time.schedule.planning.bookingdefined"];
            this.bookingUndefined = this.terms["time.schedule.planning.bookingundefined"];
            this.bookingsDefined = this.terms["time.schedule.planning.bookingsdefined"];
            this.bookingsUndefined = this.terms["time.schedule.planning.bookingsundefined"];
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        let features: number[] = [];

        // Common
        features.push(Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue);

        // Mode specific
        if (this.isSchedulePlanningMode) {
            features.push(Feature.Time_Schedule_SchedulePlanning_CalendarView);
            features.push(Feature.Time_Schedule_SchedulePlanning_DayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_ScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_TemplateDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_TemplateScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_EmployeePostDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_EmployeePostScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_StandbyDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_StandbyScheduleView);
            features.push(Feature.Time_Schedule_StaffingNeeds);
            features.push(Feature.Time_Schedule_StaffingNeeds_Tasks);
            features.push(Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries);
            features.push(Feature.Time_Schedule_SchedulePlanning_Bookings);
            features.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);
            features.push(Feature.Time_Schedule_SchedulePlanning_OnDutyShifts);
            features.push(Feature.Time_Schedule_SchedulePlanning_ShowCosts);
            features.push(Feature.Time_Schedule_SchedulePlanning_ShowUnscheduledTasks);
            features.push(Feature.Time_Schedule_StaffingNeeds);
            features.push(Feature.Economy_Accounting_SalesBudget);
            features.push(Feature.Economy_Accounting_SalesForecast);
            features.push(Feature.Time_Distribution_Reports_Selection);
            features.push(Feature.Time_Distribution_Reports_Selection_Download);
            features.push(Feature.Time_Schedule_SchedulePlanning_Dashboard);
            features.push(Feature.Time_Schedule_SchedulePlanning_Dashboard_AdjustKPIs);
            features.push(Feature.Time_Schedule_Availability_EditOnOtherEmployees);
        } else if (this.isOrderPlanningMode) {
            features.push(Feature.Billing_Order_ShowOnMap);
            features.push(Feature.Billing_Order_Planning_Bookings);
        }

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            // Common
            this.showQueuePermission = x[Feature.Time_Schedule_SchedulePlanningUser_HandleShiftShowQueue];

            // Mode specific
            if (this.isSchedulePlanningMode) {
                this.calendarViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_CalendarView];
                this.dayViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_DayView];
                this.scheduleViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_ScheduleView];
                this.templateDayViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_TemplateDayView];
                this.templateScheduleViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_TemplateScheduleView];
                this.standbyDayViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyDayView];
                this.standbyScheduleViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyScheduleView];
                this.employeePostDayViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_EmployeePostDayView];
                this.employeePostScheduleViewReadPermission = x[Feature.Time_Schedule_SchedulePlanning_EmployeePostScheduleView];
                this.tasksAndDeliveriesDayViewReadPermission = x[Feature.Time_Schedule_StaffingNeeds_Tasks] || x[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries];
                this.tasksAndDeliveriesScheduleViewReadPermission = x[Feature.Time_Schedule_StaffingNeeds_Tasks] || x[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries];
                this.staffingNeedsDayViewReadPermission = x[Feature.Time_Schedule_StaffingNeeds];
                this.staffingNeedsScheduleViewReadPermission = x[Feature.Time_Schedule_StaffingNeeds];
                this.bookingReadPermission = x[Feature.Time_Schedule_SchedulePlanning_Bookings];
                this.standbyShiftsReadPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyShifts];
                this.onDutyShiftsReadPermission = x[Feature.Time_Schedule_SchedulePlanning_OnDutyShifts];
                this.showTotalCostPermission = x[Feature.Time_Schedule_SchedulePlanning_ShowCosts];
                this.showUnscheduledTasksPermission = x[Feature.Time_Schedule_SchedulePlanning_ShowUnscheduledTasks];
                this.showStaffingNeedsPermission = x[Feature.Time_Schedule_StaffingNeeds];
                this.showBudgetPermission = x[Feature.Economy_Accounting_SalesBudget];
                this.showForecastPermission = x[Feature.Economy_Accounting_SalesForecast];
                this.reportPermission = x[Feature.Time_Distribution_Reports_Selection] && x[Feature.Time_Distribution_Reports_Selection_Download];
                this.showDashboardPermission = x[Feature.Time_Schedule_SchedulePlanning_Dashboard];
                if (this.showDashboardPermission)
                    this.loadFollowUpCalculationTypes();
                this.adjustKPIsPermission = x[Feature.Time_Schedule_SchedulePlanning_Dashboard_AdjustKPIs];
                this.viewAvailabilityPermission = x[Feature.Time_Schedule_Availability_EditOnOtherEmployees];
            } else if (this.isOrderPlanningMode) {
                this.showOrdersOnMapPermission = x[Feature.Billing_Order_ShowOnMap];
                this.bookingReadPermission = x[Feature.Billing_Order_Planning_Bookings];
            }
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        let features: number[] = [];

        // Common
        features.push(Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts);
        features.push(Feature.Time_Distribution_Reports_SavePublicSelections);

        // Mode specific
        if (this.isSchedulePlanningMode) {
            features.push(Feature.Time_Schedule_SchedulePlanning_CalendarView);
            features.push(Feature.Time_Schedule_SchedulePlanning_DayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_ScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_TemplateDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_TemplateScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_EmployeePostDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_EmployeePostScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_ScenarioDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_ScenarioScheduleView);
            features.push(Feature.Time_Schedule_SchedulePlanning_StandbyDayView);
            features.push(Feature.Time_Schedule_SchedulePlanning_StandbyScheduleView);
            features.push(Feature.Time_Schedule_StaffingNeeds);
            features.push(Feature.Time_Schedule_StaffingNeeds_Tasks);
            features.push(Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries);
            features.push(Feature.Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee);
            features.push(Feature.Time_Schedule_SchedulePlanning_Bookings);
            features.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);
            features.push(Feature.Time_Schedule_SchedulePlanning_OnDutyShifts);
            features.push(Feature.Time_Time_Attest);
            features.push(Feature.Time_Schedule_SchedulePlanning_PreliminaryShifts);
            features.push(Feature.Time_Schedule_SchedulePlanning_CopySchedule);
            features.push(Feature.Time_Schedule_Placement);
            features.push(Feature.Time_Schedule_SchedulePlanning_Placement);
            features.push(Feature.Time_Schedule_Needs_To_TemplateSchedule);
            features.push(Feature.Time_Time_Attest_RestoreToSchedule);
            features.push(Feature.Time_Schedule_Availability_EditOnOtherEmployees);
        } else if (this.isOrderPlanningMode) {
            features.push(Feature.Billing_Order_Planning_CalendarView);
            features.push(Feature.Billing_Order_Planning_DayView);
            features.push(Feature.Billing_Order_Planning_ScheduleView);
            features.push(Feature.Billing_Order_Planning_Bookings);
            features.push(Feature.Billing_Order_PlanningUser_CalendarView);
            features.push(Feature.Billing_Order_PlanningUser_DayView);
            features.push(Feature.Billing_Order_PlanningUser_ScheduleView);
        }

        return this.coreService.hasModifyPermissions(features).then((x) => {
            // Common
            this.seeOtherEmployeesShiftsPermission = x[Feature.Time_Schedule_SchedulePlanningUser_SeeOtherEmployeesShifts];
            this.savePublicSelectionPermission = x[Feature.Time_Distribution_Reports_SavePublicSelections];

            // TODO: Replace with permission
            this.activeScheduleEditHiddenPermission = true;

            // Mode specific
            if (this.isSchedulePlanningMode) {
                this.calendarViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_CalendarView];
                this.dayViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_DayView];
                this.scheduleViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_ScheduleView];
                this.templateDayViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_TemplateDayView];
                this.templateScheduleViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_TemplateScheduleView];
                this.employeePostDayViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_EmployeePostDayView];
                this.employeePostScheduleViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_EmployeePostScheduleView];
                this.scenarioDayViewPermission = x[Feature.Time_Schedule_SchedulePlanning_ScenarioDayView];
                this.scenarioScheduleViewPermission = x[Feature.Time_Schedule_SchedulePlanning_ScenarioScheduleView];
                this.standbyDayViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyDayView];
                this.standbyScheduleViewModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyScheduleView];
                this.tasksAndDeliveriesDayViewModifyPermission = x[Feature.Time_Schedule_StaffingNeeds_Tasks] || x[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries];
                this.tasksAndDeliveriesScheduleViewModifyPermission = x[Feature.Time_Schedule_StaffingNeeds_Tasks] || x[Feature.Time_Schedule_StaffingNeeds_IncomingDeliveries];
                this.staffingNeedsDayViewModifyPermission = x[Feature.Time_Schedule_StaffingNeeds];
                this.staffingNeedsScheduleViewModifyPermission = x[Feature.Time_Schedule_StaffingNeeds];
                this.templateScheduleEditHiddenPermission = x[Feature.Time_Schedule_SchedulePlanning_TemplateSchedule_EditHiddenEmployee];
                // TODO: Replace with permission
                this.standbyEditHiddenPermission = false;
                this.bookingModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_Bookings];
                this.standbyShiftsModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyShifts];
                this.onDutyShiftsModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_OnDutyShifts];
                this.attestPermission = x[Feature.Time_Time_Attest];
                this.preliminaryPermission = x[Feature.Time_Schedule_SchedulePlanning_PreliminaryShifts];
                this.copySchedulePermission = x[Feature.Time_Schedule_SchedulePlanning_CopySchedule];
                this.placementPermission = x[Feature.Time_Schedule_Placement] || x[Feature.Time_Schedule_SchedulePlanning_Placement];
                this.showNeedsPermission = x[Feature.Time_Schedule_Needs_To_TemplateSchedule];
                this.restoreToSchedulePermission = x[Feature.Time_Time_Attest_RestoreToSchedule];
                this.editAvailabilityPermission = x[Feature.Time_Schedule_Availability_EditOnOtherEmployees];
            } else if (this.isOrderPlanningMode) {
                this.calendarViewModifyPermission = x[Feature.Billing_Order_Planning_CalendarView] || x[Feature.Billing_Order_PlanningUser_CalendarView];
                this.dayViewModifyPermission = x[Feature.Billing_Order_Planning_DayView] || x[Feature.Billing_Order_PlanningUser_DayView];
                this.scheduleViewModifyPermission = x[Feature.Billing_Order_Planning_ScheduleView] || x[Feature.Billing_Order_PlanningUser_ScheduleView];
                this.bookingModifyPermission = x[Feature.Billing_Order_Planning_Bookings];
            }
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];

        // Common
        settingTypes.push(CompanySettingType.TimeUseVacant);
        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);
        settingTypes.push(CompanySettingType.TimeSkillCantBeOverridden);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningCalendarViewShowDaySummary);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewStartTime);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewEndTime);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningClockRounding);
        settingTypes.push(CompanySettingType.TimeShiftTypeMandatory);
        settingTypes.push(CompanySettingType.TimeEditShiftAllowHoles);
        settingTypes.push(CompanySettingType.TimeDefaultDoNotKeepShiftsTogether);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSendXEMailOnChange);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSkipWorkRules);
        settingTypes.push(CompanySettingType.TimeDefaultEmployeeScheduleDayReport);
        settingTypes.push(CompanySettingType.TimeDefaultEmployeeScheduleWeekReport);
        settingTypes.push(CompanySettingType.TimeDefaultEmployeeTemplateScheduleDayReport);
        settingTypes.push(CompanySettingType.TimeDefaultEmployeeTemplateScheduleWeekReport);
        settingTypes.push(CompanySettingType.TimeDefaultEmployeePostTemplateScheduleDayReport);
        settingTypes.push(CompanySettingType.TimeDefaultEmployeePostTemplateScheduleWeekReport);
        settingTypes.push(CompanySettingType.TimeDefaultScenarioScheduleDayReport);
        settingTypes.push(CompanySettingType.TimeDefaultScenarioScheduleWeekReport);
        settingTypes.push(CompanySettingType.ExtraShiftAsDefaultOnHidden);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDragDropMoveAsDefault);

        // Mode specific
        if (this.isSchedulePlanningMode) {
            settingTypes.push(CompanySettingType.UseAccountHierarchy);
            settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
            settingTypes.push(CompanySettingType.TimeMaxNoOfBrakes);
            settingTypes.push(CompanySettingType.TimeUseStopDateOnTemplate);
            settingTypes.push(CompanySettingType.TimePlacementDefaultPreliminary);
            settingTypes.push(CompanySettingType.TimePlacementHidePreliminary);
            settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTime);
            settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);
            settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeColors);
            settingTypes.push(CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute);
            settingTypes.push(CompanySettingType.UseMultipleScheduleTypes);
            settingTypes.push(CompanySettingType.TimeDefaultScheduleTasksAndDeliverysDayReport);
            settingTypes.push(CompanySettingType.TimeDefaultScheduleTasksAndDeliverysWeekReport);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningInactivateLending);
            settingTypes.push(CompanySettingType.UseLeisureCodes);
            settingTypes.push(CompanySettingType.UseAnnualLeave);
        } else if (this.isOrderPlanningMode) {
            settingTypes.push(CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment);
        }

        return this.coreService.getCompanySettings(settingTypes).then(x => {

            // Common
            this.getHiddenEmployeeId();
            this.useVacant = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseVacant);
            if (this.useVacant)
                this.getVacantEmployeeIds();
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultTimeCode);
            this.skillCantBeOverridden = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSkillCantBeOverridden);
            this.showSummaryInCalendarView = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningCalendarViewShowDaySummary);
            this.dayViewStartTime = this.originalDayViewStartTime = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewStartTime);
            this.dayViewEndTime = this.originalDayViewEndTime = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewEndTime);
            this.adjustDayViewTimes(true);
            this.dayViewMinorTickLength = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
            if (this.dayViewMinorTickLength < 15)
                this.dayViewMinorTickLength = 15;
            this.clockRounding = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningClockRounding);
            this.shiftTypeMandatory = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeShiftTypeMandatory);
            this.allowHolesWithoutBreaks = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeEditShiftAllowHoles);
            this.keepShiftsTogether = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeDefaultDoNotKeepShiftsTogether);
            this.sendXEMailOnChange = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSendXEMailOnChange);
            this.possibleToSkipWorkRules = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSkipWorkRules);
            this.dayScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeeScheduleDayReport);
            this.weekScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeeScheduleWeekReport);
            this.dayTemplateScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeeTemplateScheduleDayReport);
            this.weekTemplateScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeeTemplateScheduleWeekReport);
            this.dayEmployeePostTemplateScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeePostTemplateScheduleDayReport);
            this.weekEmployeePostTemplateScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultEmployeePostTemplateScheduleWeekReport);
            this.dayScenarioScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultScenarioScheduleDayReport);
            this.weekScenarioScheduleReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultScenarioScheduleWeekReport);
            this.extraShiftAsDefaultOnHidden = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ExtraShiftAsDefaultOnHidden);
            this.dragDropMoveAsDefault = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningDragDropMoveAsDefault);

            // Mode specific
            if (this.isSchedulePlanningMode) {
                this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
                this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
                this.maxNbrOfBreaks = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeMaxNoOfBrakes, this.maxNbrOfBreaks);
                this.useTemplateScheduleStopDate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseStopDateOnTemplate);
                this.placementDefaultPreliminary = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimePlacementDefaultPreliminary);
                this.placementHidePreliminary = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimePlacementHidePreliminary);
                this.calculatePlanningPeriodScheduledTime = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTime);
                this.calculatePlanningPeriodScheduledTimeUseAveragingPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);

                const planningPeriodColorString = SettingsUtility.getStringCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeColors);
                let colors = planningPeriodColorString && planningPeriodColorString !== ';;' ? planningPeriodColorString.split(';') : [];
                // Override default colors
                if (colors.length > 0 && colors[0])
                    this.planningPeriodColors[0] = colors[0];
                if (colors.length > 1 && colors[1])
                    this.planningPeriodColors[1] = colors[1];
                if (colors.length > 2 && colors[2])
                    this.planningPeriodColors[2] = colors[2];

                this.showGrossTimeSetting = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing);
                if (!this.showGrossTimeSetting)
                    this.showTotalCostPermission = false;
                this.showExtraShift = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
                this.showSubstitute = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute);
                this.useMultipleScheduleTypes = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseMultipleScheduleTypes);
                this.tasksAndDeliveriesDayReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultScheduleTasksAndDeliverysDayReport);
                this.tasksAndDeliveriesWeekReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultScheduleTasksAndDeliverysWeekReport);
                this.useShiftRequestPreventTooEarly = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly);
                this.inactivateLending = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningInactivateLending);
                this.useLeisureCodes = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseLeisureCodes);
                this.useAnnualLeave = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAnnualLeave);
            } else if (this.isOrderPlanningMode) {
                this.orderPlanningIgnoreScheduledBreaksOnAssignment = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment);
            }
        });
    }

    private getEmploymentContractShortSubstituteReport(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();
        if (this.isSchedulePlanningMode) {
            this.reportService.getSettingReportCheckPermission(SettingMainType.Company, CompanySettingType.DefaultEmploymentContractShortSubstituteReport, SoeReportTemplateType.TimeEmploymentContract).then(x => {
                deferral.resolve(true);
                if (x) {
                    this.employmentContractShortSubstituteReportId = x.reportId;
                    this.employmentContractShortSubstituteReportName = x.name;
                }

            }).catch(reason => {
                // TODO: Hard coded title
                this.notificationService.showServiceError(reason, "Fel vid hämtning av rapport för kortare vikariat");
                deferral.resolve(false);
            });
        } else {

            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private loadHasEmployeeTemplates(): ng.IPromise<any> {
        return this.scheduleService.hasEmployeeTemplatesOfTypeSubstituteShifts().then(x => {
            this.hasEmployeeTemplates = x;
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];

        // Common
        settingTypes.push(UserSettingType.TimeSchedulePlanningDisableAutoLoad);
        settingTypes.push(UserSettingType.TimeSchedulePlanningStartWeek);
        settingTypes.push(UserSettingType.TimeSchedulePlanningCalendarViewCountType);
        settingTypes.push(UserSettingType.TimeSchedulePlanningDisableCheckBreakTimesWarning);
        settingTypes.push(UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning);
        settingTypes.push(UserSettingType.TimeSchedulePlanningDefaultShiftStyle);
        settingTypes.push(UserSettingType.TimeSchedulePlanningDayViewDefaultGroupBy);
        settingTypes.push(UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy);
        settingTypes.push(UserSettingType.TimeSchedulePlanningScheduleViewDefaultGroupBy);
        settingTypes.push(UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy);

        // Mode specific
        if (this.isSchedulePlanningMode) {
            settingTypes.push(UserSettingType.TimeSchedulePlanningDefaultView);
            settingTypes.push(UserSettingType.TimeSchedulePlanningDefaultInterval);
            settingTypes.push(UserSettingType.TimeSchedulePlanningShowEmployeeList);
            settingTypes.push(UserSettingType.TimeSchedulePlanningDisableTemplateScheduleWarning);
            settingTypes.push(UserSettingType.StaffingNeedsDayViewShowDiagram);
            settingTypes.push(UserSettingType.StaffingNeedsDayViewShowDetailedSummary);
            settingTypes.push(UserSettingType.StaffingNeedsScheduleViewShowDetailedSummary);
        } else if (this.isOrderPlanningMode) {
            settingTypes.push(UserSettingType.BillingOrderPlanningDefaultView);
            settingTypes.push(UserSettingType.BillingOrderPlanningDefaultInterval);
            settingTypes.push(UserSettingType.BillingOrderPlanningShiftInfoTopRight);
            settingTypes.push(UserSettingType.BillingOrderPlanningShiftInfoBottomLeft);
            settingTypes.push(UserSettingType.BillingOrderPlanningShiftInfoBottomRight);
        }

        return this.coreService.getUserSettings(settingTypes).then(x => {
            // Common
            this.disableAutoLoad = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningDisableAutoLoad);
            if (!this.disableAutoLoad && this.planningTab !== PlanningTabs.SchedulePlanning)
                this.disableAutoLoad = true;
            this.setInitialHiddenEmployeeFilter = this.disableAutoLoad;
            this.showFilters = this.disableAutoLoad;
            this.startWeek = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningStartWeek);
            this.calendarViewCountByEmployee = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningCalendarViewCountType);

            this.disableCheckBreakTimesWarning = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningDisableCheckBreakTimesWarning);
            this.disableBreaksWithinHolesWarning = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning);
            this.defaultShiftStyle = this.shiftStyle = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningDefaultShiftStyle, this.defaultShiftStyle);
            this.dayViewDefaultGroupBy = this.dayViewGroupBy = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningDayViewDefaultGroupBy);
            this.dayViewDefaultSortBy = this.dayViewSortBy = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy);
            this.scheduleViewDefaultGroupBy = this.scheduleViewGroupBy = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningScheduleViewDefaultGroupBy);
            this.scheduleViewDefaultSortBy = this.scheduleViewSortBy = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy);

            this.tadDayViewGroupBy = this.tadDayViewDefaultGroupBy;
            this.tadDayViewSortBy = this.tadDayViewDefaultSortBy;
            this.tadScheduleViewGroupBy = this.tadScheduleViewDefaultGroupBy;

            // Module specific
            if (this.isSchedulePlanningMode) {
                this.defaultView = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningDefaultView, this.defaultView, true);
                this.defaultInterval = SettingsUtility.getIntUserSetting(x, UserSettingType.TimeSchedulePlanningDefaultInterval, this.defaultInterval);
                this.defaultShowEmployeeList = this.showEmployeeList = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningShowEmployeeList);
                if (this.showEmployeeList) {
                    if (this.hasCurrentViewModifyPermission)
                        this.showEmployeeListFilters = true;
                    else
                        this.showEmployeeList = false;
                }
                this.disableTemplateScheduleWarning = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningDisableTemplateScheduleWarning);

                this.staffingNeedsDayViewShowDiagram = SettingsUtility.getBoolUserSetting(x, UserSettingType.StaffingNeedsDayViewShowDiagram);
                this.staffingNeedsDayViewShowDetailedSummary = SettingsUtility.getBoolUserSetting(x, UserSettingType.StaffingNeedsDayViewShowDetailedSummary);
                this.staffingNeedsScheduleViewShowDetailedSummary = SettingsUtility.getBoolUserSetting(x, UserSettingType.StaffingNeedsScheduleViewShowDetailedSummary);
            } else if (this.isOrderPlanningMode) {
                this.defaultView = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingOrderPlanningDefaultView, this.defaultView, true);
                this.defaultInterval = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingOrderPlanningDefaultInterval, this.defaultInterval);

                this.orderPlanningShiftInfoTopRight = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingOrderPlanningShiftInfoTopRight, this.orderPlanningShiftInfoTopRight, true);
                this.orderPlanningShiftInfoBottomLeft = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingOrderPlanningShiftInfoBottomLeft, this.orderPlanningShiftInfoBottomLeft, true);
                this.orderPlanningShiftInfoBottomRight = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingOrderPlanningShiftInfoBottomRight, this.orderPlanningShiftInfoBottomRight, true);

                this.showAllEmployees = true;
            }

            if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                this.selectedVisibleDays = 1;
            else
                this.selectedVisibleDays = this.defaultInterval ? this.defaultInterval : TermGroup_TimeSchedulePlanningVisibleDays.Week;

            // Default view
            if (soeConfig.view === 'schedule') {
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Calendar)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Calendar;
                else if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Day;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Schedule;
            } else if (soeConfig.view === 'template') {
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.TemplateDay;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.TemplateSchedule;
            } else if (soeConfig.view === 'employeepost') {
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.EmployeePostsDay;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule;
            } else if (soeConfig.view === 'scenario') {
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.ScenarioDay;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;
            } else if (soeConfig.view === 'standby') {
                this.displayMode = TimeSchedulePlanningDisplayMode.User;
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.StandbyDay;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.StandbySchedule;
            } else if (soeConfig.view === 'tasksanddeliveries') {
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule;
            } else if (soeConfig.view === 'staffingneeds') {
                if (this.defaultView === TermGroup_TimeSchedulePlanningViews.Day)
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay;
                else
                    this.viewDefinition = TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule;
            } else {
                this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Schedule;
            }

            // No employee list in standby view
            if (this.isStandbyView && this.showEmployeeList)
                this.showEmployeeList = false;
        });
    }

    private loadUserAndCompanysSettings(): ng.IPromise<any> {
        if (this.isSchedulePlanningMode) {
            let settingTypes: number[] = [UserSettingType.AccountHierarchyId];

            return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
                if (this.isSchedulePlanningMode) {
                    this.accountHierarchyId = SettingsUtility.getStringUserSetting(x, UserSettingType.AccountHierarchyId, '0');
                    if (this.accountHierarchyId === '0') {
                        this.allAccountsSelected = true;
                        // User has selected all accounts
                        // Get permitted accounts
                        this.coreService.getAccountIdsFromHierarchyByUser(this.dateFrom, this.dateTo, false, false, false, !this.showSecondaryAccounts, true, true).then(a => {
                            // Will only fetch last level
                            this.accountHierarchyId = a.join('-');
                            // In loadAccountDims() top level will be removed, therefore add a dummy here
                            this.accountHierarchyId = '0-' + this.accountHierarchyId;
                            this.setUserAccountId();
                        });
                    } else {
                        this.setUserAccountId();
                    }
                }
            });
        }
        else {
            let deferral = this.$q.defer<any>();
            deferral.resolve();
            return deferral.promise;
        }
    }

    private setUserAccountId() {
        // Set user account to current user setting
        if (this.useAccountHierarchy && this.accountHierarchyId) {
            const userAccountIds: number[] = this.accountHierarchyId.split('-').map(Number);
            if (userAccountIds.length > 0)
                this.userAccountId = _.last(userAccountIds);
        }
    }

    private parseSelectableInformationSettings(settingsString: string) {
        // Set default
        this.selectableInformationSettings = new TimeSchedulePlanningSettingsDTO(true);

        if (!settingsString)
            return;

        // Override default with loaded settings
        angular.extend(this.selectableInformationSettings, JSON.parse(settingsString));
    }

    private loadSettingsForView() {
        let settingType: UserSettingType;
        switch (this.viewDefinition) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsCalendarView;
                break;
            case TermGroup_TimeSchedulePlanningViews.Day:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.Schedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTemplateDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTemplateScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsEmployeePostDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsEmployeePostScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScenarioDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScenarioScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStandbyDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStandbyScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTasksAndDeliveriesDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTasksAndDeliveriesScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStaffingNeedsDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStaffingNeedsScheduleView;
                break;
        }

        return this.coreService.getUserSettings([settingType]).then(x => {
            this.parseSelectableInformationSettings(SettingsUtility.getStringUserSetting(x, settingType));
            if (this.selectableInformationSettings.showPlanningPeriodSummary) {
                this.loadCurrentPlanningPeriod();
            }
            if (this.selectableInformationSettings.showAnnualLeaveBalance) {
                this.loadAnnualLeaveBalance = true;
            }
        });
    }

    private saveSelectableInformationSettings() {
        let settingType: UserSettingType;
        switch (this.viewDefinition) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsCalendarView;
                break;
            case TermGroup_TimeSchedulePlanningViews.Day:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.Schedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTemplateDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTemplateScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsEmployeePostDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsEmployeePostScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScenarioDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScenarioScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStandbyDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStandbyScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTasksAndDeliveriesDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsTasksAndDeliveriesScheduleView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStaffingNeedsDayView;
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                settingType = UserSettingType.TimeSchedulePlanningSelectableInformationSettingsStaffingNeedsScheduleView;
                break;
        }

        this.coreService.saveStringSetting(SettingMainType.User, settingType, JSON.stringify(this.selectableInformationSettings));
    }

    private getHiddenEmployeeId() {
        this.sharedScheduleService.getHiddenEmployeeId().then((id) => {
            this.hiddenEmployeeId = id;
        });
    }

    private getVacantEmployeeIds() {
        this.sharedScheduleService.getVacantEmployeeIds().then((ids) => {
            this.vacantEmployeeIds = ids;
        });
    }

    private loadVisibleDays(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningVisibleDays, false, false, true).then(x => {
            this.visibleDays = x;
        });
    }

    private loadIntervals(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.StaffingNeedsHeadInterval, false, true).then(x => {
            this.intervals = x;
        });
    }

    private loadScenarioHeads(): ng.IPromise<any> {
        return this.scheduleService.getScenarioHeadsDict(this.useAccountHierarchy ? this.getFilteredAccountIds() : null, false).then(x => {
            this.scenarioHeads = x;
            this.scenarioHeads.splice(0, 0, new SmallGenericType(0, this.terms["time.schedule.planning.scenario.noselected"]));

            if (this.timeScheduleScenarioHeadId)
                this.loadScenarioHead();
            else
                this.timeScheduleScenarioHeadId = 0;
        });
    }

    private loadScenarioHead(): ng.IPromise<any> {
        this.startWork("core.loading");
        return this.scheduleService.getScenarioHead(this.timeScheduleScenarioHeadId, true, true).then(x => {
            this.scenarioHead = x;

            if (this.scenarioHead?.dateFrom) {
                this.forceNoLoadData = true;
                // If not set to custom, dateFrom will be changed to monday
                if (!this.scenarioHead.dateFrom.isBeginningOfWeek())
                    this.selectedVisibleDays = TermGroup_TimeSchedulePlanningVisibleDays.Custom;
                this.dateFrom = this.scenarioHead.dateFrom;
                this.dateTo = this.scenarioHead.dateTo;
                this.forceNoLoadData = false;
            } else {
                this.dateFrom = this.isScenarioDayView ? new Date().beginningOfDay() : new Date().beginningOfWeek();
                this.selectedVisibleDays = this.isScenarioDayView ? TermGroup_TimeSchedulePlanningVisibleDays.Day : TermGroup_TimeSchedulePlanningVisibleDays.Week;
            }

            this.scenarioDays = this.scenarioHead ? CalendarUtility.getDaysBetweenDates(this.scenarioHead.dateTo, this.scenarioHead.dateFrom) : 0;

            this.viewDefinition = this.scenarioHead ? TermGroup_TimeSchedulePlanningViews.ScenarioComplete : TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;

            this.loadEmployees(false).then(() => {
                this.loadData('loadScenarioHead');
            });
        });
    }

    private loadCategories(reloadShiftTypes: boolean): ng.IPromise<any> {
        if (reloadShiftTypes)
            this.loadUserShiftTypes(true);

        return this.coreService.getCategoriesForRoleFromType(this.employeeId, SoeCategoryType.Employee, this.isAdmin, this.showSecondaryCategories, false).then(x => {
            this.categories = [];
            x.forEach(y => {
                this.categories.push({ id: y.id, label: y.name });
            });
        });
    }

    private loadEmployees(stopProgressWhenDone: boolean = true): ng.IPromise<any> {
        this.loadingEmployees = true;

        var deferral = this.$q.defer();
        this.sharedScheduleService.getEmployeesForPlanning(null, null, true, this.selectableInformationSettings.showInactiveEmployees, true, this.selectableInformationSettings.showAvailability, false, this.dateFrom, this.dateTo, this.showSecondaryCategories || this.showSecondaryAccounts, this.displayMode).then(x => {
            if (this.isScenarioView && this.scenarioHead) {
                this.allEmployees = x.filter(y => this.scenarioHead.employees.map(e => e.employeeId).includes(y.employeeId));
            } else {
                this.allEmployees = x;
            }
            this.setEmployedEmployees();
            this.inactiveEmployeeIds = this.allEmployees.filter(e => !e.active).map(e => e.employeeId);
            this.setEmployeeData();

            let loadData: boolean = (!this.disableAutoLoad || this.delayLoadData);
            let abortLoad = this.filterEmployees('loadEmployees', !loadData, stopProgressWhenDone);
            if (abortLoad && loadData) {
                loadData = false;
                this.showEmployeeRemovedFromFilterMessage();
            }

            this.loadingEmployees = false;
            this.employeesLoaded = true;
            deferral.resolve();

            if (this.delayFilterByUserSelection) {
                this.delayFilterByUserSelection = false;
                if (this.selectedUserSelection)
                    this.filterByUserSelection();
            }

            if (loadData)
                this.loadData('loadEmployees', true);
            else if (stopProgressWhenDone)
                this.completedWork(null, true);
        });

        return deferral.promise;
    }

    private reloadEmployees(employeeIds: number[], render: boolean): ng.IPromise<any> {
        this.loadingEmployees = true;

        return this.sharedScheduleService.getEmployeesForPlanning(employeeIds, null, !employeeIds || employeeIds.length === 0, this.selectableInformationSettings.showInactiveEmployees, true, this.selectableInformationSettings.showAvailability, false, this.dateFrom, this.dateTo, this.showSecondaryCategories || this.showSecondaryAccounts, this.displayMode).then(x => {
            // Replace existing employee with new (updating information)
            x.forEach(emp => {
                let employee = this.getEmployeeById(emp.employeeId);
                if (employee)
                    _.pull(this.allEmployees, employee);
                this.allEmployees.push(emp);
                this.setEmployedEmployees();

                if (!render) {
                    // If render flag is set, all employees will be rendered in filterEmployees() below.
                    // If render flag is not set, only redraw current employee.
                    let rows = this.scheduleHandler.getEmployeeRows(emp.employeeId);
                    if (rows.length > 0) {
                        this.calculateEmployeeWorkTimes(emp);
                        rows.forEach(row => {
                            this.scheduleHandler.updateEmployeeRow(row, emp);
                            this.scheduleHandler.updateEmployeeInfo(emp);
                        });
                    }
                }
            });

            if (!render)
                this.calculateTimes();

            if (this.selectableInformationSettings.showAnnualLeaveBalance) {
                this.loadAnnualLeaveBalance = true;
            }

            this.setEmployeeData();
            this.filterEmployees('reloadEmployees', render);

            this.loadingEmployees = false;
        });
    }

    private loadEmployeePosts(verifyEmployeesAreLoaded: boolean) {
        if (verifyEmployeesAreLoaded && !this.hasEmployeesLoaded) {
            this.loadEmployees(false).then(() => {
                this.loadEmployeePosts(false);
            });
            return;
        }

        this.loadingEmployees = true;

        this.scheduleService.getEmployeePostsForPlanning(null, this.dateFrom, this.dateTo).then(x => {
            // Add employee posts to list of employees
            this.allEmployees = _.concat(this.allEmployees, x);
            this.setEmployedEmployees();

            this.employeePostsLoaded = true;

            this.setEmployeeData();

            this.loadingEmployees = false;

            if (!this.disableAutoLoad || this.delayLoadData)
                this.loadData('loadEmployeePosts', true);
            else
                this.completedWork(null, true);
        });
    }

    private reloadEmployeePosts(employeePostIds: number[], render: boolean): ng.IPromise<any> {
        this.loadingEmployees = true;

        return this.scheduleService.getEmployeePostsForPlanning(employeePostIds, this.dateFrom, this.dateTo).then(x => {
            // Replace existing employee post with new (updating information)
            x.forEach(emp => {
                let employeePost = this.getEmployeePostById(emp.employeePostId);
                if (employeePost)
                    _.pull(this.allEmployees, employeePost);
                this.allEmployees.push(emp);
                this.setEmployedEmployees();

                if (render) {
                    this.calculateEmployeeWorkTimes(emp);
                    this.calculateTimes();
                    this.scheduleHandler.updateEmployeeInfo(emp);
                }

                this.setEmployeeToolTip(emp);
            });

            this.setEmployeeData();

            this.loadingEmployees = false;
        });
    }

    private loadEmployeeAvailability() {
        this.loadingAvailability = true;

        this.sharedScheduleService.getEmployeeAvailability(this.employedEmployees.map(e => e.employeeId)).then(x => {
            x.forEach(employee => {
                let existingEmployee = this.getEmployeeById(employee.employeeId);
                if (existingEmployee) {
                    existingEmployee.available = employee.available;
                    existingEmployee.unavailable = employee.unavailable;
                }
            });

            this.loadingAvailability = false;
            this.renderBody('loadEmployeeAvailability');
        });
    }

    private getAvailableEmployees() {
        this.$timeout(() => {
            let selectedShifts = this.scheduleHandler.getSelectedShifts();
            if (this.employeeListFilterOnSelectedShift && selectedShifts.length > 0) {
                this.employeeListFilterEmployeeIds = this.getVisibleEmployeeIds();
                this.employeeListFilterShiftIds = selectedShifts.map(s => s.timeScheduleTemplateBlockId);
            } else {
                this.filteringEmployeesDone(null);
            }
        });
    }

    private getTimeScheduleEmployeePeriodId(employeeId: number, date: Date): ng.IPromise<number> {
        let deferral = this.$q.defer<number>();

        this.scheduleService.getTimeScheduleEmployeePeriodId(employeeId, date).then(x => {
            if (x === 0) {
                // Employee has no employee period
                let keys: string[] = [
                    "time.schedule.planning.editassignment.cannotcreate",
                    "time.schedule.planning.noplacement"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.editassignment.cannotcreate"], terms["time.schedule.planning.noplacement"], SOEMessageBoxImage.Forbidden);
                });
            }

            deferral.resolve(x);
        });

        return deferral.promise;
    }

    // Using debounce because of multiple calls from setDateRange sometimes
    private loadStaffingNeed = _.debounce(() => {
        if (this.scheduleHandler.isRendering) {
            this.delayLoadStaffingNeed = true;
            this.progressMessage = this.terms["time.schedule.planning.selectableinformation.followup.delayloading"];
            this.progressBusy = true;
            return;
        } else {
            this.delayLoadStaffingNeed = false;
        }

        this.staffingNeedData = []

        if (!this.selectableInformationSettings.followUpOnNeed &&
            !this.selectableInformationSettings.followUpOnNeedFrequency &&
            !this.selectableInformationSettings.followUpOnNeedRowFrequency &&
            !this.selectableInformationSettings.followUpOnBudget && !this.selectableInformationSettings.showBudget &&
            !this.selectableInformationSettings.followUpOnForecast && !this.selectableInformationSettings.showForecast &&
            !this.selectableInformationSettings.followUpOnTemplateSchedule && !this.selectableInformationSettings.showTemplateSchedule &&
            !this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost && !this.selectableInformationSettings.showTemplateScheduleForEmployeePost &&
            !this.selectableInformationSettings.followUpOnSchedule && !this.selectableInformationSettings.showSchedule &&
            !this.selectableInformationSettings.followUpOnTime && !this.selectableInformationSettings.showTime) {
            this.showSelectableInformation(true);
            return;
        }

        const employeeIds = this.getVisibleEmployeeIds();
        if (employeeIds.length === 0)
            return;

        this.loadingStaffingNeed = true;
        this.progressMessage = this.terms["time.schedule.planning.selectableinformation.followup.loading"];
        this.progressBusy = true;

        this.scheduleService.generateStaffingNeedsHeadsForInterval(TermGroup_StaffingNeedHeadsFilterType.ActualNeed, this.dateFrom, this.dateTo, TermGroup_TimeSchedulePlanningFollowUpCalculationType.All, this.selectableInformationSettings.followUpOnNeed, this.selectableInformationSettings.followUpOnNeedFrequency || this.selectableInformationSettings.followUpShowCalculationTypeSalesTime, this.selectableInformationSettings.followUpOnNeedRowFrequency, (this.selectableInformationSettings.followUpOnBudget || this.selectableInformationSettings.showBudget), (this.selectableInformationSettings.followUpOnForecast || this.selectableInformationSettings.showForecast), (this.selectableInformationSettings.followUpOnTemplateSchedule || this.selectableInformationSettings.showTemplateSchedule), (this.selectableInformationSettings.followUpOnSchedule || this.selectableInformationSettings.showSchedule), (this.selectableInformationSettings.followUpOnTime || this.selectableInformationSettings.showTime), this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost || this.selectableInformationSettings.showTemplateScheduleForEmployeePost, this.selectableInformationSettings.followUpAccountDimId || 0, this.selectableInformationSettings.followUpAccountId || 0, employeeIds, this.getFilteredEmployeePostIds(), this.timeScheduleScenarioHeadId, this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge, this.isFilteredOnShiftType ? this.getFilteredShiftTypeIds() : null, this.isCommonScheduleView).then(x => {
            this.staffingNeedData = x;
            this.staffingNeedOriginalSummaryRow = CoreUtility.cloneDTO(this.getStaffingNeedsSummaryRow());

            this.loadingStaffingNeed = false;
            this.calculateTimes();
            this.scheduleHandler.renderScheduleSummary();
            this.stopProgress();
            this.$timeout(() => {
                if (this.showPlanningAgChart)
                    this.renderPlanningAgChart(true);
                if (this.showPlanningFollowUpTable)
                    this.renderPlanningFollowUpTable(true);
            });
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.loadingStaffingNeed = false;
        });
    }, 500, { leading: false, trailing: true });

    private getStaffingNeedsSummaryRow(): StaffingStatisticsIntervalRow {
        if (this.staffingNeedData && this.staffingNeedData.length > 0) {
            let summaryData: StaffingStatisticsInterval = _.last(this.staffingNeedData);
            if (summaryData.rows.length > 0)
                return summaryData.rows[0];
        }

        return null;
    }

    private setStaffingNeedsSummaryRow(summaryRow: StaffingStatisticsIntervalRow) {
        if (summaryRow && this.getStaffingNeedsSummaryRow()) {
            angular.extend(_.last(this.staffingNeedData).rows[0], summaryRow);
            this.renderPlanningFollowUpTable(true);
        }
    }

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountIdsFromHierarchyByUser(this.dateFrom, this.dateTo, false, false, false, !this.showSecondaryAccounts, true, true).then(x => {
            this.validAccountIds = x;
        });
    }

    private loadAccountDims(keepFilters: boolean): ng.IPromise<any> {
        let accountDimFilters: { dimId: number, accountIds: number[] }[];
        if (keepFilters) {
            accountDimFilters = [];
            this.accountDims.filter(d => !d['hidden'] && d.selectedAccounts.length > 0).forEach(dim => {
                accountDimFilters.push({ dimId: dim.accountDimId, accountIds: dim.selectedAccounts.map(a => a.accountId) });
            });
        }

        return this.sharedScheduleService.getAccountDimsForPlanning(!this.showSecondaryAccounts, true, this.displayMode, true).then(x => {
            this.accountDims = x;

            if (this.accountDims.length > 0) {
                let index = 11;
                this.accountDims.forEach(accountDim => {
                    accountDim.groupByIndex = index;
                    index++;
                });

                // Pre filter on account dims based on user setting
                if (this.useAccountHierarchy && this.accountHierarchyId) {
                    let accounts = this.accountHierarchyId.split('-');

                    // If user selected 'All accounts', a dummy account with id 0 is added, in that case do not pre filter
                    // showSecondaryAccounts was added in #61375, but does not seem to be necessary anymore.
                    //let filter: boolean = !this.showSecondaryAccounts && ((accounts.length > 0 && accounts[0] !== '0') || accounts.length === 2);
                    let filter: boolean = (accounts.length > 0 && accounts[0] !== '0') || accounts.length === 2;

                    // 'accounts' will contain all account dims.
                    // Remove accounts connected to dimensions not specified as UseInSchedulePlanning.
                    let firstDimLevel = this.accountDims[0].level;
                    if (firstDimLevel > 1) {
                        for (let i = 1; i < firstDimLevel; i++) {
                            if (accounts.length > 0)
                                _.pullAt(accounts, 0);
                        }
                    }

                    let defaultDim = this.accountDims.find(a => a.accountDimId === this.defaultEmployeeAccountDimId);
                    if (defaultDim && !this.allAccountsSelected)
                        this.isDefaultAccountDimLevel = defaultDim.accounts.map(a => a.accountId).includes(this.userAccountId);

                    if (filter) {
                        for (let i = 0; i < this.accountDims.length; i++) {
                            let dim = this.accountDims[i];
                            let account = accounts.length > i ? dim.accounts.find(a => a.accountId === parseInt(accounts[i], 10)) : null;
                            if (!account && dim.accounts.length === 1)
                                account = dim.accounts[0];

                            if (account) {
                                dim['hidden'] = true;
                                dim.filteredAccounts = [account];
                                dim.selectedAccounts = [{ accountId: account.accountId }];
                                this.filterAccounts(dim, true);
                            }
                        }
                    }
                }

                // Reselect previously filtered accounts
                if (keepFilters && accountDimFilters && accountDimFilters.length > 0) {
                    accountDimFilters.forEach(filter => {
                        let dim = this.accountDims.find(d => d.accountDimId === filter.dimId);
                        if (dim && !dim['hidden']) {
                            let dimAccountIds = dim.filteredAccounts.map(a => a.accountId);
                            dim.selectedAccounts = [];
                            filter.accountIds.forEach(accountId => {
                                if (dimAccountIds.includes(accountId)) {
                                    dim.selectedAccounts.push({ accountId: accountId });
                                    this.filterAccounts(dim, false);
                                }
                            });
                        }
                    });
                }
            }

            if (this.isScenarioView)
                this.loadScenarioHeads();
        });
    }

    private loadShiftTypeAccountDim(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeAccountDim(false).then(x => {
            this.shiftTypeAccountDim = x;
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(true, true, false, false, false, this.useAccountHierarchy).then(x => {
            this.allShiftTypes = x.filter(s => !s.accountIsNotActive);
            // Insert empty shift type
            this.translationService.translate("core.notselected").then((term) => {
                var shiftType: ShiftTypeDTO = new ShiftTypeDTO();
                shiftType.shiftTypeId = 0;
                shiftType.name = term;
                shiftType.color = Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
                this.allShiftTypes.splice(0, 0, shiftType);
            });
        });
    }

    private loadUserShiftTypes(setShiftTypes: boolean): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeIdsForUser(this.employeeId, this.isAdmin, this.showSecondaryCategories || this.showSecondaryAccounts, this.dateFrom, this.dateTo).then(x => {
            this.shiftTypeIds = x;

            if (setShiftTypes)
                this.setShiftTypes();
        });
    }

    private loadTimeScheduleTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeScheduleTypes(false, true, true).then(x => {
            this.timeScheduleTypes = x;

            // Add empty row
            var t = new TimeScheduleTypeSmallDTO();
            t.timeScheduleTypeId = 0;
            t.name = '';
            this.timeScheduleTypes.splice(0, 0, t);
        });
    }

    private setupStatusFilter(keepSelected: boolean) {
        if (!keepSelected)
            this.selectedStatuses = [];

        this.statuses = [];
        let statusIds = this.selectedStatuses.map(s => s.id);

        if (this.isCalendarView) {
            this.statuses.push({ id: PlanningStatusFilterItems.Open, label: this.terms["time.schedule.planning.shiftstatus.open"] });
            this.statuses.push({ id: PlanningStatusFilterItems.Assigned, label: this.terms["time.schedule.planning.shiftstatus.assigned"] });
        }

        this.statuses.push({ id: PlanningStatusFilterItems.AbsenceRequested, label: this.terms["time.schedule.planning.shiftstatus.absencerequested"], disabled: statusIds.includes(PlanningStatusFilterItems.HideAbsenceRequested) });
        if (!this.isCalendarView)
            this.statuses.push({ id: PlanningStatusFilterItems.HideAbsenceRequested, label: this.terms["time.schedule.planning.shiftstatus.hideabsencerequested"], disabled: statusIds.includes(PlanningStatusFilterItems.AbsenceRequested) });
        this.statuses.push({ id: PlanningStatusFilterItems.AbsenceApproved, label: this.terms["time.schedule.planning.shiftstatus.absenceapproved"], disabled: statusIds.includes(PlanningStatusFilterItems.HideAbsenceApproved) });
        if (!this.isCalendarView)
            this.statuses.push({ id: PlanningStatusFilterItems.HideAbsenceApproved, label: this.terms["time.schedule.planning.shiftstatus.hideabsenceapproved"], disabled: statusIds.includes(PlanningStatusFilterItems.AbsenceApproved) });

        if (this.preliminaryPermission) {
            this.statuses.push({ id: PlanningStatusFilterItems.Preliminary, label: this.terms["time.schedule.planning.shiftstatus.preliminary"], disabled: statusIds.includes(PlanningStatusFilterItems.HidePreliminary) });
            if (!this.isCalendarView)
                this.statuses.push({ id: PlanningStatusFilterItems.HidePreliminary, label: this.terms["time.schedule.planning.shiftstatus.hidepreliminary"], disabled: statusIds.includes(PlanningStatusFilterItems.Preliminary) });
        }

        this.statuses.push({ id: PlanningStatusFilterItems.Wanted, label: this.terms["time.schedule.planning.shiftstatus.wanted"] });
        this.statuses.push({ id: PlanningStatusFilterItems.Unwanted, label: this.terms["time.schedule.planning.shiftstatus.unwanted"] });
    }

    private loadDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(false, false).then(x => {
            this.deviationCauses = [];
            x.forEach(y => {
                this.deviationCauses.push({ id: y.id, label: y.name });
            });
        });
    }

    private setupBlockTypesFilter() {
        this.blockTypes = [];

        this.blockTypes.push({ id: TermGroup_TimeScheduleTemplateBlockType.Schedule, label: this.terms["time.schedule.planning.blocktype.schedule"] });
        if (this.isOrderPlanningMode)
            this.blockTypes.push({ id: TermGroup_TimeScheduleTemplateBlockType.Order, label: this.terms["time.schedule.planning.blocktype.order"] });
        if (this.bookingReadPermission)
            this.blockTypes.push({ id: TermGroup_TimeScheduleTemplateBlockType.Booking, label: this.terms["time.schedule.planning.blocktype.booking"] });
        if (this.standbyShiftsReadPermission && !this.isOrderPlanningMode)
            this.blockTypes.push({ id: TermGroup_TimeScheduleTemplateBlockType.Standby, label: this.terms["time.schedule.planning.blocktype.standby"] });
        if (this.onDutyShiftsReadPermission && !this.isOrderPlanningMode)
            this.blockTypes.push({ id: TermGroup_TimeScheduleTemplateBlockType.OnDuty, label: this.terms["time.schedule.planning.blocktype.onduty"] });

        if (this.isOrderPlanningMode)
            this.selectedBlockTypes = this.blockTypes.filter(b => b.id !== TermGroup_TimeScheduleTemplateBlockType.Schedule);
    }

    private loadShiftStyles(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningShiftStyle, false, false, true).then(x => {
            this.shiftStyles = x;
        });
    }

    private loadBreakTimeCodes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeCodeBreaks(false).then(x => {
            this.breakTimeCodes = x;
        });
    }

    private loadLeisureCodes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeLeisureCodesSmall().then(x => {
            this.leisureCodes = x;
        });
    }

    private loadAbsenceTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeScheduleTemplateBlockAbsenceType, false, true, false).then(x => {
            this.absenceTypes = x;
        });
    }

    private loadFollowUpCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeSchedulePlanningFollowUpCalculationType, false, true).then(x => {
            this.gaugeSalesLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales).name;
            this.gaugeHoursLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours).name;
            this.gaugeCostLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost).name;
            this.gaugeSalaryPercentLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent).name;
            this.gaugeLPATLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT).name;
            this.gaugeFPATLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT).name;
            this.gaugeBPATLabel = x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT).name;

            // Need to be sorted correctly
            this.followUpCalculationTypes = [];
            this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales));
            this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours));
            this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost));
            this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent));
            this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT));
            this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT));
            //this.followUpCalculationTypes.push(x.find(t => t.id === TermGroup_TimeSchedulePlanningFollowUpCalculationType.BPAT));
            // Currently BPAT is not supported, so don't add it to the list
        });
    }

    private loadHeadStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.StaffingNeedsHeadStatus, false, false).then(x => {
            this.headStatuses = x;
        });
    }

    private loadStaffingNeedsFilterTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.StaffingNeedHeadsFilterType, false, true).then(x => {
            this.needsFilterTypes = x;
        });
    }

    private loadTimeScheduleTaskTypes(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTaskTypesDict(false, true).then(x => {
            this.timeScheduleTaskTypes = x;
        });
    }

    private loadFrequencyTasks(): ng.IPromise<any> {
        this.frequencyTasks = [];

        return this.scheduleService.getTimeScheduleTasksForFrequency(false).then(x => {
            this.frequencyTasks = x;
        });
    }

    private loadAllUnscheduledOrders = _.debounce(() => {
        this.loadAllUnscheduledOrdersData();
    }, 800, { leading: false, trailing: true });

    private loadAllUnscheduledOrdersData() {
        if (!(this.isOrderPlanningMode && (this.isDayView || this.isScheduleView)))
            return;

        this.loadingUnscheduledOrders = true;
        this.orderList = [];

        this.scheduleService.getUnscheduledOrders(this.getFilteredCategoryIds(), this.orderListShowFutureOrders ? null : this.dateTo).then((x: OrderListDTO[]) => {
            this.allUnscheduledOrders = x;
            this.allUnscheduledOrders.forEach(order => {
                this.setOrderListToolTip(order);
            });
            this.filterOrderList();
            this.loadingUnscheduledOrders = false;
        });
    }

    private loadUnscheduledOrders(orderIds: number[]) {
        this.loadingUnscheduledOrders = true;

        this.scheduleService.getUnscheduledOrdersByIds(orderIds).then((orders: OrderListDTO[]) => {
            orderIds.forEach(orderId => {
                _.pullAll(this.allUnscheduledOrders, this.allUnscheduledOrders.filter(o => o.orderId === orderId));
            });

            orders.forEach(order => {
                this.setOrderListToolTip(order);
                this.allUnscheduledOrders.push(order);
            });
            this.filterOrderList();
            this.loadingUnscheduledOrders = false;
        });
    }

    private loadUnscheduledOrder(orderId: number): ng.IPromise<any> {
        this.loadingUnscheduledOrders = true;

        return this.scheduleService.getUnscheduledOrder(orderId).then((order: OrderListDTO) => {
            _.pullAll(this.allUnscheduledOrders, this.allUnscheduledOrders.filter(o => o.orderId === orderId));
            if (order) {
                this.setOrderListToolTip(order);
                this.allUnscheduledOrders.push(order);
            }

            this.filterOrderList();
            this.loadingUnscheduledOrders = false;

            // Reset order info on affected shifts
            this.shifts.filter(s => s.isOrder && s.order.orderId === orderId).forEach(shift => {
                shift.order = order;
                if (shift.order)
                    this.setOrderListToolTip(shift.order);
            });
        });
    }

    // OTHER DATA SERVICE CALLS

    private delayLoadData: boolean = false;
    private loadData(source: string, forceLoadAll: boolean = false) {
        if (this.dateToChanged) {
            this.setDateRange(true);
            return;
        }

        this.subsetOfShiftsLoaded = this.isFilteredOnCategory || this.isFilteredOnEmployee;

        if (this.loadingEmployees) {
            // Employees needs to be loaded before loading shifts.
            // If user is too fast clicking load shifts, we need to wait until the employees are fetched.
            // After employees are loaded, it will automatically load shifts.
            this.delayLoadData = true;
            return;
        }

        this.scheduleHandler.rememberVerticalScroll();
        this.loadingShifts = true;

        if (forceLoadAll)
            this.clearEmployeeIdsForShiftLoad();

        switch (this.viewDefinition) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                this.loadPeriods();
                break;
            case TermGroup_TimeSchedulePlanningViews.Day:
            case TermGroup_TimeSchedulePlanningViews.Schedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
                this.loadShifts();
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
                this.loadTemplateShifts();
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
                this.loadEmployeePostShifts();
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
                this.loadTasks();
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                this.loadStaffingNeedsHeads();
                break;
        }
    }

    // Use debounce
    // This will enable fast clicking on increase/decrease date buttons without loading after each click
    private loadPeriods = _.debounce(() => {
        if (!this.hasEmployeesLoaded)
            return;

        this.startWork("core.loading");

        this.filteredButNotLoaded = false;
        if (this.showFilters)
            this.toggleFilters();

        // TODO: Check preliminary parameter
        this.scheduleService.getShiftPeriods(this.dateFrom, this.dateTo, this.employeeId, this.displayMode, [this.isOrderPlanningMode ? TermGroup_TimeScheduleTemplateBlockType.Order : TermGroup_TimeScheduleTemplateBlockType.Schedule], this.getFilteredEmployeeIds(), this.isFilteredOnShiftType ? this.getFilteredShiftTypeIds() : null, this.isFilteredOnDeviationCause ? this.getFilteredDeviationCauseIds() : null, this.showSummaryInCalendarView && (this.selectableInformationSettings.showGrossTime || this.selectableInformationSettings.showTotalCost), false, this.preliminaryPermission, this.selectableInformationSettings.showTotalCost, null, this.timeScheduleScenarioHeadId, this.selectableInformationSettings.showWeekendSalary).then(x => {
            this.periods = x;

            // Count all open and assigned shifts
            this.nbrOfPeriodShifts = _.sumBy(this.periods, p => p.open) + _.sumBy(this.periods, p => p.assigned);

            // Filter periods and count all open and assigned shifts
            this.filterPeriods();
            let nbrOfPeriodShifts = 0;
            this.periods.forEach(p => {
                // We compare like this cause we have trouble with timezones and also what hours are used. Sometimes its 00:00:00, sometimes its 23:59:59
                let date = this.dates.find(d => d.date.getFullYear() === p.date.getFullYear() && d.date.getMonth() === p.date.getMonth() && d.date.getDate() === p.date.getDate());
                if (date) {
                    date.shiftPeriod = p;
                    nbrOfPeriodShifts += (p.open + p.assigned);
                }
            });
            this.nbrOfVisiblePeriodShifts = nbrOfPeriodShifts;

            this.setFirstLoadHasOccurred();
            this.renderBody('loadPeriods');
            this.grossNetAndCostLoaded = (this.selectableInformationSettings.showGrossTime || this.selectableInformationSettings.showTotalCost);
        });
    }, 300, { leading: false, trailing: true });

    private loadPeriodsGrossNetAndCost() {
        if (!this.showSummaryInCalendarView || this.grossNetAndCostLoaded || !this.periods || this.periods.length === 0)
            return;

        this.loadingGrossNetAndCost = true;
        this.grossNetAndCostLoaded = true;
        this.progressMessage = this.terms["time.schedule.planning.grossnetcost.loading"];
        this.progressBusy = true;

        // TODO: Check preliminary parameter
        this.scheduleService.getShiftPeriodsGrossNetAndCost(this.dateFrom, this.dateTo, this.employeeId, [TermGroup_TimeScheduleTemplateBlockType.Schedule], this.getFilteredEmployeeIds(), this.isFilteredOnShiftType ? this.getFilteredShiftTypeIds() : null, this.isFilteredOnDeviationCause ? this.getFilteredDeviationCauseIds() : null, this.preliminaryPermission, this.selectableInformationSettings.showTotalCost, null, null, this.selectableInformationSettings.showWeekendSalary).then(x => {
            // Convert to typed DTOs
            let grossNetAndCosts = x.map(s => {
                let obj = new ShiftPeriodDTO;
                angular.extend(obj, s);
                obj.date = new Date((<any>obj.date));
                return obj;
            });

            grossNetAndCosts.forEach(g => {
                // We compare like this cause we have trouble with timezones and also what hours are used. Sometimes its 00:00:00, sometimes its 23:59:59
                let date = this.dates.find(d => d.date.getFullYear() === g.date.getFullYear() && d.date.getMonth() === g.date.getMonth() && d.date.getDate() === g.date.getDate());
                if (date) {
                    date.shiftPeriod.grossTime = g.grossTime;
                    date.shiftPeriod.totalCost = g.totalCost;
                    date.shiftPeriod.totalCostIncEmpTaxAndSuppCharge = g.totalCostIncEmpTaxAndSuppCharge;
                }
            });
            this.loadingGrossNetAndCost = false;
            this.stopProgress();
        });
    }

    private loadShifts = _.debounce(() => {
        if (!this.hasEmployeesLoaded || (this.isScenarioView && !this.timeScheduleScenarioHeadId)) {
            this.shifts = [];
            this.filterShifts('loadShifts no employees');
            this.loadingShifts = false;
            this.completedWork(null, true);
            if (!this.isScenarioView || this.existingScenarioSelected)
                this.notificationService.showDialogEx('', this.terms["time.schedule.planning.noemployeesforshifts"], SOEMessageBoxImage.Information);
            return;
        }

        if (!this.loadShiftsSilent) {
            this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.load"];
            this.progressBusy = true;
        }

        if (this.reloadShiftsForSpecifiedEmployeeIds.length === 0)
            this.shifts = [];

        let employeeIds: number[] = this.getEmployeeIdsForShiftLoad();

        // Don't show loading dialog if only one or two employees are affected, it will just be an annoying flash
        if (!this.loadShiftsSilent && employeeIds.length > 2)
            this.startWork("core.loading");

        let includeAbsenceRequests: boolean = true;
        if (this.isScenarioView) {
            // Absence requests are opt-in in scenario view
            if (!this.timeScheduleScenarioHeadId || !this.selectableInformationSettings.showAbsenceRequests)
                includeAbsenceRequests = false;
        }

        this.filteredButNotLoaded = false;
        if (this.showFilters && this.firstLoadHasOccurred)
            this.toggleFilters();

        this.sharedScheduleService.getShifts(this.employeeId, this.dateFrom, this.dateTo, employeeIds, this.planningMode, this.displayMode, this.showSecondaryCategories, true, this.isSchedulePlanningMode && (this.selectableInformationSettings.showGrossTime || this.selectableInformationSettings.showTotalCost), this.preliminaryPermission, this.showTotalCostPermission, true, includeAbsenceRequests, this.isOrderPlanningMode, this.isScenarioView ? this.timeScheduleScenarioHeadId || 0 : null, this.selectableInformationSettings.showWeekendSalary, this.useLeisureCodes).then(x => {
            let shfts: ShiftDTO[] = x;

            this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.process"];

            this.setShiftData(shfts);

            // If shifts are loaded for specified employees,
            // remove existing shifts for those employees then add the new ones
            if (this.reloadShiftsForSpecifiedEmployeeIds.length > 0) {
                this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeId => {
                    _.remove(this.shifts, s => s.employeeId === employeeId);
                });
                this.shifts = this.shifts.concat(shfts);
            } else {
                // All shifts are loaded
                this.shifts = shfts;
            }
            this.setAllShiftsMap();

            if (this.isHiddenEmployeeReadOnly)
                shfts.filter(s => s.employeeId === this.hiddenEmployeeId).forEach(s => s.isReadOnly = true);

            if (this.hasCutOrCopiedShifts) {
                // If copied shifts are reloaded, replace the copied ones with the reloaded ones, to prevent pasting outdated shifts.
                this.cutCopiedShifts.forEach(cutCopy => {
                    let loadedShift = shfts.find(s => s.timeScheduleTemplateBlockId === cutCopy.timeScheduleTemplateBlockId);
                    if (loadedShift) {
                        cutCopy = loadedShift;
                    }
                });
            }

            if (this.isDayView)
                this.setShiftDataForDayView();

            this.grossNetAndCostLoaded = (this.isSchedulePlanningMode && (this.selectableInformationSettings.showGrossTime || this.selectableInformationSettings.showTotalCost));
            this.recalculateEmployeeWorkTimes = true;
            if (this.isGrouped) {
                // Since grouping may have been altered based on modified shifts,
                // employees must be resorted and not only reloaded employees must be rendered.
                this.sortEmployees(false);
                this.reloadShiftsForSpecifiedEmployeeIds = [];
            }
            this.setFirstLoadHasOccurred();
            this.filterShifts('loadShifts');

            if (this.selectableInformationSettings.showCyclePlannedTime && this.isScheduleView)
                this.loadCyclePlannedTime(employeeIds, false);

            if (this.evaluateAllWorkRulesAfterLoadingShifts) {
                this.evaluateAllWorkRulesAfterLoadingShifts = false;
                this.evaluateAllWorkRules();
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }, 300, { leading: false, trailing: true });

    private loadShiftsGrossNetAndCost = _.debounce(() => {
        if (!this.shifts || this.shifts.length === 0)
            return;

        if (this.grossNetAndCostLoaded) {
            this.calculateTimes();
            this.clearShiftToolTips();
            this.renderBody('loadShiftsGrossNetAndCost 1');
            return;
        }

        this.loadingGrossNetAndCost = true;
        this.grossNetAndCostLoaded = true;

        // Show spinners
        this.renderBody('loadShiftsGrossNetAndCost 2', false);
        this.progressBusy = true;
        this.progressMessage = this.terms["time.schedule.planning.grossnetcost.loading"];

        // TODO: Check preliminary parameter
        if (this.isTemplateView) {
            this.scheduleService.getTemplateShiftsGrossNetAndCost(this.dateFrom, this.dateTo, this.getFilteredEmployeeIds(), this.showTotalCostPermission, this.selectableInformationSettings.showWeekendSalary).then(x => {
                this.setGrossNetAndCosts(x);
            });
        } else {
            this.scheduleService.getShiftsGrossNetAndCost(this.employeeId, this.dateFrom, this.dateTo, this.getFilteredEmployeeIds(), this.showSecondaryCategories, true, this.preliminaryPermission, this.showTotalCostPermission, this.timeScheduleScenarioHeadId, this.selectableInformationSettings.showWeekendSalary).then(x => {
                this.setGrossNetAndCosts(x);
            });
        }
    }, 300, { leading: false, trailing: true });

    private setGrossNetAndCosts(x) {
        // Convert to typed DTOs
        let grossNetAndCosts = x.map(s => {
            let obj = new ShiftDTO;
            angular.extend(obj, s);
            return obj;
        });

        grossNetAndCosts.forEach(g => {
            if (this.isTemplateView) {
                // Update all shifts (even in repeating weeks)
                let shfts = this.shifts.filter(s => s.timeScheduleTemplateBlockId === g.timeScheduleTemplateBlockId || s.originalBlockId === g.timeScheduleTemplateBlockId);
                shfts.forEach(shift => this.setGrossNetAndCost(shift, g));
            } else {
                let shift = this.shifts.find(s => s.timeScheduleTemplateBlockId === g.timeScheduleTemplateBlockId);
                if (shift)
                    this.setGrossNetAndCost(shift, g);
            }
        });

        this.loadingGrossNetAndCost = false;
        this.calculateTimes();
        this.clearShiftToolTips();
        this.renderBody('setGrossNetAndCosts', false);
        this.stopProgress();
    }

    private setGrossNetAndCost(shift, grossNetAndCost) {
        shift.grossTime = grossNetAndCost.grossTime;
        shift.totalCost = grossNetAndCost.totalCost;
        shift.totalCostIncEmpTaxAndSuppCharge = grossNetAndCost.totalCostIncEmpTaxAndSuppCharge;
    }

    private loadTemplateShifts = _.debounce(() => {
        if (!this.hasEmployeesLoaded) {
            this.loadingShifts = false;
            this.completedWork(null, true);
            this.notificationService.showDialogEx('', this.terms["time.schedule.planning.noemployeesforshifts"], SOEMessageBoxImage.Information);
            return;
        }

        this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.load"];
        this.progressBusy = true;

        if (this.reloadShiftsForSpecifiedEmployeeIds.length === 0)
            this.shifts = [];

        let employeeIds: number[] = this.getEmployeeIdsForShiftLoad();
        // Don't show loading dialog if only one or two employees are affected, it will just be an annoying flash
        if (employeeIds.length > 2)
            this.startWork("core.loading");

        this.filteredButNotLoaded = false;
        if (this.showFilters)
            this.toggleFilters();

        this.scheduleService.getTemplateShifts(this.dateFrom, this.dateTo, true, employeeIds, this.isSchedulePlanningMode && (this.selectableInformationSettings.showGrossTime || this.selectableInformationSettings.showTotalCost), this.showTotalCostPermission, true, this.selectableInformationSettings.showWeekendSalary).then(x => {
            let shfts: ShiftDTO[] = x;

            this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.process"];

            this.setShiftData(shfts);

            // Make recurring weeks read only
            shfts.filter(s => !s.timeScheduleTemplateBlockId).forEach(shift => {
                shift.timeScheduleTemplateBlockId = this.templateHelper.getNextTempBlockId();
                shift['isRecurringWeek'] = true;
                shift.isReadOnly = true;
            });

            // Make shifts from template groups read only
            let empGroup = _.groupBy(shfts, s => s.employeeId);
            let empIds: string[] = Object.keys(empGroup);
            empIds.forEach(empId => {
                let employeeId: number = parseInt(empId, 10);
                // Check if employee has any template groups
                let employee = this.getEmployeeById(employeeId);
                if (employee) {
                    let templateGroupHeadIds: number[] = [];
                    if (employee.hasTemplateSchedules)
                        templateGroupHeadIds = employee.templateSchedules.filter(s => s.timeScheduleTemplateGroupId).map(t => t.timeScheduleTemplateHeadId);
                    if (templateGroupHeadIds.length > 0) {
                        shfts.filter(s => s.employeeId === employeeId && templateGroupHeadIds.includes(s.timeScheduleTemplateHeadId)).forEach(empShift => {
                            empShift['isTemplateGroup'] = true;
                            empShift.isReadOnly = true;
                        });
                    }
                }
            });

            if (this.isHiddenEmployeeReadOnly)
                shfts.filter(s => s.employeeId === this.hiddenEmployeeId).forEach(shift => shift.isReadOnly = true);

            // If shifts are loaded for specified employees,
            // remove existing shifts for those employees then add the new ones
            if (this.reloadShiftsForSpecifiedEmployeeIds.length > 0) {
                this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeId => {
                    _.remove(this.shifts, s => s.employeeId === employeeId);
                });
                this.shifts = this.shifts.concat(shfts);
            } else {
                // All shifts are loaded
                this.shifts = shfts;
            }
            this.setAllShiftsMap();

            if (this.isTemplateDayView)
                this.setShiftDataForDayView();

            this.grossNetAndCostLoaded = (this.isSchedulePlanningMode && (this.selectableInformationSettings.showGrossTime || this.selectableInformationSettings.showTotalCost));
            this.recalculateEmployeeWorkTimes = true;
            this.setFirstLoadHasOccurred();
            this.filterShifts('loadTemplateShifts');
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }, 300, { leading: false, trailing: true });

    private loadEmployeePostShifts = _.debounce(() => {
        if (!this.hasEmployeesLoaded)
            return;

        let employeePostIds: number[] = this.getEmployeeIdsForShiftLoad();
        // Don't show loading dialog if only one or two employees are affected, it will just be an annoying flash
        if (employeePostIds.length > 2)
            this.startWork("core.loading");

        this.filteredButNotLoaded = false;
        if (this.showFilters)
            this.toggleFilters();

        this.scheduleService.getEmployeePostTemplateShifts(this.dateFrom, this.dateTo, employeePostIds, true).then(x => {
            let shfts: ShiftDTO[] = x;

            this.setShiftData(shfts);

            // Make recurring weeks read only
            shfts.filter(s => !s.timeScheduleTemplateBlockId).forEach(shift => {
                shift.timeScheduleTemplateBlockId = this.templateHelper.getNextTempBlockId();
                shift['isRecurringWeek'] = true;
                shift.isReadOnly = true;
            });

            // Set assigned or locked employee post shifts as read only
            let empPostIds: number[] = this.reloadShiftsForSpecifiedEmployeeIds.length > 0 ? this.reloadShiftsForSpecifiedEmployeeIds : this.allEmployees.filter(e => e.employeePostId).map(e => e.employeePostId);
            this.setEmployeePostShiftsAsReadonly(shfts, empPostIds);

            // If shifts are loaded for specified employees,
            // remove existing shifts for those employees then add the new ones
            if (this.reloadShiftsForSpecifiedEmployeeIds.length > 0) {
                this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeId => {
                    _.remove(this.shifts, s => s.employeePostId === employeeId);
                });
                this.shifts = this.shifts.concat(shfts);
            } else {
                // All shifts are loaded
                this.shifts = shfts;
            }
            this.setAllShiftsMap();

            if (this.isEmployeePostDayView)
                this.setShiftDataForDayView();

            this.recalculateEmployeeWorkTimes = true;
            this.setFirstLoadHasOccurred();
            this.filterShifts('loadEmployeePostShifts');
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }, 300, { leading: false, trailing: true });

    private getTimeScheduleTemplateHeadForEmployeePost(timeScheduleTemplateHeadId: number, employee: EmployeeListDTO): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTemplateHeadSmall(timeScheduleTemplateHeadId).then(x => {
            if (x) {
                if (!employee.templateSchedules)
                    employee.templateSchedules = [];

                // Convert to typed DTO
                let head = new TimeScheduleTemplateHeadSmallDTO;
                angular.extend(head, x);
                head.fixDates();

                employee.templateSchedules.push(head);
                employee.templateSchedules = _.orderBy(employee.templateSchedules, 'startDate', 'desc');
                this.setEmployeeToolTip(employee);
            }
        });
    }

    private loadTasks = _.debounce(() => {
        this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.load"];
        this.progressBusy = true;

        this.startWork("core.loading");
        this.allTasks = [];

        this.$q.all([
            this.loadTimeScheduleTasks(null),
            this.loadIncomingDeliveries(null)]
        ).then(() => {
            this.setGroupBy(this.isDayView ? this.dayViewGroupBy : this.scheduleViewGroupBy, false);
            this.filterTasks();
            this.setFirstLoadHasOccurred();
        });
    }, 300, { leading: false, trailing: true });

    private loadTimeScheduleTasks(ids: number[]): ng.IPromise<any> {
        if (!ids) {
            // No id passed, all tasks will be fetched, so clear existing ones
            this.tasks = [];
        } else {
            // Remove passed tasks from collection and reload only those
            ids.forEach(id => {
                _.pullAll(this.allTasks, this.allTasks.filter(t => t.type === SoeStaffingNeedsTaskType.Task && t.id === id));
                _.pullAll(this.tasks, this.tasks.filter(t => t.timeScheduleTaskId === id));
            });
        }

        this.scheduleHandler.rememberVerticalScroll();

        return this.scheduleService.getTimeScheduleTasksForInterval(this.dateFrom, this.dateTo, ids, false).then(tasks => {
            tasks.forEach(task => {
                if (task.hasRecurrenceDates) {
                    let validDates = task.recurringDates.getValidDates(false);
                    _.orderBy(validDates, d => d).forEach(date => {
                        let dto = new TimeScheduleTaskDTO();
                        angular.extend(dto, task);

                        dto.setTimesByRecurrence(date);

                        // Only add it to filter collection once
                        if (!this.tasks.map(t => t.timeScheduleTaskId).includes(dto.timeScheduleTaskId))
                            this.tasks.push(dto);

                        let taskDto = dto.toStaffingNeedsTaskDTO();
                        taskDto.headName = dto.name;
                        taskDto.headDescription = dto.description;
                        this.setTaskData(taskDto);
                        this.allTasks.push(taskDto);
                    });
                }
            });
            // Sort filters
            this.tasks = _.sortBy(this.tasks, t => t.name);
        });
    }

    private loadIncomingDeliveries(ids: number[]): ng.IPromise<any> {
        if (!ids) {
            // No id passed, all deliveries will be fetched, so clear existing ones
            this.deliveries = [];
        } else {
            // Remove passed deliveries from collection and reload only those
            ids.forEach(id => {
                _.pullAll(this.allTasks, this.allTasks.filter(t => t.type === SoeStaffingNeedsTaskType.Delivery && t.parentId === id));
                _.pullAll(this.deliveries, this.deliveries.filter(t => t.incomingDeliveryHeadId === id));
            });
        }

        this.scheduleHandler.rememberVerticalScroll();

        return this.scheduleService.getIncomingDeliveriesForInterval(this.dateFrom, this.dateTo, ids, false).then(deliveries => {
            deliveries.forEach(delivery => {
                if (delivery.hasRecurrenceDates) {
                    let validDates = delivery.recurringDates.getValidDates(false);
                    delivery.rows.forEach(row => {
                        _.orderBy(validDates, d => d).forEach(date => {
                            let dto = new IncomingDeliveryRowDTO();
                            angular.extend(dto, row);

                            dto.setTimesByRecurrence(date);

                            dto.headName = delivery.name;
                            dto.isReccurring = delivery.recurrencePattern && delivery.recurrencePattern.length > 0;
                            dto.recurrencePattern = delivery.recurrencePattern;

                            // Only add it to filter collection once
                            if (!this.deliveries.map(d => d.incomingDeliveryRowId).includes(dto.incomingDeliveryRowId))
                                this.deliveries.push(dto);

                            let taskDto = dto.toStaffingNeedsTaskDTO();
                            taskDto.headDescription = delivery.description;
                            this.setTaskData(taskDto);
                            this.allTasks.push(taskDto);
                        });
                    });
                }
            })
            // Sort filters
            this.deliveries = _.sortBy(this.deliveries, d => d.name);
        });
    }

    private setTaskData(task: StaffingNeedsTaskDTO) {
        // Set shift type data
        let shiftType = this.allShiftTypes.find(s => s.shiftTypeId === task.shiftTypeId);
        task.shiftTypeName = shiftType ? shiftType.name : '';

        // Remove alpha values in color property
        task.color = GraphicsUtility.removeAlphaValue(shiftType ? shiftType.color : Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);

        // Set department name
        let dimCounter = 1;
        this.accountDims.forEach(accountDim => {
            dimCounter++;
            if (dimCounter === 2 && task.account2Id)
                task.accountDim2Name = this.getAccountName(accountDim.accountDimNr, task.account2Id);
            else if (dimCounter === 3 && task.account3Id)
                task.accountDim3Name = this.getAccountName(accountDim.accountDimNr, task.account3Id);
            else if (dimCounter === 4 && task.account4Id)
                task.accountDim4Name = this.getAccountName(accountDim.accountDimNr, task.account4Id);
            else if (dimCounter === 5 && task.account5Id)
                task.accountDim5Name = this.getAccountName(accountDim.accountDimNr, task.account5Id);
            else if (dimCounter === 6 && task.account6Id)
                task.accountDim6Name = this.getAccountName(accountDim.accountDimNr, task.account6Id);
        });

        task.fixDates(true);
        task.setLabel(this.isCompressedStyle, this.terms["core.time.minutes"]);
        task.setToolTip(this.terms["core.time.minutes"]);

        task.isVisible = true;
    }

    private getAccountName(dimNr: number, accountId: number): string {
        let accountDim = this.accountDims.find(a => a.accountDimNr === dimNr);
        if (accountDim?.accounts) {
            let account = accountDim.accounts.find(a => a.accountId === accountId);
            if (account)
                return account.name;
        }

        return '';
    }

    private loadStaffingNeedsHeads = _.debounce(() => {
        this.startWork("core.loading");
        let dateTo = this.dateTo;

        if (this.isStaffingNeedsDayView)
            dateTo = this.dateFrom;

        this.scheduleService.generateStaffingNeedsHeads(TermGroup_StaffingNeedHeadsFilterType.None, 0, this.dateFrom, dateTo).then(x => {
            this.initSetStaffingNeedsHeadData();
            this.allHeads = x.map(h => {
                let obj = new StaffingNeedsHeadDTO();
                angular.extend(obj, h);
                obj.fixDates();
                this.setStaffingNeedsHeadData(obj);

                return obj;
            });

            this.filterStaffingNeedsHeadsToDisplay();
            this.setFirstLoadHasOccurred();
        });
    }, 300, { leading: false, trailing: true });

    private filterStaffingNeedsHeadsToDisplay() {
        // Reset specific dates
        this.dates.forEach(date => date.specificNeed = false);

        // Copy all heads
        this.heads = [];
        this.allHeads.forEach(head => {
            this.heads.push(_.cloneDeep(head));
        });

        switch (this.staffingNeedsSelection) {
            case (TermGroup_StaffingNeedHeadsFilterType.ActualNeed):
                // Remove all periods that are removed need
                this.heads.forEach(head => {
                    head.rows.forEach(row => {
                        _.pullAll(row.periods, row.periods.filter(p => p.isRemovedNeed));
                    });
                });
                break;
            case (TermGroup_StaffingNeedHeadsFilterType.BaseNeed):
                // Remove all periods that are not base need
                this.heads.forEach(head => {
                    head.rows.forEach(row => {
                        _.pullAll(row.periods, row.periods.filter(p => !p.isBaseNeed));
                    });
                });
                break;
            case (TermGroup_StaffingNeedHeadsFilterType.SpecificNeed):
                // Remove all periods that are not specific need
                this.heads.forEach(head => {
                    head.rows.forEach(row => {
                        _.pullAll(row.periods, row.periods.filter(p => !p.isSpecificNeed && !p.isRemovedNeed));
                    });
                });
                break;
        }

        // Check if any head has periods that are specific or removed
        this.allHeads.forEach(head => {
            head.rows.forEach(row => {
                if (row.periods.filter(p => p.isSpecificNeed || p.isRemovedNeed).length > 0) {
                    this.setDateAsSpecificNeed(head.date);
                }
            });
        });

        this.filterStaffingNeedsPeriods();
    }

    private setDateAsSpecificNeed(date: Date) {
        let dateDay = this.dates.find(d => d.date.isSameDayAs(date));
        if (dateDay)
            dateDay.specificNeed = true;
    }

    private dateHasSpecificNeed(date: Date) {
        let dateDay = this.dates.find(d => d.date.isSameDayAs(date));
        return dateDay?.specificNeed;
    }

    private rowTempId: number = 0;
    private periodTempId: number = 0;
    private initSetStaffingNeedsHeadData() {
        this.rowTempId = 0;
        this.periodTempId = 0;
    }

    private setStaffingNeedsHeadData(head: StaffingNeedsHeadDTO) {
        if (!head)
            return;

        // Set interval (if not set)
        if (head.interval === 0)
            head.interval = this.dayViewMinorTickLength;

        // Set status name
        if (head.status !== TermGroup_StaffingNeedsHeadStatus.None)
            head.statusName = this.headStatuses.find(s => s.id === head.status).name;

        // Convert to typed DTOs
        if (head.rows) {
            head.rows = head.rows.map(r => {
                let obj = new StaffingNeedsRowDTO();
                angular.extend(obj, r);

                if (!r.staffingNeedsRowId) {
                    this.rowTempId++;
                    obj.staffingNeedsRowId = this.rowTempId;
                }

                if (obj.periods) {
                    obj.periods = obj.periods.map(p => {
                        let pobj = new StaffingNeedsRowPeriodDTO();
                        angular.extend(pobj, p);

                        this.periodTempId++;
                        pobj.staffingNeedsRowPeriodId = this.periodTempId;
                        pobj.fixDates();

                        // Calculate actual times
                        let length: number = pobj.length ? pobj.length : pobj.interval;  // Length is a new column, in Silverlight interval was used
                        let deltaDays: number = pobj.startTime.date().diffDays(Constants.DATETIME_DEFAULT);
                        pobj.actualStartTime = head.date.mergeTime(pobj.startTime).addDays(deltaDays).clearSeconds();
                        pobj.actualStopTime = pobj.actualStartTime.addMinutes(length);

                        let shiftType = this.allShiftTypes.find(s => s.shiftTypeId === pobj.shiftTypeId);
                        pobj.shiftTypeName = shiftType ? shiftType.name : this.terms["core.notspecified"];
                        pobj.shiftTypeColor = shiftType ? shiftType.color : Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
                        pobj.shiftTypeNeedsCode = shiftType ? shiftType.needsCode : '?';
                        pobj.fixColors();

                        return pobj;
                    });
                }

                return obj;
            });
        }
    }

    private loadPermittedEmployeeIds(): ng.IPromise<any> {
        return this.scheduleService.getPermittedEmployeeIds().then(x => {
            this.permittedEmployeeIds = x;
        });
    }

    private clearEmployeeIdsForShiftLoad() {
        this.reloadShiftsForSpecifiedEmployeeIds = [];
    }

    private getEmployeeIdsForShiftLoad(): number[] {
        var employeeIds: number[] = [];
        if (this.reloadShiftsForSpecifiedEmployeeIds.length > 0)
            employeeIds = this.reloadShiftsForSpecifiedEmployeeIds;
        else
            employeeIds = this.getFilteredEmployeeIds();

        return employeeIds;
    }

    private setEmployeeData() {
        // Populate employee filter collection
        this.copyAllEmployees(false);
        this.resetSort();

        // Populate employee list
        this.employeeListFilterEmployeeIds = this.allEmployees.filter(e => e.employeeId).map(e => e.employeeId);
        this.filterEmployeeList(null);
        this.filterEmployeeGroups();
    }

    private filterEmployeeGroups() {
        this.employeeGroups = [];

        this.employedEmployees.forEach(emp => {
            if (emp.employments) {
                emp.employments.forEach(employment => {
                    if (!this.employeeGroups.map(eg => eg.id).includes(employment.employeeGroupId)) {
                        this.employeeGroups.push({ id: employment.employeeGroupId, label: employment.employeeGroupName });
                    }
                });
            }
        });

        this.employeeGroups = _.orderBy(this.employeeGroups, 'label');
    }

    private setEmployeeAsModified(employeeId: number) {
        var employee = this.getEmployeeById(employeeId);
        if (employee)
            employee.isModified = true;
    }

    private clearModifiedEmployees() {
        this.allEmployees.filter(e => e.isModified).forEach(employee => {
            employee.isModified = false;
        });
    }

    private setShiftData(shifts: ShiftDTO[]) {
        shifts.forEach(shift => {
            shift.isVisible = true;

            if (this.isEmployeeInactive(shift.employeeId) || !this.hasCurrentViewModifyPermission || (this.isStandbyView && !shift.isStandby))
                shift.isReadOnly = true;

            if (shift.shiftTypeId && (!shift.shiftTypeName || !shift.shiftTypeColor)) {
                var shiftType = this.allShiftTypes.find(s => s.shiftTypeId === shift.shiftTypeId);
                if (shiftType) {
                    shift.shiftTypeColor = shiftType.color;
                    shift.shiftTypeName = shiftType.name;
                }
            }

            // Shift account is not valid for current user, or user is looking at another account
            if (this.useAccountHierarchy && shift.accountId && !this.inactivateLending) {
                if (this.validAccountIds.length > 0 && !this.validAccountIds.includes(shift.accountId)) {
                    shift.isLended = true;
                } else if (this.isDefaultAccountDimLevel && shift.accountId !== this.userAccountId) {
                    shift.isOtherAccount = true;
                }

                if (shift.isLended || shift.isOtherAccount) {
                    shift.totalCost = 0;
                    shift.totalCostIncEmpTaxAndSuppCharge = 0;
                }
            }

            if ((shift.isLended || shift.isOtherAccount) && !this.hasStaffingByEmployeeAccount)
                shift.isReadOnly = true;

            if (shift.isSchedule && (shift.actualStartDate.isBeforeOnDay(this.dateFrom) || shift.actualDateOnLoad.isAfterOnDay(this.dateTo)))
                shift.isReadOnly = true;

            // Leisure code
            if (shift.isLeisureCode) {
                shift.employeeName = this.allEmployees.find(e => e.employeeId === shift.employeeId)?.name;
                shift.shiftTypeName = this.getLeisureCodeName(shift);
            }

            // Remove alpha values in color property
            if (shift.isAbsenceRequest || shift.isAbsence) {
                shift.shiftTypeColor = "#ef545e";   // @shiftAbsenceBackgroundColor
            } else if (shift.isLended) {
                shift.shiftTypeColor = "#dfdfdf";   // @soe-border-color
            } else if (shift.isLeisureCode) {
                shift.shiftTypeColor = "#ffffff";   // @soe-color-neutral-white
            } else {
                shift.shiftTypeColor = GraphicsUtility.removeAlphaValue(shift.shiftTypeColor, Constants.SHIFT_TYPE_UNSPECIFIED_COLOR);
            }
        });

        this.setShiftLabels(shifts);
    }

    private setShiftDataForDayView() {
        // Adjust day start/end times

        // Restore to company setting
        this.forceNoLoadData = true;
        this.dayViewStartTime = this.originalDayViewStartTime;
        this.dayViewEndTime = this.originalDayViewEndTime;
        this.adjustDayViewTimes(false);
        this.setDateRange(false);
        this.forceNoLoadData = false;

        let shifts = this.shifts.filter(s => !s.isWholeDay && s.actualStopTime.isSameDayAs(this.dateFrom));
        if (shifts.length > 0) {
            // Check if there are any shifts that starts before current start setting
            let firstStartTime: Date = _.orderBy(shifts, 'actualStartTime')[0].actualStartTime;
            if (firstStartTime.beginningOfHour().isBeforeOnMinute(this.dateFrom.date().addMinutes(this.originalDayViewStartTime)))
                this.dayViewStartTime = firstStartTime.isBeforeOnDay(this.dateFrom) ? 0 : firstStartTime.beginningOfHour().diffMinutes(this.dateFrom.date());
            if (this.dayViewStartTime > this.originalDayViewStartTime)
                this.dayViewStartTime = this.originalDayViewStartTime;

            // Check if there are any shifts that ends after current end setting
            let lastEndTime: Date = _.orderBy(shifts, 'actualStopTime', 'desc')[0].actualStopTime;
            if (lastEndTime.isAfterOnMinute(this.dateFrom.date().addMinutes(this.originalDayViewEndTime)))
                this.dayViewEndTime = lastEndTime.isAfterOnDay(this.dateFrom) ? (24 * 60) : lastEndTime.diffMinutes(this.dateFrom.date());
            if (this.dayViewEndTime < this.originalDayViewEndTime)
                this.dayViewEndTime = this.originalDayViewEndTime;

            if (!this.dateFrom.isSameMinuteAs(this.dateFrom.date().addHours(this.startHour))) {
                this.forceNoLoadData = true;
                this.dateFrom = this.dateFrom.date().addHours(this.startHour);
                this.adjustDayViewTimes(false);
                this.forceNoLoadData = false;
                this.setDateRange(false);
            }
            if (!this.dateTo.isSameMinuteAs(this.dateTo.date().addHours(this.endHour).addSeconds(-1))) {
                this.forceNoLoadData = true;
                this.dateTo = this.dateTo.date().addHours(this.endHour).addSeconds(-1);
                this.adjustDayViewTimes(false);
                this.forceNoLoadData = false;
                this.setDateRange(false);
            }

            // Need to re-sort employees if sorting is based on shifts
            if (this.dayViewSortBy === TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime)
                this.sortEmployees(false);
        }
    }

    private setEmployeePostInfoOnShifts(shifts: ShiftDTO[]) {
        shifts.forEach(shift => {
            const employeePost = this.getEmployeePostById(shift.employeePostId);
            if (employeePost)
                shift.employeeName = employeePost.name;
        });
    }

    private setEmployeePostShiftsAsReadonly(shifts: ShiftDTO[], employeePostIds: number[]) {
        // First clear read only flag on all passed shifts (except for recurring weeks)
        shifts.filter(s => !s['isRecurringWeek']).forEach(shift => {
            shift.isReadOnly = false;
        });

        // Set assigned or locked employee post shifts as read only
        this.allEmployees.filter(e => employeePostIds.includes(e.employeePostId) && (e.employeeId || e.employeePostStatus === SoeEmployeePostStatus.Locked)).forEach(employeePost => {
            shifts.filter(s => s.employeePostId === employeePost.employeePostId).forEach(shift => {
                shift.isReadOnly = true;
            });
        });

        // Make recurring weeks read only
        shifts.filter(s => !s.timeScheduleTemplateBlockId).forEach(shift => {
            shift.isReadOnly = true;
        });
    }

    private validateWorkRules(action: TermGroup_ShiftHistoryType, shifts: ShiftDTO[], showCancelAll: boolean = false): ng.IPromise<any> {
        let deferral = this.$q.defer<boolean>();

        if (shifts.length === 0) {
            deferral.resolve(true);
        } else {
            shifts.forEach(shift => {
                shift.setTimesForSave();
            });

            let rules: SoeScheduleWorkRules[] = null;
            if (this.selectableInformationSettings.skipWorkRules) {
                // The following rules should always be evaluated
                rules = [];
                rules.push(SoeScheduleWorkRules.OverlappingShifts);
                if (!this.isTemplateView)
                    rules.push(SoeScheduleWorkRules.AttestedDay);
            }

            this.startWork("time.schedule.planning.evaluateworkrules.executing");
            this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules(shifts, rules, shifts[0].employeeId, this.isTemplateView, this.timeScheduleScenarioHeadId, this.currentPlanningPeriod?.startDate, this.currentPlanningPeriod?.stopDate).then(result => {
                this.completedWork(null, true);
                this.notificationService.showValidateWorkRulesResult(action, result, this.isTemplateView || this.isEmployeePostView ? 0 : shifts[0].employeeId, showCancelAll).then(passed => {
                    deferral.resolve(passed);
                });
            }).catch(reason => {
                this.notificationService.showServiceError(reason);
                deferral.resolve(false);
            });
        }

        return deferral.promise;
    }

    private validateWorkRulesOnDelete(shifts: ShiftDTO[]): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let rules: SoeScheduleWorkRules[] = null;
        if (this.selectableInformationSettings.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
            if (!this.isTemplateView)
                rules.push(SoeScheduleWorkRules.AttestedDay);
        }

        let firstShift = shifts[0];
        let employeeId: number = firstShift.employeeId;
        let employeePostId: number = firstShift.employeePostId;
        let start = firstShift.startTime.beginningOfDay();

        if (this.isTemplateView || this.isEmployeePostView) {
            let template = this.templateHelper.getTemplateSchedule(this.isEmployeePostView ? employeePostId : employeeId, start);
            if (template) {
                this.startWork("time.schedule.planning.evaluateworkrules.executing");
                this.scheduleService.evaluateDragTemplateShiftsAgainstWorkRules(DragShiftAction.Delete, shifts.map(s => s.timeScheduleTemplateBlockId), template.timeScheduleTemplateHeadId, start, 0, employeeId, employeePostId, template.timeScheduleTemplateHeadId, start, rules).then(result => {
                    this.completedWork(null, true);
                    this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskDeleteTimeScheduleShift, result, employeeId).then(passed => {
                        deferral.resolve(passed);
                    });
                }).catch(reason => {
                    this.notificationService.showServiceError(reason);
                    deferral.resolve(false);
                });
            } else {
                deferral.resolve(false);
            }
        } else {
            this.startWork("time.schedule.planning.evaluateworkrules.executing");
            this.scheduleService.evaluateDragShiftsAgainstWorkRules(DragShiftAction.Delete, shifts.map(s => s.timeScheduleTemplateBlockId), 0, employeeId, false, rules, this.isStandbyView, this.timeScheduleScenarioHeadId, null, null, null, this.planningPeriodChild?.startDate, this.planningPeriodChild?.stopDate).then(result => {
                this.completedWork(null, true);
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.TaskDeleteTimeScheduleShift, result, employeeId).then(passed => {
                    deferral.resolve(passed);
                });
            }).catch(reason => {
                this.notificationService.showServiceError(reason);
                deferral.resolve(false);
            });
        }

        return deferral.promise;
    }

    public initValidateBreakChange(shift: ShiftDTO, dayShifts: ShiftDTO[], breakNo: number, dragStart: boolean, dragStop: boolean) {
        this.validateBreakChange(shift, dayShifts, breakNo, dragStart, dragStop).then(passedBreakChange => {
            if (passedBreakChange) {
                if (this.isTemplateDayView || this.isEmployeePostDayView) {
                    this.saveTemplateShiftsForDayView(dayShifts);
                } else {
                    this.initSaveShiftsForDayView(TermGroup_ShiftHistoryType.EditBreaks, dayShifts).then(passedSave => {
                        if (passedSave) {
                            if (this.editMode === PlanningEditModes.TemplateBreaks)
                                this.setEmployeeAsModified(shift.employeeId);
                            else
                                this.saveShifts(Guid.newGuid().toString(), dayShifts, true, false, false, 0);
                        } else {
                            this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                            this.loadData('initValidateBreakChange passed');
                        }
                    });
                }
            } else {
                this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                this.loadData('initValidateBreakChange not passed');
            }
        });
    }

    private validateBreakChange(shift: ShiftDTO, dayShifts: ShiftDTO[], breakNo: number, dragStart: boolean, dragStop: boolean): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        this.startWork("time.schedule.planning.validatebreakchange");

        const breakId: number = shift[`break${breakNo}Id`];
        const breakTimeCodeId: number = shift[`break${breakNo}TimeCodeId`];
        const startTime: Date = shift[`break${breakNo}StartTime`];
        const length: number = shift[`break${breakNo}Minutes`];

        this.scheduleService.validateBreakChange(shift.employeeId, breakId, shift.timeScheduleTemplatePeriodId, breakTimeCodeId, startTime, length, this.isTemplateView, this.timeScheduleScenarioHeadId).then(result => {
            if (!result.success) {
                if (result.error === SoeValidateBreakChangeError.TimeCodeBreakForLengthNotFound) {
                    // No valid break length, show dialog to select break type
                    this.completedWork(null, true);
                    this.openSelectBreakTimeCode(result.errorMessage, result.timeCodeBreakIds, shift, dayShifts, breakNo, startTime, startTime.addMinutes(length), dragStart, dragStop).then(val => {
                        deferral.resolve(val);
                    });
                } else if (result.error === SoeValidateBreakChangeError.TimeCodeBreakChanged) {
                    // Valid break length, but it has changed from what it was before. Just change it to returned value and save
                    dayShifts.forEach(s => {
                        s[`break${breakNo}TimeCodeId`] = _.first(result.timeCodeBreakIds);
                    });
                    deferral.resolve(true);
                } else {
                    // Validation failed, show error and reload
                    this.failedWork(result.errorMessage);
                    this.$timeout(() => {
                        deferral.resolve(false);
                    }, this.autoCloseModalDelay);   // Show error message for a while, then reload
                }
            } else {
                deferral.resolve(true);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    public initSaveShiftsForDayView(action: TermGroup_ShiftHistoryType, shifts: ShiftDTO[], showCancelAll: boolean = false): ng.IPromise<any> {
        let deferral = this.$q.defer<boolean>();

        let showOrderRemainingTimeWarning = false;
        let remaining = 0;
        if (this.isOrderPlanningMode) {
            shifts.forEach(shift => {
                if (shift.isOrder) {
                    const prevLength = shift.getShiftLength();
                    const newLength = shift.getShiftLengthDuringMove();
                    remaining = shift.order.remainingTime + prevLength - newLength;
                    if (remaining < 0)
                        showOrderRemainingTimeWarning = true;
                }
            });
        }

        if (showOrderRemainingTimeWarning) {
            const keys: string[] = [
                "common.obs",
                "time.schedule.planning.dragshift.orderremainingtimedragwarning",
                "time.schedule.planning.dragshift.orderremainingtimedragwarningmultiple"
            ];

            this.translationService.translateMany(keys).then(terms => {
                let message: string;
                if (shifts.length > 1)
                    message = terms["time.schedule.planning.dragshift.orderremainingtimedragwarningmultiple"];
                else
                    message = terms["time.schedule.planning.dragshift.orderremainingtimedragwarning"].format(CalendarUtility.minutesToTimeSpan(Math.abs(remaining)));

                const modal = this.notificationService.showDialog(terms["common.obs"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Large);
                modal.result.then(val => {
                    this.validateWorkRules(action, shifts, showCancelAll).then(passed => {
                        deferral.resolve(passed);
                    });
                }, (reason) => {
                    deferral.resolve(false);
                });
            });
        } else {
            this.validateWorkRules(action, shifts, showCancelAll).then(passed => {
                deferral.resolve(passed);
            });
        }

        return deferral.promise;
    }

    private prepareShiftsForSave(shifts: ShiftDTO[]) {
        shifts.forEach(shift => {
            shift.prepareShiftsForSave(this.defaultTimeCodeId);
        });

        // If all shifts are marked as deleted, unmark one of the them,
        // otherwise the whole day will be deleted and not visible in the attest view
        if (shifts.length > 0 && shifts.length === shifts.filter(s => s.isDeleted).length)
            _.last(shifts).isDeleted = false;
    }

    public saveShifts = _.debounce((source: string, shifts: ShiftDTO[], updateBreaks: boolean, showBreakWarning: boolean, adjustTasks: boolean, minutesMoved: number, additionalEmployeesToRefresh: number[] = null, reloadOnError: boolean = false) => {
        this.startSave();

        // Remember which employees that are saved and only reload shifts for those employees
        this.reloadShiftsForSpecifiedEmployeeIds = _.uniqBy(shifts, s => s.employeeId).map(s => s.employeeId);
        if (additionalEmployeesToRefresh && additionalEmployeesToRefresh.length > 0) {
            additionalEmployeesToRefresh.forEach(employeeId => {
                if (!this.reloadShiftsForSpecifiedEmployeeIds.includes(employeeId))
                    this.reloadShiftsForSpecifiedEmployeeIds.push(employeeId);
            })
        }

        this.prepareShiftsForSave(shifts);
        this.scheduleService.saveShifts(source, shifts, updateBreaks, this.selectableInformationSettings.skipXEMailOnChanges, adjustTasks, minutesMoved, this.timeScheduleScenarioHeadId).then(result => {
            // Copy reloaded employeeIds to new list, since loadData() will clear it
            let reloadedEmployeeIds: number[] = [];
            this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeId => {
                reloadedEmployeeIds.push(employeeId);
            });

            if (result.success || !this.editShiftModal)
                this.loadData('saveShifts');

            if (result.success) {
                // Reload affected orders in order list
                if (this.isOrderPlanningMode) {
                    let orderIds: number[] = [];
                    shifts.filter(s => s.order).forEach(shift => {
                        orderIds.push(shift.order.orderId);
                    });

                    this.loadUnscheduledOrders(orderIds);
                }

                // Reload annual scheduled time for affected employee(s)
                if (this.calculatePlanningPeriodScheduledTime) {
                    reloadedEmployeeIds.forEach(employeeId => {
                        this.loadAnnualScheduledTime(employeeId);
                    });
                }

                // Reload unscheduled tasks
                this.loadUnscheduledTasksAndDeliveriesDates();

                if (showBreakWarning)
                    this.showCheckBreakTimesDialog();

                if (this.editShiftModal)
                    this.editShiftModal.close();
            } else {
                this.failedSave(result.errorMessage);
                if (reloadOnError) {
                    this.loadShiftsSilent = true;
                    this.loadData('saveShifts');
                }
            }
        }, error => {
            if (error.error && error.error === Constants.SERVICE_ERROR_DUPLICATE_CALLS) {
                this.coreService.addSysLogMessage("Time.Schedule.Planning.EditController.saveShifts", error.message, source + "\n\n" + JSON.stringify(shifts), true);
            } else {
                this.failedSave(error.message);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }, 200, { leading: true, trailing: false });

    private saveAssignments(employeeId: number, orderId: number, shiftTypeId: number, startTime: Date, stopTime: Date, timeAdjustmentType: TermGroup_AssignmentTimeAdjustmentType) {
        this.startSave();

        this.sharedScheduleService.saveOrderAssignments(employeeId, orderId, shiftTypeId, startTime, stopTime, timeAdjustmentType, this.selectableInformationSettings.skipXEMailOnChanges).then(result => {
            if (result.success) {
                this.reloadShiftsForSpecifiedEmployeeIds = [employeeId];
                this.loadData('saveAssignments success');
                this.loadUnscheduledOrders([orderId]);
            } else {
                this.failedSave(result.errorMessage);
                this.$timeout(() => {
                    this.loadData('saveAssignments failed');
                }, this.autoCloseModalDelay);   // Show error message for a while, then reload
            }
        });
    }

    public saveTemplateShifts(employeeIdentifier: number, timeScheduleTemplateHeadId: number, shifts: ShiftDTO[], activateDayNumber: number, activateDates: Date[]): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        this.startSave();

        // Replace existing shifts on each date, with shifts coming from the dialog, excluding the deleted ones
        let dates: Date[] = _.uniqBy(shifts, s => s.startTime.beginningOfDay()).map(s => s.startTime.beginningOfDay());
        dates.forEach(date => {
            let existingIds: number[] = shifts.map(s => s.timeScheduleTemplateBlockId);
            _.pullAll(this.shifts, this.shifts.filter(s => existingIds.includes(s.timeScheduleTemplateBlockId)));
        });
        this.shifts = _.concat(this.shifts, shifts.filter(s => !s.isDeleted));
        this.setAllShiftsMap();

        this.templateHelper.updateTemplateSchedule(employeeIdentifier, timeScheduleTemplateHeadId, activateDayNumber, activateDates).then(success => {
            // Success or failure, always reload employee
            this.reloadShiftsForSpecifiedEmployeeIds = [employeeIdentifier];
            this.loadData('saveTemplateShifts');

            // Reload unscheduled tasks
            this.loadUnscheduledTasksAndDeliveriesDates();
            deferral.resolve(success);
        });

        return deferral.promise;
    }

    public saveTemplateShiftsForDayView(shifts: ShiftDTO[]) {
        this.startSave();

        let employeeIdentifier: number;
        if (this.isTemplateView)
            employeeIdentifier = shifts[0].employeeId;
        else if (this.isEmployeePostView)
            employeeIdentifier = shifts[0].employeePostId;

        // Get current template
        let template: TimeScheduleTemplateHeadSmallDTO;
        if (employeeIdentifier)
            template = this.templateHelper.getTemplateSchedule(employeeIdentifier, this.dateFrom);
        if (template) {
            this.templateHelper.updateTemplateSchedule(employeeIdentifier, template.timeScheduleTemplateHeadId, null, null).then(success => {
                // Success or failure, always reload employee
                this.reloadShiftsForSpecifiedEmployeeIds = [employeeIdentifier];
                this.loadData('saveTemplateShiftsForDayView update');
            });
        } else {
            // TODO: Show error message?
            this.reloadShiftsForSpecifiedEmployeeIds = [employeeIdentifier];
            this.loadData('saveTemplateShiftsForDayView no template');
        }
    }

    private cancelTemplateBreaks() {
        this.clearModifiedEmployees();
        this.editMode = PlanningEditModes.Shifts;
        this.clearEmployeeIdsForShiftLoad();
        this.loadData('cancelTemplateBreaks');
    }

    private allShiftsSaved: boolean;
    private initSaveTemplateBreaks() {
        let deferral = this.$q.defer<any>();

        this.startSave();

        this.allShiftsSaved = true;
        let employees = this.allEmployees.filter(e => e.isModified);
        let employeeIds: number[] = employees.map(e => e.employeeId);

        this.saveMultipleShiftsForDayView(deferral, employees).then(() => {
            this.clearEmployeeIdsForShiftLoad();
            if (this.allShiftsSaved) {
                // All successfully saved, go to edit breaks view and reload all
                this.editMode = PlanningEditModes.Breaks;
            } else {
                // Only reload successfully saved employees
                employeeIds.forEach(employeeId => {
                    let employee = this.getEmployeeById(employeeId);
                    if (employee && !employee.isModified)
                        this.reloadShiftsForSpecifiedEmployeeIds.push(employeeId);
                });
            }
            this.loadData('initSaveTemplateBreaks');
        });
    }

    private saveMultipleShiftsForDayView(deferral, employees: EmployeeListDTO[]): ng.IPromise<any> {
        let employee = employees[0];
        let empShifts = this.shifts.filter(s => s.employeeId === employee.employeeId);
        this.initSaveShiftsForDayView(TermGroup_ShiftHistoryType.EditBreaks, empShifts, true).then(passed => {
            if (passed === true) {
                // Save current
                let source: string = Guid.newGuid().toString();
                this.scheduleService.saveShifts(source, empShifts, true, true, false, 0, this.timeScheduleScenarioHeadId).then(result => {
                    if (result.success) {
                        employee.isModified = false;
                    } else {
                        this.allShiftsSaved = false;
                        this.failedSave(result.errorMessage);
                    }
                }, error => {
                    if (error.error && error.error === Constants.SERVICE_ERROR_DUPLICATE_CALLS) {
                        this.coreService.addSysLogMessage("Time.Schedule.Planning.EditController.saveMultipleShiftsForDayView", error.message, source + "\n\n" + JSON.stringify(empShifts), true);
                    } else {
                        this.allShiftsSaved = false;
                        this.failedSave(error.message);
                    }
                }).catch(reason => {
                    this.allShiftsSaved = false;
                    this.notificationService.showServiceError(reason);
                }).finally(() => {
                    if (employees.length > 1)
                        this.saveMultipleShiftsForDayView(deferral, employees.slice(1));
                    else
                        deferral.resolve();
                });
            } else if (passed === false) {
                // Cancel current
                this.allShiftsSaved = false;
                if (employees.length > 1)
                    this.saveMultipleShiftsForDayView(deferral, employees.slice(1));
                else
                    deferral.resolve();
            } else {
                // Abort all
                this.allShiftsSaved = false;
                deferral.resolve();
            }
        });

        return deferral.promise;
    }

    // Using debounce because initially both loadSettingsForView() and setDateRange() call this method
    private loadCurrentPlanningPeriod = _.debounce((forceLoad: boolean = false, forceLoadSummary: boolean = false) => {
        if (this.selectableInformationSettings?.showPlanningPeriodSummary && this.selectableInformationSettings?.planningPeriodHeadId) {
            if (forceLoadSummary)
                this.loadPlanningPeriodSummary = true;

            // If date is still in current period range, do not reload it
            this.checkCurrentPlanningPeriodRange();
            if (forceLoad || !this.currentPlanningPeriodInRange) {
                this.loadPlanningPeriodSummary = true;
                this.scheduleHandler.setColgroupWidths();

                if (this.calculatePlanningPeriodScheduledTimeUseAveragingPeriod) {
                    this.timeService.getPlanningPeriodHeadWithPeriods(this.selectableInformationSettings.planningPeriodHeadId, this.dateFrom).then(x => {
                        this.planningPeriodHead = x;
                        this.planningPeriodChild = this.planningPeriodHead.getChildByDate(this.dateFrom);
                        if (this.planningPeriodChild) {
                            this.forceNoLoadData = true;
                            this.dateFrom = this.planningPeriodChild.startDate;
                            this.dateTo = this.planningPeriodChild.stopDate;
                            this.forceNoLoadData = false;
                        }

                        // Create a "fake" TimePeriodDTO to be able to use the same logic
                        if (x.parentPeriods && x.parentPeriods.length > 0) {
                            let tp = new TimePeriodDTO();
                            tp.timePeriodHeadId = x.timePeriodHeadId;
                            tp.name = x.parentPeriods[0].name;
                            tp.startDate = x.parentPeriods[0].startDate;
                            tp.stopDate = x.parentPeriods[0].stopDate;
                            this.currentPlanningPeriod = tp;
                        } else {
                            this.planningPeriodHead = null;
                            this.currentPlanningPeriod = null;
                        }
                        this.checkCurrentPlanningPeriodRange();
                    });
                } else {
                    this.timeService.getTimePeriod(this.selectableInformationSettings.planningPeriodHeadId, this.dateFrom, true).then(x => {
                        this.currentPlanningPeriod = x;
                        this.checkCurrentPlanningPeriodRange();
                    });
                }
            } else if (this.calculatePlanningPeriodScheduledTimeUseAveragingPeriod) {
                this.checkCurrentPlanningPeriodChildRange();
                if (!this.currentPlanningPeriodChildInRange) {
                    this.planningPeriodChild = this.planningPeriodHead.getChildByDate(this.dateFrom);
                    this.checkCurrentPlanningPeriodChildRange();
                }
            }
        }
    }, 200, { leading: false, trailing: true });

    private checkCurrentPlanningPeriodRange() {
        this.currentPlanningPeriodInRange = this.currentPlanningPeriod && this.dateFrom && this.dateTo && this.dateFrom.isWithinRange(this.currentPlanningPeriod.startDate, this.currentPlanningPeriod.stopDate.endOfDay()) && this.dateTo.isWithinRange(this.currentPlanningPeriod.startDate, this.currentPlanningPeriod.stopDate.endOfDay());
    }

    private checkCurrentPlanningPeriodChildRange() {
        this.currentPlanningPeriodChildInRange = this.planningPeriodChild && this.dateFrom && this.dateTo && this.dateFrom.isWithinRange(this.planningPeriodChild.startDate, this.planningPeriodChild.stopDate.endOfDay()) && this.dateTo.isWithinRange(this.planningPeriodChild.startDate, this.planningPeriodChild.stopDate.endOfDay());
        this.currentPlanningPeriodChildInRangeExact = this.currentPlanningPeriodChildInRange && this.dateFrom.isSameDayAs(this.planningPeriodChild.startDate) && this.dateTo.isSameDayAs(this.planningPeriodChild.stopDate);
    }

    private loadAnnualScheduledTimeSummary(employeeIds: number[] = null, clearList: boolean = true) {
        if (!this.calculatePlanningPeriodScheduledTime || !this.selectableInformationSettings.showPlanningPeriodSummary || !this.currentPlanningPeriod)
            return;

        this.loadPlanningPeriodSummary = false;
        this.loadingPlanningPeriodSummary = true;

        if (clearList) {
            this.scheduledTimeEmployeeIds = [];
            // An employee should not see other employees times, only if running as admin
            if (this.isUser)
                this.scheduledTimeEmployeeIds.push(this.employeeId);
            else if (employeeIds)
                this.scheduledTimeEmployeeIds = employeeIds;
            else
                this.scheduledTimeEmployeeIds = this.getVisibleEmployeeIds();
            this.scheduledTimeEmployeeIdsCount = this.scheduledTimeEmployeeIds.length;
        }

        let processed = this.scheduledTimeEmployeeIds.length > 0 ? ((1 - (this.scheduledTimeEmployeeIds.length / this.scheduledTimeEmployeeIdsCount)) * 100).round(0) : 100;

        this.progressMessage = "{0} ({1}%)".format(this.terms["time.schedule.planning.annualsummary.loading"], processed.toString());
        this.progressBusy = true;

        let batchEmployeeIds: number[] = this.scheduledTimeEmployeeIds.splice(0, 25);  // Fetch for 25 employees at a time
        this.scheduleService.getAnnualScheduledTimeSummary(batchEmployeeIds, this.currentPlanningPeriod.startDate, this.currentPlanningPeriod.stopDate, this.currentPlanningPeriod.timePeriodHeadId).then(x => {
            x.forEach(y => {
                // Update scheduled/work times

                // Update employee in list
                let employee = this.getEmployeeById(y.employeeId);
                if (employee) {
                    employee.annualScheduledTimeMinutes = y.annualScheduledTimeMinutes;
                    employee.annualWorkTimeMinutes = y.annualWorkTimeMinutes;

                    this.scheduleHandler.getEmployeeRows(y.employeeId).forEach(row => {
                        this.scheduleHandler.updateEmployeeRow(row, employee);
                    });
                }

                // Update dialog if opened (if recalc is called from dialog)
                if (this.annualSummaryDialogOpen && x.length === 1) {
                    this.messagingService.publish('annualScheduledTimeUpdated', { employeeId: employee.employeeId, annualScheduledTime: y.annualScheduledTimeMinutes });
                }
            });

            if (this.scheduledTimeEmployeeIds.length > 0)
                this.loadAnnualScheduledTimeSummary(null, false);
            else {
                this.loadingPlanningPeriodSummary = false;
                this.stopProgress();
            }
        });
    }

    private loadAnnualScheduledTime(employeeId: number) {
        if (!this.selectableInformationSettings.showPlanningPeriodSummary || !this.currentPlanningPeriod)
            return;

        this.scheduleService.getAnnualScheduledTimeSummaryForEmployee(employeeId, this.currentPlanningPeriod.startDate, this.currentPlanningPeriod.stopDate, TimeScheduledTimeSummaryType.Both).then(minutes => {
            this.updateAnnualScheduledTimeOnEmployee(employeeId, minutes);
        });
    }

    private updateAnnualScheduledTime(employeeId: number) {
        if (!this.selectableInformationSettings.showPlanningPeriodSummary || !this.currentPlanningPeriod)
            return;

        if (this.calculatePlanningPeriodScheduledTimeUseAveragingPeriod) {
            this.loadEmployeePeriodTimeSummary([employeeId]);
        } else {
            this.scheduleService.updateAnnualScheduledTimeSummaryForEmployee(employeeId, this.currentPlanningPeriod.startDate, this.currentPlanningPeriod.stopDate, false).then(x => {
                this.loadAnnualScheduledTimeSummary([employeeId]);
            });
        }
    }

    private updateAnnualScheduledTimeOnEmployee(employeeId: number, minutes: number) {
        let employee = this.getEmployeeById(employeeId);
        if (employee)
            employee.annualScheduledTimeMinutes = minutes;
    }

    private loadEmployeePeriodTimeSummary(employeeIds: number[]) {
        if (!this.calculatePlanningPeriodScheduledTime || !this.selectableInformationSettings.showPlanningPeriodSummary)
            return;

        if (!employeeIds || employeeIds.length === 0)
            employeeIds = this.getVisibleEmployeeIds();

        if (!this.currentPlanningPeriod || (!this.currentPlanningPeriodChildInRangeExact && !this.hasPlanningPeriodHeadButNoChild)) {
            // If no planning period is selected, clear all times
            employeeIds.forEach(employeeId => {
                let employee = this.getEmployeeById(employeeId);
                if (employee) {
                    employee.annualScheduledTimeMinutes = 0;
                    employee.annualWorkTimeMinutes = 0;
                    employee.parentScheduledTimeMinutes = 0;
                    employee.parentWorkedTimeMinutes = 0;
                    employee.parentRuleWorkedTimeMinutes = 0;
                    employee.parentPeriodBalanceTimeMinutes = 0;
                    if (this.hasPlanningPeriodHeadButNoChild) {
                        employee.childRuleWorkedTimeMinutes = 0;
                        employee.childPeriodBalanceTimeMinutes = 0;
                    }

                    this.scheduleHandler.getEmployeeRows(employeeId).forEach(row => {
                        this.scheduleHandler.updateEmployeeRow(row, employee);
                    });
                }
            });
            return;
        }

        if (this.currentPlanningPeriod) {
            this.loadingPlanningPeriodSummary = true;

            this.progressMessage = this.terms["time.schedule.planning.annualsummary.loading"];
            this.progressBusy = true;

            this.scheduleService.getEmployeePeriodTimeSummary(employeeIds, this.dateFrom, this.dateTo, this.currentPlanningPeriod.timePeriodHeadId).then(x => {
                x.forEach(y => {
                    // Update scheduled/work times

                    // Update employee in list
                    let employee = this.getEmployeeById(y.employeeId);
                    if (employee) {
                        employee.annualScheduledTimeMinutes = y.childScheduledTimeMinutes;
                        employee.annualWorkTimeMinutes = y.childWorkedTimeMinutes;
                        employee.parentScheduledTimeMinutes = y.parentScheduledTimeMinutes;
                        employee.parentWorkedTimeMinutes = y.parentWorkedTimeMinutes;
                        employee.parentRuleWorkedTimeMinutes = y.parentRuleWorkedTimeMinutes;
                        employee.parentPeriodBalanceTimeMinutes = y.parentPeriodBalanceTimeMinutes;
                        if (!this.hasPlanningPeriodHeadButNoChild) {
                            employee.childRuleWorkedTimeMinutes = y.childRuleWorkedTimeMinutes;
                            employee.childPeriodBalanceTimeMinutes = y.childPeriodBalanceTimeMinutes;
                        }

                        this.scheduleHandler.getEmployeeRows(y.employeeId).forEach(row => {
                            this.scheduleHandler.updateEmployeeRow(row, employee);
                        });
                    }
                });

                // Update dialog if opened (if recalc is called from dialog)
                if (this.annualSummaryDialogOpen && x.length === 1) {
                    this.messagingService.publish('employeePeriodTimeSummaryUpdated', x[0]);
                }

                this.loadingPlanningPeriodSummary = false;
                this.stopProgress();
            });
        } else {
            employeeIds.forEach(id => {
                // Update employee in list
                let employee = this.getEmployeeById(id);
                if (employee) {
                    employee.annualScheduledTimeMinutes = 0;
                    employee.annualWorkTimeMinutes = 0;
                    employee.parentScheduledTimeMinutes = 0;
                    employee.parentWorkedTimeMinutes = 0;
                    employee.parentRuleWorkedTimeMinutes = 0;
                    employee.parentPeriodBalanceTimeMinutes = 0;
                    employee.childRuleWorkedTimeMinutes = 0;
                    employee.childPeriodBalanceTimeMinutes = 0;

                    this.scheduleHandler.getEmployeeRows(id).forEach(row => {
                        this.scheduleHandler.updateEmployeeRow(row, employee);
                    });
                }
            });
        }
    }

    private loadCyclePlannedTime(employeeIds: number[], render: boolean) {
        this.loadingCycleTimes = true;
        // Remove null values from employeeIds that can sometimes occur
        this.scheduleService.getCyclePlannedMinutes(this.dateFrom, employeeIds.filter(x => x)).then(x => {
            x.forEach(y => {
                let emp = this.getEmployeeById(y.item1);
                if (emp) {
                    emp.cyclePlannedMinutes = y.item2;
                    emp.cyclePlannedAverageMinutes = y.item3;
                    this.setEmployeeToolTip(emp);
                }
            });

            if (render)
                this.renderBody('loadCyclePlannedTime');
            else
                this.scheduleHandler.updateEmployeesInfo(this.allEmployees.filter(e => employeeIds.includes(e.employeeId)));

            this.loadingCycleTimes = false;
        });
    }

    private loadAnnualLeaveBalanceForEmployees(employeeIds: number[] = null, clearList: boolean = true) {
        if (!this.selectableInformationSettings.showAnnualLeaveBalance)
            return;

        this.loadAnnualLeaveBalance = false;
        this.loadingAnnualLeaveBalance = true;

        if (clearList)
            this.setAnnualLeaveBalanceEmployeeIds(employeeIds);

        let processed = this.annualLeaveBalanceEmployeeIds.length > 0 ? ((1 - (this.annualLeaveBalanceEmployeeIds.length / this.annualLeaveBalanceEmployeeIdsCount)) * 100).round(0) : 100;

        this.progressMessage = "{0} ({1}%)".format(this.terms["time.schedule.planning.annualleave.balance.loading"], processed.toString());
        this.progressBusy = true;

        let batchEmployeeIds: number[] = this.annualLeaveBalanceEmployeeIds.splice(0, 25);  // Fetch for 25 employees at a time
        this.scheduleService.getAnnualLeaveBalance(this.getAnnualLeaveBalanceDate(), batchEmployeeIds).then(x => {
            x.forEach(y => {
                this.updateAnnualLeaveBalanceOnEmployee(y);
            });

            if (this.annualLeaveBalanceEmployeeIds.length > 0)
                this.loadAnnualLeaveBalanceForEmployees(null, false);
            else {
                this.loadingAnnualLeaveBalance = false;
                this.stopProgress();
            }
        });
    }

    private recalculateAnnualLeaveBalanceForEmployees(employeeIds: number[] = null, clearList = true, previousYear = false) {
        if (!this.selectableInformationSettings.showAnnualLeaveBalance)
            return;

        this.loadAnnualLeaveBalance = false;
        this.loadingAnnualLeaveBalance = true;

        if (clearList)
            this.setAnnualLeaveBalanceEmployeeIds(employeeIds);

        let processed = this.annualLeaveBalanceEmployeeIds.length > 0 ? ((1 - (this.annualLeaveBalanceEmployeeIds.length / this.annualLeaveBalanceEmployeeIdsCount)) * 100).round(0) : 100;

        this.progressMessage = "{0} ({1}%)".format(this.terms["time.schedule.planning.annualleave.balance.calculating"], processed.toString());
        this.progressBusy = true;

        let batchEmployeeIds: number[] = this.annualLeaveBalanceEmployeeIds.splice(0, 25);  // Calculate for 25 employees at a time
        this.scheduleService.recalculateAnnualLeaveBalance(this.getAnnualLeaveBalanceDate(), batchEmployeeIds, previousYear).then(x => {
            x.forEach(y => {
                this.updateAnnualLeaveBalanceOnEmployee(y);
            });

            if (this.annualLeaveBalanceEmployeeIds.length > 0)
                this.recalculateAnnualLeaveBalanceForEmployees(null, false, previousYear);
            else {
                this.loadingAnnualLeaveBalance = false;
                this.stopProgress();
            }
        });
    }

    private getAnnualLeaveBalanceDate(): Date {
        // End of visible period 
        return this.dateTo.beginningOfDay();
    }

    private setAnnualLeaveBalanceEmployeeIds(employeeIds: number[] = null) {
        let tempIds: number[] = [];
        // An employee should not see other employees times, only if running as admin
        if (this.isUser)
            tempIds.push(this.employeeId);
        else if (employeeIds)
            tempIds = employeeIds;
        else
            tempIds = this.getVisibleEmployeeIds();

        // Filter out employees that does not have an annual leave group
        this.annualLeaveBalanceEmployeeIds = [];
        tempIds.forEach(employeeId => {
            if (this.hasAnnualLeaveGroupById(employeeId))
                this.annualLeaveBalanceEmployeeIds.push(employeeId);
        });

        this.annualLeaveBalanceEmployeeIdsCount = this.annualLeaveBalanceEmployeeIds.length;
    }

    private updateAnnualLeaveBalanceOnEmployee(balance: IAnnualLeaveBalance) {
        // Update balance

        // Update employee in list
        let employee = this.getEmployeeById(balance.employeeId);
        if (employee) {
            employee.annualLeaveBalanceDays = balance.annualLeaveBalanceDays;
            employee.annualLeaveBalanceMinutes = balance.annualLeaveBalanceMinutes;

            this.scheduleHandler.getEmployeeRows(balance.employeeId).forEach(row => {
                this.scheduleHandler.updateEmployeeRow(row, employee);
            });
        }
    }

    private getHasStaffingByEmployeeAccount(): ng.IPromise<any> {
        return this.sharedScheduleService.getHasStaffingByEmployeeAccount(this.dateFrom).then(result => {
            this.hasStaffingByEmployeeAccount = result;
        });
    }

    // Use debounce
    // This will enable fast clicking on increase/decrease date buttons without loading after each click
    private loadUnscheduledTasksAndDeliveriesDates = _.debounce(() => {
        // Remember which date expanders that are open
        let openDates: Date[] = this.unscheduledTaskDates.filter(d => d['isOpen']);

        this.unscheduledTaskDates = [];
        this.unscheduledTasks = [];

        if (this.isScenarioView || this.isStandbyView || this.isTasksAndDeliveriesView || this.isStaffingNeedsView)
            return;

        if (!this.showUnscheduledTasksPermission || this.getFilteredShiftTypeIds().length === 0)
            return;

        var type: SoeStaffingNeedType = SoeStaffingNeedType.Employee;
        if (this.isTemplateView)
            type = SoeStaffingNeedType.Template;
        else if (this.isEmployeePostView)
            type = SoeStaffingNeedType.EmployeePost;

        this.scheduleService.getStaffingNeedsUnscheduledTaskDates(this.getFilteredShiftTypeIds(), this.dateFrom, this.isDayView ? this.dateFrom : this.dateTo, type).then(x => {
            // Clear collections again, just in case this method is called twice, it could produce duplicate dates
            this.unscheduledTaskDates = [];
            this.unscheduledTasks = [];

            for (let i = 0, j = x.length; i < j; i++) {
                let date = x[i];
                if (CalendarUtility.includesDate(openDates, date) || x.length === 1)
                    date['isOpen'] = true;
                this.unscheduledTaskDates.push(date);
            }

            if (this.showUnscheduledTasks)
                this.loadUnscheduledTasksAndDeliveries();
        });
    }, 500, { leading: false, trailing: true });

    private loadUnscheduledTasksAndDeliveries() {
        if (!this.showUnscheduledTasksPermission)
            return;

        this.loadingUnscheduledTasksAndDeliveries = true;

        let type: SoeStaffingNeedType = SoeStaffingNeedType.Employee;
        if (this.isTemplateView)
            type = SoeStaffingNeedType.Template;
        else if (this.isEmployeePostView)
            type = SoeStaffingNeedType.EmployeePost;

        this.scheduleService.getStaffingNeedsUnscheduledTasks(this.getFilteredShiftTypeIds(), this.dateFrom, this.isDayView ? this.dateFrom : this.dateTo, type).then(x => {
            this.unscheduledTasks = x;

            if (this.unscheduledTasks.length === 0) {
                if (this.showUnscheduledTasks)
                    this.toggleShowUnscheduledTasks(true);
                this.translationService.translate("time.schedule.planning.unscheduledtasks.notasks").then(term => {
                    this.notificationService.showDialogEx(this.terms["core.info"], term, SOEMessageBoxImage.OK);
                });
            } else {
                this.unscheduledTasks = _.orderBy(this.unscheduledTasks, ['startTime', 'actualStartTime', 'actualStopTime', 'name']);
                this.unscheduledTaskDates.forEach(date => {
                    let dateTasks = this.unscheduledTasks.filter(t => t.startTime.isSameDayAs(date));
                    date['label'] = "{0}, {1} {2}".format(CalendarUtility.minutesToTimeSpan(_.sumBy(dateTasks, t => t.length)), dateTasks.length.toString(), this.terms["core.pieces.short"].toLowerCase());
                });
            }

            this.loadingUnscheduledTasksAndDeliveries = false;
        }).catch(reason => {
            this.loadingUnscheduledTasksAndDeliveries = false;
            this.translationService.translate("time.schedule.planning.unscheduledtasks.error").then(term => {
                this.notificationService.showServiceError(reason, term);
            });
        });
    }

    // Use debounce
    // This will enable fast clicking on increase/decrease date buttons without loading after each click
    private loadScheduleEventDates = _.debounce(() => {
        this.scheduleService.getTimeScheduleEventDatesForPlanning(this.dateFrom, this.dateTo).then(x => {
            this.scheduleEventDates = CalendarUtility.convertToDates(x);
        });
    }, 300, { leading: false, trailing: true });

    private loadScheduleEvents(date: Date): ng.IPromise<TimeScheduleEventForPlanningDTO[]> {
        let deferral = this.$q.defer<TimeScheduleEventForPlanningDTO[]>();

        this.scheduleService.getTimeScheduleEventsForPlanning(date).then(x => {
            let scheduleEvents = x.map(s => {
                let obj = new TimeScheduleEventForPlanningDTO();
                angular.extend(obj, s);
                obj.fixDates();
                return obj;
            });

            deferral.resolve(scheduleEvents);
        });

        return deferral.promise;
    }

    // Use debounce
    // This will enable fast clicking on increase/decrease date buttons without loading after each click
    private loadHolidays = _.debounce(() => {
        this.scheduleService.getHolidaysSmall(this.dateFrom, this.dateTo).then(x => {
            this.holidays = x;

            this.dates.forEach(dateDay => {
                this.setDateRangeDayTypes(dateDay);
            });
        });
    }, 300, { leading: false, trailing: true });

    private createEmptyScheduleForEmployeePost(employeePost: EmployeeListDTO) {
        if (!employeePost.employeePostId)
            return;

        this.createEmptyScheduleForSpecifiedEmployeePosts([employeePost.employeePostId]);
    }

    private createEmptyScheduleForEmployeePosts() {
        // Only create empty templates for filtered employee posts without templates
        let employeePostIds: number[] = [];
        let filteredEmployeePostIds: number[] = this.getFilteredEmployeePostIds();
        filteredEmployeePostIds.forEach(employeePostId => {
            let employee = this.getEmployeePostById(employeePostId);
            if (employee && employee.employeePostStatus !== SoeEmployeePostStatus.Locked && !employee.hasTemplateScheduleByStartDate(this.dateFrom))
                employeePostIds.push(employeePostId);
        });
        if (employeePostIds.length === 0)
            return;

        this.createEmptyScheduleForSpecifiedEmployeePosts(employeePostIds);
    }

    private createEmptyScheduleForSpecifiedEmployeePosts(employeePostIds: number[]) {
        this.startWork();

        this.scheduleService.createEmptyScheduleForEmployeePosts(employeePostIds, this.dateFrom).then((result: IActionResult) => {
            if (!result.success) {
                const keys: string[] = [
                    "time.schedule.planning.createemptyscheduleforemployeeposts.error.title",
                    "time.schedule.planning.createemptyscheduleforemployeeposts.error.message"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    // If an error message was passed from server, use it.
                    // Otherwise build message of failed employee posts.
                    var message: string;
                    if (result.errorMessage)
                        message = result.errorMessage;
                    else {
                        message = terms["time.schedule.planning.createemptyscheduleforemployeeposts.error.message"] + "\n";

                        let failedIds: number[] = _.difference(employeePostIds, result.keys);
                        failedIds.forEach(id => {
                            var employeePost = this.getEmployeePostById(id);
                            if (employeePost)
                                message += "{0}\n".format(employeePost.name);
                        })
                    }
                    this.failedWork(message, terms["time.schedule.planning.createemptyscheduleforemployeeposts.error.title"]);
                });
            }
            // Reload successful posts
            if (result.keys.length > 0) {
                this.reloadEmployeePosts(result.keys, false).then(() => {
                    this.reloadShiftsForSpecifiedEmployeeIds = result.keys;
                    this.loadEmployeePostShifts();
                });
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }

    private generateScheduleForEmployeePost(employeePost: EmployeeListDTO) {
        if (!employeePost.employeePostId)
            return;

        this.startWork();

        this.scheduleService.createScheduleFromEmployeePost(employeePost.employeePostId, this.dateFrom).then(shifts => {
            if (shifts.length === 0) {
                var keys: string[] = [
                    "time.schedule.planning.noschedulegeneratedforemployeepost.title",
                    "time.schedule.planning.noschedulegeneratedforemployeepost.message",
                    "time.schedule.planning.noschedulegeneratedforemployeepost.createemptymessage"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.completedWork(null, true);
                    var message: string = terms["time.schedule.planning.noschedulegeneratedforemployeepost.message"];
                    if (!employeePost.hasTemplateSchedules) {
                        message += "\n\n{0}".format(terms["time.schedule.planning.noschedulegeneratedforemployeepost.createemptymessage"]);

                        this.notificationService.showDialogEx(terms["time.schedule.planning.noschedulegeneratedforemployeepost.title"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo).result.then(val => {
                            if (val)
                                this.createEmptyScheduleForEmployeePost(employeePost);
                        });
                    } else {
                        this.notificationService.showDialogEx(terms["time.schedule.planning.noschedulegeneratedforemployeepost.title"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    }
                });
            } else {
                this.reloadEmployeePosts([employeePost.employeePostId], false).then(() => {
                    this.loadUnscheduledTasksAndDeliveriesDates();
                    this.reloadShiftsForSpecifiedEmployeeIds = [employeePost.employeePostId];
                    this.loadData('generateScheduleForEmployeePost');
                });
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }

    private generateScheduleForEmployeePosts() {
        let employeePostIds: number[] = [];
        let filteredEmployeePostIds: number[] = this.getFilteredEmployeePostIds();
        filteredEmployeePostIds.forEach(employeePostId => {
            let employee = this.getEmployeePostById(employeePostId);
            if (employee && employee.employeePostStatus !== SoeEmployeePostStatus.Locked)
                employeePostIds.push(employeePostId);
        });
        if (employeePostIds.length === 0)
            return;

        // Start long running process
        let abort = false;
        if (this.progressModal)
            this.progressModal = null;
        this.startWork(null, true, () => { abort = true; });
        this.scheduleService.createScheduleFromEmployeePostsAsync(employeePostIds, this.dateFrom).then(x => {
            // Get key (guid) for the process
            let pollingKey = x.pollingKey;

            // At two seconds intervals, poll and check progress
            let createScheduleTimer = setInterval(() => {
                this.coreService.getProgressInfo(pollingKey.toString()).then(progress => {
                    if (abort)
                        progress.abort = true;

                    if (progress.error) {
                        clearInterval(createScheduleTimer);
                        this.progressModalMetaData.showabort = false;
                        this.failedWork(progress.errorMessage);
                    } else if (progress.done || progress.abort) {
                        // Done or aborted, stop polling and display result
                        clearInterval(createScheduleTimer);
                        this.reloadEmployeePosts(employeePostIds, false).then(() => {
                            this.reloadShiftsForSpecifiedEmployeeIds = employeePostIds;
                            this.loadUnscheduledTasksAndDeliveriesDates();
                            this.loadData('generateScheduleForEmployeePosts');
                        });
                    } else {
                        // Update message in progress dialog
                        if (this.progressModalMetaData)
                            this.progressModalMetaData.text = progress.message;
                    }
                });
            }, 2000);
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }

    private deleteScheduleForEmployeePost(employeePost: EmployeeListDTO, render: boolean): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (!employeePost.employeePostId)
            deferral.resolve(false);
        else {
            this.startDelete();
            this.scheduleService.deleteScheduleFromEmployeePost(employeePost.employeePostId).then(result => {
                if (result.success && render) {
                    _.remove(this.shifts, s => s.employeePostId === employeePost.employeePostId);
                    this.setAllShiftsMap();
                    this.reloadEmployeePosts([employeePost.employeePostId], false).then(() => {
                        this.reloadShiftsForSpecifiedEmployeeIds = [employeePost.employeePostId];
                        this.filterShifts('deleteScheduleForEmployeePost', false);
                        this.render();
                        this.loadUnscheduledTasksAndDeliveriesDates();
                    });
                }
                deferral.resolve(result.success);
            }).catch(reason => {
                this.notificationService.showServiceError(reason);
                this.completedWork(null, true);
            });
        }

        return deferral.promise;
    }

    private deleteScheduleForEmployeePosts() {
        let employeePostIds: number[] = [];
        let filteredEmployeePostIds: number[] = this.getFilteredEmployeePostIds();
        filteredEmployeePostIds.forEach(employeePostId => {
            let employee = this.getEmployeePostById(employeePostId);
            if (employee && employee.employeePostStatus !== SoeEmployeePostStatus.Locked)
                employeePostIds.push(employeePostId);
        });

        let deferral = this.$q.defer();

        if (!employeePostIds || employeePostIds.length === 0)
            deferral.resolve(false);
        else {
            this.startDelete();
            this.scheduleService.deleteScheduleFromEmployeePosts(employeePostIds).then(result => {
                if (result.success) {
                    if (result.keys) {
                        result.keys.forEach(employeePostId => {
                            _.remove(this.shifts, s => s.employeePostId === employeePostId);
                        });
                    }
                    this.reloadEmployeePosts(result.keys, false).then(() => {
                        this.reloadShiftsForSpecifiedEmployeeIds = result.keys;
                        this.filterShifts('deleteScheduleForEmployeePosts', false);
                        this.render();
                        this.loadUnscheduledTasksAndDeliveriesDates();
                    });
                }
                deferral.resolve(result.success);
            }).catch(reason => {
                this.notificationService.showServiceError(reason);
                this.completedWork(null, true);
            });
        }

        return deferral.promise;
    }

    private preAnalyseEmployeePost(employeePost: EmployeeListDTO) {
        this.startWork("core.analysing")
        this.scheduleService.getPreAnalysisInformation(employeePost.employeePostId, this.dateFrom).then(x => {
            this.completedWork(null, true);

            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/PreAnalysisInformation/Views/preAnalysisInformation.html"),
                controller: PreAnalysisInformationController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'lg',
                windowClass: 'fullsize-modal',
                resolve: {
                    info: () => { return x }
                }
            }
            this.$uibModal.open(options);
        });
    }

    private removeEmployeeFromEmployeePost(employeePost: EmployeeListDTO) {
        if (!employeePost.employeePostId || !employeePost.employeeId)
            return;

        let employee = this.getEmployeeById(employeePost.employeeId);
        if (employee) {
            let template = this.templateHelper.getTemplateSchedule(this.isEmployeePostView ? employeePost.employeePostId : employeePost.employeeId, this.dateFrom);
            if (template) {
                this.translationService.translate("time.schedule.planning.removeemployeefromemployeepost.warning").then(term => {
                    const modal = this.notificationService.showDialog(this.terms["core.warning"], term.format(employee.numberAndName, employeePost.name.trim()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(result => {
                        this.startDelete();
                        this.scheduleService.removeEmployeeFromTimeScheduleTemplate(template.timeScheduleTemplateHeadId).then(res => {
                            if (res.success) {
                                employeePost.employeeId = 0;
                                this.reloadShiftsForSpecifiedEmployeeIds = [employeePost.employeePostId];
                                this.loadData('removeEmployeeFromEmployeePost');
                            }
                        }).catch(reason => {
                            this.notificationService.showServiceError(reason);
                            this.completedWork(null, true);
                        });
                    });
                });
            }
        }
    }

    private changeStatusForEmployeePost(employeePost: EmployeeListDTO, status: SoeEmployeePostStatus) {
        if (!employeePost.employeePostId)
            return;

        this.scheduleService.changeStatusForEmployeePost(employeePost.employeePostId, status).then(x => {
            employeePost.employeePostStatus = status;
            this.setEmployeePostShiftsAsReadonly(this.shifts.filter(s => s.employeePostId === employeePost.employeePostId), [employeePost.employeePostId]);
            this.scheduleHandler.rememberVerticalScroll();
            this.reloadShiftsForSpecifiedEmployeeIds = [employeePost.employeePostId];
            this.render();
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }

    // EVENTS

    private closeTemplateScheduleWarning() {
        this.disableTemplateScheduleWarning = true;
        if (this.doNotShowTemplateScheduleWarningAgain)
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeSchedulePlanningDisableTemplateScheduleWarning, true);
    }

    private toggleFilters() {
        this.showFilters = !this.showFilters;
    }

    private showSelectableInformation(showFollowUpOnly: boolean = false, loadAll: boolean = false) {
        // Show selectable information dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/SelectableInformation/Views/selectableInformation.html"),
            controller: SelectableInformationController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: this.isCalendarView ? 'md' : 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                settings: () => { return this.selectableInformationSettings },
                useAccountHierarchy: () => { return this.useAccountHierarchy },
                accountHierarchyId: () => { return this.accountHierarchyId; },
                userAccountId: () => { return this.userAccountId; },
                allAccountsSelected: () => { return this.allAccountsSelected },
                isAdmin: () => { return this.isAdmin },
                isSchedulePlanningMode: () => { return this.isSchedulePlanningMode },
                viewDefinition: () => { return this.viewDefinition },
                modifyPermission: () => { return this.hasCurrentViewModifyPermission },
                minutesLabel: () => { return this.terms["core.time.minutes"].toLocaleLowerCase() },
                showGrossTimeSetting: () => { return this.showGrossTimeSetting },
                showTotalCostPermission: () => { return this.showTotalCostPermission },
                showDashboardPermission: () => { return this.showDashboardPermission },
                showStaffingNeedsPermission: () => { return this.showStaffingNeedsPermission },
                showBudgetPermission: () => { return this.showBudgetPermission },
                showForecastPermission: () => { return this.showForecastPermission },
                calculatePlanningPeriodScheduledTime: () => { return this.calculatePlanningPeriodScheduledTime },
                showFollowUpOnly: () => { return showFollowUpOnly },
                sendXEMailOnChange: () => { return this.sendXEMailOnChange },
                followUpCalculationTypes: () => { return this.followUpCalculationTypes },
                accountDims: () => { return this.accountDims },
                useAnnualLeave: () => { return this.useAnnualLeave },
            }
        }
        this.$uibModal.open(options).result.then(result => {
            let settings: TimeSchedulePlanningSettingsDTO = result.settings;

            // Action flags
            let calculateTimes = false;
            let renderBody = false;
            let renderChart = false;

            // Changes flags
            let showInactiveEmployeesChanged = false;
            let showUnemployedEmployeesChanged = false;
            let showFullyLendedEmployeesChanged = false;
            let showEmployeeGroupChanged = false;
            let showCyclePlannedTimeChanged = false;
            let showScheduleTypeFactorTimeChanged = false;
            let showGrossTimeChanged = false;
            let showTotalCostChanged = false;
            let showTotalCostIncEmpTaxAndSuppChargeChanged = false;
            let showWeekendSalaryChanged = false;
            let includeLendedShiftsInTimeCalculationsChanged = false;
            let useShiftTypeCodeChanged = false;
            let showWeekNumberChanged = false;
            let shiftTypePositionChanged = false;
            let timePositionChanged = false;
            let hideTimeOnShiftShorterThanMinutesChanged = false;
            let breakVisibilityChanged = false;
            let showAvailabilityChanged = false;
            let showAbsenceRequestsChanged = false;
            let showPlanningPeriodSummaryChanged = false;
            let showAnnualLeaveBalanceChanged = false;
            let showAnnualLeaveBalanceFormatChanged = false;

            // Check for changes
            if (settings.showHiddenShifts !== this.selectableInformationSettings.showHiddenShifts)
                renderBody = true;

            if (settings.showInactiveEmployees !== this.selectableInformationSettings.showInactiveEmployees)
                showInactiveEmployeesChanged = true;

            if (settings.showUnemployedEmployees !== this.selectableInformationSettings.showUnemployedEmployees)
                showUnemployedEmployeesChanged = true;

            if (settings.showFullyLendedEmployees !== this.selectableInformationSettings.showFullyLendedEmployees)
                showFullyLendedEmployeesChanged = true;

            if (settings.showEmployeeGroup !== this.selectableInformationSettings.showEmployeeGroup)
                showEmployeeGroupChanged = true;

            if (settings.showCyclePlannedTime !== this.selectableInformationSettings.showCyclePlannedTime)
                showCyclePlannedTimeChanged = true;

            if (settings.showScheduleTypeFactorTime !== this.selectableInformationSettings.showScheduleTypeFactorTime)
                showScheduleTypeFactorTimeChanged = true;

            if (settings.showGrossTime !== this.selectableInformationSettings.showGrossTime)
                showGrossTimeChanged = true;

            if (settings.showTotalCost !== this.selectableInformationSettings.showTotalCost)
                showTotalCostChanged = true;

            if (settings.showTotalCostIncEmpTaxAndSuppCharge !== this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge)
                showTotalCostIncEmpTaxAndSuppChargeChanged = true;

            if (settings.showWeekendSalary !== this.selectableInformationSettings.showWeekendSalary)
                showWeekendSalaryChanged = true;

            if (settings.includeLendedShiftsInTimeCalculations !== this.selectableInformationSettings.includeLendedShiftsInTimeCalculations)
                includeLendedShiftsInTimeCalculationsChanged = true;

            if (settings.showPlanningPeriodSummary !== this.selectableInformationSettings.showPlanningPeriodSummary ||
                settings.planningPeriodHeadId !== this.selectableInformationSettings.planningPeriodHeadId)
                showPlanningPeriodSummaryChanged = true;

            if (settings.showAnnualLeaveBalance !== this.selectableInformationSettings.showAnnualLeaveBalance)
                showAnnualLeaveBalanceChanged = true;

            if (settings.showAnnualLeaveBalanceFormat !== this.selectableInformationSettings.showAnnualLeaveBalanceFormat)
                showAnnualLeaveBalanceFormatChanged = true;

            if (settings.useShiftTypeCode !== this.selectableInformationSettings.useShiftTypeCode)
                useShiftTypeCodeChanged = true;

            if (settings.showWeekNumber !== this.selectableInformationSettings.showWeekNumber)
                showWeekNumberChanged = true;

            if (settings.shiftTypePosition !== this.selectableInformationSettings.shiftTypePosition)
                shiftTypePositionChanged = true;

            if (settings.timePosition !== this.selectableInformationSettings.timePosition)
                timePositionChanged = true;

            if (settings.hideTimeOnShiftShorterThanMinutes !== this.selectableInformationSettings.hideTimeOnShiftShorterThanMinutes)
                hideTimeOnShiftShorterThanMinutesChanged = true;

            if (settings.breakVisibility !== this.selectableInformationSettings.breakVisibility)
                breakVisibilityChanged = true;

            if (settings.showAvailability !== this.selectableInformationSettings.showAvailability)
                showAvailabilityChanged = true;

            if (settings.showAbsenceRequests !== this.selectableInformationSettings.showAbsenceRequests)
                showAbsenceRequestsChanged = true;

            if (settings.followUpOnNeed !== this.selectableInformationSettings.followUpOnNeed ||
                settings.followUpOnNeedFrequency !== this.selectableInformationSettings.followUpOnNeedFrequency ||
                settings.followUpOnNeedRowFrequency !== this.selectableInformationSettings.followUpOnNeedRowFrequency ||
                settings.followUpOnBudget !== this.selectableInformationSettings.followUpOnBudget ||
                settings.followUpOnForecast !== this.selectableInformationSettings.followUpOnForecast ||
                settings.followUpOnTemplateSchedule !== this.selectableInformationSettings.followUpOnTemplateSchedule ||
                settings.followUpOnTemplateScheduleForEmployeePost !== this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost ||
                settings.followUpOnSchedule !== this.selectableInformationSettings.followUpOnSchedule ||
                settings.followUpOnTime !== this.selectableInformationSettings.followUpOnTime ||
                settings.followUpCalculationType !== this.selectableInformationSettings.followUpCalculationType ||
                settings.followUpAccountDimId !== this.selectableInformationSettings.followUpAccountDimId ||
                settings.followUpAccountId !== this.selectableInformationSettings.followUpAccountId) {
                renderChart = true;
            }

            if (settings.followUpShowCalculationTypeSales !== this.selectableInformationSettings.followUpShowCalculationTypeSales ||
                settings.followUpShowCalculationTypeHours !== this.selectableInformationSettings.followUpShowCalculationTypeHours ||
                settings.followUpShowCalculationTypePersonelCost !== this.selectableInformationSettings.followUpShowCalculationTypePersonelCost ||
                settings.followUpShowCalculationTypeSalaryPercent !== this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercent ||
                settings.followUpShowCalculationTypeLPAT !== this.selectableInformationSettings.followUpShowCalculationTypeLPAT ||
                settings.followUpShowCalculationTypeFPAT !== this.selectableInformationSettings.followUpShowCalculationTypeFPAT) {
                renderChart = true;
                loadAll = true;
            }

            if (!renderChart && !loadAll) {
                if (settings.followUpShowCalculationTypeSalesBudget !== this.selectableInformationSettings.followUpShowCalculationTypeSalesBudget ||
                    settings.followUpShowCalculationTypeSalesTime !== this.selectableInformationSettings.followUpShowCalculationTypeSalesTime ||
                    settings.followUpShowCalculationTypeHoursBudget !== this.selectableInformationSettings.followUpShowCalculationTypeHoursBudget ||
                    settings.followUpShowCalculationTypeHoursTemplateSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeHoursTemplateSchedule ||
                    settings.followUpShowCalculationTypeHoursSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeHoursSchedule ||
                    settings.followUpShowCalculationTypeHoursTime !== this.selectableInformationSettings.followUpShowCalculationTypeHoursTime ||
                    settings.followUpShowCalculationTypePersonelCostBudget !== this.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget ||
                    settings.followUpShowCalculationTypePersonelCostTemplateSchedule !== this.selectableInformationSettings.followUpShowCalculationTypePersonelCostTemplateSchedule ||
                    settings.followUpShowCalculationTypePersonelCostSchedule !== this.selectableInformationSettings.followUpShowCalculationTypePersonelCostSchedule ||
                    settings.followUpShowCalculationTypePersonelCostScheduleAndTime !== this.selectableInformationSettings.followUpShowCalculationTypePersonelCostScheduleAndTime ||
                    settings.followUpShowCalculationTypePersonelCostTime !== this.selectableInformationSettings.followUpShowCalculationTypePersonelCostTime ||
                    settings.followUpShowCalculationTypeSalaryPercentBudget !== this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentBudget ||
                    settings.followUpShowCalculationTypeSalaryPercentTemplateSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTemplateSchedule ||
                    settings.followUpShowCalculationTypeSalaryPercentSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentSchedule ||
                    settings.followUpShowCalculationTypeSalaryPercentTime !== this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTime ||
                    settings.followUpShowCalculationTypeLPATBudget !== this.selectableInformationSettings.followUpShowCalculationTypeLPATBudget ||
                    settings.followUpShowCalculationTypeLPATTemplateSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeLPATTemplateSchedule ||
                    settings.followUpShowCalculationTypeLPATSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeLPATSchedule ||
                    settings.followUpShowCalculationTypeLPATTime !== this.selectableInformationSettings.followUpShowCalculationTypeLPATTime ||
                    settings.followUpShowCalculationTypeFPATBudget !== this.selectableInformationSettings.followUpShowCalculationTypeFPATBudget ||
                    settings.followUpShowCalculationTypeFPATTemplateSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeFPATTemplateSchedule ||
                    settings.followUpShowCalculationTypeFPATSchedule !== this.selectableInformationSettings.followUpShowCalculationTypeFPATSchedule ||
                    settings.followUpShowCalculationTypeFPATTime !== this.selectableInformationSettings.followUpShowCalculationTypeFPATTime)
                    renderChart = true;
                loadAll = true;
            }

            // Update settings
            this.selectableInformationSettings = new TimeSchedulePlanningSettingsDTO(false);
            angular.extend(this.selectableInformationSettings, settings);

            // Actions due to changes
            if (showInactiveEmployeesChanged) {
                this.loadEmployees().then(() => {
                    if (this.isEmployeePostView)
                        this.loadEmployeePosts(true);
                });
            } else {
                if (showAbsenceRequestsChanged || showWeekendSalaryChanged) {
                    if (showAbsenceRequestsChanged || (showWeekendSalaryChanged && !this.isCalendarView))
                        this.loadShifts();
                    else if (showWeekendSalaryChanged && this.isCalendarView) {
                        this.loadPeriods();
                    }
                } else {
                    if (showUnemployedEmployeesChanged) {
                        this.setEmployeeData();
                        this.setEmployedEmployees();
                        this.filterEmployees('showSelectableInformation', false);
                        renderBody = true;
                    }

                    if (showFullyLendedEmployeesChanged) {
                        renderBody = true;
                    }

                    if (showEmployeeGroupChanged) {
                        renderBody = true;
                        if (this.isCommonDayView)
                            this.setEmployeesToolTip();
                    }

                    if (showCyclePlannedTimeChanged) {
                        if (this.selectableInformationSettings.showCyclePlannedTime) {
                            this.loadCyclePlannedTime(this.visibleEmployees.map(e => e.employeeId), true);
                        } else {
                            this.setEmployeesToolTip();
                            renderBody = true;
                        }
                    }

                    if (showScheduleTypeFactorTimeChanged) {
                        renderBody = true;
                    }

                    if (showGrossTimeChanged || showTotalCostChanged || showTotalCostIncEmpTaxAndSuppChargeChanged) {
                        if (this.isCalendarView)
                            this.loadPeriodsGrossNetAndCost();
                        else
                            this.loadShiftsGrossNetAndCost();
                    }

                    if (includeLendedShiftsInTimeCalculationsChanged) {
                        calculateTimes = true;
                        renderBody = true;
                    }

                    if (showPlanningPeriodSummaryChanged && (this.isScheduleView || this.isTemplateScheduleView) && this.hasEmployeesLoaded) {
                        if (this.selectableInformationSettings.showPlanningPeriodSummary) {
                            this.loadCurrentPlanningPeriod(true);
                            if (this.hasEmployeesLoaded && (!this.disableAutoLoad || this.firstLoadHasOccurred))
                                this.loadPlanningPeriodSummary = true;
                        } else {
                            this.scheduleHandler.setColgroupWidths();
                        }
                        renderBody = true;
                    }

                    if ((showAnnualLeaveBalanceChanged || showAnnualLeaveBalanceFormatChanged) && (this.isScheduleView || this.isDayView) && this.hasEmployeesLoaded) {
                        if (showAnnualLeaveBalanceChanged && this.selectableInformationSettings.showAnnualLeaveBalance) {
                            this.loadAnnualLeaveBalance = true;
                        } else if (showAnnualLeaveBalanceFormatChanged && this.selectableInformationSettings.showAnnualLeaveBalance) {
                            // Only format changed, no need to refetch data, just render body again
                        }
                        renderBody = true;
                    }

                    if (useShiftTypeCodeChanged || showWeekNumberChanged || shiftTypePositionChanged || timePositionChanged || hideTimeOnShiftShorterThanMinutesChanged || breakVisibilityChanged) {
                        renderBody = true;
                    }

                    if (showAvailabilityChanged && !this.isCalendarView && this.hasEmployeesLoaded) {
                        if (this.selectableInformationSettings.showAvailability) {
                            this.loadEmployeeAvailability();
                        } else {
                            renderBody = true;
                        }
                    }

                    if (calculateTimes) {
                        this.calculateTimes();
                        this.clearShiftToolTips();
                    }

                    if (renderBody)
                        this.renderBody('showSelectableInformation');

                    if (renderChart)
                        this.loadStaffingNeed();
                }
            }

            // Save settings
            if (result.saveSettings)
                this.saveSelectableInformationSettings();
        }, (reason) => {
        });
    }

    private setShiftStyle(index: number, render: boolean) {
        this.shiftStyle = index;
        this.setShiftLabels(this.shifts);
        this.renderBody('setShiftStyle');
    }

    private setGroupBy(index: number, render: boolean) {
        if (this.isTasksAndDeliveriesDayView) {
            this.tadDayViewGroupBy = index;
            this.setSortBy(this.tadDayViewSortBy, render);
        } else if (this.isTasksAndDeliveriesScheduleView) {
            this.tadScheduleViewGroupBy = index;
            this.setSortBy(this.tadDayViewSortBy, render);
        } else if (this.isCommonDayView) {
            this.dayViewGroupBy = index;
            this.setSortBy(this.dayViewSortBy, render);
        } else if (this.isCommonScheduleView) {
            this.scheduleViewGroupBy = index;
            this.setSortBy(this.scheduleViewSortBy, render);
        }
    }

    private setSortBy(index: number, render: boolean) {
        if (this.isTasksAndDeliveriesDayView)
            this.tadDayViewSortBy = index;
        else if (this.isCommonDayView)
            this.dayViewSortBy = index;
        else if (this.isCommonScheduleView)
            this.scheduleViewSortBy = index;

        if (this.isTasksAndDeliveriesView)
            this.sortTasks(render);
        else
            this.sortEmployees(render);
    }

    private resetSort() {
        let sort: number = 0;
        if (this.isCommonDayView)
            sort = this.dayViewSortBy;
        else if (this.isCommonScheduleView)
            sort = this.scheduleViewSortBy;
        this.setSortBy(sort, false);
    }

    private setEmployeeListSortBy(index: number) {
        this.employeeListSortBy = index;

        this.sortEmployeeList();
    }

    private setOrderListSortBy(index: number) {
        this.orderListSortBy = index;

        this.sortOrderList();
    }

    private toggleSecondaryAccounts() {
        this.$timeout(() => {
            this.$q.all([
                this.loadAccountsByUserFromHierarchy(),
                this.loadAccountDims(true),
                this.loadUserShiftTypes(true)
            ]).then(() => {
                this.loadEmployees(true);
            });
        });
    }

    private selectOrderInList(order: OrderListDTO) {
        // When an order is selected in the order list, highlight it and also highlight related shifts.
        // If same order is selected again, remove selection.

        let alreadySelected = order.selected;
        this.orderList.filter(o => o.selected).forEach(o => {
            o.selected = false;
        });
        this.scheduleHandler.unhighlightShifts(this.shifts.filter(s => s.highlighted));

        if (!alreadySelected) {
            order.selected = true;
            this.scheduleHandler.highlightShifts(this.shifts.filter(s => s.isOrder && s.order.orderId === order.orderId));
        }
    }

    private openOrderInList(order: OrderListDTO) {
        this.openOrder(order);
    }

    private primaryViewDefinitionSelected(viewDef: TermGroup_TimeSchedulePlanningViews) {
        // Switch between main views (schedule, template, scenario etc.)

        switch (viewDef) {
            case TermGroup_TimeSchedulePlanningViews.Day:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.Day : TermGroup_TimeSchedulePlanningViews.Schedule;
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.TemplateDay : TermGroup_TimeSchedulePlanningViews.TemplateSchedule;
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.EmployeePostsDay : TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule;
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.ScenarioDay : TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.StandbyDay : TermGroup_TimeSchedulePlanningViews.StandbySchedule;
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay : TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule;
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                viewDef = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay : TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule;
                break;
        }

        this.viewDefinitionSelected(viewDef);
    }

    private secondaryViewDefinitionSelected(viewDef: TermGroup_TimeSchedulePlanningViews) {
        // Switch between calendar, day and schedule views

        if (viewDef !== TermGroup_TimeSchedulePlanningViews.ScenarioComplete) {
            if (this.isTemplateView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.TemplateDay : TermGroup_TimeSchedulePlanningViews.TemplateSchedule;
            else if (this.isEmployeePostView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.EmployeePostsDay : TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule;
            else if (this.isScenarioView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.ScenarioDay : TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;
            else if (this.isStandbyView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.StandbyDay : TermGroup_TimeSchedulePlanningViews.StandbySchedule;
            else if (this.isTasksAndDeliveriesView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay : TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule;
            else if (this.isStaffingNeedsView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay : TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule;
            else if (this.isScenarioCompleteView)
                viewDef = viewDef === TermGroup_TimeSchedulePlanningViews.Day ? TermGroup_TimeSchedulePlanningViews.ScenarioDay : TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;
        }

        this.viewDefinitionSelected(viewDef);
    }

    private viewDefinitionSelected(viewDef: TermGroup_TimeSchedulePlanningViews) {
        if (this.viewDefinition !== viewDef)
            this.viewDefinition = viewDef;
    }

    private viewDefinitionChanged(fromViewDef: TermGroup_TimeSchedulePlanningViews, toViewDef: TermGroup_TimeSchedulePlanningViews) {
        // Create some convenience properties
        let fromSchedule = (fromViewDef === TermGroup_TimeSchedulePlanningViews.Calendar || fromViewDef === TermGroup_TimeSchedulePlanningViews.Day || fromViewDef === TermGroup_TimeSchedulePlanningViews.Schedule);
        let fromTemplate = (fromViewDef === TermGroup_TimeSchedulePlanningViews.TemplateDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.TemplateSchedule);
        let fromEmployeePost = (fromViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule);
        let fromScenario = (fromViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || fromViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioComplete);
        let fromStandby = (fromViewDef === TermGroup_TimeSchedulePlanningViews.StandbyDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.StandbySchedule);
        let fromTasksAndDeliveries = (fromViewDef === TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule);
        let fromStaffingNeeds = (fromViewDef === TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule);
        let toSchedule = (toViewDef === TermGroup_TimeSchedulePlanningViews.Calendar || toViewDef === TermGroup_TimeSchedulePlanningViews.Day || toViewDef === TermGroup_TimeSchedulePlanningViews.Schedule);
        let toTemplate = (toViewDef === TermGroup_TimeSchedulePlanningViews.TemplateDay || toViewDef === TermGroup_TimeSchedulePlanningViews.TemplateSchedule);
        let toEmployeePost = (toViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay || toViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule);
        let toScenario = (toViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioDay || toViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || toViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioComplete);
        let toStandby = (toViewDef === TermGroup_TimeSchedulePlanningViews.StandbyDay || toViewDef === TermGroup_TimeSchedulePlanningViews.StandbySchedule);
        let toTasksAndDeliveries = (toViewDef === TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay || toViewDef === TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule);
        let toStaffingNeeds = (toViewDef === TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay || toViewDef === TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule);

        let fromAnyDay = (fromViewDef === TermGroup_TimeSchedulePlanningViews.Day || fromViewDef === TermGroup_TimeSchedulePlanningViews.TemplateDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioDay || fromViewDef === TermGroup_TimeSchedulePlanningViews.StandbyDay);
        let toAnyDay = (toViewDef === TermGroup_TimeSchedulePlanningViews.Day || toViewDef === TermGroup_TimeSchedulePlanningViews.TemplateDay || toViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsDay || toViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioDay || toViewDef === TermGroup_TimeSchedulePlanningViews.StandbyDay);
        let fromAnyWeek = (fromViewDef === TermGroup_TimeSchedulePlanningViews.Schedule || fromViewDef === TermGroup_TimeSchedulePlanningViews.TemplateSchedule || fromViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule || fromViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || fromViewDef === TermGroup_TimeSchedulePlanningViews.StandbySchedule);
        let toAnyWeek = (toViewDef === TermGroup_TimeSchedulePlanningViews.Schedule || toViewDef === TermGroup_TimeSchedulePlanningViews.TemplateSchedule || toViewDef === TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule || toViewDef === TermGroup_TimeSchedulePlanningViews.ScenarioSchedule || toViewDef === TermGroup_TimeSchedulePlanningViews.StandbySchedule);

        this.loadSettingsForView().then(() => {
            this.userSelectionType = toViewDef + 100;

            this.grossNetAndCostLoaded = false;

            // Set tab label to current view definition
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: this.setViewLabel(toViewDef),
            });

            // If switching to employee posts view the first time, reset firstLoadHasOccurred,
            // otherwise both setDateRange and loadEmployeePosts will call loadData.
            if (toEmployeePost) {
                if (!this.employeePostsLoaded)
                    this.firstLoadHasOccurred = false;
            }

            // If switching from standby (DisplayMode User) change to Admin
            if (fromStandby && !toStandby)
                this.displayMode = TimeSchedulePlanningDisplayMode.Admin;
            else if (!fromStandby && toStandby)
                this.displayMode = TimeSchedulePlanningDisplayMode.User;

            // If switching between different kind of views (schedule/template etc.) clear data
            if ((fromSchedule && !toSchedule) || (fromTemplate && !toTemplate) || (fromEmployeePost && !toEmployeePost) || (fromScenario && !toScenario)) {
                this.clearShifts();
            }

            // If switching between same kind of views (day/schedule) keep dates
            let keepDates: boolean = ((fromAnyDay && toAnyDay) || (fromAnyWeek && toAnyWeek));
            if (this.selectedVisibleDays == TermGroup_TimeSchedulePlanningVisibleDays.Custom && toScenario)
                keepDates = true;
            if (!keepDates)
                this.setViewDate(toViewDef);

            // If switching to employee posts, load them (but only once)
            if (toEmployeePost) {
                if (!this.employeePostsLoaded)
                    this.loadEmployeePosts(true);

                if (!this.showEmployeeList && this.hasCurrentViewModifyPermission)
                    this.toggleShowEmployeeList(true);
            }

            // If switching from employee posts, to a schedule view, hide employee list if user has not checked it as default show
            if (fromEmployeePost && toViewDef !== TermGroup_TimeSchedulePlanningViews.Calendar) {
                if (!this.defaultShowEmployeeList && this.showEmployeeList)
                    this.toggleShowEmployeeList(true);
            }

            if (fromEmployeePost || toEmployeePost) {
                if (this.employeePostsLoaded)
                    this.setEmployeeData();
            }

            // If switching to scenario view, load scenario heads if not already loaded
            if (toScenario) {
                // Show all employees, regardless of previous view
                if (!fromScenario)
                    this.showAllEmployees = true;

                if (this.scenarioHeads.length === 0 && this.accountDims)
                    this.loadScenarioHeads();

                if (!fromScenario && !this.keepScenarioHeadId)
                    this.timeScheduleScenarioHeadId = 0;

                this.keepScenarioHeadId = false;
            }

            // If switching from scenario view, clear selected scenario and reload employees
            if (fromScenario && !toScenario) {
                this.scenarioHead = null;
                this.timeScheduleScenarioHeadId = null;
                this.loadEmployees().then(() => {
                    // Switching from scenario view when just activating a scenario
                    if (this.scenarioEmployeeIds.length > 0) {
                        // Filter on employees included in scenario
                        this.selectedEmployees = this.employees.filter(e => this.scenarioEmployeeIds.includes(e.id));
                        this.scenarioEmployeeIds = [];
                        this.fromScenarioEvaluate = true;
                    }
                });
            }

            if ((fromAnyDay && !toAnyDay) || (fromAnyWeek && !toAnyWeek)) {
                // If switching between day and week view, clear chart element
                // because it needs to be recreated in correct view
                this.scheduleHandler.clearPlanningAgChartElem();

                // Also sorting needs to be reset
                this.resetSort()
            }

            let loadDataInSetDateRange: boolean = true;

            if (toTasksAndDeliveries && this.allTasks.length === 0)
                this.loadData('viewDefinitionChanged to tasks');
            else if (toStaffingNeeds && this.allHeads.length === 0)
                this.loadData('viewDefinitionChanged to staffing needs');
            else if ((fromTasksAndDeliveries || fromStaffingNeeds) && !this.hasEmployeesLoaded && !this.loadingEmployees) {
                loadDataInSetDateRange = false;
                this.loadEmployees();
            }

            if (this.showPlanningAgChart) {
                this.showPlanningAgChart = false;
                this.planningAgChartData = [];
                this.renderPlanningAgChart(false);
            }

            this.setupStatusFilter(false);
            this.sortEmployees(false);
            this.scheduleHandler.clearScheduleViewBody().then(() => {
                let actuallyChanged = this.setDateRange(loadDataInSetDateRange);
                if (!actuallyChanged || !this.firstLoadHasOccurred) {
                    // setDateRange() will reload unscheduled tasks, but only if the dates actually changed.
                    // So if they don't changed (for example switching between schedule week view and template schedule week view) we need to relod here.
                    this.loadUnscheduledTasksAndDeliveriesDates();
                }
            });
        });
    }

    private setViewLabel(viewDef: TermGroup_TimeSchedulePlanningViews): string {
        let label: string;
        switch (viewDef) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                this.selectedViewLabel = this.isOrderPlanningMode ? this.terms["time.schedule.planning.orderplanning"] : this.terms["time.schedule.planning.viewdefinition.group.schedule"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.calendar"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.Day:
                this.selectedViewLabel = this.isOrderPlanningMode ? this.terms["time.schedule.planning.orderplanning"] : this.terms["time.schedule.planning.viewdefinition.group.schedule"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.day"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.Schedule:
                this.selectedViewLabel = this.isOrderPlanningMode ? this.terms["time.schedule.planning.orderplanning"] : this.terms["time.schedule.planning.viewdefinition.group.schedule"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.schedule"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.template"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.templateday"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.template"]
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.templateschedule"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
                this.selectedViewLabel = this.terms["time.schedule.staffingneeds.viewdefinition.employeeposts"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.day"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
                this.selectedViewLabel = this.terms["time.schedule.staffingneeds.viewdefinition.employeeposts"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.schedule"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.scenario"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.day"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.scenario"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.schedule"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.scenario"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.complete"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.standby"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.day"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
                this.selectedViewLabel = this.terms["time.schedule.planning.viewdefinition.group.standby"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.schedule"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
                this.selectedViewLabel = this.terms["time.schedule.staffingneeds.viewdefinition.tasksanddeliveries"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.day"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
                this.selectedViewLabel = this.terms["time.schedule.staffingneeds.viewdefinition.tasksanddeliveries"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.schedule"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                this.selectedViewLabel = this.terms["time.schedule.staffingneeds.viewdefinition.planning"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.day"].toLowerCase());
                break;
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                this.selectedViewLabel = this.terms["time.schedule.staffingneeds.viewdefinition.planning"];
                label = "{0} {1}".format(this.selectedViewLabel, this.terms["time.schedule.planning.viewdefinition.schedule"].toLowerCase());
                break;
        }

        return label;
    }

    private setViewDate(viewDef: TermGroup_TimeSchedulePlanningViews) {
        switch (viewDef) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                // Set start to first day of specified start week
                this.dateFrom = this.dateFrom.beginningOfDay().beginningOfWeek().addWeeks(this.startWeek);
                break;
            case TermGroup_TimeSchedulePlanningViews.Day:
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                // If today is within current date range, use today, otherwise use current date range start
                this.dateFrom = ((new Date().isWithinRange(this.dateFrom.beginningOfDay(), this.dateTo.endOfDay())) ? new Date() : this.dateFrom).beginningOfDay().addHours(this.startHour);
                break;
            case TermGroup_TimeSchedulePlanningViews.Schedule:
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                this.selectedVisibleDays = this.defaultInterval ? this.defaultInterval : TermGroup_TimeSchedulePlanningVisibleDays.Week;
                // Set start to first day of current week
                this.dateFrom = this.dateFrom.beginningOfWeek();
                break;
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
                this.selectedVisibleDays = this.scenarioHead ? CalendarUtility.getDaysBetweenDates(this.scenarioHead.dateFrom, this.scenarioHead.dateTo) : TermGroup_TimeSchedulePlanningVisibleDays.Week;
                // Set start to first day of current week
                this.dateFrom = this.scenarioHead ? this.scenarioHead.dateFrom : this.dateFrom.beginningOfWeek();
                break;
        }
    }

    private decreaseDate() {
        let dayRange = this.nbrOfVisibleDays;

        this.dateFrom = this.isCommonDayView ? this.dateFrom.addDays(-1) : this.dateFrom.addDays(-dayRange);
        if (this.isCommonScheduleView && this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Custom)
            this.dateTo = this.dateFrom.addDays(dayRange - 1).endOfDay();
    }

    private increaseDate() {
        let extraDays: number = 0;
        let dayRange = this.nbrOfVisibleDays;

        if (this.isCommonScheduleView) {
            if (this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.WorkWeek)
                extraDays = (TermGroup_TimeSchedulePlanningVisibleDays.Week - TermGroup_TimeSchedulePlanningVisibleDays.WorkWeek);
        }

        this.dateFrom = this.isCommonDayView ? this.dateFrom.addDays(1) : this.dateTo.beginningOfDay().addDays(1 + extraDays);
        if (this.isCommonScheduleView && this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Custom)
            this.dateTo = this.dateFrom.addDays(dayRange - 1).endOfDay();
    }

    private visibleDaysChanged(item) {
    }

    private scenarioHeadChanged(item) {
        this.$timeout(() => {
            this.loadScenarioHead();
        });
    }

    private openEditScenarioHead(scenarioHead: TimeScheduleScenarioHeadDTO) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateScenarioHead/Views/createScenarioHead.html"),
            controller: CreateScenarioHeadController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                viewDefinition: () => { return this.viewDefinition },
                useVacant: () => { return this.useVacant },
                useAccountHierarchy: () => { return this.useAccountHierarchy },
                accountHierarchyId: () => { return this.accountHierarchyId },
                accountDim: () => { return this.accountDims.find(a => a.accountDimId === this.defaultEmployeeAccountDimId); },
                filteredAccountIds: () => { return this.getFilteredAccountIds(); },
                filteredCategoryIds: () => { return this.getFilteredCategoryIds(); },
                sourceEmployees: () => { return this.visibleEmployees; },
                includeSecondaryCategoriesOrAccounts: () => { return this.useAccountHierarchy ? this.showSecondaryAccounts : this.showSecondaryCategories },
                currentScenarioHeadId: () => { return this.timeScheduleScenarioHeadId },
                scenarioHead: () => { return scenarioHead },
                scenarioName: () => { return this.scenarioHead ? this.scenarioHead.name : null },
                scenarioDateFrom: () => { return this.scenarioHead ? this.scenarioHead.dateFrom.beginningOfDay() : null },
                scenarioDateTo: () => { return this.scenarioHead ? this.scenarioHead.dateTo.endOfDay() : null },
                dateFrom: () => { return this.dateFrom.beginningOfDay() },
                dateTo: () => { return this.dateTo.endOfDay() }
            }
        }

        this.$uibModal.open(options).result.then(result => {
            if (result?.scenarioHead && result?.employees) {
                if (!scenarioHead)
                    scenarioHead = new TimeScheduleScenarioHeadDTO();
                angular.extend(scenarioHead, result.scenarioHead);
                scenarioHead.employees = [];
                scenarioHead.employees = result.employees;
                const includeAbsence = result.includeAbsence;
                const dateFunction = result.dateFunction;

                if (this.useAccountHierarchy) {
                    scenarioHead.accounts = [];
                    let accountIds: number[] = result.accountIds;
                    accountIds.forEach(accountId => {
                        let acc: TimeScheduleScenarioAccountDTO = new TimeScheduleScenarioAccountDTO();
                        acc.accountId = accountId;
                        scenarioHead.accounts.push(acc);
                    });
                }

                this.startSave();

                this.scheduleService.saveTimeScheduleScenarioHead(scenarioHead, this.timeScheduleScenarioHeadId, includeAbsence, dateFunction).then(saveResult => {
                    if (saveResult.success) {
                        this.timeScheduleScenarioHeadId = saveResult.integerValue;
                        this.keepScenarioHeadId = true;
                        if (!this.isScenarioView) {
                            // Switching to scenario view will load scenario heads if not loaded
                            this.scenarioHeads = [];
                            this.viewDefinition = this.isCommonDayView ? TermGroup_TimeSchedulePlanningViews.ScenarioDay : TermGroup_TimeSchedulePlanningViews.ScenarioSchedule;
                        } else {
                            this.loadScenarioHeads();
                        }
                        this.completedWork(null, true);
                    } else {
                        this.failedSave(saveResult.errorMessage);
                    }
                });
            }
        }, (reason) => {
            // User cancelled dialog
        });
    }

    private deleteScenarioHead() {
        if (this.scenarioHead) {
            this.translationService.translate('time.schedule.planning.scenario.delete.warning').then(term => {
                this.notificationService.showDialogEx(this.terms["time.schedule.planning.scenario.delete"] + ' ({0})'.format(this.scenarioHead.name), term + '\n\n' + this.scenarioHead.name, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                    if (val) {
                        this.startDelete();
                        this.scheduleService.deleteScenarioHead(this.scenarioHead.timeScheduleScenarioHeadId).then(result => {
                            if (result.success) {
                                this.timeScheduleScenarioHeadId = 0;
                                this.scenarioHead = null;
                                this.clearShifts();
                                this.loadScenarioHeads();
                                this.reloadShiftsForSpecifiedEmployeeIds = [];
                                this.loadEmployees().then(() => {
                                    this.completedDelete(null, true);
                                    this.loadData('deleteScenarioHead');
                                });
                            } else {
                                this.failedDelete(result.errorMessage);
                            }
                        });
                    }
                });
            });
        }
    }

    private openActivateScenario() {
        if (this.existingScenarioSelected) {
            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/ActivateScenario/Views/activateScenario.html"),
                controller: ActivateScenarioController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                resolve: {
                    scenarioHead: () => { return this.scenarioHead },
                }
            }

            this.$uibModal.open(options).result.then(result => {
                if (result && result.success) {
                    // Load employees included in scenario
                    this.scheduleService.getScenarioEmployeeIds(this.timeScheduleScenarioHeadId).then(x => {
                        this.scenarioEmployeeIds = x;
                        // Switch to schedule view and select included employees
                        this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Schedule;
                    });
                }
            });
        }
    }

    private openCreateTemplateFromScenario() {
        if (this.existingScenarioSelected) {
            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateTemplateFromScenario/Views/createTemplateFromScenario.html"),
                controller: CreateTemplateFromScenarioController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                resolve: {
                    scenarioHead: () => { return this.scenarioHead },
                    useStopDate: () => { return this.useTemplateScheduleStopDate },
                    templateScheduleEditHiddenPermission: () => { return this.templateScheduleEditHiddenPermission },
                    hiddenEmployeeId: () => { return this.hiddenEmployeeId }
                }
            }

            this.$uibModal.open(options).result.then(result => {
                if (result && result.success) {
                    // Load employees included in scenario
                    //    this.scheduleService.getScenarioEmployeeIds(this.timeScheduleScenarioHeadId).then(x => {
                    //        this.scenarioEmployeeIds = x;
                    //        // Switch to schedule view and select included employees
                    //        this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Schedule;
                    //    });
                }
            });
        }
    }

    private calendarWeekSelected(week) {
        // Get first date of selected week
        for (let date of this.dates) {
            if (week === date.date.week()) {
                this.dateFrom = date.date;
                break;
            }
        }
        this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Schedule;
    }

    private calendarDaySelected(date) {
        this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Day;
        this.dateFrom = date;
    }

    private scheduleDaySelected(date) {
        if (this.isScheduleView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.Day;
        else if (this.isTemplateScheduleView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.TemplateDay;
        else if (this.isEmployeePostScheduleView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.EmployeePostsDay;
        else if (this.isScenarioScheduleView || this.isScenarioCompleteView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.ScenarioDay;
        else if (this.isStandbyScheduleView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.StandbyDay;
        else if (this.isTasksAndDeliveriesScheduleView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay;
        else if (this.isStaffingNeedsScheduleView)
            this.viewDefinition = TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay;

        this.dateFrom = date;
    }

    private toggleShowEmployeeList(updateGui: boolean) {
        // No employee list in standby view
        if (!this.showEmployeeList && this.isStandbyView)
            return;

        this.showEmployeeList = !this.showEmployeeList;

        if (this.showEmployeeList) {
            // If unscheduled tasks is visible, hide it
            if (this.showUnscheduledTasks)
                this.toggleShowUnscheduledTasks(false);

            // If work rule violations is visible, hide it
            if (this.showWorkRuleViolations)
                this.toggleShowWorkRuleViolations(false);

            // If dashboard is visible, hide it
            if (this.showDashboard)
                this.toggleShowDashboard(false);
        }

        if (updateGui) {
            this.setDateColumnWidth().then(() => {
                this.scheduleHandler.updateWidthOnAllElements();
            });
        }

        if (this.showEmployeeList) {
            if (!this.showEmployeeListFilters)
                this.showEmployeeListFilters = true;

            // If employee list is shown before employees (shifts) are loaded, work time is not calculated.
            // Happens for example in EmployeePost view or if list is shown with disabled initial load.
            // So if no employees have work time, calculate for all of them.
            if (this.employedEmployees.filter(e => e.oneWeekWorkTimeMinutes > 0).length === 0) {
                this.employedEmployees.forEach(employee => {
                    this.calculateEmployeeWorkTimes(employee);
                });
            }

            this.filterEmployeeList(null);
        }
    }

    private toggleShowWorkRuleViolations(updateGui: boolean) {
        this.showWorkRuleViolations = !this.showWorkRuleViolations;

        if (this.showWorkRuleViolations) {
            // If employee list is visible, hide it
            if (this.showEmployeeList)
                this.toggleShowEmployeeList(false);

            // If unscheduled tasks is visible, hide it
            if (this.showUnscheduledTasks)
                this.toggleShowUnscheduledTasks(false);

            // If dashboard is visible, hide it
            if (this.showDashboard)
                this.toggleShowDashboard(false);
        }

        if (updateGui) {
            this.setDateColumnWidth().then(() => {
                this.scheduleHandler.updateWidthOnAllElements();
            });
        }
    }

    private toggleShowUnscheduledTasks(updateGui: boolean) {
        this.showUnscheduledTasks = !this.showUnscheduledTasks;
        if (this.showUnscheduledTasks) {
            // If employee list is visible, hide it
            if (this.showEmployeeList)
                this.toggleShowEmployeeList(false);

            // If work rule violations is visible, hide it
            if (this.showWorkRuleViolations)
                this.toggleShowWorkRuleViolations(false);

            // If dashboard is visible, hide it
            if (this.showDashboard)
                this.toggleShowDashboard(false);
        }

        if (this.showUnscheduledTasks && (!this.unscheduledTasks || this.unscheduledTasks.length === 0))
            this.loadUnscheduledTasksAndDeliveries();

        if (updateGui) {
            this.setDateColumnWidth().then(() => {
                this.scheduleHandler.updateWidthOnAllElements();
            });
        }
    }

    private doShowUnscheduledTasks(date: Date) {
        // Close all date expanders
        this.unscheduledTaskDates.forEach(td => {
            td['isOpen'] = false;
        });

        // Open expander for clicked date
        let taskDate = this.unscheduledTaskDates.find(d => d.isSameDayAs(date));
        if (taskDate)
            taskDate['isOpen'] = true;

        if (!this.showUnscheduledTasks)
            this.toggleShowUnscheduledTasks(true);
    }

    private doShowScheduleEvents(date: Date) {
        this.loadScheduleEvents(date).then((scheduleEvents: TimeScheduleEventForPlanningDTO[]) => {
            this.showScheduleEvents(date, scheduleEvents);
        });
    }

    private toggleShowDashboard(updateGui: boolean) {
        this.setupDashboard().then(() => {
            this.showDashboard = !this.showDashboard;
            if (this.showDashboard) {
                // If employee list is visible, hide it
                if (this.showEmployeeList)
                    this.toggleShowEmployeeList(false);

                // If unscheduled tasks is visible, hide it
                if (this.showUnscheduledTasks)
                    this.toggleShowUnscheduledTasks(false);

                // If work rule violations is visible, hide it
                if (this.showWorkRuleViolations)
                    this.toggleShowWorkRuleViolations(false);
            }

            if (updateGui) {
                this.setDateColumnWidth().then(() => {
                    this.scheduleHandler.updateWidthOnAllElements();
                });
            }
        });

        if (this.useAccountHierarchy && (!this.selectableInformationSettings.followUpAccountId || this.selectableInformationSettings.followUpAccountId !== this.userAccountId)) {
            this.showSelectableInformation(true, true);
        }
    }

    private onCategoryFiltered(items) {
        this.filterEmployees('onCategoryFiltered');
    }

    private onShowSecondaryCategoriesChanged() {
        this.$timeout(() => {
            this.loadCategories(true);
        });
    }

    private onEmployeeFiltered() {
        if (this.selectableInformationSettings.doNotSearchOnFilter)
            this.filteredButNotLoaded = true;
        else
            this.loadData('onEmployeeFiltered', true);
    }

    private onFreeTextFiltered = _.debounce(() => {
        this.filter();
    }, 250, { leading: false, trailing: true });

    private createUserSelections() {
        this.userSelections = new SelectionCollection();

        this.accountDims.forEach(dim => {
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_ACCOUNT_DIM + dim.accountDimId, new IdListSelectionDTO(this.isFilteredOnAccountDim() ? this.getFilteredAccountDimAccountIds(dim) : []));
        });

        this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_SHIFT_TYPES, new IdListSelectionDTO(this.isFilteredOnShiftType ? this.getFilteredShiftTypeIds() : []));

        if (this.isTasksAndDeliveriesView) {
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_TASK_TYPES, new IdListSelectionDTO(this.isFilteredOnTaskType ? this.getFilteredTaskTypeIds() : []));
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_TIME_SCHEDULE_TASK_TYPES, new IdListSelectionDTO(this.isFilteredOnTimeScheduleTaskType ? this.getFilteredTimeScheduleTaskTypeIds() : []));
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_TASKS, new IdListSelectionDTO(this.isFilteredOnTask ? this.getSelectedTaskIds() : []));
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_DELIVERIES, new IdListSelectionDTO(this.isFilteredOnDelivery ? this.getSelectedDeliveryIds() : []));
        }

        if (!this.isTasksAndDeliveriesView && !this.isStaffingNeedsView)
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_EMPLOYEE_GROUPS, new IdListSelectionDTO(this.isFilteredOnEmployeeGroup ? this.getFilteredEmployeeGroupIds() : []));

        if (!this.useAccountHierarchy && !this.isEmployeePostView && !this.isTasksAndDeliveriesView && !this.isStaffingNeedsView) {
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_CATEGORIES, new IdListSelectionDTO(this.isFilteredOnCategory ? this.getFilteredCategoryIds() : []));
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_SHOW_SECONDARY_CATEGORIES, new BoolSelectionDTO(this.showSecondaryCategories));
        }

        if (this.isEmployeePostView)
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_EMPLOYEE_POSTS, new IdListSelectionDTO(this.isFilteredOnEmployee ? this.getFilteredEmployeePostIds() : []));
        else if (!this.isTasksAndDeliveriesView && !this.isStaffingNeedsView)
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_EMPLOYEES, new IdListSelectionDTO(this.isFilteredOnEmployee ? this.getFilteredEmployeeIds() : []));

        if (this.useAccountHierarchy && !this.isTasksAndDeliveriesView && !this.isStaffingNeedsView)
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_SHOW_SECONDARY_ACCOUNTS, new BoolSelectionDTO(this.showSecondaryAccounts));

        if (!this.isTemplateView && !this.isTasksAndDeliveriesView && !this.isStaffingNeedsView) {
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_STATUSES, new IdListSelectionDTO(this.isFilteredOnStatus ? this.getFilteredStatusIds() : []));
            this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_DEVIATION_CAUSES, new IdListSelectionDTO(this.isFilteredOnDeviationCause ? this.getFilteredDeviationCauseIds() : []));
        }

        this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_BLOCK_TYPES, new IdListSelectionDTO(this.isFilteredOnBlockType ? this.getFilteredBlockTypes() : []));

        this.userSelections.upsert(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_FREE_TEXT, new TextSelectionDTO(this.freeTextFilter));
    }

    private userSelectionsLoaded(selections: SmallGenericType[]) {
        if (this.selectableInformationSettings.defaultUserSelectionId && selections.find(s => s.id === this.selectableInformationSettings.defaultUserSelectionId))
            this.selectedUserSelectionId = this.selectableInformationSettings.defaultUserSelectionId;
    }

    private userSelectionChanged(userSelection: UserSelectionDTO) {
        this.selectedUserSelection = userSelection;
        this.filterByUserSelection();
    }

    private staffingNeedsSelectionChanged(item) {
        this.$timeout(() => {
            this.filterStaffingNeedsHeadsToDisplay();
        });
    }

    private onEmployeeListFreeTextFiltered = _.debounce(() => {
        this.filterEmployeeListFromGUI();
    }, 250, { leading: false, trailing: true });

    private filterEmployeeListFromGUI() {
        this.$timeout(() => {
            this.filterEmployeeList(null);
        });
    }

    private onOrderListFreeTextFiltered = _.debounce(() => {
        this.filterOrderListFromGui();
    }, 250, { leading: false, trailing: true });

    private onShowFutureOrdersChanged() {
        this.$timeout(() => {
            this.loadAllUnscheduledOrders();
        });
    }

    private setFirstLoadHasOccurred() {
        if (this.firstLoadHasOccurred)
            return;

        this.firstLoadHasOccurred = true;
        if (this.disableAutoLoad && this.showFilters)
            this.toggleFilters();
    }

    private render(stopProgressWhenDone: boolean = true) {
        if (!this.terms)
            return;

        if (!this.loadShiftsSilent) {
            this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.render"];
            this.progressBusy = true;
        }

        if (this.reloadShiftsForSpecifiedEmployeeIds.length > 0) {
            let employeeNotRendered = false;
            for (let employeeId of this.reloadShiftsForSpecifiedEmployeeIds) {
                let employee = this.isEmployeePostView ? this.getEmployeePostById(employeeId) : this.getEmployeeById(employeeId);
                this.calculateEmployeeWorkTimes(employee);
                let rows = this.scheduleHandler.getEmployeeRows(employeeId);
                if (rows.length > 0) {
                    rows.forEach(row => {
                        this.scheduleHandler.updateEmployeeRow(row, employee);
                        this.scheduleHandler.updateEmployeeInfo(employee);
                    });
                } else {
                    employeeNotRendered = true;
                    break;
                }
            }
            if (employeeNotRendered) {
                // If an employee was not previously rendered, it can not be replaced.
                // In that case render all employees
                this.renderBody('render on reload', stopProgressWhenDone);
            } else {
                this.scheduleHandler.renderScheduleSummary();
                this.renderingDone(stopProgressWhenDone);
            }
        } else {
            this.renderBody('render', stopProgressWhenDone);
        }
    }

    public renderingDone(stopProgress: boolean = true) {
        if (stopProgress && !this.loadShiftsSilent && (this.firstLoadHasOccurred || this.disableAutoLoad || (this.isScenarioView && !this.timeScheduleScenarioHeadId)) && !this.loadingPlanningPeriodSummary)
            this.completedWork(null, true);

        this.loadShiftsSilent = false;

        if (this.loadPlanningPeriodSummary && this.isSchedulePlanningMode && (this.isScheduleView || this.isTemplateScheduleView) && this.visibleEmployees.length > 0) {
            if (this.calculatePlanningPeriodScheduledTimeUseAveragingPeriod)
                this.loadEmployeePeriodTimeSummary(this.reloadShiftsForSpecifiedEmployeeIds.length > 0 ? this.reloadShiftsForSpecifiedEmployeeIds : null)
            else
                this.loadAnnualScheduledTimeSummary(this.reloadShiftsForSpecifiedEmployeeIds.length > 0 ? this.reloadShiftsForSpecifiedEmployeeIds : null);
        }

        if (this.loadAnnualLeaveBalance && this.isSchedulePlanningMode && (this.isScheduleView || this.isDayView) && this.visibleEmployees.length > 0) {
            this.loadAnnualLeaveBalanceForEmployees(this.reloadShiftsForSpecifiedEmployeeIds.length > 0 ? this.reloadShiftsForSpecifiedEmployeeIds : null);
        }

        this.loadingShifts = false;
        this.nbrOfColumnsChanged = false;

        if (this.delayLoadStaffingNeed)
            this.loadStaffingNeed();

        this.filterEmployeeList(null);
        this.nbrOfVisibleEmployees = this.visibleEmployees.filter(e => !e.isGroupHeader).length;

        this.$timeout(() => {
            // Set tooltips for reloaded employees shifts, or all visible if no specific employees has been loaded
            let shifts: ShiftDTO[] = [];
            if (this.reloadShiftsForSpecifiedEmployeeIds.length > 0) {
                this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeIdentifier => {
                    let empShifts = this.shifts.filter(s => (this.isEmployeePostView ? s.employeePostId === employeeIdentifier : s.employeeId === employeeIdentifier));
                    shifts = shifts.concat(empShifts);
                });
            } else {
                shifts = this.visibleShifts;
            }

            this.clearShiftToolTips(shifts.filter(s => s.isSchedule || s.isStandby || s.isOnDuty));
            if (this.bookingModifyPermission)
                this.clearShiftToolTips(shifts.filter(s => s.isBooking));
            if (this.isOrderPlanningMode)
                this.clearShiftToolTips(shifts.filter(s => s.isOrder));

            if (this.fromScenarioEvaluate) { // Evaluat work rules
                this.evaluateAllWorkRules();
                this.fromScenarioEvaluate = false;
            }
        });
    }

    public shiftSelected() {
        this.filterEmployeeList(null);
        this.getAvailableEmployees();
    }

    public employeePostSelected(employeePostId: number) {
        this.employeeListFilterEmployeePostId = employeePostId;
        this.$scope.$applyAsync();
        this.filterEmployeeList(null);
    }

    // BUTTON FUNCTIONS

    private executeFunction(option) {
        if (this.isTasksAndDeliveriesView || this.isStaffingNeedsView) {
            switch (option.id) {
                case StaffingNeedsFunctions.PrintTasksAndDeliveries:
                    this.printTasksAndDeliveries();
                    break;
                case StaffingNeedsFunctions.AddNeed:
                    this.showOpenNeed();
                    break;
                case StaffingNeedsFunctions.ReloadNeed:
                    this.reloadNeed();
                    break;
            }
        } else {
            switch (option.id) {
                case SchedulePlanningFunctions.AddShift:
                    let { employeeId: employeeId, date: date } = this.scheduleHandler.getSlotInfo();
                    if (!date)
                        date = this.dateFrom;
                    if (this.isSchedulePlanningMode)
                        this.openEditShift(null, null, date, employeeId, false, false);
                    else if (this.isOrderPlanningMode)
                        this.openEditAssignment(null, date, employeeId);
                    break;
                case SchedulePlanningFunctions.EditBreaks:
                    this.editMode = PlanningEditModes.Breaks;
                    this.renderBody('executeFunction edit breaks');
                    break;
                case SchedulePlanningFunctions.EditTemplateBreaks:
                    this.openCreateTemplateBreaks();
                    break;
                case SchedulePlanningFunctions.EditShifts:
                    this.editMode = PlanningEditModes.Shifts;
                    this.renderBody('executeFunction edit shifts');
                    break;
                case SchedulePlanningFunctions.ShowEmployeeList:
                case SchedulePlanningFunctions.HideEmployeeList:
                    this.toggleShowEmployeeList(true);
                    break;
                case SchedulePlanningFunctions.ShowDashboard:
                case SchedulePlanningFunctions.HideDashboard:
                    this.toggleShowDashboard(true);
                    break;
                case SchedulePlanningFunctions.PrelToDef:
                    this.openDefToFromPrelShift(true);
                    break;
                case SchedulePlanningFunctions.DefToPrel:
                    this.openDefToFromPrelShift(false);
                    break;
                case SchedulePlanningFunctions.AllocateLeisureCodes:
                    this.openAllocateLeisureCodes(false);
                    break;
                case SchedulePlanningFunctions.DeleteLeisureCodes:
                    this.openAllocateLeisureCodes(true);
                    break;
                case SchedulePlanningFunctions.RecalculateAnnualLeaveBalances:
                    this.recalculateAnnualLeaveBalanceForEmployees(null, true, false);
                    break;
                case SchedulePlanningFunctions.CopySchedule:
                    this.openCopySchedule(null);
                    break;
                case SchedulePlanningFunctions.RestoreToSchedule:
                    this.openTimeCalculationDialog(SoeTimeAttestFunctionOption.RestoreToSchedule);
                    break;
                case SchedulePlanningFunctions.RemoveAbsenceInScenario:
                    this.openTimeCalculationDialog(SoeTimeAttestFunctionOption.ScenarioRemoveAbsence);
                    break;
                case SchedulePlanningFunctions.PrintSchedule:
                    this.printScheduleForEmployees(this.getVisibleEmployeeIds(), [SoeReportTemplateType.TimeEmployeeSchedule, SoeReportTemplateType.TimeEmployeeLineSchedule, SoeReportTemplateType.TimeEmployeeScheduleSmallReport]);
                    break;
                case SchedulePlanningFunctions.PrintTemplateSchedule:
                    this.printScheduleForEmployees(this.getVisibleEmployeeIds(), [SoeReportTemplateType.TimeEmployeeTemplateSchedule]);
                    break;
                case SchedulePlanningFunctions.PrintEmployeePostTemplateSchedule:
                    this.printScheduleForEmployees(this.getVisibleEmployeePostIds(), [SoeReportTemplateType.TimeEmployeeTemplateSchedule]);
                    break;
                case SchedulePlanningFunctions.PrintScenarioSchedule:
                    this.printScheduleForEmployees(this.getVisibleEmployeeIds(), [SoeReportTemplateType.TimeEmployeeSchedule, SoeReportTemplateType.TimeEmployeeLineSchedule, SoeReportTemplateType.TimeEmployeeScheduleSmallReport]);
                    break;
                case SchedulePlanningFunctions.PrintEmploymentCertificate:
                    this.printEmploymentCertificateForEmployees(this.visibleEmployees.filter(e => !e.isGroupHeader));
                    break;
                case SchedulePlanningFunctions.SendEmploymentCertificate:
                    this.sendEmploymentCertificateForEmployees(this.visibleEmployees.filter(e => !e.isGroupHeader));
                    break;
                case SchedulePlanningFunctions.ExportToExcel:
                    this.exportScheduleToExcel();
                    break;
                case SchedulePlanningFunctions.EvaluateAllWorkRules:
                    this.evaluateAllWorkRules();
                    break;
                case SchedulePlanningFunctions.ShowUnscheduledTasks:
                case SchedulePlanningFunctions.HideUnscheduledTasks:
                    this.toggleShowUnscheduledTasks(true);
                    break;
                case SchedulePlanningFunctions.CreateEmptyScheduleForEmployeePosts:
                    this.createEmptyScheduleForEmployeePosts();
                    break;
                case SchedulePlanningFunctions.RegenerateScheduleForEmployeePost:
                    this.generateScheduleForEmployeePosts();
                    break;
                case SchedulePlanningFunctions.DeleteScheduleForEmployeePost:
                    this.deleteScheduleForEmployeePosts();
                    break;
                case SchedulePlanningFunctions.AddOrder:
                    this.openOrder(null);
                    break;
                case SchedulePlanningFunctions.NewTemplates:
                    this.openCreateTemplates();
                    break;
                case SchedulePlanningFunctions.OpenActivateSchedule:
                    this.openActivateSchedule();
                    break;
                case SchedulePlanningFunctions.AddScenario:
                    this.openEditScenarioHead(null);
                    break;
                case SchedulePlanningFunctions.DeleteScenario:
                    this.deleteScenarioHead();
                    break;
                case SchedulePlanningFunctions.ActivateScenario:
                    this.openActivateScenario();
                    break;
                case SchedulePlanningFunctions.CreateTemplateFromScenario:
                    this.openCreateTemplateFromScenario();
                    break;
            }
        }
    }

    // CONTEXT MENU FUNCTIONS

    public editEmployee(employee: EmployeeListDTO) {
        if (!employee)
            return;

        if (this.permittedEmployeeIds.length === 0) {
            this.loadPermittedEmployeeIds().then(() => {
                if (this.permittedEmployeeIds.length > 0)
                    this.editEmployee(employee);
                else
                    this.showNotPermittedToEditEmployee();
            });
            return;
        } else {
            if (!this.permittedEmployeeIds.includes(employee.employeeId)) {
                this.showNotPermittedToEditEmployee();
                return;
            }
        }

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Views/edit.html"),
            controller: EmployeeEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: employee.employeeId,
            });
        });

        modal.result.then(result => {
            if (result.modified) {
                this.reloadEmployees([employee.employeeId], false);
                if (this.scheduleHandler.showingContactInfo(employee.employeeId))
                    this.showContactInfo(employee.employeeId);
            }
        });
    }

    private showNotPermittedToEditEmployee() {
        let keys: string[] = [
            "time.schedule.planning.notpermitted",
            "time.schedule.planning.notpermitted.editemployee"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.schedule.planning.notpermitted"], terms["time.schedule.planning.notpermitted.editemployee"], SOEMessageBoxImage.Forbidden);
        });
    }

    private showEmployeeRemovedFromFilterMessage() {
        let keys = ["time.schedule.planning.employeeremovedfromfilter.title", "time.schedule.planning.employeeremovedfromfilter.message"];
        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.schedule.planning.employeeremovedfromfilter.title"], terms["time.schedule.planning.employeeremovedfromfilter.message"], SOEMessageBoxImage.Information);
        });
        this.clearShifts();
        this.render(true);
    }

    private showContactInfo(employeeId: number) {
        this.scheduleService.getEmployeeContactInfo(employeeId).then(x => {
            this.scheduleHandler.setContactInfo(employeeId, x);
        });
    }

    public editEmployeePost(employeePost: EmployeeListDTO) {
        if (!employeePost)
            return;

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/EmployeePosts/Views/edit.html"),
            controller: EmployeePostEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: employeePost.employeePostId,
            });
        });

        modal.result.then(result => {
            if (result.deleted) {
                _.pull(this.allEmployees, employeePost);
                this.render();
            } else {
                this.reloadEmployeePosts([employeePost.employeePostId], true);
            }
        });
    }

    public editShift(shift: ShiftDTO, date: Date, employeeId: number, isStandby: boolean, isOnDuty: boolean) {
        if (this.editMode === PlanningEditModes.Breaks || this.editMode === PlanningEditModes.TemplateBreaks)
            return;

        if (employeeId === this.hiddenEmployeeId && this.isHiddenEmployeeReadOnly)
            return;

        if (this.isScenarioView) {
            if (!date && shift)
                date = shift.startTime;
            if (!this.isInsideScenario(date))
                return;
        }

        if (this.isEmployeeInactive(employeeId))
            return;

        if (shift && shift.isAbsenceRequest === true)
            this.openAbsenceRequestDialog(shift);
        else
            this.openEditShift(shift, null, date, employeeId, isStandby, isOnDuty);
    }

    private deleteShiftById = _.debounce((shiftId: number) => {
        let shift = this.scheduleHandler.getShiftById(shiftId);
        this.scheduleHandler.selectShift(shift);
        this.deleteShifts(this.scheduleHandler.getSelectedShifts());
    }, 500, { leading: true, trailing: false });

    private deleteShift(shift: ShiftDTO) {
        this.deleteShifts([shift]);
    }

    private deleteShifts(shifts: ShiftDTO[]) {
        this.validateWorkRulesOnDelete(shifts).then(passed => {
            if (passed) {
                this.openDeleteShift(shifts);
            }
        });
    }

    public editAssignment(shift: ShiftDTO, date: Date = null, employeeId: number = null) {
        this.openEditAssignment(shift, date, employeeId);
    }

    public editBooking(shift: ShiftDTO, date: Date = null, employeeId: number = null) {
        this.openEditBooking(shift, date, employeeId);
    }

    public editLeisureCode(shift: ShiftDTO, date: Date = null, employeeId: number = null) {
        if (this.editMode === PlanningEditModes.Breaks || this.editMode === PlanningEditModes.TemplateBreaks)
            return;

        if (employeeId === this.hiddenEmployeeId)
            return;

        if (this.isEmployeeInactive(employeeId))
            return;

        this.openEditLeisureCode(shift, date, employeeId);
    }

    private deleteLeisureCode(shift: ShiftDTO) {
        this.deleteLeisureCodes([shift]);
    }

    private deleteLeisureCodes(shifts: ShiftDTO[]) {
        this.openDeleteLeisureCode(shifts);
    }

    public editTask(task: StaffingNeedsTaskDTO) {
        if (task.type === SoeStaffingNeedsTaskType.Delivery)
            this.openEditDelivery(task, null);
        else if (task.type === SoeStaffingNeedsTaskType.Task)
            this.openEditTask(task, null);
    }

    private deleteTaskById = _.debounce((taskId: string) => {
        // TODO: Ugly work around for ng-click fireing twice when clicking on delete icon on task
        this.$timeout(() => {
            let task = this.scheduleHandler.getTaskById(taskId);
            this.scheduleHandler.selectTask(task);
            this.deleteTask(task);
        });
    }, 500, { leading: true, trailing: false });

    private deleteDelivery(task: StaffingNeedsTaskDTO) {
        this.openDeleteDelivery(task);
    }

    private deleteTask(task: StaffingNeedsTaskDTO) {
        this.openDeleteTask(task);
    }

    private editAvailability(dateFrom: Date, dateTo: Date, employeeId: number) {
        let employee = this.getEmployeeById(employeeId);

        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/EditEmployeeAvailability/EditEmployeeAvailabilityDialog.html"),
            controller: EditEmployeeAvailabilityDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                readOnly: () => { return !this.editAvailabilityPermission },
                employeeId: () => { return employeeId },
                dateFrom: () => { return dateFrom },
                dateTo: () => { return dateTo },
                date: () => { return dateFrom.isSameDayAs(dateTo) ? dateFrom : null },
                employeeInfo: () => { return employee },
                commentMandatory: () => { return true }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.succeess)
                this.reloadEmployees([employeeId], true);
        });
    }

    private changeEmployee() {
        let shifts = this.scheduleHandler.getSelectedShifts();
        if (shifts.length === 0)
            return;

        this.openDragShift(shifts, null, shifts[0].actualStartDate, null, 0, DragShiftAction.Move, true);
    }

    private cutOrCopyShifts() {
        this.cutCopiedShifts = this.scheduleHandler.getSelectedShifts();
        this.cutCopiedShifts.forEach(shift => {
            shift['selectedForPaste'] = true;
        });
    }

    private pasteShift(targetEmployeeId: number, targetDate: Date) {
        if (!targetEmployeeId || !targetDate)
            return;

        // Get shifts to paste
        let shifts = this.getSelectedCutOrCopiedShifts();
        if (shifts.length === 0)
            return;

        let targetEmployee = this.getEmployeeById(targetEmployeeId);
        if (!targetEmployee)
            return;

        let moveOffsetDays: number = targetDate.diffDays(shifts[0].actualStartDate);

        if (this.isTemplateView) {
            let targetTemplate = this.templateHelper.getTemplateSchedule(targetEmployeeId, targetDate);
            if (!targetTemplate)
                return;

            let lastTargetDate: Date = _.orderBy(shifts, s => s.actualDateOnLoad, 'desc')[0].actualDateOnLoad.addDays(moveOffsetDays);
            if (targetTemplate.stopDate && lastTargetDate.isAfterOnDay(targetTemplate.stopDate)) {
                let keys = [
                    "time.schedule.planning.unabletopasteshifts",
                    "time.schedule.planning.unabletopasteshifts.multipletargettemplates"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.unabletopasteshifts"], terms["time.schedule.planning.unabletopasteshifts.multipletargettemplates"], SOEMessageBoxImage.Forbidden);
                });
                return;
            }
        }

        let action: DragShiftAction = this.isCut ? DragShiftAction.Move : DragShiftAction.Copy;
        this.openDragShift(shifts, targetEmployee, targetDate, null, moveOffsetDays, action);
    }

    private showClipboard() {
        if (!this.hasCutOrCopiedShifts)
            return;

        let employee = this.getEmployeeById(this.cutCopiedShifts[0].employeeId);

        // Show clipboard dialog
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/Clipboard/Views/clipboard.html"),
            controller: ClipboardController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                employeeName: () => { return employee.name },
                shifts: () => { return this.cutCopiedShifts },
                isCut: () => { return this.isCut }
            }
        }
        this.$uibModal.open(options);
    }

    private splitShift(shift: ShiftDTO) {
        this.openSplitShift(shift);
    }

    private linkShifts(shifts: ShiftDTO[]) {
        if (shifts.length === 0)
            return;

        let employeeIdentifier: number = this.isEmployeePostView ? shifts[0].employeePostId : shifts[0].employeeId;
        let isHidden: boolean = (employeeIdentifier === this.hiddenEmployeeId);
        // Collect all breaks
        let breaks: ShiftBreakDTO[] = [];
        if (isHidden) {
            shifts.forEach(shift => {
                breaks = _.concat(breaks, shift.shiftToBreaks());
            });
        }
        let shiftsOverlapping: boolean = false;
        let link: string = Guid.newGuid();
        let prevShift: ShiftDTO = null;
        for (let shift of _.orderBy(shifts, ['actualStartTime', 'actualStopTime'])) {
            // Check for overlapping shifts
            if (prevShift) {
                // Overlapping
                if (shift.actualStartTime.isBeforeOnMinute(prevShift.actualStopTime)) {
                    shiftsOverlapping = true;
                    break;
                }
            }

            shift.link = link;
            if (isHidden) {
                // Take first four breaks
                let breakNr = 1;
                for (let brk of _.orderBy(breaks, 'breakStartTime')) {
                    brk.breakLink = link;
                    shift.breakToShift(brk, breakNr);
                    breakNr++;
                    if (breakNr > 4)
                        break;
                }
                shift.isModified = true;
            }
            prevShift = shift;
        }

        if (shiftsOverlapping) {
            this.translationService.translate("time.schedule.planning.editshift.overlappingshifts").then(term => {
                this.notificationService.showDialogEx(this.terms["time.schedule.planning.contextmenu.linkshift"].format(this.shiftUndefined), term.format(this.shiftsUndefined), SOEMessageBoxImage.Forbidden);
            });
            return;
        }

        if (this.isTemplateView || this.isEmployeePostView) {
            let template = this.templateHelper.getTemplateSchedule(employeeIdentifier, shifts[0].startTime);
            if (template)
                this.saveTemplateShifts(employeeIdentifier, template.timeScheduleTemplateHeadId, shifts, null, null);
        } else {
            this.saveShifts(Guid.newGuid().toString(), shifts, true, false, false, 0);
        }
    }

    private unlinkShifts(shifts: ShiftDTO[]) {
        if (shifts.length === 0)
            return;

        let employeeIdentifier: number = this.isEmployeePostView ? shifts[0].employeePostId : shifts[0].employeeId;
        let isHidden = (employeeIdentifier === this.hiddenEmployeeId);

        // Create ShiftDTO records of the breaks to be able to use break validation methods below
        shifts = _.concat(shifts, shifts[0].createBreaksFromShift());

        // Check for overlapping breaks
        if (ShiftDTO.hasOverlappingBreaks(shifts)) {
            let keys: string[] = [
                "time.schedule.planning.unlinkshifts.cannotunlink",
                "time.schedule.planning.unlinkshifts.hasoverlappingbreaks"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.unlinkshifts.cannotunlink"].format(this.shiftsDefined), terms["time.schedule.planning.unlinkshifts.hasoverlappingbreaks"].format(this.shiftsDefined.toUpperCaseFirstLetter(), this.shiftsDefined), SOEMessageBoxImage.Forbidden);
            });
            return;
        }

        // Check for overlapping shifts
        if (ShiftDTO.areShiftsOverlapping(shifts.filter(s => !s.isBreak))) {
            let keys: string[] = [
                "time.schedule.planning.unlinkshifts.checkbreaks.title",
                "time.schedule.planning.unlinkshifts.hasoverlappingshifts"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.unlinkshifts.checkbreaks"], terms["time.schedule.planning.unlinkshifts.hasoverlappingshifts"].format(this.shiftUndefined, this.shiftUndefined), SOEMessageBoxImage.Warning);
            });
        } else if (shifts.filter(s => s.isBreak).length > 0) {
            let keys: string[] = [
                "time.schedule.planning.unlinkshifts.checkbreaks.title",
                "time.schedule.planning.unlinkshifts.checkbreaks.message"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.unlinkshifts.checkbreaks.title"], terms["time.schedule.planning.unlinkshifts.checkbreaks.message"].format(this.shiftsDefined.toUpperCaseFirstLetter()), SOEMessageBoxImage.Warning);
            });
        }

        // This is a one time flag to keep the link on the first shift.
        // Otherwise, existing breaks will not be deleted.
        let firstShift = isHidden;
        shifts.filter(s => !s.isBreak).forEach(shift => {
            if (!firstShift)
                shift.link = Guid.newGuid();

            if (isHidden)
                shift.linkBreaks();

            shift.isModified = true;
            firstShift = false;
        });

        if (this.isTemplateView || this.isEmployeePostView) {
            let template = this.templateHelper.getTemplateSchedule(employeeIdentifier, shifts[0].startTime);
            if (template)
                this.saveTemplateShifts(employeeIdentifier, template.timeScheduleTemplateHeadId, shifts.filter(s => !s.isBreak), null, null);
        } else {
            this.saveShifts(Guid.newGuid().toString(), shifts.filter(s => !s.isBreak), true, false, false, 0);
        }
    }

    private sendShiftRequest(shift: ShiftDTO) {
        this.openShiftRequestDialog(shift);
    }

    private absence(shift: ShiftDTO) {
        this.openAbsenceDialog(shift);
    }

    private createAbsence(date: Date, employeeId: number) {
        this.openCreateAbsenceDialog(date, employeeId);
    }

    private deleteAbsence(shift: ShiftDTO) {
        this.openDeleteAbsenceDialog(shift);
    }

    private showHistory(shift: ShiftDTO) {
        this.openShiftHistory(shift);
    }

    private newTemplate(slot: SlotDTO) {
        this.initOpenTemplateScheduleDialog(TemplateScheduleModes.New, slot);
    }

    private editTemplate(slot: SlotDTO) {
        this.initOpenTemplateScheduleDialog(TemplateScheduleModes.Edit, slot);
    }

    private activateTemplate(slot: SlotDTO) {
        this.initOpenTemplateScheduleDialog(TemplateScheduleModes.Activate, slot);
    }

    // ACTIONS

    private renderBody = _.debounce((source: string, stopProgressWhenDone: boolean = true) => {
        if (this.isCalendarView)
            this.scheduleHandler.renderCalendar();
        else if (this.isTasksAndDeliveriesView)
            this.scheduleHandler.renderTasksAndDeliveries();
        else if (this.isStaffingNeedsView)
            this.scheduleHandler.renderStaffingNeeds();
        else {
            this.nbrOfVisibleEmployees = 0;
            this.scheduleHandler.renderSchedule(stopProgressWhenDone);
        }
    }, 200, { leading: false, trailing: true });

    public renderPlanningAgChart(createData: boolean = true) {
        if (!this.showDashboardPermission)
            return;

        if (createData)
            this.createPlanningAgChartData();

        this.scheduleHandler.renderPlanningAgChart();

        if (this.showPlanningAgChart && !this.planningAgChartData)
            this.showSelectableInformation(true, false);
    }

    public createPlanningAgChartData() {
        this.planningAgChartData = { isDayView: this.isCommonDayView, calculationType: this.selectableInformationSettings.followUpCalculationType, series: [], rows: [] };
        let needData = [];
        let needFreqData = [];
        let needRowFreqData = [];
        let budgetData = [];
        let forecastData = [];
        let templateScheduleData = [];
        let templateScheduleForEmployeePostData = [];
        let scheduleData = [];
        let timeData = [];

        let calculationType = this.followUpCalculationTypes.find(t => t.id === this.selectableInformationSettings.followUpCalculationType);
        let calculationTypeHours = this.selectableInformationSettings.followUpCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours;

        // Get currently displayed times
        let dates = this.dates.map(d => d.date);
        if (this.isCommonDayView) {
            // Add one for the last (ending time), otherwise chart stops at for example 23:45.
            dates.push(_.last(this.dates).date.addMinutes(this.dayViewMinorTickLength));
        } else if (this.isCommonScheduleView) {
            // Add one for the last (ending date), otherwise chart stops one day too early.
            dates.push(_.last(this.dates).date.addDays(1));
        }

        // Add keys depending on which values the user has selected to view
        if (this.selectableInformationSettings.followUpOnBudget)
            this.planningAgChartData.series.push({ key: 'budget', name: this.terms["time.schedule.planning.selectableinformation.followup.budget"], fill: '#99ccff', fillOpacity: 0.6, type: 'bar' });
        if (this.selectableInformationSettings.followUpOnForecast)
            this.planningAgChartData.series.push({ key: 'forecast', name: this.terms["time.schedule.planning.selectableinformation.followup.forecast"], fill: '#cc99ff', fillOpacity: 0.6, type: 'bar' });
        if (this.selectableInformationSettings.followUpOnTemplateSchedule && this.selectableInformationSettings.followUpCalculationType !== TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
            this.planningAgChartData.series.push({ key: 'templateSchedule', name: this.terms["time.schedule.planning.selectableinformation.followup.templateschedule"], fill: '#dfdfdf', fillOpacity: 1, type: 'bar' });
        if (this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost && this.selectableInformationSettings.followUpCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours)
            this.planningAgChartData.series.push({ key: 'templateScheduleForEmployeePost', name: this.terms["time.schedule.planning.selectableinformation.followup.templatescheduleforemployeepost"], fill: '#cde0f4', fillOpacity: 1, type: 'bar' });
        if (this.selectableInformationSettings.followUpOnSchedule && this.selectableInformationSettings.followUpCalculationType !== TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales)
            this.planningAgChartData.series.push({ key: 'schedule', name: this.terms["time.schedule.planning.selectableinformation.followup.schedule"], fill: '#d3ecda', fillOpacity: 1, type: 'bar' });
        if (this.selectableInformationSettings.followUpOnTime)
            this.planningAgChartData.series.push({ key: 'time', name: this.terms["time.schedule.planning.selectableinformation.followup.time"], fill: '#ffe6d5', fillOpacity: 1, type: 'bar' });
        if (this.selectableInformationSettings.followUpOnNeed)
            this.planningAgChartData.series.push({ key: 'need', name: this.terms["time.schedule.staffingneeds.planning.need"], fill: '#ffcd00', fillOpacity: 1, type: 'line' });
        if (this.selectableInformationSettings.followUpOnNeedRowFrequency)
            this.planningAgChartData.series.push({ key: 'needRowFreq', name: this.terms["time.schedule.staffingneeds.planning.needfrequency"], fill: '#da1e28', fillOpacity: 1, type: 'line' });
        if (this.selectableInformationSettings.followUpOnNeedFrequency && this.isDayView)
            this.planningAgChartData.series.push({ key: 'needFreq', name: "{0} ({1})".format(this.terms["time.schedule.staffingneeds.planning.needfrequency"], this.terms["core.rounded"].toLocaleLowerCase()), fill: '#f8d4d4', fillOpacity: 1, type: 'line' });

        // Use calculated data that is displayed in summary row
        if (this.selectableInformationSettings.followUpOnNeed || this.selectableInformationSettings.followUpOnNeedFrequency || this.selectableInformationSettings.followUpOnNeedRowFrequency || this.selectableInformationSettings.followUpOnBudget || this.selectableInformationSettings.followUpOnForecast || this.selectableInformationSettings.followUpOnTemplateSchedule || this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost || this.selectableInformationSettings.followUpOnSchedule || this.selectableInformationSettings.followUpOnTime) {
            dates.forEach(date => {
                let need: any = this.staffingNeedSum.find(d => this.isCommonDayView ? d.date.isSameMinuteAs(date) : d.date.isSameDayAs(date));
                let needSum = (need?.need || 0);
                let needRowFreqSum = (need?.needRowFrequency || 0);
                let needFreqSum = (need?.needFrequency || 0);
                let budgetSum = (need?.budget || 0);
                let forecastSum = (need?.forecast || 0);
                let templateScheduleSum = (need?.templateSchedule || 0);
                let templateScheduleForEmployeePostSum = (need?.templateScheduleForEmployeePost || 0);
                let scheduleSum = (need?.schedule || 0);
                let timeSum = (need?.time || 0);

                let multiplier = 1;

                if (this.isCommonDayView) {
                    // Copy value from last date to final date that is actually next day.
                    // Otherwise the line will dip to 0 in the end.
                    if (date.isSameMinuteAs(_.last(dates))) {
                        needSum = _.last(needData) ? _.last(needData).y : 0;
                        needFreqSum = _.last(needFreqData) ? _.last(needFreqData).y : 0;
                        needRowFreqSum = _.last(needRowFreqData) ? _.last(needRowFreqData).y : 0;
                        budgetSum = _.last(budgetData) ? _.last(budgetData).y : 0;
                        forecastSum = _.last(forecastData) ? _.last(forecastData).y : 0;
                        templateScheduleSum = _.last(templateScheduleData) ? _.last(templateScheduleData).y : 0;
                        templateScheduleForEmployeePostSum = _.last(templateScheduleForEmployeePostData) ? _.last(templateScheduleForEmployeePostData).y : 0;
                        scheduleSum = _.last(scheduleData) ? _.last(scheduleData).y : 0;
                        timeSum = _.last(timeData) ? _.last(timeData).y : 0;
                    }

                    if (calculationTypeHours)
                        multiplier = this.dayViewMinorTickLength;
                }

                if (!date.isSameMinuteAs(_.last(dates))) {
                    let data = { date: date };

                    if (this.planningAgChartData.series.map(s => s.key).contains('need'))
                        data['need'] = needSum * multiplier;
                    if (this.planningAgChartData.series.map(s => s.key).contains('needRowFreq'))
                        data['needRowFreq'] = needRowFreqSum;
                    if (this.planningAgChartData.series.map(s => s.key).contains('needFreq'))
                        data['needFreq'] = needFreqSum;
                    if (this.planningAgChartData.series.map(s => s.key).contains('budget'))
                        data['budget'] = budgetSum * multiplier;
                    if (this.planningAgChartData.series.map(s => s.key).contains('forecast'))
                        data['forecast'] = forecastSum * multiplier;
                    if (this.planningAgChartData.series.map(s => s.key).contains('templateSchedule'))
                        data['templateSchedule'] = templateScheduleSum * multiplier;
                    if (this.planningAgChartData.series.map(s => s.key).contains('templateScheduleForEmployeePost'))
                        data['templateScheduleForEmployeePost'] = templateScheduleForEmployeePostSum * multiplier;
                    if (this.planningAgChartData.series.map(s => s.key).contains('schedule'))
                        data['schedule'] = scheduleSum * multiplier;
                    if (this.planningAgChartData.series.map(s => s.key).contains('time'))
                        data['time'] = timeSum * multiplier;

                    this.planningAgChartData.rows.push(data);
                }
            });
        }

        // Create title based on selection
        let title: string = '';

        let titleSelections: string[] = [];
        if (this.selectableInformationSettings.followUpOnBudget)
            titleSelections.push(this.terms["time.schedule.planning.selectableinformation.followup.budget"]);
        if (this.selectableInformationSettings.followUpOnForecast)
            titleSelections.push(this.terms["time.schedule.planning.selectableinformation.followup.forecast"]);
        if (this.selectableInformationSettings.followUpOnTemplateSchedule)
            titleSelections.push(this.terms["time.schedule.planning.selectableinformation.followup.templateschedule"]);
        if (this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost)
            titleSelections.push(this.terms["time.schedule.planning.selectableinformation.followup.templatescheduleforemployeepost"]);
        if (this.selectableInformationSettings.followUpOnSchedule)
            titleSelections.push(this.terms["time.schedule.planning.selectableinformation.followup.schedule"]);
        if (this.selectableInformationSettings.followUpOnTime)
            titleSelections.push(this.terms["time.schedule.planning.selectableinformation.followup.time"]);

        if (titleSelections.length) {
            title = `${this.terms["core.selection"]}: ${titleSelections.join(', ')}`;

            if (calculationType)
                title += `. ${this.terms["time.schedule.planning.selectableinformation.followup.calculationtype"]} ${calculationType.name}`;

            let dim = this.accountDims.find(d => d.accountDimId === this.selectableInformationSettings.followUpAccountDimId);
            if (dim) {
                title += `, ${dim.name}`;

                let acc = dim.accounts.find(a => a.accountId === this.selectableInformationSettings.followUpAccountId);
                if (acc)
                    title += ` ${acc.name}`;
            }
        }

        this.planningAgChartData.title = title;

        this.scheduleHandler.setPlanningAgChartData(this.planningAgChartData);
    }

    public renderPlanningFollowUpTable(createData: boolean = true) {
        if (!this.showDashboardPermission)
            return;

        if (!createData && this.planningFollowUpTableData.length === 0)
            createData = true;

        if (createData) {
            if (this.useAccountHierarchy && (this.allAccountsSelected || (!this.selectableInformationSettings.followUpAccountId || this.selectableInformationSettings.followUpAccountId !== this.userAccountId))) {
                this.showSelectableInformation(true, true);
                return;
            }

            this.createPlanningFollowUpTableData(true);
        } else {
            this.scheduleHandler.renderPlanningFollowUpTable();
        }
    }

    public createPlanningFollowUpTableData(render: boolean = false) {
        if (this.staffingNeedData.length === 0) {
            this.loadStaffingNeed();
            return;
        }

        this.planningFollowUpTableData = [];

        // Get currently displayed times
        let dates = this.dates.map(d => d.date);
        // Summary row is returned with last date + 1
        dates.push(_.last(dates).addDays(1));
        dates.forEach(date => {
            let sales: any = { budget: 0, forecast: 0, templateSchedule: 0, schedule: 0, time: 0 };
            let hours: any = { budget: 0, forecast: 0, templateSchedule: 0, schedule: 0, time: 0 };
            let cost: any = { budget: 0, forecast: 0, templateSchedule: 0, templateScheduleForEmployeePost: 0, schedule: 0, scheduleAndTime: 0, time: 0 };
            let salaryPercent: any = { budget: 0, forecast: 0, templateSchedule: 0, schedule: 0, time: 0 };
            let lpat: any = { budget: 0, forecast: 0, templateSchedule: 0, schedule: 0, time: 0 };
            let fpat: any = { budget: 0, forecast: 0, templateSchedule: 0, schedule: 0, time: 0 };

            let intervalData: StaffingStatisticsInterval = this.staffingNeedData.find(d => d.interval.isSameDayAs(date));
            if (intervalData?.rows) {
                intervalData.rows.forEach(row => {
                    // Sales
                    sales.budget += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    sales.forecast += row.getForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    sales.templateSchedule += row.getTemplateScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    sales.schedule += row.getScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    sales.scheduleAndTime += row.getScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    sales.time += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);

                    // Hours
                    hours.budget += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    hours.forecast += row.getForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    hours.templateSchedule += row.getTemplateScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    hours.templateScheduleForEmployeePost += row.getTemplateScheduleForEmployeePostValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    hours.schedule += row.getScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    hours.scheduleAndTime += row.getScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    hours.time += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);

                    // Personel cost
                    cost.budget += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    cost.forecast += row.getForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    cost.templateSchedule += row.getTemplateScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    cost.schedule += row.getScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    cost.scheduleAndTime += row.getScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    cost.time += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);

                    // Salary percent
                    salaryPercent.budget += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    salaryPercent.forecast += row.getForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    salaryPercent.templateSchedule += row.getTemplateScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    salaryPercent.schedule += row.getScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    salaryPercent.scheduleAndTime += row.getScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    salaryPercent.time += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);

                    // LPAT
                    lpat.budget += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);
                    lpat.forecast += row.getForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);
                    lpat.templateSchedule += row.getTemplateScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);
                    lpat.schedule += row.getScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);
                    lpat.scheduleAndTime += row.getScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);
                    lpat.time += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);

                    // FPAT
                    fpat.budget += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    fpat.forecast += row.getForecastValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    fpat.templateSchedule += row.getTemplateScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    fpat.schedule += row.getScheduleValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    fpat.scheduleAndTime += row.getScheduleAndTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    fpat.time += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                });
            }

            this.planningFollowUpTableData.push({ date: date, sales: sales, hours: hours, cost: cost, salaryPercent: salaryPercent, lpat: lpat, fpat: fpat });
        });

        if (render)
            this.scheduleHandler.renderPlanningFollowUpTable();
    }

    private exportPlanningFollowUpTableToExcel() {
        // https://github.com/sheetjs/js-xlsx

        let tbl = document.getElementById('planning-followup-table');

        // Format columns
        // Remove thousand separators, replace decimal point
        _.forEach(tbl.getElementsByClassName('format-amount'), elem => {
            elem.innerHTML = elem.innerHTML.trim().replace('&nbsp;', '').replace(',', '.');
        });
        // Convert to minutes
        _.forEach(tbl.getElementsByClassName('format-time'), elem => {
            elem.innerHTML = (CalendarUtility.timeSpanToMinutes(elem.innerHTML) / 60).toString();
        });

        // Create work sheet
        // Skip summary row
        let ws = XLSX.utils.table_to_sheet(tbl, { sheetRows: this.dates.length + 2 });

        // Setup columns
        let columnLetters: string[] = ['B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'AA', 'AB', 'AC', 'AD'];
        ws["!cols"] = [];

        // Format columns
        let fmtAmount = "0.00";
        let fmtTime = "0.00";   // Minutes
        let fmtPercent = "0.0%";

        // Skip first two rows (labels)
        for (let rowNbr = 3; rowNbr <= this.dates.length + 2; rowNbr++) {
            let addColumn: boolean = (rowNbr === 3);
            let columnLetterCounter: number = 0;

            // Sales
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalesBudget && this.showBudgetPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalesForecast && this.showForecastPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalesTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);

            // Hours
            if (this.selectableInformationSettings.followUpShowCalculationTypeHoursBudget && this.showBudgetPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtTime);
            if (this.selectableInformationSettings.followUpShowCalculationTypeHoursForecast && this.showForecastPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtTime);
            if (this.selectableInformationSettings.followUpShowCalculationTypeHoursTemplateSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtTime);
            if (this.selectableInformationSettings.followUpShowCalculationTypeHoursSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtTime);
            if (this.selectableInformationSettings.followUpShowCalculationTypeHoursTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtTime);

            // Personel costs
            if (this.selectableInformationSettings.followUpShowCalculationTypePersonelCostBudget && this.showBudgetPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypePersonelCostForecast && this.showForecastPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypePersonelCostTemplateSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypePersonelCostSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypePersonelCostTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypePersonelCostTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);

            // Salary percent
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentBudget && this.showBudgetPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtPercent);
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentForecast && this.showForecastPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtPercent);
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTemplateSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtPercent);
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtPercent);
            if (this.selectableInformationSettings.followUpShowCalculationTypeSalaryPercentTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtPercent);

            // LPAT
            if (this.selectableInformationSettings.followUpShowCalculationTypeLPATBudget && this.showBudgetPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeLPATForecast && this.showForecastPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeLPATTemplateSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeLPATSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeLPATTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);

            // FPAT
            if (this.selectableInformationSettings.followUpShowCalculationTypeFPATBudget && this.showBudgetPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeFPATForecast && this.showForecastPermission)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeFPATTemplateSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeFPATSchedule)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
            if (this.selectableInformationSettings.followUpShowCalculationTypeFPATTime)
                this.formatExcelColumn(ws, addColumn, columnLetters[columnLetterCounter++], rowNbr, fmtAmount);
        }

        // Create work book
        let workbookName = this.terms["time.schedule.planning.followuptable.exportname"];
        let wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, workbookName);
        // Write document
        let wbout = XLSX.write(wb, { bookType: 'xlsx', bookSST: true, type: 'binary' });
        let buf = new ArrayBuffer(wbout.length);
        let view = new Uint8Array(buf);
        for (let i = 0; i < wbout.length; i++)
            view[i] = wbout.charCodeAt(i) & 0xFF;

        saveAs(new Blob([buf], { type: "application/octet-stream" }), workbookName + ".xlsx");

        // Restore original formatting
        _.forEach(tbl.getElementsByClassName('format-amount'), elem => {
            elem.innerHTML = parseFloat(elem.innerHTML).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        });
        _.forEach(tbl.getElementsByClassName('format-time'), elem => {
            elem.innerHTML = CalendarUtility.minutesToTimeSpan(parseFloat(elem.innerHTML) * 60);
        });
    }

    private formatExcelColumn(ws: any, addColumn: boolean, letter: string, i: number, format: string) {
        if (addColumn)
            ws["!cols"].push({ wpx: 100 });

        ws[`${letter}${i}`].z = format;
    }

    public renderStaffingNeedsAgChart(createData: boolean = true) {
        if (createData)
            this.createStaffingNeedsAgChartData();

        this.scheduleHandler.renderStaffingNeedsAgChart();
    }

    public createStaffingNeedsAgChartData() {
        let data = { name: this.terms["time.schedule.staffingneeds.planning.need"], rows: [], maxValue: 0 };

        // Get currently displayed times
        let dates = this.dates.map(d => d.date);
        // Add one for the last (ending time), otherwise chart stops at for example 23:45.
        dates.push(_.last(this.dates).date.addMinutes(this.dayViewMinorTickLength));

        // Get head (should be only one, since chart is only displayed in day view)
        let head = this.heads && this.heads.length > 0 ? this.heads[0] : null;
        if (!head)
            return;

        const filteredOnShiftType = this.isFilteredOnShiftType;
        const shiftTypeIds = this.getFilteredShiftTypeIds();
        let maxValue = 0;

        // Sum for each hour
        dates.forEach(date => {
            let value = 0;

            head.rows.forEach(row => {
                let isInDate = row.periods.some(p => {
                    return ((filteredOnShiftType && shiftTypeIds.includes(p.shiftTypeId)) || !filteredOnShiftType) && date.isSameOrAfterOnMinute(p.actualStartTime) && date.isBeforeOnMinute(p.actualStopTime);
                });
                if (isInDate)
                    value++;
            });

            if (value > maxValue)
                maxValue = value;

            data.rows.push({ date: date, value: value });
        });

        data.maxValue = maxValue;
        this.scheduleHandler.setStaffingNeedsAgChartData(data);
    }

    private setupDashboard(): ng.IPromise<any> {
        // https://github.com/ashish-chopra/angular-gauge

        let deferral = this.$q.defer<any>();

        if (this.dashboardInitialized) {
            deferral.resolve();
        } else {
            // Get threshold settings
            var settingTypes: number[] = [];
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold2);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold2);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold2);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold2);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold2);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold2);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold1);
            settingTypes.push(CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold2);

            this.coreService.getCompanySettings(settingTypes).then(x => {
                let red = 'rgba(255, 0, 0, 0.6)';
                let yellow = 'rgba(255, 205, 0, 0.6)';
                let green = 'rgba(0, 230, 0, 0.6)';

                let sales1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold1, 50);
                let sales2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeSalesThreshold2, 80);
                this.gaugeSalesThresholds['0'] = { color: red };
                this.gaugeSalesThresholds[sales1] = { color: yellow };
                this.gaugeSalesThresholds[sales2] = { color: green };

                let hours1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold1, 50);
                let hours2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeHoursThreshold2, 80);
                this.gaugeHoursThresholds['0'] = { color: red };
                this.gaugeHoursThresholds[hours1] = { color: yellow };
                this.gaugeHoursThresholds[hours2] = { color: green };

                let salaryCost1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold1, 50);
                let salaryCost2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeSalaryCostThreshold2, 80);
                this.gaugeCostThresholds['0'] = { color: green };
                this.gaugeCostThresholds[salaryCost1] = { color: yellow };
                this.gaugeCostThresholds[salaryCost2] = { color: red };

                let salaryPercent1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold1, 50);
                let salaryPercent2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeSalaryPercentThreshold2, 80);
                this.gaugeSalaryPercentThresholds['0'] = { color: green };
                this.gaugeSalaryPercentThresholds[salaryPercent1] = { color: yellow };
                this.gaugeSalaryPercentThresholds[salaryPercent2] = { color: red };

                let lpat1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold1, 50);
                let lpat2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeLPATThreshold2, 80);
                this.gaugeLPATThresholds['0'] = { color: green };
                this.gaugeLPATThresholds[lpat1] = { color: yellow };
                this.gaugeLPATThresholds[lpat2] = { color: red };

                let fpat1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold1, 50);
                let fpat2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeFPATThreshold2, 80);
                this.gaugeFPATThresholds['0'] = { color: red };
                this.gaugeFPATThresholds[fpat1] = { color: yellow };
                this.gaugeFPATThresholds[fpat2] = { color: green };

                let bpat1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold1, 50);
                let bpat2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningGaugeBPATThreshold2, 80);
                this.gaugeBPATThresholds['0'] = { color: red };
                this.gaugeBPATThresholds[bpat1] = { color: yellow };
                this.gaugeBPATThresholds[bpat2] = { color: green };

                this.dashboardInitialized = true;
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    private showCalendarDetails(period: ShiftPeriodDTO) {
        if (!period)
            return;

        // Show calendar details dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CalendarDetails/Views/calendarDetails.html"),
            controller: CalendarDetailsController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                date: () => { return period.date },
                dayDescription: () => { return period.dayDescription },
                employeeId: () => { return this.employeeId },
                employeeIds: () => { return this.getFilteredEmployeeIds() },
                shiftTypeIds: () => { return this.isFilteredOnShiftType ? this.getFilteredShiftTypeIds() : null },
                deviationCauseIds: () => { return this.isFilteredOnDeviationCause ? this.getFilteredDeviationCauseIds() : null },
                preliminaryPermission: () => { return this.preliminaryPermission },
                calendarViewCountByEmployee: () => { return this.calendarViewCountByEmployee },
                isOrderPlanningMode: () => { return this.isOrderPlanningMode }
            }
        }
        this.$uibModal.open(options);
    }

    private showScheduleEvents(date: Date, scheduleEvents: TimeScheduleEventForPlanningDTO[]) {
        // Show schedule events details dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/ScheduleEvents/Views/scheduleEvents.html"),
            controller: ScheduleEventsController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                date: () => { return date },
                scheduleEvents: () => { return scheduleEvents },
            }
        }
        this.$uibModal.open(options);
    }

    public openEditShift(shift: ShiftDTO, shifts: ShiftDTO[], date: Date, employeeIdentifier: number, isStandby: boolean, isOnDuty: boolean, reloadOnCancel = false) {
        if (!employeeIdentifier && shift)
            employeeIdentifier = this.isEmployeePostView ? shift.employeePostId : shift.employeeId;
        if (!date && shift)
            date = shift.actualStartDate;
        let employees = this.isEmployeePostView ? this.employedEmployees.filter(e => e.employeePostId) : this.employedEmployees.filter(e => !e.employeePostId);

        if (!shifts)
            shifts = [];

        // Get current template
        let template: TimeScheduleTemplateHeadSmallDTO;
        if (this.isTemplateView || this.isEmployeePostView)
            template = this.templateHelper.getTemplateSchedule(employeeIdentifier, date);

        // Read only
        let readOnly: boolean = false;
        if (shift)
            readOnly = this.isOrderPlanningMode && !shift.isOrder && !shift.isBooking;
        else if (shifts.length > 0)
            readOnly = this.isOrderPlanningMode && shifts.filter(s => s.isSchedule || s.isStandby || s.isOnDuty).length > 0;
        else if (this.isEmployeeInactive(employeeIdentifier))
            employeeIdentifier = 0;

        let shiftTypes: ShiftTypeDTO[] = [];
        if (this.useAccountHierarchy)
            shiftTypes.push(...this.allShiftTypes);
        else
            shiftTypes.push(...this.allShiftTypes.filter(s => this.shiftTypeIds.includes(s.shiftTypeId) || s.shiftTypeId === 0));

        // Show edit shift dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/EditShift/Views/editShift.html"),
            controller: EditShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                isAdmin: () => { return this.isAdmin },
                currentEmployeeId: () => { return this.employeeId },
                templateHelper: () => { return this.templateHelper },
                isScheduleView: () => { return this.isScheduleView },
                isTemplateView: () => { return this.isTemplateView },
                isEmployeePostView: () => { return this.isEmployeePostView },
                isScenarioView: () => { return this.isScenarioView },
                isStandbyView: () => { return this.isStandbyView },
                isReadonly: () => { return readOnly },
                template: () => { return template },
                standby: () => { return isStandby },
                onDuty: () => { return isOnDuty },
                shift: () => { return shift },
                shifts: () => { return shifts },
                loadTasks: () => { return shifts.length > 0 },
                date: () => { return date },
                employeeId: () => { return employeeIdentifier },
                shiftTypes: () => { return shiftTypes; },
                shiftTypeAccountDim: () => { return this.shiftTypeAccountDim; },
                timeScheduleTypes: () => { return this.timeScheduleTypes; },
                allBreakTimeCodes: () => { return this.breakTimeCodes; },
                singleEmployeeMode: () => { return false; },
                employees: () => { return employees; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds; },
                showSkills: () => { return this.showSkills; },
                standbyModifyPermission: () => { return this.standbyShiftsModifyPermission; },
                onDutyModifyPermission: () => { return this.onDutyShiftsModifyPermission; },
                attestPermission: () => { return this.attestPermission; },
                hasStaffingByEmployeeAccount: () => { return this.hasStaffingByEmployeeAccount; },
                placementPermission: () => { return this.placementPermission; },
                showTotalCost: () => { return this.selectableInformationSettings.showTotalCost; },
                showTotalCostIncEmpTaxAndSuppCharge: () => { return this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge; },
                showWeekendSalary: () => { return this.selectableInformationSettings.showWeekendSalary; },
                showGrossTime: () => { return this.selectableInformationSettings.showGrossTime; },
                showExtraShift: () => { return this.showExtraShift; },
                showSubstitute: () => { return this.showSubstitute; },
                useMultipleScheduleTypes: () => { return this.useMultipleScheduleTypes; },
                showAvailability: () => { return this.selectableInformationSettings.showAvailability; },
                maxNbrOfBreaks: () => { return this.maxNbrOfBreaks; },
                clockRounding: () => { return this.clockRounding; },
                useAccountHierarchy: () => { return this.useAccountHierarchy; },
                accountDim: () => { return this.accountDims.find(a => a.accountDimId === this.defaultEmployeeAccountDimId); },
                accountDims: () => { return this.accountDims; },
                accountHierarchyId: () => { return this.accountHierarchyId; },
                validAccountIds: () => { return this.validAccountIds; },
                showSecondaryAccounts: () => { return this.showSecondaryAccounts; },
                shiftTypeMandatory: () => { return this.shiftTypeMandatory; },
                keepShiftsTogether: () => { return this.keepShiftsTogether; },
                disableBreaksWithinHolesWarning: () => { return this.disableBreaksWithinHolesWarning; },
                disableSaveAndActivateCheck: () => { return this.disableSaveAndActivateCheck; },
                autoSaveAndActivate: () => { return this.autoSaveAndActivate; },
                allowHolesWithoutBreaks: () => { return this.allowHolesWithoutBreaks; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                useShiftRequestPreventTooEarly: () => { return this.useShiftRequestPreventTooEarly; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.selectableInformationSettings.skipXEMailOnChanges; },
                dayHasDeviations: () => { return false; },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId; },
                scenarioDateFrom: () => { return this.scenarioHead ? this.scenarioHead.dateFrom : null; },
                scenarioDateTo: () => { return this.scenarioHead ? this.scenarioHead.dateTo : null; },
                loadedRangeDateFrom: () => { return this.dateFrom; },
                loadedRangeDateTo: () => { return this.dateTo; },
                inactivateLending: () => { return this.inactivateLending; },
                extraShiftAsDefaultOnHidden: () => { return this.extraShiftAsDefaultOnHidden; },
                planningPeriodStartDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.startDate : null; },
                planningPeriodStopDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.stopDate : null; },
            }
        }
        this.editShiftModal = this.$uibModal.open(options);

        this.editShiftModal.result.then((result: any) => {
            if (result?.reload && result.reload === true) {
                this.reloadShiftsForSpecifiedEmployeeIds = result.reloadEmployeeIds;
                this.loadData('openEditShift');
            } else if (reloadOnCancel) {
                this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                this.loadData('openEditShift');

            }
        });
    }

    private openSelectBreakTimeCode(message: string, timeCodeBreakIds: number[], shift: ShiftDTO, dayShifts: ShiftDTO[], breakNo: number, breakStartTime: Date, breakStopTime: Date, dragStart: boolean, dragStop: boolean): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        let breakTimeCodes = this.breakTimeCodes.filter(t => timeCodeBreakIds.includes(t.timeCodeId));

        // Show select break time code dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/SelectBreakTimeCode/Views/selectBreakTimeCode.html"),
            controller: SelectBreakTimeCodeController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                message: () => { return message; },
                breakTimeCodes: () => { return breakTimeCodes; },
                breakStartTime: () => { return breakStartTime; },
                breakStopTime: () => { return breakStopTime; },
                dragStart: () => { return dragStart; },
                dragStop: () => { return dragStop; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                const breakTimeCode: ITimeCodeBreakSmallDTO = result.breakTimeCode;
                if (breakTimeCode) {
                    shift[`break${breakNo}TimeCodeId`] = breakTimeCode.timeCodeId;
                    shift[`break${breakNo}StartTime`] = result.breakStartTime;
                    shift[`break${breakNo}Minutes`] = breakTimeCode.defaultMinutes;

                    // Validate again
                    this.validateBreakChange(shift, dayShifts, breakNo, dragStart, dragStop).then(passed => {
                        deferral.resolve(passed);
                    });
                }
            } else {
                deferral.resolve(false);
            }
        }, (reason) => {
            // Cancelled
            deferral.resolve(false);
        });

        return deferral.promise;
    }

    private openShiftRequestDialog(shift: ShiftDTO) {
        this.validateSendShiftRequest(shift).then(passed => {
            if (passed) {
                this.sharedScheduleService.getShiftRequestStatus(shift.timeScheduleTemplateBlockId).then(x => {
                    let excludeEmployeeIds: number[] = [];
                    if (x?.recipients)
                        excludeEmployeeIds = x.recipients.map(r => r.employeeId);

                    // Do some filtering on valid accounts for the date of the selected shift
                    let validEmployees: EmployeeListDTO[] = [];

                    if (this.useAccountHierarchy) {
                        this.employedEmployees.forEach(employee => {
                            if (!employee.hidden && employee.accounts) {
                                for (let empAccount of employee.accounts) {
                                    if (this.validAccountIds.includes(empAccount.accountId) && shift.date.isSameOrAfterOnDay(empAccount.dateFrom) && (empAccount.dateTo !== null && shift.date.isSameOrBeforeOnDay(empAccount.dateTo) || CalendarUtility.isEmptyDate(empAccount.dateTo))) {
                                        validEmployees.push(employee);
                                        break;
                                    }
                                }
                            }
                        });
                    } else {
                        validEmployees = this.employedEmployees;
                    }

                    const modal = this.modalInstance.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/MessageMenu/edit.html"),
                        controller: MessageEditController,
                        controllerAs: 'ctrl',
                        bindToController: true,
                        backdrop: 'static',
                        size: 'lg',
                        scope: this.$scope,
                    });

                    modal.rendered.then(() => {
                        this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                            source: 'Planning',
                            modal: modal,
                            title: this.isOrderPlanningMode ? this.terms["time.schedule.planning.contextmenu.sendassignmentrequest"] : this.terms["time.schedule.planning.contextmenu.sendshiftrequest"],
                            id: 0,
                            messageMinHeight: 250,
                            type: XEMailType.Outgoing,
                            messageType: TermGroup_MessageType.ShiftRequest,
                            shift: shift,
                            showAvailableEmployees: true,
                            showAvailability: this.selectableInformationSettings.showAvailability,
                            allEmployees: validEmployees.filter(e => e.employeeId && !e.hidden && !e.vacant && !excludeEmployeeIds.includes(e.employeeId))
                        });
                    });

                    modal.result.then(result => {
                        if (result?.success) {
                            this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                            this.loadData('openShiftRequestDialog');
                        }
                    });
                });
            }
        });
    }

    private validateSendShiftRequest(shift: ShiftDTO): ng.IPromise<any> {
        const deferral = this.$q.defer<boolean>();

        if (this.useShiftRequestPreventTooEarly) {
            this.sharedScheduleService.checkIfShiftRequestIsTooEarlyToSend(shift.actualStartTime).then(result => {
                this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.ShiftRequest, result, shift.employeeId, false, "time.schedule.planning.contextmenu.sendshiftrequest").then(passed => {
                    deferral.resolve(passed);
                });
            }).catch(reason => {
                this.notificationService.showServiceError(reason);
                deferral.resolve(false);
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private showShiftRequestStatus = _.debounce((shiftId: number) => {
        // TODO: Ugly work around for ng-click fireing twice when clicking on icon on shift
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/ShiftRequestStatus/Views/shiftRequestStatus.html"),
            controller: ShiftRequestStatusController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                shiftId: () => { return shiftId },
                modifyPermission: () => { return this.modifyPermission && this.isAdmin }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result.reload) {
                let shift = this.scheduleHandler.getShiftById(shiftId);
                if (shift) {
                    this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                    this.loadShifts();
                }
            }
        }).catch(reason => {
            ModalUtility.handleModalClose(reason);
        });
    }, 500, { leading: true, trailing: false });

    private openAbsenceDialog(shift: ShiftDTO) {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
            controller: AbsenceRequestsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: 0,
                employeeId: shift.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.AbsenceDialog,
                skipXEMailOnShiftChanges: this.selectableInformationSettings.skipXEMailOnChanges,
                shiftId: shift.timeScheduleTemplateBlockId,
                date: shift.date,
                hideOptionSelectedShift: this.isOrderPlanningMode,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                timeScheduleScenarioHeadId: this.timeScheduleScenarioHeadId,
            });
        });

        modal.result.then(employeeIds => {
            this.reloadShiftsForSpecifiedEmployeeIds = employeeIds;
            this.loadData('openAbsenceDialog');
        });
    }

    private openAbsenceRequestDialog(shift: ShiftDTO) {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
            controller: AbsenceRequestsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: 0,
                employeeId: shift.employeeId,
                viewMode: AbsenceRequestViewMode.Attest,
                guiMode: AbsenceRequestGuiMode.EmployeeRequest,
                loadRequestFromInterval: true,
                date: shift.date,
                skipXEMailOnShiftChanges: false,
                shiftId: 0,
                hideOptionSelectedShift: false,
                parentMode: AbsenceRequestParentMode.SchedulePlanning,
                readOnly: this.isScenarioView,
                timeScheduleScenarioHeadId: this.timeScheduleScenarioHeadId,
            });
        });

        modal.result.then(employeeIds => {
            this.reloadShiftsForSpecifiedEmployeeIds = employeeIds;
            this.loadData('openAbsenceRequestDialog');
        });
    }

    private openCreateAbsenceDialog(date: Date, employeeId: number) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateAbsence/Views/createAbsence.html"),
            controller: CreateAbsenceController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                date: () => { return date; },
                employeeId: () => { return employeeId; },
                absenceTypes: () => { return this.absenceTypes; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.reload) {
                this.reloadShiftsForSpecifiedEmployeeIds = [employeeId];
                this.loadData('openCreateAbsenceDialog');
                this.recalculateAnnualLeaveBalanceForEmployees([employeeId]);
            }
        });
    }

    private openDeleteAbsenceDialog(shift: ShiftDTO) {
        if (!shift)
            return;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DeleteAbsence/Views/deleteAbsence.html"),
            controller: DeleteAbsenceController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                shift: () => { return shift },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.reload) {
                this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                this.loadData('openDeleteAbsenceDialog');
                this.recalculateAnnualLeaveBalanceForEmployees([shift.employeeId]);
            }
        });
    }

    private openDeleteShift(shifts: ShiftDTO[]) {
        if (this.isEmployeePostView)
            this.setEmployeePostInfoOnShifts(shifts);

        let onDutyShifts: ShiftDTO[] = [];
        shifts.forEach(shft => {
            let ods = this.scheduleHandler.getIntersectingOnDutyShifts(shft.timeScheduleTemplateBlockId);
            ods.forEach(sh => {
                if (!onDutyShifts.find(s => s.timeScheduleTemplateBlockId === sh.timeScheduleTemplateBlockId)) {
                    onDutyShifts.push(sh);
                }
            });
        });

        // Show delete shift dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DeleteShift/Views/deleteShift.html"),
            controller: DeleteShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                shifts: () => { return shifts },
                viewDefinition: () => { return this.viewDefinition },
                onDutyShiftsModifyPermission: () => { return this.onDutyShiftsModifyPermission },
                onDutyShifts: () => { return onDutyShifts }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.selectedShifts) {
                let shiftsToDelete: ShiftDTO[] = result.selectedShifts;
                const includeOnDutyShifts: boolean = result.includeOnDutyShifts;
                const onDutyShiftIds: number[] = result.onDutyShiftIds;

                // Template shifts have their own save method
                if (this.isTemplateView || this.isEmployeePostView) {
                    shiftsToDelete.forEach(shift => {
                        this.shifts.splice(this.shifts.indexOf(shift), 1);
                    });

                    const employeeIdentifier = this.isEmployeePostView ? shiftsToDelete[0].employeePostId : shiftsToDelete[0].employeeId;
                    const date = shiftsToDelete[0].actualStartDate;

                    const template = this.templateHelper.getTemplateSchedule(employeeIdentifier, date);
                    if (template) {
                        this.templateHelper.updateTemplateSchedule(employeeIdentifier, template.timeScheduleTemplateHeadId, null, null).then(success => {
                            // Success or failure, always reload employee
                            this.reloadShiftsForSpecifiedEmployeeIds = [employeeIdentifier];
                            this.loadData('openDeleteShift template');

                            // Reload unscheduled tasks
                            this.loadUnscheduledTasksAndDeliveriesDates();
                        });
                    }
                } else {
                    const shiftIds = shiftsToDelete.map(s => s.timeScheduleTemplateBlockId);
                    this.startModalProgress("core.deleting");
                    this.scheduleService.deleteShifts(shiftIds, this.selectableInformationSettings.skipXEMailOnChanges, this.timeScheduleScenarioHeadId, includeOnDutyShifts ? onDutyShiftIds : []).then(deleteResult => {
                        this.stopModalProgress(null, true);
                        if (deleteResult.success) {
                            let orderIds: number[] = [];
                            this.clearEmployeeIdsForShiftLoad();

                            // Remove shifts from collection
                            shiftsToDelete.forEach(shift => {
                                // Remember orders connected to deleted shifts
                                if (this.isOrderPlanningMode && shift.isOrder)
                                    orderIds.push(shift.order.orderId);

                                this.reloadShiftsForSpecifiedEmployeeIds.push(shift.employeeId);
                            });

                            // Reload affected orders in order list
                            if (this.isOrderPlanningMode && orderIds.length > 0)
                                this.loadUnscheduledOrders(orderIds);

                            // Reload shifts for affected employee(s)
                            this.loadData('openDeleteShift schedule');

                            // Reload annual scheduled time for affected employee(s)
                            if (this.calculatePlanningPeriodScheduledTime) {
                                shiftsToDelete.map(s => s.employeeId).forEach(employeeId => {
                                    this.loadAnnualScheduledTime(employeeId);
                                });
                            }

                            // Reload unscheduled tasks
                            this.loadUnscheduledTasksAndDeliveriesDates();
                        } else {
                            this.notificationService.showDialogEx(this.terms["error.default_error"], deleteResult.errorMessage, SOEMessageBoxImage.Error);
                        }
                    }).catch(reason => {
                        this.notificationService.showServiceError(reason);
                    });
                }
            }
        });
    }

    private openSplitShift(shift: ShiftDTO) {
        // Show split shift dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/SplitShift/Views/splitShift.html"),
            controller: SplitShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                currentEmployeeId: () => { return this.employeeId },
                templateHelper: () => { return this.templateHelper },
                isTemplate: () => { return this.isTemplateView },
                isEmployeePost: () => { return this.isEmployeePostView },
                showSkills: () => { return this.showSkills; },
                showExtraShift: () => { return this.showExtraShift; },
                showSubstitute: () => { return this.showSubstitute; },
                clockRounding: () => { return this.clockRounding },
                keepShiftsTogether: () => { return this.keepShiftsTogether },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds },
                allEmployees: () => { return this.employedEmployees },
                shift: () => { return shift },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId; },
                planningPeriodStartDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.startDate : null; },
                planningPeriodStopDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.stopDate : null; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                if (this.isTemplateView || this.isEmployeePostView)
                    this.performSplitTemplateShift(shift, result.splitTime, result.employeeId1, result.employeeId2);
                else
                    this.performSplitShift(shift, result.splitTime, result.employeeId1, result.employeeId2);
            }
        });
    }

    private performSplitTemplateShift(shift: ShiftDTO, splitTime: Date, selectedEmployeeId1: number, selectedEmployeeId2: number) {
        let sourceTemplate: TimeScheduleTemplateHeadSmallDTO;
        let template1: TimeScheduleTemplateHeadSmallDTO;
        let template2: TimeScheduleTemplateHeadSmallDTO;
        if (this.templateHelper) {
            sourceTemplate = this.templateHelper.getTemplateSchedule(this.isEmployeePostView ? shift.employeePostId : shift.employeeId, shift.startTime);
            template1 = this.templateHelper.getTemplateSchedule(selectedEmployeeId1, shift.startTime);
            template2 = this.templateHelper.getTemplateSchedule(selectedEmployeeId2, shift.startTime);
        }
        let sourceTemplateHeadId: number = sourceTemplate ? sourceTemplate.timeScheduleTemplateHeadId : 0;
        let template1HeadId: number = template1 ? template1.timeScheduleTemplateHeadId : 0;
        let template2HeadId: number = template2 ? template2.timeScheduleTemplateHeadId : 0;
        let employeeId1 = this.isEmployeePostView ? 0 : selectedEmployeeId1;
        let employeePostId1 = this.isEmployeePostView ? selectedEmployeeId1 : 0;
        let employeeId2 = this.isEmployeePostView ? 0 : selectedEmployeeId2;
        let employeePostId2 = this.isEmployeePostView ? selectedEmployeeId2 : 0;

        this.sharedScheduleService.splitTemplateShift(shift, sourceTemplateHeadId, splitTime, employeeId1, employeePostId1, template1HeadId, employeeId2, employeePostId2, template2HeadId, this.keepShiftsTogether, this.selectableInformationSettings.skipXEMailOnChanges).then(result => {
            if (result.success) {
                // Reload shifts for affected employee(s)
                this.reloadShiftsForSpecifiedEmployeeIds = _.uniq([this.isEmployeePostView ? shift.employeePostId : shift.employeeId, selectedEmployeeId1, selectedEmployeeId2])
                this.loadData('performSplitTemplateShift');

                // Reload annual scheduled time for affected employee(s)
                if (this.calculatePlanningPeriodScheduledTime) {
                    this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeId => {
                        this.loadAnnualScheduledTime(employeeId);
                    });
                }
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    private performSplitShift(shift: ShiftDTO, splitTime: Date, employeeId1: number, employeeId2: number) {
        this.sharedScheduleService.splitShift(shift, splitTime, employeeId1, employeeId2, this.keepShiftsTogether, this.isTemplateView, this.selectableInformationSettings.skipXEMailOnChanges, this.timeScheduleScenarioHeadId).then(result => {
            if (result.success) {
                // Reload shifts for affected employee(s)
                this.reloadShiftsForSpecifiedEmployeeIds = [shift.employeeId];
                if (employeeId1 !== shift.employeeId)
                    this.reloadShiftsForSpecifiedEmployeeIds.push(employeeId1);
                if (employeeId2 !== shift.employeeId)
                    this.reloadShiftsForSpecifiedEmployeeIds.push(employeeId2);
                this.loadData('performSplitShift');

                // Reload annual scheduled time for affected employee(s)
                if (this.calculatePlanningPeriodScheduledTime) {
                    this.reloadShiftsForSpecifiedEmployeeIds.forEach(employeeId => {
                        this.loadAnnualScheduledTime(employeeId);
                    });
                }
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    public openDragShiftByIds(shiftIds: number[], targetEmployee: EmployeeListDTO, targetDate: Date, targetShift: ShiftDTO, moveOffsetDays: number, defaultAction?: DragShiftAction) {
        let sourceShifts: ShiftDTO[] = this.shifts.filter(s => shiftIds.includes(s.timeScheduleTemplateBlockId));

        // Special for dragging a shift that starts the day before.
        // targetDate will be different depending on if you drag on first day or second day.
        if (sourceShifts.length > 0 && sourceShifts[0].belongsToNextDay && sourceShifts[0].actualDateOnLoad.isSameDayAs(targetDate.addDays(1)))
            targetDate = targetDate.addDays(1);

        if (defaultAction === undefined && this.dragDropMoveAsDefault)
            defaultAction = DragShiftAction.Move;

        this.openDragShift(sourceShifts, targetEmployee, targetDate, targetShift, moveOffsetDays, defaultAction);
    }

    private openDragShift(sourceShifts: ShiftDTO[], targetEmployee: EmployeeListDTO, targetDate: Date, targetShift: ShiftDTO, moveOffsetDays: number, defaultAction?: DragShiftAction, changeEmployeeMode: boolean = false) {
        if (this.isEmployeePostView)
            this.setEmployeePostInfoOnShifts(sourceShifts);

        let targetEmployees: EmployeeListDTO[] = null;
        if (changeEmployeeMode) {
            targetEmployees = this.employedEmployees.filter(e => e.hasEmployment(targetDate, targetDate));
            let validEmployeeIds = this.getValidEmployeeIdsForInterval(targetEmployees, targetDate, targetDate);
            targetEmployees = targetEmployees.filter(e => validEmployeeIds.includes(e.employeeId));
        }

        if (targetEmployees && this.isHiddenEmployeeReadOnly)
            targetEmployees = targetEmployees.filter(e => !e.hidden);

        const sourceShiftIds = sourceShifts.map(s => s.timeScheduleTemplateBlockId);
        let onDutyShifts: ShiftDTO[] = [];
        sourceShifts.forEach(shft => {
            let ods = this.scheduleHandler.getIntersectingOnDutyShifts(shft.timeScheduleTemplateBlockId);
            ods.forEach(sh => {
                if (!onDutyShifts.find(s => s.timeScheduleTemplateBlockId === sh.timeScheduleTemplateBlockId) && !sourceShiftIds.includes(sh.timeScheduleTemplateBlockId)) {
                    onDutyShifts.push(sh);
                }
            });
        });

        // Show drag shift dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DragShift/Views/dragShift.html"),
            controller: DragShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                templateHelper: () => { return this.templateHelper },
                sourceShifts: () => { return sourceShifts },
                targetEmployee: () => { return targetEmployee; },
                targetDate: () => { return targetDate; },
                targetShift: () => { return targetShift; },
                moveOffsetDays: () => { return moveOffsetDays; },
                planningMode: () => { return this.planningMode; },
                viewDefinition: () => { return this.viewDefinition; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds; },
                useVacant: () => { return this.useVacant; },
                showExtraShift: () => { return this.showExtraShift; },
                showSubstitute: () => { return this.showSubstitute; },
                keepShiftsTogether: () => { return this.keepShiftsTogether; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.selectableInformationSettings.skipXEMailOnChanges; },
                useAccountHierarchy: () => { return this.useAccountHierarchy; },
                validAccountIds: () => { return this.validAccountIds; },
                inactivateLending: () => { return this.inactivateLending; },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId; },
                onDutyShiftsModifyPermission: () => { return this.onDutyShiftsModifyPermission },
                onDutyShifts: () => { return onDutyShifts },
                defaultAction: () => { return defaultAction; },
                changeEmployeeMode: () => { return changeEmployeeMode; },
                employees: () => { return targetEmployees; },
                planningPeriodStartDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.startDate : null; },
                planningPeriodStopDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.stopDate : null; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                // Dragged
                let selectedAction: DragShiftAction = <DragShiftAction>result.action;

                // Reload only affected employees
                const sourceEmployeeId: number = this.isEmployeePostView ? sourceShifts[0].employeePostId : sourceShifts[0].employeeId;
                const targetEmployeeId: number = result.targetEmployeeId ? result.targetEmployeeId : targetEmployee ? (this.isEmployeePostView ? targetEmployee.employeePostId : targetEmployee.employeeId) : 0;
                this.clearEmployeeIdsForShiftLoad();
                this.reloadShiftsForSpecifiedEmployeeIds.push(sourceEmployeeId);
                if (!this.reloadShiftsForSpecifiedEmployeeIds.includes(targetEmployeeId))
                    this.reloadShiftsForSpecifiedEmployeeIds.push(targetEmployeeId);
                if (selectedAction == DragShiftAction.ReplaceAndFree)
                    this.reloadShiftsForSpecifiedEmployeeIds.push(this.hiddenEmployeeId);
                this.loadData('openDragShift');

                // Reload affected orders in order list
                if (this.isOrderPlanningMode) {
                    let orderIds: number[] = [];
                    sourceShifts.filter(s => s.order).forEach(shift => {
                        orderIds.push(shift.order.orderId);
                    });
                    <ShiftDTO[]>result.targetShifts.filter(s => s.order).forEach(shift => {
                        orderIds.push(shift.order.orderId);
                    });

                    this.loadUnscheduledOrders(orderIds);
                }

                // Reload annual scheduled time for affected employee(s)
                if (this.calculatePlanningPeriodScheduledTime) {
                    if (sourceEmployeeId)
                        this.loadAnnualScheduledTime(sourceEmployeeId);
                    if (targetEmployeeId && targetEmployeeId !== sourceEmployeeId)
                        this.loadAnnualScheduledTime(targetEmployeeId);
                }

                // Warn about break times
                if (selectedAction === DragShiftAction.Copy || selectedAction === DragShiftAction.Move || selectedAction === DragShiftAction.Replace || selectedAction === DragShiftAction.ReplaceAndFree) {
                    if (sourceShifts[0].break1Minutes + sourceShifts[0].break2Minutes + sourceShifts[0].break3Minutes + sourceShifts[0].break4Minutes > 0)
                        this.showCheckBreakTimesDialog();
                }
            }
        }, (reason) => {
            // Cancelled

            // Reload only affected employees
            this.clearEmployeeIdsForShiftLoad();
            this.reloadShiftsForSpecifiedEmployeeIds.push(sourceShifts[0].employeeId);
            if (targetEmployee && !this.reloadShiftsForSpecifiedEmployeeIds.includes(targetEmployee.employeeId))
                this.reloadShiftsForSpecifiedEmployeeIds.push(targetEmployee.employeeId);
            this.loadData('openDragShift cancelled');
        });
    }

    public employeeDroppedOnShift(employeeId: number, shiftId: number) {
        if (!employeeId || !shiftId)
            return;

        // Get shift
        var shift = this.shifts.find(s => s.timeScheduleTemplateBlockId === shiftId);
        if (!shift)
            return;

        // TODO: Notify?
        // Can't assign shift to same employee
        if (employeeId === shift.employeeId)
            return;

        // Get employee
        var employee = this.getEmployeeById(employeeId);
        if (!employee)
            return;

        // Get target employee
        var targetEmployee = this.getEmployeeById(shift.employeeId);
        if (!targetEmployee)
            return;

        if (this.keepShiftsTogether) {
            // Get target shift(s)
            this.sharedScheduleService.getShiftsForDay(shift.employeeId, shift.actualStartDate, [TermGroup_TimeScheduleTemplateBlockType.Schedule, TermGroup_TimeScheduleTemplateBlockType.Standby], true, false, shift.link, false, false, false, true, this.timeScheduleScenarioHeadId).then(x => {
                this.openDropEmployee(employee, x, targetEmployee);
            });
        } else {
            this.openDropEmployee(employee, [shift], targetEmployee);
        }
    }

    private openDropEmployee(employee: EmployeeListDTO, targetShifts: ShiftDTO[], targetEmployee: EmployeeListDTO) {
        // TODO: Notify?
        // Target shift is absence, can't assign
        if (targetShifts.filter(s => s.timeDeviationCauseId).length > 0)
            return;

        // Show drop employee dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DropEmployee/Views/dropEmployee.html"),
            controller: DropEmployeeController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                employee: () => { return employee; },
                targetShifts: () => { return targetShifts; },
                targetEmployeeName: () => { return targetEmployee.name; },
                planningMode: () => { return this.planningMode; },
                viewDefinition: () => { return this.viewDefinition; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds; },
                keepShiftsTogether: () => { return this.keepShiftsTogether; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.selectableInformationSettings.skipXEMailOnChanges; },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId; },
                planningPeriodStartDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.startDate : null; },
                planningPeriodStopDate: () => { return this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.stopDate : null; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            // Assigned
            if (result.success)
                this.saveShifts(Guid.newGuid().toString(), targetShifts, false, false, false, 0, [targetEmployee.employeeId]);
        }, (reason) => {
            // Cancelled
        });
    }

    public employeeDroppedOnEmployeePost(employeeId: number, employeePostId: number) {
        if (!employeeId || !employeePostId)
            return;

        var employee = this.getEmployeeById(employeeId);
        if (!employee)
            return;

        var employeePost = this.getEmployeePostById(employeePostId);
        if (!employeePost)
            return;

        var template = this.templateHelper.getTemplateSchedule(this.isEmployeePostView ? employeePost.employeePostId : employeePost.employeeId, this.dateFrom);
        if (!template)
            return;

        this.openAssignEmployeePost(employee, employeePost, template.timeScheduleTemplateHeadId);
    }

    private openAssignEmployeePost(employee: EmployeeListDTO, employeePost: EmployeeListDTO, timeScheduleTemplateHeadId: number) {
        // Check if employee post already have an employee assigned to it
        if (employeePost.employeeId) {
            let keys: string[] = [
                "time.schedule.planning.assignemployeepost.employeeexiststitle",
                "time.schedule.planning.assignemployeepost.employeeexistsmessage"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.schedule.planning.assignemployeepost.employeeexiststitle"], terms["time.schedule.planning.assignemployeepost.employeeexistsmessage"], SOEMessageBoxImage.Forbidden);
            });
            return;
        }

        // Show assign employee post dialog
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/AssignEmployeePost/Views/assignEmployeePost.html"),
            controller: AssignEmployeePostController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                employee: () => { return employee; },
                employeePost: () => { return employeePost; },
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result.success) {
                let template = employeePost.templateSchedules.find(t => t.timeScheduleTemplateHeadId === timeScheduleTemplateHeadId);
                if (template) {
                    if (template.noOfDays === 0)
                        template.noOfDays = 7;
                    let startDate: Date = template.startDate;
                    let currentStartDate: Date = this.dateFrom.beginningOfWeek();
                    if (template.startDate.isBeforeOnDay(currentStartDate)) {
                        while (startDate.addDays(template.noOfDays).isSameOrBeforeOnDay(currentStartDate)) {
                            startDate = startDate.addDays(template.noOfDays);
                        }
                    } else if (template.startDate.isAfterOnDay(currentStartDate)) {
                        while (startDate.addDays(-template.noOfDays).isSameOrAfterOnDay(currentStartDate)) {
                            startDate = startDate.addDays(-template.noOfDays);
                        }
                    }

                    if (!startDate.isSameDayAs(currentStartDate)) {
                        let keys: string[] = [
                            "time.schedule.planning.assignemployeepost.checkstartdate.title",
                            "time.schedule.planning.assignemployeepost.checkstartdate.message"
                        ];
                        this.translationService.translateMany(keys).then(terms => {
                            let modal = this.notificationService.showDialogEx(terms["time.schedule.planning.assignemployeepost.checkstartdate.title"], terms["time.schedule.planning.assignemployeepost.checkstartdate.message"].format(template.startDate.toFormattedDate(), (template.noOfDays / 7).toString(), currentStartDate.toFormattedDate(), startDate.toFormattedDate()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                            modal.result.then(val => {
                                this.assignEmployeePost(employee, employeePost, timeScheduleTemplateHeadId, startDate);
                            }, (reason) => {
                                // User cancelled
                            });
                        });
                    } else {
                        this.assignEmployeePost(employee, employeePost, timeScheduleTemplateHeadId, startDate);
                    }
                }
            }
        });
    }

    private assignEmployeePost(employee: EmployeeListDTO, employeePost: EmployeeListDTO, timeScheduleTemplateHeadId: number, startDate: Date) {
        this.startSave();
        this.scheduleService.assignTimeScheduleTemplateToEmployee(timeScheduleTemplateHeadId, employee.employeeId, startDate).then(res => {
            if (res.success) {
                employeePost.employeeId = employee.employeeId;
                this.reloadEmployeePosts([employeePost.employeePostId], true).then(() => {
                    this.reloadEmployees([employee.employeeId], true).then(() => {
                        this.reloadShiftsForSpecifiedEmployeeIds = [employeePost.employeePostId];
                        this.loadData('assignEmployeePost');
                    });
                });
            } else {
                this.failedSave(res.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    public openAssignTaskToEmployee(tasks: StaffingNeedsTaskDTO[], targetEmployee: EmployeeListDTO, targetDate: Date) {
        // Show assign task dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/AssignTask/Views/assignTask.html"),
            controller: AssignTaskController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                tasks: () => { return tasks; },
                targetEmployeeId: () => { return targetEmployee.employeeId; },
                targetEmployeeName: () => { return targetEmployee.name; },
                targetDate: () => { return targetDate; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.selectableInformationSettings.skipXEMailOnChanges; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.success) {
                // Reload only affected employees
                this.reloadShiftsForSpecifiedEmployeeIds = [targetEmployee.employeeId];
                this.loadData('openAssignTaskToEmployee');

                // Reload unscheduled tasks
                this.loadUnscheduledTasksAndDeliveriesDates();
            }
        }, (reason) => {
            // Cancelled
        });
    }

    public openAssignTaskToEmployeePost(tasks: StaffingNeedsTaskDTO[], targetEmployee: EmployeeListDTO, targetDate: Date) {
        var template = this.templateHelper.getTemplateSchedule(this.isEmployeePostView ? targetEmployee.employeePostId : targetEmployee.employeeId, targetDate);
        if (!template)
            return;

        this.scheduleService.assignTemplateShiftTask(tasks, targetDate, template.timeScheduleTemplateHeadId).then(x => {
            if (!x || x.length === 0) {
                var keys: string[] = [
                    "time.schedule.planning.assigntasktoemployeepost.errortitle",
                    "time.schedule.planning.assigntasktoemployeepost.errormessage"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.assigntasktoemployeepost.errortitle"], terms["time.schedule.planning.assigntasktoemployeepost.errormessage"].format(tasks[0].name, targetEmployee.name), SOEMessageBoxImage.Error);
                });
            } else {
                // Convert to typed DTOs
                let shifts = x.map(s => {
                    let obj = new ShiftDTO;
                    angular.extend(obj, s);
                    obj.fixDates();
                    return obj;
                });

                this.setShiftData(shifts);
                this.openEditShift(null, shifts, shifts[0].date, this.isEmployeePostView ? shifts[0].employeePostId : shifts[0].employeeId, false, false);
            }
        }).catch(reason => {
            this.notificationService.showServiceError(reason);
        });
    }

    private showCheckBreakTimesDialog() {
        if ((!this.isCommonDayView) || this.disableCheckBreakTimesWarning || this.isOrderPlanningMode)
            return;

        var keys: string[] = [
            "time.schedule.planning.dragshift.breaktimeswarningtitle",
            "time.schedule.planning.dragshift.breaktimeswarningmessage"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialog(terms["time.schedule.planning.dragshift.breaktimeswarningtitle"], terms["time.schedule.planning.dragshift.breaktimeswarningmessage"].format(this.shiftUndefined, this.shiftDefined), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large, false, true, this.terms["core.donotshowagain"]);
            modal.result.then(result => {
                if (result.isChecked) {
                    this.disableCheckBreakTimesWarning = true;
                    this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeSchedulePlanningDisableCheckBreakTimesWarning, this.disableCheckBreakTimesWarning);
                }
            });
        });
    }

    private openShiftHistory(shift: ShiftDTO) {
        // Show shifthistory dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/ShiftHistory/Views/shiftHistory.html"),
            controller: ShiftHistoryController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                shiftType: () => { return shift.type },
                timeScheduleTemplateBlockId: () => { return shift.timeScheduleTemplateBlockId }
            }
        }
        this.$uibModal.open(options);
    }

    private annualSummaryDialogOpen: boolean = false;
    private openAnnualSummary(employeeId: number) {
        if (!this.currentPlanningPeriod)
            return;

        // Get selected employee
        const employee = this.getEmployeeById(employeeId);
        if (!employee)
            return;

        let options: angular.ui.bootstrap.IModalSettings;

        if (this.calculatePlanningPeriodScheduledTimeUseAveragingPeriod) {
            if (!this.currentPlanningPeriodChildInRangeExact && !this.hasPlanningPeriodHeadButNoChild) {
                this.translationService.translateMany(["time.schedule.planning.employeeperiodtimesummary.opensummaryerror.title", "time.schedule.planning.employeeperiodtimesummary.opensummaryerror.message"]).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.employeeperiodtimesummary.opensummaryerror.title"], terms["time.schedule.planning.employeeperiodtimesummary.opensummaryerror.message"], SOEMessageBoxImage.Error);
                });
                return;
            }

            // Show employee period summary dialog
            options = {
                templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/EmployeePeriodTimeSummary/Views/employeePeriodTimeSummary.html"),
                controller: EmployeePeriodTimeSummaryController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'md',
                resolve: {
                    dateFrom: () => { return this.dateFrom },
                    dateTo: () => { return this.dateTo },
                    planningPeriodHead: () => { return this.planningPeriodHead },
                    planningPeriodChild: () => { return this.planningPeriodChild },
                    currentPlanningPeriod: () => { return this.currentPlanningPeriod },
                    employee: () => { return employee },
                    planningPeriodColorOver: () => { return this.planningPeriodColorOver },
                    planningPeriodColorEqual: () => { return this.planningPeriodColorEqual },
                    planningPeriodColorUnder: () => { return this.planningPeriodColorUnder },
                }
            }
        } else {
            // Show annual summary dialog
            options = {
                templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/AnnualSummary/Views/annualSummary.html"),
                controller: AnnualSummaryController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'md',
                resolve: {
                    dateFrom: () => { return this.currentPlanningPeriod.startDate },
                    dateTo: () => { return this.currentPlanningPeriod.stopDate },
                    planningPeriodHeadId: () => { return this.currentPlanningPeriod.timePeriodHeadId },
                    periodName: () => { return this.currentPlanningPeriod.name },
                    employee: () => { return employee },
                    recalcOnOpen: () => { return false },
                    planningPeriodColorOver: () => { return this.planningPeriodColorOver },
                    planningPeriodColorEqual: () => { return this.planningPeriodColorEqual },
                    planningPeriodColorUnder: () => { return this.planningPeriodColorUnder },
                }
            }
        }

        this.annualSummaryDialogOpen = true;
        this.$uibModal.open(options).result.then(() => {
            this.annualSummaryDialogOpen = false;
        }, (reason) => {
            this.annualSummaryDialogOpen = false;
        });
    }

    private initOpenTemplateScheduleDialog(mode: TemplateScheduleModes, slot: SlotDTO) {
        // Get selected employee
        const employee = this.getEmployeeById(slot.employeeId);
        if (!employee)
            return;

        const date: Date = slot.startTime.date();

        if (mode === TemplateScheduleModes.New) {
            this.openTemplateScheduleDialog(mode, date, employee, null);
        } else {
            // Check if current employee has any template in visible interval
            let templateHeads = employee.getTemplateSchedulesForDate(date);
            if (templateHeads?.length === 0) {
                const keys: string[] = [
                    "time.schedule.planning.templateschedule.cantopen",
                    "time.schedule.planning.templateschedule.save.error.notemplate"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.templateschedule.cantopen"], terms["time.schedule.planning.templateschedule.save.error.notemplate"].format(employee.name), SOEMessageBoxImage.Forbidden);
                });
                return;
            } else if (templateHeads?.length === 1) {
                this.openTemplateScheduleDialog(mode, date, employee, templateHeads[0]);
            } else {
                // Show select template schedule dialog
                const options: angular.ui.bootstrap.IModalSettings = {
                    templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/SelectTemplateSchedule/Views/selectTemplateSchedule.html"),
                    controller: SelectTemplateScheduleController,
                    controllerAs: "ctrl",
                    bindToController: true,
                    backdrop: 'static',
                    size: 'md',
                    resolve: {
                        templateHeads: () => { return templateHeads }
                    }
                }
                this.$uibModal.open(options).result.then((result: any) => {
                    if (result && result.templateHead) {
                        this.openTemplateScheduleDialog(mode, date, employee, result.templateHead);
                    }
                });
            }
        }
    }

    private openTemplateScheduleDialog(mode: TemplateScheduleModes, date: Date, employee: EmployeeListDTO, templateHead: TimeScheduleTemplateHeadSmallDTO) {
        // Get number of weeks currently visible
        let nbrOfWeeks = this.nbrOfVisibleDays / 7;
        if (nbrOfWeeks < 1)
            nbrOfWeeks = 1;

        // Get employees
        let empList: TemplateScheduleEmployeeDTO[] = [];
        this.employedEmployees.forEach(emp => {
            let dto: TemplateScheduleEmployeeDTO = new TemplateScheduleEmployeeDTO();
            dto.employeeId = emp.employeeId;
            dto.employeeNr = emp.employeeNr;
            dto.employeeNrSort = emp.employeeNrSort;
            dto.name = emp.name;
            dto.templateStartDate = this.dateFrom;
            dto.nbrOfWeeks = nbrOfWeeks;

            // Get current template
            let template = emp.getTemplateSchedule(date);
            if (template) {
                let templateWeeks: number = template.noOfDays / 7;
                if (templateWeeks < 1)
                    templateWeeks = 1;

                dto.currentTemplate = template.name;
                if (!template.name.endsWithCaseInsensitive(template.startDate.toFormattedDate()))
                    dto.currentTemplate += " " + template.startDate.toFormattedDate();
                dto.currentTemplateNbrOfWeeks = templateWeeks;
            } else {
                dto.currentTemplate = '';
                dto.currentTemplateNbrOfWeeks = 0;
            }

            empList.push(dto);
        });

        // Show template schedule dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/TemplateSchedule/Views/templateSchedule.html"),
            controller: TemplateScheduleController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                mode: () => { return mode },
                useAccountHierarchy: () => { return this.useAccountHierarchy },
                useStopDate: () => { return this.useTemplateScheduleStopDate },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId },
                employees: () => { return empList },
                selectedPeriodFrom: () => { return this.dateFrom.beginningOfDay() },
                selectedPeriodTo: () => { return this.dateTo.endOfDay() },
                templateHead: () => { return templateHead },
                employee: () => { return employee },
                startDate: () => { return date },
                nbrOfWeeks: () => { return nbrOfWeeks },
                placementDefaultPreliminary: () => { return this.placementDefaultPreliminary },
                placementHidePreliminary: () => { return this.placementHidePreliminary }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.success) {
                if (result.action === 'save' || result.action === 'delete') {
                    // Reload employee, to get correct information about the templates
                    this.reloadEmployees([employee.employeeId], false).then(() => {
                        // Reload template for employee
                        this.reloadShiftsForSpecifiedEmployeeIds = [employee.employeeId];
                        this.loadData('openTemplateScheduleDialog');
                    });
                }
            }
        });
    }

    private openDefToFromPrelShift(prelToDef: boolean) {
        // Show dialog for moving shifts from preliminary ta definitive or vice versa
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DefToFromPrelShift/Views/defToFromPrelShift.html"),
            controller: DefToFromPrelShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                prelToDef: () => { return prelToDef },
                dateFrom: () => { return this.dateFrom },
                dateTo: () => { return this.dateTo },
                employeeId: () => { return this.employeeId },
                filteredEmployeeIds: () => { return this.getFilteredEmployeeIds() }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.success) {
                this.reloadShiftsForSpecifiedEmployeeIds = result.employeeIds;
                this.loadData('openDefToFromPrelShift');
            }
        });
    }

    private openAllocateLeisureCodes(deleteMode: boolean) {
        // Show dialog for allocating leisure days
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/AllocateLeisureCodes/Views/allocateLeisureCodes.html"),
            controller: AllocateLeisureCodesController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                dateFrom: () => { return this.dateFrom },
                dateTo: () => { return this.dateTo },
                sourceEmployees: () => { return this.employedEmployees.filter(e => e.isVisible && !e.hidden) },
                deleteMode: () => { return deleteMode }
            }
        }
        this.$uibModal.open(options).result.then((result: { reloadEmployeeIds: number[], evaluateWorkRules: boolean }) => {
            if (result && result.reloadEmployeeIds.length > 0) {
                // Reload shifts on employees that were partly or fully successful
                this.reloadShiftsForSpecifiedEmployeeIds = result.reloadEmployeeIds;
                // After loading shifts, evaluate work rules on all employees, if selected in dialog
                this.evaluateAllWorkRulesAfterLoadingShifts = result.evaluateWorkRules;
                this.loadData('allocateLeisureCodes');
            }
        });
    }

    private openCopySchedule(employeeId: number) {
        if (!employeeId) {
            // Get employee from selected shift or slot (if any)
            employeeId = this.scheduleHandler.getSlotInfo().employeeId;
        }

        if (employeeId) {
            // Can not copy from hidden employee
            if (employeeId === this.hiddenEmployeeId)
                employeeId = null;
        }

        // Show copy schedule dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CopySchedule/Views/copySchedule.html"),
            controller: CopyScheduleController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                useAccountHierarchy: () => { return this.useAccountHierarchy },
                employees: () => { return this.visibleEmployees.filter(e => !e.hidden && e.active) },
                employeeId: () => { return employeeId }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.success) {
                this.reloadEmployees([result.sourceEmployeeId, result.targetEmployeeId], false).then(() => {
                    this.reloadShiftsForSpecifiedEmployeeIds = [result.sourceEmployeeId, result.targetEmployeeId];
                    this.loadData('openCopySchedule');
                });
            }
        });
    }

    private openTimeCalculationDialog(calculationFunction: SoeTimeAttestFunctionOption) {
        // Get employees
        let empList: TimeAttestCalculationFunctionDTO[] = [];
        let employeeIds = this.getFilteredEmployeeIds();
        employeeIds.forEach(id => {
            if (id !== this.hiddenEmployeeId && !this.isEmployeeInactive(id)) {
                let employee = this.getEmployeeById(id);
                if (employee) {
                    let dto = new TimeAttestCalculationFunctionDTO();
                    dto.employeeId = employee.employeeId;
                    dto.employeeName = employee.name;
                    dto.employeeNr = employee.employeeNr;
                    empList.push(dto);
                }
            }
        });

        let calculationText: string = "";
        if (calculationFunction == SoeTimeAttestFunctionOption.RestoreToSchedule)
            calculationText = this.terms["time.schedule.planning.editshift.functions.restoretoschedule"];
        else if (calculationFunction == SoeTimeAttestFunctionOption.ScenarioRemoveAbsence)
            calculationText = this.terms["time.schedule.planning.editshift.functions.removeabsence"];

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Time/TimeAttest/Dialogs/Calculation/Views/timeAttestCalculation.html"),
            controller: TimeAttestCalculationController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                calculationFunction: () => { return calculationFunction },
                calculationText: () => { return calculationText },
                employees: () => { return empList },
                setAsSelected: () => { return false },
                dateFrom: () => { return this.dateFrom.beginningOfDay() },
                dateTo: () => { return this.dateTo.endOfDay() },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId },
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.reloadEmployeeIds.length > 0) {
                this.reloadShiftsForSpecifiedEmployeeIds = result.reloadEmployeeIds;
                this.loadData('openTimeCalculationDialog');
            }
        });
    }

    private openCreateTemplateBreaks() {
        // Get visible employees
        // Exclude employees with absence
        let employees: EmployeeListDTO[] = [];
        this.visibleEmployees.filter(e => this.getFilteredEmployeeIds().includes(e.employeeId)).forEach(employee => {
            let hasAbsence = this.shifts.filter(s => s.employeeId === employee.employeeId && s.isAbsence).length > 0;
            if (employee.active && !hasAbsence)
                employees.push(employee);
        });

        // Show dialog for creating breaks from break template
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateTemplateBreaks/Views/createTemplateBreaks.html"),
            controller: CreateTemplateBreaksController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                date: () => { return this.dateFrom },
                employees: () => { return employees },
                timeScheduleScenarioHeadId: () => { return this.timeScheduleScenarioHeadId },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.success) {
                this.editMode = PlanningEditModes.TemplateBreaks;

                const keys: string[] = [
                    "time.schedule.planning.createtemplatebreaks.saveinfo.title",
                    "time.schedule.planning.createtemplatebreaks.saveinfo.message"
                ];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.schedule.planning.createtemplatebreaks.saveinfo.title"], terms["time.schedule.planning.createtemplatebreaks.saveinfo.message"], SOEMessageBoxImage.Information);
                });

                // Convert to typed DTOs
                let tempBreakId = -1;
                result.shifts.forEach(s => {
                    // Find shift and set new break information
                    // Create temp id's for the breaks, otherwise they will not be rendered
                    let shift: ShiftDTO = this.shifts.find(shft => shft.timeScheduleTemplateBlockId === s.timeScheduleTemplateBlockId);
                    if (shift) {
                        shift.clearBreaks();
                        if (s.break1TimeCodeId)
                            shift.setBreakInformation(1, tempBreakId--, CalendarUtility.convertToDate(s.break1StartTime), s.break1TimeCodeId, s.break1Minutes, s.break1Link, s.break1IsPreliminary);
                        if (s.break2TimeCodeId)
                            shift.setBreakInformation(2, tempBreakId--, CalendarUtility.convertToDate(s.break2StartTime), s.break2TimeCodeId, s.break2Minutes, s.break2Link, s.break2IsPreliminary);
                        if (s.break3TimeCodeId)
                            shift.setBreakInformation(3, tempBreakId--, CalendarUtility.convertToDate(s.break3StartTime), s.break3TimeCodeId, s.break3Minutes, s.break3Link, s.break3IsPreliminary);
                        if (s.break4TimeCodeId)
                            shift.setBreakInformation(4, tempBreakId--, CalendarUtility.convertToDate(s.break4StartTime), s.break4TimeCodeId, s.break4Minutes, s.break4Link, s.break4IsPreliminary);
                        this.setShiftLabel(shift);
                    }
                });

                // Set affected employees to modified, to be able to know which to save
                result.employeeIds.forEach(employeeId => {
                    this.setEmployeeAsModified(employeeId);
                });

                this.reloadShiftsForSpecifiedEmployeeIds = [];
                this.recalculateEmployeeWorkTimes = true;
                this.filterShifts('createTemplateBreaks');
            }
        });
    }

    private openCreateTemplates() {
        let nbrOfWeeks = this.nbrOfVisibleWeeks;

        // Get employees
        let empList: TemplateScheduleEmployeeDTO[] = [];
        // Add empty
        let emptyDto: TemplateScheduleEmployeeDTO = new TemplateScheduleEmployeeDTO();
        emptyDto.employeeId = 0;
        emptyDto.employeeNr = '0';
        emptyDto.employeeNrSort = '0';
        emptyDto.name = '';
        empList.push(emptyDto);

        this.employedEmployees.filter(e => !e.isGroupHeader && !e['isShiftType']).forEach(emp => {
            let dto: TemplateScheduleEmployeeDTO = new TemplateScheduleEmployeeDTO();
            dto.employeeId = emp.employeeId;
            dto.employeeNr = emp.employeeNr;
            dto.employeeNrSort = emp.employeeNrSort;
            dto.name = emp.name;
            dto.templateStartDate = this.dateFrom;
            dto.nbrOfWeeks = nbrOfWeeks;
            empList.push(dto);
        });

        // Get visible employees
        let employees: TemplateScheduleEmployeeDTO[] = [];
        this.visibleEmployees.filter(e => !e.isGroupHeader && !e['isShiftType'] && e.active).forEach(emp => {
            if (!this.isHiddenEmployeeReadOnly || emp.employeeId !== this.hiddenEmployeeId) {
                let dto: TemplateScheduleEmployeeDTO = new TemplateScheduleEmployeeDTO();

                dto.employeeId = emp.employeeId;
                dto.employeeNr = emp.employeeNr;
                dto.employeeNrSort = emp.employeeNrSort;
                dto.name = emp.name;
                dto.templateStartDate = this.dateFrom;
                dto.templateStopDate = null;
                dto.nbrOfWeeks = nbrOfWeeks;

                // Get current template
                let template = emp.getTemplateSchedule(this.dateFrom);
                if (template) {
                    dto.currentTemplate = template.name;
                    dto.currentTemplateNbrOfWeeks = template.noOfDays / 7;
                    if (dto.currentTemplateNbrOfWeeks < 1)
                        dto.currentTemplateNbrOfWeeks = 1;

                    if (template.startDate && !template.name.endsWithCaseInsensitive(template.startDate.toFormattedDate()))
                        dto.currentTemplate += ", {0}".format(template.startDate.toFormattedDate());
                } else {
                    dto.currentTemplate = '';
                    dto.currentTemplateNbrOfWeeks = 0;
                }

                employees.push(dto);
            }
        });

        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateTemplateSchedules/Views/createTemplateSchedules.html"),
            controller: CreateTemplateSchedulesController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                useAccountHierarchy: () => { return this.useAccountHierarchy },
                useStopDate: () => { return this.useTemplateScheduleStopDate },
                allEmployees: () => { return empList },
                employees: () => { return employees },
                dateFrom: () => { return this.dateFrom },
                dateTo: () => { return null },
                nbrOfWeeks: () => { return nbrOfWeeks }
            }
        });
    }

    private openActivateSchedule() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"),
            controller: ActivateScheduleGridController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal no-toolbar-margin',
            scope: this.$scope,
            resolve: {
            }
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, employeeIds: this.getVisibleEmployeeIds(), dateFrom: this.dateFrom, dateTo: this.dateTo });
        });

        modal.result.then((result: any) => {
            if (result?.reload) {
                if (result.employeeIds && result.employeeIds.length > 0) {
                    // Reload employee, to get correct information about the templates
                    this.reloadEmployees(result.employeeIds, false).then(() => {
                        // Reload template for employee
                        this.reloadShiftsForSpecifiedEmployeeIds = result.employeeIds;
                        this.loadData('openActivateSchedule');
                    });
                } else {
                    this.loadData('openActivateSchedule');
                }
            }
        });
    }

    public openAdjustFollowUpData() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/AdjustFollowUpData/Views/adjustFollowUpData.html"),
            controller: AdjustFollowUpDataController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                followUpCalculationTypes: () => { return this.followUpCalculationTypes },
                selectableInformationSettings: () => { return this.selectableInformationSettings },
                originalRow: () => { return this.staffingNeedOriginalSummaryRow },
                row: () => { return this.getStaffingNeedsSummaryRow() },
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                let row = (result.row ? result.row : (result.restore ? this.staffingNeedOriginalSummaryRow : null));
                if (row)
                    this.setStaffingNeedsSummaryRow(CoreUtility.cloneDTO(row));
            }
        });
    }

    public openEditAssignment(shift: ShiftDTO, date: Date = null, employeeId: number = null): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (!employeeId && shift)
            employeeId = shift.employeeId;
        if (!date && shift)
            date = shift.startTime;

        if (shift && !shift.timeScheduleEmployeePeriodId) {
            this.getTimeScheduleEmployeePeriodId(employeeId, date).then((x: number) => {
                shift.timeScheduleEmployeePeriodId = x;
                if (!shift.timeScheduleEmployeePeriodId) {
                    deferral.resolve(false);
                } else {
                    this.openEditAssignmentDialog(shift, date, employeeId);
                    deferral.resolve(true);
                }
            });
        } else {
            this.openEditAssignmentDialog(shift, date, employeeId);
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private openEditAssignmentDialog(shift: ShiftDTO, date: Date, employeeId: number) {
        // Read only
        let readOnly: boolean = false;
        if (shift)
            readOnly = (shift.isReadOnly || shift.isAbsence || shift.isAbsenceRequest);

        // Show edit order dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/EditAssignment/Views/editAssignment.html"),
            controller: EditAssignmentController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                currentEmployeeId: () => { return this.employeeId },
                isReadonly: () => { return readOnly },
                modifyPermission: () => { return this.modifyPermission },
                shift: () => { return shift },
                date: () => { return date },
                employeeId: () => { return employeeId },
                shiftTypes: () => { return this.shiftTypes; },
                employees: () => { return this.employedEmployees; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds; },
                showSkills: () => { return this.showSkills; },
                dayStartTime: () => { return this.dayViewStartTime; },
                dayEndTime: () => { return this.dayViewEndTime; },
                showAvailability: () => { return this.selectableInformationSettings.showAvailability; },
                clockRounding: () => { return this.clockRounding; },
                shiftTypeMandatory: () => { return this.shiftTypeMandatory; },
                keepShiftsTogether: () => { return this.keepShiftsTogether; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.selectableInformationSettings.skipXEMailOnChanges; },
                ignoreScheduledBreaksOnAssignment: () => { return this.orderPlanningIgnoreScheduledBreaksOnAssignment; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.save && result.save === true) {
                if (result.isMultipleDays)
                    this.saveAssignments(result.employeeId, result.orderId, result.shiftTypeId, result.startTime, result.stopTime, result.timeAdjustmentType);
                else
                    this.saveShifts(Guid.newGuid().toString(), result.shifts, true, false, false, 0);
            } else if (result.reload && result.reload === true) {
                this.reloadShiftsForSpecifiedEmployeeIds = result.reloadEmployeeIds;
                this.loadData('openEditAssignmentDialog');
                if (this.isOrderPlanningMode)
                    this.loadAllUnscheduledOrders();
            }
        });
    }

    private openEditOrder(shift: ShiftDTO, date: Date, employeeId: number) {
        if (!shift) {
            // New order, first create a shift to hold it
            this.getTimeScheduleEmployeePeriodId(employeeId, date).then((x: number) => {
                if (x != 0)
                    this.openEditOrderDialog(null, date, employeeId);
            });
        } else {
            this.openEditOrderDialog(shift, date, employeeId);
        }
    }

    private openEditOrderDialog(shift: ShiftDTO, date: Date, employeeId: number) {
        let label = '';
        if (shift?.order)
            label = this.terms["common.order"] + " " + (shift.order.orderNr ? shift.order.orderNr.toString() : '');
        else
            label = this.terms["time.schedule.planning.contextmenu.neworder"];

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Orders/Views/edit.html"),
            controller: EditOrderController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, id: shift && shift.order ? shift.order.orderId : 0, label: label, orderPlanning: true });
        });

        modal.result.then(result => {
            if (result.invoiceId) {
                this.loadUnscheduledOrder(result.invoiceId).then(() => {
                    let order = this.allUnscheduledOrders.find(o => o.orderId === result.invoiceId);
                    if (order) {
                        this.messagingService.publish('editOrderDone', order);

                        if (!shift) {
                            // New order created, create a shift to hang it on
                            let newShift: ShiftDTO = new ShiftDTO(TermGroup_TimeScheduleTemplateBlockType.Order);
                            newShift.order = order;
                            newShift.employeeId = employeeId;
                            newShift.shiftTypeId = order.shiftTypeId;
                            newShift.shiftTypeName = order.shiftTypeName;
                            newShift.actualStartTime = date;  // TODO: In day view, keep time from slot
                            this.openEditAssignment(newShift, date, employeeId);
                        }
                    }
                });
            }
        });
    }

    private openOrder(order: OrderListDTO) {
        let label = order ? this.terms["common.order"] + " " + order.orderNr : this.terms["time.schedule.planning.contextmenu.neworder"];

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Orders/Views/edit.html"),
            controller: EditOrderController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, label: label, id: order ? order.orderId : 0, orderPlanning: true });
        });

        modal.result.then(result => {
            if (result.invoiceId) {
                this.loadUnscheduledOrder(result.invoiceId).then(() => { });
            }
        });
    }

    public openEditBooking(shift: ShiftDTO, date: Date = null, employeeId: number = null) {
        if (!employeeId && shift)
            employeeId = shift.employeeId;
        if (!date && shift)
            date = shift.startTime;

        // Read only
        let readOnly = false;
        if (shift)
            readOnly = (shift.isReadOnly || shift.isAbsence || shift.isAbsenceRequest);

        if (!shift) {
            shift = new ShiftDTO(TermGroup_TimeScheduleTemplateBlockType.Booking);
            shift.employeeId = employeeId;
            shift.startTime = shift.actualStartTime = date;
            shift.stopTime = shift.actualStopTime = date;
            shift.link = Guid.newGuid();
        }

        // Show edit order dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/EditBooking/Views/editBooking.html"),
            controller: EditBookingController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                isReadonly: () => { return readOnly },
                modifyPermission: () => { return this.modifyPermission },
                shift: () => { return shift },
                date: () => { return date },
                employeeId: () => { return employeeId },
                shiftTypes: () => { return this.getBookingShiftTypes(); },
                employees: () => { return this.employedEmployees; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                dayStartTime: () => { return this.dayViewStartTime; },
                dayEndTime: () => { return this.dayViewEndTime; },
                shiftTypeMandatory: () => { return this.shiftTypeMandatory; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.save && result.save === true) {
                this.saveShifts(Guid.newGuid().toString(), result.shifts, false, false, false, 0);
            } else if (result.reload && result.reload === true) {
                this.reloadShiftsForSpecifiedEmployeeIds = result.reloadEmployeeIds;
                this.loadData('openEditBooking');
            }
        });
    }

    private openEditLeisureCode(shift: ShiftDTO, date: Date = null, employeeId: number = null) {
        if (!employeeId && shift)
            employeeId = shift.employeeId;
        if (!date && shift)
            date = shift.startTime;

        // Read only
        let readOnly = false;
        if (shift)
            readOnly = shift.isReadOnly;

        if (!shift) {
            shift = new ShiftDTO(TermGroup_TimeScheduleTemplateBlockType.Schedule);
            shift.employeeId = employeeId;
            shift.timeLeisureCodeId = this.leisureCodes[0].timeLeisureCodeId;
            shift.startTime = shift.actualStartTime = date;
            shift.stopTime = shift.actualStopTime = date;
            shift.link = Guid.newGuid();
        }

        // Show edit leisure code dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/EditLeisureCode/Views/editLeisureCode.html"),
            controller: EditLeisureCodeController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                isReadonly: () => { return readOnly },
                modifyPermission: () => { return this.modifyPermission },
                shift: () => { return shift },
                leisureCodes: () => { return this.leisureCodes; },
                skipWorkRules: () => { return this.selectableInformationSettings.skipWorkRules; },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.save && result.save === true && result.shift) {
                this.startSave();
                this.scheduleService.saveTimeScheduleEmployeePeriodDetail(result.shift).then(res => {
                    this.reloadShiftsForSpecifiedEmployeeIds = [employeeId];
                    this.loadData('openEditLeisureCode');
                });
            } else if (result?.reload && result.reload === true) {
                this.reloadShiftsForSpecifiedEmployeeIds = [employeeId];
                this.loadData('openEditLeisureCode');
            }
        });
    }

    private openDeleteLeisureCode(shifts: ShiftDTO[]) {
        if (shifts.length === 0)
            return;

        // Show edit order dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DeleteLeisureCode/Views/deleteLeisureCode.html"),
            controller: DeleteLeisureCodeController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                shifts: () => { return shifts },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result?.selectedShifts) {
                this.startDelete()
                this.scheduleService.deleteTimeScheduleEmployeePeriodDetail((result.selectedShifts as ShiftDTO[]).map(s => s.timeScheduleEmployeePeriodDetailId)).then(res => {
                    this.reloadShiftsForSpecifiedEmployeeIds = [shifts[0].employeeId];
                    this.loadData('openEditLeisureCode');
                });
            }
        });
    }

    private openDialogPrintOrSendEmploymentCertificate(employees: EmployeeListDTO[], enableSendMode: boolean) {

        // Show dialog for print/send EmploymentCertificate
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/PrintEmploymentCertificate/Views/printEmploymentCertificate.html"),
            controller: PrintEmploymentCertificateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                title: () => { return enableSendMode ? this.terms["time.schedule.planning.employee.contextmenu.sendemploymentcertificate"] : this.terms["time.schedule.planning.employee.contextmenu.printemploymentcertificate"] },
                dateFrom: () => { return this.dateFrom },
                dateTo: () => { return this.dateTo },
                inputEmployees: () => { return employees },
                reportId: () => { return this.employmentContractShortSubstituteReportId },
                reportName: () => { return this.employmentContractShortSubstituteReportName },
                isSendMode: () => { return enableSendMode },
                hasEmployeeTemplates: () => { return this.hasEmployeeTemplates }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            console.log(result);
        });
    }

    private printEmploymentCertificateForEmployee(employee: EmployeeListDTO) {
        this.openDialogPrintOrSendEmploymentCertificate([employee], false)
    }

    private sendEmploymentCertificateForEmployee(employee: EmployeeListDTO) {
        this.openDialogPrintOrSendEmploymentCertificate([employee], true)
    }

    private printEmploymentCertificateForEmployees(employees: EmployeeListDTO[]) {
        this.openDialogPrintOrSendEmploymentCertificate(employees, false)
    }

    private sendEmploymentCertificateForEmployees(employees: EmployeeListDTO[]) {
        this.openDialogPrintOrSendEmploymentCertificate(employees, true)
    }

    private printScheduleForEmployees(employeeIds: number[], templateTypes: SoeReportTemplateType[]) {
        if (!employeeIds || employeeIds.length === 0)
            return;

        // Set default report based on current view
        let defaultReportId: number = 0;
        if (this.isDayView)
            defaultReportId = this.dayScheduleReportId;
        else if (this.isScheduleView)
            defaultReportId = this.weekScheduleReportId;
        else if (this.isTemplateDayView)
            defaultReportId = this.dayTemplateScheduleReportId;
        else if (this.isTemplateScheduleView)
            defaultReportId = this.weekTemplateScheduleReportId;
        else if (this.isEmployeePostDayView)
            defaultReportId = this.dayEmployeePostTemplateScheduleReportId;
        else if (this.isEmployeePostScheduleView)
            defaultReportId = this.weekEmployeePostTemplateScheduleReportId;
        else if (this.isScenarioDayView)
            defaultReportId = this.dayScenarioScheduleReportId;
        else if (this.isScenarioScheduleView || this.isScenarioCompleteView)
            defaultReportId = this.weekScenarioScheduleReportId;
        else if (this.isStandbyDayView)
            defaultReportId = this.dayScheduleReportId;
        else if (this.isStandbyScheduleView)
            defaultReportId = this.weekScheduleReportId;

        let excludeAbsence = this.selectedStatuses.find(s => s.id === PlanningStatusFilterItems.HideAbsenceApproved) != undefined;

        this.openPrintDialog(templateTypes, defaultReportId).then(result => {
            if (result && result.reportId) {
                let includeVacant = _.intersection(employeeIds, this.vacantEmployeeIds).length > 0;
                let includeHidden = employeeIds.includes(this.hiddenEmployeeId);
                let includeSecondary = this.showSecondaryCategories || this.showSecondaryAccounts;
                let accountIds = this.useAccountHierarchy ? this.getFilteredAccountIds() : null;
                let accountingType = TermGroup_EmployeeSelectionAccountingType.TimeScheduleTemplateBlock;
                let isEmployeePost = this.isEmployeePostDayView || this.isEmployeePostScheduleView;

                if (this.isTemplateView)
                    accountingType = this.useAccountHierarchy ? TermGroup_EmployeeSelectionAccountingType.EmployeeAccount : TermGroup_EmployeeSelectionAccountingType.EmployeeCategory;

                this.reportDataService.createReportJob(ReportJobDefinitionFactory.createSimpleScheduleReportDefinition(result.reportId, result.reportType, employeeIds, this.dateFrom, this.dateTo, this.getFilteredShiftTypeIds(), this.timeScheduleScenarioHeadId, null, includeVacant, includeHidden, includeSecondary, accountIds, accountingType, isEmployeePost, excludeAbsence), true);
            }
        });
    }

    private exportScheduleToExcelWithDialog() {
        this.openPrintDialog([SoeReportTemplateType.Generic], 0).then(result => {
            this.exportScheduleToExcel();
        });
    }

    private exportScheduleToExcel(showProgress: boolean = true) {
        if (showProgress) {
            this.translationService.translate("time.schedule.planning.exportingtoexcel").then(term => {
                this.notificationService.showDialogEx(this.terms["core.exportexcel"], term, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, { autoCloseDelay: this.autoCloseModalDelay }).result.then(val => {
                }, (reason) => {
                    // Prevent unhandled rejection
                });
            });
        }

        let shiftsToExport: ShiftDTO[] = [];
        let employees: EmployeeListDTO[] = this.employedEmployees;
        employees.forEach(emp => {
            if (this.isCommonDayView) {
                shiftsToExport.push(...this.getShifts(emp.identifier, this.dates[0].date, emp['accountId'], true));
            } else {
                this.dates.map(d => d.date).forEach(date => {
                    shiftsToExport.push(...this.getShifts(emp.identifier, date, emp['accountId'], true));
                });
            }
        });

        shiftsToExport.forEach(shift => {
            shift.startTime = shift.actualStartTime;
            shift.stopTime = shift.actualStopTime;
        });

        // Set options/selections for export
        let showContactInfo: boolean = false;
        for (let employee of employees.filter(e => !e.isGroupHeader)) {
            if (this.scheduleHandler.showingContactInfo(employee.employeeId)) {
                showContactInfo = true;
                break;
            }
        }

        let selections: SelectionCollection = new SelectionCollection();
        selections.upsert('showEmployeeGroup', new BoolSelectionDTO(this.selectableInformationSettings.showEmployeeGroup));
        selections.upsert('showContactInfo', new BoolSelectionDTO(showContactInfo));

        if (employees.filter(e => e.isGroupHeader).length === 0) {
            let newEmp: EmployeeListDTO = new EmployeeListDTO();
            newEmp.isGroupHeader = true;
            newEmp.groupName = newEmp.name = newEmp.firstName = this.terms["core.others"];
            newEmp.lastName = '';
            newEmp.hidden = false;
            newEmp.vacant = false;
            newEmp.isVisible = true;
            this.employedEmployees.push(newEmp);

            employees.forEach(employee => {
                employee.groupName = newEmp.name + '__';
            });
        }

        this.scheduleService.exportShiftsToExcel(shiftsToExport, employees.filter(e => e.isVisible), this.dates.map(d => d.date), selections.materialize()).then(result => {
            if (result.success) {
                // Download file when created
                this.messagingService.publish(Constants.EVENT_SHOW_REPORT_MENU_ONLY_DOWNLOAD, { reportPrintoutId: result.success && result.integerValue ? result.integerValue : null });
            } else {
                this.notificationService.showDialogEx(this.terms["error.default_error"], result.errorMessage, SOEMessageBoxImage.Error);
            }
        });
    }

    private openPrintDialog(templateTypes: SoeReportTemplateType[], defaultReportId: number): ng.IPromise<any> {
        let deferral = this.$q.defer();

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
            controller: SelectReportController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => { return SoeModule.Time },
                reportTypes: () => { return templateTypes },
                showCopy: () => { return false },
                showEmail: () => { return false },
                copyValue: () => { return false },
                reports: () => { return null },
                defaultReportId: () => { return defaultReportId },
                langId: () => { return null },
                showReminder: () => { return false },
                showLangSelection: () => { return false },
                showSavePrintout: () => { return false },
                savePrintout: () => { return false }
            }
        }

        this.$uibModal.open(options).result.then(result => {
            deferral.resolve(result);
        });

        return deferral.promise;
    }

    private evaluateAllWorkRules() {
        this.startWork("time.schedule.planning.evaluateworkrules.executing");

        this.visibleShifts.forEach(s => {
            s.setTimesForSave();
        });

        this.scheduleService.evaluateAllWorkRules(this.visibleShifts, this.getVisibleEmployeeIds(), this.dateFrom, this.dateTo, this.isTemplateView, null, this.timeScheduleScenarioHeadId, this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.startDate : null, this.currentPlanningPeriodChildInRangeExact ? this.planningPeriodChild.stopDate : null).then(result => {
            if (result.result.success) {
                const prevShowWorkRuleViolations = this.showWorkRuleViolations;

                if (result.evaluatedRuleResults.length === 0) {
                    if (this.showWorkRuleViolations)
                        this.toggleShowWorkRuleViolations(true);
                    this.translationService.translate("time.schedule.planning.evaluateworkrules.noviolations").then(term => {
                        this.completedWork(null, false, term);
                    });
                } else {
                    this.completedWork(null, true);
                    this.workRuleViolations = _.sortBy(result.evaluatedRuleResults.map(r => {
                        let obj = new EvaluateAllWorkRulesResultDTO();
                        angular.extend(obj, r);

                        // Set employee info
                        const employee = this.employees.find(e => e.id === obj.employeeId);
                        if (employee) {
                            obj.label = employee.label;
                            obj.sort = employee.sort;
                        }

                        return obj;
                    }), 'sort');
                    if (!this.showWorkRuleViolations)
                        this.toggleShowWorkRuleViolations(true);
                }

                // If list of violations has been opened or closed,
                // we need to rerender schedule, due to change in calendar width.
                if (prevShowWorkRuleViolations !== this.showWorkRuleViolations) {
                    this.setDateColumnWidth().then(() => {
                        this.scheduleHandler.updateWidthOnAllElements();
                    });
                }
            } else {
                this.failedWork(result.result.errorMessage);
            }

            this.visibleShifts.forEach(s => {
                s.resetTimesForSave();
            });

        }).catch(reason => {
            this.notificationService.showServiceError(reason);
            this.completedWork(null, true);
        });
    }

    private showWorkRuleViolationsForEmployee(employee: EvaluateAllWorkRulesResultDTO) {
        this.notificationService.showDialogEx(this.terms["core.info"], employee.violations.join('<br/>'), SOEMessageBoxImage.Forbidden);
    }

    private removeWorkRuleViolationFromList(employee: EvaluateAllWorkRulesResultDTO) {
        _.pull(this.workRuleViolations, employee);

        if (this.workRuleViolations.length === 0)
            this.hideWorkRuleViolations();
    }

    private hideWorkRuleViolations() {
        if (this.showWorkRuleViolations) {
            this.toggleShowWorkRuleViolations(true);
        }
    }

    // HELP-METHODS

    private setDateRange(loadData: boolean): boolean {
        if (!this.dateFrom)
            return false;

        if (loadData && !this.viewPermissionsLoaded)
            loadData = false;

        this.recalculateEmployeeWorkTimes = true;

        // Set date to
        if (this.isCalendarView) {
            if (this.nbrOfVisibleWeeks === 0)
                return false;

            this.dateTo = this.dateFrom.addWeeks(this.nbrOfVisibleWeeks).addDays(-1);
        } else if (this.isCommonDayView) {
            if (this.nbrOfVisibleHours === 0)
                return false;

            if (!this.dateFrom.isSameHourAs(this.dateFrom.beginningOfDay().addHours(this.startHour))) {
                this.dateFrom = this.dateFrom.beginningOfDay().addHours(this.startHour);
                return;
            }

            this.dateTo = (this.dateFrom.addHours(this.nbrOfVisibleHours).addSeconds(-1)).endOfHour();
        } else if (this.isCommonScheduleView) {
            if (this.nbrOfVisibleDays === 0)
                return false;

            if (this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Year) {
                // If year is selected, set date range to current year
                this.dateFrom = this.dateFrom.beginningOfYear();
                this.dateTo = this.dateFrom.endOfYear();
                loadData = false;
                this.clearShifts();
                this.calculateTimes();
                this.renderBody('setDateRange year');
                // TODO: Load annual work time
            } else {
                if (this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Custom) {
                    this.clearShifts();
                } else {
                    // Do not set date to if custom days are selected
                    this.dateFrom = this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Day ? this.dateFrom : this.dateFrom.beginningOfWeek();
                    let newDateTo = this.dateFrom.addDays(this.nbrOfVisibleDays - 1).endOfDay();
                    if (!this.dateTo || this.dateTo.diffDays(newDateTo) !== 0)
                        this.dateTo = newDateTo;
                }
            }

            // Check if any date is actually changed
            if (this.dates && this.dates.length > 0) {
                if (!loadData && this.dates[0].date.isSameMinuteAs(this.dateFrom) && this.dates[this.dates.length - 1].date.isSameMinuteAs(this.dateTo))
                    return false;
            }
        }

        // If show planned time for cycle is checked, uncheck if if not showing week
        if (this.selectableInformationSettings.showCyclePlannedTime && this.selectedVisibleDays !== TermGroup_TimeSchedulePlanningVisibleDays.Week)
            this.selectableInformationSettings.showCyclePlannedTime = false;

        // If year has changed, reload employees to get employments etc. for new year
        let yearChanged: boolean = false;
        if (this.currentYearFrom && this.currentYearTo && (this.currentYearFrom !== this.dateFrom.year() || this.currentYearTo !== this.dateTo.year())) {
            yearChanged = true;
            this.loadEmployees().then(() => {
                if (this.isEmployeePostView)
                    this.loadEmployeePosts(true);
            });
        } else {
            this.setEmployedEmployees();

            if (this.viewPermissionsLoaded) {
                let abortLoad = this.filterEmployees('setDateRange', !loadData);
                if (abortLoad && loadData) {
                    loadData = false;
                    this.showEmployeeRemovedFromFilterMessage();
                }
            }
        }

        this.currentYearFrom = this.dateFrom.year();
        this.currentYearTo = this.dateTo.year();

        let tmpDates: DateDay[] = [];
        let cols = 0;
        switch (this.viewDefinition) {
            case TermGroup_TimeSchedulePlanningViews.Calendar:
                cols = 7 * this.nbrOfVisibleWeeks;
                break;
            case TermGroup_TimeSchedulePlanningViews.Day:
            case TermGroup_TimeSchedulePlanningViews.TemplateDay:
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsDay:
            case TermGroup_TimeSchedulePlanningViews.ScenarioDay:
            case TermGroup_TimeSchedulePlanningViews.StandbyDay:
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesDay:
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsDay:
                cols = this.nbrOfVisibleHours * this.hourParts;
                break;
            case TermGroup_TimeSchedulePlanningViews.Schedule:
            case TermGroup_TimeSchedulePlanningViews.TemplateSchedule:
            case TermGroup_TimeSchedulePlanningViews.EmployeePostsSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioSchedule:
            case TermGroup_TimeSchedulePlanningViews.ScenarioComplete:
            case TermGroup_TimeSchedulePlanningViews.StandbySchedule:
            case TermGroup_TimeSchedulePlanningViews.TasksAndDeliveriesSchedule:
            case TermGroup_TimeSchedulePlanningViews.StaffingNeedsSchedule:
                if (this.selectedVisibleDays === TermGroup_TimeSchedulePlanningVisibleDays.Year)
                    cols = 1;
                else
                    cols = this.nbrOfVisibleDays;
                break;
        }

        for (var i: number = 0; i < cols; i++) {
            let dateDay: DateDay = this.isCommonDayView ? new DateDay(this.dateFrom.addMinutes(i * 60 / this.hourParts)) : new DateDay(this.dateFrom.addDays(i));
            this.setDateRangeNbrOfDays(dateDay);
            this.setDateRangeDayTypes(dateDay);

            tmpDates.push(dateDay);
        }

        this.nbrOfColumnsChanged = (!this.dates || this.dates.length !== tmpDates.length);
        this.dates = tmpDates;
        this.setDateColumnWidth(0);
        this.dateToChanged = false;

        this.loadCurrentPlanningPeriod(false, yearChanged);
        this.loadAnnualLeaveBalance = true;

        if (loadData) {
            if (this.firstLoadHasOccurred) {
                if (this.useAccountHierarchy) {
                    this.loadAccountsByUserFromHierarchy().then(() => {
                        if (this.selectableInformationSettings.doNotSearchOnFilter)
                            this.filteredButNotLoaded = true;
                        else
                            this.loadData('setDateRange useAccountHierarchy', true);
                    });
                } else {
                    if (this.selectableInformationSettings.doNotSearchOnFilter)
                        this.filteredButNotLoaded = true;
                    else
                        this.loadData('setDateRange', true);
                }
            }

            // Reload date range specific data
            this.getHasStaffingByEmployeeAccount();
            this.loadUnscheduledTasksAndDeliveriesDates();
            this.loadAllUnscheduledOrders();
            this.loadHolidays();
            this.loadScheduleEventDates();
            if (this.showStaffingNeedsAgChart || this.showPlanningFollowUpTable) {
                this.planningFollowUpTableData = [];
                this.staffingNeedData = [];
            }
        }

        return true;
    }

    private setDateRangeNbrOfDays(dateDay: DateDay) {
        let days: number = 1;
        if (this.selectedVisibleDays !== TermGroup_TimeSchedulePlanningVisibleDays.Year) {
            let startDate = (dateDay.date.beginningOfWeek().isBeforeOnDay(this.dateFrom)) ? this.dateFrom : dateDay.date.beginningOfWeek();
            let stopDate = (dateDay.date.endOfWeek().isAfterOnDay(this.dateTo)) ? this.dateTo : dateDay.date.endOfWeek();
            days = stopDate.diffDays(startDate) + 1;
        }

        dateDay.rangeNbrOfDays = days;
    }

    private setDateRangeDayTypes(dateDay: DateDay) {
        dateDay.isToday = this.isToday(dateDay.date);
        dateDay.isSaturday = this.isSaturday(dateDay.date);
        dateDay.isSunday = this.isSunday(dateDay.date);
    }

    private iso8601Week = function (calStartDate) {
        let startDate: any;
        let currentDate: any
        currentDate = calStartDate;
        startDate = new Date(currentDate.getFullYear(), 0, 1);
        let days = Math.floor((currentDate - startDate) / (24 * 60 * 60 * 1000));

        return Math.ceil(days / 7);
    };

    private getDateRangeText(date: Date): string {
        if (!this.terms)
            return '';

        let text = '';

        if (this.isCalendarView) {
            if (this.dateFrom && this.dateTo) {
                var wkFrom = 0;
                var wkTo = 0;
                if (soeConfig.language == "fi-FI") {
                    wkFrom = moment(moment(this.dateFrom).locale('fi').format('YYYY-MM-DD')).week();
                    wkTo = moment(moment(this.dateTo).locale('fi').format('YYYY-MM-DD')).week();
                } else {
                    wkFrom = moment(this.dateFrom.toLocaleDateString(soeConfig.language)).week();
                    wkTo = moment(this.dateTo.toLocaleDateString(soeConfig.language)).week();
                }
                text = "{0} - {1}, {2} {3}-{4}".format(
                    this.dateFrom.format('dddd D MMMM'),
                    this.dateTo.format('dddd D MMMM'),
                    this.terms["common.week"].toLocaleLowerCase(),
                    wkFrom.toString(),
                    wkTo.toString());
            }
        } else if (this.isCommonDayView) {
            if (this.dateFrom && this.dateTo) {
                if (this.dateFrom.isBeginningOfDay() && this.dateTo.isEndOfDay()) {
                    text = "{0}, {1} {2}".format(
                        this.dateFrom.format('dddd D MMMM'),
                        this.terms["common.week"].toLocaleLowerCase(),
                        this.dateFrom.format('W'));
                } else if (this.dateFrom.isSameDayAs(this.dateTo)) {
                    text = "{0} {1}-{2}, {3} {4}".format(
                        this.dateFrom.format('dddd D MMMM'),
                        this.dateFrom.toFormattedTime(),
                        this.dateTo.addSeconds(1).toFormattedTime(),
                        this.terms["common.week"].toLocaleLowerCase(),
                        this.dateFrom.format('W'));
                } else {
                    text = "{0} {1} - {2} {3}, {4} {5}".format(
                        this.dateFrom.format('dddd D MMMM'),
                        this.dateFrom.toFormattedTime(),
                        this.dateTo.addSeconds(1).format('dddd D MMMM'),
                        this.dateTo.addSeconds(1).toFormattedTime(),
                        this.terms["common.week"].toLocaleLowerCase(),
                        this.dateFrom.format('W'));
                }
            }
        } else if (this.isCommonScheduleView) {
            if (date) {
                let startDate = this.dateFrom;
                let stopDate = this.dateTo;

                if (this.selectedVisibleDays !== TermGroup_TimeSchedulePlanningVisibleDays.Year) {
                    startDate = (date.beginningOfWeek().isBeforeOnDay(this.dateFrom)) ? this.dateFrom : date.beginningOfWeek();
                    stopDate = (date.endOfWeek().isAfterOnDay(this.dateTo)) ? this.dateTo : date.endOfWeek();
                }

                if (this.dateFrom && this.dateTo) {
                    if (this.nbrOfVisibleWeeks > 10) {
                        text = "{0} {1}".format(this.terms["common.weekshort"].toLocaleLowerCase(), date.format('W'));
                    } else {
                        text = startDate.format(this.nbrOfVisibleWeeks > 6 ? 'D/M' : (this.nbrOfVisibleWeeks > 3 ? 'ddd D MMM' : 'dddd D MMMM'));
                        if (!startDate.isSameDayAs(stopDate))
                            text += " - {0}".format(stopDate.format(this.nbrOfVisibleWeeks > 6 ? 'D/M' : (this.nbrOfVisibleWeeks > 3 ? 'ddd D MMM' : 'dddd D MMMM')));

                        text += ", {0} {1}".format(this.terms[this.nbrOfVisibleWeeks > 3 ? "common.weekshort" : "common.week"].toLocaleLowerCase(), date.format('W'));
                    }
                }
            }
        }

        return text;
    }

    public setDateColumnWidth(delay: number = 100): ng.IPromise<any> {
        return this.$timeout(() => {
            let elem = $('.planning-scheduleview-table');
            if (elem.length)
                this.dateColumnWidth = (elem.width() - 230) / (this.dates.length > 0 ? this.dates.length : 1);

            // 230 = Width of two first columns (employee = 200) and to -15px margins on planning-scheduleview div
        }, delay);
    }

    private getDateText(date: Date): string {
        if (!date || !this.terms)
            return '';

        let text = '';
        let showingIcon = (this.showUnscheduledTasksPermission && this.unscheduledTaskDates.length > 0);

        if (this.nbrOfVisibleWeeks > 4)
            text = date.format('dd').left(1).toUpperCase();
        else if ((this.nbrOfVisibleWeeks > 2 && showingIcon) || this.nbrOfVisibleWeeks > 3)
            text = date.format('ddd D');
        else
            text = date.format('dddd D');

        return text;
    }

    private dateHasPassed(date: Date): boolean {
        return date.isBeforeOnDay(new Date());
    }

    private getDayName(day: number): string {
        return CalendarUtility.getDayName(day);
    }

    private createSlot(formattedDate: string, employeeId?: number): SlotDTO {
        let slot: SlotDTO = new SlotDTO();
        slot.startTime = formattedDate.parsePipedDateTime();
        slot.stopTime = this.isCommonDayView ? slot.startTime.addMinutes(60 / this.hourParts) : slot.startTime.endOfDay();
        slot.employeeId = employeeId;

        return slot;
    }

    public setSlotReadOnly(slot: SlotDTO) {
        if (this.isEmployeeInactive(slot.employeeId) || (this.isScenarioView && !this.isInsideScenario(slot.startTime)))
            slot.isReadOnly = true;

        // Check if slot is on a recurring week
        if (this.isTemplateScheduleView) {
            let template = this.templateHelper.getTemplateSchedule(slot.employeeId, slot.startTime, false);
            if (template) {
                let range = this.getTemplateVisibleRange(template);
                if (slot.startTime.isAfterOnDay(range.stop))
                    slot.isReadOnly = true;
            }
        }
    }

    public isInsideScenario(date: Date): boolean {
        return this.isScenarioView && this.scenarioHead && date.isSameOrAfterOnDay(this.scenarioHead.dateFrom) && date.isSameOrBeforeOnDay(this.scenarioHead.dateTo);
    }

    private adjustDayViewTimes(adjustOriginalEndTime: boolean) {
        // Make sure start time is a whole hour
        if (this.dayViewStartTime % 60 !== 0)
            this.dayViewStartTime -= (60 - this.dayViewStartTime % 60);

        if (this.dayViewEndTime === 0)
            this.dayViewEndTime = (24 * 60);

        // Make sure end time is a whole hour
        if (this.dayViewEndTime % 60 !== 0)
            this.dayViewEndTime += (60 - this.dayViewEndTime % 60);

        // Make sure we only show max 24 hours in day view
        if (this.dayViewEndTime - this.dayViewStartTime > (24 * 60))
            this.dayViewEndTime = this.dayViewStartTime + (24 * 60);

        if (adjustOriginalEndTime)
            this.originalDayViewEndTime = this.dayViewEndTime;
    }

    private setShiftTypes() {
        if (!this.allShiftTypes || !this.shiftTypeIds)
            return;

        this.shiftTypes = [];
        let filteredAccountIds = this.getFilteredAccountIds();
        let usingShiftTypeHierarchyAccounts = this.useAccountHierarchy && _.some(this.allShiftTypes, s => s.hierarchyAccounts && s.hierarchyAccounts.length > 0);

        this.allShiftTypes.filter(s => this.shiftTypeIds.includes(s.shiftTypeId) || s.shiftTypeId === 0).forEach(shiftType => {
            let isValid = true;
            // Shift type "Not selected" should always be visible
            if (this.isFilteredOnAccountDim() && shiftType.shiftTypeId > 0) {
                let isValidHierarchy = false;
                if (this.shiftTypeAccountDim || usingShiftTypeHierarchyAccounts) {
                    // Linked to account dim
                    if (this.shiftTypeAccountDim && shiftType.accountId && filteredAccountIds.includes(shiftType.accountId))
                        isValidHierarchy = true;

                    if (usingShiftTypeHierarchyAccounts) {
                        // Hierarcy accounts, none selected
                        if (!isValidHierarchy && !this.shiftTypeAccountDim && (!shiftType.hierarchyAccounts || shiftType.hierarchyAccounts.length === 0))
                            isValidHierarchy = true;

                        // Hierarcy accounts, at least one valid account selected
                        if (!isValidHierarchy && shiftType.hierarchyAccounts && shiftType.hierarchyAccounts.length > 0 && _.intersection(filteredAccountIds, shiftType.hierarchyAccounts.map(a => a.accountId)).length > 0)
                            isValidHierarchy = true;
                    }

                    if (!isValidHierarchy)
                        isValid = false;
                } else if (shiftType.accountingSettings) {
                    // Get shift type account for each account dim
                    for (let accountDim of this.accountDims) {
                        if (this.isFilteredOnAccountDimNr(accountDim.accountDimNr)) {
                            let accountDimAccounts = accountDim.selectedAccounts.map(a => a.accountId);
                            if (accountDimAccounts) {
                                let shiftTypeAccountId = shiftType.accountingSettings.getAccountId(accountDim.accountDimNr);
                                if (shiftTypeAccountId && !accountDimAccounts.includes(shiftTypeAccountId)) {
                                    isValid = false;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (isValid) {
                this.shiftTypes.push({ id: shiftType.shiftTypeId, label: shiftType.name, timeScheduleTypeId: shiftType.timeScheduleTypeId, timeScheduleTemplateBlockType: shiftType.timeScheduleTemplateBlockType });
                if (!this.showSkills && shiftType.shiftTypeSkills && shiftType.shiftTypeSkills.length > 0)
                    this.showSkills = true;
            }
        });
    }

    private getBookingShiftTypes() {
        var bookingShiftTypes: any[] = [];
        this.allShiftTypes.filter(s => s.timeScheduleTemplateBlockType === TermGroup_TimeScheduleTemplateBlockType.Booking).forEach(shiftType => {
            bookingShiftTypes.push({ id: shiftType.shiftTypeId, label: shiftType.name, timeScheduleTypeId: shiftType.timeScheduleTypeId, defaultLength: shiftType.defaultLength });
        });

        return bookingShiftTypes;
    }

    private setColorByBackground(backgroundColor) {
        return GraphicsUtility.foregroundColorByBackgroundBrightness(backgroundColor);
    }

    private get showStatusFilter(): boolean {
        return !this.isTemplateView && !this.isTasksAndDeliveriesView && !this.isStaffingNeedsView;
    }

    private get showBlockTypesFilter(): boolean {
        return !this.isTasksAndDeliveriesView && !this.isStaffingNeedsView;
    }

    private get showDeviationCauseFilter(): boolean {
        let statusIds = this.selectedStatuses.map(s => s.id);
        return this.isAdmin && this.isFilteredOnStatus && (statusIds.includes(PlanningStatusFilterItems.AbsenceRequested) || statusIds.includes(PlanningStatusFilterItems.AbsenceApproved));
    }

    public renderedShifts: { employeeId: number, shiftIds: number[] }[] = [];
    public renderedLeisureCodes: { employeeId: number, detailIds: number[] }[] = [];
    public getShifts(employeeId: number, date: Date, accountId: number, ignoreRendered?: boolean): ShiftDTO[] {
        if (!this.employeeShiftsMap)
            return [];

        const key = this.createMapDateKey(date);

        const employeeShifts = this.employeeShiftsMap.get(employeeId);
        if (!employeeShifts) {
            return [];
        }

        let shiftsForDate = (employeeShifts.get(key) || []).filter(s => !s.isZeroShift && !s.isLeisureCode);
        let leisureCodesForDate = (employeeShifts.get(key) || []).filter(s => s.isLeisureCode);

        // Filter shifts for hidden employee on account
        if (this.useAccountHierarchy && employeeId === this.hiddenEmployeeId && this.isFilteredOnAccountDim()) {
            let filteredAccountIds = this.getFilteredAccountIds();
            const hasFilteredAccount = (shift: ShiftDTO) => filteredAccountIds.includes(shift.accountId);
            shiftsForDate = shiftsForDate.filter(s => hasFilteredAccount(s));
        } else if (accountId) {
            shiftsForDate = shiftsForDate.filter(s => s.accountId == accountId);
        }

        const employee = this.isEmployeePostView ? this.getEmployeePostById(employeeId) : this.getEmployeeById(employeeId);
        const hasEmployment = this.isTemplateView || employee.hasEmployment(date, date);
        const filterOnBlockType = this.isOrderPlanningMode && this.isFilteredOnBlockType;

        const includeIfVisible = (shift: ShiftDTO) => hasEmployment && shift.isVisible;
        const includeIfHidden = (shift: ShiftDTO) => {
            const canDisplayHiddenShift = this.selectableInformationSettings.showHiddenShifts && !shift.isVisible;
            if (!canDisplayHiddenShift)
                return false;

            return !filterOnBlockType || this.selectedBlockTypes.find(type => type.id === shift.type);
        };

        // Shifts
        let renderedShiftsForEmployee;
        if (!ignoreRendered) {
            renderedShiftsForEmployee = this.renderedShifts.find(r => r.employeeId === employeeId);
            if (!renderedShiftsForEmployee) {
                renderedShiftsForEmployee = { employeeId: employeeId, shiftIds: [] };
                this.renderedShifts.push(renderedShiftsForEmployee);
            }
        }

        const nonRenderedShifts = ignoreRendered ? shiftsForDate : shiftsForDate.filter(s => renderedShiftsForEmployee.shiftIds.indexOf(s.timeScheduleTemplateBlockId) < 0);
        let shiftsToDisplay = nonRenderedShifts.filter(s => includeIfVisible(s));
        if (shiftsToDisplay.length > 0 || this.employeeHasVisibleShifts(employeeId) || this.selectableInformationSettings.showFullyLendedEmployees)
            shiftsToDisplay = shiftsToDisplay.concat(nonRenderedShifts.filter(s => includeIfHidden(s)));

        if (!ignoreRendered)
            renderedShiftsForEmployee.shiftIds.push(...shiftsToDisplay.map(s => s.timeScheduleTemplateBlockId));

        // Leisure codes
        let renderedLeisureCodesForEmployee;
        if (!ignoreRendered) {
            renderedLeisureCodesForEmployee = this.renderedLeisureCodes.find(r => r.employeeId === employeeId);
            if (!renderedLeisureCodesForEmployee) {
                renderedLeisureCodesForEmployee = { employeeId: employeeId, detailIds: [] };
                this.renderedLeisureCodes.push(renderedLeisureCodesForEmployee);
            }
        }

        const nonRenderedLeisureCodes = ignoreRendered ? leisureCodesForDate : leisureCodesForDate.filter(s => renderedLeisureCodesForEmployee.detailIds.indexOf(s.timeScheduleEmployeePeriodDetailId) < 0);
        let leisureCodesToDisplay = nonRenderedLeisureCodes.filter(s => includeIfVisible(s));
        if (leisureCodesToDisplay.length > 0)
            leisureCodesToDisplay = leisureCodesToDisplay.concat(nonRenderedLeisureCodes.filter(s => includeIfHidden(s)));

        if (!ignoreRendered)
            renderedLeisureCodesForEmployee.detailIds.push(...leisureCodesToDisplay.map(s => s.timeScheduleEmployeePeriodDetailId));

        // Merge shifts and leisure codes
        shiftsToDisplay = shiftsToDisplay.concat(leisureCodesToDisplay);
        shiftsToDisplay = shiftsToDisplay.sort(ShiftDTO.wholeDayStartTimeSort);

        if (employeeId !== this.hiddenEmployeeId)
            return shiftsToDisplay;

        // Group linked shifts for hidden employee
        const group = _.groupBy(shiftsToDisplay.filter(s => s.link), s => s.link);
        const links = Object.keys(group);
        links.forEach(link => {
            const linkedShifts = shiftsToDisplay.filter(s => s.link === link);
            if (linkedShifts.length > 0) {
                const groupStart: Date = _.orderBy(linkedShifts, 'actualStartTime')[0].actualStartTime;
                const groupStop: Date = _.orderBy(linkedShifts, 'actualStopTime', 'desc')[0].actualStopTime;
                linkedShifts.forEach(s => {
                    s['groupStartTime'] = groupStart;
                    s['groupStopTime'] = groupStop;
                });
            }
        });

        return _.orderBy(shiftsToDisplay, ['groupStartTime', 'groupStopTime', 'link', 'actualStartTime']);
    }

    private employeeHasVisibleShifts(employeeId: number): boolean {
        return this.visibleEmployeeIdsSet.has(employeeId);
    }

    public getTemplateVisibleRange(template: TimeScheduleTemplateHeadSmallDTO): DateRangeDTO {
        return this.templateHelper.getTemplateVisibleRange(template);
    }

    public getEmployeeRowHeight(nbrOfRows: number) {
        let height: number = this.isCompressedStyle ? this.shiftHeightCompressed : this.shiftHeight;
        height += this.isCompressedStyle ? this.shiftMarginCompressed + 1 : this.shiftMargin;
        height *= nbrOfRows;
        if (nbrOfRows > 0)
            height += this.isCompressedStyle ? this.shiftMarginCompressed : this.shiftMargin;

        return height;
    }

    public getShiftTopPosition(index: number) {
        let top = this.getEmployeeRowHeight(index);
        if (index === 0) {
            top += this.isCompressedStyle ? this.shiftMarginCompressed : this.shiftMargin;
        } else {
            top += this.isCompressedStyle ? index : -index;
        }

        return top;
    }

    public getStaffingNeedsRowHeight(nbrOfRows: number) {
        let height: number = this.staffingNeedsHeight;
        if (this.isStaffingNeedsScheduleView) {
            height += this.shiftMargin;
            height *= nbrOfRows;
            if (nbrOfRows > 0)
                height += this.shiftMargin;
        }

        return height;
    }

    public getStaffingNeedsPeriodTopPosition(index: number) {
        let top = 0;

        if (this.isStaffingNeedsScheduleView) {
            top += this.shiftMargin;
            if (index > 0)
                top += (this.staffingNeedsHeight + 1) * index;
        }

        return top;
    }

    private showTaskTypeTask(): boolean {
        return this.isTasksAndDeliveriesView && this.getFilteredTaskTypeIds().includes(SoeStaffingNeedsTaskType.Task);
    }

    private showTaskTypeDelivery(): boolean {
        return this.isTasksAndDeliveriesView && this.getFilteredTaskTypeIds().includes(SoeStaffingNeedsTaskType.Delivery);
    }

    public getTasks(taskId: string, date: Date): StaffingNeedsTaskDTO[] {
        if (!this.allTasks || !this.allTasks.length || !date)
            return undefined;

        let accountIds: number[] = [];
        if (this.useAccountHierarchy)
            accountIds = this.getFilteredAccountIds();

        let split = taskId.split('_');
        let type = parseInt(split[0], 10);
        let parentId = parseInt(split[1], 10);

        let taskItems = _.orderBy(this.allTasks.filter(t => t.type == type && t.parentId === parentId && t.isVisible === true), ['actualStartTime'], ['desc']);
        let taskDateItems: any[] = [];
        taskItems.forEach(taskItem => {
            if (date.isSameDayAs(taskItem.startTime)) {
                if (accountIds.length === 0 || accountIds.includes(taskItem.accountId) || !taskItem.accountId)
                    taskDateItems.push(taskItem);
            }
        });

        return taskDateItems;
    }

    private printTasksAndDeliveries() {
        if (this.visibleTasks.length === 0)
            return;

        let reportId = this.isTasksAndDeliveriesDayView ? this.tasksAndDeliveriesDayReportId : this.tasksAndDeliveriesWeekReportId;
        this.reportDataService.createReportJob(ReportJobDefinitionFactory.createSimpleScheduleReportDefinition(reportId, SoeReportTemplateType.TimeScheduleTasksAndDeliverysReport, [], this.dateFrom, this.dateTo, this.getFilteredShiftTypeIds(), null, TermGroup_ReportExportType.Pdf), true);
    }

    private showOpenNeed() {
        // Show open/add need dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/CreateNeed/Views/createNeed.html"),
            controller: CreateNeedController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                weekdays: () => { return this.weekdays; },
                date: () => { return this.dateFrom.beginningOfWeek(); },
                frequencyTasks: () => { return this.frequencyTasks; }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.startWork("core.loading");
                this.scheduleService.createStaffingNeedsHeadsFromTasks(this.dayViewMinorTickLength, '', result.date, 0, result.weekday, result.wholeWeek, true, result.intervalDateFrom, result.intervalDateTo, result.dayOfWeeks, result.adjustPercent, result.fromDate ? result.fromDate : this.dateFrom, result.timeScheduleTaskId || 0).then(x => {
                    if (x.success)
                        this.loadStaffingNeedsHeads();
                    else
                        this.completedWork(null, true);
                });
            }
        }, (reason) => {
            // User cancelled
            this.heads = [];
        });
    }

    private reloadNeed() {
        this.loadStaffingNeedsHeads();
    }

    private openEditDelivery(task: StaffingNeedsTaskDTO, slot: SlotDTO) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/IncomingDeliveries/Views/edit.html"),
            controller: IncomingDeliveriesEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: task ? task.parentId : 0,
                startTime: slot ? slot.startTime : null,
                stopTime: slot ? slot.stopTime : null
            });
        });

        modal.result.then(id => {
            this.loadIncomingDeliveries([id]).then(() => {
                this.setGroupBy(this.isDayView ? this.dayViewGroupBy : this.scheduleViewGroupBy, false);
                this.filterTasks();
            });
        }, (reason) => {
            // User cancelled dialog
        });
    }

    private openEditTask(task: StaffingNeedsTaskDTO, slot: SlotDTO) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/TimeScheduleTasks/Views/edit.html"),
            controller: TimeScheduleTasksEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: task ? task.id : 0,
                startTime: slot ? slot.startTime : null,
                stopTime: slot ? slot.stopTime : null
            });
        });

        modal.result.then(id => {
            this.loadTimeScheduleTasks([id]).then(() => {
                this.setGroupBy(this.isDayView ? this.dayViewGroupBy : this.scheduleViewGroupBy, false);
                this.filterTasks();
            });
        }, (reason) => {
            // User cancelled dialog
        });
    }

    private openDeleteDelivery(task: StaffingNeedsTaskDTO) {
        // Get all delivery rows connected to same head
        let rows = this.deliveries.filter(d => d.incomingDeliveryHeadId === task.parentId);
        if (!rows || rows.length === 0)
            return;

        // Show delete delivery dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DeleteDelivery/Views/deleteDelivery.html"),
            controller: DeleteDeliveryController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                deliveries: () => { return rows },
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.scheduleService.deleteIncomingDelivery(task.parentId).then(res => {
                    if (res.success) {
                        _.pullAll(this.allTasks, this.allTasks.filter(t => t.type === SoeStaffingNeedsTaskType.Delivery && t.parentId === task.parentId));
                        _.pullAll(this.deliveries, this.deliveries.filter(t => t.incomingDeliveryHeadId === task.parentId));
                        this.renderBody('deleteIncomingDelivery');
                    } else {
                        this.notificationService.showDialogEx(this.terms["error.default_error"], res.errorMessage, SOEMessageBoxImage.Error);
                    }
                });
            }
        }, (reason) => {
            // User cancelled dialog
        });
    }

    private openDeleteTask(task: StaffingNeedsTaskDTO) {
        // Show delete task dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/DeleteTask/Views/deleteTask.html"),
            controller: DeleteTaskController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                task: () => { return task },
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.scheduleService.deleteTimeScheduleTask(task.id).then(res => {
                    if (res.success) {
                        _.pullAll(this.allTasks, this.allTasks.filter(t => t.type === SoeStaffingNeedsTaskType.Task && t.id === task.id));
                        _.pullAll(this.tasks, this.tasks.filter(t => t.timeScheduleTaskId === task.id));
                        this.renderBody('deleteTimeScheduleTask');
                    } else {
                        this.notificationService.showDialogEx(this.terms["error.default_error"], res.errorMessage, SOEMessageBoxImage.Error);
                    }
                });
            }
        }, (reason) => {
            // User cancelled dialog
        });
    }

    private openGeneratedNeedsDialog(timeScheduleTaskId: number, date: Date) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/TimeScheduleTasks/Dialogs/GeneratedNeedsDialog.html"),
            controller: GeneratedNeedsDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                timeScheduleTaskId: () => { return timeScheduleTaskId },
                date: () => { return date }
            }
        }

        this.$uibModal.open(options);
    }

    private isValidUrl(url: string): boolean {
        if (!url || url.contains('report=0')) {
            let keys: string[] = [
                "core.warning",
                "common.reportsettingmissing"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["core.warning"], terms["common.reportsettingmissing"], SOEMessageBoxImage.Warning);
            });
            return false;
        }
        return true;
    }

    // Filter

    private clearFilters(keepUserSelection: boolean = false, render: boolean = true, loadData: boolean = false) {
        let _isFilteredOnShiftType = this.isFilteredOnShiftType;

        this.selectedCategories = [];
        this.showSecondaryCategories = false;
        this.selectedEmployees = [];
        this.showSecondaryAccounts = false;
        this.accountDims.forEach(accountDim => {
            if (!accountDim['hidden'])
                accountDim.selectedAccounts = [];
        });
        this.selectedEmployeeGroups = [];
        this.selectedShiftTypes = [];
        this.selectedStatuses = [];
        this.selectedDeviationCauses = [];
        this.selectedBlockTypes = [];
        this.freeTextFilter = '';

        if (!keepUserSelection)
            this.selectedUserSelectionId = 0;

        this.copyAllEmployees(true);

        this.setShiftTypes();

        if (this.subsetOfShiftsLoaded) {
            if (this.selectableInformationSettings.doNotSearchOnFilter)
                this.filteredButNotLoaded = true;
            else if (loadData)
                this.loadData('clearFilters', true);
        } else {
            this.filter(render);
        }

        // Reload unscheduled tasks
        if (_isFilteredOnShiftType)
            this.loadUnscheduledTasksAndDeliveriesDates();
    }

    private showAllEmployeesChanging() {
        this.$timeout(() => {
            this.clearEmployeeIdsForShiftLoad();
            this.recalculateEmployeeWorkTimes = true;
            this.filterShifts('showAllEmployeesChanging');
        });
    }

    private filterEmployees(source: string, render: boolean = true, stopProgressWhenDone: boolean = true): boolean {
        if (this.isEmployeePostView) {
            this.copyAllEmployees(false);
            return false;
        }

        if (this.employedEmployees.length === 0 && this.employees.length === 0)
            return false;

        this.employees = [];
        let validEmployeeIds = this.getValidEmployeeIdsForInterval(this.employedEmployees, this.dateFrom, this.dateTo);
        this.employedEmployees.forEach(employee => {
            if (validEmployeeIds.includes(employee.employeeId))
                this.copyEmployee(employee);
        });

        // Make sure previously selected employees are not selected if not permitted anymore.
        // For example category with end date and user moves to next week.
        let nbrOfSelectedEmployees = this.selectedEmployees.length;
        if (nbrOfSelectedEmployees > 0)
            this.selectedEmployees = this.selectedEmployees.filter(se => this.employees.map(e => e.id).includes(se.id));

        // If all filtered employees are removed from filter, notify user and prevent loading shifts for all employees.
        let abortLoad: boolean = false;
        if (nbrOfSelectedEmployees > 0 && this.selectedEmployees.length === 0) {
            abortLoad = true;
            render = false;
        }

        this.resortEmployeeFilter();

        if (render || abortLoad)
            this.filterShifts('filterEmployees', render, stopProgressWhenDone);

        return abortLoad;
    }

    private getValidEmployeeIdsForInterval(employees: EmployeeListDTO[], dateFrom: Date, dateTo: Date): number[] {
        let validEmployeeIds: number[] = [];

        if (employees.length > 0) {
            let employeeGroupIds = this.getFilteredEmployeeGroupIds();

            if (this.useAccountHierarchy) {
                // Filter employees based on account hierarchy, filtered accounts and employee groups
                let filteredAccountIds = this.getFilteredAccountIds();
                employees.forEach(employee => {
                    if (employee.hidden) {
                        validEmployeeIds.push(employee.employeeId);
                    } else if ((!this.isFilteredOnEmployeeGroup || employeeGroupIds.includes(this.getCurrentEmployeeGroupId(employee))) && employee.accounts) {
                        for (let empAccount of employee.accounts) {
                            if (this.isEmployeeAccountValid(filteredAccountIds, empAccount, dateFrom, dateTo)) {
                                validEmployeeIds.push(employee.employeeId);
                                break;
                            }
                        }
                    }
                });
            } else {
                // Filter employees based on filtered categories and employee groups
                let categoryIds = this.getFilteredCategoryIds();

                employees.forEach(employee => {
                    if (employee.hidden) {
                        validEmployeeIds.push(employee.employeeId);
                    } else if ((!this.isFilteredOnEmployeeGroup || employeeGroupIds.includes(this.getCurrentEmployeeGroupId(employee))) && employee.categoryRecords) {
                        for (let categoryId of categoryIds) {
                            // Check if employee has current category
                            let empCategory = employee.categoryRecords.find(c => c.categoryId === categoryId);
                            if (empCategory) {
                                // Check dates
                                if ((!empCategory.dateFrom || empCategory.dateFrom.isSameOrBeforeOnDay(dateTo)) && (!empCategory.dateTo || empCategory.dateTo.isSameOrAfterOnDay(dateFrom))) {
                                    validEmployeeIds.push(employee.employeeId);
                                    break;
                                }
                            }
                        }
                    }
                });
            }
        }

        return validEmployeeIds;
    }

    private isEmployeeInactive(employeeId: number): boolean {
        if (!this.selectableInformationSettings.showInactiveEmployees)
            return false;

        return this.inactiveEmployeeIds.includes(employeeId);
    }

    private isEmployeeAccountValid(filteredAccountIds: number[], empAccount: EmployeeAccountDTO, dateFrom: Date, dateTo: Date): boolean {
        // Check account
        if (filteredAccountIds.includes(empAccount.accountId)) {
            // Check default
            if (!this.showSecondaryAccounts && !empAccount.default)
                return false;

            // Check date interval
            if (empAccount.dateFrom.isSameOrBeforeOnDay(dateTo) && (!empAccount.dateTo || empAccount.dateTo.isSameOrAfterOnDay(dateFrom))) {
                // Check children
                if (!empAccount.children || empAccount.children.length === 0) {
                    // No children, parent was valid so it's OK
                    return true;
                } else {
                    // Recursively check each child account
                    // If one is valid it's OK
                    let childValid: boolean = false;
                    for (let childAccount of empAccount.children) {
                        if (this.isEmployeeAccountValid(filteredAccountIds, childAccount, dateFrom, dateTo)) {
                            childValid = true;
                            break;
                        }
                    }
                    if (childValid)
                        return true;
                }
            }
        }

        return false;
    }

    public get visibleEmployees(): EmployeeListDTO[] {
        return this.allEmployees.filter(e => e.isVisible);
    }

    public getVisibleEmployeeIds(): number[] {
        return _.uniq(this.visibleEmployees.filter(e => e.employeeId).map(e => e.employeeId));
    }

    public getVisibleEmployeePostIds(): number[] {
        return _.uniq(this.visibleEmployees.filter(e => e.employeePostId).map(e => e.employeePostId));
    }

    public get activeEmployees(): EmployeeListDTO[] {
        if (this.selectableInformationSettings.showInactiveEmployees)
            return this.allEmployees;
        else
            return this.allEmployees.filter(e => e.active);
    }

    public get employedEmployeeIds(): number[] {
        return this.employedEmployees.map(e => e.employeeId);
    }

    private setEmployedEmployees() {
        let employedEmployeesChanged = false;

        const preEmployedEmployeeIds = this.employedEmployeeIds;

        this.employedEmployees = [];
        for (let employee of this.activeEmployees) {
            if (this.selectableInformationSettings.showUnemployedEmployees || employee.isGroupHeader || employee.hasEmployment(this.dateFrom, this.dateTo))
                this.employedEmployees.push(employee);
        }

        const postEmployedEmployeeIds = this.employedEmployeeIds;

        if (!NumberUtility.compareArrays(preEmployedEmployeeIds, postEmployedEmployeeIds))
            employedEmployeesChanged = true;

        if (employedEmployeesChanged) {
            // Reload information based on employed employees
            if (this.selectableInformationSettings.showAvailability && postEmployedEmployeeIds.length > 0) {
                // Reload Employee availability
                this.loadEmployeeAvailability();
            }
        }

        this.resetSort();
    }

    private copyAllEmployees(setEmployeeAsVisible: boolean) {
        this.employees = [];
        this.employedEmployees.forEach(employee => {
            this.copyEmployee(employee);
            if (setEmployeeAsVisible)
                employee.isVisible = true;
        });
        this.resortEmployeeFilter();
    }

    private copyEmployee(employee: EmployeeListDTO) {
        if (employee.isGroupHeader)
            return;

        if (!this.isEmployeePostView && employee.employeePostId)
            return;
        if (this.isEmployeePostView && !employee.employeePostId)
            return;

        this.employees.push({ id: employee.identifier, label: employee.hidden || employee.employeePostId ? employee.name : employee.numberAndName, isEmployeePost: !employee.employeeId });
    }

    private copyAllEmployeeLists() {
        this.employeeList = [];
        _.orderBy(this.employedEmployees.filter(e => e.employeeId && !e.employeePostId), 'name').forEach(employee => {
            this.copyEmployeeList(employee);
        });
    }

    private copyEmployeeList(employee: EmployeeListDTO) {
        if (employee.isGroupHeader || employee.employeeId === this.hiddenEmployeeId || this.vacantEmployeeIds.includes(employee.employeeId))
            return;

        if (!this.isEmployeePostView && employee.employeePostId)
            return;

        if (employee.oneWeekWorkTimeMinutes === 0)
            this.calculateEmployeeWorkTimes(employee);

        let emp = new EmployeeRightListDTO();
        emp.employeeId = employee.employeeId;
        emp.employeeNr = employee.employeeNr;
        emp.employeeNrSort = employee.employeeNrSort;
        emp.firstName = employee.firstName;
        emp.lastName = employee.lastName;
        emp.name = employee.name;
        //emp.imageSource = employee.imageSource;
        emp.employeePostId = undefined;
        emp.wantsExtraShifts = false;
        emp.workTimeMinutes = employee.oneWeekWorkTimeMinutes;

        emp.employeeSkills = employee.employeeSkills;
        emp.employments = employee.employments;

        this.employeeList.push(emp);
    }

    private clearAccountOnEmployees() {
        // Clear existing account on all employees
        this.employedEmployees.forEach(employee => {
            employee.groupName = null;
            employee['accountId'] = null;
            employee['accountIds'] = [];
            employee['isDuplicate'] = false;
        });
    }

    private setAccountOnEmployees() {
        this.clearAccountOnEmployees();

        // Get grouped by account dim
        let accountDim: AccountDimSmallDTO;
        if (this.isCommonDayView)
            accountDim = this.accountDims.find(a => a.groupByIndex === this.dayViewGroupBy);
        else if (this.isCommonScheduleView)
            accountDim = this.accountDims.find(a => a.groupByIndex === this.scheduleViewGroupBy);
        else
            return;

        // Set all available accounts on employee (all levels)
        let employees: EmployeeListDTO[] = this.employedEmployees.filter(e => this.getFilteredEmployeeIds().includes(e.employeeId) && e.accounts && e.accounts.length > 0);
        employees.forEach(employee => {
            employee['accountIds'] = this.getEmployeeAccountIds(employee);
        });

        // Loop through accounts and add first name that exists on employee
        let filteredAccountIds = this.getFilteredAccountDimAccountIds(accountDim);
        accountDim.accounts.filter(a => filteredAccountIds.includes(a.accountId)).forEach(account => {
            let hasEmployees: boolean = false;
            employees.filter(e => e['accountIds']).forEach(employee => {
                if (employee['accountIds'].includes(account.accountId)) {
                    let hasShifts: boolean = false;

                    if (this.selectedShiftTypes.length > 0) {
                        if (this.isEmployeePostView)
                            hasShifts = _.some(this.shifts, s => s.employeePostId === employee.employeePostId && s.accountId === account.accountId && this.selectedShiftTypes.map(st => st.id).includes(s.shiftTypeId));
                        else
                            hasShifts = _.some(this.shifts, s => s.employeeId === employee.employeeId && s.accountId === account.accountId && this.selectedShiftTypes.map(st => st.id).includes(s.shiftTypeId));

                    } else {
                        if (this.isEmployeePostView)
                            hasShifts = _.some(this.shifts, s => s.employeePostId === employee.employeePostId && s.accountId === account.accountId);
                        else
                            hasShifts = _.some(this.shifts, s => s.employeeId === employee.employeeId && s.accountId === account.accountId);

                    }


                    if (hasShifts) {
                        if (!employee['shiftAccountIds'])
                            employee['shiftAccountIds'] = [];
                        employee['shiftAccountIds'].push(account.accountId);

                        employee.groupName = account.name + '__';    // The underscore is to make it sort after the account dim group name
                        hasEmployees = true;
                    }
                }
            });

            // Only add account dims that has any employees
            if (hasEmployees) {
                let newEmp: EmployeeListDTO = new EmployeeListDTO();
                newEmp.isGroupHeader = true;
                newEmp['accountId'] = account.accountId;
                newEmp['isAccount'] = true;
                newEmp.groupName = account.name + "_";    // Need to add an underscore to support blanks in name, eg: "Kassa", "Kassa arbetsledning"
                newEmp.name = newEmp.firstName = account.name;
                newEmp.lastName = '';
                newEmp.hidden = false;
                newEmp.vacant = false;
                newEmp.isVisible = true;
                this.employedEmployees.push(newEmp);
            }
        });

        // Create "no account" group
        let noAccountEmployees: EmployeeListDTO[] = this.employedEmployees.filter(e => !e.groupName && this.getFilteredEmployeeIds().includes(e.employeeId));
        if (noAccountEmployees.length > 0) {
            // Check if any of the "no account" employees actually has any shifts
            let hasShifts: boolean = false;
            for (let employee of noAccountEmployees) {
                if (this.isEmployeePostView)
                    hasShifts = _.some(this.shifts, s => s.employeePostId === employee.employeePostId && !s.accountId);
                else
                    hasShifts = _.some(this.shifts, s => s.employeeId === employee.employeeId && !s.accountId);
                if (hasShifts)
                    break;
            }

            if (hasShifts) {
                let newEmp: EmployeeListDTO = new EmployeeListDTO();
                newEmp.isGroupHeader = true;
                newEmp['accountId'] = 0;
                newEmp['isAccount'] = true;
                newEmp.groupName = newEmp.name = newEmp.firstName = this.terms["core.others"];
                newEmp.lastName = '';
                newEmp.hidden = false;
                newEmp.vacant = false;
                newEmp.isVisible = true;
                this.employedEmployees.push(newEmp);

                noAccountEmployees.forEach(employee => {
                    employee.groupName = newEmp.name + '_';
                });
            }
        }
    }

    private getEmployeeAccountIds(employee: EmployeeListDTO): number[] {
        let accountIds: number[] = [];

        employee.accounts.forEach(account => {
            // Add main account
            accountIds.push(account.accountId);

            // Add child accounts
            if (account.children)
                accountIds.push(...account.children.map(c => c.accountId));

            // Add parent account
            this.accountDims.forEach(dim => {
                const acc: AccountDTO = dim.accounts.find(a => a.accountId === account.accountId);
                if (acc?.parentAccountId) {
                    const parentDim = this.accountDims.find(d => d.accountDimId === dim.parentAccountDimId);
                    if (parentDim?.accounts) {
                        const parentAccount = parentDim.accounts.find(a => a.accountId === acc.parentAccountId);
                        if (parentAccount)
                            accountIds.push(parentAccount.accountId);
                    }
                }
            });
        });

        return accountIds;
    }

    private setCategoryOnEmployees() {
        // Clear existing categories on all employees
        this.employedEmployees.forEach(employee => {
            employee.groupName = null;
        });

        // Loop through categories and add first category name that exists on employee
        let employeesWithShifts: number[] = this.isEmployeePostView ? this.shifts.map(s => s.employeePostId) : this.shifts.map(s => s.employeeId);
        let employees: EmployeeListDTO[] = this.employedEmployees.filter(e => !e.groupName && this.getFilteredEmployeeIds().includes(e.employeeId) && e.categoryRecords && e.categoryRecords.length > 0 && employeesWithShifts.includes(e.employeeId));
        this.categories.forEach(category => {
            let hasEmployees: boolean = false;
            employees.forEach(employee => {
                if (_.first(employee.categoryRecords).categoryId === category.id) {
                    employee.groupName = category.label + '__';    // The underscore is to make it sort after the category group name
                    hasEmployees = true;
                }
            });

            // Only add categories that has any employees
            if (hasEmployees) {
                let newEmp: EmployeeListDTO = new EmployeeListDTO();
                newEmp.isGroupHeader = true;
                newEmp['categoryId'] = category.id;
                newEmp['isCategory'] = true;
                newEmp.groupName = category.label + "_";    // Need to add an underscore to support blanks in name, eg: "Kassa", "Kassa arbetsledning"
                newEmp.name = newEmp.firstName = category.label;
                newEmp.lastName = '';
                newEmp.hidden = false;
                newEmp.vacant = false;
                newEmp.isVisible = true;
                this.employedEmployees.push(newEmp);
            }
        });
    }

    private setShiftTypeOnEmployees() {
        // Clear existing shift types on all employees
        this.employedEmployees.forEach(employee => {
            employee.groupName = null;
        });

        // Loop through employees and add shift type on their first shift in current date range
        let shiftTypesWithEmployees = [];
        let todaysShifts = this.visibleShifts;
        this.employedEmployees.forEach(employee => {
            let empShifts = _.orderBy(todaysShifts.filter(s => (this.isEmployeePostView ? s.employeePostId === employee.employeePostId : s.employeeId === employee.employeeId)), 'actualStartTime');
            if (empShifts.length > 0) {
                if (employee.hidden) {
                    // Do not add any group for hidden employee
                    // It will always be at the top
                } else {
                    employee.groupName = empShifts[0].shiftTypeName + '__';   // The underscore is to make it sort after the shift type group name
                    if (!shiftTypesWithEmployees.includes(empShifts[0].shiftTypeId))
                        shiftTypesWithEmployees.push(empShifts[0].shiftTypeId);
                }
            }
        });

        this.shiftTypes.filter(s => s.id !== 0).forEach(shiftType => {
            // Only add shift types that has any employees
            if (shiftTypesWithEmployees.includes(shiftType.id)) {
                let newEmp: EmployeeListDTO = new EmployeeListDTO();
                newEmp.isGroupHeader = true;
                newEmp['shiftTypeId'] = shiftType.id;
                newEmp['isShiftType'] = true;
                newEmp.groupName = shiftType.label + "_";    // Need to add an underscore to support blanks in name, eg: "Kassa", "Kassa arbetsledning"
                newEmp.name = newEmp.firstName = shiftType.label;
                newEmp.lastName = '';
                newEmp.hidden = false;
                newEmp.vacant = false;
                newEmp.isVisible = true;
                this.employedEmployees.push(newEmp);
            }
        });
    }

    private setStartTimeOnEmployees() {
        // Clear existing start times on all employees
        this.employedEmployees.forEach(employee => {
            employee['startTime'] = null;
            employee['stopTime'] = null;
        });

        // Loop through employees and add start time for their first shift in current date range
        let todaysShifts = this.visibleShifts;
        this.employedEmployees.forEach(employee => {
            let empShifts = _.orderBy(todaysShifts.filter(s => (this.isEmployeePostView ? s.employeePostId === employee.employeePostId : s.employeeId === employee.employeeId)), ['actualStartTime', 'actualStopTime', 'shiftTypeName']);
            if (empShifts.length > 0) {
                employee['startTime'] = _.first(empShifts).actualStartTime;
                employee['stopTime'] = _.last(empShifts).actualStopTime;
            }
        });
    }

    private setAvailabilityOnEmployees() {
        let availableRangeStart: Date;
        let availableRangeStop: Date;

        let selectedShifts = this.scheduleHandler.getSelectedShifts();
        if (selectedShifts.length === 0) {
            availableRangeStart = this.dateFrom;
            availableRangeStop = this.dateTo;
        } else {
            availableRangeStart = _.first(_.sortBy(selectedShifts, s => s.actualStartTime)).actualStartTime;
            availableRangeStop = _.last(_.sortBy(selectedShifts, s => s.actualStopTime)).actualStopTime;
        }

        this.employeeList.forEach(employee => {
            let fullEmp = this.getEmployeeById(employee.employeeId);
            if (fullEmp) {
                let availabilityToolTip: string = ''
                if (fullEmp.isFullyAvailableInRange(availableRangeStart, availableRangeStop)) {
                    employee.isFullyAvailable = true;
                    employee.availabilitySort = EmployeeAvailabilitySortOrder.FullyAvailable;
                    availabilityToolTip = this.terms["time.schedule.planning.available"];
                } else if (fullEmp.isFullyUnavailableInRange(availableRangeStart, availableRangeStop)) {
                    employee.isFullyUnavailable = true;
                    employee.availabilitySort = EmployeeAvailabilitySortOrder.FullyUnavailable;
                    availabilityToolTip = this.terms["time.schedule.planning.unavailable"];
                } else {
                    let partlyAvailable = fullEmp.isAvailableInRange(availableRangeStart, availableRangeStop);
                    let partlyUnavailable = fullEmp.isUnavailableInRange(availableRangeStart, availableRangeStop);
                    if (partlyAvailable && !partlyUnavailable) {
                        employee.isPartlyAvailable = true;
                        employee.availabilitySort = EmployeeAvailabilitySortOrder.PartlyAvailable;
                    } else if (partlyUnavailable && !partlyAvailable) {
                        employee.isPartlyUnavailable = true;
                        employee.availabilitySort = EmployeeAvailabilitySortOrder.PartlyUnavailable;
                    } else if (partlyAvailable && partlyUnavailable) {
                        employee.isMixedAvailable = true;
                        employee.availabilitySort = EmployeeAvailabilitySortOrder.MixedAvailable;
                    }
                    if (partlyAvailable) {
                        let availableDates = fullEmp.getAvailableInRange(availableRangeStart, availableRangeStop);
                        if (availableDates.length > 0) {
                            availableDates.forEach(availableDate => {
                                availabilityToolTip += "{0} {1}-{2}".format(this.terms["time.schedule.planning.available"], availableDate.start.toFormattedTime(), availableDate.stop.toFormattedTime());
                                if (availableDate.comment)
                                    availabilityToolTip += ", {0}".format(availableDate.comment);
                                availabilityToolTip += "\n";
                            });
                        }
                    }
                    if (partlyUnavailable) {
                        let unavailableDates = fullEmp.getUnavailableInRange(availableRangeStart, availableRangeStop);
                        if (unavailableDates.length > 0) {
                            availabilityToolTip += ' ';
                            unavailableDates.forEach(unavailableDate => {
                                availabilityToolTip += "{0} {1}-{2}".format(this.terms["time.schedule.planning.unavailable"], unavailableDate.start.toFormattedTime(), unavailableDate.stop.toFormattedTime());
                                if (unavailableDate.comment)
                                    availabilityToolTip += ", {0}".format(unavailableDate.comment);
                                availabilityToolTip += "\n";
                            });
                        }
                    }
                }

                if (availabilityToolTip.length > 0)
                    employee.toolTip = availabilityToolTip;
            }
        });
    }

    private setDepartmentOnTasks(dimNr: number) {
        // Remove all "department tasks"
        this.allTasks = this.allTasks.filter(e => !e['isDepartment']);

        // Loop through tasks and add department on them
        let departmentsWithTasks: number[] = [];
        let departmentsWithDeliveries: number[] = [];
        this.allTasks.forEach(task => {
            task['departmentGroup'] = '';
            if (dimNr === 2 && task.account2Id) {
                task['departmentGroup'] = task.accountDim2Name + '_';   // The underscore is to make it sort after the department group name
                if (task.isTask)
                    departmentsWithTasks.push(task.account2Id);
                else if (task.isDelivery)
                    departmentsWithDeliveries.push(task.account2Id);
            } else if (dimNr === 3 && task.account3Id) {
                task['departmentGroup'] = task.accountDim3Name + '_';   // The underscore is to make it sort after the department group name
                if (task.isTask)
                    departmentsWithTasks.push(task.account3Id);
                else if (task.isDelivery)
                    departmentsWithDeliveries.push(task.account3Id);
            } else if (dimNr === 4 && task.account4Id) {
                task['departmentGroup'] = task.accountDim4Name + '_';   // The underscore is to make it sort after the department group name
                if (task.isTask)
                    departmentsWithTasks.push(task.account4Id);
                else if (task.isDelivery)
                    departmentsWithDeliveries.push(task.account4Id);
            } else if (dimNr === 5 && task.account5Id) {
                task['departmentGroup'] = task.accountDim5Name + '_';   // The underscore is to make it sort after the department group name
                if (task.isTask)
                    departmentsWithTasks.push(task.account5Id);
                else if (task.isDelivery)
                    departmentsWithDeliveries.push(task.account5Id);
            } else if (dimNr === 6 && task.account6Id) {
                task['departmentGroup'] = task.accountDim6Name + '_';   // The underscore is to make it sort after the department group name
                if (task.isTask)
                    departmentsWithTasks.push(task.account6Id);
                else if (task.isDelivery)
                    departmentsWithDeliveries.push(task.account6Id);
            }
        });

        // Get all departments that has tasks
        departmentsWithTasks = _.uniq(departmentsWithTasks);
        departmentsWithDeliveries = _.uniq(departmentsWithDeliveries);

        let accountDim = this.accountDims[dimNr - 2];
        accountDim.accounts.forEach(account => {
            // Only add departments that has any tasks
            if (departmentsWithTasks.includes(account.accountId)) {
                let newTask: StaffingNeedsTaskDTO = new StaffingNeedsTaskDTO(SoeStaffingNeedsTaskType.Task);
                newTask.parentId = 0;
                newTask.dateId = CalendarUtility.convertToDate(this.dateFrom).timeValueDay();
                newTask['isDepartment'] = true;
                newTask['departmentGroup'] = account.name;
                newTask.name = account.name;
                this.allTasks.push(newTask);
            }
            // Only add departments that has any deliveries
            if (departmentsWithDeliveries.includes(account.accountId)) {
                let newTask: StaffingNeedsTaskDTO = new StaffingNeedsTaskDTO(SoeStaffingNeedsTaskType.Delivery);
                newTask.parentId = 0;
                newTask.dateId = CalendarUtility.convertToDate(this.dateFrom).timeValueDay();
                newTask['isDepartment'] = true;
                newTask['departmentGroup'] = account.name;
                newTask.name = account.name;
                this.allTasks.push(newTask);
            }
        });
    }

    private setShiftTypeOnTasks() {
        // Remove all "shift type tasks"
        this.allTasks = this.allTasks.filter(e => !e['isShiftType']);

        // Loop through tasks and add shift type on them
        this.allTasks.forEach(task => {
            task['shiftTypeGroup'] = task.shiftTypeName + '_';   // The underscore is to make it sort after the shift type group name
        });

        // Get all shift types that has tasks
        let shiftTypesWithTasks = _.uniq(this.allTasks.map(t => t.shiftTypeId));

        this.shiftTypes.filter(s => s.id !== 0).forEach(shiftType => {
            // Only add shift types that has any tasks
            if (shiftTypesWithTasks.includes(shiftType.id)) {
                let newTask: StaffingNeedsTaskDTO = new StaffingNeedsTaskDTO(SoeStaffingNeedsTaskType.Unknown);
                newTask['isShiftType'] = true;
                newTask['shiftTypeGroup'] = shiftType.label;
                newTask.name = shiftType.label;
                this.allTasks.push(newTask);
            }
        });
    }

    private resortEmployeeFilter() {
        this.employees.forEach(employee => {
            let fullEmp = employee.isEmployeePost ? this.getEmployeePostById(employee.id) : this.getEmployeeById(employee.id);
            if (fullEmp) {
                employee.label = fullEmp.hidden || fullEmp.employeePostId ? fullEmp.name : fullEmp.numberAndName;
                employee.hidden = fullEmp.hidden;
                employee.vacant = fullEmp.vacant;
                employee.sort = ((this.isCommonDayView && this.dayViewSortBy === TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr) ||
                    (this.isCommonScheduleView && this.scheduleViewSortBy === TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr)) ? fullEmp.employeeNrSort : fullEmp.name;
            }
        });
        this.employees = _.orderBy(this.employees, ['hidden', 'vacant', 'sort'], ['desc', 'asc', 'asc']);
    }

    private filterPeriods() {
        if (this.isFilteredOnStatus) {
            let statusIds = this.selectedStatuses.map(s => s.id);

            this.periods.forEach(period => {
                if (!statusIds.includes(PlanningStatusFilterItems.Open))
                    period.open = 0;
                if (!statusIds.includes(PlanningStatusFilterItems.Assigned))
                    period.assigned = 0;
                if (!statusIds.includes(PlanningStatusFilterItems.Wanted))
                    period.wanted = 0;
                if (!statusIds.includes(PlanningStatusFilterItems.Unwanted))
                    period.unwanted = 0;
                if (!statusIds.includes(PlanningStatusFilterItems.AbsenceRequested))
                    period.absenceRequested = 0;
                if (!statusIds.includes(PlanningStatusFilterItems.AbsenceApproved))
                    period.absenceApproved = 0;
                if (!statusIds.includes(PlanningStatusFilterItems.Preliminary))
                    period.preliminary = 0;
            });
        }
    }

    private filterAccounts(accountDim: AccountDimSmallDTO, preFilter: boolean) {
        let selectedAccountIds: number[] = (accountDim.selectedAccounts.length > 0 ? accountDim.selectedAccounts : accountDim.filteredAccounts).map(a => a.accountId);

        // Clear selection on all child dims
        let childDim = this.getChildDim(accountDim.accountDimId);
        while (childDim) {
            childDim.selectedAccounts = [];
            childDim.filteredAccounts = [];
            selectedAccountIds.forEach(selectedAccountId => {
                childDim.filteredAccounts = _.concat(childDim.filteredAccounts, childDim.accounts.filter(a => a.parentAccountId === selectedAccountId && !a.hasVirtualParent));
            });
            // Add accounts without parent
            childDim.filteredAccounts = _.sortBy(_.concat(childDim.filteredAccounts, childDim.accounts.filter(a => !a.parentAccountId || a.hasVirtualParent)), a => a.name);

            selectedAccountIds = childDim.filteredAccounts.map(a => a.accountId);

            childDim = this.getChildDim(childDim.accountDimId);
        }

        this.setShiftTypes();

        if (this.useAccountHierarchy) {
            this.filterEmployees('filterAccounts', !preFilter);
            if (!preFilter) {
                if (this.selectableInformationSettings.doNotSearchOnFilter)
                    this.filteredButNotLoaded = true;
                else
                    this.loadData('filterAccounts');
            }
        } else {
            this.filterShiftTypes();
        }
    }

    private getChildDim(accountDimId: number): AccountDimSmallDTO {
        return this.accountDims.find(d => d.parentAccountDimId === accountDimId);
    }

    private filterShiftTypes() {
        this.filter();

        // Reload unscheduled tasks
        this.loadUnscheduledTasksAndDeliveriesDates();
    }

    private statusFilterChanged() {
        if (this.isCalendarView)
            this.loadPeriods();
        else
            this.filter();
    }

    private filter(render: boolean = true, stopProgressWhenDone: boolean = true) {
        if (this.isTasksAndDeliveriesView)
            this.filterTasks();
        else if (this.isStaffingNeedsView)
            this.filterStaffingNeedsPeriods();
        else
            this.filterShifts('filter', render, stopProgressWhenDone);
    }

    private filterShifts(source: string, render: boolean = true, stopProgressWhenDone: boolean = true) {
        if (this.isCalendarView)
            return;

        if (this.isFiltered)
            this.clearEmployeeIdsForShiftLoad();

        // Reset filter
        const visibleShifts = this.shifts.filter(s => s.actualStartTime.isSameOrBeforeOnMinute(this.dateTo) && s.actualStopTime.isSameOrAfterOnMinute(this.dateFrom));
        const invisibleShiftIds = new Set<number>();

        // Employee
        if (this.isFilteredOnCategory || this.isFilteredOnEmployee) {
            let employeeIds = this.getFilteredEmployeeIds();
            visibleShifts.forEach(shift => {
                if (!employeeIds.includes((this.isEmployeePostView ? shift.employeePostId : shift.employeeId))) {
                    invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                }
            });
            if (this.showPlanningFollowUpTable)
                this.planningFollowUpTableData = [];
        }

        // ShiftType
        if (this.isFilteredOnShiftType || this.isFilteredOnAccountDim()) {
            visibleShifts.forEach(shift => {
                if (!this.isFilteredOnShiftType) {
                    if (!this.shiftTypes.map(s => s.id).includes(shift.shiftTypeId) && !shift.isAbsence)
                        invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                } else {
                    if (!this.selectedShiftTypes.map(s => s.id).includes(shift.shiftTypeId))
                        invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                }
            });
        }

        // Status
        if (this.isFilteredOnStatus) {
            visibleShifts.forEach(shift => {
                // Show
                if (this.isFilteredOnAnySpecifiedStatus([
                    PlanningStatusFilterItems.AbsenceRequested,
                    PlanningStatusFilterItems.AbsenceApproved,
                    PlanningStatusFilterItems.Preliminary,
                    PlanningStatusFilterItems.Wanted,
                    PlanningStatusFilterItems.Unwanted])) {
                    if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.AbsenceRequested) && (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested || shift.isAbsenceRequest)) {
                        invisibleShiftIds.delete(shift.timeScheduleTemplateBlockId);
                    } else if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.AbsenceApproved) && (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved || (this.isScenarioView && shift.timeDeviationCauseId))) {
                        invisibleShiftIds.delete(shift.timeScheduleTemplateBlockId);
                    } else if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.Preliminary) && (shift.isPreliminary && this.preliminaryPermission && !shift.isAbsenceRequest)) {
                        invisibleShiftIds.delete(shift.timeScheduleTemplateBlockId);
                    } else if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.Wanted) && shift.nbrOfWantedInQueue > 0) {
                        invisibleShiftIds.delete(shift.timeScheduleTemplateBlockId);
                    } else if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.Unwanted) && shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.Unwanted) {
                        invisibleShiftIds.delete(shift.timeScheduleTemplateBlockId);
                    } else {
                        invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                    }
                }

                // Hide
                if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.HideAbsenceRequested) && (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested || shift.isAbsenceRequest)) {
                    invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                }
                if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.HideAbsenceApproved) && (shift.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceApproved || (this.isScenarioView && shift.timeDeviationCauseId))) {
                    invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                }
                if (this.isFilteredOnSpecifiedStatus(PlanningStatusFilterItems.HidePreliminary) && (shift.isPreliminary && !shift.isAbsenceRequest)) {
                    invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                }
            });
        }

        // DeviationCause
        if (this.isFilteredOnDeviationCause) {
            visibleShifts.forEach(shift => {
                if (!this.selectedDeviationCauses.map(d => d.id).includes(shift.timeDeviationCauseId))
                    invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
            });
        }

        // BlockType
        if (this.isFilteredOnBlockType) {
            visibleShifts.forEach(shift => {
                if (!this.selectedBlockTypes.map(b => b.id).includes(shift.type)) {
                    if (this.isOrderPlanningMode) {
                        // In order planning mode, always show absence and absence requests, without showing schedule
                        if (!shift.isAbsence && !shift.isAbsenceRequest)
                            invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                    } else {
                        invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                    }
                }
            });
        }

        // Free text
        if (this.isFilteredOnFreeText) {
            visibleShifts.forEach(shift => {
                if (shift.isOrder) {
                    if ((!this.matchFreeTextFilter(shift.shiftTypeName) && !this.matchFreeTextFilter(shift.employeeName) && !this.hasFreeTextMatchOnOrder(shift)))
                        invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                } else {
                    if ((!this.matchFreeTextFilter(shift.shiftTypeName) && !this.matchFreeTextFilter(shift.employeeName)))
                        invisibleShiftIds.add(shift.timeScheduleTemplateBlockId);
                }
            });
        }

        this.shifts.forEach(shift => {
            shift.isVisible = !invisibleShiftIds.has(shift.timeScheduleTemplateBlockId);
        });
        this.setVisibleShifts();

        if ((this.isCommonDayView && this.dayViewGroupBy !== TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee) ||
            (this.isCommonScheduleView && this.scheduleViewGroupBy !== TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee))
            this.sortEmployees(false);

        this.buildShiftsMap();

        // Redraw
        this.calculateTimes();
        if (render && (this.firstLoadHasOccurred || !this.disableAutoLoad)) {
            this.render(stopProgressWhenDone);
        }
    }

    private filterTasks() {
        // Reset filter
        this.allTasks.forEach(task => {
            task.isVisible = true;
        });

        // Task type
        this.setFilteredTasks();
        if (this.isFilteredOnTaskType) {
            this.visibleTasks.forEach(task => {
                if (!this.selectedTaskTypes.map(t => t.id).includes(task.type))
                    task.isVisible = false;
            });
        }

        // TimeScheduleTaskType
        if (this.isFilteredOnTimeScheduleTaskType) {
            this.visibleTasks.forEach(task => {
                if (!this.selectedTimeScheduleTaskTypes.map(t => t.id).includes(task.typeId))
                    task.isVisible = false;
            });
        }

        // ShiftType
        if (this.isFilteredOnShiftType) {
            this.visibleTasks.forEach(task => {
                // Special for 'not specified'
                if (!task.shiftTypeId && this.selectedShiftTypes.map(s => s.id).includes(0))
                    task.isVisible = true;
                else if (!this.selectedShiftTypes.map(s => s.id).includes(task.shiftTypeId))
                    task.isVisible = false;
            });
        }

        // Task
        if (this.isFilteredOnTask) {
            this.visibleTasksOfTypeTask.forEach(task => {
                if (!this.selectedTasks.map(t => t.timeScheduleTaskId).includes(task.id))
                    task.isVisible = false;
            });
        }

        // Delivery
        if (this.isFilteredOnDelivery) {
            this.visibleTasksOfTypeDelivery.forEach(task => {
                if (!this.selectedDeliveries.map(d => d.incomingDeliveryRowId).includes(task.id))
                    task.isVisible = false;
            });
        }

        // Free text
        if (this.isFilteredOnFreeText) {
            this.visibleTasks.forEach(task => {
                if (!task.name.toLocaleLowerCase().includes(this.freeTextFilter.toLocaleLowerCase()) &&
                    !task.description.toLocaleLowerCase().includes(this.freeTextFilter.toLocaleLowerCase()))
                    task.isVisible = false;
            });
        }

        // Redraw
        this.renderBody('filterTasks');
    }

    private filterStaffingNeedsPeriods() {
        // Reset filter
        this.heads.forEach(head => {
            head.rows.forEach(row => {
                row.periods.forEach(period => period.isVisible = true);
            });
        });

        // ShiftType
        if (this.isFilteredOnShiftType || this.isFilteredOnAccountDim()) {
            this.visibleStaffingNeedsPeriods.forEach(period => {
                if (!this.isFilteredOnShiftType) {
                    if (!this.shiftTypes.map(s => s.id).includes(period.shiftTypeId))
                        period.isVisible = false;
                } else {
                    if (!this.selectedShiftTypes.map(s => s.id).includes(period.shiftTypeId))
                        period.isVisible = false;
                }
            });
        }

        // Free text
        if (this.isFilteredOnFreeText) {
            this.visibleStaffingNeedsPeriods.forEach(period => {
                if (!period.shiftTypeName.toLocaleLowerCase().includes(this.freeTextFilter.toLocaleLowerCase()))
                    period.isVisible = false;
            });
        }

        this.heads.forEach(head => {
            head.rows.forEach(row => row.isVisible = (row.visiblePeriods.length > 0));
        });

        // Redraw
        this.calculateStaffingNeedsRowSums();
        this.renderBody('filterStaffingNeedsPeriods', true);
    }

    private filterByUserSelection() {
        if (!this.selectedUserSelection || this.delayFilterByUserSelection)
            return;

        this.clearFilters(true, false, false);

        this.selectedUserSelection.selections.forEach(selection => {
            if (selection.key.startsWith(Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_ACCOUNT_DIM)) {
                let accountDimId: number = parseInt(selection.key.right(selection.key.length - Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_ACCOUNT_DIM.length), 10);
                if (accountDimId) {
                    let dim = this.accountDims.find(d => d.accountDimId === accountDimId);
                    if (dim) {
                        dim.selectedAccounts = [];
                        const filteredAccountIds = dim.filteredAccounts.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.accountId)).map(a => a.accountId);
                        if (filteredAccountIds.length > 1 && filteredAccountIds.length !== dim.filteredAccounts.length) {
                            filteredAccountIds.forEach(id => {
                                dim.selectedAccounts.push({ accountId: id });
                            });
                            this.filterAccounts(dim, true);
                        }
                    }
                }
            } else {
                switch (selection.key) {
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_SHIFT_TYPES:
                        this.selectedShiftTypes = this.shiftTypes.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_TASK_TYPES:
                        this.selectedTaskTypes = this.taskTypes.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_TIME_SCHEDULE_TASK_TYPES:
                        this.selectedTimeScheduleTaskTypes = this.timeScheduleTaskTypes.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_TASKS:
                        this.selectedTasks = this.filteredTasks.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.timeScheduleTaskId));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_DELIVERIES:
                        this.selectedDeliveries = this.deliveries.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.incomingDeliveryRowId));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_EMPLOYEE_GROUPS:
                        this.selectedEmployeeGroups = this.employeeGroups.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_CATEGORIES:
                        this.selectedCategories = this.categories.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        this.filterEmployees('filterByUserSelection', false);
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_SHOW_SECONDARY_CATEGORIES:
                        if (this.showSecondaryCategories !== (<BoolSelectionDTO>selection).value) {
                            this.showSecondaryCategories = (<BoolSelectionDTO>selection).value;
                            this.onShowSecondaryCategoriesChanged();
                        }
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_EMPLOYEE_POSTS:
                        this.selectedEmployees = this.employees.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_EMPLOYEES:
                        this.selectedEmployees = this.employees.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_SHOW_SECONDARY_ACCOUNTS:
                        if (this.showSecondaryAccounts !== (<BoolSelectionDTO>selection).value) {
                            this.showSecondaryAccounts = (<BoolSelectionDTO>selection).value;
                            this.toggleSecondaryAccounts();
                        }
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_STATUSES:
                        this.selectedStatuses = this.statuses.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_DEVIATION_CAUSES:
                        this.selectedDeviationCauses = this.deviationCauses.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_BLOCK_TYPES:
                        this.selectedBlockTypes = this.blockTypes.filter(s => (<IdListSelectionDTO>selection).ids.includes(s.id));
                        break;
                    case Constants.USER_SELECTION_KEY_SCHEDULE_PLANNING_FREE_TEXT:
                        this.freeTextFilter = (<TextSelectionDTO>selection).text;
                        break;
                }
            }
        });

        if (!this.disableAutoLoad || this.firstLoadHasOccurred) {
            this.$timeout(() => {
                let abortLoad = this.filterEmployees('loadEmployees', false);
                if (abortLoad) {
                    this.showEmployeeRemovedFromFilterMessage();
                } else {
                    this.loadData("filterByUserSelection");
                }
            }, 200);
        } else {
            this.filter();
        }
    }

    private createNormalizedDate(date: Date): Date {
        const d = new Date(date.getTime());
        d.setHours(0, 0, 0, 0);
        return d;
    }

    private createMapDateKey(date: Date): number {
        return this.createNormalizedDate(date).getTime();
    }

    private buildShiftsMap() {
        //Private helper methods-------
        const getOrCreateFromMap = <K, V>(key: K, m: Map<K, V>, creater: () => V): V => {
            return m.get(key) || m.set(key, creater()).get(key);
        };

        const maxDate = (a: Date, b: Date): Date => {
            const nA = this.createNormalizedDate(a);
            const nB = this.createNormalizedDate(b);

            return nA < nB ? nB : nA;
        };

        const minDate = (a: Date, b: Date): Date => {
            const nA = this.createNormalizedDate(a);
            const nB = this.createNormalizedDate(b);

            return nA < nB ? nA : nB;
        };
        //--------------

        this.employeeShiftsMap = new Map<number, Map<number, ShiftDTO[]>>();
        let getIdentifier = (s: ShiftDTO) => s.employeeId;
        if (this.isEmployeePostView) {
            getIdentifier = (s: ShiftDTO) => s.employeePostId;
        }

        this.shifts.forEach((shift) => {
            let employeeMap = getOrCreateFromMap(getIdentifier(shift), this.employeeShiftsMap, () => new Map<number, ShiftDTO[]>());

            const dateRangeStart = shift.actualStartDate.isBeforeOnDay(shift.actualStartTime) && shift.actualStartTime.isAfterOnDay(this.dateTo) ? shift.actualStartDate : shift.actualStartTime.beginningOfDay();
            const dateRangeEnd = shift.actualStartDate.isAfterOnDay(shift.actualStopTime) ? shift.actualStartDate : shift.actualStopTime.beginningOfDay();

            for (let curr = dateRangeStart; curr <= dateRangeEnd; curr = curr.addDays(1)) {
                const key = this.createMapDateKey(curr);
                let employeeShifts = getOrCreateFromMap(key, employeeMap, () => <ShiftDTO[]>[]);
                if (!employeeShifts.find(s => s.timeScheduleTemplateBlockId === shift.timeScheduleTemplateBlockId))
                    employeeShifts.push(shift);
            }
        });
    }

    private hasFreeTextMatchOnOrder(shift: ShiftDTO): boolean {
        if (!shift.isOrder)
            return false;

        return this.matchFreeTextFilter(shift.order.orderNr.toString()) ||
            this.matchFreeTextFilter(shift.order.customerName) ||
            this.matchFreeTextFilter(shift.order.projectName) ||
            this.matchFreeTextFilter(shift.order.workingDescription);
    }

    private matchFreeTextFilter(field: string): boolean {
        return field && field.toLocaleLowerCase().includes(this.freeTextFilter.toLocaleLowerCase());
    }

    public get isFiltered(): boolean {
        return this.isFilteredOnCategory || this.isFilteredOnEmployee || this.isFilteredOnAccountDim(true) || this.isFilteredOnEmployeeGroup || this.isFilteredOnShiftType || this.isFilteredOnStatus || this.isFilteredOnDeviationCause || this.isFilteredOnBlockType || this.isFilteredOnFreeText;
    }

    private get isFilteredOnCategory(): boolean {
        return this.selectedCategories?.length !== 0 && this.selectedCategories.length !== this.categories.length;
    }

    public get isFilteredOnEmployee(): boolean {
        return this.selectedEmployees?.length !== 0 && this.selectedEmployees.length !== this.employees.length;
    }

    public getFilteredEmployeeIds(): number[] {
        return <number[]>(((this.isFilteredOnEmployee ? this.selectedEmployees : this.employees) || []).map(e => e.id));
    }

    public getFilteredEmployeePostIds(): number[] {
        return <number[]>(((this.isFilteredOnEmployee ? this.selectedEmployees : this.employees) || []).map(e => e.id));
    }

    public getFilteredEmployeeGroupIds(): number[] {
        return <number[]>(((this.isFilteredOnEmployeeGroup ? this.selectedEmployeeGroups : this.employeeGroups) || []).map(e => e.id));
    }

    private getFilteredCategoryIds(): number[] {
        return ((this.isFilteredOnCategory ? this.selectedCategories : this.categories) || []).map(c => c.id);
    }

    public isFilteredOnAccountDim(ignoreHidden?: boolean): boolean {
        let filtered: boolean = false;

        for (let accountDim of this.accountDims) {
            if (!ignoreHidden || !accountDim['hidden']) {
                if (accountDim.selectedAccounts.length !== 0 && accountDim.selectedAccounts.length !== accountDim.accounts.length) {
                    filtered = true;
                    break;
                }
            }
        }

        return filtered;
    }

    private isFilteredOnAccountDimNr(nr: number): boolean {
        let dim = this.accountDims.find(a => a.accountDimNr === nr);
        return (dim && dim.selectedAccounts.length !== 0 && dim.selectedAccounts.length !== dim.accounts.length);
    }

    private getFilteredAccountIds(): number[] {
        let accountIds: number[] = [];

        if (this.accountDims) {
            this.accountDims.forEach(accountDim => {
                accountIds = accountIds.concat(this.getFilteredAccountDimAccountIds(accountDim));
            });
        }

        return accountIds;
    }

    private getFilteredAccountDimAccountIds(accountDim: AccountDimSmallDTO): number[] {
        return ((accountDim.selectedAccounts.length > 0 ? accountDim.selectedAccounts : accountDim.filteredAccounts) || []).map(a => a.accountId);
    }

    public get isFilteredOnEmployeeGroup(): boolean {
        return this.selectedEmployeeGroups && this.selectedEmployeeGroups.length !== 0 && this.selectedEmployeeGroups.length !== this.employeeGroups.length;
    }

    public get isFilteredOnShiftType(): boolean {
        return this.selectedShiftTypes && this.selectedShiftTypes.length !== 0 && this.selectedShiftTypes.length !== this.shiftTypes.length;
    }

    public getFilteredShiftTypeIds(): number[] {
        return <number[]>((this.isFilteredOnShiftType ? this.selectedShiftTypes : this.shiftTypes).map(s => s.id));
    }

    private get isFilteredOnStatus(): boolean {
        return this.selectedStatuses && this.selectedStatuses.length !== 0 && this.selectedStatuses.length !== this.statuses.length;
    }

    private isFilteredOnSpecifiedStatus(status: PlanningStatusFilterItems): boolean {
        return this.getFilteredStatusIds().includes(status);
    }

    private isFilteredOnAnySpecifiedStatus(statuses: PlanningStatusFilterItems[]): boolean {
        return _.intersection(statuses, this.getFilteredStatusIds()).length > 0;
    }

    private getFilteredStatusIds(): number[] {
        return this.selectedStatuses.map(s => s.id);
    }

    private get isFilteredOnDeviationCause(): boolean {
        return this.selectedDeviationCauses && this.selectedDeviationCauses.length !== 0 && this.selectedDeviationCauses.length !== this.deviationCauses.length;
    }

    public getFilteredDeviationCauseIds(): number[] {
        return <number[]>((this.isFilteredOnDeviationCause ? this.selectedDeviationCauses : this.deviationCauses).map(d => d.id));
    }

    private get isFilteredOnBlockType(): boolean {
        return this.selectedBlockTypes && this.selectedBlockTypes.length !== 0 && this.selectedBlockTypes.length !== this.blockTypes.length;
    }

    private getFilteredBlockTypes(): number[] {
        return <number[]>((this.isFilteredOnBlockType ? this.selectedBlockTypes : this.blockTypes).map(b => b.id));
    }

    private get isFilteredOnBlockTypeStandbyOnly(): boolean {
        let types: number[] = this.getFilteredBlockTypes();
        return types.length === 1 && types[0] == TermGroup_TimeScheduleTemplateBlockType.Standby;
    }

    private get isFilteredOnFreeText(): boolean {
        return this.freeTextFilter && this.freeTextFilter.length > 0 || false;
    }

    private setAllShiftsMap() {
        this.allShiftsMap.clear();
        for (const shift of this.shifts) {
            const key = this.getShiftKey(shift.timeScheduleTemplateBlockId, shift.isAbsenceRequest);
            this.allShiftsMap.set(key, shift);
        }
    }

    private clearShifts() {
        this.shifts = [];
        this.setAllShiftsMap();
    }

    private setVisibleShifts() {
        this.visibleShiftsMap.clear();
        this.visibleEmployeeIdsSet.clear();

        this.visibleShifts = this.shifts.filter(s => s.isVisible === true && s.actualStartTime.isSameOrBeforeOnMinute(this.dateTo) && s.actualStopTime.isSameOrAfterOnMinute(this.dateFrom));

        for (const shift of this.visibleShifts) {
            const key = this.getShiftKey(shift.timeScheduleTemplateBlockId, shift.isAbsenceRequest);
            this.visibleShiftsMap.set(key, shift);

            const id = this.isEmployeePostView ? shift.employeePostId : shift.employeeId;
            if (!this.visibleEmployeeIdsSet.has(id))
                this.visibleEmployeeIdsSet.add(id);
        }
    }

    public getShiftKey(shiftId: number, isAbsenceRequest: boolean): string {
        return `${shiftId}_${isAbsenceRequest ? 1 : 0}`;
    }

    // Tasks and deliveries

    private get isFilteredOnTask(): boolean {
        return this.selectedTasks?.length !== 0 && this.selectedTasks.length !== this.allTasks.length;
    }

    private get isFilteredOnDelivery(): boolean {
        return this.selectedDeliveries?.length !== 0 && this.selectedDeliveries.length !== this.deliveries.length;
    }

    private get isFilteredOnTaskType(): boolean {
        return this.selectedTaskTypes?.length !== 0 && this.selectedTaskTypes.length !== this.taskTypes.length;
    }

    private get isFilteredOnTimeScheduleTaskType(): boolean {
        return this.selectedTimeScheduleTaskTypes?.length !== 0 && this.selectedTimeScheduleTaskTypes.length !== this.timeScheduleTaskTypes.length;
    }

    public getFilteredTaskTypeIds(): number[] {
        return <number[]>(((this.isFilteredOnTaskType ? this.selectedTaskTypes : this.taskTypes) || []).map(t => t.id));
    }

    public getFilteredTimeScheduleTaskTypeIds(): number[] {
        return <number[]>(((this.isFilteredOnTimeScheduleTaskType ? this.selectedTimeScheduleTaskTypes : this.timeScheduleTaskTypes) || []).map(t => t.id));
    }

    private get visibleTasks(): StaffingNeedsTaskDTO[] {
        return this.allTasks.filter(t => t.isVisible);
    }

    private getSelectedTaskIds(): number[] {
        return this.selectedTasks.map(t => t.timeScheduleTaskId);
    }

    private getSelectedDeliveryIds(): number[] {
        return this.selectedDeliveries.map(d => d.incomingDeliveryRowId);
    }

    public get tasksOfTypeTask(): StaffingNeedsTaskDTO[] {
        return this.allTasks.filter(t => t.isTask);
    }

    private get visibleTasksOfTypeTask(): StaffingNeedsTaskDTO[] {
        return this.allTasks.filter(t => t.isTask && t.isVisible);
    }

    public get tasksOfTypeDelivery(): StaffingNeedsTaskDTO[] {
        return this.allTasks.filter(t => t.isDelivery);
    }

    private get visibleTasksOfTypeDelivery(): StaffingNeedsTaskDTO[] {
        return this.allTasks.filter(t => t.isDelivery && t.isVisible);
    }

    private setFilteredTasks() {
        this.filteredTasks = this.isFilteredOnTimeScheduleTaskType ? this.tasks.filter(t => this.getFilteredTimeScheduleTaskTypeIds().includes(t.timeScheduleTaskTypeId)) : this.tasks;
    }

    public get visibleStaffingNeedsRows(): StaffingNeedsRowDTO[] {
        let rows: StaffingNeedsRowDTO[] = [];
        this.heads.forEach(head => {
            rows = rows.concat(head.rows.filter(r => r.isVisible));
        });

        return rows;
    }

    private get visibleStaffingNeedsPeriods(): StaffingNeedsRowPeriodDTO[] {
        let periods: StaffingNeedsRowPeriodDTO[] = [];
        this.heads.forEach(head => {
            head.rows.forEach(row => {
                periods = _.concat(periods, row.visiblePeriods);
            });
        });

        return periods;
    }

    // Employee list filter

    public getEmployeeById(employeeId: number): EmployeeListDTO {
        return this.allEmployees.find(e => e.employeeId === employeeId && e.employeePostId === 0);
    }

    public getEmployeePostById(employeePostId: number): EmployeeListDTO {
        return this.allEmployees.find(e => e.employeePostId === employeePostId);
    }

    private clearEmployeeListFilters() {
        this.employeeListFreeTextFilter = '';

        if (this.isEmployeePostView) {
            this.filterEmployeeListOnSkills = false;
            this.filterEmployeeListOnPercent = false;
            this.onEmployeeListFreeTextFiltered();
        } else {
            this.$scope.$broadcast('clearEmployeeFilters', null);
        }
    }

    private filteringEmployeesDone(employees: IAvailableEmployeesDTO[]) {
        this.filterEmployeeList(employees);
    }

    private filterEmployeeList(employees: IAvailableEmployeesDTO[]) {
        if (!this.showEmployeeList || this.isCalendarView || this.isTemplateView)
            return;

        this.filteringEmployees = true;
        this.employeesWantsExtraShifts = false;
        this.copyAllEmployeeLists();

        if (!employees)
            employees = [];

        if (this.isEmployeePostView) {
            // Hide/show assigned
            let assignedEmployeeIds: number[] = this.allEmployees.filter(e => e.employeePostId && e.employeeId).map(e => e.employeeId);
            if (this.employeeListHideAssignedToPost) {
                // Hide assigned employees
                this.employeeList = this.employeeList.filter(el => !assignedEmployeeIds.includes(el.employeeId));
            } else {
                // Set EmployeePostId on assigned employees
                this.employeeList.filter(el => assignedEmployeeIds.includes(el.employeeId)).forEach(emp => {
                    let fullEmp = this.allEmployees.find(e => e.employeeId === emp.employeeId && e.employeePostId);
                    if (fullEmp)
                        emp.employeePostId = fullEmp.employeePostId;
                });
            }

            // Show employees based on selected employee post
            if (this.employeeListFilterOnSelectedEmployeePost && this.employeeListFilterEmployeePostId) {
                let employeePost: EmployeeListDTO = this.getEmployeePostById(this.employeeListFilterEmployeePostId);
                if (employeePost) {
                    let validEmployeeIds: number[] = [];
                    let filterOnSkills = this.filterEmployeeListOnSkills && employeePost.employeeSkills && employeePost.employeeSkills.length > 0;

                    this.employeeList.forEach(emp => {
                        let skillInvalid = false;
                        if (filterOnSkills) {
                            for (let empPostSkill of employeePost.employeeSkills) {
                                if (!emp.employeeSkills || emp.employeeSkills.length === 0) {
                                    skillInvalid = true;
                                    break;
                                }
                                if (emp.employeeSkills.filter(e => e.skillId === empPostSkill.skillId && e.skillLevel >= empPostSkill.skillLevel && (!e.dateTo || e.dateTo.isSameOrAfterOnDay(this.dateFrom))).length === 0) {
                                    skillInvalid = true;
                                    break;
                                }
                            }
                        }

                        let percentInvalid: boolean = false;
                        if (this.filterEmployeeListOnPercent && !skillInvalid) {
                            let empPostPercent: number = this.getCurrentPercent(employeePost);
                            let empPercent: number = this.getEmployeeRightListCurrentPercent(emp);
                            emp.percentDiff = empPercent - empPostPercent;
                            emp.hasValidatedPercent = true;
                            if (Math.abs(emp.percentDiff) > this.filterEmployeeListOnPercentDiff)
                                percentInvalid = true;
                        }

                        if (!skillInvalid && !percentInvalid)
                            validEmployeeIds.push(emp.employeeId);
                    });

                    this.employeeList = this.employeeList.filter(el => validEmployeeIds.includes(el.employeeId));
                }
            }
        } else {
            if (this.employeeListFilterOnSelectedShift)
                this.employeeList = this.employeeList.filter(el => employees.map(e => e.employeeId).includes(el.employeeId));

            // Set wantsExtraShifts flag
            employees.filter(e => e.wantsExtraShifts).forEach(employee => {
                let empList = this.employeeList.find(e => e.employeeId === employee.employeeId);
                if (empList)
                    empList.wantsExtraShifts = true;
                if (!this.employeesWantsExtraShifts)
                    this.employeesWantsExtraShifts = true;
            });
        }

        // Free text (number and name)
        if (this.isEmployeeListFilteredOnFreeText)
            this.employeeList = this.employeeList.filter(e => e.employeeNr.contains(this.employeeListFreeTextFilter, false) || e.name.contains(this.employeeListFreeTextFilter, false));

        if (this.selectableInformationSettings.showAvailability)
            this.setAvailabilityOnEmployees();
        this.sortEmployeeList();
        this.filteringEmployees = false;
    }

    private get isEmployeeListFilteredOnFreeText() {
        return this.employeeListFreeTextFilter && this.employeeListFreeTextFilter.length > 0 || false;
    }

    // Order list

    private copyAllUnscheduledOrders() {
        // Copy all unscheduled orders to the filtered collection
        this.orderList = [];
        this.allUnscheduledOrders.forEach(order => {
            let clone = new OrderListDTO();
            angular.extend(clone, order);
            clone.fixDates();
            this.orderList.push(clone);
        });
    }

    private filterOrderListFromGui() {
        this.$timeout(() => {
            this.filterOrderList();
        });
    }

    private filterOrderList() {
        this.copyAllUnscheduledOrders();

        // Remove orders without specified shift type
        if (this.orderListFilterOnShiftType)
            _.pullAll(this.orderList, this.orderList.filter(o => o.shiftTypeId !== this.orderListFilterOnShiftType));

        // Free text
        if (this.orderListFreeTextFilter)
            this.orderList = this.orderList.filter(o => (o.orderNr && o.orderNr.toString().contains(this.orderListFreeTextFilter, false)) ||
                (o.customerName && o.customerName.toLocaleLowerCase().contains(this.orderListFreeTextFilter, false)) ||
                (o.projectName && o.projectName.toLocaleLowerCase().contains(this.orderListFreeTextFilter, false)) ||
                (o.workingDescription && o.workingDescription.toLocaleLowerCase().contains(this.orderListFreeTextFilter, false)) ||
                (o.categoryString && o.categoryString.toLocaleLowerCase().contains(this.orderListFreeTextFilter, false)) ||
                (o.deliveryAddress && o.deliveryAddress.toLocaleLowerCase().contains(this.orderListFreeTextFilter, false)));

        this.sortOrderList();

        this.scheduleHandler.setOrderListHeight(false);
    }

    private clearOrderListFilters() {
        this.orderListFilterOnShiftType = 0;
        this.orderListFreeTextFilter = '';
        this.filterOrderList();
    }

    // Grouping and sorting

    private sortEmployees(render: boolean) {
        // Remove all "group header employees"
        this.employedEmployees = this.employedEmployees.filter(e => !e.isGroupHeader);
        this.clearAccountOnEmployees();

        if (this.isCommonDayView) {
            // Day view
            switch (this.dayViewSortBy) {
                case TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname:
                    if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    } else {
                        if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Category) {
                            this.setCategoryOnEmployees();
                        } else if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType) {
                            this.setShiftTypeOnEmployees();
                        } else if (this.dayViewGroupBy > 10) {
                            this.setAccountOnEmployees();
                        }
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
                case TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname:
                    if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'lastName', 'firstName', 'employeeNrSort'], ['desc', 'asc']);
                    } else {
                        if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Category) {
                            this.setCategoryOnEmployees();
                        } else if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType) {
                            this.setShiftTypeOnEmployees();
                        } else if (this.dayViewGroupBy > 10) {
                            this.setAccountOnEmployees();
                        }
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'lastName', 'firstName', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
                case TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr:
                    if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'employeeNrSort'], ['desc', 'asc']);
                    } else {
                        if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Category) {
                            this.setCategoryOnEmployees();
                        } else if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType) {
                            this.setShiftTypeOnEmployees();
                        } else if (this.dayViewGroupBy > 10) {
                            this.setAccountOnEmployees();
                        }
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
                case TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime:
                    this.setStartTimeOnEmployees();
                    if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'startTime', 'stopTime', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    } else {
                        if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.Category) {
                            this.setCategoryOnEmployees();
                        } else if (this.dayViewGroupBy === TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType) {
                            this.setShiftTypeOnEmployees();
                        } else if (this.dayViewGroupBy > 10) {
                            this.setAccountOnEmployees();
                        }
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'startTime', 'stopTime', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
            }

            this.employedEmployees.forEach(employee => {
                employee.name = (this.dayViewSortBy === TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname && !employee.hidden ? employee.lastName + " " + employee.firstName : employee.firstName + " " + employee.lastName);
            });
        } else if (this.isCommonScheduleView) {
            // Schedule view
            switch (this.scheduleViewSortBy) {
                case TermGroup_TimeSchedulePlanningScheduleViewSortBy.Firstname:
                    if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Category) {
                        this.setCategoryOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.ShiftType) {
                        this.setShiftTypeOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy > 10) {
                        this.setAccountOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'firstName', 'lastName', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
                case TermGroup_TimeSchedulePlanningScheduleViewSortBy.Lastname:
                    if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'lastName', 'firstName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Category) {
                        this.setCategoryOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'lastName', 'firstName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.ShiftType) {
                        this.setShiftTypeOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'lastName', 'firstName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy > 10) {
                        this.setAccountOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'lastName', 'firstName', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
                case TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr:
                    if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Employee) {
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.Category) {
                        this.setCategoryOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy === TermGroup_TimeSchedulePlanningScheduleViewGroupBy.ShiftType) {
                        this.setShiftTypeOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'groupName', 'employeeNrSort'], ['desc', 'asc']);
                    } else if (this.scheduleViewGroupBy > 10) {
                        this.setAccountOnEmployees();
                        this.employedEmployees = _.orderBy(this.employedEmployees, ['hidden', 'vacant', 'employeeNrSort'], ['desc', 'asc']);
                    }
                    break;
            }

            this.employedEmployees.forEach(employee => {
                employee.name = (this.scheduleViewSortBy === TermGroup_TimeSchedulePlanningScheduleViewSortBy.Lastname && !employee.hidden ? employee.lastName + " " + employee.firstName : employee.firstName + " " + employee.lastName);
            });
        }

        this.resortEmployeeFilter();
        if (render) {
            this.progressMessage = this.terms["time.schedule.planning.loadscheduleprogress.render"];
            this.progressBusy = true;
            this.calculateTimes();
            this.renderBody('sortEmployees');
        }
    }

    private sortEmployeeList() {
        switch (this.employeeListSortBy) {
            case PlanningEmployeeListSortBy.Firstname:
                this.employeeList = _.orderBy(this.employeeList, ['firstName', 'lastName', 'employeeNrSort']);
                break;
            case PlanningEmployeeListSortBy.Lastname:
                this.employeeList = _.orderBy(this.employeeList, ['lastName', 'firstName', 'employeeNrSort']);
                break;
            case PlanningEmployeeListSortBy.EmployeeNr:
                this.employeeList = _.orderBy(this.employeeList, ['employeeNrSort']);
                break;
            case PlanningEmployeeListSortBy.Availability:
                this.employeeList = _.orderBy(this.employeeList, ['availabilitySort', 'firstName', 'lastName', 'employeeNrSort']);
                break;
        }

        this.employeeList.forEach(employee => {
            employee.name = (this.employeeListSortBy === PlanningEmployeeListSortBy.Lastname ? employee.lastName + " " + employee.firstName : employee.firstName + " " + employee.lastName);
        });
    }

    private sortOrderList() {
        this.unscheduledOrderDates = [];
        let dateField: string = '';

        switch (this.orderListSortBy) {
            case PlanningOrderListSortBy.Priority:
                this.orderList = _.orderBy(this.orderList, ['priority', 'orderNr']);
                break;
            case PlanningOrderListSortBy.PlannedStartDate:
                this.orderList = _.orderBy(this.orderList, ['plannedStartDate', 'orderNr']);
                dateField = 'plannedStartDate';
                break;
            case PlanningOrderListSortBy.PlannedStopDate:
                this.orderList = _.orderBy(this.orderList, ['plannedStopDate', 'orderNr']);
                dateField = 'plannedStopDate';
                break;
            case PlanningOrderListSortBy.RemainingTime:
                this.orderList = _.orderBy(this.orderList, ['remainingTime', 'orderNr']);
                break;
            case PlanningOrderListSortBy.OrderNr:
                this.orderList = _.orderBy(this.orderList, ['orderNr']);
                break;
        }

        if (dateField) {
            this.orderList.filter(o => o[dateField]).map(o => o[dateField]).forEach(date => {
                if (!CalendarUtility.includesDate(this.unscheduledOrderDates, date))
                    this.unscheduledOrderDates.push(date);
            });
            this.unscheduledOrderDates.forEach(date => {
                let dateOrders = this.orderList.filter(t => t[dateField]?.isSameDayAs(date));
                date['label'] = dateOrders.length;
            });

            this.currentUnscheduledOrderDates = this.unscheduledOrderDates.filter(d => d.isSameOrAfterOnDay(this.dateFrom) && d.isSameOrBeforeOnDay(this.dateTo));

            this.notCurrentUnscheduledOrderDates = [];
            this.unscheduledOrderDates.filter(d => d.isBeforeOnDay(this.dateFrom) || d.isAfterOnDay(this.dateTo)).forEach(date => {
                let monthDate = date.beginningOfMonth();
                if (!CalendarUtility.includesDate(this.notCurrentUnscheduledOrderDates, monthDate))
                    this.notCurrentUnscheduledOrderDates.push(monthDate);
            });
            this.notCurrentUnscheduledOrderDates.forEach(date => {
                let dateOrders = this.orderList.filter(t => t[dateField]?.isSameMonthAs(date));
                date['label'] = dateOrders.length;
            });
        }
    }

    private sortTasks(render: boolean) {
        if (this.isTasksAndDeliveriesDayView) {
            // Day view
            switch (this.tadDayViewSortBy) {
                case TermGroup_StaffingNeedsDayViewSortBy.Name:
                    switch (this.tadDayViewGroupBy) {
                        case TermGroup_StaffingNeedsDayViewGroupBy.None:
                            this.allTasks = _.orderBy(this.allTasks, 'name');
                            break;
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim2:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim3:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim4:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim5:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim6:
                            this.setDepartmentOnTasks(this.tadDayViewGroupBy + 1);
                            this.allTasks = _.orderBy(this.allTasks, ['departmentGroup', 'name']);
                            break;
                        case TermGroup_StaffingNeedsDayViewGroupBy.ShiftType:
                            this.setShiftTypeOnTasks();
                            this.allTasks = _.orderBy(this.allTasks, ['shiftTypeGroup', 'name']);
                            break;
                    }
                    break;
                case TermGroup_StaffingNeedsDayViewSortBy.StartTime:
                    switch (this.tadDayViewGroupBy) {
                        case TermGroup_StaffingNeedsDayViewGroupBy.None:
                            this.allTasks = _.orderBy(this.allTasks, ['actualStartTime', 'name']);
                            break;
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim2:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim3:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim4:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim5:
                        case TermGroup_StaffingNeedsDayViewGroupBy.AccountDim6:
                            this.setDepartmentOnTasks(this.tadDayViewGroupBy + 1);
                            this.allTasks = _.orderBy(this.allTasks, ['departmentGroup', 'actualStartTime', 'name']);
                            break;
                        case TermGroup_StaffingNeedsDayViewGroupBy.ShiftType:
                            this.setShiftTypeOnTasks();
                            this.allTasks = _.orderBy(this.allTasks, ['shiftTypeGroup', 'actualStartTime', 'name']);
                            break;
                    }
                    break;
            }
        } else if (this.isTasksAndDeliveriesScheduleView) {
            // Schedule view
            switch (this.tadScheduleViewGroupBy) {
                case TermGroup_StaffingNeedsScheduleViewGroupBy.None:
                    this.allTasks = _.orderBy(this.allTasks, ['type', 'headName', 'actualStartTime']);
                    break;
                case TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim2:
                case TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim3:
                case TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim4:
                case TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim5:
                case TermGroup_StaffingNeedsScheduleViewGroupBy.AccountDim6:
                    this.setDepartmentOnTasks(this.tadScheduleViewGroupBy + 1);
                    this.allTasks = _.orderBy(this.allTasks, ['departmentGroup', 'type', 'headName', 'actualStartTime']);
                    break;
                case TermGroup_StaffingNeedsScheduleViewGroupBy.ShiftType:
                    this.setShiftTypeOnTasks();
                    this.allTasks = _.orderBy(this.allTasks, ['shiftTypeGroup', 'type', 'headName', 'actualStartTime']);
                    break;
                default:
                    break;
            }
        }

        if (render)
            this.renderBody('sortTasks');
    }

    // Calculations

    private calculateTimes() {
        if (this.recalculateEmployeeWorkTimes) {
            this.recalculateEmployeeWorkTimes = false;
            this.calculateEmployeesWorkTimes();
        }

        if (this.selectableInformationSettings.followUpOnNeed || this.selectableInformationSettings.followUpOnNeedFrequency || this.selectableInformationSettings.followUpOnNeedRowFrequency || this.selectableInformationSettings.followUpOnBudget || this.selectableInformationSettings.followUpOnForecast || this.selectableInformationSettings.followUpOnTemplateSchedule || this.selectableInformationSettings.followUpOnTemplateScheduleForEmployeePost || this.selectableInformationSettings.followUpOnSchedule || this.selectableInformationSettings.followUpOnTime)
            this.calculateStaffingNeed();

        // Grand total for all employees in visible date range
        this.plannedMinutesSum = [];
        this.factorMinutesSum = [];
        this.grossMinutesSum = [];
        this.totalCostSum = [];
        this.totalCostIncEmpTaxAndSupplementChargeSum = [];

        let filteredAccountIds: number[];
        if (this.useAccountHierarchy)
            filteredAccountIds = this.getFilteredAccountIds();

        if (this.isCommonDayView) {
            // Total number of visible shifts for each interval in visible date range
            this.timeShifts = [];
            let currentStartTime = this.dateFrom;
            while (currentStartTime.isBeforeOnMinute(this.dateTo)) {
                let currentStopTime = currentStartTime.addMinutes(this.dayViewMinorTickLength);
                let nbrOfShifts: number = 0;
                let groupedShifts: { groupName: string, nbrOfShifts: number }[] = [];
                let shifts = this.visibleShifts.filter(s => s.actualStartTime.isBeforeOnMinute(currentStopTime) && s.actualStopTime.isAfterOnMinute(currentStartTime) && !s.timeDeviationCauseId && (!s.isLended || this.selectableInformationSettings.includeLendedShiftsInTimeCalculations) && (!s.isOtherAccount || this.selectableInformationSettings.includeLendedShiftsInTimeCalculations) && !s.isOnDuty && !s.isLeisureCode);
                shifts = shifts.filter(s => this.isFilteredOnBlockTypeStandbyOnly ? s.isStandby : !s.isStandby);
                shifts.forEach(shift => {
                    // Check each shift in time slot to see if there are any breaks that span over the whole slot.
                    // In that case do not count it.
                    let hasBreak: boolean = ((shift.break1StartTime.isSameOrBeforeOnMinute(currentStartTime) && shift.break1StartTime.addMinutes(shift.break1Minutes).isSameOrAfterOnMinute(currentStopTime)) ||
                        (shift.break2StartTime.isSameOrBeforeOnMinute(currentStartTime) && shift.break2StartTime.addMinutes(shift.break2Minutes).isSameOrAfterOnMinute(currentStopTime)) ||
                        (shift.break3StartTime.isSameOrBeforeOnMinute(currentStartTime) && shift.break3StartTime.addMinutes(shift.break3Minutes).isSameOrAfterOnMinute(currentStopTime)) ||
                        (shift.break4StartTime.isSameOrBeforeOnMinute(currentStartTime) && shift.break4StartTime.addMinutes(shift.break4Minutes).isSameOrAfterOnMinute(currentStopTime)));

                    if (!hasBreak) {
                        nbrOfShifts++;
                        if (this.isGrouped) {
                            let group: { groupName: string, nbrOfShifts: number };
                            if (this.isGroupedByAccount && shift.accountName) {
                                group = groupedShifts.find(g => g.groupName === shift.accountName);
                                if (!group) {
                                    group = { groupName: shift.accountName, nbrOfShifts: 0 };
                                    groupedShifts.push(group);
                                }
                            } else if (this.isGroupedByCategory) {
                                let employee = this.getEmployeeById(shift.employeeId);
                                if (employee?.['category']) {
                                    let categoryName: string = employee['category'];
                                    if (_.endsWith(categoryName, '__'))
                                        categoryName = categoryName.left(categoryName.length - 2);
                                    group = groupedShifts.find(g => g.groupName === categoryName);
                                    if (!group) {
                                        group = { groupName: categoryName, nbrOfShifts: 0 };
                                        groupedShifts.push(group);
                                    }
                                }
                            } else if (this.isGroupedByShiftType && shift.shiftTypeName) {
                                group = groupedShifts.find(g => g.groupName === shift.shiftTypeName);
                                if (!group) {
                                    group = { groupName: shift.shiftTypeName, nbrOfShifts: 0 };
                                    groupedShifts.push(group);
                                }
                            }
                            if (group)
                                group.nbrOfShifts++;
                        }
                    }
                });
                this.timeShifts.push({ time: currentStartTime.diffMinutes(currentStartTime.beginningOfDay()), nbrOfShifts: nbrOfShifts, groupedShifts: groupedShifts });
                currentStartTime = currentStopTime;
            }

            // Totals for all employees in the whole visible date range
            let totalPlannedMinutes: number = 0;
            let totalFactorMinutes: number = 0;
            let totalGrossMinutes: number = 0;
            let totalCostSum: number = 0;
            let totalCostIncEmpTaxAndSuppChargeSum: number = 0;

            let filteredEmployeeIds: number[] = this.getFilteredEmployeeIds();
            for (let employee of this.employedEmployees) {
                if (filteredEmployeeIds.includes(employee.identifier)) {
                    employee.hasTimeScheduleTypeIsNotScheduleTime = false;

                    // Time and cost for current employee
                    let plannedMinutes: number = 0;
                    let factorMinutes: number = 0;
                    let absenceMinutes: number = 0;
                    let grossMinutes: number = 0;
                    let totalCost: number = 0;
                    let totalCostIncEmpTaxAndSuppCharge: number = 0;

                    // Get shifts for current employee
                    let empShifts = this.visibleShifts.filter(s => (this.isEmployeePostView ? s.employeePostId === employee.employeePostId : s.employeeId === employee.employeeId) && (!s.isLended || this.selectableInformationSettings.includeLendedShiftsInTimeCalculations) && (!s.isOtherAccount || this.selectableInformationSettings.includeLendedShiftsInTimeCalculations) && !s.isOnDuty);
                    empShifts = empShifts.filter(s => this.isFilteredOnBlockTypeStandbyOnly ? s.isStandby : !s.isStandby);
                    for (let shift of empShifts) {
                        // Make sure shift is in visible range
                        if (shift.actualStartDate.isSameOrAfterOnDay(this.dateFrom) && shift.actualStartDate.isSameOrBeforeOnDay(this.dateTo) && (this.isTemplateView || employee.hasEmployment(shift.actualStartTime, shift.actualStopTime))) {
                            // Need to set this flag to be able to show employees with only these kind of shifts, even if planned time gets zero
                            if (shift.timeScheduleTypeIsNotScheduleTime)
                                employee.hasTimeScheduleTypeIsNotScheduleTime = true;

                            // Do not calculate schedule in order planning
                            if (this.isOrderPlanningMode && !shift.isOrder && !shift.isBooking)
                                continue;

                            // Do not calculate whole day absence
                            if (shift.isWholeDayAbsence)
                                continue;

                            // Absence
                            if (shift.isAbsence) {
                                absenceMinutes += shift.getShiftLength();
                                absenceMinutes -= shift.getBreakTimeWithinShift();
                            } else {
                                let includePlannedShift: boolean = true;
                                if (this.useAccountHierarchy && !filteredAccountIds.includes(shift.accountId) && shift.shiftStatus == TermGroup_TimeScheduleTemplateBlockShiftStatus.Open) {
                                    includePlannedShift = false;
                                }
                                if (includePlannedShift) {
                                    // Planned minutes for current shift
                                    plannedMinutes += shift.getShiftLength();
                                    if (!shift.isBreak) {
                                        // Gross time for current shift
                                        if (this.selectableInformationSettings.showGrossTime)
                                            grossMinutes += shift.grossTime;

                                        // Total cost for current shift
                                        if (this.selectableInformationSettings.showTotalCost || this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge) {
                                            totalCost += shift.totalCost;
                                            totalCostIncEmpTaxAndSuppCharge += shift.totalCostIncEmpTaxAndSuppCharge;
                                        }
                                    }

                                    // Breaks within current shift
                                    plannedMinutes -= shift.getBreakTimeWithinShift();

                                    factorMinutes += shift.getTimeScheduleTypeFactorsWithinShift();
                                }
                            }
                        }
                    }
                    employee.plannedMinutes = plannedMinutes + absenceMinutes;
                    employee.grossMinutes = grossMinutes;
                    employee.totalCost = totalCost;
                    employee.totalCostIncEmpTaxAndSuppCharge = totalCostIncEmpTaxAndSuppCharge;
                    this.setEmployeeToolTip(employee);

                    totalPlannedMinutes += plannedMinutes;
                    totalFactorMinutes += factorMinutes;
                    totalGrossMinutes += grossMinutes;
                    totalCostSum += totalCost;
                    totalCostIncEmpTaxAndSuppChargeSum += totalCostIncEmpTaxAndSuppCharge
                }
            }

            this.addScheduleViewSummary(this.dateFrom, [{ date: this.dateFrom, minutes: totalPlannedMinutes }], [{ date: this.dateFrom, minutes: totalFactorMinutes }], [{ date: this.dateFrom, minutes: totalGrossMinutes }], [{ date: this.dateFrom, cost: totalCostSum }], [{ date: this.dateFrom, costIncEmpTaxAndSuppCharge: totalCostIncEmpTaxAndSuppChargeSum }]);
        } else if (this.isCommonScheduleView) {
            // Totals for all employees in the whole visible date range
            let totalPlannedMinutes: number = 0;
            let totalWorkTimeMinutes: number = 0;
            let totalGrossMinutes: number = 0;
            let totalCostSum: number = 0;
            let totalCostIncEmpTaxAndSuppChargeSum: number = 0;

            // Totals for all employees for each date in visible date range
            let dailyPlannedMinutes: any[] = [];
            let dailyFactorMinutes: any[] = [];
            let dailyGrossMinutes: any[] = [];
            let dailyTotalCost: any[] = [];
            let dailyTotalCostIncEmpTaxAndSuppCharge: any[] = [];

            let filteredEmployeeIds: number[] = this.getFilteredEmployeeIds();
            for (let employee of this.employedEmployees) {
                if (filteredEmployeeIds.includes(employee.identifier)) {
                    employee.hasTimeScheduleTypeIsNotScheduleTime = false;

                    // Time and cost for current employee
                    let plannedMinutes: number = 0;
                    let shiftFactorMinutes: number = 0;
                    let employeeFactorMinutes: number = 0;
                    let absenceMinutes: number = 0;
                    let grossMinutes: number = 0;
                    let totalCost: number = 0;
                    let totalCostIncEmpTaxAndSuppCharge: number = 0;

                    // Get shifts for current employee
                    let empShifts = this.visibleShifts.filter(s => (this.isEmployeePostView ? s.employeePostId === employee.employeePostId : s.employeeId === employee.employeeId) && (!s.isLended || this.selectableInformationSettings.includeLendedShiftsInTimeCalculations) && (!s.isOtherAccount || this.selectableInformationSettings.includeLendedShiftsInTimeCalculations) && !s.isOnDuty && !s.isLeisureCode);

                    empShifts = empShifts.filter(s => this.isFilteredOnBlockTypeStandbyOnly ? s.isStandby : !s.isStandby);
                    for (let shift of empShifts) {
                        // Make sure shift is in visible range
                        if (shift.actualStartDate.isSameOrAfterOnDay(this.dateFrom) && shift.actualStartDate.isSameOrBeforeOnDay(this.dateTo) && (this.isTemplateView || employee.hasEmployment(shift.actualStartTime, shift.actualStopTime))) {

                            // Need to set this flag to be able to show employees with only these kind of shifts, even if planned time gets zero
                            if (shift.timeScheduleTypeIsNotScheduleTime)
                                employee.hasTimeScheduleTypeIsNotScheduleTime = true;

                            // Do not calculate schedule in order planning
                            if (this.isOrderPlanningMode && !shift.isOrder && !shift.isBooking)
                                continue;

                            // Do not calculate whole day absence
                            if (shift.isWholeDayAbsence)
                                continue;

                            // TimeScheduleType factor multiplyer
                            shiftFactorMinutes = shift.getTimeScheduleTypeFactorsWithinShift();
                            employeeFactorMinutes += shiftFactorMinutes;

                            let date: Date = shift.actualStartDate;
                            if (employee.hasEmployment(date, date)) {
                                if (shift.isAbsence) {
                                    absenceMinutes += shift.getShiftLength();
                                    absenceMinutes -= shift.getBreakTimeWithinShift();
                                    plannedMinutes += shiftFactorMinutes;

                                    if (!shift.isBreak) {
                                        // Total cost for current shift
                                        if (this.selectableInformationSettings.showTotalCost || this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge) {
                                            let shiftTotalCost: number = shift.totalCost;
                                            totalCost += shiftTotalCost;

                                            let dailyTotal = dailyTotalCost.find(d => d.date.isSameDayAs(date));
                                            if (dailyTotal)
                                                dailyTotal.cost += shiftTotalCost;
                                            else
                                                dailyTotalCost.push({ date: date, cost: shiftTotalCost });

                                            let shiftTotalCostIncEmpTaxAndSuppCharge: number = shift.totalCostIncEmpTaxAndSuppCharge;
                                            totalCostIncEmpTaxAndSuppCharge += shiftTotalCostIncEmpTaxAndSuppCharge;

                                            let dailyTotalIncTaxes = dailyTotalCostIncEmpTaxAndSuppCharge.find(d => d.date.isSameDayAs(date));
                                            if (dailyTotalIncTaxes)
                                                dailyTotalIncTaxes.costIncEmpTaxAndSuppCharge += shiftTotalCostIncEmpTaxAndSuppCharge;
                                            else
                                                dailyTotalCostIncEmpTaxAndSuppCharge.push({ date: date, costIncEmpTaxAndSuppCharge: shiftTotalCostIncEmpTaxAndSuppCharge });
                                        }
                                    }
                                } else {

                                    let includePlannedShift: boolean = true;
                                    if (this.useAccountHierarchy && !filteredAccountIds.includes(shift.accountId) && shift.shiftStatus == TermGroup_TimeScheduleTemplateBlockShiftStatus.Open) {
                                        includePlannedShift = false;
                                    }

                                    if (includePlannedShift) {

                                        // Planned time for current shift
                                        let shiftPlannedMinutes: number = shift.getShiftLength();

                                        // Breaks within current shift
                                        let breakMinutes: number = shift.getBreakTimeWithinShift();
                                        shiftPlannedMinutes -= breakMinutes;

                                        let shftFactorMinutes: number = shift.getTimeScheduleTypeFactorsWithinShift();
                                        shiftPlannedMinutes += shftFactorMinutes;
                                        plannedMinutes += shiftPlannedMinutes;

                                        let dailyPlanned = dailyPlannedMinutes.find(d => d.date.isSameDayAs(date));
                                        if (dailyPlanned)
                                            dailyPlanned.minutes += shiftPlannedMinutes;
                                        else
                                            dailyPlannedMinutes.push({ date: date, minutes: shiftPlannedMinutes });

                                        let dailyFactor = dailyFactorMinutes.find(d => d.date.isSameDayAs(date));
                                        if (dailyFactor)
                                            dailyFactor.minutes += shftFactorMinutes;
                                        else
                                            dailyFactorMinutes.push({ date: date, minutes: shftFactorMinutes });

                                        if (!shift.isBreak) {
                                            // Gross time for current shift
                                            if (this.selectableInformationSettings.showGrossTime) {
                                                let shiftGrossMinutes: number = shift.grossTime ? shift.grossTime : 0;
                                                grossMinutes += shiftGrossMinutes;

                                                let dailyGross = dailyGrossMinutes.find(d => d.date.isSameDayAs(date));
                                                if (dailyGross)
                                                    dailyGross.minutes += shiftGrossMinutes;
                                                else
                                                    dailyGrossMinutes.push({ date: date, minutes: shiftGrossMinutes });
                                            }

                                            // Total cost for current shift
                                            if (this.selectableInformationSettings.showTotalCost || this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge) {
                                                let shiftTotalCost: number = shift.totalCost;
                                                totalCost += shiftTotalCost;

                                                let dailyTotal = dailyTotalCost.find(d => d.date.isSameDayAs(date));
                                                if (dailyTotal)
                                                    dailyTotal.cost += shiftTotalCost;
                                                else
                                                    dailyTotalCost.push({ date: date, cost: shiftTotalCost });

                                                let shiftTotalCostIncEmpTaxAndSuppCharge: number = shift.totalCostIncEmpTaxAndSuppCharge;
                                                totalCostIncEmpTaxAndSuppCharge += shiftTotalCostIncEmpTaxAndSuppCharge;

                                                let dailyTotalIncTaxes = dailyTotalCostIncEmpTaxAndSuppCharge.find(d => d.date.isSameDayAs(date));
                                                if (dailyTotalIncTaxes)
                                                    dailyTotalIncTaxes.costIncEmpTaxAndSuppCharge += shiftTotalCostIncEmpTaxAndSuppCharge;
                                                else
                                                    dailyTotalCostIncEmpTaxAndSuppCharge.push({ date: date, costIncEmpTaxAndSuppCharge: shiftTotalCostIncEmpTaxAndSuppCharge });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    employee.plannedMinutes = plannedMinutes + absenceMinutes;
                    employee.timeScheduleTypeFactorMinutes = employeeFactorMinutes;
                    employee.grossMinutes = grossMinutes;
                    employee.totalCost = totalCost;
                    employee.totalCostIncEmpTaxAndSuppCharge = totalCostIncEmpTaxAndSuppCharge;
                    this.setEmployeeToolTip(employee);

                    totalPlannedMinutes += plannedMinutes;
                    totalGrossMinutes += grossMinutes;
                    totalCostSum += totalCost;
                    totalCostIncEmpTaxAndSuppChargeSum += totalCostIncEmpTaxAndSuppCharge

                    let absEmpShifts = empShifts.filter(s => s.timeDeviationCauseId && s.timeDeviationCauseId !== 0);
                    employee.hasAbsence = absEmpShifts.length > 0;

                    // Total work time (only add to this if employee is visible)
                    if (plannedMinutes > 0 || employee.hasAbsence)
                        totalWorkTimeMinutes += employee.workTimeMinutes;
                }
            }

            for (let i = 0; i < this.nbrOfVisibleDays; i++) {
                this.addScheduleViewSummary(this.dateFrom.addDays(i), dailyPlannedMinutes, dailyFactorMinutes, dailyGrossMinutes, dailyTotalCost, dailyTotalCostIncEmpTaxAndSuppCharge);
            }
        }
    }

    private calculateStaffingNeed() {
        this.staffingNeedSum = [];

        let totalBudgetSales = 0;
        let totalBudgetHours = 0;
        let totalBudgetCost = 0;
        let totalBudgetSalaryPercent = 0;
        let totalBudgetFPAT = 0;
        let totalBudgetLPAT = 0;

        let totalSales = 0;
        let totalHours = 0;
        let totalCost = 0;
        let totalSalaryPercent = 0;
        let totalFPAT = 0;
        let totalLPAT = 0;

        this.dates.forEach(dateDay => {
            let date: Date = dateDay.date;
            let intervalData: StaffingStatisticsInterval = this.staffingNeedData.find(d => d.interval.isSameMinuteAs(date));
            if (intervalData) {
                let needSum = 0;
                let needFreqSum = 0;
                let needRowFreqSum = 0;
                let budgetSum = 0;
                let templateScheduleSum = 0;
                let templateScheduleForEmployeePostSum = 0;
                let scheduleSum = 0;
                let scheduleAndTimeSum = 0;
                let timeSum = 0;
                intervalData.rows.forEach(row => {
                    let need = row.need;
                    let needFreq = row.needFrequency;
                    let needRowFreq = row.needRowFrequency * 60;    // Comes in hours, want minutes
                    let budget = row.getBudgetValue(this.selectableInformationSettings.followUpCalculationType);
                    let template = row.getTemplateScheduleValue(this.selectableInformationSettings.followUpCalculationType);
                    let templateForEmployeePost = row.getTemplateScheduleForEmployeePostValue(this.selectableInformationSettings.followUpCalculationType);
                    let schedule = row.getScheduleValue(this.selectableInformationSettings.followUpCalculationType);
                    let scheduleAndTime = row.getScheduleAndTimeValue(this.selectableInformationSettings.followUpCalculationType);
                    let time = row.getTimeValue(this.selectableInformationSettings.followUpCalculationType);

                    if (this.selectableInformationSettings.followUpCalculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours) {
                        if (this.isCommonDayView) {
                            // Schedule is returned in minutes per slot, we want number of slots
                            budget = Math.ceil(budget / this.dayViewMinorTickLength);
                            template = Math.ceil(template / this.dayViewMinorTickLength);
                            templateForEmployeePost = Math.ceil(templateForEmployeePost / this.dayViewMinorTickLength);
                            schedule = Math.ceil(schedule / this.dayViewMinorTickLength);
                            scheduleAndTime = Math.ceil(scheduleAndTime / this.dayViewMinorTickLength);
                            time = Math.ceil(time / this.dayViewMinorTickLength);
                        } else {
                            // Need is represented in hours and should also be in minutes
                            need *= 60;
                        }
                    }

                    needSum += need;
                    needFreqSum += needFreq;
                    needRowFreqSum += needRowFreq;
                    budgetSum += budget;
                    templateScheduleSum += template;
                    templateScheduleForEmployeePostSum += templateForEmployeePost;
                    scheduleSum += schedule;
                    scheduleAndTimeSum += scheduleAndTime;
                    timeSum += time;

                    totalBudgetSales += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    totalBudgetHours += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    totalBudgetCost += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    totalBudgetSalaryPercent += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    totalBudgetFPAT += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    totalBudgetLPAT += row.getBudgetValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);

                    totalSales += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Sales);
                    totalHours += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours);
                    totalCost += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.PersonelCost);
                    totalSalaryPercent += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.SalaryPercent);
                    totalFPAT += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.FPAT);
                    totalLPAT += row.getTimeValue(TermGroup_TimeSchedulePlanningFollowUpCalculationType.LPAT);
                });
                this.staffingNeedSum.push({ date: date, need: needSum, needFrequency: needFreqSum, needRowFrequency: needRowFreqSum, budget: budgetSum, templateSchedule: templateScheduleSum, templateScheduleForEmployeePost: templateScheduleForEmployeePostSum, schedule: scheduleSum, scheduleAndTime: scheduleAndTimeSum, time: timeSum });
            }
        });

        this.gaugeSalesValue = totalBudgetSales ? totalSales / totalBudgetSales * 100 : 0;
        this.gaugeHoursValue = totalBudgetHours ? totalHours / totalBudgetHours * 100 : 0;
        this.gaugeCostValue = totalBudgetCost ? totalCost / totalBudgetCost * 100 : 0;
        this.gaugeSalaryPercentValue = totalBudgetSalaryPercent ? totalSalaryPercent / totalBudgetSalaryPercent * 100 : 0;
        this.gaugeFPATValue = totalBudgetFPAT ? totalFPAT / totalBudgetFPAT * 100 : 0;
        this.gaugeLPATValue = totalBudgetLPAT ? totalLPAT / totalBudgetLPAT * 100 : 0;
    }

    private addScheduleViewSummary(date: Date, dailyPlannedMinutes: any[], dailyFactorMinutes: any[], dailyGrossMinutes: any[], dailyTotalCost: any[], dailyTotalCostIncEmpTaxAndSupplementCharge: any[]) {
        let planned = dailyPlannedMinutes.find(d => d.date.isSameDayAs(date));
        let plannedMinutes = planned ? planned['minutes'] : 0;
        let factor = dailyFactorMinutes.find(d => d.date.isSameDayAs(date));
        let factorMinutes = factor ? factor['minutes'] : 0;
        let gross = dailyGrossMinutes.find(d => d.date.isSameDayAs(date));
        let grossMinutes = gross ? gross['minutes'] : 0;
        let cost = dailyTotalCost.find(d => d.date.isSameDayAs(date));
        let totalCost = cost ? cost['cost'] : 0;
        let costEmpTaxAndSupplementCharge = dailyTotalCostIncEmpTaxAndSupplementCharge.find(d => d.date.isSameDayAs(date));
        let totalCostIncEmpTaxAndSupplementCharge = costEmpTaxAndSupplementCharge ? costEmpTaxAndSupplementCharge['costIncEmpTaxAndSuppCharge'] : 0;

        this.plannedMinutesSum.push({ date: date, value: plannedMinutes });
        this.factorMinutesSum.push({ date: date, value: factorMinutes });
        this.grossMinutesSum.push({ date: date, value: grossMinutes });
        this.totalCostSum.push({ date: date, value: totalCost });
        this.totalCostIncEmpTaxAndSupplementChargeSum.push({ date: date, value: totalCostIncEmpTaxAndSupplementCharge })
    }

    private calculateStaffingNeedsRowSums() {
        this.staffingNeedsTotalSum = 0;
        this.staffingNeedsFilteredSum = 0;
        this.staffingNeedsShiftTypeSum = [];

        this.heads.forEach(head => {
            head.rows.forEach(row => {
                // Total sum
                this.staffingNeedsTotalSum += _.sumBy(row.periods.filter(p => !p.isBreak), p => p.length);

                // Filtered sum
                row.totalLength = _.sumBy(row.visiblePeriods.filter(p => !p.isBreak), p => p.length);
                this.staffingNeedsFilteredSum += row.totalLength;

                // Sum per shift type
                row.visiblePeriods.filter(p => !p.isBreak).forEach(period => {
                    var shiftTypeSum = this.staffingNeedsShiftTypeSum.find(s => s.shiftTypeId === (period.shiftTypeId ? period.shiftTypeId : 0));
                    if (!shiftTypeSum) {
                        shiftTypeSum = { shiftTypeId: period.shiftTypeId ? period.shiftTypeId : 0, shiftTypeName: period.shiftTypeId ? period.shiftTypeName : this.terms["core.notspecified"], shiftTypeColor: period.shiftTypeId ? period.shiftTypeColor : Constants.SHIFT_TYPE_UNSPECIFIED_COLOR, sum: period.length, unspecified: !period.shiftTypeId };
                        this.staffingNeedsShiftTypeSum.push(shiftTypeSum);
                    } else {
                        shiftTypeSum.sum += period.length;
                    }
                });
            });
        });
    }

    private calculateEmployeesWorkTimes() {
        this.visibleEmployees.forEach(employee => {
            this.calculateEmployeeWorkTimes(employee);
        });
    }

    public calculateEmployeeWorkTimes(employee: EmployeeListDTO) {
        if (!employee)
            return;

        const isWholeYear = (this.dateFrom.isBeginningOfYear() && this.dateTo.isEndOfYear());
        if (isWholeYear && employee.annualWorkTimeMinutes !== 0)
            employee.workTimeMinutes = employee.annualWorkTimeMinutes;
        else {
            employee.oneWeekWorkTimeMinutes = this.getCurrentWorkTimeWeek(employee);
            if (this.nbrOfVisibleDays > 7)
                employee.workTimeMinutes = employee.oneWeekWorkTimeMinutes * this.nbrOfVisibleDays / 7;
            else
                employee.workTimeMinutes = employee.oneWeekWorkTimeMinutes;
        }
        employee.minScheduleTime = this.getCurrentMinScheduleTime(employee);
        employee.maxScheduleTime = this.getCurrentMaxScheduleTime(employee);

        this.scheduleHandler.updateEmployeeInfo(employee);
    }

    // Help-methods

    private get hasEmployeesLoaded(): boolean {
        return this.allEmployees && this.allEmployees.length > 0;
    }
    public get hasStaffingNeedsRows(): boolean {
        return this.heads.filter(h => h.rows && h.rows.length > 0).length > 0;
    }

    private getCurrentEmployeeGroupId(employee: EmployeeListDTO): number {
        const employment = this.getCurrentEmployment(employee);
        return employment?.employeeGroupId || 0;
    }

    public getCurrentEmployeeGroupName(employee: EmployeeListDTO): string {
        const employment = this.getCurrentEmployment(employee);
        return employment?.employeeGroupName || '';
    }

    public getCurrentWorkTimeWeek(employee: EmployeeListDTO): number {
        const employment = this.getCurrentEmployment(employee);
        return employment?.workTimeWeekMinutes || 0;
    }

    private getCurrentMinScheduleTime(employee: EmployeeListDTO): number {
        const employment = this.getCurrentEmployment(employee);
        return employment?.minScheduleTime || 0;
    }

    private getCurrentMaxScheduleTime(employee: EmployeeListDTO): number {
        const employment = this.getCurrentEmployment(employee);
        return employment?.maxScheduleTime || 0;
    }

    private getCurrentPercent(employee: EmployeeListDTO): number {
        const employment = this.getCurrentEmployment(employee);
        return employment?.percent || 0;
    }

    private getCurrentEmployment(employee: EmployeeListDTO): EmployeeListEmploymentDTO {
        return employee.getEmployment(this.dateFrom.beginningOfDay(), this.dateTo.endOfDay());
    }

    public isCurrentEmploymentTemporaryPrimary(employee: EmployeeListDTO): boolean {
        const employment = this.getCurrentEmployment(employee);
        return employment?.isTemporaryPrimary || false;
    }

    private getEmployeeRightListCurrentPercent(employee: EmployeeRightListDTO): number {
        var employment = this.getEmployeeRightListCurrentEmployment(employee);
        return employment?.percent || 0;
    }

    private getEmployeeRightListCurrentEmployment(employee: EmployeeRightListDTO): EmployeeListEmploymentDTO {
        return employee.getEmployment(this.dateFrom.beginningOfDay(), this.dateTo.endOfDay());
    }

    public hasAnnualLeaveGroup(employee: EmployeeListDTO): boolean {
        if (!employee)
            return false;

        const employment = this.getCurrentEmployment(employee);
        return !!employment?.annualLeaveGroupId;
    }

    public hasAnnualLeaveGroupById(employeeId: number): boolean {
        return this.hasAnnualLeaveGroup(this.getEmployeeById(employeeId))
    }

    private setEmployeesToolTip() {
        this.visibleEmployees.forEach(employee => {
            this.setEmployeeToolTip(employee);
        });
    }

    public setEmployeeToolTip(employee: EmployeeListDTO) {
        let toolTip = '';

        toolTip = "{0}".format(employee.hidden || employee.employeePostId ? employee.name : employee.numberAndName);
        if (!employee.active)
            toolTip += " ({0})".format(this.terms["common.inactive"])
        if (employee.description)
            toolTip += "\n{0}".format(employee.description);

        toolTip += "\n";

        if (this.selectableInformationSettings.showEmployeeGroup)
            toolTip += this.getCurrentEmployeeGroupName(employee) + "\n";

        if (this.isAdmin) {
            toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.nettime"], this.minutesToTimeSpanFilter(employee.plannedMinutes, false, false));
            if (employee.timeScheduleTypeFactorMinutes != 0)
                toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.scheduletypefactortime"], this.minutesToTimeSpanFilter(employee.timeScheduleTypeFactorMinutes, false, false));

            if (!employee.hidden) {
                if (this.isCommonScheduleView) {
                    toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.worktimeweek"], this.minutesToTimeSpanFilter(employee.workTimeMinutes, false, false));
                    if (this.nbrOfVisibleDays > 7)
                        toolTip += " ({0})".format(this.minutesToTimeSpanFilter(employee.oneWeekWorkTimeMinutes, false, false));

                    if (this.isScheduleView && this.selectableInformationSettings.showCyclePlannedTime) {
                        toolTip += "\n\n{0}: {1}".format(this.terms["time.schedule.planning.cycletime.total"], this.minutesToTimeSpanFilter(employee.cyclePlannedMinutes, false, false));
                        toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.cycletime.average"], this.minutesToTimeSpanFilter(employee.cyclePlannedAverageMinutes, false, false));
                    }
                }

                if (this.selectableInformationSettings.showGrossTime)
                    toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.grosstime"], this.minutesToTimeSpanFilter(employee.grossMinutes, false, false));

                if (this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge)
                    toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.cost"], this.amountFilter(employee.totalCostIncEmpTaxAndSuppCharge, 0));
                else if (this.selectableInformationSettings.showTotalCost)
                    toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.cost"], this.amountFilter(employee.totalCost, 0));

                if ((this.isScheduleView || this.isDayView) && this.selectableInformationSettings.showAnnualLeaveBalance) {
                    toolTip += "\n\n{0}: ".format(this.terms["time.schedule.planning.annualleave.balance"]);
                    switch (this.selectableInformationSettings.showAnnualLeaveBalanceFormat) {
                        case TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days:
                            toolTip += "{0} ({1})".format(employee.getAnnualLeaveBalanceValue(TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days, this.terms["core.time.day"].toLocaleLowerCase(), this.terms["core.time.days"].toLocaleLowerCase()), employee.getAnnualLeaveBalanceValue(TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours));
                            break;
                        case TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours:
                            toolTip += "{0} ({1})".format(employee.getAnnualLeaveBalanceValue(TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours), employee.getAnnualLeaveBalanceValue(TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days, this.terms["core.time.day"].toLocaleLowerCase(), this.terms["core.time.days"].toLocaleLowerCase()));
                            break;
                    }
                }
            }

            if (this.isTemplateView || this.isEmployeePostView) {
                toolTip += "\n\n";
                if (!employee.hasTemplateSchedules)
                    toolTip += this.terms["time.schedule.planning.notemplateschedule"];
                else {
                    toolTip += "{0}:".format(employee.templateSchedules.length === 1 ? this.terms["time.schedule.planning.templateschedule"] : this.terms["time.schedule.planning.templateschedules"]);

                    employee.templateSchedules.forEach(template => {
                        var noOfWeeks: number = template.noOfDays / 7;
                        if (noOfWeeks < 1)
                            noOfWeeks = 1;

                        toolTip += "\n";
                        if (template.name)
                            toolTip += "{0}, ".format(template.name);

                        if (template.startDate && !template.name.endsWithCaseInsensitive(template.startDate.toFormattedDate()))
                            toolTip += template.startDate.toFormattedDate();

                        if (template.stopDate)
                            toolTip += " - {0}".format(template.stopDate.toFormattedDate());

                        if (!toolTip.endsWithCaseInsensitive(", "))
                            toolTip += ", ";

                        toolTip += "{0}{1}".format(noOfWeeks.toString(), this.terms["common.weekshort"]);

                        if (template.timeScheduleTemplateGroupId)
                            toolTip += " ({0}: {1})".format(this.terms["time.schedule.templategroup.templategroup"], template.timeScheduleTemplateGroupName);
                    });
                }
            }
        }

        employee.toolTip = toolTip;
    }

    private setShiftLabels(shifts: ShiftDTO[]) {
        shifts.forEach(shift => {
            this.setShiftLabel(shift);
        });
    }

    private setShiftLabel(shift: ShiftDTO) {
        shift.setLabel(this.terms["time.schedule.planning.breaklabel"], this.terms["time.schedule.planning.wholedaylabel"], this.terms["common.absence"], (this.isCommonDayView) && this.selectableInformationSettings.breakVisibility == TermGroup_TimeSchedulePlanningBreakVisibility.TotalMinutes, false, (this.isCommonDayView || this.isCompressedStyle), this.orderPlanningShiftInfoTopRight, this.orderPlanningShiftInfoBottomLeft, this.orderPlanningShiftInfoBottomRight, this.useMultipleScheduleTypes, this.isStandbyView, this.isCommonDayView && this.selectableInformationSettings.useShiftTypeCode);
    }

    private shiftToolTipsToCreate: number[] = [];
    private shiftMouseEnter(timeScheduleTemplateBlockId: number) {
        // Create tooltip on hover

        if (this.shiftToolTipsToCreate.includes(timeScheduleTemplateBlockId))
            return;

        this.shiftToolTipsToCreate.push(timeScheduleTemplateBlockId);

        // Don't create tooltip if user is just quickly dragging mouse passing the shift
        this.$timeout(() => {
            this.createShiftToolTip(timeScheduleTemplateBlockId);
        }, 100);
    }

    private shiftMouseLeave(timeScheduleTemplateBlockId: number) {
        _.pull(this.shiftToolTipsToCreate, timeScheduleTemplateBlockId);
    }

    private leisureCodeToolTipsToCreate: number[] = [];
    private leisureCodeMouseEnter(timeScheduleEmployeePeriodDetailId: number) {
        // Create tooltip on hover

        if (this.leisureCodeToolTipsToCreate.includes(timeScheduleEmployeePeriodDetailId))
            return;

        this.leisureCodeToolTipsToCreate.push(timeScheduleEmployeePeriodDetailId);

        // Don't create tooltip if user is just quickly dragging mouse passing the shift
        this.$timeout(() => {
            this.createLeisureCodeToolTip(timeScheduleEmployeePeriodDetailId);
        }, 100);
    }

    private leisureCodeMouseLeave(timeScheduleEmployeePeriodDetailId: number) {
        _.pull(this.leisureCodeToolTipsToCreate, timeScheduleEmployeePeriodDetailId);
    }

    private createShiftToolTip(timeScheduleTemplateBlockId: number, forceCreate: boolean = false) {
        if (!this.shiftToolTipsToCreate.includes(timeScheduleTemplateBlockId))
            return;

        const shift: ShiftDTO = this.shifts.find(s => s.timeScheduleTemplateBlockId === timeScheduleTemplateBlockId);
        if (!shift || (shift.toolTip && !forceCreate))
            return;

        let toolTip = '';
        let wholeDayToolTip = '';
        const breakPrefix: string = this.terms["time.schedule.planning.breakprefix"];
        const isHiddenEmployee: boolean = (shift.employeeId === this.hiddenEmployeeId);

        // Current shift

        // Time
        if (!shift.isAbsenceRequest) {
            if (shift.isWholeDay)
                toolTip += `${this.terms["time.schedule.planning.wholedaylabel"]}  `;
            else
                toolTip += `${shift.actualStartTime.toFormattedTime()}-${shift.actualStopTime.toFormattedTime()}  `;
        }

        if (shift.timeDeviationCauseId && shift.timeDeviationCauseId !== 0) {
            // Absence
            toolTip += shift.timeDeviationCauseName;
        } else {
            // Shift type
            if (shift.shiftTypeName)
                toolTip += shift.shiftTypeName;

            if (shift.isOnDuty)
                toolTip += ` (${this.terms["time.schedule.planning.blocktype.onduty"].toLocaleLowerCase()})`;

            if (this.useAccountHierarchy && shift.accountName)
                toolTip += ` (${shift.accountName})`;

            // Order number, customer and delivery address
            if (shift.isOrder) {
                toolTip += '\n';
                if (shift.order.orderNr)
                    toolTip += `${shift.order.orderNr}, `;
                toolTip += shift.order.customerName;
                if (shift.order.deliveryAddress && shift.order.deliveryAddress.length > 0)
                    toolTip += `${shift.order.deliveryAddress}, `;
            }
        }

        // Schedule type
        let scheduleTypeNames = shift.getTimeScheduleTypeNames(this.useMultipleScheduleTypes);
        if (!shift.isOrder && scheduleTypeNames)
            toolTip += ` - ${scheduleTypeNames}`;

        // Week number/Number of weeks
        if (shift.nbrOfWeeks > 0) {
            if (toolTip && toolTip.length > 0)
                toolTip += ', ';
            toolTip += `${CalendarUtility.getWeekNr(shift.dayNumber)}/${shift.nbrOfWeeks}${this.terms["common.weekshort"]}`;
        }

        // Description
        if (shift.description) {
            if (toolTip && toolTip.length > 0)
                toolTip += '\n';
            toolTip += shift.description;
        }

        // Order planning
        if (shift.isOrder) {
            toolTip += `\n${this.terms["time.schedule.planning.orderstatus"]}: ${shift.order.attestStateName}`;
            if (shift.order.workingDescription && shift.order.workingDescription.length > 0)
                toolTip += `\n\n${shift.order.workingDescription}`;
            if (shift.order.internalDescription && shift.order.internalDescription.length > 0)
                toolTip += `\n\n${shift.order.internalDescription}`;
        }

        // Whole day

        let dayShifts: ShiftDTO[];
        if (this.isEmployeePostView) {
            dayShifts = this.shifts.filter(s => s.actualStartDate.isSameDayAs(shift.actualStartDate) && s.employeePostId === shift.employeePostId);
        } else {
            // If whole day absence, skip this part
            dayShifts = this.shifts.filter(s => s.actualStartDate.isSameDayAs(shift.actualStartDate) && s.employeeId === shift.employeeId &&
                (s.link === shift.link || !isHiddenEmployee) &&
                !((s.isAbsence || s.isAbsenceRequest) && CalendarUtility.toFormattedTime(s.actualStartTime, true) === '00:00:00' && CalendarUtility.toFormattedTime(s.actualStopTime, true) === '23:59:59'));
        }

        let minutes: number = _.sumBy(dayShifts.filter(s => !s.isStandby && !s.isOnDuty), s => s.getShiftLength());
        let factorMinutes: number = 0;

        if (dayShifts.length > 0) {
            // Get all breaks

            let timeCode = this.breakTimeCodes.find(b => b.timeCodeId === shift.break1TimeCodeId);
            let break1TimeCode: string = timeCode ? timeCode.name : '';
            if (!break1TimeCode.startsWithCaseInsensitive(breakPrefix))
                break1TimeCode = `${breakPrefix} ${break1TimeCode}`;

            timeCode = this.breakTimeCodes.find(b => b.timeCodeId === shift.break2TimeCodeId);
            let break2TimeCode: string = timeCode ? timeCode.name : '';
            if (!break2TimeCode.startsWithCaseInsensitive(breakPrefix))
                break2TimeCode = `${breakPrefix} ${break2TimeCode}`;

            timeCode = this.breakTimeCodes.find(b => b.timeCodeId === shift.break3TimeCodeId);
            let break3TimeCode: string = timeCode ? timeCode.name : '';
            if (!break3TimeCode.startsWithCaseInsensitive(breakPrefix))
                break3TimeCode = `${breakPrefix} ${break3TimeCode}`;

            timeCode = this.breakTimeCodes.find(b => b.timeCodeId === shift.break4TimeCodeId);
            let break4TimeCode: string = timeCode ? timeCode.name : '';
            if (!break4TimeCode.startsWithCaseInsensitive(breakPrefix))
                break4TimeCode = `${breakPrefix} ${break4TimeCode}`;

            let break1: string = shift.break1TimeCodeId !== 0 && (shift.break1Link === shift.link || !isHiddenEmployee) ? `\n${shift.break1StartTime.toFormattedTime()}-${shift.break1StartTime.addMinutes(shift.break1Minutes).toFormattedTime()}  ${break1TimeCode}` : '';
            let break2: string = shift.break2TimeCodeId !== 0 && (shift.break2Link === shift.link || !isHiddenEmployee) ? `\n${shift.break2StartTime.toFormattedTime()}-${shift.break2StartTime.addMinutes(shift.break2Minutes).toFormattedTime()}  ${break2TimeCode}` : '';
            let break3: string = shift.break3TimeCodeId !== 0 && (shift.break3Link === shift.link || !isHiddenEmployee) ? `\n${shift.break3StartTime.toFormattedTime()}-${shift.break3StartTime.addMinutes(shift.break3Minutes).toFormattedTime()}  ${break3TimeCode}` : '';
            let break4: string = shift.break4TimeCodeId !== 0 && (shift.break4Link === shift.link || !isHiddenEmployee) ? `\n${shift.break4StartTime.toFormattedTime()}-${shift.break4StartTime.addMinutes(shift.break4Minutes).toFormattedTime()}  ${break4TimeCode}` : '';

            if (shift.isSchedule || shift.isStandby || shift.isOnDuty)
                wholeDayToolTip += `${this.terms["time.schedule.planning.todaysschedule"]}:`;
            else if (shift.isOrder)
                wholeDayToolTip += `${this.terms["time.schedule.planning.todaysorders"]}:`;
            else if (shift.isBooking)
                wholeDayToolTip += `${this.terms["time.schedule.planning.todaysbookings"]}:`;

            _.orderBy(dayShifts, 'actualStartTime').forEach(dayShift => {
                // Breaks within day

                minutes -= dayShift.getBreakTimeWithinShift();

                if (shift.isSchedule) {
                    let breakEndTime: Date;
                    if (break1) {
                        breakEndTime = shift.break1StartTime.addMinutes(shift.break1Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break1;
                            break1 = '';
                        }
                    }
                    if (break2) {
                        breakEndTime = shift.break2StartTime.addMinutes(shift.break2Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break2;
                            break2 = '';
                        }
                    }
                    if (break3) {
                        breakEndTime = shift.break3StartTime.addMinutes(shift.break3Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break3;
                            break3 = '';
                        }
                    }
                    if (break4) {
                        breakEndTime = shift.break4StartTime.addMinutes(shift.break4Minutes);
                        if (breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)) {
                            wholeDayToolTip += break4;
                            break4 = '';
                        }
                    }
                }

                // Time
                wholeDayToolTip += `\n${dayShift.actualStartTime.toFormattedTime()}-${dayShift.actualStopTime.toFormattedTime()}  `;

                // Shift type
                if (dayShift.shiftTypeName)
                    wholeDayToolTip += dayShift.shiftTypeName;

                if (dayShift.isOnDuty)
                    wholeDayToolTip += ` (${this.terms["time.schedule.planning.blocktype.onduty"].toLocaleLowerCase()})`;

                if (this.useAccountHierarchy && dayShift.accountName)
                    wholeDayToolTip += ` (${dayShift.accountName})`;

                // Order number, customer and delivery address
                if (dayShift.isOrder) {
                    wholeDayToolTip += '\n';
                    if (dayShift.order.orderNr)
                        wholeDayToolTip += `${dayShift.order.orderNr}, `;
                    wholeDayToolTip += dayShift.order.customerName;
                    if (dayShift.order.deliveryAddress && dayShift.order.deliveryAddress.length > 0)
                        wholeDayToolTip += `, ${dayShift.order.deliveryAddress}`;
                }

                // TimeScheduleType factor multiplyer
                factorMinutes += dayShift.getTimeScheduleTypeFactorsWithinShift();
            });

            if (shift.isSchedule || shift.isStandby) {
                // The rest of the breaks
                if (break1)
                    wholeDayToolTip += break1;
                if (break2)
                    wholeDayToolTip += break2;
                if (break3)
                    wholeDayToolTip += break3;
                if (break4)
                    wholeDayToolTip += break4;

                // Summary

                let breakMinutes: number = 0;
                if (shift.break1TimeCodeId !== 0)
                    breakMinutes += shift.break1Minutes;
                if (shift.break2TimeCodeId !== 0)
                    breakMinutes += shift.break2Minutes;
                if (shift.break3TimeCodeId !== 0)
                    breakMinutes += shift.break3Minutes;
                if (shift.break4TimeCodeId !== 0)
                    breakMinutes += shift.break4Minutes;

                wholeDayToolTip += `\n\n${this.terms["time.schedule.planning.scheduletime"]}: ${CalendarUtility.minutesToTimeSpan(minutes)}`;
                if (breakMinutes > 0)
                    wholeDayToolTip += ` (${breakMinutes.toString()})`;
            }

            if (factorMinutes !== 0)
                wholeDayToolTip += `\n${this.terms["time.schedule.planning.scheduletypefactortime"]}: ${CalendarUtility.minutesToTimeSpan(factorMinutes)}`;

            // Gross time
            if (this.selectableInformationSettings.showGrossTime)
                wholeDayToolTip += `\n${this.terms["time.schedule.planning.grosstime"]}: ${CalendarUtility.minutesToTimeSpan(_.sumBy(dayShifts.filter(s => !s.isBreak), s => s.grossTime))} `;

            // Cost
            if (this.selectableInformationSettings.showTotalCostIncEmpTaxAndSuppCharge)
                wholeDayToolTip += `\n${this.terms["time.schedule.planning.cost"]}: ${this.amountFilter(_.sumBy(dayShifts.filter(s => !s.isBreak), s => s.totalCostIncEmpTaxAndSuppCharge), 0)}`;
            else if (this.selectableInformationSettings.showTotalCost)
                wholeDayToolTip += `\n${this.terms["time.schedule.planning.cost"]}: ${this.amountFilter(_.sumBy(dayShifts.filter(s => !s.isBreak), s => s.totalCost), 0)}`;
        }

        if (wholeDayToolTip.length === 0)
            shift.toolTip = toolTip;
        else {
            let shiftTypeName: string = '';
            if (shift.isSchedule || shift.isStandby || shift.isOnDuty)
                shiftTypeName = this.terms["time.schedule.planning.thisshift"];
            else if (shift.isOrder)
                shiftTypeName = this.terms["time.schedule.planning.thisorder"];
            else if (shift.isBooking)
                shiftTypeName = this.terms["time.schedule.planning.thisbooking"];

            shift.toolTip = (toolTip.length > 0 ? "{0}:\n{1}\n\n".format(shiftTypeName, toolTip) : '') + wholeDayToolTip;
        }

        if (shift.availabilityToolTip)
            shift.toolTip += `\n\n${shift.availabilityToolTip}`;

        this.scheduleHandler.setShiftToolTip(shift.timeScheduleTemplateBlockId, shift.toolTip);
    }

    private createLeisureCodeToolTip(timeScheduleEmployeePeriodDetailId: number, forceCreate: boolean = false) {
        if (!this.leisureCodeToolTipsToCreate.includes(timeScheduleEmployeePeriodDetailId))
            return;

        const shift: ShiftDTO = this.shifts.find(s => s.timeScheduleEmployeePeriodDetailId === timeScheduleEmployeePeriodDetailId);
        if (!shift || (shift.toolTip && !forceCreate))
            return;

        shift.toolTip = this.getLeisureCodeName(shift);

        this.scheduleHandler.setLeisureCodeToolTip(shift.timeScheduleEmployeePeriodDetailId, shift.toolTip);
    }

    private getLeisureCodeName(shift: ShiftDTO): string {
        let code = this.leisureCodes?.find(l => l.timeLeisureCodeId === shift.timeLeisureCodeId);
        return code ? code.name : shift.description;
    }

    public setShiftAvailabilityToolTip(timeScheduleTemplateBlockId: number, toolTip: string) {
        const shift: ShiftDTO = this.shifts.find(s => s.timeScheduleTemplateBlockId === timeScheduleTemplateBlockId);
        if (shift)
            shift.availabilityToolTip = toolTip;
    }

    private clearShiftToolTips(shifts: ShiftDTO[] = []) {
        if (!shifts || shifts.length === 0)
            shifts = this.visibleShifts;

        shifts.filter(s => s.toolTip).forEach(shift => {
            shift.toolTip = '';
        });
    }

    private setOrderListToolTip(order: OrderListDTO) {
        let toolTip = '';

        toolTip += "{0}: {1}\n".format(this.terms["common.priority"], order.priority ? order.priority.toString() : '');
        toolTip += "{0}: {1}\n".format(this.terms["time.schedule.planning.orderlist.plannedstartdate"], order.plannedStartDate ? order.plannedStartDate.toFormattedDate() : '');
        toolTip += "{0}: {1}\n".format(this.terms["time.schedule.planning.orderlist.plannedstopdate"], order.plannedStopDate ? order.plannedStopDate.toFormattedDate() : '');
        if (order.shiftTypeId)
            toolTip += "{0}: {1}\n".format(this.terms["common.ordershifttype"], order.shiftTypeName);
        if (order.workingDescription)
            toolTip += "\n{0}\n\n".format(order.workingDescription);
        if (order.internalDescription)
            toolTip += "\n{0}\n\n".format(order.internalDescription);
        toolTip += "{0}: {1}\n".format(this.terms["time.schedule.planning.orderlist.estimatedtime"], CalendarUtility.minutesToTimeSpan(order.estimatedTime));
        toolTip += "{0}: {1}\n\n".format(this.terms["time.schedule.planning.orderlist.remainingtime"], CalendarUtility.minutesToTimeSpan(order.remainingTime));
        toolTip += "{0}: ({1}) {2}".format(this.terms["common.customer"], order.customerNr, order.customerName);
        if (order.projectId)
            toolTip += "\n{0}: ({1}) {2}".format(this.terms["common.customer.customer.orderproject"], order.projectNr, order.projectName);
        if (order.categories.length > 0)
            toolTip += "\n\n{0}: {1}".format(this.terms["common.categories"], order.categoryString);
        if (order.deliveryAddress)
            toolTip += "\n{0}: {1}".format(this.terms["time.schedule.planning.orderlist.deliveryaddress"], order.deliveryAddress);

        order.toolTip = toolTip;
    }
}

export class DateDay {
    constructor(date: Date) {
        this.date = date;
    }

    public date: Date;
    public rangeNbrOfDays: number;
    public specificNeed = false;
    public shiftPeriod: ShiftPeriodDTO;
    public isToday = false;
    public isSaturday = false;
    public isSunday = false;

    public get weekday(): DayOfWeek {
        return this.date.dayOfWeek();
    }
}