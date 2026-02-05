import { IUserGaugeSettingDTO, IEmployeeRequestsGaugeDTO, IOpenShiftsGaugeDTO, IMyShiftsGaugeDTO, IWantedShiftsGaugeDTO, ITimeStampAttendanceGaugeDTO, IDashboardStatisticType, IDashboardStatisticRowDTO, IDashboardStatisticPeriodDTO, IDashboardStatisticsDTO } from "../../Scripts/TypeLite.Net4";
import { SettingDataType, TermGroup_TimeScheduleTemplateBlockShiftStatus, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, TermGroup_EmployeeRequestType, DashboardStatisticsType, TimeStampEntryType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { ShiftDTO } from "./TimeSchedulePlanningDTOs";

export class UserGaugeSettingDTO implements IUserGaugeSettingDTO {
    userGaugeSettingId: number;
    userGaugeId: number;
    dataType: number;
    name: string;
    strData: string;
    intData: number;
    decimalData: number;
    boolData: boolean;
    dateData: Date;
    timeData: Date;

    constructor(name: string, dataType: SettingDataType) {
        this.name = name;
        this.dataType = dataType;
    }
}

export class EmployeeRequestsGaugeDTO implements IEmployeeRequestsGaugeDTO {
    appliedDate: Date;
    employeeId: number;
    employeeName: string;
    employeeRequestType: TermGroup_EmployeeRequestType;
    employeeRequestTypeName: string;
    requestId: number;
    start: Date;
    status: number;
    statusName: string;
    stop: Date;
    timeDeviationCauseId: number;
    timeDeviationCauseName: string;
}

export class MyShiftsGaugeDTO implements IMyShiftsGaugeDTO {
    date: Date;
    shiftStatus: TermGroup_TimeScheduleTemplateBlockShiftStatus;
    shiftStatusName: string;
    shiftTypeId: number;
    shiftTypeName: string;
    shiftUserStatus: TermGroup_TimeScheduleTemplateBlockShiftUserStatus;
    shiftUserStatusName: string;
    time: string;
    timeScheduleTemplateBlockId: number;

    // Extensions
    dayName: string;

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class OpenShiftsGaugeDTO implements IOpenShiftsGaugeDTO {
    date: Date;
    iamInQueue: boolean;
    link: string;
    nbrInQueue: number;
    openType: number;
    openTypeName: string;
    shiftTypeId: number;
    shiftTypeName: string;
    time: string;
    timeScheduleTemplateBlockId: number;

    // Extensions
    dayName: string;

    public get dateString(): string {
        return this.date ? this.date.toFormattedDate() : '';
    }

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class WantedShiftsGaugeDTO implements IWantedShiftsGaugeDTO {
    date: Date;
    employee: string;
    employeeId: number;
    employeesInQueue: string;
    link: string;
    openType: number;
    openTypeName: string;
    shiftTypeId: number;
    shiftTypeName: string;
    time: string;
    timeScheduleTemplateBlockId: number;

    // Extensions
    dayName: string;

    public get dateString(): string {
        return this.date ? this.date.toFormattedDate() : '';
    }

    public fixDates() {
        this.date = CalendarUtility.convertToDate(this.date);
    }
}

export class TimeStampAttendanceGaugeDTO implements ITimeStampAttendanceGaugeDTO {
    accountName: string;
    employeeId: number;
    employeeNr: string;
    isBreak: boolean;
    isDistanceWork: boolean;
    isMissing: boolean;
    isPaidBreak: boolean;
    name: string;
    scheduleStartTime: Date;
    time: Date;
    timeStr: string;
    timeDeviationCauseName: string;
    timeTerminalName: string;
    type: TimeStampEntryType;
    typeName: string;
}

export class DashboardStatisticType implements IDashboardStatisticType {
    dashboardStatisticsType: DashboardStatisticsType;
    decription: string;
    key: string;
    name: string;
}

export class DashboardStatisticsDTO implements IDashboardStatisticsDTO {
    dashboardStatisticRows: DashboardStatisticRowDTO[];
    dashboardStatisticsType: DashboardStatisticsType;
    description: string;
    interval: any;
    name: string;
}

export class DashboardStatisticRowDTO implements IDashboardStatisticRowDTO {
    dashboardStatisticPeriods: DashboardStatisticPeriodDTO[];
    dashboardStatisticsRowType: any;
    name: string;
}

export class DashboardStatisticPeriodDTO implements IDashboardStatisticPeriodDTO {
    dashboardStatisticsPeriodRowType: any;
    from: Date;
    to: Date;
    value: number;

    public fixDates() {
        this.from = CalendarUtility.convertToDate(this.from);
        this.to = CalendarUtility.convertToDate(this.to);
    }
}
