import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IPayrollLevelDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IPayrollLevelsForm {
  validationHandler: ValidationHandler;
  element: IPayrollLevelDTO | undefined;
}
export class PayrollLevelsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IPayrollLevelsForm) {
    super(validationHandler, {
      payrollLevelId: new SoeTextFormControl(element?.payrollLevelId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { maxLength: 50, minLength: 1 },
        'common.code'
      ),
      externalCode: new SoeTextFormControl(
        element?.externalCode || '',
        { maxLength: 50, minLength: 1 },
        'common.externalcode'
      ),
    });
  }
}
