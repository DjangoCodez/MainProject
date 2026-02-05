import { Validators } from "./Validators";

export class ValidateNumeric {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            scope: {
                allowEmpty: "=",
                noDecimals: "=",
                noNegative: "="
            },
            link: function (scope, elem, attr, ngModel: ng.INgModelController) {
                var validator = function (value) {
                    return ((scope['allowEmpty'] && !value) || Validators.isNumeric(value, !scope['noDecimals'], !scope['noNegative']));
                };
                ngModel.$validators.numeric = validator;
            },
            require: 'ngModel',
            restrict: 'A',
        };
    }
}
