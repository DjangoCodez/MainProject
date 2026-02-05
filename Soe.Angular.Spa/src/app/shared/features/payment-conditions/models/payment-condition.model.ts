import { IPaymentConditionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class PaymentConditionDTO implements IPaymentConditionDTO {
  paymentConditionId!: number;
  code!: string;
  name!: string;
  days!: number;
  discountDays?: number;
  discountPercent?: number;
  startOfNextMonth!: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
}
