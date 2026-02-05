import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SpShiftDeleteDialogShiftForm } from './sp-shift-delete-dialog-shift-form.model';
import { PlanningShiftDTO } from '../../models/shift.model';
import { ShiftUtil } from '../../util/shift-util';

interface ISpShiftDeleteDialogFormElement {
  employeeId: number;
  employeeName: string;
  firstDate?: Date;
  lastDate?: Date;
  shifts: PlanningShiftDTO[];
  includeOnDutyShifts: boolean;
}

export class SpShiftDeleteDialogForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  shifts: PlanningShiftDTO[] = [];
  onDutyShifts: PlanningShiftDTO[] = [];

  constructor(args: {
    validationHandler: ValidationHandler;
    element: ISpShiftDeleteDialogFormElement | undefined;
  }) {
    super(args.validationHandler, {
      employeeId: new SoeNumberFormControl(args.element?.employeeId ?? 0),
      employeeName: new SoeTextFormControl(args.element?.employeeName ?? ''),
      firstDate: new SoeDateFormControl(args.element?.firstDate ?? undefined),
      lastDate: new SoeDateFormControl(args.element?.lastDate ?? undefined),
      shifts: new FormArray<SpShiftDeleteDialogShiftForm>([]),
      onDutyShifts: new FormArray<SpShiftDeleteDialogShiftForm>([]),
      includeOnDutyShifts: new SoeCheckboxFormControl(
        args.element?.includeOnDutyShifts ?? false
      ),
    });

    this.thisValidationHandler = args.validationHandler;
  }

  get shiftForms(): FormArray<SpShiftDeleteDialogShiftForm> {
    return <FormArray>this.controls.shifts;
  }

  get onDutyShiftForms(): FormArray<SpShiftDeleteDialogShiftForm> {
    return <FormArray>this.controls.onDutyShifts;
  }

  patchShifts(shifts: PlanningShiftDTO[]) {
    this.shifts = shifts;
    this.shiftForms.clear();

    for (const shift of shifts) {
      this.shiftForms.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftDeleteDialogShiftForm, {
          validationHandler: this.thisValidationHandler,
          element: shift,
        }),
        {
          emitEvent: false,
        }
      );
    }
    this.shiftForms.updateValueAndValidity();
  }

  patchOnDutyShifts(shifts: PlanningShiftDTO[]) {
    this.onDutyShifts = shifts;
    this.onDutyShiftForms.clear();

    for (const shift of shifts) {
      this.onDutyShiftForms.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftDeleteDialogShiftForm, {
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
}
