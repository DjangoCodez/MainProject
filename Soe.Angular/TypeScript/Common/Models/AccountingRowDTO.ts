import { IAccountingRowDTO, ISplitAccountingRowDTO } from "../../Scripts/TypeLite.Net4";
import { AccountingRowType, SoeEntityState, TermGroup_CurrencyType } from "../../Util/CommonEnumerations";

export class AccountingRowDTO implements IAccountingRowDTO {
    accountDistributionHeadId: number;
    accountDistributionNbrOfPeriods: number;
    accountDistributionStartDate: Date;
    amount: number;
    amountCurrency: number;
    amountEntCurrency: number;
    amountLedgerCurrency: number;
    amountStop: number;
    attestStatus: number;
    attestUserId: number;
    attestUserName: string;
    balance: number;
    creditAmount: number;
    creditAmountCurrency: number;
    creditAmountEntCurrency: number;
    creditAmountLedgerCurrency: number;
    date: Date;
    debitAmount: number;
    debitAmountCurrency: number;
    debitAmountEntCurrency: number;
    debitAmountLedgerCurrency: number;
    inventoryId: number;
    invoiceAccountRowId: number;
    invoiceId: number;
    invoiceRowId: number;
    isCentRoundingRow: boolean;
    isClaimRow: boolean;
    isContractorVatRow: boolean;
    isCreditRow: boolean;
    isDebitRow: boolean;
    isDeleted: boolean;
    isErrorRow: boolean;
    isHouseholdRow: boolean;
    isInterimRow: boolean;
    isManuallyAdjusted: boolean;
    isModified: boolean;
    isProcessed: boolean;
    isTemplateRow: boolean;
    isVatRow: boolean;
    mergeSign: number;
    parentRowId: number;
    productName: string;
    productRowNr: number;
    quantity: number;
    quantityStop: boolean;
    rowNr: number;
    rowTextStop: boolean;
    splitPercent: number;
    splitType: number;
    splitValue: number;
    state: SoeEntityState;
    tempInvoiceRowId: number;
    tempRowId: number;
    text: string;
    type: AccountingRowType;
    unit: string;
    voucherRowId: number;
    voucherRowMergeType: any;
    dim1Disabled: boolean;
    dim1Id: number;
    dim1Mandatory: boolean;
    dim1ManuallyChanged: boolean;
    dim1Name: string;
    dim1Nr: string;
    dim1Stop: boolean;
    dim2Disabled: boolean;
    dim2Id: number;
    dim2Mandatory: boolean;
    dim2ManuallyChanged: boolean;
    dim2Name: string;
    dim2Nr: string;
    dim2Stop: boolean;
    dim3Disabled: boolean;
    dim3Id: number;
    dim3Mandatory: boolean;
    dim3ManuallyChanged: boolean;
    dim3Name: string;
    dim3Nr: string;
    dim3Stop: boolean;
    dim4Disabled: boolean;
    dim4Id: number;
    dim4Mandatory: boolean;
    dim4ManuallyChanged: boolean;
    dim4Name: string;
    dim4Nr: string;
    dim4Stop: boolean;
    dim5Disabled: boolean;
    dim5Id: number;
    dim5Mandatory: boolean;
    dim5ManuallyChanged: boolean;
    dim5Name: string;
    dim5Nr: string;
    dim5Stop: boolean;
    dim6Disabled: boolean;
    dim6Id: number;
    dim6Mandatory: boolean;
    dim6ManuallyChanged: boolean;
    dim6Name: string;
    dim6Nr: string;
    dim6Stop: boolean;

    // Extensions
    isAccrualAccount: boolean;
    voucherHeadId: number;

    //Accruals
    startDate?: Date;
    numberOfPeriods?: number;

    //used to show errors in the grid.
    dim1Error: string;
    dim2Error: string;
    dim3Error: string;
    dim4Error: string;
    dim5Error: string;
    dim6Error: string;

    private static readonly DECIMAL_PLACES: number = 2;

