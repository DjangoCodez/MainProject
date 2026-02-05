import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { PlanningShiftDTO } from '../../models/shift.model';

interface ISpShiftSplitDialogForm {
  validationHandler: ValidationHandler;
  element: PlanningShiftDTO | undefined;
}
export class SpShiftSplitDialogForm extends SoeFormGroup {
  shift!: PlanningShiftDTO;

  constructor({ validationHandler, element }: ISpShiftSplitDialogForm) {
    super(validationHandler, {
      timeScheduleTemplateBlockId: new SoeNumberFormControl(
        element?.timeScheduleTemplateBlockId || 0,
        { isIdField: true }
      ),
      employeeId: new SoeNumberFormControl(element?.employeeId || 0),
      employeeName: new SoeTextFormControl(element?.employeeName || '', {
        disabled: true,
      }),
      actualStartDate: new SoeDateFormControl(
        element?.actualStartDate || undefined,
        { disabled: true }
      ),
      shiftTypeId: new SoeNumberFormControl(element?.shiftTypeId || 0),
      shiftTypeName: new SoeTextFormControl(element?.shiftTypeName || '', {
        disabled: true,
      }),
      startTime: new SoeDateFormControl(element?.startTime || undefined, {
        disabled: true,
      }),
      stopTime: new SoeDateFormControl(element?.stopTime || undefined, {
        disabled: true,
      }),
      splitTime: new SoeDateFormControl(element?.stopTime || undefined),
      splitTimeOffset: new SoeNumberFormControl(0),
      employeeId1: new SoeNumberFormControl(element?.employeeId || 0),
      employeeId2: new SoeNumberFormControl(element?.employeeId || 0),
    });
  }

  get startTime(): Date {
    return this.controls.startTime.value;
  }

  get stopTime(): Date {
    return this.controls.stopTime.value;
  }

  get splitTime(): Date {
    return this.controls.splitTime.value;
  }

  get duration(): number {
    return this.stopTime.diffMinutes(this.startTime);
  }

  get employeeId1(): number {
    return this.controls.employeeId1.value;
  }

  get employeeId2(): number {
    return this.controls.employeeId2.value;
  }

  patchShift(shift: PlanningShiftDTO) {
    this.shift = shift;
    this.reset(shift);
  }

  setDefaultSplitTime() {
    const splitTime: Date = this.startTime.addMinutes(this.duration / 2);

    this.controls.splitTime.setValue(splitTime);
    this.controls.splitTimeOffset.setValue(this.duration / 2);
  }

  setSplitTimeOffset() {
    const offset = this.splitTime.diffMinutes(this.startTime);
    this.controls.splitTimeOffset.setValue(offset);
  }

  setInitialEmployees() {
    this.controls.employeeId1.setValue(this.controls.employeeId.value);
    this.controls.employeeId2.setValue(this.controls.employeeId.value);
  }
}
