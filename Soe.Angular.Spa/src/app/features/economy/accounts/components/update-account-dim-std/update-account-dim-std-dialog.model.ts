import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class UpdateAccountDimStdDialogDTO {
  accountStdTypeId?: number;
}

interface IUpdateAccountDimStdDialogForm {
  validationHandler: ValidationHandler;
  element: UpdateAccountDimStdDialogDTO;
}

export class UpdateAccountDimStdDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IUpdateAccountDimStdDialogForm) {
    super(validationHandler, {
      accountStdTypeId: new SoeNumberFormControl(
        element?.accountStdTypeId || undefined
      ),
    });
  }

  get accountStdTypeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountStdTypeId;
  }
}
