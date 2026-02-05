import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IFollowUpTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IFollowupTypeForm {
  validationHandler: ValidationHandler;
  element: IFollowUpTypeDTO | undefined;
}
export class FollowupTypeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IFollowupTypeForm) {
    super(validationHandler, {
      followUpTypeId: new SoeTextFormControl(element?.followUpTypeId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, maxLength: 100, required: true },
        'common.name'
      ),
      type: new SoeSelectFormControl(
        element?.type || 0,
        { required: true },
        'common.type'
      ),
    });
  }

  get followUpTypeId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.followUpTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
}
