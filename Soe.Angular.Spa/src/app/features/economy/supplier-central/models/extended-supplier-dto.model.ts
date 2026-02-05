import { SupplierDTO } from '@features/economy/suppliers/models/supplier.model';

export class ExtSupplierDTO extends SupplierDTO {
  blockPaymentString!: string;
  currencyName!: string;
  paymentConditionName!: string;
  isPrivatePersonString!: string;
}
