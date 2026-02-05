import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeGroupTimeDeviationCauseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeeGroupTimeDeviationCause {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupTimeDeviationCauseDTO | undefined;
}
export class EmployeeGroupTimeDeviationCauseForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IEmployeeGroupTimeDeviationCause) {
    super(validationHandler, {
      employeeGroupTimeDeviationCauseId: new SoeSelectFormControl(
        element?.employeeGroupTimeDeviationCauseId || 0,
        { isIdField: true }
      ),
      employeeGroupId: new SoeSelectFormControl(
        element?.employeeGroupId || 0,
        {}
      ),
      timeDeviationCauseId: new SoeSelectFormControl(
        element?.timeDeviationCauseId || 0,
        {}
      ),
      useInTimeTerminal: new SoeCheckboxFormControl(
        element?.useInTimeTerminal || false,
        {}
      ),
    });
  }

  customPatchValue(element: IEmployeeGroupTimeDeviationCauseDTO) {
    this.patchValue(element);
  }
}
