import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeGroupTimeDeviationCauseTimeCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeeGroupsTimeDeviationCauseTimeCodeForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupTimeDeviationCauseTimeCodeDTO | undefined;
}
export class EmployeeGroupsTimeDeviationCauseTimeCodeForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IEmployeeGroupsTimeDeviationCauseTimeCodeForm) {
    super(validationHandler, {
      timeDeviationCauseId: new SoeSelectFormControl(
        element?.timeDeviationCauseId || 0,
        {}
      ),
      timeCodeId: new SoeSelectFormControl(element?.timeCodeId || 0, {}),
    });
  }

  customPatchValue(element: IEmployeeGroupTimeDeviationCauseTimeCodeDTO) {
    this.patchValue(element);
  }
}
