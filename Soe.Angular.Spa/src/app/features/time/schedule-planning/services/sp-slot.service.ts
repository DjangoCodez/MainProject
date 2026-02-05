import { Injectable, inject, signal } from '@angular/core';
import { SpFilterService } from './sp-filter.service';
import {
  TermGroup_StaffingNeedsHeadInterval,
  TermGroup_TimeSchedulePlanningVisibleDays,
} from '@shared/models/generated-interfaces/Enumerations';
import { TermCollection } from '@shared/localization/term-types';
import { Observable, tap } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
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
  SpSumHourSlot,
  SpWeekSlot,
} from '../models/time-slot.model';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { SpSettingService } from './sp-setting.service';
import { SpTranslateService } from './sp-translate.service';

@Injectable({
  providedIn: 'root',
})
export class SpSlotService {
  weekSlots = signal<SpWeekSlot[]>([]);
  daySlots = signal<SpDaySlot[]>([]);
  sumDaySlots = signal<SpSumDaySlot[]>([]);
  totalSumDaySlots = signal(0);
  hourSlots = signal<SpHourSlot[]>([]);
  halfHourSlots = signal<SpHalfHourSlot[]>([]);
  quarterHourSlots = signal<SpQuarterHourSlot[]>([]);

  filterService = inject(SpFilterService);
  settingService = inject(SpSettingService);
  spTranslate = inject(SpTranslateService);
  translate = inject(TranslateService);

  terms: TermCollection = {};

  constructor() {
    this.loadTerms().subscribe();
  }

  private loadTerms(): Observable<TermCollection> {
    return this.translate.get(['common.week', 'common.weekshort']).pipe(
      tap(terms => {
        this.terms = terms;
      })
    );
  }

  createTimeSlots() {
    let dateFrom = this.filterService.dateFrom();
    const daySlots: SpDaySlot[] = [];
    const sumDaySlots: SpSumDaySlot[] = [];

    if (this.filterService.isCommonDayView()) {
      dateFrom = dateFrom.beginningOfHour();

      // Create one day slot to put all hour slots in
      daySlots.push(this.createDaySlot(dateFrom));
      const sumDaySlot = this.createSumDaySlot(dateFrom);

      // Always create hour slots, they will contain the start time of the hour, even in 15 or 30 minute slots
      const hourSlots: SpHourSlot[] = [];
      for (let i = 0; i < this.filterService.nbrOfHours(); i++) {
        const time = dateFrom.addHours(i);
        hourSlots.push(this.createHourSlot(time));
      }
      this.hourSlots.set(hourSlots);

      switch (this.settingService.dayViewMinorTickLength()) {
        case TermGroup_StaffingNeedsHeadInterval.SixtyMinutes:
          sumDaySlot.hourSlots = hourSlots;
          break;
        case TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes:
          const halfHourSlots: SpHalfHourSlot[] = [];
          for (let i = 0; i < this.filterService.nbrOfHours() * 2; i++) {
            const time = dateFrom.addMinutes(i * 30);
            halfHourSlots.push(this.createHalfHourSlot(time));
          }
          sumDaySlot.halfHourSlots = halfHourSlots;
          this.halfHourSlots.set(halfHourSlots);
          break;
        case TermGroup_StaffingNeedsHeadInterval.FifteenMinutes:
          const quarterHourSlots: SpQuarterHourSlot[] = [];
          for (let i = 0; i < this.filterService.nbrOfHours() * 4; i++) {
            const time = dateFrom.addMinutes(i * 15);
            quarterHourSlots.push(this.createQuarterHourSlot(time));
          }
          sumDaySlot.quarterHourSlots = quarterHourSlots;
          this.quarterHourSlots.set(quarterHourSlots);
          break;
      }
      sumDaySlots.push(sumDaySlot);
    } else if (this.filterService.isCommonScheduleView()) {
      dateFrom = dateFrom.beginningOfDay();
      const days = this.filterService.nbrOfDays();
      const weekSlots: SpWeekSlot[] = [];

      for (let i = 0; i < days; i++) {
        const date = dateFrom.addDays(i);
        daySlots.push(this.createDaySlot(date));
        sumDaySlots.push(this.createSumDaySlot(date));
        if (i === 0 || date.isMonday()) {
          let nbrOfDaysInWeek = 7;
          if (i === 0) {
            // First week, calculate number of days in week
            if (dateFrom.isSunday()) {
              // Since sunday is 0 it will not work with the subtraction below
              nbrOfDaysInWeek = 1;
            } else if (!dateFrom.isMonday()) {
              const daysLeftInWeek = 8 - dateFrom.dayOfWeek();
              if (days >= daysLeftInWeek) {
                nbrOfDaysInWeek = daysLeftInWeek;
              } else {
                nbrOfDaysInWeek = days;
              }
            }
          } else {
            // Other weeks, always 7 days unless at end of range
            if (i + 7 > days) {
              nbrOfDaysInWeek = days - i;
            }
          }
          const weekSlot = this.createWeekSlot(date, nbrOfDaysInWeek);

          weekSlots.push(weekSlot);
        }
      }

      this.weekSlots.set(weekSlots);
    }

    this.daySlots.set(daySlots);
    this.sumDaySlots.set(sumDaySlots);
  }

