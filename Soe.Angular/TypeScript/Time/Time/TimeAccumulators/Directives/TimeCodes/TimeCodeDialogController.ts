import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TimeAccumulatorTimeCodeDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class TimeCodeDialogController {

    private timeCode: TimeAccumulatorTimeCodeDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private timeCodes: ISmallGenericType[],
        timeCode: TimeAccumulatorTimeCodeDTO) {

        this.isNew = !timeCode;

        this.timeCode = new TimeAccumulatorTimeCodeDTO();
        angular.extend(this.timeCode, timeCode);
        if (this.isNew) {
            this.timeCode.importDefault = false;
            this.timeCode.factor = 1;
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ timeCode: this.timeCode });
    }
}
