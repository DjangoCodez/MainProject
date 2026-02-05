import { ValidationHandler } from '@shared/handlers';
import {
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';

interface IUserSelectorForm {
  validationHandler: ValidationHandler;
}

export class UserSelectorForm extends SoeFormGroup {
  constructor({ validationHandler }: IUserSelectorForm) {
    super(validationHandler, {
      type: new SoeSelectFormControl<number>(
        0,
        { },
        'economy.supplier.attestgroup.type'
      ),
    });
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

}
