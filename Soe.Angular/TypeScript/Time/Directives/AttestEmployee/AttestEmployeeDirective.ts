import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { TimeAttestMode, Feature, CompanySettingType, SoeTimeCodeType, SoeEntityState, TermGroup_TimeReportType } from "../../../Util/CommonEnumerations";
import { AttestEmployeeDayDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { IEmployeeTimeCodeDTO, ITimeDeviationCauseDTO } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../../Time/TimeService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, ProjectTimeRegistrationType } from "../../../Util/Enumerations";
import { Constants } from "../../../Util/Constants";
import { ProjectTimeBlockDTO, ProjectTimeBlockSaveDTO } from "../../../Common/Models/ProjectDTO";
import { IColumnAggregate, IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { StringUtility } from "../../../Util/StringUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { EditTimeGridController } from "../../../Common/Directives/TimeProjectReport/EditTimeGridController";
import { RowDetailDialogController } from "./Dialogs/RowDetailDialog/RowDetailDialogController";

export class AttestEmployeeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/AttestEmployee.html'),
            scope: {
                registerControl: '&',
                progressBusy: '=?',
                isReadonly: '=?',
                attestEmployeeDays: '=',
                showGrouping: '=?',
                hideDetails: '=?',
                hideDaysWithoutSchedule: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: AttestEmployeeController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class AttestEmployeeController extends GridControllerBaseAg {

    private registerControl: Function;
    private attestMode: TimeAttestMode;
    private isReadonly: boolean;
    private terms: any;
    private attestEmployeeDays: AttestEmployeeDayDTO[];
    private filteredAttestEmployeeDays: AttestEmployeeDayDTO[];
    private hideDetails: boolean;
    private hideDaysWithoutSchedule = false;
    private modalInstance: any;
    private isErp: boolean = false;
    private get isMyTime(): boolean {
        return this.attestMode == TimeAttestMode.TimeUser;
    }

    // Init parameters
    private showSortButtons: boolean;
    private showGrouping = true;
    private _collapseAllRowGroups: boolean;
    public get collapseAllRowGroups(): boolean {
        return this._collapseAllRowGroups;
    }
    public set collapseAllRowGroups(value: boolean) {
        this._collapseAllRowGroups = value
        this.setRowGroupExpension();
    }
    protected gridId: string;

    // Converted init parameters
    private showSortButtonsValue: boolean;

    // Lookups
    private employeeTimeCodeId: number;
    private employeeTimeCode: IEmployeeTimeCodeDTO;
    private employeeTimeCodes: IEmployeeTimeCodeDTO[] = [];
    private timeCodesDict: any[] = [];
    private timeDeviationCauses: ITimeDeviationCauseDTO[] = [];
    private allProjects: any[] = [];
    private allProjectsAndInvoices: any[] = [];
    private allOrders: any[] = [];
    private filteredProjectsDict: any[] = [];
    private filteredOrdersDict: any[] = [];
    private projectInvoices: any = [];

    // Permissions
    private invoiceTimePermission: boolean = false;
    private workTimePermission: boolean = false;
    private modifyOtherEmployeesPermission: boolean = false;

    // Company settings
    private defaultTimeCodeId: number;

    //ui stuff
    private lastNavigation: { row: any, column: any };
    private gridHeightStyle;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        protected $uibModal,
        protected coreService: ICoreService,
        private timeService: ITimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.AttestEmployee", "time.time.attest.attestemployee", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants, null, null, null, null, null, null, true);
        this.modalInstance = $uibModal;
    }

    // INIT

    public $onInit() {
        //Config parameters
        this.attestMode = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;
        this.showSortButtonsValue = <any>this.showSortButtons === 'true';
        this.initGrid();
        if (this.registerControl)
            this.registerControl({ control: this });
    }

    private initGrid() {
        this.setGridName(false);
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.ignoreResizeToFit = true;

        var gridOptions = (this.soeGridOptions as any).gridOptions;
        gridOptions.suppressHorizontalScroll = false;
        gridOptions.getRowClass = function (params) {
            if (params.data.dayOfWeekNr === 0) {
                return 'underline';
            }
        }

        this.$scope.$on('focusRow', (e, a) => {
            this.soeGridOptions.startEditingCell(a.row - 1, 0);
        });
    }

    private setGridName(restore: boolean) {
        this.soeGridOptions.clearColumnDefs();
        super.setName(this.getGridName());
        if (restore) {
            this.restoreState(true);
            this.soeGridOptions.refreshGrid();
        }
    }

    private getGridName(): string {
        if (this.isMyTime && !this.isErp)
            return "Common.Directives.AttestMyTime";
        else if (this.isMyTime && this.isErp)
            return "Common.Directives.AttestMyTime.ERP";
        else if (!this.isMyTime && this.isErp)
            return "Common.Directives.AttestEmployee.ERP";
        else
            return "Common.Directives.AttestEmployee";
    }

    // SETUP

    public setupGrid() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        this.soeGridOptions.subscribe(events);

        this.startLoad();
        if (this.attestMode === TimeAttestMode.Time || this.attestMode === TimeAttestMode.TimeUser) {
            this.$q.all([
                this.loadTerms().then(() => {
                    this.setupGridColumns();
                    this.setupGridFooter();
                    this.finalizeGrid();
                }),
                this.loadCompanySettings()]).then(() => {
                    this.gridAndDataIsReady();
                });
        }
        else {
            this.$q.all([
                this.loadTerms().then(() => {
                    this.setupGridColumns();
                    this.setupGridFooter();
                }),
                this.loadModifyPermissions(),
                this.loadReadOnlyPermissions(),
                this.loadCompanySettings(),
                this.loadTimeCodes(),
                this.loadTimeDeviationCauses(),
                this.loadEmployeeForProject()]).then(() => {
                    this.$q.all([this.loadEmployeesForProject()]).then(() => {
                        this.loadProjectInvoices();
                        this.gridAndDataIsReady();
                    })
                })
        }
    }

    //Adjust SettingManager.GetValidColumnSettingsForTimeAttest() and enum AgGridTimeAttest when changing or adding columns here 
    private setupGridColumns() {
        //Info columns: Fixed - can hide but mot move
        var dayColumnOptions = { suppressMovable: true, enableHiding: true, clearZero: true, alignLeft: true, toolTipField: "sumGrossSalaryAbsenceText" };
        var dateColumnOptions = { suppressMovable: true, enableHiding: true, toolTipField: "sumGrossSalaryAbsenceText", cellClassRules: { "excelDate": () => true } };
        var dayNameColumnOptions = { suppressMovable: true, enableHiding: true, toolTipField: "sumGrossSalaryAbsenceText" };
        var weekNrColumnOptions = { suppressMovable: true, enableHiding: true, toolTipField: "sumGrossSalaryAbsenceText" };
        var attestStateColorColumnOptions = { suppressMovable: true, enableHiding: false, shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" };
        var attestStateNameColumnOptions = { suppressMovable: true, enableHiding: true, clearZero: true, toolTipField: "sumGrossSalaryAbsenceText" };
        var workedInsideScheduleColorColumnOptions = { suppressMovable: true, enableHiding: false, shape: Constants.SHAPE_RECTANGLE, color: "#A9D18E", showIconField: "hasWorkedInsideSchedule", shapeWidth: 5, toolTip: this.terms["time.time.attest.workedinsideschedulecolor"], suppressExport: true };
        var workedOutsideScheduleColorColumnOptions = { suppressMovable: true, enableHiding: false, shape: Constants.SHAPE_RECTANGLE, color: "#3333FF", showIconField: "hasWorkedOutsideSchedule", shapeWidth: 5, toolTip: this.terms["time.time.attest.workedoutsideschedulecolor"], suppressExport: true };
        var absenceTimeColorColumnOptions = { suppressMovable: true, enableHiding: false, shape: Constants.SHAPE_RECTANGLE, color: "#FC0516", showIconField: "hasAbsenceTime", shapeWidth: 5, toolTip: this.terms["time.time.attest.absencetimecolor"], suppressExport: true };
        var standbyTimeColorColumnOptions = { suppressMovable: true, enableHiding: false, shape: Constants.SHAPE_RECTANGLE, color: "#FFFF00", showIconField: "hasStandbyTime", shapeWidth: 5, toolTip: this.terms["time.time.attest.standbytimecolor"], suppressExport: true };
        var expenseColorColumnOptions = { suppressMovable: true, enableHiding: false, shape: Constants.SHAPE_RECTANGLE, color: "#8332a8", showIconField: "hasExpense", shapeWidth: 5, toolTip: this.terms["time.time.attest.expensecolor"], suppressExport: true };

        //Data columns: Optional - can hide and move
        var headerColumnOptions = { enableHiding: true };
        var timeColumnOptions = { enableHiding: true, hideDays: true, clearZero: true, alignLeft: false, cellClassRules: { "excelTime": () => true } };

        var colDefDay = super.addColumnNumber("day", this.terms["common.day"], 66, dayColumnOptions);
        if (colDefDay) {
            colDefDay.suppressMovable = true;
            if (!this.hideDetails)
                colDefDay.cellRenderer = 'agGroupCellRenderer';
            colDefDay.cellClassRules = {
                "lightCoralRow": (params) => params.data.isWholedayAbsence === true,
                "warningRow": (params) => params.data.isGeneratingTransactions === true,
            };
        }
        var colDefDate = super.addColumnDate("date", this.terms["common.date"], 123, true, null, null, dateColumnOptions)
        if (colDefDate) {
            colDefDay.suppressMovable = true;
            if (!this.hideDetails)
                colDefDate.cellRenderer = 'agGroupCellRenderer';
            colDefDate.cellClassRules = {
                "indiscreet": (params) => (params.data.date).isSameDayAs((new Date()).beginningOfDay()),
                "lightCoralRow": (params) => params.data.isWholedayAbsence === true,
                "warningRow": (params) => params.data.isGeneratingTransactions === true,
            };
        }
        var colDefDayName = super.addColumnText("dayName", this.terms["time.time.attest.dayname"], 100, dayNameColumnOptions)
        if (colDefDayName) {
            colDefDayName.cellRenderer = function (params) {
                if (params.data['holidayName'])
                    return '<span style="color:red">' + params.data['holidayName'] + '</span';
                else
                    return '<span>' + params.value + '</span';
            };
            colDefDayName.cellClassRules = {
                "indiscreet": (params) => (params.data.date).isSameDayAs((new Date()).beginningOfDay()),
                "lightCoralRow": (params) => params.data.isWholedayAbsence === true,
                "warningRow": (params) => params.data.isGeneratingTransactions === true,
            };
        }
        var colDefWeekNr = super.addColumnText("weekNr", this.terms["common.week"], 40, weekNrColumnOptions);
        if (colDefWeekNr) {
            colDefWeekNr.cellClassRules = {
                "lightCoralRow": (params) => params.data.isWholedayAbsence === true,
                "warningRow": (params) => params.data.isGeneratingTransactions === true,
            };
        }
        var colDefAttestStateColor = super.addColumnShape("attestStateColor", null, 22, attestStateColorColumnOptions);
        if (colDefAttestStateColor) {
            colDefAttestStateColor.cellClassRules = {
                "lightCoralRow": (params) => params.data.isWholedayAbsence === true,
                "warningRow": (params) => params.data.isGeneratingTransactions === true,
            };
        }
        var colDefAttestStateName = super.addColumnText("attestStateName", this.terms["time.time.attest.atteststate"], 100, attestStateNameColumnOptions);
        if (colDefAttestStateName) {
            colDefAttestStateName.cellClassRules = {
                "lightCoralRow": (params) => params.data.isWholedayAbsence === true,
                "warningRow": (params) => params.data.isGeneratingTransactions === true,
            };
        }

        super.addColumnShape("workedInsideScheduleColor", "", 10, workedInsideScheduleColorColumnOptions);
        super.addColumnShape("workedOutsideScheduleColor", "", 10, workedOutsideScheduleColorColumnOptions);
        super.addColumnShape("absenceTimeColor", "", 10, absenceTimeColorColumnOptions);
        super.addColumnShape("standbyTimeColor", "", 10, standbyTimeColorColumnOptions);
        super.addColumnShape("expenseColor", "", 10, expenseColorColumnOptions);

        var colHeaderTemplateSchedule = super.addColumnHeader("templateSchedule", this.terms["time.time.attest.templateschedule"], headerColumnOptions);
        colHeaderTemplateSchedule.marryChildren = true;
        super.addColumnTime("templateScheduleStartTime", this.terms["time.time.attest.start"], 63, { enableHiding: true, clearZero: true, secondaryField: "templateScheduleStopTime", alignLeft: false, cellClassRules: { "excelTime": () => true } }, colHeaderTemplateSchedule);
        super.addColumnTime("templateScheduleStopTime", this.terms["time.time.attest.stop"], 63, { enableHiding: true, clearZero: true, secondaryField: "templateScheduleStartTime", alignLeft: false, cellClassRules: { "excelTime": () => true } }, colHeaderTemplateSchedule);
        super.addColumnTimeSpan("templateScheduleTime", this.terms["time.time.attest.time"], 63, timeColumnOptions, colHeaderTemplateSchedule);
        super.addColumnTimeSpan("templateScheduleBreakTime", this.terms["time.time.attest.break"], 63, timeColumnOptions, colHeaderTemplateSchedule);

        var colHeaderSchedule = super.addColumnHeader("schedule", this.terms["time.time.attest.schedule"], headerColumnOptions);
        colHeaderSchedule.marryChildren = true;
        super.addColumnTime("scheduleStartTime", this.terms["time.time.attest.start"], 63, { enableHiding: true, clearZero: true, secondaryField: "scheduleStopTime", alignLeft: false, cellClassRules: { "excelTime": () => true } }, colHeaderSchedule);
        super.addColumnTime("scheduleStopTime", this.terms["time.time.attest.stop"], 63, { enableHiding: true, clearZero: true, secondaryField: "scheduleStartTime", alignLeft: false, cellClassRules: { "excelTime": () => true } }, colHeaderSchedule);
        super.addColumnTimeSpan("scheduleTime", this.terms["time.time.attest.time"], 63, timeColumnOptions, colHeaderSchedule);
        super.addColumnTimeSpan("scheduleBreakTime", this.terms["time.time.attest.break"], 63, timeColumnOptions, colHeaderSchedule);
        if (!this.isMyTime)
            super.addColumnText("isPreliminary", this.terms["time.time.attest.ispreliminary"], 40, { enableHiding: true }, colHeaderSchedule);

        var colHeaderTime = super.addColumnHeader("time", this.terms["time.time.attest.presence"], headerColumnOptions);
        colHeaderTime.marryChildren = true;
        super.addColumnTime("presenceStartTime", this.terms["time.time.attest.start"], 80, {
            enableHiding: true, clearZero: true, secondaryField: "presenceStopTime", alignLeft: false,
            cellClassRules: {
                "lightCoralRow": (row: AttestEmployeeDayDTO) => this.isPresenceStartAfterSchedule(row['data']),
                "lightBlueRow": (row: AttestEmployeeDayDTO) => this.isPresenceStartBeforeSchedule(row['data']),
            }
        }, colHeaderTime);
        super.addColumnTime("presenceStopTime", this.terms["time.time.attest.stop"], 80, {
            enableHiding: true, clearZero: true, secondaryField: "presenceStartTime", alignLeft: false,
            cellClassRules: {
                "lightCoralRow": (row: AttestEmployeeDayDTO) => this.isPresenceStopBeforeSchedule(row['data']),
                "lightBlueRow": (row: AttestEmployeeDayDTO) => this.isPresenceStopAfterSchedule(row['data']),
            }
        }, colHeaderTime);
        super.addColumnTimeSpan("presenceTime", this.terms["time.time.attest.time"], 80, {
            enableHiding: true, clearZero: true, alignLeft: false,
            cellClassRules: {
                "lightCoralRow": (row: AttestEmployeeDayDTO) => this.isPresenceTimeLessThanSchedule(row['data']),
                "lightBlueRow": (row: AttestEmployeeDayDTO) => this.isPresenceTimeMoreThanSchedule(row['data']),
            }
        }, colHeaderTime);
        super.addColumnTimeSpan("presenceBreakTime", this.terms["time.time.attest.break"], 80, {
            enableHiding: true, clearZero: true, alignLeft: false,
            cellClassRules: {
                "lightCoralRow": (row: AttestEmployeeDayDTO) => row['data'] && row['data'].presenceBreakMinutes && row['data'].scheduleBreakMinutes < row['data'].presenceBreakMinutes,
                "lightBlueRow": (row: AttestEmployeeDayDTO) => row['data'] && row['data'].presenceBreakMinutes && row['data'].scheduleBreakMinutes > row['data'].presenceBreakMinutes,
            }
        }, colHeaderTime);
        super.addColumnTimeSpan("presencePayedTime", this.terms["time.time.attest.approved"], 80, timeColumnOptions, colHeaderTime);

        var colHeaderExpense = super.addColumnHeader("expense", this.terms["time.time.attest.expense"], { enableHiding: true });
        colHeaderExpense.marryChildren = true;
        super.addColumnText("sumExpenseRows", this.terms["common.rows"], 10, timeColumnOptions, colHeaderExpense);
        super.addColumnText("sumExpenseAmount", this.terms["common.amount"], 10, timeColumnOptions, colHeaderExpense);

        var colHeaderSums = super.addColumnHeader("sums", this.terms["time.time.attest.sums"], headerColumnOptions);
        colHeaderSums.marryChildren = true;
        super.addColumnTimeSpan("sumTimeWorkedScheduledTime", this.terms["time.time.attest.sums.workedscheduledtime"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumTimeAccumulator", this.terms["time.time.attest.sums.timeaccumulator"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumTimeAccumulatorOverTime", this.terms["time.time.attest.sums.timeaccumulatorovertime"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAbsence", this.terms["time.time.attest.sums.absence"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnText("sumGrossSalaryAbsenceText", this.terms["time.time.attest.sums.wholedayabsence"], 100, { enableHiding: true }, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAbsenceVacation", this.terms["time.time.attest.sums.absencevacation"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAbsenceSick", this.terms["time.time.attest.sums.absencesick"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAbsenceLeaveOfAbsence", this.terms["time.time.attest.sums.leaveofabsence"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAbsenceParentalLeave", this.terms["time.time.attest.sums.absenceparentalleave"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAbsenceTemporaryParentalLeave", this.terms["time.time.attest.sums.absencetempparentalleave"], 5, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryWeekendSalary", this.terms["time.time.attest.sums.weekendsalary"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryDuty", this.terms["time.time.attest.sums.duty"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAdditionalTime", this.terms["time.time.attest.sums.addedtime"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAdditionalTime35", this.terms["time.time.attest.sums.addedtime35"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAdditionalTime70", this.terms["time.time.attest.sums.addedtime70"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryAdditionalTime100", this.terms["time.time.attest.sums.addedtime100"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition", this.terms["time.time.attest.sums.obaddition"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition40", this.terms["time.time.attest.sums.obaddition40"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition50", this.terms["time.time.attest.sums.obaddition50"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition57", this.terms["time.time.attest.sums.obaddition57"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition70", this.terms["time.time.attest.sums.obaddition70"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition79", this.terms["time.time.attest.sums.obaddition79"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition100", this.terms["time.time.attest.sums.obaddition100"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOBAddition113", this.terms["time.time.attest.sums.obaddition113"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOvertime", this.terms["time.time.attest.sums.compensationandaddition"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOvertime35", this.terms["time.time.attest.sums.compensationandaddition35"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOvertime50", this.terms["time.time.attest.sums.compensationandaddition50"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOvertime70", this.terms["time.time.attest.sums.compensationandaddition70"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumGrossSalaryOvertime100", this.terms["time.time.attest.sums.compensationandaddition100"], 100, timeColumnOptions, colHeaderSums);
        super.addColumnTimeSpan("sumInvoicedTime", this.terms["time.time.attest.invoicedtime"], 100, timeColumnOptions, colHeaderTime);

        this.addColumnIcon("additionalStatusIconValue", null, 30, { toolTipField: "additionalStatusIconMessage", pinned: "right", enableHiding: true, enableResizing: false, suppressExport: false });
        super.addColumnIcon("informations", ' ', null, { icon: "far fa-info-circle infoColor", toolTip: this.terms["core.showinfo"], onClick: this.showInformation.bind(this), showIcon: this.showInformationIcon.bind(this), pinned: "right", enableHiding: false, enableResizing: false, suppressExport: true });
        super.addColumnIcon("warnings", ' ', null, { icon: "far fa-exclamation-circle warningColor", toolTip: this.terms["core.showwarning"], onClick: this.showWarning.bind(this), showIcon: this.showWarningIcon.bind(this), pinned: "right", enableHiding: false, enableResizing: false, suppressExport: true });
        super.addColumnIcon("comment", ' ', null, { icon: "fal fa-comment-dots", toolTip: this.terms["common.showcomment"], onClick: this.showComment.bind(this), showIcon: this.showCommentIcon.bind(this), pinned: "right", enableHiding: false, enableResizing: false, suppressExport: true });
        super.addColumnEdit(this.terms["core.edit"], this.showRowDetail.bind(this));
    }

    private setupGridFooter() {
        const timeSpanColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => CalendarUtility.sumTimeSpan(acc, next),
            cellRenderer: this.timeSpanAggregateRenderer.bind(this)
        } as IColumnAggregate;

        this.soeGridOptions.addFooterRow("#attest-employee-sum-footer-grid", {
            "templateScheduleTime": timeSpanColumnAggregate,
            "templateScheduleBreakTime": timeSpanColumnAggregate,
            "scheduleTime": timeSpanColumnAggregate,
            "scheduleBreakTime": timeSpanColumnAggregate,
            "presenceTime": timeSpanColumnAggregate,
            "presenceBreakTime": timeSpanColumnAggregate,
            "presencePayedTime": timeSpanColumnAggregate,
            "sumExpenseRows": "sum",
            "sumExpenseAmount": timeSpanColumnAggregate,
            "sumTimeWorkedScheduledTime": timeSpanColumnAggregate,
            "sumTimeAccumulator": timeSpanColumnAggregate,
            "sumTimeAccumulatorOverTime": timeSpanColumnAggregate,
            "sumGrossSalaryAbsence": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceVacation": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceSick": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceLeaveOfAbsence": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceParentalLeave": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceTemporaryParentalLeave": timeSpanColumnAggregate,
            "sumGrossSalaryWeekendSalary": timeSpanColumnAggregate,
            "sumGrossSalaryDuty": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime35": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime70": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime100": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition40": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition50": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition70": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition79": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition100": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition113": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime35": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime50": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime70": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime100": timeSpanColumnAggregate,
            "sumInvoicedTime": timeSpanColumnAggregate,
        } as IColumnAggregations);
    }

    private finalizeGrid() {
        this.soeGridOptions.finalizeInitGrid();
        this.restoreState();
        this.soeGridOptions.enableMasterDetailWithDirective("row-detail");
    }

    private setupWatchers() {
        if (!this.attestEmployeeDays)
            this.attestEmployeeDays = [];

        this.$scope.$watch(() => this.attestEmployeeDays, () => {
            this.filteredAttestEmployeeDays = this.attestEmployeeDays;

            var isErp = this.filteredAttestEmployeeDays && (this.filteredAttestEmployeeDays.length > 0) && (this.filteredAttestEmployeeDays[0].timeReportType == TermGroup_TimeReportType.ERP);
            if (isErp !== this.isErp) {
                this.isErp = isErp;
                this.setGridName(true);
            }

            setTimeout(() => {
                if (this.filteredAttestEmployeeDays.length === 1) {
                    let row: any = this.soeGridOptions.getVisibleRowByIndex(0);
                    if (row && row.data)
                        row.setExpanded(true);
                }
            }, 100);

            super.gridDataLoaded(this.filteredAttestEmployeeDays);
            if (this.attestEmployeeDays) {
                //Need to let the UI-thread purge some work before updating the grid height.
                setTimeout(() => {
                    this.soeGridOptions.updateGridHeightBasedOnActualRows();
                }, 100);
            }
        });

        this.$scope.$watch(() => this.hideDaysWithoutSchedule, (newValue, oldValue) => {
            if (this.hideDaysWithoutSchedule)
                this.filteredAttestEmployeeDays = _.filter(this.attestEmployeeDays, (day: AttestEmployeeDayDTO) => !day.isScheduleZeroDay || day.hasTimeStampEntries);
            else
                this.filteredAttestEmployeeDays = this.attestEmployeeDays;
            this.soeGridOptions.setData(this.filteredAttestEmployeeDays);
            this.soeGridOptions.refreshRows();
        });

        // Data updated (after save), need to refresh
        this.$scope.$on("EmployeeContentGridChanged", (event, data) => {
            this.soeGridOptions.refreshCells(true);
            this.soeGridOptions.clearSelectedRows();
            this.soeGridOptions.refreshRows();
        });
    }

    // DIALOGS

    private showInformationIcon(day: AttestEmployeeDayDTO): boolean {
        if (day) {
            var message = this.getInformationMessage(day);
            if (message && message.length > 0)
                return true;
        }
        return false;
    }

    private showInformation(day: AttestEmployeeDayDTO) {
        var message = this.getInformationMessage(day);
        this.notificationService.showDialog(this.terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    private showWarningIcon(day: AttestEmployeeDayDTO): boolean {
        if (day) {
            var message = this.getWarningMessage(day);
            if (message && message.length > 0)
                return true;
        }
        return false;
    }

    private showWarning(day: AttestEmployeeDayDTO) {
        var message = this.getWarningMessage(day);
        this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private showCommentIcon(day: AttestEmployeeDayDTO): boolean {
        if (!day || !day.attestPayrollTransactions || day.attestPayrollTransactions.length === 0)
            return false;

        return _.filter(day.attestPayrollTransactions, t => t.hasComment).length > 0;
    }

    private showComment(day: AttestEmployeeDayDTO) {
        var message = this.getCommentMessage(day);
        this.notificationService.showDialog(this.terms["common.comment"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    private showTransactionInfo(transaction: any) {
        var message = this.getCommentMessage(transaction);
        this.notificationService.showDialog(this.terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    private showEditIcon(row: any) {
        return this.attestMode === TimeAttestMode.Project ? true : false;
    }

    // LOOKUPS

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Billing_Project_TimeSheetUser_OtherEmployees);
        featureIds.push(Feature.Time_Time_TimeSheetUser_OtherEmployees);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.modifyOtherEmployeesPermission = (x[Feature.Billing_Project_TimeSheetUser_OtherEmployees] || x[Feature.Time_Time_TimeSheetUser_OtherEmployees]);
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Project_Invoice_WorkedTime);       // Show worked time
        featureIds.push(Feature.Time_Project_Invoice_InvoicedTime);     // Show invoiced time

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.workTimePermission = x[Feature.Time_Project_Invoice_WorkedTime];
            this.invoiceTimePermission = x[Feature.Time_Project_Invoice_InvoicedTime];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultTimeCodeId = x[CompanySettingType.TimeDefaultTimeCode];
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            //Columns
            "core.yes",
            "core.no",
            "core.edit",
            "core.comment",
            "core.functions",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.info",
            "core.showinfo",
            "core.warning",
            "core.showwarning",
            "common.showcomment",
            "common.rows",
            "common.amount",
            "common.comment",
            "common.day",
            "common.date",
            "common.week",
            "common.absence",
            "common.quantity",
            "common.accounting",
            "common.createdbyat",
            "time.time.attest.dayname",
            "time.time.attest.workedinsideschedulecolor",
            "time.time.attest.workedoutsideschedulecolor",
            "time.time.attest.absencetimecolor",
            "time.time.attest.standbytimecolor",
            "time.time.attest.expensecolor",
            "time.time.attest.templateschedule",
            "time.time.attest.schedule",
            "time.time.attest.presence",
            "time.time.attest.start",
            "time.time.attest.stop",
            "time.time.attest.time",
            "time.time.attest.break",
            "time.time.attest.approved",
            "time.time.attest.expense",
            "time.time.attest.ispreliminary",
            "time.time.attest.atteststate",
            "time.time.attest.atteststate.short",
            "time.time.attest.sums",
            "time.time.attest.sums.absence",
            "time.time.attest.sums.wholedayabsence",
            "time.time.attest.sums.absencevacation",
            "time.time.attest.sums.absencesick",
            "time.time.attest.sums.leaveofabsence",
            "time.time.attest.sums.absenceparentalleave",
            "time.time.attest.sums.absencetempparentalleave",
            "time.time.attest.sums.weekendsalary",
            "time.time.attest.sums.duty",
            "time.time.attest.sums.addedtime",
            "time.time.attest.sums.addedtime35",
            "time.time.attest.sums.addedtime70",
            "time.time.attest.sums.addedtime100",
            "time.time.attest.sums.obaddition",
            "time.time.attest.sums.obaddition40",
            "time.time.attest.sums.obaddition50",
            "time.time.attest.sums.obaddition57",
            "time.time.attest.sums.obaddition70",
            "time.time.attest.sums.obaddition79",
            "time.time.attest.sums.obaddition100",
            "time.time.attest.sums.obaddition113",
            "time.time.attest.sums.compensationandaddition",
            "time.time.attest.sums.compensationandaddition35",
            "time.time.attest.sums.compensationandaddition50",
            "time.time.attest.sums.compensationandaddition70",
            "time.time.attest.sums.compensationandaddition100",
            "time.time.attest.sums.timeaccumulator",
            "time.time.attest.sums.timeaccumulatorovertime",
            "time.time.attest.sums.workedscheduledtime",
            "time.time.attest.sums.suminvoicedtime",
            "time.time.attest.invoicedtime",
            "time.time.payrollproduct.payrollproduct",
            "time.time.attest.transactionstring",

            //Dialogs
            "time.time.attest.dayhasnoatteststates",
            "time.time.attest.dayhasdiscardedbreakeval",
            "time.time.attest.dayhasschedulewithouttransactions",
            "time.time.attest.dayhastimestampswithouttransactions",
            "time.time.attest.dayhaspayrollinformation",
            "time.time.attest.dayhasinvalidtimestamps",
            "time.time.attest.dayhaschangedschedulefromtemplate",
            "time.time.attest.dayhaschangedschedule",
            "time.time.attest.dayhastimescheduletypefactor",
            "time.time.attest.containsduplicatetimeBlocks",
            "time.time.attest.isgeneratingtransactions",
            "time.time.attest.dayhaspayrollimportwarnings",
            "time.time.attest.hasshiftswaps",
            "time.payrollproduct.payrollproduct",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadEmployeeForProject(): ng.IPromise<any> {
        return this.timeService.getEmployeeForUser().then(x => {
            this.employeeTimeCode = x;
            if (this.employeeTimeCode)
                this.employeeTimeCodeId = this.employeeTimeCode.employeeId;
        });
    }

    private loadEmployeesForProject(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.modifyOtherEmployeesPermission) {
            this.timeService.getEmployeesForProjectTimeCode(false, false, false, this.employeeTimeCodeId).then(x => {
                this.employeeTimeCodes = x;
                _.forEach(x, (e) => {
                    this.employeeTimeCodes.push(e);
                });
                deferral.resolve();
            });
        } else {
            this.employeeTimeCodes = [];
            this.employeeTimeCodes.push(this.employeeTimeCode);
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodes(SoeTimeCodeType.WorkAndAbsense, true, false).then((x) => {
            _.forEach(x, (t) => {
                this.timeCodesDict.push({ value: t.timeCodeId, label: t.name });
            });
        });
    }

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCauses().then((x) => {
            this.timeDeviationCauses = x;
        });
    }

    private loadProjectInvoices() {
        var employeeIds = _.map(this.employeeTimeCodes, e => e.employeeId);
        return this.timeService.getProjectsForTimeSheetEmployees(employeeIds).then((x) => {
            this.projectInvoices = x;
            for (let e of this.projectInvoices) {
                //Filter projects
                for (let p of e.projects) {
                    if (_.filter(this.allProjects, ap => ap.id === p.projectId).length === 0) {
                        this.allProjectsAndInvoices.push(p);
                        this.allProjects.push({ id: p.projectId, label: p.numberName });
                        this.filteredProjectsDict.push({ id: p.projectId, label: p.numberName })
                    }
                }
                //Filter invoices
                for (let i of e.invoices) {
                    if (_.filter(this.allOrders, ao => ao.id === i.invoiceId).length === 0) {
                        this.allOrders.push({ id: i.invoiceId, label: i.numberName });
                        this.filteredOrdersDict.push({ id: i.invoiceId, label: i.numberName })
                    }
                }
            }
        });
    }

    // ACTIONS

    private showEdit(row: AttestEmployeeDayDTO) {
        if (!row)
            return;

        if (row.projectTimeBlocks) {
            row.projectTimeBlocks = row.projectTimeBlocks.map(tb => {
                var obj = new ProjectTimeBlockDTO();
                angular.extend(obj, tb);
                if (obj.date)
                    obj.date = CalendarUtility.convertToDate(obj.date);
                if (obj.startTime)
                    obj.startTime = CalendarUtility.convertToDate(obj.startTime);
                if (obj.stopTime)
                    obj.stopTime = CalendarUtility.convertToDate(obj.stopTime);
                if (!obj['originalStartTime']) {
                    obj['originalStartTime'] = obj.startTime;
                    obj['originalStopTime'] = obj.stopTime;
                }
                return obj;
            });
        }

        // Show edit time dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/editTimeGrid.html"),
            controller: EditTimeGridController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                rows: () => { return row.projectTimeBlocks },
                employee: () => { return this.employeeTimeCode },
                employees: () => { return this.employeeTimeCodes },
                timeCodes: () => { return this.timeCodesDict },
                timeDeviationCauses: () => { return this.timeDeviationCauses },
                defaultTimeCodeId: () => { return this.defaultTimeCodeId },
                invoiceTimePermission: () => { return this.invoiceTimePermission },
                workTimePermission: () => { return this.workTimePermission },
                modifyOtherEmployeesPermission: () => { return this.modifyOtherEmployeesPermission },
                registrationType: () => { return ProjectTimeRegistrationType.Attest },
                useExtendedTimeRegistration: () => { return true },
                createTransactionsBasedOnTimeRules: () => { return true },
                projectInvoices: () => { return this.projectInvoices },
                employeeDaysWithSchedule: () => { return null },
                readOnly: () => { return !(this.workTimePermission && this.invoiceTimePermission) },
                enableAddNew: () => { return false },
                dayIsAttested: () => { return _.filter(row.attestStates, a => (a.initial === false || a.closed === true || a.locked === true)).length > 0 },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.rows && result.rows.length > 0) {
                // Get modified rows back from dialog
                this.saveRows(result.rows);
            }
        });
    }

    private showRowDetail(row: AttestEmployeeDayDTO) {
        if (!row)
            return;

        let nbrOfSelectedRows = this.soeGridOptions.getSelectedRows().length;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/RowDetailDialog/RowDetailDialog.html"),
            controller: RowDetailDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                days: () => { return nbrOfSelectedRows > 1 ? this.soeGridOptions.getSelectedRows() : this.attestEmployeeDays },
                data: () => { return row },
                showMultipleDays: () => { return nbrOfSelectedRows > 1 }
            }
        }
        this.$uibModal.open(options);
    }

    private saveRows(rows: ProjectTimeBlockDTO[]) {
        this.startSave();

        var dtos: ProjectTimeBlockSaveDTO[] = [];
        _.forEach(rows, (row: ProjectTimeBlockDTO) => {
            var dto = new ProjectTimeBlockSaveDTO();
            dto.projectTimeBlockId = row.projectTimeBlockId;
            dto.actorCompanyId = CoreUtility.actorCompanyId;
            dto.customerInvoiceId = row.customerInvoiceId;
            dto.date = row.date;
            dto.employeeId = row.employeeId;
            dto.from = row.startTime;
            dto.to = row.stopTime;
            dto.timeDeviationCauseId = row.timeDeviationCauseId;
            dto.externalNote = row.externalNote;
            dto.internalNote = row.internalNote;
            dto.invoiceQuantity = row.invoiceQuantity;
            dto.isFromTimeSheet = true;
            dto.projectId = row.projectId;
            dto.projectInvoiceDayId = 0;
            dto.projectInvoiceWeekId = row.projectInvoiceWeekId;
            dto.state = row.isDeleted ? SoeEntityState.Deleted : SoeEntityState.Active;
            dto.timeBlockDateId = row.timeBlockDateId;
            dto.timeCodeId = row.timeCodeId;
            dto.timePayrollQuantity = row.timePayrollQuantity;
            dto.timeSheetWeekId = row.timeSheetWeekId;
            dtos.push(dto);
        });

        this.$timeout(() => {
            this.timeService.saveProjectTimeBlocks(dtos).then(saveResult => {
                if (saveResult.success) {
                    this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWS_RELOAD, { date: null, fromModal: false });
                    this.completedSave(null, true);
                } else {
                    this.failedSave(saveResult.errorMessage);
                }
            });
        });
    }

    // EVENTS

    private afterCellEdit(entity, colDef) {

    }

    // HELP-METHODS

    private gridAndDataIsReady() {
        this.setupWatchers();
    }

    private timeSpanAggregateRenderer({ data, colDef, formatValue }) {
        var value = data[colDef.field];
        if (!value || value === "0" || value === "00:00")
            return "<div></div>";
        return "<b>" + data[colDef.field] + "<b>";
    }

    private scrollToColumn(row: AttestEmployeeDayDTO, fieldName: string) {
        var colDef = this.soeGridOptions.getColumnDefByField(fieldName);
        this.soeGridOptions.startEditingCell(<any>row, colDef);
    }

    private setRowGroupExpension() {
        this.soeGridOptions.setAllGroupExpended(!this.collapseAllRowGroups);
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWS_SELECTED, this.soeGridOptions.getSelectedRows());
        });
    }

    private getInformationMessage(day: AttestEmployeeDayDTO): string {
        var message: string = '';
        if (day) {
            if (day.hasShiftSwaps)
                message += this.terms["time.time.attest.hasshiftswaps"] + "\n";
        }
        return message;
    }

    private getWarningMessage(day: AttestEmployeeDayDTO): string {
        var message: string = '';
        if (day) {
            if (day.isScheduleChangedFromTemplate)
                message += this.terms["time.time.attest.dayhaschangedschedulefromtemplate"] + "\n";
            if (day.hasScheduleWithoutTransactions)
                message += this.terms["time.time.attest.dayhasschedulewithouttransactions"] + "\n";
            if (day.hasPeriodTimeScheduleTypeFactorMinutes)
                message += this.terms["time.time.attest.dayhastimescheduletypefactor"].format(day.timeScheduleTypeFactorMinutes) + "\n";
            if (day.hasPeriodDiscardedBreakEvaluation)
                message += this.terms["time.time.attest.dayhasdiscardedbreakeval"] + "\n";
            if (day.hasTimeStampsWithoutTransactions)
                message += this.terms["time.time.attest.dayhastimestampswithouttransactions"] + "\n";
            if (day.hasPayrollImports)
                message += this.terms["time.time.attest.dayhaspayrollinformation"] + "\n";
            if (day.hasInvalidTimeStamps)
                message += this.terms["time.time.attest.dayhasinvalidtimestamps"] + "\n";
            if (day.containsDuplicateTimeBlocks)
                message += this.terms["time.time.attest.containsduplicatetimeBlocks"] + "\n";
            if (day.isGeneratingTransactions)
                message += this.terms["time.time.attest.isgeneratingtransactions"] + "\n";
            if (day.hasPayrollImportWarnings)
                message += this.terms["time.time.attest.dayhaspayrollimportwarnings"] + "\n";
        }
        return message;
    }

    private getCommentMessage(day: AttestEmployeeDayDTO): string {
        var message: string = '';
        if (!day)
            return message;

        if (day != null && day.attestPayrollTransactions != null) {
            _.forEach(day.attestPayrollTransactions, (transaction) => {
                if (transaction.hasComment) {
                    if (message.length > 0)
                        message += "\n\n";
                    message += "{0}: {1} {2}\n{3}: {4}\n{5}".format(this.terms["time.payrollproduct.payrollproduct"], transaction.payrollProductNumber, transaction.payrollProductName, this.terms["common.quantity"], transaction.quantityString, transaction.comment);
                }
            });
        }

        return message;
    }

    private getTransactionInfoMessage(transaction: any) {
        var message = this.terms["time.time.attest.transactionstring"].format(transaction.timePayrollTransactionId);
        message += "\r\n";
        message += "\r\n";

        if (!transaction)
            return;

        if (transaction.created) {
            message += this.terms["common.createdbyat"].format(StringUtility.nullToEmpty(transaction.createdBy), CalendarUtility.toFormattedDateAndTime(transaction.created));
            message += "\r\n";
        }

        if (transaction.attestTransitionLogs) {
            _.forEach(transaction.attestTransitionLogs, (log) => {
                message += this.terms["time.time.attest.transactionchangedfromtoat"].format(log.attestStateFromName, log.attestStateToName, log.attestTransitionCreatedBySupport ? "SoftOne" + " (" + log.attestTransitionUserId + ")" : log.attestTransitionUserName, CalendarUtility.toFormattedDateAndTime(log.attestTransitionDate));
                message += "\r\n";
            });
        }

        return message;
    }

    private isPresenceStartBeforeSchedule(row: AttestEmployeeDayDTO): boolean {
        return row && row.presenceStartTime && (row.presenceStartTime < row.scheduleStartTime || (!CalendarUtility.isTimeZero(row.presenceTime) && CalendarUtility.isTimeZero(row.scheduleTime)));
    }

    private isPresenceStartAfterSchedule(row: AttestEmployeeDayDTO): boolean {
        return row && row.presenceStartTime && row.presenceStartTime > row.scheduleStartTime && !CalendarUtility.isTimeZero(row.scheduleTime);
    }

    private isPresenceStopBeforeSchedule(row: AttestEmployeeDayDTO): boolean {
        return row && row.presenceStopTime && row.presenceStopTime < row.scheduleStopTime;
    }

    private isPresenceStopAfterSchedule(row: AttestEmployeeDayDTO): boolean {
        return row && row.presenceStopTime && row.presenceStopTime > row.scheduleStopTime;
    }

    private isPresenceTimeLessThanSchedule(row: AttestEmployeeDayDTO): boolean {
        return row && row.presenceTime && row.presenceTime < row.scheduleTime
    }

    private isPresenceTimeMoreThanSchedule(row: AttestEmployeeDayDTO): boolean {
        return row && row.presenceTime && row.presenceTime > row.scheduleTime
    }

    private formatPresence(value: boolean) {
        return value ? this.terms["time.time.attest.presence"] : this.terms["common.absence"];
    }

    private formatPayrollProductPayed(value: boolean) {
        return value ? this.terms["core.yes"] : this.terms["core.no"];
    }
}