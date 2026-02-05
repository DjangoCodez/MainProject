import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { InvoiceProductDialogController } from "./InvoiceProductDialogController";
import { ITimeService } from "../../../../Time/TimeService";
import { TermGroup_InvoiceProductVatType } from "../../../../../Util/CommonEnumerations";
import { TimeAccumulatorInvoiceProductDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class InvoiceProductsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeAccumulators/Directives/InvoiceProducts/InvoiceProducts.html'),
            scope: {
                invoiceProducts: '=',
                readOnly: '=',
                onChange: '&',
            },
            restrict: 'E',
            replace: true,
            controller: InvoiceProductsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class InvoiceProductsController {

    // Init parameters
    private invoiceProducts: TimeAccumulatorInvoiceProductDTO[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedInvoiceProduct: TimeAccumulatorInvoiceProductDTO;
    private products: ISmallGenericType[];

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private timeService: ITimeService,
        private urlHelperService: IUrlHelperService) {

        this.loadProducts().then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.invoiceProducts, (newVal, oldVal) => {
            this.selectedInvoiceProduct = this.invoiceProducts && this.invoiceProducts.length > 0 ? this.invoiceProducts[0] : null;
            this.setProductNames();
        });
    }

    // SERVICE CALLS

    private loadProducts(): ng.IPromise<any> {
        return this.timeService.getInvoiceProductsDict(TermGroup_InvoiceProductVatType.Service, true, true).then(x => {
            this.products = x;
        });
    }

    // EVENTS

    private editProduct(invoiceProduct: TimeAccumulatorInvoiceProductDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAccumulators/Directives/InvoiceProducts/InvoiceProductDialog.html"),
            controller: InvoiceProductDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                products: () => { return this.products },
                product: () => { return invoiceProduct },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.product) {
                if (!invoiceProduct) {
                    // Add new
                    invoiceProduct = new TimeAccumulatorInvoiceProductDTO();
                    if (!this.invoiceProducts)
                        this.invoiceProducts = [];
                    this.invoiceProducts.push(invoiceProduct);
                }

                // Update fields
                invoiceProduct.invoiceProductId = result.product.invoiceProductId;
                invoiceProduct.factor = result.product.factor;
                this.setProductName(invoiceProduct);
                this.selectedInvoiceProduct = invoiceProduct;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteProduct(invoiceProduct: TimeAccumulatorInvoiceProductDTO) {
        _.pull(this.invoiceProducts, invoiceProduct);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setProductNames() {
        _.forEach(this.invoiceProducts, invoiceProduct => {
            this.setProductName(invoiceProduct);
        });
    }

    private setProductName(product: TimeAccumulatorInvoiceProductDTO) {
        let prod = _.find(this.products, p => p.id === product.invoiceProductId);
        product.productName = prod ? prod.name : '';
    }
}