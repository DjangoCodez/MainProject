import { AnnualSummaryController } from "../../Schedule/Planning/Dialogs/AnnualSummary/AnnualSummaryController";
import { AttestEmployeeListDTO } from "../../../Common/Models/AttestEmployeeListDTO";
import { AttestReminderDialogController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/AttestReminder/AttestReminderDialogController";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { EditController as EmployeeEditController } from "../../Employee/Employees/EditController";
import { EditController as AbsenceRequestsEditController } from "../../../Shared/Time/Schedule/Absencerequests/EditController";
import { EmployeeListDTO, EmployeeListEmploymentDTO } from "../../../Common/Models/EmployeeListDTO";
import { EmployeeVacationPeriodDTO } from "../../../Common/Models/EmployeeVacationPeriodDTO";
import { ReverseTransactionsDialogController } from "../../Dialogs/ReverseTransactions/ReverseTransactionsDialogController";
import { SelectReportController } from "../../../Common/Dialogs/SelectReport/SelectReportController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { TimeAccumulatorItem } from "../../../Common/Models/TimeAccumulatorDTOs";
import { TimeAttestCalculationController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/Calculation/TimeAttestCalculationController";
import { TimeAttestCalculationFunctionDTO, AttestEmployeeDayDTO, AttestEmployeePeriodDTO, TimeEmployeeTreeDTO, TimeEmployeeTreeGroupNodeDTO, TimeEmployeeTreeNodeDTO, TimeEmployeeTreeSettings, AttestEmployeeAdditionDeductionDTO, TimeAttestCalculationFunctionValidationDTO, AttestEmployeeDaySmallDTO, TimeUnhandledShiftChangesEmployeeDTO, TimeUnhandledShiftChangesWeekDTO, EmployeesAttestResult } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Guid, StringUtility } from "../../../Util/StringUtility";
import { TimePayrollUtility } from "../../../Util/TimePayrollUtility";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { TimeAbsenceDetailDTO } from "../../../Common/Models/TimeAbsenceDetailDTO";
import { UpdateAbsenceDetailsDialogController } from "../../Directives/AbsenceDetails/Dialogs/UpdateAbsenceDetailsDialogController";
import { ISmallGenericType, IReverseTransactionsValidationDTO, IAttestEmployeeAdditionDeductionTransactionDTO, IAttestEmployeeAdditionDeductionDTO } from "../../../Scripts/TypeLite.Net4";
import { TermGroup_AttestTreeGrouping, TermGroup_AttestTreeSorting, Feature, TimeAttestMode, CompanySettingType, UserSettingType, TermGroup, SoeCategoryType, TermGroup_AttestEntity, SettingMainType, SoeTimeAttestFunctionOption, SoeReportTemplateType, TermGroup_TimeReportType } from "../../../Util/CommonEnumerations";
import { TimePeriodSelectorType, TimeTreeViewMode, TimeAttestContentViewMode, IconLibrary, AbsenceRequestGuiMode, AbsenceRequestViewMode, AbsenceRequestParentMode, SOEMessageBoxSize, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { ITimeService as ISharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IReportService } from "../../../Core/Services/ReportService";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { TimeBlockDateDTO } from "../../../Common/Models/TimeBlockDateDTO";
import { PeriodCalculationController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/PeriodCalculation/PeriodCalculationController";
import { MessageGroupDTO } from "../../../Common/Models/MessageDTOs";
import { AttestResultDialogController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/AttestResult/AttestResultDialogController";

export class EditController extends EditControllerBase {
    // Collections
    private termsArray: any;

    // Init params
    private defaultTimePeriodType = TimePeriodSelectorType.Month;
    private employeeId: number = 0;
    private attestMode: TimeAttestMode;
    private get isMyTime(): boolean {
        return this.attestMode == TimeAttestMode.TimeUser;
    }
    private get isMySelf(): boolean {
        return this.isMyTime || (this.currentTreeViewModeIsEmployee && this.currentEmployee && this.currentEmployee.employeeId == this.employeeId);
    }
    private get isERP(): boolean {
        return this.currentTimeReportType == TermGroup_TimeReportType.ERP;
    }
    private getGridName(): string {
        if (this.isMyTime && !this.isERP)
            return "Common.Directives.AttestMyTime";
        else if (this.isMyTime && this.isERP)
            return "Common.Directives.AttestMyTime.ERP";
        else if (!this.isMyTime && this.isERP)
            return "Common.Directives.AttestEmployee.ERP";
        else
            return "Common.Directives.AttestEmployee";
    }
    private getSearchWatermark(): string {
        if (this.socialSecPermission)
            return "time.time.tree.search";
        else
            return "time.time.tree.search_nosocialsec";
    }
    private getSearchDescription(): string {
        if (this.socialSecPermission)
            return "time.time.tree.disableautoload.choice1search";
        else
            return "time.time.tree.disableautoload.choice1search_nosocialsec";
    }

    //Permissions
    permissionToSeeDateTo: Date;
    dontSeeFuturePlacementsPermission: boolean = false;
    editEmployeePermission: boolean = false;
    showAccumulatorsPermission: boolean = false;
    editDayPermission: boolean = false;
    editSchedulePermission: boolean = false;
    editAbsencePermission: boolean = false;
    showAdditionDeductionPermission: boolean = false;
    showAbsenceDetailsPermission: boolean = false;
    editAbsenceDetailsPermission: boolean = false;
    projectAttestUser: boolean = false;
    projectAttestOthers: boolean = false;
    restoreToSchedulePermission: boolean = false;
    restoreScheduleToTemplatePermission: boolean = false;
    reGenerateDaysBasedOnTimeStampsPermission: boolean = false;
    regenerateTransactionsPermission: boolean = false;
    deleteTransactionsPermission: boolean = false;
    reverseTransactionsPermission: boolean = false;
    sendAttestReminderPermission: boolean = false;
    recalculateAccountingPermission: boolean = false;
    socialSecPermission: boolean = false;
    protected showAbsence(): boolean {
        return (this.isMyTime && this.editAbsencePermission) || !this.isMyTime;
    }

    //Lookups        
    toolbarSelectionCategories = [];
    toolbarSelectionCategoriesSelected = [];
    toolbarSelectionAccounts = [];
    toolbarSelectionAccountsSelected = [];
    toolbarSelectionEmployees = [];
    toolbarSelectionEmployeesSelected = [];
    toolbarSelectionAttestStates = [];
    toolbarSelectionAttestStatesSelected = [];
    toolbarGroupingOptions: SmallGenericType[];
    toolbarSortingOptions: SmallGenericType[];
    timePeriods: TimePeriodDTO[];
    timeAccumulators: TimeAccumulatorItem[];
    timePeriodTypeTerms: any;
    timePeriodTypes: SmallGenericType[];
    messageGroups: SmallGenericType[];
    contentEmployee: AttestEmployeeDayDTO[];
    contentEmployeeSelected: AttestEmployeeDayDTO[];
    contentGroupSelected: AttestEmployeePeriodDTO[];
    contentAdditionDeduction: AttestEmployeeAdditionDeductionDTO[];
    contentAdditionDeductionSelected: AttestEmployeeAdditionDeductionDTO[];
    contentAbsenceDetails: TimeAbsenceDetailDTO[];
    contentAbsenceDetailsSelected: TimeAbsenceDetailDTO[];
    tree: TimeEmployeeTreeDTO;
    treeUrl: any;
    timeTreeUrl: any;
    periodSelectorUrl: any;
    modalInstance: any;

    //Search
    toolbarSearchPatternInput: string;
    toolbarSearchPattern: string;

    //Filter
    treeFilterTextInput: string;
    treeFilterText: string;

    //Data
    toolbarFilterActive: boolean = false;
    toolbarGroupingActive: boolean = false;
    toolbarSortingActive: boolean = false;
    companyUseAccountHierarchy: boolean = false;
    companyUseMessageGroupInAttest: boolean = false;
    companyTimeDefaultTimePeriodHeadId: number = 0;
    companyTimeDefaultMonthlyReportId: number = 0;
    companyTimeDefaultPreviousTimePeriod: boolean = false;
    companyUseExtraShift: boolean = false;
    companyPayrollMinimumAttestStateId: number = 0;
    companyPayrollResultingAttestStateId: number = 0;
    companyPayrollLockedAttestStateId: number = 0;
    companyPayrollApproved1AttestStateId: number = 0;
    companyPayrollApproved2AttestStateId: number = 0;
    companyPayrollExportFileCreatedAttestStateId: number = 0;
    companyCalculateAnnualScheduledTime: boolean = false;
    companyTimeAttestTreeIncludeAdditionalEmployees: boolean = false;
    companyTimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod: boolean = false;
    toolbarIgnoreEmploymentStopDate: boolean = false;
    userSettingTimeAttestTreeIncludeAdditionalEmployees: boolean = false;
    userSettingTimeLatestTimePeriodType: TimePeriodSelectorType;
    userSettingTimeAttestTreeLatestGrouping: TermGroup_AttestTreeGrouping = TermGroup_AttestTreeGrouping.None;
    userSettingTimeAttestTreeLatestSorting: TermGroup_AttestTreeSorting = TermGroup_AttestTreeSorting.None;
    userSettingTimeAttestTreeDisableAutoLoad: boolean = false;
    userSettingTimeAttestTreeDoNotShowAttested: boolean = false;
    userSettingTimeAttestTreeDoShowEmptyGroups: boolean = false;
    userSettingTimeAttestTreeDoShowOnlyWithWarnings: boolean = false;
    userSettingTimeAttestTreeDoNotShowWithoutTransactions: boolean = false;
    userSettingTimeAttestTreeShowOnlyShiftSwaps: boolean = false;
    userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount: boolean = false;
    userSettingTimeAttestDisableSaveAttestWarning: boolean = false;
    userSettingTimeLatestAttestStateTo: number = 0;
    userSettingTimeAttestMessageGroupId: number = 0;
    unhandledEmployees: TimeUnhandledShiftChangesEmployeeDTO[] = null;

    //Buttons
    contentViewModeFunctions: any[] = [];
    calculationFunctions: any[] = [];
    selectedAttestStateOption: {};

    //Permissions
    readPermissionsLoaded: boolean = false;
    modifyPermissionsLoaded: boolean = false;

    //Current values and flags
    loadingTree: boolean = false;
    loadedContentEmployee: boolean = false;
    loadingTreeWarnings: boolean = false;
    loadedTreeWarnings = false;
    loadedContentWithShowAll: boolean = false;
    chartsVisible: boolean = false;
    currentGuid: Guid;
    currentTimerToken: number;
    currentTimePeriodSelectedDebounced: any;
    currentTimePeriodType: TimePeriodSelectorType;
    currentTimePeriodName: string;
    currentTimePeriodId: number;
    currentTimePeriod: TimePeriodDTO;
    get currentTimePeriodDescription() {
        if (!this.currentStartDate || !this.currentStopDate)
            return '';
        return this.currentStartDate.toLocaleDateString() + " - " + this.currentStopDate.toLocaleDateString();
    }
    get currentFilterAccountIds() {
        if (!this.currentEmployee || !this.currentEmployee.additionalOnAccountIds)
            return null;
        return this.currentEmployee.additionalOnAccountIds;
    }
    private _currentStartDate: Date = CalendarUtility.getDateToday();
    get currentStartDate() {
        return this._currentStartDate;
    }
    set currentStartDate(date: Date) {
        this._currentStartDate = date;
        if (this.currentTimePeriodType == TimePeriodSelectorType.Day) {
            this.setTimePeriodTypeDay(this._currentStartDate);
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Week) {
            this.setTimePeriodTypeWeek(this._currentStartDate);
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Month) {
            this.setTimePeriodTypeMonth(this._currentStartDate);
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Period) {
            var timePeriod = TimePayrollUtility.getTimePeriodFromDate(this.timePeriods, this._currentStartDate)
            if (!timePeriod)
                return;
            this.currentTimePeriod = timePeriod;
        }
        else {
            //Invaid type
            return;
        }

        if (this.isMyTime)
            this.loadEmployeeContent();
        else
            this.loadTreeDefault();
    }
    currentStopDate: Date;
    currentGroup: TimeEmployeeTreeGroupNodeDTO;
    currentEmployee: TimeEmployeeTreeNodeDTO;
    currentEmployeeGoToDebounced: any;
    currentEmployeeVacationPeriod: EmployeeVacationPeriodDTO;
    currentAttestStates: AttestStateDTO[] = [];
    currentAttestStateOptions: any = [{}];
    currentLoadingAttestStates: boolean;
    currentSumsExpanded: boolean = false;
    currentLastLoadAttestStatesEmployeeId: number = 0;
    currentLastLoadAttestStatesStartDate: Date;
    currentLastLoadAttestStatesStopDate: Date;
    currentLastLoadAttestMode: TimeAttestMode;
    currentTreeViewMode = TimeTreeViewMode.None;
    isCurrentTreeViewModeEmployeeOrGroup(): boolean {
        if (this.currentTreeViewModeIsGroup)
            return true;
        else if (this.currentTreeViewModeIsEmployee)
            return this.currentContentViewMode === TimeAttestContentViewMode.AttestEmployee;
        return false;
    }
    get currentTreeViewModeIsGroup(): boolean {
        return this.currentTreeViewMode == TimeTreeViewMode.Group;
    }
    get currentTreeViewModeIsEmployee(): boolean {
        return this.currentTreeViewMode == TimeTreeViewMode.Employee;
    }
    currentContentViewMode = TimeAttestContentViewMode.None;
    get currentContentViewModeIsAttestEmployee(): boolean {
        return this.currentContentViewMode == TimeAttestContentViewMode.AttestEmployee;
    }
    get currentContentViewModeIsAdditionAndDeduction(): boolean {
        return this.currentContentViewMode == TimeAttestContentViewMode.AdditionAndDeduction;
    }
    get currentContentViewModeIsTimeCalendar(): boolean {
        return this.currentContentViewMode == TimeAttestContentViewMode.TimeCalendar;
    }
    get currentContentViewModeIsAbsenceDetails(): boolean {
        return this.currentContentViewMode == TimeAttestContentViewMode.AbsenceDetails;
    }

    get showHideDaysWithoutScheduleCheckbox(): boolean {
        return false;
    }
    get currentTimeReportType(): TermGroup_TimeReportType {
        if (this.currentEmployee)
            return this.currentEmployee.timeReportType;
        else
            return TermGroup_TimeReportType.Stamp;
    }
    currentEmployeeLoading: boolean = false;
    hideDaysWithoutSchedule: boolean = false;

    //Absence
    private get showAbsenceFunctions(): boolean {
        return (
            this.showAbsenceFunction ||
            this.showAbsenceDetailsFunction
        );
    }
    private get showAbsenceFunction(): boolean {
        if (this.showAbsence) {
            if (this.currentContentViewModeIsAttestEmployee)
                return true;
            if (this.currentTreeViewModeIsGroup && this.getSelectedEmployeeIds().length === 1)
                return true;
        }
        return false;
    }
    private get showAbsenceDetailsFunction(): boolean {
        return this.currentContentViewModeIsAbsenceDetails && this.editAbsenceDetailsPermission && this.hasContentIgnoreSelectedForGroup();
    }

    //Restore
    private get showRestoreFunctions(): boolean {
        return this.showRestoreToScheduleFunction || this.showRestoreToTemplateScheduleFunction || this.showRestoreToScheduleDiscardDeviationsFunction;
    }
    private get showRestoreToScheduleFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.restoreToSchedulePermission && this.hasContentIgnoreSelectedForGroup();
    }
    private get showRestoreToTemplateScheduleFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.restoreScheduleToTemplatePermission && this.hasContentIgnoreSelectedForGroup();
    }
    private get showRestoreToScheduleDiscardDeviationsFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.restoreToSchedulePermission && this.hasContentIgnoreSelectedForGroup();
    }
    //Transactions
    private get showTransactionFunctions(): boolean {
        return this.showRegenerateDaysBasedOnTimeStampsFunction || this.showRegenerateTransactionsFunction || this.showRecalculateAccountingFunction || this.showDeleteTimeBlocksAndTransactionsFunction || this.showReverseTransactionsFunctions;
    }
    private get showRegenerateDaysBasedOnTimeStampsFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.reGenerateDaysBasedOnTimeStampsPermission && this.hasContentIgnoreSelectedForGroup();
    }
    private get showRegenerateTransactionsFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.regenerateTransactionsPermission && this.hasContentIgnoreSelectedForGroup();
    }
    private get showRecalculateAccountingFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.recalculateAccountingPermission && this.hasContentIgnoreSelectedForGroup();
    }
    private get showDeleteTimeBlocksAndTransactionsFunction(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.deleteTransactionsPermission && this.hasContentIgnoreSelectedForGroup(true);
    }
    private get showReverseTransactionsFunctions(): boolean {
        return this.isCurrentTreeViewModeEmployeeOrGroup() && this.currentTreeViewModeIsEmployee && this.reverseTransactionsPermission && this.hasContentIgnoreSelectedForGroup();
    }
    //Attest
    private get showAttestFunction(): boolean {
        return this.showAttestReminderFunction ||
            this.showRunAutoAttestFunction;
    }
    private get showAttestReminderFunction(): boolean {
        return this.currentTreeViewModeIsGroup && this.sendAttestReminderPermission && this.hasContentIgnoreSelectedForGroup();
    }
    private get showRunAutoAttestFunction(): boolean {
        return this.currentTreeViewModeIsGroup && this.hasContentIgnoreSelectedForGroup();
    }
    private get ShowCalculatePeriodsFunction(): boolean {
        return this.currentTreeViewModeIsGroup && this.hasContentIgnoreSelectedForGroup() && this.companyTimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod;
    }

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Charts
    private chartsCreated: boolean = false;
    private groupChartsCreated: boolean = false;

    // Annual summary
    private planningPeriodColors: string[] = ["da1e28", "24a148", "0565c9"]; // @soe-color-semantic-error, @soe-color-semantic-success, @soe-color-semantic-information
    private get planningPeriodColorOver(): string {
        return `#${this.planningPeriodColors[0]}`;
    }
    private get planningPeriodColorEqual(): string {
        return `#${this.planningPeriodColors[1]}`;
    }
    private get planningPeriodColorUnder(): string {
        return `#${this.planningPeriodColors[2]}`;
    }

    constructor(
        private timePeriodAccountValueId: number,
        private $timeout: ng.ITimeoutService,
        private $window: ng.IWindowService,
        $uibModal,
        private $http,
        private $templateCache,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private timeService: ITimeService,
        private sharedTimeService: ISharedTimeService,
        private reportService: IReportService,
        private reportDataService: IReportDataService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private feature: Feature) {
        super("Time.Time.TimeAttest.Edit", feature, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.modalInstance = $uibModal;
        this.treeUrl = urlHelperService.getViewUrl("attestTree.html");
        this.timeTreeUrl = urlHelperService.getCoreComponent("timeTree.html");
        this.periodSelectorUrl = urlHelperService.getViewUrl("periodSelector.html");
        this.currentTimePeriodSelectedDebounced = _.debounce(this.setTimePeriodChanged, 1000);
        if (!this.isMyTime)
            this.currentEmployeeGoToDebounced = _.debounce(this.setEmployeeChanged, 1000);

        // Config parameters
        this.employeeId = soeConfig.employeeId;
        this.attestMode = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;

        this.initGrid();
    }

    // SETUP

    protected setupLookups() {
        this.setupPeriodSelector(false);
        this.loadTerms(); //must be called after permissions in base class is done            
        this.startLoad();

        return this.$q.all([
            this.loadCompanySettings(),
            this.loadModifyPermissions(),
            this.loadReadPermissions(),
        ]).then(() => {
            this.$q.all([
                this.loadUserSettings(),
            ]).then(() => {
                this.loadToolbarSorting();
                this.loadToolbarGrouping();
                this.loadTimePeriods();
                this.isDirty = false;
                this.stopProgress();
            });
        });
    }

    private createCharts() {
        this.chartsCreated = true;
    }

    private reloadCharts() {
        this.$scope.$broadcast(Constants.EVENT_RELOAD_CHARTS, { guid: this.guid });
    }

    private createGroupCharts() {
        this.groupChartsCreated = true;
    }

    private reloadGroupCharts() {
        this.$scope.$broadcast(Constants.EVENT_RELOAD_GROUP_CHARTS, { guid: this.guid });
    }

    private setupToolBar() {
        if (this.gridButtonGroups) {
            this.gridButtonGroups.length = 0;
        }
        if (this.setupDefaultToolBar()) {

            if (this.currentTreeViewModeIsGroup) {
                //Reload
                this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                    this.loadGroupContent(true, true);
                })));
                // Print
                this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.print", IconLibrary.FontAwesome, "fa-print", () => {
                    this.print();
                }, () => {
                    return !this.hasSelectedGroupContent();
                })));
            }
            if (this.currentTreeViewModeIsEmployee) {
                if (this.currentContentViewModeIsAttestEmployee) {
                    //Reload
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.loadEmployeeContent(null, true, true);
                    })));
                    //Annual summary
                    if (this.companyCalculateAnnualScheduledTime) {
                        this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.schedule.planning.showannualsummary", IconLibrary.FontAwesome, "fa-clock", () => {
                            this.openAnnualSummary();
                        })));
                    }
                    // Print
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.print", IconLibrary.FontAwesome, "fa-print", () => {
                        this.print();
                    }, () => {
                        return !this.currentEmployee;
                    })));
                    //Hide days without schedule not-active
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.time.attest.hidedayswithoutschedule", IconLibrary.FontAwesome, "fa-calendar-minus", () => {
                        this.setHideDaysWithoutSchedule();
                    }, () => {

                    }, () => {
                        return !this.toolbarShowOptionHideDaysWithoutScheduleNotActive();
                    })));
                    //Hide days without schedule active
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.time.attest.hidedayswithoutschedule", IconLibrary.FontAwesome, "fa-calendar-minus active", () => {
                        this.setHideDaysWithoutSchedule();
                    }, () => {

                    }, () => {
                        return !this.toolbarShowOptionHideDaysWithoutScheduleActive();
                    })));
                } else if (this.currentContentViewModeIsAdditionAndDeduction) {
                    //Reload
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.loadAdditionDeductionTransactions(true);
                    })));
                } else if (this.currentContentViewModeIsAbsenceDetails) {
                    //Reload
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.loadAbsenceDetails();
                    })));
                }
            }
        }
    }

    private initGrid() {
        this.messagingService.subscribe(Constants.EVENT_ATTESTGROUP_ROWS_SELECTED, (selectedItems: AttestEmployeePeriodDTO[]) => {
            this.contentGroupSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ATTESTGROUP_ROWS_FILTERED, (data: { rows: AttestEmployeePeriodDTO[], totalCount: number }) => {
            if (this.currentGroup) {
                this.currentGroup.filteredTimeEmployeePeriods = data.rows;
                this.currentGroup.totalCount = data.totalCount;
                if (this.currentGroup.employeeNodes) {
                    _.forEach(this.currentGroup.employeeNodes, (employeeNode: TimeEmployeeTreeNodeDTO) => {
                        employeeNode.visible = _.filter(this.currentGroup.filteredTimeEmployeePeriods, { employeeId: employeeNode.employeeId }).length > 0;
                    });
                    this.currentGroup.expanded = true;
                }
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ATTESTEMPLOYEE_ROWS_SELECTED, (selectedItems: AttestEmployeeDayDTO[]) => {
            this.contentEmployeeSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ATTESTEMPLOYEE_ROWS_RELOAD, (data: { date: Date, fromModal: boolean }) => {
            this.loadEmployeeContent(data.date, true, false, data.fromModal);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ATTESTEMPLOYEE_ROWSANDWARNINGS_RELOAD, (data: { date: Date, fromModal: boolean }) => {
            this.loadEmployeeContent(data.date, true, false, data.fromModal);
            if (this.currentEmployee)
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ADDITIONDEDUCTION_ROWS_RELOAD, () => {
            this.loadAdditionDeductionTransactions(true);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ADDITIONDEDUCTION_ROWS_SELECTED, (selectedItems: AttestEmployeeAdditionDeductionDTO[]) => {
            this.contentAdditionDeductionSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ABSENCEDETAILS_ROWS_RELOAD, () => {
            this.loadAbsenceDetails();
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ABSENCEDETAILS_ROWS_SELECTED, (selectedItems: TimeAbsenceDetailDTO[]) => {
            this.contentAbsenceDetailsSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SAVE_START, () => {
            this.startSave(false);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SAVE_COMPLETE, () => {
            this.completedSave(null, true);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SAVE_FAILED, (message: string) => {
            this.failedSave(message);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_RELOAD_SHIFTS_FOR_EMPLOYEE, (message: string) => {
            this.loadEmployeeContent();
            this.refreshTreeForEmployee(this.currentEmployee.employeeId);
        }, this.$scope);
    }

    private loadTerms() {
        var keys: string[] = [
            "core.loading",
            "core.warning",
            "core.donotshowagain",
            "time.employee.name",
            "time.schedule.planning.breakprefix",
            "time.atteststate.state",
            "time.time.attest.accounts",
            "time.time.attest.attestemployee",
            "time.time.attest.additiondeduction",
            "time.time.attest.absencedetails",
            "time.time.attest.timecalendar",
            "time.time.attest.restoretoschedule",
            "time.time.attest.restoretoschedulediscarddeviations",
            "time.time.attest.restorescheduletotemplate",
            "time.time.attest.restorefromtimestamps",
            "time.time.attest.absence",
            "time.time.attest.regeneratetransactions",
            "time.time.attest.regeneratevacationstransactions",
            "time.time.attest.recalculateaccounting",
            "time.time.attest.deletetimeblockandtransactions",
            "time.time.attest.reversetransactions",
            "time.time.attest.sendattestreminder",
            "time.time.attest.saveattestresultvalid",
            "time.time.attest.saveattestresultinvalid",
            "time.time.attest.saveattestemployees",
            "time.time.attest.tree.autoattest",
            "time.time.attest.tree.autoattest.filtered",
            "time.time.attest.tree.autoattest.runs",
            "time.time.attest.tree.autoattest.failed",
            "time.time.attest.cleartimerulecache",
            "time.time.attest.clearingtimerulecache",
            "time.time.attest.partlyadditional",
            "time.time.attest.completelyadditional",
            "time.time.attest.absencedetails.update",
            "time.time.attest.calculate.overtime",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;
        });
    }

    private setupPeriodSelector(excludePeriod: boolean) {

        var keys: string[] = [
            "core.loading",
            "common.day",
            "common.week",
            "common.month",
            "common.period",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.timePeriodTypeTerms = terms;
            this.currentTimePeriodType = TimePeriodSelectorType.None;
            this.currentTimePeriodName = terms["core.loading"];
            this.setTimePeriodTypes(excludePeriod);
        });
    }

    private setTimePeriodTypes(excludePeriod: boolean) {
        this.timePeriodTypes = [];
        this.timePeriodTypes.push(new SmallGenericType(TimePeriodSelectorType.Day, this.timePeriodTypeTerms["common.day"]));
        this.timePeriodTypes.push(new SmallGenericType(TimePeriodSelectorType.Week, this.timePeriodTypeTerms["common.week"]));
        this.timePeriodTypes.push(new SmallGenericType(TimePeriodSelectorType.Month, this.timePeriodTypeTerms["common.month"]));
        if (!excludePeriod)
            this.timePeriodTypes.push(new SmallGenericType(TimePeriodSelectorType.Period, this.timePeriodTypeTerms["common.period"]));
    }

    // LOOKUPS

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DoNotUseMessageGroupInAttest);
        settingTypes.push(CompanySettingType.TimeDefaultTimePeriodHead);
        settingTypes.push(CompanySettingType.TimeDefaultMonthlyReport);
        settingTypes.push(CompanySettingType.TimeDefaultPreviousTimePeriod);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
        settingTypes.push(CompanySettingType.SalaryExportPayrollMinimumAttestStatus);
        settingTypes.push(CompanySettingType.SalaryExportPayrollResultingAttestStatus);
        settingTypes.push(CompanySettingType.SalaryPaymentLockedAttestStateId);
        settingTypes.push(CompanySettingType.SalaryPaymentApproved1AttestStateId);
        settingTypes.push(CompanySettingType.SalaryPaymentApproved2AttestStateId);
        settingTypes.push(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId);
        settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTime);
        settingTypes.push(CompanySettingType.TimeAttestTreeIncludeAdditionalEmployees);
        settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);
        settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeColors);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.companyUseAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.companyUseMessageGroupInAttest = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.DoNotUseMessageGroupInAttest);
            this.companyTimeDefaultTimePeriodHeadId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultTimePeriodHead);
            this.companyTimeDefaultMonthlyReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultMonthlyReport);
            this.companyTimeDefaultPreviousTimePeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeDefaultPreviousTimePeriod);
            this.companyUseExtraShift = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
            this.companyPayrollMinimumAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportPayrollMinimumAttestStatus);
            this.companyPayrollResultingAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportPayrollResultingAttestStatus);
            this.companyPayrollLockedAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentLockedAttestStateId);
            this.companyPayrollApproved1AttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentApproved1AttestStateId);
            this.companyPayrollApproved2AttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentApproved2AttestStateId);
            this.companyPayrollExportFileCreatedAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId);
            this.companyCalculateAnnualScheduledTime = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTime);
            this.companyTimeAttestTreeIncludeAdditionalEmployees = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeAttestTreeIncludeAdditionalEmployees);
            this.companyTimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);

            const planningPeriodColorString = SettingsUtility.getStringCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeColors);
            let colors = planningPeriodColorString && planningPeriodColorString !== ';;' ? planningPeriodColorString.split(';') : [];
            // Override default colors
            if (colors.length > 0 && colors[0])
                this.planningPeriodColors[0] = colors[0];
            if (colors.length > 1 && colors[1])
                this.planningPeriodColors[1] = colors[1];
            if (colors.length > 2 && colors[2])
                this.planningPeriodColors[2] = colors[2];
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(UserSettingType.TimeLatestTimePeriodType);
        settingTypes.push(UserSettingType.TimeAttestTreeLatestGrouping);
        settingTypes.push(UserSettingType.TimeAttestTreeLatestSorting);
        settingTypes.push(UserSettingType.TimeAttestTreeDisableAutoLoad);
        settingTypes.push(UserSettingType.TimeAttestTreeDoNotShowAttested);
        settingTypes.push(UserSettingType.TimeAttestTreeDoShowEmptyGroups);
        settingTypes.push(UserSettingType.TimeAttestTreeDoShowOnlyWithWarnings);
        settingTypes.push(UserSettingType.TimeAttestTreeDoNotShowWithoutTransactions);
        settingTypes.push(UserSettingType.TimeAttestTreeShowOnlyShiftSwaps);
        settingTypes.push(UserSettingType.TimeAttestTreeMessageGroupId);
        if (this.companyUseAccountHierarchy)
            settingTypes.push(UserSettingType.TimeAttestTreeIncludeAdditionalEmployees);
        settingTypes.push(UserSettingType.TimeAttestTreeDoNotShowDaysOutsideEmployeeAccount);
        settingTypes.push(UserSettingType.TimeDisableApplySaveAttestWarning);
        settingTypes.push(UserSettingType.TimeLatestAttestStateTo);

        return this.coreService.getUserSettings(settingTypes).then(result => {
            this.userSettingTimeLatestTimePeriodType = SettingsUtility.getIntUserSetting(result, UserSettingType.TimeLatestTimePeriodType, this.defaultTimePeriodType, false);
            this.userSettingTimeAttestTreeLatestGrouping = SettingsUtility.getIntUserSetting(result, UserSettingType.TimeAttestTreeLatestGrouping, TermGroup_AttestTreeGrouping.All, false);
            this.userSettingTimeAttestTreeLatestSorting = SettingsUtility.getIntUserSetting(result, UserSettingType.TimeAttestTreeLatestSorting, TermGroup_AttestTreeSorting.FirstName, false);
            this.userSettingTimeAttestTreeDisableAutoLoad = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeDisableAutoLoad);
            this.userSettingTimeAttestTreeDoNotShowAttested = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeDoNotShowAttested);
            this.userSettingTimeAttestTreeDoShowEmptyGroups = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeDoShowEmptyGroups);
            this.userSettingTimeAttestTreeDoShowOnlyWithWarnings = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeDoShowOnlyWithWarnings);
            this.userSettingTimeAttestTreeDoNotShowWithoutTransactions = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeDoNotShowWithoutTransactions);
            this.userSettingTimeAttestTreeShowOnlyShiftSwaps = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeShowOnlyShiftSwaps);
            this.userSettingTimeAttestMessageGroupId = SettingsUtility.getIntUserSetting(result, UserSettingType.TimeAttestTreeMessageGroupId);
            if (this.companyUseAccountHierarchy)
                this.userSettingTimeAttestTreeIncludeAdditionalEmployees = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeIncludeAdditionalEmployees, this.companyTimeAttestTreeIncludeAdditionalEmployees);
            this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeAttestTreeDoNotShowDaysOutsideEmployeeAccount);
            this.userSettingTimeAttestDisableSaveAttestWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.TimeDisableApplySaveAttestWarning);
            this.userSettingTimeLatestAttestStateTo = SettingsUtility.getIntUserSetting(result, UserSettingType.TimeLatestAttestStateTo);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        if (this.isMyTime) {
            featureIds.push(Feature.Time_Time_AttestUser_RestoreToSchedule);
            featureIds.push(Feature.Time_Time_AttestUser_RestoreScheduleToTemplate);
            featureIds.push(Feature.Time_Time_AttestUser_ReGenerateDaysBasedOnTimeStamps);
            featureIds.push(Feature.Time_Time_AttestUser_RegenerateTransactions);
            featureIds.push(Feature.Time_Time_AttestUser_DeleteTransactions);
            featureIds.push(Feature.Time_Time_AttestUser_AbsenceDetails);
        }
        else {
            featureIds.push(Feature.Time_Time_Attest_RestoreToSchedule);
            featureIds.push(Feature.Time_Time_Attest_RestoreScheduleToTemplate);
            featureIds.push(Feature.Time_Time_Attest_RestoreFromTimeStamps);
            featureIds.push(Feature.Time_Time_Attest_RegenerateTransactions);
            featureIds.push(Feature.Time_Time_Attest_DeleteTransactions);
            featureIds.push(Feature.Time_Time_Attest_ReverseTransactions);
            featureIds.push(Feature.Time_Time_Attest_SendAttestReminder);
            featureIds.push(Feature.Time_Time_Attest_RecalculateAccunting);
            featureIds.push(Feature.Time_Time_Attest_AbsenceDetails);
            featureIds.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
        }
        featureIds.push(Feature.Billing_Project_Attest_User);
        featureIds.push(Feature.Billing_Project_Attest_Other);
        //modifyPermissions is loaded in base class

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (this.isMyTime) {
                this.restoreToSchedulePermission = x[Feature.Time_Time_AttestUser_RestoreToSchedule];
                this.restoreScheduleToTemplatePermission = x[Feature.Time_Time_AttestUser_RestoreScheduleToTemplate];
                this.reGenerateDaysBasedOnTimeStampsPermission = x[Feature.Time_Time_AttestUser_ReGenerateDaysBasedOnTimeStamps];
                this.regenerateTransactionsPermission = x[Feature.Time_Time_AttestUser_RegenerateTransactions];
                this.deleteTransactionsPermission = x[Feature.Time_Time_AttestUser_DeleteTransactions];
                this.editAbsenceDetailsPermission = x[Feature.Time_Time_AttestUser_AbsenceDetails];
            }
            else {
                this.restoreToSchedulePermission = x[Feature.Time_Time_Attest_RestoreToSchedule];
                this.restoreScheduleToTemplatePermission = x[Feature.Time_Time_Attest_RestoreScheduleToTemplate];
                this.reGenerateDaysBasedOnTimeStampsPermission = x[Feature.Time_Time_Attest_RestoreFromTimeStamps];
                this.regenerateTransactionsPermission = x[Feature.Time_Time_Attest_RegenerateTransactions];
                this.deleteTransactionsPermission = x[Feature.Time_Time_Attest_DeleteTransactions];
                this.reverseTransactionsPermission = x[Feature.Time_Time_Attest_ReverseTransactions];
                this.sendAttestReminderPermission = x[Feature.Time_Time_Attest_SendAttestReminder];
                this.recalculateAccountingPermission = x[Feature.Time_Time_Attest_RecalculateAccunting];
                this.editAbsenceDetailsPermission = x[Feature.Time_Time_Attest_AbsenceDetails];
                this.socialSecPermission = x[Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec];
            }
            this.projectAttestUser = x[Feature.Billing_Project_Attest_User];
            this.projectAttestOthers = x[Feature.Billing_Project_Attest_Other];
            this.modifyPermissionsLoaded = true;
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        if (this.isMyTime) {
            featureIds.push(Feature.Time_Employee_Employees_Edit_MySelf);
            featureIds.push(Feature.Time_Time_AttestUser_DontSeeFuturePlacements);
            featureIds.push(Feature.Time_Time_AttestUser_Edit);
            featureIds.push(Feature.Time_Time_AttestUser_ShowAccumulators);
            featureIds.push(Feature.Time_Time_AttestUser_EditSchedule);
            featureIds.push(Feature.Time_Time_AttestUser_EditAbsence);
            featureIds.push(Feature.Time_Time_AttestUser_AdditionAndDeduction);
            featureIds.push(Feature.Time_Time_AttestUser_AbsenceDetails);
        }
        else {
            featureIds.push(Feature.Time_Employee_Employees_Edit);
            featureIds.push(Feature.Time_Time_Attest_Edit);
            featureIds.push(Feature.Time_Time_Attest_ShowAccumulators);
            featureIds.push(Feature.Time_Time_Attest_EditSchedule);
            featureIds.push(Feature.Time_Time_Attest_AdditionAndDeduction);
            featureIds.push(Feature.Time_Time_Attest_AbsenceDetails);
        }

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (this.isMyTime) {
                this.editEmployeePermission = x[Feature.Time_Employee_Employees_Edit_MySelf];
                this.dontSeeFuturePlacementsPermission = x[Feature.Time_Time_AttestUser_DontSeeFuturePlacements];
                this.editDayPermission = x[Feature.Time_Time_AttestUser_Edit];
                this.editSchedulePermission = x[Feature.Time_Time_AttestUser_EditSchedule];
                this.editAbsencePermission = x[Feature.Time_Time_AttestUser_EditAbsence];
                this.showAccumulatorsPermission = x[Feature.Time_Time_AttestUser_ShowAccumulators];
                this.showAdditionDeductionPermission = x[Feature.Time_Time_AttestUser_AdditionAndDeduction];
                this.showAbsenceDetailsPermission = x[Feature.Time_Time_AttestUser_AbsenceDetails];
            }
            else {
                this.editEmployeePermission = x[Feature.Time_Employee_Employees_Edit];
                this.editDayPermission = x[Feature.Time_Time_Attest_Edit];
                this.editSchedulePermission = x[Feature.Time_Time_Attest_EditSchedule];
                this.showAccumulatorsPermission = x[Feature.Time_Time_Attest_ShowAccumulators];
                this.showAdditionDeductionPermission = x[Feature.Time_Time_Attest_AdditionAndDeduction];
                this.showAbsenceDetailsPermission = x[Feature.Time_Time_Attest_AbsenceDetails];
            }
            this.readPermissionsLoaded = true;
        });
    }

    private loadToolbarGrouping(): ng.IPromise<any> {
        if (!this.doLoadToolbar())
            return;

        return this.coreService.getTermGroupContent(TermGroup.TimeAttestTreeGrouping, false, false).then((result: SmallGenericType[]) => {
            this.toolbarGroupingOptions = result;

            if (this.companyUseAccountHierarchy) {
                var optionEmployeeAuthModel = (_.filter(this.toolbarGroupingOptions, { id: TermGroup_AttestTreeGrouping.EmployeeAuthModel }))[0];
                if (optionEmployeeAuthModel)
                    optionEmployeeAuthModel.name = this.termsArray["time.time.attest.accounts"];
            }
        });
    }

    private loadToolbarSorting(): ng.IPromise<any> {
        if (!this.doLoadToolbar())
            return;

        return this.coreService.getTermGroupContent(TermGroup.TimeAttestTreeSorting, false, false).then((result: SmallGenericType[]) => {
            this.toolbarSortingOptions = result;
        });
    }

    private loadToolbarSelectionCategories() {
        if (!this.doLoadToolbar())
            return;

        var selectedIds: number[] = [];
        if (this.toolbarSelectionCategoriesSelected) {
            _.forEach(this.toolbarSelectionCategoriesSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        this.coreService.getCategoriesForRoleFromType(this.employeeId, SoeCategoryType.Employee, true, true, false).then((result: ISmallGenericType[]) => {
            this.toolbarSelectionCategories.length = 0;
            this.toolbarSelectionCategoriesSelected.length = 0;
            _.forEach(result, (category: ISmallGenericType) => {
                var item = {
                    id: category.id,
                    label: category.name
                };
                this.toolbarSelectionCategories.push(item);
                if (selectedIds.some(id => id == category.id))
                    this.toolbarSelectionCategoriesSelected.push(item);
            });
            this.isDirty = false;
            this.loadToolbarSelectionEmployees();
        });
    }

    private loadToolbarSelectionAccounts() {
        if (!this.doLoadToolbar())
            return;

        var selectedIds: number[] = [];
        if (this.toolbarSelectionAccountsSelected) {
            _.forEach(this.toolbarSelectionAccountsSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        return this.coreService.getAccountsFromHierarchyByUserSetting(this.currentStartDate, this.currentStopDate, false, false, true, false).then(result => {
            this.toolbarSelectionAccounts.length = 0;
            this.toolbarSelectionAccountsSelected.length = 0;
            _.forEach(result, (account) => {
                var item = {
                    id: account.accountId,
                    label: account.name + " (" + account.accountDim?.name + ")",
                };
                this.toolbarSelectionAccounts.push(item);
                if (selectedIds.some(id => id == account.accountId))
                    this.toolbarSelectionAccountsSelected.push(item);
            });
            this.isDirty = false;
            this.loadToolbarSelectionEmployees();
        });
    }

    private loadToolbarSelectionEmployees() {
        if (!this.doLoadToolbar())
            return;

        var selectedIds: number[] = [];
        if (this.toolbarSelectionEmployeesSelected) {
            _.forEach(this.toolbarSelectionEmployeesSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        var filterGroupIds = StringUtility.getCollectionIdsStr(this.companyUseAccountHierarchy ? this.toolbarSelectionCategoriesSelected : this.toolbarSelectionAccountsSelected);

        this.timeService.getEmployeesForTimeAttestTree(filterGroupIds, this.currentStartDate, this.currentStopDate).then((result: AttestEmployeeListDTO[]) => {
            this.toolbarSelectionEmployees.length = 0;
            this.toolbarSelectionEmployeesSelected.length = 0;
            _.forEach(_.sortBy(result, t => t.employeeNrSort), (employee: AttestEmployeeListDTO) => {
                var selectionEmployee = {
                    id: employee.employeeId,
                    label: "(" + employee.employeeNr + ") " + employee.name
                };
                this.toolbarSelectionEmployees.push(selectionEmployee);
                if (selectedIds.some(id => id == employee.employeeId))
                    this.toolbarSelectionEmployeesSelected.push(selectionEmployee);
            });
            this.isDirty = false;
        });
    }

    private loadToolbarSelectionAttestStates() {
        if (!this.doLoadToolbar())
            return;

        var selectedIds: number[] = [];
        if (this.toolbarSelectionAttestStatesSelected) {
            _.forEach(this.toolbarSelectionAttestStatesSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        this.timeService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, this.currentStartDate, this.currentStopDate, true, this.isMySelf ? this.currentEmployee.employeeGroupId : null).then((result) => {
            this.toolbarSelectionAttestStates.length = 0;
            this.toolbarSelectionAttestStatesSelected.length = 0;

            _.forEach(result, (attestState: any) => {
                var item = {
                    id: attestState.attestStateId,
                    label: attestState.name
                };
                this.toolbarSelectionAttestStates.push(item);
                if (selectedIds.some(id => id == attestState.attestStateId))
                    this.toolbarSelectionAttestStatesSelected.push(item);
            });
            this.isDirty = false;
        });
    }

    private loadToolbarSelectionMessageGroups() {
        if (!this.doLoadToolbar())
            return;

        this.timeService.getAttestTreeMessageGroups().then((result: MessageGroupDTO[]) => {

            this.messageGroups = [];
            this.messageGroups.push({
                id: 0,
                name: ''
            });

            _.forEach(result, (messageGroup: MessageGroupDTO) => {
                var item: SmallGenericType = {
                    id: messageGroup.messageGroupId,
                    name: messageGroup.name
                };

                if (messageGroup.messageGroupId == this.userSettingTimeAttestMessageGroupId && messageGroup.noUserValidation) {
                    this.userSettingTimeAttestMessageGroupId = 0;
                    this.saveUserSettingTimeAttestMessageGroupId();
                }

                this.messageGroups.push(item);
            });
        });
        this.isDirty = false;
    }

    private loadToolbarSelection() {
        if (!this.doLoadToolbar())
            return;

        this.loadToolbarSelectionAttestStates();
        this.loadToolbarSelectionMessageGroups();
        if (this.companyUseAccountHierarchy)
            this.loadToolbarSelectionAccounts();
        else
            this.loadToolbarSelectionCategories();
    }

    private doLoadToolbar(): boolean {
        return !this.isMyTime;
    }

    // EVENTS  

    protected toolbarSelectionTimePeriodTypeChanged(timePeriodType) {
        this.setTimePeriodType(timePeriodType);
        this.saveUserSettingTimeLatestTimePeriodType();
    }

    protected toolbarSelectionCurrentDateChanged() {
        if (!this.hasPermissionToNextPeriod())
            return;
        this.$timeout(() => {
            this.setTimePeriodType(this.currentTimePeriodType);
        });
    }

    protected toolbarPrevPeriodGoTo() {
        if (!this.toolbarPrevPeriodValid())
            return;

        if (this.currentTimePeriodType == TimePeriodSelectorType.Day) {
            this.setTimePeriodTypeDay(this.currentStartDate.addDays(-1));
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Week) {
            this.setTimePeriodTypeWeek(this.currentStartDate.addWeeks(-1));
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Month) {
            this.setTimePeriodTypeMonth(this.currentStartDate.addMonths(-1));
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Period) {
            var previousTimePeriod = TimePayrollUtility.getPreviousTimePeriod(this.timePeriods, this.currentTimePeriodId);
            if (!previousTimePeriod)
                return;
            this.setTimePeriodTypePeriod(this.getTimePeriodFromId(previousTimePeriod.timePeriodId));
        }
        else {
            //Invaid type
            return;
        }

        this.currentTimePeriodSelectedDebounced();
    }

    protected toolbarNextPeriodGoTo() {
        if (this.currentTimePeriodType == TimePeriodSelectorType.Day) {
            this.setTimePeriodTypeDay(this.currentStartDate.addDays(1));
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Week) {
            this.setTimePeriodTypeWeek(this.currentStartDate.addWeeks(1));
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Month) {
            this.setTimePeriodTypeMonth(this.currentStartDate.addMonths(1));
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Period) {
            var nextTimePeriod = TimePayrollUtility.getNextTimePeriod(this.timePeriods, this.currentTimePeriodId);
            if (!nextTimePeriod)
                return;
            this.setTimePeriodTypePeriod(this.getTimePeriodFromId(nextTimePeriod.timePeriodId));
        }
        else {
            return;
        }

        this.currentTimePeriodSelectedDebounced();
    }

    protected toolbarPrevPeriodValid(): boolean {
        return this.hasLoadedTimePeriodTypes && this.hasPrevPeriod();
    }

    protected toolbarNextPeriodValid(): boolean {
        return this.hasLoadedTimePeriodTypes && this.hasNextPeriod() && this.hasPermissionToNextPeriod();
    }

    protected toolbarFilterShow() {
        this.toolbarFilterActive = !this.toolbarFilterActive;
        this.toolbarGroupingActive = false;
        this.toolbarSortingActive = false;
    }

    protected isToolbarFilterActiveOrOpen(): boolean {
        return this.toolbarFilterActive || this.hasToolbarFilter()
    }

    protected toolbarSelectionCategoriesChanged() {
        this.loadToolbarSelectionEmployees();
        this.loadAfterSelection();
    }

    protected toolbarSelectionCategoriesDisabled(): boolean {
        if (!this.hasSelectableCategories())
            return true;
        return false;
    }

    protected toolbarSelectionAccountsChanged() {
        this.loadToolbarSelectionEmployees();
        this.loadAfterSelection();
    }

    protected toolbarSelectionAccountsDisabled(): boolean {
        if (!this.hasSelectableAccounts())
            return true;
        return false;
    }

    protected toolbarSelectionEmployeesChanged() {
        this.loadAfterSelection();
    }

    protected toolbarSelectionAttestStatesChanged() {
        this.loadAfterSelection();
    }

    protected toolbarSelectionEmployeesDisabled(): boolean {
        if (!this.hasSelectableEmployees())
            return true;
        return false;
    }

    protected toolbarSelectionAttestStatesDisabled(): boolean {
        if (!this.hasSelectableAttestStates())
            return true;
        return false;
    }

    protected toolbarGroupingShow() {
        this.toolbarGroupingActive = !this.toolbarGroupingActive;
        this.toolbarFilterActive = false;
        this.toolbarSortingActive = false;
    }

    protected toolbarGroupingDisabled(): boolean {
        return !this.hasLoadedTimePeriodTypes();
    }

    protected toolbarGroupingChanged(id) {
        this.userSettingTimeAttestTreeLatestGrouping = id;
        var forceLoad = true;
        if (this.userSettingTimeAttestTreeDisableAutoLoad && !this.tree)
            forceLoad = false;
        this.loadTreeDefault(forceLoad);
        this.saveUserSettingTimeAttestTreeLatestGrouping();
    }

    protected toolbarSortingShow() {
        this.toolbarSortingActive = !this.toolbarSortingActive;
        this.toolbarFilterActive = false;
        this.toolbarGroupingActive = false;
    }

    protected toolbarDisableAutoLoadChanged() {
        this.saveUserSettingDisableAutoLoad();
    }

    protected toolbarSortingDisabled(): boolean {
        return !this.hasLoadedTimePeriodTypes();
    }

    protected toolbarSortingChanged(id) {
        this.userSettingTimeAttestTreeLatestSorting = id;
        var forceLoad = true;
        if (this.userSettingTimeAttestTreeDisableAutoLoad && !this.tree)
            forceLoad = false;
        this.saveUserSettingTimeAttestTreeLatestSorting();
        this.loadTreeDefault(forceLoad);
    }

    protected toolbarDoNotShowAttestedChanged() {
        this.saveUserSettingTimeAttestTreeDoNotShowAttested();
    }

    protected toolbarDoShowEmptyGroupsChanged() {
        this.saveUserSettingTimeAttestTreeDoShowEmptyGroups();
    }

    protected toolbarDoShowOnlyWithWarningsChanged() {
        this.saveUserSettingTimeAttestTreeDoShowOnlyWithWarnings();
    }

    protected toolbarDoShowOnlyShiftSwapsChanged() {
        this.saveUserSettingTimeAttestTreeShowOnlyShiftSwaps();
    }

    protected toolbarDoNotShowWithoutTransactionsChanged() {
        this.saveUserSettingTimeAttestTreeDoNotShowWithoutTransactions();
    }

    protected toolbarIncludeAdditionalEmployeesChanged() {
        this.saveUserSettingTimeAttestTreeIncludeAdditionalEmployees();
    }

    protected toolbarDoNotShowDaysOutsideEmployeeAccountChanged() {
        this.saveUserSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount();
    }

    protected toolbarMessageGroupIdChanged(id: any) {
        this.userSettingTimeAttestMessageGroupId = id;
        this.saveUserSettingTimeAttestMessageGroupId();
        if (!this.userSettingTimeAttestTreeDisableAutoLoad || this.tree)
            this.toolbarReload();
    }

    protected toolbarAutoAttestDisabled() {
        return !this.tree || this.loadingTree || !this.hasLoadedTimePeriodTypes();
    }

    protected toolbarLoadTreeDisabled(): boolean {
        return this.loadingTree || !this.hasLoadedTimePeriodTypes();
    }

    protected toolbarShowAttestGroup() {
        return this.currentTreeViewModeIsGroup && this.modifyPermission;
    }

    protected toolbarShowFunctionsGroup() {
        return this.currentTreeViewModeIsGroup && this.modifyPermission && this.calculationFunctions.length > 0;
    }

    protected toolbarShowAttestEmployee() {
        return this.currentTreeViewModeIsEmployee && this.currentContentViewModeIsAttestEmployee && this.modifyPermission;
    }

    protected toolbarShowAttestAdditionDeduction() {
        return this.currentTreeViewModeIsEmployee && this.currentContentViewModeIsAdditionAndDeduction && this.modifyPermission;
    }

    protected toolbarShowFunctionsEmployee() {
        return this.currentTreeViewModeIsEmployee && (this.currentContentViewModeIsAttestEmployee || this.currentContentViewModeIsAbsenceDetails) && this.modifyPermission && this.calculationFunctions.length > 0;
    }

    protected toolbarShowOptionHideDaysWithoutSchedule() {
        return this.currentTreeViewModeIsEmployee && this.currentContentViewModeIsAttestEmployee;
    }

    protected toolbarShowOptionHideDaysWithoutScheduleActive() {
        return this.hideDaysWithoutSchedule && this.toolbarShowOptionHideDaysWithoutSchedule();
    }

    protected toolbarShowOptionHideDaysWithoutScheduleNotActive() {
        return !this.hideDaysWithoutSchedule && this.toolbarShowOptionHideDaysWithoutSchedule();
    }

    protected toolbarReload() {
        this.synchToolbarSearchPattern();
        this.loadTree(true, true, false, false);
    }

    protected toolbarSearch() {
        this.synchToolbarSearchPattern();
        this.loadTree(false, false, true, false);
    }

    protected toolbarSelectionClear() {
        this.toolbarSelectionCategoriesSelected.length = 0;
        this.toolbarSelectionAccountsSelected.length = 0;
        this.toolbarSelectionEmployeesSelected.length = 0;

        if (this.hasToolbarSearchPatternInput()) {
            this.toolbarSearchPatternInput = '';
            this.toolbarSearchPattern = '';
        }

        if (this.hasTreeFilter()) {
            this.treeFilterTextInput = '';
            this.treeFilterText = '';
        }

        if (this.userSettingTimeAttestTreeDisableAutoLoad)
            this.tree = null;
        else
            this.loadTree(true, true, false, true);
    }

    protected prevEmployeeValid(): boolean {
        return this.tree && TimePayrollUtility.hasPrevEmployeeNode(this.tree.groupNodes, this.currentEmployee);
    }

    protected nextEmployeeValid(): boolean {
        return this.tree && TimePayrollUtility.hasNextEmployeeNode(this.tree.groupNodes, this.currentEmployee);
    }

    protected prevEmployeeGoTo() {
        if (!this.currentEmployee)
            return;

        this.currentEmployeeLoading = true;
        var prevEmployeeNode = TimePayrollUtility.getPrevEmployeeNode(this.tree.groupNodes, this.currentEmployee);
        if (!prevEmployeeNode)
            return;

        this.currentEmployee = prevEmployeeNode;
        this.currentEmployeeGoToDebounced();
    }

    protected nextEmployeeGoTo() {
        if (!this.currentEmployee)
            return;

        this.currentEmployeeLoading = true;
        var nextEmployeeNode = TimePayrollUtility.getNextEmployeeNode(this.tree.groupNodes, this.currentEmployee);
        if (!nextEmployeeNode)
            return;

        this.currentEmployee = nextEmployeeNode;
        this.currentEmployeeGoToDebounced();
    }

    protected refreshCurrentEmployeeNode(currentEmployeeNodeIndex: number) {
        var nextOrPrevEmployeeNode = TimePayrollUtility.refreshCurrentEmployeeNode(this.tree.groupNodes, this.currentEmployee, currentEmployeeNodeIndex);
        if (!nextOrPrevEmployeeNode)
            return;

        this.currentEmployee = nextOrPrevEmployeeNode;
        this.currentEmployeeGoToDebounced();
    }

    private treeFilterChanged = _.debounce(() => {
        this.treeFilterText = this.treeFilterTextInput;
        TimePayrollUtility.setFilterVisibility(this.tree, this.treeFilterText);
        this.$scope.$apply();
    }, 500, { leading: false, trailing: true });

    protected doShowGroup(groupNode: TimeEmployeeTreeGroupNodeDTO): boolean {
        return groupNode.doShowGroup(this.treeFilterText);
    }

    private isGroupNodeActive(groupNode: TimeEmployeeTreeGroupNodeDTO) {
        return groupNode && this.currentGroup && groupNode.guid == this.currentGroup.guid;
    }

    protected groupExpanded(groupNode: TimeEmployeeTreeGroupNodeDTO) {
        groupNode.expanded = !groupNode.expanded;
    }

    protected groupNodeClick(groupNode: TimeEmployeeTreeGroupNodeDTO) {
        this.setupGroup(groupNode);
    }

    private viewEmployee(row: any) {
        if (row && this.currentGroup) {
            var employeeNode = TimePayrollUtility.getEmployeeNode(this.currentGroup, row.employeeId);
            if (employeeNode) {
                this.setupEmployee(employeeNode);
            }
        }
    }

    protected employeeNodeClick(employeeNode: TimeEmployeeTreeNodeDTO) {
        this.setupEmployee(employeeNode);
    }

    private setupGroup(groupNode: TimeEmployeeTreeGroupNodeDTO) {
        if (!groupNode)
            return;

        window.scrollTo(0, 0);

        //reset values
        this.currentEmployee = null;
        this.currentGroup = groupNode;
        if (!this.currentGroup.timeEmployeePeriods)
            this.currentGroup.preview = true;
        this.initTreeViewModeGroup();
        this.setCalculationFunctions();
        this.loadGroupContent(false);
        this.loadAttestStates();
    }

    protected setupMyTime() {
        if (!this.isMyTime)
            return;

        var employee: TimeEmployeeTreeNodeDTO = new TimeEmployeeTreeNodeDTO();
        employee.employeeId = this.employeeId;

        this.setupEmployee(employee);
    }

    private setupEmployee(employeeNode: TimeEmployeeTreeNodeDTO, force: boolean = false) {
        if (!employeeNode)
            return;

        if (force || !this.isEmployeeNodeActive(employeeNode)) {
            window.scrollTo(0, 0);
            this.currentEmployee = employeeNode;
            this.currentEmployeeLoading = true;
            this.currentGroup = null;

            this.initTreeViewModeEmployee();
            if (this.currentContentViewMode === TimeAttestContentViewMode.None)
                this.initContentViewModeAttestEmployee(false);
            this.initContentViewModeFunctions();
        }

        this.loadEmployee();
        if (this.currentContentViewModeIsAdditionAndDeduction)
            this.loadAdditionDeductionTransactions();
        else if (this.currentContentViewModeIsAbsenceDetails)
            this.loadAbsenceDetails();
        else
            this.loadEmployeeContent(null, false);
        this.loadTimeAccumulators();
        this.loadEmployeeVacationPeriod();
    }

    protected currentSumsExpandedChanged() {
        this.currentSumsExpanded = !this.currentSumsExpanded;
    }

    protected executeContentViewModeFunction(option) {
        switch (option.id) {
            case TimeAttestContentViewMode.AttestEmployee:
                this.initContentViewModeAttestEmployee(true);
                break;
            case TimeAttestContentViewMode.AdditionAndDeduction:
                this.initContentViewModeAdditionDeduction();
                break;
            case TimeAttestContentViewMode.TimeCalendar:
                this.initContentViewModeTimeCalendar();
                break;
            case TimeAttestContentViewMode.AbsenceDetails:
                this.initContentViewModeAbsenceDetails();
                break;
        }
    }

    protected setCalculationFunctions() {
        if (!this.isPermissionsLoaded())
            return;
        if (!this.modifyPermission)
            return;

        this.calculationFunctions.length = 0;

        //Absence
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.OpenAbsenceDialog, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.OpenAbsenceDialog), hidden: () => { return !this.showAbsenceFunction; } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.UpdateAbsenceDetails, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.UpdateAbsenceDetails), hidden: () => { return !this.showAbsenceDetailsFunction } });
        this.calculationFunctions.push({ hidden: () => { return !this.showAbsenceFunctions } });

        //Restore
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.RestoreToSchedule, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.RestoreToSchedule), hidden: () => { return !this.showRestoreToScheduleFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.RestoreScheduleToTemplate, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.RestoreScheduleToTemplate), hidden: () => { return !this.showRestoreToTemplateScheduleFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.RestoreToScheduleDiscardDeviations, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.RestoreToScheduleDiscardDeviations), hidden: () => { return !this.showRestoreToScheduleDiscardDeviationsFunction } });
        this.calculationFunctions.push({ hidden: () => { return !this.showRestoreFunctions } });

        //Transactions
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps), hidden: () => { return !this.showRegenerateDaysBasedOnTimeStampsFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest), hidden: () => { return !this.showRegenerateTransactionsFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.ReGenerateVacationsTransactionsDiscardAttest, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.ReGenerateVacationsTransactionsDiscardAttest), hidden: () => { return !this.showRegenerateTransactionsFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.RecalculateAccounting, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.RecalculateAccounting), hidden: () => { return !this.showRecalculateAccountingFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions), hidden: () => { return !this.showDeleteTimeBlocksAndTransactionsFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.ReverseTransactions, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.ReverseTransactions), hidden: () => { return !this.showReverseTransactionsFunctions } });
        this.calculationFunctions.push({ hidden: () => { return !this.showTransactionFunctions } });

        //Attest
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.AttestReminder, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.AttestReminder), hidden: () => { return !this.showAttestReminderFunction } });
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.RunAutoAttest, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.RunAutoAttest), hidden: () => { return !this.showRunAutoAttestFunction } });
        this.calculationFunctions.push({ hidden: () => { return !this.showAttestFunction || this.calculationFunctions.length === 0 } });

        //Future option
        this.calculationFunctions.push({ id: SoeTimeAttestFunctionOption.CalculatePeriods, name: this.getCalculationFunctionTerm(SoeTimeAttestFunctionOption.CalculatePeriods), hidden: () => { return !this.ShowCalculatePeriodsFunction } });

    }

    protected executeCalculationFunction(option) {

        switch (option.id) {
            case SoeTimeAttestFunctionOption.RestoreToSchedule:
            case SoeTimeAttestFunctionOption.RestoreToScheduleDiscardDeviations:
                this.initRestoreToSchedule(option.id);
                break;
            case SoeTimeAttestFunctionOption.RestoreScheduleToTemplate:
                this.initRestoreScheduleToTemplate(option.id);
                break;
            case SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps:
                this.initReGenerateDaysBasedOnTimeStamps(option.id);
                break;
            case SoeTimeAttestFunctionOption.ReGenerateVacationsTransactionsDiscardAttest:
                this.initRegenerateTransactions(option.id);
                break;
            case SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest:
                this.initReGenerateTransactionsDiscardAttest(option.id);
                break;
            case SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions:
                this.initDeleteTimeBlocksAndTransactions(option.id);
                break;
            case SoeTimeAttestFunctionOption.OpenAbsenceDialog:
                this.openAbsenceDialog();
                break;
            case SoeTimeAttestFunctionOption.ReverseTransactions:
                this.initReverseTransactions();
                break;
            case SoeTimeAttestFunctionOption.AttestReminder:
                this.initAttestReminder();
                break;
            case SoeTimeAttestFunctionOption.RunAutoAttest:
                this.runAutoAttestForEmployees();
                break;
            case SoeTimeAttestFunctionOption.RecalculateAccounting:
                this.initRecalculateAccounting(option.id);
                break;
            case SoeTimeAttestFunctionOption.UpdateAbsenceDetails:
                this.initUpdateAbsenceDetails(option.id);
                break;
            case SoeTimeAttestFunctionOption.CalculatePeriods:
                this.initCalculatePeriods(option.id);

        }
    }

    protected initTreeViewModeGroup() {
        if (this.currentTreeViewModeIsGroup)
            return;
        this.currentTreeViewMode = TimeTreeViewMode.Group;
        this.setupToolBar();
    }

    protected initTreeViewModeEmployee() {
        if (this.currentTreeViewModeIsEmployee)
            return;
        this.currentTreeViewMode = TimeTreeViewMode.Employee;
        this.setupToolBar();
    }

    protected initContentViewModeAttestEmployee(loadContent: boolean) {
        if (this.currentContentViewModeIsAttestEmployee)
            return;
        this.currentContentViewMode = TimeAttestContentViewMode.AttestEmployee;
        this.setupToolBar();
        if (loadContent)
            this.loadEmployeeContent();
    }

    protected initContentViewModeAdditionDeduction() {
        if (this.currentContentViewModeIsAdditionAndDeduction)
            return;
        this.currentContentViewMode = TimeAttestContentViewMode.AdditionAndDeduction;
        this.setupToolBar();
        this.loadAdditionDeductionTransactions();
    }

    protected initContentViewModeTimeCalendar() {
        if (this.currentContentViewModeIsTimeCalendar)
            return;
        this.currentContentViewMode = TimeAttestContentViewMode.TimeCalendar;
        this.setupToolBar();
    }

    protected initContentViewModeAbsenceDetails() {
        if (this.currentContentViewModeIsAbsenceDetails)
            return;
        this.currentContentViewMode = TimeAttestContentViewMode.AbsenceDetails;
        this.setupToolBar();
        this.loadAbsenceDetails();
    }

    protected initContentViewModeFunctions() {
        if (!this.isPermissionsLoaded())
            return;

        this.contentViewModeFunctions.length = 0;
        this.contentViewModeFunctions.push({ id: TimeAttestContentViewMode.AttestEmployee, name: this.termsArray["time.time.attest.attestemployee"] });
        if (this.showAdditionDeductionPermission)
            this.contentViewModeFunctions.push({ id: TimeAttestContentViewMode.AdditionAndDeduction, name: this.termsArray["time.time.attest.additiondeduction"] });
        if (this.showAbsenceDetailsPermission)
            this.contentViewModeFunctions.push({ id: TimeAttestContentViewMode.AbsenceDetails, name: this.termsArray["time.time.attest.absencedetails"] });
    }

    protected initRestoreToSchedule(option: SoeTimeAttestFunctionOption) {
        if (!this.restoreToSchedulePermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }

    protected initRestoreScheduleToTemplate(option: SoeTimeAttestFunctionOption) {
        if (!this.restoreScheduleToTemplatePermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }

    protected initReGenerateDaysBasedOnTimeStamps(option: SoeTimeAttestFunctionOption) {
        if (!this.reGenerateDaysBasedOnTimeStampsPermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }

    protected initRegenerateTransactions(option: SoeTimeAttestFunctionOption) {
        if (!this.regenerateTransactionsPermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }
    protected initCalculatePeriods(option: SoeTimeAttestFunctionOption) {
        if (!this.regenerateTransactionsPermission)
            return;

        if (this.currentTreeViewModeIsGroup)
            this.openPeriodCalculationDialog(option);
    }
    protected initDeleteTimeBlocksAndTransactions(option: SoeTimeAttestFunctionOption) {
        if (!this.deleteTransactionsPermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }

    protected initReGenerateTransactionsDiscardAttest(option: SoeTimeAttestFunctionOption) {
        if (!this.regenerateTransactionsPermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }

    protected initRecalculateAccounting(option: SoeTimeAttestFunctionOption) {
        if (!this.recalculateAccountingPermission)
            return;

        if (this.currentTreeViewModeIsEmployee)
            this.applyCalculationFunctionValidation(option);
        else if (this.currentTreeViewModeIsGroup)
            this.openTimeCalculationDialog(option);
    }

    protected initUpdateAbsenceDetails(option: SoeTimeAttestFunctionOption) {
        if (!this.currentTreeViewModeIsEmployee || !this.showAbsence)
            return;

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AbsenceDetails/Dialogs/UpdateAbsenceDetailsDialog.html"),
            controller: UpdateAbsenceDetailsDialogController,
            controllerAs: "ctrl",
            size: 'sm',
            resolve: {
                absenceDetails: () => { return this.contentAbsenceDetailsSelected }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.absenceDetails && result.absenceDetails.length > 0) {
                this.saveAbsenceDetailsRatio();
            }
        });
    }

    protected initReverseTransactions() {
        if (!this.reverseTransactionsPermission)
            return;

        this.reverseTransactionsValidation();
    }

    protected initAttestReminder() {
        if (!this.sendAttestReminderPermission)
            return;

        this.openAttestReminderDialog();
    }

    protected initSaveAttestForEmployees(option: any) {
        if (!this.isCurrentGroupValid())
            return;

        var employeeIds = this.getSelectedEmployeeIds();
        var attestStateTo: AttestStateDTO = this.getAttestState(option.id);
        if (!employeeIds || !attestStateTo)
            return;

        if (this.userSettingTimeAttestDisableSaveAttestWarning) {
            this.saveAttestForEmployees(employeeIds, attestStateTo);
        }
        else {
            var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.time.attest.saveattestemployees"].format(employeeIds.length, StringUtility.nullToEmpty(attestStateTo.name)), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            modal.result.then(result => {
                if (result) {
                    if (result.isChecked)
                        this.saveUserSettingDisableAttestWarning();
                    this.saveAttestForEmployees(employeeIds, attestStateTo);
                }
            });
        }
    }

    protected initSaveAttestForEmployee(option: any) {
        if (!this.isCurrentEmployeeValid() || !this.hasSelectedEmployeeContent())
            return;

        var validItems = this.contentEmployeeSelected;
        var attestStateTo: AttestStateDTO = this.getAttestState(option.id);
        if (!validItems || !attestStateTo)
            return;

        this.timeService.saveAttestForEmployeeValidation(validItems, this.currentEmployee.employeeId, attestStateTo.attestStateId, this.isMySelf).then((validationResult) => {
            if (validationResult.success && (validationResult.canSkipDialog && this.userSettingTimeAttestDisableSaveAttestWarning)) {
                this.saveAttestForEmployee(validationResult.validItems, attestStateTo);
            }
            else {
                var showCheckBox = validationResult.success && validationResult.canSkipDialog;
                var modal = this.notificationService.showDialog(validationResult.title, validationResult.message, TimePayrollUtility.getSaveAttestValidationMessageIcon(validationResult), TimePayrollUtility.getSaveAttestValidationMessageButton(validationResult), SOEMessageBoxSize.Medium, false, showCheckBox, this.termsArray["core.donotshowagain"]);
                if (validationResult.success) {
                    modal.result.then(result => {
                        if (result) {
                            if (result.isChecked)
                                this.saveUserSettingDisableAttestWarning();
                            this.saveAttestForEmployee(validationResult.validItems, attestStateTo);
                        }
                        else {
                            this.stopProgress();
                        }
                    });
                }
                else {
                    this.stopProgress();
                }
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected initSaveAttestForAdditionDeductions(option: any) {
        if (!this.isCurrentEmployeeValid() || !this.hasSelectedAdditionsDeductions())
            return;

        var attestStateTo: AttestStateDTO = this.getAttestState(option.id);
        if (!attestStateTo)
            return;

        var transactionItems: IAttestEmployeeAdditionDeductionTransactionDTO[] = [];
        var selectedRows = this.contentAdditionDeductionSelected;
        _.forEach(selectedRows, (row: IAttestEmployeeAdditionDeductionDTO) => {
            _.forEach(row.transactions, (transactionItem: any) => {
                if (transactionItem.attestStateId)
                    transactionItems.push(transactionItem);
            });
        });

        this.timeService.saveAttestForAdditionDeductionsValidation(transactionItems, this.currentEmployee.employeeId, attestStateTo.attestStateId, this.isMySelf).then((validationResult) => {
            if (validationResult.success && this.userSettingTimeAttestDisableSaveAttestWarning) {
                this.saveAttestForAdditionDeductions(validationResult.validItems, attestStateTo);
            }
            else {
                var modal = this.notificationService.showDialog(validationResult.title, validationResult.message, TimePayrollUtility.getSaveAttestValidationMessageIcon(validationResult), TimePayrollUtility.getSaveAttestValidationMessageButton(validationResult), SOEMessageBoxSize.Medium, false, validationResult.success, this.termsArray["core.donotshowagain"]);
                if (validationResult.success) {
                    modal.result.then(result => {
                        if (result) {
                            if (result.isChecked)
                                this.saveUserSettingDisableAttestWarning();
                            this.saveAttestForAdditionDeductions(validationResult.validItems, attestStateTo);
                        }
                        else {
                            this.stopProgress();
                        }
                    });
                }
                else {
                    this.stopProgress();
                }
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    // ACTIONS

    private print() {
        let templateTypes: SoeReportTemplateType[] = [];
        templateTypes.push(SoeReportTemplateType.TimeMonthlyReport);
        templateTypes.push(SoeReportTemplateType.TimePayrollTransactionReport);
        templateTypes.push(SoeReportTemplateType.TimePayrollTransactionSmallReport);
        templateTypes.push(SoeReportTemplateType.TimeAccumulatorReport);
        templateTypes.push(SoeReportTemplateType.TimeAccumulatorDetailedReport);
        templateTypes.push(SoeReportTemplateType.TimeStampEntryReport);

        let employeeIds: number[] = [];
        if (this.currentTreeViewModeIsEmployee)
            employeeIds.push(this.currentEmployee.employeeId);
        else if (this.currentTreeViewModeIsGroup)
            employeeIds = _.map(this.contentGroupSelected, g => g.employeeId);

        this.openPrintDialog(templateTypes, this.companyTimeDefaultMonthlyReportId).then(result => {
            if (result && result.reportId) {
                this.printReport(employeeIds, this.currentStartDate, this.currentStopDate, result.reportId, result.reportType).then((reportPrintout) => {
                });
            }
        });
    }

    private printReport(employeeIds: number[], startDate: Date, stopDate: Date, reportId: number, reportTemplateType: SoeReportTemplateType): ng.IPromise<any> {
        if (reportId && employeeIds) {
            return this.reportDataService.createReportJob(ReportJobDefinitionFactory.createSimpleTimeReportDefinition(reportId, reportTemplateType, employeeIds, startDate, stopDate), true);
        }
    }

    private openPrintDialog(templateTypes: SoeReportTemplateType[], defaultReportId: number): ng.IPromise<any> {
        var deferral = this.$q.defer();

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
            controller: SelectReportController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => { return null },
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

    protected setHideDaysWithoutSchedule() {
        this.hideDaysWithoutSchedule = !this.hideDaysWithoutSchedule;
    }

    private openAbsenceDialog() {
        if (this.currentTreeViewModeIsEmployee && !this.isCurrentEmployeeValid())
            return;
        if (this.currentTreeViewModeIsGroup && this.getSelectedEmployeeIds().length !== 1)
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Absencerequests/Views/edit.html"),
            controller: AbsenceRequestsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',

            scope: this.$scope
        });

        var viewMode: AbsenceRequestViewMode;
        var employeeId: number;
        var selectedDates: Date[];
        if (this.currentTreeViewModeIsEmployee) {
            viewMode = this.isMyTime ? AbsenceRequestViewMode.Employee : AbsenceRequestViewMode.Attest;
            employeeId = this.currentEmployee.employeeId;
            selectedDates = _.map(this.contentEmployeeSelected, b => b.date);
        }
        else {
            viewMode = AbsenceRequestViewMode.Attest;
            employeeId = this.getSelectedEmployeeIds()[0];
            selectedDates = CalendarUtility.getDates(this.currentStartDate, this.currentStopDate);
        }

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: 0,
                viewMode: viewMode,
                employeeId: employeeId,
                selectedDates: selectedDates,
                parentMode: AbsenceRequestParentMode.TimeAttest,
                guiMode: AbsenceRequestGuiMode.AbsenceDialog,
                skipXEMailOnShiftChanges: false,
                timeScheduleScenarioHeadId: null,
            });
        });

        modal.result.then(val => {
            this.loadEmployeeContent();
            this.refreshTreeForEmployee(this.currentEmployee.employeeId);
        });
    }

    private openAttestReminderDialog() {
        if (!this.isCurrentGroupValid())
            return;

        let empList: number[] = this.getSelectedEmployeeIds(true);

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Time/TimeAttest/Dialogs/AttestReminder/AttestReminderDialog.html"),
            controller: AttestReminderDialogController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            scope: this.$scope,
            resolve: {
                employeeIds: () => { return empList },
                dateFrom: () => { return this.currentStartDate },
                dateTo: () => { return this.currentStopDate },
            }
        });

        modal.result.then(val => {
        });
    }

    private openTimeCalculationDialog(option: SoeTimeAttestFunctionOption) {
        if (!this.isCurrentGroupValid())
            return;

        // Get employees
        let empList: TimeAttestCalculationFunctionDTO[] = [];
        let selectedRows: AttestEmployeePeriodDTO[] = null;
        let setAsSelected: boolean = false;
        if (this.hasSelectedGroupContent()) {
            selectedRows = this.contentGroupSelected;
            setAsSelected = true;
        }
        else {
            selectedRows = this.currentGroup.timeEmployeePeriods;
        }

        _.forEach(selectedRows, (row: AttestEmployeePeriodDTO) => {
            let dto = new TimeAttestCalculationFunctionDTO();
            dto.employeeId = row.employeeId;
            dto.employeeName = row.employeeName;
            dto.employeeNr = row.employeeNr;
            empList.push(dto);
        });

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Time/TimeAttest/Dialogs/Calculation/Views/timeAttestCalculation.html"),
            controller: TimeAttestCalculationController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                calculationFunction: () => { return option },
                calculationText: () => { return this.getCalculationFunctionTerm(option) },
                employees: () => { return empList },
                setAsSelected: () => { return setAsSelected },
                dateFrom: () => { return this.currentStartDate },
                dateTo: () => { return this.currentStopDate },
                timeScheduleScenarioHeadId: () => { return null },
            }
        });

        modal.result.then((result: any) => {
            if (result && result.reloadEmployeeIds.length > 0)
                this.loadGroupContent(true);
        });
    }
    private openPeriodCalculationDialog(option: SoeTimeAttestFunctionOption) {
        if (!this.isCurrentGroupValid())
            return;

        // Get employees
        let empList: TimeAttestCalculationFunctionDTO[] = [];
        let selectedRows: AttestEmployeePeriodDTO[] = null;
        let setAsSelected: boolean = false;
        if (this.hasSelectedGroupContent()) {
            selectedRows = this.contentGroupSelected;
            setAsSelected = true;
        }
        else {
            selectedRows = this.currentGroup.timeEmployeePeriods;
        }
        _.forEach(selectedRows, (row: AttestEmployeePeriodDTO) => {
            let dto = new TimeAttestCalculationFunctionDTO();
            dto.employeeId = row.employeeId;
            dto.employeeName = row.employeeName;
            dto.employeeNr = row.employeeNr;
            empList.push(dto);
        });

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Time/TimeAttest/Dialogs/PeriodCalculation/periodCalculation.html"),
            controller: PeriodCalculationController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                employees: () => { return empList },
                setAsSelected: () => { return setAsSelected },
                dateFrom: () => { return this.currentStartDate },
                dateTo: () => { return this.currentStopDate },
            }
        });

        modal.result.then((result: any) => {
            if (result && result.reloadEmployeeIds.length > 0)
                this.loadGroupContent(true);
        });
    }

    // SERVER CALLS

    private loadTimePeriods(): ng.IPromise<any> {
        return this.timeService.getTimePeriods(this.companyTimeDefaultTimePeriodHeadId).then((periods: TimePeriodDTO[]) => {
            this.timePeriods = periods;
            this.timePeriods = (_.filter(this.timePeriods, { extraPeriod: false }));
            this.timePeriods = _.orderBy(this.timePeriods, ['paymentDate'], ['desc']);
            if (_.size(this.timePeriods) > 0) {

                // First try with current month and a day in the future
                let result = _.filter(this.timePeriods, function (obj: TimePeriodDTO) {
                    return (new Date(<any>obj.stopDate).getFullYear() === CalendarUtility.getDateNow().getFullYear())
                        && (new Date(<any>obj.stopDate).getMonth() === CalendarUtility.getDateNow().getMonth())
                        && (new Date(<any>obj.stopDate).getDay() > CalendarUtility.getDateNow().getDay());
                });
                // Second try with current month and a passed day
                if (_.size(result) === 0) {
                    result = _.filter(this.timePeriods, function (obj: TimePeriodDTO) {
                        return (new Date(<any>obj.stopDate).getFullYear() === CalendarUtility.getDateNow().getFullYear())
                            && (new Date(<any>obj.stopDate).getMonth() === CalendarUtility.getDateNow().getMonth())
                    });
                    result = result.reverse();
                }
                // If nothing found in current month, take next date in the future
                if (_.size(result) === 0) {
                    result = _.filter(this.timePeriods, function (obj: TimePeriodDTO) {
                        return (new Date(<any>obj.stopDate).getDay() > CalendarUtility.getDateNow().getDay());
                    });
                }
                // If nothing found in the future, take last date
                if (_.size(result) === 0) {
                    result = this.timePeriods;
                    result = result.reverse();
                }
                if (_.size(result) > 0) {
                    this.currentTimePeriodId = result[0].timePeriodId;
                }
            }
            else {
                this.setTimePeriodTypes(true);
                if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Period)
                    this.userSettingTimeLatestTimePeriodType = this.defaultTimePeriodType;
            }

            var today = CalendarUtility.getDateToday();
            var timePeriodToday = TimePayrollUtility.getTimePeriodFromDate(this.timePeriods, today);
            if (timePeriodToday)
                this.permissionToSeeDateTo = timePeriodToday.stopDate;
            else
                this.permissionToSeeDateTo = today.endOfMonth();

            this.setTimePeriodType(this.userSettingTimeLatestTimePeriodType, true);
            this.loadToolbarSelection();
            this.isDirty = false;

            if (this.isMyTime)
                this.setupMyTime();
        });
    }

    protected loadAttestStates() {
        if (this.currentLoadingAttestStates || !this.isCurrentViewModeValid())
            return;

        if (!this.currentAttestStates || this.currentAttestStates.length === 0 || this.isAttestStatesLoadNeeded()) {
            this.currentLoadingAttestStates = true;
            this.currentAttestStates.length = 0;
            this.timeService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, this.currentStartDate, this.currentStopDate, true, this.isMySelf ? this.currentEmployee.employeeGroupId : null).then((result) => {
                this.currentAttestStates = result;
                this.currentLoadingAttestStates = false;
                this.currentAttestStateOptions.length = 0;
                var latestAttestState: AttestStateDTO;
                _.forEach(this.currentAttestStates, (attestState: AttestStateDTO) => {
                    this.currentAttestStateOptions.push({ id: attestState.attestStateId, name: attestState.name });
                    if (this.userSettingTimeLatestAttestStateTo === attestState.attestStateId)
                        latestAttestState = attestState;
                });
                if (this.currentAttestStateOptions.length > 0)
                    this.selectedAttestStateOption = latestAttestState ? { id: latestAttestState.attestStateId, name: latestAttestState.name } : this.currentAttestStateOptions[0];
                if (this.currentEmployee)
                    this.currentLastLoadAttestStatesEmployeeId = this.currentEmployee.employeeId;
                this.currentLastLoadAttestStatesStartDate = this.currentStartDate;
                this.currentLastLoadAttestStatesStopDate = this.currentStopDate;
                this.currentLastLoadAttestMode = this.attestMode;
            });
        }
    }

    private isAttestStatesLoadNeeded() {
        var changedToOrFromMySelf = (this.currentEmployee && this.currentLastLoadAttestStatesEmployeeId !== this.currentEmployee.employeeId) && (this.currentLastLoadAttestStatesEmployeeId === this.employeeId || (this.currentEmployee && this.currentEmployee.employeeId === this.employeeId));
        if (changedToOrFromMySelf) {
            return true;
        }
        var changedAttestMode = this.currentLastLoadAttestMode !== this.attestMode;
        if (changedAttestMode) {
            return true;
        }
        var changedTimePeriod = this.currentLastLoadAttestStatesStartDate !== this.currentStartDate || this.currentLastLoadAttestStatesStopDate !== this.currentStopDate;
        if (changedTimePeriod) {
            return true;
        }
        return false;
    }

    protected loadTimeAccumulators() {
        if (!this.showAccumulatorsPermission || (this.currentTimePeriod && this.currentTimePeriod.extraPeriod) || !this.currentEmployee) {
            this.timeAccumulators = [];
            return;
        }

        this.timeAccumulators = null;
        this.timeService.getTimeAccumulatorsForEmployee(this.currentEmployee.employeeId, this.currentStartDate, this.currentStopDate, false, false, true, true, true, true, false).then((result: TimeAccumulatorItem[]) => {
            this.timeAccumulators = result;
            this.isDirty = false;
        });
    }

    protected loadEmployee() {
        if (!this.isCurrentEmployeeValid())
            return;

        this.sharedTimeService.getEmployee(this.currentEmployee.employeeId, this.currentStartDate, this.currentStopDate, false, true, true, true).then((result) => {
            var employee = result;
            if (employee) {
                this.currentEmployee.employeeNr = employee.employeeNr;
                this.currentEmployee.employeeName = employee.name;
                this.currentEmployee.socialSec = employee.socialSec;
                this.currentEmployee.employeeGroupId = employee.currentEmployeeGroupId;
                this.currentEmployee.employeeGroupName = employee.currentEmployeeGroupName;
                this.currentEmployee.payrollGroupId = employee.currentPayrollGroupId;
                this.currentEmployee.payrollGroupName = employee.currentPayrollGroupName;
                this.currentEmployee.vacationGroupId = employee.currentVacationGroupId;
                this.currentEmployee.vacationGroupName = employee.currentVacationGroupName;
                this.currentEmployee.employmentPercent = employee.currentEmploymentPercent;
                this.currentEmployee.currentEmploymentTypeString = employee.currentEmploymentTypeString;
                this.currentEmployee.currentEmploymentDateFromString = employee.currentEmploymentDateFromString;
                this.currentEmployee.currentEmploymentDateToString = employee.currentEmploymentDateToString;
            }
            this.currentEmployeeLoading = false;
            this.setCalculationFunctions();
            this.loadAttestStates();
            this.isDirty = false;
        });
    }

    protected loadEmployeeVacationPeriod() {
        if (!this.isCurrentEmployeeValid())
            return;

        this.currentEmployeeVacationPeriod = null;
        this.timeService.getEmployeeVacationPeriod(this.currentEmployee.employeeId, this.currentStartDate, this.currentStopDate).then((result) => {
            this.currentEmployeeVacationPeriod = result;
        });
    }

    protected loadAfterSelection() {
        this.loadTreeDefault();
    }

    protected loadTreeDefault(forceLoad: boolean = false) {
        this.loadTree(false, false, false, false);
    }

    protected loadTree(flushCache: boolean, forceLoad: boolean, forceSearch: boolean, discardExpandedGroups: boolean) {
        if (this.currentTimePeriodType == TimePeriodSelectorType.Period && this.currentTimePeriodId === 0)
            return;

        var hasToolbarSelection = this.hasToolbarSelection();
        var hasToolbarSearch = this.hasToolbarSearchPattern();
        var doSearch: boolean = hasToolbarSearch || forceSearch;

        if (!forceLoad && !hasToolbarSelection && !doSearch && (this.userSettingTimeAttestTreeDisableAutoLoad && !this.tree))
            return;

        if (doSearch && forceSearch && !hasToolbarSearch)
            discardExpandedGroups = true;
        var expandedGroupIds: number[] = discardExpandedGroups ? [] : TimePayrollUtility.getExpandedGroupIds(this.tree, this.isMyTime);

        this.loadingTree = true;
        this.progressMessage = this.termsArray["core.loading"];
        this.tree = null;
        this.loadAttestStates();

        var reloadGroupContent: boolean = false;
        if (!this.isMyTime) {
            if (this.currentEmployee != null)
                this.employeeNodeClick(this.currentEmployee);
            else if (this.currentGroup != null)
                reloadGroupContent = true;
        }

        if (!doSearch)
            this.toolbarSearchPattern = Constants.WEBAPI_STRING_EMPTY;

        var settings = new TimeEmployeeTreeSettings();
        settings.filterEmployeeAuthModelIds = this.companyUseAccountHierarchy ? TimePayrollUtility.getCollectionIds(this.toolbarSelectionAccountsSelected) : TimePayrollUtility.getCollectionIds(this.toolbarSelectionCategoriesSelected);
        settings.filterEmployeeIds = TimePayrollUtility.getCollectionIds(this.toolbarSelectionEmployeesSelected);
        settings.filterAttestStateIds = TimePayrollUtility.getCollectionIds(this.toolbarSelectionAttestStatesSelected);
        settings.filterMessageGroupId = this.userSettingTimeAttestMessageGroupId;
        settings.searchPattern = this.toolbarSearchPattern;
        settings.includeEnded = this.toolbarIgnoreEmploymentStopDate;
        settings.includeEmptyGroups = !hasToolbarSelection && !doSearch && this.userSettingTimeAttestTreeDoShowEmptyGroups;
        settings.doNotShowAttested = this.userSettingTimeAttestTreeDoNotShowAttested;
        settings.doNotShowWithoutTransactions = this.userSettingTimeAttestTreeDoNotShowWithoutTransactions;
        settings.doShowOnlyShiftSwaps = this.userSettingTimeAttestTreeShowOnlyShiftSwaps;
        settings.includeAdditionalEmployees = this.userSettingTimeAttestTreeIncludeAdditionalEmployees;
        settings.doNotShowDaysOutsideEmployeeAccount = this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount;
        settings.isProjectAttest = this.attestMode === TimeAttestMode.Project;
        settings.cacheKeyToUse = this.getCacheKey(flushCache);

        this.timeService.getTimeAttestTree(this.userSettingTimeAttestTreeLatestGrouping, this.userSettingTimeAttestTreeLatestSorting, this.currentStartDate, this.currentStopDate, this.currentTimePeriodId, settings).then((result: TimeEmployeeTreeDTO) => {
            this.setTreeContent(result, null, expandedGroupIds, reloadGroupContent, hasToolbarSelection, hasToolbarSearch, false);
        });
    }

    protected refreshTreeForEmployee(employeeId: number) {
        var employeeIds: number[] = [];
        employeeIds.push(employeeId);
        this.refreshTreeForEmployees(employeeIds);
    }

    protected refreshTreeForEmployees(employeeIds: number[], refreshGroupNode: boolean = false) {
        if (this.isMyTime)
            return;

        var expandedGroupIds: number[] = TimePayrollUtility.getExpandedGroupIds(this.tree, this.isMyTime);
        var settings = new TimeEmployeeTreeSettings();
        settings.filterEmployeeAuthModelIds = this.companyUseAccountHierarchy ? TimePayrollUtility.getCollectionIds(this.toolbarSelectionAccountsSelected) : TimePayrollUtility.getCollectionIds(this.toolbarSelectionCategoriesSelected);
        settings.filterEmployeeIds = employeeIds;
        settings.filterAttestStateIds = TimePayrollUtility.getCollectionIds(this.toolbarSelectionAttestStatesSelected);
        settings.filterMessageGroupId = this.userSettingTimeAttestMessageGroupId;
        settings.includeEnded = this.toolbarIgnoreEmploymentStopDate;
        settings.doNotShowAttested = this.userSettingTimeAttestTreeDoNotShowAttested;
        settings.doNotShowWithoutTransactions = this.userSettingTimeAttestTreeDoNotShowWithoutTransactions;
        settings.doShowOnlyShiftSwaps = this.userSettingTimeAttestTreeShowOnlyShiftSwaps;
        settings.includeAdditionalEmployees = this.userSettingTimeAttestTreeIncludeAdditionalEmployees;
        settings.doNotShowDaysOutsideEmployeeAccount = this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount;
        settings.isProjectAttest = this.attestMode === TimeAttestMode.Project;
        settings.cacheKeyToUse = this.getCacheKey();

        this.timeService.refreshTimeAttestTree(this.tree, this.currentStartDate, this.currentStopDate, this.currentTimePeriodId, settings).then((result) => {
            this.setTreeContent(result, employeeIds, expandedGroupIds, false, false, false, true);
            if (this.currentGroup) {
                var currentGroupNode = _.filter(this.tree.groupNodes, { id: this.currentGroup.id })[0];
                if (!currentGroupNode || !currentGroupNode.employeeNodes || currentGroupNode.employeeNodes.length === 0) {
                    this.currentGroup.timeEmployeePeriods = null;
                    refreshGroupNode = true;
                }
                if (refreshGroupNode)
                    this.refreshGroupNode();
            }
        });
    }

    protected setTreeContent(tree: TimeEmployeeTreeDTO, employeeIds: number[], expandedGroupIds: number[], hasSelection: boolean, hasSearch: boolean, reloadGroupContent: boolean, flushCache: boolean) {
        this.tree = tree;
        this.loadingTree = false;
        this.loadedTreeWarnings = false;
        this.isDirty = false;
        if (this.tree) {
            this.$timeout(() => {
                this.loadTreeWarnings(employeeIds, expandedGroupIds, hasSelection, hasSearch, reloadGroupContent, flushCache);
            });
        }
    }

    protected loadTreeWarnings(employeeIds: number[], expandedGroupIds: number[], hasSelection: boolean, hasSearch: boolean, reloadGroupContent: boolean, flushCache?: boolean) {
        if (!this.tree)
            return;

        this.loadingTreeWarnings = true;
        TimePayrollUtility.trySetGroupsExpanded(this.tree, expandedGroupIds, hasSelection, hasSearch);

        var currentEmployeeNodeIndex = TimePayrollUtility.getCurrentEmployeeNodeIndex(this.tree.groupNodes, this.currentEmployee);

        this.timeService.getTimeAttestTreeWarnings(this.tree, this.currentStartDate, this.currentStopDate, this.currentTimePeriodId, employeeIds, this.userSettingTimeAttestTreeDoShowOnlyWithWarnings, flushCache).then((result) => {
            this.tree = result;
            this.loadingTreeWarnings = false;
            this.loadedTreeWarnings = true;

            if (this.currentGroup != null && reloadGroupContent)
                this.reloadGroupContent();
            if (this.hasTreeFilter())
                this.treeFilterChanged();

            var currentEmployeeIds = this.tree.getVisibleEmployeeIds();
            if (this.currentEmployee && _.filter(currentEmployeeIds, id => id == this.currentEmployee.employeeId).length === 0)
                this.refreshCurrentEmployeeNode(currentEmployeeNodeIndex);
            if (currentEmployeeIds.length === 0)
                this.clearEmployeeContent();

            this.isDirty = false;
        });
    }

    protected reloadGroupContent() {
        if (this.currentGroup && this.tree && this.tree.groupNodes)
            this.currentGroup = (_.filter(this.tree.groupNodes, { id: this.currentGroup.id }))[0];
        if (this.currentGroup)
            this.currentGroup.preview = true;
        this.loadGroupContent(true);
    }

    protected loadGroupContent(flushCache: boolean, force: boolean = false) {
        if (!this.isCurrentGroupValid())
            return;

        if (force || !this.currentGroup.preview) {
            this.currentGroup.timeEmployeePeriods = null;
            this.startLoad();

            var cacheKeyToUse = this.getCacheKey(flushCache);

            this.timeService.getTimeAttestEmployeePeriods(this.currentStartDate, this.currentStopDate, this.userSettingTimeAttestTreeLatestGrouping, this.currentGroup.id, this.currentGroup.getVisibleEmployeeIds(true), this.currentGroup.isAdditional, this.userSettingTimeAttestTreeIncludeAdditionalEmployees, this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount, cacheKeyToUse, false, this.currentTimePeriodId).then((result: AttestEmployeePeriodDTO[]) => {
                this.stopProgress();
                this.isDirty = false;
                if (this.currentGroup) //may have changed to employee before group has loaded
                    this.currentGroup.timeEmployeePeriods = result;

                this.currentGroup.preview = false;
                this.unhandledEmployees = this.formatTimeUnhandledShiftChanges(result.map(a => a.unhandledEmployee));
            });
        }
        else {
            this.previewGroupContent();
        }
    }

    protected previewGroupContent() {
        if (!this.tree || !this.currentGroup)
            return;

        this.timeService.getTimeAttestEmployeePeriodsPreview(this.tree, this.currentGroup).then((result: AttestEmployeePeriodDTO[]) => {
            this.stopProgress();
            this.isDirty = false;
            this.currentGroup.timeEmployeePeriods = result;
        });
    }

    protected refreshGroupNode() {
        if (!this.currentGroup)
            return;

        var preview = this.currentGroup.preview;

        this.timeService.refreshTimeAttestTreeGroupNode(this.tree, this.currentGroup).then((result: TimeEmployeeTreeGroupNodeDTO) => {
            this.$timeout(() => {
                this.currentGroup = result;
                if (this.currentGroup)
                    this.currentGroup.preview = preview;
            });

        });
    }

    protected loadEmployeeContent(date?: Date, loadTimeAccumulators: boolean = true, isManualRefresh: boolean = true, fromModal?: boolean) {
        if (!this.isCurrentEmployeeValid())
            return;

        var showChartsAfterLoad = this.chartsVisible;
        this.chartsVisible = false;
        this.startLoad();

        // If specified date has absence, more days might be affected, reload the whole period. Parameter forceDate should be used in cases when days not are recalculated, for example attesting...
        var day: AttestEmployeeDayDTO;
        if (date) {
            day = _.find(this.contentEmployee, e => e.date.isSameDayAs(date));
            if (day && (day.hasAbsenceTime || day.hasOvertime || day.hasPeriodOvertime || day.hasPeriodTimeWorkReduction))
                date = null;
        }
        this.loadedContentEmployee = false;
        this.contentEmployeeSelected = null;
        this.unhandledEmployees = [];

        var includeProjectTimeBlocks: boolean = this.isCurrentEmployeeERP();
        var includeShifts: boolean = true;
        var includeTimeStamps: boolean = !includeProjectTimeBlocks;
        var includeTimeBlocks: boolean = true;
        var includeTimeCodeTransactions: boolean = true;
        var includeTimeInvoiceTransactions: boolean = true;
        var doNotShowDaysOutsideEmployeeAccount = this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount && !this.isMyTime;
        var gridName = this.getGridName().replace(/\./g, '_');
        var cacheKeyToUse = this.getCacheKey();
        var startDate = date ? date : this.currentStartDate;
        var stopDate = date ? date : this.currentStopDate;
        var hasDayFilter = !isManualRefresh && date !== null;

        this.timeService.getTimeAttestEmployeeDays(gridName, this.currentEmployee.employeeId, startDate, stopDate, hasDayFilter, includeProjectTimeBlocks, includeShifts, includeTimeStamps, includeTimeBlocks, includeTimeCodeTransactions, includeTimeInvoiceTransactions, doNotShowDaysOutsideEmployeeAccount, this.currentFilterAccountIds, cacheKeyToUse).then((result: AttestEmployeeDayDTO[]) => {
            this.loadedContentEmployee = true;

            if (this.isMyTime) {
                result = (_.filter(result, { isPrel: false }));
                result.map(r => r.isMyTime = true);
            }

            //solution to problem of random presence fields not being cleared when absence is applied/restored and timestamps being cleared
            if (day)
                day.clearPresence();

            if (day && result.length == 1) {
                angular.extend(day, result[0]);
                this.setAdditionalOnDay(day);
                this.$scope.$broadcast(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, { employeeId: this.currentEmployee.employeeId, date: day.date });
                this.$scope.$broadcast("EmployeeContentGridChanged", null);
                if (fromModal)
                    this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, { employeeId: this.currentEmployee.employeeId, date: day.date });
            }
            else {
                var doRefreshEmployeeDays = this.doRefreshEmployeeDays(result, date);
                if (doRefreshEmployeeDays) {
                    _.forEach(this.contentEmployee, (currentDay: AttestEmployeeDayDTO) => {
                        var dayResult = _.find(result, r => r.date.isSameDayAs(currentDay.date));
                        if (dayResult) {
                            this.clearPresenceOnDay(currentDay);
                            angular.extend(currentDay, dayResult);
                            this.$scope.$broadcast(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, { employeeId: dayResult.employeeId, date: dayResult.date });
                            if (fromModal)
                                this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_CONTENT_DAY_CHANGED, { employeeId: dayResult.employeeId, date: dayResult.date });
                        }
                    });
                    this.$scope.$broadcast("EmployeeContentGridChanged", null);
                }
                else {
                    this.contentEmployee = result;
                }
                _.forEach(this.contentEmployee, (currentDay: AttestEmployeeDayDTO) => {
                    this.setAdditionalOnDay(currentDay);
                });
            } 

            if (result && result.length > 0 && result[0].unhandledEmployee)
                this.unhandledEmployees.push(this.formatTimeUnhandledShiftChange(result[0].unhandledEmployee));
            if (showChartsAfterLoad)
                this.chartsVisible = true;

            this.stopProgress();

            if (loadTimeAccumulators)
                this.loadTimeAccumulators();

            this.isDirty = false;
        });
    }

    protected clearEmployeeContent() {
        this.currentEmployee = null;
        this.contentEmployee = null;
    } s

    private formatTimeUnhandledShiftChanges(l: TimeUnhandledShiftChangesEmployeeDTO[]): TimeUnhandledShiftChangesEmployeeDTO[] {
        let employeeObjs: TimeUnhandledShiftChangesEmployeeDTO[] = [];
        _.forEach(l, (e: TimeUnhandledShiftChangesEmployeeDTO) => {
            employeeObjs.push(this.formatTimeUnhandledShiftChange(e));
        });
        return employeeObjs;
    }

    private formatTimeUnhandledShiftChange(e: TimeUnhandledShiftChangesEmployeeDTO): TimeUnhandledShiftChangesEmployeeDTO {
        let employeeObj = new TimeUnhandledShiftChangesEmployeeDTO();
        angular.extend(employeeObj, e);

        if (employeeObj.weeks) {
            employeeObj.weeks = employeeObj.weeks.map(w => {
                let weekObj = new TimeUnhandledShiftChangesWeekDTO();
                angular.extend(weekObj, w);
                weekObj.fixDates();

                if (weekObj.extraShiftDays) {
                    weekObj.extraShiftDays = weekObj.extraShiftDays.map(d => {
                        let extraShiftsObj = new TimeBlockDateDTO();
                        angular.extend(extraShiftsObj, d);
                        extraShiftsObj.fixDates();
                        return extraShiftsObj;
                    });
                }

                if (weekObj.shiftDays) {
                    weekObj.shiftDays = weekObj.shiftDays.map(d => {
                        let sickDaysObj = new TimeBlockDateDTO();
                        angular.extend(sickDaysObj, d);
                        sickDaysObj.fixDates();
                        return sickDaysObj;
                    });
                }
                return weekObj;
            });
        }
        return employeeObj;
    }

    private clearPresenceOnDay(day: AttestEmployeeDayDTO) {
        if (!day)
            return;

        day.clearPresence();
    }

    private setAdditionalOnDay(day: AttestEmployeeDayDTO) {
        if (!day)
            return;

        day.setAdditionalStatus(this.termsArray["time.time.attest.partlyadditional"], this.termsArray["time.time.attest.completelyadditional"]);
    }

    protected loadAdditionDeductionTransactions(loadTimeAccumulators: boolean = false) {
        if (!this.isCurrentEmployeeValid())
            return;

        this.startLoad();

        this.timeService.getAdditionDeductions(this.currentEmployee.employeeId, this.currentStartDate, this.currentStopDate, 0, this.isMySelf).then((result: AttestEmployeeAdditionDeductionDTO[]) => {
            this.contentAdditionDeduction = result;
            if (loadTimeAccumulators)
                this.loadTimeAccumulators();

            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected loadAbsenceDetails() {
        if (!this.isCurrentEmployeeValid())
            return;

        this.startLoad();

        this.timeService.getAbsenceDetails(this.currentEmployee.employeeId, this.currentStartDate, this.currentStopDate).then((result: TimeAbsenceDetailDTO[]) => {
            this.contentAbsenceDetails = result;
            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected applyCalculationFunctionValidation(option: SoeTimeAttestFunctionOption) {
        if (!this.isCurrentEmployeeValid())
            return;

        var selectedRows = this.contentEmployeeSelected;
        this.timeService.applyCalculationFunctionValidation(this.currentEmployee.employeeId, selectedRows, option).then((validationResult: TimeAttestCalculationFunctionValidationDTO) => {
            let image: SOEMessageBoxImage;
            let buttons: SOEMessageBoxButtons;
            if (validationResult.success && validationResult.canOverride) {
                buttons = SOEMessageBoxButtons.OKCancel;
                image = SOEMessageBoxImage.Information;
            } else if (!validationResult.success && !validationResult.canOverride) {
                buttons = SOEMessageBoxButtons.OK;
                image = SOEMessageBoxImage.Warning;
            } else {
                buttons = SOEMessageBoxButtons.OKCancel;
                image = SOEMessageBoxImage.Warning;
            }

            if (validationResult.applySilent) {
                this.applyAttestCalculationFunction(validationResult.validItems, option)
            } else {
                var modal = this.notificationService.showDialog(validationResult.title, validationResult.message, image, buttons);
                if (validationResult.success) {
                    modal.result.then(val => {
                        this.startSave();
                        window.scrollTo(0, 0);

                        if (val) {
                            this.applyAttestCalculationFunction(validationResult.validItems, option)
                        }
                    });
                } else {
                    this.stopProgress();
                }
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected applyAttestCalculationFunction(items: AttestEmployeeDaySmallDTO[], option: SoeTimeAttestFunctionOption) {

        this.startSave();

        this.timeService.applyAttestCalculationFunctionEmployee(items, option).then((result) => {
            if (result.success) {
                this.completedSave(null, false, result.infoMessage);
                this.loadEmployeeContent();
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
            }
            else {
                this.failedSave(result.errorMessage);
            }

        }, error => {
            this.failedSave(error.message);
        });
    }

    protected recalculateUnhandledShiftChanges() {
        this.recalculateUnhandledEmployees(true, false);
    }

    protected recalculateUnhandledExtraShiftChanges() {
        this.recalculateUnhandledEmployees(false, true);
    }

    protected recalculateUnhandledEmployees(doRecalculateShifts: boolean, doRecalculateExtraShifts: boolean) {
        if (!this.hasUnhandledEmployees())
            return;

        this.startWork();
        this.timeService.recalculateUnhandledShiftChangesEmployees(this.unhandledEmployees, doRecalculateShifts, doRecalculateExtraShifts).then(result => {
            this.completedWork(result, true);
            if (result.success) {
                if (this.currentTreeViewModeIsGroup)
                    this.loadGroupContent(true, true);
                else if (this.currentTreeViewModeIsEmployee)
                    this.loadEmployeeContent();
            }
        })
    }

    protected hasUnhandledEmployees() {
        return this.unhandledEmployees && this.unhandledEmployees.length > 0;
    }

    protected reverseTransactionsValidation() {
        if (!this.isCurrentEmployeeValid())
            return;

        var selectedDates = _.map(this.contentEmployeeSelected, t => t.date);
        this.timeService.reverseTransactionsValidation(this.currentEmployee.employeeId, selectedDates).then((validationResult: IReverseTransactionsValidationDTO) => {
            var image: SOEMessageBoxImage = SOEMessageBoxImage.None;
            var buttons: SOEMessageBoxButtons = SOEMessageBoxButtons.None;

            if (validationResult.success && validationResult.canContinue) {
                buttons = SOEMessageBoxButtons.OKCancel;
                image = SOEMessageBoxImage.Information;
            }
            else {
                buttons = SOEMessageBoxButtons.OK;
                image = SOEMessageBoxImage.Warning;
            }

            if (validationResult.applySilent) {
                this.openReverseTransactionsDialog(validationResult);
            }
            else {
                var modal = this.notificationService.showDialog(validationResult.title, validationResult.message, image, buttons);
                if (validationResult.success && validationResult.canContinue) {
                    modal.result.then(val => {
                        window.scrollTo(0, 0);

                        if (val)
                            this.openReverseTransactionsDialog(validationResult);
                    });
                }
                else {
                    this.stopProgress();
                }
            }

        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveAbsenceDetailsRatio() {
        this.startSave();
        window.scrollTo(0, 0);

        this.timeService.saveTimeAbsenceDetailRatios(this.currentEmployee.employeeId, this.contentAbsenceDetailsSelected).then((result) => {
            if (result.success) {
                this.completedSave(null, false, result.infoMessage);
                this.loadAbsenceDetails();
            }
            else {
                this.failedSave(result.errorMessage);
            }

        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveAttest(option: any) {
        if (this.currentTreeViewModeIsGroup)
            this.initSaveAttestForEmployees(option);
        else if (this.currentTreeViewModeIsEmployee) {
            if (this.currentContentViewModeIsAttestEmployee)
                this.initSaveAttestForEmployee(option);
            else if (this.currentContentViewModeIsAdditionAndDeduction)
                this.initSaveAttestForAdditionDeductions(option);
        }
    }

    protected saveAttestForEmployees(employeeIds: number[], attestStateTo: AttestStateDTO) {
        this.startSave();
        window.scrollTo(0, 0);

        this.timeService.saveAttestForEmployees(this.employeeId, employeeIds, attestStateTo.attestStateId, this.currentStartDate, this.currentStopDate).then((result) => {
            if (result.success) {
                this.completedSave(null, true, null, false);
                this.showSaveAttestEmployeesResultMessage(result.value, attestStateTo);
                this.refreshTreeForEmployees(employeeIds, true);
                this.clearCacheKey();
                this.saveUserSettingTimeLatestAttestStateTo(attestStateTo.attestStateId);
            }
            else {
                this.failedSave(result.errorMessage);
            }

        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveAttestForEmployee(validItems: any, attestStateTo: AttestStateDTO) {
        this.startSave();
        window.scrollTo(0, 0);

        this.timeService.saveAttestForEmployee(validItems, this.currentEmployee.employeeId, attestStateTo.attestStateId, this.isMySelf).then((result) => {
            if (result.success) {
                this.showSaveAttestResultMessage(result, attestStateTo);
                if (validItems && validItems.length === 1)
                    this.loadEmployeeContent(new Date(validItems[0].date));
                else
                    this.loadEmployeeContent();
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
                this.saveUserSettingTimeLatestAttestStateTo(attestStateTo.attestStateId);
            }
            else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveAttestForAdditionDeductions(validItems: any, attestStateTo: AttestStateDTO) {
        if (!validItems || !attestStateTo)
            return;

        this.startSave();
        window.scrollTo(0, 0);

        this.timeService.saveAttestForTransactions(validItems, attestStateTo.attestStateId, this.isMySelf).then((result) => {
            if (result.success) {
                this.showSaveAttestResultMessage(result, attestStateTo);
                this.loadAdditionDeductionTransactions();
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
                this.saveUserSettingTimeLatestAttestStateTo(attestStateTo.attestStateId);
            }
            else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected runAutoAttestForTree() {
        if (!this.tree)
            return;

        this.loadingTree = true;
        var employeeIds = this.tree.getVisibleEmployeeIds();
        this.progressMessage = this.termsArray["time.time.attest.tree.autoattest.runs"] + " (" + employeeIds.length + ")";


        this.timeService.runAutoAttest(employeeIds, this.currentStartDate, this.currentStopDate).then((result) => {
            if (result.success) {
                if (result.integerValue > 0)
                    this.loadTreeDefault();
                else
                    this.loadingTree = false;
            }
            else {
                this.notificationService.showDialog(this.termsArray["time.time.attest.tree.runautoattestfailed"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                this.loadingTree = false;
            }
            this.isDirty = false;
        }, error => {
            this.loadingTree = false;
        });
    }

    protected runAutoAttestForEmployees() {

        window.scrollTo(0, 0);
        var employeeIds = this.getSelectedEmployeeIds(true);
        this.startModalProgress(this.termsArray["time.time.attest.tree.autoattest.runs"] + " (" + employeeIds.length + ")");

        this.timeService.runAutoAttest(employeeIds, this.currentStartDate, this.currentStopDate).then((result) => {
            if (result.success) {
                if (result.integerValue > 0) {
                    this.completedSave(null, false, result.infoMessage);
                    this.loadTreeDefault();
                }
                else
                    this.completedSave(null, true, result.infoMessage);
            }
            else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveUserSettingDisableAutoLoad() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeDisableAutoLoad, this.userSettingTimeAttestTreeDisableAutoLoad).then((result) => {
                if (!this.userSettingTimeAttestTreeDisableAutoLoad && !this.tree)
                    this.loadTreeDefault(true);
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeLatestTimePeriodType() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.TimeLatestTimePeriodType, this.userSettingTimeLatestTimePeriodType).then((result) => {
            this.isDirty = false;
        });
    }

    protected saveUserSettingTimeLatestAttestStateTo(attestStateToId: number) {
        if (attestStateToId == this.userSettingTimeLatestAttestStateTo)
            return;

        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.TimeLatestAttestStateTo, attestStateToId);
        this.userSettingTimeLatestAttestStateTo = attestStateToId;
    }

    protected saveUserSettingTimeAttestTreeLatestGrouping() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.TimeAttestTreeLatestGrouping, this.userSettingTimeAttestTreeLatestGrouping).then((result) => {
            this.isDirty = false;
        });
    }

    protected saveUserSettingTimeAttestTreeLatestSorting() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.TimeAttestTreeLatestSorting, this.userSettingTimeAttestTreeLatestSorting).then((result) => {
            this.isDirty = false;
        });
    }

    protected saveUserSettingTimeAttestTreeDoNotShowAttested() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeDoNotShowAttested, this.userSettingTimeAttestTreeDoNotShowAttested).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeAttestTreeDoShowEmptyGroups() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeDoShowEmptyGroups, this.userSettingTimeAttestTreeDoShowEmptyGroups).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeAttestTreeDoShowOnlyWithWarnings() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeDoShowOnlyWithWarnings, this.userSettingTimeAttestTreeDoShowOnlyWithWarnings).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeAttestTreeShowOnlyShiftSwaps() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeShowOnlyShiftSwaps, this.userSettingTimeAttestTreeShowOnlyShiftSwaps).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeAttestTreeDoNotShowWithoutTransactions() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeDoNotShowWithoutTransactions, this.userSettingTimeAttestTreeDoNotShowWithoutTransactions).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeAttestTreeIncludeAdditionalEmployees() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeIncludeAdditionalEmployees, this.userSettingTimeAttestTreeIncludeAdditionalEmployees).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeAttestTreeDoNotShowDaysOutsideEmployeeAccount, this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount).then((result) => {
                this.isDirty = false;
            });
        });
    }

    protected saveUserSettingDisableAttestWarning() {
        this.userSettingTimeAttestDisableSaveAttestWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeDisableApplySaveAttestWarning, this.userSettingTimeAttestDisableSaveAttestWarning);
    }

    protected saveUserSettingTimeAttestMessageGroupId() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.TimeAttestTreeMessageGroupId, this.userSettingTimeAttestMessageGroupId);
    }

    // HELP-METHODS

    protected hasToolbarFilter(): boolean {
        return this.userSettingTimeAttestTreeDisableAutoLoad === true ||
            this.toolbarIgnoreEmploymentStopDate === true ||
            this.userSettingTimeAttestTreeDoNotShowAttested === true ||
            this.userSettingTimeAttestTreeDoShowEmptyGroups === true ||
            this.userSettingTimeAttestTreeDoShowOnlyWithWarnings === true ||
            this.userSettingTimeAttestTreeDoNotShowWithoutTransactions === true ||
            this.userSettingTimeAttestTreeIncludeAdditionalEmployees === true ||
            this.userSettingTimeAttestTreeDoNotShowDaysOutsideEmployeeAccount === true ||
            this.userSettingTimeAttestTreeShowOnlyShiftSwaps === true ||
            this.userSettingTimeAttestMessageGroupId > 0;
    }

    protected hasToolbarSearchPatternInput(): boolean {
        if (!this.toolbarSearchPatternInput || this.toolbarSearchPatternInput.length === 0)
            return false;
        return true;
    }

    protected hasToolbarSearchPattern(): boolean {
        if (!this.toolbarSearchPattern || this.toolbarSearchPattern.length === 0 || this.toolbarSearchPattern === Constants.WEBAPI_STRING_EMPTY)
            return false;
        return true;
    }

    protected synchToolbarSearchPattern() {
        this.toolbarSearchPattern = this.toolbarSearchPatternInput;
    }

    protected hasToolbarSelection(): boolean {
        if (this.hasSelectionEmployees() || this.hasSelectedCategories() || this.hasSelectedAccounts())
            return true;
        return false;
    }

    protected hasSelectableEmployees() {
        if (this.toolbarSelectionEmployees && this.toolbarSelectionEmployees.length > 0)
            return true;
        return false;
    }

    protected hasSelectableAttestStates() {
        if (this.toolbarSelectionAttestStates && this.toolbarSelectionAttestStates.length > 0)
            return true;
        return false;
    }

    protected hasSelectionEmployees() {
        if (this.toolbarSelectionEmployeesSelected && this.toolbarSelectionEmployeesSelected.length > 0)
            return true;
        return false;
    }

    protected hasSelectableCategories() {
        if (this.toolbarSelectionCategories && this.toolbarSelectionCategories.length > 0)
            return true;
        return false;
    }

    protected hasSelectedCategories() {
        if (this.toolbarSelectionCategoriesSelected && this.toolbarSelectionCategoriesSelected.length > 0)
            return true;
        return false;
    }

    protected hasSelectableAccounts() {
        if (this.toolbarSelectionAccounts && this.toolbarSelectionAccounts.length > 0)
            return true;
        return false;
    }

    protected hasSelectedAccounts() {
        if (this.toolbarSelectionAccountsSelected && this.toolbarSelectionAccountsSelected.length > 0)
            return true;
        return false;
    }

    protected hasTreeFilter(): boolean {
        if (this.treeFilterText && this.treeFilterText.length > 0)
            return true;
        return false;
    }

    protected hasContentIgnoreSelectedForGroup(isDelete: boolean = false): boolean {
        return this.hasContent(isDelete, this.currentTreeViewModeIsGroup);
    }

    protected hasContent(isDelete: boolean = false, ignoreSelected: boolean = false): boolean {
        if (isDelete && this.isCurrentEmployeeERP())
            return false;

        if (this.currentTreeViewModeIsGroup) {
            if (this.isCurrentGroupValid() && (this.hasSelectedGroupContent() || ignoreSelected)) {
                return true;
            }
        }
        else if (this.currentTreeViewModeIsEmployee) {
            if (this.isCurrentEmployeeValid()) {
                if ((this.currentContentViewModeIsAttestEmployee && this.hasSelectedEmployeeContent()) ||
                    (this.currentContentViewModeIsAdditionAndDeduction && this.hasSelectedAdditionsDeductions()) ||
                    (this.currentContentViewModeIsAbsenceDetails && this.hasSelectedAbsenceDetails()))
                    return true;
            }
        }
        return false;
    }

    protected hasSelectedGroupContent() {
        return this.contentGroupSelected && this.contentGroupSelected.length > 0;
    }

    protected hasSelectedEmployeeContent() {
        return this.contentEmployeeSelected && this.contentEmployeeSelected.length > 0;
    }

    protected hasSelectedAdditionsDeductions() {
        return this.contentAdditionDeductionSelected && this.contentAdditionDeductionSelected.length > 0;
    }

    protected hasSelectedAbsenceDetails() {
        return this.contentAbsenceDetailsSelected && this.contentAbsenceDetailsSelected.length > 0;
    }

    protected hasSelectedTreeItem() {
        if (this.currentTreeViewModeIsGroup) {
            if (this.currentGroup) {
                return true;
            }
        }
        else if (this.currentTreeViewModeIsEmployee) {
            if (this.currentEmployee) {
                return true;
            }
        }
        return false;
    }

    protected hasLoadedTimePeriodTypes(): boolean {
        return this.currentTimePeriodType > 0;
    }

    protected hasPrevPeriod(): boolean {
        if (this.currentTimePeriodType !== TimePeriodSelectorType.Period)
            return true;
        var prevTimePeriod = this.currentTimePeriodId > 0 ? TimePayrollUtility.getPreviousTimePeriod(this.timePeriods, this.currentTimePeriodId) : null;
        return prevTimePeriod ? true : false;
    }

    protected hasNextPeriod(): boolean {
        if (this.currentTimePeriodType !== TimePeriodSelectorType.Period)
            return true;
        var nextTimePeriod = this.currentTimePeriodId > 0 ? TimePayrollUtility.getNextTimePeriod(this.timePeriods, this.currentTimePeriodId) : null;
        return nextTimePeriod ? true : false;
    }

    protected hasPermissionToNextPeriod(): boolean {
        if (this.currentTimePeriodType == TimePeriodSelectorType.Day) {
            return this.hasPermissionToSeeDate(this.currentStopDate.addDays(1).endOfDay());
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Week) {
            return this.hasPermissionToSeeDate(this.currentStopDate.addWeeks(1).endOfWeek());
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Month) {
            return this.hasPermissionToSeeDate(this.currentStopDate.addMonths(1).endOfMonth());
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Period) {
            var nextTimePeriod = TimePayrollUtility.getNextTimePeriod(this.timePeriods, this.currentTimePeriodId);
            return nextTimePeriod && this.hasPermissionToSeeDate(nextTimePeriod.stopDate);
        }
        else {
            return false;
        }
    }

    protected hasPermissionToSeeDate(date: Date): boolean {
        //TODO: Check dontSeeFuturePlacements for my time
        if (this.currentTimePeriodType == TimePeriodSelectorType.Day || this.currentTimePeriodType == TimePeriodSelectorType.Week || this.currentTimePeriodType == TimePeriodSelectorType.Month) {
            return this.dontSeeFuturePlacementsPermission ? date <= this.permissionToSeeDateTo : true;
        }
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Period) {
            var timePeriod: TimePeriodDTO = null;
            if (this.currentTimePeriodId > 0)
                timePeriod = TimePayrollUtility.getNextTimePeriod(this.timePeriods, this.currentTimePeriodId);
            else
                timePeriod = TimePayrollUtility.getTimePeriodFromDate(this.timePeriods, date)

            if (timePeriod && (!this.dontSeeFuturePlacementsPermission || timePeriod.stopDate <= this.permissionToSeeDateTo))
                return true;
            return false;
        }
        else
            return false;
    }

    protected isCurrentEmployeeERP(): boolean {
        return (this.currentEmployee && this.currentEmployee.timeReportType === TermGroup_TimeReportType.ERP)
    }

    protected isEmployeeNodeActive(employeeNode: TimeEmployeeTreeNodeDTO): boolean {
        return employeeNode && this.currentEmployee && employeeNode.employeeId == this.currentEmployee.employeeId && employeeNode.guid == this.currentEmployee.guid;
    }

    protected isCurrentViewModeValid() {
        if (this.currentTreeViewModeIsGroup) {
            if (this.isCurrentGroupValid) {
                return true;
            }
        }
        else if (this.currentTreeViewModeIsEmployee) {
            if (this.isCurrentEmployeeValid) {
                return true;
            }
        }
        return false;
    }

    protected isCurrentGroupValid() {
        if (!this.currentGroup)
            return false;
        else
            return true;
    }

    protected isCurrentEmployeeValid() {
        if (!this.currentEmployee || this.currentEmployee.employeeId === 0)
            return false;
        else
            return true;
    }

    protected isCurrentTimePeriodValid() {
        if (!this.currentTimePeriod || this.currentTimePeriod.timePeriodId === 0)
            return false;
        else
            return true;
    }

    protected isCurrentEmployeeAndPeriodValid() {
        if (!this.isCurrentEmployeeValid() || !this.isCurrentTimePeriodValid())
            return false;
        else
            return true;
    }

    protected isContentGroupValid(): boolean {
        return this.currentTreeViewModeIsGroup && this.isCurrentGroupValid() && this.currentGroup.timeEmployeePeriods !== null;
    }

    protected isContentEmployeeValid(): boolean {
        return this.currentTreeViewModeIsEmployee && this.isCurrentEmployeeValid();
    }

    protected isPermissionsLoaded() {
        return (this.modifyPermissionsLoaded && this.readPermissionsLoaded);
    }

    protected isAttestDisabled(): boolean {
        return !this.hasContent();
    }

    protected isCalculationFunctionsEmployeeDisabled(): boolean {
        return this.currentTreeViewModeIsGroup;
    }

    protected isCalculationFunctionsGroupDisabled(): boolean {
        return this.currentTreeViewModeIsEmployee;
    }

    protected getCacheKey(flushCache?: boolean) {
        if (this.tree && !flushCache)
            return this.tree.cacheKey;
        else
            return Constants.WEBAPI_STRING_EMPTY;
    }

    protected clearCacheKey() {
        if (this.tree)
            this.tree.cacheKey = undefined;
    }

    protected getSelectedEmployeeIds(allIfNoSelected: boolean = false): number[] {
        var employeeIds: number[] = [];

        if (this.currentTreeViewModeIsGroup) {
            if (this.contentGroupSelected && this.contentGroupSelected.length > 0) {
                _.forEach(this.contentGroupSelected, (row: any) => {
                    employeeIds.push(row.employeeId);
                });
            }
            else if (this.currentGroup && allIfNoSelected) {
                employeeIds = this.currentGroup.getVisibleEmployeeIds();
            }
        }

        return employeeIds;
    }

    protected getCalculationFunctionTerm(option: SoeTimeAttestFunctionOption) {
        switch (option) {
            case SoeTimeAttestFunctionOption.RestoreToSchedule:
                return this.termsArray["time.time.attest.restoretoschedule"];
            case SoeTimeAttestFunctionOption.RestoreToScheduleDiscardDeviations:
                return this.termsArray["time.time.attest.restoretoschedulediscarddeviations"];
            case SoeTimeAttestFunctionOption.RestoreScheduleToTemplate:
                return this.termsArray["time.time.attest.restorescheduletotemplate"];
            case SoeTimeAttestFunctionOption.ReGenerateDaysBasedOnTimeStamps:
                return this.termsArray["time.time.attest.restorefromtimestamps"];
            case SoeTimeAttestFunctionOption.ReGenerateTransactionsDiscardAttest:
                return this.termsArray["time.time.attest.regeneratetransactions"];
            case SoeTimeAttestFunctionOption.DeleteTimeBlocksAndTransactions:
                return this.termsArray["time.time.attest.deletetimeblockandtransactions"];
            case SoeTimeAttestFunctionOption.OpenAbsenceDialog:
                return this.termsArray["time.time.attest.absence"];
            case SoeTimeAttestFunctionOption.ReverseTransactions:
                return this.termsArray["time.time.attest.reversetransactions"];
            case SoeTimeAttestFunctionOption.ReGenerateVacationsTransactionsDiscardAttest:
                return this.termsArray["time.time.attest.regeneratevacationstransactions"];
            case SoeTimeAttestFunctionOption.AttestReminder:
                return this.termsArray["time.time.attest.sendattestreminder"];
            case SoeTimeAttestFunctionOption.RunAutoAttest:
                return this.termsArray["time.time.attest.tree.autoattest"];
            case SoeTimeAttestFunctionOption.RecalculateAccounting:
                return this.termsArray["time.time.attest.recalculateaccounting"];
            case SoeTimeAttestFunctionOption.UpdateAbsenceDetails:
                return this.termsArray["time.time.attest.absencedetails.update"];
            case SoeTimeAttestFunctionOption.CalculatePeriods:
                return this.termsArray["time.time.attest.calculate.overtime"];
        }
    }

    protected showSaveAttestResultMessage(result: any, attestStateTo: AttestStateDTO) {
        var skipDialog: boolean = true;
        var message: string = '';

        if (result) {
            message = '<span>{0}</span>'.format(this.termsArray["time.time.attest.saveattestresultvalid"].format(result.integerValue.toString(), StringUtility.nullToEmpty(attestStateTo.name)));
        }
        if (result && (!result.integerValue || result.integerValue2)) { //show if valid is zero or invalid is over zero
            message += "<br />";
            message += '<span style="color: #DB000C">{0}</span>'.format(this.termsArray["time.time.attest.saveattestresultinvalid"].format(result.integerValue2.toString(), StringUtility.nullToEmpty(attestStateTo.name)));
            skipDialog = false;
        }

        this.completedSave(null, skipDialog, message, true);
    }

    protected showSaveAttestEmployeesResultMessage(result: EmployeesAttestResult, attestStateTo: AttestStateDTO) {
        if (!this.isCurrentGroupValid() || !result || result.success || result.attestStateToId != attestStateTo.attestStateId)
            return;

        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Time/TimeAttest/Dialogs/AttestResult/AttestResultDialog.html"),
            controller: AttestResultDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                employeeResults: () => { return result.employeeResults },
                attestStateTo: () => { return attestStateTo },
            }
        });
    }

    protected getTimePeriodFromId(timePeriodId: number): TimePeriodDTO {
        var timePeriod = (_.filter(this.timePeriods, { timePeriodId: timePeriodId }))[0];
        if (timePeriod) {
            timePeriod = TimePayrollUtility.setTimePeriodDates(timePeriod);
        }
        return timePeriod;
    }

    protected getTimePeriodNext(date: Date): TimePeriodDTO {
        var timePeriod = _.filter(this.timePeriods, s => s.stopDate.isAfterOnDay(date))[0];
        if (timePeriod) {
            timePeriod = TimePayrollUtility.setTimePeriodDates(timePeriod);
        }
        return timePeriod;
    }

    protected getTimePeriodPrev(date: Date): TimePeriodDTO {
        var timePeriod = _.filter(this.timePeriods, s => s.startDate.isBeforeOnDay(date))[0];
        if (timePeriod) {
            timePeriod = TimePayrollUtility.setTimePeriodDates(timePeriod);
        }
        return timePeriod;
    }

    protected getLatestTimePeriod(date: Date): TimePeriodDTO {
        var timePeriod = _.filter(this.timePeriods, s => s.stopDate.isBeforeOnDay(date))[0];
        if (timePeriod) {
            timePeriod = TimePayrollUtility.setTimePeriodDates(timePeriod);
        }
        return timePeriod;
    }

    protected getAttestState(attestStateId: number): AttestStateDTO {
        return (_.filter(this.currentAttestStates, { attestStateId: attestStateId }))[0];
    }

    protected getDateFromCurrentView(setPreviousTimePeriod: boolean = false): Date {
        var date: Date = CalendarUtility.getDateToday();
        if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Day) {
            date = this.currentStartDate;
            if (setPreviousTimePeriod)
                date = date.addDays(-1);
        }
        else if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Week) {
            date = this.currentStartDate.beginningOfWeek();
            if (setPreviousTimePeriod)
                date = date.addWeeks(-1);
        }
        else if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Month) {
            date = this.currentStartDate.beginningOfMonth();
            if (setPreviousTimePeriod)
                date = date.addMonths(-1);
        }
        else if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Period) {
            if (this.currentTimePeriod) {
                date = this.currentTimePeriod.startDate;
                if (setPreviousTimePeriod) {
                    var previousTimePeriod = TimePayrollUtility.getPreviousTimePeriod(this.timePeriods, this.currentTimePeriodId);
                    if (previousTimePeriod)
                        date = previousTimePeriod.startDate;
                }
            }
        }
        return date;
    }

    protected getWeekFromCurrentView(): Date {
        var date: Date = CalendarUtility.getDateToday();
        if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Day) {
            date = this.currentStartDate;
        }
        else if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Month) {
            date = this.currentStartDate.beginningOfWeek();
        }
        if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Period) {
            if (this.currentTimePeriod)
                date = this.currentTimePeriod.startDate;
        }
        return date;
    }

    protected getMonthFromCurrentView(): Date {
        var date: Date = CalendarUtility.getDateToday();
        if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Day || this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Week) {
            date = this.currentStartDate.beginningOfMonth();
        }
        if (this.userSettingTimeLatestTimePeriodType === TimePeriodSelectorType.Period) {
            if (this.currentTimePeriod)
                date = this.currentTimePeriod.startDate.beginningOfMonth();
        }
        return date;
    }

    protected getTimePeriodFromCurrentView(date: Date): TimePeriodDTO {
        var timePeriod = TimePayrollUtility.getTimePeriodFromDate(this.timePeriods, date)
        if (!timePeriod)
            timePeriod = this.getLatestTimePeriod(date);
        return timePeriod;
    }

    protected setTimePeriodType(timePeriodType: TimePeriodSelectorType, init: boolean = false) {
        var date = this.getDateFromCurrentView(init && this.companyTimeDefaultPreviousTimePeriod);
        this.setTimePeriodTypeForDate(timePeriodType, date);
    }

    protected setTimePeriodTypeForDate(timePeriodType: TimePeriodSelectorType, startDate: Date) {
        this.currentTimePeriodType = timePeriodType;
        if (this.currentTimePeriodType == TimePeriodSelectorType.Day)
            this.setTimePeriodTypeDay(startDate);
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Week)
            this.setTimePeriodTypeWeek(startDate);
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Month)
            this.setTimePeriodTypeMonth(startDate);
        else if (this.currentTimePeriodType == TimePeriodSelectorType.Period)
            this.setTimePeriodTypePeriod(this.getTimePeriodFromCurrentView(startDate));
        else
            return;

        this.userSettingTimeLatestTimePeriodType = this.currentTimePeriodType;
        this.setTimePeriodChanged();
    }

    protected setTimePeriodTypeDay(date: Date) {
        var stopDate = date.endOfDay();
        if (!this.hasPermissionToSeeDate(date))
            return;

        this._currentStartDate = date.beginningOfDay();
        this.currentStopDate = stopDate;
        this.currentTimePeriodId = 0;
        this.currentTimePeriodName = this.currentStartDate.toFormattedDate();
    }

    protected setTimePeriodTypeWeek(date: Date) {
        var stopDate = date.endOfWeek().endOfDay();
        if (!this.hasPermissionToSeeDate(date))
            return;

        this._currentStartDate = date.beginningOfWeek();
        this.currentStopDate = stopDate;
        this.currentTimePeriodId = 0;
        this.currentTimePeriodName = this.currentStartDate.week().toString();
    }

    protected setTimePeriodTypeMonth(date: Date) {
        var stopDate = date.endOfMonth().endOfDay();
        if (!this.hasPermissionToSeeDate(stopDate))
            return;

        this._currentStartDate = date.beginningOfMonth();
        this.currentStopDate = stopDate;
        this.currentTimePeriodId = 0;
        this.currentTimePeriodName = new Date(1900, this.currentStartDate.getMonth() + 1, 0).toLocaleString(CoreUtility.language, { month: "long" }).toUpperCaseFirstLetter();
    }

    protected setTimePeriodTypePeriod(timePeriod: TimePeriodDTO) {
        if (!timePeriod)
            return;

        var stopDate = timePeriod.stopDate.endOfDay();
        if (this.currentTimePeriod && this.currentTimePeriod.stopDate < stopDate && !this.hasPermissionToSeeDate(timePeriod.stopDate.endOfDay()))
            return;

        this._currentStartDate = timePeriod.startDate.beginningOfDay();
        this.currentStopDate = stopDate;
        this.currentTimePeriodId = timePeriod.timePeriodId;
        this.currentTimePeriodName = timePeriod.name;
        this.currentTimePeriod = timePeriod;
    }

    protected setTimePeriodChanged() {
        if (this.isMyTime) {
            this.loadAttestStates();
            this.loadEmployeeContent();
        }
        else
            this.loadTreeDefault();
    }

    protected setEmployeeChanged() {
        if (!this.isMyTime) {
            this.setupEmployee(this.currentEmployee, true);
        }
    }

    protected doShowHeaderWarningForUnhandledShifts() {
        return this.hasUnhandledEmployees() && _.filter(this.unhandledEmployees, (unhandledShiftChange: TimeUnhandledShiftChangesEmployeeDTO) => unhandledShiftChange.hasShiftDays).length > 0;
    }

    protected doShowHeaderWarningForUnhandledExtraShifts() {
        return this.hasUnhandledEmployees() && _.filter(this.unhandledEmployees, (unhandledShiftChange: TimeUnhandledShiftChangesEmployeeDTO) => unhandledShiftChange.hasExtraShiftDays).length > 0;
    }

    protected doShowHeaderWarningForDays() {
        return this.currentContentViewModeIsAttestEmployee && this.contentEmployee && _.filter(this.contentEmployee, (day: AttestEmployeeDayDTO) => day.hasWarnings).length > 0;
    }

    protected doShowHeaderWarningForGroupNotLoaded() {
        return this.currentTreeViewModeIsGroup && this.currentGroup.preview;
    }

    protected doShowEmployeeContent(): boolean {
        return this.contentEmployee && this.contentEmployee.length > 0;
    }

    protected doShowNoEmployeeContent(): boolean {
        return this.loadedContentEmployee && !this.doShowEmployeeContent();
    }

    protected doRefreshEmployeeDays(result: AttestEmployeeDayDTO[], filterDate?: Date): boolean {
        if (!this.contentEmployee || this.contentEmployee.length <= 0 || !result) {
            return false;
        }
        if (this.contentEmployee.length > 0 && result.length > 0 && this.contentEmployee[0].employeeId !== result[0].employeeId) {
            return false;
        }
        if (!filterDate) {
            if (this.contentEmployee.length !== result.length) {
                return false;
            }
            if (!_.min(_.map(this.contentEmployee, e => e.date)).isSameDayAs(_.min(_.map(result, e => e.date)))) {
                return false;
            }
            if (!_.max(_.map(this.contentEmployee, e => e.date)).isSameDayAs(_.max(_.map(result, e => e.date)))) {
                return false;
            }
        }
        return true;
    }

    protected editEmployee() {
        if (!this.currentEmployee)
            return;

        var modal = this.modalInstance.open({
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
                id: this.currentEmployee.employeeId,
            });
        });

        modal.result.then(result => {
            if (result.modified) {
                this.loadEmployee();
            }
        });
    }

    protected openAnnualSummary() {
        if (!this.isCurrentEmployeeValid())
            return;

        var employee: EmployeeListDTO = new EmployeeListDTO();
        employee.employeeId = this.currentEmployee.employeeId;
        employee.name = this.currentEmployee.employeeName;
        let employment = new EmployeeListEmploymentDTO();
        employment.dateFrom = CalendarUtility.convertToDate(this.currentEmployee.currentEmploymentDateFromString);
        employment.dateTo = CalendarUtility.convertToDate(this.currentEmployee.currentEmploymentDateToString);
        employee.employments = [employment];

        // Show annual summary dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/Planning/Dialogs/AnnualSummary/Views/annualSummary.html"),
            controller: AnnualSummaryController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                dateFrom: () => { return this.currentStartDate },
                dateTo: () => { return null },
                planningPeriodHeadId: () => { return null },
                periodName: () => { return null },
                employee: () => { return employee },
                recalcOnOpen: () => { return true },
                planningPeriodColorOver: () => { return this.planningPeriodColorOver },
                planningPeriodColorEqual: () => { return this.planningPeriodColorEqual },
                planningPeriodColorUnder: () => { return this.planningPeriodColorUnder },
            }
        }
        this.$uibModal.open(options);
    }

    protected openReverseTransactionsDialog(validationResult: IReverseTransactionsValidationDTO) {
        if (!this.isCurrentEmployeeValid())
            return;

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/ReverseTransactions/ReverseTransactionsDialog.html"),
            controller: ReverseTransactionsDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                employeeId: () => { return this.currentEmployee.employeeId },
                usePayroll: () => { return validationResult.usePayroll },
                timePeriods: () => { return validationResult.validPeriods },
                deviationCauses: () => { return validationResult.validCauses },
                dates: () => { return validationResult.validDates },
            }
        }
        var modal = this.$uibModal.open(options);
        modal.result.then(val =>
            this.loadEmployeeContent()
        );
    }

    protected validate() {
    }
}