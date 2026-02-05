import { computed, inject, Injectable, signal } from '@angular/core';
import { IEmployeeListDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { SchedulePlanningService } from './schedule-planning.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import {
  TermGroup_StaffingNeedsHeadInterval,
  TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat,
  TermGroup_TimeSchedulePlanningDayViewGroupBy,
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningScheduleViewSortBy,
  TermGroup_TimeScheduleTemplateBlockShiftStatus,
  TimeSchedulePlanningDisplayMode,
} from '@shared/models/generated-interfaces/Enumerations';
import { SpFilterService } from './sp-filter.service';
import {
  EmployeeListAvailabilityDTO,
  EmployeeListEmploymentDTO,
  PlanningEmployeeDTO,
} from '../models/employee.model';
import { SpSlotService } from './sp-slot.service';
import { SpSettingService } from './sp-setting.service';
import { PlanningShiftDTO } from '../models/shift.model';
import { SpEventService } from './sp-event.service';
import {
  SpDaySlot,
  SpEmployeeDaySlot,
  SpEmployeeHalfHourSlot,
  SpEmployeeHourSlot,
  SpEmployeeQuarterHourSlot,
  SpHalfHourSlot,
  SpHourSlot,
  SpQuarterHourSlot,
  SpSumDaySlot,
} from '../models/time-slot.model';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { TranslateService } from '@ngx-translate/core';
import { ShiftUtil } from '../util/shift-util';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { groupBy as _groupBy } from 'lodash-es';

@Injectable({
  providedIn: 'root',
})
export class SpEmployeeService {
  service = inject(SchedulePlanningService);
  eventService = inject(SpEventService);
  filterService = inject(SpFilterService);
  settingService = inject(SpSettingService);
  slotService = inject(SpSlotService);
  toasterService = inject(ToasterService);
  translate = inject(TranslateService);

  employees = signal<PlanningEmployeeDTO[]>([]);
  employedEmployees = signal<PlanningEmployeeDTO[]>([]);

  visibleEmployees = computed(() => {
    return this.employees().filter(e => e.isVisible);
  });

  showInactive = signal(false);
  showAvailability = signal(true);
  includeSecondaryCategoriesOrAccounts = signal(false);
  loadingEmployees = signal(false);

  loadingEmployeeIdsChanged = new BehaviorSubject<number[]>([]);
  loadingShiftsForEmployeeIdsChanged = new BehaviorSubject<number[]>([]);
  shiftsForEmployeeIdsChanged = new BehaviorSubject<number[]>([]);

  selectedEmployeeChanged = new BehaviorSubject<
    PlanningEmployeeDTO | undefined
  >(undefined);

  selectedSlotChanged = new BehaviorSubject<
    | SpEmployeeDaySlot
    | SpEmployeeHourSlot
    | SpEmployeeHalfHourSlot
    | SpEmployeeQuarterHourSlot
    | undefined
  >(undefined);

  loadEmployees(employeeIds: number[]): Observable<IEmployeeListDTO[]> {
    this.loadingEmployees.set(true);
    this.loadingEmployeeIdsChanged.next(employeeIds);

    // If reloading one or two employees, show toaster message
    if (employeeIds.length === 1 || employeeIds.length === 2) {
      employeeIds.forEach(employeeId => {
        const emp = this.employees().find(e => e.employeeId === employeeId);
        if (emp) {
          // TODO: Create new term
          this.toasterService.info(
            `Laddar om anställd ${emp.numberAndName}...`
          );
        }
      });
    }

    return this.service
      .getEmployees(
        employeeIds,
        !employeeIds || employeeIds.length === 0,
        this.showInactive(),
        true,
        this.showAvailability(),
        this.filterService.dateFrom(),
        this.filterService.dateTo(),
        this.includeSecondaryCategoriesOrAccounts(),
        TimeSchedulePlanningDisplayMode.Admin
      )
      .pipe(
        tap((data: IEmployeeListDTO[]) => {
          let hasAddedEmployee = false;
          const employees = this.createPlanningEmployees(data);
          if (employeeIds.length > 0) {
            employees.forEach(employee => {
              if (!this.employeeExists(employee.employeeId)) {
                // Employee not loaded before, add it
                this.employees.update(emps => {
                  return [...emps, employee];
                });
                hasAddedEmployee = true;
              } else {
                // Employee already loaded, update it
                this.employees.update(emps =>
                  emps.map(e =>
                    e.employeeId === employee.employeeId ? employee : e
                  )
                );
              }
            });
          } else {
            // No employee ids specified, replace all employees
            this.employees.set(employees);
            hasAddedEmployee = true;
          }

          const employedEmployeesChanged = this.setEmployedEmployees();

          // Only need to sort if employees have been added or changed
          if (hasAddedEmployee || employedEmployeesChanged)
            this.sortEmployees();

          this.loadingEmployees.set(false);
          this.loadingEmployeeIdsChanged.next([]);
        })
      );
  }

  createPlanningEmployees(
    employees: IEmployeeListDTO[]
  ): PlanningEmployeeDTO[] {
    const showHiddenShifts = this.filterService.showHiddenShifts();

    const e: PlanningEmployeeDTO[] = employees.map(item => {
      const obj = new PlanningEmployeeDTO();
      Object.assign(obj, item);

      obj.setEmployeeNrSort();
      obj.setTypes();

      // Create slots
      obj.daySlots = [];
      this.slotService.daySlots().forEach(daySlot => {
        const eDaySlot = this.slotService.createEmployeeDaySlot(
          daySlot.start,
          obj.employeeId,
          obj.numberAndName
        );
        eDaySlot.shifts = [];
        eDaySlot.setFilteredShifts(showHiddenShifts);

        // Availability for whole day
        // Used both for schedule view and day view
        this.setAvailabilityOnDaySlot(obj, daySlot, eDaySlot);

        // Create hour slots in day view
        if (this.filterService.isCommonDayView()) {
          eDaySlot.hourSlots = [];
          eDaySlot.halfHourSlots = [];
          eDaySlot.quarterHourSlots = [];

          switch (this.settingService.dayViewMinorTickLength()) {
            case TermGroup_StaffingNeedsHeadInterval.SixtyMinutes:
              this.slotService.hourSlots().forEach(hourSlot => {
                const eHourSlot = this.slotService.createEmployeeHourSlot(
                  hourSlot.start,
                  obj.employeeId,
                  obj.numberAndName
                );
                // Availability for each hour slot
                // Day slot is used to check availability for the whole day
                this.setAvailabilityOnHourSlot(
                  obj,
                  eDaySlot,
                  hourSlot,
                  eHourSlot
                );
                eDaySlot.hourSlots.push(eHourSlot);
              });
              break;
            case TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes:
              this.slotService.halfHourSlots().forEach(halfHourSlot => {
                const eHalfHourSlot =
                  this.slotService.createEmployeeHalfHourSlot(
                    halfHourSlot.start,
                    obj.employeeId,
                    obj.numberAndName
                  );
                // Availability for each half hour slot
                // Day slot is used to check availability for the whole day
                this.setAvailabilityOnHourSlot(
                  obj,
                  eDaySlot,
                  halfHourSlot,
                  eHalfHourSlot
                );
                eDaySlot.halfHourSlots.push(eHalfHourSlot);
              });
              break;
            case TermGroup_StaffingNeedsHeadInterval.FifteenMinutes:
              this.slotService.quarterHourSlots().forEach(quarterHourSlot => {
                const eQuarterHourSlot =
                  this.slotService.createEmployeeQuarterHourSlot(
                    quarterHourSlot.start,
                    obj.employeeId,
                    obj.numberAndName
                  );
                // Availability for each quarter hour slot
                // Day slot is used to check availability for the whole day
                this.setAvailabilityOnHourSlot(
                  obj,
                  eDaySlot,
                  quarterHourSlot,
                  eQuarterHourSlot
                );
                eDaySlot.quarterHourSlots.push(eQuarterHourSlot);
              });
              break;
          }
        }

        // If employee already has shifts, reuse them
        const existingShifts = this.getEmployeeShifts(obj.employeeId);
        if (existingShifts.length > 0) {
          this.setShiftsOnEmployee(obj, existingShifts);
        }

        obj.daySlots.push(eDaySlot);
      });

      return obj;
    });

    return e;
  }

