import {
  TermGroup_SysPaymentService,
  SoeEntityState,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IInvoiceExportDTO,
  IInvoiceExportIODTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class InvoiceExportDTO implements IInvoiceExportDTO {
  invoiceExportId!: number;
  actorCompanyId?: number;
  batchId!: number;
  sysPaymentServiceId!: TermGroup_SysPaymentService;
  totalAmount: number = 0;
  numberOfInvoices: number = 0;
  exportDate!: Date;
  filename!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = 0;
  serviceName!: string;
}

export class InvoiceExportIODTO implements IInvoiceExportIODTO {
  invoiceExportIOId!: number;
  invoiceExportId!: number;
  batchId!: number;
  customerId?: number;
  customerName!: string;
  invoiceType!: TermGroup_BillingType;
  invoiceId?: number;
  invoiceNr!: string;
  invoiceSeqnr!: string;
  invoiceAmount?: number;
  currency!: string;
  invoiceDate?: Date;
  dueDate?: Date;
  bankAccount!: string;
  payerId!: string;
  state: SoeEntityState = 0;
  isSelected!: boolean;
  isVisible!: boolean;
  typeName!: string;
  stateName!: string;
  invoiceTypeName!: string;
}
