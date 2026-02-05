import { TimeScheduleTypeFactorDTO } from "../../../Common/Models/TimeScheduleTypeDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class FactorDialogController {

    private factor: TimeScheduleTypeFactorDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout: ng.ITimeoutService,
        factor: TimeScheduleTypeFactorDTO) {

        this.isNew = !factor;

        this.factor = new TimeScheduleTypeFactorDTO();
        angular.extend(this.factor, factor);
        if (this.isNew) {
            this.factor.fromTime = CalendarUtility.DefaultDateTime();
            this.factor.toTime = CalendarUtility.DefaultDateTime();
            this.validateTimes();
        }
    }

    private fromTimeChanged() {
        this.$timeout(() => {
            this.validateTimes();
        });
    }

    private toTimeChanged() {
        this.$timeout(() => {
            this.validateTimes();
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        if (this.validate())
            this.$uibModalInstance.close({ factor: this.factor });
    }

    private validateTimes() {
        while (this.factor.fromTime >= this.factor.toTime)
            this.factor.toTime = this.factor.toTime.addDays(1);

        while (this.factor.fromTime.addDays(1) < this.factor.toTime)
            this.factor.toTime = this.factor.toTime.addDays(-1);
    }

    private validate(): boolean {
        return true;
    }
}
