import { NumberUtil } from './number-util';
import {
  addDays as df_addDays,
  differenceInDays as df_differenceInDays,
  differenceInMonths as df_differenceInMonths,
  differenceInYears as df_differenceInYears,
  format as df_format,
  getWeekYear as df_getWeekYear,
  getWeek as df_getWeek,
  getISOWeeksInYear as df_getWeeksInYear,
  isValid as df_isValid,
  lastDayOfMonth as df_lastDayOfMonth,
  lastDayOfWeek as df_lastDayOfWeek,
  parse as df_parse,
  parseISO as df_parseISO,
  Locale,
  LocaleWidth,
  Month,
} from 'date-fns';
import { SoeConfigUtil } from './soeconfig-util';
import { ViewFormat } from '@ui/forms/datepicker/mat-date-format.class';
import { DayOfWeek } from './Enumerations';
import { StringUtil } from './string-util';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { sv } from 'date-fns/locale';

let allLocales: any;
import('date-fns/locale').then(locales => {
  allLocales = locales;
});

const MINUTES_IN_A_DAY = 1440;
const MINUTES_IN_AN_HOUR = 60;

export class DateUtil {
  // GET
  static defaultDateTime(): Date {
    return new Date(1900, 0, 1);
  }

  static getToday() {
    const date = new Date();
    date.setHours(0, 0, 0, 0);
    return date;
  }

  static getQuarter(date: Date) {
    const month = date.getMonth();
    if (month < 3) return 1;
    else if (month >= 3 && month < 6) return 2;
    else if (month >= 6 && month < 9) return 3;
    else return 4;
  }

  static getWeekNr(dayNumber: number): number {
    if (dayNumber < 1) dayNumber = 1;

    return Math.floor((dayNumber - 1) / 7) + 1;
  }

  static getWeekYear(date: Date) {
    return df_getWeekYear(date, {
      weekStartsOn: SoeConfigUtil.firstDayOfWeek as any,
    });
  }

  static getDateFirstInYear(date: Date): Date {
    return new Date(date.getFullYear(), 0, 1);
  }

  static getDateLastInYear(date: Date): Date {
    return new Date(date.getFullYear(), 11, 31);
  }

