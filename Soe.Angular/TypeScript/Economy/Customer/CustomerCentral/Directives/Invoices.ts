import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature, TermGroup, SettingMainType, UserSettingType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";

export class CustomerInvoiceFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getViewUrl("Invoices.html"),
            restrict: 'E',
            replace: true,
            controller: CustomerInvoiceController,
            controllerAs: "ctrl",
            bindToController: true,
            scope: {
                supplierName: "="
            },
            link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
                scope.$watch(() => (ngModelController.supplierName), (newVAlue, oldvalue, scope) => {
                    if (newVAlue) {
                        ngModelController.loadGridData();
                    }
                }, true);
            }

        };
    }
}

export class CustomerInvoiceController extends GridControllerBase {
    hasCurrencyPermission: boolean;
    allItemsSelectionDict: any[];
    invoiceBillingTypes: any[];
    originStatus: any[];
    setupComplete: boolean;
    supplierName: string;
    filteredTotal: number = 0;
    selectedTotal: number = 0;
    filteredTotalExVat: number = 0;
    selectedTotalIncVat: number = 0;
    filteredTotalIncVat: number = 0;
    selectedTotalExVat: number = 0;
    showVatFree: boolean = true;
    private terms: { [index: string]: string; };

    // Properties
    private _loadOpen: any;

    get loadOpen() {
        return this._loadOpen;
    }

    set loadOpen(item: any) {
        this._loadOpen = item;
        if (this.setupComplete)
            this.loadGridData();
    }

    private _loadClosed: any;

    get loadClosed() {
        return this._loadClosed;
    }

    set loadClosed(item: any) {
        this._loadClosed = item;
        if (this.setupComplete)
            this.loadGridData();
    }

    private _allItemsSelection: any;

