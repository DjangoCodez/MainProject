import { PaymentRowDTO } from "../../../Common/Models/PaymentRowDTO";
import { SupplierInvoiceDTO } from "../../../Common/Models/InvoiceDTO";
import { AccountingRowDTO } from "../../../Common/Models/AccountingRowDTO";
import { TermGroup_AccountStatus, SoePaymentStatus } from "../../../Util/CommonEnumerations";

export class SupplierPaymentValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                invoice: '=',
                accountingRows: '=',
                isBaseCurrency: '=',
                accountYearIsOpen: '=',
                accountPeriod: '=',
                defaultVoucherSeriesId: '=',
                selectedSupplier: '=',
                selectedPaymentMethod: '=',
                selectedPayDate: '=',
                selectedPayToAccount: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['invoice', 'accountingRows', 'isBaseCurrency', 'accountYearIsOpen', 'accountPeriod', 'defaultVoucherSeriesId', 'selectedSupplier', 'selectedPaymentMethod', 'selectedPayDate', 'selectedPayToAccount'], (newValues, oldValues, scope) => {
                    // Account year
                    ngModelController.$setValidity("accountYearStatus", scope['accountYearIsOpen'] === true);

                    // Account period
                    var accountPeriod = scope['accountPeriod'];
                    ngModelController.$setValidity("accountPeriod", accountPeriod !== null);

                    if (accountPeriod != null)
                        ngModelController.$setValidity("accountPeriodStatus", accountPeriod.status === TermGroup_AccountStatus.Open);

                    // Voucher series
                    ngModelController.$setValidity("defaultVoucherSeries", scope['defaultVoucherSeriesId'] !== 0);

                    // Supplier
                    ngModelController.$setValidity("supplier", scope['selectedSupplier'] !== null);

                    // Payment method
                    ngModelController.$setValidity("paymentMethod", scope['selectedPaymentMethod'] !== null);

                    // Pay date
                    ngModelController.$setValidity("payDate", scope['selectedPayDate'] !== null);

                    // Payment info
                    ngModelController.$setValidity("payToAccount", scope['selectedPayToAccount'] !== null);

                    var payment: PaymentRowDTO = ngModelController.$modelValue;
                    var invoice: SupplierInvoiceDTO = scope['invoice'];
                    var rows: AccountingRowDTO[] = scope['accountingRows'];
                    var isBaseCurrency: boolean = scope['isBaseCurrency'] === true;

                    var invalidRows: boolean = false;

                    if (payment && invoice) {
                        var isPendingPayment: boolean = (payment.status === SoePaymentStatus.Pending || payment.status === SoePaymentStatus.Error);
                        var hasVoucher: boolean = (payment.voucherHeadId && payment.voucherHeadId !== 0);

                        if (!rows || rows.length === 0) {
                            invalidRows = true;
                        } else if (!isPendingPayment && !hasVoucher && isBaseCurrency) {
                            var compareAmount: number = 0;
                            var noOfAccRows: number = 0;
                            var remainingAmount: number = payment.totalAmountCurrency - payment.paidAmountCurrency;

                            if (invoice.totalAmount < 0) {
                                compareAmount = _.sumBy(rows, (row) => { return row.creditAmountCurrency });
                                noOfAccRows = (compareAmount < remainingAmount ? _.filter(rows, { isCreditRow: true }) : _.filter(rows, { isDebitRow: true })).length;
                            } else {
                                compareAmount = _.sumBy(rows, (row) => { return row.debitAmountCurrency });
                                noOfAccRows = (compareAmount > remainingAmount ? _.filter(rows, { isDebitRow: true }) : _.filter(rows, { isCreditRow: true })).length;
                            }

                            if (Math.abs(remainingAmount) != compareAmount && payment.fullyPaid && noOfAccRows <= 1) {
                                invalidRows = true;
                            } else {
                                var payAmount: number = isBaseCurrency ? payment.amount : payment.amountCurrency;
                                if (Math.abs(payAmount) != compareAmount) {
                                    if (payment.fullyPaid) {
                                        if (noOfAccRows === 1)
                                            invalidRows = true;
                                    } else {
                                        invalidRows = true;
                                    }
                                }
                            }
                        }
                    }
                    ngModelController.$setValidity("rowAmounts", !invalidRows);
                });
            }
        }
    }
}
