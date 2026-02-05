import {
  SoeFormGroup,
  SoeRadioFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class EmployeeCsrExportForm extends SoeFormGroup {
  constructor(validationHandler: ValidationHandler, selectedYearType: number) {
    super(validationHandler, {
      currentViewType: new SoeSelectFormControl(0),
      selectedYearType: new SoeRadioFormControl(selectedYearType),
    });
  }
}
