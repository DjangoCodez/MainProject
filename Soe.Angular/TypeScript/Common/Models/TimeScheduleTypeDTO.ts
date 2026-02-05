import { ITimeScheduleTypeFactorSmallDTO, ITimeScheduleTypeSmallDTO, ITimeScheduleTypeDTO, ITimeScheduleTypeFactorDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeScheduleTypeDTO implements ITimeScheduleTypeDTO {
    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    factors: TimeScheduleTypeFactorDTO[];
    isActive: boolean;
    isAll: boolean;
    isBilagaJ: boolean;
    isNotScheduleTime: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    timeScheduleTypeId: number;
    useScheduleTimeFactor: boolean;
    ignoreIfExtraShift: boolean;
}

export class TimeScheduleTypeSmallDTO implements ITimeScheduleTypeSmallDTO {
    timeScheduleTypeId: number;
    code: string;
    name: string;
    factors: ITimeScheduleTypeFactorSmallDTO[];
}

export class TimeScheduleTypeFactorDTO implements ITimeScheduleTypeFactorDTO {
    created: Date;
    createdBy: string;
    factor: number;
    fromTime: Date;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    timeScheduleTypeFactorId: number;
    timeScheduleTypeId: number;
    toTime: Date;

    public get length(): number {
        return this.toTime.diffMinutes(this.fromTime);
    }

    public fixDates() {
        this.fromTime = CalendarUtility.convertToDate(this.fromTime);
        this.toTime = CalendarUtility.convertToDate(this.toTime);
    }
}

export class TimeScheduleTypeFactorSmallDTO implements ITimeScheduleTypeFactorSmallDTO {
    factor: number;
    fromTime: Date;
    toTime: Date;

    public fixDates() {
        this.fromTime = CalendarUtility.convertToDate(this.fromTime);
        this.toTime = CalendarUtility.convertToDate(this.toTime);
    }
}