  get activeEmployees(): PlanningEmployeeDTO[] {
    if (this.settingService.showInactiveEmployees()) return this.employees();
    else return this.employees().filter(e => e.active);
  }

  setEmployedEmployees(): boolean {
    // Set employed employees based on current date range
    const employedEmployees = [];
    for (const employee of this.activeEmployees) {
      if (
        this.settingService.showUnemployedEmployees() ||
        employee.isGroupHeader ||
        employee.hasEmployment(
          this.filterService.dateFrom(),
          this.filterService.dateTo()
        )
      )
        employedEmployees.push(employee);
    }

    // Check if employed employees have changed
    let employedEmployeesChanged = false;
    const preEmployedEmployeeIds = this.employedEmployees().map(
      e => e.employeeId
    );
    const postEmployedEmployeeIds = employedEmployees.map(e => e.employeeId);
    if (
      !NumberUtil.compareArrays(preEmployedEmployeeIds, postEmployedEmployeeIds)
    )
      employedEmployeesChanged = true;

    if (employedEmployeesChanged) {
      this.employedEmployees.set(employedEmployees);
      this.sortEmployees();

      // Reload information based on employed employees
      if (
        this.settingService.showAvailability() &&
        preEmployedEmployeeIds.length > 0 &&
        postEmployedEmployeeIds.length > 0
      ) {
        // Reload Employee availability
        this.loadEmployeeAvailability().subscribe(() => {});
      }
    }

    return employedEmployeesChanged;
  }

  private loadEmployeeAvailability() {
    const employeeIds: number[] =
      this.employedEmployees().map(e => e.employeeId) || [];

    this.loadingEmployeeIdsChanged.next(employeeIds);

    return this.service.getEmployeeAvailability(employeeIds).pipe(
      tap(x => {
        x.forEach(employee => {
          const existingEmployee = this.getEmployee(employee.employeeId);
          if (existingEmployee) {
            existingEmployee.available = employee.available.map(a => {
              const avail = new EmployeeListAvailabilityDTO(a.start, a.stop);
              Object.assign(avail, a);
              return avail;
            });
            existingEmployee.unavailable = employee.unavailable.map(a => {
              const avail = new EmployeeListAvailabilityDTO(a.start, a.stop);
              Object.assign(avail, a);
              return avail;
            });
          }
        });

        this.loadingEmployeeIdsChanged.next([]);
      })
    );
  }

  createEmployeeToolTips(onlyVisible = true): void {
    if (onlyVisible) {
      this.visibleEmployees().forEach(employee => {
        this.createEmployeeToolTip(employee);
      });
    } else {
      this.employees().forEach(employee => {
        this.createEmployeeToolTip(employee);
      });
    }
  }

  private createEmployeeToolTip(employee: PlanningEmployeeDTO): void {
    let toolTip = '';

    // Employee number and name
    toolTip +=
      employee.hidden || employee.employeePostId
        ? employee.name
        : employee.numberAndName;

    // Inactive
    if (!employee.active)
      toolTip += ` ${this.translate.instant('common.inactive')}`;

    // Description (Employee post)
    if (employee.description) toolTip += `\n${employee.description}`;

    toolTip += '\n';

    // Employee group
    if (!employee.hidden && this.settingService.showEmployeeGroup())
      toolTip += `${employee.employeeGroupName}\n`;

    // Net time
    toolTip += `\n${this.translate.instant('time.schedule.planning.nettime')}: ${DateUtil.minutesToTimeSpan(employee.totalNetTime)}`;

    // Factor time
    if (employee.totalFactorTime != 0) {
      toolTip += `\n${this.translate.instant('time.schedule.planning.scheduletypefactortime')}: ${DateUtil.minutesToTimeSpan(employee.totalFactorTime)}`;
    }

    if (!employee.hidden) {
      if (this.filterService.isCommonScheduleView()) {
        // Work time week
        toolTip += `\n${this.translate.instant('time.schedule.planning.worktimeweek')}: ${DateUtil.minutesToTimeSpan(employee.workTimeMinutes)}`;
        if (this.filterService.nbrOfDays() > 7) {
          toolTip += ` (${DateUtil.minutesToTimeSpan(employee.oneWeekWorkTimeMinutes)})`;
        }

        // Cycle planned time
        if (
          this.filterService.isScheduleView() &&
          this.settingService.showCyclePlannedTime()
        ) {
          toolTip += `\n\n${this.translate.instant('time.schedule.planning.cycletime.total')}: ${DateUtil.minutesToTimeSpan(employee.cyclePlannedMinutes)}`;
          toolTip += `\n${this.translate.instant('time.schedule.planning.cycletime.average')}: ${DateUtil.minutesToTimeSpan(employee.cyclePlannedAverageMinutes)}`;
        }
      }

      // Gross time
      if (this.settingService.showGrossTime())
        toolTip += `\n${this.translate.instant('time.schedule.planning.grosstime')}: ${DateUtil.minutesToTimeSpan(employee.totalGrossTime)}`;

      // Cost
      if (this.settingService.showTotalCostIncEmpTaxAndSuppCharge())
        toolTip += `\n${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(employee.totalCostIncEmpTaxAndSuppCharge, 0)}`;
      else if (this.settingService.showTotalCost())
        toolTip += `\n${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(employee.totalCost, 0)}`;
    }

    // TODO: Annual leave not implemented
    // Annual leave balance
    // if (
    //   (this.filterService.isScheduleView() ||
    //     this.filterService.isDayView()) &&
    //   this.settingService.showAnnualLeaveBalance()
    // ) {
    //   toolTip += `\n\n${this.translate.instant('time.schedule.planning.annualleave.balance')}: `;
    //   switch (this.settingService.showAnnualLeaveBalanceFormat()) {
    //     case TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days:
    //       toolTip += `${employee.getAnnualLeaveBalanceValue(
    //         TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days,
    //         this.translate.instant('core.time.day').toLocaleLowerCase(),
    //         this.translate.instant('core.time.days').toLocaleLowerCase()
    //       )} (${employee.getAnnualLeaveBalanceValue(
    //         TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours
    //       )})`;
    //       break;
    //     case TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours:
    //       toolTip += `${employee.getAnnualLeaveBalanceValue(
    //         TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Hours
    //       )} (${employee.getAnnualLeaveBalanceValue(
    //         TermGroup_TimeSchedulePlanningAnnualLeaveBalanceFormat.Days,
    //         this.translate.instant('core.time.day').toLocaleLowerCase(),
    //         this.translate.instant('core.time.days').toLocaleLowerCase()
    //       )})`;
    //       break;
    //   }
    // }

    // Template schedules
    if (
      this.filterService.isTemplateView() ||
      this.filterService.isEmployeePostView()
    ) {
      toolTip += '\n\n';
      if (!employee.hasTemplateSchedules)
        toolTip += this.translate.instant(
          'time.schedule.planning.notemplateschedule'
        );
      else {
        toolTip += `${
          employee.templateSchedules.length === 1
            ? this.translate.instant('time.schedule.planning.templateschedule')
            : this.translate.instant('time.schedule.planning.templateschedules')
        }:`;

        employee.templateSchedules.forEach(template => {
          let noOfWeeks: number = template.noOfDays / 7;
          if (noOfWeeks < 1) noOfWeeks = 1;

          // Name
          toolTip += '\n';
          if (template.name) toolTip += `${template.name}, `;

          // Start date
          if (
            template.startDate &&
            !template.name.endsWithCaseInsensitive(
              template.startDate.toFormattedDate()
            )
          ) {
            toolTip += template.startDate.toFormattedDate();
          }

          // Stop date
          if (template.stopDate) {
            toolTip += ` - ${template.stopDate.toFormattedDate()}`;
          }

          if (!toolTip.endsWithCaseInsensitive(', ')) toolTip += ', ';

          // Number of weeks
          toolTip += `${noOfWeeks}${this.translate.instant('common.weekshort')}`;

          // Group
          if (template.timeScheduleTemplateGroupId)
            toolTip += ` (${this.translate.instant(
              'time.schedule.templategroup.templategroup'
            )}: ${template.timeScheduleTemplateGroupName})`;
        });
      }
    }

    employee.toolTip = toolTip;
  }

