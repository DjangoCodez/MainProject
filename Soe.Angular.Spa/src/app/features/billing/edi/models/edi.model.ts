import {
  TermGroup_EDISourceType,
  TermGroup_EDIStatus,
  TermGroup_EdiMessageType,
  TermGroup_ScanningMessageType,
  TermGroup_BillingType,
  SoeEntityState,
  EdiImportSource,
  TermGroup_ScanningStatus,
  TermGroup_EDIOrderStatus,
  TermGroup_EDIInvoiceStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEdiEntryViewDTO,
  IUpdateEdiEntryDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class EdiEntryViewDTO implements IEdiEntryViewDTO {
  nrOfPages: number;
  nrOfInvoices: number;
  ediEntryId!: number;
  actorCompanyId!: number;
  type!: TermGroup_EDISourceType;
  status!: TermGroup_EDIStatus;
  statusName!: string;
  ediMessageType!: TermGroup_EdiMessageType;
  ediMessageTypeName!: string;
  scanningMessageType!: TermGroup_ScanningMessageType;
  scanningMessageTypeName!: string;
  sourceTypeName!: string;
  billingType!: TermGroup_BillingType;
  billingTypeName!: string;
  wholesellerId!: number;
  wholesellerName!: string;
  buyerId!: string;
  buyerReference!: string;
  hasPdf!: boolean;
  errorCode!: number;
  created?: Date;
  state!: SoeEntityState;
  errorMessage!: string;
  importSource!: EdiImportSource;
  scanningEntryId?: number;
  operatorMessage!: string;
  scanningStatus!: TermGroup_ScanningStatus;
  date?: Date;
  invoiceDate?: Date;
  dueDate?: Date;
  sum!: number;
  sumCurrency!: number;
  sumVat!: number;
  sumVatCurrency!: number;
  sysCurrencyId!: number;
  currencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  orderId?: number;
  orderStatus!: TermGroup_EDIOrderStatus;
  orderStatusName!: string;
  orderNr!: string;
  sellerOrderNr!: string;
  invoiceId?: number;
  invoiceStatus!: TermGroup_EDIInvoiceStatus;
  invoiceStatusName!: string;
  invoiceNr!: string;
  seqNr?: number;
  customerId?: number;
  customerNr!: string;
  customerName!: string;
  supplierId?: number;
  supplierNr!: string;
  supplierName!: string;
  langId!: number;
  supplierAttestGroupId?: number;
  supplierAttestGroupName!: string;
  roundedInterpretation!: number;
  isVisible!: boolean;
  isModified!: boolean;
  isSelectDisabled!: boolean;
  isSelected!: boolean;
  //Extensions
  supplierNrName: string;
  hasInvalidSupplier: boolean;

  constructor() {
    this.supplierNrName = '';
    this.hasInvalidSupplier = true;
    this.nrOfPages = 0;
    this.nrOfInvoices = 0;
  }
}

export class UpdateEdiEntryDTO implements IUpdateEdiEntryDTO {
  ediEntryId!: number;
  supplierId?: number;
  attestGroupId?: number;
  scanningEntryId?: number;
  orderNr!: string;
}
