import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmployeeCalculatedCostDTO } from "../../../../../Common/Models/EmployeeCalculatedCostDTO";

export class EmployeeCalculatedDialogController {

    private group: EmployeeCalculatedCostDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        group: EmployeeCalculatedCostDTO) {

        this.isNew = !group;
        this.group = new EmployeeCalculatedCostDTO();
        angular.extend(this.group, group);
        if (this.isNew) {
            this.group.fromDate = CalendarUtility.getDateToday();
            this.group.isModified = true;
            this.group.employeeCalculatedCostId = 0;
        }
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ group: this.group });
    }
}