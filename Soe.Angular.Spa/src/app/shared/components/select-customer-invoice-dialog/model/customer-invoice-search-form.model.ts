import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InvoiceGridFormDTO } from './customer-invoice-search.model';

interface ICustomerInvoiceSearchForm {
  validationHandler: ValidationHandler;
  element: InvoiceGridFormDTO | undefined;
}

export class CustomerInvoiceSearchForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICustomerInvoiceSearchForm) {
    super(validationHandler, {
      invoiceNumber: new SoeTextFormControl(element?.invoiceNumber || ''),
    });
  }

  get invoiceNumber(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceNumber;
  }
}
