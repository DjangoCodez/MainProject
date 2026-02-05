import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { MatchCodeDTO } from './match-codes.model';

interface IMatchCodesForm {
  validationHandler: ValidationHandler;
  element: MatchCodeDTO | undefined;
}

export class MatchCodesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IMatchCodesForm) {
    super(validationHandler, {
      matchCodeId: new SoeTextFormControl(element?.matchCodeId ?? 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name ?? '',
        { isNameField: true, required: true, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description ?? '',
        { maxLength: 50 },
        'common.description'
      ),
      type: new SoeSelectFormControl(
        element?.type ?? 0,
        {
          required: true,
          zeroNotAllowed: true,
        },
        'common.type'
      ),
      accountId: new SoeSelectFormControl(
        element?.accountId ?? 0,
        { required: true, zeroNotAllowed: true },
        'economy.accounting.account'
      ),
      accountNr: new SoeTextFormControl(element?.accountNr ?? ''),
      vatAccountId: new SoeSelectFormControl(element?.vatAccountId ?? 0),
      vatAccountNr: new SoeTextFormControl(element?.vatAccountNr ?? ''),
    });
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }

  get vatAccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.vatAccountId;
  }
}
