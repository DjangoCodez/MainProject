import { Injectable, computed, inject, signal } from '@angular/core';
import { IGetShiftsModel } from '@shared/models/generated-interfaces/TimeModels';
import { BehaviorSubject, map, Observable, tap } from 'rxjs';
import { PlanningShiftBreakDTO, PlanningShiftDTO } from '../models/shift.model';
import { SchedulePlanningService } from './schedule-planning.service';
import { SpFilterService } from './sp-filter.service';
import { SpEmployeeService } from './sp-employee.service';
import { SpSettingService } from './sp-setting.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import {
  TermGroup_TimeScheduleTemplateBlockType,
  TimeSchedulePlanningDisplayMode,
} from '@shared/models/generated-interfaces/Enumerations';
import { groupBy as _groupBy } from 'lodash-es';
import { SpGraphicsService } from './sp-graphics.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TranslateService } from '@ngx-translate/core';
import { ShiftUtil } from '../util/shift-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class SpShiftService {
  service = inject(SchedulePlanningService);
  employeeService = inject(SpEmployeeService);
  filterService = inject(SpFilterService);
  graphicsService = inject(SpGraphicsService);
  progressService = inject(ProgressService);
  settingService = inject(SpSettingService);
  toasterService = inject(ToasterService);
  translate = inject(TranslateService);

  private perform = new Perform<any>(this.progressService);

  loadingShifts = signal(false);
  reloadShiftsForEmployeeIds: number[] = [];

  // Will be set to true the first time shifts are loaded
  // Is used in some places to know if shifts are loaded or not even if length is 0
  initialShiftsLoaded = signal(false);
  shiftsLoaded = new BehaviorSubject<PlanningShiftDTO[] | undefined>(undefined);

  // When selecting or unselecting shift(s), this array will contain the shifts that should be selected
  selectedShiftsChanged = new BehaviorSubject<PlanningShiftDTO[]>([]);
  nbrOfSelectedShifts = signal(0);

  createdTooltipIds: number[] = [];

  singleRowShift = computed(() => {
    // TODO: Implement setting for compressed view in schedule view
    return this.filterService.isCommonDayView();
  });

  loadShifts(employeeIds: number[] = []): Observable<PlanningShiftDTO[]> {
    this.loadingShifts.set(true);

    if (employeeIds.length > 0) {
      this.employeeService.loadingShiftsForEmployeeIdsChanged.next(employeeIds);
    }

    const model = this.defaultGetShiftsModel;
    if (employeeIds.length > 0) {
      model.employeeIds = employeeIds;
    } else if (this.filterService.isFilteredOnEmployee()) {
      model.employeeIds = this.filterService.employeeIds();
    }

    this.clearShiftTooltips();

    return this.service.getShifts(model).pipe(
      map((data: IShiftDTO[]) => {
        const s: PlanningShiftDTO[] = data.map(item => {
          const obj = new PlanningShiftDTO();
          Object.assign(obj, item);
          obj.fixDates();
          obj.setProperties();
          if (this.filterService.isCommonDayView()) {
            this.setShiftPositionAndWidthForDayView(obj);
          } else if (this.filterService.isCommonScheduleView()) {
            this.setShiftPositionAndWidthForScheduleView(obj);
          }
          return obj;
        });

        if (this.filterService.isCommonDayView()) {
          this.setDayViewVisibleRange(s);
        }

        this.employeeService.setShiftsOnEmployees(s);
        if (!this.initialShiftsLoaded()) this.initialShiftsLoaded.set(true);

        this.selectedShiftsChanged.next([]);
        this.nbrOfSelectedShifts.set(0);
        this.shiftsLoaded.next(s);
        this.loadingShifts.set(false);
        if (employeeIds.length > 0) {
          this.employeeService.loadingShiftsForEmployeeIdsChanged.next([]);
        }

        return s;
      })
    );
  }

  loadShiftsForEmployeeAndDate(
    employeeId: number,
    date: Date,
    blockTypes: TermGroup_TimeScheduleTemplateBlockType[],
    includeBreaks: boolean,
    includeGrossNetAndCost: boolean,
    link: string,
    loadQueue: boolean,
    loadDeviationCause: boolean,
    loadTasks: boolean,
    includePreliminary: boolean,
    timeScheduleScenarioHeadId?: number
  ): Observable<PlanningShiftDTO[]> {
    return this.service
      .getShiftsForDay(
        employeeId,
        date,
        blockTypes,
        includeBreaks,
        includeGrossNetAndCost,
        link,
        loadQueue,
        loadDeviationCause,
        loadTasks,
        includePreliminary,
        timeScheduleScenarioHeadId
      )
      .pipe(
        map((data: IShiftDTO[]) => {
          return this.toPlanningShiftDTOs(data);
        })
      );
  }

  loadLinkedShifts(
    timeScheduleTemplateBlockId: number
  ): Observable<PlanningShiftDTO[]> {
    return this.service.getLinkedShifts(timeScheduleTemplateBlockId).pipe(
      map((data: IShiftDTO[]) => {
        return this.toPlanningShiftDTOs(data);
      })
    );
  }

  private toPlanningShiftDTOs(shifts: IShiftDTO[]): PlanningShiftDTO[] {
    return shifts.map(item => {
      const obj = new PlanningShiftDTO();
      Object.assign(obj, item);
      obj.fixDates();
      obj.setProperties();
      obj.setTypes();
      return obj;
    });
  }

  saveShifts(
    source: string,
    shifts: PlanningShiftDTO[],
    updateBreaks = true,
    adjustTasks = false,
    minutesMoved = 0,
    timeScheduleScenarioHeadId?: number
  ): Observable<BackendResponse> {
    this.setReloadShiftsForEmployeeIds(shifts);

    shifts.forEach(shift => {
      shift.prepareShiftForSave();
    });

    // If all shifts are marked as deleted, unmark one of the them,
    // otherwise the whole day will be deleted and not visible in the attest view
    if (
      shifts.length > 0 &&
      shifts.length === shifts.filter(s => s.isDeleted).length
    )
      shifts[shifts.length - 1].isDeleted = false;

    return this.service
      .saveShifts(
        source,
        shifts,
        updateBreaks,
        !this.settingService.sendXEMailOnChange(),
        adjustTasks,
        minutesMoved,
        timeScheduleScenarioHeadId
      )
      .pipe(
        tap((result: BackendResponse) => {
          if (result.success) {
            this.perform.load(this.loadShifts(this.reloadShiftsForEmployeeIds));
          }
        })
      );
  }

  deleteShifts(
    shifts: PlanningShiftDTO[],
    timeScheduleScenarioHeadId?: number,
    includedOnDutyShiftIds: number[] = []
  ): Observable<BackendResponse> {
    this.setReloadShiftsForEmployeeIds(shifts);

    return this.service
      .deleteShifts(
        shifts.map(s => s.timeScheduleTemplateBlockId),
        !this.settingService.sendXEMailOnChange(),
        timeScheduleScenarioHeadId,
        includedOnDutyShiftIds
      )
      .pipe(
        tap((result: BackendResponse) => {
          if (result.success) {
            this.perform.load(this.loadShifts(this.reloadShiftsForEmployeeIds));
          }
        })
      );
  }

  private setReloadShiftsForEmployeeIds(shifts: PlanningShiftDTO[]): void {
    // Remember which employees that are affected and only reload shifts for those employees
    const uniqueShifts = Array.from(
      new Map(shifts.map(s => [s.employeeId, s])).values()
    );
    this.reloadShiftsForEmployeeIds = uniqueShifts.map(s => s.employeeId);

    // if (additionalEmployeesToRefresh && additionalEmployeesToRefresh.length > 0) {
    //     additionalEmployeesToRefresh.forEach(employeeId => {
    //         if (!this.reloadShiftsForEmployeeIds.includes(employeeId))
    //             this.reloadShiftsForEmployeeIds.push(employeeId);
    //     })
    // }
  }

  private setDayViewVisibleRange(data: PlanningShiftDTO[]) {
    // Adjust day start/end times

    // Restore to company setting
    let newStartTime = this.settingService.originalDayViewStartTime;
    let newEndTime = this.settingService.originalDayViewEndTime;

    newStartTime = this.adjustDayViewStartTime(newStartTime);
    newEndTime = this.adjustDayViewEndTime(newStartTime, newEndTime, false);

    // Get shifts that starts or ends on same date as visible range
    const shifts = data.filter(
      s =>
        !s.isWholeDay &&
        (s.actualStartTime.isSameDay(this.filterService.dateTo()) ||
          s.actualStopTime.isSameDay(this.filterService.dateFrom()))
    );
    if (shifts.length > 0) {
      // Get first shift in collection
      // TODO: Replace [...shifts].sort with shifts.toSorted after updating to ES2023 in tsconfig
      const firstShiftStartTime = [...shifts].sort((a, b) => {
        return (
          new Date(a.actualStartTime).getTime() -
          new Date(b.actualStartTime).getTime()
        );
      })[0].actualStartTime;

      // Check if there are any shifts that starts before current start setting
      newStartTime = this.adjustStartTimeBasedOnFirstShift(
        firstShiftStartTime,
        newStartTime
      );

      // Get last shift in collection
      // TODO: Replace [...shifts].sort with shifts.toSorted after updating to ES2023 in tsconfig
      const lastShiftEndTime = [...shifts].sort((a, b) => {
        return (
          new Date(b.actualStopTime).getTime() -
          new Date(a.actualStopTime).getTime()
        );
      })[0].actualStopTime;

      // Check if there are any shifts that ends after current end setting
      newEndTime = this.adjustEndTimeBasedOnLastShift(
        lastShiftEndTime,
        newEndTime
      );

      // Update settings if they are different from the current settings
      let dateRangeChanged = false;
      if (this.settingService.dayViewStartTime() != newStartTime) {
        this.settingService.dayViewStartTime.set(newStartTime);
        dateRangeChanged = true;
      }
      if (this.settingService.dayViewEndTime() != newEndTime) {
        this.settingService.dayViewEndTime.set(newEndTime);
        dateRangeChanged = true;
      }

      if (dateRangeChanged) {
        // Settings were changed, update date range in filter service
        this.filterService.setDateRange(
          'setDayViewVisibleRange',
          this.filterService
            .dateFrom()
            .beginningOfDay()
            .addMinutes(newStartTime),
          this.filterService.dateTo().beginningOfDay().addMinutes(newEndTime),
          false
        );
      }
    }
  }

  private adjustStartTimeBasedOnFirstShift(
    firstShiftStartTime: Date,
    newStartTime: number
  ): number {
    if (
      firstShiftStartTime
        .beginningOfHour()
        .isBeforeOnMinute(
          this.filterService
            .dateFrom()
            .beginningOfDay()
            .addMinutes(this.settingService.originalDayViewStartTime)
        )
    ) {
      newStartTime = firstShiftStartTime.isBeforeOnDay(
        this.filterService.dateFrom()
      )
        ? 0
        : firstShiftStartTime
            .beginningOfHour()
            .diffMinutes(this.filterService.dateFrom().beginningOfDay());
    }

    // Never set start time to less than original setting
    if (newStartTime > this.settingService.originalDayViewStartTime)
      newStartTime = this.settingService.originalDayViewStartTime;

    return newStartTime;
  }

  private adjustEndTimeBasedOnLastShift(
    lastShiftEndTime: Date,
    newEndTime: number
  ): number {
    if (
      lastShiftEndTime.isAfterOnMinute(
        this.filterService
          .dateFrom()
          .beginningOfDay()
          .addMinutes(this.settingService.originalDayViewEndTime)
      )
    ) {
      newEndTime = lastShiftEndTime.isAfterOnDay(this.filterService.dateFrom())
        ? 24 * 60
        : lastShiftEndTime.diffMinutes(
            this.filterService.dateFrom().beginningOfDay()
          );
    }

    // Never set end time to less than original setting
    if (newEndTime < this.settingService.originalDayViewEndTime)
      newEndTime = this.settingService.originalDayViewEndTime;

    return newEndTime;
  }

  private adjustDayViewStartTime(startTime: number): number {
    // Make sure start time is a whole hour, if not, round it down to the nearest hour
    if (startTime % 60 !== 0) startTime -= 60 - (startTime % 60);

    return startTime;
  }

  private adjustDayViewEndTime(
    startTime: number,
    endTime: number,
    adjustOriginalEndTime: boolean
  ): number {
    // If end time is not set, set it to 24 hours
    if (endTime === 0) endTime = 24 * 60;

    // Make sure end time is a whole hour, if not, round it up to the nearest hour
    if (endTime % 60 !== 0) endTime += 60 - (endTime % 60);

    // Make sure we only show max 24 hours in day view
    if (endTime - startTime > 24 * 60) endTime = startTime + 24 * 60;

    if (adjustOriginalEndTime)
      this.settingService.originalDayViewEndTime = endTime;

    return endTime;
  }

  setShiftPositionAndWidthForDayView(shift: PlanningShiftDTO) {
    const left = this.graphicsService.getPixelsForTime(shift.actualStartTime);
    const width =
      this.graphicsService.getPixelsForTime(shift.actualStopTime) - left;

    shift.positionLeft = left;
    shift.positionWidth = width;

    this.setBreakPositions(shift);
  }

  private setBreakPositions(shift: PlanningShiftDTO) {
    shift.breaks.forEach(brk => {
      const left = this.graphicsService.getPixelsForTime(brk.startTime);
      const width = this.graphicsService.getPixelsForTime(brk.stopTime) - left;

      brk.positionTop = 0;
      brk.positionLeft = left - shift.positionLeft;
      brk.positionWidth = width;
    });
  }

  setShiftPositionAndWidthForScheduleView(shift: PlanningShiftDTO) {
    const days = this.getShiftWidthInDaysForScheduleView(shift);

    const belongsToDayBeforeVisibleRange = shift.actualStartDate.isBeforeOnDay(
      this.filterService.dateFrom()
    );
    const startBeforeFirstDayInRange = shift.actualStartTime.isBeforeOnDay(
      this.filterService.dateFrom()
    );
    const belongsToDayAfterVisibleRange = shift.actualStartDate.isAfterOnDay(
      this.filterService.dateTo()
    );
    const belongsToLastDayInRange = shift.actualStartDate.isSameDay(
      this.filterService.dateTo()
    );
    const startsPreviousDay = shift.actualStartTime.isBeforeOnDay(
      shift.actualStartDate
    );
    const endsNextDay = shift.actualStopTime.isAfterOnDay(
      shift.actualStartDate
    );

    // Default position
    shift.positionLeft = 0;
    shift.positionWidth = 0; // Rendered in full width

    if (days > 1 && shift.isWholeDay) {
      // Whole day shifts that spans over multiple days should be rendered in full width
      shift.positionWidth =
        this.scheduleViewDayFullWidth * days - this.scheduleViewDayMargin;
    } else if (shift.actualStartTime.isBeforeOnDay(shift.actualStopTime)) {
      // Shift spans over midnight

      if (belongsToDayBeforeVisibleRange) {
        // Shift belongs to day before visible range, show shift with only 25% of the width, left justified in cell
        shift.positionWidth = this.scheduleViewDay25Width;
      } else if (startBeforeFirstDayInRange) {
        // Shift starts before visible range, show shift with only 75% of the width, left justified in cell
        shift.positionWidth =
          this.scheduleViewDay75Width - this.scheduleViewDayMargin;
      } else if (belongsToDayAfterVisibleRange) {
        // Shift belongs to day after visible range, show shift with only 25% of the width, right justified in cell
        shift.positionLeft = this.scheduleViewDay75Width;
        shift.positionWidth =
          this.scheduleViewDay25Width - this.scheduleViewDayMargin;
      } else if (belongsToLastDayInRange) {
        // Shift belongs to last day in visible range but ends after visible range
        shift.positionLeft = this.scheduleViewDay25Width;
        shift.positionWidth =
          this.scheduleViewDay75Width - this.scheduleViewDayMargin;
      } else if (startsPreviousDay) {
        // Shift belongs to current day but starts on previous day
        // Show shift with 25% of the width in previous day and 75% in current day
        shift.positionLeft = -this.scheduleViewDay25Width;
        shift.positionWidth = this.scheduleViewDayFullWidth;
      } else if (endsNextDay) {
        // Shift belongs to current day but ends on next day
        // Show shift with 75% of the width in current day and 25% in next day
        shift.positionLeft = this.scheduleViewDay25Width;
        shift.positionWidth = this.scheduleViewDayFullWidth;
      }
    }
  }

  private get scheduleViewDayFullWidth(): number {
    return this.graphicsService.pixelsPerDay.value;
  }

  private get scheduleViewDay25Width(): number {
    return this.scheduleViewDayFullWidth * 0.25;
  }

  private get scheduleViewDay75Width(): number {
    return this.scheduleViewDayFullWidth * 0.75;
  }

  private get scheduleViewDayMargin(): number {
    return 5;
  }

  getShiftDurationInDays(shift: PlanningShiftDTO): number {
    // Get shift actual start time or beginning of display if shift goes beyond that
    const actualStart = DateUtil.getMaxDate(
      shift.actualStartTime.beginningOfDay(),
      this.filterService.dateFrom()
    );
    // Get shift actual stop time or end of display if shift goes beyond that
    const actualStop = DateUtil.getMinDate(
      shift.actualStopTime,
      this.filterService.dateTo()
    ).beginningOfDay();

    let days = actualStop.diffDays(actualStart) + 1;
    if (days === 0) days = 1;

    return days;
  }

  getShiftWidthInDaysForScheduleView(shift: PlanningShiftDTO): number {
    let days = this.getShiftDurationInDays(shift);
    if (days > this.filterService.nbrOfDays())
      days = this.filterService.nbrOfDays();

    return days;
  }

  clearShiftTooltips(): void {
    // This will make all tooltips being recreated
    this.createdTooltipIds = [];
  }

  createShiftTooltip(shift: PlanningShiftDTO): string {
    if (
      shift.toolTip &&
      this.createdTooltipIds.includes(shift.timeScheduleTemplateBlockId)
    ) {
      return shift.toolTip;
    }

    let toolTip = '';
    let wholeDayToolTip = '';
    const isHiddenEmployee: boolean =
      shift.employeeId === this.service.hiddenEmployeeId;

    // Current shift

    // Time
    if (!shift.isAbsenceRequest) {
      if (shift.isWholeDay)
        toolTip += `${this.translate.instant('time.schedule.planning.wholedaylabel')}  `;
      else
        toolTip += `${shift.actualStartTime.toFormattedTime()}-${shift.actualStopTime.toFormattedTime()}  `;
    }

    if (shift.timeDeviationCauseId && shift.timeDeviationCauseId !== 0) {
      // Absence
      toolTip += shift.timeDeviationCauseName;
    } else {
      // Shift type
      if (shift.shiftTypeName) toolTip += shift.shiftTypeName;

      if (shift.isOnDuty)
        toolTip += ` (${this.translate.instant('time.schedule.planning.blocktype.onduty').toLocaleLowerCase()})`;

      if (this.settingService.useAccountHierarchy() && shift.accountName)
        toolTip += ` (${shift.accountName})`;
    }

    // Schedule type
    const scheduleTypeNames = shift.getTimeScheduleTypeNames(
      this.settingService.useMultipleScheduleTypes()
    );
    if (scheduleTypeNames) toolTip += ` - ${scheduleTypeNames}`;

    // Week number/Number of weeks
    if (shift.nbrOfWeeks > 0) {
      if (toolTip && toolTip.length > 0) toolTip += ', ';
      toolTip += `${DateUtil.getWeekNr(shift.dayNumber)}/${shift.nbrOfWeeks}${this.translate.instant('common.weekshort')}`;
    }

    // Description
    if (shift.description) {
      if (toolTip && toolTip.length > 0) toolTip += '\n';
      toolTip += shift.description;
    }

    // Whole day

    const employeeShifts = this.employeeService.getEmployeeShifts(
      shift.employeeId
    );

    const breakPrefix = this.translate.instant(
      'time.schedule.planning.breakprefix'
    );

    let dayShifts: PlanningShiftDTO[];
    // TODO: EmployeePost not implemented
    // if (this.filterService.isEmployeePostView()) {
    //   dayShifts = employeeShifts.filter(
    //     s =>
    //       s.actualStartDate.isSameDay(shift.actualStartDate) &&
    //       s.employeePostId === shift.employeePostId
    //   );
    // } else {
    // If whole day absence, skip this part
    dayShifts = employeeShifts.filter(
      s =>
        s.actualStartDate.isSameDay(shift.actualStartDate) &&
        (s.link === shift.link || !isHiddenEmployee) &&
        !((s.isAbsence || s.isAbsenceRequest) && s.isWholeDay)
    );
    //}

    let minutes = dayShifts
      .filter(s => !s.isStandby && !s.isOnDuty)
      .reduce((sum, s) => sum + s.shiftLength, 0);
    let factorMinutes = 0;

    if (dayShifts.length > 0) {
      if (shift.isSchedule || shift.isStandby || shift.isOnDuty)
        wholeDayToolTip += `${this.translate.instant('time.schedule.planning.todaysschedule')}:`;
      else if (shift.isBooking)
        wholeDayToolTip += `${this.translate.instant('time.schedule.planning.todaysbookings')}:`;

      // Sort shifts by start time
      dayShifts
        .slice()
        .sort(
          (a, b) =>
            new Date(a.actualStartTime).getTime() -
            new Date(b.actualStartTime).getTime()
        )
        .forEach(dayShift => {
          // Breaks within day
          minutes -= ShiftUtil.getBreakTimeWithinShift(dayShift, dayShifts);

          if (shift.isSchedule) {
            let breakEndTime: Date;
            shift.breaks.forEach(brk => {
              breakEndTime = brk.startTime.addMinutes(brk.minutes);
              if (
                breakEndTime.isSameOrBeforeOnMinute(dayShift.actualStartTime)
              ) {
                wholeDayToolTip += this.getBreakText(brk, breakPrefix);
              }
            });
          }

          // Time
          wholeDayToolTip += `\n${dayShift.actualStartTime.toFormattedTime()}-${dayShift.actualStopTime.toFormattedTime()}  `;

          // Shift type
          if (dayShift.shiftTypeName) wholeDayToolTip += dayShift.shiftTypeName;

          if (dayShift.isOnDuty)
            wholeDayToolTip += ` (${this.translate.instant('time.schedule.planning.blocktype.onduty').toLocaleLowerCase()})`;

          if (this.settingService.useAccountHierarchy() && dayShift.accountName)
            wholeDayToolTip += ` (${dayShift.accountName})`;

          // TimeScheduleType factor multiplyer
          factorMinutes += ShiftUtil.getFactorMinutesWithinShift(
            dayShift,
            dayShifts
          );
        });

      if (shift.isSchedule || shift.isStandby) {
        // Summary

        let breakMinutes: number = 0;
        shift.breaks.forEach(brk => {
          breakMinutes += brk.minutes;
        });

        wholeDayToolTip += `\n\n${this.translate.instant('time.schedule.planning.scheduletime')}: ${DateUtil.minutesToTimeSpan(minutes)}`;
        if (breakMinutes > 0)
          wholeDayToolTip += ` (${breakMinutes.toString()})`;
      }

      if (factorMinutes !== 0)
        wholeDayToolTip += `\n${this.translate.instant('time.schedule.planning.scheduletypefactortime')}: ${DateUtil.minutesToTimeSpan(factorMinutes)}`;

      // Gross time
      if (this.settingService.showGrossTime())
        wholeDayToolTip += `\n${this.translate.instant('time.schedule.planning.grosstime')}: ${DateUtil.minutesToTimeSpan(
          dayShifts
            .filter(s => !s.isBreak)
            .reduce((sum, s) => sum + (s.grossTime || 0), 0)
        )} `;

      // Cost
      if (this.settingService.showTotalCostIncEmpTaxAndSuppCharge()) {
        wholeDayToolTip += `\n${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(
          dayShifts
            .filter(s => !s.isBreak)
            .reduce(
              (sum, s) => sum + (s.totalCostIncEmpTaxAndSuppCharge || 0),
              0
            ),
          0
        )}`;
      } else if (this.settingService.showTotalCost()) {
        wholeDayToolTip += `\n${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(
          dayShifts
            .filter(s => !s.isBreak)
            .reduce((sum, s) => sum + (s.totalCost || 0), 0),
          0
        )}`;
      }
    }

    if (wholeDayToolTip.length > 0) {
      let shiftTypeName = '';
      if (shift.isSchedule || shift.isStandby || shift.isOnDuty)
        shiftTypeName = this.translate.instant(
          'time.schedule.planning.thisshift'
        );
      else if (shift.isBooking)
        shiftTypeName = this.translate.instant(
          'time.schedule.planning.thisbooking'
        );

      if (toolTip.length > 0) {
        toolTip = `${shiftTypeName}:\n${toolTip}\n\n`;
      }
      toolTip += wholeDayToolTip;
    }

    // Availability
    const daySlot = this.employeeService.getDaySlotForShift(
      shift.timeScheduleTemplateBlockId,
      shift.employeeId
    );
    if (daySlot?.tooltip) {
      toolTip += `\n\n${this.translate.instant(
        'time.schedule.planning.availability'
      )}:\n${daySlot.tooltip}`;
    }

    shift.toolTip = toolTip;

    this.createdTooltipIds.push(shift.timeScheduleTemplateBlockId);

    return toolTip;
  }

  private getBreakText(
    brk: PlanningShiftBreakDTO,
    breakPrefix: string
  ): string {
    const breakTimeCode = this.getBreakTimeCode(brk, breakPrefix);
    const breakText = `\n${brk.startTime.toFormattedTime()}-${brk.startTime.addMinutes(brk.minutes).toFormattedTime()}  ${breakTimeCode}`;

    return breakText;
  }

  private getBreakTimeCode(
    brk: PlanningShiftBreakDTO,
    breakPrefix: string
  ): string {
    const timeCode = this.service.timeCodeBreaks.find(
      b => b.timeCodeId === brk.timeCodeId
    );
    let breakTimeCode = timeCode ? timeCode.name : '';
    if (!breakTimeCode.startsWithCaseInsensitive(breakPrefix))
      breakTimeCode = `${breakPrefix} ${breakTimeCode}`;

    return breakTimeCode;
  }

  private get defaultGetShiftsModel(): IGetShiftsModel {
    return {
      employeeId: SoeConfigUtil.employeeId,
      dateFrom: this.filterService.dateFrom(),
      dateTo: this.filterService.dateTo(),
      employeeIds: this.employeeService.employees().map(e => e.employeeId),
      planningMode: this.filterService.planningMode(),
      displayMode: TimeSchedulePlanningDisplayMode.Admin,
      includeSecondaryCategories: this.filterService.showSecondaryCategories(),
      includeBreaks: true,
      includeGrossNetAndCost:
        this.filterService.isSchedulePlanningMode() &&
        (this.settingService.showGrossTime() ||
          this.settingService.showTotalCost()),
      includePreliminary: true,
      includeEmploymentTaxAndSupplementChargeCost:
        this.settingService.showTotalCostIncEmpTaxAndSuppCharge(),
      includeShiftRequest: true,
      includeAbsenceRequest: true,
      checkToIncludeDeliveryAdress: this.filterService.isOrderPlanningMode(),
      timeScheduleScenarioHeadId: undefined,
      includeHolidaySalary: this.settingService.showWeekendSalary(),
      includeLeisureCodes: this.settingService.useLeisureCodes(),
      loadYesterdayAlso: false,
      loadTasks: false,
    };
  }

  shiftSelected(
    shift: PlanningShiftDTO,
    ctrlKeyPressed: boolean,
    shiftKeyPressed: boolean
  ) {
    if (this.filterService.isTasksAndDeliveriesView()) {
      // TODO: Implement
    } else {
      // TODO: Implement permissions
      //if (!hasCurrentViewModifyPermission) return;

      if (shiftKeyPressed) {
        // User selected a shift while holding the shift key down
        // Select all shifts between first and last in range
        const selectedShifts = this.selectedShiftsChanged.value;
        selectedShifts.sort((a, b) =>
          a.actualStartTime.isBefore(b.actualStartTime) ? -1 : 1
        );

        // User must select same employee
        if (
          selectedShifts.length > 0 &&
          selectedShifts[0].employeeId === shift.employeeId
        ) {
          const firstSelected = selectedShifts[0];
          let start = firstSelected.actualStartTime;
          if (shift.actualStartTime.isBeforeOnMinute(start))
            start = shift.actualStartTime;
          let stop = selectedShifts[selectedShifts.length - 1].actualStopTime;
          if (shift.actualStopTime.isAfterOnDay(stop))
            stop = shift.actualStopTime;

          const empShifts = this.employeeService.getEmployeeShifts(
            shift.employeeId
          );
          const validEmpShifts = empShifts.filter(s => {
            return (
              s.isVisible &&
              s.type === firstSelected.type &&
              !!s.isAbsence === !!firstSelected.isAbsence &&
              !!s.isLeisureCode === !!firstSelected.isLeisureCode &&
              s.actualStartTime.isSameOrAfterOnMinute(start) &&
              s.actualStopTime.isSameOrBeforeOnMinute(stop)
            );
          });
          validEmpShifts.forEach(s => {
            this.selectShift(s);
          });
        } else {
          // TODO: Add new term
          this.toasterService.info(
            'Du kan bara markera flera pass om de tillhör samma anställd'
          );
          this.clearSelectedShifts();
        }

        // Can't select shifts from different templates
        if (this.filterService.isTemplateView()) {
          // TODO: Make sure this works
          // This is new ES6 syntax instead of using lodash _uniqBy
          const unique = [
            ...new Set(
              this.selectedShiftsChanged.value.map(
                s => s.timeScheduleTemplateHeadId
              )
            ),
          ];
          if (unique.length > 1) {
            // TODO: Add new term
            this.toasterService.info(
              'Du kan bara markera flera pass om de tillhör samma grundschema'
            );
            this.clearSelectedShifts();
          }
        }
      } else {
        this.selectedShiftsChanged.value.forEach(s => {
          if (
            this.filterService.isTemplateView() &&
            s.timeScheduleTemplateHeadId !== shift.timeScheduleTemplateHeadId
          ) {
            // Can't select shifts from different templates
            // TODO: Add new term
            this.toasterService.info(
              'Du kan bara markera flera pass om de tillhör samma grundschema'
            );
            this.unselectShift(s);
          } else {
            if (ctrlKeyPressed) {
              // User can select multiple shifts (using the ctrl key) as long as it's the same employee and same type of shift
              if (s.employeeId !== shift.employeeId) {
                // TODO: Add new term
                this.toasterService.info(
                  'Du kan bara markera flera pass om de tillhör samma anställd'
                );
                this.unselectShift(s);
              }

              if (
                s.type !== shift.type ||
                !!s.isAbsence !== !!shift.isAbsence ||
                !!s.isLeisureCode !== !!shift.isLeisureCode
              ) {
                // TODO: Add new term
                this.toasterService.info(
                  'Du kan bara markera flera pass om de är av samma typ'
                );
                this.unselectShift(s);
              }
            } else {
              // Ctrl key was not pressed, unselect all other shifts
              this.unselectShift(s);
            }
          }
        });

        if (shift.isSelected) {
          this.unselectShift(shift);
        } else {
          this.selectShift(shift);
        }
      }
    }
  }

  private selectShift(shift: PlanningShiftDTO) {
    if (!shift.isReadOnly) {
      const selected: PlanningShiftDTO[] = this.selectedShiftsChanged.value;
      if (!this.isShiftSelected(shift)) {
        selected.push(shift);
      }
      //this.clearSelectedSlots();
      if (shift.link) {
        // Select all linked shifts
        const linkedShifts = this.getLinkedShifts(shift);
        linkedShifts.forEach(linked => {
          if (!this.isShiftSelected(linked)) {
            selected.push(linked);
          }
        });
      }
      this.selectedShiftsChanged.next(selected);
      this.nbrOfSelectedShifts.set(selected.length);
    }
  }

  private unselectShift(shift: PlanningShiftDTO) {
    if (!shift.isReadOnly) {
      let selected: PlanningShiftDTO[] = this.selectedShiftsChanged.value;
      selected = selected.filter(
        s => s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId
      );

      if (shift.link) {
        // Unselect all linked shifts
        const linkedShifts = this.getLinkedShifts(shift);
        if (linkedShifts.length > 0) {
          linkedShifts.forEach(linked => {
            selected = selected.filter(
              s =>
                s.timeScheduleTemplateBlockId !==
                linked.timeScheduleTemplateBlockId
            );
          });
        }
      }
      this.selectedShiftsChanged.next(selected);
      this.nbrOfSelectedShifts.set(selected.length);
    }
  }

  isShiftSelected(shift: PlanningShiftDTO): boolean {
    return this.selectedShiftsChanged.value.some(
      s => s.timeScheduleTemplateBlockId === shift.timeScheduleTemplateBlockId
    );
  }

  private clearSelectedShifts(notify: boolean = true) {
    if (notify) this.selectedShiftsChanged.next([]);

    this.nbrOfSelectedShifts.set(0);
  }

  private getLinkedShifts(shift: PlanningShiftDTO): PlanningShiftDTO[] {
    const empShifts = this.employeeService.getEmployeeShifts(shift.employeeId);
    return empShifts.filter(
      s =>
        s.link === shift.link &&
        s.type === shift.type &&
        s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId
    );
  }

  getIntersectingOnDutyShifts(shift: PlanningShiftDTO): PlanningShiftDTO[] {
    const employee = this.employeeService.getEmployee(shift.employeeId);
    if (employee) {
      const shifts = employee.getShifts();
      return shifts.filter(
        s =>
          s.isOnDuty &&
          s.timeScheduleTemplateBlockId !== shift.timeScheduleTemplateBlockId &&
          s.employeeId === shift.employeeId &&
          DateUtil.isRangesOverlapping(
            s.actualStartTime.addSeconds(1),
            s.actualStopTime.addSeconds(-1),
            shift.actualStartTime,
            shift.actualStopTime
          ) &&
          (s.accountId === shift.accountId || !s.accountId || !shift.accountId)
      );
    }
    return [];
  }
}
