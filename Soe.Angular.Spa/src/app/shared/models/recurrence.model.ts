import {
  IDailyRecurrenceDatesOutput,
  IDailyRecurrencePatternDTO,
  IDailyRecurrenceRangeDTO,
} from './generated-interfaces/DailyRecurrencePatternDTO';
import {
  DailyRecurrencePatternType,
  DailyRecurrencePatternWeekIndex,
  DailyRecurrenceRangeType,
} from './generated-interfaces/Enumerations';
import { TranslateService } from '@ngx-translate/core';
import { DateUtil } from '@shared/util/date-util';
import { DayOfWeek } from './generated-interfaces/ClientEnumerations';
import { DailyRecurrencePatternDialogResult } from '@shared/components/daily-recurrence-pattern-dialog/daily-recurrence-pattern-dialog.component';
import { TimeScheduleTasksForm } from '@features/time/time-schedule-tasks/models/time-schedule-tasks-form.model';
import { IncomingDeliveriesForm } from '@features/time/incoming-deliveries/models/incoming-deliveries-form.model';

type RecurrenceForm =
  | TimeScheduleTasksForm
  | IncomingDeliveriesForm
  | undefined;

export class DailyRecurrencePatternDTO implements IDailyRecurrencePatternDTO {
  type: DailyRecurrencePatternType = DailyRecurrencePatternType.Daily;
  interval: number = 1;
  dayOfMonth: number = 1;
  month: number = 0;
  daysOfWeek: DayOfWeek[] = [];
  firstDayOfWeek: DayOfWeek = DayOfWeek.Monday;
  weekIndex: DailyRecurrencePatternWeekIndex =
    DailyRecurrencePatternWeekIndex.First;
  sysHolidayTypeIds: number[] = [];

  get isPatternTypeNone(): boolean {
    return this.type === DailyRecurrencePatternType.None;
  }

  toString(): string {
    const days: number[] = [];
    if (this.daysOfWeek) {
      this.daysOfWeek.forEach(day => {
        days.push(day);
      });
    }

    return '{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}'.format(
      this.type ? this.type.toString() : '',
      !this.isPatternTypeNone && this.interval ? this.interval.toString() : '',
      !this.isPatternTypeNone && this.dayOfMonth
        ? this.dayOfMonth.toString()
        : '',
      !this.isPatternTypeNone && this.month ? this.month.toString() : '',
      !this.isPatternTypeNone && days.length > 0 ? days.join(',') : '',
      !this.isPatternTypeNone && this.firstDayOfWeek
        ? this.firstDayOfWeek.toString()
        : '',
      !this.isPatternTypeNone && this.weekIndex
        ? this.weekIndex.toString()
        : '',
      !this.isPatternTypeNone &&
        this.sysHolidayTypeIds &&
        this.sysHolidayTypeIds.length > 0
        ? this.sysHolidayTypeIds.join(',')
        : ''
    );
  }

  static parse(str: string): DailyRecurrencePatternDTO | undefined {
    const parts: string[] = str.split('_');
    if (parts.length !== 7 && parts.length !== 8) return undefined;

    const dto: DailyRecurrencePatternDTO = new DailyRecurrencePatternDTO();

    dto.type = parseInt(parts[0], 10) || 0;
    if (dto.type !== DailyRecurrencePatternType.None) {
      dto.interval = parseInt(parts[1], 10) || 0;
      dto.dayOfMonth = parseInt(parts[2], 10) || 0;
      dto.month = parseInt(parts[3], 10) || 0;

      // Days are comma separated
      const strDays: string[] = parts[4].split(',');
      const days: DayOfWeek[] = [];
      strDays.forEach(strDay => {
        if (strDay.length > 0) days.push(parseInt(strDay, 10));
      });
      dto.daysOfWeek = days;

      dto.firstDayOfWeek = parseInt(parts[5], 10) || 0;
      dto.weekIndex = parseInt(parts[6], 10) || 0;

      if (parts.length > 7) {
        // Sys holidays are comma separated
        const strSysHolidayTypeIds: string[] = parts[7].split(',');
        const sysDays: number[] = [];
        strSysHolidayTypeIds.forEach(strSysHolidayTypeId => {
          if (strSysHolidayTypeId.length > 0)
            sysDays.push(parseInt(strSysHolidayTypeId, 10));
        });
        dto.sysHolidayTypeIds = sysDays;
      }
    }

    return dto;
  }
}

export class DailyRecurrenceRangeDTO implements IDailyRecurrenceRangeDTO {
  type: DailyRecurrenceRangeType = DailyRecurrenceRangeType.NoEnd;
  startDate: Date = DateUtil.getToday();
  endDate?: Date;
  numberOfOccurrences = 0;

  toString(): string {
    return '{0}_{1}_{2}_{3}'.format(
      this.type ? this.type.toString() : '',
      this.startDate ? this.startDate.toFormattedDate('YYYY-MM-DD') : '',
      this.endDate ? this.endDate.toFormattedDate('YYYY-MM-DD') : '',
      this.numberOfOccurrences ? this.numberOfOccurrences.toString() : ''
    );
  }

