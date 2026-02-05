import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TimeCodePayrollProductDialogController } from "./TimeCodePayrollProductDialogController";
import { TimeCodePayrollProductDTO } from "../../../Common/Models/TimeCode";
import { ITimeService } from "../../Time/TimeService";

export class TimeCodePayrollProductsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/TimeCodePayrollProducts/TimeCodePayrollProducts.html'),
            scope: {
                timeCodeProducts: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TimeCodePayrollProductsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class TimeCodePayrollProductsController {

    // Init parameters
    private timeCodeProducts: TimeCodePayrollProductDTO[];
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private selectedTimeCodeProduct: TimeCodePayrollProductDTO;
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
        this.$scope.$watch(() => this.timeCodeProducts, (newVal, oldVal) => {
            this.selectedTimeCodeProduct = this.timeCodeProducts && this.timeCodeProducts.length > 0 ? this.timeCodeProducts[0] : null;
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

    private editProduct(timeCodeProduct: TimeCodePayrollProductDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/TimeCodePayrollProducts/TimeCodePayrollProductDialog.html"),
            controller: TimeCodePayrollProductDialogController,
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
                    timeCodeProduct = new TimeCodePayrollProductDTO();
                    if (!this.timeCodeProducts)
                        this.timeCodeProducts = [];
                    this.timeCodeProducts.push(timeCodeProduct);
                }

                // Update fields
                timeCodeProduct.payrollProductId = result.timeCodeProduct.payrollProductId;
                timeCodeProduct.factor = result.timeCodeProduct.factor;
                this.setProductName(timeCodeProduct);
                this.selectedTimeCodeProduct = timeCodeProduct;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteProduct(timeCodeProduct: TimeCodePayrollProductDTO) {
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

    private setProductName(timeCodeProduct: TimeCodePayrollProductDTO) {
        let prod = _.find(this.products, p => p.id === timeCodeProduct.payrollProductId);
        timeCodeProduct.productName = prod ? prod.name : '';
    }
}