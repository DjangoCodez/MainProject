import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountYearDTO } from './account-years-and-periods.model';

interface IOpeningBalancesForm {
  validationHandler: ValidationHandler;
  element: AccountYearDTO | undefined;
}

export class OpeningBalancesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IOpeningBalancesForm) {
    super(validationHandler, {
      accountYearId: new SoeTextFormControl(element?.accountYearId),
      yearFromTo: new SoeTextFormControl(element?.yearFromTo || ''),
    });
  }

  get accountYearId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountYearId;
  }
}