  static parse(str: string): DailyRecurrenceRangeDTO | undefined {
    const parts: string[] = str.split('_');
    if (parts.length !== 4) return undefined;

    const dto: DailyRecurrenceRangeDTO = new DailyRecurrenceRangeDTO();

    // Type
    dto.type = parseInt(parts[0], 10) || 0;

    // Start date
    if (parts[1].length > 0) {
      const dateParts = parts[1].split('-');
      dto.startDate = new Date(
        parseInt(dateParts[0], 10),
        parseInt(dateParts[1], 10) - 1,
        parseInt(dateParts[2], 10)
      );
    }

    // End date
    if (parts[2].length > 0) {
      const dateParts = parts[2].split('-');
      dto.endDate = new Date(
        parseInt(dateParts[0], 10),
        parseInt(dateParts[1], 10) - 1,
        parseInt(dateParts[2], 10)
      );
    }

    // Number of occurrences
    dto.numberOfOccurrences = parseInt(parts[3], 10) || 0;

    return dto;
  }

  static setRecurrenceInfo(form: RecurrenceForm, translate: TranslateService) {
    if (form) {
      if (!form.value.startDate)
        form.patchValue({ startDate: DateUtil.getToday() });
      if (!form.value.nbrOfOccurrences)
        form.patchValue({ nbrOfOccurrences: 0 });

      const startDescription = translate
        .instant('common.dailyrecurrencepattern.startson')
        .format(form.value.startDate.toFormattedDate());

      const type: DailyRecurrenceRangeType = this.calculateType(
        form.value.stopDate,
        form.value.nbrOfOccurrences
      );

      let endDescription = '';
      if (type === DailyRecurrenceRangeType.EndDate) {
        if (form.value.stopDate) {
          endDescription = translate
            .instant('common.dailyrecurrencepattern.endson')
            .format(form.value.stopDate.toFormattedDate());
        }
      } else if (type === DailyRecurrenceRangeType.Numbered) {
        endDescription = translate
          .instant('common.dailyrecurrencepattern.endsafter')
          .format(form.value.nbrOfOccurrences.toString());
      }

      form.patchValue({
        recurrenceStartsOnDescription: startDescription,
        recurrenceEndsOnDescription: endDescription,
      });
    }
  }

  static calculateType(
    endDate: Date | undefined,
    numberOfOccurrences: number
  ): DailyRecurrenceRangeType {
    if (endDate) return DailyRecurrenceRangeType.EndDate;
    else if (numberOfOccurrences) return DailyRecurrenceRangeType.Numbered;
    else return DailyRecurrenceRangeType.NoEnd;
  }
}

export class DailyRecurrenceParamsDTO {
  date: Date;
  range: DailyRecurrenceRangeDTO;
  pattern?: DailyRecurrencePatternDTO;

  constructor(form: RecurrenceForm) {
    if (form) {
      this.date = form.value.startDate ?? DateUtil.getToday();
      this.range = new DailyRecurrenceRangeDTO();
      this.range.type = DailyRecurrenceRangeDTO.calculateType(
        form.value.stopDate,
        form.value.nbrOfOccurrences
      );
      this.range.numberOfOccurrences = form.value.nbrOfOccurrences;
      this.range.startDate = form.value.startDate;
      this.range.endDate = form.value.stopDate;
      if (form.value.recurrencePattern)
        this.pattern = DailyRecurrencePatternDTO.parse(
          form.value.recurrencePattern
        );
    } else {
      this.date = DateUtil.getToday();
      this.range = new DailyRecurrenceRangeDTO();
      this.pattern = new DailyRecurrencePatternDTO();
    }
  }

  parseResult(
    form: RecurrenceForm,
    result: DailyRecurrencePatternDialogResult
  ) {
    if (form && result) {
      if (result.pattern) {
        // Need to create DTO object to be able to call custom toString() on it
        const pattern = new DailyRecurrencePatternDTO();
        Object.assign(pattern, result.pattern);
        form.patchValue({
          recurrencePattern: pattern.toString(),
        });
      }

      if (result.range) {
        form.patchValue({
          startDate: result.range.startDate,
          stopDate: result.range.endDate,
          nbrOfOccurrences: result.range.numberOfOccurrences,
        });
      }

      result.excludedDates.forEach(d => d.clearHours());
      form.customExcludedDatesPatchValue(result.excludedDates);
    }
  }
}

export class DailyRecurrenceDatesOutput implements IDailyRecurrenceDatesOutput {
  recurrenceDates: Date[] = [];
  removedDates: Date[] = [];

  getValidDates(includeRemovedDates: boolean = false): Date[] {
    return includeRemovedDates
      ? this.recurrenceDates
      : this.recurrenceDates.filter(
          d => !DateUtil.includesDate(this.removedDates, d)
        );
  }

  doRecurOnDate(date: Date, includeRemovedDates: boolean = false): boolean {
    return DateUtil.includesDate(this.getValidDates(includeRemovedDates), date);
  }

  doRecurOnDateButIsRemoved(date: Date): boolean {
    return (
      DateUtil.includesDate(this.recurrenceDates, date) &&
      DateUtil.includesDate(this.removedDates, date)
    );
  }

  hasRecurringDates(includeRemovedDates: boolean = false): boolean {
    return this.getValidDates(includeRemovedDates).length > 0;
  }
}
