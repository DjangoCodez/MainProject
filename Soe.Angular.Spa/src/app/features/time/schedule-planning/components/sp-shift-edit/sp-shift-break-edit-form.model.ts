import {
  SoeFormGroup,
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeNumberFormControl,
  SoeTextFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PlanningShiftBreakDTO } from '../../models/shift.model';
import { TermGroup_TimeSchedulePlanningShiftStartsOnDay } from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from '@shared/util/date-util';

interface ISpShiftBreakEditForm {
  validationHandler: ValidationHandler;
  element: PlanningShiftBreakDTO | undefined;
}

export class SpShiftBreakEditForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISpShiftBreakEditForm) {
    super(validationHandler, {
      breakId: new SoeNumberFormControl(element?.breakId || 0, {
        isIdField: true,
      }),
      tempBreakId: new SoeNumberFormControl(element?.tempBreakId || 0, {}),
      timeCodeId: new SoeNumberFormControl(element?.timeCodeId || 0, {
        disabled: !!element?.isIntersecting,
      }),
      actualStartDate: new SoeDateFormControl(
        element?.actualStartDate || undefined,
        { disabled: true }
      ),
      startTime: new SoeDateFormControl(element!.startTime, {
        disabled: !!element?.isIntersecting,
      }),
      startTimeStartsOn: new SoeSelectFormControl(
        element?.startTimeStartsOn ||
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay,
        {
          disabled: !!element?.isIntersecting,
        }
      ),
      stopTime: new SoeDateFormControl(element!.stopTime, {
        disabled: !!element?.isIntersecting,
      }),
      stopTimeStartsOn: new SoeSelectFormControl(
        element?.stopTimeStartsOn ||
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay,
        { disabled: true }
      ),
      belongsToPreviousDay: new SoeCheckboxFormControl(
        element?.belongsToPreviousDay || false
      ),
      belongsToNextDay: new SoeCheckboxFormControl(
        element?.belongsToNextDay || false
      ),
      minutes: new SoeNumberFormControl(element?.minutes || 0, {
        disabled: true,
      }),
      link: new SoeTextFormControl(element?.link || ''),
      isPreliminary: new SoeCheckboxFormControl(
        element?.isPreliminary || false
      ),
      isIntersecting: new SoeCheckboxFormControl(
        element?.isIntersecting || false
      ),
    });

