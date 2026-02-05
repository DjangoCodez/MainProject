import { SupplierInvoiceDTO } from "../../../../../Common/Models/InvoiceDTO";
import { InvoiceUtility } from "../../../../../Util/InvoiceUtility";
import { TermGroup_BillingType } from "../../../../../Util/CommonEnumerations";

export class SupplierInvoiceValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                accountPeriodId: '=',
                checkFiOcr: '=',
                ocr: '=',
                showConfirmAccounting: '=',
                confirmAccounting: '=',
                linkToOrderOrderSet: '=',
                linkToProjectProjectSet: '=',
                linkToProjectTimeCodeSet: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var invoice: SupplierInvoiceDTO = ngModelController.$modelValue;

                        //var validTotalAmount: boolean = !!(invoice && invoice.totalAmountCurrency);
                        //ngModelController.$setValidity("totalAmount", validTotalAmount);
                        var validBillingType: boolean = (invoice && ( (invoice.totalAmount >= 0 && invoice.billingType !== TermGroup_BillingType.Credit) || invoice.totalAmount <= 0 && invoice.billingType == TermGroup_BillingType.Credit));
                        ngModelController.$setValidity("billingType", validBillingType);

                        /*var validVatAmount: boolean = !!(invoice && (invoice.vatAmountCurrency || invoice.vatType !== TermGroup_InvoiceVatType.Merchandise));
                        ngModelController.$setValidity("vatAmount", validVatAmount);*/

                        var validVoucherSeries: boolean = !!(invoice && invoice.voucherSeriesId);
                        ngModelController.$setValidity("voucherSeries", validVoucherSeries);


                        // Dates
                        var validInvoiceDate: boolean = !!(invoice && invoice.invoiceDate && invoice.invoiceDate > new Date((new Date()).getFullYear() - 3, 1, 1));
                        ngModelController.$setValidity("invoiceDate", validInvoiceDate);

                        var validInvoiceDate: boolean = !!(invoice && invoice.dueDate && invoice.dueDate > new Date((new Date()).getFullYear() - 3, 1, 1));
                        ngModelController.$setValidity("dueDate", validInvoiceDate);

                        var validInvoiceDate: boolean = !!(invoice && invoice.voucherDate && invoice.voucherDate > new Date((new Date()).getFullYear() - 3, 1, 1));
                        ngModelController.$setValidity("voucherDate", validInvoiceDate);
                    }
                }, true);
                scope.$watchGroup(['accountPeriodId', 'checkFiOcr', 'ocr', 'showConfirmAccounting', 'confirmAccounting', 'linkToOrderOrderSet', 'linkToProjectProjectSet', 'linkToProjectTimeCodeSet'], (newValues, oldValues, scope) => {

                    // Account period
                    ngModelController.$setValidity("accountPeriod", !!scope['accountPeriodId']);

                    // OCR
                    var validFiOcr: boolean = true;
                    /*if (scope['checkFiOcr'] && scope['ocr'])
                        validFiOcr = InvoiceUtility.validateFIBankPaymentReference(scope['ocr']);*/
                    ngModelController.$setValidity("validFiOcr", validFiOcr);


                    // Confirm accounting
                    var showConfirmAccounting: boolean = scope['showConfirmAccounting'] === true;
                    var confirmAccounting: boolean = scope['confirmAccounting'] === true;
                    ngModelController.$setValidity("confirmAccounting", !showConfirmAccounting || confirmAccounting);

                    ngModelController.$setValidity("linkToOrderOrderSet", scope['linkToOrderOrderSet']);
                    ngModelController.$setValidity("linkToProjectProjectSet", scope['linkToProjectProjectSet']);
                    ngModelController.$setValidity("linkToProjectTimeCodeSet", scope['linkToProjectTimeCodeSet']);
                });
            }
        }
    }
}

