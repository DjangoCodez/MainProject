import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IEmployeeRequestDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { AbsenceShiftsForm } from '../components/absence-shifts/absence-shifts-form.model';
import { FormArray } from '@angular/forms';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';

interface IAbsenceRequestsForm {
  validationHandler: ValidationHandler;
  element: IEmployeeRequestDTO | undefined;
}

export class AbsenceRequestsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IAbsenceRequestsForm) {
    super(validationHandler, {
      employeeRequestId: new SoeNumberFormControl(
        element?.employeeRequestId || 0,
        {
          isIdField: true,
        }
      ),
      employeeName: new SoeTextFormControl(element?.employeeName || '', {
        isNameField: true,
      }),
      timeDeviationCauseName: new SoeTextFormControl(
        element?.timeDeviationCauseName || '',
        {}
      ),
      employeeId: new SoeNumberFormControl(element?.employeeId || 0, {
        required: true,
      }),
      timeDeviationCauseId: new SoeNumberFormControl(
        element?.timeDeviationCauseId || 0,
        {
          required: true,
        }
      ),
      start: new SoeDateFormControl(element?.start || '', {}),
      stop: new SoeDateFormControl(element?.stop || '', {}),
      comment: new SoeTextFormControl(element?.comment || '', {}),

      shifts: new FormArray<AbsenceShiftsForm>([]),
    });
    this.thisValidationHandler = validationHandler;
  }

  get employeeRequestId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.employeeRequestId;
  }

  get employeeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.employeeName;
  }

  get timeDeviationCauseName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.timeDeviationCauseName;
  }

  get employeeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.employeeId;
  }

  get timeDeviationCauseId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeDeviationCauseId;
  }

  get start(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.start;
  }

  get stop(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.stop;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get shifts(): FormArray<AbsenceShiftsForm> {
    return <FormArray<AbsenceShiftsForm>>this.controls.shifts;
  }

  populateShiftsForm(shifts: IShiftDTO[]) {
    // Only populate if form is empty or has different number of items
    if (this.shifts.length === 0 || this.shifts.length !== shifts.length) {
      this.shifts.clear();
      for (const shift of shifts) {
        const shiftForm = new AbsenceShiftsForm({
          validationHandler: this.thisValidationHandler,
          element: shift,
        });
        this.shifts.push(shiftForm);
      }
    }
  }
}
