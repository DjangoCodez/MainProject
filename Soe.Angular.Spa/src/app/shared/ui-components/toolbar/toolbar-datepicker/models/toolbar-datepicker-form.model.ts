import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ToolbarDatepickerModel } from './toolbar-datepicker.model';

interface IToolbarDatepickerForm {
  validationHandler: ValidationHandler;
  element: ToolbarDatepickerModel;
}

export class ToolbarDatepickerForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IToolbarDatepickerForm) {
    super(validationHandler, {
      date: new SoeDateFormControl(element?.date),
    });
    this.thisValidationHandler = validationHandler;
  }

  get date(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.date;
  }
}
