import { FormArray } from '@angular/forms';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

interface IAdjustTimeStampsForm {
  validationHandler: ValidationHandler;
  element: any | undefined;
}
export class AdjustTimeStampsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler | undefined;

  constructor({ validationHandler, element }: IAdjustTimeStampsForm) {
    super(validationHandler, {
      selectedEmployees: new SoeSelectFormControl(
        element?.selectedEmployees || [],
        {},
        'common.employees'
      ),

      selectedDateFrom: new SoeDateFormControl(
        element?.selectedDateFrom || new Date(),
        {},
        'common.fromdate'
      ),
      selectedDateTo: new SoeDateFormControl(
        element?.selectedDateTo || new Date(),
        {},
        'common.todate'
      ),

      rows: new FormArray([]),
    });

    this.thisValidationHandler = validationHandler;
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

  patchRows(inputRows: any[]): void {
    (this.rows).clear();
    for (const row of inputRows) {
      const rowForm = new AdjustTimeStampsForm({
        validationHandler: this.thisValidationHandler!,
        element: row,
      });
      this.rows.push(rowForm);
    }
  }
}
