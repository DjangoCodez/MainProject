import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SetAccountDialogData } from './account-years-and-periods.model';

interface ISetAccountDialogForm {
  validationHandler: ValidationHandler;
  element: SetAccountDialogData | undefined;
}

export class SetAccountDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISetAccountDialogForm) {
    super(validationHandler, {
      amount: new SoeNumberFormControl(element?.amount || ''),
      accountId: new SoeSelectFormControl(element?.accounts || '', {
        required: true,
      }),
    });
  }

  get amount(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amount;
  }

  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }
}
