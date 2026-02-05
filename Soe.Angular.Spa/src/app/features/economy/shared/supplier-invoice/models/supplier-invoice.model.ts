import {
  SoeInvoiceType,
  TermGroup_BillingType,
  TermGroup_InvoiceVatType,
  SoeEntityState,
  SoeStatusIcon,
  SoeOriginStatus,
  TermGroup_VatDeductionType,
  TermGroup_SupplierInvoiceSource,
  TermGroup_SupplierInvoiceStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { IGenericImageDTO } from '@shared/models/generated-interfaces/GenericImageDTO';
import {
  IFileUploadDTO,
  IInvoiceDTO,
  IOriginUserDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISupplierInvoiceCostAllocationDTO } from '@shared/models/generated-interfaces/SupplierInvoiceCostAllocationDTO';
import {
  ISupplierInvoiceDTO,
  ISupplierInvoiceRowDTO,
} from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';
import { ISupplierInvoiceOrderRowDTO } from '@shared/models/generated-interfaces/SupplierInvoiceOrderRowDTO';
import { ISupplierInvoiceProjectRowDTO } from '@shared/models/generated-interfaces/SupplierInvoiceProjectRowDTO';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SupplierInvoiceDTO implements ISupplierInvoiceDTO, IInvoiceDTO {
  //Misc
  source: TermGroup_SupplierInvoiceSource =
    TermGroup_SupplierInvoiceSource.Unknown;
  status: TermGroup_SupplierInvoiceStatus = TermGroup_SupplierInvoiceStatus.New;

  //Ids
  invoiceId!: number;
  prevInvoiceId!: number;
  ediEntryId!: number;
  scanningEntryId!: number;
  actorId?: number;
  contactEComId?: number;
  voucheHeadId?: number;
  voucheHead2Id?: number;
  sysPaymentTypeId?: number;
  vatCodeId?: number;
  deliveryCustomerId?: number;
  remainingAmount?: number;
  remainingAmountExVat?: number;
  voucherSeriesId!: number;
  voucherSeriesTypeId!: number;

  //Main fields
  type: SoeInvoiceType = SoeInvoiceType.SupplierInvoice;
  billingType: TermGroup_BillingType = TermGroup_BillingType.Debit;
  vatType: TermGroup_InvoiceVatType = TermGroup_InvoiceVatType.Merchandise;
  invoiceNr: string = '';
  seqNr?: number;
  ocr: string = '';
  invoiceDate?: Date;
  dueDate?: Date;
  voucherDate?: Date;
  referenceOur: string = '';
  referenceYour: string = '';

  //Payment fields
  fullyPayed: boolean = false;
  onlyPayment: boolean = false;
  paymentNr: string = '';

  //Amounts
  totalAmount = 0;
  totalAmountCurrency = 0;
  totalAmountEntCurrency = 0;
  totalAmountLedgerCurrency = 0;
  vatAmount = 0;
  vatAmountCurrency = 0;
  vatAmountEntCurrency = 0;
  vatAmountLedgerCurrency = 0;
  paidAmount = 0;
  paidAmountCurrency = 0;
  paidAmountEntCurrency = 0;
  paidAmountLedgerCurrency = 0;

  //Currency fields
  currencyId!: number;
  currencyRate: number = 1;
  currencyDate!: Date;

  //Attest
  paymentMethodId?: number;
  attestStateId?: number;
  attestGroupId?: number;
  attestStateName: string = '';

  //Time discount
  timeDiscountDate?: Date;
  timeDiscountPercent?: number;

  //Origin
  originStatus!: SoeOriginStatus;
  originStatusName!: string;
  originDescription!: string;
  originUsers!: IOriginUserDTO[];

  //Accounting fields
  defaultDim1AccountId?: number;
  defaultDim2AccountId?: number;
  defaultDim3AccountId?: number;
  defaultDim4AccountId?: number;
  defaultDim5AccountId?: number;
  defaultDim6AccountId?: number;
  claimAccountId!: number;
  interimInvoice: boolean = false;
  multipleDebtRows: boolean = false;
  manuallyAdjustedAccounting: boolean = false;

  //VAT Fields
  vatDeductionType!: TermGroup_VatDeductionType;
  vatDeductionAccountId?: number;
  vatDeductionPercent: number = 0;

  //Default fields
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = SoeEntityState.Active;

  //Cost allocation, products & sales related
  projectId?: number;
  projectNr!: string;
  projectName!: string;

  orderNr?: number;
  orderCustomerInvoiceId?: number;
  orderCustomerName!: string;
  orderProjectId?: number;

  hasOrderRows = false;
  hasProjectRows = false;

  supplierInvoiceRows: ISupplierInvoiceRowDTO[] = [];
  supplierInvoiceProjectRows: ISupplierInvoiceProjectRowDTO[] = [];
  supplierInvoiceOrderRows: ISupplierInvoiceOrderRowDTO[] = [];
  supplierInvoiceCostAllocationRows: ISupplierInvoiceCostAllocationDTO[] = [];

  //Images
  hasImage = false;
  image!: IGenericImageDTO;
  scanningImage!: IGenericImageDTO;
  supplierInvoiceFiles: IFileUploadDTO[] = [];

  //Block payment
  blockPayment = false;
  blockReasonTextId?: number;
  blockReason: string = '';

  //Irrelevant fields
  isTemplate: boolean = false;
  statusIcon = SoeStatusIcon.None;
  contactGLNId?: number;
}

export type InvoiceImageFile = {
  dataStorageRecordId?: number;
  data: string;
  fileName: string;
  extension: string;
};

export class SupplierInvoiceCostAllocationDTO
  implements ISupplierInvoiceCostAllocationDTO
{
  customerInvoiceRowId!: number;
  timeCodeTransactionId!: number;
  supplierInvoiceId!: number;
  projectId!: number;
  orderId!: number;
  attestStateId!: number;
  createdDate!: Date;
  timeInvoiceTransactionId?: number;
  projectAmount!: number;
  projectAmountCurrency!: number;
  rowAmount!: number;
  rowAmountCurrency!: number;
  orderAmount!: number;
  orderAmountCurrency!: number;
  supplementCharge?: number;
  chargeCostToProject!: boolean;
  includeSupplierInvoiceImage!: boolean;
  isReadOnly!: boolean;
  productId?: number;
  productNr!: string;
  productName!: string;
  timeCodeId!: number;
  timeCodeCode!: string;
  timeCodeName!: string;
  timeCodeDescription!: string;
  employeeId?: number;
  employeeNr!: string;
  employeeName!: string;
  employeeDescription!: string;
  projectNr!: string;
  projectName!: string;
  orderNr!: string;
  customerInvoiceNumberName!: string;
  attestStateName!: string;
  attestStateColor!: string;
  state!: SoeEntityState;
  isTransferToOrderRow!: boolean;
  isConnectToProjectRow!: boolean;

  //extention fields
  orderNrName!: string;
  projectNrName!: string;
  employeeNrName!: string;
  remainingAllocationAmount: number = 0;

  constructor(supplierInvoiceId: number = 0) {
    this.supplierInvoiceId = supplierInvoiceId;
    this.createdDate = new Date();
    this.rowAmount = 0.0;
    this.rowAmountCurrency = 0.0;
    this.orderAmount = 0.0;
    this.orderAmountCurrency = 0.0;
    this.rowAmount = 0.0;
    this.rowAmountCurrency = 0.0;
    this.projectAmount = 0.0;
    this.projectAmountCurrency = 0.0;
  }
}
