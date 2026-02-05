import { SupplierInvoiceRowIODTO } from "./SupplierInvoiceIORowDTO";

export class SupplierInvoiceIODTO  {

    invoiceRows: SupplierInvoiceRowIODTO[];
    invoiceNr: string;
    invoiceId: number;
    seqNr: number;
    originStatus: number;
    originStatusName: string;
    batchId: string;
    supplierId: number;
    supplierNr: string;
    supplierExternalNr: string;
    supplierName: string;
    supplierOrgnr: string;
    invoiceDate: Date;
    dueDate: Date;
    voucherDate: Date;
    referenceOur: string;
    referenceYour: string;
    currencyRate: number;
    currencyDate: Date;
    currency: string;
    totalAmount: number;
    totalAmountCurrency: number;
    vatAmount: number;
    vatAmountCurrency: number;
    paidAmount: number;
    paidAmountCurrency: number;
    remainingAmount: number;
    centRounding: number;
    fullyPayed: boolean;
    paymentNr: string;
    voucherNr: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    vatType: number;
    paymentConditionCode: string;
    vatAccountNr: string;
    workingDescription: string;
    internalDescription: string;
    externalDescription: string;
    projectNr: string;
    billingType: number;
    invoiceLabel: string;
    invoiceHeadText: string;
    externalId: string;
    ocr: string;
   
}
