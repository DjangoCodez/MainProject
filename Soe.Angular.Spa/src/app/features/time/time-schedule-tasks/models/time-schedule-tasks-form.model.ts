import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util'
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { FormArray, FormControl } from '@angular/forms';
import { ITimeScheduleTaskDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ITimeScheduleTaskForm {
  validationHandler: ValidationHandler;
  element: ITimeScheduleTaskDTO | undefined;
}

export class TimeScheduleTasksForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ITimeScheduleTaskForm) {
    super(validationHandler, {
      timeScheduleTaskId: new SoeTextFormControl(element?.timeScheduleTaskId, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(
        element?.name,
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(element?.description),
      shiftTypeId: new SoeSelectFormControl(element?.shiftTypeId || 0),
      timeScheduleTaskTypeId: new SoeSelectFormControl(
        element?.timeScheduleTaskTypeId,
        {}
      ),
      startTime: new SoeDateFormControl(element?.startTime),
      stopTime: new SoeDateFormControl(element?.stopTime),
      length: new SoeTextFormControl(element?.length ?? 0), // Length in minutes (stored in database)
      lengthFormatted: new SoeTextFormControl(element?.length), // Length formatted as H:MM
      minSplitLength: new SoeTextFormControl(element?.minSplitLength ?? 0), // Length in minutes (stored in database)
      minSplitLengthFormatted: new SoeTextFormControl(element?.minSplitLength), // Length formatted as H:MM
      nbrOfPersonsOne: new SoeNumberFormControl(1, {
        disabled: true,
      }),
      nbrOfPersons: new SoeNumberFormControl(element?.nbrOfPersons ?? 1),
      accountId: new SoeSelectFormControl(element?.accountId),
      onlyOneEmployee: new SoeCheckboxFormControl(
        element?.onlyOneEmployee ?? false
      ),
      dontAssignBreakLeftovers: new SoeCheckboxFormControl(
        element?.dontAssignBreakLeftovers ?? false
      ),
      allowOverlapping: new SoeCheckboxFormControl(
        element?.allowOverlapping ?? false
      ),
      isStaffingNeedsFrequency: new SoeCheckboxFormControl(
        element?.isStaffingNeedsFrequency ?? false
      ),

      // Recurrence
      startDate: new SoeDateFormControl(element?.startDate),
      stopDate: new SoeDateFormControl(element?.stopDate),
      nbrOfOccurrences: new SoeNumberFormControl(element?.nbrOfOccurrences),
      recurrencePattern: new SoeTextFormControl(element?.recurrencePattern),
      recurrencePatternDescription: new SoeTextFormControl(
        element?.recurrencePatternDescription
      ),
      recurrenceStartsOnDescription: new SoeTextFormControl(
        element?.recurrenceStartsOnDescription
      ),
      recurrenceEndsOnDescription: new SoeTextFormControl(
        element?.recurrenceEndsOnDescription
      ),
      excludedDates: arrayToFormArray(element?.excludedDates || []),
      excludedDatesDescription: new SoeTextFormControl(''),
    });
    this.thisValidationHandler = validationHandler;
  }

  get onlyOneEmployee(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.onlyOneEmployee;
  }

  get minSplitLengthFormatted(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.minSplitLengthFormatted;
  }

  get recurrencePatternDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.recurrencePatternDescription;
  }

  get excludedDates(): FormArray<FormControl<Date>> {
    return <FormArray>this.controls.excludedDates;
  }

  get excludedDatesDescription(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.excludedDatesDescription;
  }

  get hasRecurrencePattern(): boolean {
    return (
      this.controls.recurrencePattern.value &&
      this.controls.recurrencePattern.value !== '______'
    );
  }

  customPatchValue(element: ITimeScheduleTaskDTO) {
    this.patchValue(element);
    this.customExcludedDatesPatchValue(element.excludedDates);
  }

  customExcludedDatesPatchValue(excludedDates: Date[]) {
    clearAndSetFormArray(excludedDates, this.excludedDates);
  }

  setInitialFormattedValues() {
    this.controls.lengthFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.length.value)
    );

    this.controls.minSplitLengthFormatted.setValue(
      DateUtil.minutesToTimeSpan(this.controls.minSplitLength.value)
    );
  }

  calculateLength() {
    if (!this.controls.startTime.value || !this.controls.stopTime.value) return;

    // Make sure start and stop time is on the same day (or just over midnight)
    const diffDays = this.controls.stopTime.value.diffDays(
      this.controls.startTime.value
    );
    if (diffDays < 0) {
      this.controls.stopTime.setValue(
        this.controls.stopTime.value.addDays(Math.abs(diffDays))
      );
    } else if (diffDays > 1) {
      this.controls.stopTime.setValue(
        this.controls.stopTime.value.addDays(-Math.abs(diffDays))
      );
    }

    let newLength = this.controls.stopTime.value.diffMinutes(
      this.controls.startTime.value
    );
    if (newLength < 0) newLength = 0;

    const isModified = this.controls.length.value !== newLength;

    this.controls.length.setValue(newLength);
    this.controls.lengthFormatted.setValue(
      DateUtil.minutesToTimeSpan(newLength)
    );

    if (isModified) {
      this.controls.length.markAsDirty();
      this.controls.lengthFormatted.markAsDirty();
    }
  }

  formatLength(minLength: number) {
    this.controls.length.setValue(
      DateUtil.timeSpanToMinutes(this.controls.lengthFormatted.value)
    );

    if (this.controls.length.value < 0) this.controls.length.setValue(0);

    this.controls.length.setValue(
      Math.round(this.controls.length.value / minLength) * minLength
    );

    if (this.controls.length.value === 0)
      this.controls.minSplitLength.setValue(0);

    const newFormattedValue = DateUtil.minutesToTimeSpan(
      this.controls.length.value
    );

    if (newFormattedValue !== this.controls.lengthFormatted.value)
      this.controls.lengthFormatted.setValue(newFormattedValue);
  }

  formatMinSplitLength(minLength: number) {
    this.controls.minSplitLength.setValue(
      DateUtil.timeSpanToMinutes(this.controls.minSplitLengthFormatted.value)
    );

    if (this.controls.minSplitLength.value < 0)
      this.controls.minSplitLength.setValue(0);

    this.controls.minSplitLength.setValue(
      Math.round(this.controls.minSplitLength.value / minLength) * minLength
    );

    const newFormattedValue = DateUtil.minutesToTimeSpan(
      this.controls.minSplitLength.value
    );

    if (newFormattedValue !== this.controls.minSplitLengthFormatted.value)
      this.controls.minSplitLengthFormatted.setValue(newFormattedValue);
  }
}
