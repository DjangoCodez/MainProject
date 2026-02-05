import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { SoeOriginType, TermGroup_BillingType } from "../../../Util/CommonEnumerations";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";


export class SelectCustomerInvoiceController {
    private searching: boolean = false;
    private timeout = null;
    private name: string;
    private number: string;
    private customerNr: string;
    private customerName: string;
    private managerName: string;
    private orderNr: string;
    private allInvoices: any[];
    private setupFinished: boolean = false;

    // Grid
    private gridHandler: EmbeddedGridController;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private gridHandlerFactory: IGridHandlerFactory,
        private title: string,
        private isNew: boolean,
        private ignoreChildren: boolean,
        private originType: number,
        private customerId: number,
        private projectId: number,
        private invoiceId: number,
        private selectedProjectName: string,
        shortCutService: IShortCutService,
        $scope: ng.IScope,
        private currentMainInvoiceId: number,
        private userId?: number,
        private includePreliminary?: boolean,
        private includeVoucher?: boolean,
        private fullyPaid?: boolean,
        private useExternalInvoiceNr?: boolean,
        private importRow?: any) {

        shortCutService.bindEnterCloseDialog($scope, () => { this.buttonOkClick(); })
        console.log("import row", this.importRow)
    }

    private $onInit() {
        this.gridHandler = new EmbeddedGridController(this.gridHandlerFactory, "common.dialogs.searchcustomerinvoice");
        //this.soeGridOptions = SoeGridOptionsAg.create("common.dialogs.searchcustomerinvoice", this.$timeout);
        this.setupGrid();
        this.$timeout(() => {
            this.gridHandler.gridAg.options.setFilterFocus();
        });
    }

    public setupGrid() {

        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = true;
        this.gridHandler.gridAg.options.enableRowSelection = false;
        this.gridHandler.gridAg.options.ignoreResetFilterModel = true;
        this.gridHandler.gridAg.options.enableSingleSelection();
        this.gridHandler.gridAg.options.setMinRowsToShow(10);

        // Columns
        var keys: string[] = [
            "billing.order.ordernr",
            "common.customer.invoices.invoicenr",
            "billing.invoices.externalinvoicenr",
            "billing.import.edi.customernr",
            "common.customer.customer.customername",
            "common.customer.invoices.internaltext",
            "common.customer.invoices.projectnr",
            "common.report.selection.projectname",
            "common.customer.invoices.duedate",
            "common.customer.invoices.payableamount",
            "common.currency"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (this.originType === SoeOriginType.CustomerInvoice) {
                this.gridHandler.gridAg.addColumnText("number", terms["common.customer.invoices.invoicenr"], null, false, { suppressFilter: true });
                if (this.useExternalInvoiceNr)
                    this.gridHandler.gridAg.addColumnText("externalNr", terms["billing.invoices.externalinvoicenr"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("customerNr", terms["billing.import.edi.customernr"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("customerName", terms["common.customer.customer.customername"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnDate("dueDate", terms["common.customer.invoices.duedate"], null, true);
                this.gridHandler.gridAg.addColumnNumber("balance", terms["common.customer.invoices.payableamount"], null, { decimals: 2 });
                this.gridHandler.gridAg.addColumnText("currencyCode", terms["common.currency"], null);
            }
            else {
                this.gridHandler.gridAg.addColumnText("number", terms["billing.order.ordernr"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("customerNr", terms["billing.import.edi.customernr"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("customerName", terms["common.customer.customer.customername"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("internalText", terms["common.customer.invoices.internaltext"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("projectNr", terms["common.customer.invoices.projectnr"], null, false, { suppressFilter: true });
                this.gridHandler.gridAg.addColumnText("projectName", terms["common.report.selection.projectname"], null, false, { suppressFilter: true });
            }

            this.gridHandler.gridAg.finalizeInitGrid("", true);

            // Events
            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
            this.gridHandler.gridAg.options.subscribe(events);

            events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, () => {
                if (!this.searching)
                    this.loadInvoices();
            }));

            if ((this.customerId && this.customerId > 0) || (this.projectId && this.projectId > 0) || (this.currentMainInvoiceId && this.currentMainInvoiceId > 0)) {
                this.$timeout(() => {
                    this.loadInvoices();
                });
            }

            this.setupFinished = true;
        });
    }

    private loadInvoices(): ng.IPromise<any> {
        var filterModels = this.gridHandler.gridAg.options.getFilterModels();

        if (!filterModels) {
            return;
        }
     
        this.searching = true;
        this.gridHandler.gridAg.options.setData([]);
        var columnValueNumber = filterModels["number"] ? filterModels["number"].filter : "";
        var columnValueCustomerNumber = filterModels["customerNr"] ? filterModels["customerNr"].filter : "";
        var columnValueCustomerName = filterModels["customerName"] ? filterModels["customerName"].filter : "";
        var columnValueInternalText = filterModels["internalText"] ? filterModels["internalText"].filter : "";
        var columnValueProjectNr = filterModels["projectNr"] ? filterModels["projectNr"].filter : "";
        var columnValueProjectName = filterModels["projectName"] ? filterModels["projectName"].filter : "";
        var columnValueExternalNr = filterModels["externalNr"] ? filterModels["externalNr"].filter : "";
        return this.coreService.getCustomerInvoicesBySearch(columnValueNumber, columnValueExternalNr, columnValueCustomerNumber, columnValueCustomerName, columnValueInternalText, columnValueProjectNr, columnValueProjectName, this.originType, this.ignoreChildren, this.customerId && this.customerId > 0 ? this.customerId : undefined, this.projectId && this.projectId > 0 ? this.projectId : undefined, this.invoiceId && this.invoiceId > 0 ? this.invoiceId : undefined, this.userId > 0 ? this.userId : undefined, this.includePreliminary, this.includeVoucher, this.fullyPaid).then(x => {
            if (this.currentMainInvoiceId && this.currentMainInvoiceId > 0) {
                var current = _.filter(x, (i) => i.customerInvoiceId === this.currentMainInvoiceId);
                var rest = _.filter(x, (i) => i.customerInvoiceId !== this.currentMainInvoiceId);
                this.allInvoices = current.concat(rest);
            }
            else {
                this.allInvoices = x;
            }

            _.forEach(this.allInvoices, (i) => {                
                i['balance'] = i.totalAmount - i.paidAmount;                
            });

            this.gridHandler.gridAg.options.setData(this.allInvoices)
            this.searching = false;
            if (this.currentMainInvoiceId) {
                this.selectMainInvoice();
            }
            else {
                this.selectFirstRow();
            }
        }).then(() => this.selectRow())
    }

    protected edit(row) {
        this.buttonOkClick();
    }

    selectMainInvoice() {
        this.$timeout(() => {
            var rowToSelect = _.find(this.allInvoices, { 'customerInvoiceId': this.currentMainInvoiceId });
            if (rowToSelect)
                this.gridHandler.gridAg.options.selectRow(rowToSelect);
            else
                this.selectFirstRow();
        });
    }

    selectFirstRow() {
        if (this.allInvoices.length > 0 && !this.searching) {
            var row: any = this.gridHandler.gridAg.options.selectRowByVisibleIndex(0)
            if (row) {
                this.gridHandler.gridAg.options.selectRow(row)
            }
        }
    }

    selectRow() {
        if (this.currentMainInvoiceId) {
            var rowToSelect = _.find(this.allInvoices, { 'customerInvoiceId': this.currentMainInvoiceId });
            if (rowToSelect)
                this.gridHandler.gridAg.options.selectRow(rowToSelect);
            else
                this.selectFirstRow();
        }
    }

    buttonRemoveClick() {
        this.$uibModalInstance.close({ remove: true });
    }

    buttonOkCopyClick() {
        var invoices = this.gridHandler.gridAg.options.getSelectedRows();
        if (invoices[0])
            this.$uibModalInstance.close({ invoice: invoices[0], copy: true });
    }

    buttonOkClick() {
        var invoices = this.gridHandler.gridAg.options.getSelectedRows();
        if (invoices[0])
            this.$uibModalInstance.close({ invoice: invoices[0], copy: false });
        else
            this.$uibModalInstance.close({ invoice: {}, copy: false })
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    clearProject() {
        this.projectId = undefined;
        this.selectedProjectName = "";
        this.gridHandler.gridAg.options.setFilterFocus();
        this.loadInvoices();
    }
}