import {
  SoeEntityState,
  SoeOriginType,
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