  private createWeekSlot(date: Date, nbrOfDaysInWeek: number): SpWeekSlot {
    return new SpWeekSlot(
      date,
      this.getWeekInfo(date, nbrOfDaysInWeek),
      nbrOfDaysInWeek
    );
  }

  private getWeekInfo(date: Date, nbrOfDaysInWeek: number): string {
    let text = '';

    let startDate = this.filterService.dateFrom();
    let stopDate = this.filterService.dateTo();

    if (
      this.filterService.nbrOfDays() !==
      TermGroup_TimeSchedulePlanningVisibleDays.Year
    ) {
      // Get beginning of week or first visible day
      startDate = date.beginningOfWeek().isBeforeOnDay(startDate)
        ? startDate
        : date.beginningOfWeek();
      // Get end of week or last visible day
      stopDate = date.endOfWeek().isAfterOnDay(stopDate)
        ? stopDate
        : date.endOfWeek();
    }

    // Format week info based on number of weeks visible
    // The reason for this is the amount of space available for the text
    const nbrOfWeeks = this.filterService.nbrOfWeeks();
    const weekNbr = date.weekNbr();
    if (nbrOfWeeks > 10) {
      text = `${this.terms['common.weekshort'].toLocaleLowerCase()} ${weekNbr}`;
    } else if (nbrOfDaysInWeek === 1) {
      text = `${this.terms['common.week'].toLocaleLowerCase()} ${weekNbr}`;
    } else {
      const useLongFormat = nbrOfWeeks <= 3;
      const useShortFormat = nbrOfWeeks > 6;
      const useMediumFormat = nbrOfWeeks > 3 && nbrOfWeeks <= 6;
      const shortFormat = 'd/M';
      const mediumFormat = 'eee d MMM';
      const longFormat = 'eeee d MMMM';
      let format = longFormat;
      if (useShortFormat) format = shortFormat;
      else if (useMediumFormat) format = mediumFormat;

      text = startDate.toFormattedDate(format);
      if (text.endsWith('.')) text = text.slice(0, -1);

      if (!startDate.isSameDay(stopDate)) {
        text += ` - ${stopDate.toFormattedDate(format)}`;
        if (text.endsWith('.')) text = text.slice(0, -1);
      }

      const weekTerm =
        this.terms[
          useLongFormat ? 'common.week' : 'common.weekshort'
        ].toLocaleLowerCase();

      text += `, ${weekTerm} ${weekNbr}`;
    }

    return text;
  }

  private createDaySlot(date: Date): SpDaySlot {
    return new SpDaySlot(date, this.getDayInfo(date));
  }

  private getDayInfo(date: Date): string {
    let text = '';

    if (this.filterService.isCommonDayView()) {
      const from = this.filterService.dateFrom();
      const to = this.filterService.dateTo();

      const fromDate = `${DateUtil.format(from, 'eeee')} ${DateUtil.format(from, 'd MMMM')}`;
      const fromTime = from.toFormattedTime();
      const fromWeek = `${this.terms['common.week'].toLocaleLowerCase()} ${DateUtil.format(from, 'w')}`;

      const toDate = `${DateUtil.format(to, 'eeee')} ${DateUtil.format(to, 'd MMMM')}`;
      const toTime = to.addSeconds(1).toFormattedTime();

      if (from.isBeginningOfDay() && to.isEndOfDay()) {
        // Whole day, no time
        text = `${fromDate}, ${fromWeek}`;
      } else if (from.isSameDay(to)) {
        // Part of day, show time
        text = `${fromDate} ${fromTime}-${toTime}, ${fromWeek}`;
      } else {
        text = `${fromDate} ${fromTime} - ${toDate} ${toTime}, ${fromWeek}`;
      }
    } else if (this.filterService.isCommonScheduleView()) {
      // Format day info based on number of weeks visible
      // The reason for this is the amount of space available for the text
      const showingIcon = false; //(this.showUnscheduledTasksPermission && this.unscheduledTaskDates.length > 0);

      const nbrOfWeeks = this.filterService.nbrOfWeeks();
      const useShortFormat = nbrOfWeeks > 4;
      const useMediumFormat =
        (nbrOfWeeks > 2 && nbrOfWeeks <= 4 && showingIcon) || nbrOfWeeks > 3;
      //const useLongFormat = nbrOfWeeks <= 2;
      const shortFormat = 'eeeee';
      const mediumFormat = 'eee d';
      const longFormat = 'eeee d';
      let format = longFormat;
      if (useShortFormat) format = shortFormat;
      else if (useMediumFormat) format = mediumFormat;

      text = date.toFormattedDate(format);
      if (useShortFormat) text = text.left(1).toUpperCase();
    }

    return text;
  }

