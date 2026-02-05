import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { FixedPayrollRowDTO } from "../../../../../Common/Models/FixedPayrollRowDTO";
import { Constants } from "../../../../../Util/Constants";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { EditTimeHelper } from "../../../../../Common/Directives/TimeProjectReport/EditTimeHelper";
import { ICoreService } from "../../../../../Core/Services/CoreService";

export class PayrollCalculationFixedDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollCalculation/Directives/PayrollCalculationFixed/PayrollCalculationFixed.html'),
            scope: {
                rows: '=',
            },
            restrict: 'E',
            replace: true,
            controller: PayrollCalculationFixedController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class PayrollCalculationFixedController {

    // Terms
    private terms: { [index: string]: string; };

    // Init parameters
    private rows: FixedPayrollRowDTO[];

    // Lookups
    private payrollProducts: any[] = [];

    private gridHandler: EmbeddedGridController;
    editTimeHelper: EditTimeHelper;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        gridHandlerFactory: IGridHandlerFactory,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private payrollService: IPayrollService) {

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "PayrollCalculationFixed");

        this.editTimeHelper = new EditTimeHelper(coreService, this.$q, (id: number) => { return this.getPayrollProduct(id) });

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
        }, true);
    }

    private setupGrid() {
        const payrollProductOptions = this.editTimeHelper.createTypeAheadOptions("productId");
        payrollProductOptions.source = (filter) => this.filterProducts(filter);
        payrollProductOptions.displayField = "payrollProductNrAndName";
        payrollProductOptions.dataField = "payrollProductNrAndName";
        payrollProductOptions.minLength = 0;
        payrollProductOptions.delay = 0;
        payrollProductOptions.useScroll = true;

        this.gridHandler.gridAg.options.setMinRowsToShow(20);
        this.gridHandler.gridAg.options.enableRowSelection = false; 
        this.gridHandler.gridAg.addColumnTypeAhead("payrollProductNrAndName", this.terms["time.payrollproduct.payrollproduct"], 100, { typeAheadOptions: payrollProductOptions, editable: (row: FixedPayrollRowDTO) => { return !row.isReadOnly }, displayField: "payrollProductNrAndName", suppressSorting: false, suppressMovable: true });
        this.gridHandler.gridAg.addColumnDate("fromDate", this.terms["common.fromdate"], 136, true, null, { editable: (row: FixedPayrollRowDTO) => { return !row.isReadOnly }, suppressSizeToFit: true })
        this.gridHandler.gridAg.addColumnDate("toDate", this.terms["common.todate"], 136, true, null, { editable: (row: FixedPayrollRowDTO) => { return !row.isReadOnly }, suppressSizeToFit: true })
        this.gridHandler.gridAg.addColumnNumber("quantity", this.terms["common.quantity"], 70, { editable: (row: FixedPayrollRowDTO) => { return !row.isReadOnly }, decimals: 2, suppressSizeToFit: true });
        this.gridHandler.gridAg.addColumnBoolEx("isSpecifiedUnitPrice", this.terms["time.payroll.payrollcalculation.specifiedunitprice"], 70, { enableEdit: true, disabledField: "isReadOnly", suppressSizeToFit: true })
        this.gridHandler.gridAg.addColumnBoolEx("distribute", this.terms["time.payroll.payrollcalculation.distribute"], 70, { editable: true, enableEdit: true, disabledField: "isReadOnly", suppressSizeToFit: true })
        this.gridHandler.gridAg.addColumnNumber("unitPrice", this.terms["common.price"], 120, { editable: (row: FixedPayrollRowDTO) => { return !row.isReadOnly && row.isSpecifiedUnitPrice }, suppressSizeToFit: true, decimals: 2 });
        this.gridHandler.gridAg.addColumnNumber("vatAmount", this.terms["common.vat"], 120, { editable: (row: FixedPayrollRowDTO) => { return !row.isReadOnly && row.isSpecifiedUnitPrice }, suppressSizeToFit: true, decimals: 2 });
        this.gridHandler.gridAg.addColumnNumber("amount", this.terms["common.amount"], 120, { editable: false, suppressSizeToFit: true, decimals: 2, cellClassRules: { "errorColor": (row) => { return row.data.amount < 0 } } });
        this.gridHandler.gridAg.addColumnDelete(this.terms["core.delete"], this.deleteRow.bind(this), false, (row: FixedPayrollRowDTO) => { return !row.isReadOnly });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.finalizeInitGrid("time.payroll.payrollcalculation.calculation", true);
    }

    // SERVICE CALLS

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadPayrollProducts()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.delete",
            "common.amount",
            "common.fromdate",
            "common.todate",
            "common.price",
            "common.quantity",
            "common.vat",
            "time.payroll.payrollcalculation.distribute",
            "time.payroll.payrollcalculation.specifiedunitprice",
            "time.payrollproduct.payrollproduct",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadPayrollProducts() {
        this.payrollService.getPayrollProducts(false).then(x => {
            _.forEach(x, y => {
                if (y.useInPayroll)
                    this.payrollProducts.push({ id: y.productId, payrollProductNrAndName: y.number + ' ' + y.name })
            });
        });
    }
    
    private filterProducts(filter) {
        return _.orderBy(this.payrollProducts.filter(p => {
            return p.payrollProductNrAndName.contains(filter);
        }), 'payrollProductNrAndName');
    }

    private getPayrollProduct(id: number): any {
        return this.payrollProducts.find(e => e.id === id);
    }
    // EVENTS

    private afterCellEdit(row: FixedPayrollRowDTO, colDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        if (colDef.field === "quantity" || colDef.field === "unitPrice") {
            row.amount = row.quantity * row.unitPrice;
            this.gridHandler.gridAg.options.refreshRows(row);
        } else if (colDef.field === "payrollProductNrAndName") {
            const payrollRow = this.payrollProducts.find(x => x.payrollProductNrAndName === newValue);
            row.productId = payrollRow.id;
        }
    }

    private deleteRow(row: FixedPayrollRowDTO) {
        this.messagingService.publish(Constants.EVENT_PAYROLL_CALCULATION_FIXED_DELETE_ROW, row);
    }
}