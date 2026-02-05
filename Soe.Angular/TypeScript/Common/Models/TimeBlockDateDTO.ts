import { ITimeBlockDateDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_TimeBlockDateStampingStatus, SoeTimeBlockDateStatus } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeBlockDateDTO implements ITimeBlockDateDTO {
    date: Date;
    discardedBreakEvaluation: boolean;
    employeeId: number;
    stampingStatus: TermGroup_TimeBlockDateStampingStatus;
    status: SoeTimeBlockDateStatus;
    timeBlockDateId: number;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}
