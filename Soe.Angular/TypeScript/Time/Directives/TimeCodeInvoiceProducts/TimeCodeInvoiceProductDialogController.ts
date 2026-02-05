import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TimeCodeInvoiceProductDTO } from "../../../Common/Models/TimeCode";

export class TimeCodeInvoiceProductDialogController {

    private timeCodeProduct: TimeCodeInvoiceProductDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private products: ISmallGenericType[],
        timeCodeProduct: TimeCodeInvoiceProductDTO) {

        this.isNew = !timeCodeProduct;

        this.timeCodeProduct = new TimeCodeInvoiceProductDTO();
        angular.extend(this.timeCodeProduct, timeCodeProduct);
        if (this.isNew)
            this.timeCodeProduct.factor = 1;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ timeCodeProduct: this.timeCodeProduct });
    }
}
