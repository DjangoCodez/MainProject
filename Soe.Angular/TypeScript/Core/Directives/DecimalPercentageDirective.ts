
export class DecimalPercentageDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: {
                ngModel: '='

            },
            link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                ngModelController.$parsers.push(function (data) {
                    if (angular.isUndefined(val)) {
                        var val = '';
                    }
                    val = val + '';
                    var clean = val;
                    var decimalCheck = clean.split('.');

                    if (!angular.isUndefined(decimalCheck[1])) {
                        decimalCheck[1] = decimalCheck[1].slice(0, 2);
                        clean = decimalCheck[0] + '.' + decimalCheck[1];
                    }

                    if (val !== clean) {
                        ngModelController.$setViewValue(clean);
                        ngModelController.$render();
                    }
                    return clean;
                });

                ngModelController.$formatters.push(function (val) {
                    if (angular.isUndefined(val)) {
                        val = '';
                    }
                    val = val + '';
                    var clean = val;
                    var decimalCheck = clean.split('.');

                    if (!angular.isUndefined(decimalCheck[1])) {
                        decimalCheck[1] = decimalCheck[1].slice(0, 2);
                        clean = decimalCheck[0] + '.' + decimalCheck[1];
                    }

                    if (val !== clean) {
                        ngModelController.$setViewValue(clean);
                        ngModelController.$render();
                    }
                    return clean;
                });

                element.bind('keypress', function (event) {
                    if (event.keyCode === 32) {
                        event.preventDefault();
                    }
                });
            }
        }
    }
}