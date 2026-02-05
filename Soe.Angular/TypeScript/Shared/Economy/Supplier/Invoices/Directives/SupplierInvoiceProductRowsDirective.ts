import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../SupplierService";
import { Feature, SoeOriginType, SupplierInvoiceRowType } from "../../../../../Util/CommonEnumerations";
import { SelectCustomerInvoiceController } from "../../../../../Common/Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { IActionResult, ISupplierInvoiceProductRowDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ICommonCustomerService } from "../../../../../Common/Customer/CommonCustomerService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { Constants } from "../../../../../Util/Constants";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";


export class SupplierInvoiceProductRowsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/supplierInvoiceProductRows.html"),
            scope: {
                supplierInvoiceId: '=',
                customerInvoiceName: '=',
                customerInvoiceId: '=',
            },
            restrict: 'E',
            replace: true,
            controller: SupplierProductRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class SupplierProductRowsController extends GridControllerBase2Ag implements ICompositionGridController {
    private supplierInvoiceId: number;
    private customerInvoiceId: number;
    private customerInvoiceName: string;
    private rows: ISupplierInvoiceProductRowDTO[];

    private wholesellerId: number;
    private wholesellers: any[];

    // Flags
    private finishedSetup = false

    //@ngInject
    constructor(
        private supplierService: ISupplierService,
        private commonCustomerService: ICommonCustomerService,
        protected $uibModal,
        protected coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        protected messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "common.Directives.SupplierInvoiceProductRows", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            //.onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())

        this.onInit({});
        this.setupWatchers();
    }

    //SETUP
    onInit(parameters: any) {
        this.parameters = parameters;
        this.loadWholesellers();

        this.flowHandler.start([
            { feature: Feature.Economy_Supplier_Invoice_ProductRows, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_ProductRows].modifyPermission;
        this.readPermission = response[Feature.Economy_Supplier_Invoice_ProductRows].readPermission;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.supplierInvoiceId, (newValue, oldValue) => {
            this.supplierInvoiceId = newValue;
            if (this.supplierInvoiceId && this.finishedSetup)
                this.loadSupplierProductRows();
        })
    }

    private openOrder(row: ISupplierInvoiceProductRowDTO) {
        this.translationService.translate("common.order").then(term => {
            const data = {
                originId: row.customerInvoiceId,
                name: term + " " + row.customerInvoiceNumber
            };
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, data);
        });
    }

    //GRID
    public setupGrid() {
        this.gridAg.options.enableRowSelection = true;
        let keys: string[] = [
            "common.sum",
            "economy.supplier.invoice.connectpurchaserowtooltip",
            "common.productnr",
            "common.quantity",
            "common.unit",
            "common.purchaseprice",
            "common.amount",
            "common.text",
            "common.customer.invoices.ordernr"
        ];

        return this.translationService.translateMany(keys).then((terms) => {

            this.gridAg.options.setSingelValueConfiguration([
                {
                    field: "text",
                    predicate: (data) => data.rowType === SupplierInvoiceRowType.TextRow,
                    editable: false,
                    spanTo: "amountCurrency"
                },
            ]);

            this.gridAg.addColumnIcon("rowTypeIcon", null, null, { pinned: "left", editable: false, enableHiding: false, suppressMovable: true, });
            this.gridAg.addColumnText("sellerProductNumber", terms["common.productnr"], null, false);
            this.gridAg.addColumnText("text", terms["common.text"], null, false, { enableHiding: false });
            this.gridAg.addColumnNumber("quantity", terms["common.quantity"], null,
                {
                    decimals: 2,
                    maxDecimals: 4,
                    enableHiding: false,
                    editable: false,
                });
            this.gridAg.addColumnText("unitCode", terms["common.unit"], null);
            this.gridAg.addColumnNumber("priceCurrency", terms["common.purchaseprice"], null,
                {
                    decimals: 2,
                    enableHiding: false,
                    editable: false
                });
            this.gridAg.addColumnNumber("amountCurrency", terms["common.amount"], null,
                {
                    decimals: 2,
                    enableHiding: false,
                    editable: false,
                });

            this.gridAg.addColumnText("customerInvoiceNumber", terms["common.customer.invoices.ordernr"], null, null, { buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => row && row.customerInvoiceId, callback: this.openOrder.bind(this) } });

            this.gridAg.finalizeInitGrid("common.directives.supplierPurchaseRows", true);

            if (this.supplierInvoiceId)
                this.loadSupplierProductRows();

            this.finishedSetup = true;
        });
    }

    private loadSupplierProductRows(): ng.IPromise<any> {
        return this.supplierService.getSupplierProductRows(this.supplierInvoiceId)
            .then(data => {
                this.rows = data;
                _.forEach(this.rows, (row) => {
                    row['rowTypeIcon'] = row.rowType === SupplierInvoiceRowType.ProductRow ? 'fal fa-box-alt' : 'fal fa-text';
                });
                this.gridAg.setData(data);
            })
    }

    private loadWholesellers(): ng.IPromise<any> {
        return this.commonCustomerService.getSysWholesellersDict(true).then(x => {
            this.wholesellers = x;
        });
    }

    private showOrderDialog() {
        if (!this.modifyPermission) return;
        
        let rows = this.gridAg.options.getSelectedRows() as ISupplierInvoiceProductRowDTO[];
        if (!rows || this.rows.length === 0) {
            rows = this.rows;
        }

        if (!rows || rows.length == 0) {
            return;
        }

        const ids = rows.map(r => r.supplierInvoiceProductRowId);
        
        this.translationService.translate("common.customer.invoices.selectorder").then((term) => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomerInvoice", "selectcustomerinvoice.html"),
                controller: SelectCustomerInvoiceController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => term,
                    isNew: () => false,
                    ignoreChildren: () => false,
                    originType: () => SoeOriginType.Order,
                    customerId: () => null,
                    projectId: () => undefined,
                    invoiceId: () => null,
                    selectedProjectName: () => null,
                    currentMainInvoiceId: () => null,
                    userId: () => null,
                    includePreliminary: () => null,
                    includeVoucher: () => null,
                    fullyPaid: () => null,
                    useExternalInvoiceNr: () => { return null },
                    importRow: () => { return null },
                }
            });

            modal.result.then(result => {
                if (result && result.invoice) {
                    this.startTransfer(result.invoice.customerInvoiceId, ids);
                }
            });
        });
    }

    private startTransfer(customerInvoiceId: number, supplierInvoiceProductRowIds: number[]) {
        this.translationService.translateMany(["core.continue", "economy.supplier.invoice.productrows.verifytransfer"]).then(terms => {
            this.notificationService.showDialog(
                terms["core.continue"],
                terms["economy.supplier.invoice.productrows.verifytransfer"].replace("{0}", String(supplierInvoiceProductRowIds.length)),
                SOEMessageBoxImage.Question,
                SOEMessageBoxButtons.OKCancel).result.then(val => {
                    if (val) {
                        this.performTransfer(customerInvoiceId, supplierInvoiceProductRowIds)
                    }
                })
        })
    }

    private performTransfer(customerInvoiceId: number, supplierInvoiceProductRowIds: number[]) {
        this.progress.startLoadingProgress([
            () => this.supplierService.transferSupplierProductRowsToOrder(this.supplierInvoiceId, customerInvoiceId, supplierInvoiceProductRowIds, this.wholesellerId)
                    .then((result) => {
                        this.handleTransferResponse(result);
                    })
            ])
    }

    private handleTransferResponse(response: IActionResult) {
        if (response.success) {
            this.loadSupplierProductRows();
        }
        else {
            this.translationService.translate("core.error").then(term => {
                this.notificationService.showDialogEx(term, response.errorMessage, SOEMessageBoxImage.Error);
            })
        }
    }

    // HELPERS
}
