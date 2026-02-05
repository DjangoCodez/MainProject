import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeCodeInvoiceProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeCodeInvoiceProductForm {
  validationHandler: ValidationHandler;
  element: ITimeCodeInvoiceProductDTO | undefined;
}

export class TimeCodeInvoiceProductForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeCodeInvoiceProductForm) {
    super(validationHandler, {
      timeCodeInvoiceProductId: new SoeTextFormControl(
        element?.timeCodeInvoiceProductId || 0,
        { isIdField: true }
      ),
      timeCodeId: new SoeTextFormControl(element?.timeCodeId || 0),
      invoiceProductId: new SoeSelectFormControl(
        element?.invoiceProductId || undefined,
        { required: true, zeroNotAllowed: true },
        'time.time.timecode.invoiceproduct'
      ),
      factor: new SoeNumberFormControl(
        element?.factor || 1,
        {
          minValue: -999.99,
          maxValue: 999.99,
        },
        'time.time.timecode.factor'
      ),
    });
  }

  get invoiceProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.invoiceProductId;
  }

  get factor(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.factor;
  }

  customPatchValue(element: ITimeCodeInvoiceProductDTO) {
    this.patchValue(element);
  }
}
