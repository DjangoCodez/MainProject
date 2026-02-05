import { ValidationHandler } from '@shared/handlers';
import { AccountingReconciliationFilterDTO } from './accounting-reconciliation.model';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';

interface IAccountingReconciliationFilterForm {
  validationHandler: ValidationHandler;
  element: AccountingReconciliationFilterDTO;
}

export class AccountingReconciliationFilterForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element,
  }: IAccountingReconciliationFilterForm) {
    super(validationHandler, {
      fromDate: new SoeDateFormControl(element?.fromDate || 0),
      toDate: new SoeDateFormControl(element?.toDate || 0),
      fromAccount: new SoeSelectFormControl(element?.fromAccount || 0),
      toAccount: new SoeSelectFormControl(element?.toAccount || 0),
      currentAccountYearId: new SoeNumberFormControl(
        element?.currentAccountYearId || 0
      ),
      currentAccountDimId: new SoeNumberFormControl(
        element?.currentAccountDimId || 0
      ),
    });
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }

  get fromAccount(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.fromAccount;
  }

  get toAccount(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.toAccount;
  }

  get currentAccountYearId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.currentAccountYearId;
  }

  get currentAccountDimId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.currentAccountDimId;
  }
}