    get allItemsSelection() {
        return this._allItemsSelection;
    }

    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete)
            this.updateItemsSelection();
    }


    //@ngInject
    constructor($http,
        $templateCache,
        private $window,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        private reportService: IReportService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants, private $q: ng.IQService) {

        super("Economy.Supplier.Invoices", "economy.supplier.invoice.invoices", Feature.Economy_Supplier_Invoice_Status, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        this.loadOpen = true;
        this.loadClosed = false;
        $q.all([this.loadInvoiceBillingTypes(), this.loadOriginStatus(), this.loadSelectionTypes()]).then(() => {
            this.setupComplete = true;
            this.loadGridData();
        });
    }

    public setupGrid(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        // Columns
        var keys: string[] = [
            "common.type",
            "economy.supplier.invoice.seqnr",
            "economy.supplier.invoice.invoicenr",
            "economy.supplier.invoice.invoicetype",
            "common.tracerows.status",
            "economy.supplier.supplier.supplier",
            "economy.supplier.invoice.amountexvat",
            "economy.supplier.invoice.amountincvat",
            "economy.supplier.invoice.remainingamount",
            "economy.supplier.invoice.foreignamount",
            "economy.supplier.invoice.foreignremainingamount",
            "economy.supplier.invoice.currencycode",
            "economy.supplier.invoice.invoicedate",
            "economy.supplier.invoice.duedate",
            "economy.supplier.invoice.paiddate",
            "economy.supplier.invoice.attest",
            "economy.supplier.invoice.attestname",
            "core.edit",
            "economy.supplier.invoice.invoice"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            super.addColumnNumber("seqNr", terms["economy.supplier.invoice.seqnr"], null);
            super.addColumnText("invoiceNr", terms["economy.supplier.invoice.invoicenr"], null);
            super.addColumnSelect("billingTypeName", terms["economy.supplier.invoice.invoicetype"], null, this.invoiceBillingTypes);
            super.addColumnText("attestStateName", terms["economy.supplier.invoice.attest"], null);
            super.addColumnSelect("statusName", terms["common.tracerows.status"], null, this.originStatus);
            super.addColumnText("supplierName", terms["economy.supplier.supplier.supplier"], null);
            super.addColumnNumber("totalAmountExVat", terms["economy.supplier.invoice.amountexvat"], null, false, 2);
            super.addColumnNumber("totalAmount", terms["economy.supplier.invoice.amountincvat"], null, false, 2);
            super.addColumnNumber("payAmount", terms["economy.supplier.invoice.remainingamount"], null, false, 2);
            if (this.hasCurrencyPermission) {
                super.addColumnNumber("totalAmountCurrency", terms["economy.supplier.invoice.foreignamount"], null, false, 2);
                super.addColumnNumber("payAmountCurrency", terms["economy.supplier.invoice.foreignremainingamount"], null, false, 2);
                super.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null);
            }
            super.addColumnDate("invoiceDate", terms["economy.supplier.invoice.invoicedate"], null);
            super.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null);
            super.addColumnDate("payDate", terms["economy.supplier.invoice.paiddate"], null);

            this.soeGridOptions.addColumnEdit(terms["core.edit"], "openEdit");
            this.soeGridOptions.getColumnDefs().forEach(f => {
                // Append closedRow to cellClass
                var cellcls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                    return cellcls + (row.entity.useClosedStyle ? " closedRow" : "");
                };
            });

            var events: GridEvent[] = [];
            var eventRowBeforeSelectionChanged: GridEvent = new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => {
                this.summarizeSelected();
            });

            var eventRowBeforeSelectionChangedBatch: GridEvent = new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => {
                this.summarizeSelected();
            });

            this.soeGridOptions.rowSelectDisabledProperty = "useClosedStyle";
            events.push(eventRowBeforeSelectionChanged);
            events.push(eventRowBeforeSelectionChangedBatch);
            this.soeGridOptions.subscribe(events);
            deferral.resolve();
        });
        return deferral.promise;
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then((x) => {
            this.invoiceBillingTypes = [];
            _.forEach(x, (row) => {
                this.invoiceBillingTypes.push({ value: row.name, label: row.name });
            });
            deferral.resolve();
        });
        return deferral.promise;
    }

    private loadOriginStatus(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.coreService.getTermGroupContent(TermGroup.OriginStatus, false, false).then((x) => {
            this.originStatus = [];
            _.forEach(x, (row) => {
                this.originStatus.push({ value: row.name, label: row.name });
            });
            deferral.resolve();
        });
        return deferral.promise;
    }

    public loadGridData() {

        // Load data
        this.supplierService.getInvoicesForGrid(this.allItemsSelection, this.loadOpen, this.loadClosed).then((x: any[]) => {
            var filteredResult = x.filter((y) => y.supplierName === this.supplierName);
            _.forEach(filteredResult, (y) => {
                y.invoiceDate = CalendarUtility.toFormattedDate(y.invoiceDate);
                y.dueDate = CalendarUtility.toFormattedDate(y.dueDate);
                y.payDate = CalendarUtility.toFormattedDate(y.payDate);
                y.expandableDataIsLoaded = false;
                this.stopProgress();
            });
            super.gridDataLoaded(filteredResult);
            this.summarize(x);
        });
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.SupplierInvoiceAllItemsSelection, this.allItemsSelection).then((x) => {
            this.loadGridData();
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true).then((x) => {
            this.allItemsSelectionDict = x;
            this.allItemsSelection = 1;
            deferral.resolve();

        });
        return deferral.promise;
    }

    private summarize(x) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        _.forEach(x, (y: any) => {
            this.filteredTotalIncVat += y.totalAmount;
            this.filteredTotalExVat += y.totalAmountExVat;
        });
        if (this.showVatFree)
            this.filteredTotal = this.filteredTotalIncVat;
        else
            this.filteredTotal = this.filteredTotalExVat;
    }

    private summarizeSelected() {

        this.selectedTotal = 0;
        this.selectedTotalIncVat = 0;
        this.selectedTotalExVat = 0;
        var rows = this.soeGridOptions.getSelectedRows();
        _.forEach(rows, (y: any) => {
            this.selectedTotalIncVat += y.totalAmount;
            this.selectedTotalExVat += y.totalAmountExVat;
        });
        if (this.showVatFree)
            this.selectedTotal = this.selectedTotalIncVat;
        else
            this.selectedTotal = this.selectedTotalExVat;

    }

    private showVatFreeChanged() {

        if (this.showVatFree) {
            this.filteredTotal = this.filteredTotalIncVat;
            this.selectedTotal = this.selectedTotalIncVat;
        } else {
            this.filteredTotal = this.filteredTotalExVat;
            this.selectedTotal = this.selectedTotalExVat;
        }

    }

    protected openEdit(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITSUPPLIERINVOICE, {
            id: row.supplierInvoiceId,
            name: this.terms["economy.supplier.invoice.invoice"] + " " + row.seqNr,
        });
    }
}