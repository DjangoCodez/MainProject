import { NumberUtility } from "../../../Util/NumberUtility";
import { AccountingRowDTO } from "../../Models/AccountingRowDTO";

export class AccountingRowsValidationDirectiveFactory {
    //@ngInject
    public static create(): ng.IDirective {
        return {
            restrict: 'A',
            require: '?ngModel',
            scope: {
                ngModel: '=',
                allowZeroAmount: '=',
                allowUnbalancedRows: '=',
                ignoreInternal: '=?'
            },
            link(scope: ng.IScope, elem: JQuery, attributes: ng.IAttributes, ngModelController: ng.INgModelController) {
                scope.$watch(() => (ngModelController.$modelValue), (newValue, oldvalue, scope) => {

                    if (newValue) {
                        var accountingRows: AccountingRowDTO[] = ngModelController.$modelValue;

                        var accountStandardMissing: boolean = false;
                        var accountInternalMissing: boolean = false;
                        var amountIsZero: boolean = false;
                        var hasDiff: boolean = false;

                        var debitSum: number = 0;
                        var creditSum: number = 0;
                        var debitTransSum: number = 0;
                        var creditTransSum: number = 0;
                        var debitEntSum: number = 0;
                        var creditEntSum: number = 0;
                        var debitLedgerSum: number = 0;
                        var creditLedgerSum: number = 0;

                        _.forEach(_.filter(accountingRows, r => !r.isDeleted), (row) => {
                            var debit: number = isNaN(row.debitAmount) ? 0 : row.debitAmount;
                            var credit: number = isNaN(row.creditAmount) ? 0 : row.creditAmount;
                            var debitTrans: number = isNaN(row.debitAmountCurrency) ? 0 : row.debitAmountCurrency;
                            var creditTrans: number = isNaN(row.creditAmountCurrency) ? 0 : row.creditAmountCurrency;
                            var debitEnt: number = isNaN(row.debitAmountEntCurrency) ? 0 : row.debitAmountEntCurrency;
                            var creditEnt: number = isNaN(row.creditAmountEntCurrency) ? 0 : row.creditAmountEntCurrency;
                            var debitLedger: number = isNaN(row.debitAmountLedgerCurrency) ? 0 : row.debitAmountLedgerCurrency;
                            var creditLedger: number = isNaN(row.creditAmountLedgerCurrency) ? 0 : row.creditAmountLedgerCurrency;

                            // Calculate sum (for diff)
                            if (scope['allowUnbalancedRows'] === false) {
                                debitSum += NumberUtility.parseNumericDecimal(debit);
                                creditSum += NumberUtility.parseNumericDecimal(credit);
                                debitTransSum += NumberUtility.parseNumericDecimal(debitTrans);
                                creditTransSum += NumberUtility.parseNumericDecimal(creditTrans);
                                debitEntSum += NumberUtility.parseNumericDecimal(debitEnt);
                                creditEntSum += NumberUtility.parseNumericDecimal(creditEnt);
                                debitLedgerSum += NumberUtility.parseNumericDecimal(debitLedger);
                                creditLedgerSum += NumberUtility.parseNumericDecimal(creditLedger);
                            }

                            // Ignore empty rows (no account and no amounts)
                            if ((!row.dim1Id || row.dim1Id === 0) && debit === 0 && credit === 0 && debitTrans === 0 && creditTrans === 0 && debitEnt === 0 && creditEnt === 0 && debitLedger === 0 && creditLedger === 0) {
                                // Will not break, but continue
                                return;
                            }

                            // Standard account mandatory if any amount is set
                            if (!row.dim1Id || row.dim1Id === 0)
                                accountStandardMissing = true;

                            // Mandatory internal accounts
                            if (!scope['ignoreInternal']) {
                                if (row.dim2Mandatory && (!row.dim2Id || row.dim2Id === 0))
                                    accountInternalMissing = true;
                                if (row.dim3Mandatory && (!row.dim3Id || row.dim3Id === 0))
                                    accountInternalMissing = true;
                                if (row.dim4Mandatory && (!row.dim4Id || row.dim4Id === 0))
                                    accountInternalMissing = true;
                                if (row.dim5Mandatory && (!row.dim5Id || row.dim5Id === 0))
                                    accountInternalMissing = true;
                            }

                            // Amount can not be zero on both debit and credit, unless parameter says so
                            if (scope['allowZeroAmount'] === false) {
                                if (debit - credit === 0)
                                    amountIsZero = true;
                            }
                        });

                        debitSum = parseFloat(debitSum.toFixed(2));
                        creditSum = parseFloat(creditSum.toFixed(2));
                        debitTransSum = parseFloat(debitTransSum.toFixed(2));
                        creditTransSum = parseFloat(creditTransSum.toFixed(2));
                        debitEntSum = parseFloat(debitEntSum.toFixed(2));
                        creditEntSum = parseFloat(creditEntSum.toFixed(2));
                        debitLedgerSum = parseFloat(debitLedgerSum.toFixed(2));
                        creditLedgerSum = parseFloat(creditLedgerSum.toFixed(2));

                        // Diff
                        if (scope['allowUnbalancedRows'] === false) {
                            if (debitSum !== creditSum || debitTransSum !== creditTransSum || debitEntSum !== creditEntSum || debitLedgerSum !== creditLedgerSum)
                                hasDiff = true;
                        }

                        ngModelController.$setValidity("accountStandard", accountStandardMissing === false);
                        ngModelController.$setValidity("accountInternal", accountInternalMissing === false);
                        ngModelController.$setValidity("rowAmount", amountIsZero === false);
                        ngModelController.$setValidity("amountDiff", hasDiff === false);
                    }
                }, true);
            }
        }
    }
}