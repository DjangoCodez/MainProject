import { IRecalculateTimeHeadDTO, IRecalculateTimeRecordDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TermGroup_RecalculateTimeRecordStatus, TermGroup_RecalculateTimeHeadStatus } from "../../Util/CommonEnumerations";

export class RecalculateTimeHeadDTO implements IRecalculateTimeHeadDTO {
    action: any;
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    excecutedStartTime: Date;
    excecutedStopTime: Date;
    recalculateTimeHeadId: number;
    records: RecalculateTimeRecordDTO[];
    startDate: Date;
    status: TermGroup_RecalculateTimeHeadStatus;
    statusName: string;
    stopDate: Date;
    userId: number;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.excecutedStartTime = CalendarUtility.convertToDate(this.excecutedStartTime);
        this.excecutedStopTime = CalendarUtility.convertToDate(this.excecutedStopTime);
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}

export class RecalculateTimeRecordDTO implements IRecalculateTimeRecordDTO {
    employeeId: number;
    employeeName: string;
    errorMsg: string;
    recalculateTimeHeadId: number;
    recalculateTimeRecordId: number;
    recalculateTimeRecordStatus: TermGroup_RecalculateTimeRecordStatus;
    statusName: string;
    startDate: Date;
    stopDate: Date;
    warningMsg: string;

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
    }
}