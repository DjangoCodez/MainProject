
export class IncomingDeliveryRowsValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                minLength: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue) => {
                    if (newValue) {
                        var startTime: number = newValue['startTime'];
                        var stopTime: number = newValue['stopTime'];
                        var length: number = newValue['length'];
                        var minSplitLength: number = newValue['minSplitLength'];
                        var minLength = scope["minLength"];
                        if (!minLength)
                            minLength = 15;

                        if (length && minLength) {
                            var lengthValid: boolean = length >= minLength;
                            ngModelController.$setValidity("length", lengthValid);
                        }
                        if (startTime && stopTime) {
                            var stopTimeValid: boolean = stopTime >= startTime;
                            ngModelController.$setValidity("stopTime", stopTimeValid);
                        }
                        if (minSplitLength && minLength) {
                            var minSplitLengthValid: boolean = minSplitLength >= minLength;
                            ngModelController.$setValidity("minSplitLength", minSplitLengthValid);
                        }
                    }
                }, true);
            }
        }
    }
}


