import { ICustomerInvoiceRowDTO, ICustomerInvoiceRowSmallDTO } from "../../Scripts/TypeLite.Net4";
import { AccountingRowDTO } from "./AccountingRowDTO";
import { SoeInvoiceRowType, SoeOriginType, SoeInvoiceRowDiscountType, SoeEntityState } from "../../Util/CommonEnumerations";

export class CustomerInvoiceRowDTO implements ICustomerInvoiceRowDTO {
    accountingRows: AccountingRowDTO[];
    amount: number;
    amountCurrency: number;
    amountEntCurrency: number;
    amountFormula: string;
    amountLedgerCurrency: number;
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    created: Date;
    createdBy: string;
    currencyCode: string;
    customerInvoiceInterestId: number;
    customerInvoiceReminderId: number;
    customerInvoiceRowId: number;
    date: Date;
    dateTo: Date;
    deliveryDateText: string;
    detailVisible: boolean;
    discountAmount: number;
    discountAmountCurrency: number;
    discountAmountEntCurrency: number;
    discountAmountLedgerCurrency: number;
    discountPercent: number;
    discountType: number;
    discountValue: number;
    ediEntryId: number;
    ediTextValue: string;
    hasMultipleSalesRows: boolean;
    householdAmount: number;
    householdAmountCurrency: number;
    householdApartmentNbr: string;
    householdApplied: boolean;
    householdAppliedDate: Date;
    householdCooperativeOrgNbr: string;
    householdDeductionType: number;
    householdName: string;
    householdProperty: string;
    householdReceived: boolean;
    householdReceivedDate: Date;
    householdSocialSecNbr: string;
    houseHoldTaxDeductionType: number;
    intrastatCodeId: number;
    intrastatTransactionId: number;
    invoiceId: number;
    invoiceNr: string;
    invoiceQuantity: number;
    isCentRoundingRow: boolean;
    isClearingProduct: boolean;
    isContractProduct: boolean;
    isFixedPriceProduct: boolean;
    isFreightAmountRow: boolean;
    isHouseholdTextRow: boolean;
    isInterestRow: boolean;
    isInvoiceFeeRow: boolean;
    isLiftProduct: boolean;
    isLocked: boolean;
    isManuallyAdjusted: boolean;
    isModified: boolean;
    isReminderRow: boolean;
    isSelectDisabled: boolean;
    isSelected: boolean;
    isStockRow: boolean;
    isSupplementChargeProduct: boolean;
    isTimeProjectRow: boolean;
    marginalIncome: number;
    marginalIncomeCurrency: number;
    marginalIncomeEntCurrency: number;
    marginalIncomeLedgerCurrency: number;
    marginalIncomeLimit: number;
    marginalIncomeRatio: number;
    modified: Date;
    modifiedBy: string;
    originType: SoeOriginType;
    parentRowId: number;
    previouslyInvoicedQuantity: number;
    productId: number;
    productName: string;
    productNr: string;
    productUnitCode: string;
    productUnitId: number;
    projectId: number;
    purchasePrice: number;
    purchasePriceCurrency: number;
    purchasePriceEntCurrency: number;
    purchasePriceLedgerCurrency: number;
    quantity: number;
    rowNr: number;
    rowState: number;
    rowStateName: string;
    state: SoeEntityState;
    stockCode: string;
    stockId: number;
    sumAmount: number;
    sumAmountCurrency: number;
    sumAmountEntCurrency: number;
    sumAmountLedgerCurrency: number;
    supplementCharge: number;
    supplementChargePercent: number;
    supplierInvoiceId: number;
    sysCountryId: number;
    sysWholesellerName: string;
    targetRowId: number;
    tempRowId: number;
    text: string;
    timeManuallyChanged: boolean;
    timeManuallyChangedText: string;
    type: SoeInvoiceRowType;
    vatAccountEnabled: boolean;
    vatAccountId: number;
    vatAccountName: string;
    vatAccountNr: string;
    vatAmount: number;
    vatAmountCurrency: number;
    vatAmountEntCurrency: number;
    vatAmountLedgerCurrency: number;
    vatCodeCode: string;
    vatCodeId: number;
    vatRate: number;

    // Extensions
    isReadOnly: boolean;
    discountTypeText: string;
    householdDeductionTypeText: string;

    public get isProductRow(): boolean {
        return this.type === SoeInvoiceRowType.ProductRow;
    }

    public static toAccountingRowDTOs(rows: CustomerInvoiceRowDTO[]): AccountingRowDTO[] {
        var dtos: AccountingRowDTO[] = [];

        _.forEach(rows, (row) => {
            _.forEach(row.accountingRows, (accRow) => {
                dtos.push(accRow);
            });
        });

        dtos = dtos.map(dto => {
            var obj = new AccountingRowDTO();
            angular.extend(obj, dto);
            return obj;
        });

        return dtos;
    }

    public static getNextRowNr(rows: CustomerInvoiceRowDTO[]) {
        var rowNr = 0;
        var maxRow = _.maxBy(rows, 'rowNr');
        if (maxRow)
            rowNr = maxRow.rowNr;

        return rowNr + 1;
    }
}

export class CustomerInvoiceRowSmallDTO implements ICustomerInvoiceRowSmallDTO {
    amountCurrency: number;
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    currencyCode: string;
    customerInvoiceRowId: number;
    deliveryDateText: string;
    discountType: number;
    discountValue: number;
    ediEntryId: number;
    ediTextValue: string;
    invoiceId: number;
    marginalIncomeLimit: number;
    previouslyInvoicedQuantity: number;
    productId: number;
    productName: string;
    productNr: string;
    productUnitCode: string;
    quantity: number;
    rowNr: number;
    sumAmountCurrency: number;
    text: string;
    type: SoeInvoiceRowType;
    vATAmountCurrency: number;
    vatRate: number;
}
