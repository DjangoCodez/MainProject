import { DateUtil } from '@shared/util/date-util'
import { NumberUtil } from '@shared/util/number-util'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DayOfWeek } from '@shared/util/Enumerations';
import {
  add as df_add,
  differenceInDays as df_differenceInDays,
  differenceInMinutes as df_differenceInMinutes,
  endOfDay as df_endOfDay,
  endOfHour as df_endOfHour,
  endOfWeek as df_endOfWeek,
  endOfYear as df_endOfYear,
  isAfter as df_isAfter,
  isBefore as df_isBefore,
  isEqual as df_isEqual,
  isSameDay as df_isSameDay,
  isSameMinute as df_isSameMinute,
  isMonday as df_isMonday,
  isSaturday as df_isSaturday,
  isSunday as df_isSunday,
  format as df_format,
  getWeek as df_getWeek,
  startOfHour as df_startOfHour,
  startOfDay as df_startOfDay,
  startOfWeek as df_startOfWeek,
  startOfYear as df_startOfYear,
} from 'date-fns';

export default class PrototypeDateExtensions {}

declare global {
  interface Date {
    // GET
    dayOfWeek: () => DayOfWeek;
    weekNbr: () => number;
    // FORMAT
    toFormattedDate: (format?: string) => string;
    toFormattedTime: (showSeconds?: boolean) => string;
    toFormattedDateTime: (showSeconds?: boolean) => string;
    toDateTimeString: () => string;
    // COMPARE
    isBefore: (date: Date) => boolean;
    isEqual: (date: Date) => boolean;
    isAfter: (date: Date) => boolean;

    isToday: () => boolean;

    // COMPARE: Minute
    isBeforeOnMinute: (date: Date) => boolean;
    isSameOrBeforeOnMinute: (date: Date) => boolean;
    isSameMinute: (date: Date) => boolean;
    isSameOrAfterOnMinute: (date: Date) => boolean;
    isAfterOnMinute: (date: Date) => boolean;
    diffMinutes: (earlierDate: Date) => number;
    // COMPARE: Hour
    isSameHourAs: (date: Date) => boolean;
    // COMPARE: Day
    isBeforeOnDay: (date: Date) => boolean;
    isSameOrBeforeOnDay: (date: Date) => boolean;
    isSameDay: (date: Date) => boolean;
    isSameOrAfterOnDay: (date: Date) => boolean;
    isAfterOnDay: (date: Date) => boolean;
    isBeginningOfDay: () => boolean;
    isEndOfDay: () => boolean;
    isBeginningOfWeek: () => boolean;
    isEndOfWeek: () => boolean;
    isBeginningOfYear: () => boolean;
    isEndOfYear: () => boolean;
    isMonday: () => boolean;
    isSaturday: () => boolean;
    isSunday: () => boolean;
    diffDays: (earlierDate: Date) => number;
    // COMPARE: Month
    isSameMonthAs: (date: Date) => boolean;
    // COMPARE: Year
    isSameYearAs: (date: Date) => boolean;

    // MANIPULATE (Return new date)
    addYears: (years: number) => Date;
    addMonths: (months: number) => Date;
    addWeeks: (weeks: number) => Date;
    addDays: (days: number) => Date;
    addHours: (hours: number) => Date;
    addMinutes: (minutes: number) => Date;
    addSeconds: (seconds: number) => Date;
    beginningOfHour: () => Date;
    beginningOfDay: () => Date;
    endOfDay: () => Date;
    endOfHour: () => Date;
    beginningOfWeek: () => Date;
    endOfWeek: () => Date;
    beginningOfYear: () => Date;
    endOfYear: () => Date;
    // MUTATE (Modify current date)
    clearSeconds: () => void;
    clearMinutes: () => void;
    clearHours: () => void;
    clearDate: () => void;
    // MANIPULATE or MUTATE
    mergeTime: (time: Date, mutate?: boolean) => Date;
    mergeTimeSpan: (timeSpan: string, mutate?: boolean) => Date;
  }
}

// GET
Date.prototype.dayOfWeek = function (): DayOfWeek {
  return <DayOfWeek>this.getDay();
};

Date.prototype.weekNbr = function (): number {
  return df_getWeek(this);
};

// FORMAT
Date.prototype.toFormattedDate = function (format?: string): string {
  return format
    ? df_format(this, format, { locale: SoeConfigUtil.dateFnsLocale })
    : this.toLocaleDateString();
};