    constructor() {
        this.amount = 0;
        this.amountCurrency = 0;
        this.amountEntCurrency = 0;
        this.amountLedgerCurrency = 0;
        this.creditAmount = 0;
        this.creditAmountCurrency = 0;
        this.creditAmountEntCurrency = 0;
        this.creditAmountLedgerCurrency = 0;
        this.debitAmount = 0;
        this.debitAmountCurrency = 0;
        this.debitAmountEntCurrency = 0;
        this.debitAmountLedgerCurrency = 0;
    }

    public clearRowIds(keepTempIds: boolean) {
        this.invoiceRowId = 0;
        this.invoiceAccountRowId = 0;
        if (!keepTempIds) {
            this.tempRowId = 0;
            this.tempInvoiceRowId = 0;
            this.parentRowId = undefined;
        }
    }

    public getAmount(currencyType: TermGroup_CurrencyType) {
        var amount: number = 0;

        switch (currencyType) {
            case TermGroup_CurrencyType.BaseCurrency:
                if (typeof this.amount != 'undefined')
                    amount = this.amount;
                else if (typeof this.amountCurrency != 'undefined')
                    amount = this.amountCurrency;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                if (typeof this.amount != 'undefined')
                    amount = this.amount;
                else if (typeof this.amountEntCurrency != 'undefined')
                    amount = this.amountEntCurrency;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                if (typeof this.amount != 'undefined')
                    amount = this.amount;
                else if (typeof this.amountLedgerCurrency != 'undefined')
                    amount = this.amountLedgerCurrency;
                break;
            case TermGroup_CurrencyType.TransactionCurrency:
                if (typeof this.amountCurrency != 'undefined')
                    amount = this.amountCurrency;
                else if (typeof this.amount != 'undefined')
                    amount = this.amount;
                break;
        }

        return amount;
    }

    public getDebitAmount(currencyType: TermGroup_CurrencyType) {
        var amount: number = 0;

        switch (currencyType) {
            case TermGroup_CurrencyType.BaseCurrency:
                amount = this.debitAmount;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                amount = this.debitAmountEntCurrency;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                amount = this.debitAmountLedgerCurrency;
                break;
            case TermGroup_CurrencyType.TransactionCurrency:
                amount = this.debitAmountCurrency;
                break;
        }

        return amount;
    }

    public getCreditAmount(currencyType: TermGroup_CurrencyType) {
        var amount: number = 0;

        switch (currencyType) {
            case TermGroup_CurrencyType.BaseCurrency:
                amount = this.creditAmount;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                amount = this.creditAmountEntCurrency;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                amount = this.creditAmountLedgerCurrency;
                break;
            case TermGroup_CurrencyType.TransactionCurrency:
                amount = this.creditAmountCurrency;
                break;
        }

        return amount;
    }

    public setAmount(currencyType: TermGroup_CurrencyType, amount: number) {
        switch (currencyType) {
            case TermGroup_CurrencyType.BaseCurrency:
                this.amount = amount;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                this.amountEntCurrency = amount;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                this.amountLedgerCurrency = amount;
                break;
            case TermGroup_CurrencyType.TransactionCurrency:
                this.amountCurrency = amount;
                break;
        }

        if (amount < 0)
            this.setCreditAmount(currencyType, Math.abs(amount), false);
        else
            this.setDebitAmount(currencyType, amount, false);
    }

    public setDebitAmount(currencyType: TermGroup_CurrencyType, amount: number, updateAmount: boolean = true) {
        switch (currencyType) {
            case TermGroup_CurrencyType.BaseCurrency:
                this.debitAmount = amount;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                this.debitAmountEntCurrency = amount;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                this.debitAmountLedgerCurrency = amount;
                break;
            case TermGroup_CurrencyType.TransactionCurrency:
                this.debitAmountCurrency = amount;
                break;
        }

        if (updateAmount)
            this.updateAmount();
    }

    public setCreditAmount(currencyType: TermGroup_CurrencyType, amount: number, updateAmount: boolean = true) {
        switch (currencyType) {
            case TermGroup_CurrencyType.BaseCurrency:                
                this.creditAmount = amount;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                this.creditAmountEntCurrency = amount;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                this.creditAmountLedgerCurrency = amount;
                break;
            case TermGroup_CurrencyType.TransactionCurrency:
                this.creditAmountCurrency = amount;
                break;
        }

        if (updateAmount)
            this.updateAmount();
    }

