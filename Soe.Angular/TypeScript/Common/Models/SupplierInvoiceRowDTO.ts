import { ISupplierInvoiceRowDTO } from "../../Scripts/TypeLite.Net4";
import { AccountingRowDTO } from "./AccountingRowDTO";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class SupplierInvoiceRowDTO implements ISupplierInvoiceRowDTO {

    supplierInvoiceRowId: number;
    invoiceId: number;
    quantity: number;
    amount: number;
    amountCurrency: number;
    amountEntCurrency: number;
    amountLedgerCurrency: number;
    vatAmount: number;
    vatAmountCurrency: number;
    vatAmountEntCurrency: number;
    vatAmountLedgerCurrency: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    accountingRows: AccountingRowDTO[];

    public static toAccountingRowDTOs(rows: SupplierInvoiceRowDTO[]): AccountingRowDTO[] {
        var dtos: AccountingRowDTO[] = [];

        _.forEach(rows, (row) => {
            _.forEach(row.accountingRows, (accRow) => {
                // Fix date
                if (accRow.date)
                    accRow.date = new Date(<any>accRow.date);
                if (accRow.startDate)
                    accRow.startDate = new Date(<any>accRow.startDate);
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
}