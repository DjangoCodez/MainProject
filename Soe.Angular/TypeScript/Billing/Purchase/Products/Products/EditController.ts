import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { SupplierProductDTO, SupplierProductPriceDTO, SupplierProductSaveDTO } from "../../../../Common/Models/SupplierProductDTO";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IActionResult, IProductDTO, IProductSmallDTO, IProductUnitSmallDTO, ISmallGenericType, ISupplierProductDTO } from "../../../../Scripts/TypeLite.Net4";
import { IProductService } from "../../../../Shared/Billing/Products/ProductService";
import { ISupplierProductService } from "../../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { Feature } from "../../../../Util/CommonEnumerations";
import { SupplierHelper } from "../../../../Shared/Billing/Purchase/Helpers/SupplierHelper";
import { EditController as ProductsEditController } from "../../../../Shared/Billing/Products/Products/EditController";
import { Constants } from "../../../../Util/Constants";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { IMessagingService } from "../../../../Core/Services/MessagingService";


export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private supplierProductId = 0;
    private supplierProduct: SupplierProductDTO;
    private priceRows: SupplierProductPriceDTO[] = [];
    private invoiceProductId: number = 0;

    private productUnits: IProductUnitSmallDTO[];
    private invoiceProducts: IProductDTO[];
    private supplierHelper: SupplierHelper;
    private countries: ISmallGenericType[];

    private supplierProductIds: number[];

    //gui
    private showNavigationButtons = true;
    private priceRowsRendered = false;

    private _selectedProduct;
    public get selectedProduct(): IProductSmallDTO {
        return this._selectedProduct;
    }
    public set selectedProduct(item: IProductSmallDTO) {
        this._selectedProduct = item;
        if (this.selectedProduct) {
            if (this.supplierProduct.productId !== this.selectedProduct.productId) {
                this.supplierProduct.productId = this.selectedProduct.productId;
            }
        }
    }

    private editInvoiceProductPermission: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private productService: IProductService,
        supplierService: ISupplierService,
        private supplierProductService: ISupplierProductService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private messagingService: IMessagingService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.supplierHelper = new SupplierHelper(this, coreService, translationService, supplierService, urlHelperService, $q, $scope, $uibModal, (supplier: SupplierDTO) => { this.supplierChanged(supplier); });
    }

    public onInit(parameters: any) {

        this.guid = parameters.guid;
        if (!parameters.createNew) {
            this.supplierProductId = parameters.id;
        }

        if (parameters.ids && parameters.ids.length > 0) {
            this.supplierProductIds = parameters.ids;
        }
        else {
            this.showNavigationButtons = false;
        }

        if (parameters.productId) {
            this.invoiceProductId = parameters.productId;
        }
                
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Products, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: false, loadModifyPermissions: true },
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Purchase_Products].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Products].modifyPermission;
        this.editInvoiceProductPermission = response[Feature.Billing_Product_Products_Edit].modifyPermission;

        this.supplierHelper.setPermissions(response);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        //Navigation
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null);

        this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (newsupplierProductId) => {
            this.supplierProductId = newsupplierProductId;
            this.loadData(true);
        }, this.supplierProductIds, this.supplierProductId);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadProductUnits(),
            this.loadSuppliers(),
            this.loadInvoiceProducts(),
            this.loadCountries()
        ]);
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierHelper.loadSuppliers(true);
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then((x) => {
            this.productUnits = x;
        });
    }

    private loadInvoiceProducts(): ng.IPromise<any> {
        return this.productService.getInvoiceProductsForSelect().then((x) => {
            this.invoiceProducts = x;

            // Set value to display in typeahead
            _.forEach(this.invoiceProducts, (p) => p['displayValue'] = p.number);

            if (this.invoiceProductId)
                this.setProduct(this.invoiceProductId, true);
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(true, false).then(x => {
            this.countries = x;
        });
    }

    private loadPriceRows() {
        if (this.supplierProductId) {
            this.progress.startLoadingProgress([() => this.supplierProductService.getSupplierProductPrices(this.supplierProductId).then((data: SupplierProductPriceDTO[]) => {

                const startDate = new Date("1901-01-02");
                const stopDate = new Date("9998-12-31");

                this.priceRows = data.map(r => {
                    const rowStart = CalendarUtility.convertToDate(r.startDate);
                    const rowEnd = CalendarUtility.convertToDate(r.endDate);
                    if (rowStart < startDate)
                        r.startDate = null;
                    if (rowEnd > stopDate)
                        r.endDate = null;

                    return r;
                });

            })]);
        }
        else {
            this.priceRows = [];
        }
    }

    private openPriceRowsExpander() {
        if (!this.priceRowsRendered) {
            this.priceRowsRendered = true;
            this.loadPriceRows();
        }
    }

    private supplierChanged(supplier: SupplierDTO) {
        if (!this.supplierProductId && supplier) {
            this.supplierProduct.supplierId = supplier.actorSupplierId;
        }
    }

    private setProduct(productId: number, isLoading = false) {
        if (isLoading && productId) {
            this._selectedProduct = this.invoiceProducts.find(x => x.productId === productId);
        }
    }

    private selectProduct(item) {
        this._selectedProduct = item;
    }

    private editProduct() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Products/Products/Views/edit.html"),
            controller: ProductsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {}
        });
        
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                sourceGuid: this.guid,
                id: this.selectedProduct?.productId,
            });
        });

        modal.result.then(product => {
            if (product) {
                product.numberName = `${product.number} ${product.name}`;
                if (this.invoiceProducts) {
                    let index = this.invoiceProducts.findIndex(p => p.productId === product.productId);
                    if (index === -1) this.invoiceProducts.push(product);
                    else this.invoiceProducts[index] = { ...this.invoiceProducts[index], ...product };
                }
                if (!this.selectedProduct || this.selectedProduct.productId !== product.productId) this.dirtyHandler.setDirty();
                this.selectedProduct = product;
            }
        });
    }
   
    private loadData(updateTab=false): ng.IPromise<any> {
        if (this.supplierProductId) {
            return this.progress.startLoadingProgress([
                () => this.supplierProductService.getSupplierProduct(this.supplierProductId).then((data: ISupplierProductDTO) => {
                    this.isNew = false;
                    this.supplierProduct = data;

                    this.setProduct(this.supplierProduct.productId, true);
                    this.performReload(true);

                    if (updateTab) {
                        this.updateTabCaption();
                    }
                })
            ])
        }
        else {
            this.new();
        }
    }

    private save(): ng.IPromise<any> {
        return this.progress.startSaveProgress((completion) => {

            const saveDto = new SupplierProductSaveDTO();
            saveDto.product = this.supplierProduct;
            saveDto.priceRows = this.priceRows.filter(r => r.isModified && (r.price || r.quantity || r.supplierProductPriceId));
            if (this.selectedProduct && this.selectedProduct.productId) {
                saveDto.product.productId = this.selectedProduct.productId;
            }

            this.supplierProductService.saveSupplierProduct(saveDto).then((result) => {
                if (result.success && !this.supplierProductId) {
                    this.supplierProductId = this.supplierProduct.supplierProductId = result.integerValue
                }

                if (result.success) {
                    completion.completed(this.getSaveEvent(), this.supplierProduct);
                    this.performReload(false);

                    this.messagingService.publish(Constants.EVENT_PURCHASE_PRODUCT_SAVED, {
                        productId: this.supplierProduct.productId,
                    });
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });

        }, this.guid).then(data => {
            this.dirtyHandler.clean();
        });
    }

    private performReload(force: boolean) {
        if (this.priceRowsRendered && (force || this.priceRows.find(r => r.isModified))) this.loadPriceRows();
        if (force) this.supplierHelper.loadSupplier(this.supplierProduct.supplierId, true);
    }

    private delete() {

        this.translationService.translate("billing.purchase.product.delete").then((term: string) => {
            this.progress.startDeleteProgress((completion) => {
                this.supplierProductService.deleteSupplierProduct(this.supplierProduct.supplierProductId).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed(this.supplierProduct, false, null);
                        this.new();

                        this.updateTabCaption();
                    }
                    else {
                        if (result.errorMessage) {
                            completion.failed(result.errorMessage);
                        }
                        else {
                            this.translationService.translate("billing.order.delete.notsuccess").then((term) => {
                                completion.failed(term);
                            })
                        }
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, null, term);
        });
    }

    private updateTabCaption() {
        const termKey = this.isNew ? "billing.purchase.product.new_product" : "billing.purchase.product.product";
        this.translationService.translate(termKey).then((term) => {
            if (this.isNew)
                this.messagingHandler.publishSetTabLabel(this.guid, term);
            else
                this.messagingHandler.publishSetTabLabel(this.guid, term + " " + this.supplierProduct.supplierProductNr);
        });
    }

    private new() {
        this.isNew = true;
        if (!this.invoiceProductId)
            this.selectedProduct = undefined;
        this.supplierHelper.selectedSupplier = undefined;
        this.supplierProduct = new SupplierProductDTO();
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this['edit'].$invalid;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;
        });
    }
}