  setShiftsOnEmployees(shifts: PlanningShiftDTO[]) {
    const showHiddenShifts = this.filterService.showHiddenShifts();

    const employeeIds: number[] = [];

    // Group shifts by employeeId and assign them to the corresponding employee
    const groupedShifts = _groupBy(shifts, 'employeeId');
    Object.entries(groupedShifts).forEach(([employeeId, empShifts]) => {
      const employee = this.getEmployee(+employeeId);
      if (employee) {
        this.setShiftsOnEmployee(employee, empShifts, showHiddenShifts);
        employeeIds.push(employee.employeeId);
      }
    });

    this.shiftsForEmployeeIdsChanged.next(employeeIds);
  }

  private setShiftsOnEmployee(
    employee: PlanningEmployeeDTO,
    shifts: PlanningShiftDTO[],
    showHiddenShifts?: boolean
  ) {
    // If showHiddenShifts is not provided, get setting from filterService
    if (showHiddenShifts === undefined) {
      showHiddenShifts = this.filterService.showHiddenShifts();
    }

    employee.daySlots.forEach(eDaySlot => {
      // Get shifts for current date

      if (this.filterService.isCommonDayView()) {
        // Include shifts that start outside visible range but ends inside
        eDaySlot.shifts = shifts.filter(
          s =>
            s.actualStartTime.isBeforeOnMinute(this.filterService.dateTo()) &&
            s.actualStopTime.isSameOrAfterOnMinute(
              this.filterService.dateFrom()
            )
        );
      } else {
        // Include shifts that starts before first day in visible range but ends on first day
        // Include shifts that starts on the last day in visible range but belongs to next day
        eDaySlot.shifts = shifts.filter(
          s =>
            s.actualStartDate.isSameDay(eDaySlot.start) ||
            (s.actualStopTime.isSameDay(eDaySlot.start) &&
              s.actualStartTime.isBeforeOnMinute(
                this.filterService.dateFrom()
              )) ||
            (s.actualStartTime.isSameDay(eDaySlot.start) &&
              eDaySlot.start.isSameDay(this.filterService.dateTo()))
        );
      }
      eDaySlot.setFilteredShifts(showHiddenShifts);
    });
  }

  /*
   Calculates the maximum number of overlapping shifts per minor time cell (hour / half-hour / quarter-hour)
   for the specified employee (day view only – first visible day).
   Side effects:
     - Sets rowNbr on each shift (stacking index, zero-based).
     - Sets positionTop for rendering.
   Row assignment rules:
     - Shifts continuing from previous cell keep their row.
     - New shifts are appended after the highest used row (no compaction).
     - Midnight reuse logic NOT applied in day view (allowMidnightReuse = false).
   Result:
     - Returns max number of simultaneous visible shifts in any minor time slot.
  */
  getMaxNbrOfShiftsPerDayForDayView(employeeId: number): number {
    let maxPerDay = 0;
    const employee = this.getEmployee(employeeId);
    if (!employee || !employee.daySlots.length) return maxPerDay;

    // Only the first day slot is relevant in day view
    const daySlot = employee.daySlots[0];
    const allShifts = daySlot.shifts;
    if (!allShifts.length) return maxPerDay;

    // Map of templateBlockId -> row index for shifts that continue into the next slot
    let previousSlotRows = new Map<number, number>();

    // Minor tick granularity determines which slots we iterate
    const minorTick = this.settingService.dayViewMinorTickLength();
    let slotStarts: Date[] = [];
    switch (minorTick) {
      case TermGroup_StaffingNeedsHeadInterval.SixtyMinutes:
        slotStarts = this.slotService.hourSlots().map(s => s.start);
        break;
      case TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes:
        slotStarts = this.slotService.halfHourSlots().map(s => s.start);
        break;
      case TermGroup_StaffingNeedsHeadInterval.FifteenMinutes:
        slotStarts = this.slotService.quarterHourSlots().map(s => s.start);
        break;
    }

    slotStarts.forEach(slotStart => {
      // Inclusive end (subtract one second to avoid overlap edge off-by-one)
      const slotEnd = slotStart.addMinutes(minorTick).addSeconds(-1);

      // All shifts intersecting current slot time span
      const overlapping = this.getOverlappingShifts(
        allShifts,
        slotStart,
        slotEnd
      );

      // continuing = shifts already assigned a row in previous slot
      const continuing = this.getContinuingShifts(
        overlapping,
        previousSlotRows
      );
      // newNow = first appearance of shift in this slot sequence
      const newNow = this.getNewShifts(overlapping, previousSlotRows);

      // Tracks which row indices are occupied in this slot
      const used = new Set<number>();
      // For day view: only set row if not previously set (alwaysSetRow=false)
      this.assignContinuingRows(continuing, previousSlotRows, used, false);

      // Compute starting row for new shifts (append after highest existing)
      const startIndex = this.computeStartIndexForNew(used, newNow, false);
      this.assignNewRows(newNow, used, startIndex);

      // Update maximum parallel shifts (size = highestRow + 1)
      if (used.size) {
        const slotMax = Math.max(...used) + 1;
        if (slotMax > maxPerDay) maxPerDay = slotMax;
      }

      // Prepare mapping for next minor slot: only shifts that extend beyond slotEnd
      previousSlotRows = this.buildContinuationMap(overlapping, slotEnd);
    });

    return maxPerDay;
  }

