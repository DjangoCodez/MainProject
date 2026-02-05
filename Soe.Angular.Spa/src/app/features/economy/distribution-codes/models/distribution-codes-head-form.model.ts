import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  DistributionCodeHeadDTO,
  DistributionCodePeriodDTO,
} from './distribution-codes.model';
import { DistributionCodePeriodForm } from './distribution-codes-period-form.model';
import { FormArray, ValidationErrors, ValidatorFn } from '@angular/forms';

interface IDistributionCodesForm {
  validationHandler: ValidationHandler;
  element: DistributionCodeHeadDTO | undefined;
}
export class DistributionCodeHeadForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IDistributionCodesForm) {
    super(validationHandler, {
      distributionCodeHeadId: new SoeNumberFormControl(
        element?.distributionCodeHeadId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      fromDate: new SoeDateFormControl(
        element?.fromDate || undefined,
        {},
        'common.validfrom'
      ),
      noOfPeriods: new SoeNumberFormControl(
        element?.noOfPeriods || 12,
        {},
        'economy.accounting.distributioncode.numberofperiods'
      ),
      typeId: new SoeSelectFormControl(
        element?.typeId || 1,
        {
          required: true,
        },
        'common.type'
      ),
      subType: new SoeSelectFormControl(
        element?.subType || undefined,
        {},
        'economy.accounting.distributioncode.subtype'
      ),
      parentId: new SoeSelectFormControl(element?.parentId || undefined),
      accountDimId: new SoeSelectFormControl(element?.accountDimId || ''),
      openingHoursId: new SoeSelectFormControl(
        element?.openingHoursId || undefined
      ),
      periods: new FormArray<DistributionCodePeriodForm>([]),
      sumPeriods: new SoeNumberFormControl(0.0),
      sumPercent: new SoeNumberFormControl(0.0),
      diff: new SoeNumberFormControl(0.0),
    });
    this.thisValidationHandler = validationHandler;
    this.customPeriodsPatchValue(element?.periods ?? []);
    this.periods.valueChanges.subscribe(() => this.updateSummary());
  }

  get distributionCodeHeadId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.distributionCodeHeadId;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get noOfPeriods(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.noOfPeriods;
  }

  get accountDimId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountDimId;
  }

  get typeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.typeId;
  }

  get subType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.subType;
  }

  get parentId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.parentId;
  }

  get periods(): FormArray<DistributionCodePeriodForm> {
    return <FormArray>this.controls.periods;
  }

  get sumPeriods(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sumPeriods;
  }

  get sumPercent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sumPercent;
  }

  get diff(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.diff;
  }

  customPeriodsPatchValue(distributionPeriods: DistributionCodePeriodDTO[]) {
    this.periods.clear({
      emitEvent: false,
    });
    if (distributionPeriods) {
      for (const distributionPeriod of distributionPeriods) {
        const row = new DistributionCodePeriodForm({
          validationHandler: this.thisValidationHandler,
          element: distributionPeriod,
        });
        this.periods.push(row, {
          emitEvent: false,
        });
      }
      this.periods.updateValueAndValidity();
    }
    return <DistributionCodePeriodDTO[]>this.periods.value;
  }

  customPatch(element: DistributionCodeHeadDTO) {
    this.reset(element);
    this.periods.clear({ emitEvent: false });
    element.periods.forEach(s => {
      this.periods.push(
        new DistributionCodePeriodForm({
          validationHandler: this.thisValidationHandler,
          element: s,
        }),
        { emitEvent: false }
      );
    });
    this.periods.markAsUntouched({ onlySelf: true });
    this.periods.markAsPristine({ onlySelf: true });
    this.periods.updateValueAndValidity();
  }

  addPeriod(row: Partial<DistributionCodePeriodDTO>): void {
    this.periods.push(
      new DistributionCodePeriodForm({
        validationHandler: this.thisValidationHandler,
        element: row as DistributionCodePeriodDTO,
      })
    );
  }

  public updateSummary(): void {
    let sum = 0;
    const _periods = this.periods.value;
    for (let i = 0; i < _periods.length; i++) {
      sum = sum + +_periods[i].percent;
    }
    let diff = sum - 100;
    if (Object.is(diff.round(2), -0)) diff = 0;

    this.patchValue({
      sumPeriods: _periods.length,
      sumPercent: +sum.toFixed(2),
      diff: +diff.toFixed(2),
    });
  }
}

export function addPeriodValidator(errorTerm: string): ValidatorFn {
  return (_form): ValidationErrors | null => {
    const periods = _form.get('sumPeriods')?.value;
    const diff = _form.get('diff')?.value;
    if (!periods || !diff) return null;
    if (+periods === 0 || (+periods > 0 && Number(diff).round(2) !== 100.0)) {
      const error: ValidationErrors = {};
      error[errorTerm] = true;
      return error;
    }

    return null;
  };
}
