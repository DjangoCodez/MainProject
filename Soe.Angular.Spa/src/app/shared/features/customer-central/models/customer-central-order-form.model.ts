import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CustomerCentralOrderDTO } from './customer-central.model';

interface ICustomerCentralOrderForm {
  validationHandler: ValidationHandler;
  element: CustomerCentralOrderDTO | undefined;
}

export class CustomerCentralOrderForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICustomerCentralOrderForm) {
    super(validationHandler, {
      orderSelectedTotal: new SoeNumberFormControl(
        element?.orderSelectedTotal || 0.0
      ),
      orderFilteredTotal: new SoeNumberFormControl(
        element?.orderFilteredTotal || 0.0
      ),
      orderSelectedToBeInvoicedTotal: new SoeNumberFormControl(
        element?.orderSelectedToBeInvoicedTotal || 0.0
      ),
      orderFilteredToBeInvoicedTotal: new SoeNumberFormControl(
        element?.orderFilteredToBeInvoicedTotal || 0.0
      ),
    });
  }

  get orderSelectedTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderSelectedTotal;
  }
  get orderFilteredTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderFilteredTotal;
  }
  get orderSelectedToBeInvoicedTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderSelectedToBeInvoicedTotal;
  }
  get orderFilteredToBeInvoicedTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.orderFilteredToBeInvoicedTotal;
  }
}
