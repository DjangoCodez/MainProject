import { ValidationHandler } from '@shared/handlers';
import { DatespickerModel } from './datespicker.model';
import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { arrayToFormArray } from '@shared/util/form-util';
import { FormArray } from '@angular/forms';

interface IDatespickerForm {
  validationHandler: ValidationHandler;
  element: DatespickerModel | undefined;
}

export class DatespickerForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IDatespickerForm) {
    super(validationHandler, {
      date: new SoeDateFormControl(element?.date),
      dates: arrayToFormArray(element?.dates || []),
    });
    this.thisValidationHandler = validationHandler;
  }

  get dates(): FormArray<SoeDateFormControl> {
    return <FormArray>this.controls.dates;
  }

  public patchDates(dates: Date[] | undefined): void {
    this.dates.clear({ emitEvent: false });

    if (dates) {
      arrayToFormArray(dates).controls.forEach(f => {
        this.dates.push(<SoeDateFormControl>f, { emitEvent: false });
      });
    }
    this.dates.updateValueAndValidity();
  }
}
