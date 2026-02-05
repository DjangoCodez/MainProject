import { ISupplierInvoiceIncomingHallGridDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { SoeOriginStatus, TermGroup_SupplierInvoiceType } from "@shared/models/generated-interfaces/Enumerations";

export class SupplierInvoicesArrivalHallDTO
  implements ISupplierInvoiceIncomingHallGridDTO
{
  idField!: string;
  actorCompanyId!: number;
  invoiceId!: number;
  invoiceSource!: number;
  invoiceSourceName!: string;
  billingTypeId!: number;
  invoiceNr: string = '';
  supplierId: number = 0;
  supplierNr: string = '';
  supplierName: string = '';
  internalText: string = '';
  totalAmount: number = 0;
  totalAmountCurrency: number = 0;
  vatAmount: number = 0;
  vatAmountCurrency: number = 0;
  totalAmountExcludingVat: number = 0;
  totalAmountCurrencyExcludingVat: number = 0;
  invoiceDate: Date | undefined;
  dueDate: Date | undefined;
  invoiceState!: number;
  invoiceStateName!: string;
  created!: Date;
  ediEntryId: number = 0;
  hasPDF: boolean = false;
  ediType: number = 0;
  isOverdue!: boolean;
  isAboutToDue!: boolean;
  scanningEntryId!: number;
  supplierInvoiceHeadIOId?: number;
  blockPayment!: boolean;
  underInvestigation!: boolean;
  icon!: string;
  sysCurrencyId?: number;
  currencyCode: string = '';
  attestGroupId?: number;
  attestGroupName: string = '';
  originStatus: SoeOriginStatus = SoeOriginStatus.None;
  supplierInvoiceType: TermGroup_SupplierInvoiceType = TermGroup_SupplierInvoiceType.None;
}
