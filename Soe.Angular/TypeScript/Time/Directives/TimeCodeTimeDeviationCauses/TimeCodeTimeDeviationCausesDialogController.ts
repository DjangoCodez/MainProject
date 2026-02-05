import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { TimeCodeBreakTimeCodeDeviationCauseDTO } from "../../../Common/Models/TimeCode";

export class TimeCodeTimeDeviationCausesDialogController {

    private row: TimeCodeBreakTimeCodeDeviationCauseDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private timeDeviationCauses: ISmallGenericType[],
        private timeCodes: ISmallGenericType[],
        row: TimeCodeBreakTimeCodeDeviationCauseDTO) {

        this.isNew = !row;

        this.row = new TimeCodeBreakTimeCodeDeviationCauseDTO();
        angular.extend(this.row, row);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ row: this.row });
    }
}
