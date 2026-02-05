import { ValidationHandler } from '@shared/handlers';
import { AuthenticationFormData } from './authentication-form-data.model';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { Validators } from '@angular/forms';

interface ISignatoryContractAuthForm {
  validationHandler: ValidationHandler;
  element: AuthenticationFormData | undefined;
}

export class SignatoryContractAuthForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISignatoryContractAuthForm) {
    super(validationHandler, {
      username: new SoeTextFormControl(
        element?.username || '',
        { required: true },
        'common.username'
      ),
      password: new SoeTextFormControl(
        element?.password || '',
        { required: true },
        'common.password'
      ),
      code: new SoeTextFormControl(element?.code || '', {}, 'common.code'),
    });
  }

  get username(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.username;
  }

  get password(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.password;
  }

  get code(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.code;
  }

  setCodeValidators(showCode: boolean) {
    if (showCode) {
      this.code.addValidators([Validators.required]);
      this.code.updateValueAndValidity();
    }
  }
}