    public updateAmount() {
        this.amount = this.debitAmount - this.creditAmount;
        this.amountCurrency = (
            this.debitAmountCurrency - this.creditAmountCurrency
        ).round(AccountingRowDTO.DECIMAL_PLACES);
        this.amountEntCurrency = (
            this.debitAmountEntCurrency - this.creditAmountEntCurrency
        ).round(AccountingRowDTO.DECIMAL_PLACES);
        this.amountLedgerCurrency = (
            this.debitAmountLedgerCurrency - this.creditAmountLedgerCurrency
        ).round(AccountingRowDTO.DECIMAL_PLACES);
    }

    public static clearRowIds(rows: IAccountingRowDTO[], keepTempIds: boolean) {
        var counter: number = 1;
        _.forEach(rows, (row) => {
            row.invoiceRowId = 0;
            row.invoiceAccountRowId = 0;

            if (!row.rowNr)
                row.rowNr = counter;

            if (!keepTempIds) {
                row.tempRowId = 0;
                row.tempInvoiceRowId = 0;
            }
            counter++;
        });
    }

    public static getNextRowNr(rows: IAccountingRowDTO[]) {
        var rowNr = 0;
        var maxRow = _.maxBy(rows, 'rowNr');
        if (maxRow)
            rowNr = maxRow.rowNr;

        return rowNr + 1;
    }

    public static invertAmounts(rows: IAccountingRowDTO[]) {
        _.forEach(rows, (row) => {
            row.isDebitRow = !row.isDebitRow;
            row.isCreditRow = !row.isCreditRow;

            var debitAmount = row.debitAmount;
            row.debitAmount = row.creditAmount;
            row.creditAmount = debitAmount;

            debitAmount = row.debitAmountCurrency;
            row.debitAmountCurrency = row.creditAmountCurrency;
            row.creditAmountCurrency = debitAmount;

            debitAmount = row.debitAmountEntCurrency;
            row.debitAmountEntCurrency = row.creditAmountEntCurrency;
            row.creditAmountEntCurrency = debitAmount;

            debitAmount = row.debitAmountLedgerCurrency;
            row.debitAmountLedgerCurrency = row.creditAmountLedgerCurrency;
            row.creditAmountLedgerCurrency = debitAmount;
        });
    }
}

export class SplitAccountingRowDTO implements ISplitAccountingRowDTO {
    amountCurrency: number;
    creditAmountCurrency: number;
    debitAmountCurrency: number;
    dim1Disabled: boolean;
    dim1Id: number;
    dim1Mandatory: boolean;
    dim1Name: string;
    dim1Nr: string;
    dim1Stop: boolean;
    dim2Disabled: boolean;
    dim2Id: number;
    dim2Mandatory: boolean;
    dim2Name: string;
    dim2Nr: string;
    dim2Stop: boolean;
    dim3Disabled: boolean;
    dim3Id: number;
    dim3Mandatory: boolean;
    dim3Name: string;
    dim3Nr: string;
    dim3Stop: boolean;
    dim4Disabled: boolean;
    dim4Id: number;
    dim4Mandatory: boolean;
    dim4Name: string;
    dim4Nr: string;
    dim4Stop: boolean;
    dim5Disabled: boolean;
    dim5Id: number;
    dim5Mandatory: boolean;
    dim5Name: string;
    dim5Nr: string;
    dim5Stop: boolean;
    dim6Disabled: boolean;
    dim6Id: number;
    dim6Mandatory: boolean;
    dim6Name: string;
    dim6Nr: string;
    dim6Stop: boolean;
    invoiceAccountRowId: number;
    isCreditRow: boolean;
    isDebitRow: boolean;
    splitPercent: number;
    splitType: number;
    splitValue: number;

    // Extensions
    dim1Error: string;
    dim2Error: string;
    dim3Error: string;
    dim4Error: string;
    dim5Error: string;
    dim6Error: string;

    // Flags
    excludeFromSplit: boolean;
}
