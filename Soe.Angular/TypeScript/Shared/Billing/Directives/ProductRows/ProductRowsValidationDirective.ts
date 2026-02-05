import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { SoeInvoiceRowType, SoeOriginType, TermGroup_BillingType } from "../../../../Util/CommonEnumerations";

export class ProductRowsValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                useFreightAmount: '=',
                freightAmountProductId: '=',
                useInvoiceFee: '=',
                invoiceFeeProductId: '=',
                useCentRounding: '=',
                centRoundingProductId: '=',
                originType: '=',
                billingType: '=',
                totalAmount: '=',
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {
                    if (newValue) {
                        var useFreightAmount: boolean = scope["useFreightAmount"];
                        var freightAmountId: number = scope["freightAmountProductId"];
                        var validFreightAmount: boolean = useFreightAmount ? !!(freightAmountId && freightAmountId > 0) : true;
                        ngModelController.$setValidity("freightBase", validFreightAmount);

                        var useInvoiceFee: boolean = scope["useInvoiceFee"];
                        var invoiceFeeProductId: number = scope["invoiceFeeProductId"];
                        var validInvoiceFee: boolean = useInvoiceFee ? !!(invoiceFeeProductId && invoiceFeeProductId > 0) : true;
                        ngModelController.$setValidity("invoiceFeeBase", validInvoiceFee);

                        var useCentRounding: boolean = scope["useCentRounding"];
                        var centRoundingProductId: number = scope["centRoundingProductId"];
                        var validCentRounding: boolean = useCentRounding ? !!(centRoundingProductId && centRoundingProductId > 0) : true;
                        ngModelController.$setValidity("centRoundingBase", validCentRounding);

                        var productRows: ProductRowDTO[] = ngModelController.$modelValue;
                        var noMissingProduct = !(_.filter(productRows, (r) => (r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow) && (!r.productId || r.productId === 0) && (r.sumAmountCurrency && r.sumAmountCurrency != 0)).length > 0);
                        ngModelController.$setValidity("missingProduct", noMissingProduct);

                        //Should be done in the page implementing the product rows
                        /*var hideVatWarnings = scope["hideVatWarnings"];
                        if (!hideVatWarnings) {
                        }
                        else {
                        }*/

                        var originType = scope["originType"];
                        var billingType = scope["billingType"];
                        var totalAmount = scope["totalAmount"];
                        if (originType === SoeOriginType.SupplierInvoice || originType === SoeOriginType.CustomerInvoice) {
                            if (billingType === TermGroup_BillingType.Credit) {
                                ngModelController.$setValidity("credit", (totalAmount && totalAmount < 0));
                                ngModelController.$setValidity("nonCredit", true);
                            }
                            else if (billingType !== TermGroup_BillingType.Credit) {
                                ngModelController.$setValidity("nonCredit", (totalAmount && totalAmount > 0));
                                ngModelController.$setValidity("credit", true);
                            }
                        }
                    }
                }, true);
            }
        }
    }
}


