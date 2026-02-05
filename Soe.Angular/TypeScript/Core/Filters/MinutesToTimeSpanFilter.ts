import { CalendarUtility } from "../../Util/CalendarUtility";

export class MinutesToTimeSpanFilter {

    private static filter(value: number, showDays: boolean, showSeconds: boolean, padHours: boolean, treatUndefinedAsEmpty = false, maxOneDay = false, addPlusIfPositive = false) {
        if (treatUndefinedAsEmpty && !value)
            return "";

        return CalendarUtility.minutesToTimeSpan(value, showDays, showSeconds, padHours, maxOneDay, addPlusIfPositive);
    }

    public static create() {
        return MinutesToTimeSpanFilter.filter;
    }
}
