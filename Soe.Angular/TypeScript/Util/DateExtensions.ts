import { MomentUnitsOfTime, DayOfWeek } from "./Enumerations";
import { Constants } from "./Constants";
import { DurationInputArg1 } from "moment";

/* Date prototype extensions */

declare global {
    interface Date {
        isToday: () => boolean;
        isBeforeOnDay: (date: Date) => boolean;
        isSameOrBeforeOnDay: (date: Date) => boolean;
        isSameOrAfterOnDay: (date: Date) => boolean;
        isBeforeOnMinute: (date: Date) => boolean;
        isSameOrBeforeOnMinute: (date: Date) => boolean;
        isSameYearAs: (date: Date) => boolean;
        isSameMonthAs: (date: Date) => boolean;
        isSameDayAs: (date: Date) => boolean;
        isSameHourAs: (date: Date) => boolean;
        isSameMinuteAs: (date: Date) => boolean;
        isAfterOnMinute: (date: Date) => boolean;
        isSameOrAfterOnMinute: (date: Date) => boolean;
        isAfterOnDay: (date: Date) => boolean;
        isDST: () => boolean;
        beginningOfDay: () => Date;
        endOfDay: () => Date;

        toFormattedDate: (format?: string) => string;
        toFormattedTime: (showSeconds?: boolean) => string;
        toFormattedDateTime: (showSeconds?: boolean) => string;
        toPipedDate: () => string;
        toPipedTime: (showSeconds?: boolean) => string;
        toPipedDateTime: (showSeconds?: boolean) => string;
        toDateTimeString: () => string;
        toISODateTimeString: () => string;

        date: () => Date;
        beginningOfHour: () => Date;
        endOfHour: () => Date;
        beginningOfWeek: () => Date;
        endOfWeek: () => Date;
        beginningOfISOWeek: () => Date;
        endOfISOWeek: () => Date;
        beginningOfMonth: () => Date;
        endOfMonth: () => Date;
        beginningOfYear: () => Date;
        endOfYear: () => Date;
        daysInMonth: () => number;
        weeksInYear: () => number;
        year: () => number;
        week: () => number;
        hour: () => number;
        minutes: () => number;
        dayOfWeek: () => DayOfWeek;
        minutesFromMidnight: () => number;
        timeValue: () => number;
        timeValueSec: () => number;
        timeValueDay: () => number;

        add: (amount: number, unitOfTime: MomentUnitsOfTime) => Date;
        addYears: (years: number) => Date;
        addQuarters: (quarters: number) => Date;
        addMonths: (months: number) => Date;
        addWeeks: (weeks: number) => Date;
        addDays: (days: number) => Date;
        addHours: (hours: number) => Date;
        addMinutes: (minutes: number) => Date;
        addSeconds: (seconds: number) => Date;
        addMilliseconds: (milliseconds: number) => Date;

        getNextDayOfWeek: (dayOfWeek: DayOfWeek) => Date;

        mergeTime: (time: any) => Date;
        mergeTimeSpan: (timeSpan: string) => Date;

        clearSeconds: () => Date;
        roundMinutes: (interval: number) => Date;

        isBeginningOfDay: () => boolean;
        isEndOfDay: () => boolean;
        isBeginningOfWeek: () => boolean;
        isEndOfWeek: () => boolean;
        isBeginningOfISOWeek: () => boolean;
        isEndOfISOWeek: () => boolean;
        isBeginningOfYear: () => boolean;
        isEndOfYear: () => boolean;

        isWithinRange: (rangeStart: Date, rangeStop: Date) => boolean;

        diff: (date: Date) => number;
        diffDays: (date: Date) => number;
        diffMinutes: (date: Date) => number;

        defaultTimeZoneOffsetFromUTC: () => number;
        localTimeZoneOffsetFromUTC: () => number;
        localTimeZoneOffsetFromDefault: () => number;

        format: (format: string) => string;
        sortOnMonday: () => number;
    }
}

function pad(number) {
    if (number < 10) {
        return '0' + number;
    }
    return number;
}

