import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export interface IId {
  id: number;
}

interface IIdForm {
  validationHandler: ValidationHandler;
  element: IId | undefined;
}
export class IdForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IIdForm) {
    super(validationHandler, {
      id: new SoeSelectFormControl(element?.id || 0, {}),
    });
  }

  customPatchValue(element: IId) {
    this.patchValue(element);
  }
}
