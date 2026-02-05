import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TimeCodePayrollProductDTO } from "../../../Common/Models/TimeCode";

export class TimeCodePayrollProductDialogController {

    private timeCodeProduct: TimeCodePayrollProductDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private products: ISmallGenericType[],
        timeCodeProduct: TimeCodePayrollProductDTO) {

        this.isNew = !timeCodeProduct;

        this.timeCodeProduct = new TimeCodePayrollProductDTO();
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
