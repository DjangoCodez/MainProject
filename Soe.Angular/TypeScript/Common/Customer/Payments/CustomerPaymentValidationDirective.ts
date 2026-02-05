import { CoreUtility } from "../../../Util/CoreUtility";
import { PaymentRowDTO } from "../../Models/PaymentRowDTO";
import { CustomerInvoiceDTO } from "../../Models/InvoiceDTO";
import { AccountingRowDTO } from "../../Models/AccountingRowDTO";
import { TermGroup_AccountStatus, SoePaymentStatus, TermGroup_Languages } from "../../../Util/CommonEnumerations";

export class CustomerPaymentValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                invoice: '=',
                accountingRows: '=',
                hasModifiedAccountingRows: '=',
                isBaseCurrency: '=',
                accountYearIsOpen: '=',
                accountPeriod: '=',
                defaultVoucherSeriesId: '=',
                selectedCustomer: '=',
                selectedPaymentMethod: '=',
                selectedPayDate: '=',
                selectedVoucherDate: '='
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watchGroup(['invoice', 'accountingRows', 'hasModifiedAccountingRows', 'isBaseCurrency', 'accountYearIsOpen', 'defaultVoucherSeriesId', 'selectedCustomer', 'selectedPaymentMethod', 'selectedPayDate', 'selectedVoucherDate'], (newValues, oldValues, scope) => {
                    // Account year
                    ngModelController.$setValidity("accountYearStatus", scope['accountYearIsOpen'] === true);

                    // Account period
                    const accountPeriod = scope['accountPeriod'];
                    ngModelController.$setValidity("accountPeriod", accountPeriod !== null);

                    if (accountPeriod != null)
                        ngModelController.$setValidity("accountPeriodStatus", accountPeriod.status === TermGroup_AccountStatus.Open);

                    // Voucher series
                    ngModelController.$setValidity("defaultVoucherSeries", scope['defaultVoucherSeriesId'] !== 0);

                    // Supplier
                    ngModelController.$setValidity("customer", scope['selectedCustomer'] !== null);

                    // Payment method
                    ngModelController.$setValidity("paymentMethod", scope['selectedPaymentMethod'] !== null);

                    // Pay date
                    ngModelController.$setValidity("payDate", scope['selectedPayDate'] !== null);

                    // Payment info
                    ngModelController.$setValidity("voucherDate", scope['selectedVoucherDate'] !== null);

                    const payment: PaymentRowDTO = ngModelController.$modelValue;
                    const invoice: CustomerInvoiceDTO = scope['invoice'];
                    const rows: AccountingRowDTO[] = scope['accountingRows'];
                    const isBaseCurrency: boolean = scope['isBaseCurrency'] === true;

                    let invalidRows: boolean = false;

                    if (payment && invoice && invoice.invoiceId) {
                        const isPendingPayment: boolean = (payment.status === SoePaymentStatus.Pending);
                        const hasVoucher: boolean = (payment.voucherHeadId && payment.voucherHeadId !== 0);

                        if ( (!rows || rows.length === 0) && !payment.isRestPayment) {
                            invalidRows = true;
                        } else if (!isPendingPayment && !hasVoucher) {
                            let compareAmount: number = 0;
                            let noOfAccRows: number = 0;

                            if (invoice.totalAmount < 0) {
                                compareAmount = _.sumBy(rows, (row) => { return row.creditAmountCurrency ? row.creditAmountCurrency : 0 });
                                compareAmount = -compareAmount;
                                noOfAccRows = _.filter(rows, { isCreditRow: true }).length;
                            } else {
                                compareAmount = _.sumBy(rows, (row) => { return row.debitAmountCurrency ? row.debitAmountCurrency : 0 });
                                noOfAccRows = _.filter(rows, { isDebitRow: true }).length;
                            }

                            const payAmount: number = isBaseCurrency ? payment.amount : payment.amountCurrency;

                            if ((payAmount != compareAmount && !payment.fullyPaid) || noOfAccRows < 1) {
                                //this validation does not concern Finnish users                                    
                                if (CoreUtility.sysCountryId != TermGroup_Languages.Finnish)
                                    invalidRows = true;
                            }
                        }
                    }
                    ngModelController.$setValidity("rowAmounts", !invalidRows);
                });
            }
        }
    }
}
