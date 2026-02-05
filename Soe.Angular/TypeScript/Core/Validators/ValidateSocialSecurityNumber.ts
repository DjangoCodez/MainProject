import { CalendarUtility } from "../../Util/CalendarUtility";

export class ValidateSocialSecurityNumberFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: 'ngModel',
            scope: {
                checkValidDate: '=',
                mustSpecifyCentury: '=',
                mustSpecifyDash: '=',
                sex: '=',
                allowEmpty: "="
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {

                var validator = function (value) {
                    return (scope['allowEmpty'] && !value) || CalendarUtility.isValidSocialSecurityNumber(value, scope['checkValidDate'], scope['mustSpecifyCentury'], scope['mustSpecifyDash'], scope['sex']);
                };
                ngModelController.$validators['socialSecurityNumber'] = validator;
            },
        };
    }
}
