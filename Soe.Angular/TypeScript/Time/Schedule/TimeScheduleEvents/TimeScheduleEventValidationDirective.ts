export class TimeScheduleEventValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                nbrReceivers: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue) => {
                    if (newValue) {
                        var length = scope["nbrReceivers"];
                        if (!length)
                            length = 0;
                        var validLength: boolean = length >= 1;
                        ngModelController.$setValidity("length", validLength);
                    }
                }, true);
            }
        }
    }
}