  private createSumDaySlot(date: Date): SpSumDaySlot {
    return new SpSumDaySlot(date);
  }

  createSumSlotTooltips() {
    this.sumDaySlots().forEach(daySlot => {
      if (this.filterService.isCommonDayView()) {
        daySlot.hourSlots.forEach(hourSlot => {
          this.createSumHourSlotTooltip(hourSlot);
        });
      } else if (this.filterService.isCommonScheduleView()) {
        this.createSumDaySlotTooltip(daySlot);
      }
    });
  }

  private createSumDaySlotTooltip(daySlot: SpSumDaySlot): void {
    const tooltip: string[] = [];

    // Staffing needs
    if (this.settingService.showFollowUpOnNeed()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.staffingneeds.planning.need')}: ${DateUtil.minutesToTimeSpan(daySlot.needTime)}`
      );
    }

    // Net time
    tooltip.push(
      `${this.translate.instant('time.schedule.planning.nettime')}: ${DateUtil.minutesToTimeSpan(daySlot.netTime)}`
    );

    // Factor time
    if (this.settingService.showScheduleTypeFactorTime()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.scheduletypefactortime')}: ${DateUtil.minutesToTimeSpan(daySlot.factorTime)}`
      );
    }

    // Gross time
    if (this.settingService.showGrossTime()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.grosstime')}: ${DateUtil.minutesToTimeSpan(daySlot.grossTime)}`
      );
    }

    // Cost
    if (this.settingService.showTotalCostIncEmpTaxAndSuppCharge()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(daySlot.costIncEmpTaxAndSuppCharge, 0)}`
      );
    } else if (this.settingService.showTotalCost()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(daySlot.cost, 0)}`
      );
    }

    daySlot.tooltip = tooltip.join('\n');
  }

  private createSumHourSlotTooltip(hourSlot: SpSumHourSlot): void {
    const tooltip: string[] = [];

    // Number of shifts
    tooltip.push(
      `${hourSlot.nbrOfShifts} ${hourSlot.nbrOfShifts > 1 ? this.spTranslate.shiftsUndefined().toLocaleLowerCase() : this.spTranslate.shiftUndefined().toLocaleLowerCase()}`
    );

    hourSlot.tooltip = tooltip.join('\n');
  }

  createEmployeeDaySlot(
    date: Date,
    employeeId: number,
    employeeInfo: string
  ): SpEmployeeDaySlot {
    return new SpEmployeeDaySlot(date, employeeId, employeeInfo);
  }

  private createHourSlot(date: Date): SpHourSlot {
    return new SpHourSlot(date, this.getHourInfo(date));
  }

  private getHourInfo(time: Date): string {
    return time.toFormattedTime();
  }

  createEmployeeHourSlot(
    date: Date,
    employeeId: number,
    employeeInfo: string
  ): SpEmployeeHourSlot {
    return new SpEmployeeHourSlot(date, employeeId, employeeInfo);
  }

  private createHalfHourSlot(date: Date): SpHalfHourSlot {
    return new SpHalfHourSlot(date, this.getHalfHourInfo(date));
  }

  private getHalfHourInfo(time: Date): string {
    return time.toFormattedTime();
  }

  createEmployeeHalfHourSlot(
    date: Date,
    employeeId: number,
    employeeInfo: string
  ): SpEmployeeHalfHourSlot {
    return new SpEmployeeHalfHourSlot(date, employeeId, employeeInfo);
  }

  private createQuarterHourSlot(date: Date): SpQuarterHourSlot {
    return new SpQuarterHourSlot(date, this.getQuarterHourInfo(date));
  }

  private getQuarterHourInfo(time: Date): string {
    return time.toFormattedTime();
  }

  createEmployeeQuarterHourSlot(
    date: Date,
    employeeId: number,
    employeeInfo: string
  ): SpEmployeeQuarterHourSlot {
    return new SpEmployeeQuarterHourSlot(date, employeeId, employeeInfo);
  }
}