Date.prototype.toFormattedTime = function (showSeconds = false): string {
  let formatted = NumberUtil.padZeroLen2(this.getHours()).toString() + ':';
  formatted += NumberUtil.padZeroLen2(this.getMinutes()).toString();
  if (showSeconds)
    formatted += ':' + NumberUtil.padZeroLen2(this.getSeconds()).toString();

  return formatted;
};

Date.prototype.toFormattedDateTime = function (showSeconds = false): string {
  const year = this.getFullYear();
  const month = this.getMonth() + 1;
  const day = this.getDate();
  const hour = this.getHours();
  const minute = this.getMinutes();
  const second = this.getSeconds();

  let formatted = year.toString() + '-';
  formatted += NumberUtil.padZeroLen2(month).toString() + '-';
  formatted += NumberUtil.padZeroLen2(day).toString() + ' ';

  formatted += NumberUtil.padZeroLen2(hour).toString() + ':';
  formatted += NumberUtil.padZeroLen2(minute).toString();
  if (showSeconds) formatted += ':' + NumberUtil.padZeroLen2(second).toString();

  return formatted;
};

Date.prototype.toDateTimeString = function (): string {
  const year = this.getFullYear();
  const month = this.getMonth() + 1;
  const day = this.getDate();
  const hour = this.getHours();
  const minute = this.getMinutes();
  const second = this.getSeconds();

  return `${year}${NumberUtil.padZeroLen2(month)}${NumberUtil.padZeroLen2(
    day
  )}T${NumberUtil.padZeroLen2(hour)}${NumberUtil.padZeroLen2(
    minute
  )}${NumberUtil.padZeroLen2(second)}`;
};

// COMPARE
Date.prototype.isBefore = function (date: Date): boolean {
  return df_isBefore(this, date);
};

Date.prototype.isEqual = function (date: Date): boolean {
  // Match exactly, down to millisecond
  // If only date is to be compared, use isSameDay() instead
  return df_isEqual(this, date);
};

Date.prototype.isAfter = function (date: Date): boolean {
  return df_isAfter(this, date);
};

Date.prototype.isToday = function (): boolean {
  return this.toDateString() == new Date(Date.now()).toDateString();
};

// COMPARE: Minute
Date.prototype.isBeforeOnMinute = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearSeconds();
  secondDate.clearSeconds();

  return firstDate < secondDate;
};

Date.prototype.isSameOrBeforeOnMinute = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearSeconds();
  secondDate.clearSeconds();

  return firstDate <= secondDate;
};

Date.prototype.isSameMinute = function (date: Date): boolean {
  return df_isSameMinute(this, date);
};

Date.prototype.isSameOrAfterOnMinute = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearSeconds();
  secondDate.clearSeconds();

  return firstDate >= secondDate;
};

Date.prototype.isAfterOnMinute = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearSeconds();
  secondDate.clearSeconds();

  return firstDate > secondDate;
};

Date.prototype.diffMinutes = function (earlierDate: Date): number {
  return df_differenceInMinutes(this, earlierDate);
};

// COMPARE: Hour
Date.prototype.isSameHourAs = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearMinutes();
  secondDate.clearMinutes();

  return (
    firstDate.toDateString() === secondDate.toDateString() &&
    firstDate.toTimeString() === secondDate.toTimeString()
  );
};

// COMPARE: Day
Date.prototype.isBeforeOnDay = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearHours();
  secondDate.clearHours();

  return firstDate < secondDate;
};

Date.prototype.isSameOrBeforeOnDay = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearHours();
  secondDate.clearHours();

  return firstDate <= secondDate;
};

Date.prototype.isSameDay = function (date: Date): boolean {
  return df_isSameDay(this, date);
};

Date.prototype.isSameOrAfterOnDay = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearHours();
  secondDate.clearHours();

  return firstDate >= secondDate;
};

Date.prototype.isAfterOnDay = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  firstDate.clearHours();
  secondDate.clearHours();

  return firstDate > secondDate;
};

Date.prototype.isBeginningOfDay = function (): boolean {
  return this.isSameMinute(this.beginningOfDay());
};

Date.prototype.isEndOfDay = function (): boolean {
  return (
    this.getHours() === 23 &&
    this.getMinutes() === 59 &&
    this.getSeconds() === 59
  );
};

Date.prototype.isBeginningOfWeek = function (): boolean {
  return this.isSameDay(this.beginningOfWeek());
};

Date.prototype.isEndOfWeek = function (): boolean {
  return this.isSameDay(this.endOfWeek());
};

