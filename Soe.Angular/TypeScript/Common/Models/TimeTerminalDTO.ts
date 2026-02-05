import { ITimeTerminalDTO, ITimeTerminalSettingDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TimeTerminalType, TimeTerminalSettingDataType, TimeTerminalSettingType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { Guid } from "../../Util/StringUtility";

export class TimeTerminalDTO implements ITimeTerminalDTO {
    actorCompanyId: number;
    categoryIds: number[];
    companyApiKey: string;
    companyName: string;
    created: Date;
    createdBy: string;
    lastSync: Date;
    macAddress: string;
    macName: string;
    macNumber: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    registered: boolean;
    state: SoeEntityState;
    sysCompDBId: number;
    terminalDbSchemaVersion: number;
    terminalVersion: string;
    timeTerminalId: number;
    timeTerminalGuid: Guid;
    timeTerminalSettings: TimeTerminalSettingDTO[];
    type: TimeTerminalType;
    typeName: string;
    uri: string;

    // Extensions
    syncStateTooltip: string;
    lastSyncStateColor: string;

    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public get isTimeSpotType(): boolean {
        return this.type === TimeTerminalType.TimeSpot;
    }

    public get isTimeStampType(): boolean {
        return this.type === TimeTerminalType.XETimeStamp;
    }

    public get isWebTimeStampType(): boolean {
        return this.type === TimeTerminalType.WebTimeStamp;
    }

    public get isGoTimeStampType(): boolean {
        return this.type === TimeTerminalType.GoTimeStamp;
    }

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.lastSync = CalendarUtility.convertToDate(this.lastSync);
    }

    public setTypes() {
        if (this.timeTerminalSettings) {
            this.timeTerminalSettings = this.timeTerminalSettings.map(x => {
                let obj = new TimeTerminalSettingDTO(x.type, x.dataType);
                angular.extend(obj, x);
                obj.fixDates();
                obj.setTypes();
                return obj;
            });
        } else {
            this.timeTerminalSettings = [];
        }
    }
}

export class TimeTerminalSettingDTO implements ITimeTerminalSettingDTO {
    boolData: boolean;
    children: TimeTerminalSettingDTO[];
    created: Date;
    createdBy: string;
    dataType: TimeTerminalSettingDataType;
    dateData: Date;
    decimalData: number;
    id: number;
    intData: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    parentId: number;
    state: SoeEntityState;
    strData: string;
    timeData: Date;
    timeTerminalId: number;
    timeTerminalSettingId: number;
    type: TimeTerminalSettingType;

    constructor(type: TimeTerminalSettingType, dataType: TimeTerminalSettingDataType) {
        this.type = type;
        this.dataType = dataType;
    }

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.dateData = CalendarUtility.convertToDate(this.dateData);
    }

    public setTypes() {
        if (this.children) {
            this.children = this.children.map(x => {
                let obj = new TimeTerminalSettingDTO(x.type, x.dataType);
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            });
        } else {
            this.children = [];
        }
    }
}