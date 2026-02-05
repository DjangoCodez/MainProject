import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

interface IGeneratedNeedsForm {
  validationHandler: ValidationHandler;
}

export class GeneratedNeedsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler }: IGeneratedNeedsForm) {
    super(validationHandler, {});
    this.thisValidationHandler = validationHandler;
  }
}

export class GeneratedNeedsDialogData implements DialogData {
  size?: DialogSize;
  title: string = '';

  timeScheduleTaskId: number = 0;
  date?: Date;
}
