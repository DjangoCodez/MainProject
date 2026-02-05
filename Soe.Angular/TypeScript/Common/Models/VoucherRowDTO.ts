import { AccountingRowDTO } from "./AccountingRowDTO";
import { IVoucherRowDTO } from "../../Scripts/TypeLite.Net4";
import { AccountInternalDTO } from "./AccountInternalDTO";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class VoucherRowDTO implements IVoucherRowDTO {
    accountDistributionHeadId: number;
    accountInternalDTO_forReports: AccountInternalDTO[];
    amount: number;
    amountEntCurrency: number;
    date: Date;
    dim1AccountType: number;
    dim1AmountStop: number;
    dim1Id: number;
    dim1Name: string;
    dim1Nr: string;
    dim1UnitStop: boolean;
    dim2Id: number;
    dim2Name: string;
    dim2Nr: string;
    dim3Id: number;
    dim3Name: string;
    dim3Nr: string;
    dim4Id: number;
    dim4Name: string;
    dim4Nr: string;
    dim5Id: number;
    dim5Name: string;
    dim5Nr: string;
    dim6Id: number;
    dim6Name: string;
    dim6Nr: string;
    merged: boolean;
    parentRowId: number;
    quantity: number;
    state: SoeEntityState;
    sysVatAccountId: number;
    tempRowId: number;
    text: string;
    voucherHeadId: number;
    voucherNr: number;
    voucherRowId: number;
    voucherSeriesTypeName: string;
    voucherSeriesTypeNr: number;
    rowNr: number;

    public static toAccountingRowDTO(row: VoucherRowDTO): AccountingRowDTO {
        var dto: AccountingRowDTO = new AccountingRowDTO();

        dto.invoiceRowId = row.voucherRowId;
        dto.tempInvoiceRowId = row.voucherRowId;
        dto.tempRowId = row.voucherRowId;
        dto.voucherHeadId = row.voucherHeadId;
        dto.accountDistributionHeadId = row.accountDistributionHeadId ? row.accountDistributionHeadId : 0;
        dto.parentRowId = row.parentRowId ? row.parentRowId : 0;
        dto.date = row.date;
        dto.text = row.text;
        dto.quantity = row.quantity;
        dto.amount = row.amount;
        dto.amountEntCurrency = row.amountEntCurrency;
        dto.creditAmount = row.amount < 0 ? Math.abs(row.amount) : 0;
        dto.creditAmountEntCurrency = row.amountEntCurrency < 0 ? Math.abs(row.amountEntCurrency) : 0;
        dto.creditAmountCurrency = row.amount < 0 ? Math.abs(row.amount) : 0;       // CreditAmountCurrency missing in VoucherRow
        dto.creditAmountLedgerCurrency = row.amount < 0 ? Math.abs(row.amount) : 0; // CreditAmountLedgerCurrency missing in VoucherRow
        dto.debitAmount = row.amount > 0 ? row.amount : 0;
        dto.debitAmountEntCurrency = row.amountEntCurrency > 0 ? row.amountEntCurrency : 0;
        dto.debitAmountCurrency = row.amount > 0 ? row.amount : 0;                  // DebitAmountCurrency missing in VoucherRow
        dto.debitAmountLedgerCurrency = row.amount > 0 ? row.amount : 0;            // CreditAmountLedgerCurrency missing in VoucherRow
        dto.isCreditRow = row.amount < 0;
        dto.isDebitRow = row.amount > 0;
        dto.isTemplateRow = false;
        dto.state = row.state;
        dto.rowNr = row.rowNr;

        // Standard account
        dto.dim1Id = row.dim1Id;
        dto.dim1Nr = row.dim1Nr;
        dto.dim1Name = row.dim1Name;
        dto.dim1Disabled = false;
        dto.dim1Mandatory = true;
        dto.quantityStop = row.dim1UnitStop;
        dto.amountStop = row.dim1AmountStop;

        // Internal accounts (dim 2-6)
        dto.dim2Id = row.dim2Id;
        dto.dim2Nr = row.dim2Nr ? row.dim2Nr : "";
        dto.dim2Name = row.dim2Name ? row.dim2Name : "";
        dto.dim3Id = row.dim3Id;
        dto.dim3Nr = row.dim3Nr ? row.dim3Nr : "";
        dto.dim3Name = row.dim3Name ? row.dim3Name : "";
        dto.dim4Id = row.dim4Id;
        dto.dim4Nr = row.dim4Nr ? row.dim4Nr : "";
        dto.dim4Name = row.dim4Name ? row.dim4Name : "";
        dto.dim5Id = row.dim5Id;
        dto.dim5Nr = row.dim5Nr ? row.dim5Nr : "";
        dto.dim5Name = row.dim5Name ? row.dim5Name : "";
        dto.dim6Id = row.dim6Id;
        dto.dim6Nr = row.dim6Nr ? row.dim6Nr : "";
        dto.dim6Name = row.dim6Name ? row.dim6Name : "";

        return dto;
    }

    public static toAccountingRowDTOs(rows: VoucherRowDTO[]): AccountingRowDTO[] {
        var dtos: AccountingRowDTO[] = [];

        _.forEach(_.orderBy(rows, 'rowNr'), (row) => {
            dtos.push(this.toAccountingRowDTO(row));
        });

        return dtos;
    }
}