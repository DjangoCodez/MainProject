import { IContactAddressItem } from '@shared/models/generated-interfaces/ContactAddressItem';
import {
  ICustomerInvoiceGridDTO,
  ICustomerInvoiceRowDetailDTO,
} from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  SoeEntityState,
  SoeInvoiceRowType,
  TermGroup_InvoiceVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ICustomerInvoiceRowAttestStateViewDTO,
  ICustomerProductPriceSmallDTO,
  IFileUploadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CustomerCentralDTO {
  //implements ICustomerDTO
  actorCustomerId!: number;
  vatType!: TermGroup_InvoiceVatType;
  deliveryConditionId?: number;
  deliveryTypeId?: number;
  paymentConditionId?: number;
  priceListTypeId?: number;
  currencyId!: number;
  sysCountryId?: number;
  sysLanguageId?: number;
  sysWholeSellerId?: number;
  customerNr!: string;
  name!: string;
  orgNr!: string;
  vatNr!: string;
  invoiceReference!: string;
  gracePeriodDays!: number;
  paymentMorale!: number;
  supplierNr!: string;
  offerTemplate?: number;
  orderTemplate?: number;
  billingTemplate?: number;
  agreementTemplate?: number;
  manualAccounting!: boolean;
  discountMerchandise!: number;
  discountService!: number;
  disableInvoiceFee!: boolean;
  note!: string;
  showNote!: boolean;
  finvoiceAddress!: string;
  finvoiceOperator!: string;
  isFinvoiceCustomer!: boolean;
  blockNote!: string;
  blockOrder!: boolean;
  blockInvoice!: boolean;
  creditLimit?: number;
  isCashCustomer!: boolean;
  isOneTimeCustomer!: boolean;
  invoiceDeliveryType?: number;
  importInvoicesDetailed!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  departmentNr!: string;
  payingCustomerId!: number;
  invoicePaymentService?: number;
  bankAccountNr!: string;
  addAttachementsToEInvoice!: boolean;
  contactEComId?: number;
  orderContactEComId?: number;
  reminderContactEComId?: number;
  contactGLNId?: number;
  invoiceLabel!: string;
  addSupplierInvoicesToEInvoice!: boolean;
  isEUCountryBased!: boolean;
  triangulationSales!: boolean;
  active!: boolean;
  isPrivatePerson!: boolean;
  hasConsent!: boolean;
  consentDate?: Date;
  consentModified?: Date;
  consentModifiedBy!: string;
  contactAddresses!: IContactAddressItem[];
  contactPersons!: number[];
  categoryIds!: number[];
  participants!: string;
  customerUsers!: CustomerUserDTO[];
  customerProducts?: ICustomerProductPriceSmallDTO[];
  files?: IFileUploadDTO[];
  billingAddress: string;
  deliveryAddress: string;
  phoneNumber: string;
  blockOrderString!: string;
  categoryString: string;
  invoiceDeliveryTypeString: string;

  constructor() {
    this.billingAddress = '';
    this.deliveryAddress = '';
    this.phoneNumber = '';
    this.categoryString = '';
    this.invoiceDeliveryTypeString = '';
  }
}

export class CustomerUserDTO {
  customerUserId!: number;
  actorCustomerId!: number;
  actorCompanyId!: number;
  userId!: number;
  main!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = SoeEntityState.Active;
  loginName!: string;
  name!: string;
}

export class CustomerInvoiceRowDetailDTO
  implements ICustomerInvoiceRowDetailDTO
{
  customerInvoiceRowId!: number;
  invoiceId!: number;
  attestStateId?: number;
  ediEntryId?: number;
  productId?: number;
  rowNr!: number;
  fromDate?: Date;
  toDate?: Date;
  type!: SoeInvoiceRowType;
  ediTextValue!: string;
  productNr!: string;
  productName!: string;
  text!: string;
  quantity?: number;
  previouslyInvoicedQuantity?: number;
  productUnitCode!: string;
  amountCurrency!: number;
  discountValue!: number;
  currencyCode!: string;
  sumAmountCurrency!: number;
  marginalIncomeLimit!: number;
  discountType!: number;
  attestStateName!: string;
  attestStateColor!: string;
  isTimeProjectRow!: boolean;
  isExpenseRow!: boolean;
  isTimeBillingRow!: boolean;
  discountTypeText!: string;
  rowTypeIcon!: string;
}

