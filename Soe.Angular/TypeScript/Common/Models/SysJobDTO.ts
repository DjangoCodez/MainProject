import { ISysJobDTO, ISysJobSettingDTO, ISysScheduledJobDTO, ISysScheduledJobLogDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState, SysJobSettingType, ScheduledJobRecurrenceType, ScheduledJobRetryType, ScheduledJobState, ScheduledJobType, SettingDataType } from "../../Util/CommonEnumerations";

export class SysJobDTO implements ISysJobDTO {
    allowParallelExecution: boolean;
    assemblyName: string;
    className: string;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    state: SoeEntityState;
    sysJobId: number;
    sysJobSettings: SysJobSettingDTO[];

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}

export class SysJobSettingDTO implements ISysJobSettingDTO {
    boolData: boolean;
    dataType: SettingDataType;
    dateData: Date;
    decimalData: number;
    intData: number;
    name: string;
    strData: string;
    sysJobSettingId: number;
    timeData: Date;
    type: SysJobSettingType;

    // Extensions
    value: any;

    fixDates() {
        this.dateData = CalendarUtility.convertToDate(this.dateData);
        this.timeData = CalendarUtility.convertToDate(this.timeData);
    }

    get icon(): string {
        switch (this.dataType) {
            case SettingDataType.Boolean:
                return "fal fa-check";
            case SettingDataType.Date:
                return "fal fa-calendar-alt";
            case SettingDataType.Decimal:
                return "fal fa-hashtag";
            case SettingDataType.Integer:
                return "fal fa-hashtag";
            case SettingDataType.String:
                return "fal fa-font";
            case SettingDataType.Time:
                return "fal fa-clock";
        }
    }

    setValue(trueLabel: string, falseLabel: string) {
        switch (this.dataType) {
            case SettingDataType.Boolean:
                this.value = this.boolData ? trueLabel : falseLabel;
                break;
            case SettingDataType.Date:
                this.value = this.dateData ? this.dateData.toFormattedDate() : "";
                break;
            case SettingDataType.Decimal:
                this.value = this.decimalData || 0;
                break;
            case SettingDataType.Integer:
                this.value = this.intData || 0;
                break;
            case SettingDataType.String:
                this.value = this.strData || "";
                break;
            case SettingDataType.Time:
                this.value = this.timeData ? this.timeData.toFormattedTime() : "";
                break;
        }
    }
}

export class SysScheduledJobDTO implements ISysScheduledJobDTO {
    allowParallelExecution: boolean;
    created: Date;
    createdBy: string;
    databaseName: string;
    description: string;
    executeTime: Date;
    executeUserId: number;
    jobStatusMessage: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    recurrenceCount: number;
    recurrenceDate: Date;
    recurrenceInterval: string;
    recurrenceType: ScheduledJobRecurrenceType;
    retryCount: number;
    retryTypeForExternalError: ScheduledJobRetryType;
    retryTypeForInternalError: ScheduledJobRetryType;
    state: ScheduledJobState;
    stateName: string;
    sysJob: SysJobDTO;
    sysJobId: number;
    sysJobSettings: SysJobSettingDTO[];
    sysScheduledJobId: number;
    type: ScheduledJobType;

    fixDates() {
        this.executeTime = CalendarUtility.convertToDate(this.executeTime);
        this.recurrenceDate = CalendarUtility.convertToDate(this.recurrenceDate);
    }

    setTypes() {
        if (this.sysJobSettings) {
            this.sysJobSettings = this.sysJobSettings.map(s => {
                let obj = new SysJobSettingDTO();
                angular.extend(obj, s);
                obj.fixDates();
                return obj;
            });
        } else {
            this.sysJobSettings = [];
        }
    }

    get stateColor(): string {
        if (this.state === ScheduledJobState.Interrupted) {
            return "#f8d4d4";
        } else if (this.state === ScheduledJobState.Inactive) {
            return "#fce3cc";
        } else {
            let minutesPassed = CalendarUtility.getDateNow().diffMinutes(this.executeTime);
            if (minutesPassed < 0 || (this.state === ScheduledJobState.Running && minutesPassed < 5))
                return "#def1de";
            else if (minutesPassed < 30)
                return "#fce3cc";
            else
                return "#f8d4d4";
        }
    }
}

export class SysScheduledJobLogDTO implements ISysScheduledJobLogDTO {
    batchNr: number;
    logLevel: number;
    logLevelName: string;
    message: string;
    sysScheduledJobId: number;
    sysScheduledJobLogId: number;
    sysScheduledJobName: string;
    time: Date;

    fixDates() {
        this.time = CalendarUtility.convertToDate(this.time);
    }
}
