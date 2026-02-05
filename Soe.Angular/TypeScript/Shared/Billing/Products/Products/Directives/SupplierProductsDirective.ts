import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { ISupplierProductService } from "../../../Purchase/Purchase/SupplierProductService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { Constants } from "../../../../../Util/Constants";
import { ISupplierProductSearchDTO } from "../../../../../Scripts/TypeLite.Net4";

export class SupplierProductDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Products/Products/Directives/Views/SupplierProducts.html'),
            scope: {
                productId: '=',
            },
            restrict: 'E',
            replace: true,
            controller: SupplierProductDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class SupplierProductDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    private productId: number;
    private progressBusy: boolean;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private supplierProductService: ISupplierProductService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private $scope: ng.IScope) {

        super(gridHandlerFactory, "Billing.Products.Products.Views.SupplierProducts", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onSetUpGrid(() => this.setupGrid())

        this.onInit({});
    }

    onInit(parameters: any) {
        this.setupWatchers();
        this.setupPurchaseProductSavedListener();
        this.flowHandler.start([
            { feature: Feature.Billing_Purchase, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Billing_Purchase].modifyPermission;
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.productId, (oldValue, newValue) => {
            this.loadData();
        });
    }

    public setupGrid() {
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(8);
        this.setupGridColumns();
    }


    private setupGridColumns() {
        const keys: string[] = [
            "billing.purchase.supplierno",
            "billing.purchase.suppliername",
            "billing.purchase.product.supplieritemno",
            "billing.purchase.product.supplieritemname"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.gridAg.options.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null)
            this.gridAg.options.addColumnText("supplierName", terms["billing.purchase.suppliername"], null)
            this.gridAg.options.addColumnText("supplierProductNr", terms["billing.purchase.product.supplieritemno"], null)
            this.gridAg.options.addColumnText("supplierProductName", terms["billing.purchase.product.supplieritemname"], null)
            if (this.modifyPermission) {
                this.gridAg.options.addColumnEdit("", this.openProduct.bind(this));
            }
            this.gridAg.finalizeInitGrid("billing.product.stocks.stock", false)
        });
    }


    private loadData() {
        if (this.productId) {
            this.progressBusy = true;
            this.supplierProductService.getSupplierProducts({ invoiceProductId: this.productId } as ISupplierProductSearchDTO).then(rows => {
                this.gridAg.setData(rows);
                this.progressBusy = false;
            })
        }
    }

    private newProduct() {
        this.openProduct(null);
    }

    private openProduct(row) {
        //billing.purchase.product.new_product
        if (row === null) {
            this.translationService.translate("billing.purchase.product.new_product").then(term => {
                this.messagingService.publish(Constants.EVENT_OPEN_PURCHASE_PRODUCT, {
                    id: null, createNew: true, productId: this.productId, name: term 
                });
            })
        }
        else {
            this.translationService.translate("billing.purchase.product.product").then(term => {
                this.messagingService.publish(Constants.EVENT_OPEN_PURCHASE_PRODUCT, {
                    id: row.supplierProductId, name: term + " " + row.supplierProductNr
                });
            })
        }
    }

    private setupPurchaseProductSavedListener() {
        const purchaseProductSavedEvent = this.messagingService.subscribe(Constants.EVENT_PURCHASE_PRODUCT_SAVED, (data: any) => {
            if (data && data.productId === this.productId) {
                this.loadData();
            }
        });

        this.$scope.$on('$destroy', () => {
            if (purchaseProductSavedEvent) {
                purchaseProductSavedEvent.unsubscribe();
            }
        });
    }

}