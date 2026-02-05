import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmploymentTypeDTO } from '@shared/models/generated-interfaces/EmploymentTypeDTO';

interface IEmploymentTypesForm {
  validationHandler: ValidationHandler;
  element: IEmploymentTypeDTO | undefined;
}

export class EmploymentTypesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmploymentTypesForm) {
    super(validationHandler, {
      employmentTypeId: new SoeTextFormControl(element?.employmentTypeId || 0, {
        isIdField: true,
      }),
      type: new SoeSelectFormControl(
        element?.type || undefined,
        { required: true },
        'common.type'
      ),
      code: new SoeTextFormControl(element?.code || '', { maxLength: 50 }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 128 },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || '', {
        maxLength: 512,
      }),
      excludeFromWorkTimeWeekCalculationOnSecondaryEmployment:
        new SoeCheckboxFormControl(
          element?.excludeFromWorkTimeWeekCalculationOnSecondaryEmployment ||
            false
        ),
      settingOnly: new SoeCheckboxFormControl(element?.settingOnly || false),
      externalCode: new SoeTextFormControl(element?.externalCode || '', {
        maxLength: 100,
      }),
    });
  }
}
