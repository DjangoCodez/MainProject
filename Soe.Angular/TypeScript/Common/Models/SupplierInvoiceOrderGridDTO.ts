import { ISupplierInvoiceOrderGridDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_BillingType, SupplierInvoiceOrderLinkType } from "../../Util/CommonEnumerations";

export class SupplierInvoiceOrderGridDTO implements ISupplierInvoiceOrderGridDTO {
    amount: number;
    billingType: TermGroup_BillingType;
    customerInvoiceId: number;
    customerInvoiceRowId: number;
    customerInvoiceRowAttestStateId: number;
    hasImage: boolean;
    icon: string;
    includeImageOnInvoice: boolean;
    invoiceAmountExVat: number;
    invoiceDate: Date;
    invoiceNr: string;
    seqNr: number;
    supplierInvoiceId: number;
    supplierInvoiceOrderLinkType: SupplierInvoiceOrderLinkType;
    supplierName: string;
    supplierNr: string;
    timeCodeTransactionId: number;
    targetCustomerInvoiceDate: Date;
    targetCustomerInvoiceNr: string;

    //Extensions:
    customerInvoiceRowAttestStateColor: string;
    customerInvoiceRowAttestStateName: string;
}