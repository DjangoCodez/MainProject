import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { UserSelectionFormDTO } from './user-selection.model';

interface IUserSelectionForm {
  validationHandler: ValidationHandler;
  element: UserSelectionFormDTO | undefined;
}

export class UserSelectionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IUserSelectionForm) {
    super(validationHandler, {
      selectedUserSelectionId: new SoeNumberFormControl(
        element?.selectedUserSelectionId
      ),
    });
  }

  get selectedUserSelectionId(): number {
    return <number>this.controls.selectedUserSelectionId.value;
  }
}
