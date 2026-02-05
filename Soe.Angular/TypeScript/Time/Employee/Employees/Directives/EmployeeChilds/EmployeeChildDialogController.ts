import { EmployeeChildDTO } from "../../../../../Common/Models/EmployeeChildDTOs";

export class EmployeeChildDialogController {

    private child: EmployeeChildDTO;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        child: EmployeeChildDTO,
        private openingBalancePermission: boolean) {

        this.child = new EmployeeChildDTO();
        angular.extend(this.child, child);

        if (!this.child.openingBalanceUsedDays)
            this.child.openingBalanceUsedDays = 0;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ child: this.child });
    }
}
