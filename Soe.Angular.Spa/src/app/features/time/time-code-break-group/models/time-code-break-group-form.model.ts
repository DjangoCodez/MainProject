import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ITimeCodeBreakGroupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeCodeBreakGroupForm {
  validationHandler: ValidationHandler;
  element: ITimeCodeBreakGroupDTO | undefined;
}
export class TimeCodeBreakGroupForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeCodeBreakGroupForm) {
    super(validationHandler, {
      timeCodeBreakGroupId: new SoeTextFormControl(
        element?.timeCodeBreakGroupId || 0,
        {
          isIdField: true,
        }
      ),
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

  get timeCodeBreakGroupId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeCodeBreakGroupId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
}
