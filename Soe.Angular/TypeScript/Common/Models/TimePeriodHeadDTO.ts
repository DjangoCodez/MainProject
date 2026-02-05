import { ITimePeriodHeadDTO, ITimePeriodHeadGridDTO } from "../../Scripts/TypeLite.Net4";
import { TimePeriodDTO } from "./TimePeriodDTO";
import { TermGroup_TimePeriodType, SoeEntityState } from "../../Util/CommonEnumerations";

export class TimePeriodHeadDTO implements ITimePeriodHeadDTO {
    accountId: number;
    actorCompanyId: number;
    childId: number;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    timePeriodHeadId: number;
    timePeriods: TimePeriodDTO[];
    timePeriodType: TermGroup_TimePeriodType;
    timePeriodTypeName: string;
}

export class TimePeriodHeadGridDTO implements ITimePeriodHeadGridDTO {
    accountName: string;
    childName: string;
    description: string;
    name: string;
    timePeriodHeadId: number;
    timePeriodType: TermGroup_TimePeriodType;
    timePeriodTypeName: string;
}
