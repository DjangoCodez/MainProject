import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TermGroup_AccountStatus } from '@shared/models/generated-interfaces/Enumerations';
import { AccountPeriodDTO } from './account-years-and-periods.model';

interface IAccountPeriodRowsForm {
  validationHandler: ValidationHandler;
  element: AccountPeriodDTO | undefined;
}

export class AccountPeriodRowsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAccountPeriodRowsForm) {
    super(validationHandler, {
      status: new SoeSelectFormControl(
        element?.status || TermGroup_AccountStatus.New
      ),
    });
  }

  get periodStatus(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.status;
  }
}