Date.prototype.isToday = function (): boolean {
    return this.toDateString() == new Date(Date.now()).toDateString();
}

Date.prototype.isSameYearAs = function (date: Date): boolean {
    if (!date)
        return false;

    return (
        this.getFullYear() === date.getFullYear()
    );
}

Date.prototype.isSameMonthAs = function (date: Date): boolean {
    if (!date)
        return false;

    return (
        this.getFullYear() === date.getFullYear() &&
        this.getMonth() === date.getMonth()
    );
}

Date.prototype.isSameDayAs = function (date: Date): boolean {
    if (!date)
        return false;

    return (
        this.getFullYear() === date.getFullYear() &&
        this.getMonth() === date.getMonth() &&
        this.getDate() === date.getDate()
    );
}


Date.prototype.isSameHourAs = function (date: Date): boolean {
    if (!date)
        return false;

    return (
        this.getFullYear() === date.getFullYear() &&
        this.getMonth() === date.getMonth() &&
        this.getDate() === date.getDate() &&
        this.getHours() === date.getHours()
    );
}

Date.prototype.isSameMinuteAs = function (date: Date): boolean {
    if (!date)
        return false;

    return (
        this.getFullYear() === date.getFullYear() &&
        this.getMonth() === date.getMonth() &&
        this.getDate() === date.getDate() &&
        this.getHours() === date.getHours() &&
        this.getMinutes() === date.getMinutes()
    );
}

Date.prototype.isBeforeOnDay = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() < date.getFullYear()) return true;
    if (this.getFullYear() > date.getFullYear()) return false;

    if (this.getMonth() < date.getMonth()) return true;
    if (this.getMonth() > date.getMonth()) return false;

    return this.getDate() < date.getDate();
}

Date.prototype.isSameOrBeforeOnDay = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() < date.getFullYear()) return true;
    if (this.getFullYear() > date.getFullYear()) return false;

    if (this.getMonth() < date.getMonth()) return true;
    if (this.getMonth() > date.getMonth()) return false;

    if (this.getDate() < date.getDate()) return true;
    if (this.getDate() > date.getDate()) return false;

    return true;
}

Date.prototype.isSameOrAfterOnDay = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() > date.getFullYear()) return true;
    if (this.getFullYear() < date.getFullYear()) return false;

    if (this.getMonth() > date.getMonth()) return true;
    if (this.getMonth() < date.getMonth()) return false;

    if (this.getDate() > date.getDate()) return true;
    if (this.getDate() < date.getDate()) return false;

    return true;
}

Date.prototype.isBeforeOnMinute = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() < date.getFullYear()) return true;
    if (this.getFullYear() > date.getFullYear()) return false;

    if (this.getMonth() < date.getMonth()) return true;
    if (this.getMonth() > date.getMonth()) return false;

    if (this.getDate() < date.getDate()) return true;
    if (this.getDate() > date.getDate()) return false;

    if (this.getHours() < date.getHours()) return true;
    if (this.getHours() > date.getHours()) return false;

    return this.getMinutes() < date.getMinutes();
}

Date.prototype.isSameOrBeforeOnMinute = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() < date.getFullYear()) return true;
    if (this.getFullYear() > date.getFullYear()) return false;

    if (this.getMonth() < date.getMonth()) return true;
    if (this.getMonth() > date.getMonth()) return false;

    if (this.getDate() < date.getDate()) return true;
    if (this.getDate() > date.getDate()) return false;

    if (this.getHours() < date.getHours()) return true;
    if (this.getHours() > date.getHours()) return false;

    if (this.getMinutes() < date.getMinutes()) return true;
    if (this.getMinutes() > date.getMinutes()) return false;

    return true;
}

Date.prototype.isAfterOnMinute = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() > date.getFullYear()) return true;
    if (this.getFullYear() < date.getFullYear()) return false;

    if (this.getMonth() > date.getMonth()) return true;
    if (this.getMonth() < date.getMonth()) return false;

    if (this.getDate() > date.getDate()) return true;
    if (this.getDate() < date.getDate()) return false;

    if (this.getHours() > date.getHours()) return true;
    if (this.getHours() < date.getHours()) return false;

    return this.getMinutes() > date.getMinutes();
}

