import { FormArray, FormControl } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DayOfWeek } from '@shared/models/generated-interfaces/ClientEnumerations';
import {
  DailyRecurrencePatternType,
  DailyRecurrencePatternWeekIndex,
  DailyRecurrenceRangeType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  DailyRecurrencePatternDTO,
  DailyRecurrenceRangeDTO,
} from '@shared/models/recurrence.model';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

interface IDailyRecurrencePatternForm {
  validationHandler: ValidationHandler;
  element: DailyRecurrencePatternDTO;
}

interface IDailyRecurrenceRangeForm {
  validationHandler: ValidationHandler;
  element: DailyRecurrenceRangeDTO;
}

export class DailyRecurrencePatternForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDailyRecurrencePatternForm) {
    super(validationHandler, {
      type: new SoeSelectFormControl(
        element?.type || DailyRecurrencePatternType.Daily
      ),
      interval: new SoeTextFormControl(element?.interval || 1),
      weekIndex: new SoeSelectFormControl(
        element?.weekIndex || DailyRecurrencePatternWeekIndex.First
      ),
      firstDayOfWeek: new SoeSelectFormControl(
        element?.firstDayOfWeek || DayOfWeek.Monday
      ),
      month: new SoeSelectFormControl(element?.month || 0),
      daysOfWeek: new SoeSelectFormControl(element?.daysOfWeek || []),
      dayOfMonth: new SoeTextFormControl(element?.dayOfMonth || 1),
      sysHolidayTypeIds: arrayToFormArray(element?.sysHolidayTypeIds || []),

      mondaySelected: new SoeCheckboxFormControl(false),
      tuesdaySelected: new SoeCheckboxFormControl(false),
      wednesdaySelected: new SoeCheckboxFormControl(false),
      thursdaySelected: new SoeCheckboxFormControl(false),
      fridaySelected: new SoeCheckboxFormControl(false),
      saturdaySelected: new SoeCheckboxFormControl(false),
      sundaySelected: new SoeCheckboxFormControl(false),
    });
  }

  get sysHolidayTypeIds(): FormArray<FormControl<number>> {
    return <FormArray<FormControl<number>>>this.controls.sysHolidayTypeIds;
  }

  get isPatternTypeNone(): boolean {
    return this.controls.type.value === DailyRecurrencePatternType.None;
  }

  get isPatternTypeDaily(): boolean {
    return this.controls.type.value === DailyRecurrencePatternType.Daily;
  }

  get isPatternTypeWeekly(): boolean {
    return this.controls.type.value === DailyRecurrencePatternType.Weekly;
  }

  get isPatternTypeAbsoluteMonthly(): boolean {
    return (
      this.controls.type.value === DailyRecurrencePatternType.AbsoluteMonthly
    );
  }

  get isPatternTypeRelativeMonthly(): boolean {
    return (
      this.controls.type.value === DailyRecurrencePatternType.RelativeMonthly
    );
  }

  get isPatternTypeAbsoluteYearly(): boolean {
    return (
      this.controls.type.value === DailyRecurrencePatternType.AbsoluteYearly
    );
  }

  get isPatternTypeRelativeYearly(): boolean {
    return (
      this.controls.type.value === DailyRecurrencePatternType.RelativeYearly
    );
  }

  get isPatternTypeSysHoliday(): boolean {
    return this.controls.type.value === DailyRecurrencePatternType.SysHoliday;
  }

  customPatchValue(element: DailyRecurrencePatternDTO) {
    this.reset(element);
    this.customSysHolidayTypeIdsPatchValue(element.sysHolidayTypeIds);
  }

  customSysHolidayTypeIdsPatchValue(sysHolidayTypeIds: number[]) {
    clearAndSetFormArray(sysHolidayTypeIds, this.sysHolidayTypeIds);
  }

  setDaysOfWeek() {
    const days: DayOfWeek[] = [];
    if (this.controls.mondaySelected.value) days.push(DayOfWeek.Monday);
    if (this.controls.tuesdaySelected.value) days.push(DayOfWeek.Tuesday);
    if (this.controls.wednesdaySelected.value) days.push(DayOfWeek.Wednesday);
    if (this.controls.thursdaySelected.value) days.push(DayOfWeek.Thursday);
    if (this.controls.fridaySelected.value) days.push(DayOfWeek.Friday);
    if (this.controls.saturdaySelected.value) days.push(DayOfWeek.Saturday);
    if (this.controls.sundaySelected.value) days.push(DayOfWeek.Sunday);
    this.patchValue({ daysOfWeek: days });
  }
}

export class DailyRecurrenceRangeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDailyRecurrenceRangeForm) {
    super(validationHandler, {
      type: new SoeSelectFormControl(
        element?.type || DailyRecurrenceRangeType.NoEnd
      ),
      startDate: new SoeDateFormControl(element?.startDate || undefined),
      endDate: new SoeDateFormControl(element?.endDate || undefined),
      numberOfOccurrences: new SoeTextFormControl(
        element?.numberOfOccurrences || 0
      ),
    });
  }

  get isRangeTypeEndDate(): boolean {
    return this.controls.type.value === DailyRecurrenceRangeType.EndDate;
  }

  get isRangeTypeNoEnd(): boolean {
    return this.controls.type.value === DailyRecurrenceRangeType.NoEnd;
  }

  get isRangeTypeNumbered(): boolean {
    return this.controls.type.value === DailyRecurrenceRangeType.Numbered;
  }
}

export class DailyRecurrencePatternDialogData implements DialogData {
  size?: DialogSize;
  title: string = '';

  pattern?: DailyRecurrencePatternDTO;
  range?: DailyRecurrenceRangeDTO;
  excludedDates: Date[] = [];
  date?: Date;
  hideRange: boolean = false;
}
