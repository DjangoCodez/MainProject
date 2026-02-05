import { inject, Injectable } from '@angular/core';
import { PlanningShiftDTO, PlanningShiftDayDTO } from '../models/shift.model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import {
  SpShiftDialogComponent,
  SpShiftDialogData,
  SpShiftDialogResult,
} from '../dialogs/sp-shift-dialog/sp-shift-dialog.component';
import {
  SpFilterDialogComponent,
  SpFilterDialogData,
  SpFilterDialogResult,
} from '../dialogs/sp-filter-dialog/sp-filter-dialog.component';
import { Observable } from 'rxjs';
import {
  SpSettingDialogComponent,
  SpSettingDialogData,
  SpSettingDialogResult,
} from '../dialogs/sp-setting-dialog/sp-setting-dialog.component';
import {
  SpShiftDeleteDialogComponent,
  SpShiftDeleteDialogData,
  SpShiftDeleteDialogResult,
} from '../dialogs/sp-shift-delete-dialog/sp-shift-delete-dialog.component';
import {
  SpShiftDragDialogComponent,
  SpShiftDragDialogData,
  SpShiftDragDialogResult,
} from '../dialogs/sp-shift-drag-dialog/sp-shift-drag-dialog.component';
import { PlanningEmployeeDTO } from '../models/employee.model';
import {
  SpShiftSplitDialogComponent,
  SpShiftSplitDialogData,
  SpShiftSplitDialogResult,
} from '../dialogs/sp-shift-split-dialog/sp-shift-split-dialog.component';
import { DragShiftAction } from '@shared/models/generated-interfaces/Enumerations';
import {
  SpShiftHistoryDialogComponent,
  SpShiftHistoryDialogData,
  SpShiftHistoryDialogResult,
} from '../dialogs/sp-shift-history-dialog/sp-shift-history-dialog.component';
import {
  SpShiftAccountingDialogComponent,
  SpShiftAccountingDialogData,
  SpShiftAccountingDialogResult,
} from '../dialogs/sp-shift-accounting-dialog/sp-shift-accounting-dialog.component';
import {
  SpShiftRequestDialogComponent,
  SpShiftRequestDialogData,
  SpShiftRequestDialogResult,
} from '../dialogs/sp-shift-request-dialog/sp-shift-request-dialog.component';
import {
  AbsenceQuickDialogComponent,
  IAbsenceQuickDialogData,
} from '@features/time/absence-requests/dialogs/absence-quick-dialog/absence-quick-dialog.component';

@Injectable({
  providedIn: 'root',
})
export class SpDialogService {
  dialogService = inject(DialogService);

  openFilterDialog(): Observable<SpFilterDialogResult> {
    const dialogData: SpFilterDialogData = {
      title: 'core.filter',
      size: 'lg',
      disableContentScroll: true,
    };

    return this.dialogService
      .open(SpFilterDialogComponent, dialogData)
      .afterClosed();
  }

