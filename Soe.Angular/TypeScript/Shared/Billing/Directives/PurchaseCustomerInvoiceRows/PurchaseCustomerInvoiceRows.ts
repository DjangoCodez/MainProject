import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { Feature, PurchaseCustomerInvoiceViewType, TermGroup_AdjustQuantityByBreakTime, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { Guid } from "../../../../Util/StringUtility";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IOrderService } from "../../Orders/OrderService";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { ISoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { ICustomerInvoiceRowPurchaseDTO, IGenericType, IPurchaseGridDTO, IPurchaseRowGridDTO } from "../../../../Scripts/TypeLite.Net4";
import { IPurchaseService } from "../../Purchase/Purchase/PurchaseService";
import { AttestStateDTO } from "../../../../Common/Models/AttestStateDTO";
import { CustomerInvoiceAttestHelper } from "../../Helpers/CustomerInvoiceAttestHelper";
import { INotificationService } from "../../../../Core/Services/NotificationService";


type Terms = { [key: string]: string };

class PurchaseCustomerInvoiceRows extends GridControllerBase2Ag implements ICompositionGridController {
    guid: Guid;

    private purchaseId: number;
    private customerInvoiceId: number;
    private customerInvoiceRowId: number;
    private purchaseDeliveryId: number;

    private performReloadCb = this.reloadData;

    private terms: Terms = {};

    private attestHelper: CustomerInvoiceAttestHelper;

    get allowStatusChange(): boolean {
        return this.viewType === PurchaseCustomerInvoiceViewType.FromPurchase || this.viewType === PurchaseCustomerInvoiceViewType.FromPurchaseDelivery;
    }

    get id(): number {
        switch (this.viewType) {
            case PurchaseCustomerInvoiceViewType.FromPurchase:
                return this.purchaseId;
            case PurchaseCustomerInvoiceViewType.FromCustomerInvoice:
                return this.customerInvoiceId;
            case PurchaseCustomerInvoiceViewType.FromCustomerInvoiceRow:
                return this.customerInvoiceRowId;
            case PurchaseCustomerInvoiceViewType.FromPurchaseDelivery:
                return this.purchaseDeliveryId;
            default:
                return 0;
        }
    }
    get viewType(): PurchaseCustomerInvoiceViewType {
        if (this.purchaseId && this.purchaseId > 0)
            return PurchaseCustomerInvoiceViewType.FromPurchase;
        else if (this.customerInvoiceId && this.customerInvoiceId > 0)
            return PurchaseCustomerInvoiceViewType.FromCustomerInvoice;
        else if (this.customerInvoiceRowId && this.customerInvoiceRowId > 0)
            return PurchaseCustomerInvoiceViewType.FromCustomerInvoiceRow;
        else if (this.purchaseDeliveryId && this.purchaseDeliveryId > 0)
            return PurchaseCustomerInvoiceViewType.FromPurchaseDelivery;
        else
            return PurchaseCustomerInvoiceViewType.Unknown;
    }

    get useAttest(): boolean {
        return this.viewType === PurchaseCustomerInvoiceViewType.FromPurchase || this.viewType === PurchaseCustomerInvoiceViewType.FromPurchaseDelivery
    }

    get useDetail(): boolean {
        return this.viewType !== PurchaseCustomerInvoiceViewType.FromCustomerInvoiceRow;
    }

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private orderService: IOrderService,
        private purchaseService: IPurchaseService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Shared.Directives.PurchaseCustomerInvoiceRows", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.loadData())
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())

        this.$scope.$watchGroup([() => this.purchaseId, () => this.customerInvoiceId, () => this.customerInvoiceRowId, () => this.purchaseDeliveryId],
            (newval, oldval, scope) => {
                if (newval !== oldval)
                    this.reloadData();
            });
    }

    $onInit() {
        if (this.useAttest) {
            this.attestHelper = new CustomerInvoiceAttestHelper(this.$q, this.$timeout, this.coreService, this.translationService, this.notificationService, this.gridAg.options);
        }
        this.onInit({});
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        this.flowHandler.start([
            { feature: Feature.Billing_Order_SupplierInvoices, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }


    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Order_SupplierInvoices].readPermission;
        this.modifyPermission = response[Feature.Billing_Order_SupplierInvoices].modifyPermission;
    }

    private loadData(): ng.IPromise<any> {
        return this.purchaseService.getCustomerInvoicePurchase(this.viewType, this.id).then(data => {
            let rows: IPurchaseRowGridDTO[] | ICustomerInvoiceRowPurchaseDTO[] = [];
            if (this.useDetail) {
                rows = data.map(r => {
                    r["expander"] = "";
                    return r;
                });
            } else {
                rows = data.reduce((all, current) => {
                    return all.concat(current.purchaseRows);
                }, []);
            }
            this.setData(rows);
        })
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private doLookups(): ng.IPromise<any> {
        let lookups = [];

        if (this.attestHelper)
            lookups.push(this.attestHelper.setup(TermGroup_AttestEntity.Order));

        return this.$q.all(lookups);
    }

    public setupGrid(): void {
        const translationKeys: string[] = [
            "common.customer.invoices.seqnr",
            "common.unit",
            "common.status",
            "common.type",
            "common.text",
            "common.date",
            "common.report.selection.purchasenr",

            "billing.productrows.quantity",
            "billing.productrows.invoicequantity",
            "billing.productrows.productnr",
            "billing.productrows.purchasestatus",
            "billing.order.deliverydate",

            "billing.purchase.supplierno",
            "billing.purchase.suppliername",
            "billing.purchase.delivery.remainingqty",
            "billing.purchase.delivery.purchaseqty",
            "billing.purchaserows.quantity",
            "billing.purchaserows.deliveredquantity",
            "billing.purchaserows.wanteddeliverydate",
            "billing.purchaserows.accdeliverydate",
            "billing.purchaserows.deliverydate",
            "billing.projects.list.invoicedquantity",
        ];

        this.translationService.translateMany(translationKeys).then((terms: Terms) => {
            this.terms["billing.purchaserows.wanteddeliverydate"] = terms["billing.purchaserows.wanteddeliverydate"];
            this.terms["billing.purchaserows.accdeliverydate"] = terms["billing.purchaserows.accdeliverydate"];
            this.terms["billing.purchaserows.deliverydate"] = terms["billing.purchaserows.deliverydate"];

            this.gridAg.options.setMinRowsToShow(10);
            if (this.useDetail) {
                this.gridAg.enableMasterDetail(true, null, null, true);
                this.gridAg.options.setDetailCellDataCallback(({ data, successCallback }) => this.setDetailData(data, successCallback))

                this.setupPurchaseRowsGrid(this.gridAg.detailOptions, terms);
                this.setupCustomerInvoiceRowsGrid(this.gridAg.options, terms);

                this.gridAg.detailOptions.finalizeInitGrid();
            } else {
                this.setupPurchaseRowsGrid(this.gridAg.options, terms);
            }

            const events: GridEvent[] = [
                new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); })
            ];

            this.gridAg.options.subscribe(events);
            this.gridAg.finalizeInitGrid("Shared.Directives.PurchaseCustomerInvoiceRows", true);
        });
    }

    private setupPurchaseRowsGrid(grid: ISoeGridOptionsAg, terms: Terms) {
        grid.addColumnText("purchaseNr", terms["common.report.selection.purchasenr"], null);
        grid.addColumnText("purchaseStatusName", terms["billing.productrows.purchasestatus"], null);
        grid.addColumnText("supplierName", terms["billing.purchase.suppliername"], null);
        grid.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null);
        grid.addColumnText("productNr", terms["billing.productrows.productnr"], null);
        grid.addColumnText("text", terms["common.text"], null);
        grid.addColumnNumber("purchaseQuantity", terms["billing.purchase.delivery.purchaseqty"], null);
        grid.addColumnNumber("deliveredQuantity", terms["billing.purchaserows.deliveredquantity"], null);
        grid.addColumnNumber("remainingQuantity", terms["billing.purchase.delivery.remainingqty"], null);
        grid.addColumnText("unit", terms["common.unit"], null);
        grid.addColumnText("rowStatusName", terms["common.status"], null);
        grid.addColumnText("dateStatus", terms["common.type"], null);
        grid.addColumnNumber("date", terms["common.date"], null);
    }

    private setupCustomerInvoiceRowsGrid(grid: ISoeGridOptionsAg, terms: Terms) {
        const isFullyDelivered = {
            "successRow": ({ data }) => Math.abs(data.quantity) <= Math.abs(data.deliveredPurchaseQuantity),
        }

        grid.addColumnNumber("invoiceSeqNr", terms["common.customer.invoices.seqnr"], null);
        grid.addColumnText("productNr", terms["billing.productrows.productnr"], null);
        grid.addColumnText("text", terms["common.text"], null);
        grid.addColumnText("unit", terms["common.unit"], null);
        grid.addColumnNumber("quantity", terms["billing.productrows.quantity"], null, { cellClassRules: isFullyDelivered });
        grid.addColumnText("invoiceQuantity", terms["billing.productrows.invoicequantity"], null, { hide: true });
        //grid.addColumnText("invoicedQuantity", terms["billing.projects.list.invoicedquantity"], null);
        grid.addColumnNumber("deliveredPurchaseQuantity", terms["billing.purchaserows.deliveredquantity"], null, { cellClassRules: isFullyDelivered });
        grid.addColumnDate("deliveryDate", terms["billing.order.deliverydate"], null);
        grid.addColumnShape("attestStateColor", null, 30, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStatus", showIconField: "attestStateColor" })
    }

    private saveAttestState() {
        if (!this.attestHelper.selectedAttestState)
            return;

        // Get selected attest state
        let attestState: AttestStateDTO = this.attestHelper.getSelectedAttestState();
        this.progress.startLoadingProgress(([
            () => this.setAttestStatus(attestState.attestStateId),
        ]))
    }

    private setAttestStatus(attestStateId: number): ng.IPromise<any> {
        const rows = <ICustomerInvoiceRowPurchaseDTO[]>this.gridAg.options.getSelectedRows();

        const ids = rows.map(r => {
            return {
                field1: r.invoiceId,
                field2: r.customerInvoiceRowId
            }
        });

        return this.orderService.changeAttestStateOnOrderRows(ids, attestStateId).then((result) => {
            this.reloadData();
        })
    }

    private setDetailData(row: ICustomerInvoiceRowPurchaseDTO, successCallback: ((rows: any[]) => void)) {
        const rows = row.purchaseRows ?? [];
        rows.forEach(r => {
            if (r.deliveryDate) {
                r["date"] = r.deliveryDate;
                r["dateStatus"] = this.terms["billing.purchaserows.deliverydate"]
            }
            else if (r.confirmedDate) {
                r["date"] = r.confirmedDate;
                r["dateStatus"] = this.terms["billing.purchaserows.accdeliverydate"]
            }
            else {
                r["date"] = r.requestedDate;
                r["dateStatus"] = this.terms["billing.purchaserows.wanteddeliverydate"]
            }

            if (!r["date"]) r["date"] = null;

        })
        successCallback(rows);
    }

    private reloadData() {
        this.progress.startLoadingProgress([
            () => this.loadData(),
        ])
    }
}

export class PurchaseCustomerInvoiceRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Billing/Directives/PurchaseCustomerInvoiceRows/PurchaseCustomerInvoiceRows.html"),
            scope: {
                purchaseId: "=",
                customerInvoiceId: "=",
                customerInvoiceRowId: "=",
                purchaseDeliveryId: "=",
                performReloadCb: "=",
            },
            restrict: 'E',
            replace: true,
            controller: PurchaseCustomerInvoiceRows,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}