Date.prototype.isSameOrAfterOnMinute = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() > date.getFullYear()) return true;
    if (this.getFullYear() < date.getFullYear()) return false;

    if (this.getMonth() > date.getMonth()) return true;
    if (this.getMonth() < date.getMonth()) return false;

    if (this.getDate() > date.getDate()) return true;
    if (this.getDate() < date.getDate()) return false;

    if (this.getHours() > date.getHours()) return true;
    if (this.getHours() < date.getHours()) return false;

    if (this.getMinutes() > date.getMinutes()) return true;
    if (this.getMinutes() < date.getMinutes()) return false;

    return true;
}

Date.prototype.isAfterOnDay = function (date: Date): boolean {
    if (!date)
        return false;

    if (this.getFullYear() > date.getFullYear()) return true;
    if (this.getFullYear() < date.getFullYear()) return false;

    if (this.getMonth() > date.getMonth()) return true;
    if (this.getMonth() < date.getMonth()) return false;

    return this.getDate() > date.getDate();
}

Date.prototype.isDST = function (): boolean {
    return moment(this).isDST();
}

Date.prototype.beginningOfDay = function (): Date {
    let date = new Date(this.getTime());
    date.setHours(0, 0, 0, 0);
    return date;
}


Date.prototype.endOfDay = function (): Date {
    let newDate = new Date(this.getTime());
    newDate.setHours(23, 59, 59, 999);
    return newDate;
}

/* MomentJS methods */

Date.prototype.toFormattedDate = function (format: string = ''): string {
    if (format) {
        return this ? moment(this).format(format) : null;
    } else
        return this.toLocaleDateString();
};

Date.prototype.toFormattedTime = function (showSeconds: boolean = false): string {
    let formatted = `${pad(this.getHours())}:${pad(this.getMinutes())}`;

    if (showSeconds)
        formatted += `:${pad(this.getSeconds())}`;

    return formatted;
};

Date.prototype.toFormattedDateTime = function (showSeconds: boolean = false): string {
    let formatted = `${this.getFullYear()}-${pad(this.getMonth() + 1)}-${pad(this.getDate())} ${pad(this.getHours())}:${pad(this.getMinutes())}`;

    if (showSeconds)
        formatted += `:${pad(this.getSeconds())}`;

    return formatted;
};

Date.prototype.toPipedDate = function (): string {
    return `'${this.getFullYear()}´|${this.getMonth() + 1}|${this.getDate()}'`;
}

Date.prototype.toPipedTime = function (showSeconds: boolean = false): string {
    let formatted = `'${this.getHours()}´|${this.getMinutes()}`;

    if (showSeconds)
        formatted += `|${this.getSeconds()}`;

    formatted += "'";

    return formatted;
}

Date.prototype.toPipedDateTime = function (showSeconds: boolean = false): string {
    let formatted = `'${this.getFullYear()}|${pad(this.getMonth() + 1)}|${pad(this.getDate())}|${pad(this.getHours())}|${pad(this.getMinutes())}`;

    if (showSeconds)
        formatted += `|${pad(this.getSeconds())}`;

    formatted += "'";

    return formatted;
}

Date.prototype.toDateTimeString = function (): string {
    return `${this.getFullYear()}${pad(this.getMonth() + 1)}${pad(this.getDate())}T${pad(this.getHours())}${pad(this.getMinutes())}${pad(this.getSeconds())}`;
};

Date.prototype.toISODateTimeString = function (): string {
    return `${this.getFullYear()}-${pad(this.getMonth() + 1)}-${pad(this.getDate())}T${pad(this.getHours())}:${pad(this.getMinutes())}:${pad(this.getSeconds())}.000Z`;
};

Date.prototype.date = function (): Date {
    // Return only the date part
    return moment(this).startOf('day').toDate();
}

Date.prototype.beginningOfHour = function (): Date {
    return moment(this).startOf('hour').toDate();
}

