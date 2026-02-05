import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { arrayToFormArray } from '@shared/util/form-util';
import {
  FormArray,
  FormControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { ITimeHalfdayEditDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SoeTimeHalfdayType } from '@shared/models/generated-interfaces/Enumerations';

interface ITimeHalfdayForm {
  validationHandler: ValidationHandler;
  element: ITimeHalfdayEditDTO | undefined;
}

export class HalfdayForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeHalfdayForm) {
    super(validationHandler, {
      timeHalfdayId: new SoeTextFormControl(element?.timeHalfdayId || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 255, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description || '', {
        maxLength: 512,
      }),
      dayTypeId: new SoeSelectFormControl(
        element?.dayTypeId || undefined,
        {
          required: true,
        },
        'time.schedule.halfday.daytype'
      ),
      type: new SoeSelectFormControl(
        element?.type || undefined,
        {
          required: true,
        },
        'time.schedule.halfday.type'
      ),
      value: new SoeNumberFormControl(
        element?.value || 0,
        {
          required: true,
        },
        'common.value'
      ),
      timeCodeBreakIds: arrayToFormArray(element?.timeCodeBreakIds || []),
    });
  }

  get isTypePercent(): boolean {
    return (
      this.controls.type.value === SoeTimeHalfdayType.RelativeStartPercentage ||
      this.controls.type.value === SoeTimeHalfdayType.RelativeEndPercentage
    );
  }

  get isTypeMinutes(): boolean {
    return (
      this.controls.type.value === SoeTimeHalfdayType.RelativeStartValue ||
      this.controls.type.value === SoeTimeHalfdayType.RelativeEndValue
    );
  }

  get isTypeClockMinutes(): boolean {
    return this.controls.type.value === SoeTimeHalfdayType.ClockInMinutes;
  }

  get timeCodeBreakIds(): FormArray<FormControl<number>> {
    return <FormArray<FormControl<number>>>this.controls.timeCodeBreakIds;
  }

  customPatchValue(value: ITimeHalfdayEditDTO) {
    this.patchValue(value);
    this.timeCodeBreakIds.clear();
    value.timeCodeBreakIds.forEach((breakId: number) => {
      this.timeCodeBreakIds.push(
        new FormControl<number>(breakId) as FormControl<number>
      );
    });
    this.controls.timeCodeBreakIds.updateValueAndValidity({
      emitEvent: false,
    });
  }
}

export function createDayTypeValidator(
  errormessage: string,
  gridData: any
): ValidatorFn {
  return (form): ValidationErrors | null => {
    let duplicateNameOrDaytype = false;
    const name = form.get('name')?.value;
    const dayTypeId = form.get('dayTypeId')?.value;

    gridData.forEach((row: any) => {
      if (row.name === name || row.dayTypeId === dayTypeId)
        duplicateNameOrDaytype = true;
    });
    return duplicateNameOrDaytype ? { [errormessage]: true } : null;
  };
}
