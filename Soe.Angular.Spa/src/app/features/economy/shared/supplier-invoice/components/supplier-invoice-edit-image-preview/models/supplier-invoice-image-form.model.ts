import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

interface ISupplierInvoiceImageForm {
  validationHandler: ValidationHandler;
}

export class SupplierInvoiceImageForm extends SoeFormGroup {
  constructor({ validationHandler }: ISupplierInvoiceImageForm) {
    super(validationHandler, {
      file: new SoeTextFormControl(undefined),
    });
  }
}
