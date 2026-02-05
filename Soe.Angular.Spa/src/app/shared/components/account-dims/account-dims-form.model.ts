import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountDTO } from '@shared/models/account.model';

interface IAccountDimsForm {
  accountDimsValidationHandler: ValidationHandler;
  element: AccountDims | undefined;
}

export class SelectedAccountsChangeSet {
  selectedAccounts!: SelectedAccounts;
  manuallyChanged!: boolean;
  dimNr!: number;
}

export class SelectedAccounts {
  account1!: AccountDTO;
  account2!: AccountDTO;
  account3!: AccountDTO;
  account4!: AccountDTO;
  account5!: AccountDTO;
  account6!: AccountDTO;
}

export class AccountDims {
  account1!: number;
  account2!: number;
  account3!: number;
  account4!: number;
  account5!: number;
  account6!: number;
}

export class AccountDimsForm extends SoeFormGroup implements IAccountDimsForm {
  accountDimsValidationHandler: ValidationHandler;
  element: AccountDims | undefined;

  constructor({ accountDimsValidationHandler, element }: IAccountDimsForm) {
    super(accountDimsValidationHandler, {
      account1: new SoeSelectFormControl(element?.account1),
      account2: new SoeSelectFormControl(element?.account2),
      account3: new SoeSelectFormControl(element?.account3),
      account4: new SoeSelectFormControl(element?.account4),
      account5: new SoeSelectFormControl(element?.account5),
      account6: new SoeSelectFormControl(element?.account6),
    });
    this.accountDimsValidationHandler = accountDimsValidationHandler;
  }

  get account1(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.account1;
  }

  get account2(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.account2;
  }

  get account3(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.account3;
  }

  get account4(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.account4;
  }

  get account5(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.account5;
  }

  get account6(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.account6;
  }
}
