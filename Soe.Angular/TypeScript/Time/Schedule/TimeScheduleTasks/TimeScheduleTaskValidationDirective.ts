
export class TimeScheduleTaskValidationDirectiveFactory {
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
                        var length: number = newValue['length'];
                        var nbrOfPersons = newValue['nbrOfPersons'];
                        var onlyOneEmployee: boolean = newValue["onlyOneEmployee"];
                        var minSplitLength: number = newValue['minSplitLength'];
                        var startTime: Date = newValue['startTime'];
                        var stopTime: Date = newValue['stopTime'];
                        var minLength = newValue["minLength"];
                        if (!minLength)
                            minLength = 15;

                        if (length && minLength) {
                            var validLength: boolean = length >= minLength;
                            ngModelController.$setValidity("length", validLength);
                        }
                        if (startTime && stopTime) {
                            var stopTimeValid: boolean = stopTime >= startTime;
                            ngModelController.$setValidity("stopTime", stopTimeValid);
                        }
                        if (onlyOneEmployee || (minSplitLength && minLength)) {
                            var minSplitLengthValid: boolean = onlyOneEmployee ? true : minSplitLength >= minLength;
                            ngModelController.$setValidity("minSplitLength", minSplitLengthValid);
                        }
                        if (startTime && stopTime && length && nbrOfPersons) {
                            var plannedTimeValid: boolean = stopTime.diffMinutes(startTime) >= (length / nbrOfPersons);
                            ngModelController.$setValidity("plannedTime", plannedTimeValid);
                        }
                    }
                }, true);
            }
        }
    }
}


