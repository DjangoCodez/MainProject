import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { EditDeliveryAddressDTO } from './edit-delivery-address.model';

interface IEditDeliveryAddressForm {
  validationHandler: ValidationHandler;
  element: EditDeliveryAddressDTO | undefined;
}
export class EditDeliveryAddressForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEditDeliveryAddressForm) {
    super(validationHandler, {
      name: new SoeTextFormControl(element?.name || ''),
      address: new SoeTextFormControl(element?.address || ''),
      postalCode: new SoeTextFormControl(element?.postalCode || ''),
      postalAddress: new SoeTextFormControl(element?.postalAddress || ''),
      country: new SoeTextFormControl(element?.country || ''),
      deliveryAddress: new SoeCheckboxFormControl(
        element?.deliveryAddress || ''
      ),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get address(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.address;
  }

  get postalCode(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.postalCode;
  }

  get postalAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.postalAddress;
  }

  get country(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.country;
  }
  get deliveryAddress(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.deliveryAddress;
  }
}
