import { ValidationHandler } from '@shared/handlers';
import { PlanningShiftDTO } from '../../models/shift.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
} from '@shared/extensions';

interface ISpShiftRequestDialogShiftForm {
  validationHandler: ValidationHandler;
  element: PlanningShiftDTO | undefined;
}

export class SpShiftRequestDialogShiftForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ISpShiftRequestDialogShiftForm) {
    super(validationHandler, {
      timeScheduleTemplateBlockId: new SoeNumberFormControl(
        element?.timeScheduleTemplateBlockId || 0
      ),
      actualStartDate: new SoeDateFormControl(
        element?.actualStartDate || undefined
      ),
      startTime: new SoeDateFormControl(element?.startTime || undefined),
      stopTime: new SoeDateFormControl(element?.stopTime || undefined),
      shiftTypeName: new SoeCheckboxFormControl(element?.shiftTypeName || ''),
    });

    this.thisValidationHandler = validationHandler;
  }
}
