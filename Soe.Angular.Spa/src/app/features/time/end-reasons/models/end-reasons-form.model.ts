import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEndReasonDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IEndReasonsForm {
  validationHandler: ValidationHandler;
  element: IEndReasonDTO | undefined;
}
export class EndReasonsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEndReasonsForm) {
    super(validationHandler, {
      endReasonId: new SoeTextFormControl(element?.endReasonId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { maxLength: 50 },
        'common.code'
      ),
      isActive: new SoeCheckboxFormControl(
        element?.isActive || true,
        {},
        'common.active'
      ),
    });
  }

  get endReasonId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dayTypeId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  get isActive(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isActive;
  }
}
