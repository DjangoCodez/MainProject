import { AttestEmployeeListDTO } from "../../../Common/Models/AttestEmployeeListDTO";
import { AttestPayrollTransactionDTO } from "../../../Common/Models/AttestPayrollTransactionDTO";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { EmployeeTimePeriodDTO } from "../../../Common/Models/EmployeeTimePeriodDTO";
import { EmployeeVacationPeriodDTO } from "../../../Common/Models/EmployeeVacationPeriodDTO";
import { FixedPayrollRowDTO } from "../../../Common/Models/FixedPayrollRowDTO";
import { PayrollCalculationProductDTO } from "../../../Common/Models/PayrollCalculationProductDTO";
import { RetroactivePayrollDTO } from "../../../Common/Models/RetroactivePayroll";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { TimeAccumulatorItem } from "../../../Common/Models/TimeAccumulatorDTOs";
import { AttestEmployeeAdditionDeductionDTO, EmployeesAttestResult, PayrollCalculationEmployeePeriodDTO, PayrollCalculationPeriodSumDTO, TimeEmployeeTreeDTO, TimeEmployeeTreeGroupNodeDTO, TimeEmployeeTreeNodeDTO, TimeEmployeeTreeSettings } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IReportService } from "../../../Core/Services/reportservice";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAttestPayrollTransactionDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { AttestReminderDialogController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/AttestReminder/AttestReminderDialogController";
import { AttestResultDialogController } from "../../../Shared/Time/Time/TimeAttest/Dialogs/AttestResult/AttestResultDialogController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CompanySettingType, Feature, SettingMainType, SoeCategoryType, SoeEmployeeTimePeriodStatus, SoeEmploymentFinalSalaryStatus, SoeEntityState, SoeReportTemplateType, TermGroup_TimeTreeWarningFilter, TermGroup, TermGroup_AttestEntity, TermGroup_AttestTreeGrouping, TermGroup_AttestTreeSorting, TermGroup_EmployeeTaxType, TermGroup_ReportExportType, TermGroup_TimePeriodType, UserSettingType, TermGroup_PayrollControlFunctionStatus } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IconLibrary, PayrollCalculationContentViewMode, PayrollCalculationFunctions, PayrollCalculationRecalculateFunctions, PayrollCalculationReloadFunctions, SOEMessageBoxButtons, SOEMessageBoxImage, SOEMessageBoxSize, TimeTreeViewMode } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { SoeGridOptions } from "../../../Util/SoeGridOptions";
import { Guid, StringUtility } from "../../../Util/StringUtility";
import { TimePayrollUtility } from "../../../Util/TimePayrollUtility";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { EditController as EmployeeEditController } from "../../Employee/Employees/EditController";
import { IPayrollService } from "../PayrollService";
import { EditController as RetroactiveEditController } from "../Retroactive/EditController";
import { AddedTransactionDialogControlller } from "./Dialogs/AddedTransaction/AddedTransactionDialogController";
import { UnhandledTransactionsDialogController } from "./Dialogs/UnhandledTransactions/UnhandledTransactionsDialogController";

export class EditController extends EditControllerBase {
    // Collections
    termsArray: any;

    // Init params
    employeeId: number = 0;

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
    toolbarWarningFilter: SmallGenericType[];
    timePeriodHeads: ISmallGenericType[];
    timePeriods: TimePeriodDTO[];
    contentEmployee: PayrollCalculationProductDTO[];
    contentEmployeeSelected: PayrollCalculationProductDTO[];
    contentGroupSelected: PayrollCalculationEmployeePeriodDTO[];
    contentFixed: FixedPayrollRowDTO[];
    contentRetroactive: RetroactivePayrollDTO[];
    contentAdditionDeduction: AttestEmployeeAdditionDeductionDTO[];
    contentAdditionDeductionSelected: AttestEmployeeAdditionDeductionDTO[];
    deletedFixedPayrollRows: FixedPayrollRowDTO[] = [];
    calculationPeriodSum: PayrollCalculationPeriodSumDTO;
    timeAccumulators: TimeAccumulatorItem[];
    employmentTaxBasisBeforeGivenPeriod: number;
    tree: TimeEmployeeTreeDTO;
    treeUrl: any;
    timeTreeUrl: any;
    modalInstance: any;

    //Search
    toolbarSearchPatternInput: string;
    toolbarSearchPattern: string;

    //Filter
    treeFilterTextInput: string;
    treeFilterText: string;

    private get isMySelf(): boolean {
        return this.currentEmployee && this.currentEmployee.employeeId == this.employeeId;
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

    // Data
    toolbarFilterActive: boolean = false;
    toolbarSelectionIgnoreEmploymentStopDate: boolean = false;
    toolbarSelectionShowOnlyApplyFinalSalary: boolean = false;
    toolbarSelectionShowOnlyAppliedFinalSalary: boolean = false;
    toolbarGroupingActive: boolean = false;
    toolbarSortingActive: boolean = false;
    companyUseAccountHierarchy: boolean = false;
    companyPayrollMinimumAttestStateId: number = 0;
    companyPayrollResultingAttestStateId: number = 0;
    companyPayrollLockedAttestStateId: number = 0;
    companyPayrollApproved1AttestStateId: number = 0;
    companyPayrollApproved2AttestStateId: number = 0;
    companyPayrollExportFileCreatedAttestStateId: number = 0;    
    userSettingPayrollCalculationTreeLatestGrouping: TermGroup_AttestTreeGrouping = TermGroup_AttestTreeGrouping.None;
    userSettingPayrollCalculationTreeLatestSorting: TermGroup_AttestTreeSorting = TermGroup_AttestTreeSorting.None;
    userSettingPayrollCalculationTreeDoNotShowCalculated: boolean = false;
    userSettingPayrollCalculationTreeWarningFilter: TermGroup_TimeTreeWarningFilter = TermGroup_TimeTreeWarningFilter.None;
    userSettingPayrollCalculationTreeDoShowOnlyWithWarnings: boolean = false;
    userSettingPayrollCalculationTreeDisableAutoLoad: boolean = false;
    userSettingPayrollCalculationDisableSaveAttestWarning: boolean = false;
    userSettingPayrollCalculationDisableRecalculatePeriodWarning: boolean = false;
    userSettingPayrollCalculationDisableRecalculateAccountingWarning: boolean = false;
    userSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning: boolean = false;
    userSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning: boolean = false;
    userSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning: boolean = false;
    userSettingTimeLatestAttestStateTo: number = 0;
    payrollSlipReportId: number = 0;
    payrollSlipDataStorageId: number = 0;

    //Buttons
    contentViewModeFunctions: any = [];
    calculationOptions: any = [];
    recalculateOptions: any = [];
    selectedRecalculateOption: {};
    reloadOptions: any = [];
    selectedReloadOption: {};
    selectedAttestStateOption: {};
    showRecalculateExportedEmploymentTax: boolean = false;

    //Permissions
    readPermissionsLoaded: boolean = false;
    modifyPermissionsLoaded: boolean = false;
    fixedPayrollRowsReadPermission: boolean = false;
    fixedPayrollRowsModifyPermission: boolean = false;
    retroactivePayrollReadPermission: boolean = false;
    retroactivePayrollModifyPermission: boolean = false;
    sendAttestReminderPermission: boolean = false;
    socialSecPermission: boolean = false;

    //Current values and flags
    loadingTree: boolean = false;
    loadingTreeWarnings: boolean = false;
    loadedTreeWarnings = false;
    loadedContentWithShowAll: boolean = false;
    currentGuid: Guid;
    currentTimerToken: number;
    currentTimePeriodSelectedDebounced: any;
    currentTimePeriodHeadId: number = 0;
    currentTimePeriod: TimePeriodDTO;
    get currentTimePeriodId(): number {
        if (this.currentTimePeriod)
            return this.currentTimePeriod.timePeriodId;
        else
            return 0;
    }
    set currentTimePeriodId(id: number) {
        if (this.timePeriods) {
            var timePeriod = this.getTimePeriodFromId(id);
            if (timePeriod) {
                if (this.currentTimePeriod && this.currentTimePeriod.timePeriodHeadId != timePeriod.timePeriodHeadId) {
                    this.currentEmployee = null;
                    this.currentGroup = null;
                }
                this.currentTimePeriod = timePeriod;
                this.timePeriodSelected();
            }
        }
        else {
            this.currentTimePeriod = null;
        }
    }
    currentGroup: TimeEmployeeTreeGroupNodeDTO;
    currentEmployee: TimeEmployeeTreeNodeDTO;
    currentEmployeeGoToDebounced: any;
    currentEmployeeVacationPeriod: EmployeeVacationPeriodDTO;
    currentEmployeeTimePeriod: EmployeeTimePeriodDTO;
    currentAttestStates: AttestStateDTO[] = [];
    currentAttestStateOptions: any = [{}];
    currentLoadingAttestStates: boolean;
    currentLastLoadAttestStatesEmployeeId: number = 0;
    currentLastLoadAttestStatesTimePeriodId: number = 0;
    currentSumsExpanded: boolean = false;
    currentEmployeeNoteHasChanged: boolean = false;
    currentWatchLogs: string[];
    currentTreeViewMode = TimeTreeViewMode.None;
    get currentTreeViewModeIsGroup(): boolean {
        return this.currentTreeViewMode == TimeTreeViewMode.Group;
    }
    get currentTreeViewModeIsEmployee(): boolean {
        return this.currentTreeViewMode == TimeTreeViewMode.Employee;
    }
    currentContentViewMode = PayrollCalculationContentViewMode.None;
    get currentContentViewModeIsCalculation(): boolean {
        return this.currentContentViewMode == PayrollCalculationContentViewMode.Calculation;
    }
    get currentContentViewModeIsFixed(): boolean {
        return this.currentContentViewMode == PayrollCalculationContentViewMode.Fixed;
    }
    get currentContentViewModeIsRetroactive(): boolean {
        return this.currentContentViewMode == PayrollCalculationContentViewMode.Retroactive;
    }
    get currentContentViewModeIsControl(): boolean {
        return this.currentContentViewMode == PayrollCalculationContentViewMode.Control;
    }
    get currentContentViewModeIsCalendar(): boolean {
        return this.currentContentViewMode == PayrollCalculationContentViewMode.Calendar;
    }
    get currentContentViewModeIsAdditionAndDeduction(): boolean {
        return this.currentContentViewMode == PayrollCalculationContentViewMode.AdditionAndDeduction;
    }
    currentEmployeeLoading: boolean = false;

    isEmploymentTaxMinimumLimitReachedIncludingThisPeriod: boolean = false;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Charts
    private chartsCreated: boolean = false;

    // Warnings
    private _payrollWarnings: any = [];
    private payrollWarningsLoaded = false;
    private warningsOpen: boolean = false;
    private payrollWarningsError: number = 0;
    private payrollWarningsWarning: number = 0;
    private warningsVisible: boolean = false;
    private _showHistory: boolean = false;

    get showHistory(): boolean {
        return this._showHistory;
    }
    set showHistory(value: boolean) {
        this._showHistory = value;
    }
    get payrollWarnings(): any {
        return this._payrollWarnings;
    }
    set payrollWarnings(value: any) {
        this._payrollWarnings = value;
        this.updateWarnings();
    }

    constructor(
        private timePeriodAccountValueId: number,
        private $timeout: ng.ITimeoutService,
        private $window: ng.IWindowService,
        $uibModal,
        private $http,
        private $templateCache,
        coreService: ICoreService,
        private payrollService: IPayrollService,
        private reportService: IReportService,
        private reportDataService: IReportDataService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        //private $scope: ng.IScope,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super("Time.Payroll.PayrollCalculation.Edit", Feature.Time_Payroll_Calculation, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.modalInstance = $uibModal;
        this.treeUrl = urlHelperService.getViewUrl("payrollCalculationTree.html");
        this.timeTreeUrl = urlHelperService.getCoreComponent("timeTree.html");
        this.currentTimePeriodSelectedDebounced = _.debounce(this.timePeriodSelected, 1000);
        this.currentEmployeeGoToDebounced = _.debounce(this.setEmployeeChanged, 1000);

        // Config parameters
        this.employeeId = soeConfig.employeeId;

        this.initGrid();
    }

    // SETUP

    protected setupLookups() {
        this.startLoad();

        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadModifyPermissions(),
            this.loadReadPermissions(),
            this.loadUserSettings(),
            this.loadToolbarSorting(),
            this.loadToolbarWarningFilter(),
            this.loadTimePeriodHeads()]).then(() => {
                this.loadToolbarGrouping();
                this.loadTreeDefault();
                this.isDirty = false;
            });

        this.loadPayrollSlipReportId();
    }

    private setupToolBar() {
        if (this.gridButtonGroups) {
            this.gridButtonGroups.length = 0;
        }
        if (this.setupDefaultToolBar()) {
            if (this.currentTreeViewModeIsGroup) {
                //Reload
                this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                    this.loadGroupContent();
                })));

                if (CoreUtility.isSupportAdmin && this.showRecalculateExportedEmploymentTax) {
                    this.showRecalculateExportedEmploymentTax = false;
                    //RecalculateExportedEmploymentTax
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("time.payroll.payrollcalculation.recalculateexportedemploymenttax", "time.payroll.payrollcalculation.recalculateexportedemploymenttax", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.initRecalculateExportedEmploymentTax();
                    })));
                }
            }
            if (this.currentTreeViewModeIsEmployee) {

                if (this.currentContentViewModeIsCalculation) {
                    //Lock payrollperiod
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.payroll.payrollcalculation.lockperiod", IconLibrary.FontAwesome, "fal fa-lock-alt",
                        () => {
                            this.executeCalculationFunction({ id: PayrollCalculationFunctions.LockPeriod });
                        },
                        null,
                        () => {
                            return !this.showLockPeriodButton();
                        },
                    )));
                    //UnLock payrollperiod
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.payroll.payrollcalculation.unlockperiod", IconLibrary.FontAwesome, "fal fa-unlock-alt",
                        () => {
                            this.executeCalculationFunction({ id: PayrollCalculationFunctions.UnLockPeriod });
                        },
                        null,
                        () => {
                            return !this.showUnLockPeriodButton();
                        },
                    )));
                    //Report TimeSalarySpecification
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.payroll.payrollcalculation.timesalaryspecificationreport", IconLibrary.FontAwesome, "fa-print", () => {
                        this.printPayrollSlipReport();
                    })));
                    //Add transaction
                    if (this.modifyPermission) {
                        this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.payroll.payrollcalculation.addtransaction", IconLibrary.FontAwesome, "fa-plus", () => {
                            this.openAddedTransactionDialog(null);
                        })));
                    }
                }
                else if (this.currentContentViewModeIsFixed) {
                    //Reload
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.loadEmployeeFixedContent();
                    })));
                    //Add row
                    if (this.fixedPayrollRowsModifyPermission) {
                        this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                            this.fixedPayrollRowsAddRow();
                        })));
                    }
                }
                else if (this.currentContentViewModeIsRetroactive) {
                    //Reload
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.loadEmployeeRetroactiveContent(false);
                    })));
                    //Add row
                    if (this.retroactivePayrollModifyPermission) {
                        this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "time.payroll.retroactive.new", IconLibrary.FontAwesome, "fa-plus", () => {
                            this.retroactivePayrollAddRow();
                        })));
                    }
                }
                else if (this.currentContentViewModeIsAdditionAndDeduction) {
                    //Reload
                    this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton(null, "core.reload_data", IconLibrary.FontAwesome, "fa-sync", () => {
                        this.loadAdditionDeductionTransactions();
                    })));
                }
            }
        }
    }

    private initGrid() {
        //Grid group
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_GROUP_ROWS_SELECTED, (selectedItems: PayrollCalculationEmployeePeriodDTO[]) => {
            this.contentGroupSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_GROUP_ROWS_FILTERED, (data: { rows: PayrollCalculationEmployeePeriodDTO[], totalCount: number }) => {
            if (this.currentGroup) {
                this.currentGroup.filteredPayrollEmployeePeriods = data.rows;
                this.currentGroup.totalCount = data.totalCount;
                if (this.currentGroup.employeeNodes) {
                    _.forEach(this.currentGroup.employeeNodes, (employeeNode: TimeEmployeeTreeNodeDTO) => {
                        employeeNode.visible = _.filter(this.currentGroup.filteredPayrollEmployeePeriods, { employeeId: employeeNode.employeeId }).length > 0;
                    });
                    this.currentGroup.expanded = true;
                }
            }
        }, this.$scope);

        //Grid Employee
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_EMPLOYEE_EDIT_ADDED_TRANSACTION, (transaction: AttestPayrollTransactionDTO) => {
            this.openAddedTransactionDialog(transaction);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_EMPLOYEE_ROWS_SELECTED, (selectedItems: PayrollCalculationProductDTO[]) => {
            this.contentEmployeeSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_RECALCULATE_PERIOD, (incPrelTransactions: boolean) => {
            this.recalculatePeriodForEmployee(incPrelTransactions);
        }, this.$scope);

        //Grid Fixed
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_FIXED_DELETE_ROW, (row: FixedPayrollRowDTO) => {
            this.fixedPayrollRowsDeleteRow(row);
        }, this.$scope);

        //Grid Retroactive
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_EMPLOYEE_RETROACTIVE_EDIT, (retroactivePayroll: RetroactivePayrollDTO) => {
            this.editRetroactivePayroll(retroactivePayroll);
        }, this.$scope);

        //Grid AdditionDeduction
        this.messagingService.subscribe(Constants.EVENT_ADDITIONDEDUCTION_ROWS_SELECTED, (selectedItems: AttestEmployeeAdditionDeductionDTO[]) => {
            this.contentAdditionDeductionSelected = selectedItems;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_ADDITIONDEDUCTION_ROWS_RELOAD, () => {
            this.loadAdditionDeductionTransactions();
        }, this.$scope);

        //Payroll warnings
        this.messagingService.subscribe(Constants.EVENT_PAYROLL_CALCULATION_WARNINGS_SAVEDORCALCULATED, () => {
            this.refreshTreeForEmployee(this.currentEmployee.employeeId);
        }, this.$scope);
    }

    private createCharts() {
        this.chartsCreated = true;
    }

    private reloadCharts() {
        this.$scope.$broadcast(Constants.EVENT_RELOAD_CHARTS, { guid: this.guid });
    }

    private openWarnings() {
        this.warningsOpen = true;
    }
    private reloadWarnings() {        
        if (this.currentEmployee && this.currentEmployee.employeeId != 0)
            this.payrollService.getPayrollWarnings(this.currentEmployee.employeeId, this.currentEmployeeTimePeriod?.employeeTimePeriodId ?? 0, this.showHistory).then(x => {
                this.payrollWarnings = x;
                this.payrollWarningsLoaded = true;
                this.$scope.$broadcast(Constants.EVENT_RELOAD_GRID, { guid: this.guid });

                if (!this.warningsOpen && this.payrollWarningsError > 0)
                    this.warningsVisible = true;
            });
        else if (this.currentGroup && this.currentGroup.filteredPayrollEmployeePeriods)
            this.reloadWarningsGroup();
    }
    private reloadWarningsGroup() {
        return; //Not implemented yet
        /*
        let employeeIds = _.map(this.currentGroup.filteredPayrollEmployeePeriods, e => e.employeeId);
        let timePeriodId = this.currentGroup.filteredPayrollEmployeePeriods[0].timePeriodId;

        this.payrollService.getPayrollWarningsGroup(employeeIds, timePeriodId, this.showHistory).then(x => {

                this.payrollWarnings = x;
                this.$scope.$broadcast(Constants.EVENT_RELOAD_GRID, { guid: this.guid });

                if (!this.warningsOpen && this.payrollWarningsError > 0)
                    this.warningsVisible = true;
            });
            */
    }

    private updateWarnings() {
        this.payrollWarningsError = 0;
        this.payrollWarningsWarning = 0;
        this.payrollWarningsError = _.filter(this.payrollWarnings, f =>
            f.state != SoeEntityState.Deleted &&
            f.status != TermGroup_PayrollControlFunctionStatus.HideforPeriod &&
            (f.isStoppingPayrollWarning)
        ).length;
        this.payrollWarningsWarning = _.filter(this.payrollWarnings, f =>
            f.state != SoeEntityState.Deleted &&
            f.status != TermGroup_PayrollControlFunctionStatus.HideforPeriod
        ).length - this.payrollWarningsError;
    }

    // LOOKUPS

    private loadTerms() {
        var keys: string[] = [
            "core.reload_data",
            "core.warning",
            "core.donotshowagain",
            "common.employee",
            "common.reportsettingmissing",
            "time.time.attest.accounts",
            "time.time.attest.sendattestreminder",
            "time.time.attest.saveattestemployees",
            "time.time.attest.additiondeduction",
            "time.time.attest.saveattestresultinvalid",
            "time.time.attest.saveattestresultvalid",
            "time.payroll.retroactive.payroll",
            "time.payroll.payrollcalculation.calculation",
            "time.payroll.payrollcalculation.finalsalary",
            "time.payroll.payrollcalculation.finalsalaryquestion",
            "time.payroll.payrollcalculation.finalsalariesquestion",
            "time.payroll.payrollcalculation.finalsalariesnoselected",
            "time.payroll.payrollcalculation.createfinalsalaryerror",
            "time.payroll.payrollcalculation.createfinalsalaryprogress",
            "time.payroll.payrollcalculation.deletefinalsalaryerror",
            "time.payroll.payrollcalculation.deletefinalsalary",
            "time.payroll.payrollcalculation.deletefinalsalariesquestion",
            "time.payroll.payrollcalculation.deletefinalsalaryquestion",
            "time.payroll.payrollcalculation.deletefinalsalaryprogress",            
            "time.payroll.payrollcalculation.fixedpayrollrows",
            "time.payroll.payrollcalculation.getunhandledtransactionsbackwards",
            "time.payroll.payrollcalculation.getunhandledtransactionsforward",
            "time.payroll.payrollcalculation.getunhandledtransactionsquestionbackwards",
            "time.payroll.payrollcalculation.getunhandledtransactionsquestionforward",
            "time.payroll.payrollcalculation.lockperiod",
            "time.payroll.payrollcalculation.lockperioderror",
            "time.payroll.payrollcalculation.lockperiodquestion",
            "time.payroll.payrollcalculation.nounhandledtransactions",
            "time.payroll.payrollcalculation.recalculate",
            "time.payroll.payrollcalculation.recalculateaccounting",
            "time.payroll.payrollcalculation.recalculateaccountingquestion",
            "time.payroll.payrollcalculation.recalculateerror",
            "time.payroll.payrollcalculation.recalculateincprel",
            "time.payroll.payrollcalculation.recalculatequestion",
            "time.payroll.payrollcalculation.recalculateexportedemploymenttaxquestion",
            "time.payroll.payrollcalculation.recalculateperiodlogs",
            "time.payroll.payrollcalculation.reloaddetailed",
            "time.payroll.payrollcalculation.transactionshasinvalidstatus",
            "time.payroll.payrollcalculation.transactionsislocked",
            "time.payroll.payrollcalculation.unlockperiod",
            "time.payroll.payrollcalculation.unlockperioderror",
            "time.payroll.payrollcalculation.unlockperiodquestion",
            "time.payroll.payrollcalculation.finalsalariesnoselected",
            "time.payroll.payrollcalculation.clearcalculation",
            "time.payroll.payrollcalculation.warnings.runcontroll",
            "time.payroll.payrollcalculation.warnings.runcontroll.question"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.SalaryExportPayrollMinimumAttestStatus);
        settingTypes.push(CompanySettingType.SalaryExportPayrollResultingAttestStatus);
        settingTypes.push(CompanySettingType.SalaryPaymentLockedAttestStateId);
        settingTypes.push(CompanySettingType.SalaryPaymentApproved1AttestStateId);
        settingTypes.push(CompanySettingType.SalaryPaymentApproved2AttestStateId);
        settingTypes.push(CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId);        

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.companyUseAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.companyPayrollMinimumAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportPayrollMinimumAttestStatus);
            this.companyPayrollResultingAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryExportPayrollResultingAttestStatus);
            this.companyPayrollLockedAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentLockedAttestStateId);
            this.companyPayrollApproved1AttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentApproved1AttestStateId);
            this.companyPayrollApproved2AttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentApproved2AttestStateId);
            this.companyPayrollExportFileCreatedAttestStateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentExportFileCreatedAttestStateId);                       
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(UserSettingType.PayrollCalculationTreeLatestGrouping);
        settingTypes.push(UserSettingType.PayrollCalculationTreeLatestSorting);
        settingTypes.push(UserSettingType.PayrollCalculationTreeDoNotShowCalculated);
        settingTypes.push(UserSettingType.PayrollCalculationTreeDoShowOnlyWithWarnings);
        settingTypes.push(UserSettingType.PayrollCalculationTreeWarningFilter);
        settingTypes.push(UserSettingType.PayrollCalculationTreeDisableAutoLoad);
        settingTypes.push(UserSettingType.PayrollCalculationDisableApplySaveAttestWarning);
        settingTypes.push(UserSettingType.PayrollCalculationDisableRecalculatePeriodWarning);
        settingTypes.push(UserSettingType.PayrollCalculationDisableRecalculateAccountingWarning);
        settingTypes.push(UserSettingType.PayrollCalculationDisableRecalculateExportedEmploymentTaxWarning);
        settingTypes.push(UserSettingType.PayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning);
        settingTypes.push(UserSettingType.PayrollCalculationDisableGetUnhandledTransactionsForwardsWarning);
        settingTypes.push(UserSettingType.PayrollLatestAttestStateTo);

        return this.coreService.getUserSettings(settingTypes).then(result => {
            this.userSettingPayrollCalculationTreeLatestGrouping = SettingsUtility.getIntUserSetting(result, UserSettingType.PayrollCalculationTreeLatestGrouping, TermGroup_AttestTreeGrouping.All, false);
            this.userSettingPayrollCalculationTreeLatestSorting = SettingsUtility.getIntUserSetting(result, UserSettingType.PayrollCalculationTreeLatestSorting, TermGroup_AttestTreeSorting.FirstName, false);
            this.userSettingPayrollCalculationTreeDoNotShowCalculated = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationTreeDoNotShowCalculated);
            this.userSettingPayrollCalculationTreeDoShowOnlyWithWarnings = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationTreeDoShowOnlyWithWarnings);
            this.userSettingPayrollCalculationTreeWarningFilter = SettingsUtility.getIntUserSetting(result, UserSettingType.PayrollCalculationTreeWarningFilter);
            this.userSettingPayrollCalculationTreeDisableAutoLoad = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationTreeDisableAutoLoad);
            this.userSettingPayrollCalculationDisableSaveAttestWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationDisableApplySaveAttestWarning);
            this.userSettingPayrollCalculationDisableRecalculatePeriodWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationDisableRecalculatePeriodWarning);
            this.userSettingPayrollCalculationDisableRecalculateAccountingWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationDisableRecalculateAccountingWarning);
            this.userSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationDisableRecalculateExportedEmploymentTaxWarning);
            this.userSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning);
            this.userSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning = SettingsUtility.getBoolUserSetting(result, UserSettingType.PayrollCalculationDisableGetUnhandledTransactionsForwardsWarning);
            this.userSettingTimeLatestAttestStateTo = SettingsUtility.getIntUserSetting(result, UserSettingType.PayrollLatestAttestStateTo);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Payroll_Calculation_FixedPayrollRows);
        featureIds.push(Feature.Time_Payroll_Retroactive);
        featureIds.push(Feature.Time_Time_Attest_SendAttestReminder);
        featureIds.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.fixedPayrollRowsModifyPermission = x[Feature.Time_Payroll_Calculation_FixedPayrollRows];
            this.retroactivePayrollModifyPermission = x[Feature.Time_Payroll_Retroactive];
            this.sendAttestReminderPermission = x[Feature.Time_Time_Attest_SendAttestReminder];
            this.socialSecPermission = x[Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec];
            this.modifyPermissionsLoaded = true;
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Payroll_Calculation_FixedPayrollRows);
        featureIds.push(Feature.Time_Payroll_Retroactive);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.fixedPayrollRowsReadPermission = x[Feature.Time_Payroll_Calculation_FixedPayrollRows];
            this.retroactivePayrollReadPermission = x[Feature.Time_Payroll_Retroactive];
            this.readPermissionsLoaded = true;
        });
    }

    private loadToolbarGrouping(): ng.IPromise<any> {
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
        return this.coreService.getTermGroupContent(TermGroup.TimeAttestTreeSorting, false, false).then((result: SmallGenericType[]) => {
            this.toolbarSortingOptions = result;
        });
    }

    private loadToolbarSelectionCategories() {

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
        if (!this.currentTimePeriod)
            return;

        var selectedIds: number[] = [];
        if (this.toolbarSelectionAccountsSelected) {
            _.forEach(this.toolbarSelectionAccountsSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        return this.coreService.getAccountsFromHierarchyByUserSetting(this.currentTimePeriod.startDate, this.currentTimePeriod.stopDate, false, false, true, false).then(result => {
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
        var selectedIds: number[] = [];
        if (this.toolbarSelectionEmployeesSelected) {
            _.forEach(this.toolbarSelectionEmployeesSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        var filterGroupIds = StringUtility.getCollectionIdsStr(this.companyUseAccountHierarchy ? this.toolbarSelectionCategoriesSelected : this.toolbarSelectionAccountsSelected);

        this.payrollService.getEmployeesForPayrollCalculationTree(filterGroupIds, this.currentTimePeriodId).then((result: AttestEmployeeListDTO[]) => {
            this.toolbarSelectionEmployees.length = 0;
            this.toolbarSelectionEmployeesSelected.length = 0;
            _.forEach(_.sortBy(result, t => t.employeeNrSort), (employee: AttestEmployeeListDTO) => {
                var selectionEmployee = {
                    id: employee.employeeId,
                    label: "(" + employee.employeeNr + ") " + employee.name,
                };
                this.toolbarSelectionEmployees.push(selectionEmployee);
                if (selectedIds.some(id => id == employee.employeeId))
                    this.toolbarSelectionEmployeesSelected.push(selectionEmployee);
            });
            this.isDirty = false;
        });
    }

    private loadToolbarSelectionAttestStates() {
        var selectedIds: number[] = [];
        if (this.toolbarSelectionAttestStatesSelected) {
            _.forEach(this.toolbarSelectionAttestStatesSelected, (item: any) => {
                selectedIds.push(item.id);
            });
        }

        this.payrollService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, this.currentTimePeriod.startDate, this.currentTimePeriod.stopDate, false, this.isMySelf ? this.currentEmployee.employeeGroupId : null).then((result) => {
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

    private loadToolbarWarningFilter(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeTreeWarningFilter, false, false, true).then((result: SmallGenericType[]) => {
            this.toolbarWarningFilter = result;
        });
    }

    private loadPayrollWarnings(loadAll: boolean = false) {
        this.payrollWarnings = [];
        if (loadAll)
            this.$scope.$broadcast(Constants.EVENT_RELOAD_GROUP_GRID, { guid: this.guid });
        else
            this.$scope.$broadcast(Constants.EVENT_RELOAD_GRID, { guid: this.guid });
        this.reloadWarnings();
    }

    private loadPayrollSlipReportId() {
        this.reportService.getStandardReportId(SettingMainType.Company, CompanySettingType.DefaultPayrollSlipReport, SoeReportTemplateType.PayrollSlip).then((x) => {
            this.payrollSlipReportId = x;
        });
    }

    // EVENTS

    protected timePeriodHeadSelected(id) {
        this.currentTimePeriodHeadId = id;
        this.loadTimePeriods();
    }

    protected timePeriodSelected() {
        this.loadTreeDefault();

        this.loadToolbarSelectionAttestStates();
        if (!this.hasSelectionEmployees()) {
            if (this.companyUseAccountHierarchy)
                this.loadToolbarSelectionAccounts();
            else
                this.loadToolbarSelectionCategories();
        }
        else {
            this.loadToolbarSelectionEmployees();
        }            
    }

    protected setEmployeeChanged() {
        this.setupEmployee(this.currentEmployee, true);
    }

    protected toolbarFilterShow() {
        this.toolbarFilterActive = !this.toolbarFilterActive;
        this.toolbarGroupingActive = false;
        this.toolbarSortingActive = false;
    }

    protected isToolbarFilterActiveOrOpen(): boolean {
        return this.toolbarFilterActive || this.hasToolbarFilter()
    }

    protected toolbarSelectionDisabled(): boolean {
        return this.currentTimePeriodId <= 0;
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

    protected toolbarSelectionShowOnlyApplyFinalSalaryChanged() {
        this.toolbarSelectionShowOnlyAppliedFinalSalary = false;
    }

    protected toolbarSelectionShowOnlyAppliedFinalSalaryChanged() {
        this.toolbarSelectionShowOnlyApplyFinalSalary = false;
    }

    protected toolbarGroupingShow() {
        this.toolbarGroupingActive = !this.toolbarGroupingActive;
        this.toolbarFilterActive = false;
        this.toolbarSortingActive = false;
    }

    protected toolbarGroupingDisabled(): boolean {
        return this.currentTimePeriodId <= 0;
    }

    protected toolbarGroupingChanged(id: any) {
        this.userSettingPayrollCalculationTreeLatestGrouping = id;
        var forceLoad = true;
        if (this.userSettingPayrollCalculationTreeDisableAutoLoad && !this.tree)
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
        return this.currentTimePeriodId <= 0;
    }

    protected toolbarSortingChanged(id) {
        this.userSettingPayrollCalculationTreeLatestSorting = id;
        var forceLoad = true;
        if (this.userSettingPayrollCalculationTreeDisableAutoLoad && !this.tree)
            forceLoad = false;
        this.saveUserSettingTimeAttestTreeLatestSorting();
        this.loadTreeDefault(forceLoad);
    }

    protected toolbarDoNotShowCalculatedChanged() {
        this.saveUserSettingPayrollCalculationTreeDoNotShowCalculated();
    }

    protected toolbarFilterOnWarnings(id: any) {
        this.userSettingPayrollCalculationTreeWarningFilter = id;
        this.saveUserSettingPayrollCalculationTreeWarningFilter();
        if (!this.userSettingPayrollCalculationTreeDisableAutoLoad || this.tree)
            this.toolbarReload();
    }

    protected toolbarDoShowOnlyWithWarnings() {
        this.saveUserSettingPayrollCalculationTreeDoShowOnlyWithWarnings();
    }

    protected toolbarLoadTreeDisabled(): boolean {
        return this.currentTimePeriodId <= 0 || this.loadingTree;
    }

    protected toolbarPrevPeriodDisabled(): boolean {
        return this.currentTimePeriodId <= 0 || !this.timePeriods || !TimePayrollUtility.getPreviousTimePeriodByPaymentDate(this.timePeriods, this.currentTimePeriodId);
    }

    protected toolbarPrevPeriodGoTo() {
        var previousTimePeriod = TimePayrollUtility.getPreviousTimePeriodByPaymentDate(this.timePeriods, this.currentTimePeriodId);
        if (previousTimePeriod) {
            this.currentTimePeriod = this.getTimePeriodFromId(previousTimePeriod.timePeriodId);
            this.currentTimePeriodSelectedDebounced();
        }
    }

    protected toolbarNextPeriodDisabled(): boolean {
        return this.currentTimePeriodId <= 0 || !this.timePeriods || !TimePayrollUtility.getNextTimePeriodByPaymentDate(this.timePeriods, this.currentTimePeriodId);
    }

    protected toolbarNextPeriodGoTo() {
        var nextTimePeriod = TimePayrollUtility.getNextTimePeriodByPaymentDate(this.timePeriods, this.currentTimePeriodId);
        if (nextTimePeriod) {
            this.currentTimePeriod = this.getTimePeriodFromId(nextTimePeriod.timePeriodId);
            this.currentTimePeriodSelectedDebounced();
        }
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

        if (this.userSettingPayrollCalculationTreeDisableAutoLoad)
            this.tree = null;
        else
            this.loadTree(true, true, true, false);
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

        var prevEmployeeNode = TimePayrollUtility.getPrevEmployeeNode(this.tree.groupNodes, this.currentEmployee);
        if (!prevEmployeeNode)
            return;

        this.currentEmployee = prevEmployeeNode;
        this.currentEmployeeGoToDebounced();
    }

    protected nextEmployeeGoTo() {
        if (!this.currentEmployee)
            return;

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

    protected groupNodeClick(groupNode: TimeEmployeeTreeGroupNodeDTO, force: boolean = false) {
        if (!groupNode)
            return;

        window.scrollTo(0, 0);

        //reset values
        this.currentEmployee = null;
        this.calculationPeriodSum = null;
        this.timeAccumulators = null;

        this.currentGroup = groupNode;
        this.initTreeViewModeGroup();
        this.setCalculationFunctions();
        this.loadAttestStates();
        this.loadGroupContent();
    }

    protected employeeNodeClick(employeeNode: TimeEmployeeTreeNodeDTO, force: boolean = false) {
        this.setupEmployee(employeeNode, force);
    }

    private setupEmployee(employeeNode: TimeEmployeeTreeNodeDTO, force: boolean = false) {
        window.scrollTo(0, 0);

        //reset
        this.currentEmployeeLoading = true;
        this.currentGroup = null;
        this.calculationPeriodSum = null;
        this.timeAccumulators = null;
        this.currentEmployee = employeeNode;
        this.initTreeViewModeEmployee();

        //First time we open the right side
        if (this.currentContentViewMode === PayrollCalculationContentViewMode.None)
            this.initContentViewModeCalculation(false);

        //reload
        this.setContentViewModeFunctions();
        this.loadEmployeeTimePeriod(true);
        this.loadEmployeeVacationPeriod();
        this.loadAttestStates();

        if (this.currentContentViewModeIsCalculation)             
            this.loadEmployeeContent(false);
        else if (this.currentContentViewModeIsFixed)
            this.loadEmployeeFixedContent();
        else if (this.currentContentViewModeIsRetroactive)
            this.loadEmployeeRetroactiveContent(false);
        else if (this.currentContentViewModeIsAdditionAndDeduction)
            this.loadAdditionDeductionTransactions();       

        if (this.currentSumsExpanded)
            this.loadTimeAccumulators();
    }

    private editRetroactivePayroll(retroactivePayroll: RetroactivePayrollDTO) {
        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/Retroactive/Views/edit.html"),
            controller: RetroactiveEditController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: retroactivePayroll ? retroactivePayroll.retroactivePayrollId : 0,
                employeeId: retroactivePayroll ? 0 : this.currentEmployee.employeeId,
                timePeriodHeadId: retroactivePayroll ? 0 : this.currentTimePeriod.timePeriodHeadId,
                timePeriodId: retroactivePayroll ? 0 : this.currentTimePeriod.timePeriodId,
            });
        });

        modal.result.then(id => {
            this.loadEmployeeRetroactiveContent(true);
        });
    }

    protected currentEmployeeNoteChanged(model) {
        if (this.currentEmployee && this.currentEmployee.note != model) {
            this.currentEmployee.note = model;
            this.currentEmployeeNoteHasChanged = true;
        }
    }

    protected currentSumsExpandedChanged() {
        this.currentSumsExpanded = !this.currentSumsExpanded;
        this.loadTimeAccumulators();
    }

    protected executeContentViewModeFunction(option) {
        switch (option.id) {
            case PayrollCalculationContentViewMode.Calculation:
                this.initContentViewModeCalculation(true);
                break;
            case PayrollCalculationContentViewMode.Fixed:
                this.initContentViewModeFixed();
                break;
            case PayrollCalculationContentViewMode.Retroactive:
                this.initContentViewModeRetroactive();
                break;
            case PayrollCalculationContentViewMode.Control:
                this.initContentViewModeControl();
                break;
            case PayrollCalculationContentViewMode.Calendar:
                this.initContentViewModeCalendar();
                break;
            case PayrollCalculationContentViewMode.AdditionAndDeduction:
                this.initContentViewModeAdditionAndDeduction();
                break;
        }
    }

    protected executeReloadOption(option) {
        switch (option.id) {
            case PayrollCalculationReloadFunctions.Reload:
                this.loadEmployeeContent(false);
                break;
            case PayrollCalculationReloadFunctions.ReloadDetailed:
                this.loadEmployeeContent(true);
                break;
        }
    }

    protected executeRecalculateFunction(option) {
        switch (option.id) {
            case PayrollCalculationRecalculateFunctions.Recalculate:
                this.initRecalculatePeriod(false);
                break;
            case PayrollCalculationRecalculateFunctions.RecalculateIncPrelTransaction:
                this.initRecalculatePeriod(true);
                break;
            case PayrollCalculationRecalculateFunctions.RecalculateAccounting:
                this.initRecalculateAccounting();
                break;
        }
    }

    protected executeCalculationFunction(option) {

        switch (option.id) {
            case PayrollCalculationFunctions.LockPeriod:
                this.initLockPeriod();
                break;
            case PayrollCalculationFunctions.UnLockPeriod:
                this.initUnLockPeriod();
                break;
            case PayrollCalculationFunctions.CreateFinalSalary:
                this.initCreateFinalSalary();
                break;
            case PayrollCalculationFunctions.DeleteFinalSalary:
                this.initDeleteFinalSalary();
                break;
            case PayrollCalculationFunctions.GetUnhandledTransactionsBackwards:
                this.initGetUnhandledTransactionsBackwards();
                break;
            case PayrollCalculationFunctions.GetUnhandledTransactionsForward:
                this.initGetUnhandledTransactionsForward();
                break;
            case PayrollCalculationFunctions.AttestReminder:
                this.initAttestReminder();
                break;
            case PayrollCalculationFunctions.ClearPayrollCalculation:
                this.initClearPayrollCalculation();
                break;
            case PayrollCalculationFunctions.RunPayrollControll:
                this.initRunPayrollControll();
                break;
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

    protected initContentViewModeCalculation(loadContent: boolean) {
        if (this.currentContentViewModeIsCalculation)
            return;
        this.currentContentViewMode = PayrollCalculationContentViewMode.Calculation;
        this.setupToolBar();
        if (loadContent)
            this.loadEmployeeContent(false);
    }

    protected initContentViewModeFixed() {
        if (this.currentContentViewModeIsFixed)
            return;
        this.currentContentViewMode = PayrollCalculationContentViewMode.Fixed;
        this.setupToolBar();
        this.loadEmployeeFixedContent();
    }

    protected initContentViewModeRetroactive() {
        if (this.currentContentViewModeIsRetroactive)
            return;
        this.currentContentViewMode = PayrollCalculationContentViewMode.Retroactive;
        this.setupToolBar();
        this.loadEmployeeRetroactiveContent(false);
    }

    protected initContentViewModeAdditionAndDeduction() {
        if (this.currentContentViewModeIsAdditionAndDeduction)
            return;
        this.currentContentViewMode = PayrollCalculationContentViewMode.AdditionAndDeduction;
        this.setupToolBar();
        this.loadAdditionDeductionTransactions();
    }

    protected initContentViewModeControl() {
        if (this.currentContentViewModeIsControl)
            return;
        this.currentContentViewMode = PayrollCalculationContentViewMode.Control;
        this.setupToolBar();
    }

    protected initContentViewModeCalendar() {
        if (this.currentContentViewModeIsCalendar)
            return;
        this.currentContentViewMode = PayrollCalculationContentViewMode.Calendar;
        this.setupToolBar();
    }

    protected initRecalculatePeriod(incPrelTransactions: boolean) {
        if (!this.validateRecalculatePeriod())
            return;

        var { result: result, msg: msg } = this.hasLockedTransactions();
        if (result) {
            var message: string = this.termsArray["time.payroll.payrollcalculation.transactionsislocked"];
            if (!StringUtility.isEmpty(msg))
                message += "<br>" + this.termsArray["common.employee"] + ": " + msg;

            this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.recalculateerror"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        if (this.userSettingPayrollCalculationDisableRecalculatePeriodWarning) {
            this.recalculatePeriod(incPrelTransactions);
        }
        else {
            var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.recalculatequestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.termsArray["core.donotshowagain"]);
            modal.result.then((res: any) => {
                if (res) {
                    if (res.isChecked)
                        this.saveUserSettingPayrollCalculationDisableRecalculatePeriodWarning();
                    this.recalculatePeriod(incPrelTransactions);
                }
            });
        }
    }

    protected initRecalculateAccounting() {
        if (!this.validateRecalculateAccounting())
            return;

        if (this.userSettingPayrollCalculationDisableRecalculateAccountingWarning) {
            this.recalculateAccounting();
        }
        else {
            var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.recalculateaccountingquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.termsArray["core.donotshowagain"]);
            modal.result.then((res: any) => {
                if (res) {
                    if (res.isChecked)
                        this.saveUserSettingPayrollCalculationDisableRecalculateAccountingWarning();
                    this.recalculateAccounting();
                }
            });
        }
    }

    protected initRecalculateExportedEmploymentTax() {
        if (!this.validateRecalculateExportedEmploymentTax())
            return;

        if (this.userSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning) {
            this.recalculateExportedEmploymentTax();
        }
        else {
            var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.recalculateexportedemploymenttaxquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.termsArray["core.donotshowagain"]);
            modal.result.then((res: any) => {
                if (res) {
                    if (res.isChecked)
                        this.saveUserSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning();
                    this.recalculateExportedEmploymentTax();
                }
            });
        }
    }

    protected initLockPeriod() {
        if (!this.validateLockPeriod())
            return;

        var { result: result, msg: msg } = this.hasLockPeriodInvalidTransactionStates();
        if (result) {
            var message: string = this.termsArray["time.payroll.payrollcalculation.transactionshasinvalidstatus"];
            if (!StringUtility.isEmpty(msg))
                message += "<br>" + this.termsArray["common.employee"] + ": " + msg;

            this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.lockperioderror"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        //User cannot choose show not again!
        var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.lockperiodquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then((res: any) => {
            if (res) {
                if (this.currentTreeViewModeIsEmployee) {
                    this.lockPeriod();
                }
                else if (this.currentTreeViewModeIsGroup) {
                    this.lockPeriodForEmployees();
                }
            }
        });
    }
    protected initUnLockPeriod() {
        if (!this.validateUnLockPeriod())
            return;

        if (this.hasUnLockPeriodInvalidTransactionStates()) {
            this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.unlockperioderror"], this.termsArray["time.payroll.payrollcalculation.transactionshasinvalidstatus"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        //User cannot choose show not again!
        var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.unlockperiodquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then((res: any) => {
            if (res) {
                if (this.currentTreeViewModeIsEmployee) {
                    this.unLockPeriod();
                }
                else if (this.currentTreeViewModeIsGroup) {
                    this.unLockPeriodForEmployees();
                }

            }
        });
    }

    protected initCreateFinalSalary() {

        var { result: result } = this.hasLockedTransactions();
        if (result) {
            var message: string = this.termsArray["time.payroll.payrollcalculation.transactionsislocked"];
            this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.createfinalsalaryerror"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        if (this.currentTreeViewModeIsEmployee) {
            if (!this.validateCreateFinalSalary())
                return;

            //User cannot choose show not again!
            var modalEmployee = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.finalsalaryquestion"].format(StringUtility.nullToEmpty(CalendarUtility.toFormattedDate(this.currentEmployee.finalSalaryEndDate))), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNoCancel);
            modalEmployee.result.then((res: any) => {
               this.createFinalSalary(res === true);
            }, (reason) => {
                if ("cancel") {
                    //do nothing
                }
            });
        }
        else if (this.currentTreeViewModeIsGroup) {
            if (!this.validateCreateFinalSalaries())
                return;

            //User cannot choose show not again!
            var modalGroup = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.finalsalariesquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNoCancel);
            modalGroup.result.then((res: any) => {
                this.createFinalSalaries(res === true);
            }, (reason) => {
                if ("cancel") {
                    //do nothing
                }
            });
        }
    }
    protected initDeleteFinalSalary() {

        var { result: result } = this.hasLockedTransactions();
        if (result) {
            var message: string = this.termsArray["time.payroll.payrollcalculation.transactionsislocked"];
            this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.deletefinalsalaryerror"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        if (this.currentTreeViewModeIsEmployee) {
            if (!this.validateDeleteFinalSalary())
                return;

            //User cannot choose show not again!
            var modal1 = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.deletefinalsalaryquestion"].format(StringUtility.nullToEmpty(CalendarUtility.toFormattedDate(this.currentEmployee.finalSalaryEndDateApplied))), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal1.result.then((res: any) => {
                if (res) {
                    this.deleteFinalSalary();
                }
            });
        }
        else if (this.currentTreeViewModeIsGroup) {
            if (!this.validateDeleteFinalSalaries())
                return;

            //User cannot choose show not again!
            var modal2 = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.deletefinalsalariesquestion"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal2.result.then((res: any) => {
                if (res) {
                    this.deleteFinalSalaries();
                }
            });
        }
    }

    protected initClearPayrollCalculation() {
        if (!this.validateClearPayrollCalculation())
            return;
       
        //User cannot choose show not again!
        var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.clearcalculation"].format(StringUtility.nullToEmpty(CalendarUtility.toFormattedDate(this.currentEmployee.finalSalaryEndDateApplied))), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then((res: any) => {
            if (res) {
                this.clearPayrollCalculation();
            }
        });
    }

    protected initRunPayrollControll() {
        if (!this.validateRecalculatePeriod())
            return;

        var modalGroup = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.warnings.runcontroll.question"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
        modalGroup.result.then((res: any) => {
            if (res)
                this.runPayrollControll();
        }, (reason) => {
            if ("cancel") {
                //do nothing
            }
        });
    }

    protected initGetUnhandledTransactionsBackwards() {
        if (!this.validGetUnhandledTransactionsBackwards())
            return;

        if (this.userSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning) {
            this.getUnhandledTransactionsBackwards();
        }
        else {
            var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.getunhandledtransactionsquestionbackwards"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.termsArray["core.donotshowagain"]);
            modal.result.then((res: any) => {
                if (res) {
                    if (res.isChecked)
                        this.saveUserSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning();
                    this.getUnhandledTransactionsBackwards();
                }
            });
        }
    }
    protected initGetUnhandledTransactionsForward() {
        if (!this.validGetUnhandledTransactionsForward)
            return;
        
        if (this.userSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning) {
            this.getUnhandledTransactionsForward();
        }
        else {
            var modal = this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.getunhandledtransactionsquestionforward"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, false, true, this.termsArray["core.donotshowagain"]);
            modal.result.then((res: any) => {
                if (res) {
                    if (res.isChecked)
                        this.saveUserSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning();
                    this.getUnhandledTransactionsForward();
                }
            });
        }
    }

    protected initAttestReminder() {
        if (!this.sendAttestReminderPermission)
            return;

        this.openAttestReminderDialog();
    }

    protected fixedPayrollRowsDeleteRow(row: FixedPayrollRowDTO) {
        if (row.fixedPayrollRowId > 0) {
            row.state = SoeEntityState.Deleted;
            this.deletedFixedPayrollRows.push(row);
        }

        _.pull(this.contentFixed, row);
    }

    protected fixedPayrollRowsAddRow() {
        var row = new FixedPayrollRowDTO();
        row.fromDate = null;
        row.toDate = null;
        row.quantity = 1;
        row.unitPrice = 0;
        row.amount = 0;
        row.vatAmount = 0;
        row.actorCompanyId = CoreUtility.actorCompanyId;
        row.employeeId = this.currentEmployee.employeeId;
        row.state = SoeEntityState.Active;

        if (!this.contentFixed)
            this.contentFixed = [];
        this.contentFixed.push(row);
    }

    protected retroactivePayrollAddRow() {
        this.editRetroactivePayroll(null);
    }

    protected openAddedTransactionDialog(transaction: AttestPayrollTransactionDTO) {
        if (transaction && !transaction.isAdded)
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getUrl("Dialogs/AddedTransaction/AddedTransactionDialog.html"),
            controller: AddedTransactionDialogControlller,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                employeeId: () => { return this.currentEmployee.employeeId },
                timePeriodId: () => { return this.currentTimePeriodId },
                ignoreEmploymentHasEnded: () => { return this.toolbarSelectionIgnoreEmploymentStopDate },
                transaction: () => { return transaction },
            }
        });

        modal.result.then(val => {
            if (val === true) {
                this.recalculatePeriodForEmployee(false, true);
            } else {
                this.loadEmployeeContent(this.loadedContentWithShowAll, false);
                this.loadPayrollWarnings();
            }
        });
    }

    protected openUnhandledTransactionsDialog(transactions: IAttestPayrollTransactionDTO[], backwards: boolean, startDate: Date, stopDate: Date) {
        if (!transactions)
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getUrl("Dialogs/UnhandledTransactions/UnhandledTransactionsDialog.html"),
            controller: UnhandledTransactionsDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                payrollService: () => { return this.payrollService },
                translationService: () => { return this.translationService },
                messagingService: () => { return this.messagingService },
                employeeId: () => { return this.currentEmployee.employeeId },
                timePeriodId: () => { return this.currentTimePeriodId },
                timePeriods: () => { return this.timePeriods },
                transactions: () => { return transactions },
                isBackwards: () => { return backwards },
                startDate: () => { return startDate },
                stopDate: () => { return stopDate },
            }
        });

        modal.result.then(val => {
            if (val === true) {
                this.loadEmployeeContent(this.loadedContentWithShowAll, false);
                this.loadTimePeriods();
            }
        });
    }

    private openAttestReminderDialog() {
        if (!this.isCurrentGroupAndPeriodValid())
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
                dateFrom: () => { return this.currentTimePeriod.startDate },
                dateTo: () => { return this.currentTimePeriod.stopDate },
            }
        });

        modal.result.then(val => {
        });
    }

    // ACTIONS

    protected printPayrollSlipReport() {
        if (this.payrollSlipReportId && this.payrollSlipReportId != 0) {
            this.reportDataService.createReportJob(ReportJobDefinitionFactory.createSimplePayrollReportDefinition(this.payrollSlipReportId, SoeReportTemplateType.PayrollSlip, [this.currentEmployee.employeeId], [this.currentTimePeriod.timePeriodId], TermGroup_ReportExportType.Pdf), true);
        }
        else {
            this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    // SERVICE CALLS

    protected loadTimePeriodHeads(): ng.IPromise<any> {
        return this.payrollService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.Payroll, false).then((result: ISmallGenericType[]) => {
            this.timePeriodHeads = result;
            if (_.size(this.timePeriodHeads) == 1) {
                this.timePeriodHeadSelected(this.timePeriodHeads[0].id);
            }
        });
    }

    protected hasNegativeVacationDays(): boolean {
        return this.currentEmployeeVacationPeriod.daysSum < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysAdvance < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysOverdue < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysPaid < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysUnpaid < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear1 < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear2 < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear3 < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear4 < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear5 < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear3 < 0 ||
            this.currentEmployeeVacationPeriod.remainingDaysYear3 < 0
    }

    protected loadTimePeriods() {
        this.payrollService.getTimePeriods(this.currentTimePeriodHeadId).then((periods: TimePeriodDTO[]) => {
            this.timePeriods = _.filter(periods, function (o: TimePeriodDTO) {
                return o.paymentDate
            });

            this.timePeriods = _.orderBy(this.timePeriods, ['paymentDate'], ['desc']);

            if (_.size(this.timePeriods) > 0) {
                // First try with current month and a day in the future
                let result = _.filter(this.timePeriods, function (obj: TimePeriodDTO) {
                    return (new Date(<any>obj.paymentDate).getFullYear() === CalendarUtility.getDateNow().getFullYear())
                        && (new Date(<any>obj.paymentDate).getMonth() === CalendarUtility.getDateNow().getMonth())
                        && (new Date(<any>obj.paymentDate).getDay() > CalendarUtility.getDateNow().getDay());
                });
                // Second try with current month and a passed day
                if (_.size(result) === 0) {
                    result = _.filter(this.timePeriods, function (obj: TimePeriodDTO) {
                        return (new Date(<any>obj.paymentDate).getFullYear() === CalendarUtility.getDateNow().getFullYear())
                            && (new Date(<any>obj.paymentDate).getMonth() === CalendarUtility.getDateNow().getMonth())
                    });
                    result = result.reverse();
                }
                // If nothing found in current month, take next date in the future
                if (_.size(result) === 0) {
                    result = _.filter(this.timePeriods, function (obj: TimePeriodDTO) {
                        return (new Date(<any>obj.paymentDate).getDay() > CalendarUtility.getDateNow().getDay());
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
                this.tree = null;
            }

            this.isDirty = false;
        });
    }

    protected loadAttestStates() {
        if (this.currentLoadingAttestStates || !this.isCurrentViewModeValid())
            return;

        var changedToOrFromMySelf: boolean =
            this.currentEmployee &&
            this.currentLastLoadAttestStatesTimePeriodId !== this.currentTimePeriod.timePeriodId &&
            (this.currentLastLoadAttestStatesEmployeeId === this.employeeId || this.currentEmployee.employeeId === this.employeeId);
        var changedFromMySelfToGroup: boolean =
            this.currentLastLoadAttestStatesEmployeeId === this.employeeId && this.currentTreeViewModeIsGroup;
        var changedTimePeriod: boolean =
            this.currentLastLoadAttestStatesTimePeriodId !== this.currentTimePeriodId;

        if (!this.currentAttestStates || changedToOrFromMySelf || changedFromMySelfToGroup || changedTimePeriod) {
            this.currentLoadingAttestStates = true;
            this.currentAttestStates.length = 0;
            this.payrollService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, this.currentTimePeriod.startDate, this.currentTimePeriod.stopDate, false, this.isMySelf ? this.currentEmployee.employeeGroupId : null).then((result) => {
                this.currentAttestStates = result;
                this.currentAttestStateOptions.length = 0;
                this.currentLoadingAttestStates = false;
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
                this.currentLastLoadAttestStatesTimePeriodId = this.currentTimePeriod.timePeriodId;
            });
        }
    }

    protected loadTimeAccumulators(force: boolean = false) {
        if (this.timeAccumulators && !force)
            return;

        if (this.currentTimePeriod.extraPeriod) {
            this.timeAccumulators = [];
            return;
        }

        this.payrollService.getTimeAccumulatorsForEmployee(this.currentEmployee.employeeId, this.currentTimePeriod.startDate, this.currentTimePeriod.stopDate, false, false, true, true, true, true, true).then((result: TimeAccumulatorItem[]) => {
            this.timeAccumulators = result;
            this.isDirty = false;
        });
    }

    protected loadEmployee() {
        if (!this.isCurrentEmployeeAndPeriodValid())
            return;

        this.currentEmployeeNoteHasChanged = false;
        this.payrollService.getEmployeeForPayrollCalculation(this.currentEmployee.employeeId, this.currentTimePeriod).then((result) => {
            var employee = result;
            if (employee && this.currentEmployee) {
                this.currentEmployee.employeeGroupId = employee.currentEmployeeGroupId;
                this.currentEmployee.employeeGroupName = employee.currentEmployeeGroupName;
                this.currentEmployee.payrollGroupId = employee.currentPayrollGroupId;
                this.currentEmployee.payrollGroupName = employee.currentPayrollGroupName;
                this.currentEmployee.vacationGroupId = employee.currentVacationGroupId;
                this.currentEmployee.vacationGroupName = employee.currentVacationGroupName;
                this.currentEmployee.employmentPercent = employee.currentEmploymentPercent;
                this.currentEmployee.disbursementMethod = employee.disbursementMethod;
                this.currentEmployee.disbursementMethodName = employee.disbursementMethodName;
                this.currentEmployee.disbursementAccountNrIsMissing = employee.disbursementAccountNrIsMissing;
                this.currentEmployee.disbursementMethodIsCash = employee.disbursementMethodIsCash;
                this.currentEmployee.disbursementMethodIsUnknown = employee.disbursementMethodIsUnknown;
                this.currentEmployee.taxSettingsAreMissing = employee.taxSettingsAreMissing;
                this.currentEmployee.note = employee.note;
                this.currentEmployee.showNote = employee.showNote;

                if (employee.finalSalaryEndDate)
                    this.currentEmployee.finalSalaryEndDate = new Date(<any>employee.finalSalaryEndDate);
                else
                    this.currentEmployee.finalSalaryEndDate = null;

                if (employee.finalSalaryEndDateApplied) {
                    this.currentEmployee.finalSalaryEndDateApplied = new Date(employee.finalSalaryEndDateApplied);
                    this.currentEmployee.finalSalaryAppliedTimePeriodId = employee.finalSalaryAppliedTimePeriodId;
                }
                else {
                    this.currentEmployee.finalSalaryEndDateApplied = null;
                    this.currentEmployee.finalSalaryAppliedTimePeriodId = null;
                }

                if (employee.employeeTaxSE) {
                    this.currentEmployee.taxRate = employee.employeeTaxSE.taxRate;

                    if (employee.employeeTaxSE.type === TermGroup_EmployeeTaxType.NoTax || employee.employeeTaxSE.type === TermGroup_EmployeeTaxType.SchoolYouth || employee.employeeTaxSE.type === TermGroup_EmployeeTaxType.SideIncomeTax || employee.employeeTaxSE.type === TermGroup_EmployeeTaxType.Sink)
                        this.currentEmployee.taxTableInfo = employee.employeeTaxSE.typeName;
                    else
                        this.currentEmployee.taxTableInfo = employee.employeeTaxSE.taxRate;

                    this.currentEmployee.adjustmentValue = employee.employeeTaxSE.adjustmentValue;
                    this.currentEmployee.adjustmentType = employee.employeeTaxSE.adjustmentType;
                    this.currentEmployee.applyEmploymentTaxMinimumRule = employee.employeeTaxSE.applyEmploymentTaxMinimumRule
                }
                else {
                    this.currentEmployee.taxRate = 0;
                    this.currentEmployee.adjustmentValue = 0;
                    this.currentEmployee.adjustmentType = 0;
                    this.currentEmployee.applyEmploymentTaxMinimumRule = false;
                }
                this.currentEmployee.currentEmploymentTypeString = employee.currentEmploymentTypeString;
                this.currentEmployee.currentEmploymentDateFromString = employee.currentEmploymentDateFromString;
                this.currentEmployee.currentEmploymentDateToString = employee.currentEmploymentDateToString;

                if (this.currentEmployee.applyEmploymentTaxMinimumRule)
                    this.getEmploymentTaxBasisBeforeGivenPeriod();
            }
            this.currentEmployeeLoading = false;
            this.setCalculationFunctions();
            this.setReloadOptions();
            this.isDirty = false;
        });
    }

    protected loadEmployeeVacationPeriod() {
        if (!this.isCurrentEmployeeAndPeriodValid())
            return;

        this.payrollService.getEmployeeVacationPeriod(this.currentEmployee.employeeId, this.currentTimePeriod.timePeriodId).then((result) => {
            this.currentEmployeeVacationPeriod = result;            
        });
    }

    protected loadEmployeeTimePeriod(loadEmployee: boolean = false) {
        if (!this.isCurrentEmployeeAndPeriodValid())
            return;

        this.payrollService.getEmployeeTimePeriod(this.currentEmployee.employeeId, this.currentTimePeriod.timePeriodId).then((result) => {
            this.currentEmployeeTimePeriod = result;
            this.setCalculationFunctions();
            if (loadEmployee)
                this.loadEmployee();
            this.loadPayrollWarnings();
        });
    }

    protected loadEmployeeContent(showAllTransactions: boolean, loadEmployeeVacationPeriod: boolean = true) {
        if (!this.isCurrentEmployeeAndPeriodValid()) {
            return;
        }

        this.contentEmployee = null;
        this.startLoad();
        this.loadedContentWithShowAll = showAllTransactions;

        this.payrollService.getPayrollCalculationProducts(this.currentTimePeriodId, this.currentEmployee.employeeId, showAllTransactions).then((result: PayrollCalculationProductDTO[]) => {
            this.contentEmployee = _.orderBy(result, ['payrollProductNumberSort', 'dateFrom', 'dateTo', 'accountingShortString'], ['asc']);
            if (this.contentEmployee.length > 0) {
                this.loadCalculationPeriodSum();
            } else {
                this.stopProgress();
                this.isDirty = false;
            }

            for (var payrollCalculationProduct in this.contentEmployee) {
                var row = this.contentEmployee[payrollCalculationProduct];
                row["subGridOptions"] = new SoeGridOptions(row.payrollProductString, this.$timeout, this.uiGridConstants);
            }

            if (loadEmployeeVacationPeriod)
                this.loadEmployeeVacationPeriod();
        });
    }

    protected clearEmployeeContent() {
        this.currentEmployee = null;
        this.contentEmployee = null;
    }

    protected loadEmployeeFixedContent() {
        if (!this.isCurrentEmployeeAndPeriodValid()) {
            return;
        }
        this.contentFixed = null;
        this.startLoad();

        this.payrollService.getFixedPayrollRows(this.currentEmployee.employeeId, this.currentTimePeriodId).then(result => {
            this.contentFixed = result;
            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected loadEmployeeRetroactiveContent(loadEmployeeContent: boolean) {
        if (!this.isCurrentEmployeeAndPeriodValid()) {
            return;
        }
        this.contentRetroactive = null;
        this.startLoad();

        this.payrollService.getRetroactivePayrollsForEmployee(this.currentTimePeriod.timePeriodId, this.currentEmployee.employeeId).then(result => {
            this.contentRetroactive = result;
            if (loadEmployeeContent)
                this.loadEmployeeContent(this.loadedContentWithShowAll, false);
            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected loadAdditionDeductionTransactions() {
        if (!this.isCurrentEmployeeAndPeriodValid())
            return;

        this.payrollService.getAdditionDeductions(this.currentEmployee.employeeId, this.currentTimePeriod.startDate, this.currentTimePeriod.stopDate, this.currentTimePeriod.timePeriodId).then((result: AttestEmployeeAdditionDeductionDTO[]) => {
            this.contentAdditionDeduction = result;
            this.isDirty = false;
        });
    }

    protected loadCalculationPeriodSum() {
        if (!this.isCurrentEmployeeAndPeriodValid() || !this.contentEmployee || this.contentEmployee.length == 0) {
            return;
        }

        this.calculationPeriodSum = null;
        this.startLoad();

        this.payrollService.getPayrollCalculationPeriodSum(this.contentEmployee).then((result) => {
            this.calculationPeriodSum = result;
            this.isEmploymentTaxMinimumLimitReachedIncludingThisPeriod = (this.employmentTaxBasisBeforeGivenPeriod + this.calculationPeriodSum.gross + this.calculationPeriodSum.benefitInvertExcluded) >= 1000;
            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected getEmploymentTaxBasisBeforeGivenPeriod() {
        if (!this.isCurrentEmployeeAndPeriodValid())
            return;

        if (this.currentEmployee)

            this.payrollService.getEmploymentTaxBasisBeforeGivenPeriod(this.currentTimePeriod.timePeriodId, this.currentEmployee.employeeId).then((result) => {
                this.employmentTaxBasisBeforeGivenPeriod = result;
            });
    }

    protected loadAfterSelection() {
        this.loadTreeDefault();
    }

    protected loadTreeDefault(forceLoad: boolean = false) {
        this.loadTree(false, false, false, false);
    }
    protected loadTree(flushCache: boolean, forceLoad: boolean, forceSearch: boolean, discardExpandedGroups: boolean) {
        if (this.currentTimePeriodId === 0)
            return;

        var hasToolbarSelection = this.hasToolbarSelection();
        var hasToolbarSearch = this.hasToolbarSearchPattern();
        var doSearch: boolean = hasToolbarSearch || forceSearch;

        if (!forceLoad && !hasToolbarSelection && !doSearch && (this.userSettingPayrollCalculationTreeDisableAutoLoad && !this.tree))
            return;

        if (doSearch && forceSearch && !hasToolbarSearch)
            discardExpandedGroups = true;
        var expandedGroupIds: number[] = discardExpandedGroups ? [] : TimePayrollUtility.getExpandedGroupIds(this.tree, false);

        this.loadingTree = true;
        this.progressMessage = this.termsArray["core.loading"];
        this.tree = null;
        this.loadAttestStates();

        if (this.currentEmployee != null)
            this.employeeNodeClick(this.currentEmployee, true);

        if (!doSearch)
            this.toolbarSearchPattern = Constants.WEBAPI_STRING_EMPTY;

        var settings = new TimeEmployeeTreeSettings();
        settings.filterEmployeeAuthModelIds = this.companyUseAccountHierarchy ? TimePayrollUtility.getCollectionIds(this.toolbarSelectionAccountsSelected) : TimePayrollUtility.getCollectionIds(this.toolbarSelectionCategoriesSelected);
        settings.filterEmployeeIds = TimePayrollUtility.getCollectionIds(this.toolbarSelectionEmployeesSelected);
        settings.filterAttestStateIds = TimePayrollUtility.getCollectionIds(this.toolbarSelectionAttestStatesSelected);
        settings.searchPattern = this.toolbarSearchPattern;
        settings.doNotShowCalculated = this.userSettingPayrollCalculationTreeDoNotShowCalculated;
        settings.includeEnded = this.toolbarSelectionIgnoreEmploymentStopDate;
        settings.includeEmptyGroups = !hasToolbarSelection && !doSearch;
        settings.showOnlyApplyFinalSalary = this.toolbarSelectionShowOnlyApplyFinalSalary;
        settings.showOnlyAppliedFinalSalary = this.toolbarSelectionShowOnlyAppliedFinalSalary
        settings.cacheKeyToUse = this.getCacheKey(flushCache);

        this.payrollService.getPayrollCalculationTree(this.userSettingPayrollCalculationTreeLatestGrouping, this.userSettingPayrollCalculationTreeLatestSorting, this.currentTimePeriodId, settings).then((result: TimeEmployeeTreeDTO) => {
            this.setTreeContent(result, expandedGroupIds, null, hasToolbarSelection, hasToolbarSearch, true, false);
        });
    }

    protected refreshTreeForEmployee(employeeId: number, refreshFinalSalaryStatus: boolean = false) {
        var employeeIds: number[] = [];
        employeeIds.push(employeeId);
        this.refreshTreeForEmployees(employeeIds, refreshFinalSalaryStatus);
    }
    protected refreshTreeForEmployees(employeeIds: number[], refreshFinalSalaryStatus: boolean = false, reloadGroupContent: boolean = false) {
        if (!this.tree || !employeeIds || employeeIds.length === 0)
            return;

        var expandedGroupIds: number[] = TimePayrollUtility.getExpandedGroupIds(this.tree, false);

        var settings = new TimeEmployeeTreeSettings();
        settings.filterEmployeeIds = employeeIds;
        settings.filterAttestStateIds = TimePayrollUtility.getCollectionIds(this.toolbarSelectionAttestStatesSelected);
        settings.includeEnded = this.toolbarSelectionIgnoreEmploymentStopDate;
        settings.doNotShowCalculated = this.userSettingPayrollCalculationTreeDoNotShowCalculated;
        settings.showOnlyApplyFinalSalary = this.toolbarSelectionShowOnlyApplyFinalSalary;
        settings.showOnlyAppliedFinalSalary = this.toolbarSelectionShowOnlyAppliedFinalSalary
        settings.doRefreshFinalSalaryStatus = refreshFinalSalaryStatus;
        settings.cacheKeyToUse = this.getCacheKey();
        
        this.payrollService.refreshPayrollCalculationTree(this.tree, this.currentTimePeriodId, settings).then((result) => {
            this.setTreeContent(result, expandedGroupIds, employeeIds, false, false, reloadGroupContent, true);
            if (this.currentGroup) {
                var currentGroupNode = _.filter(this.tree.groupNodes, { id: this.currentGroup.id })[0];
                if (!currentGroupNode || !currentGroupNode.employeeNodes || currentGroupNode.employeeNodes.length === 0) {
                    this.currentGroup.payrollEmployeePeriods = null;
                }
            }
        });
    }

    protected setTreeContent(tree: TimeEmployeeTreeDTO, expandedGroupIds: number[], employeeIds: number[], hasSelection: boolean, hasSearch: boolean, reloadGroupContent: boolean, flushCache: boolean) {
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

    protected loadTreeWarnings(employeeIds: number[], expandedGroupIds: number[], hasSelection: boolean, hasSearch: boolean, reloadGroupContent: boolean, flushCache: boolean) {
        if (!this.tree)
            return;

        TimePayrollUtility.trySetGroupsExpanded(this.tree, expandedGroupIds, hasSelection, hasSearch);
        this.loadingTreeWarnings = true;

        var warningFilter: TermGroup_TimeTreeWarningFilter = this.userSettingPayrollCalculationTreeWarningFilter;
        var currentEmployeeNodeIndex = TimePayrollUtility.getCurrentEmployeeNodeIndex(this.tree.groupNodes, this.currentEmployee);

        this.payrollService.getPayrollCalculationTreeWarnings(this.tree, this.currentTimePeriodId, employeeIds, warningFilter, flushCache).then((result) => {
            this.tree = result;
            this.loadingTreeWarnings = false;
            this.loadedTreeWarnings = true;

            if (this.currentGroup != null && reloadGroupContent)
                this.reloadGroupContent();
            if (this.hasTreeFilter())
                this.treeFilterChanged();

            TimePayrollUtility.trySetGroupsExpanded(this.tree, expandedGroupIds, hasSelection, hasSearch);

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
            this.currentGroup = TimePayrollUtility.getGroupNode(this.tree.groupNodes, this.currentGroup.id, true);
        this.loadGroupContent();
    }
    protected loadGroupContent() {
        if (!this.isCurrentGroupAndPeriodValid || this.currentGroup == null)
            return;

        this.currentGroup.payrollEmployeePeriods = null;
        this.startLoad();
        var cacheKeyToUse = this.getCacheKey();
        this.payrollService.getPayrollCalculationEmployeePeriods(this.currentTimePeriod.timePeriodId, this.userSettingPayrollCalculationTreeLatestGrouping, this.currentGroup.id, this.currentGroup.getVisibleEmployeeIds(true), this.toolbarSelectionIgnoreEmploymentStopDate, cacheKeyToUse).then(result => {
            this.currentGroup.payrollEmployeePeriods = result;
            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected recalculatePeriod(incPrelTransactions: boolean) {
        if (this.currentTreeViewModeIsEmployee)
            this.recalculatePeriodForEmployee(incPrelTransactions);
        else if (this.currentTreeViewModeIsGroup)
            this.recalculatePeriodForEmployees(incPrelTransactions);
    }
    protected recalculatePeriodForEmployee(incPrelTransactions: boolean, loadEmployeeVacationPeriod: boolean = false) {
        if (!this.validateRecalculatePeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);
        this.currentGuid = Guid.newGuid();

        this.payrollService.recalculatePayrollPeriod(this.currentEmployee.employeeId, this.currentTimePeriod.timePeriodId, incPrelTransactions, this.toolbarSelectionIgnoreEmploymentStopDate).then((result) => {
            if (result.success) {                
                if (result.strings) {                                        
                    this.currentWatchLogs = result.strings;
                }
                this.loadEmployeeContent(this.loadedContentWithShowAll, loadEmployeeVacationPeriod);
                this.loadEmployeeTimePeriod();                
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
                this.completedSave(null);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }
    protected recalculatePeriodForEmployees(incPrelTransactions: boolean) {
        if (!this.validateRecalculatePeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);
        this.currentGuid = Guid.newGuid();

        var employeeIds = this.getSelectedEmployeeIds();
        this.payrollService.recalculatePayrollPeriodForEmployees(this.currentGuid.toString(), employeeIds, this.currentTimePeriod.timePeriodId, incPrelTransactions, this.toolbarSelectionIgnoreEmploymentStopDate).then((result) => {
            if (result.success) {
                if (result.strings) {
                    this.currentWatchLogs = result.strings;
                }
                this.loadGroupContent();                
                this.refreshTreeForEmployees(employeeIds, false, true);
                this.completedSave(null);
            } else {
                if (result.infoMessage) {
                    this.loadGroupContent();
                    this.completedSave(null);

                    this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.recalculateerror"], result.infoMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    return;
                } else {
                    this.failedSave(result.errorMessage);
                }
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected recalculateAccounting() {
        if (this.currentTreeViewModeIsEmployee)
            this.recalculateAccountingForEmployee();
        else if (this.currentTreeViewModeIsGroup)
            this.recalculateAccountingForEmployees();
    }
    protected recalculateAccountingForEmployee() {
        if (!this.validateRecalculateAccounting())
            return;

        var employeeIds: number[] = [];
        employeeIds.push(this.currentEmployee.employeeId);
        this.applyRecalculateAccounting(employeeIds);
    }
    protected recalculateAccountingForEmployees() {
        if (!this.validateRecalculateAccounting())
            return;

        var employeeIds = this.getSelectedEmployeeIds();
        this.applyRecalculateAccounting(employeeIds);
    }
    protected applyRecalculateAccounting(employeeIds: number[]) {
        this.startSave();
        window.scrollTo(0, 0);

        this.payrollService.recalculateAccounting(employeeIds, this.currentTimePeriod.timePeriodId).then((result) => {
            if (result.success) {
                this.loadEmployeeContent(this.loadedContentWithShowAll, false);
                this.completedSave(null);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected recalculateExportedEmploymentTax() {
        if (this.currentTreeViewModeIsGroup)
            this.recalculateExportedEmploymentTaxForEmployees();
    }
    protected recalculateExportedEmploymentTaxForEmployees() {
        if (!this.validateRecalculateExportedEmploymentTax())
            return;

        this.startSave();
        window.scrollTo(0, 0);

        var employeeIds = this.getSelectedEmployeeIds();
        this.payrollService.recalculateExportedEmploymentTaxForEmployees(employeeIds, this.currentTimePeriod.timePeriodId).then((result) => {
            if (result.success) {
                this.loadGroupContent();                
                this.refreshTreeForEmployees(employeeIds);
                this.completedSave(null);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected lockPeriod() {
        if (!this.validateLockPeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);

        this.payrollService.lockPayrollPeriod(this.currentEmployee.employeeId, this.currentTimePeriod.timePeriodId).then((result) => {
            if (result.success) {
                if (result.strings) {
                    this.currentWatchLogs = result.strings;
                }
                this.completedSave(null);
                this.loadEmployeeTimePeriod();
                this.loadEmployeeContent(this.loadedContentWithShowAll);
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }
    protected lockPeriodForEmployees() {
        if (!this.validateLockPeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);

        var employeeIds = this.getSelectedEmployeeIds();
        this.payrollService.lockPayrollPeriodForEmployees(employeeIds, this.currentTimePeriod.timePeriodId).then((result) => {
            if (result.success) {
                if (result.strings) {
                    this.currentWatchLogs = result.strings;
                }
                this.completedSave(null);
                this.loadGroupContent();
                this.refreshTreeForEmployees(employeeIds);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected unLockPeriod() {
        if (!this.validateUnLockPeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);

        this.payrollService.unLockPayrollPeriod(this.currentEmployee.employeeId, this.currentTimePeriod.timePeriodId).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.loadEmployeeTimePeriod();
                this.loadEmployeeContent(this.loadedContentWithShowAll);
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }
    protected unLockPeriodForEmployees() {
        if (!this.validateUnLockPeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);

        var employeeIds = this.getSelectedEmployeeIds();
        this.payrollService.unLockPayrollPeriodForEmployees(employeeIds, this.currentTimePeriod.timePeriodId).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.loadGroupContent();
                this.refreshTreeForEmployees(employeeIds);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected createFinalSalaries(createReport: boolean) {
        if (!this.validateCreateFinalSalaries())
            return;

        var employeeIds = this.getSelectedEmployeeIds();
        if (!employeeIds || employeeIds.length === 0) {
            this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.finalsalariesnoselected"].format(StringUtility.nullToEmpty(CalendarUtility.toFormattedDate(this.currentEmployee?.finalSalaryEndDate))), SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        this.startWork("time.payroll.payrollcalculation.createfinalsalaryprogress");
        window.scrollTo(0, 0);

        this.payrollService.createFinalSalaries(employeeIds, this.currentTimePeriodId, createReport).then((result) => {
            this.tryShowFinalSalaryNotCreatedDialog(result.strings);

            if (result.success) {
                this.refreshTreeForEmployees(employeeIds, true, true);
                this.completedSave(null);
            }
            else {
                if (result.strings)
                    this.failedSave(null, true);
                else
                    this.failedSave(result.errorMessage);
            }

            this.isDirty = false;
        }, error => {
            this.failedSave(error.message);
        });
    }
    protected createFinalSalary(createReport : boolean) {
        if (!this.validateCreateFinalSalary())
            return;

        this.startWork("time.payroll.payrollcalculation.createfinalsalaryprogress");
        window.scrollTo(0, 0);

        this.payrollService.createFinalSalary(this.currentEmployee.employeeId, this.currentTimePeriodId, createReport).then((result) => {
            if (result.success) {
                this.loadEmployeeTimePeriod(true);
                this.loadEmployeeContent(this.loadedContentWithShowAll);
                this.refreshTreeForEmployee(this.currentEmployee.employeeId, true);
                this.loadTimeAccumulators(true);
                this.completedSave(null);
            } else {
                this.failedSave(result.errorMessage);
            }

            this.tryShowFinalSalaryNotCreatedDialog(result.strings);
            this.isDirty = false;
        }, error => {
            this.failedSave(error.message);
        });
    }
    private tryShowFinalSalaryNotCreatedDialog(strings: string[]) {
        if (!strings || strings.length === 0)
            return;

        var title: string = strings[0];
        var message: string = '';
        strings.forEach(str => {
            if (str && str !== title)
                message += str + "\n";
        });
        this.notificationService.showDialog(title, message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    protected deleteFinalSalaries() {
        if (!this.validateDeleteFinalSalaries())
            return;

        var employeeIds = this.getSelectedEmployeeIds();
        if (!employeeIds || employeeIds.length === 0) {
            this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.finalsalariesnoselected"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return;
        }

        this.startWork("time.payroll.payrollcalculation.deletefinalsalaryprogress");
        window.scrollTo(0, 0);

        this.payrollService.deleteFinalSalaries(employeeIds, this.currentTimePeriodId).then((result) => {
            this.tryShowFinalSalaryNotCreatedDialog(result.strings);
            if (result.success) {
                this.refreshTreeForEmployees(employeeIds, true, true);
                this.completedSave(null);
            }
            else {
                if (result.strings)
                    this.failedSave(null, true);
                else
                    this.failedSave(result.errorMessage);
            }

            this.isDirty = false;
        }, error => {
            this.failedSave(error.message);
        });
    }
    protected deleteFinalSalary() {
        if (!this.validateDeleteFinalSalary())
            return;

        this.startWork("time.payroll.payrollcalculation.deletefinalsalaryprogress");
        window.scrollTo(0, 0);

        this.payrollService.deleteFinalSalary(this.currentEmployee.employeeId, this.currentTimePeriodId).then((result) => {
            if (result.success) {
                this.loadEmployeeTimePeriod(true);
                this.loadEmployeeContent(this.loadedContentWithShowAll);
                this.refreshTreeForEmployee(this.currentEmployee.employeeId, true);
                this.completedSave(null);
            } else {
                this.failedSave(result.errorMessage);
            }
            this.isDirty = false;
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected clearPayrollCalculation() {
        if (!this.validateClearPayrollCalculation())
            return;

        this.startSave();
        window.scrollTo(0, 0);
        
        this.payrollService.clearPayrollCalculation(this.currentEmployee.employeeId, this.currentTimePeriodId).then((result) => {
            if (result.success) {
                this.loadEmployeeTimePeriod(true);
                this.loadEmployeeContent(this.loadedContentWithShowAll);
                this.refreshTreeForEmployee(this.currentEmployee.employeeId, true);
                this.completedSave(null);
            } else {
                this.failedSave(result.errorMessage);
            }
            this.isDirty = false;
        }, error => {
            this.failedSave(error.message);
        });
        
    }

    protected runPayrollControll() {
        if (!this.validateRecalculatePeriod())
            return;

        this.startSave();
        window.scrollTo(0, 0);
        var employeeIds = this.getSelectedEmployeeIds();
        this.payrollService.runPayrollControll(employeeIds, this.currentTimePeriodId).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.loadGroupContent();
                this.refreshTreeForEmployees(employeeIds);
            } else {
                this.failedSave(result.errorMessage);
            }
            this.isDirty = false;
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveFixedPayrollRows() {
        if (!this.validateLockPeriod())
            return;

        this.startProgress();
        var rowsToSave: FixedPayrollRowDTO[] = [];
        rowsToSave = _.concat(this.contentFixed, this.deletedFixedPayrollRows);
        rowsToSave = _.filter(rowsToSave, row => !row.isReadOnly);

        this.payrollService.saveFixedPayrollRows(rowsToSave, this.currentEmployee.employeeId).then((result) => {
            if (result.success) {
                if (this.deletedFixedPayrollRows)
                    this.deletedFixedPayrollRows.length = 0;

                this.completedSave(null);
                this.loadEmployeeFixedContent();
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected getUnhandledTransactionsBackwards() {
        if (!this.validGetUnhandledTransactionsBackwards())
            return;

        this.startLoad();
        window.scrollTo(0, 0);

        var unhandledStopDate = this.currentTimePeriod.extraPeriod ? this.currentTimePeriod.paymentDate.addDays(-1) : this.currentTimePeriod.startDate.addDays(-1);
        var unhandledStartDate = unhandledStopDate.addYears(-1);

        this.payrollService.getUnhandledPayrollTransactions(this.currentEmployee.employeeId, unhandledStartDate, unhandledStopDate, true).then((result: IAttestPayrollTransactionDTO[]) => {
            var unhandledTransactions = result;
            this.openUnhandledTransactionsDialog(unhandledTransactions, true, unhandledStartDate, unhandledStopDate);
            this.stopProgress();
            this.isDirty = false;
        });
    }
    protected getUnhandledTransactionsForward() {
        if (!this.validGetUnhandledTransactionsForward())
            return;

        this.startLoad();
        window.scrollTo(0, 0);

        var unhandledStartDate = this.currentTimePeriod.payrollStartDate;
        var unhandledStopDate = this.currentTimePeriod.payrollStopDate;

        this.payrollService.getUnhandledPayrollTransactions(this.currentEmployee.employeeId, unhandledStartDate, unhandledStopDate, false).then((result: IAttestPayrollTransactionDTO[]) => {
            var unhandledTransactions = result;
            if (unhandledTransactions.length > 0)
                this.openUnhandledTransactionsDialog(unhandledTransactions, false, unhandledStartDate, unhandledStopDate);
            else {
                this.notificationService.showDialog(this.termsArray["core.warning"], this.termsArray["time.payroll.payrollcalculation.nounhandledtransactions"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
            }
            this.stopProgress();
            this.isDirty = false;
        });
    }

    protected saveAttest(option: any) {
        if (this.currentTreeViewModeIsGroup)
            this.initSaveAttestForEmployees(option);
        else if (this.currentTreeViewModeIsEmployee)
            this.initSaveAttestForTransactions(option);
    }
    protected initSaveAttestForEmployees(option: any) {
        if (!this.isCurrentGroupAndPeriodValid())
            return;

        var employeeIds = this.getSelectedEmployeeIds();
        var attestStateTo: AttestStateDTO = this.getAttestState(option.id);
        if (!employeeIds || !attestStateTo)
            return;

        if (this.userSettingPayrollCalculationDisableSaveAttestWarning) {
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
    protected saveAttestForEmployees(employeeIds: number[], attestStateTo: AttestStateDTO) {
        this.startSave();
        window.scrollTo(0, 0);

        this.payrollService.saveAttestForEmployees(this.employeeId, employeeIds, attestStateTo.attestStateId, this.currentTimePeriod.timePeriodId, true).then((result) => {
            if (result.success) {
                this.completedSave(null, true, null, false);
                this.showSaveAttestEmployeesResultMessage(result.value, attestStateTo);
                this.loadGroupContent();
                this.refreshTreeForEmployees(employeeIds, true);
            }
            else {
                this.failedSave(result.errorMessage);
            }

        }, error => {
            this.failedSave(error.message);
        });
    }

    protected initSaveAttestForTransactions(option: any) {
        if (!this.isCurrentEmployeeAndPeriodValid())
            return;

        var validTransactions = this.getSelectedTransactions();
        var attestStateTo: AttestStateDTO = this.getAttestState(option.id);
        if (!validTransactions || !attestStateTo)
            return;

        this.payrollService.saveAttestForTransactionsValidation(validTransactions, attestStateTo.attestStateId, this.isMySelf).then((validationResult) => {
            if (validationResult.success && this.userSettingPayrollCalculationDisableSaveAttestWarning) {
                this.saveAttestForTransactions(validationResult.validItems, attestStateTo);
            }
            else {
                var modal = this.notificationService.showDialog(validationResult.title, validationResult.message, TimePayrollUtility.getSaveAttestValidationMessageIcon(validationResult), TimePayrollUtility.getSaveAttestValidationMessageButton(validationResult), SOEMessageBoxSize.Medium, false, validationResult.success, this.termsArray["core.donotshowagain"]);
                if (validationResult.success) {
                    modal.result.then(result => {
                        if (result) {
                            if (result.isChecked)
                                this.saveUserSettingDisableAttestWarning();
                            this.saveAttestForTransactions(validationResult.validItems, attestStateTo);
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
    protected saveAttestForTransactions(validItems: any, attestStateTo: AttestStateDTO) {
        this.startSave();
        window.scrollTo(0, 0);

        this.payrollService.saveAttestForTransactions(validItems, attestStateTo.attestStateId, this.isMySelf).then((result) => {
            if (result.success) {
                this.showSaveAttestResultMessage(result, attestStateTo);
                this.loadEmployeeContent(this.loadedContentWithShowAll, false);
                this.refreshTreeForEmployee(this.currentEmployee.employeeId);
                this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.PayrollLatestAttestStateTo, attestStateTo.attestStateId);
            }
            else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected saveCurrentEmployeeNote() {
        if (this.currentEmployee) {
            this.payrollService.saveEmployeeNote(this.currentEmployee.note, this.currentEmployee.employeeId).then((result) => {
                this.isDirty = false;
                this.currentEmployeeNoteHasChanged = false;
            });
        }
    }

    protected saveUserSettingDisableAutoLoad() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationTreeDisableAutoLoad, this.userSettingPayrollCalculationTreeDisableAutoLoad).then((result) => {
                if (!this.userSettingPayrollCalculationTreeDisableAutoLoad && !this.tree)
                    this.loadTreeDefault(true);
                this.isDirty = false;
            });
        });
    }
    protected saveUserSettingTimeAttestTreeLatestGrouping() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.PayrollCalculationTreeLatestGrouping, this.userSettingPayrollCalculationTreeLatestGrouping).then((result) => {
            this.isDirty = false;
        });
    }
    protected saveUserSettingTimeAttestTreeLatestSorting() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.PayrollCalculationTreeLatestSorting, this.userSettingPayrollCalculationTreeLatestSorting).then((result) => {
            this.isDirty = false;
        });
    }
    protected saveUserSettingPayrollCalculationTreeDoNotShowCalculated() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationTreeDoNotShowCalculated, this.userSettingPayrollCalculationTreeDoNotShowCalculated).then((result) => {
                this.isDirty = false;
            });
        });
    }
    protected saveUserSettingPayrollCalculationTreeWarningFilter() {
        this.$timeout(() => {
            this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.PayrollCalculationTreeWarningFilter, this.userSettingPayrollCalculationTreeWarningFilter).then((result) => {
                this.isDirty = false;
            });
        });
    }
    protected saveUserSettingPayrollCalculationTreeDoShowOnlyWithWarnings() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationTreeDoShowOnlyWithWarnings, this.userSettingPayrollCalculationTreeDoShowOnlyWithWarnings).then((result) => {
                this.isDirty = false;
            });
        });
    }
    protected saveUserSettingDisableAttestWarning() {
        this.userSettingPayrollCalculationDisableSaveAttestWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationDisableApplySaveAttestWarning, this.userSettingPayrollCalculationDisableSaveAttestWarning);
    }
    protected saveUserSettingPayrollCalculationDisableRecalculatePeriodWarning() {
        this.userSettingPayrollCalculationDisableRecalculatePeriodWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationDisableRecalculatePeriodWarning, this.userSettingPayrollCalculationDisableRecalculatePeriodWarning);
    }
    protected saveUserSettingPayrollCalculationDisableRecalculateAccountingWarning() {
        this.userSettingPayrollCalculationDisableRecalculateAccountingWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationDisableRecalculateAccountingWarning, this.userSettingPayrollCalculationDisableRecalculateAccountingWarning);
    }
    protected saveUserSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning() {
        this.userSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationDisableRecalculateExportedEmploymentTaxWarning, this.userSettingPayrollCalculationDisableRecalculateExportedEmploymentTaxWarning);
    }
    protected saveUserSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning() {
        this.userSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning, this.userSettingPayrollCalculationDisableGetUnhandledTransactionsBackwardsWarning);
    }
    protected saveUserSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning() {
        this.userSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.PayrollCalculationDisableGetUnhandledTransactionsForwardsWarning, this.userSettingPayrollCalculationDisableGetUnhandledTransactionsForwardsWarning);
    }

    // HELP-METHODS

    protected hasToolbarFilter(): boolean {
        return this.userSettingPayrollCalculationTreeDisableAutoLoad === true ||
            this.toolbarSelectionIgnoreEmploymentStopDate === true ||
            this.userSettingPayrollCalculationTreeDoNotShowCalculated === true ||
            this.userSettingPayrollCalculationTreeDoShowOnlyWithWarnings === true ||
            this.toolbarSelectionShowOnlyApplyFinalSalary === true ||
            this.toolbarSelectionShowOnlyAppliedFinalSalary === true ||
            this.userSettingPayrollCalculationTreeWarningFilter > 0;
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

    protected hasContent(hasSelected: boolean): boolean {
        if (this.currentTreeViewModeIsGroup) {
            if (this.isCurrentGroupAndPeriodValid() && this.currentGroup && this.currentGroup.payrollEmployeePeriods && this.currentGroup.payrollEmployeePeriods.length > 0 && (!hasSelected || (this.contentGroupSelected && this.contentGroupSelected.length > 0))) {
                return true;
            }
        } else if (this.currentTreeViewModeIsEmployee) {
            if (this.isCurrentEmployeeAndPeriodValid()) {
                if (this.currentContentViewModeIsCalculation && this.contentEmployee && (!hasSelected || (this.contentEmployeeSelected && this.contentEmployeeSelected.length > 0))) {
                    return true;
                }
                if (this.currentContentViewModeIsFixed && this.contentFixed && this.contentFixed.length > 0 && (!hasSelected || (this.contentEmployeeSelected && this.contentEmployeeSelected.length > 0))) {
                    return true;
                }
                if (this.currentContentViewModeIsRetroactive && this.contentRetroactive && this.contentRetroactive.length > 0) {
                    return true;
                }
                if (this.currentContentViewModeIsAdditionAndDeduction && this.contentAdditionDeductionSelected && this.contentAdditionDeductionSelected.length > 0) {
                    return true;
                }
            }
        }
        return false;
    }

    protected hasSelectedTreeItem() {
        if (this.currentTreeViewModeIsGroup) {
            if (this.currentTimePeriod && this.currentGroup) {
                return true;
            }
        }
        else if (this.currentTreeViewModeIsEmployee) {
            if (this.currentTimePeriod && this.currentEmployee) {
                return true;
            }
        }
        return false;
    }

    protected hasLockedTransactions() {
        var result: boolean = false;
        var msg: string = "";

        if (this.currentTreeViewModeIsEmployee) {
            _.forEach(this.contentEmployee, payrollCalculationProduct => {
                _.forEach(payrollCalculationProduct.attestStates, (attestState: AttestStateDTO) => {
                    if (attestState.attestStateId != 0 && (
                        attestState.attestStateId === this.companyPayrollLockedAttestStateId ||
                        attestState.attestStateId === this.companyPayrollApproved1AttestStateId ||
                        attestState.attestStateId === this.companyPayrollApproved2AttestStateId ||
                        attestState.attestStateId === this.companyPayrollExportFileCreatedAttestStateId)) {
                        result = true;
                    }
                });
            });
        } else if (this.currentTreeViewModeIsGroup) {
            if (this.contentGroupSelected) {
                _.forEach(this.contentGroupSelected, employePeriod => {
                    _.forEach(employePeriod.attestStates, (attestState: AttestStateDTO) => {
                        if (attestState.attestStateId != 0 && (
                            attestState.attestStateId === this.companyPayrollLockedAttestStateId ||
                            attestState.attestStateId === this.companyPayrollApproved1AttestStateId ||
                            attestState.attestStateId === this.companyPayrollApproved2AttestStateId ||
                            attestState.attestStateId === this.companyPayrollExportFileCreatedAttestStateId)) {
                            result = true;
                            if (msg.length != 0)
                                msg += ", ";

                            msg += employePeriod.employeeName;
                        }
                    });
                });
            }
        }

        return { result, msg };
    }

    protected hasLockPeriodInvalidTransactionStates() {
        var msg: string = "";
        var result: boolean = false;

        if (this.currentTreeViewModeIsEmployee) {
            _.forEach(this.contentEmployee, payrollCalculationProduct => {
                _.forEach(payrollCalculationProduct.attestPayrollTransactions, (transaction: AttestPayrollTransactionDTO) => {
                    if (!transaction.isScheduleTransaction && transaction.attestStateId != this.companyPayrollResultingAttestStateId) {
                        result = true;
                    }
                });
            });
        } else if (this.currentTreeViewModeIsGroup) {
            if (this.contentGroupSelected) {
                _.forEach(this.contentGroupSelected, employePeriod => {
                    _.forEach(employePeriod.attestStates, (attestState: AttestStateDTO) => {
                        if (attestState.attestStateId != this.companyPayrollResultingAttestStateId) {
                            result = true;
                            if (msg.length != 0)
                                msg += ", ";

                            msg += employePeriod.employeeName;
                        }
                    });
                });
            }
        }

        return { result, msg };
    }

    protected hasUnLockPeriodInvalidTransactionStates(): boolean {
        var result: boolean = false;

        if (this.currentTreeViewModeIsEmployee) {
            var transactions: AttestPayrollTransactionDTO[] = [];
            _.forEach(this.contentEmployee, payrollCalculationProduct => {
                _.forEach(payrollCalculationProduct.attestPayrollTransactions, (transaction:
                    AttestPayrollTransactionDTO) => {
                    if (!transaction.isScheduleTransaction) {
                        transactions.push(transaction);
                    }
                });
            });
            result = _.filter(transactions, x => x.attestStateId === this.companyPayrollLockedAttestStateId).length === 0;
        } else if (this.currentTreeViewModeIsGroup) {
            var items: AttestStateDTO[] = [];
            if (this.contentGroupSelected) {
                _.forEach(this.contentGroupSelected, employePeriod => {
                    _.forEach(employePeriod.attestStates, (attestState: AttestStateDTO) => {
                        items.push(attestState);
                    });
                });
            }
            result = _.filter(items, x => x.attestStateId === this.companyPayrollLockedAttestStateId).length === 0;
        }

        this.isDirty = false;
        return result;
    }

    protected hasWatchLogs(): boolean {
        return this.currentWatchLogs && this.currentWatchLogs.length > 0;
    }

    protected isEmployeeNodeActive(employeeNode: TimeEmployeeTreeNodeDTO): boolean {
        return employeeNode && this.currentEmployee && employeeNode.employeeId == this.currentEmployee.employeeId;
    }

    protected isCurrentViewModeValid() {
        if (this.currentTreeViewModeIsGroup) {
            if (this.isCurrentGroupAndPeriodValid) {
                return true;
            }
        }
        else if (this.currentTreeViewModeIsEmployee) {
            if (this.isCurrentEmployeeAndPeriodValid) {
                return true;
            }
        }
        return false;
    }

    protected isCurrentGroupAndPeriodValid() {
        if (!this.currentGroup || this.currentGroup.id === 0 || !this.currentTimePeriod || this.currentTimePeriodId === 0)
            return false;
        else
            return true;
    }

    protected isCurrentEmployeeAndPeriodValid() {
        if (!this.currentEmployee || this.currentEmployee.employeeId === 0 || !this.currentTimePeriod || this.currentTimePeriodId === 0)
            return false;
        else
            return true;
    }

    protected isAttestDisabled(): boolean {
        return !this.hasContent(true);
    }

    protected isCalculationFunctionsDisabled(): boolean {
        return !this.hasContent(false);
    }

    protected isSaveFixedPayrollRowsDisabled(): boolean {
        return !this.fixedPayrollRowsModifyPermission;
    }

    protected isPermissionsLoaded() {
        return (this.modifyPermissionsLoaded && this.readPermissionsLoaded);
    }

    protected getCacheKey(flushCache?: boolean) {
        if (this.tree && !flushCache)
            return this.tree.cacheKey;
        else
            return Constants.WEBAPI_STRING_EMPTY;
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

    protected getSelectedTransactions(): AttestPayrollTransactionDTO[] {
        var transactionItems: AttestPayrollTransactionDTO[] = [];
        var selectedRows = this.contentEmployeeSelected ? this.contentEmployeeSelected : [];
        _.forEach(selectedRows, (row: any) => {
            _.forEach(row.attestPayrollTransactions, (transactionItem: any) => {
                if (transactionItem.attestStateId)
                    transactionItems.push(transactionItem);
            });
        });
        return transactionItems;
    }

    protected showSaveAttestResultMessage(result: any, attestStateTo: AttestStateDTO) {
        var skipDialog: boolean = true;
        var message: string = '';

        if (result) {
            message = this.termsArray["time.time.attest.saveattestresultvalid"].format(result.integerValue.toString(), StringUtility.nullToEmpty(attestStateTo.name));
        }
        if (result && (!result.integerValue || result.integerValue2)) { //show if valid is zero or invalid is over zero
            message += "<br />";
            message += this.termsArray["time.time.attest.saveattestresultinvalid"].format(result.integerValue2.toString(), StringUtility.nullToEmpty(attestStateTo.name));
            skipDialog = false;
        }

        this.completedSave(null, skipDialog, message);
    }

    protected showSaveAttestEmployeesResultMessage(result: EmployeesAttestResult, attestStateTo: AttestStateDTO) {
        if (!result || result.success || result.attestStateToId != attestStateTo.attestStateId)
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

    protected getAttestState(attestStateId: number): AttestStateDTO {
        return (_.filter(this.currentAttestStates, { attestStateId: attestStateId }))[0];
    }

    protected showUnLockPeriodButton(): boolean {
        if (!this.currentTreeViewModeIsEmployee || !this.currentEmployeeTimePeriod)
            return false;

        return this.currentEmployeeTimePeriod && this.currentEmployeeTimePeriod.status == SoeEmployeeTimePeriodStatus.Locked;
    }

    protected showLockPeriodButton(): boolean {

        if (!this.currentTreeViewModeIsEmployee || !this.currentEmployeeTimePeriod)
            return false;

        return this.currentEmployeeTimePeriod && this.currentEmployeeTimePeriod.status == SoeEmployeeTimePeriodStatus.Open;
    }

    protected showWatchLogs() {
        if (!this.hasWatchLogs())
            return;

        var message: string = "";
        _.forEach(this.currentWatchLogs, (watchLog: any) => {

            message += watchLog;
        });

        this.notificationService.showDialog(this.termsArray["time.payroll.payrollcalculation.recalculateperiodlogs"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    protected showFinalSalaryCreated(): boolean {
        return this.hasSelectedTreeItem &&
               this.currentEmployee && this.currentEmployee.finalSalaryAppliedTimePeriodId && this.currentEmployee.finalSalaryStatus !== SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually &&
               this.currentEmployeeTimePeriod && this.currentEmployeeTimePeriod.timePeriodId == this.currentEmployee.finalSalaryAppliedTimePeriodId;
    }

    protected showFinalSalaryCreatedManually(): boolean {
        return this.currentEmployee && this.currentEmployee.finalSalaryStatus === SoeEmploymentFinalSalaryStatus.AppliedFinalSalaryManually &&
              !this.showFinalSalaryCreated();
    }

    protected setContentViewModeFunctions() {
        if (!this.isPermissionsLoaded())
            return;

        this.contentViewModeFunctions.length = 0;

        this.contentViewModeFunctions.push({ id: PayrollCalculationContentViewMode.Calculation, name: this.termsArray["time.payroll.payrollcalculation.calculation"] });
        if (this.fixedPayrollRowsReadPermission)
            this.contentViewModeFunctions.push({ id: PayrollCalculationContentViewMode.Fixed, name: this.termsArray["time.payroll.payrollcalculation.fixedpayrollrows"] });
        if (this.retroactivePayrollReadPermission)
            this.contentViewModeFunctions.push({ id: PayrollCalculationContentViewMode.Retroactive, name: this.termsArray["time.payroll.retroactive.payroll"] });

        this.contentViewModeFunctions.push({ id: PayrollCalculationContentViewMode.AdditionAndDeduction, name: this.termsArray["time.time.attest.additiondeduction"] });
    }

    protected setCalculationFunctions() {
        if (!this.isPermissionsLoaded())
            return;
        if (!this.modifyPermission)
            return;

        this.recalculateOptions.length = 0;
        this.recalculateOptions.push({ id: PayrollCalculationRecalculateFunctions.Recalculate, name: this.termsArray["time.payroll.payrollcalculation.recalculate"] });
        this.recalculateOptions.push({ id: PayrollCalculationRecalculateFunctions.RecalculateIncPrelTransaction, name: this.termsArray["time.payroll.payrollcalculation.recalculateincprel"] });
        this.recalculateOptions.push({ id: PayrollCalculationRecalculateFunctions.RecalculateAccounting, name: this.termsArray["time.payroll.payrollcalculation.recalculateaccounting"] });
        this.selectedRecalculateOption = { id: PayrollCalculationRecalculateFunctions.Recalculate, name: this.termsArray["time.payroll.payrollcalculation.recalculate"] };

        this.calculationOptions.length = 0;

        if (this.currentTreeViewModeIsGroup) {
            this.calculationOptions.push({ id: PayrollCalculationFunctions.LockPeriod, name: this.termsArray["time.payroll.payrollcalculation.lockperiod"] });
            this.calculationOptions.push({ id: PayrollCalculationFunctions.UnLockPeriod, name: this.termsArray["time.payroll.payrollcalculation.unlockperiod"] });
            if (this.validateCreateFinalSalaries())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.CreateFinalSalary, name: this.termsArray["time.payroll.payrollcalculation.finalsalary"] });
            if (this.validateDeleteFinalSalaries())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.DeleteFinalSalary, name: this.termsArray["time.payroll.payrollcalculation.deletefinalsalary"] });           

            this.calculationOptions.push({ id: PayrollCalculationFunctions.RunPayrollControll, name: this.termsArray["time.payroll.payrollcalculation.warnings.runcontroll"] });
        }

        if (this.currentTreeViewModeIsEmployee && this.currentContentViewModeIsCalculation) {
            if (this.showLockPeriodButton())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.LockPeriod, name: this.termsArray["time.payroll.payrollcalculation.lockperiod"] });
            if (this.showUnLockPeriodButton())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.UnLockPeriod, name: this.termsArray["time.payroll.payrollcalculation.unlockperiod"] });
            if (this.validateCreateFinalSalary())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.CreateFinalSalary, name: this.termsArray["time.payroll.payrollcalculation.finalsalary"] });
            if (this.validateDeleteFinalSalary())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.DeleteFinalSalary, name: this.termsArray["time.payroll.payrollcalculation.deletefinalsalary"] });
            this.calculationOptions.push({ id: PayrollCalculationFunctions.GetUnhandledTransactionsBackwards, name: this.termsArray["time.payroll.payrollcalculation.getunhandledtransactionsbackwards"] });
            //if (this.currentTimePeriod && this.currentTimePeriod.startDate.date().isBeforeOnDay(this.currentTimePeriod.payrollStartDate.date()) && this.currentTimePeriod.stopDate.date().isBeforeOnDay(this.currentTimePeriod.payrollStopDate.date()))
            this.calculationOptions.push({ id: PayrollCalculationFunctions.GetUnhandledTransactionsForward, name: this.termsArray["time.payroll.payrollcalculation.getunhandledtransactionsforward"] });
            if (this.validateClearPayrollCalculation())
                this.calculationOptions.push({ id: PayrollCalculationFunctions.ClearPayrollCalculation, name: this.termsArray["time.payroll.payrollcalculation.clearcalculation"] });
        }
        
        if (this.currentTreeViewModeIsGroup && this.sendAttestReminderPermission) {
            this.calculationOptions.push({ id: PayrollCalculationFunctions.AttestReminder, name: this.termsArray["time.time.attest.sendattestreminder"], hidden: () => { return !this.hasContent(false) } });
        }
    }

    protected setReloadOptions() {
        if (!this.isPermissionsLoaded())
            return;
        if (!this.modifyPermission)
            return;

        this.reloadOptions.length = 0;

        this.reloadOptions.push({ id: PayrollCalculationReloadFunctions.Reload, name: this.termsArray["core.reload_data"] });
        this.reloadOptions.push({ id: PayrollCalculationReloadFunctions.ReloadDetailed, name: this.termsArray["time.payroll.payrollcalculation.reloaddetailed"] });

        this.selectedReloadOption = { id: PayrollCalculationReloadFunctions.Reload, name: this.termsArray["core.reload_data"] };
    }

    protected setExpandedGroups(expandedGroupIds: number[]) {
        if (!this.tree || !this.tree.groupNodes || this.tree.groupNodes.length === 0 || !expandedGroupIds || expandedGroupIds.length === 0)
            return;

        _.forEach(expandedGroupIds, (id: number) => {
            var groupNode = _.filter(this.tree.groupNodes, n => n.id == id)[0];
            if (groupNode)
                groupNode.expanded = true;
        });
    }

    protected expandAllGroups(expanded: boolean) {
        if (!this.tree)
            return;

        _.forEach(this.tree.groupNodes, (groupNode: any) => {
            if (groupNode.employeeNodes && groupNode.employeeNodes.length)
                groupNode.expanded = expanded;
        });
    }

    protected expandFirstGroup() {
        if (!this.tree)
            return;

        this.expandAllGroups(false);
        if (this.tree && this.tree.groupNodes.length > 0 && this.tree.groupNodes.length == 1)
            this.tree.groupNodes[0].expanded = true;
    }

    protected doubleClick() {
        this.showRecalculateExportedEmploymentTax = true;
        this.setupToolBar();
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

    protected validateRecalculatePeriod(): boolean {
        return this.hasSelectedTreeItem();
    }

    protected validateRecalculateAccounting(): boolean {
        return this.hasSelectedTreeItem();
    }

    protected validateRecalculateExportedEmploymentTax(): boolean {
        return this.hasSelectedTreeItem();
    }

    protected validateLockPeriod(): boolean {
        return this.hasSelectedTreeItem();
    }

    protected validateUnLockPeriod(): boolean {
        return this.hasSelectedTreeItem();
    }

    protected validateCreateFinalSalaries(): boolean {
        if (this.currentTreeViewModeIsGroup && this.toolbarSelectionShowOnlyApplyFinalSalary)
            return true;
        return false;
    }
    protected validateCreateFinalSalary(): boolean {
        if (this.currentTreeViewModeIsEmployee && this.hasSelectedTreeItem() && this.currentEmployee.finalSalaryEndDate && this.currentEmployee.finalSalaryEndDate <= this.currentTimePeriod.payrollStopDate)
            return true;
        return false;
    }

    protected validateDeleteFinalSalaries(): boolean {
        if (this.currentTreeViewModeIsGroup && this.toolbarSelectionShowOnlyAppliedFinalSalary)
            return true;
        return false;
    }
    protected validateDeleteFinalSalary(): boolean {
        if (this.showFinalSalaryCreated() && (!this.currentEmployeeTimePeriod || this.currentEmployeeTimePeriod.status === SoeEmployeeTimePeriodStatus.Open))
            return true;
        return false;
    }

    protected validateClearPayrollCalculation(): boolean {
        if (this.currentTreeViewModeIsEmployee && this.hasSelectedTreeItem() && (this.currentEmployeeTimePeriod && this.currentEmployeeTimePeriod.status === SoeEmployeeTimePeriodStatus.Open))
            return true;
        return false;
    }

    protected validGetUnhandledTransactionsBackwards(): boolean {
        if (this.currentTimePeriod != null && this.currentEmployee != null)
            return true;
        return false;
    }

    protected validGetUnhandledTransactionsForward(): boolean {
        if (this.currentTimePeriod != null && this.currentTimePeriod.startDate != this.currentTimePeriod.payrollStartDate && this.currentTimePeriod.stopDate != this.currentTimePeriod.payrollStopDate && this.currentEmployee != null)
            return true;
        return false;
    }

    protected validate() {
    }
}
