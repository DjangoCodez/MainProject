import { NumberUtility } from "./NumberUtility";
import { DayOfWeek } from "./Enumerations";
import { SmallGenericType } from "../Common/Models/SmallGenericType";
import { DateHelperService } from "../Core/Services/DateHelperService";
import { Constants } from './Constants';
import { TermGroup_Sex, TermGroup_ContractGroupPeriod, TermGroup_MatrixDateFormatOption } from "../Util/CommonEnumerations";

export class CalendarUtility {

    static DefaultDateTime(): Date {
        // TODO: Hard coded date format
        return moment("1900-01-01", "YYYY-MM-DD").toDate();
    }

    static nullToDefaultDate(date: Date): Date {
        return date ? date : Constants.DATETIME_DEFAULT;
    }

    static defaultDateToNull(date: Date): Date {
        if (date.year() <= 1900)
            return null;
        else
            return date;
    }

    static isValidDate(date: any): boolean {
        //let d = new Date(date);
        //return d instanceof Date && !isNaN(d.getTime());

        return moment(date).isValid();
    }

    static isEmptyDate(date: any): boolean {
        if (!date)
            return true;

        return this.convertToDate(date).isSameYearAs(Constants.DATETIME_EMPTY);
    }

    static isTimeSpan(date: any): boolean {
        if (date && date.toString().length <= 8 && date.toString().contains(':')) {
            let parts: number = date.toString().split(':').length;
            if (parts === 2 || parts === 3)
                return true;
        }

        return false;
    }

    static isTimeZero(date: any): boolean {
        if (!date)
            return true;

        if (this.isTimeSpan(date))
            return (this.timeSpanToMinutes(this.parseTimeSpan(date)) === 0);

        if (!this.isValidDate(date))
            return false;

        var d = moment(date);
        var isZero = (d.hours() === 0 && d.minutes() === 0 && d.seconds() === 0);

        return isZero;
    }

    static clearEmptyDate(date: any): boolean {
        if (this.isEmptyDate(date)) {
            date = null;
            return true;
        }
        return false;
    }

    static toFormattedYearMonth(date: any): string {
        return date ? moment(date).format('YYYY-MM') : null;
    }

    static toFormattedDate(date: any): string {
        return date ? moment(date).format('YYYY-MM-DD') : null;
    }

    static toFormattedDateAndTime(date: any): string {
        return date ? moment(date).format('YYYY-MM-DD HH:mm') : null;
    }

    static toFormattedTime(date: any, showSeconds: boolean = false): string {
        return date ? showSeconds ? moment(date).format('HH:mm:ss') : moment(date).format('HH:mm') : null;
    }

    static getDayName(day: number): string {
        return day !== undefined && day !== null ? moment.weekdays(day) : '';
    }

    static getShortDayName(day: number): string {
        return day !== undefined && day !== null ? moment.weekdaysShort(day) : '';
    }

    static getMonthName(month: number): string {
        return month !== undefined && month !== null ? moment.months(month) : '';
    }

    static convertToDate(date: any, format?: string, clearIfEmpty: boolean = false): Date {
        if (date && this.isValidDate(date)) {
            let convDate = moment(date, format).toDate();
            if (clearIfEmpty && this.isEmptyDate(convDate))
                return null;

            return convDate;
        }

        return null;
    }

    static convertToDates(dates: any[]): Date[] {
        var ret: Date[] = [];
        _.forEach(dates, date => {
            if (date && this.isValidDate(date))
                ret.push(moment(date).toDate());
        });

        return ret;
    }

    static getDateNow(): Date {
        const date = new Date();
        return new Date(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours(), date.getMinutes(), 0);
    }

    static getCurrentYear(): number {
        return CalendarUtility.getDateToday().year();
    }

    static getCurrentMonth(): number {
        return CalendarUtility.getDateToday().getMonth();
    }

    static getCurrentWeekNr(): number {
        return moment().week();
    }

    static getWeeksInYear(year: number): number {
        let date = new Date(year, 0, 1);
        return moment(date).weeksInYear();
    }

