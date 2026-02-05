export class IncomingDeliveryTypesValidationDirectiveFactory {
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
                        var minLength = scope["minLength"];
                        if (!minLength)
                            minLength = 15;
                        var length: number = newValue['length'];
                        var validLength: boolean = length >= minLength;
                        ngModelController.$setValidity("length", validLength);
                    }
                }, true);
            }
        }
    }
}