  openSettingDialog(): Observable<SpSettingDialogResult> {
    const dialogData: SpSettingDialogData = {
      title: 'common.settings',
      size: 'lg',
    };

    return this.dialogService
      .open(SpSettingDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftDialog(
    day: PlanningShiftDayDTO,
    selectedShiftId?: number,
    addShift?: boolean
  ): Observable<SpShiftDialogResult> {
    // TODO: Create new term
    const dialogData: SpShiftDialogData = {
      title: 'Redigera arbetsdag',
      size: 'xl',
      disableClose: true,
      //disableContentScroll: true,
      day: day,
      selectedShiftId: selectedShiftId,
      addShift: addShift || false,
    };

    return this.dialogService
      .open(SpShiftDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftAbsenceDialog(
    employee: PlanningEmployeeDTO,
    shifts: PlanningShiftDTO[]
  ) {
    console.log('Open absence dialog for employee', employee.name, shifts);
    const dateFrom = new Date(
      Math.min(...shifts.map(s => s.startTime.getTime()))
    );
    const dateTo = new Date(Math.max(...shifts.map(s => s.stopTime.getTime())));
    const dialogData: IAbsenceQuickDialogData = {
      title: 'time.employee.employee.absence',
      employeeId: employee.employeeId,
      employeeName: employee.name,
      dateFrom: dateFrom,
      dateTo: dateTo,
      shiftIds: shifts.map(shift => shift.timeScheduleTemplateBlockId),
      timeScheduleScenarioHeadId: 0,
    };

    return this.dialogService
      .open(AbsenceQuickDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftAccountingDialog(
    shiftIds: number[]
  ): Observable<SpShiftAccountingDialogResult> {
    const dialogData: SpShiftAccountingDialogData = {
      title: 'time.schedule.planning.editshift.functions.accounting',
      size: 'lg',
      shiftIds: shiftIds,
    };

    return this.dialogService
      .open(SpShiftAccountingDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftDeleteDialog(
    employee: PlanningEmployeeDTO,
    shifts: PlanningShiftDTO[],
    onDutyShifts: PlanningShiftDTO[] = []
  ): Observable<SpShiftDeleteDialogResult> {
    const dialogData: SpShiftDeleteDialogData = {
      // TODO: New terms
      title:
        shifts.length > 1
          ? 'time.schedule.planning.delete.shifts'
          : 'time.schedule.planning.delete.shift',
      size: 'lg',
      employee: employee,
      shifts: shifts,
      onDutyShifts: onDutyShifts,
    };

    return this.dialogService
      .open(SpShiftDeleteDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftDragDialog(
    sourceDate: Date,
    sourceEmployee: PlanningEmployeeDTO,
    sourceShifts: PlanningShiftDTO[],
    onDutyShifts: PlanningShiftDTO[],
    targetDate: Date,
    targetEmployee: PlanningEmployeeDTO,
    targetShifts: PlanningShiftDTO[],
    defaultAction: DragShiftAction = DragShiftAction.Move,
    executeDefaultAction = false,
    moveOffsetDays = 0
  ): Observable<SpShiftDragDialogResult> {
    const dialogData: SpShiftDragDialogData = {
      title: 'time.schedule.planning.editshift.edit',
      size: 'lg',
      sourceDate: sourceDate,
      sourceEmployee: sourceEmployee,
      sourceShifts: sourceShifts,
      onDutyShifts: onDutyShifts,
      targetDate: targetDate,
      targetEmployee: targetEmployee,
      targetShifts: targetShifts,
      defaultAction: defaultAction,
      executeDefaultAction: executeDefaultAction,
      moveOffsetDays: moveOffsetDays,
    };

    return this.dialogService
      .open(SpShiftDragDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftHistoryDialog(
    shiftIds: number[]
  ): Observable<SpShiftHistoryDialogResult> {
    const dialogData: SpShiftHistoryDialogData = {
      title: 'time.schedule.planning.editshift.functions.history',
      size: 'lg',
      shiftIds: shiftIds,
    };

    return this.dialogService
      .open(SpShiftHistoryDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftSplitDialog(
    title: string,
    shift: PlanningShiftDTO
  ): Observable<SpShiftSplitDialogResult> {
    const dialogData: SpShiftSplitDialogData = {
      title: title,
      size: 'lg',
      shift: shift,
    };

    return this.dialogService
      .open(SpShiftSplitDialogComponent, dialogData)
      .afterClosed();
  }

  openShiftRequestDialog(
    employee: PlanningEmployeeDTO,
    shift: PlanningShiftDTO,
    possibleEmployees: PlanningEmployeeDTO[]
  ): Observable<SpShiftRequestDialogResult> {
    const dialogData: SpShiftRequestDialogData = {
      title: 'time.schedule.planning.contextmenu.sendshiftrequest',
      size: 'xl',
      hideFooter: true,
      disableContentScroll: true,
      employee: employee,
      shift: shift,
      possibleEmployees: possibleEmployees,
    };

    return this.dialogService
      .open(SpShiftRequestDialogComponent, dialogData)
      .afterClosed();
  }
}
