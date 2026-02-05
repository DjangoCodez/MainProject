export class SalesBudgetValidationDirective {
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: {
                ngModel: "="
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        if (!ngModelController.$modelValue) {
                            ngModelController.$setValidity("budget", false);
                        } else {
                            if (!ngModelController.$modelValue.noOfPeriods || ngModelController.$modelValue.noOfPeriods < 1) {
                                ngModelController.$setValidity("budget", false);
                            }
                            else {
                                ngModelController.$setValidity("budget", true);
                            }
                        }
                    }
                }, true);
            }
        }
    }
}


