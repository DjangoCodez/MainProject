import {
  Component,
  inject,
  input,
  OnDestroy,
  OnInit,
  signal,
  computed,
  HostBinding,
} from '@angular/core';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { SpShiftComponent } from '../sp-shift/sp-shift.component';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { CdkContextMenuTrigger } from '@angular/cdk/menu';
import {
  ShiftMenuComponent,
  ShiftMenuItemSelected,
  ShiftMenuOption,
} from '../../context-menus/sp-shift-menu/sp-shift-menu.component';
import { CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { SpDialogService } from '../../services/sp-dialog.service';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { SpShiftService } from '../../services/sp-shift.service';
import {
  EmployeeMenuItemSelected,
  SpEmployeeMenuComponent,
} from '../../context-menus/sp-employee-menu/sp-employee-menu.component';
import { SpSettingService } from '../../services/sp-setting.service';
import { SpFilterService } from '../../services/sp-filter.service';
import { IconModule } from '@ui/icon/icon.module';
import { Subscription } from 'rxjs';
import {
  SpEmployeeDaySlot,
  SpEmployeeHalfHourSlot,
  SpEmployeeHourSlot,
  SpEmployeeQuarterHourSlot,
} from '../../models/time-slot.model';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  EmployeesAndShiftsRecalculatedEvent,
  SpEventService,
} from '../../services/sp-event.service';
import { DragShiftAction } from '@shared/models/generated-interfaces/Enumerations';
import { ShiftUtil } from '../../util/shift-util';
import { PlanningShiftDTO } from '../../models/shift.model';
import { SpShiftDragDialogResult } from '../../dialogs/sp-shift-drag-dialog/sp-shift-drag-dialog.component';

@Component({
  selector: 'sp-employee-row',
  imports: [
    CdkContextMenuTrigger,
    CdkDropList,
    MatTooltipModule,
    IconModule,
    SpEmployeeMenuComponent,
    ShiftMenuComponent,
    SpShiftComponent,
    MinutesToTimeSpanPipe,
  ],
  templateUrl: './sp-employee-row.component.html',
  styleUrl: './sp-employee-row.component.scss',
})
export class SpEmployeeRowComponent implements OnInit, OnDestroy {
  employee = input.required<PlanningEmployeeDTO>();
  canBePinned = input(false);

  pinned = signal(false);
  readonly isPinned = computed(() => this.canBePinned() && this.pinned());

  // Bind the CSS class to the host element
  @HostBinding('class.sticky') get stickyClass() {
    return this.isPinned();
  }

