import { ContactAddressItem } from '@shared/components/contact-addresses/contact-addresses.model';
import { ContactPersonDTO } from '@shared/components/contact-persons/models/contact-persons.model';
import { IContactAddressItem } from '@shared/models/generated-interfaces/ContactAddressItem';
import { ISaveCustomerModel } from '@shared/models/generated-interfaces/CoreModels';
import {
  ICustomerDTO,
  ICustomerGridDTO,
} from '@shared/models/generated-interfaces/CustomerDTO';
import {
  SoeEntityState,
  TermGroup_InvoiceVatType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import {
  IAccountSmallDTO,
  IAccountingSettingsRowDTO,
  ICustomerUserDTO,
  ICustomerProductPriceSmallDTO,
  IFileUploadDTO,
  IHouseholdTaxDeductionApplicantDTO,
  IOriginUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CustomerGridDTO implements ICustomerGridDTO {
  actorCustomerId!: number;
  customerNr!: string;
  name!: string;
  orgNr!: string;
  categories!: string;
  invoiceDeliveryType!: number;
  invoiceReference!: string;
  invoicePaymentService!: number;
  state: SoeEntityState = SoeEntityState.Active;
  contactAddresses!: IContactAddressItem[];
  gridAddressText!: string;
  gridPhoneText!: string;
  gridPaymentServiceText!: string;
  gridBillingAddressText!: string;
  gridDeliveryAddressText!: string;
  gridHomePhoneText!: string;
  gridMobilePhoneText!: string;
  gridWorkPhoneText!: string;
  gridEmailText!: string;
  invoiceDeliveryTypeText!: string;
  isActive?: boolean | undefined;
  isPrivatePerson?: boolean | undefined;
}

export class CustomerDTO implements ICustomerDTO {
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
  discount2Merchandise!: number;
  discountService!: number;
  discount2Service!: number;
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
  invoiceDeliveryProvider?: number;
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
  contactAddresses!: ContactAddressItem[];
  contactPersons!: number[];
  categoryIds!: number[];
  debitAccounts!: Record<number, IAccountSmallDTO>;
  creditAccounts!: Record<number, IAccountSmallDTO>;
  vatAccounts!: Record<number, IAccountSmallDTO>;
  accountingSettings!: IAccountingSettingsRowDTO[];
  customerUsers!: ICustomerUserDTO[];
  customerProducts!: ICustomerProductPriceSmallDTO[];
  files!: IFileUploadDTO[];
  participants!: string;
  contractNr!: string;
}

export class SaveCustomerModel implements ISaveCustomerModel {
  customer!: ICustomerDTO;
  contactPersons: ContactPersonDTO[];
  houseHoldTaxApplicants: IHouseholdTaxDeductionApplicantDTO[];
  extraFields: IExtraFieldRecordDTO[];

  constructor() {
    this.contactPersons = [];
    this.houseHoldTaxApplicants = [];
    this.extraFields = [];
  }
}

export class CustomerUserDTO implements ICustomerUserDTO {
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

export class OriginUserSmallDTO implements IOriginUserSmallDTO {
  originUserId!: number;
  userId!: number;
  main!: boolean;
  name!: string;
  isReady!: boolean;
}
