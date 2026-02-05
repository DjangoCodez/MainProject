import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { InvoiceExportIODTO } from './direct-debit.model';
import { FormArray } from '@angular/forms';
import { DirectDebitEditGridForm } from './direct-debit-edit-grid-form.model';

interface IDirectDebitForm {
  validationHandler: ValidationHandler;
  element: InvoiceExportIODTO | undefined;
}

export class DirectDebitForm extends SoeFormGroup {
  directDebitValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IDirectDebitForm) {
    super(validationHandler, {
      batchId: new SoeTextFormControl(element?.batchId || '', {
        isNameField: true,
        disabled: true,
      }),
      invoiceExportId: new SoeTextFormControl(element?.invoiceExportId || 0, {
        isIdField: true,
      }),
      paymentServiceId: new SoeSelectFormControl(element?.payerId || 0, {
        disabled: (element?.invoiceExportId || 0) > 0,
      }),
      exportedInvoices: new FormArray<DirectDebitEditGridForm>([]),
    });

    this.directDebitValidationHandler = validationHandler;
  }

  get batchId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.batchId;
  }

  get invoiceExportId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceExportId;
  }

  get paymentServiceId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.paymentServiceId;
  }
}
