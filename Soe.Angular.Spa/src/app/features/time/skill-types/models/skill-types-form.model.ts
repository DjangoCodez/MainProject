import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISkillTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ISkillTypesForm {
  validationHandler: ValidationHandler;
  element: ISkillTypeDTO | undefined;
}
export class SkillTypesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISkillTypesForm) {
    super(validationHandler, {
      skillTypeId: new SoeTextFormControl(element?.skillTypeId || 0, {
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
    });
  }
}
