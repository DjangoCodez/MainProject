import { ValidationHandler } from '@shared/handlers';
import {
  ITimeScheduleTypeDTO,
  ITimeScheduleTypeFactorDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { FormArray } from '@angular/forms';
import { TimeScheduleTypeFactorForm } from './time-schedule-type-factor-form.model';
import { arrayToFormArray } from '@shared/util/form-util';

interface ITimeScheduleTypesForm {
  validationHandler: ValidationHandler;
  element: ITimeScheduleTypeDTO | undefined;
}
export class TimeScheduleTypeForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: ITimeScheduleTypesForm) {
    super(validationHandler, {
      timeScheduleTypeId: new SoeTextFormControl(
        element?.timeScheduleTypeId || 0,
        {
          isIdField: true,
        }
      ),
      isAll: new SoeCheckboxFormControl(
        element?.isAll || false,
        {},
        'common.all'
      ),
      code: new SoeTextFormControl(
        element?.code || '',
        { required: true, maxLength: 20, minLength: 1 },
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
      isBilagaJ: new SoeCheckboxFormControl(
        element?.isBilagaJ || false,
        {},
        'time.schedule.scheduletype.bilagaj'
      ),
      ignoreIfExtraShift: new SoeCheckboxFormControl(
        element?.ignoreIfExtraShift || false,
        {},
        'time.schedule.scheduletype.ignoreifextrashift'
      ),
      isNotScheduleTime: new SoeCheckboxFormControl(
        element?.isNotScheduleTime || false,
        {},
        'time.schedule.scheduletype.isnotscheduletime'
      ),
      useScheduleTimeFactor: new SoeCheckboxFormControl(
        element?.useScheduleTimeFactor || false,
        {},
        'time.schedule.scheduletype.usescheduletimefactor'
      ),
      showInTerminal: new SoeCheckboxFormControl(
        element?.showInTerminal || false,
        {},
        'time.schedule.scheduletype.showinterminal'
      ),
      timeDeviationCauseId: new SoeSelectFormControl(
        element?.timeDeviationCauseId || null,
        {},
        'time.schedule.scheduletype.replacewithdeviationcause'
      ),
      timeDeviationCauseName: new SoeTextFormControl(
        element?.timeDeviationCauseName || '',
        { maxLength: 100 },
        'time.schedule.scheduletype.replacewithdeviationcause'
      ),
      isActive: new SoeCheckboxFormControl(
        element?.isActive || true,
        {},
        'common.active'
      ),
      factors: arrayToFormArray(element?.factors || []),

      isCopy: new SoeCheckboxFormControl(false),
    });

    this.thisValidationHandler = validationHandler;
  }
  get isAll(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isAll;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get isNotScheduleTime(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isNotScheduleTime;
  }

  get useScheduleTimeFactor(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.useScheduleTimeFactor;
  }

  get factors(): FormArray<TimeScheduleTypeFactorForm> {
    return <FormArray>this.controls.factors;
  }

  customPatchValue(element: ITimeScheduleTypeDTO): void {
    this.reset();
    this.patchValue(element);
    this.patchFactors(element?.factors);
    this.markAsUntouched({ onlySelf: true });
    this.markAsPristine({ onlySelf: true });
  }

  patchFactors(factors: ITimeScheduleTypeFactorDTO[]): void {
    this.factors.clear({ emitEvent: false });
    if (factors && factors.length > 0) {
      factors.forEach(f => {
        const factorForm = new TimeScheduleTypeFactorForm({
          validationHandler: this.thisValidationHandler,
          element: f,
        });
        factorForm.patchValue(f);
        factorForm.setLength(); // Trigger calculation immediately
        this.factors.push(factorForm);
      });
    }
    this.factors.markAsUntouched({ onlySelf: true });
    this.factors.markAsPristine({ onlySelf: true });
    this.factors.updateValueAndValidity();
  }
}