    static getWeeksInCurrentYear(): number {
        return moment().weeksInYear();
    }

    static getDateToday(): Date {
        //var date = new Date(); //
        return new Date().date();
        /*
            var fixedDate = new Date(date.addMinutes(date.localTimeZoneOffsetFromDefault()));
            console.log("fixedDate", fixedDate, date.localTimeZoneOffsetFromDefault());
            console.log("date", date.date());
            return new Date(fixedDate.getFullYear(), fixedDate.getMonth(), fixedDate.getDate(), 0, 0, 0);
        */
        //return new Date(date.addMinutes(date.localTimeZoneOffsetFromDefault())); - REMOVED DUE TO DATES GETTING TIME - ITEM 31224
        //new Date(Date.now()).toDateString();
        //return new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0);
    }

    static getQuarter(date: Date) {
        var month = date.getMonth();
        if (month < 3)
            return 1;
        else if (month >= 3 && month < 6)
            return 2;
        else if (month >= 6 && month < 9)
            return 3;
        else
            return 4;
        //return Math.floor(((date.getMonth() + 1) + 3) / 4);
    }

    static getFirstDayOfYear(date?: Date): Date {
        if (!date)
            date = new Date();
        return new Date(date.getFullYear(), 0, 1);
    }

    static getFirstDayOfMonth(date: Date): Date {
        return new Date(date.getFullYear(), date.getMonth(), 1);
    }

    static getLastDayOfYear(date?: Date): Date {
        if (!date)
            date = new Date();
        return new Date(date.getFullYear(), 11, 31);
    }

    static getDateFormatForMatrix(dateFromat: TermGroup_MatrixDateFormatOption): string {
        switch (dateFromat) {
            case TermGroup_MatrixDateFormatOption.DateShort:
                return 'YY-MM-DD';
            case TermGroup_MatrixDateFormatOption.DateLong:
                return 'YYYY-MM-DD';
            case TermGroup_MatrixDateFormatOption.DayOfMonth:
                return 'D';
            case TermGroup_MatrixDateFormatOption.DayOfMonthPadded:
                return 'DD';
            case TermGroup_MatrixDateFormatOption.DayOfYear:
                return 'DDD';
            case TermGroup_MatrixDateFormatOption.DayOfWeekShort:
                return 'ddd';
            case TermGroup_MatrixDateFormatOption.DayOfWeekFull:
                return 'dddd';
            case TermGroup_MatrixDateFormatOption.Month:
                return 'M';
            case TermGroup_MatrixDateFormatOption.MonthPadded:
                return 'MM';
            case TermGroup_MatrixDateFormatOption.NameOfMonthShort:
                return 'MMM';
            case TermGroup_MatrixDateFormatOption.NameOfMonthFull:
                return 'MMMM';
            case TermGroup_MatrixDateFormatOption.Quarter:
                return 'Q';
            case TermGroup_MatrixDateFormatOption.YearShort:
                return 'YY';
            case TermGroup_MatrixDateFormatOption.YearFull:
                return 'YYYY';
            case TermGroup_MatrixDateFormatOption.WeekOfYear:
                return 'W';
            case TermGroup_MatrixDateFormatOption.YearMonth:
                return 'YYYY-MM';
            case TermGroup_MatrixDateFormatOption.YearWeek:
                return 'YYYY-W';
        }

        return '';
    }

    static isRangesOverlapping(
        range1Start: Date, range1Stop: Date,
        range2Start: Date, range2Stop: Date
    ): boolean {
        // Two ranges overlap if the start of one is before the end of the other and vice versa
        return range1Start < range2Stop && range2Start < range1Stop;
    }

    static getIntersectingRange(
        range1Start: Date, range1Stop: Date,
        range2Start: Date, range2Stop: Date
    ): { start: Date, stop: Date } | null {
        const start = range1Start > range2Start ? range1Start : range2Start;
        const stop = range1Stop < range2Stop ? range1Stop : range2Stop;
        // If the intersection is valid (start < stop), return it
        return start < stop ? { start, stop } : null;
    }

