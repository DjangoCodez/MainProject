export class DistributionCodeValidationDirective {
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: "="
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        if (!ngModelController.$modelValue) {
                            ngModelController.$setValidity("percentSumOfPeriods", false);
                        }
                        else {
                            var model = ngModelController.$modelValue;
                            if (model.type > 3 && (!model.accountDimId || model.accountDimId === 0))
                                ngModelController.$setValidity("accountDimId", false);
                            else
                                ngModelController.$setValidity("accountDimId", true);

                            if (model.periods) {
                                var sum = 0;
                                for (var i = 0; i < model.periods.length; i++) {
                                    sum += model.periods[i].percent;
                                }
                                if ((Math.round((sum) * 100.0) / 100.0) !== 100) {
                                    ngModelController.$setValidity("percentSumOfPeriods", false);
                                } else {
                                    ngModelController.$setValidity("percentSumOfPeriods", true);
                                }
                            }
                            else {
                                ngModelController.$setValidity("percentSumOfPeriods", true);
                            }
                        }
                    }
                }, true);
            }
        }
    }
}