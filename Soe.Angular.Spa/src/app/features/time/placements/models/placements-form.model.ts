import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IActivateScheduleGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IPlacementsForm {
  validationHandler: ValidationHandler;
  element: IActivateScheduleGridDTO;
}
export class PlacementsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPlacementsForm) {
    super(validationHandler, {
      employeeScheduleId: new SoeTextFormControl(
        element?.employeeScheduleId || 0,
        {
          isIdField: true,
        }
      ),
      employeeName: new SoeTextFormControl(
        element?.employeeName || '',
        { isNameField: true, required: true, maxLength: 100 },
        'common.name'
      ),
    });
  }
}
