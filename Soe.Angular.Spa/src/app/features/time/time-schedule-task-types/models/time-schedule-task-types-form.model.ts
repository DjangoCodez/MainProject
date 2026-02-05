import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ITimeScheduleTaskTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ValidationHandler } from '@shared/handlers';

interface ITimeScheduleTaskTypeForm {
  validationHandler: ValidationHandler;
  element: ITimeScheduleTaskTypeDTO | undefined;
}

export class TimeScheduleTaskTypeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeScheduleTaskTypeForm) {
    super(validationHandler, {
      timeScheduleTaskTypeId: new SoeTextFormControl(
        element?.timeScheduleTaskTypeId || 0,
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
      accountId: new SoeSelectFormControl(element?.accountId || undefined),
    });
  }

  get timeScheduleTaskTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeScheduleTaskTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
}