  /*
   Calculates the maximum number of overlapping shifts per day (schedule view, spans multiple days).
   Side effects:
     - Assigns rowNbr and positionTop for every shift appearance (continuing shifts always reset row).
   Differences from day view:
     - Iterates whole days (employee.daySlots).
     - allowMidnightReuse = true: If a new shift crosses midnight we reuse one row (startIndex--).
     - alwaysSetRow = true for continuing shifts.
   Result:
     - Returns max number of simultaneous visible shifts in any day span.
  */
  getMaxNbrOfShiftsPerDayForScheduleView(employeeId: number): number {
    let maxPerDay = 0;
    const employee = this.getEmployee(employeeId);
    if (!employee) return maxPerDay;

    // All shifts across all visible days (some may span multiple days)
    const allShifts = employee.daySlots.flatMap(ds => ds.shifts);

    // Map of templateBlockId -> row index for shifts continuing into the next day
    let previousDaySpanningRows = new Map<number, number>();

    employee.daySlots.forEach(daySlot => {
      const dayStart = daySlot.start.beginningOfDay();
      const dayEnd = daySlot.start.endOfDay();

      // All shifts overlapping this day (part or whole)
      const overlapping = this.getOverlappingShifts(
        allShifts,
        dayStart,
        dayEnd
      );

      // continuing = shifts that started earlier and still span into this day
      const continuing = this.getContinuingShifts(
        overlapping,
        previousDaySpanningRows
      );
      // newToday = shifts first visible in this day
      const newToday = this.getNewShifts(overlapping, previousDaySpanningRows);

      // Track rows used this day
      const used = new Set<number>();
      // For schedule view: always reapply row/position (alwaysSetRow=true)
      this.assignContinuingRows(
        continuing,
        previousDaySpanningRows,
        used,
        true
      );

      // Midnight reuse: if first new shift crosses midnight, reuse last row
      const startIndex = this.computeStartIndexForNew(used, newToday, true);
      this.assignNewRows(newToday, used, startIndex);

      // Update global max
      if (used.size) {
        const dayMax = Math.max(...used) + 1;
        if (dayMax > maxPerDay) maxPerDay = dayMax;
      }

      // Build continuation mapping for next day
      previousDaySpanningRows = this.buildContinuationMap(overlapping, dayEnd);
    });

    return maxPerDay;
  }

  // Returns shifts overlapping the inclusive time span [spanStart, spanEnd]
  private getOverlappingShifts(
    shifts: PlanningShiftDTO[],
    spanStart: Date,
    spanEnd: Date
  ): PlanningShiftDTO[] {
    return shifts.filter(
      s =>
        s.actualStartTime.isBeforeOnMinute(spanEnd) && // starts before end
        s.actualStopTime.isAfterOnMinute(spanStart) // ends after start
    );
  }

  // Extracts shifts continuing from previous segment (present in previousMap)
  private getContinuingShifts(
    overlapping: PlanningShiftDTO[],
    previousMap: Map<number, number>
  ): PlanningShiftDTO[] {
    return overlapping.filter(s =>
      previousMap.has(s.timeScheduleTemplateBlockId)
    );
  }

  // New shifts first visible in current segment (not seen in previousMap).
  // Sorted deterministically by start then stop for stable row allocation.
  private getNewShifts(
    overlapping: PlanningShiftDTO[],
    previousMap: Map<number, number>
  ): PlanningShiftDTO[] {
    return overlapping
      .filter(s => !previousMap.has(s.timeScheduleTemplateBlockId))
      .sort(ShiftUtil.sortShiftsByStartThenStop);
  }

  /*
   Assigns existing row indices to continuing shifts.
   alwaysSetRow:
     - false (day view): only set if rowNbr not previously assigned.
     - true (schedule view): reassert rowNbr & positionTop each day.
   Adds each occupied row to 'used'.
  */
  private assignContinuingRows(
    continuing: PlanningShiftDTO[],
    previousMap: Map<number, number>,
    used: Set<number>,
    alwaysSetRow: boolean
  ): void {
    continuing.forEach(shift => {
      const row = previousMap.get(shift.timeScheduleTemplateBlockId)!;
      if (alwaysSetRow || shift.rowNbr === undefined) {
        shift.rowNbr = row;
        this.setPositionTop(shift, row);
      }
      used.add(row);
    });
  }

  /*
   Determines the starting row index for new shifts.
   Logic:
     - If rows already used: next = maxUsed + 1
     - Else: start at 0
     - If allowMidnightReuse and first new shift spans into next day, reuse last row (startIndex--).
       This reduces vertical growth for overnight shifts starting before midnight and ending after.
  */
  private computeStartIndexForNew(
    used: Set<number>,
    newShifts: PlanningShiftDTO[],
    allowMidnightReuse: boolean
  ): number {
    let startIndex = used.size ? Math.max(...used) + 1 : 0;
    if (
      allowMidnightReuse &&
      startIndex > 0 &&
      newShifts.length > 0 &&
      newShifts[0].actualStopTime.isAfterOnDay(newShifts[0].actualStartTime)
    ) {
      // Reuse last existing row for first cross-midnight shift
      startIndex--;
    }
    return startIndex;
  }

  /*
   Assigns sequential rows to each new shift starting at startIndex.
   No gap filling: rows only grow upward.
  */
  private assignNewRows(
    newShifts: PlanningShiftDTO[],
    used: Set<number>,
    startIndex: number
  ): void {
    let next = startIndex;
    newShifts.forEach(shift => {
      shift.rowNbr = next;
      this.setPositionTop(shift, next);
      used.add(next);
      next++;
    });
  }

  /*
   Builds mapping for next segment:
     - Only includes shifts whose stop time exceeds current segment end.
     - Maps templateBlockId -> assigned rowNbr (for continuity).
  */
  private buildContinuationMap(
    overlapping: PlanningShiftDTO[],
    spanEnd: Date
  ): Map<number, number> {
    const map = new Map<number, number>();
    overlapping.forEach(shift => {
      if (shift.actualStopTime.getTime() > spanEnd.getTime()) {
        map.set(shift.timeScheduleTemplateBlockId, shift.rowNbr);
      }
    });
    return map;
  }

  /*
   Sets vertical position (top) for a shift based on its row number.
   Height differs for day view vs schedule view (half vs full height).
  */
  private setPositionTop(shift: PlanningShiftDTO, rowNbr: number) {
    const heightPerShift = this.filterService.isCommonDayView()
      ? ShiftUtil.SHIFT_HALF_HEIGHT + ShiftUtil.SHIFT_HALF_HEIGHT_MARGIN
      : ShiftUtil.SHIFT_FULL_HEIGHT + ShiftUtil.SHIFT_FULL_HEIGHT_MARGIN;

    shift.positionTop = rowNbr * heightPerShift;
  }

  /*
   End of methods for calculating maximum number of overlapping shifts per cell
  */

  private setAvailabilityOnDaySlot(
    employee: PlanningEmployeeDTO,
    slot: SpDaySlot,
    eSlot: SpEmployeeDaySlot
  ): void {
    const slotStartTime = slot.start.beginningOfDay();
    const slotStopTime = slot.start.endOfDay();

    // Available
    // Check available within current slot
    employee.available
      .filter(
        a =>
          a.start.isSameOrBeforeOnDay(slotStopTime) &&
          a.stop.isSameOrAfterOnDay(slotStartTime)
      )
      .forEach(a => {
        if (employee.isFullyAvailableInRange(slotStartTime, slotStopTime)) {
          eSlot.isFullyAvailable = true;
        } else if (
          employee.isPartiallyAvailableInRange(slotStartTime, slotStopTime)
        ) {
          eSlot.isPartiallyAvailable = true;
        }
      });

    // Unavailable
    // Check unavailable within current slot
    employee.unavailable
      .filter(
        a =>
          a.start.isSameOrBeforeOnDay(slotStopTime) &&
          a.stop.isSameOrAfterOnDay(slotStartTime)
      )
      .forEach(a => {
        if (employee.isFullyUnavailableInRange(slotStartTime, slotStopTime)) {
          eSlot.isFullyUnavailable = true;
        } else if (
          employee.isPartiallyUnavailableInRange(slotStartTime, slotStopTime)
        ) {
          eSlot.isPartiallyUnavailable = true;
        }
      });
    this.setAvailabilityText(employee, eSlot);
  }

