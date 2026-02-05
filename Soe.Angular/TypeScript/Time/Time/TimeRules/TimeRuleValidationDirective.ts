export class TimeRuleValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                hasStartFormulaError: '=',
                hasStopFormulaError: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['hasStartFormulaError', 'hasStopFormulaError'], (newValues, oldValues, scope) => {
                    if (newValues) {
                        ngModelController.$setValidity("ruleDefinition", !scope['hasStartFormulaError'] && !scope['hasStopFormulaError']);
                    }
                });
            }
        }
    }
}

