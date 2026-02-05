export class SupplierValidationDirectiveFactory {
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
                scope.$watchGroup(['isContactAddressesValid'], (newValues, oldValues, scope) => {
                    ngModelController.$setValidity("contactAddress", scope['isContactAddressesValid']);
                });
            }
        }
    }
}