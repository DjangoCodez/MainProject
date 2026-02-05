import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { PayrollProductDialogController } from "./PayrollProductDialogController";
import { ITimeService } from "../../../../Time/TimeService";
import { TimeAccumulatorPayrollProductDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class PayrollProductsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Time/TimeAccumulators/Directives/PayrollProducts/PayrollProducts.html'),
            scope: {
                payrollProducts: '=',
                readOnly: '=',
                onChange: '&',
            },
            restrict: 'E',
            replace: true,
            controller: PayrollProductsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollProductsController {

    // Init parameters
    private payrollProducts: TimeAccumulatorPayrollProductDTO[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedPayrollProduct: TimeAccumulatorPayrollProductDTO;
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
        this.$scope.$watch(() => this.payrollProducts, (newVal, oldVal) => {
            this.selectedPayrollProduct = this.payrollProducts && this.payrollProducts.length > 0 ? this.payrollProducts[0] : null;
            this.setProductNames();
        });
    }

    // SERVICE CALLS

    private loadProducts(): ng.IPromise<any> {
        return this.timeService.getPayrollProductsDict(true, false, true).then(x => {
            this.products = x;
        });
    }

    // EVENTS

    private editProduct(payrollProduct: TimeAccumulatorPayrollProductDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Time/TimeAccumulators/Directives/PayrollProducts/PayrollProductDialog.html"),
            controller: PayrollProductDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                products: () => { return this.products },
                product: () => { return payrollProduct },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.product) {
                if (!payrollProduct) {
                    // Add new
                    payrollProduct = new TimeAccumulatorPayrollProductDTO();
                    if (!this.payrollProducts)
                        this.payrollProducts = [];
                    this.payrollProducts.push(payrollProduct);
                }

                // Update fields
                payrollProduct.payrollProductId = result.product.payrollProductId;
                payrollProduct.factor = result.product.factor;
                this.setProductName(payrollProduct);
                this.selectedPayrollProduct = payrollProduct;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteProduct(payrollProduct: TimeAccumulatorPayrollProductDTO) {
        _.pull(this.payrollProducts, payrollProduct);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private setProductNames() {
        _.forEach(this.payrollProducts, payrollProduct => {
            this.setProductName(payrollProduct);
        });
    }

    private setProductName(product: TimeAccumulatorPayrollProductDTO) {
        let prod = _.find(this.products, p => p.id === product.payrollProductId);
        product.productName = prod ? prod.name : '';
    }
}