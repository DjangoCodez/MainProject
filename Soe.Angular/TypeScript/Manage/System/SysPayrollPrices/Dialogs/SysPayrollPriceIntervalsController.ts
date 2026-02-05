import { SysPayrollPriceDTO } from "../../../../Common/Models/SysPayrollPriceDTO";

export class SysPayrollPriceIntervalsController {

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private sysPayrollPrice: SysPayrollPriceDTO[]) {
    }

    public cancel() {
        this.$uibModalInstance.close();
    }
}
