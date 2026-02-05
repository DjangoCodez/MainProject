import {
  OrderInvoiceRegistrationType,
  SoeEntityState,
  SoeOriginStatus,
  SoeOriginType,
  SoePaymentStatus,
  TermGroup_BillingType,
  TermGroup_InventoryStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IEDITraceViewBase,
  IInvoiceTraceViewDTO,
  IOrderTraceViewDTO,
  IVoucherTraceViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class VoucherTraceViewDTO implements IVoucherTraceViewDTO {
  langId!: number;
  voucherHeadId!: number;
  isInvoice!: boolean;
  invoiceId!: number;
  isPayment!: boolean;
  paymentRowId!: number;
  paymentStatus!: SoePaymentStatus;
  paymentStatusName!: string;
  isInventory!: boolean;
  inventoryId!: number;
  inventoryName!: string;
  inventoryDescription!: string;
  inventoryTypeName!: string;
  inventoryStatusId!: TermGroup_InventoryStatus;
  inventoryStatusName!: string;
  isAccountDistribution!: boolean;
  accountDistributionHeadId!: number;
  accountDistributionName!: string;
  originType!: SoeOriginType;
  originTypeName!: string;
  originStatus!: SoeOriginStatus;
  originStatusName!: string;
  description!: string;
  number!: string;
  sysCurrencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  amount!: number;
  amountCurrency!: number;
  vatAmount!: number;
  vatAmountCurrency!: number;
  date?: Date;
  state!: SoeEntityState;
  registrationType!: OrderInvoiceRegistrationType;
  foreign!: boolean;

  showPdfIcon!: boolean;
}

export class InvoiceTraceViewDTO
  implements IInvoiceTraceViewDTO, IEDITraceViewBase
{
  langId!: number;
  isInvoice!: boolean;
  invoiceId!: number;
  isContract!: boolean;
  contractId!: number;
  isOffer!: boolean;
  offerId!: number;
  isOrder!: boolean;
  orderId!: number;
  mappedInvoiceId!: number;
  isReminderInvoice!: boolean;
  reminderInvoiceId!: number;
  isInterestInvoice!: boolean;
  interestInvoiceId!: number;
  isPayment!: boolean;
  paymentRowId!: number;
  paymentStatusId!: SoePaymentStatus;
  paymentStatusName!: string;
  isVoucher!: boolean;
  isStockVoucher!: boolean;
  voucherHeadId!: number;
  isInventory!: boolean;
  inventoryId!: number;
  inventoryName!: string;
  inventoryDescription!: string;
  inventoryTypeName!: string;
  inventoryStatusId!: TermGroup_InventoryStatus;
  inventoryStatusName!: string;
  isAccountDistribution!: boolean;
  accountDistributionHeadId!: number;
  accountDistributionName!: string;
  triggerType!: number;
  triggerTypeName!: string;
  isProject!: boolean;
  projectId!: number;
  originType!: SoeOriginType;
  originTypeName!: string;
  originStatus!: SoeOriginStatus;
  originStatusName!: string;
  description!: string;
  billingType!: TermGroup_BillingType;
  billingTypeName!: string;
  number!: string;
  sysCurrencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  amount!: number;
  amountCurrency!: number;
  vatAmount!: number;
  vatAmountCurrency!: number;
  date?: Date;
  isScanning!: boolean;
  state!: SoeEntityState;
  foreign!: boolean;

  isEdi!: boolean;
  ediEntryId!: number;
  ediHasPdf!: boolean;

  showPdfIcon!: boolean;
}

export class OrderTraceViewDTO
  implements IOrderTraceViewDTO, IEDITraceViewBase
{
  langId!: number;
  orderId!: number;
  isContract!: boolean;
  contractId!: number;
  isOffer!: boolean;
  offerId!: number;
  isInvoice!: boolean;
  invoiceId!: number;
  isProject!: boolean;
  projectId!: number;
  isSupplierInvoice!: boolean;
  supplierInvoiceId!: number;
  isPurchase!: boolean;
  purchaseId!: number;
  originType!: SoeOriginType;
  originTypeName!: string;
  originStatus!: SoeOriginStatus;
  originStatusName!: string;
  description!: string;
  billingType!: TermGroup_BillingType;
  billingTypeName!: string;
  number!: string;
  sysCurrencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  amount!: number;
  amountCurrency!: number;
  vatAmount!: number;
  vatAmountCurrency!: number;
  date?: Date;
  state!: SoeEntityState;
  foreign!: boolean;
  isStockVoucher!: boolean;
  voucherHeadId!: number;

  isEdi!: boolean;
  ediEntryId!: number;
  ediHasPdf!: boolean;

  showPdfIcon!: boolean;
}

export enum TraceRowPageName {
  Voucher = 1,
  SupplierInvoice = 2,
  CustomerInvoice = 3,
  Order = 4,
  Payment = 5,
  Project = 6,
  AccountDistribution = 7,
  Offer = 8,
  Contract = 9,
  Purchase = 10,
  PriceOptimization = 11,
}
