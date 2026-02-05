import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeCodePayrollProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeCodePayrollProductForm {
  validationHandler: ValidationHandler;
  element: ITimeCodePayrollProductDTO | undefined;
}

export class TimeCodePayrollProductForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeCodePayrollProductForm) {
    super(validationHandler, {
      timeCodePayrollProductId: new SoeTextFormControl(
        element?.timeCodePayrollProductId || 0,
        { isIdField: true }
      ),
      timeCodeId: new SoeTextFormControl(element?.timeCodeId || 0),
      payrollProductId: new SoeSelectFormControl(
        element?.payrollProductId || undefined,
        { required: true, zeroNotAllowed: true },
        'time.payroll.payrollproduct.payrollproduct'
      ),
      factor: new SoeNumberFormControl(
        element?.factor || 1,
        {
          minValue: -999.99999,
          maxValue: 999.99999,
        },
        'time.time.timecode.factor'
      ),
    });
  }

  get payrollProductId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.payrollProductId;
  }

  get factor(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.factor;
  }

  customPatchValue(element: ITimeCodePayrollProductDTO) {
    this.patchValue(element);
  }
}
