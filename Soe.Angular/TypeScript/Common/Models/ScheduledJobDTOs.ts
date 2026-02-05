import { IScheduledJobHeadDTO, IScheduledJobHeadGridDTO, IScheduledJobLogDTO, IScheduledJobRowDTO, IScheduledJobSettingDTO, ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SettingDataType, SoeEntityState, TermGroup_ScheduledJobLogLevel, TermGroup_ScheduledJobLogStatus, TermGroup_ScheduledJobSettingType } from "../../Util/CommonEnumerations";
import { GraphicsUtility } from "../../Util/GraphicsUtility";

export class ScheduledJobHeadDTO implements IScheduledJobHeadDTO {
    created: Date;
    createdBy: string;
    description: string;
    logs: ScheduledJobLogDTO[];
    modified: Date;
    modifiedBy: string;
    name: string;
    parentId: number;
    rows: ScheduledJobRowDTO[];
    scheduledJobHeadId: number;
    settings: ScheduledJobSettingDTO[];
    sharedOnLicense: boolean;
    sort: number;
    state: SoeEntityState;

    public setTypes() {
        if (this.logs) {
            this.logs = this.logs.map(l => {
                let lObj = new ScheduledJobLogDTO();
                angular.extend(lObj, l);
                lObj.fixDates();
                return lObj;
            });
        } else {
            this.logs = [];
        }

        if (this.rows) {
            this.rows = this.rows.map(r => {
                let rObj = new ScheduledJobRowDTO();
                angular.extend(rObj, r);
                rObj.fixDates();
                return rObj;
            });
        } else {
            this.rows = [];
        }

        if (this.settings) {
            this.settings = this.settings.map(s => {
                let sObj = new ScheduledJobSettingDTO();
                angular.extend(sObj, s);
                sObj.fixDates();
                return sObj;
            });
        } else {
            this.settings = [];
        }
    }
}

export class ScheduledJobHeadGridDTO implements IScheduledJobHeadGridDTO {
    description: string;
    name: string;
    scheduledJobHeadId: number;
    sharedOnLicense: boolean;
    sort: number;
    state: SoeEntityState;
}

export class ScheduledJobRowDTO implements IScheduledJobRowDTO {
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    nextExecutionTime: Date;
    recurrenceInterval: string;
    recurrenceIntervalText: string;
    scheduledJobHeadId: number;
    scheduledJobRowId: number;
    state: SoeEntityState;
    sysTimeIntervalId: number;
    timeIntervalText: string;

    public fixDates() {
        this.nextExecutionTime = CalendarUtility.convertToDate(this.nextExecutionTime);
    }

    public get nextExecutionTimeText(): string {
        return this.nextExecutionTime ? this.nextExecutionTime.toFormattedDateTime() : '';
    }
}

export class ScheduledJobLogDTO implements IScheduledJobLogDTO {
    batchNr: number;
    logLevel: TermGroup_ScheduledJobLogLevel;
    logLevelName: string;
    message: string;
    scheduledJobHeadId: number;
    scheduledJobLogId: number;
    scheduledJobRowId: number;
    status: TermGroup_ScheduledJobLogStatus;
    statusName: string;
    time: Date;

    public fixDates() {
        this.time = CalendarUtility.convertToDate(this.time);
    }

    public getCellStyle(field: string): any {
        if (field === 'statusName') {
            let color: string = '';
            switch (this.status) {
                case TermGroup_ScheduledJobLogStatus.Started:
                    color = "#f2f7fc";
                    break;
                case TermGroup_ScheduledJobLogStatus.Running:
                    color = "#cde0f4";
                    break;
                case TermGroup_ScheduledJobLogStatus.Aborted:
                    color = "#f8d4d4";
                    break;
                case TermGroup_ScheduledJobLogStatus.Finished:
                    color = "#def1de";
                    break;
            }
            return { 'background-color': GraphicsUtility.addAlphaValue(color, 0.3) };
        } else if (field === 'logLevelName') {
            let color: string = '';
            switch (this.logLevel) {
                case TermGroup_ScheduledJobLogLevel.Information:
                    color = "#cde0f4";
                    break;
                case TermGroup_ScheduledJobLogLevel.Success:
                    color = "#def1de";
                    break;
                case TermGroup_ScheduledJobLogLevel.Warning:
                    color = "#fce3cc";
                    break;
                case TermGroup_ScheduledJobLogLevel.Error:
                    color = "#f8d4d4";
                    break;
            }
            return { 'background-color': GraphicsUtility.addAlphaValue(color, 0.3) };
        }

        return undefined;
    }
}

