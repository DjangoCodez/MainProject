import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeNumberRangeFormControl,
} from '@shared/extensions';
import { NumberrangeModel } from './numberrange.model';

interface INumberrangeForm {
  validationHandler: ValidationHandler;
  element: NumberrangeModel | undefined;
}

export class NumberrangeForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: INumberrangeForm) {
    super(validationHandler, {
      valueFrom: new SoeNumberFormControl(
        element && element?.numberrange ? element?.numberrange[0] : ''
      ),
      valueTo: new SoeNumberFormControl(
        element && element?.numberrange ? element?.numberrange[1] : ''
      ),
      numberrange: new SoeNumberRangeFormControl(element?.numberrange),
    });
    this.thisValidationHandler = validationHandler;
  }

  get valueFrom(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.valueFrom;
  }

  get valueTo(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.valueTo;
  }

  get numberrange(): SoeNumberRangeFormControl {
    return <SoeNumberRangeFormControl>this.controls.numberrange;
  }
}
