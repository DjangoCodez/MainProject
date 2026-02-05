import { IOpeningHoursDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class OpeningHoursDTO implements IOpeningHoursDTO {
    accountId: number;
    accountName: string;
    actorCompanyId: number;
    closingTime: Date;
    created: Date;
    createdBy: string;
    description: string;
    fromDate: Date;
    modified: Date;
    modifiedBy: string;
    name: string;
    openingHoursId: number;
    openingTime: Date;
    specificDate: Date;
    standardWeekDay: number;
    state: number;

    // Extensions
    weekdayName: string;

    public fixDates() {
        this.openingTime = CalendarUtility.convertToDate(this.openingTime);
        this.closingTime = CalendarUtility.convertToDate(this.closingTime);
        this.specificDate = CalendarUtility.convertToDate(this.specificDate);
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}