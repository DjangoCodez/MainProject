import { PayrollGroupPayrollProductDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollGroupPayrollProductsDialogController {

    private payrollProduct: PayrollGroupPayrollProductDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        payrollProduct: PayrollGroupPayrollProductDTO,
        private payrollProducts: ISmallGenericType[]) {

        this.isNew = !payrollProduct;

        this.payrollProduct = new PayrollGroupPayrollProductDTO();
        angular.extend(this.payrollProduct, payrollProduct);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ payrollProduct: this.payrollProduct });
    }
}
