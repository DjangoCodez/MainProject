import { StringUtility } from "./StringUtility";

/* String prototype extensions */

declare global {
    interface String {
        right: (length: number) => string;
        left: (length: number) => string;
        startsWithCaseInsensitive: (searchString: string, position?: number) => boolean;
        endsWithCaseInsensitive: (searchString: string) => boolean;
        contains: (searchString: string, caseSensitive?: boolean) => boolean;
        format: (...args: string[]) => string;
        toUpperCaseFirstLetter: () => string;
        toNumber: (decimals: number) => string;
        toFormattedNumber: (decimals: number) => string;
        toOnlyNumbers: (keepDecimalPoint: boolean) => number;
        toEllipsisString: (maxLength: number) => string;
        padLeft: (targetLength: number, padString: string) => string;
        padRight: (targetLength: number, padString: string) => string;
        parsePipedDate: () => Date;
        parsePipedTime: (date: Date) => Date;
        parsePipedDateTime: () => Date;
    }
}


String.prototype.right = function (length: number): string {
    return this.substr(this.length - length);
}

String.prototype.left = function (length: number): string {
    return this.substr(0, length);
}

String.prototype.startsWithCaseInsensitive = function (searchString: string, position?: number): boolean {
    if (!searchString)
        return true;

    position = position || 0;
    return this.substr(position, searchString.length).toLowerCase() === searchString.toLowerCase();
};

String.prototype.endsWithCaseInsensitive = function (searchString: string): boolean {
    if (!searchString)
        return true;

    return this.toLowerCase().indexOf(searchString.toLowerCase(), this.length - searchString.length) !== -1;
};

String.prototype.contains = function (searchString: string, caseSensitive?: boolean): boolean {
    if (!searchString)
        return true;

    if (caseSensitive)
        return this.indexOf(searchString) > -1;
    else
        return this.toLowerCase().indexOf(searchString.toLowerCase()) > -1;
};

String.prototype.format = function (...args: string[]): string {
    var formatted = this;
    for (var i = 0; i < arguments.length; i++) {
        if (arguments[i])
            formatted = formatted.replace(RegExp("\\{" + i + "\\}", 'g'), arguments[i].toString());
    }

    // Replace remaing placeholders with empty string.
    // (In case original string contains more placeholders than arguments).
    formatted = formatted.replace(RegExp("\\{[0-9]+\\}", 'g'), '');

    return formatted;
};

String.prototype.toUpperCaseFirstLetter = function (): string {
    return this.left(1).toUpperCase() + this.substr(1).toLowerCase();
}

String.prototype.toNumber = function (decimals: number): string {
    var float: number = StringUtility.toFloat(this);

    return float.toFixed(decimals);
}

String.prototype.toFormattedNumber = function (decimals: number, maxDecimals?: number): string {
    var float: number = StringUtility.toFloat(this);

    return float.toLocaleString("sv-se", { minimumFractionDigits: decimals, maximumFractionDigits: maxDecimals ? maxDecimals : decimals });
}

String.prototype.toOnlyNumbers = function (keepDecimalPoint: boolean): number {
    var regEx: string = keepDecimalPoint ? '/[^0-9.,]/g' : '/[^0-9]/g';

    return parseFloat(this.replace(regEx, ''));
}

String.prototype.toEllipsisString = function (maxLength: number): string {
    return this.length > maxLength ? this.left(maxLength) + '...' : this;
}

String.prototype.padLeft = function (targetLength: number, padString: string): string {
    // TODO: ECMAScript 2017 has support for string padding
    // str.padStart(targetLength [, padString])
    // str.padEnd(targetLength[, padString])

    let s: string = this;
    while (s.length < targetLength)
        s = padString + s;

    return s;
}

String.prototype.padRight = function (targetLength: number, padString: string): string {
    // TODO: ECMAScript 2017 has support for string padding
    // str.padStart(targetLength [, padString])
    // str.padEnd(targetLength[, padString])

    let s: string = this;
    while (s.length < targetLength)
        s += padString;

    return s;

}

String.prototype.parsePipedDate = function (): Date {
    let s: string = this;

    // Remove trailing and ending apostrophs
    s = s.replace('\'', '');

    var parts = s.split('|');

    var year = parseInt(parts[0], 10);
    var month = parseInt(parts[1], 10);
    var day = parseInt(parts[2], 10);

    return new Date(year, month - 1, day);
}

String.prototype.parsePipedTime = function (date: Date): Date {
    let s: string = this;
    let newDate: Date = new Date(date);

    // Remove trailing and ending apostrophs
    s = s.replace('\'', '');

    var parts = s.split('|');

    var hour = parseInt(parts[0], 10);
    var minute = parseInt(parts[1], 10);
    var second = 0;
    if (parts.length > 2)
        second = parseInt(parts[2], 10);

    newDate.setHours(hour);
    newDate.setMinutes(minute);
    newDate.setSeconds(0);

    return newDate;
}

String.prototype.parsePipedDateTime = function (): Date {
    let s: string = this;

    // Remove trailing and ending apostrophs
    s = s.replace('\'', '');

    var parts = s.split('|');

    var year = parseInt(parts[0], 10);
    var month = parseInt(parts[1], 10);
    var day = parseInt(parts[2], 10);
    var hour = parseInt(parts[3], 10);
    var minute = parseInt(parts[4], 10);
    var second = 0;
    if (parts.length > 5)
        second = parseInt(parts[5], 10);

    return new Date(year, month - 1, day, hour, minute, second);
}


