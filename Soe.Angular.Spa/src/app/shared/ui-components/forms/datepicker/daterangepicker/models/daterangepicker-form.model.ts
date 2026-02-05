import { ValidationHandler } from '@shared/handlers';
import {
  SoeDateFormControl,
  SoeDateRangeFormControl,
  SoeFormGroup,
} from '@shared/extensions';
import { DaterangepickerModel } from './daterangepicker.model';
import { DateUtil } from '@shared/util/date-util';

interface IDaterangepickerForm {
  validationHandler: ValidationHandler;
  element: DaterangepickerModel | undefined;
}

export class DaterangepickerForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IDaterangepickerForm) {
    super(validationHandler, {
      dateFrom: new SoeDateFormControl(
        element && element?.daterange
          ? DateUtil.toDateTimeString(element?.daterange[0] || new Date())
          : ''
      ),
      dateTo: new SoeDateFormControl(
        element && element?.daterange
          ? DateUtil.toDateTimeString(element?.daterange[1] || new Date())
          : ''
      ),
      daterange: new SoeDateRangeFormControl(element?.daterange),
    });
    this.thisValidationHandler = validationHandler;
  }

  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }

  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }

  get daterange(): SoeDateRangeFormControl {
    return <SoeDateRangeFormControl>this.controls.daterange;
  }
}
