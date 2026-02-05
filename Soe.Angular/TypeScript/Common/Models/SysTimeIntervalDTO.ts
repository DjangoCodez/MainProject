import { ISysTimeIntervalDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_TimeIntervalPeriod, TermGroup_TimeIntervalStart, TermGroup_TimeIntervalStop } from "../../Util/CommonEnumerations";

export class SysTimeIntervalDTO implements ISysTimeIntervalDTO {
    name: string;
    period: TermGroup_TimeIntervalPeriod;
    sort: number;
    start: TermGroup_TimeIntervalStart;
    startOffset: number;
    stop: TermGroup_TimeIntervalStop;
    stopOffset: number;
    sysTermId: number;
    sysTimeIntervalId: number;
}