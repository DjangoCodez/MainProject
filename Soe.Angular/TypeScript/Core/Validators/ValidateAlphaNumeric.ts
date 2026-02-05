import { Validators } from "./Validators";

export class ValidateAlphaNumericFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            link: function (scope, elem, attr, ngModel: ng.INgModelController) {
                var validator = function (value) {
                    return Validators.isAlphaNumeric(value);
                };
                ngModel.$validators.alphaNumeric = validator;
            },
            require: 'ngModel',
            restrict: 'A',
        };
    }
}
