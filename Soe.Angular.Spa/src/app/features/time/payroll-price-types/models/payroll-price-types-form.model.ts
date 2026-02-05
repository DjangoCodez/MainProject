import { FormArray } from '@angular/forms';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IPayrollPriceTypeDTO,
  IPayrollPriceTypePeriodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { PayrollPriceTypesPeriodsForm } from './payroll-price-types-periods-form.model';
import { arrayToFormArray } from '@shared/util/form-util';

interface IPayrollPriceTypesForm {
  validationHandler: ValidationHandler;
  element: IPayrollPriceTypeDTO | undefined;
}

export class PayrollPriceTypesForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IPayrollPriceTypesForm) {
    super(validationHandler, {
      payrollPriceTypeId: new SoeTextFormControl(
        element?.payrollPriceTypeId || 0,
        { isIdField: true }
      ),
      type: new SoeSelectFormControl(
        element?.type || undefined,
        { required: true },
        'common.type'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { maxLength: 10, required: true, minLength: 1 },
        'common.code'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      periods: arrayToFormArray(element?.periods || []),
      conditionAgeYears: new SoeNumberFormControl(
        element?.conditionAgeYears || 0,
        { minValue: 0, maxValue: 100 },
        'time.payroll.payrollpricetype.age'
      ),
      conditionEmployeedMonths: new SoeNumberFormControl(
        element?.conditionEmployeedMonths || 0,
        { minValue: 0, maxValue: 1200 },
        'time.payroll.payrollpricetype.employeed'
      ),
      conditionExperienceMonths: new SoeNumberFormControl(
        element?.conditionExperienceMonths || 0,
        { minValue: 0, maxValue: 1200 },
        'time.payroll.payrollpricetype.experience'
      ),
    });

    this.thisValidationHandler = validationHandler;
  }

  get payrollPriceTypePeriods(): FormArray<PayrollPriceTypesPeriodsForm> {
    return <FormArray>this.controls.periods;
  }

  customPatchValue(element: IPayrollPriceTypeDTO) {
    this.patchValue(element);
    this.patchPeriods(element.periods);
  }

  patchPeriods(periods: IPayrollPriceTypePeriodDTO[]) {
    this.payrollPriceTypePeriods.clear({ emitEvent: false });
    periods.forEach(period => {
      const periodForm = new PayrollPriceTypesPeriodsForm({
        validationHandler: this.thisValidationHandler,
        element: period,
      });
      periodForm.customPatchValue(period);

      this.payrollPriceTypePeriods.push(periodForm, { emitEvent: false });
    });
    this.payrollPriceTypePeriods.markAsUntouched({ onlySelf: true });
    this.payrollPriceTypePeriods.markAsPristine({ onlySelf: true });
    this.payrollPriceTypePeriods.updateValueAndValidity();
  }

  onDoCopy() {
    // Remember the rows
    const periods = this.payrollPriceTypePeriods.value;

    // Clear the form, we need to create new rows with correct form
    this.payrollPriceTypePeriods.clear({ emitEvent: false });

    periods.forEach((period: IPayrollPriceTypePeriodDTO) => {
      period.payrollPriceTypeId = 0;
      period.payrollPriceTypePeriodId = 0;

      // Create new form for each row
      const periodsForm = new PayrollPriceTypesPeriodsForm({
        validationHandler: this.thisValidationHandler,
        element: undefined,
      });
      periodsForm.patchValue(period);

      this.payrollPriceTypePeriods.push(periodsForm, { emitEvent: false });
    });
  }
}
