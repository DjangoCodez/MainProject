import { FormArray } from '@angular/forms';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountProvisionTransactionsRowForm } from './account-provision-transactions-row-form.model';
import { IAccountProvisionTransactionGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAccountProvisionTransactionsForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}
export class AccountProvisionTransactionsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;

  constructor({
    validationHandler,
    element,
  }: IAccountProvisionTransactionsForm) {
    super(validationHandler, {
      timePeriodHeadId: new SoeSelectFormControl(
        element?.timePeriodHeadId || 0,
        {
          required: true,
        },
        'time.time.timeperiod.timeperiodhead'
      ),
      timePeriodId: new SoeSelectFormControl(
        element?.timePeriodId || 0,
        {
          required: true,
          disabled: true,
        },
        'time.time.timeperiod.timeperiod'
      ),
      rows: new FormArray([]),
    });
  }
  set rows(formArray: FormArray) {
    this.controls.rows = formArray;
  }
  get rows(): FormArray {
    return <FormArray>this.controls.rows;
  }

  patchRows(inputRows: IAccountProvisionTransactionGridDTO[]): void {
    this.rows.clear();
    for (const row of inputRows) {
      const rowForm = new AccountProvisionTransactionsRowForm({
        validationHandler: this.thisValidationHandler!,
        element: row,
      });
      this.rows.push(rowForm);
    }
  }
}