  private setAvailabilityOnHourSlot(
    employee: PlanningEmployeeDTO,
    eDaySlot: SpEmployeeDaySlot,
    slot: SpHourSlot | SpHalfHourSlot | SpQuarterHourSlot,
    eSlot:
      | SpEmployeeHourSlot
      | SpEmployeeHalfHourSlot
      | SpEmployeeQuarterHourSlot
  ): void {
    const slotStartTime = slot.start;
    const slotStopTime = slot.start
      .addMinutes(this.settingService.dayViewMinorTickLength())
      .addSeconds(-1);

    // Available
    // Check available within current slot
    employee.available
      .filter(
        a =>
          a.start.isSameOrBeforeOnMinute(slotStopTime) &&
          a.stop.isSameOrAfterOnMinute(slotStartTime)
      )
      .forEach(a => {
        if (employee.isFullyAvailableInRange(slotStartTime, slotStopTime)) {
          eSlot.isFullyAvailable = true;
        } else if (
          employee.isPartiallyAvailableInRange(slotStartTime, slotStopTime)
        ) {
          eSlot.isPartiallyAvailable = true;
        }
      });

    // Unavailable
    // Check unavailable within current slot
    employee.unavailable
      .filter(
        a =>
          a.start.isSameOrBeforeOnMinute(slotStopTime) &&
          a.stop.isSameOrAfterOnMinute(slotStartTime)
      )
      .forEach(a => {
        if (employee.isFullyUnavailableInRange(slotStartTime, slotStopTime)) {
          eSlot.isFullyUnavailable = true;
        } else if (
          employee.isPartiallyUnavailableInRange(slotStartTime, slotStopTime)
        ) {
          eSlot.isPartiallyUnavailable = true;
        }
      });

    this.setAvailabilityText(employee, eDaySlot, eSlot);
  }

  private setAvailabilityText(
    employee: PlanningEmployeeDTO,
    eDaySlot: SpEmployeeDaySlot,
    eHourSlot?: SpEmployeeHourSlot
  ): void {
    // Set text for availability

    const daySlotStartTime = eDaySlot.start.beginningOfDay();
    const daySlotStopTime = eDaySlot.start.endOfDay();

    const hourSlotStartTime = eHourSlot?.start;
    // TODO: Hard coded to one hour slot, need to be able to specify 15 or 30 min slots
    const hourSlotStopTime = eHourSlot?.start
      .addMinutes(this.settingService.dayViewMinorTickLength())
      .addSeconds(-1);

    // Available
    let availableText = '';
    const availableTimesForDay = employee.getAvailableInRange(
      daySlotStartTime,
      daySlotStopTime
    );
    const availableTimesForHour =
      hourSlotStartTime && hourSlotStopTime
        ? employee.getAvailableInRange(hourSlotStartTime, hourSlotStopTime)
        : [];
    if (eDaySlot.isFullyAvailable) {
      availableText = this.createTextForFullAvailability(
        availableTimesForDay,
        true
      );
    } else if (eDaySlot.isPartiallyAvailable) {
      if (eHourSlot) {
        if (availableTimesForHour.length > 0) {
          availableTimesForHour.forEach(a => {
            availableText += this.createTextForPartialAvailability(a, true);
          });
        }
      } else {
        if (availableTimesForDay.length > 0) {
          availableTimesForDay.forEach(a => {
            availableText += this.createTextForPartialAvailability(a, true);
          });
        }
      }
    }

    if (eHourSlot) eHourSlot.availableText = availableText;
    else eDaySlot.availableText = availableText;

    // Unavailable
    let unavailableText = '';
    const unavailableTimesForDay = employee.getUnavailableInRange(
      daySlotStartTime,
      daySlotStopTime
    );
    const unavailableTimesForHour =
      hourSlotStartTime && hourSlotStopTime
        ? employee.getUnavailableInRange(hourSlotStartTime, hourSlotStopTime)
        : [];
    if (eDaySlot.isFullyUnavailable) {
      unavailableText = this.createTextForFullAvailability(
        unavailableTimesForDay,
        false
      );
    } else if (eDaySlot.isPartiallyUnavailable) {
      if (eHourSlot) {
        if (unavailableTimesForHour.length > 0) {
          unavailableTimesForHour.forEach(u => {
            unavailableText += this.createTextForPartialAvailability(u, false);
          });
        }
      } else {
        if (unavailableTimesForDay.length > 0) {
          unavailableTimesForDay.forEach(u => {
            unavailableText += this.createTextForPartialAvailability(u, false);
          });
        }
      }
    }

    if (eHourSlot) {
      eHourSlot.unavailableText = unavailableText;
      eHourSlot.availabilityTooltip =
        (eHourSlot.availableText.length > 0
          ? eHourSlot.availableText + '\n'
          : '') +
        (eHourSlot.unavailableText.length > 0 ? eHourSlot.unavailableText : '');
    } else {
      eDaySlot.unavailableText = unavailableText;
      eDaySlot.availabilityTooltip =
        (eDaySlot.availableText.length > 0
          ? eDaySlot.availableText + '\n'
          : '') +
        (eDaySlot.unavailableText.length > 0 ? eDaySlot.unavailableText : '');
    }
  }

  private createTextForFullAvailability(
    availability: EmployeeListAvailabilityDTO[],
    isAvailable: boolean
  ): string {
    let text = this.translate.instant(
      isAvailable
        ? 'time.schedule.planning.available'
        : 'time.schedule.planning.unavailable'
    );

    if (availability.length > 0 && availability[0].comment) {
      text += `, ${availability[0].comment}\n`;
    }

    return text;
  }

  private createTextForPartialAvailability(
    availability: EmployeeListAvailabilityDTO,
    isAvailable: boolean
  ): string {
    let text = `${this.translate.instant(isAvailable ? 'time.schedule.planning.available' : 'time.schedule.planning.unavailable')} ${availability.start.toFormattedTime()}-${availability.stop.toFormattedTime()}`;

    if (availability.comment) text += `, ${availability.comment}`;

    text += '\n';

    return text;
  }

