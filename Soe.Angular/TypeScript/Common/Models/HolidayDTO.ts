import { IHolidayDTO, IHolidaySmallDTO, IDayTypeDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class HolidayDTO implements IHolidayDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    date: Date;
    dayType: IDayTypeDTO;
    dayTypeId: number;
    dayTypeName: string;
    description: string;
    holidayId: number;
    import: boolean;
    isRedDay: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    sysHolidayId: number;
    sysHolidayTypeId: number;
    sysHolidayTypeName: string;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.date = CalendarUtility.convertToDate(this.date);
        this.modified = CalendarUtility.convertToDate(this.modified);
    }
}

export class HolidaySmallDTO implements IHolidaySmallDTO {
    date: Date;
    description: string;
    holidayId: number;
    isRedDay: boolean;
    name: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}