export class ScheduledJobSettingDTO implements IScheduledJobSettingDTO {
    boolData: boolean;
    dataType: SettingDataType;
    dateData: Date;
    decimalData: number;
    intData: number;
    name: string;
    options: ISmallGenericType[];
    scheduledJobHeadId: number;
    scheduledJobSettingId: number;
    state: SoeEntityState;
    strData: string;
    timeData: Date;
    type: TermGroup_ScheduledJobSettingType;

    // Extensions
    value: any;

    public fixDates() {
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

    setDataType() {
        switch (this.type) {
            case TermGroup_ScheduledJobSettingType.BridgeSetupAddress:
            case TermGroup_ScheduledJobSettingType.BridgeSetupPath:
            case TermGroup_ScheduledJobSettingType.BridgeSetupContainer:
            case TermGroup_ScheduledJobSettingType.BridgeSetupFileName:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialSecret:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialUser:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialPassword:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialTokenEndPoint:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialGrantType:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialTenent:
            case TermGroup_ScheduledJobSettingType.ExportKey:
            case TermGroup_ScheduledJobSettingType.BridgeSetupCallBackUrl:
            case TermGroup_ScheduledJobSettingType.BridgeSetupPathTransfer:
            case TermGroup_ScheduledJobSettingType.BridgeSetupImportKey:
            case TermGroup_ScheduledJobSettingType.BridgeSetupImportSettings:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialConnectionString:
            case TermGroup_ScheduledJobSettingType.BridgeCredentialToken:
            case TermGroup_ScheduledJobSettingType.BridgeFileInformationMatchExpression:
            case TermGroup_ScheduledJobSettingType.TimeAccumulator_AdjustCurrentBalanceDate:
                this.dataType = SettingDataType.String;
                break;
            case TermGroup_ScheduledJobSettingType.BridgeJobType:
            case TermGroup_ScheduledJobSettingType.ExportId:
            case TermGroup_ScheduledJobSettingType.BridgeSetupCallBackTimeOutInSeconds:
            case TermGroup_ScheduledJobSettingType.BridgeJobFileType:
            case TermGroup_ScheduledJobSettingType.BridgeFileInformationDefinitionId:
            case TermGroup_ScheduledJobSettingType.BridgeFileInformationImportHeadId:
            case TermGroup_ScheduledJobSettingType.EventActivationType:
            case TermGroup_ScheduledJobSettingType.SpecifiedType:
                this.dataType = SettingDataType.Integer;
                break;
            case TermGroup_ScheduledJobSettingType.BridgeJob:
            case TermGroup_ScheduledJobSettingType.BridgeJobRunChildJobs:
            case TermGroup_ScheduledJobSettingType.BridgeMergeFileWithPrevious:

            case TermGroup_ScheduledJobSettingType.ExportIsPreliminary:
            case TermGroup_ScheduledJobSettingType.ExportLock:
            case TermGroup_ScheduledJobSettingType.TimeAccumulator_AdjustCurrentBalance:
            case TermGroup_ScheduledJobSettingType.TimeAccumulator_SendToExecutive:
            case TermGroup_ScheduledJobSettingType.TimeAccumulator_SendToUser:
            case TermGroup_ScheduledJobSettingType.TimeAccumulator_IncludeFutureMonth:
                this.dataType = SettingDataType.Boolean;
                break;
            default:
                this.dataType = SettingDataType.String;
                break;
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
                if (this.options && this.options.length)
                    this.value = this.options.find(o => o.id === this.intData)?.name || this.intData || 0;
                else
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