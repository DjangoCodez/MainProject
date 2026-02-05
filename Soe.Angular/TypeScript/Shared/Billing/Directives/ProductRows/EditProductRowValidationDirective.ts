import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";


export class EditProductRowValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldValue, scope) => {
                    if (newValue) {
                        var row: ProductRowDTO = ngModelController.$modelValue;

                        // Household validation
                        if (row.isHouseholdRow) {
                            if (!row.householdTypeIsRUT) {
                                // Property is mandatory - if ApartmentNbr and CooperativeOrgNbr is empty
                                var missingProperty = (!row.householdApartmentNbr && !row.householdCooperativeOrgNbr && !row.householdProperty);
                                ngModelController.$setValidity("householdProperty", !missingProperty);
                            }
                        }
                    }
                }, true);
            }
        }
    }
}


