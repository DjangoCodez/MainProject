import { TimeRuleEditDTO } from "../../../../../Common/Models/TimeRuleDTOs";

export class ImportTimeRulesValidationDirectiveFactory {
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
                    let rules: TimeRuleEditDTO[] = ngModelController.$modelValue;

                    let missingMandatory: boolean = false;

                    _.forEach(rules, rule => {
                        // Mandatory fields
                        if (!rule.name || !rule.type || !rule.ruleStartDirection || !rule.timeCodeId)
                            missingMandatory = true;
                    });

                    ngModelController.$setValidity("mandatoryFields", !missingMandatory);
                }, true);
            }
        }
    }
}



