import { CustomerInvoiceDTO } from "../../Models/InvoiceDTO";
import { TermGroup_InvoiceVatType } from "../../../Util/CommonEnumerations";

export class CustomerInvoiceValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                accountPeriodId: '=',
                skipVatAmountValidation: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {                
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var invoice: CustomerInvoiceDTO = ngModelController.$modelValue;

                        var validVatAmount: boolean = !!(invoice && (invoice.vatAmountCurrency || invoice.vatType !== TermGroup_InvoiceVatType.Merchandise) || scope['skipVatAmountValidation']);
                        ngModelController.$setValidity("vatAmount", validVatAmount);                        
                    }
                }, true);
            }
        }
    }
}