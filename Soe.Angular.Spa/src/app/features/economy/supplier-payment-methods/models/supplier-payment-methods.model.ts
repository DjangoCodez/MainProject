import {
  SoeEntityState,
  SoeOriginType,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IPaymentInformationRowDTO,
  IPaymentMethodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class PaymentMethodDTO implements IPaymentMethodDTO {
  paymentMethodId!: number;
  actorCompanyId!: number;
  accountId!: number;
  paymentInformationRowId?: number;
  sysPaymentMethodId!: number;
  paymentType: SoeOriginType = 0;
  name!: string;
  customerNr!: string;
  useInCashSales!: boolean;
  useRoundingInCashSales!: boolean;
  state: SoeEntityState = 0;
  paymentNr!: string;
  payerBankId!: string;
  sysPaymentMethodName!: string;
  sysPaymentTypeId?: number;
  paymentInformationRow!: IPaymentInformationRowDTO;
  accountNr!: string;
  transactionCode?: number;
  currencyCode!: string;
}

export class PaymentInformationRowDTO implements IPaymentInformationRowDTO {
  paymentInformationRowId!: number;
  paymentInformationId!: number;
  sysPaymentTypeId!: number;
  paymentNr!: string;
  default!: boolean;
  shownInInvoice!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;
  bic!: string;
  clearingCode!: string;
  paymentCode!: string;
  paymentMethodCode?: number;
  paymentForm?: number;
  chargeCode?: number;
  intermediaryCode?: number;
  currencyAccount!: string;
  payerBankId!: string;
  bankConnected!: boolean;
  currencyId?: number;
  sysPaymentTypeName!: string;
  paymentMethodCodeName!: string;
  paymentFormName!: string;
  chargeCodeName!: string;
  intermediaryCodeName!: string;
  currencyCode!: string;
  paymentNrDisplay!: string;
  billingType!: TermGroup_BillingType;
}
