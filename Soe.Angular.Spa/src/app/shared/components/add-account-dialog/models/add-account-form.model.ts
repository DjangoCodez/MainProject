import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IAccountEditDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAddAccountForm {
  validationHandler: ValidationHandler;
  element: IAccountEditDTO | undefined;
}
export class AddAccountForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAddAccountForm) {
    super(validationHandler, {
      accountNr: new SoeTextFormControl(
        element?.accountNr || '',
        { required: true, maxLength: 50 },
        'economy.accounting.accountnr'
      ),

      name: new SoeTextFormControl(
        element?.name || '',
        { required: true, maxLength: 100 },
        'common.name'
      ),

      accountTypeSysTermId: new SoeSelectFormControl(
        element?.accountTypeSysTermId || '',
        { required: true },
        'economy.accounting.accounttype'
      ),

      sysVatAccountId: new SoeSelectFormControl(
        element?.sysVatAccountId || '',
        {},
        'economy.accounting.account.sysvataccount'
      ),

      sysAccountSruCode1Id: new SoeSelectFormControl(
        element?.sysAccountSruCode1Id || '',
        {},
        'economy.accounting.account.scrucode1'
      ),
    });
  }

  get accountNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountNr;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get accountTypeSysTermId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountTypeSysTermId;
  }

  get sysVatAccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysVatAccountId;
  }
  get sysAccountSruCode1Id(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.sysAccountSruCode1Id;
  }
}
