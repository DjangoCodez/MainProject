import { SoeCheckboxFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountDistributionGridFilterDTO } from './account-distribution.model';

interface IAccountDistributionGridFilterForm {
  validationHandler: ValidationHandler;
  element: AccountDistributionGridFilterDTO | undefined;
}
export class AccountDistributionGridFilterForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IAccountDistributionGridFilterForm) {
    super(validationHandler, {
      showOpen: new SoeCheckboxFormControl(element?.showOpen || true),
      showClosed: new SoeCheckboxFormControl(element?.showClosed || false),
    });
  }

  get showOpen(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showOpen;
  }
  get showClosed(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.showClosed;
  }
}
