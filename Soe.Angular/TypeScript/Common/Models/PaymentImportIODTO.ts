import { IPaymentImportIODTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_BillingType, ImportPaymentIOStatus, ImportPaymentIOState, ImportPaymentType } from "../../Util/CommonEnumerations";

export class PaymentImportIODTO implements IPaymentImportIODTO {
    paymentImportIOId: number;
    actorCompanyId: number;
    batchNr: number;
    type: TermGroup_BillingType;
    customerId: number;
    customer: string;
    dueDate: Date;
    invoiceId: number;
    invoiceNr: string;
    invoiceAmount: number;
    restAmount: number;
    paidAmount: number;
    currency: string;
    invoiceDate: Date;
    paidDate: Date;
    matchCodeId: number;
    status: ImportPaymentIOStatus;
    state: ImportPaymentIOState;
    invoiceSeqnr: string;
    paidAmountCurrency: number;
    importType: ImportPaymentType;
    isSelected: boolean;
    isFullyPaid: boolean;
    isVisible: boolean;
    amountDiff: number;
    typeName: string;
    statusName: string;
    stateName: string;
    paymentTypeName: string;
    matchCodeName: string;
    paymentRowId: number;
    paymentRowSeqNr: number;
    ocr: string;
    comment: string;

    // Extension
    tempRowId: number;
    isModified: boolean;
}
