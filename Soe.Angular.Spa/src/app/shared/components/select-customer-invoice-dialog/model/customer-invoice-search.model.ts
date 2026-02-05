import { PaymentImportIODTO } from '@features/economy/import-payments/models/import-payments.model';
import { ISearchCustomerInvoiceModel } from '@shared/models/generated-interfaces/CoreModels';
import { ICustomerInvoiceSearchDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class CustomerInvoiceSearchDTO implements DialogData {
  title!: string;
  content?: string | undefined;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  callbackAction?: (() => unknown) | undefined;
  size?: DialogSize | undefined;
  invoiceValue: SearchCustomerInvoiceDTO | undefined;
  originType!: number;
  isNew: boolean = false;
  ignoreChildren: boolean = false;
  customerId?: number;
  invoiceId?: number;
  projectId?: number;
  currentMainInvoiceId?: number;
  selectedProjectName?: string;
  userId?: number;
  includePreliminary?: boolean | undefined;
  includeVoucher?: boolean | undefined;
  fullyPaid?: boolean | undefined;
  useExternalInvoiceNr?: boolean;
  importRow?: PaymentImportIODTO;
}

export class SearchCustomerInvoiceDTO implements ISearchCustomerInvoiceModel {
  number: string;
  externalNr: string;
  customerNr: string;
  customerName: string;
  internalText: string;
  projectNr: string;
  projectName: string;
  originType: number;
  customerId?: number;
  currentMainInvoiceId?: number;
  projectId?: number;
  userId?: number;
  ignoreInvoiceId?: number;
  ignoreChildren: boolean;
  includePreliminary?: boolean | undefined;
  includeVoucher?: boolean | undefined;
  fullyPaid?: boolean | undefined;
  isNew: boolean | undefined;
  importRow?: PaymentImportIODTO;

  constructor() {
    this.number = '';
    this.externalNr = '';
    this.customerNr = '';
    this.customerName = '';
    this.internalText = '';
    this.projectNr = '';
    this.projectName = '';
    this.originType = 0;
    this.ignoreChildren = false;
    this.fullyPaid = false;
    this.includePreliminary = false;
    this.includeVoucher = false;
    this.projectId = 0;
  }
}

export class SelectInvoiceDialogDTO implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  originType!: number;
  invoiceValue!: ISearchCustomerInvoiceModel;
}

export class InvoiceGridFormDTO {
  invoiceNumber?: string = '';
}

export interface ICustomerInvoiceSearchResultDTO
  extends ICustomerInvoiceSearchDTO {
  balance: number;
}
