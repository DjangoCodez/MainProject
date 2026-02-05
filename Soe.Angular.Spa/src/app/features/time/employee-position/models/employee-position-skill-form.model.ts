import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IPositionSkillDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEmployeePositionSkillForm {
  validationHandler: ValidationHandler;
  element: IPositionSkillDTO | undefined;
}

export class EmployeePositionSkillForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmployeePositionSkillForm) {
    super(validationHandler, {
      positionSkillId: new SoeTextFormControl(element?.positionSkillId || 0, {
        isIdField: true,
      }),
      positionId: new SoeNumberFormControl(element?.positionId || 0),
      skillId: new SoeNumberFormControl(element?.skillId || 0),
      skillLevel: new SoeNumberFormControl(element?.skillLevel || 20),
      positionName: new SoeTextFormControl(element?.positionName || ''),
      skillLevelStars: new SoeNumberFormControl(element?.skillLevelStars || 0),
      skillLevelUnreached: new SoeCheckboxFormControl(
        element?.skillLevelUnreached || false
      ),
      skillName: new SoeTextFormControl(element?.skillName || ''),
      skillTypeName: new SoeTextFormControl(element?.skillTypeName || ''),
      missing: new SoeCheckboxFormControl(element?.missing || false),
    });
  }

  get positionSkillId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.positionSkillId;
  }
  get positionId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.positionId;
  }
  get skillLevel(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillLevel;
  }
  get positionName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.positionName;
  }
  get skillLevelStars(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillLevelStars;
  }
  get skillLevelUnreached(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillLevelUnreached;
  }
  get skillName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillName;
  }
  get skillTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.skillTypeName;
  }
  get missing(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.missing;
  }
}
