import { PayrollGroupPriceTypePeriodDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class PayrollGroupPriceTypePeriodDialogController {

    private period: PayrollGroupPriceTypePeriodDTO;
    private isNew: boolean;

    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, period: PayrollGroupPriceTypePeriodDTO) {
        this.isNew = !period;

        this.period = new PayrollGroupPriceTypePeriodDTO();
        angular.extend(this.period, period);
        if (this.isNew) {
            this.period.fromDate = CalendarUtility.getDateToday();
            this.period.amount = 0;
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ period: this.period });
    }
}
