import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISkillDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ISkillsForm {
  validationHandler: ValidationHandler;
  element: ISkillDTO | undefined;
}
export class SkillsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISkillsForm) {
    super(validationHandler, {
      skillId: new SoeTextFormControl(element?.skillId || 0, {
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
      skillTypeId: new SoeSelectFormControl(
        element?.skillTypeId || 0,
        { required: true, zeroNotAllowed: true },
        'common.type'
      ),
    });
  }

  get skillName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillName;
  }

  get skillType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.skillType;
  }

  get skillDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillDescription;
  }
}