  private readonly dialogService = inject(SpDialogService);
  readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);
  private readonly shiftService = inject(SpShiftService);

  private loadingEmployeeIdsChangedSubscription?: Subscription;
  private loadingShiftsForEmployeeIdsChangedSubscription?: Subscription;
  private shiftsForEmployeeChangedSubscription?: Subscription;
  private selectedEmployeeChangedSubscription?: Subscription;
  private selectedSlotChangedSubscription?: Subscription;
  private employeesAndShiftsRecalculatedSubscription?: Subscription;

  loadingEmployee = signal(false);

  // Number of rows to display (based on max number of shifts for the employee)
  nbrOfRows = signal(1);
  height = computed(() => {
    if (this.filterService.isCommonDayView()) {
      return (
        this.nbrOfRows() * ShiftUtil.SHIFT_HALF_HEIGHT +
        2 * ShiftUtil.SHIFT_HALF_HEIGHT_PADDING +
        (this.nbrOfRows() - 1) * ShiftUtil.SHIFT_HALF_HEIGHT_MARGIN
      );
    } else {
      return (
        this.nbrOfRows() * ShiftUtil.SHIFT_FULL_HEIGHT +
        2 * ShiftUtil.SHIFT_FULL_HEIGHT_PADDING +
        (this.nbrOfRows() - 1) * ShiftUtil.SHIFT_FULL_HEIGHT_MARGIN
      );
    }
  });

  isHoveringShift = signal(false); // Prevent slot tooltip from showing when hovering over shift
  isHoveringPinIcon = signal(false); // Prevent employee tooltip from showing when hovering over pin icon

  totalNetTime = signal(0);
  // totalAbsenceTime = signal(0);
  // totalFactorTime = signal(0);
  // totalGrossTime = signal(0);
  // totalCost = signal(0);
  // totalCostIncEmpTaxAndSuppCharge = signal(0);

  hoveredDaySlotStart: Date | null = null;

  ngOnInit(): void {
    this.loadingEmployeeIdsChangedSubscription =
      this.employeeService.loadingEmployeeIdsChanged.subscribe(
        (employeeIds: number[]) => {
          this.loadingEmployee.set(
            employeeIds.includes(this.employee().employeeId)
          );
        }
      );

    this.loadingShiftsForEmployeeIdsChangedSubscription =
      this.employeeService.loadingShiftsForEmployeeIdsChanged.subscribe(
        (employeeIds: number[]) => {
          this.loadingEmployee.set(
            employeeIds.includes(this.employee().employeeId)
          );
        }
      );

    this.shiftsForEmployeeChangedSubscription =
      this.employeeService.shiftsForEmployeeIdsChanged.subscribe(
        (employeeIds: number[]) => {
          if (employeeIds.includes(this.employee().employeeId)) {
            if (this.filterService.isCommonDayView()) {
              this.nbrOfRows.set(
                this.employeeService.getMaxNbrOfShiftsPerDayForDayView(
                  this.employee().employeeId
                )
              );
            } else if (this.filterService.isCommonScheduleView()) {
              this.nbrOfRows.set(
                this.employeeService.getMaxNbrOfShiftsPerDayForScheduleView(
                  this.employee().employeeId
                )
              );
            }
          }
        }
      );

    this.selectedEmployeeChangedSubscription =
      this.employeeService.selectedEmployeeChanged.subscribe(
        (employee: PlanningEmployeeDTO | undefined) => {
          this.employee().isSelected =
            employee?.employeeId === this.employee().employeeId;
        }
      );

    this.selectedSlotChangedSubscription =
      this.employeeService.selectedSlotChanged.subscribe(
        (
          slot:
            | SpEmployeeDaySlot
            | SpEmployeeHourSlot
            | SpEmployeeHalfHourSlot
            | SpEmployeeQuarterHourSlot
            | undefined
        ) => {
          if (this.filterService.isCommonDayView()) {
            this.employee().daySlots.forEach(eDaySlot => {
              eDaySlot.hourSlots.forEach(eHourSlot => {
                eHourSlot.isSelected =
                  eHourSlot.employeeId === slot?.employeeId &&
                  eHourSlot.start === slot?.start;
              });
              eDaySlot.halfHourSlots.forEach(eHalfHourSlot => {
                eHalfHourSlot.isSelected =
                  eHalfHourSlot.employeeId === slot?.employeeId &&
                  eHalfHourSlot.start === slot?.start;
              });
              eDaySlot.quarterHourSlots.forEach(eQuarterHourSlot => {
                eQuarterHourSlot.isSelected =
                  eQuarterHourSlot.employeeId === slot?.employeeId &&
                  eQuarterHourSlot.start === slot?.start;
              });
            });
          } else if (this.filterService.isCommonScheduleView()) {
            this.employee().daySlots.forEach(eDaySlot => {
              eDaySlot.isSelected =
                eDaySlot.employeeId === slot?.employeeId &&
                eDaySlot.start === slot?.start;
            });
          }
        }
      );

    this.employeesAndShiftsRecalculatedSubscription =
      this.eventService.employeesAndShiftsRecalculated.subscribe(
        (event: EmployeesAndShiftsRecalculatedEvent | undefined) => {
          if (event && this.employee().isVisible) {
            this.calculateTotals();
          }
        }
      );
  }

  ngOnDestroy(): void {
    this.loadingEmployeeIdsChangedSubscription?.unsubscribe();
    this.loadingShiftsForEmployeeIdsChangedSubscription?.unsubscribe();
    this.shiftsForEmployeeChangedSubscription?.unsubscribe();
    this.selectedEmployeeChangedSubscription?.unsubscribe();
    this.selectedSlotChangedSubscription?.unsubscribe();
    this.employeesAndShiftsRecalculatedSubscription?.unsubscribe();
  }

  private calculateTotals() {
    // Use signal for this since its used in UI and we want to avoid unnecessary recalculations on UI changes
    this.totalNetTime.set(this.employee().totalNetTime);
    // this.totalAbsenceTime.set(this.employee().totalAbsenceTime);
    // this.totalFactorTime.set(this.employee().totalFactorTime);
    // this.totalGrossTime.set(this.employee().totalGrossTime);
    // this.totalCost.set(this.employee().totalCost);
    // this.totalCostIncEmpTaxAndSuppCharge.set(
    //   this.employee().totalCostIncEmpTaxAndSuppCharge
    // );
  }

  // CLICK EVENTS

  onEmployeeClick() {
    // Check if current employee is selected
    const currentEmployeeWasSelected =
      this.employeeService.selectedEmployeeChanged.value?.employeeId ===
      this.employee().employeeId;

    if (currentEmployeeWasSelected) {
      // Same employee was selected again, deselect it.
      this.employeeService.selectedEmployeeChanged.next(undefined);
    } else {
      // A new employee was selected.
      this.employeeService.selectedEmployeeChanged.next(this.employee());
    }
  }

  onSlotClick(
    slot:
      | SpEmployeeDaySlot
      | SpEmployeeHourSlot
      | SpEmployeeHalfHourSlot
      | SpEmployeeQuarterHourSlot
  ) {
    this.employeeService.selectedSlotChanged.next(slot);
  }

  onSlotDoubleClick(date: Date) {
    // Double click in slot will open the shift edit dialog in new shift mode.
    this.eventService.addShift(date, this.employee().employeeId);
  }

  // DRAG & DROP EVENTS

  onDaySlotEnter(daySlot: any): void {
    this.hoveredDaySlotStart = daySlot.start;
  }

  onDaySlotExit(daySlot: any): void {
    if (this.hoveredDaySlotStart === daySlot.start) {
      this.hoveredDaySlotStart = null;
    }
  }

  onDaySlotDrop(event: CdkDragDrop<any>, daySlot: any): void {
    this.hoveredDaySlotStart = null;
    this.onDrop(event);
  }

  onEmployeeDrop(event: CdkDragDrop<any>): void {
    // Dropped on employee slot, not in a specific day slot
    // Don't do anything
  }

  onDrop(event: CdkDragDrop<any>) {
    if (event.previousContainer === event.container) return; // Dropped in the same slot, do nothing

    const ctrlKeyPressed = event.event?.ctrlKey;
    const shiftKeyPressed = event.event?.shiftKey;
    const altKeyPressed = event.event?.altKey;

    let defaultAction = DragShiftAction.Move;
    if (ctrlKeyPressed) {
      defaultAction = DragShiftAction.Copy;
    } else if (altKeyPressed) {
      defaultAction = DragShiftAction.Absence;
    }

    // Get source
    const sourceSlot = event.previousContainer.data;
    if (!sourceSlot) return;
    const sourceEmployee = this.employeeService.getEmployee(
      sourceSlot.employeeId
    );
    if (!sourceEmployee) return;
    const sourceShifts =
      this.shiftService.selectedShiftsChanged.value.length > 1
        ? this.shiftService.selectedShiftsChanged.value
        : [event.item.data()];
    if (sourceShifts.length === 0) return;

    // Also send intersecting on duty shifts to the dialog
    const onDutyShifts: PlanningShiftDTO[] = [];
    sourceShifts.forEach(shift => {
      const ods = this.shiftService.getIntersectingOnDutyShifts(shift);
      ods.forEach(sh => {
        if (
          !onDutyShifts.find(
            s =>
              s.timeScheduleTemplateBlockId === sh.timeScheduleTemplateBlockId
          )
        ) {
          onDutyShifts.push(sh);
        }
      });
    });

    // Get target
    const targetSlot = event.container.data;
    if (!targetSlot) return;
    const targetEmployee = this.employeeService.getEmployee(
      targetSlot.employeeId
    );
    if (!targetEmployee) return;

    const offsetDays = targetSlot.start.diffDays(sourceSlot.start);

    this.dialogService
      .openShiftDragDialog(
        sourceSlot.start,
        sourceEmployee,
        sourceShifts,
        onDutyShifts,
        targetSlot.start,
        targetEmployee,
        targetSlot.shifts,
        defaultAction,
        ctrlKeyPressed || shiftKeyPressed, // Execute default action if ctrl or shift key is pressed
        offsetDays
      )
      .subscribe((result: SpShiftDragDialogResult) => {
        if (result.shiftModified) {
          this.shiftService
            .loadShifts([sourceEmployee.employeeId, targetEmployee.employeeId])
            .subscribe();
        }
      });
  }

  // CONTEXT MENU EVENTS

  onEmployeeMenuSelected(event: EmployeeMenuItemSelected) {
    switch (event.option) {
      default:
        console.log('onEmployeeMenuSelected', event);
        break;
    }
  }

  onSlotMenuSelected(event: ShiftMenuItemSelected) {
    const slot = event.date
      ? this.employee().getDaySlot(event.date)
      : undefined;

    switch (event.option) {
      case ShiftMenuOption.Debug:
        if (slot) console.log('Slot:', slot);
        break;
      default:
        console.log('onSlotMenuSelected', event);
        break;
    }
  }
}
