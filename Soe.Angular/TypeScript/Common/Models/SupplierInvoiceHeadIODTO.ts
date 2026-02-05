import { TermGroup_IOType, TermGroup_IOStatus, TermGroup_IOSource, TermGroup_IOImportHeadType, SoeOriginStatus, SoeEntityState } from "../../Util/CommonEnumerations";

export class SupplierInvoiceHeadIODTO  {

    supplierInvoiceHeadIOId: number;
    actorCompanyId: number;
    import: boolean;
    type: TermGroup_IOType;
    status: TermGroup_IOStatus;
    source: TermGroup_IOSource;
    importHeadType: TermGroup_IOImportHeadType;
    batchId: string;
    errorMessage: string;
    soeOriginStatus: SoeOriginStatus;
    supplierId: number;
    supplierNr: string;
    invoiceId: number;
    supplierInvoiceNr: string;
    seqNr: number;
    billingType: number;
    invoiceDate: Date;
    dueDate: Date;
    voucherDate: Date;
    referenceOur: string;
    referenceYour: string;
    ocr: string;
    currencyId: number;
    currency: string;
    currencyRate: number;
    currencyDate: Date;
    totalAmount: number;
    totalAmountCurrency: number;
    vatAmount: number;
    vatAmountCurrency: number;
    paidAmount: number;
    paidAmountCurrency: number;
    remainingAmount: number;
    fullyPayed: boolean;
    paymentNr: string;
    voucherNr: string;
    createAccountingInXE: boolean;
    note: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;

    // Extensions
    billingTypeName: string;
    statusName: string;

    // Flags
    isSelected: boolean;        
    isModified: boolean;        
   
}