    if (this.startTime && this.minutes > 0) {
      this.setBreakStopTime();
    }
  }

  toDTO(): PlanningShiftBreakDTO {
    const dto = new PlanningShiftBreakDTO();
    const raw = this.getRawValue();
    Object.keys(raw).forEach(key => {
      // Check if the key is safe to assign
      const descriptor = Object.getOwnPropertyDescriptor(
        Object.getPrototypeOf(dto),
        key
      );
      if (!descriptor || descriptor.writable || descriptor.set) {
        // @ts-ignore
        dto[key] = raw[key];
        if (key === 'minutes') {
          if (raw[key].toString().includes(':')) {
            dto[key] = DateUtil.timeSpanToMinutes(raw[key]);
          }
        }
      }
    });
    return dto;
  }

  get breakId(): number {
    return this.value.breakId || this.value.tempBreakId || 0;
  }

  get startTime(): Date {
    return this.value.startTime;
  }

  get stopTime(): Date {
    return this.value.stopTime;
  }

  get minutes(): number {
    return this.controls.minutes.value || 0;
  }

  get startsOnPreviousDay(): boolean {
    return (
      this.controls.startTimeStartsOn.value ===
      TermGroup_TimeSchedulePlanningShiftStartsOnDay.PreviousDay
    );
  }

  get startsOnCurrentDay(): boolean {
    return (
      this.controls.startTimeStartsOn.value ===
      TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
    );
  }

  get startsOnNextDay(): boolean {
    return (
      this.controls.startTimeStartsOn.value ===
      TermGroup_TimeSchedulePlanningShiftStartsOnDay.NextDay
    );
  }

  setBreakStopTime() {
    if (this.startTime) {
      this.controls.stopTime.setValue(this.startTime.addMinutes(this.minutes));
    } else {
      this.controls.stopTime.setValue(undefined);
    }
  }

  setBreakLength() {
    if (this.startTime && this.stopTime) {
      this.controls.minutes.setValue(this.stopTime.diffMinutes(this.startTime));
    }
  }

  setBreakStartTimeBasedOnStartsOn(
    startsOn: TermGroup_TimeSchedulePlanningShiftStartsOnDay
  ): void {
    // Previous day (starts day after current date)
    // Current day (starts on current date)
    // Next day (starts day before current date)

    switch (startsOn) {
      case TermGroup_TimeSchedulePlanningShiftStartsOnDay.PreviousDay:
        this.controls.startTime.setValue(
          this.controls.actualStartDate.value
            .mergeTime(this.controls.startTime.value)
            .addDays(-1)
        );
        break;
      case TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay:
        this.controls.startTime.setValue(
          this.controls.actualStartDate.value.mergeTime(
            this.controls.startTime.value
          )
        );
        break;
      case TermGroup_TimeSchedulePlanningShiftStartsOnDay.NextDay:
        this.controls.startTime.setValue(
          this.controls.actualStartDate.value
            .mergeTime(this.controls.startTime.value)
            .addDays(1)
        );
        break;
    }
  }

  setBreakStartsOnBasedOnStartTime(): void {
    if (
      this.controls.startTime.value.isSameDay(
        this.controls.actualStartDate.value
      )
    ) {
      this.controls.startTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
      );
    } else if (
      this.controls.startTime.value.isBeforeOnDay(
        this.controls.actualStartDate.value
      )
    ) {
      this.controls.startTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.PreviousDay
      );
    } else {
      this.controls.startTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.NextDay
      );
    }
  }

  setBreakBelongsToBasedOnStartTime(): void {
    if (
      this.controls.startTime.value.isSameDay(
        this.controls.actualStartDate.value
      )
    ) {
      this.controls.belongsToPreviousDay.setValue(false);
      this.controls.belongsToNextDay.setValue(false);
    } else if (
      this.controls.startTime.value.isBeforeOnDay(
        this.controls.actualStartDate.value
      )
    ) {
      this.controls.belongsToPreviousDay.setValue(false);
      this.controls.belongsToNextDay.setValue(true);
    } else {
      this.controls.belongsToPreviousDay.setValue(true);
      this.controls.belongsToNextDay.setValue(false);
    }
  }

  adjustBreakStopTimeConsideringMidnight(): void {
    let stopTimeChanged = false;

    if (this.startTime && this.stopTime) {
      // Handle over midnight
      if (this.stopTime.isBeforeOnMinute(this.startTime)) {
        this.controls.stopTime.setValue(this.stopTime.addDays(1));
        stopTimeChanged = true;
      }

      // Handle switch back (if end is set to less than start, and then back again)
      // So, if shift ends more than 24 hours after it starts, reduce end by 24 hours
      while (this.stopTime.isSameOrAfterOnMinute(this.startTime.addDays(1))) {
        this.controls.stopTime.setValue(this.stopTime.addDays(-1));
        stopTimeChanged = true;
      }
    }

    if (stopTimeChanged)
      this.setBreakStopTimeStartsOnBasedOnActualStopTimeAndDate();
  }

  setBreakStopTimeStartsOnBasedOnActualStopTimeAndDate(): void {
    if (this.stopTime?.isSameDay(this.controls.actualStartDate.value)) {
      this.controls.stopTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
      );
    } else if (
      this.stopTime?.isBeforeOnDay(this.controls.actualStartDate.value)
    ) {
      this.controls.stopTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.PreviousDay
      );
    } else {
      this.controls.stopTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.NextDay
      );
    }
  }

  async validateBreakStartTimeBoundary(
    shiftStart: Date,
    shiftStop: Date
  ): Promise<{ startTimeChanged: boolean; stopTimeChanged: boolean }> {
    if (!this.controls.startTime.value || !this.controls.stopTime.value) {
      return { startTimeChanged: false, stopTimeChanged: false };
    }

    let startTimeChanged = false;
    let stopTimeChanged = false;

    // Remember break length to be able to keep it when adjusting start time
    const minutesValue = this.controls.minutes.value;
    const breakLength =
      typeof minutesValue === 'string' && minutesValue.includes(':')
        ? DateUtil.timeSpanToMinutes(minutesValue)
        : minutesValue;

    if (
      this.controls.startTime.value < shiftStart &&
      this.controls.startTime.value.addDays(1) <= shiftStop &&
      (this.startsOnCurrentDay || this.startsOnPreviousDay)
    ) {
      // If break start time is before shift start time, and can be moved to next day within shift time, do that
      this.controls.startTime.setValue(
        this.controls.startTime.value.addDays(1)
      );
      this.controls.stopTime.setValue(
        this.controls.startTime.value.addMinutes(breakLength)
      );
      startTimeChanged = true;
      stopTimeChanged = true;

      if (this.startsOnCurrentDay) {
        this.controls.startTimeStartsOn.setValue(
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.NextDay
        );
      } else if (this.startsOnPreviousDay) {
        this.controls.startTimeStartsOn.setValue(
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
        );
      }
    } else if (
      this.controls.startTime.value > shiftStop &&
      this.controls.startTime.value.addDays(-1) >= shiftStart &&
      (this.startsOnCurrentDay || this.startsOnNextDay)
    ) {
      // If break start time is after shift stop time, and can be moved to previous day within shift time, do that
      this.controls.startTime.setValue(
        this.controls.startTime.value.addDays(-1)
      );
      this.controls.stopTime.setValue(
        this.controls.startTime.value.addMinutes(breakLength)
      );
      startTimeChanged = true;
      stopTimeChanged = true;

      if (this.startsOnCurrentDay) {
        this.controls.startTimeStartsOn.setValue(
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.PreviousDay
        );
      } else if (this.startsOnNextDay) {
        this.controls.startTimeStartsOn.setValue(
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
        );
      }
    }

    // Ensure break start time is within shift start and stop time
    let breakStartTime = new Date(this.controls.startTime.value);
    if (breakStartTime < shiftStart) {
      breakStartTime = new Date(shiftStart);
      startTimeChanged = true;
    } else if (breakStartTime > shiftStop) {
      breakStartTime = new Date(shiftStop);
      startTimeChanged = true;
    }

    if (startTimeChanged) {
      this.controls.startTime.setValue(undefined, {
        emitEvent: false,
      });
      await new Promise<void>(resolve => {
        setTimeout(() => {
          this.controls.startTime.setValue(breakStartTime, {
            emitEvent: true,
          });
          this.setBreakStartsOnBasedOnStartTime();
          resolve();
        }, 0);
      });
    }

    // Ensure break stop time is not before start time
    if (
      this.controls.stopTime.value &&
      this.controls.stopTime.value < breakStartTime
    ) {
      stopTimeChanged = true;
      this.controls.stopTime.setValue(new Date(breakStartTime));
    }

    return { startTimeChanged, stopTimeChanged };
  }

  async validateBreakStopTimeBoundary(
    shiftStart: Date,
    shiftStop: Date
  ): Promise<{ stopTimeChanged: boolean; stopTimeBeforeStartTime: boolean }> {
    if (!this.controls.startTime.value || !this.controls.stopTime.value) {
      return { stopTimeChanged: false, stopTimeBeforeStartTime: false };
    }

    let stopTimeChanged = false;
    let stopTimeBeforeStartTime = false;

    // Ensure break stop time is within shift start and stop time
    let breakStopTime = new Date(this.controls.stopTime.value);
    if (breakStopTime < shiftStart) {
      breakStopTime = new Date(shiftStart);
      stopTimeChanged = true;
    } else if (breakStopTime > shiftStop) {
      breakStopTime = new Date(shiftStop);
      stopTimeChanged = true;
    }

    // Ensure break stop time is not before start time
    if (
      this.controls.startTime.value &&
      this.controls.startTime.value > breakStopTime
    ) {
      breakStopTime = new Date(this.controls.startTime.value);
      stopTimeChanged = true;
      stopTimeBeforeStartTime = true;
    }

    if (stopTimeChanged) {
      this.controls.stopTime.setValue(undefined, {
        emitEvent: false,
      });
      await new Promise<void>(resolve => {
        setTimeout(() => {
          this.controls.stopTime.setValue(new Date(breakStopTime), {
            emitEvent: true,
          });
          resolve();
        }, 0);
      });
      this.adjustBreakStopTimeConsideringMidnight();
    }

    return { stopTimeChanged, stopTimeBeforeStartTime };
  }
}
