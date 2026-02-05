import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CustomerCentralOfferDTO } from './customer-central.model';

interface ICustomerCentralOfferForm {
  validationHandler: ValidationHandler;
  element: CustomerCentralOfferDTO | undefined;
}

export class CustomerCentralOfferForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICustomerCentralOfferForm) {
    super(validationHandler, {
      offerSelectedTotal: new SoeNumberFormControl(
        element?.offerSelectedTotal || 0.0
      ),
      offerFilteredTotal: new SoeNumberFormControl(
        element?.offerFilteredTotal || 0.0
      ),
    });
  }

  get offerSelectedTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.offerSelectedTotal;
  }
  get offerFilteredTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.offerFilteredTotal;
  }
}