    static getIntersectingDuration(
        range1Start: Date, range1Stop: Date,
        range2Start: Date, range2Stop: Date
    ): number {
        const intersection = this.getIntersectingRange(range1Start, range1Stop, range2Start, range2Stop);
        if (!intersection) return 0;
        return Math.floor((intersection.stop.getTime() - intersection.start.getTime()) / 60000);
    }

    static getWeekNr(dayNumber: number): number {
        if (dayNumber < 1)
            dayNumber = 1;

        return Math.floor((dayNumber - 1) / 7) + 1;
    }

    static getMonthNr(date: Date): number {
        return moment(date).month() + 1;
    }

    static parseDate(str: string, dateHelperService: DateHelperService): Date {
        var dateFormat = dateHelperService.getShortDateFormat();
        var separator = dateHelperService.getDateSeparator();
        var isFinnish: boolean = (dateFormat && dateFormat.left(1) === "d");
        var isEnglish: boolean = (dateFormat && dateFormat.left(1) === "M");

        // Default date
        var date = new Date();
        var year: number = date.getFullYear();
        var month: number = date.getMonth() + 1;
        var day: number = date.getDate();
        var success: boolean = false;

        var len: number = str ? str.length : 0;
        var dateParts: string[] = str ? str.split(separator) : [];

        // Parse entered date string
        if (dateParts.length > 1) {
            // User has typed at least one separator
            if (dateParts.length === 2) {
                // User has only typed one separator, use preceeding and trailing digits as month and day in current year
                if (isFinnish) {
                    day = NumberUtility.tryParseInt(dateParts[0], day);
                    month = NumberUtility.tryParseInt(dateParts[1], month);
                } else {
                    month = NumberUtility.tryParseInt(dateParts[0], month);
                    day = NumberUtility.tryParseInt(dateParts[1], day);
                }
                success = true;
            } else if (dateParts.length === 3) {
                // User has typed two separators
                if (isFinnish) {
                    day = NumberUtility.tryParseInt(dateParts[0], day);
                    month = NumberUtility.tryParseInt(dateParts[1], month);
                    year = NumberUtility.tryParseInt(dateParts[2], year);
                }
                else if (isEnglish) {
                    year = NumberUtility.tryParseInt(dateParts[2], year);
                    month = NumberUtility.tryParseInt(dateParts[0], month);
                    day = NumberUtility.tryParseInt(dateParts[1], day);
                } else {
                    year = NumberUtility.tryParseInt(dateParts[0], year);
                    month = NumberUtility.tryParseInt(dateParts[1], month);
                    day = NumberUtility.tryParseInt(dateParts[2], day);
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
                    day = NumberUtility.tryParseInt(str, day);
                    success = true;
                    break;
                case 3:
                case 4:
                    // Month and day is entered
                    if (isFinnish) {
                        // Finnish vesion other way around (short date starts with day)
                        day = NumberUtility.tryParseInt(str.left(2), day);
                        month = NumberUtility.tryParseInt(str.substr(2), month);
                        success = true;
                    } else {
                        month = NumberUtility.tryParseInt(str.left(len - 2), month);
                        day = NumberUtility.tryParseInt(str.right(2), day);
                        success = true;
                    }
                    break;
                case 5:
                case 6:
                    // Year (2 digits), month and day is entered
                    if (isFinnish) {
                        // Finnish vesion other way around (short date starts with day)
                        day = NumberUtility.tryParseInt(str.left(2), day);
                        month = NumberUtility.tryParseInt(str.substr(2, 2), month);
                        year = NumberUtility.tryParseInt(str.substr(4), year);
                        success = true;
                    } else {
                        year = NumberUtility.tryParseInt(str.left(2), year);
                        month = NumberUtility.tryParseInt(str.substr(2, 2), month);
                        day = NumberUtility.tryParseInt(str.substr(4), day);
                        success = true;
                    }
                    break;
                case 7:
                case 8:
                    // Year (4 digits), month and day is entered
                    if (isFinnish) {
                        // Finnish vesion other way around (short date starts with day)
                        day = NumberUtility.tryParseInt(str.left(2), day);
                        month = NumberUtility.tryParseInt(str.substr(2, 2), month);
                        year = NumberUtility.tryParseInt(str.substr(4), year);
                        success = true;
                    } else {
                        year = NumberUtility.tryParseInt(str.left(4), year);
                        month = NumberUtility.tryParseInt(str.substr(4, 2), month);
                        day = NumberUtility.tryParseInt(str.substr(6), day);
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

    static parseTimeSpan(str: string, useSeconds: boolean = false, padHours: boolean = false, allowNegative: boolean = true, allowMoreThan99Hours: boolean = false): string {
        var isNegative: boolean = false;
        if (str) {
            // Check if negative
            if (str.startsWithCaseInsensitive('-')) {
                isNegative = true;
                str = str.substr(1);
            }

            // Check for decimals
            if (str.contains(',') || str.contains('.')) {
                let parts: string[] = [];
                if (str.contains(','))
                    parts = str.split(',');
                else if (str.contains('.'))
                    parts = str.split('.');

                if (parts.length > 0) {
                    var minutePart = parts[1];
                    if (minutePart.length > 1) {
                        str = "{0}:{1}".format(parts[0], parts[1].substring(0, 2));
                        if (parts.length > 2 && useSeconds)
                            str += ":{0}".format(parts[2].substring(0, 2));
                    } else {
                        var num = NumberUtility.parseDecimal("0." + minutePart);
                        str = "{0}:{1}".format(parts[0], (60 * num).toString().substring(0, 2));
                        if (parts.length > 2 && useSeconds)
                            str += ":{0}".format(parts[2]);
                    }
                }
            }

            if (!str.contains(':') && !allowMoreThan99Hours) {
                // No separator entered, check length to see if more than hour is specified
                if (str.length > 4 && useSeconds)
                    str = "{0}:{1}:{2}".format(str.left(str.length - 4), str.substr(str.length - 4, 2), str.right(2));
                else if (str.length > 2)
                    str = "{0}:{1}".format(str.left(str.length - 2), str.right(2));
            }
        }

        var hours: number = 0;
        var minutes: number = 0;
        var seconds: number = 0;

        if (str) {
            let parts: string[] = str.split(':');
            if (parts.length > 0)
                hours = Number(parts[0]);
            if (parts.length > 1)
                minutes = Number(parts[1]);
            if (parts.length > 2)
                seconds = Number(parts[2]);
        }

        var span: string = "{0}:{1}".format(padHours ? _.padStart(hours.toString(), 2, '0') : hours.toString(), _.padStart(minutes.toString(), 2, '0'));
        if (useSeconds)
            span += ":{0}".format(_.padStart(seconds.toString(), 2, '0'));

        if (isNegative && allowNegative)
            span = "-{0}".format(span);
        return span;
    }

    static sumTimeSpan(acc: string, next: string, showDays: boolean = false) {
        if (!acc || acc === "0")
            acc = "0:00";

        if (next) {
            let minutes: number = 0;

            if (acc.contains('.')) {
                let parts = acc.split('.');
                if (parts.length === 2) {
                    let days = parseInt(parts[0], 10);
                    minutes = (days * 24 * 60) + CalendarUtility.timeSpanToMinutes(parts[1]);
                }
            } else {
                minutes = CalendarUtility.timeSpanToMinutes(acc);
            }

            if (next.contains('.')) {
                let parts = next.split('.');
                if (parts.length === 2) {
                    let days = parseInt(parts[0], 10);
                    minutes += (days * 24 * 60) + CalendarUtility.timeSpanToMinutes(parts[1]);
                }
            } else {
                minutes += CalendarUtility.timeSpanToMinutes(next);
            }

            if (minutes === 0)
                return '';

            acc = CalendarUtility.minutesToTimeSpan(minutes, showDays, false, false);
        }

        return acc;
    }

    static timeSpanToMinutes(span: string): number {
        if (!span)
            return 0;

        // Check if negative
        var isNegative: boolean = false;
        if (span.startsWithCaseInsensitive('-')) {
            isNegative = true;
            span = span.substr(1);
        }

        span = this.parseTimeSpan(span);

        var hours: number = 0;
        var minutes: number = 0;

        var parts: string[] = span.split(':');
        hours = Number(parts[0]);
        minutes = Number(parts[1]);

        var result = hours * 60 + minutes;
        if (isNegative)
            result = -result;

        return result;
    }

    static minutesToTimeSpan(value: number, showDays = false, showSeconds = false, padHours = false, maxOneDay = false, addPlusIfPositive = false): string {
        if (!value)
            value = 0;

        let formatted = '';

        let isNegative: boolean = value < 0;

        if (isNegative)
            value = value * -1;

        let duration = moment.duration(value, 'minutes');

        let days = 0;
        let hours = 0;
        let minutes = duration.minutes();
        let seconds = duration.seconds();

        if (showDays) {
            days = duration.days();
            hours = duration.hours();
        } else {
            hours = Math.floor(duration.asHours());
            if (maxOneDay && hours > 23) {
                while (hours > 23) {
                    hours -= 24;
                }
            }
        }

        // Add days
        if (days != 0)
            formatted += days + ".";

        // Add time
        formatted += "{0}:{1}".format(((padHours || showDays && days > 0) ? _.padStart(hours.toString(), 2, '0') : hours.toString()), _.padStart(minutes.toString(), 2, '0'));
        if (showSeconds)
            formatted += ":{0}".format(_.padStart(seconds.toString(), 2, '0'));

        if (isNegative)
            formatted = "-" + formatted;
        else if (addPlusIfPositive && value > 0)
            formatted = "+" + formatted;

        return formatted;
    }

    static minutesToDecimal(value: number): string {
        if (!value)
            value = 0;

        return (value / 60).round(2).toLocaleString();
    }

    static minOfDates(a: Date, b: Date): Date {
        return (a < b) ? a : b;
    }

    static maxOfDates(a: Date, b: Date): Date {
        return (a > b) ? a : b;
    }

    static getMinDate(date1: Date, date2: Date): Date {
        return date1 < date2 ? date1 : date2;
        //return moment.min(moment(date1), moment(date2)).toDate();
    }

    static getMaxDate(date1: Date, date2: Date): Date {
        return date1 > date2 ? date1 : date2;
        //return moment.max(moment(date1), moment(date2)).toDate();
    }

    static getDates(startDate: Date, stopDate: Date): Date[] {
        var dateArray: Date[] = [];
        var currentDate: Date = startDate;
        while (currentDate.isSameOrBeforeOnDay(stopDate)) {
            dateArray.push(this.convertToDate(currentDate))
            currentDate = currentDate.addDays(1);
        }

        return dateArray;
    }

    static getDayOfWeekNames(upperCaseFirstLetter: boolean = false): SmallGenericType[] {
        var dayOfWeeks: SmallGenericType[] = [];
        for (var i = 1; i < 7; i++) {
            var name = CalendarUtility.getDayName(i).toLocaleLowerCase();
            if (upperCaseFirstLetter)
                name = name.toUpperCaseFirstLetter()
            dayOfWeeks.push({ id: i, name: name });
        }
        var sunday = CalendarUtility.getDayName(DayOfWeek.Sunday).toLocaleLowerCase();
        dayOfWeeks.push({ id: DayOfWeek.Sunday, name: sunday.charAt(0).toUpperCase() + sunday.slice(1) });
        return dayOfWeeks;
    }

    static includesDate(dates: Date[], date: Date, checkByTime: boolean = false): boolean {
        var exists: boolean = false;
        if (dates && dates.length > 0) {
            for (let i = 0, j = dates.length; i < j; i++) {
                if (dates[i] && ((checkByTime && dates[i].isSameMinuteAs(date)) || (!checkByTime && dates[i].isSameDayAs(date)))) {
                    exists = true;
                    break;
                }
            }
        }

        return exists;
    }

    static uniqueDates(dates: Date[]): Date[] {
        let uniqueDates = [];
        for (let i = 0; i < dates.length; i++) {
            if (!this.includesDate(uniqueDates, dates[i])) {
                uniqueDates.push(dates[i]);
            }
        }

        return uniqueDates;
    }

    static isCoherent(datesInput: Date[]): boolean {
        let dates = this.convertToDates(datesInput);
        let dateFrom: Date = _.min(dates);
        let dateTo: Date = _.max(dates);

        while (dateFrom <= dateTo) {
            if (!this.includesDate(dates, dateFrom))
                return false;

            dateFrom = dateFrom.addDays(1);
        }

        return true;
    }

    static getSexFromSocialSecNr(socialSecNr: string): TermGroup_Sex {
        // Last but one digit is odd for men and even for women
        var gender: TermGroup_Sex = TermGroup_Sex.Unknown;
        var genderNumber: number = -1;

        if (socialSecNr && socialSecNr.length > 1) {
            var charArray: string[] = socialSecNr.split('');
            genderNumber = Number(charArray[charArray.length - 2]);
            gender = (genderNumber % 2 == 0 ? TermGroup_Sex.Female : TermGroup_Sex.Male);
        }

        return gender;
    }

    static getBirthDateFromSecurityNumber(socialSecNr: string): Date {
        var date = null;

        // Supported formats:
        // YYYYMMDD-NNNN
        // YYYYMMDDNNNN
        // YYMMDD-NNNN
        // YYMMDDNNNN
        // YYYYMMDD
        // YYMMDD

        // Remove all but digits
        socialSecNr = socialSecNr.replace(/[^0-9]/, '');

        // Possible formats left:
        // YYYYMMDDNNNN
        // YYMMDDNNNN
        // YYYYMMDD
        // YYMMDD

        if (socialSecNr.length < 6 || socialSecNr.length > 12)
            return null;

        // Remove four last digits
        if (socialSecNr.length === 10 || socialSecNr.length === 12)
            socialSecNr = socialSecNr.substr(0, socialSecNr.length - 4);

        // Possible formats left:
        // YYYYMMDD
        // YYMMDD

        if (socialSecNr.length !== 8 && socialSecNr.length !== 6)
            return null;

        var day: number = Number(socialSecNr.right(2));
        if (day > 60)   // Samordningsnummer
            day -= 60;
        socialSecNr = socialSecNr.left(socialSecNr.length - 2);

        // Possible formats left:
        // YYYYMM
        // YYMM

        var month: number = Number(socialSecNr.right(2));
        socialSecNr = socialSecNr.left(socialSecNr.length - 2);

        // Possible formats left:
        // YYYY
        // YY

        var year: number = 0;

        if (socialSecNr.length === 4)
            year = Number(socialSecNr);
        else {
            // Use Windows default two-digit year intepretation
            // 00-29 => 2000-2029
            // 30-99 => 1930-1999
            year = Number(socialSecNr.left(2));
            year += year < 30 ? 2000 : 1900;
        }

        try {
            date = new Date(year, month, day);
        }
        catch (ex) {
            date = null;
        }

        return date;
    }

    static isValidSocialSecurityNumber(source: string, checkValidDate: boolean, mustSpecifyCentury: boolean, mustSpecifyDash: boolean, sex: TermGroup_Sex = TermGroup_Sex.Unknown): boolean {
        if (!source)
            return false;
        // Check length
        var length: number = 10;
        if (mustSpecifyDash)
            length++;
        else {
            source = source.trim().replace('-', '');
            source = source.trim().replace('+', '');
        }

        if (mustSpecifyCentury)
            length += 2;
        else if (source.length === 12 && (source.startsWithCaseInsensitive("19") || source.startsWithCaseInsensitive("20")))
            source = source.substr(2);

        if (!source || source.length != length)
            return false;

        // Remove dash
        source = source.trim().replace('-', '');
        source = source.trim().replace('+', '');

        if (checkValidDate) {
            // First six or eight chars must be a valid date
            try {
                var year: number = Number(source.substr(0, mustSpecifyCentury ? 4 : 2));
                // Year 2000 is special case since datetime must be greater then 0
                if (year == 0)
                    year = 2000;

                var month: number = Number(source.substr(mustSpecifyCentury ? 4 : 2, 2));
                var day: number = Number(source.substr(mustSpecifyCentury ? 6 : 4, 2));
                // Samordningsnummer
                if (day > 60)
                    day -= 60;
            }
            catch (ex) {
                return false;
            }
        }

        // Validate sex if not unknown
        if (sex != TermGroup_Sex.Unknown && sex !== this.getSexFromSocialSecNr(source))
            return false;

        // Remove century before control digit check
        if (mustSpecifyCentury)
            source = source.substr(2);

        // Check control digit
        try {
            var chars: string[] = source.split('');
            var sum: number = 0;
            for (var i = source.length - 1; i >= 0; i--) {
                var val: number = Number(chars[i]);

                val = val * (i % 2 - 2) * -1;
                if (val > 9)
                    val -= 9;
                sum += val;
            }
        }
        catch (ex) {
            return false;
        }

        return sum % 10 == 0;
    }

    static isDateLesserThan(date1?: Date, date2?: Date): boolean {
        //Case 1: Changed from date to null
        if (date1 && !date2)
            return true;
        //Case 2: Changed from date to lesser date
        if (date1 && date2 && date1 < date2)
            return true;

        return false;
    }

    static isDateGreaterThan(date1?: Date, date2?: Date): boolean {
        //Case 1: Changed from date to null
        if (!date1 && date2)
            return true;
        //Case 2: Changed from date to greater date
        if (date1 && date2 && date1 > date2)
            return true;

        return false;
    }

    static calculateNextPeriod(period: TermGroup_ContractGroupPeriod, interval: number, currentYear: number, currentValue: number): any {
        var today = this.getDateToday();

        if (currentYear == 0)
            currentYear = today.getFullYear();

        switch (period) {
            case TermGroup_ContractGroupPeriod.Week:
                if (currentValue == 0)
                    currentValue = today.week();

                var nbrOfWeeks: number = 0;
                for (var i = 1; i <= interval; i++) {
                    // Check number of weeks in current year
                    nbrOfWeeks = new Date(currentYear, 12, 31).week();
                    currentValue++;
                    if (currentValue > nbrOfWeeks) {
                        // If week number passed number of avaliable weeks, move to next year
                        currentYear++;
                        currentValue = 1;
                    }
                }
                break;
            case TermGroup_ContractGroupPeriod.Month:
                if (currentValue == 0)
                    currentValue = today.getMonth();
                currentValue += interval;
                while (currentValue > 12) {
                    // If month passed number of available months, move to next year
                    currentValue -= 12;
                    currentYear++;
                }
                break;
            case TermGroup_ContractGroupPeriod.Quarter:
                if (currentValue == 0)
                    currentValue = today.getMonth();
                currentValue = this.getQuarter(today);

                currentValue += interval;
                while (currentValue > 4) {
                    // If quarter passed number of available quarters, move to next year
                    currentValue -= 4;
                    currentYear++;
                }
                break;
            case TermGroup_ContractGroupPeriod.Year:
                if (currentValue == 0)
                    currentValue = 1;
                currentYear += interval;
                break;
            case TermGroup_ContractGroupPeriod.CalendarYear:
                // Move to next year
                currentYear++;
                currentValue = 1;
                break;
        }

        return { currentYear: currentYear, currentValue: currentValue };
    }

    static calculateCurrentPeriod(period: TermGroup_ContractGroupPeriod, date: Date): any {
        var currentYear: number = date.getFullYear();
        var currentValue: number = 0;
        switch (period) {
            case TermGroup_ContractGroupPeriod.Week:
                currentValue = date.week();
                break;
            case TermGroup_ContractGroupPeriod.Quarter:
                currentValue = this.getQuarter(date);
                break;
            default:
                currentValue = date.getMonth() + 1;
                break;
            /* OLD VALUES
            case TermGroup_ContractGroupPeriod.Week:
                currentValue = date.week();
                break;
            case TermGroup_ContractGroupPeriod.Month:
                currentValue = date.getMonth() + 1;
                break;
            case TermGroup_ContractGroupPeriod.Year:
            case TermGroup_ContractGroupPeriod.CalendarYear:
                currentValue = 1;
                break;*/
        }

        return { currentYear: currentYear, currentValue: currentValue };
    }

    public static convertContractPeriodToDate(period: TermGroup_ContractGroupPeriod, startDate: Date, year: number, value: number, dayInMonth: number): Date {
        if (year == 0)
            year = this.getDateToday().getFullYear();
        else if (startDate.getFullYear() > year)
            year = startDate.getFullYear();

        //Always set next invoice date from month
        value = startDate.getMonth();

        //Check startdate
        if (dayInMonth < startDate.getDate())
            value = value + 1;

        // Default is first day in specified month
        let date: Date = new Date(year, value, 1);
        if (dayInMonth > 0) {
            var lastDayInMonth: Date = date.endOfMonth();
            if (dayInMonth > lastDayInMonth.getDate())
                date = new Date(year, value, lastDayInMonth.getDate());
            else
                date = new Date(year, value, dayInMonth);
        }

        /*switch (period) {
            case TermGroup_ContractGroupPeriod.Week:
                var start = new Date(year, 0);
                start = start.addWeeks(value);
                date = moment(start).startOf('week').toDate();
                break;
            case TermGroup_ContractGroupPeriod.Month:
                // Month must be 1-12
                if (value < 1 || value > 12)
                    break;

                //Check startdate
                if (dayInMonth >= startDate.getDate())
                    value = value - 1;

                // Default is first day in specified month
                date = new Date(year, value, 1);
                if (dayInMonth > 0) {
                    var lastDayInMonth: Date = date.endOfMonth();
                    if (dayInMonth > lastDayInMonth.getDate())
                        date = new Date(year, value, lastDayInMonth.getDate());
                    else
                        date = new Date(year, value, dayInMonth);
                }
                break;
            case TermGroup_ContractGroupPeriod.Quarter:
                // Quarter must be 1-4
                if (value < 1 || value > 4)
                    break;
                // Always the first day in specified quarter
                date = new Date(year, (value * 3) - 2, 1);
                break;
            case TermGroup_ContractGroupPeriod.Year:
                // Same date as start date, just changing the year
                date = new Date(year, startDate.getMonth(), dayInMonth);
                break;
            case TermGroup_ContractGroupPeriod.CalendarYear:
                // Always 1:st of january, just changing the year
                date = new Date(year, 1, 1);
                break;
        }*/

        return date;
    }

    public static monthToDateRange(year: number, month: number): { beginning: Date, end: Date } {
        const init = new Date(year, month);

        return {
            beginning: init.beginningOfMonth(),
            end: init.endOfMonth()
        };
    }

    public static weekToDateRange(year: number, week: number): { beginning: Date, end: Date } {
        let init = moment().isoWeekYear(year).isoWeek(week).toDate();
        return {
            beginning: init.beginningOfISOWeek(),
            end: init.endOfISOWeek()
        };
    }

    public static getMonthsBetweenDates(d1: Date, d2: Date): number {
        let months = (d2.getFullYear() - d1.getFullYear()) * 12;
        months -= d1.getMonth();
        months += d2.getMonth();
        return months <= 0 ? 0 : (months + 1);
    }

    public static getDaysBetweenDates(d1: Date, d2: Date): number {
        const msInDay = 24 * 60 * 60 * 1000;
        return Math.round(
            Math.abs(d2.getTime() - d1.getTime()) / msInDay,
        );
    }
}
