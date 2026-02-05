import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { StaffingNeedsRuleDTO } from '../../models/staffing-needs.model';
import { FormArray, FormControl, FormGroup } from '@angular/forms';
import { IStaffingNeedsRuleRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { StaffingNeedsRulesRowForm } from './staffing-needs-rules-row-form.model';

interface IStaffingNeedsRulesForm {
  validationHandler: ValidationHandler;
  element: StaffingNeedsRuleDTO | undefined;
}
export class StaffingNeedsRulesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IStaffingNeedsRulesForm) {
    super(validationHandler, {
      staffingNeedsRuleId: new SoeTextFormControl(
        element?.staffingNeedsRuleId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      staffingNeedsLocationGroupId: new SoeSelectFormControl(
        element?.staffingNeedsLocationGroupId || 0,
        { required: true },
        'common.group'
      ),
      unit: new SoeSelectFormControl(
        element?.unit || 0,
        { required: true, zeroNotAllowed: true },
        'common.unit'
      ),
      maxQuantity: new SoeNumberFormControl(
        element?.maxQuantity || 0,
        { maxValue: 100 },
        'time.schedule.staffingneedsrule.maxquantity'
      ),
      accountId: new SoeSelectFormControl(element?.accountId || null),
      rows: new FormArray([]),
    });
    this.onCopy = this.onDoCopy.bind(this);
  }

  onDoCopy() {
    // CLEAR relation ids
  }

  customPatch(value: StaffingNeedsRuleDTO | undefined) {
    if (value) {
      this.patchValue(value, { emitEvent: false });
      this.rows.clear();
      value.rows.forEach(r => {
        this.rows.push(
          new StaffingNeedsRulesRowForm({
            validationHandler: this.formValidationHandler,
            element: r,
          })
        );
      });
      this.updateValueAndValidity();
    }
  }

  patchRows(rowItems: IStaffingNeedsRuleRowDTO[]): void {
    (this.controls.rows as FormArray).clear();
    for (const rowItem of rowItems) {
      const row = new StaffingNeedsRulesRowForm({
        validationHandler: this.formValidationHandler,
        element: rowItem,
      });
      this.rows.push(row);
    }
  }

  //   addRow(rowItem: any) {
  //     const row = new StaffingNeedsRulesRowForm({
  //       validationHandler: this.formValidationHandler,
  //       element: rowItem,
  //     });
  //     this.rows.push(row);
  //   }

  //   deleteRow(rowItem: any) {
  //     this.rows.removeAt(rowItem.AG_NODE_ID);
  //   }

  get rows(): FormArray {
    return <FormArray>this.controls.rows;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get staffingNeedsLocationGroupId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.staffingNeedsLocationGroupId;
  }

  get unit(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.unit;
  }

  get maxQuantity(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.maxQuantity;
  }

  get accountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountId;
  }
}
