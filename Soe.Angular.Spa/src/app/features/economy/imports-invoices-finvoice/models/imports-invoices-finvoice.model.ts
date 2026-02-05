import {
  IFInvoiceModel,
  ITransferEdiStateModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import {
  EdiImportSource,
  SoeEntityState,
  SoeEntityType,
  TermGroup_BillingType,
  TermGroup_EDIInvoiceStatus,
  TermGroup_EDIOrderStatus,
  TermGroup_EDISourceType,
  TermGroup_EDIStatus,
  TermGroup_EdiMessageType,
  TermGroup_ScanningMessageType,
  TermGroup_ScanningStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEdiEntryViewDTO,
  IUpdateEdiEntryDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class EdiEntryViewDTO implements IEdiEntryViewDTO {
  nrOfPages!: number;
  nrOfInvoices!: number;
  errorCode!: number;
  ediEntryId!: number;
  actorCompanyId!: number;
  type: TermGroup_EDISourceType = TermGroup_EDISourceType.Finvoice;
  status: TermGroup_EDIStatus = TermGroup_EDIStatus.Unprocessed;
  statusName!: string;
  ediMessageType: TermGroup_EdiMessageType = TermGroup_EdiMessageType.Unknown;
  ediMessageTypeName!: string;
  scanningMessageType: TermGroup_ScanningMessageType =
    TermGroup_ScanningMessageType.Unknown;
  scanningMessageTypeName!: string;
  sourceTypeName!: string;
  billingType: TermGroup_BillingType = TermGroup_BillingType.None;
  billingTypeName!: string;
  wholesellerId!: number;
  wholesellerName!: string;
  buyerId!: string;
  buyerReference!: string;
  hasPdf!: boolean;
  errorCoe!: number;
  created!: Date;
  state: SoeEntityState = SoeEntityState.Inactive;
  errorMessage!: string;
  importSource: EdiImportSource = EdiImportSource.Undefined;
  scanningEntryId?: number;
  operatorMessage!: string;
  scanningStatus: TermGroup_ScanningStatus =
    TermGroup_ScanningStatus.Unprocessed;
  date!: Date;
  invoiceDate!: Date;
  dueDate!: Date;
  sum!: number;
  sumCurrency!: number;
  sumVat!: number;
  sumVatCurrency!: number;
  sysCurrencyId!: number;
  currencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  orderId!: number;
  orderStatus: TermGroup_EDIOrderStatus = TermGroup_EDIOrderStatus.Unprocessed;
  orderStatusName!: string;
  orderNr!: string;
  sellerOrderNr!: string;
  invoiceId!: number;
  invoiceStatus: TermGroup_EDIInvoiceStatus =
    TermGroup_EDIInvoiceStatus.Unprocessed;
  invoiceStatusName!: string;
  invoiceNr!: string;
  seqNr?: number;
  customerId?: number;
  customerNr!: string;
  customerName!: string;
  supplierId?: number;
  supplierNr!: string;
  supplierName!: string;
  supplierNrName: string = '';
  customerInvoiceNumberName!: string;
  langId!: number;
  supplierAttestGroupId?: number;
  supplierAttestGroupName!: string;
  roundedInterpretation!: number;
  isVisible!: boolean;
  isModified!: boolean;
  isSelectDisabled!: boolean;
  isSelected!: boolean;
  hasInvalidSupplier?: boolean;
  editIcon?: string;
}
export class FinvoiceGridFilterDTO {
  constructor() {
    this.allItemsSelection = 1;
    this.showOnlyUnHandled = false;
  }
  allItemsSelection!: number;
  showOnlyUnHandled!: boolean;
}
export class FInvoiceModel implements IFInvoiceModel {
  fileName!: string;
  fileString!: string;
  extention!: string;
  entity!: SoeEntityType;
}
export class UpdateEdiEntryDTO implements IUpdateEdiEntryDTO {
  ediEntryId!: number;
  supplierId?: number;
  attestGroupId?: number;
  scanningEntryId?: number;
  orderNr!: string;
}
export class TransferEdiStateModel implements ITransferEdiStateModel {
  idsToTransfer!: number[];

  stateTo!: number;
}
