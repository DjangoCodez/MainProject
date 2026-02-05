import { ITimeScheduleEventDTO, ITimeScheduleEventMessageGroupDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class TimeScheduleEventDTO implements ITimeScheduleEventDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    date: Date;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    timeScheduleEventId: number;
    timeScheduleEventMessageGroups: ITimeScheduleEventMessageGroupDTO[];
}

export class TimeScheduleEventMessageGroupDTO implements ITimeScheduleEventMessageGroupDTO {
    messageGroupId: number;
    timeScheduleEventId: number;
    timeScheduleEventMessageGroupId: number;
}
