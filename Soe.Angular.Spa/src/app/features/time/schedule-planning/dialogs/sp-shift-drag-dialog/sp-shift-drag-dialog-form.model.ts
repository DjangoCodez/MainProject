import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeRadioFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PlanningShiftDTO } from '../../models/shift.model';
import { FormArray } from '@angular/forms';
import { SpShiftDragDialogShiftForm } from './sp-shift-drag-dialog-shift-form.model';
import { DragShiftAction } from '@shared/models/generated-interfaces/Enumerations';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { signal } from '@angular/core';
import { DateUtil } from '@shared/util/date-util';
import { ShiftUtil } from '../../util/shift-util';

export interface ISpShiftDragDialogFormElement {
  sourceDate: Date;
  sourceEmployee: PlanningEmployeeDTO;
  sourceShifts: PlanningShiftDTO[];
  targetDate: Date;
  targetEmployee: PlanningEmployeeDTO;
  targetShifts: PlanningShiftDTO[];
  action: DragShiftAction;
  moveOffsetDays?: number;
}

export class SpShiftDragDialogForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  private hiddenEmployeeId = 0;

  // Stored as DTO's to be able to use their extension methods
  private sourceEmployee!: PlanningEmployeeDTO;
  private targetEmployee!: PlanningEmployeeDTO;
  sourceShifts: PlanningShiftDTO[] = [];
  onDutyShifts: PlanningShiftDTO[] = [];
  targetShifts: PlanningShiftDTO[] = [];
  targetDates: Date[] = [];

  targetEmployeeIsFullyUnavailable = signal(false);
  targetEmployeeIsPartiallyUnavailable = signal(false);

  sourceShiftIsExtra = signal(false);
  sourceShiftIsSubstitute = signal(false);

  constructor(args: {
    validationHandler: ValidationHandler;
    element: ISpShiftDragDialogFormElement | undefined;
  }) {
    super(args.validationHandler, {
      sourceDate: new SoeDateFormControl(args.element?.sourceDate || undefined),
      sourceEmployeeId: new SoeNumberFormControl(
        args.element?.sourceEmployee.employeeId || 0
      ),
      sourceEmployeeName: new SoeTextFormControl(
        args.element?.sourceEmployee.name || '',
        {
          disabled: true,
        }
      ),
      sourceEmployeePostId: new SoeNumberFormControl(0),
      sourceShifts: new FormArray<SpShiftDragDialogShiftForm>(
        args.element?.sourceShifts.map(
          shift =>
            new SpShiftDragDialogShiftForm({
              validationHandler: args.validationHandler,
              element: shift,
            })
        ) || []
      ),
      onDutyShifts: new FormArray<SpShiftDragDialogShiftForm>([]),
      targetDate: new SoeDateFormControl(args.element?.targetDate || undefined),
      targetEmployeeId: new SoeNumberFormControl(
        args.element?.targetEmployee.employeeId || 0
      ),
      targetEmployeeName: new SoeTextFormControl(
        args.element?.targetEmployee.name || '',
        {
          disabled: true,
        }
      ),
      targetEmployeePostId: new SoeNumberFormControl(0),
      targetShifts: new FormArray<SpShiftDragDialogShiftForm>(
        args.element?.targetShifts.map(
          shift =>
            new SpShiftDragDialogShiftForm({
              validationHandler: args.validationHandler,
              element: shift,
            })
        ) || []
      ),
      action: new SoeRadioFormControl<DragShiftAction>(
        args.element?.action || DragShiftAction.Move
      ),
      moveOffsetDays: new SoeNumberFormControl(
        args.element?.moveOffsetDays || 0
      ),
      timeDeviationCauseId: new SoeNumberFormControl(
        0,
        {},
        'time.schedule.planning.dragshift.deviationcause'
      ),
      employeeChildId: new SoeNumberFormControl(
        0,
        {},
        'time.schedule.planning.dragshift.child'
      ),
      isWholeDayAbsence: new SoeCheckboxFormControl(false),
      includeOnDutyShifts: new SoeCheckboxFormControl(false),
    });

    this.thisValidationHandler = args.validationHandler;

    // If whole day absence is selected, all shifts will be moved including any on duty shifts.
    // No checks will be done on server side.
    // Therefore we set the includeOnDutyShifts checkbox and disables it, to make it show correct behaviour.
    this.controls.isWholeDayAbsence.valueChanges.subscribe(value => {
      if (value) {
        this.patchValue({ includeOnDutyShifts: true }, { emitEvent: false });
        this.controls.includeOnDutyShifts.disable({ emitEvent: false });
      } else {
        this.controls.includeOnDutyShifts.enable({ emitEvent: false });
      }
    });
  }

  get sourceDate(): Date {
    return <Date>this.controls.sourceDate.value;
  }

  get multipleSourceDates(): boolean {
    return (
      this.sourceShifts.length > 1 &&
      this.sourceShifts.some(s => !s.actualStartDate.isSameDay(this.sourceDate))
    );
  }

  get multipleSourceGuids(): boolean {
    const uniqueLinks = new Set(this.sourceShifts.map(s => s.link));
    return uniqueLinks.size > 1;
  }

  get sourceEmployeeId(): number {
    return <number>this.controls.sourceEmployeeId.value;
  }

  get sourceEmployeeName(): string {
    return <string>this.controls.sourceEmployeeName.value;
  }

  get sourceShiftForms(): FormArray<SpShiftDragDialogShiftForm> {
    return <FormArray>this.controls.sourceShifts;
  }

  get onDutyShiftForms(): FormArray<SpShiftDragDialogShiftForm> {
    return <FormArray>this.controls.onDutyShifts;
  }

  get firstSourceShift(): PlanningShiftDTO | undefined {
    return this.sourceShifts.length > 0 ? this.sourceShifts[0] : undefined;
  }

  get targetDate(): Date {
    return <Date>this.controls.targetDate.value;
  }

  get targetEmployeeId(): number {
    return <number>this.controls.targetEmployeeId.value;
  }

  get targetEmployeeName(): string {
    return <string>this.controls.targetEmployeeName.value;
  }

  get targetShiftForms(): FormArray<SpShiftDragDialogShiftForm> {
    return <FormArray>this.controls.targetShifts;
  }

  get targetShiftsOnDuty(): PlanningShiftDTO[] {
    return this.targetShifts.filter(s => s.isOnDuty);
  }

  get firstTargetShift(): PlanningShiftDTO | undefined {
    return this.targetShifts.length > 0 ? this.targetShifts[0] : undefined;
  }

  get action(): DragShiftAction {
    return <DragShiftAction>this.controls.action.value;
  }

  get moveOffsetDays(): number {
    return <number>this.controls.moveOffsetDays.value;
  }

  get sourceShiftIsAbsence(): boolean {
    return this.sourceShifts.some(s => s.isAbsence);
  }

  get sourceShiftIsBooking(): boolean {
    return this.sourceShifts.some(s => s.isBooking);
  }

  get sourceShiftIsOnDuty(): boolean {
    return this.sourceShifts.some(s => s.isOnDuty);
  }

  get sourceShiftIsStandby(): boolean {
    return this.sourceShifts.some(s => s.isStandby);
  }

  private get sourceShiftsWithoutAccount(): boolean {
    return !this.sourceShifts.some(s => s.accountId);
  }

  get targetSlotIsEmpty(): boolean {
    return this.targetShifts.length === 0;
  }

  get targetShiftIsAbsence(): boolean {
    return this.targetShifts.some(s => s.isAbsence);
  }

  get targetShiftIsLended(): boolean {
    return this.targetShifts.some(s => s.isLended);
  }

  get targetEmployeeIsHidden(): boolean {
    return this.targetEmployeeId === this.hiddenEmployeeId;
  }

  get isWholeDayAbsence(): boolean {
    return <boolean>this.controls.isWholeDayAbsence.value;
  }

  private get targetEmployeeHasShiftAccountId(): boolean {
    let valid = true;

    if (this.targetEmployeeIsHidden && this.targetEmployee.accounts) {
      this.sourceShifts.forEach(shift => {
        if (
          this.targetEmployee.accounts
            .map(a => a.accountId)
            .includes(shift.accountId)
        ) {
          valid = false;
        }
      });
    }

    return valid;
  }

  get sameAccount(): boolean {
    return (
      this.sourceShiftsWithoutAccount || this.targetEmployeeHasShiftAccountId
    );
  }

  get sourceAndTargetIsSameEmployee(): boolean {
    return this.sourceEmployeeId === this.targetEmployeeId;
  }

  setHiddenEmployeeId(hiddenEmployeeId: number) {
    this.hiddenEmployeeId = hiddenEmployeeId;
  }

  setSourceEmployee(employee: PlanningEmployeeDTO) {
    this.sourceEmployee = employee;
    this.patchValue(
      {
        sourceEmployeeId: employee.employeeId,
        sourceEmployeeName: employee.name,
        sourceEmployeePostId: employee.employeePostId,
      },
      { emitEvent: false }
    );
  }

  setTargetEmployee(employee: PlanningEmployeeDTO) {
    this.targetEmployee = employee;
    this.patchValue(
      {
        targetEmployeeId: employee.employeeId,
        targetEmployeeName: employee.name,
        targetEmployeePostId: employee.employeePostId,
      },
      { emitEvent: false }
    );
    this.createTargetDates();
    this.setAvailabilityStatus();
    this.setSourceShiftExtraAndSubstituteStatus();
  }

  patchSourceShifts(shifts: PlanningShiftDTO[]) {
    this.sourceShifts = shifts;
    this.sourceShiftForms.clear();

    for (const shift of shifts) {
      this.sourceShiftForms.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftDragDialogShiftForm, {
          validationHandler: this.thisValidationHandler,
          element: shift,
        }),
        {
          emitEvent: false,
        }
      );
    }
    this.sourceShiftForms.updateValueAndValidity();
  }

  patchOnDutyShifts(shifts: PlanningShiftDTO[]) {
    this.onDutyShifts = shifts;
    this.onDutyShiftForms.clear();

    for (const shift of shifts) {
      this.onDutyShiftForms.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftDragDialogShiftForm, {
          validationHandler: this.thisValidationHandler,
          element: shift,
        }),
        {
          emitEvent: false,
        }
      );
    }
    this.onDutyShiftForms.updateValueAndValidity();
  }

  patchTargetShifts(shifts: PlanningShiftDTO[]) {
    this.targetShifts = shifts;
    this.targetShiftForms.clear();

    for (const shift of shifts) {
      this.targetShiftForms.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftDragDialogShiftForm, {
          validationHandler: this.thisValidationHandler,
          element: shift,
        }),
        {
          emitEvent: false,
        }
      );
    }
    this.targetShiftForms.updateValueAndValidity();
  }

  clearTimeDeviationCause() {
    if (this.controls.timeDeviationCauseId.value)
      this.patchValue(
        { timeDeviationCauseId: undefined },
        { emitEvent: false }
      );

    this.clearChild();
  }

  clearChild() {
    if (this.controls.employeeChildId.value)
      this.patchValue({ employeeChildId: undefined }, { emitEvent: false });
  }

  private createTargetDates() {
    this.targetDates = [];

    const sourceDates: Date[] = DateUtil.getUniqueDates(
      this.sourceShifts.map(s => s.actualStartTime)
    );
    sourceDates.forEach(date => {
      this.targetDates.push(date.addDays(this.moveOffsetDays));
    });
  }

  private setAvailabilityStatus() {
    this.targetEmployeeIsFullyUnavailable.set(false);
    this.targetEmployeeIsPartiallyUnavailable.set(false);

    if (!this.targetEmployee || !this.targetDates) return;

    this.targetDates.forEach(date => {
      if (
        this.targetEmployee.isFullyUnavailableInRange(
          date.beginningOfDay(),
          date.endOfDay()
        )
      ) {
        this.targetEmployeeIsFullyUnavailable.set(true);
      } else if (
        this.targetEmployee.isPartiallyUnavailableInRange(
          date.beginningOfDay(),
          date.endOfDay()
        )
      ) {
        this.targetEmployeeIsPartiallyUnavailable.set(true);
      }
    });

    if (
      this.targetEmployeeIsFullyUnavailable() &&
      this.targetEmployeeIsPartiallyUnavailable()
    ) {
      this.targetEmployeeIsPartiallyUnavailable.set(false);
    }
  }

  private setSourceShiftExtraAndSubstituteStatus() {
    this.sourceShiftIsExtra.set(this.sourceShifts.some(s => s.extraShift));
    this.sourceShiftIsSubstitute.set(
      this.sourceShifts.some(s => s.substituteShift)
    );
  }
}
