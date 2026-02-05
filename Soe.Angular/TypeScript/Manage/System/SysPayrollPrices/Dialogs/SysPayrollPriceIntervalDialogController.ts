import { SysPayrollPriceIntervalDTO } from "../../../../Common/Models/SysPayrollPriceDTO";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";

export class SysPayrollPriceIntervalDialogController {

    private interval: SysPayrollPriceIntervalDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private amountTypes: ISmallGenericType[][],
        interval: SysPayrollPriceIntervalDTO) {

        this.isNew = !interval;

        this.interval = new SysPayrollPriceIntervalDTO();
        angular.extend(this.interval, interval);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ interval: this.interval });
    }
}