  static getDateFirstInMonth(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), 1);
  }

  static getDateLastInMonth(date: Date): Date {
    return df_lastDayOfMonth(date);
  }

  static getDateFirstInWeek(date: Date): Date {
    return df_addDays(this.getDateLastInWeek(date), -6);
  }

  static getDateLastInWeek(date: Date): Date {
    return df_lastDayOfWeek(date, { locale: this.getLocale('sv-SE') });
  }

  static getWeekNumber(date: Date): number {
    return df_getWeek(date, {
      weekStartsOn: SoeConfigUtil.firstDayOfWeek as any,
    });
  }

  static getWeekCountInYear(year: number): number {
    return df_getWeeksInYear(new Date(year, 0, 1));
  }

  static getMinDate(date1: Date, date2: Date): Date {
    return date1 < date2 ? date1 : date2;
  }

  static getMaxDate(date1: Date, date2: Date): Date {
    return date1 > date2 ? date1 : date2;
  }

  static getUniqueDates(dates: Date[]): Date[] {
    const sortedDates = DateUtil.sortImmutable(dates);
    const uniqueDates: Date[] = [];
    const seen = new Set<number>();

    for (const date of sortedDates) {
      const dayKey = date.getTime();
      if (!seen.has(dayKey)) {
        seen.add(dayKey);
        uniqueDates.push(date);
      }
    }
    return uniqueDates;
  }

  // PARSE
  static parseDate(value: string, format?: string): Date | undefined {
    return value
      ? df_parse(value, format || 'yyyy-MM-dd', new Date())
      : undefined;
  }

  static parseDateOrJson(value: string | Date | undefined): Date | undefined {
    if (!value) return undefined;
    if (value instanceof Date) return value;

    return df_parseISO(value.toString());
  }

  static parseTimeSpan(
    str: string,
    useSeconds = false,
    padHours = false,
    allowNegative = true,
    allowMoreThan99Hours = false
  ): string {
    let isNegative = false;
    if (str) {
      // Check if negative
      if (str.startsWithCaseInsensitive('-')) {
        isNegative = true;
        str = str.substring(1);
      }

      // Check for decimals
      if (str.includes(',') || str.includes('.')) {
        let parts: string[] = [];
        if (str.includes(',')) parts = str.split(',');
        else if (str.includes('.')) parts = str.split('.');

        if (parts.length > 0) {
          const minutePart = parts[1];
          if (minutePart.length > 1) {
            str = `${parts[0]}:${parts[1].substring(0, 2)}`;
            if (parts.length > 2 && useSeconds)
              str += `:${parts[2].substring(0, 2)}`;
          } else {
            const num = NumberUtil.parseDecimal('0.' + minutePart);
            str = `${parts[0]}:${(60 * num).toString().substring(0, 2)}`;
            if (parts.length > 2 && useSeconds) str += `:${parts[2]}`;
          }
        }
      }

      if (!str.includes(':') && !allowMoreThan99Hours) {
        // No separator entered, check length to see if more than hour is specified
        if (str.length > 4 && useSeconds)
          str = `${str.left(str.length - 4)}:${str.substring(
            str.length - 4,
            2
          )}:${str.right(2)}`;
        else if (str.length > 2)
          str = `${str.left(str.length - 2)}:${str.right(2)}`;
      }
    }

    let hours = 0;
    let minutes = 0;
    let seconds = 0;

    if (str) {
      const parts: string[] = str.split(':');
      if (parts.length > 0) hours = Number(parts[0]);
      if (parts.length > 1) minutes = Number(parts[1]);
      if (parts.length > 2) seconds = Number(parts[2]);
    }

    let span = `${
      padHours ? NumberUtil.padZeroLen2(hours) : hours.toString()
    }:${NumberUtil.padZeroLen2(minutes)}`;
    if (useSeconds) span += `:${NumberUtil.padZeroLen2(seconds)}`;

    if (isNegative && allowNegative) span = `-${span}`;
    return span;
  }

  // VALIDATE
  static isValidDate(value: Date): boolean {
    if (value && !df_isValid(value)) return false;

    return true;
  }

  static isValidDateOrString(value: Date | string): boolean {
    if (value && value instanceof Date) return this.isValidDate(value);

    if (value) {
      const parsedDate = this.parseDateOrJson(value);
      if (parsedDate) return df_isValid(parsedDate);
    }

    return true;
  }

  static isWithinRange(date: Date, rangeStart: Date, rangeStop: Date): boolean {
    return date >= rangeStart && date <= rangeStop;
  }

  static includesDate(
    dates: Date[],
    date: Date,
    checkByTime: boolean = false
  ): boolean {
    let exists: boolean = false;
    if (dates && dates.length > 0) {
      for (let i = 0, j = dates.length; i < j; i++) {
        if (
          dates[i] &&
          ((checkByTime && dates[i].isSameMinute(date)) ||
            (!checkByTime && dates[i].isSameDay(date)))
        ) {
          exists = true;
          break;
        }
      }
    }

    return exists;
  }

  static isDefaultDateTime(date: Date | undefined): boolean {
    if (
      (date &&
        date.getFullYear() === 1970 &&
        date.getMonth() === 0 &&
        date.getDate() === 1) ||
      date == this.defaultDateTime()
    )
      return true;

    return false;
  }

  static isFullMonth(dateFrom: Date, dateTo: Date): boolean {
    if (!dateFrom || !dateTo) return false;

    // Must be same year & month
    if (dateFrom.getFullYear() !== dateTo.getFullYear()) return false;
    if (dateFrom.getMonth() !== dateTo.getMonth()) return false;

    // First day check
    if (dateFrom.getDate() !== 1) return false;

    // Last day check
    const lastDay = df_lastDayOfMonth(dateFrom).getDate();
    return dateTo.getDate() === lastDay;
  }

  // CONVERT
  static minutesToTimeSpan(
    value: number,
    showDays = false,
    showSeconds = false,
    padHours = false,
    maxOneDay = false
  ): string {
    if (!value) value = 0;

    const isNegative: boolean = value < 0;

    if (isNegative) value = value * -1;

    let days = Math.floor(value / MINUTES_IN_A_DAY);
    const remainingMinutes = value % MINUTES_IN_A_DAY;

    let hours = Math.floor(remainingMinutes / MINUTES_IN_AN_HOUR);
    const minutes = (remainingMinutes % MINUTES_IN_AN_HOUR).round(0);

    if (!showDays) {
      hours += days * 24;
      days = 0;
      if (maxOneDay && hours > 23) {
        while (hours > 23) {
          hours -= 24;
        }
      }
    }

    let formatted = '';

    // Add days
    if (days !== 0) formatted += days + '.';

    // Add time
    const formattedHours =
      padHours || (showDays && days > 0)
        ? NumberUtil.padZeroLen2(hours)
        : hours.toString();

    formatted += `${formattedHours}:${NumberUtil.padZeroLen2(minutes)}`;

    if (showSeconds) formatted += `:${NumberUtil.padZeroLen2(0)}`;

    if (isNegative) formatted = '-' + formatted;

    return formatted;
  }

  static timeSpanToMinutes(span: string): number {
    if (!span) return 0;

    // Check if negative
    let isNegative = false;
    if (span.startsWithCaseInsensitive('-')) {
      isNegative = true;
      span = span.substring(1);
    }

    span = this.parseTimeSpan(span);

    let hours = 0;
    let minutes = 0;

    const parts: string[] = span.split(':');
    hours = Number(parts[0]);
    minutes = Number(parts[1]);

    let result = hours * 60 + minutes;
    if (isNegative) result = -result;

    return result;
  }

  // FORMAT
  static format(date: number | Date, format: string): string {
    return date
      ? df_format(date, format, {
          locale: this.getLocale(),
          weekStartsOn: SoeConfigUtil.firstDayOfWeek as any,
        })
      : '';
  }

  static toSwedishFormattedDate(date: number | Date): string {
    return date ? df_format(date, 'yyyy-MM-dd') : '';
  }

  static toDateTimeString(date: Date): string {
    const year = '' + date.getFullYear();
    const month = '' + (date.getMonth() + 1);
    const day = '' + date.getDate();
    const hour = '' + date.getHours();
    const minute = '' + date.getMinutes();
    const second = '' + date.getSeconds();

    return (
      [year, NumberUtil.padZeroLen2(month), NumberUtil.padZeroLen2(day)].join(
        ''
      ) +
      'T' +
      [
        NumberUtil.padZeroLen2(hour),
        NumberUtil.padZeroLen2(minute),
        NumberUtil.padZeroLen2(second),
      ].join('')
    );
  }

  static toDateString(date: Date): string {
    const year = '' + date.getFullYear();
    const month = '' + (date.getMonth() + 1);
    const day = '' + date.getDate();

    return (
      [year, NumberUtil.padZeroLen2(month), NumberUtil.padZeroLen2(day)].join(
        ''
      ) +
      'T' +
      ['00', '00', '00'].join('')
    );
  }

  static getSeparatorFromFormattedDate(formattedDate: string): string {
    // Use a regular expression to find the separator
    const separatorMatch = formattedDate.match(/[^0-9]/);

    // If a match is found, return the separator, otherwise return a default value
    return separatorMatch ? separatorMatch[0] : '/';
  }

  static parseDateByString(str: string): Date | null {
    const separator = this.getSeparatorFromFormattedDate(
      df_format(new Date(), 'P', { locale: this.getLocale() })
    );
    const isFinnish = separator === '.';
    const isEnglish = separator === '/';

    // Default date
    let date: Date | null = new Date();
    let year = date.getFullYear();
    let month = date.getMonth() + 1;
    let day = date.getDate();
    let success = false;

    const len = str ? str.length : 0;
    const dateParts: string[] = str ? str.split(separator) : [];

    // Parse entered date string
    if (dateParts.length > 1) {
      // User has typed at least one separator
      if (dateParts.length === 2) {
        // User has only typed one separator, use preceeding and trailing digits as month and day in current year
        if (isFinnish) {
          day = NumberUtil.tryParseInt(dateParts[0], day);
          month = NumberUtil.tryParseInt(dateParts[1], month);
        } else {
          if (NumberUtil.tryParseInt(dateParts[0], month) > 1000) {
            year = NumberUtil.tryParseInt(dateParts[0], year);
            month = NumberUtil.tryParseInt(dateParts[1], month);
          } else {
            month = NumberUtil.tryParseInt(dateParts[0], month);
            day = NumberUtil.tryParseInt(dateParts[1], day);
          }
        }
        success = true;
      } else if (dateParts.length === 3) {
        // User has typed two separators
        if (isFinnish) {
          day = NumberUtil.tryParseInt(dateParts[0], day);
          month = NumberUtil.tryParseInt(dateParts[1], month);
          year = NumberUtil.tryParseInt(dateParts[2], year);
        } else if (isEnglish) {
          year = NumberUtil.tryParseInt(dateParts[2], year);
          month = NumberUtil.tryParseInt(dateParts[0], month);
          day = NumberUtil.tryParseInt(dateParts[1], day);
        } else {
          year = NumberUtil.tryParseInt(dateParts[0], year);
          month = NumberUtil.tryParseInt(dateParts[1], month);
          day = NumberUtil.tryParseInt(dateParts[2], day);
        }
        success = true;
      }
    } else {
      // User has only typed digits, no separators
      switch (len) {
        case 0:
          // Date cleared
          success = true;
          break;
        case 1:
        case 2:
          // Only day is entered
          day = NumberUtil.tryParseInt(str, day);
          success = true;
          break;
        case 3:
        case 4:
          // Month and day is entered
          if (isFinnish) {
            // Finnish vesion other way around (short date starts with day)
            day = NumberUtil.tryParseInt(str.left(2), day);
            month = NumberUtil.tryParseInt(str.substring(2), month);
            success = true;
          } else {
            month = NumberUtil.tryParseInt(str.left(len - 2), month);
            day = NumberUtil.tryParseInt(str.right(2), day);
            success = true;
          }
          break;
        case 5:
        case 6:
          if (NumberUtil.tryParseInt(str.substring(2, 4), month) > 12) {
            // Year (4 digits), month (2 digits)
            if (isFinnish) {
              // Finnish vesion other way around (short date starts with day)
              month = NumberUtil.tryParseInt(str.left(2), month);
              year = NumberUtil.tryParseInt(str.substring(2), year);
              success = true;
            } else {
              year = NumberUtil.tryParseInt(str.left(4), year);
              month = NumberUtil.tryParseInt(str.substring(4), month);
              success = true;
            }
          } else {
            // Year (2 digits), month and day is entered
            if (isFinnish) {
              // Finnish vesion other way around (short date starts with day)
              day = NumberUtil.tryParseInt(str.left(2), day);
              month = NumberUtil.tryParseInt(str.substring(2, 4), month);
              year = NumberUtil.tryParseInt(str.substring(4), year);
              success = true;
            } else {
              year = NumberUtil.tryParseInt(str.left(2), year);
              month = NumberUtil.tryParseInt(str.substring(2, 4), month);
              day = NumberUtil.tryParseInt(str.substring(4), day);
              success = true;
            }
          }
          break;
        case 7:
        case 8:
          // Year (4 digits), month and day is entered
          if (isFinnish) {
            // Finnish vesion other way around (short date starts with day)
            day = NumberUtil.tryParseInt(str.left(2), day);
            month = NumberUtil.tryParseInt(str.substring(2, 4), month);
            year = NumberUtil.tryParseInt(str.substring(4), year);
            success = true;
          } else {
            year = NumberUtil.tryParseInt(str.left(4), year);
            month = NumberUtil.tryParseInt(str.substring(4, 6), month);
            day = NumberUtil.tryParseInt(str.substring(6), day);
            success = true;
          }
          break;
        default:
          // More than eight digits has been entered
          break;
      }
    }

    // Successfully parsed
    if (success && len > 0) {
      // Add current century to year if not specified
      if (year < 100)
        year = parseInt(date.getFullYear().toString().left(2)) * 100 + year;

      // Create new date from parsed values
      date = new Date(year, month - 1, day, 0, 0, 0);
    } else {
      date = null;
    }

    return date;
  }

  static getISODateString(date: Date): string {
    return date.toFormattedDate('yyyyMMdd') + 'T000000';
  }

  static getLocale(langStr = ''): Locale {
    if (!allLocales) return sv;

    const lang: string = (
      langStr !== '' ? langStr : SoeConfigUtil.language
    ).replace('-', '');
    const rootLocale = lang.substring(0, 2);
    return allLocales[lang] || allLocales[rootLocale];
  }

  static localeDateFormat(date: Date | string): string {
    const locale = this.getLocale();
    if (
      locale &&
      (date instanceof String ||
        (date instanceof Date && DateUtil.isValidDate(date)))
    ) {
      return df_format(date, 'P', { locale: locale });
    } else {
      return date.toString();
    }
  }

  static localeTimeFormat(date: Date, use24HourFormat = false): string {
    const locale = this.getLocale(use24HourFormat ? 'sv-SE' : '');
    if (locale && date instanceof Date && DateUtil.isValidDate(date)) {
      return df_format(date, 'p', { locale: locale });
    } else {
      return date.toTimeString();
    }
  }

  static getLocalDateFromUTCDate(utcDate: Date | string): Date {
    return new Date(utcDate + ' UTC');
  }

  static get languageDateFormat(): string {
    const lang: string = SoeConfigUtil.language;
    if (lang.startsWith('sv-')) return 'YYYY-MM-DD';
    else if (lang.startsWith('fi-')) return 'DD.MM.YYYY';
    else return 'MM/DD/YYYY';
  }

  static get languageDateFormatText(): string {
    const lang: string = SoeConfigUtil.language;
    if (lang.startsWith('sv-')) return 'åååå-mm-dd';
    else if (lang.startsWith('fi-')) return 'dd.mm.yyyy';
    else return 'mm/dd/yyyy';
  }

  static get dateFnsLanguageDateFormats(): string {
    const lang: string = SoeConfigUtil.language;
    if (lang.startsWith('sv-')) return 'yyyy-MM-dd';
    else if (lang.startsWith('fi-')) return 'dd.MM.yyyy';
    else return 'MM/dd/yyyy';
  }

  static get languageDateFormatRegExp(): RegExp {
    const lang: string = SoeConfigUtil.language;
    if (lang.startsWith('sv-')) return /^\d{4}-\d{2}-\d{2}$/;
    else if (lang.startsWith('fi-')) return /^\d{2}.\d{2}.\d{4}$/;
    else return /^\d{2}\/\d{2}\/\d{4}$/;
  }

  static getDateFormatForView(view: 'day' | 'week' | 'month' | 'year') {
    const formats = {
      day: this.dateFnsLanguageDateFormats,
      week: ViewFormat.WeekYear,
      month: ViewFormat.MonthYear,
      year: ViewFormat.Year,
    };
    return formats[`${view}`];
  }

  static getDayOfWeekName(
    day: DayOfWeek,
    camelCase = false,
    width: LocaleWidth = 'wide'
  ): string {
    // LocaleWidth formats:
    // 'narrow' = [M, T, O, T, F, L, S]
    // 'short' = [må, ti, on, to, fr, lö, sö]
    // 'abbreviated' = [mån, tis, ons, tors, fre, lör, sön]
    // 'wide' = [måndag, tisdag, onsdag, torsdag, fredag, lördag, söndag]

    const locale = this.getLocale();
    const localized = locale.localize.day(day, { width: width });
    return camelCase ? StringUtil.camelCaseWord(localized) : localized;
  }

  static getDayOfWeekNames(
    camelCase = false,
    width: LocaleWidth = 'wide',
    forceStartOnMonday = false
  ): SmallGenericType[] {
    const locale = this.getLocale();
    const startDayOfWeek = forceStartOnMonday
      ? DayOfWeek.Monday
      : locale.options?.weekStartsOn ?? 1;

    const names: SmallGenericType[] = [];
    for (let i = 0; i < 7; i++) {
      let day = i + startDayOfWeek;
      if (day > 6) day -= 7;
      names.push(
        new SmallGenericType(day, this.getDayOfWeekName(day, camelCase, width))
      );
    }
    return names;
  }

  static getMonthName(
    month: Month | number,
    camelCase = false,
    width: LocaleWidth = 'wide'
  ): string {
    // LocaleWidth formats:
    // 'narrow' = [J, F, M, A, M, J, J, A, S, O, N, D]
    // 'abbreviated' = [jan, feb, mars, apr, maj, juni, juli, aug, sep, okt, nov, dec]
    // 'wide' = [januari, februari, mars, april, maj, juni, juli, augusti, september, oktober, november, december]

    const locale = this.getLocale();
    const localized = locale.localize.month(<Month>month, { width: width });
    return camelCase ? StringUtil.camelCaseWord(localized) : localized;
  }

  static getMonthNames(
    camelCase = false,
    width: LocaleWidth = 'wide'
  ): SmallGenericType[] {
    const names: SmallGenericType[] = [];
    for (let i = 0; i < 12; i++) {
      const month = i;
      // In abbreviated form, a dot is added to the end of the month names that are not in full length
      // We don't want that, so remove the dot
      // Month names are 0-indexed
      names.push(
        new SmallGenericType(
          month + 1,
          this.getMonthName(<Month>month, camelCase, width).replace('.', '')
        )
      );
    }
    return names;
  }

  // RANGE

  static diffDays(laterDate: Date, earlierDate: Date): number {
    return df_differenceInDays(laterDate, earlierDate);
  }

  static diffMonths(laterDate: Date, earlierDate: Date): number {
    return df_differenceInMonths(laterDate, earlierDate);
  }

  static diffYears(laterDate: Date, earlierDate: Date): number {
    return df_differenceInYears(laterDate, earlierDate);
  }

  static createRange(
    rangeStart: Date,
    rangeStop: Date,
    clearTime = false
  ): { start: Date; stop: Date } {
    const range = { start: new Date(rangeStart), stop: new Date(rangeStop) };
    if (clearTime) {
      range.start.clearHours();
      range.stop.clearHours();
    }
    return range;
  }

  static getIntersectingMinutes(
    range1Start: Date,
    range1Stop: Date,
    range2Start: Date,
    range2Stop: Date,
    clearTime = false
  ): number {
    const range1 = this.createRange(range1Start, range1Stop, clearTime);
    const range2 = this.createRange(range2Start, range2Stop, clearTime);

    const intersectStart =
      range1.start > range2.start ? range1.start : range2.start;
    const intersectStop = range1.stop < range2.stop ? range1.stop : range2.stop;

    if (intersectStart >= intersectStop) {
      return 0; // No intersection
    }

    const diffMs = intersectStop.getTime() - intersectStart.getTime();
    const diffMinutes = Math.floor(diffMs / 60000); // Convert milliseconds to minutes

    return diffMinutes;
  }

  static isRangesOverlapping(
    range1Start: Date,
    range1Stop: Date,
    range2Start: Date,
    range2Stop: Date,
    clearTime = false
  ): boolean {
    const range1 = this.createRange(range1Start, range1Stop, clearTime);
    const range2 = this.createRange(range2Start, range2Stop, clearTime);

    return (
      this.getIntersectingMinutes(
        range1Start,
        range1Stop,
        range2Start,
        range2Stop,
        clearTime
      ) > 0
    );
  }

  static dateEquals(date1?: Date, date2?: Date): boolean {
    if (!date1 && !date2) return true;
    if (!date1 || !date2) return false;

    return date1.getTime() === date2.getTime();
  }

  // SORT

  static sort(dates: Date[], desc = false): void {
    dates.sort((a: Date, b: Date) => {
      if (desc) return b.getTime() - a.getTime();
      else return a.getTime() - b.getTime();
    });
  }

  static sortImmutable(dates: Date[], desc = false): Date[] {
    return [...dates].sort((a: Date, b: Date) =>
      desc ? b.getTime() - a.getTime() : a.getTime() - b.getTime()
    );
  }

  static sortByKey(objects: any[], key: any, desc = false): void {
    objects.sort((a: any, b: any) => {
      if (desc) return new Date(b[key]).getTime() - new Date(a[key]).getTime();
      else return new Date(a[key]).getTime() - new Date(b[key]).getTime();
    });
  }

  static sortByKeyImmutable(objects: any[], key: any, desc = false): Date[] {
    return [...objects].sort((a: any, b: any) =>
      desc
        ? new Date(b[key]).getTime() - new Date(a[key]).getTime()
        : new Date(a[key]).getTime() - new Date(b[key]).getTime()
    );
  }
}
