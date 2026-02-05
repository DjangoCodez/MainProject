import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  PlanningShiftBreakDTO,
  PlanningShiftDayDTO,
  PlanningShiftDTO,
} from '../../models/shift.model';
import { FormArray } from '@angular/forms';
import { SpShiftEditForm } from '../../components/sp-shift-edit/sp-shift-edit-form.model';
import { TermGroup_TimeScheduleTemplateBlockType } from '@shared/models/generated-interfaces/Enumerations';
import { ShiftUtil } from '../../util/shift-util';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { SpShiftBreakEditForm } from '../../components/sp-shift-edit/sp-shift-break-edit-form.model';

interface ISpShiftDialogFormElement {
  day: PlanningShiftDayDTO;
}

export class SpShiftDialogForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  summaryRow1 = ''; // Time range, total shift length (total break length)
  summaryRow2 = ''; // Total factor length
  summaryRow3 = ''; // Gross time
  summaryRow4 = ''; // Cost

  constructor(args: {
    validationHandler: ValidationHandler;
    element: ISpShiftDialogFormElement | undefined;
  }) {
    super(args.validationHandler, {
      date: new SoeDateFormControl(args.element?.day.date || undefined, {
        disabled: true,
      }),
      employeeId: new SoeNumberFormControl(args.element?.day.employeeId || 0),
      employeeName: new SoeTextFormControl('', {
        disabled: true,
      }),
      shifts: new FormArray<SpShiftEditForm>(
        args.element?.day.shifts.map(
          elem =>
            new SpShiftEditForm({
              validationHandler: args.validationHandler,
              element: elem,
            })
        ) ?? []
      ),
      deletedShifts: new FormArray<SpShiftEditForm>([]),
    });

    this.thisValidationHandler = args.validationHandler;
    this.patchShifts(args.element?.day.shifts ?? []);
  }

  toShiftsDTOs(): PlanningShiftDTO[] {
    return this.shifts.controls.map(shiftForm => shiftForm.toDTO());
  }

  toDeletedShiftsDTOs(): PlanningShiftDTO[] {
    return this.deletedShifts.controls.map(shiftForm => shiftForm.toDTO());
  }

  get date(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.date;
  }

  get isSaturday(): boolean {
    return this.date.value?.isSaturday() ?? false;
  }

  get isSunday(): boolean {
    return this.date.value?.isSunday() ?? false;
  }

  get employeeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.employeeId;
  }

  get employeeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeName;
  }

  get maxTempTimeScheduleTemplateBlockId(): number {
    return this.shifts.controls.reduce((max, shift) => {
      const id = shift.value.tempTimeScheduleTemplateBlockId ?? 0;
      return id > max ? id : max;
    }, 0);
  }

  get shifts(): FormArray<SpShiftEditForm> {
    return <FormArray>this.controls.shifts;
  }

  get deletedShifts(): FormArray<SpShiftEditForm> {
    return <FormArray>this.controls.deletedShifts;
  }

  get firstShift(): SpShiftEditForm | undefined {
    return this.shifts.at(0);
  }

  get lastShift(): SpShiftEditForm | undefined {
    return this.shifts.at(this.shifts.length - 1);
  }

  get firstShiftStartTime(): Date {
    return this.firstShift?.actualStartTime || this.date.value.beginningOfDay();
  }

  get lastShiftStopTime(): Date {
    return this.lastShift?.actualStopTime || this.date.value.beginningOfDay();
  }

  get dayStartStopTime(): string {
    return `${this.firstShiftStartTime.toFormattedTime()}-${this.lastShiftStopTime.toFormattedTime()}`;
  }

  get dayShiftsLengthExcludingBreaks(): number {
    let length = 0;
    for (const shift of this.shifts.controls) {
      if (!shift.value.isBreak) {
        length += ShiftUtil.shiftLengthExcludingBreaks(
          shift.value,
          this.shifts.controls.map(s => s.value)
        );
      }
    }
    return length;
  }

  get dayBreaksLength(): number {
    let length = 0;
    for (const shift of this.shifts.controls) {
      length += ShiftUtil.shiftBreaksLength(
        shift.value,
        this.shifts.controls.map(s => s.value)
      );
    }
    return length;
  }

  get dayFactorMinutes(): number {
    let length = 0;
    for (const shift of this.shifts.controls) {
      // Since factors are not part of form, get them from property
      shift.value.timeScheduleTypeFactors = shift.timeScheduleTypeFactors;
      length +=
        ShiftUtil.getFactorMinutesWithinShift(
          shift.value,
          this.shifts.controls.map(s => s.value)
        ) || 0;
    }
    return length;
  }

  get dayGrossTime(): number {
    let length = 0;
    for (const shift of this.shifts.controls) {
      length += shift.value.grossTime || 0;
    }
    return length;
  }

  get dayCost(): number {
    let cost = 0;
    for (const shift of this.shifts.controls) {
      cost += shift.value.totalCost || 0;
    }
    return cost;
  }

  get dayCostIncEmpTaxAndSuppCharge(): number {
    let cost = 0;
    for (const shift of this.shifts.controls) {
      cost += shift.value.totalCostIncEmpTaxAndSuppCharge || 0;
    }
    return cost;
  }

  setSummaryRows(
    setGrossTime: boolean,
    setCost: boolean,
    setCostIncEmpTaxAndSuppCharge: boolean
  ) {
    this.summaryRow1 = `${this.dayStartStopTime}, ${DateUtil.minutesToTimeSpan(this.dayShiftsLengthExcludingBreaks)} (${DateUtil.minutesToTimeSpan(this.dayBreaksLength)})`;

    this.summaryRow2 = ``;
    const factorMinutes = this.dayFactorMinutes;
    if (factorMinutes !== 0)
      this.summaryRow2 = `${DateUtil.minutesToTimeSpan(factorMinutes)}`;

    this.summaryRow3 = ``;
    const grossTime = setGrossTime ? this.dayGrossTime : 0;
    if (grossTime !== 0)
      this.summaryRow3 = `${DateUtil.minutesToTimeSpan(grossTime)}`;

    this.summaryRow4 = '';
    if (setCost || setCostIncEmpTaxAndSuppCharge) {
      let cost = 0;
      if (!setCostIncEmpTaxAndSuppCharge) {
        cost = this.dayCost;
      } else {
        cost = this.dayCostIncEmpTaxAndSuppCharge;
      }
      if (cost !== 0) this.summaryRow4 = `${NumberUtil.formatDecimal(cost, 0)}`;
    }
  }

  patchShifts(shifts: PlanningShiftDTO[]) {
    this.shifts.clear();

    for (const shift of shifts) {
      this.shifts.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftEditForm, {
          validationHandler: this.thisValidationHandler,
          element: shift,
        }),
        {
          emitEvent: false,
        }
      );
    }
    this.shifts.updateValueAndValidity();
  }

  addShift(shiftForm: SpShiftEditForm) {
    shiftForm.patchValue(
      {
        tempTimeScheduleTemplateBlockId:
          this.maxTempTimeScheduleTemplateBlockId + 1,
      },
      { emitEvent: false }
    );
    this.shifts.push(shiftForm, { emitEvent: false });
    this.shifts.updateValueAndValidity();
  }

  deleteShift(shiftForm: SpShiftEditForm) {
    // If shift has been saved, store it in deleted collection
    // Must be passed when saving
    if (
      !shiftForm.value.isBreak &&
      shiftForm.value.timeScheduleTemplateBlockId
    ) {
      const newTime = new Date(shiftForm.value.startTime);

      // Clear some values, because if shift is last remaining on day, it will be kept as zero day placeholder
      shiftForm.patchValue(
        {
          type: TermGroup_TimeScheduleTemplateBlockType.Schedule,
          shiftTypeId: undefined,
          startTime: newTime,
          actualStartTime: newTime,
          stopTime: newTime,
          actualStopTime: newTime,
          isModified: true,
          isDeleted: true,
        },
        { emitEvent: false }
      );

      this.deletedShifts.push(shiftForm, { emitEvent: false });

      // TODO: Tasks not implemented
      //this.setConnectedTasksToDeleted(shift);
    }

    this.shifts.removeAt(this.shifts.controls.indexOf(shiftForm), {
      emitEvent: true,
    });
    this.shifts.updateValueAndValidity();
  }

  getShiftEditFormById(
    timeScheduleTemplateBlockId: number,
    tempTimeScheduleTemplateBlockId: number
  ): SpShiftEditForm {
    console.log(
      'Getting shift form by id:',
      timeScheduleTemplateBlockId,
      tempTimeScheduleTemplateBlockId,
      this.shifts.controls
    );
    return this.shifts.controls.find(s =>
      timeScheduleTemplateBlockId !== 0
        ? s.value.timeScheduleTemplateBlockId === timeScheduleTemplateBlockId
        : s.value.tempTimeScheduleTemplateBlockId ===
          tempTimeScheduleTemplateBlockId
    ) as SpShiftEditForm;
  }

  createShiftBreakFormFromDTO(
    brk: PlanningShiftBreakDTO
  ): SpShiftBreakEditForm {
    return new SpShiftBreakEditForm({
      validationHandler: this.thisValidationHandler,
      element: brk,
    });
  }
}
