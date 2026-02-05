import { IEmployeeRequestDTO, IExtendedAbsenceSettingDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_EmployeeRequestResultStatus, SoeEntityState, TermGroup_EmployeeRequestStatus, TermGroup_EmployeeRequestType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class EmployeeRequestDTO implements IEmployeeRequestDTO {
    accountNamesString: string;
    actorCompanyId: number;
    categoryNamesString: string;
    comment: string;
    created: Date;
    createdBy: string;
    createdString: string;
    employeeChildId: number;
    employeeChildName: string;
    employeeId: number;
    employeeName: string;
    employeeRequestId: number;
    extendedSettings: IExtendedAbsenceSettingDTO;
    intersectMessage: string;
    isSelected: boolean;
    modified: Date;
    modifiedBy: string;
    reActivate: boolean;
    requestIntersectsWithCurrent: boolean;
    resultStatus: TermGroup_EmployeeRequestResultStatus;
    resultStatusName: string;
    start: Date;
    startString: string;
    state: SoeEntityState;
    status: TermGroup_EmployeeRequestStatus;
    statusName: string;
    stop: Date;
    stopString: string;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
    type: TermGroup_EmployeeRequestType;

    // Extensions
    typeName: string;

    public fixDates() {
        this.start = CalendarUtility.convertToDate(this.start);
        this.stop = CalendarUtility.convertToDate(this.stop);
        this.created = CalendarUtility.convertToDate(this.created);
        this.modified = CalendarUtility.convertToDate(this.modified);
    }

    public get isWholeDay(): boolean {
        return (this.start && this.start.isBeginningOfDay() && this.stop && (this.stop.isBeginningOfDay() || this.stop.isEndOfDay() || this.stop.addSeconds(1).isBeginningOfDay()));
    }
}

export class ExtendedAbsenceSettingDTO implements IExtendedAbsenceSettingDTO {

    absenceFirstAndLastDay: boolean;
    absenceFirstDayStart: Date;
    absenceLastDayStart: Date;
    absenceWholeFirstDay: boolean;
    absenceWholeLastDay: boolean;
    adjustAbsenceAllDaysStart: Date;
    adjustAbsenceAllDaysStop: Date;
    adjustAbsenceFriStart: Date;
    adjustAbsenceFriStop: Date;
    adjustAbsenceMonStart: Date;
    adjustAbsenceMonStop: Date;
    adjustAbsencePerWeekDay: boolean;
    adjustAbsenceSatStart: Date;
    adjustAbsenceSatStop: Date;
    adjustAbsenceSunStart: Date;
    adjustAbsenceSunStop: Date;
    adjustAbsenceThuStart: Date;
    adjustAbsenceThuStop: Date;
    adjustAbsenceTueStart: Date;
    adjustAbsenceTueStop: Date;
    adjustAbsenceWedStart: Date;
    adjustAbsenceWedStop: Date;
    extendedAbsenceSettingId: number;
    percentalAbsence: boolean;
    percentalAbsenceOccursEndOfDay: boolean;
    percentalAbsenceOccursStartOfDay: boolean;
    percentalValue: number;
}
