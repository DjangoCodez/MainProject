import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { ISelectSupplierService } from "../../../Common/Dialogs/SelectSupplier/selectsupplierservice";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { SelectSupplierController } from "../../../Common/Dialogs/SelectSupplier/SelectSupplierController";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController as SupplierEditController } from "../../../Shared/Economy/Supplier/Suppliers/EditController";
import { Feature, SettingMainType, SoeOriginStatusClassification, TermGroup, UserSettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";



export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private modalInstance: any;

    private supplierId: number;
    private supplier: any;
    //private suplierIdParameter: number = 0;

    // Terms
    terms: { [index: string]: string; };

    // Permissions
    supplierSupplierPermission = false;
    supplierSupplierSuppliersEditPermission = false;
    supplierInvoicePermission = false;
    supplierInvoiceInvoiceInvoicesEditPermission = false;
    supplierInvoiceStatusForeignPermission = false;
    supplierInvoiceStatusAttestFlowPermission = false;

    // Data
    supplierNumber: string;
    supplierName: string = "";
    supplierAddress: string;
    supplierPhone: string;

    supplierPaymentsSupplierCentralUnpayed: number;
    supplierPaymentsSupplierCentralUnpayedExVat: number;
    supplierInvoicesOverdue: number;
    supplierInvoicesOverdueExVat: number;
    supplierPaymentsSupplierCentralPayed: number;
    supplierPaymentsSupplierCentralPayedExVat: number;
    supplierInvoicesAmountTotal: number;
    supplierInvoicesAmountTotalExVat: number;

    supplierPaymentsSupplierCentralUnpayedForeign: number;
    supplierPaymentsSupplierCentralUnpayedForeignExVat: number;
    supplierInvoicesOverdueForeign: number;
    supplierInvoicesOverdueForeignExVat: number;
    supplierPaymentsSupplierCentralPayedForeign: number;
    supplierPaymentsSupplierCentralPayedForeignExVat: number;
    supplierInvoicesForeignAmountTotal: number;
    supplierInvoicesForeignAmountTotalExVat: number;

    hasCurrencyPermission: boolean;
    allItemsSelectionDict: any[];
    invoiceBillingTypes: any[];
    originStatus: any[];
    setupComplete = false;
    filteredTotal = 0;
    selectedTotal = 0;
    filteredTotalExVat = 0;
    selectedTotalIncVat = 0;
    filteredTotalIncVat = 0;
    selectedTotalExVat = 0;

    private _showVatFree = true;
    currencies: any;
    yesNoDict: { yes: string; no: string; };
    get showVatFree() {
        return this._showVatFree;
    }
    set showVatFree(item: any) {
        this._showVatFree = item;
        if (this.showVatFree) {
            this.filteredTotal = this.filteredTotalIncVat;
            this.selectedTotal = this.selectedTotalIncVat;
        } else {
            this.filteredTotal = this.filteredTotalExVat;
            this.selectedTotal = this.selectedTotalExVat;
        }
    }

    private _allItemsSelection: any;

    get allItemsSelection() {
        return this._allItemsSelection;
    }

    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this._allItemsSelection)
            this.updateItemsSelection();
    }

    private _loadOpen = true;

    get loadOpen() {
        return this._loadOpen;
    }

    set loadOpen(item: any) {
        this._loadOpen = item;
        if (this._loadOpen)
            this.loadGridData();
    }

    private _loadClosed = false;

    get loadClosed() {
        return this._loadClosed;
    }

    set loadClosed(item: any) {
        this._loadClosed = item;
        if (this._loadClosed)
            this.loadGridData();
    }

    //Grids
    private gridHandler: EmbeddedGridController;

    //@ngInject
    constructor(
        private $scope,
        $uibModal,
        private coreService: ICoreService,
        private invoiceService: IInvoiceService,
        private supplierService: ISupplierService,
        private selectSupplierService: ISelectSupplierService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            //.onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.doLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.modalInstance = $uibModal;
        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "invoiceGrid");
        this.gridHandler.gridAg.options.setMinRowsToShow(15);
    }

    public onInit(parameters: any) {
        this.supplierId = soeConfig.supplierId ? soeConfig.supplierId : (parameters.id || 0);
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.flowHandler.start([{ feature: Feature.Economy_Customer_Customers, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadTerms(),
            () => this.loadModifyPermissions(),
            () => this.loadOriginStatus(),
            () => this.loadSelectionTypes(),
            () => this.loadInvoiceBillingTypes(),
            () => this.loadCurrencies(),
        ]).then(
            () => this.setupGrids()
        ).then(
            () => this.startAction(),
        )
    }

    private startAction() {
        if (this.supplierId && this.supplierId > 0)
            this.showSupplier();
        else
            this.showSelectSupplier();
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "economy.supplier.supplier.supplier",
            "economy.supplier.invoice.new",
            "core.yes",
            "core.no",
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private getPaymentCondition(paymentConditionId: number) {
        if (!paymentConditionId) {
            return;
        }
        this.invoiceService.getPaymentCondition(paymentConditionId).then((x) => {
            this.supplier.paymentConditionName = x.name;
        });
    }

    protected onCreateToolbar(toolbarFactory: IToolbarFactory) {
        //if (super.setupDefaultToolBar(true)) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew)
        if (this.toolbar) {
            const groupSearchSupplier = ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.suppliercentral.seeksupplierbutton", "economy.supplier.suppliercentral.seeksupplierbutton", IconLibrary.FontAwesome, "fa-search",
                () => { this.seekSupplier(); },
                null,
                null,
                { buttonClass: "ngSoeMainButton pull-left" }));
            this.toolbar.addButtonGroup(groupSearchSupplier);

            const groupCreate = ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.createinvoice", "economy.supplier.invoice.createinvoice", IconLibrary.FontAwesome, "fa-plus", () => {
                this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
                    name: this.terms["economy.supplier.invoice.new"],
                    supplierId: this.supplierId,
                    keepOpen: true,
                });
            }));


            this.toolbar.addButtonGroup(groupCreate);
        }
    }

    private setupGrids() {
        const keys: string[] = [
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

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridHandler.gridAg.addColumnNumber("seqNr", terms["economy.supplier.invoice.seqnr"], null, { clearZero: true });
            this.gridHandler.gridAg.addColumnText("invoiceNr", terms["economy.supplier.invoice.invoicenr"], null);
            this.gridHandler.gridAg.addColumnText("billingTypeName", terms["economy.supplier.invoice.invoicetype"], null);
            this.gridHandler.gridAg.addColumnText("attestStateName", terms["economy.supplier.invoice.attest"], null);
            this.gridHandler.gridAg.addColumnText("statusName", terms["common.tracerows.status"], null) 
            this.gridHandler.gridAg.addColumnText("supplierName", terms["economy.supplier.supplier.supplier"], null);
            this.gridHandler.gridAg.addColumnNumber("totalAmountExVat", terms["economy.supplier.invoice.amountexvat"], null, { decimals: 2 });
            this.gridHandler.gridAg.addColumnNumber("totalAmount", terms["economy.supplier.invoice.amountincvat"], null, { decimals: 2 });
            this.gridHandler.gridAg.addColumnNumber("payAmount", terms["economy.supplier.invoice.remainingamount"], null, { decimals: 2 });
            if (this.hasCurrencyPermission) {
                this.gridHandler.gridAg.addColumnNumber("totalAmountCurrency", terms["economy.supplier.invoice.foreignamount"], null, { decimals: 2 });
                this.gridHandler.gridAg.addColumnNumber("payAmountCurrency", terms["economy.supplier.invoice.foreignremainingamount"], null, { decimals: 2 });
                this.gridHandler.gridAg.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null);
            }
            this.gridHandler.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.invoicedate"], null);
            this.gridHandler.gridAg.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null);
            this.gridHandler.gridAg.addColumnDate("payDate", terms["economy.supplier.invoice.paiddate"], null);

            this.gridHandler.gridAg.addColumnEdit(terms["core.edit"], this.openEdit.bind(this));
            this.gridHandler.gridAg.options.getColumnDefs().forEach(f => {
                // Append closedRow to cellClass
                const cellcls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                    return cellcls + (row.entity.useClosedStyle ? " closedRow" : "");
                };
            });


            this.gridHandler.gridAg.options.getColumnDefs()
                .forEach(f => {
                    // Append closedRow to cellClass
                    var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                    f.cellClass = (item: any) => {
                        return cellCls + (item.data.useClosedStyle ? " closedRow" : "");
                    };
                });

            const events: GridEvent[] = [
                new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows) => { this.summarizeFiltered(rows); }),
                new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row) => { this.summarizeSelected(); }),
                new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.openEdit(row); }),
            ];

            this.gridHandler.gridAg.options.subscribe(events);

            this.gridHandler.gridAg.finalizeInitGrid("economy.supplier.invoice.invoices", false);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.SupplierInvoiceAllItemsSelection];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.SupplierInvoiceAllItemsSelection, 1);
        });
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();
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
        const deferral = this.$q.defer();
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
        if (!this.supplierId || this.supplierId === 0)
            return;

        // Load data
        this.supplierService.getInvoicesForSupplier(this.loadOpen, this.loadClosed, false, this.allItemsSelection, this.supplierId).then(data => {
            this.gridHandler.gridAg.setData(data);
            this.summarize(data);
        });
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.SupplierInvoiceAllItemsSelection, this.allItemsSelection).then((x) => {
            this.loadGridData();
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
            this.allItemsSelection = 1;
            deferral.resolve();

        });
        return deferral.promise;
    }

    private summarize(filtered) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        filtered.forEach(row => {
            this.filteredTotalIncVat += row.totalAmount;
            this.filteredTotalExVat += row.totalAmountExVat;
        });
        if (this.showVatFree)
            this.filteredTotal = this.filteredTotalIncVat;
        else
            this.filteredTotal = this.filteredTotalExVat;
    }

    private summarizeFiltered(x) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        x.forEach(row => {
            this.filteredTotalIncVat += row.totalAmount;
            this.filteredTotalExVat += row.totalAmountExVat;
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
        const rows = this.gridHandler.gridAg.options.getSelectedRows();
        rows.forEach(row => {
            this.selectedTotalIncVat += row.totalAmount;
            this.selectedTotalExVat += row.totalAmountExVat;
        });
        if (this.showVatFree)
            this.selectedTotal = this.selectedTotalIncVat;
        else
            this.selectedTotal = this.selectedTotalExVat;
        this.$scope.$applyAsync();
    }

    protected openEdit(row) {
        this.translationService.translate("economy.supplier.invoice.invoice").then((term) => {
            const message = new TabMessage(
                `${term} ${row.invoiceNr}`,
                row.supplierInvoiceId,
                SupplierInvoicesEditController,
                { id: row.supplierInvoiceId },
                this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
            );
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
        });
    }

    private seekSupplier() {
        this.showSelectSupplier();
    }

    private showSelectSupplier(): any {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectSupplier", "selectsupplier.html"),
            controller: SelectSupplierController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                selectSupplierService: () => { return this.selectSupplierService }
            }
        });

        modal.result.then(id => {
            if (id) {
                this.supplierId = id;
                this.showSupplier();
            }
        }, function () {
        });

        return modal;
    }

    public loadPageData() {
        //No page data is loaded            
        //this.stopProgress();
        this.progress.hideProgressDialog();
    }

    public openSupplier() {
        const message = new TabMessage(
            `${this.terms["economy.supplier.supplier.supplier"]} ${this.supplierName}`,
            this.supplierId,
            SupplierEditController,
            { id: this.supplierId },
            this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Suppliers/Views/edit.html")
        );
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }

    private showSupplier() {
        this.supplierService.getSupplier(this.supplierId, true, true, true, false).then((x) => {
            this.supplier = x;
            this.getPaymentCondition(this.supplier.paymentConditionId);
            this.supplierNumber = x.supplierNr;
            this.supplierName = x.name;
            this.supplier.currencyName = this.currencies.find(c => c.currencyId == this.supplier.currencyId).name;
            this.supplier.blockPaymentString = this.supplier.blockPayment == true ? this.terms['core.yes'] : this.terms['core.no']
            this.supplier.isPrivatePersonString = this.supplier.isPrivatePerson == true ? this.terms['core.yes'] : this.terms['core.no']
            
            const counterTypes: number[] = [
                SoeOriginStatusClassification.SupplierPaymentsSupplierCentralUnpayed,
                SoeOriginStatusClassification.SupplierInvoicesOverdue,
                SoeOriginStatusClassification.SupplierPaymentsSupplierCentralPayed,
                SoeOriginStatusClassification.SupplierPaymentsSupplierCentralUnpayedForeign,
                SoeOriginStatusClassification.SupplierInvoicesOverdueForeign,
                SoeOriginStatusClassification.SupplierPaymentsSupplierCentralPayedForeign
            ];
            this.supplierService.getSupplierCentralCountersAndBalance(counterTypes, this.supplierId, null, null).then((y) => {
                this.supplierPaymentsSupplierCentralUnpayed = (y[0].balanceTotal);
                this.supplierPaymentsSupplierCentralUnpayedExVat = (y[0].balanceExVat);
                this.supplierInvoicesOverdue = (y[2].balanceTotal);
                this.supplierInvoicesOverdueExVat = (y[2].balanceExVat);
                this.supplierPaymentsSupplierCentralPayed = (y[4].balanceTotal);
                this.supplierPaymentsSupplierCentralPayedExVat = (y[4].balanceExVat);
                this.supplierInvoicesAmountTotal = (y[0].balanceTotal + y[4].balanceTotal);
                this.supplierInvoicesAmountTotalExVat = (y[0].balanceExVat + y[4].balanceExVat);

                this.supplierPaymentsSupplierCentralUnpayedForeign = (y[1].balanceTotal);
                this.supplierPaymentsSupplierCentralUnpayedForeignExVat = (y[1].balanceExVat)
                this.supplierInvoicesOverdueForeign = (y[3].balanceTotal);
                this.supplierInvoicesOverdueForeignExVat = (y[3].balanceExVat);
                this.supplierPaymentsSupplierCentralPayedForeign = (y[5].balanceTotal);
                this.supplierPaymentsSupplierCentralPayedForeignExVat = (y[5].balanceExVat);
                this.supplierInvoicesForeignAmountTotal = (y[1].balanceTotal + y[5].balanceTotal);
                this.supplierInvoicesForeignAmountTotalExVat = (y[1].balanceExVat + y[5].balanceExVat);
            }, error => {
            });
        });
        this.loadGridData();
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Economy_Supplier,
            Feature.Economy_Supplier_Suppliers_Edit,
            Feature.Economy_Supplier_Invoice,
            Feature.Economy_Supplier_Invoice_Invoices_Edit,
            Feature.Economy_Supplier_Invoice_Status_Foreign,
            Feature.Economy_Supplier_Invoice_AttestFlow
        ];

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (x[Feature.Economy_Supplier])
                this.supplierSupplierPermission = true;
            if (x[Feature.Economy_Supplier_Suppliers_Edit])
                this.supplierSupplierSuppliersEditPermission = true;
            if (x[Feature.Economy_Supplier_Invoice])
                this.supplierInvoicePermission = true;
            if (x[Feature.Economy_Supplier_Invoice_Invoices_Edit])
                this.supplierInvoiceInvoiceInvoicesEditPermission = true;
            if (x[Feature.Economy_Supplier_Invoice_Status_Foreign])
                this.supplierInvoiceStatusForeignPermission = true;
            if (x[Feature.Economy_Supplier_Invoice_AttestFlow])
                this.supplierInvoiceStatusAttestFlowPermission = true;
        });
    }
}