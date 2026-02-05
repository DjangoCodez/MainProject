import { PayrollGroupVacationGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollGroupVacationGroupsDialogController {

    private vacationGroup: PayrollGroupVacationGroupDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        vacationGroup: PayrollGroupVacationGroupDTO,
        private payrollVacationGroups: ISmallGenericType[]) {

        this.isNew = !vacationGroup;

        this.vacationGroup = new PayrollGroupVacationGroupDTO();
        angular.extend(this.vacationGroup, vacationGroup);
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ vacationGroup: this.vacationGroup });
    }
}
