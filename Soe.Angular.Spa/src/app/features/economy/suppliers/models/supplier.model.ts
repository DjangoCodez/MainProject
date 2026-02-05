import { ContactAddressItem } from '@shared/components/contact-addresses/contact-addresses.model';
import {
  TermGroup_InvoiceVatType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountSmallDTO,
  IAccountingSettingsRowDTO,
  IAttestWorkFlowHeadDTO,
  IPaymentInformationDTO,
  ISupplierDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class SupplierDTO implements ISupplierDTO {
  actorSupplierId!: number;
  vatType!: TermGroup_InvoiceVatType;
  paymentConditionId?: number;
  factoringSupplierId?: number;
  currencyId!: number;
  sysCountryId?: number;
  sysLanguageId?: number;
  vatCodeId?: number;
  intrastatCodeId?: number;
  supplierNr!: string;
  name!: string;
  orgNr!: string;
  vatNr!: string;
  invoiceReference!: string;
  bic!: string;
  ourCustomerNr!: string;
  copyInvoiceNrToOcr!: boolean;
  interim!: boolean;
  manualAccounting!: boolean;
  blockPayment!: boolean;
  isEDISupplier!: boolean;
  riksbanksCode!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  note!: string;
  showNote!: boolean;
  state!: SoeEntityState;
  ourReference!: string;
  sysWholeSellerId?: number;
  attestWorkFlowGroupId?: number;
  deliveryConditionId?: number;
  deliveryTypeId?: number;
  contactEcomId?: number;
  isEUCountryBased!: boolean;
  active!: boolean;
  isPrivatePerson!: boolean;
  hasConsent!: boolean;
  consentDate?: Date;
  consentModified?: Date;
  consentModifiedBy!: string;
  contactAddresses!: ContactAddressItem[];
  contactPersons!: number[];
  categoryIds!: number[];
  paymentInformationForegin!: IPaymentInformationDTO;
  paymentInformationDomestic!: IPaymentInformationDTO;
  debitAccounts!: Record<number, IAccountSmallDTO>;
  creditAccounts!: Record<number, IAccountSmallDTO>;
  vatAccounts!: Record<number, IAccountSmallDTO>;
  interimAccounts!: Record<number, IAccountSmallDTO>;
  accountingSettings!: IAccountingSettingsRowDTO[];
  templateAttestHead!: IAttestWorkFlowHeadDTO;
}
