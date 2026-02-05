import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  PlanningShiftBreakDTO,
  PlanningShiftDTO,
} from '../../models/shift.model';
import {
  TermGroup_TimeSchedulePlanningShiftStartsOnDay,
  TermGroup_TimeScheduleTemplateBlockType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ShiftUtil } from '../../util/shift-util';
import { FormArray } from '@angular/forms';
import { SpShiftBreakEditForm } from './sp-shift-break-edit-form.model';
import { ITimeScheduleTypeFactorSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface ISpShiftEditForm {
  validationHandler: ValidationHandler;
  element: PlanningShiftDTO | undefined;
}
export class SpShiftEditForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  timeScheduleTypeFactors: ITimeScheduleTypeFactorSmallDTO[];

  constructor({ validationHandler, element }: ISpShiftEditForm) {
    super(validationHandler, {
      timeScheduleTemplateBlockId: new SoeNumberFormControl(
        element?.timeScheduleTemplateBlockId || 0,
        { isIdField: true }
      ),
      tempTimeScheduleTemplateBlockId: new SoeNumberFormControl(
        element?.tempTimeScheduleTemplateBlockId || 0
      ),
      type: new SoeSelectFormControl(
        element?.type || TermGroup_TimeScheduleTemplateBlockType.Schedule
      ),
      actualStartDate: new SoeDateFormControl(
        element?.actualStartDate || undefined,
        { disabled: true }
      ),
      employeeId: new SoeNumberFormControl(element?.employeeId || 0),
      employeeName: new SoeTextFormControl(element?.employeeName || '', {
        disabled: true,
      }),
      startTime: new SoeDateFormControl(element?.startTime || undefined),
      startTimeStartsOn: new SoeSelectFormControl(
        element?.startTimeStartsOn ||
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
      ),
      actualStartTime: new SoeDateFormControl(
        element?.actualStartTime || undefined
      ),
      stopTime: new SoeDateFormControl(element?.stopTime || undefined),
      stopTimeStartsOn: new SoeSelectFormControl(
        element?.stopTimeStartsOn ||
          TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay,
        { disabled: true }
      ),
      actualStopTime: new SoeDateFormControl(
        element?.actualStopTime || undefined
      ),
      belongsToPreviousDay: new SoeCheckboxFormControl(
        element?.belongsToPreviousDay || false
      ),
      belongsToNextDay: new SoeCheckboxFormControl(
        element?.belongsToNextDay || false
      ),
      shiftLength: new SoeNumberFormControl(element?.shiftLength || 0, {
        disabled: true,
      }),
      shiftTypeId: new SoeNumberFormControl(element?.shiftTypeId || 0),
      shiftTypeName: new SoeTextFormControl(element?.shiftTypeName || ''),
      timeScheduleTypeId: new SoeNumberFormControl(
        element?.timeScheduleTypeId || 0
      ),
      timeScheduleTypeCode: new SoeTextFormControl(
        element?.timeScheduleTypeCode || ''
      ),
      timeScheduleTypeName: new SoeTextFormControl(
        element?.timeScheduleTypeName || ''
      ),
      timeScheduleTypeIsNotScheduleTime: new SoeCheckboxFormControl(
        element?.timeScheduleTypeIsNotScheduleTime || false
      ),
      description: new SoeTextFormControl(element?.description || '', {
        maxLength: 255,
      }),
      isPreliminary: new SoeCheckboxFormControl(
        element?.isPreliminary || false
      ),
      extraShift: new SoeCheckboxFormControl(element?.extraShift || false),
      substituteShift: new SoeCheckboxFormControl(
        element?.substituteShift || false
      ),
      grossTime: new SoeNumberFormControl(element?.grossTime || 0),
      totalCost: new SoeNumberFormControl(element?.totalCost || 0),
      totalCostIncEmpTaxAndSuppCharge: new SoeNumberFormControl(
        element?.totalCostIncEmpTaxAndSuppCharge || 0
      ),
      breaks: new FormArray<SpShiftBreakEditForm>(
        element?.breaks.map(
          elem => new SpShiftBreakEditForm({ validationHandler, element: elem })
        ) ?? []
      ),

      shiftTypeColor: new SoeTextFormControl(
        element?.shiftTypeColor || '#ffffff'
      ),
      textColor: new SoeTextFormControl(element?.textColor || '#1e1e1e'),

      // Extensions
      isCreatedAsFirstOnDay: new SoeCheckboxFormControl(false),
    });

    this.thisValidationHandler = validationHandler;

    this.timeScheduleTypeFactors = element?.timeScheduleTypeFactors ?? [];
  }

  toDTO(): PlanningShiftDTO {
    const dto = new PlanningShiftDTO();
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
        if (key === 'breaks') {
          dto.breaks = this.breaks.controls.map(brk => {
            return brk.toDTO();
          });
        }
      }
    });
    return dto;
  }

  get isBooking(): boolean {
    return ShiftUtil.isBooking(this.value);
  }

  get isStandby(): boolean {
    return ShiftUtil.isStandby(this.value);
  }

  get isOnDuty(): boolean {
    return ShiftUtil.isOnDuty(this.value);
  }

  get extraShift(): boolean {
    return this.value.extraShift;
  }

  get substituteShift(): boolean {
    return this.value.substituteShift;
  }

  get actualStartTime(): Date | undefined {
    return this.value.actualStartTime;
  }

  get actualStopTime(): Date | undefined {
    return this.value.actualStopTime;
  }

  get breaks(): FormArray<SpShiftBreakEditForm> {
    return <FormArray>this.controls.breaks;
  }

  get shiftLength(): number {
    return ShiftUtil.shiftLength(this.value);
  }

  get shiftTypeColor(): string {
    return this.value.shiftTypeColor; // ShiftUtil.shiftTypeColor(this.value);
  }

  get textColor(): string {
    return this.value.textColor; // ShiftUtil.shiftTypeTextColor(this.value);
  }

  get isWholeDay(): boolean {
    return ShiftUtil.isWholeDay(this.value);
  }

  setShiftLength(): void {
    if (this.actualStartTime && this.actualStopTime) {
      this.controls.shiftLength.setValue(
        this.actualStopTime.diffMinutes(this.actualStartTime)
      );
    }
  }

  setStartStopTimeFromActual(): void {
    if (this.actualStartTime)
      this.controls.startTime.setValue(this.actualStartTime.beginningOfDay());
    if (this.actualStopTime)
      this.controls.stopTime.setValue(this.actualStopTime.endOfDay());
  }

  setStartTimeBasedOnStartsOn(
    startsOn: TermGroup_TimeSchedulePlanningShiftStartsOnDay
  ): void {
    // Previous day (starts day after current date)
    // Current day (starts on current date)
    // Next day (starts day before current date)

    switch (startsOn) {
      case TermGroup_TimeSchedulePlanningShiftStartsOnDay.PreviousDay:
        this.controls.actualStartTime.setValue(
          this.controls.actualStartDate.value
            .mergeTime(this.controls.actualStartTime.value)
            .addDays(-1)
        );
        break;
      case TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay:
        this.controls.actualStartTime.setValue(
          this.controls.actualStartDate.value.mergeTime(
            this.controls.actualStartTime.value
          )
        );
        break;
      case TermGroup_TimeSchedulePlanningShiftStartsOnDay.NextDay:
        this.controls.actualStartTime.setValue(
          this.controls.actualStartDate.value
            .mergeTime(this.controls.actualStartTime.value)
            .addDays(1)
        );
        break;
    }
  }

  setBelongsToBasedOnStartTime(): void {
    if (
      this.controls.actualStartTime.value.isSameDay(
        this.controls.actualStartDate.value
      )
    ) {
      this.controls.belongsToPreviousDay.setValue(false);
      this.controls.belongsToNextDay.setValue(false);
    } else if (
      this.controls.actualStartTime.value.isBeforeOnDay(
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

  setStopTimeBasedOnStartAndLength(shiftLength: number): void {
    this.controls.actualStopTime.setValue(
      new Date(this.controls.actualStartTime.value).addMinutes(shiftLength)
    );
  }

  adjustStopTimeConsideringMidnight(): void {
    if (this.actualStartTime && this.actualStopTime) {
      // Handle over midnight
      if (this.actualStopTime.isBeforeOnMinute(this.actualStartTime))
        this.controls.actualStopTime.setValue(this.actualStopTime.addDays(1));
      // Handle switch back (if end is set to less than start, and then back again)
      // So, if shift ends more than 24 hours after it starts, reduce end by 24 hours
      while (
        this.actualStopTime.isSameOrAfterOnMinute(
          this.actualStartTime.addDays(1)
        )
      ) {
        this.controls.actualStopTime.setValue(this.actualStopTime.addDays(-1));
      }

      // Make it possible to have a 24 hour standby shift
      if (
        this.isStandby &&
        this.actualStartTime.isSameMinute(this.actualStopTime)
      )
        this.controls.actualStopTime.setValue(this.actualStopTime.addDays(1));

      this.setStartStopTimeFromActual();
    }
  }

  setStopTimeStartsOnBasedOnActualStopTimeAndDate(): void {
    if (this.actualStopTime?.isSameDay(this.controls.actualStartDate.value)) {
      this.controls.stopTimeStartsOn.setValue(
        TermGroup_TimeSchedulePlanningShiftStartsOnDay.CurrentDay
      );
    } else if (
      this.actualStopTime?.isBeforeOnDay(this.controls.actualStartDate.value)
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

  createShiftBreakFormFromDTO(
    brk: PlanningShiftBreakDTO
  ): SpShiftBreakEditForm {
    return new SpShiftBreakEditForm({
      validationHandler: this.thisValidationHandler,
      element: brk,
    });
  }

  addBreak(breakForm: SpShiftBreakEditForm): number {
    this.breaks.push(breakForm, { emitEvent: false });
    this.breaks.updateValueAndValidity();

    return this.breaks.length - 1;
  }

  deleteBreak(index: number): void {
    if (index >= 0) {
      this.breaks.removeAt(index);
      this.breaks.updateValueAndValidity();
    }
  }
}
