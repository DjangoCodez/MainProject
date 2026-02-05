export class SchedulerUtility {

    public static readonly CRONTAB_ALL_SELECTED = '*';
    public static readonly CRONTAB_ITEMTYPE_SEPARATOR = ' ';
    public static readonly CRONTAB_ITEM_SEPARATOR = ',';
    public static readonly CRONTAB_RANGE_SEPARATOR = '-';
    public static readonly CRONTAB_MINUTES_LOWER = 0;
    public static readonly CRONTAB_MINUTES_UPPER = 59;
    public static readonly CRONTAB_HOURS_LOWER = 0;
    public static readonly CRONTAB_HOURS_UPPER = 23;
    public static readonly CRONTAB_DAYS_LOWER = 1;
    public static readonly CRONTAB_DAYS_UPPER = 31;
    public static readonly CRONTAB_MONTHS_LOWER = 1;
    public static readonly CRONTAB_MONTHS_UPPER = 12;
    public static readonly CRONTAB_WEEKDAYS_LOWER = 1;
    public static readonly CRONTAB_WEEKDAYS_UPPER = 7;

    public static getCrontabExpression(minutes: number[], hours: number[], days: number[], months: number[], weekdays: number[]): string {
        var expression: string = "";
        expression += this.getCrontabMinutesExpression(minutes) + this.CRONTAB_ITEMTYPE_SEPARATOR;
        expression += this.getCrontabHoursExpression(hours) + this.CRONTAB_ITEMTYPE_SEPARATOR;
        expression += this.getCrontabDaysExpression(days) + this.CRONTAB_ITEMTYPE_SEPARATOR;
        expression += this.getCrontabMonthsExpression(months) + this.CRONTAB_ITEMTYPE_SEPARATOR;
        expression += this.getCrontabWeekdaysExpression(weekdays);

        return expression;
    }

    public static getCrontabMinutesExpression(minutes: number[]): string {
        return this.getCrontabIntegerExpression(minutes, this.CRONTAB_MINUTES_LOWER, this.CRONTAB_MINUTES_UPPER);
    }

    public static getCrontabHoursExpression(hours: number[]): string {
        return this.getCrontabIntegerExpression(hours, this.CRONTAB_HOURS_LOWER, this.CRONTAB_HOURS_UPPER);
    }

    public static getCrontabDaysExpression(days: number[]): string {
        return this.getCrontabIntegerExpression(days, this.CRONTAB_DAYS_LOWER, this.CRONTAB_DAYS_UPPER);
    }

    public static getCrontabMonthsExpression(months: number[]): string {
        return this.getCrontabIntegerExpression(months, this.CRONTAB_MONTHS_LOWER, this.CRONTAB_MONTHS_UPPER);
    }

    public static getCrontabWeekdaysExpression(weekdays: number[]): string {
        return this.getCrontabIntegerExpression(weekdays, this.CRONTAB_WEEKDAYS_LOWER, this.CRONTAB_WEEKDAYS_UPPER);
    }

    public static getCrontabIntegerExpression(ints: number[], lowerBound: number, upperBound: number): string {
        // All or no items selected
        if (ints.length === (upperBound - lowerBound + 1) || ints.length === 0)
            return this.CRONTAB_ALL_SELECTED;

        var expression = "";
        var prevInt = undefined;

        _.forEach(ints, (i) => {
            // Range
            if (prevInt && prevInt + 1 === i) {
                if (!expression.endsWith(this.CRONTAB_RANGE_SEPARATOR))
                    expression += this.CRONTAB_RANGE_SEPARATOR;
            }
            else if (expression.endsWith(this.CRONTAB_RANGE_SEPARATOR)) {
                expression += prevInt;  // End range
            }

            // Single
            if (!expression.endsWith(this.CRONTAB_RANGE_SEPARATOR)) {
                if (expression.length > 0)
                    expression += this.CRONTAB_ITEM_SEPARATOR;
                expression += i;
            }

            prevInt = i;
        });

        if (expression.endsWith(this.CRONTAB_RANGE_SEPARATOR))
            expression += prevInt;

        return expression;
    }

    public static parseCrontabExpression(expression: string): CronExpressionPart[] {
        let parts: CronExpressionPart[] = [];

        if (!expression)
            expression = ("{0} {1} {2} {3} {4}").format(this.CRONTAB_ALL_SELECTED, this.CRONTAB_ALL_SELECTED, this.CRONTAB_ALL_SELECTED, this.CRONTAB_ALL_SELECTED, this.CRONTAB_ALL_SELECTED);

        let types: string[] = expression.split(this.CRONTAB_ITEMTYPE_SEPARATOR);

        parts.push(new CronExpressionPart(CronExpressionType.Minute, this.parseCrontabSingleExpression(types[0], this.CRONTAB_MINUTES_LOWER, this.CRONTAB_MINUTES_UPPER)));
        parts.push(new CronExpressionPart(CronExpressionType.Hour, this.parseCrontabSingleExpression(types[1], this.CRONTAB_HOURS_LOWER, this.CRONTAB_HOURS_UPPER)));
        parts.push(new CronExpressionPart(CronExpressionType.Day, this.parseCrontabSingleExpression(types[2], this.CRONTAB_DAYS_LOWER, this.CRONTAB_DAYS_UPPER)));
        parts.push(new CronExpressionPart(CronExpressionType.Month, this.parseCrontabSingleExpression(types[3], this.CRONTAB_MONTHS_LOWER, this.CRONTAB_MONTHS_UPPER)));
        parts.push(new CronExpressionPart(CronExpressionType.Weekday, this.parseCrontabSingleExpression(types[4], this.CRONTAB_WEEKDAYS_LOWER, this.CRONTAB_WEEKDAYS_UPPER)));

        return parts;
    }

    private static parseCrontabSingleExpression(expression: string, lowerBound: number, upperBound: number): number[] {
        let list: number[] = [];

        // None or all
        if (expression.length === 0 || expression === this.CRONTAB_ALL_SELECTED)
            return list;

        let items: string[] = expression.split(this.CRONTAB_ITEM_SEPARATOR);
        let range: string[];

        _.forEach(items, (item) => {
            // Range
            if (item.includes(this.CRONTAB_RANGE_SEPARATOR)) {
                range = item.split(this.CRONTAB_RANGE_SEPARATOR);
                if (range.length == 2) {
                    // Get first part of range
                    let lowerBoundRange: number = isNaN(Number(range[0])) ? -1 : Number(range[0]);
                    // Get last part of range
                    let upperBoundRange: number = isNaN(Number(range[1])) ? -1 : Number(range[1]);

                    // Check that first part has a lower number than last part
                    // Otherwise switch places between first and last part
                    if (lowerBoundRange > upperBoundRange) {
                        var tmp = lowerBoundRange;
                        lowerBoundRange = upperBoundRange;
                        upperBoundRange = tmp;
                    }

                    // Add all numbers in range
                    if (lowerBoundRange < lowerBound)
                        lowerBoundRange = lowerBound;
                    if (upperBoundRange > upperBound)
                        upperBoundRange = upperBound;
                    for (var j = lowerBoundRange; j < upperBoundRange + 1; j++) {
                        if (!_.includes(list, j))
                            list.push(j);
                    }
                } else {
                    return this.CRONTAB_ALL_SELECTED;
                }
            } else {
                let integer: number = isNaN(Number(item)) ? -1 : Number(item);
                if (integer > -1) {
                    if (integer >= lowerBound && integer <= upperBound && !_.includes(list, integer))
                        list.push(integer);
                } else {
                    return this.CRONTAB_ALL_SELECTED;
                }
            }
        });

        return list;
    }

    public static getCrontabPartValue(parts: CronExpressionPart[], type: CronExpressionType):number[] {
        let part = _.find(parts, p => p.type === type);
        return part ? part.value : [];
    }
}

export enum CronExpressionType {
    Minute = 0,
    Hour = 1,
    Day = 2,
    Month = 3,
    Weekday = 4
}

export class CronExpressionPart {
    type: CronExpressionType;
    value: number[];

    constructor(type: CronExpressionType, value: number[]) {
        this.type = type;
        this.value = value;
    }
}

export class CronIntervalItem {
    id: number;
    name: string;
    selected: boolean;

    constructor(id: number, name: string, selected: boolean) {
        this.id = id;
        this.name = name;
        this.selected = selected;
    }
}
