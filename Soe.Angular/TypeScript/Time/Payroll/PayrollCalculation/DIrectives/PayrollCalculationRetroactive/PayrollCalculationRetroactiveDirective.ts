import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { RetroactivePayrollDTO } from "../../../../../Common/Models/RetroactivePayroll";
import { IColumnAggregations } from "../../../../../Util/SoeGridOptionsAg";
import { Constants } from "../../../../../Util/Constants";

export class PayrollCalculationRetroactiveDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollCalculation/Directives/PayrollCalculationRetroactive/PayrollCalculationRetroactive.html'),
            scope: {
                rows: '=',
                timePeriodHeadId: '=',
                timePeriodId: '=',
                employeeId: '='
            },
            restrict: 'E',
            replace: true,
            controller: PayrollCalculationRetroactiveController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class PayrollCalculationRetroactiveController {

    // Terms
    private terms: { [index: string]: string; };

    // Init parameters
    private rows: RetroactivePayrollDTO[];
    private employeeId: number;
    private timePeriodHeadId: number;
    private timePeriodId: number;

    private gridHandler: EmbeddedGridController;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private payrollService: IPayrollService) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "PayrollCalculationRetroactive");
    }

    // INIT

    public $onInit() {
        this.doLookups().then(() => {
            this.setupGrid();
            this.setupWatchers();
        })
    }

    private setupWatchers() {
        if (!this.rows)
            this.rows = [];

        this.$scope.$watch(() => this.rows, () => {
            this.$timeout(() => {
                this.gridHandler.gridAg.setData(this.rows);
            });
        });
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.setMinRowsToShow(20);
        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.addColumnText("name", this.terms["common.name"], null, false);
        this.gridHandler.gridAg.addColumnText("timePeriodHeadName", this.terms["time.time.timeperiod.timeperiodhead"], null, false);
        this.gridHandler.gridAg.addColumnText("timePeriodName", this.terms["time.time.timeperiod.timeperiod"], null, false);
        this.gridHandler.gridAg.addColumnDate("dateFrom", this.terms["common.fromdate"], 136, true, null, { suppressSizeToFit: true })
        this.gridHandler.gridAg.addColumnDate("dateTo", this.terms["common.todate"], 136, true, null, { suppressSizeToFit: true })
        this.gridHandler.gridAg.addColumnNumber("nrOfEmployees", this.terms["time.time.attest.nrofemployees"], 100, { suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnEdit(this.terms["core.edit"], this.editRetroactivePayroll.bind(this));

        this.gridHandler.gridAg.finalizeInitGrid("time.payroll.payrollcalculation.calculation", true);
    }

    // SERVICE CALLS

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.edit",
            "common.fromdate",
            "common.name",
            "common.todate",
            "time.time.attest.nrofemployees",
            "time.time.timeperiod.timeperiod",
            "time.time.timeperiod.timeperiodhead",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    // DIALOGS

    private editRetroactivePayroll(retroactivePayroll: RetroactivePayrollDTO) {
        this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_EMPLOYEE_RETROACTIVE_EDIT, retroactivePayroll);
    }
}