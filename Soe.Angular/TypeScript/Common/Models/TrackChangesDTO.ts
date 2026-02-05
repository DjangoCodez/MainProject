import { ITrackChangesDTO, ITrackChangesLogDTO, System } from "../../Scripts/TypeLite.Net4";
import { TermGroup_TrackChangesAction, SettingDataType, SoeEntityType, TermGroup_TrackChangesColumnType, TermGroup_TrackChangesActionMethod } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TrackChangesDTO implements ITrackChangesDTO {
    action: TermGroup_TrackChangesAction;
    actionMethod: TermGroup_TrackChangesActionMethod;
    actorCompanyId: number;
    batch: System.IGuid;
    columnName: string;
    columnType: TermGroup_TrackChangesColumnType;
    created: Date;
    createdBy: string;
    dataType: SettingDataType;
    entity: SoeEntityType;
    fromValue: string;
    fromValueName: string;
    parentEntity: SoeEntityType;
    parentRecordId: number;
    recordId: number;
    role: string;
    topEntity: SoeEntityType;
    topRecordId: number;
    toValue: string;
    toValueName: string;
    trackChangesId: number;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
    }
}

export class TrackChangesLogDTO implements ITrackChangesLogDTO {
    actionMethodText: string;
    actionText: string;
    batch: System.IGuid;
    batchNbr: number;
    columnText: string;
    created: Date;
    createdBy: string;
    entity: SoeEntityType;
    entityText: string;
    fromValueText: string;
    recordId: number;
    recordName: string;
    role: string;
    topRecordName: string;
    toValueText: string;
    trackChangesId: number;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
    }
}