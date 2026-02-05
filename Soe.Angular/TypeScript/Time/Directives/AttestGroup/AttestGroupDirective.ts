import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { TimeAttestMode, Feature } from "../../../Util/CommonEnumerations";
import { AttestEmployeePeriodDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../../Time/TimeService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IColumnAggregate, IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Constants } from "../../../Util/Constants";

export class AttestGroupDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestGroup/Views/AttestGroup.html'),
            scope: {
                registerControl: '&',
                progressBusy: '=?',
                attestMode: '=',
                isReadonly: '=?',
                attestEmployeePeriods: '=',
                showGrouping: '=?',
                viewEmployee: '&',
                preview: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: AttestGroupController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class AttestGroupController extends GridControllerBaseAg {

    private registerControl: Function;
    private viewEmployee: Function;
    private attestMode: TimeAttestMode;
    private isReadonly: boolean;
    private preview: boolean;
    private terms: any;
    private attestEmployeePeriods: AttestEmployeePeriodDTO[];

    private modalInstance: any;

    // Init parameters
    private showSortButtons: boolean;
    private showGrouping: boolean = true;
    private minRowsToShow: number;
    private _collapseAllRowGroups: boolean;
    public get collapseAllRowGroups(): boolean {
        return this._collapseAllRowGroups;
    }
    public set collapseAllRowGroups(value: boolean) {
        this._collapseAllRowGroups = value
        this.setRowGroupExpension();
    }

    // Converted init parameters
    private showSortButtonsValue: boolean;

    // Company settings

    //ui stuff
    private lastNavigation: { row: any, column: any };
    private gridHeightStyle;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        protected $uibModal,
        private $filter: ng.IFilterService,
        protected coreService: ICoreService,
        private timeService: ITimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.AttestGroup", "time.time.attest.attestemployee", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        this.modalInstance = $uibModal;

        this.showSortButtonsValue = <any>this.showSortButtons === 'true';
        this.initGrid();

        if (this.registerControl)
            this.registerControl({ control: this });
    }

    // INIT

    private initGrid() {
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.ignoreResizeToFit = true;

        var gridOptions = (this.soeGridOptions as any).gridOptions;
        gridOptions.suppressHorizontalScroll = false;
        gridOptions.suppressMovableColumns = true;
        gridOptions.headerRowHeight = 20;//partial fix for scrolling issue
        gridOptions.rowHeight = 22;//partial fix for scrolling issue
        gridOptions.suppressMaxRenderedRowRestriction = true;

        var height = gridOptions.minRowsToShow * gridOptions.rowHeight + 92;//This causes the canvas to fit 8.5 rows, which makes a scrolling bug on the first row that causes a scrollbar to go away. The important part is that the canvas need to a be a bit bigger than the rows the grid thinks its rendering.
        this.gridHeightStyle = { height: height + "px" };

        this.$scope.$on('focusRow', (e, a) => {
            this.soeGridOptions.startEditingCell(a.row - 1, 0);
        });
    }

    // SETUP

    public setupGrid() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, (row: uiGrid.IGridRow) => { this.gridFilterChanged(); }));
        this.soeGridOptions.subscribe(events);

        this.startLoad();
        this.$q.all([
            this.loadTerms()]).then(() => {
                this.gridAndDataIsReady();
            });
    }

    private setupGridColumns() {
        var timeColumnOptions = { enableHiding: true, clearZero: true, alignLeft: true, cellClassRules: { "excelTime": () => true } }
        var numberColumnOptions = { enableHiding: true, clearZero: true }
        var textColumnOptions = { enableHiding: true }

        super.addColumnText("employeeNrAndName", this.terms["common.employee"], null, { enableHiding: false });
        super.addColumnShape("attestStateColor", null, 40, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "attestStateColor" });
        if (this.preview) {
            super.addColumnText("attestStateName", this.terms["time.time.attest.atteststate"], 600, { enableHiding: true, clearZero: true, minWidth: 600 });
        }
        else {            
            super.addColumnText("attestStateName", this.terms["time.time.attest.atteststate"], 100, { enableHiding: true, clearZero: true });

            var colHeaderSchedule = super.addColumnHeader("schedule", this.terms["time.time.attest.schedule"], { enableHiding: true });
            colHeaderSchedule.marryChildren = true;
            super.addColumnNumber("scheduleDays", this.terms["time.time.attest.days"], 10, numberColumnOptions, colHeaderSchedule);
            super.addColumnText("scheduleTimeInfo", this.terms["time.time.attest.time"], 10, textColumnOptions, colHeaderSchedule);
            super.addColumnText("scheduleBreakTimeInfo", this.terms["time.time.attest.break"], 10, textColumnOptions, colHeaderSchedule);

            var colHeaderTime = super.addColumnHeader("time", this.terms["time.time.attest.presence"], { enableHiding: true });
            colHeaderTime.marryChildren = true;
            super.addColumnNumber("presenceDays", this.terms["time.time.attest.days"], 10, {
                enableHiding: true,
                cellClassRules: {
                    "lightCoralRow": (row: AttestEmployeePeriodDTO) => row['data'] && row['data'].scheduleDays > row['data'].presenceDays,
                    "lightBlueRow": (row: AttestEmployeePeriodDTO) => row['data'] && row['data'].scheduleDays < row['data'].presenceDays,
                }
            }, colHeaderTime);
            super.addColumnText("presenceTimeInfo", this.terms["time.time.attest.time"], 10, {
                enableHiding: true,
                cellClassRules: {
                    "lightCoralRow": (row: AttestEmployeePeriodDTO) => row['data'] && row['data'].scheduleTime > row['data'].presenceTime,
                    "lightBlueRow": (row: AttestEmployeePeriodDTO) => row['data'] && row['data'].scheduleTime < row['data'].presenceTime,
                }
            }, colHeaderTime);
            super.addColumnText("presenceBreakTimeInfo", this.terms["time.time.attest.break"], 10, textColumnOptions, colHeaderTime);
            super.addColumnText("presencePayedTimeInfo", this.terms["time.time.attest.payed"], 63, textColumnOptions, colHeaderTime);

            var colHeaderExpense = super.addColumnHeader("expense", this.terms["time.time.attest.expense"], { enableHiding: true });
            colHeaderExpense.marryChildren = true;
            super.addColumnText("sumExpenseRows", this.terms["common.rows"], 10, timeColumnOptions, colHeaderExpense);
            super.addColumnText("sumExpenseAmount", this.terms["common.amount"], 10, timeColumnOptions, colHeaderExpense);

            var colHeaderSums = super.addColumnHeader("sums", this.terms["time.time.attest.sums"], { enableHiding: true });
            colHeaderSums.marryChildren = true;
            super.addColumnText("sumTimeWorkedScheduledTimeText", this.terms["time.time.attest.sums.workedscheduledtime"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumTimeAccumulatorText", this.terms["time.time.attest.sums.timeaccumulator"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumTimeAccumulatorOverTimeText", this.terms["time.time.attest.sums.timeaccumulatorovertime"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAbsenceText", this.terms["time.time.attest.sums.absence"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAbsenceVacationText", this.terms["time.time.attest.sums.absencevacation"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAbsenceSickText", this.terms["time.time.attest.sums.absencesick"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAbsenceLeaveOfAbsenceText", this.terms["time.time.attest.sums.leaveofabsence"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAbsenceParentalLeaveText", this.terms["time.time.attest.sums.absenceparentalleave"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAbsenceTemporaryParentalLeaveText", this.terms["time.time.attest.sums.absencetempparentalleave"], 5, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryWeekendSalaryText", this.terms["time.time.attest.sums.weekendsalary"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryDutyText", this.terms["time.time.attest.sums.duty"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAdditionalTimeText", this.terms["time.time.attest.sums.addedtime"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAdditionalTime35Text", this.terms["time.time.attest.sums.addedtime35"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAdditionalTime70Text", this.terms["time.time.attest.sums.addedtime70"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryAdditionalTime100Text", this.terms["time.time.attest.sums.addedtime100"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAdditionText", this.terms["time.time.attest.sums.obaddition"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition40Text", this.terms["time.time.attest.sums.obaddition40"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition50Text", this.terms["time.time.attest.sums.obaddition50"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition57Text", this.terms["time.time.attest.sums.obaddition57"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition70Text", this.terms["time.time.attest.sums.obaddition70"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition79Text", this.terms["time.time.attest.sums.obaddition79"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition100Text", this.terms["time.time.attest.sums.obaddition100"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOBAddition113Text", this.terms["time.time.attest.sums.obaddition113"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOvertimeText", this.terms["time.time.attest.sums.compensationandaddition"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOvertime35Text", this.terms["time.time.attest.sums.compensationandaddition35"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOvertime50Text", this.terms["time.time.attest.sums.compensationandaddition50"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOvertime70Text", this.terms["time.time.attest.sums.compensationandaddition70"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumGrossSalaryOvertime100Text", this.terms["time.time.attest.sums.compensationandaddition100"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnText("sumInvoicedTimeText", this.terms["time.time.attest.sums.suminvoicedtime"], 10, timeColumnOptions, colHeaderSums);
            super.addColumnIcon("view", " ", null, { icon: "fal fa-file-search iconEdit", toolTip: this.terms["time.time.attest.showemployeeperiod"], onClick: this.viewEmployeeClick.bind(this), pinned: "right", enableHiding: false, enableResizing: false });
            super.addColumnIcon("informations", " ", null, { icon: "far fa-info-circle infoColor", toolTip: this.terms["core.showinfo"], onClick: this.showInformation.bind(this), showIcon: this.showInformationIcon.bind(this), pinned: "right", enableHiding: false, enableResizing: false });
            super.addColumnIcon("warnings", " ", null, { icon: "far fa-exclamation-circle warningColor", toolTip: this.terms["core.showwarning"], onClick: this.showWarning.bind(this), showIcon: this.showWarningIcon.bind(this), pinned: "right", enableHiding: false, enableResizing: false });
        }
    }

    private setupGridFooter() {
        const timeSpanColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => CalendarUtility.sumTimeSpan(acc, next),
            cellRenderer: this.timeSpanAggregateRenderer.bind(this)
        } as IColumnAggregate;

        const numberColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => acc + next,
            cellRenderer: this.numberAggregateRenderer.bind(this)
        } as IColumnAggregate;

        this.soeGridOptions.addFooterRow("#attest-group-sum-footer-grid", {
            "scheduleDays": numberColumnAggregate,
            "scheduleTimeInfo": timeSpanColumnAggregate,
            "scheduleBreakTimeInfo": timeSpanColumnAggregate,
            "presenceDays": numberColumnAggregate,
            "presenceTimeInfo": timeSpanColumnAggregate,
            "presenceBreakTimeInfo": timeSpanColumnAggregate,
            "presencePayedTimeInfo": timeSpanColumnAggregate,
            "sumExpenseRows": "sum",
            "sumTimeWorkedScheduledTimeText": timeSpanColumnAggregate,
            "sumExpenseAmount": timeSpanColumnAggregate,
            "sumTimeAccumulatorText": timeSpanColumnAggregate,
            "sumTimeAccumulatorOverTimeText": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceText": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceVacationText": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceSickText": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceLeaveOfAbsenceText": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceParentalLeaveText": timeSpanColumnAggregate,
            "sumGrossSalaryAbsenceTemporaryParentalLeaveText": timeSpanColumnAggregate,
            "sumGrossSalaryWeekendSalaryText": timeSpanColumnAggregate,
            "sumGrossSalaryDutyText": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTimeText": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime35Text": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime70Text": timeSpanColumnAggregate,
            "sumGrossSalaryAdditionalTime100Text": timeSpanColumnAggregate,
            "sumGrossSalaryOBAdditionText": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition40Text": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition50Text": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition70Text": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition79Text": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition100Text": timeSpanColumnAggregate,
            "sumGrossSalaryOBAddition113Text": timeSpanColumnAggregate,
            "sumGrossSalaryOvertimeText": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime35Text": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime50Text": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime70Text": timeSpanColumnAggregate,
            "sumGrossSalaryOvertime100Text": timeSpanColumnAggregate,
            "sumInvoicedTimeText": timeSpanColumnAggregate,

        } as IColumnAggregations);
    }

    private finalizeGrid() {
        this.soeGridOptions.finalizeInitGrid();
        this.restoreState();
    }

    private setupWatchers() {
        if (!this.attestEmployeePeriods)
            this.attestEmployeePeriods = [];

        this.$scope.$watch(() => this.attestEmployeePeriods, () => {
            this.$timeout(() => {
                super.gridDataLoaded(this.attestEmployeePeriods)
                if (this.attestEmployeePeriods)
                    this.soeGridOptions.setMinRowsToShow(this.attestEmployeePeriods.length + 2);
                this.soeGridOptions.autosizeColumns();
                this.gridFilterChanged();
                if (this.attestEmployeePeriods)
                    this.soeGridOptions.updateGridHeightBasedOnActualRows();
            }, 100);
        });
    }

    // DIALOGS

    private showInformationIcon(period: AttestEmployeePeriodDTO): boolean {
        return period && period.hasInformations;
    }

    private showInformation(period: AttestEmployeePeriodDTO) {
        var message = this.getInformationMessage(period);
        this.notificationService.showDialog(this.terms["core.info"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    private showWarningIcon(period: AttestEmployeePeriodDTO): boolean {
        return period && period.hasWarnings;
    }

    private showWarning(period: AttestEmployeePeriodDTO) {
        var message = this.getWarningMessage(period);
        this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private getInformationMessage(period: AttestEmployeePeriodDTO): string {
        var message: string = '';
        if (period) {
            if (period.hasShiftSwaps)
                message += this.terms["time.time.attest.hasshiftswaps"] + "\n";
        }
        return message;
    }

    private getWarningMessage(period: AttestEmployeePeriodDTO): string {
        var message: string = '';
        if (!period || !period.hasWarnings)
            return message;

        if (period.hasScheduleWithoutTransactions)
            message += this.terms["time.time.attest.hasschedulewithouttransactions"] + "\n";
        if (period.hasTimeStampsWithoutTransactions)
            message += this.terms["time.time.attest.hastimestampswithouttransactions"] + "\n";
        if (period.hasInvalidTimeStamps)
            message += this.terms["time.time.attest.dayhasinvalidtimestamps"] + "\n";
        if (period.hasPayrollImports)
            message += this.terms["time.time.attest.dayhaspayrollinformation"] + "\n";
        return message;
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        // Columns
        var keys: string[] = [
            "common.employee",
            "common.rows",
            "common.amount",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "time.time.attest.schedule",
            "time.time.attest.presence",
            "time.time.attest.days",
            "time.time.attest.time",
            "time.time.attest.break",
            "time.time.attest.payed",
            "time.time.attest.atteststate",
            "time.time.attest.expense",
            "time.time.attest.sums",
            "time.time.attest.sums.absence",
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
            "time.time.attest.showemployeeperiod",
            "time.time.attest.hasshiftswaps",
            //Dialogs
            "time.time.attest.hasschedulewithouttransactions",
            "time.time.attest.hastimestampswithouttransactions",
            "time.time.attest.dayhasinvalidtimestamps",
            "time.time.attest.periodhaschangedschedule",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // EVENTS

    // HELP-METHODS

    private gridAndDataIsReady() {
        this.setupGridColumns();
        this.setupGridFooter();
        this.finalizeGrid();
        this.setupWatchers();
    }

    private timeSpanAggregateRenderer({ data, colDef, formatValue }) {
        var value = data[colDef.field];
        if (!value || value === "0" || value === "00:00")
            return "<div></div>";
        return "<b>" + data[colDef.field] + "<b>";
    }

    private numberAggregateRenderer({ data, colDef, formatValue }) {
        var value = data[colDef.field];
        if (!value || value === "0")
            return "<div></div>";
        return "<b>" + data[colDef.field] + "<b>";
    }

    private scrollToColumn(row: AttestEmployeePeriodDTO, fieldName: string) {
        var colDef = this.soeGridOptions.getColumnDefByField(fieldName);
        this.soeGridOptions.startEditingCell(<any>row, colDef);
    }

    private setRowGroupExpension() {
        this.soeGridOptions.setAllGroupExpended(!this.collapseAllRowGroups);
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.messagingService.publish(Constants.EVENT_ATTESTGROUP_ROWS_SELECTED, this.soeGridOptions.getSelectedRows());
        });
    }

    private gridFilterChanged() {
        var data = this.soeGridOptions.getData();
        var length: number = data ? data.length : 0;
        this.messagingService.publish(Constants.EVENT_ATTESTGROUP_ROWS_FILTERED, { rows: this.soeGridOptions.getFilteredRows(), totalCount: length });
    }

    private viewEmployeeClick(period: AttestEmployeePeriodDTO) {
        if (this.viewEmployee)
            this.viewEmployee({ row: period });
    }
}