  sortEmployees(): void {
    if (this.employees().length === 0) return;

    // Sort employees by setting
    if (this.filterService.isCommonDayView()) {
      const sort = this.filterService.sortEmployeesByForDayView();
      const groupBy = this.filterService.groupEmployeesByForDayView();

      this.employees.set([
        ...this.employees().sort((a, b) => {
          switch (sort) {
            case TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname:
              if (
                groupBy ===
                TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee
              ) {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.firstName.localeCompare(b.firstName) ||
                  a.lastName.localeCompare(b.lastName) ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
              } else {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.firstName.localeCompare(b.firstName) ||
                  a.lastName.localeCompare(b.lastName) ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
                // if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.Category
                // ) {
                //   this.setCategoryOnEmployees();
                // } else if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType
                // ) {
                //   this.setShiftTypeOnEmployees();
                // } else if (groupBy > 10) {
                //   this.setAccountOnEmployees();
                // }
                // this.employedEmployees = _.orderBy(
                //   this.employedEmployees,
                //   [
                //     'hidden',
                //     'vacant',
                //     'groupName',
                //     'firstName',
                //     'lastName',
                //     'employeeNrSort',
                //   ],
                //   ['desc', 'asc']
                // );
              }
            case TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname:
              if (
                groupBy ===
                TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee
              ) {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.lastName.localeCompare(b.lastName) ||
                  a.firstName.localeCompare(b.firstName) ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
              } else {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.lastName.localeCompare(b.lastName) ||
                  a.firstName.localeCompare(b.firstName) ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
                // if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.Category
                // ) {
                //   this.setCategoryOnEmployees();
                // } else if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType
                // ) {
                //   this.setShiftTypeOnEmployees();
                // } else if (groupBy > 10) {
                //   this.setAccountOnEmployees();
                // }
                // this.employedEmployees = _.orderBy(
                //   this.employedEmployees,
                //   [
                //     'hidden',
                //     'vacant',
                //     'groupName',
                //     'lastName',
                //     'firstName',
                //     'employeeNrSort',
                //   ],
                //   ['desc', 'asc']
                // );
              }
            case TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr:
              if (
                groupBy ===
                TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee
              ) {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
              } else {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
                // if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.Category
                // ) {
                //   this.setCategoryOnEmployees();
                // } else if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType
                // ) {
                //   this.setShiftTypeOnEmployees();
                // } else if (groupBy > 10) {
                //   this.setAccountOnEmployees();
                // }
                // this.employedEmployees = _.orderBy(
                //   this.employedEmployees,
                //   ['hidden', 'vacant', 'groupName', 'employeeNrSort'],
                //   ['desc', 'asc']
                // );
              }
            case TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime:
              this.setStartTimeOnEmployees();
              if (
                groupBy ===
                TermGroup_TimeSchedulePlanningDayViewGroupBy.Employee
              ) {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.startTime - b.startTime ||
                  a.stopTime - b.stopTime ||
                  a.firstName.localeCompare(b.firstName) ||
                  a.lastName.localeCompare(b.lastName) ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
              } else {
                return (
                  +b.hidden - +a.hidden ||
                  +a.vacant - +b.vacant ||
                  a.startTime - b.startTime ||
                  a.stopTime - b.stopTime ||
                  a.firstName.localeCompare(b.firstName) ||
                  a.lastName.localeCompare(b.lastName) ||
                  a.employeeNrSort.localeCompare(b.employeeNrSort)
                );
                // if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.Category
                // ) {
                //   this.setCategoryOnEmployees();
                // } else if (
                //   groupBy ===
                //   TermGroup_TimeSchedulePlanningDayViewGroupBy.ShiftType
                // ) {
                //   this.setShiftTypeOnEmployees();
                // } else if (groupBy > 10) {
                //   this.setAccountOnEmployees();
                // }
                // this.employedEmployees = _.orderBy(
                //   this.employedEmployees,
                //   [
                //     'hidden',
                //     'vacant',
                //     'groupName',
                //     'startTime',
                //     'stopTime',
                //     'firstName',
                //     'lastName',
                //     'employeeNrSort',
                //   ],
                //   ['desc', 'asc']
                // );
              }
          }
        }),
      ]);
    } else if (this.filterService.isCommonScheduleView()) {
      // Always sort hidden first, vacant last and then by selected sort
      const sort = this.filterService.sortEmployeesByForScheduleView();
      // The spread operator is used to trigger change detection
      this.employees.set([
        ...this.employees().sort((a, b) => {
          switch (sort) {
            case TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr:
              return (
                +b.hidden - +a.hidden ||
                +a.vacant - +b.vacant ||
                a.employeeNrSort.localeCompare(b.employeeNrSort)
              );
            case TermGroup_TimeSchedulePlanningScheduleViewSortBy.Firstname:
              return (
                +b.hidden - +a.hidden ||
                +a.vacant - +b.vacant ||
                a.firstName.localeCompare(b.firstName) ||
                a.lastName.localeCompare(b.lastName) ||
                a.employeeNrSort.localeCompare(b.employeeNrSort)
              );
            case TermGroup_TimeSchedulePlanningScheduleViewSortBy.Lastname:
              return (
                +b.hidden - +a.hidden ||
                +a.vacant - +b.vacant ||
                a.lastName.localeCompare(b.lastName) ||
                a.firstName.localeCompare(b.firstName) ||
                a.employeeNrSort.localeCompare(b.employeeNrSort)
              );
            default:
              return 0;
          }
        }),
      ]);
    }

    this.createEmployeesDict();
  }

  private setStartTimeOnEmployees() {
    // Clear existing start times on all employees
    this.employees().forEach(employee => {
      employee.startTime = 0;
      employee.stopTime = 0;
    });

    // Loop through employees and add start time for their first shift in current date range
    this.employees().forEach(employee => {
      ShiftUtil.sortShifts(employee.daySlots[0].shifts);

      if (employee.daySlots[0].shifts.length > 0) {
        employee.startTime =
          employee.daySlots[0].shifts[0].actualStartTime.getTime();
        employee.stopTime =
          employee.daySlots[0].shifts[
            employee.daySlots[0].shifts.length - 1
          ].actualStopTime.getTime();
      }
    });
  }

  private createEmployeesDict(): void {
    this.filterService.employeesDict = this.employees().map(x => ({
      id: x.employeeId,
      name: x.numberAndName,
    }));
  }

  getEmployee(employeeId: number): PlanningEmployeeDTO | undefined {
    return this.employees().find(e => e.employeeId === employeeId);
  }

  getEmployeeShifts(employeeId: number, date?: Date): PlanningShiftDTO[] {
    const employee = this.getEmployee(employeeId);
    return employee
      ? employee.getShifts(date, this.filterService.dateTo())
      : [];
  }

  getDaySlotForShift(
    shiftId: number,
    employeeId?: number
  ): SpEmployeeDaySlot | undefined {
    // Find the employee by passed ID or by passed shift
    let employee: PlanningEmployeeDTO | undefined;
    if (employeeId) {
      employee = this.getEmployee(employeeId);
    } else {
      employee = this.employees().find(e =>
        e.daySlots.some(slot =>
          slot.shifts.some(
            shift => shift.timeScheduleTemplateBlockId === shiftId
          )
        )
      );
    }

    if (employee) {
      return employee.daySlots.find(slot =>
        slot.shifts.some(shift => shift.timeScheduleTemplateBlockId === shiftId)
      );
    }

    return undefined;
  }

  private employeeExists(employeeId: number): boolean {
    return this.employees().some(e => e.employeeId === employeeId);
  }

  recalculateEmployeesAndShifts() {
    const shiftCount = this.setShiftsVisibilityForAllEmployees();
    const employeeCount = this.setEmployeesVisibility();
    this.calculateWorkTimesForVisibleEmployees();
    this.calculateTimesForVisibleEmployees();
    this.calculateTimesSummary();
    this.createEmployeeToolTips();
    this.slotService.createSumSlotTooltips();

    // Notify subscribers that employees and shifts have been recalculated
    this.eventService.employeesAndShiftsRecalculated.next({
      allEmployees: employeeCount.allEmployees,
      visibleEmployees: employeeCount.visibleEmployees,
      allShifts: shiftCount.allShifts,
      visibleShifts: shiftCount.visibleShifts,
    });
  }

  setShiftsVisibilityForAllEmployees(): {
    allShifts: number;
    visibleShifts: number;
  } {
    const shiftTypeIds = this.filterService.shiftTypeIds();
    const showHiddenShifts = this.filterService.showHiddenShifts();
    const isFilteredOnShiftType = this.filterService.isFilteredOnShiftType();
    const blockTypes = this.filterService.blockTypes();
    let allCount = 0;
    let visibleCount = 0;

    this.employees().forEach(employee => {
      employee.daySlots.forEach(slot => {
        allCount += slot.shifts.length;
        slot.shifts.forEach(shift => {
          shift.isVisible =
            (!isFilteredOnShiftType ||
              shiftTypeIds.includes(shift.shiftTypeId)) &&
            (!this.filterService.isFilteredOnBlockType() ||
              blockTypes.includes(shift.type));
          if (shift.isVisible) visibleCount++;
        });
        slot.setFilteredShifts(showHiddenShifts);
      });
    });

    return {
      allShifts: allCount,
      visibleShifts: visibleCount,
    };
  }

