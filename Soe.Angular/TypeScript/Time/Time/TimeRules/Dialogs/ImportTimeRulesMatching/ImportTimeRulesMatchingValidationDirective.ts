import { TimeRuleExportImportDTO } from "../../../../../Common/Models/TimeRuleDTOs";

export class ImportTimeRulesMatchingValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    let importResult: TimeRuleExportImportDTO = ngModelController.$modelValue;

                    let timeCodesMatch: boolean = _.filter(importResult.timeCodes, t => !t.matchedTimeCodeId).length === 0;
                    let employeeGroupsMatch: boolean = _.filter(importResult.employeeGroups, e => e.matchedEmployeeGroupId < 0).length === 0;
                    let timeScheduleTypesMatch: boolean = _.filter(importResult.timeScheduleTypes, t => t.matchedTimeScheduleTypeId < 0).length === 0;
                    let timeDeviationCausesMatch: boolean = _.filter(importResult.timeDeviationCauses, t => !t.matchedTimeDeviationCauseId).length === 0;
                    let dayTypesMatch: boolean = _.filter(importResult.dayTypes, t => !t.matchedDayTypeId).length === 0;

                    ngModelController.$setValidity("timeCodesMatch", timeCodesMatch);
                    ngModelController.$setValidity("employeeGroupsMatch", employeeGroupsMatch);
                    ngModelController.$setValidity("timeScheduleTypesMatch", timeScheduleTypesMatch);
                    ngModelController.$setValidity("timeDeviationCausesMatch", timeDeviationCausesMatch);
                    ngModelController.$setValidity("dayTypesMatch", dayTypesMatch);
                }, true);
            }
        }
    }
}



