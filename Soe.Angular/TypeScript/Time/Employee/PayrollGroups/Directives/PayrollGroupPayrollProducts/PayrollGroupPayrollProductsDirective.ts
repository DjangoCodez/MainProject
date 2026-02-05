import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { PayrollGroupPayrollProductDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { PayrollGroupPayrollProductsDialogController } from "./PayrollGroupPayrollProductsDialogController";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollGroupPayrollProductsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/PayrollGroups/Directives/PayrollGroupPayrollProducts/Views/PayrollGroupPayrollProducts.html'),
            scope: {
                payrollGroupId: '=',
                payrollGroupPayrollProducts: '=',                
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollGroupPayrollProductsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollGroupPayrollProductsController {

    // Init parameters
    private payrollGroupId: number;
    private payrollGroupPayrollProducts: PayrollGroupPayrollProductDTO[] = [];

    // Data
    private payrollProducts: ISmallGenericType[] = [];
    

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private payrollService: IPayrollService) {

        this.$q.all([
            this.loadPayrollProducts()
        ]).then(() => {
        });
    }

    // SERVICE CALLS

    private loadPayrollProducts(): ng.IPromise<any> {
        this.payrollProducts = [];

        return this.payrollService.getPayrollProductsDict(false, true, true).then(x => {
            this.payrollProducts = x;
        });
    }

    // EVENTS

    private editPayrollProduct(payrollProduct: PayrollGroupPayrollProductDTO) {
        
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollGroups/Directives/PayrollGroupPayrollProducts/Views/PayrollGroupPayrollProductsDialog.html"),
            controller: PayrollGroupPayrollProductsDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                payrollProduct: () => { return payrollProduct },
                payrollProducts: () => { return this.getAvailablePayrollProducts(payrollProduct) },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.payrollProduct) {
                if (!payrollProduct) {
                    // Add new formula to the original collection
                    payrollProduct = new PayrollGroupPayrollProductDTO();
                    this.payrollGroupPayrollProducts.push(payrollProduct);
                }

                payrollProduct.productId = result.payrollProduct.productId;
                payrollProduct.distribute = result.payrollProduct.distribute;                           

                // Set name
                let temp = _.find(this.payrollProducts, p => p.id === payrollProduct.productId);
                if (temp) {
                    payrollProduct.productName = temp.name;                    
                }

                this.setAsDirty();
            }
        });
    }

    private deletePayrollProduct(payrollProduct: PayrollGroupPayrollProductDTO) {
        _.pull(this.payrollGroupPayrollProducts, payrollProduct);

        this.setAsDirty();
    }

    // HELP-METHODS

    private getAvailablePayrollProducts(product: PayrollGroupPayrollProductDTO): ISmallGenericType[] {
        let products = _.filter(this.payrollProducts, p => !_.includes(_.map(this.payrollGroupPayrollProducts, pgpp => pgpp.productId), p.id));
        if (product)
            products.push({ id: product.productId, name: product.productName });

        return products;
    }

    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}