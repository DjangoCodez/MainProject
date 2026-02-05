import { IEventHistoryDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityType, SoeEntityState, TermGroup_EventHistoryType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EventHistoryDTO implements IEventHistoryDTO {
    actorCompanyId: number;
    batchId: number;
    booleanValue: boolean;
    created: Date;
    createdBy: string;
    dateValue: Date;
    decimalValue: number;
    entity: SoeEntityType;
    entityName: string;
    eventHistoryId: number;
    integerValue: number;
    modified: Date;
    modifiedBy: string;
    recordId: number;
    recordName: string;
    state: SoeEntityState;
    stringValue: string;
    type: TermGroup_EventHistoryType;
    typeName: string;
    userId: number;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.dateValue = CalendarUtility.convertToDate(this.dateValue);
    }
}