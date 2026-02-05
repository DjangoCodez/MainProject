export class EmployeeTemplatesValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                hasInvalidPosition: '=',
                hasRemainingSystemRequiredFields: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                //scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                //    let employeeTemplate: EmployeeTemplateDTO = ngModelController.$modelValue;
                //    if (employeeTemplate) {
                //    }
                //}, true);

                scope.$watchGroup(['hasInvalidPosition', 'hasRemainingSystemRequiredFields'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("validPosition", !scope['hasInvalidPosition']);
                    ngModelController.$setValidity("noRemainingSystemRequiredFields", !scope['hasRemainingSystemRequiredFields']);
                });
            }
        }
    }
}