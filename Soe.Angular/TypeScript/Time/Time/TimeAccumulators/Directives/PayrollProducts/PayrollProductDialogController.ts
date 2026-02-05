import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TimeAccumulatorPayrollProductDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class PayrollProductDialogController {

    private product: TimeAccumulatorPayrollProductDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private products: ISmallGenericType[],
        product: TimeAccumulatorPayrollProductDTO) {

        this.isNew = !product;

        this.product = new TimeAccumulatorPayrollProductDTO();
        angular.extend(this.product, product);
        if (this.isNew)
            this.product.factor = 1;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ product: this.product });
    }
}
