import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import {  TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO } from "../../../../../Common/Models/TimeAccumulatorDTOs";

export class TimeWorkReductionEarningGroupRulesController {

    private rule: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO;
    private isNew: boolean;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private employeeGroups: ISmallGenericType[],
        rule: TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO) {

        this.isNew = !rule;
        this.rule = new TimeAccumulatorTimeWorkReductionEarningEmployeeGroupDTO();
        angular.extend(this.rule, rule);
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ rule: this.rule });
    }
}
