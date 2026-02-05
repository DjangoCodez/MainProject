import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, CompanySettingType, TermGroup, TermGroup_TimeAccumulatorCompareModel } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IEmployeeService } from "../EmployeeService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { SelectionCollection } from "../../../Core/RightMenu/ReportMenu/SelectionCollection";
import { EmployeeSelectionDTO } from "../../../Common/Models/ReportDataSelectionDTO";
import { IDateRangeSelectionDTO, IEmployeeSelectionDTO, IIdListSelectionDTO } from "../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { EmployeeAccumulatorDTO } from "../../../Common/Models/EmployeeAccumulatorDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {


    //Selection Bindings
    private selections: SelectionCollection;
    private userSelectionInput: EmployeeSelectionDTO;

    //Search values
    private employeeIds: number[] = [];
    private accumulatorIds: number[] = [];
    private fromDate: Date;
    private toDate: Date;
    private ownLimitMin: number;
    private ownLimitMax: number;
    private onlyOutsideLimits: boolean = false;
    private showEmployessWithZeroValue: boolean = false;

    // Modal
    private modal: any;
    private modalInstance: any;

    // Terms:
    private terms: any;
    private title: string;

    // Company settings    
    private useAccountsHierarchy: boolean = false;

    // Data    
    private allRows: EmployeeAccumulatorDTO[];
    private currentRows: EmployeeAccumulatorDTO[] = [];

    // Lookups
    private compareModels: any[];

    //Header
    private gridHeaderComponentUrl: any;

    // Footer
    private gridFooterComponentUrl: any;

    // Flags
    private isSearching: boolean = false;
    private hasSearched: boolean = false;
    private selectionIsOpen: boolean = true;

    // Current values
    private currentRangeType: number = 0;
    private currentCompareModel: TermGroup_TimeAccumulatorCompareModel = TermGroup_TimeAccumulatorCompareModel.SelectedRange;
    private currentSelectedRangeDescription: string;
    private currentAccTodayDescription: string;

    get ownLimitMinFormatted(): string {
        return this.ownLimitMin || this.ownLimitMin === 0 ? CalendarUtility.minutesToTimeSpan(this.ownLimitMin) : null;
    }
    set ownLimitMinFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.ownLimitMin = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.ownLimitMin = null;
        }
    }

    get ownLimitMaxFormatted(): string {
        return this.ownLimitMax || this.ownLimitMax === 0 ? CalendarUtility.minutesToTimeSpan(this.ownLimitMax) : null;
    }
    set ownLimitMaxFormatted(time: string) {
        if (time) {
            var span = CalendarUtility.parseTimeSpan(time);
            this.ownLimitMax = CalendarUtility.timeSpanToMinutes(span);
        } else {
            this.ownLimitMax = null;
        }
    }

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Time.Employee.Accumulators", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;
        this.gridHeaderComponentUrl = urlHelperService.getGlobalUrl("Time/Employee/Accumulators/Views/gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadModifyPermissions())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadCompareModels())
            .onSetUpGrid(() => this.setupGrid());
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;

        this.selections = new SelectionCollection();
        this.fromDate = new Date();
        this.toDate = new Date();

        this.flowHandler.start([
            { feature: Feature.Time_Employee_Accumulators, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_Accumulators].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Accumulators].modifyPermission;
    }

    public setupGrid() {
        var headerColumnOptions = { enableHiding: true };

        this.doubleClickToEdit = false;
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.useGrouping(true, true, { keepColumnsAfterGroup: true, selectChildren: false });
        this.gridAg.options.groupHideOpenParents = true;
        this.gridAg.options.setMinRowsToShow(100);

        this.gridAg.addColumnText("employeeNrAndName", this.terms["common.employee"], null, true, { enableRowGrouping: true });
        this.gridAg.addColumnText("accumulatorName", this.terms["time.employee.accumulators.accumulator"], 180, true, { enableRowGrouping: true });
        this.gridAg.addColumnNumber("accumulatorAmount", this.terms["time.employee.accumulators.amount"], 80, { decimals: 2, enableRowGrouping: true, enableHiding: true/*, aggFuncOnGrouping: 'sum'*/ });

        var colHeaderAccValues = this.gridAg.options.addColumnHeader("accvalues", this.terms["time.employee.accumulators.value"], headerColumnOptions);
        colHeaderAccValues.marryChildren = true;
        this.gridAg.addColumnTime("accumulatorPeriodValue", this.terms["time.employee.accumulators.periodvalue"], 70, { toolTipField: "accumulatorPeriodDates", minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderAccValues);
        this.gridAg.addColumnTime("accumulatorAccTodayValue", this.terms["time.employee.accumulators.todayvalue"], 70, { toolTipField: "accumulatorAccTodayDates", minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderAccValues);

        var colHeaderAccRule = this.gridAg.options.addColumnHeader("accrules", this.terms["time.employee.accumulator.rules"], headerColumnOptions);
        colHeaderAccRule.marryChildren = true;
        this.gridAg.addColumnTime("accumulatorRuleMinWarningMinutes", this.terms["time.employee.accumulators.minwarning"], 70, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderAccRule);
        this.gridAg.addColumnTime("accumulatorRuleMinMinutes", this.terms["time.employee.accumulators.min"], 70, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderAccRule);
        this.gridAg.addColumnTime("accumulatorRuleMaxWarningMinutes", this.terms["time.employee.accumulators.maxwarning"], 70, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderAccRule);
        this.gridAg.addColumnTime("accumulatorRuleMaxMinutes", this.terms["time.employee.accumulators.max"], 70, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderAccRule);
        this.gridAg.addColumnText("accumulatorStatusName", this.terms["time.employee.accumulators.status"], 200, true, { enableRowGrouping: true, cellClassRules: { "errorColor": (row) => { return row && row.data && row.data.accumulatorShowError }, "warningColor": (row) => { return row && row.data && row.data.accumulatorShowWarning } } }, colHeaderAccRule);
        this.gridAg.addColumnTime("accumulatorDiff", this.terms["time.employee.accumulators.diff"], 50, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true, cellClassRules: { "errorColor": (row) => { return row && row.data && row.data.accumulatorShowError }, "warningColor": (row) => { return row && row.data && row.data.accumulatorShowWarning } } }, colHeaderAccRule);

        var colHeaderOwnRule = this.gridAg.options.addColumnHeader("ownrules", this.terms["time.employee.accumulator.ownrules"], headerColumnOptions);
        colHeaderOwnRule.marryChildren = true;
        this.gridAg.addColumnTime("ownLimitMin", this.terms["time.employee.accumulators.min"], 70, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderOwnRule);
        this.gridAg.addColumnTime("ownLimitMax", this.terms["time.employee.accumulators.max"], 70, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true }, colHeaderOwnRule);
        this.gridAg.addColumnText("ownLimitStatusName", this.terms["time.employee.accumulators.status"], 200, true, { enableRowGrouping: true, cellClassRules: { "errorColor": (row) => { return row && row.data && row.data.ownLimitShowError } } }, colHeaderOwnRule);
        this.gridAg.addColumnTime("ownLimitDiff", this.terms["time.employee.accumulators.diff"], 50, { minutesToTimeSpan: true, enableRowGrouping: true, enableHiding: true, /*aggFuncOnGrouping: 'sumTimeSpan',*/ cellClassRules: { "errorColor": (row) => { return row && row.data && row.data.ownLimitShowError } } }, colHeaderOwnRule);

        this.gridAg.finalizeInitGrid("time.employee.accumulators", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.employee",
            "time.employee.employee.employeenrshort",
            "time.employee.accumulators.accumulator",
            "time.employee.accumulators.value",
            "time.employee.accumulators.amount",
            "time.time.timeaccumulator.employeegrouprule.minminuteswarning",
            "time.time.timeaccumulator.employeegrouprule.minminutes",
            "time.time.timeaccumulator.employeegrouprule.maxminuteswarning",
            "time.time.timeaccumulator.employeegrouprule.maxminutes",
            "time.employee.accumulators.diff",
            "time.employee.accumulators.status",
            "time.employee.accumulators.min",
            "time.employee.accumulators.max",
            "time.employee.accumulator.rules",
            "time.employee.accumulator.ownrules",
            "time.employee.accumulators.minwarning",
            "time.employee.accumulators.maxwarning",
            "time.employee.accumulators.todayvalue",
            "time.employee.accumulators.periodvalue",
            "time.employee.accumulators.selectedrangedescription",
            "time.employee.accumulators.acctodaydescription",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        return this.coreService.hasModifyPermissions(features).then((x) => {

        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadCompareModels(): ng.IPromise<any> {
        this.compareModels = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeAccumulatorCompareModel, false, true).then((x) => {
            this.compareModels = x;
        });
    }

    public search() {
        this.selectionIsOpen = false;
        this.isSearching = true;
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getEmployeeAccumulators(this.fromDate, this.toDate, this.employeeIds, this.accumulatorIds, this.currentRangeType, this.currentCompareModel, this.ownLimitMin, this.ownLimitMax).then(x => {
                this.allRows = x.map(s => {
                    var obj = new EmployeeAccumulatorDTO();
                    angular.extend(obj, s);
                    return obj;
                });

                this.updateGridData();
                this.isSearching = false;
                this.hasSearched = true;
            });
        }]);
    }

    // EVENTS

    public onDateRangeSelectionUpdated(selection: IDateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, selection);
        this.fromDate = selection.from;
        this.toDate = selection.to;
        if (!this.toDate)
            this.toDate = this.fromDate;

        if (this.fromDate && this.toDate && this.terms) {
            this.currentSelectedRangeDescription = this.terms["time.employee.accumulators.selectedrangedescription"].format(this.fromDate.toFormattedDate(), this.toDate.toFormattedDate());
            this.currentAccTodayDescription = this.terms["time.employee.accumulators.acctodaydescription"].format(this.toDate.toFormattedDate());
        }
        this.currentRangeType = selection.id;
    }

    public onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
        this.employeeIds = selection.employeeIds;
    }

    public onTimeAccumulatorSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_TIME_ACCUMULATORS, selection);

        this.accumulatorIds = selection.ids;
    }

    private onlyOutsideLimitsChanged() {
        this.$timeout(() => {
            this.updateGridData();
        });
    }

    private showEmployessWithZeroValueChanged() {
        this.$timeout(() => {
            this.updateGridData();
        });
    }


    // HELP-METHDS

    private updateGridData() {
        this.currentRows = [];

        if (this.allRows && this.allRows.length > 0) {
            this.selectionIsOpen = false;
            _.forEach(this.allRows, (row: EmployeeAccumulatorDTO) => {
                if (row.accumulatorId < 0) {
                    row.accumulatorAccTodayValue = row.accumulatorAccTodayValue * 60;
                    row.accumulatorPeriodValue = row.accumulatorPeriodValue * 60;
                }

                if (this.onlyOutsideLimits) {
                    if (row.accumulatorShowError === true || row.ownLimitShowError === true)
                        this.currentRows.push(row)
                }
                else if (!this.showEmployessWithZeroValue) {
                    if (row.accumulatorAccTodayValue !== 0 || row.accumulatorPeriodValue !== 0)
                        this.currentRows.push(row);
                }
                else {
                    this.currentRows.push(row)
                }
            });
        }

        this.isDirty = false;
        this.gridAg.setData(this.currentRows);
    }

}