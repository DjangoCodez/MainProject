import { IAccountingRowDTO } from '@shared/models/generated-interfaces/AccountingRowDTO';
import {
  ISupplierInvoiceHistoryGridDTO,
  ISupplierInvoiceHistoryDetailsDTO,
} from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';

export class SupplierInvoiceHistoryGridDTO
  implements ISupplierInvoiceHistoryGridDTO
{
  invoiceId!: number;
  seqNr!: number;
  invoiceNr!: string;
  invoiceDate!: Date;
  dueDate!: Date;
  paymentDate?: Date | undefined;
  approvalGroup: string;
  totalAmount!: number;
  totalAmountCurrency!: number;
  totalAmountExcludingVAT!: number;
  totalAmountCurrencyExcludingVAT!: number;
  vatAmount!: number;
  vatAmountCurrency!: number;

  constructor(
    invoiceId: number,
    seqNr: number,
    invoiceNr: string,
    invoiceDate: Date,
    dueDate: Date,
    paymentDate: Date,
    approvalGroup: string,
    totalAmount: number,
    totalAmountCurrency: number,
    vatAmount: number,
    vatAmountCurrency: number,
    totalAmountExcludingVat: number,
    totalAmountCurrencyExcludingVat: number
  ) {
    this.invoiceId = invoiceId;
    this.seqNr = seqNr;
    this.invoiceNr = invoiceNr;
    this.invoiceDate = invoiceDate;
    this.dueDate = dueDate;
    this.paymentDate = paymentDate;
    this.approvalGroup = approvalGroup;
    this.totalAmount = totalAmount;
    this.totalAmountCurrency = totalAmountCurrency;
    this.vatAmount = vatAmount;
    this.vatAmountCurrency = vatAmountCurrency;
    this.totalAmountExcludingVAT = totalAmountExcludingVat;
    this.totalAmountCurrencyExcludingVAT = totalAmountCurrencyExcludingVat;
  }
}

export class SupplierInvoiceHistoryDetailsDTO
  implements ISupplierInvoiceHistoryDetailsDTO
{
  invoiceId!: number;
  invoiceNr!: string;
  paymentNr: string;
  invoiceDate!: Date;
  invoiceType: string;
  dueDate!: Date;
  paymentDate?: Date | undefined;
  supplierReference: string;
  ourReference: string;
  vatType: string;
  vatCode: string;
  totalAmount!: number;
  totalAmountCurrency!: number;
  totalAmountExcludingVAT!: number;
  totalAmountCurrencyExcludingVAT!: number;
  vatAmount!: number;
  vatAmountCurrency!: number;
  accounting: IAccountingRowDTO[];

  constructor(
    invoiceId: number,
    invoiceNr: string,
    paymentNr: string,
    invoiceDate: Date,
    invoiceType: string,
    dueDate: Date,
    paymentDate: Date,
    supplierReference: string,
    ourReference: string,
    vatType: string,
    vatCode: string,
    totalAmount: number,
    totalAmountCurrency: number,
    vatAmount: number,
    vatAmountCurrency: number,
    totalAmountExcludingVat: number,
    totalAmountCurrencyExcludingVat: number,
    accounting: IAccountingRowDTO[]
  ) {
    this.invoiceId = invoiceId;
    this.invoiceNr = invoiceNr;
    this.paymentNr = paymentNr;
    this.invoiceDate = invoiceDate;
    this.invoiceType = invoiceType;
    this.dueDate = dueDate;
    this.paymentDate = paymentDate;
    this.supplierReference = supplierReference;
    this.ourReference = ourReference;
    this.vatType = vatType;
    this.vatCode = vatCode;
    this.totalAmount = totalAmount;
    this.totalAmountCurrency = totalAmountCurrency;
    this.vatAmount = vatAmount;
    this.vatAmountCurrency = vatAmountCurrency;
    this.totalAmountExcludingVAT = totalAmountExcludingVat;
    this.totalAmountCurrencyExcludingVAT = totalAmountCurrencyExcludingVat;
    this.accounting = accounting;
  }
}
