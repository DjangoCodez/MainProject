import { EmployeeTimeWorkAccountDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class EmployeeTimeWorkAccountDialogController {

    private employeeTimeWorkAccount: EmployeeTimeWorkAccountDTO;
    private isNew: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalInstanceService,
        private timeWorkAccounts: SmallGenericType[],
        employeeTimeWorkAccount: EmployeeTimeWorkAccountDTO) {

        this.employeeTimeWorkAccount = new EmployeeTimeWorkAccountDTO();
        angular.extend(this.employeeTimeWorkAccount, employeeTimeWorkAccount);
        this.employeeTimeWorkAccount.fixDates();
        
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ workTimeAccount: this.employeeTimeWorkAccount });
    }
}
