import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeGroupDayTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeeGroupDayTypeForm {
  validationHandler: ValidationHandler;
  element: IEmployeeGroupDayTypeDTO | undefined;
}
export class EmployeeGroupDayTypeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmployeeGroupDayTypeForm) {
    super(validationHandler, {
      dayTypeId: new SoeSelectFormControl(element?.dayTypeId || 0, {}),
      isHolidaySalary: new SoeCheckboxFormControl(
        element?.isHolidaySalary || true,
        {}
      ),
    });
  }

  customPatchValue(element: IEmployeeGroupDayTypeDTO) {
    this.patchValue(element);
  }
}
