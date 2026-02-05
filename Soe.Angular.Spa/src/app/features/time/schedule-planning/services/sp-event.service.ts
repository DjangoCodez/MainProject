import { Injectable } from '@angular/core';
import { PlanningShiftDTO } from '../models/shift.model';
import { BehaviorSubject } from 'rxjs';
import { PlanningEmployeeDTO } from '../models/employee.model';

export type EmployeesAndShiftsRecalculatedEvent = {
  allEmployees: number;
  visibleEmployees: number;
  allShifts: number;
  visibleShifts: number;
};

export type AddShiftEvent = {
  date: Date;
  employeeId: number;
  selectedShiftId?: number;
};
export type EditShiftEvent = {
  shift: PlanningShiftDTO;
};
export type DeleteShiftsEvent = {
  employee: PlanningEmployeeDTO;
  shifts: PlanningShiftDTO[];
  onDutyShifts: PlanningShiftDTO[];
};
export type SplitShiftEvent = {
  dialogTitle: string;
  shift: PlanningShiftDTO;
};
export type ShiftRequestEvent = {
  employee: PlanningEmployeeDTO;
  shift: PlanningShiftDTO;
};
export type ShiftAbsenceEvent = {
  employee: PlanningEmployeeDTO;
  shifts: PlanningShiftDTO[];
};

@Injectable({
  providedIn: 'root',
})
export class SpEventService {
  constructor() {}

  employeesAndShiftsRecalculated = new BehaviorSubject<
    EmployeesAndShiftsRecalculatedEvent | undefined
  >(undefined);

  reloadEmployeeEvent = new BehaviorSubject<{ employeeId: number } | undefined>(
    undefined
  );
  reloadAllEmployeesEvent = new BehaviorSubject<{} | undefined>(undefined);

  addShiftEvent = new BehaviorSubject<AddShiftEvent | undefined>(undefined);
  editShiftEvent = new BehaviorSubject<EditShiftEvent | undefined>(undefined);
  deleteShiftEvent = new BehaviorSubject<DeleteShiftsEvent | undefined>(
    undefined
  );
  deleteCurrentShiftEvent = new BehaviorSubject<{} | undefined>(undefined);
  splitShiftEvent = new BehaviorSubject<SplitShiftEvent | undefined>(undefined);
  shiftRequestEvent = new BehaviorSubject<ShiftRequestEvent | undefined>(
    undefined
  );
  shiftAbsenceEvent = new BehaviorSubject<ShiftAbsenceEvent | undefined>(
    undefined
  );

  shiftSummaryNeedsUpdate = new BehaviorSubject<{} | undefined>(undefined);

  // EMPLOYEE

  reloadEmployee(employeeId: number) {
    this.reloadEmployeeEvent.next({ employeeId });
  }

  reloadAllEmployees() {
    this.reloadAllEmployeesEvent.next({});
  }

  // SHIFT

  addShift(date: Date, employeeId: number, selectedShiftId?: number) {
    this.addShiftEvent.next({
      date,
      employeeId,
      selectedShiftId: selectedShiftId,
    });
  }

  editShift(shift: PlanningShiftDTO) {
    this.editShiftEvent.next({ shift });
  }

  deleteShifts(
    employee: PlanningEmployeeDTO,
    shifts: PlanningShiftDTO[],
    onDutyShifts: PlanningShiftDTO[] = []
  ) {
    this.deleteShiftEvent.next({ employee, shifts, onDutyShifts });
  }

  deleteCurrentShift() {
    this.deleteCurrentShiftEvent.next({});
  }

  splitShift(dialogTitle: string, shift: PlanningShiftDTO) {
    this.splitShiftEvent.next({ dialogTitle, shift });
  }

  shiftRequest(employee: PlanningEmployeeDTO, shift: PlanningShiftDTO) {
    this.shiftRequestEvent.next({ employee, shift });
  }

  shiftAbsence(employee: PlanningEmployeeDTO, shifts: PlanningShiftDTO[]) {
    this.shiftAbsenceEvent.next({ employee, shifts });
  }

  updateShiftSummary() {
    this.shiftSummaryNeedsUpdate.next({});
  }
}
