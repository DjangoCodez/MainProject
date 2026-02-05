import { SysJobDTO } from "../../../../../../Common/Models/SysJobDTO";

export class AddJobController {

    get validToSave() {
        return this.job.name && this.job.assemblyName && this.job.className;
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private job: SysJobDTO) {
    }

    private save() {
        this.$uibModalInstance.close({ item: this.job });
    }

    private delete() {
        this.$uibModalInstance.close({ item: this.job, delete: true });
    }

    private close() {
        this.$uibModalInstance.close();
    }
}