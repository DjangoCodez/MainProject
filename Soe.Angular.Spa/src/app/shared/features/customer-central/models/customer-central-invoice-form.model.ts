import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CustomerCentralInvoiceDTO } from './customer-central.model';

interface ICustomerCentralInvoiceForm {
  validationHandler: ValidationHandler;
  element: CustomerCentralInvoiceDTO | undefined;
}

export class CustomerCentralInvoiceForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICustomerCentralInvoiceForm) {
    super(validationHandler, {
      invoiceSelectedTotal: new SoeNumberFormControl(
        element?.invoiceSelectedTotal || 0.0
      ),
      invoiceFilteredTotal: new SoeNumberFormControl(
        element?.invoiceFilteredTotal || 0.0
      ),
      invoiceSelectedToPay: new SoeNumberFormControl(
        element?.invoiceSelectedToPay || 0.0
      ),
      invoiceFilteredToPay: new SoeNumberFormControl(
        element?.invoiceFilteredToPay || 0.0
      ),
    });
  }

  get invoiceSelectedTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceSelectedTotal;
  }
  get invoiceFilteredTotal(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceFilteredTotal;
  }
  get invoiceSelectedToPay(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceSelectedToPay;
  }
  get invoiceFilteredToPay(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceFilteredToPay;
  }
}