Date.prototype.isBeginningOfYear = function (): boolean {
  return this.isSameDay(this.beginningOfYear());
};

Date.prototype.isEndOfYear = function (): boolean {
  return this.isSameDay(this.endOfYear());
};

Date.prototype.isMonday = function (): boolean {
  return df_isMonday(this);
};

Date.prototype.isSaturday = function (): boolean {
  return df_isSaturday(this);
};

Date.prototype.isSunday = function (): boolean {
  return df_isSunday(this);
};

Date.prototype.diffDays = function (earlierDate: Date): number {
  return df_differenceInDays(this, earlierDate);
};

// COMPARE: Month
Date.prototype.isSameMonthAs = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  return (
    firstDate.getFullYear() === secondDate.getFullYear() &&
    firstDate.getMonth() === secondDate.getMonth()
  );
};

// COMPARE: Year
Date.prototype.isSameYearAs = function (date: Date): boolean {
  if (!date) return false;

  const firstDate = new Date(this.getTime());
  const secondDate = new Date(date.getTime());

  return firstDate.getFullYear() === secondDate.getFullYear();
};

// MANIPULATE
Date.prototype.addYears = function (years: number): Date {
  return df_add(this, { years: years });
};

Date.prototype.addMonths = function (months: number): Date {
  return df_add(this, { months: months });
};

Date.prototype.addWeeks = function (weeks: number): Date {
  return df_add(this, { weeks: weeks });
};

Date.prototype.addDays = function (days: number): Date {
  return df_add(this, { days: days });
};

Date.prototype.addHours = function (hours: number): Date {
  return df_add(this, { hours: hours });
};

Date.prototype.addMinutes = function (minutes: number): Date {
  return df_add(this, { minutes: minutes });
};

Date.prototype.addSeconds = function (seconds: number): Date {
  return df_add(this, { seconds: seconds });
};

Date.prototype.beginningOfHour = function (): Date {
  return df_startOfHour(this);
};

Date.prototype.beginningOfDay = function (): Date {
  return df_startOfDay(this);
};

Date.prototype.endOfDay = function (): Date {
  return df_endOfDay(this);
};

Date.prototype.endOfHour = function (): Date {
  return df_endOfHour(this);
};

Date.prototype.beginningOfWeek = function (): Date {
  return df_startOfWeek(this, { weekStartsOn: DayOfWeek.Monday });
};

Date.prototype.endOfWeek = function (): Date {
  return df_endOfWeek(this, { weekStartsOn: DayOfWeek.Monday });
};

Date.prototype.beginningOfYear = function (): Date {
  return df_startOfYear(this);
};

Date.prototype.endOfYear = function (): Date {
  return df_endOfYear(this);
};

Date.prototype.clearSeconds = function (): void {
  this.setSeconds(0, 0);
};

Date.prototype.clearMinutes = function (): void {
  this.setMinutes(0, 0, 0);
};

Date.prototype.clearHours = function (): void {
  this.setHours(0, 0, 0, 0);
};

Date.prototype.clearDate = function (): void {
  const newDate = DateUtil.defaultDateTime();
  newDate.setHours(this.getHours());
  newDate.setMinutes(this.getMinutes());
  newDate.setSeconds(this.getSeconds());
  newDate.setMilliseconds(0);

  this.setTime(newDate.getTime());
};

Date.prototype.mergeTime = function (time: Date, mutate = false): Date {
  if (mutate) {
    this.setHours(time.getHours());
    this.setMinutes(time.getMinutes());
    this.setSeconds(time.getSeconds());
    return this;
  } else {
    const newDate = new Date(this.getTime());
    newDate.setHours(time.getHours());
    newDate.setMinutes(time.getMinutes());
    newDate.setSeconds(time.getSeconds());
    return newDate;
  }
};

Date.prototype.mergeTimeSpan = function (
  timeSpan: string,
  mutate = false
): Date {
  let hours = 0;
  let minutes = 0;
  let seconds = 0;

  if (timeSpan) {
    const parts: string[] = timeSpan.split(':');
    if (parts.length > 0) hours = Number(parts[0]);
    if (parts.length > 1) minutes = Number(parts[1]);
    if (parts.length > 2) seconds = Number(parts[2]);
  }

  if (mutate) {
    this.setHours(hours);
    this.setMinutes(minutes);
    this.setSeconds(seconds);
    return this;
  } else {
    const newDate = new Date(this.getTime());
    newDate.setHours(hours);
    newDate.setMinutes(minutes);
    newDate.setSeconds(seconds);
    return newDate;
  }
};
