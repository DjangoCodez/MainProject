import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TimeAccumulatorEmployeeGroupRuleDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class EmployeeGroupRuleDialogController {

    private rule: TimeAccumulatorEmployeeGroupRuleDTO;
    private useTimeWorkReductionWithdrawal: boolean;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private types: ISmallGenericType[],
        private employeeGroups: ISmallGenericType[],
        private timeCodes: ISmallGenericType[],
        private scheduledJobHeads: ISmallGenericType[],
        rule: TimeAccumulatorEmployeeGroupRuleDTO,
        useTimeWorkReductionWithdrawal: boolean) {

        this.isNew = !rule;
        this.useTimeWorkReductionWithdrawal = useTimeWorkReductionWithdrawal;
        this.rule = new TimeAccumulatorEmployeeGroupRuleDTO();
        angular.extend(this.rule, rule);
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ rule: this.rule });
    }
}