Date.prototype.endOfHour = function (): Date {
    return moment(this).endOf('hour').toDate();
}

Date.prototype.beginningOfWeek = function (): Date {
    return moment(this).startOf('week').toDate();
}

Date.prototype.endOfWeek = function (): Date {
    return moment(this).endOf('week').toDate();
}

Date.prototype.beginningOfISOWeek = function (): Date {
    return moment(this).startOf('isoWeek').toDate();
}

Date.prototype.endOfISOWeek = function (): Date {
    return moment(this).endOf('isoWeek').toDate();
}

Date.prototype.beginningOfMonth = function (): Date {
    return moment(this).startOf('month').toDate();
}

Date.prototype.endOfMonth = function (): Date {
    return moment(this).endOf('month').toDate();
}

Date.prototype.beginningOfYear = function (): Date {
    return moment(this).startOf('year').toDate();
}

Date.prototype.endOfYear = function (): Date {
    return moment(this).endOf('year').toDate();
}

Date.prototype.daysInMonth = function (): number {
    return moment(this).daysInMonth();
}

Date.prototype.weeksInYear = function (): number {
    return moment(this).weeksInYear();
}

Date.prototype.year = function (): number {
    return moment(this).year();
}

Date.prototype.week = function (): number {
    return moment(this).week();
}

Date.prototype.hour = function (): number {
    return moment(this).hour();
}

Date.prototype.minutes = function (): number {
    return moment(this).minutes();
}

Date.prototype.dayOfWeek = function (): DayOfWeek {
    return moment(this).day();
}

Date.prototype.minutesFromMidnight = function (): number {
    let minutes = this.getMinutes();
    let hours = this.getHours();

    return (60 * hours) + minutes;
}

Date.prototype.timeValue = function (): number {
    // Number of milliseconds since the Unix Epoch (1970-01-01 00:00:00)
    return moment(this).valueOf();
}

Date.prototype.timeValueSec = function (): number {
    // Number of seconds since the Unix Epoch (1970-01-01 00:00:00)
    return moment(this).unix();
}

Date.prototype.timeValueDay = function (): number {
    // Number of days since the Unix Epoch (1970-01-01 00:00:00)
    return Math.floor(moment(this).unix() / 86400);
}

Date.prototype.add = function (amount: number, unitOfTime: MomentUnitsOfTime): Date {
    var date = moment(this);
    // any casts are stoopid hack to get around typings issue
    return date.clone().add(<DurationInputArg1>amount, <any>MomentUnitsOfTime[unitOfTime]).toDate();
}

Date.prototype.addYears = function (years: number): Date {
    return this.add(years, MomentUnitsOfTime.years);
}

Date.prototype.addQuarters = function (quarters: number): Date {
    return this.add(quarters, MomentUnitsOfTime.quarters);
}

Date.prototype.addMonths = function (months: number): Date {
    return this.add(months, MomentUnitsOfTime.months);
}

Date.prototype.addWeeks = function (weeks: number): Date {
    return this.add(weeks, MomentUnitsOfTime.weeks);
}

Date.prototype.addDays = function (days: number): Date {
    return this.add(days, MomentUnitsOfTime.days);
}

Date.prototype.addHours = function (hours: number): Date {
    return this.add(hours, MomentUnitsOfTime.hours);
}

Date.prototype.addMinutes = function (minutes: number): Date {
    return this.add(minutes, MomentUnitsOfTime.minutes);
}

Date.prototype.addSeconds = function (seconds: number): Date {
    return this.add(seconds, MomentUnitsOfTime.seconds);
}

Date.prototype.addMilliseconds = function (milliseconds: number): Date {
    return this.add(milliseconds, MomentUnitsOfTime.milliseconds);
}

Date.prototype.getNextDayOfWeek = function (dayOfWeek: DayOfWeek): Date {
    let date: Date = this;
    while (date.dayOfWeek() !== dayOfWeek) {
        date = date.addDays(1);
    }

    return date;
}

