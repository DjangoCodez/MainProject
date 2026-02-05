import { ITimeStampAdditionDTO, ITimeStampEntryDTO, ITimeStampEntryExtendedDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_TimeStampEntryOriginType, TermGroup_TimeStampEntryStatus, TimeStampAdditionType, TimeStampEntryType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class TimeStampEntryDTO implements ITimeStampEntryDTO {
    accountId: number;
    accountName: string;
    accountNr: string;
    actorCompanyId: number;
    adjustedTime: Date;
    adjustedTimeBlockDateDate: Date;
    created: Date;
    createdBy: string;
    date: Date;
    employeeChildId: number;
    employeeId: number;
    employeeManuallyAdjusted: boolean;
    employeeName: string;
    employeeNr: string;
    extended: TimeStampEntryExtendedDTO[];
    isBreak: boolean;
    isDistanceWork: boolean;
    isModified: boolean;
    isPaidBreak: boolean;
    manuallyAdjusted: boolean;
    modified: Date;
    modifiedBy: string;
    note: string;
    originalTime: Date;
    originType: TermGroup_TimeStampEntryOriginType;
    shiftTypeId: number;
    state: SoeEntityState;
    status: TermGroup_TimeStampEntryStatus;
    terminalStampData: string;
    time: Date;
    timeBlockDateDate: Date;
    timeBlockDateId: number;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    timeScheduleTemplatePeriodId: number;
    timeScheduleTypeId: number;
    timeScheduleTypeName: string;
    timeStampEntryId: number;
    timeTerminalAccountId: number;
    timeTerminalId: number;
    timeTerminalName: string;
    type: TimeStampEntryType;
    typeName: string;

    // Extensions
    terminalInfo: string;
    statusText: string;
    originTypeText: string;

    public fixDates() {
        this.adjustedTime = CalendarUtility.convertToDate(this.adjustedTime);
        this.adjustedTimeBlockDateDate = CalendarUtility.convertToDate(this.adjustedTimeBlockDateDate);
        this.created = CalendarUtility.convertToDate(this.created);
        this.date = CalendarUtility.convertToDate(this.date);
        this.modified = CalendarUtility.convertToDate(this.modified);
        this.originalTime = CalendarUtility.convertToDate(this.originalTime);
        this.time = CalendarUtility.convertToDate(this.time);
        this.timeBlockDateDate = CalendarUtility.convertToDate(this.timeBlockDateDate);
    }

    public setTypes() {
        if (this.extended) {
            this.extended = this.extended.map(x => {
                let obj = new TimeStampEntryExtendedDTO();
                angular.extend(obj, x);
                return obj;
            });
        } else {
            this.extended = [];
        }
    }
}

export class TimeStampEntryExtendedDTO implements ITimeStampEntryExtendedDTO {
    accountId: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    quantity: number;
    state: SoeEntityState;
    timeCodeId: number;
    timeScheduleTypeId: number;
    timeStampEntryExtendedId: number;
    timeStampEntryId: number;

    // Extensions
    addition: TimeStampAdditionDTO;

    get enableQuantity(): boolean {
        return this.addition?.type === TimeStampAdditionType.TimeCodeVariableValue;
    }
}

export class TimeStampAdditionDTO implements ITimeStampAdditionDTO {
    fixedQuantity: number;
    id: number;
    name: string;
    type: TimeStampAdditionType;
}