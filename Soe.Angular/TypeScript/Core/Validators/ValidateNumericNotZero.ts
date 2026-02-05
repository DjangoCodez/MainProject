import { Validators } from "./Validators";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class ValidateNumericNotZero {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            scope: {
                skipNotZeroValidation: '=',
            },
            link: function (scope, elem, attr, ngModel: ng.INgModelController) {
                var validator = function (value) {
                    if (value && (typeof value === 'string') && (<string>value).contains(":"))
                    {
                        let minutes = CalendarUtility.timeSpanToMinutes(value).toString();
                        return scope['skipNotZeroValidation'] ? Validators.isNumeric(minutes) : Validators.isNumericNotZero(minutes);
                    }
                    else {
                        return scope['skipNotZeroValidation'] ? Validators.isNumeric(value) : Validators.isNumericNotZero(value);
                    }
                };
                ngModel.$validators.numericNotZero = validator;
            },
            require: 'ngModel',
            restrict: 'A',
        };
    }
}
