import { CreateVacantEmployeeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { IEmployeeGroupSmallDTO } from "../../../../../Scripts/TypeLite.Net4";

export class EditVacantEmployeeDialogController {

    private employee: CreateVacantEmployeeDTO;
    private isNew: boolean;
    private employeeGroup: IEmployeeGroupSmallDTO;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout: ng.ITimeoutService,
        private useAccountsHierarchy: boolean,
        private defaultEmployeeAccountDimId: number,
        private employeeGroups: IEmployeeGroupSmallDTO[],
        employee: CreateVacantEmployeeDTO) {

        this.isNew = !employee;

        this.employee = new CreateVacantEmployeeDTO();
        angular.extend(this.employee, employee);
        if (this.employee.employeeGroupId)
            this.employeeGroup = _.find(this.employeeGroups, g => g.employeeGroupId === this.employee.employeeGroupId);
    }

    // EVENTS

    private employeeGroupChanged() {
        this.$timeout(() => {
            this.resetWorkTimeWeek();
            this.employee.employeeGroupId = this.employeeGroup ? this.employeeGroup.employeeGroupId : 0;
            this.employee.employeeGroupName = this.employeeGroup ? this.employeeGroup.name : '';
        });
    }

    private workTimeWeekChanged() {
        this.$timeout(() => {
            this.calculateEmploymentPercent();
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ employee: this.employee });
    }

    // HELP-METHODS

    private resetWorkTimeWeek() {
        if (this.employeeGroup && this.employee.percent == 100) {
            this.employee.workTimeWeek = this.employeeGroup.ruleWorkTimeWeek;
        }
    }

    private calculateEmploymentPercent() {
        if (this.employeeGroup) {
            this.employee.percent = this.employee.workTimeWeek > 0 ? (100 * (this.employee.workTimeWeek / this.employeeGroup.ruleWorkTimeWeek)).round(2) : 0;
        }
    }
}
