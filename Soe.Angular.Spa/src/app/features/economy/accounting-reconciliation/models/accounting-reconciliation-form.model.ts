import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ReconciliationRowDTO } from './accounting-reconciliation.model';

interface IAccountingReconciliationForm {
  validationHandler: ValidationHandler;
  element: ReconciliationRowDTO;
}

export class AccountingReconciliationForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAccountingReconciliationForm) {
    super(validationHandler, {
      fromDate: new SoeDateFormControl(element?.fromDate || 0),
      toDate: new SoeDateFormControl(element?.toDate || 0),
      accountYearId: new SoeNumberFormControl(element?.accountYearId || 0),
      accountId: new SoeNumberFormControl(element?.accountId || 0, {
        required: true,
        isIdField: true,
      }),
      number: new SoeTextFormControl(element?.number || '', {
        required: true,
        isNameField: true,
      }),
      account: new SoeTextFormControl(element?.account || ''),
    });
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }

  get accountYearId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountYearId;
  }

  get accountId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountId;
  }

  get account(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.account;
  }
}
