import { AttestEmployeeDayTimeBlockDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";

export class DeleteTimeBlockDialogController {

    private timeBlock: AttestEmployeeDayTimeBlockDTO;

    private movedStart: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        timeBlock: AttestEmployeeDayTimeBlockDTO) {

        this.movedStart = !!timeBlock.startTimeDuringMove;

        this.timeBlock = new AttestEmployeeDayTimeBlockDTO();
        angular.extend(this.timeBlock, timeBlock);

        if (this.movedStart)
            this.timeBlock.startTime = this.timeBlock.stopTime;
        else
            this.timeBlock.stopTime = this.timeBlock.startTime;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ timeBlock: this.timeBlock });
    }
}
