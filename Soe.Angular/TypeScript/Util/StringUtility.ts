import { Constants } from "./Constants";

export class StringUtility {
    // Replace \n with <br/>
    static ToBr(text: string) {
        if (text) {
            // Difference between writing \n in code and storing it in the database!
            text = text.replace(/(?:\\r\\n|\\r|\\n)/gm, '<br />');
            text = text.replace(/(?:\r\n|\r|\n)/gm, '<br />');
        }

        return text;
    }

    static nullToEmpty(text: any) {
        if (text)
            return text;
        else
            return '';
    }

    static getCollectionIdsStr(collection: any): string {
        var str = Constants.WEBAPI_STRING_EMPTY;
        if (collection && collection.length > 0) {
            var ids = [];
            _.forEach(collection, (item: any) => {
                ids.push(item.id);
            });
            str = ids.join(',')
        }
        return str;
    }

    static WildCardToRegEx(wildCard: string) {
        var s = '^';
        var length = wildCard && wildCard.length || 0;

        for (var i = 0; i < length; i++) {
            var c = wildCard[i];
            switch (c) {
                case '*':
                    s += ".*";
                    break;
                case '?':
                    s += ".";
                    break;
                // Escape special regexp-characters
                case '(':
                case ')':
                case '[':
                case ']':
                case '$':
                case '^':
                case '.':
                case '{':
                case '}':
                case '|':
                case '\\':
                    s += "\\";
                    s += c;
                    break;
                default:
                    s += c;
                    break;
            }
        }
        s += '$';

        return s;
    }

    static isEmpty(str: string): boolean {
        return (!str || 0 === str.length);
    }

    static isNumeric(str: string): boolean {
        return !isNaN(Number(str));
    }

    static toFloat(str: string): number {
        if (!str)
            return 0;

        str = str.replace(/\s/g, '');
        str = str.replace(',', '.');
        var isNegative = str.startsWithCaseInsensitive('-') || str.startsWithCaseInsensitive('−');
        if (isNegative)
            str = str.replace('-', '').replace('−', '');
        var float: number = parseFloat(str);
        if (isNaN(float))
            float = 0;

        return isNegative ? (float * -1) : float;
    }

    // This is angulars snake-case function https://github.com/angular/angular.js/blob/master/src/Angular.js which converts eg MapGauge to map-gauge.
    static snake_case(name, separator): string {
        separator = separator || '_';
        return name.replace(/[A-Z]/g, function (letter, pos) {
            return (pos ? separator : '') + letter.toLowerCase();
        });
    }

    static getFileExtension(path: string): string {
        var basename = path.split(/[\\/]/).pop(),  // extract file name from full path ...
            // (supports `\\` and `/` separators)
            pos = basename.lastIndexOf(".");       // get last position of `.`

        if (basename === "" || pos < 1)            // if file name is empty or ...
            return "";                             //  `.` not found (-1) or comes first (0)

        return basename.slice(pos + 1);            // extract extension ignoring `.`
    }

    static getYearMonthString(totalMonths: number, yearsTerms, monthsTerms): string {
        if (!totalMonths)
            return "";

        let years = Math.floor(totalMonths / 12);
        let months = totalMonths % 12;
        let text = "";

        if (years !== 0)
            text = ("{0} " + (years === 1 ? yearsTerms["one"] : yearsTerms["plural"]) + " ").format(years.toString());

        if (months !== 0)
            text += ("{0} " + (months === 1 ? monthsTerms["one"] : monthsTerms["plural"])).format(months.toString());

        return text;
    }

    static buildUrl(base: string, params: Record<string, string | number | boolean>): string {
        const paramsString = Object.keys(params)
            .map(key => `${encodeURIComponent(key)}=${encodeURIComponent(params[key])}`)
            .join('&');
        return `${base}?${paramsString}`;
    }
}

export class Guid {
    static newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}
