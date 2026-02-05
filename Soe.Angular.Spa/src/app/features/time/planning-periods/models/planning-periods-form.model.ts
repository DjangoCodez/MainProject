import { FormArray } from '@angular/forms';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ITimePeriodDTO,
  ITimePeriodHeadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimePeriodForm } from './pp-timeperiods-form.model';
import { arrayToFormArray } from '@shared/util/form-util';
interface IPlanningPeriodsForm {
  validationHandler: ValidationHandler;
  element: ITimePeriodHeadDTO;
}
export class PlanningPeriodsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IPlanningPeriodsForm) {
    super(validationHandler, {
      timePeriodHeadId: new SoeTextFormControl(element?.timePeriodHeadId || 0, {
        isIdField: true,
      }),
      timePeriodtype: new SoeTextFormControl(element?.timePeriodType || 3, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { isNameField: false, required: false, maxLength: 100, minLength: 1 },
        'common.description'
      ),
      childId: new SoeSelectFormControl(
        element?.childId || null,
        { required: false },

        'time.time.planningperiod.child'
      ),
      accountId: new SoeSelectFormControl(
        element?.accountId || undefined,
        { required: false },

        ''
      ),
      payrollProductDistributionRuleHeadId: new SoeSelectFormControl(
        element?.payrollProductDistributionRuleHeadId || null,
        { required: false },

        'time.time.planningperiod.planningperiods.distribution.rule'
      ),
      timePeriods: arrayToFormArray(element?.timePeriods || []),
    });
    this.thisValidationHandler = validationHandler;
  }

  set timePeriods(formArray: FormArray) {
    this.controls.timePeriods = formArray;
  }
  get timePeriods(): FormArray {
    return <FormArray>this.controls.timePeriods;
  }

  customPatchValue(element: ITimePeriodHeadDTO, isCopy: boolean = false) {
    this.patchValue(element);
    this.customPatchTimePeriods(element.timePeriods ?? [], isCopy);
  }

  customPatchTimePeriods(
    timePeriods: ITimePeriodDTO[],
    isCopy: boolean = false
  ): void {
    this.timePeriods.clear({ emitEvent: false });
    timePeriods.forEach(tp => {
      const dto = isCopy ? { ...tp, timePeriodId: 0 } : tp;
      const timePeriodForm = new TimePeriodForm({
        validationHandler: this.thisValidationHandler,
        element: dto,
      });
      this.timePeriods.push(timePeriodForm);
    });

    this.timePeriods.markAsUntouched({ onlySelf: true });
    this.timePeriods.markAsPristine({ onlySelf: true });
    this.timePeriods.updateValueAndValidity({ emitEvent: true });
  }
}
