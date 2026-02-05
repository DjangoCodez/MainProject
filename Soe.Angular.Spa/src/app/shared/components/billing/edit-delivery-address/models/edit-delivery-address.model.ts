//import { IEditDeliveryAddressDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';

export class EditDeliveryAddressDTO {
  name: string;
  address: string;
  postalCode: string;
  postalAddress: string;
  country: string;
  deliveryAddress: string;

  constructor() {
    this.name = '';
    this.address = '';
    this.postalCode = '';
    this.postalAddress = '';
    this.country = '';
    this.deliveryAddress = '';
  }
}
