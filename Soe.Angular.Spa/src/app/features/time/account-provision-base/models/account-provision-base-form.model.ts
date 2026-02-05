import { FormArray } from '@angular/forms';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountProvisionBaseRowForm } from './account-provision-base-row-form.model';
import { IAccountProvisionBaseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAccountProvisionBaseForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}
export class AccountProvisionBaseForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;

  constructor({ validationHandler, element }: IAccountProvisionBaseForm) {
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
  customPatchValue(element: any) {
    if (element.rows != null) {
      this.patchRows(element.rows);
    }
    this.patchValue(element);
  }
  patchRows(inputRows: IAccountProvisionBaseDTO[]): void {
    this.rows.clear();
    for (const row of inputRows) {
      const rowForm = new AccountProvisionBaseRowForm({
        validationHandler: this.thisValidationHandler!,
        element: row,
      });
      this.rows.push(rowForm);
    }
  }
}
