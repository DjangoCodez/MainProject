import { ICustomerSearchModel } from '@shared/models/generated-interfaces/CoreModels';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SelectCustomerSearchDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;
  originType!: number;
  customerValue!: ICustomerSearchModel;
}

export class CustomerSearchModelDTO implements ICustomerSearchModel {
  actorCustomerId: number = 0;
  customerNr: string = '';
  name: string = '';
  billingAddress: string = '';
  deliveryAddress: string = '';
  note: string = '';
  phoneNumber: string = '';
}

export class SelectCustomerGridFormDTO {
  customerId?: string = '';
}
