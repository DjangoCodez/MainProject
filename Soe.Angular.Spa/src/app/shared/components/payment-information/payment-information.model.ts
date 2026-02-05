import {
  SoeEntityState,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IPaymentInformationRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class PaymentInformationRowDTO implements IPaymentInformationRowDTO {
  paymentInformationRowId!: number;
  paymentInformationId!: number;
  sysPaymentTypeId: number;
  sysPaymentTypeName: string;
  paymentNr: string = '';
  default: boolean;
  shownInInvoice: boolean = false;
  created?: Date;
  createdBy: string = '';
  modified?: Date;
  modifiedBy: string = '';
  state: SoeEntityState = SoeEntityState.Active;
  bic: string = '';
  clearingCode: string = '';
  paymentCode: string = '';
  paymentMethodCode?: number;
  paymentForm?: number;
  chargeCode?: number;
  intermediaryCode?: number;
  currencyAccount: string = '';
  payerBankId: string = '';
  bankConnected: boolean = false;
  currencyId?: number;
  paymentMethodCodeName: string = '';
  paymentFormName: string = '';
  chargeCodeName: string = '';
  intermediaryCodeName: string = '';
  currencyCode: string = '';
  paymentNrDisplay: string = '';
  billingType!: TermGroup_BillingType;

  constructor(options: {
    default: boolean;
    sysPaymentType?: ISmallGenericType;
    foreignPaymentMethod?: ISmallGenericType;
    foreignPaymentForm?: ISmallGenericType;
    foreignPaymentType?: ISmallGenericType;
    foreignPaymentChargeCode?: ISmallGenericType;
    foreignPaymentIntermediaryCode?: ISmallGenericType;
  }) {
    this.default = options.default || false;
    this.sysPaymentTypeId = options.sysPaymentType?.id || 0;
    this.sysPaymentTypeName = options.sysPaymentType?.name || '';
    this.paymentMethodCode = options.foreignPaymentMethod?.id;
    this.paymentMethodCodeName = options.foreignPaymentMethod?.name || '';
    this.paymentForm = options.foreignPaymentForm?.id;
    this.paymentFormName = options.foreignPaymentForm?.name || '';
    this.chargeCode = options.foreignPaymentChargeCode?.id;
    this.chargeCodeName = options.foreignPaymentChargeCode?.name || '';
    this.intermediaryCode = options.foreignPaymentIntermediaryCode?.id;
    this.intermediaryCodeName =
      options.foreignPaymentIntermediaryCode?.name || '';
  }
}