  setEmployeesVisibility(): {
    allEmployees: number;
    visibleEmployees: number;
  } {
    // Set employee as visible if it has any shifts
    const showAllEmployees = this.filterService.showAllEmployees();
    const isFilteredOnEmployee = this.filterService.isFilteredOnEmployee();
    let visibleCount = 0;
    let isModified = false;
    this.employees().forEach(employee => {
      const visibleByFilter =
        !isFilteredOnEmployee ||
        this.filterService.employeeIds().includes(employee.employeeId);
      const visible =
        visibleByFilter &&
        (showAllEmployees || employee.hidden || employee.hasVisibleShifts);
      if (visible) visibleCount++;

      if (employee.isVisible !== visible) {
        employee.isVisible = visible;
        isModified = true;
      }

      if (this.settingService.showEmployeeGroup() && employee.isVisible) {
        this.setEmployeeGroup(employee);
      }
    });

    if (isModified) this.employees.set([...this.employees()]);

    return {
      allEmployees: this.employees().length,
      visibleEmployees: visibleCount,
    };
  }

  private setEmployeeGroup(employee: PlanningEmployeeDTO): void {
    const employment = employee.getEmployment(
      this.filterService.dateFrom(),
      this.filterService.dateTo()
    );
    employee.employeeGroupName = employment?.employeeGroupName || '';
  }

  getTotalNetTimeForVisibleEmployees(): number {
    return this.visibleEmployees().reduce(
      (sum, employee) => sum + employee.totalNetTime,
      0
    );
  }

  getTotalWorkTimeForVisibleEmployees(): number {
    return this.visibleEmployees().reduce(
      (sum, employee) => sum + employee.workTimeMinutes,
      0
    );
  }

  getTotalFactorTimeForVisibleEmployees(): number {
    return this.visibleEmployees().reduce(
      (sum, employee) => sum + employee.totalFactorTime,
      0
    );
  }

  getTotalGrossTimeForVisibleEmployees(): number {
    return this.visibleEmployees().reduce(
      (sum, employee) => sum + employee.totalGrossTime,
      0
    );
  }

  getTotalCostForVisibleEmployees(): number {
    return this.visibleEmployees().reduce(
      (sum, employee) => sum + employee.totalCost,
      0
    );
  }

  getTotalCostIncEmpTaxAndSuppChargeForVisibleEmployees(): number {
    return this.visibleEmployees().reduce(
      (sum, employee) => sum + employee.totalCostIncEmpTaxAndSuppCharge,
      0
    );
  }

  private calculateWorkTimesForVisibleEmployees() {
    this.visibleEmployees().forEach(employee => {
      this.calculateWorkTimes(employee);
    });
  }

  private calculateWorkTimes(employee: PlanningEmployeeDTO) {
    if (!employee) return;

    const isWholeYear =
      this.filterService.dateFrom().isBeginningOfYear() &&
      this.filterService.dateTo().isEndOfYear();

    if (isWholeYear && employee.annualWorkTimeMinutes !== 0) {
      employee.workTimeMinutes = employee.annualWorkTimeMinutes;
    } else {
      employee.oneWeekWorkTimeMinutes = this.getCurrentWorkTimeWeek(employee);
      if (this.filterService.nbrOfDays() > 7)
        employee.workTimeMinutes =
          (employee.oneWeekWorkTimeMinutes * this.filterService.nbrOfDays()) /
          7;
      else employee.workTimeMinutes = employee.oneWeekWorkTimeMinutes;
    }
    employee.minScheduleTime = this.getCurrentMinScheduleTime(employee);
    employee.maxScheduleTime = this.getCurrentMaxScheduleTime(employee);
  }

  private getCurrentEmployment(
    employee: PlanningEmployeeDTO
  ): EmployeeListEmploymentDTO | undefined {
    return employee.getEmployment(
      this.filterService.dateFrom().beginningOfDay(),
      this.filterService.dateTo().endOfDay()
    );
  }

  private getCurrentWorkTimeWeek(employee: PlanningEmployeeDTO): number {
    const employment = this.getCurrentEmployment(employee);
    return employment?.workTimeWeekMinutes || 0;
  }

  private getCurrentMinScheduleTime(employee: PlanningEmployeeDTO): number {
    const employment = this.getCurrentEmployment(employee);
    return employment?.minScheduleTime || 0;
  }

  private getCurrentMaxScheduleTime(employee: PlanningEmployeeDTO): number {
    const employment = this.getCurrentEmployment(employee);
    return employment?.maxScheduleTime || 0;
  }

  private calculateTimesForVisibleEmployees(): void {
    this.visibleEmployees().forEach(employee => {
      this.calculateTimes(employee);
    });
  }

  private calculateTimes(employee: PlanningEmployeeDTO): void {
    if (this.filterService.isCommonDayView()) {
      this.calculateDailyTimesAndCosts(employee);
      this.calculateNbrOfShiftsPerHour(employee);
    } else if (this.filterService.isCommonScheduleView()) {
      this.calculateDailyTimesAndCosts(employee);
    }
    this.setUnderOrOverTime(employee);
  }

  private calculateNbrOfShiftsPerHour(employee: PlanningEmployeeDTO) {
    employee.daySlots.forEach(daySlot => {
      // Get shifts for current date
      const shifts = this.getShiftsToCalculateForDaySlot(daySlot);

      // TODO: Grouped shifts not implemented

      switch (this.settingService.dayViewMinorTickLength()) {
        case TermGroup_StaffingNeedsHeadInterval.SixtyMinutes:
          // Loop over all hour slots and calculate nbr of shifts each hour separately
          daySlot.hourSlots.forEach(slot => {
            slot.nbrOfShifts = this.getNbrOfShiftsForHourSlot(
              shifts,
              slot.start
            );
          });
          break;
        case TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes:
          // Loop over all half hour slots and calculate nbr of shifts each half hour separately
          daySlot.halfHourSlots.forEach(slot => {
            slot.nbrOfShifts = this.getNbrOfShiftsForHourSlot(
              shifts,
              slot.start
            );
          });
          break;
        case TermGroup_StaffingNeedsHeadInterval.FifteenMinutes:
          // Loop over all fifteen minute slots and calculate nbr of shifts each fifteen minute slot separately
          daySlot.quarterHourSlots.forEach(slot => {
            slot.nbrOfShifts = this.getNbrOfShiftsForHourSlot(
              shifts,
              slot.start
            );
          });
          break;
      }
    });
  }

  private getNbrOfShiftsForHourSlot(
    shifts: PlanningShiftDTO[],
    slotStartTime: Date
  ): number {
    let nbrOfShifts = 0;

    const slotStopTime = slotStartTime.addMinutes(
      this.settingService.dayViewMinorTickLength()
    );

    const shiftsForSlot = shifts.filter(
      s =>
        s.actualStartTime.isBeforeOnMinute(slotStopTime) &&
        s.actualStopTime.isAfterOnMinute(slotStartTime) &&
        !s.timeDeviationCauseId
    );

    shiftsForSlot.forEach(shift => {
      // Check each shift in time slot to see if there are any breaks that span over the whole slot.
      // In that case do not count it.
      let hasBreak = false;
      shift.breaks.forEach(brk => {
        if (
          brk.startTime.isSameOrBeforeOnMinute(slotStartTime) &&
          brk.stopTime.isSameOrAfterOnMinute(slotStopTime)
        ) {
          // The break spans over the whole slot, so do not count this shift
          hasBreak = true;
        }
      });

      if (!hasBreak) {
        nbrOfShifts++;
      }
    });

    return nbrOfShifts;
  }

