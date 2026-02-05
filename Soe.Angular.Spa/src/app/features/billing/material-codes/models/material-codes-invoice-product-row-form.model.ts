import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TimeCodeInvoiceProductDTO } from './material-codes.model';

interface ITimeCodeMaterialInvoiceProductRowsForm {
  validationHandler: ValidationHandler;
  element: TimeCodeInvoiceProductDTO | undefined;
}

export class TimeCodeMaterialInvoiceProductRowsForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: ITimeCodeMaterialInvoiceProductRowsForm) {
    super(validationHandler, {
      timeCodeInvoiceProductId: new SoeTextFormControl(
        element?.timeCodeInvoiceProductId || 0,
        {
          isIdField: true,
          required: true,
        }
      ),
      timeCodeId: new SoeTextFormControl(element?.timeCodeId || 0, {
        required: true,
      }),

      invoiceProductId: new SoeSelectFormControl(
        element?.invoiceProductId || undefined,
        { required: true },
        'time.time.timecode.invoiceproduct'
      ),
      factor: new SoeNumberFormControl(
        element?.factor || 1.0,
        { required: true },
        'time.time.timecode.factor'
      ),
      invoiceProductPrice: new SoeNumberFormControl(
        element?.invoiceProductPrice || undefined
      ),
    });
  }

  get timeCodeInvoiceProductId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeInvoiceProductId;
  }

  get timeCodeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeId;
  }

  get invoiceProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoiceProductId;
  }

  get factor(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.factor;
  }
  get invoiceProductPrice(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.invoiceProductPrice;
  }
}
