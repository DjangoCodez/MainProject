import { CustomerDTO } from "../../../Models/CustomerDTO";

export class CustomerValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                isContactAddressesValid: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        // Customer
                        var customer: CustomerDTO = ngModelController.$modelValue;

                        // Customer name must not contain a comma
                        var containsComma: boolean = customer.name && customer.name.contains(',');
                        ngModelController.$setValidity("nameContainsComma", !containsComma);
                    }
                }, true);

                scope.$watchGroup(['isContactAddressesValid'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("contactAddress", scope['isContactAddressesValid']);
                });
            }
        }
    }
}