import { TimeAbsenceDetailDTO } from "../../../../Common/Models/TimeAbsenceDetailDTO";

export class UpdateAbsenceDetailsDialogController {

    private ratio: number;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private absenceDetails: TimeAbsenceDetailDTO[]) {
        this.setPreDefinedRatio();
    }

    private setPreDefinedRatio() {
        _.forEach(this.absenceDetails, (absenceDetail: TimeAbsenceDetailDTO) => {
            if (!this.ratio)
                this.ratio = absenceDetail.ratio ?? -1;
            else if (this.ratio !== absenceDetail.ratio) {
                this.ratio = undefined;
                return;
            }                
        });

        if (this.ratio < 0)
            this.ratio = undefined;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        _.forEach(this.absenceDetails, (absenceDetail: TimeAbsenceDetailDTO) => {
            if (this.ratio)
                absenceDetail.ratio = this.ratio;
        });
        this.$uibModalInstance.close({ ratio: this.ratio, absenceDetails: this.absenceDetails });
    }
}
