import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeTimeFormControl,
  SoeTimeRangeFormControl,
} from '@shared/extensions';
import { TimerangeModel } from './timerange.model';

interface ITimerangeForm {
  validationHandler: ValidationHandler;
  element: TimerangeModel | undefined;
}

export class TimerangeForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ITimerangeForm) {
    super(validationHandler, {
      valueFrom: new SoeTimeFormControl(
        element && element?.timerange ? element?.timerange[0] || '' : ''
      ),
      valueTo: new SoeTimeFormControl(
        element && element?.timerange ? element?.timerange[1] || '' : ''
      ),
      timerange: new SoeTimeRangeFormControl(element?.timerange, {
        validateRange: true,
      }),
    });
    this.thisValidationHandler = validationHandler;
  }

  get valueFrom(): SoeTimeFormControl {
    return <SoeTimeFormControl>this.controls.valueFrom;
  }

  get valueTo(): SoeTimeFormControl {
    return <SoeTimeFormControl>this.controls.valueTo;
  }

  get timerange(): SoeTimeRangeFormControl {
    return <SoeTimeRangeFormControl>this.controls.timerange;
  }
}
