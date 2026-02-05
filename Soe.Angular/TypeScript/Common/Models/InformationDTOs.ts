import { IInformationDTO, IInformationRecipientDTO, IInformationGridDTO, ISysInformationSysCompDbDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_InformationSeverity, SoeInformationType, TermGroup, XEMailAnswerType, SoeInformationSourceType, TermGroup_InformationStickyType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class InformationDTO implements IInformationDTO {
    actorCompanyId: number;
    answerDate: Date;
    answerType: XEMailAnswerType;
    created: Date;
    createdBy: string;
    createdOrModified: Date;
    displayDate: Date;
    folder: string;
    hasText: boolean;
    informationId: number;
    licenseId: number;
    messageGroupIds: number[];
    modified: Date;
    modifiedBy: string;
    needsConfirmation: boolean;
    notificationSent: Date;
    notify: boolean;
    readDate: Date;
    recipients: InformationRecipientDTO[];
    severity: TermGroup_InformationSeverity;
    severityName: string;
    shortText: string;
    showInMobile: boolean;
    showInTerminal: boolean;
    showInWeb: boolean;
    sourceType: SoeInformationSourceType;
    stickyType: TermGroup_InformationStickyType;
    subject: string;
    sysCompDbIds: number[];
    sysFeatureIds: number[];
    sysInformationSysCompDbs: ISysInformationSysCompDbDTO[];
    sysLanguageId: number;
    text: string;
    type: SoeInformationType;
    validFrom: Date;
    validTo: Date;

    public fixDates() {
        this.answerDate = CalendarUtility.convertToDate(this.answerDate);
        this.created = CalendarUtility.convertToDate(this.created);
        this.createdOrModified = CalendarUtility.convertToDate(this.createdOrModified);
        this.displayDate = CalendarUtility.convertToDate(this.displayDate);
        this.readDate = CalendarUtility.convertToDate(this.readDate);
        this.validFrom = CalendarUtility.convertToDate(this.validFrom);
        this.validTo = CalendarUtility.convertToDate(this.validTo);
        this.notificationSent = CalendarUtility.convertToDate(this.notificationSent);
    }

    public setTypes() {
        if (this.recipients) {
            this.recipients = this.recipients.map(r => {
                let obj = new InformationRecipientDTO();
                angular.extend(obj, r);
                obj.fixDates();
                return obj;
            });
        } else {
            this.recipients = [];
        }
    }

    public get isCompanyInformation(): boolean {
        return this.sourceType === SoeInformationSourceType.Company;
    }
    public get isSysInformation(): boolean {
        return this.sourceType === SoeInformationSourceType.Sys;
    }

    public get isSeverityInformation(): boolean {
        return this.severity === TermGroup_InformationSeverity.Information;
    }
    public get isSeverityImportant(): boolean {
        return this.severity === TermGroup_InformationSeverity.Important;
    }
    public get isSeverityEmergency(): boolean {
        return this.severity === TermGroup_InformationSeverity.Emergency;
    }

    public get displayDateString(): string {
        return this.displayDate.toFormattedDateTime();
    }

    public get allNotificationsSent(): boolean {
        // When all notifications are sent, notificationSent has a timestamp
        return !!this.notificationSent;
    }
    public get someNotificationsSent(): boolean {
        if (this.sourceType === SoeInformationSourceType.Company) {
            // For company informations there can only be none or all, never some
            return false;
        } else if (this.sourceType === SoeInformationSourceType.Sys) {
            // Some but not all notifications has been sent
            if (this.notificationSent)
                return false;

            return _.filter(this.sysInformationSysCompDbs, c => c.notificationSent).length > 0;
        } else {
            return false;
        }
    }
    public get noNotificationsSent(): boolean {
        if (this.sourceType === SoeInformationSourceType.Company) {
            return !this.notificationSent;
        } else if (this.sourceType === SoeInformationSourceType.Sys) {
            if (this.notificationSent)
                return false;

            return _.filter(this.sysInformationSysCompDbs, c => c.notificationSent).length === 0;
        } else {
            return true;
        }
    }
}

export class InformationGridDTO implements IInformationGridDTO {
    folder: string;
    informationId: number;
    needsConfirmation: boolean;
    notificationSent: Date;
    notify: boolean;
    severity: TermGroup_InformationSeverity;
    severityName: string;
    shortText: string;
    showInMobile: boolean;
    showInTerminal: boolean;
    showInWeb: boolean;
    subject: string;
    validFrom: Date;
    validTo: Date;

    public fixDates() {
        this.validFrom = CalendarUtility.convertToDate(this.validFrom);
        this.validTo = CalendarUtility.convertToDate(this.validTo);
        this.notificationSent = CalendarUtility.convertToDate(this.notificationSent);
    }
}

export class InformationRecipientDTO implements IInformationRecipientDTO {
    companyName: string;
    confirmedDate: Date;
    employeeNrAndName: string;
    hideDate: Date;
    informationId: number;
    informationRecipientId: number;
    readDate: Date;
    sysInformationId: number;
    userId: number;
    userName: string;

    public fixDates() {
        this.confirmedDate = CalendarUtility.convertToDate(this.confirmedDate);
        this.hideDate = CalendarUtility.convertToDate(this.hideDate);
        this.readDate = CalendarUtility.convertToDate(this.readDate);
    }
}

export class InformationFolder {
    name: string;
    expanded: boolean;
    nbrOfUnread: number;

    constructor(name: string) {
        this.name = name;
        this.expanded = false;
    }
}

export class SysInformationSysCompDbDTO implements ISysInformationSysCompDbDTO {
    notificationSent: Date;
    siteName: string;
    sysCompDbId: number;

    public fixDates() {
        this.notificationSent = CalendarUtility.convertToDate(this.notificationSent);
    }
}