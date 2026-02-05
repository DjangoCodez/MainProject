import { PreAnalysisInformation } from "../../../../../Common/Models/StaffingNeedsDTOs";

export class PreAnalysisInformationController {

    //@ngInject
    constructor(private $uibModalInstance,
        private info: PreAnalysisInformation) {
    }

    // EVENTS

    private close() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
