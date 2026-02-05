import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmploymentVacationGroupDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { PayrollGroupVacationGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";

export class EmploymentVacationGroupDialogController {

    private group: EmploymentVacationGroupDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private vacationGroups: PayrollGroupVacationGroupDTO[],
        group: EmploymentVacationGroupDTO) {

        this.isNew = !group;

        this.group = new EmploymentVacationGroupDTO();
        angular.extend(this.group, group);
        if (this.isNew) {
            this.group.fromDate = CalendarUtility.getDateToday();
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ group: this.group });
    }
}
