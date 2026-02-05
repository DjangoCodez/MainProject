import { IDateRangeDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";


export class DateRangeDTO implements IDateRangeDTO {
    comment: string;
    minutes: number;
    start: Date;
    stop: Date;

    constructor(start?: Date, stop?: Date) {
        if (start)
            this.start = start;
        if (stop)
            this.stop = stop;
    }

    public fixDates() {
        if (this.start)
            this.start = CalendarUtility.convertToDate(this.start);
        if (this.stop)
            this.stop = CalendarUtility.convertToDate(this.stop);
    }

    public get duration(): number {
        return this.stop.diffMinutes(this.start);
    }

    public isOverlapping(dateFrom: Date, dateTo: Date): boolean {
        return CalendarUtility.isRangesOverlapping(this.start, this.stop, dateFrom, dateTo);
    }

    public isFullyOverlapping(dateFrom: Date, dateTo: Date): boolean {
        var duration = CalendarUtility.getIntersectingDuration(this.start, this.stop, dateFrom, dateTo);
        return (duration > 0 && duration === dateTo.diffMinutes(dateFrom));
    }

    public isPartlyOverlapping(dateFrom: Date, dateTo: Date): boolean {
        var duration = CalendarUtility.getIntersectingDuration(this.start, this.stop, dateFrom, dateTo);
        return (duration > 0 && duration < dateTo.diffMinutes(dateFrom));
    }
}