  private calculateDailyTimesAndCosts(employee: PlanningEmployeeDTO) {
    employee.clearTimeAndCosts();

    // Loop over all day slots and calculate time and costs for each day separately
    employee.daySlots.forEach(eDaySlot => {
      // Get shifts for current date
      const shifts = this.getShiftsToCalculateForDaySlot(eDaySlot);

      shifts.forEach(shift => {
        // Make sure shift is valid for calculation
        if (this.isShiftValidForCalculation(shift, employee)) {
          const date: Date = shift.actualStartDate;

          if (employee.hasEmployment(date, date)) {
            if (shift.isAbsence) {
              eDaySlot.absenceTime += shift.shiftLength;
              eDaySlot.absenceTime -= ShiftUtil.getBreakTimeWithinShift(
                shift,
                shifts
              );

              if (!shift.isBreak) {
                // Total cost for current shift
                if (
                  this.settingService.showTotalCost() ||
                  this.settingService.showTotalCostIncEmpTaxAndSuppCharge()
                ) {
                  eDaySlot.cost += shift.totalCost;
                  eDaySlot.costIncEmpTaxAndSuppCharge +=
                    shift.totalCostIncEmpTaxAndSuppCharge;
                }
              }
            } else {
              let includePlannedShift: boolean = true;
              if (
                this.settingService.useAccountHierarchy() &&
                shift.accountId &&
                !this.filterService.accountIds().includes(shift.accountId) &&
                shift.shiftStatus ==
                  TermGroup_TimeScheduleTemplateBlockShiftStatus.Open
              ) {
                includePlannedShift = false;
              }

              if (includePlannedShift) {
                // TODO: Staffing needs not implemented
                eDaySlot.needTime = 0;

                // Planned time for current shift
                eDaySlot.netTime += shift.shiftLength;
                eDaySlot.netTime -= ShiftUtil.getBreakTimeWithinShift(
                  shift,
                  shifts
                );
                eDaySlot.netTime += ShiftUtil.getFactorMinutesWithinShift(
                  shift,
                  shifts
                );

                // TimeScheduleType factor multiplier
                if (this.settingService.showScheduleTypeFactorTime()) {
                  eDaySlot.factorTime += ShiftUtil.getFactorMinutesWithinShift(
                    shift,
                    shifts
                  );
                }

                if (!shift.isBreak) {
                  // Gross time for current shift
                  if (this.settingService.showGrossTime()) {
                    eDaySlot.grossTime += shift.grossTime;
                  }

                  // Total cost for current shift
                  if (
                    this.settingService.showTotalCost() ||
                    this.settingService.showTotalCostIncEmpTaxAndSuppCharge()
                  ) {
                    eDaySlot.cost += shift.totalCost;
                    eDaySlot.costIncEmpTaxAndSuppCharge +=
                      shift.totalCostIncEmpTaxAndSuppCharge;
                  }
                }
              }
            }
          }
        }
      });
    });
  }

  private setUnderOrOverTime(employee: PlanningEmployeeDTO) {
    const doSetClass =
      (this.settingService.useAccountHierarchy() ||
        !this.filterService.isFilteredOnAccountDim()) &&
      !this.filterService.isFilteredOnShiftType();

    employee.isUnderTime =
      doSetClass &&
      employee.totalNetTime <
        employee.workTimeMinutes +
          employee.getEmploymentMinScheduleTime(
            this.filterService.dateFrom(),
            this.filterService.dateTo()
          );

    employee.isOverTime =
      doSetClass &&
      employee.totalNetTime >
        employee.workTimeMinutes +
          employee.getEmploymentMaxScheduleTime(
            this.filterService.dateFrom(),
            this.filterService.dateTo()
          );
  }

  private getShiftsToCalculateForDaySlot(
    daySlot: SpEmployeeDaySlot
  ): PlanningShiftDTO[] {
    // Get shifts for specified day slot to be used for calculations
    return daySlot.filteredShifts.filter(
      s =>
        s.isVisible &&
        !s.isWholeDayAbsence &&
        !s.isLended &&
        !s.isOtherAccount &&
        !s.isOnDuty &&
        !s.isLeisureCode
    );
  }

  private isShiftValidForCalculation(
    shift: PlanningShiftDTO,
    employee: PlanningEmployeeDTO
  ): boolean {
    return (
      shift.actualStartDate.isSameOrAfterOnDay(this.filterService.dateFrom()) &&
      shift.actualStartDate.isSameOrBeforeOnDay(this.filterService.dateTo()) &&
      (this.filterService.isTemplateView() ||
        employee.hasEmployment(shift.actualStartTime, shift.actualStopTime))
    );
  }

  private calculateTimesSummary(): void {
    // Clear times before calculation
    this.clearTimesSummary();

    // Sum already calculated times for each employee
    this.slotService.sumDaySlots().forEach(daySlot => {
      this.visibleEmployees().forEach(employee => {
        this.calculateTimesSummaryForEmployee(employee, daySlot);
      });
    });
  }

  private clearTimesSummary(): void {
    this.slotService.sumDaySlots().forEach(daySlot => {
      daySlot.clearTimeAndCosts();

      if (this.filterService.isCommonDayView()) {
        daySlot.hourSlots.forEach(hourSlot => {
          hourSlot.clearTimeAndCosts();
        });
        daySlot.halfHourSlots.forEach(hourSlot => {
          hourSlot.clearTimeAndCosts();
        });
        daySlot.quarterHourSlots.forEach(hourSlot => {
          hourSlot.clearTimeAndCosts();
        });
      }
    });
  }

  private calculateTimesSummaryForEmployee(
    employee: PlanningEmployeeDTO,
    daySlot: SpSumDaySlot
  ): void {
    const eDaySlot = employee.getDaySlot(daySlot.start);
    if (eDaySlot) {
      daySlot.needTime += eDaySlot.needTime;
      daySlot.netTime += eDaySlot.netTime;
      daySlot.factorTime += eDaySlot.factorTime;
      daySlot.workTime += eDaySlot.workTime;
      daySlot.grossTime += eDaySlot.grossTime;
      daySlot.cost += eDaySlot.cost;
      daySlot.costIncEmpTaxAndSuppCharge += eDaySlot.costIncEmpTaxAndSuppCharge;

      if (this.filterService.isCommonDayView()) {
        switch (this.settingService.dayViewMinorTickLength()) {
          case TermGroup_StaffingNeedsHeadInterval.SixtyMinutes:
            daySlot.hourSlots.forEach(slot => {
              const eSlot = eDaySlot.hourSlots.find(h =>
                h.start.isSameMinute(slot.start)
              );
              if (eSlot) {
                slot.plannedMinutes += eSlot.plannedMinutes;
                slot.nbrOfShifts += eSlot.nbrOfShifts;
              }
            });
            break;
          case TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes:
            daySlot.halfHourSlots.forEach(slot => {
              const eSlot = eDaySlot.halfHourSlots.find(h =>
                h.start.isSameMinute(slot.start)
              );
              if (eSlot) {
                slot.plannedMinutes += eSlot.plannedMinutes;
                slot.nbrOfShifts += eSlot.nbrOfShifts;
              }
            });
            break;
          case TermGroup_StaffingNeedsHeadInterval.FifteenMinutes:
            daySlot.quarterHourSlots.forEach(slot => {
              const eSlot = eDaySlot.quarterHourSlots.find(h =>
                h.start.isSameMinute(slot.start)
              );
              if (eSlot) {
                slot.plannedMinutes += eSlot.plannedMinutes;
                slot.nbrOfShifts += eSlot.nbrOfShifts;
              }
            });
            break;
        }
      }
    }
  }
}
