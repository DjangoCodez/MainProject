import {
  AbstractControl,
  FormArray,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
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
  TermGroup_EmployeeRequestResultStatus,
  TermGroup_EmployeeRequestStatus,
  TermGroup_EmployeeRequestType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeDeviationCauseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { AbsenceShiftsForm } from '../../components/absence-shifts/absence-shifts-form.model';
import { EmployeeRequestsDTO } from '../../models/employee-request.model';

export interface IAbsenceQuickDialog {
  employeeId: number;
  employeeName: string;
  timeDeviationCauseId: number;
  dateFrom: Date;
  dateTo: Date;
  timeScheduleScenarioHeadId: number | undefined;
  shifts: IShiftDTO[];
}
export interface IAbsenceQuickDialogForm {
  validationHandler: ValidationHandler;
  element: IAbsenceQuickDialog | undefined;
}
export class AbsenceQuickDialogForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: IAbsenceQuickDialogForm) {
    super(validationHandler, {
      employeeId: new SoeNumberFormControl(
        element?.employeeId || undefined,
        {}
      ),
      employeeName: new SoeTextFormControl(element?.employeeName || undefined, {
        disabled: true,
      }),
      timeDeviationCauseId: new SoeSelectFormControl(
        element?.timeDeviationCauseId || undefined,
        {
          required: true,
        },
        'common.time.timedeviationcause'
      ),
      dateFrom: new SoeDateFormControl(element?.dateFrom || undefined, {
        disabled: true,
      }),
      dateTo: new SoeDateFormControl(element?.dateTo || undefined, {
        disabled: true,
      }),
      timeScheduleScenarioHeadId: new SoeSelectFormControl(
        element?.timeScheduleScenarioHeadId || undefined
      ),
      employeeChildId: new SoeSelectFormControl(
        undefined,
        {
          required: false,
        },
        'time.schedule.absencerequests.employeechild'
      ),
      // approveAllTypeId: new SoeSelectFormControl(
      //   undefined,
      //   { required: true },
      //   'time.schedule.absencerequests.approve'
      // ),
      replaceAllWithEmployeeId: new SoeSelectFormControl(
        undefined,
        { required: true },
        'time.schedule.absencerequests.replacewith'
      ),
      skipSendXEMail: new SoeCheckboxFormControl(
        false,
        {},
        'time.schedule.absencerequests.skipxemailonshiftchanges'
      ),
      shifts: new FormArray<AbsenceShiftsForm>([]),
    });

    this.thisValidationHandler = validationHandler;
  }

  get employeeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeId;
  }
  get timeDeviationCauseId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.timeDeviationCauseId;
  }
  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }
  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }
  get employeeChildId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeChildId;
  }
  // get approveAllTypeId(): SoeSelectFormControl {
  //   return <SoeSelectFormControl>this.controls.approveAllTypeId;
  // }
  get replaceAllWithEmployeeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.replaceAllWithEmployeeId;
  }
  get skipSendXEMail(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.skipSendXEMail;
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

  toDTO(): EmployeeRequestsDTO {
    const dto = new EmployeeRequestsDTO();
    const raw = this.getRawValue();

    dto.employeeRequestId = 0;
    dto.employeeId = raw.employeeId;
    dto.status = TermGroup_EmployeeRequestStatus.RequestPending;
    dto.resultStatus = TermGroup_EmployeeRequestResultStatus.None;
    dto.timeDeviationCauseId = raw.timeDeviationCauseId;
    dto.comment = '';
    dto.type = TermGroup_EmployeeRequestType.AbsenceRequest;
    if (raw.employeeChildId && raw.employeeChildId !== 0)
      dto.employeeChildId = raw.employeeChildId;
    console.log(dto.employeeChildId);
    dto.start = new Date(raw.dateFrom.setHours(0, 0, 0, 0));
    dto.stop = new Date(raw.dateTo.setHours(23, 59, 59, 0));
    dto.comment = '';

    return dto;
  }

  public getShiftDTOs(): IShiftDTO[] {
    return this.shifts.controls
      .map((control: AbsenceShiftsForm) => control.toShiftDTO(true))
      .filter(shift => shift) as IShiftDTO[];
  }

  public getAffectedEmployeeIds() {
    const dto = this.getRawValue();
    return [
      this.employeeId.value,
      ...dto.shifts
        // .filter(
        //   (shift: any) => shift.approvalTypeId === TermGroup_YesNo.Yes
        // )
        .map((shift: any) => shift.replaceWithEmployeeId),
    ];
  }

  public replaceAll(employeeId: number) {
    this.shifts.controls.forEach(control => {
      control.patchValue(
        {
          replaceWithEmployeeId: employeeId,
        },
        { emitEvent: false }
      );
    });
  }

  // public approveAll(approvalTypeId: TermGroup_YesNo) {
  //   this.shifts.controls.forEach(control => {
  //     control.patchValue({ approvalTypeId: approvalTypeId, emitEvent: false });
  //   });
  // }
}
export function createFullDayValidator(
  errormessage: string,
  cause: ITimeDeviationCauseDTO,
  shifts: IShiftDTO[],
  shiftsRestOfDay: IShiftDTO[]
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!cause?.onlyWholeDay) {
      return null; // Not required if not a full-day cause
    }

    const selectedShiftIds = shifts.map(s => s.timeScheduleTemplateBlockId);
    const fullDayShiftIds = shiftsRestOfDay.map(
      s => s.timeScheduleTemplateBlockId
    );

    const coversFullDay =
      selectedShiftIds.length === fullDayShiftIds.length &&
      selectedShiftIds.every(id => fullDayShiftIds.includes(id));

    return coversFullDay ? null : { [errormessage]: true };
  };
}
