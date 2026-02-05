import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SchedulePlanningFilter } from '../../models/filter.model';

interface ISpFilterDialogForm {
  validationHandler: ValidationHandler;
  element: SchedulePlanningFilter | undefined;
}
export class SpFilterDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISpFilterDialogForm) {
    super(validationHandler, {
      employeeIds: new SoeSelectFormControl(element?.employeeIds || []),
      showAllEmployees: new SoeCheckboxFormControl(
        element?.showAllEmployees || false
      ),
      shiftTypeIds: new SoeSelectFormControl(element?.shiftTypeIds || []),
      showHiddenShifts: new SoeCheckboxFormControl(
        element?.showHiddenShifts || false
      ),
      blockTypes: new SoeSelectFormControl(element?.blockTypes || []),
    });
  }
}
