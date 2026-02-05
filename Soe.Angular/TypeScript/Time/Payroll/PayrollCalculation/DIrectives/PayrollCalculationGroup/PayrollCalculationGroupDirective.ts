import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { PayrollCalculationEmployeePeriodDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { IColumnAggregations } from "../../../../../Util/SoeGridOptionsAg";
import { Constants } from "../../../../../Util/Constants";
import { TermGroup_PayrollControlFunctionStatus } from "../../../../../Util/CommonEnumerations";

export class PayrollCalculationGroupDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollCalculation/Directives/PayrollCalculationGroup/PayrollCalculationGroup.html'),
            scope: {
                rows: '=',
                filteredRows: '='                
            },
            restrict: 'E',
            replace: true,
            controller: PayrollCalculationGroupController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class PayrollCalculationGroupController {

    // Terms
    private terms: { [index: string]: string; };

    // Init parameters
    private rows: PayrollCalculationEmployeePeriodDTO[];
    private filteredRows: PayrollCalculationEmployeePeriodDTO[];    
    private gridHandler: EmbeddedGridController;
    private allWarnings: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        gridHandlerFactory: IGridHandlerFactory,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private payrollService: IPayrollService) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "PayrollCalculationGroup");
    }

    // INIT

    public $onInit() {
        this.doLookups().then(() => {
            this.setupWatchers();
            this.setupGrid();
        })
    }

    private setupWatchers() {
        
        if (!this.rows)
            this.rows = [];

        this.$scope.$watch(() => this.rows, () => {
            this.$timeout(() => {
                this.populateGrid();
                this.loadWarningsGroup();
            });
        });
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.setMinRowsToShow(20);
        this.gridHandler.gridAg.addColumnText("employeeNrAndName", this.terms["common.name"], null, false);        
        this.gridHandler.gridAg.addColumnText("periodWarnings", this.terms["common.recalculatetimestatus.warnings"], null, false, {  toolTipField: "periodWarnings" });
        this.gridHandler.gridAg.addColumnNumber("periodSumGross", this.terms["time.payroll.payrollcalculation.grossalary"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumGross < 0 } } });
        this.gridHandler.gridAg.addColumnNumber("periodSumBenefitInvertExcluded", this.terms["time.payroll.payrollcalculation.benefit"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumBenefitInvertExcluded < 0 } } });
        this.gridHandler.gridAg.addColumnNumber("periodSumCompensation", this.terms["time.payroll.payrollcalculation.compensation"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumCompensation < 0 } } });
        this.gridHandler.gridAg.addColumnNumber("periodSumDeduction", this.terms["time.payroll.payrollcalculation.deduction"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumDeduction < 0 } } });
        this.gridHandler.gridAg.addColumnNumber("periodSumTax", this.terms["time.payroll.payrollcalculation.tax"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumTax < 0 } } });
        this.gridHandler.gridAg.addColumnNumber("periodSumEmploymentTax", this.terms["time.payroll.payrollcalculation.employmenttax"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumEmploymentTax < 0 } } });
        this.gridHandler.gridAg.addColumnNumber("periodSumNet", this.terms["time.payroll.payrollcalculation.netsalary"], 70, { decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.periodSumNet < 0 } } });
        this.gridHandler.gridAg.addColumnShape("attestStateColor", null, 22, { enableHiding: false, shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateName", showIconField: "hasAttestStates", suppressExport: true });
        this.gridHandler.gridAg.addColumnText("attestStateName", this.terms["time.atteststate.state"], 136, false, { suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnDateTime("createdOrModified", this.terms["time.payroll.payrollcalculation.lastrecalculated"], 136, true, null, { suppressSizeToFit: true, cellClassRules: { "errorRow": (row) => { return !row.data.createdOrModified } } })

        // Events
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, (row: uiGrid.IGridRow) => { this.gridFilterChanged(); }));
        this.gridHandler.gridAg.options.subscribe(events);
        
        this.gridHandler.gridAg.finalizeInitGrid("time.payroll.payrollcalculation.calculation", true);

        this.$timeout(() => {
            this.gridHandler.gridAg.options.addFooterRow("#payroll-calculation-group-sum-footer-grid", {
                "periodSumGross": "sum",
                "periodSumBenefitInvertExcluded": "sum",
                "periodSumCompensation": "sum",
                "periodSumDeduction": "sum",
                "periodSumTax": "sum",
                "periodSumEmploymentTax": "sum",
                "periodSumNet": "sum"
            } as IColumnAggregations);
        });
    }

    // SERVICE CALLS

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "common.name",
            "time.atteststate.state",
            "time.payroll.payrollcalculation.calculation",
            "time.payroll.payrollcalculation.benefit",
            "time.payroll.payrollcalculation.compensation",
            "time.payroll.payrollcalculation.deduction",
            "time.payroll.payrollcalculation.employmenttax",
            "time.payroll.payrollcalculation.grossalary",
            "time.payroll.payrollcalculation.lastrecalculated",
            "time.payroll.payrollcalculation.netsalary",
            "time.payroll.payrollcalculation.tax",
            "common.recalculatetimestatus.warnings"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }
    private populateGrid() {
        this.gridHandler.gridAg.setData(this.rows);
        this.gridFilterChanged();
    }

    private loadWarningsGroup(): ng.IPromise<any> {
        console.log('Loading warnings for employees1');
        if (!this.rows || this.rows.length === 0) {
            return;
        }

        let employeeIds = _.map(this.rows, e => e.employeeId);
        let timePeriodId = this.rows[0].timePeriodId;
        console.log('Loading warnings for employees', employeeIds, 'and time period', timePeriodId);
        return this.payrollService.getPayrollWarningsGroup(employeeIds, timePeriodId, false).then(x => {
            this.allWarnings = x;
            this.rows.forEach(row => {
                let warnings = this.allWarnings.filter(w => w.status != TermGroup_PayrollControlFunctionStatus.HideforPeriod && w.employeeId === row.employeeId);
                row.periodWarnings = warnings.map(w => w.typeName).join(', ');
            });
            this.populateGrid();
        });
    }

    // EVENTS

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_GROUP_ROWS_SELECTED, this.gridHandler.gridAg.options.getSelectedRows());
        });
    }

    private gridFilterChanged() {
        this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_GROUP_ROWS_FILTERED, { rows: this.gridHandler.gridAg.options.getFilteredRows(), totalCount: this.gridHandler.gridAg.options.getData().length });
    }
}