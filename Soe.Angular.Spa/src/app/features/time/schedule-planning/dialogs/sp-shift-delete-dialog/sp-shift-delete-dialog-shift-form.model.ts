import { ValidationHandler } from '@shared/handlers';
import { PlanningShiftDTO } from '../../models/shift.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
} from '@shared/extensions';

interface ISpShiftDeleteDialogShiftForm {
  validationHandler: ValidationHandler;
  element: PlanningShiftDTO | undefined;
}

export class SpShiftDeleteDialogShiftForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ISpShiftDeleteDialogShiftForm) {
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
      selected: new SoeCheckboxFormControl((element as any)?.selected || false),
    });

    this.thisValidationHandler = validationHandler;
  }
}
