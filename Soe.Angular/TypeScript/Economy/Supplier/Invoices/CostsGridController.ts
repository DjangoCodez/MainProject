import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { Feature, SoeOriginType, TermGroup, SettingMainType, UserSettingType, TermGroup_ChangeStatusGridAllItemsSelection } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { Constants } from "../../../Util/Constants";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { SupplierInvoiceCostOverviewDTO } from "../../../Common/Models/InvoiceDTO";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage, SOEMessageBoxSize, SupplierGridButtonFunctions } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IGenericType } from "../../../Scripts/TypeLite.Net4";
import { invalid } from "moment";
import { TransferSupplierInvoiceToOrderController } from "./Dialogs/TransferSupplierInvoiceToOrder/TransferSupplierInvoiceToOrder";

export class CostGridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups
    terms: { [index: string]: string; };
    originStatus: any[];
    allItemsSelectionDict: any[];

    // Data
    items: SupplierInvoiceCostOverviewDTO[];

    // Permissions
    hasCurrencyPermission = false;
    hasOpenPermission = false;
    hasClosedPermission = false;
    hasDraftToOriginPermission = false;
    hasOriginToVoucherPermission = false;
    hasBatchInvoicingPermission = false;

    // Properties
    private _notLinked: any;
    get notLinked() {
        return this._notLinked;
    }
    set notLinked(item: any) {
        this._notLinked = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _partiallyLinked: any;
    get partiallyLinked() {
        return this._partiallyLinked;
    }
    set partiallyLinked(item: any) {
        this._partiallyLinked = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    private _fullyLinked: any;
    get fullyLinked() {
        return this._fullyLinked;
    }
    set fullyLinked(item: any) {
        this._fullyLinked = item;
        if (this.setupComplete)
            this.reloadGridFromFilter();
    }

    // Settings
    private transferSupplierInvoiceRows: boolean;
    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete === true) {
            this.updateItemsSelection();
            this.reloadGridFromFilter();
        }
    }

    // Flags
    private setupComplete: boolean;
    private activated = false;

    // Sums
    sumLinkedToProject = 0;
    sumLinkedToOrder = 0;
    sumNotLinked = 0;

    // Grid header and footer
    toolbarInclude: any;
    gridFooterComponentUrl: any;

    // Functions
    buttonFunctions: any = [];
    //modal
    private modalInstance: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private supplierService: ISupplierService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Supplier.Invoices.CostOverview", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.doLookup())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.toolbarInclude = this.urlHelperService.getGlobalUrl("economy/supplier/invoices/views/costGridHeader.html");
        this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("economy/supplier/invoices/views/costGridFooter.html");
        this.modalInstance = $uibModal;

        this.onTabActivated(() => this.tabActivated());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
    }

    private tabActivated() {
        if (!this.activated) {
            this.flowHandler.start(this.getPermissions());
            this.activated = true;
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    private doLookup() {
        return this.$q.all([
            this.loadTerms(),
            this.loadUserSettings(),
            this.loadOriginStatus(),
            this.loadSelectionTypes(),
        ]).then(() => {
            this._notLinked = true;
            this._partiallyLinked = true;
            this._fullyLinked = false;
            this.setupComplete = true;

            if (this.hasBatchInvoicingPermission)
                this.buttonFunctions.push({ id: SupplierGridButtonFunctions.BatchOnwardInvoice, name: this.terms["economy.supplier.invoice.performbatchinvoicing"] });
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "economy.supplier.supplier.suppliernr.grid",
            "economy.supplier.supplier.suppliername.grid",
            "common.tracerows.status",
            "economy.supplier.invoice.liquidityplanning.sequencenr",
            "economy.supplier.invoice.invoicedate",
            "economy.supplier.invoice.duedate",
            "economy.supplier.invoice.invoicenr",
            "economy.supplier.invoice.amountexvat",
            "economy.supplier.invoice.ordernr",
            "common.customer.invoices.projectnr",
            "economy.supplier.invoice.orderamount",
            "economy.supplier.invoice.projectamount",
            "economy.supplier.invoice.invoice",
            "economy.accounting.distributioncode.diff",
            "economy.supplier.invoice.fullcost",
            "economy.supplier.invoice.partialcost",
            "economy.supplier.invoice.nocost",
            "economy.supplier.invoice.description",
            "economy.supplier.invoice.attestgroup",
            "economy.supplier.invoice.performbatchinvoicing"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private getPermissions(): any[] {
        const features: any[] = [
            { feature: Feature.Economy_Supplier_Invoice_Project, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices_All, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Status_Foreign, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Status_DraftToOrigin, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Status_OriginToVoucher, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_BatchInvoicing, loadReadPermissions: true, loadModifyPermissions: true }
        ];
        return features;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Supplier_Invoice_Project].readPermission;
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_Project].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();

        this.hasCurrencyPermission = response[Feature.Economy_Supplier_Invoice_Status_Foreign].modifyPermission;
        this.hasDraftToOriginPermission = response[Feature.Economy_Supplier_Invoice_Status_DraftToOrigin].modifyPermission;
        this.hasOriginToVoucherPermission = response[Feature.Economy_Supplier_Invoice_Status_OriginToVoucher].modifyPermission;
        this.hasBatchInvoicingPermission = response[Feature.Economy_Supplier_Invoice_Status_OriginToVoucher].modifyPermission;
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.SupplierInvoiceCostOverviewAllItemsSelection, UserSettingType.SupplierInvoiceTransferInvoiceRows];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.SupplierInvoiceCostOverviewAllItemsSelection, TermGroup_ChangeStatusGridAllItemsSelection.One_Month, false);
            this.transferSupplierInvoiceRows = SettingsUtility.getBoolUserSetting(x, UserSettingType.SupplierInvoiceTransferInvoiceRows, false);
        });
    }

    private loadOriginStatus(): ng.IPromise<any> {
        this.originStatus = [];
        return this.supplierService.getInvoiceAndPaymentStatus(SoeOriginType.SupplierInvoice, false).then((x) => {
            _.forEach(x, (row) => {
                this.originStatus.push({ value: row.name, label: row.id });
            });
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.SupplierInvoiceCostOverviewAllItemsSelection, this.allItemsSelection);
    }

    public setupGrid() {
        // Columns
        this.gridAg.addColumnText("supplierNr", this.terms["economy.supplier.supplier.suppliernr.grid"], null, true);
        this.gridAg.addColumnText("supplierName", this.terms["economy.supplier.supplier.suppliername.grid"], null, true);
        this.gridAg.addColumnSelect("statusName", this.terms["common.tracerows.status"], null, { enableHiding: true, displayField: "statusName", selectOptions: this.originStatus });
        this.gridAg.addColumnNumber("seqNr", this.terms["economy.supplier.invoice.liquidityplanning.sequencenr"], null, { alignLeft: true, formatAsText: true });
        this.gridAg.addColumnText("invoiceNr", this.terms["economy.supplier.invoice.invoicenr"], null);
        this.gridAg.addColumnText("internalText", this.terms["economy.supplier.invoice.description"], null, true, { hide: true });
        this.gridAg.addColumnDate("invoiceDate", this.terms["economy.supplier.invoice.invoicedate"], null, true);
        this.gridAg.addColumnDate("dueDate", this.terms["economy.supplier.invoice.duedate"], null, true);
        this.gridAg.addColumnNumber("totalAmountExVat", this.terms["economy.supplier.invoice.amountexvat"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnText("orderNr", this.terms["economy.supplier.invoice.ordernr"], null);
        this.gridAg.addColumnText("projectNr", this.terms["common.customer.invoices.projectnr"], null, true);
        this.gridAg.addColumnNumber("orderAmount", this.terms["economy.supplier.invoice.orderamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("projectAmount", this.terms["economy.supplier.invoice.projectamount"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("diffAmount", this.terms["economy.accounting.distributioncode.diff"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnNumber("diffPercent", this.terms["economy.accounting.distributioncode.diff"] + "%", null, { hide: true, enableHiding: true, decimals: 2 });
        this.gridAg.addColumnText("attestGroupName", this.terms["economy.supplier.invoice.attestgroup"], null, true, { hide: true });
        this.gridAg.addColumnIcon("diffIcon", null, 30, { enableHiding: true, toolTipField: "diffTooltip", showTooltipFieldInFilter: false, pinned: 'right' });

        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: SupplierInvoiceCostOverviewDTO[]) => {
             this.summarize(rows);
        }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("economy.supplier.paymentmethod.paymentmethods", true);
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getSupplierInvoicesCostOverview(this.notLinked, this.partiallyLinked, this.fullyLinked, this.allItemsSelection).then((data) => {
                this.items = data;
                _.forEach(this.items, (item) => {
                    if (item.totalAmountExVat === 0 || ((item.totalAmountExVat > 0 && item.diffAmount <= 0) || (item.totalAmountExVat < 0 && item.diffAmount >= 0))) {
                        item.diffIcon = "fas fa-comment-dollar okColor";
                        item.diffTooltip = this.terms["economy.supplier.invoice.fullcost"];
                    }
                    else if (item.totalAmountExVat === item.diffAmount) {
                        item.diffIcon = "fas fa-comment-dollar errorColor";
                        item.diffTooltip = this.terms["economy.supplier.invoice.nocost"];
                    }
                    else {
                        item.diffIcon = "fas fa-comment-dollar warningColor";
                        item.diffTooltip = this.terms["economy.supplier.invoice.partialcost"];
                    }
                });
                this.setData(this.items);
                this.summarize();
            });
        }]);
    }

    private summarize(rows: SupplierInvoiceCostOverviewDTO[] = null) {
        this.$timeout(() => {
            this.sumLinkedToProject = 0;
            this.sumLinkedToOrder = 0;
            this.sumNotLinked = 0;

            _.forEach(rows ? rows : this.items, (i: SupplierInvoiceCostOverviewDTO) => {
                this.sumLinkedToProject += i.projectAmount;
                this.sumLinkedToOrder += i.orderAmount;
                this.sumNotLinked += i.diffAmount;
            });
        });
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) {
            const message = new TabMessage(
                `${this.terms["economy.supplier.invoice.invoice"]} ${row.invoiceNr}`,
                row.supplierInvoiceId,
                SupplierInvoicesEditController,
                { id: row.supplierInvoiceId, ediType: row.ediType, ediEntryId: row.ediEntryId },
                this.urlHelperService.getGlobalUrl("/Shared/Economy/Supplier/Invoices/Views/edit.html")
            );
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
        }
    }

    private executeButtonFunction(option) {
        const keys: string[] = [
            "economy.supplier.invoice.batchinvoicingvalidsingle",
            "economy.supplier.invoice.batchinvoicingvalidmulti",
            "economy.supplier.invoice.batchinvoicinginvalidsingle",
            "economy.supplier.invoice.batchinvoicinginvalidmulti",
            "economy.supplier.invoice.batchinvoicingsuccesssingle",
            "economy.supplier.invoice.batchinvoicingsuccessmulti",
            "economy.supplier.invoice.batchinvoicingerrorsingle",
            "economy.supplier.invoice.batchinvoicingerrormulti",
            "core.verifyquestion",
            "core.continue",
            "core.warning",
            "economy.supplier.invoice.transfersupplierinvoicerows",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            let transfer: boolean = false;
            let validatedItems: SupplierInvoiceCostOverviewDTO[] = [];
            let validMessage: string = "";
            let invalidMessage: string = "";
            let successMessage: string = "";
            let errorMessage: string = "";

            const selectedItems = this.gridAg.options.getSelectedRows();

            switch (option.id) {
                case SupplierGridButtonFunctions.BatchOnwardInvoice:
                    _.forEach(selectedItems, (row: SupplierInvoiceCostOverviewDTO) => {
                        if (row.diffAmount !== 0) {
                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? terms["economy.supplier.invoice.batchinvoicingvalidmulti"] : terms["economy.supplier.invoice.batchinvoicingvalidsingle"];
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? terms["economy.supplier.invoice.batchinvoicinginvalidmulti"] : terms["economy.supplier.invoice.batchinvoicinginvalidsingle"];
                    successMessage = validatedItems.length > 1 ? terms["economy.supplier.invoice.batchinvoicingsuccessmulti"] : terms["economy.supplier.invoice.batchinvoicingsuccesssingle"];
                    errorMessage = validatedItems.length > 1 ? terms["economy.supplier.invoice.batchinvoicingerrormulti"] : terms["economy.supplier.invoice.batchinvoicingerrorsingle"];
                    transfer = true;
                    break;
            }

            const noOfValid: number = validatedItems.length;
            const noOfInvalid = selectedItems.length - validatedItems.length;

            // Items to transfer
            let itemsToTransfer: SupplierInvoiceCostOverviewDTO[] = validatedItems;

            let title: string = "";
            let text: string = "";
            let doTransfer: boolean = false;
            let image: SOEMessageBoxImage = SOEMessageBoxImage.None;
            let buttons: SOEMessageBoxButtons = SOEMessageBoxButtons.None;

            if (selectedItems.length === validatedItems.length) {
                    title = terms["core.verifyquestion"];

                    text = "";
                    text += noOfValid.toString() + " " + validMessage + "\n";
                    text += terms["core.continue"];

                    image = SOEMessageBoxImage.Question;
                    buttons = SOEMessageBoxButtons.OKCancel;

                    doTransfer = true;
            }
            else if (selectedItems.length > validatedItems.length) {
                if (noOfValid === 0) {
                    title = terms["core.warning"];

                    text = "";
                    text += (selectedItems.length - validatedItems.length).toString() + " " + invalidMessage + "\n";

                    image = SOEMessageBoxImage.Warning;
                    buttons = SOEMessageBoxButtons.OK;

                    doTransfer = false;
                }
                else {
                    title = terms["core.verifyquestion"];

                    text = "";
                    text += (selectedItems.length - validatedItems.length).toString() + " " + invalidMessage + "\n";
                    text += noOfValid.toString() + " " + validMessage + "\n";
                    text += terms["core.continue"];

                    image = SOEMessageBoxImage.Question;
                    buttons = SOEMessageBoxButtons.OKCancel;

                    doTransfer = true;
                }
            }

            console.log("infoText", text);

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Economy/Supplier/Invoices/Dialogs/TransferSupplierInvoiceToOrder/TransferSupplierInvoiceToOrder.html"),
                controller: TransferSupplierInvoiceToOrderController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'md',
                resolve: {
                    transferSupplierInvoiceRows: () => { return this.transferSupplierInvoiceRows },
                    useMiscProduct: () => { return true },
                    infoText: () => { return text },
                }
            });

            modal.result.then(result => {
                if (result) {
                    const items = [];
                    _.forEach(itemsToTransfer, (row: SupplierInvoiceCostOverviewDTO) => {
                        items.push({ field1: row.supplierInvoiceId, field2: row.diffAmount });
                    });
                    if (this.transferSupplierInvoiceRows !== result.transferSupplierInvoiceRows) {
                        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.SupplierInvoiceTransferInvoiceRows, this.transferSupplierInvoiceRows);
                    }

                    this.performBatchOnwardInvoicing(items, successMessage, errorMessage, result.transferSupplierInvoiceRows, result.useMiscProduct);
                }
            }, function () {
                //Cancelled
            });
        });
    }

    private performBatchOnwardInvoicing(items: any[], successMessage: string, errorMessage: string, transferSupplierInvoiceRows:boolean, useMiscProduct:boolean) {
        this.progress.startSaveProgress((completion) => {
            this.supplierService.transferSupplierInvoicesToOrder(items, transferSupplierInvoiceRows, useMiscProduct).then((result) => {
                if (result.success) {
                    completion.completed(null, null, null, items.length + " " + successMessage);
                }
                else {
                    completion.failed(result.errorMessage ? result.errorMessage : items.length + " " + errorMessage);
                }
            }, error => {
                completion.failed(items.length + " " + errorMessage);
            });
        }, null).then(data => {
            this.loadGridData();
        }, error => {
        });
    }
}