Date.prototype.mergeTime = function (time: any): Date {
    let timeDate = moment(time);
    let newDate = moment(this);
    newDate.set('hour', timeDate.hours());
    newDate.set('minute', timeDate.minutes());
    newDate.set('second', timeDate.seconds());

    return newDate.toDate();
}

Date.prototype.mergeTimeSpan = function (timeSpan: string): Date {
    let hours = 0;
    let minutes = 0;
    let seconds = 0;

    if (timeSpan) {
        let parts: string[] = timeSpan.split(':');
        if (parts.length > 0)
            hours = Number(parts[0]);
        if (parts.length > 1)
            minutes = Number(parts[1]);
        if (parts.length > 2)
            seconds = Number(parts[2]);
    }

    let newDate = moment(this);
    newDate.set('hour', hours);
    newDate.set('minute', minutes);
    newDate.set('second', seconds);

    return newDate.toDate();
}

Date.prototype.clearSeconds = function (): Date {
    let newDate = moment(this);
    newDate.set('second', 0);
    newDate.set('millisecond', 0);

    return newDate.toDate();
}

Date.prototype.roundMinutes = function (interval: number): Date {
    if (interval === 0)
        return this;

    let time: Date = this;
    let newTime: Date = time.beginningOfDay();
    let prevTime: Date = time.beginningOfDay();
    while (newTime.isBeforeOnMinute(time)) {
        prevTime = newTime;
        newTime = newTime.addMinutes(interval);
    }

    if (Math.abs(time.diffMinutes(newTime)) < Math.abs(time.diffMinutes(prevTime)))
        return newTime;
    else
        return prevTime;
}

Date.prototype.isBeginningOfDay = function (): boolean {
    return this.isSameMinuteAs(this.beginningOfDay());
}

Date.prototype.isEndOfDay = function (): boolean {
    let newDate = new Date(this.getTime());
    newDate.setHours(23, 59);
    return this.isSameMinuteAs(newDate);
}

Date.prototype.isBeginningOfWeek = function (): boolean {
    return this.isSameDayAs(this.beginningOfWeek());
}

Date.prototype.isEndOfWeek = function (): boolean {
    return this.isSameDayAs(this.endOfWeek());
}

Date.prototype.isBeginningOfISOWeek = function (): boolean {
    return this.isSameDayAs(this.beginningOfISOWeek());
}

Date.prototype.isEndOfISOWeek = function (): boolean {
    return this.isSameDayAs(this.endOfISOWeek());
}

Date.prototype.isBeginningOfYear = function (): boolean {
    return this.isSameDayAs(this.beginningOfYear());
}

Date.prototype.isEndOfYear = function (): boolean {
    return this.isSameDayAs(this.endOfYear());
}

Date.prototype.isWithinRange = function (rangeStart: Date, rangeStop: Date): boolean {
    let range = moment.range(rangeStart, rangeStop);

    return moment(this).within(range);
}

Date.prototype.diff = function (date: Date): number {
    let a = moment(this);
    let b = moment(date);
    return a.diff(b);
}

Date.prototype.diffDays = function (date: Date): number {
    let a = moment(this);
    let b = moment(date);

    return a.diff(b, 'days');
}

Date.prototype.diffMinutes = function (date: Date): number {
    return Math.floor((this.getTime() - date.getTime()) / 60000);
}

Date.prototype.defaultTimeZoneOffsetFromUTC = function (): number {
    return moment.tz(this, Constants.DEFAULT_TIMEZONE).utcOffset();
}

Date.prototype.localTimeZoneOffsetFromUTC = function (): number {
    return moment.tz(this, moment.tz.guess().toString()).utcOffset();
}

Date.prototype.localTimeZoneOffsetFromDefault = function (): number {
    return this.localTimeZoneOffsetFromUTC() - this.defaultTimeZoneOffsetFromUTC();
}

Date.prototype.format = function (format: string): string {
    return moment(this).format(format);
}

Date.prototype.sortOnMonday = function (): number {
    let dayOfWeek = this.dayOfWeek();
    return (dayOfWeek === DayOfWeek.Sunday) ? 7 : dayOfWeek;
}