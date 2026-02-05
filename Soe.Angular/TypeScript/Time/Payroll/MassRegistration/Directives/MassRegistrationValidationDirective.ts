import { MassRegistrationTemplateHeadDTO } from "../../../../Common/Models/MassRegistrationDTOs";

export class MassRegistrationValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                rows: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                //scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                //    var head: MassRegistrationTemplateHeadDTO = ngModelController.$modelValue;
                //}, true);

                scope.$watch(() => scope['rows'], (newValues, oldValues, scope) => {
                    let missingEmployee: boolean = false;
                    let missingProduct: boolean = false;
                    let missingPaymentDate: boolean = false;
                    let missingDateInterval: boolean = false;

                    _.forEach(scope['rows'], row => {
                        if (!row.employeeId)
                            missingEmployee = true;
                        if (!row.productId)
                            missingProduct = true;
                        if (!row.paymentDate)
                            missingPaymentDate = true;
                        if (!row.dateFrom || !row.dateTo)
                            missingDateInterval = true;
                    });

                    ngModelController.$setValidity("rowEmployee", !missingEmployee);
                    ngModelController.$setValidity("rowProduct", !missingProduct);
                    ngModelController.$setValidity("rowPaymentDate", !missingPaymentDate);
                    ngModelController.$setValidity("rowDateInterval", !missingDateInterval);
                }, true);
            }
        }
    }
}