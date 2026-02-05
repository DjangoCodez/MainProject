import { Validators } from "./Validators";

export class ValidateEmailFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            link: function (scope, elem, attr, ngModel: ng.INgModelController) {
                var validator = function (value) {
                    return Validators.isValidEmailAddress(value);
                };
                ngModel.$validators.eMail = validator;
            },
            require: 'ngModel',
            restrict: 'A',
        };
    }
}
