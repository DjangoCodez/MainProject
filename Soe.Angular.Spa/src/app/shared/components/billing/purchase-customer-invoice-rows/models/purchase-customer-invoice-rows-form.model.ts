import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PurchaseCustomerInvoiceRowsDTO } from './purchase-customer-invoice-rows.model';

interface IPurchaseCustomerInvoiceRowsForm {
  validationHandler: ValidationHandler;
  element: PurchaseCustomerInvoiceRowsDTO | undefined;
}

export class PurchaseCustomerInvoiceRowsForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IPurchaseCustomerInvoiceRowsForm) {
    super(validationHandler, {
      attestStateTo: new SoeSelectFormControl(element?.attestStateTo || 0),
    });
  }

  get attestStateTo(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.attestStateTo;
  }
}
