import { CalendarUtility } from "../../Util/CalendarUtility";

export class GetDayNameFilter {

    private static filter(weekday: number) {
        return CalendarUtility.getDayName(weekday);
    }

    public static create() {
        return GetDayNameFilter.filter;
    }
}
