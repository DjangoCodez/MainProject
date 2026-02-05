import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TimeCodeInvoiceProductDialogController } from "./TimeCodeInvoiceProductDialogController";
import { TimeCodeInvoiceProductDTO } from "../../../Common/Models/TimeCode";
import { ITimeService } from "../../Time/TimeService";
import { TermGroup_InvoiceProductVatType, SoeTimeCodeType, TermGroup_ExpenseType } from "../../../Util/CommonEnumerations";

export class TimeCodeInvoiceProductsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/TimeCodeInvoiceProducts/TimeCodeInvoiceProducts.html'),
            scope: {
                timeCodeProducts: '=',
                timeCodeType: '=',
                readOnly: '=',
                onChange: '&',
                expenseType: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: TimeCodeInvoiceProductsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class TimeCodeInvoiceProductsController {

    // Init parameters
    private timeCodeProducts: TimeCodeInvoiceProductDTO[];
    private timeCodeType: SoeTimeCodeType;
    private expenseType: TermGroup_ExpenseType;
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedTimeCodeProduct: TimeCodeInvoiceProductDTO;
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
        this.$scope.$watch(() => this.expenseType, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.loadProducts();
        });
        this.$scope.$watch(() => this.timeCodeProducts, (newVal, oldVal) => {
            this.selectedTimeCodeProduct = this.timeCodeProducts && this.timeCodeProducts.length > 0 ? this.timeCodeProducts[0] : null;
            this.setProductNames();
        });
    }

    // SERVICE CALLS

    private loadProducts(): ng.IPromise<any> {
        return this.timeService.getInvoiceProductsDict(this.timeCodeType === SoeTimeCodeType.Material || this.expenseType === TermGroup_ExpenseType.Expense ? TermGroup_InvoiceProductVatType.None : TermGroup_InvoiceProductVatType.Service, true, true).then(x => {
            this.products = x;
        });
    }

    // EVENTS

    private editProduct(timeCodeProduct: TimeCodeInvoiceProductDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/TimeCodeInvoiceProducts/TimeCodeInvoiceProductDialog.html"),
            controller: TimeCodeInvoiceProductDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                products: () => { return this.products },
                timeCodeProduct: () => { return timeCodeProduct },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.timeCodeProduct) {
                if (!timeCodeProduct) {
                    // Add new
                    timeCodeProduct = new TimeCodeInvoiceProductDTO();
                    if (!this.timeCodeProducts)
                        this.timeCodeProducts = [];
                    this.timeCodeProducts.push(timeCodeProduct);
                }

                // Update fields
                timeCodeProduct.invoiceProductId = result.timeCodeProduct.invoiceProductId;
                timeCodeProduct.factor = result.timeCodeProduct.factor;
                this.setProductName(timeCodeProduct);
                this.selectedTimeCodeProduct = timeCodeProduct;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteProduct(timeCodeProduct: TimeCodeInvoiceProductDTO) {
        _.pull(this.timeCodeProducts, timeCodeProduct);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setProductNames() {
        _.forEach(this.timeCodeProducts, timeCodeProduct => {
            this.setProductName(timeCodeProduct);
        });
    }

    private setProductName(timeCodeProduct: TimeCodeInvoiceProductDTO) {
        let prod = _.find(this.products, p => p.id === timeCodeProduct.invoiceProductId);
        timeCodeProduct.productName = prod ? prod.name : '';
    }
}