export class CustomerInvoiceGridDTO implements ICustomerInvoiceGridDTO {
  customerInvoiceId!: number;
  originType!: number;
  deliveryType!: number;
  deliveryTypeName!: string;
  invoiceDeliveryProvider!: number;
  invoiceDeliveryProviderName!: string;
  seqNr!: number;
  invoiceNr!: string;
  ocr!: string;
  customerPaymentId!: number;
  customerPaymentRowId!: number;
  paymentSeqNr!: number;
  paymentNr!: string;
  billingTypeId!: number;
  billingTypeName!: string;
  status!: number;
  statusName!: string;
  statusIcon!: number;
  exportStatus!: number;
  exportStatusName!: string;
  actorCustomerId!: number;
  actorCustomerNr!: string;
  actorCustomerName!: string;
  actorCustomerNrName!: string;
  internalText!: string;
  workDescription!: string;
  invoiceLabel!: string;
  invoicePaymentServiceId!: number;
  invoicePaymentServiceName!: string;
  totalAmount!: number;
  totalAmountText!: string;
  totalAmountCurrency!: number;
  totalAmountCurrencyText!: string;
  totalAmountExVat!: number;
  totalAmountExVatText!: string;
  totalAmountExVatCurrency!: number;
  totalAmountExVatCurrencyText!: string;
  vatAmount!: number;
  vatAmountCurrency!: number;
  payAmount!: number;
  payAmountText!: string;
  payAmountCurrency!: number;
  payAmountCurrencyText!: string;
  paidAmount!: number;
  paidAmountText!: string;
  paidAmountCurrency!: number;
  paidAmountCurrencyText!: string;
  remainingAmount!: number;
  remainingAmountText!: string;
  remainingAmountExVat!: number;
  remainingAmountExVatText!: string;
  paymentAmount!: number;
  paymentAmountCurrency!: number;
  paymentAmountDiff!: number;
  contractYearlyValue!: number;
  contractYearlyValueExVat!: number;
  bankFee!: number;
  vatRate!: number;
  sysCurrencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  invoiceDate?: Date;
  dueDate?: Date;
  payDate?: Date;
  deliveryDate?: Date;
  ownerActorId!: number;
  fullyPaid!: boolean;
  isTotalAmountPaid!: boolean;
  invoiceHeadText!: string;
  registrationType!: number;
  deliveryAddressId!: number;
  deliveryAddress!: string;
  deliveryCity!: string;
  deliveryPostalCode!: string;
  billingAddressId!: number;
  billingAddress!: string;
  contactEComId?: number;
  contactEComText!: string;
  reminderContactEComId?: number;
  reminderContactEComText!: string;
  billingInvoicePrinted!: boolean;
  hasHouseholdTaxDeduction!: boolean;
  householdTaxDeductionType!: number;
  hasVoucher!: boolean;
  insecureDebt!: boolean;
  multipleAssetRows!: boolean;
  noOfReminders!: number;
  noOfPrintedReminders!: number;
  lastCreatedReminder?: Date;
  categories!: string;
  customerCategories!: string;
  deliverDateText!: string;
  orderNumbers!: string;
  customerGracePeriodDays!: number;
  users!: string;
  projectNr!: string;
  attestStateNames!: string;
  shiftTypeName!: string;
  shiftTypeColor!: string;
  fixedPriceOrderName!: string;
  fixedPriceOrder!: boolean;
  orderType!: number;
  orderTypeName!: string;
  attestStates!: ICustomerInvoiceRowAttestStateViewDTO[];
  onlyPayment!: boolean;
  nextContractPeriod!: string;
  contractGroupName!: string;
  nextInvoiceDate?: Date;
  defaultDim2AccountId?: number;
  defaultDim3AccountId?: number;
  defaultDim4AccountId?: number;
  defaultDim5AccountId?: number;
  defaultDim6AccountId?: number;
  defaultDim2AccountName!: string;
  defaultDim3AccountName!: string;
  defaultDim4AccountName!: string;
  defaultDim5AccountName!: string;
  defaultDim6AccountName!: string;
  defaultDimAccountNames!: string;
  referenceOur!: string;
  referenceYour!: string;
  mainUserName!: string;
  priceListName!: string;
  projectName!: string;
  myReadyState!: number;
  orderReadyStatePercent!: number;
  orderReadyStateText!: string;
  externalInvoiceNr!: string;
  einvoiceDistStatus!: number;
  myOriginUserStatus!: number;
  useClosedStyle!: boolean;
  isSelectDisabled!: boolean;
  isOverdued!: boolean;
  hasInterest!: boolean;
  isCashSales!: boolean;
  isCashSalesText!: string;
  guid!: string;
  infoIcon!: number;
  created?: Date;
  mappedContractNr!: string;

  // Extensions
  expandableDataIsLoaded: boolean = false;
  statusIconValue: string;
  statusIconMessage: string;
  attestStateColor: string;
  useGradient: boolean;
  billingIconValue: string;
  billingIconMessage: string;
  showCreatePayment: boolean;
  myReadyStateIconText: string;
  myReadyStateIcon: string;
  orderReadyStateIcon: string;
  paidInfo: string;
  paidStatusColor: string;

  constructor() {
    this.expandableDataIsLoaded = false;
    this.statusIconValue = '';
    this.statusIconMessage = '';
    this.attestStateColor = '';
    this.useGradient = false;
    this.billingIconValue = '';
    this.billingIconMessage = '';
    this.showCreatePayment = false;
    this.myReadyStateIconText = '';
    this.myReadyStateIcon = '';
    this.orderReadyStateIcon = '';
    this.paidInfo = '';
    this.paidStatusColor = '';
  }
}

export class CustomerCentralOrderDTO {
  orderSelectedTotal!: number;
  orderFilteredTotal!: number;
  orderSelectedToBeInvoicedTotal!: number;
  orderFilteredToBeInvoicedTotal!: number;
}

export class CustomerCentralInvoiceDTO {
  invoiceSelectedTotal!: number;
  invoiceFilteredTotal!: number;
  invoiceFilteredToPay!: number;
  invoiceSelectedToPay!: number;
}

export class CustomerCentralOfferDTO {
  offerSelectedTotal!: number;
  offerFilteredTotal!: number;
}
