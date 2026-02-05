import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TimeAccumulatorInvoiceProductDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class InvoiceProductDialogController {

    private product: TimeAccumulatorInvoiceProductDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private products: ISmallGenericType[],
        product: TimeAccumulatorInvoiceProductDTO) {

        this.isNew = !product;

        this.product = new TimeAccumulatorInvoiceProductDTO();
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
