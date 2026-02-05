import { IDataStorageRecipientDTO, IDocumentDTO, IDataStorageRecordDTO, IDataStorageDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityType, SoeDataStorageRecordType, SoeEntityState, XEMailAnswerType, TermGroup_DataStorageRecordAttestStatus } from "../../Util/CommonEnumerations";

export class DocumentDTO implements IDocumentDTO {
    answerDate: Date;
    answerType: XEMailAnswerType;
    created: Date;
    createdBy: string;
    dataStorageId: number;
    description: string;
    displayName: string;
    extension: string;
    fileName: string;
    fileSize: number;
    folder: string;
    messageGroupIds: number[];
    messageId: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    needsConfirmation: boolean;
    readDate: Date;
    recipients: DataStorageRecipientDTO[];
    records: DataStorageRecordDTO[];
    userId: number;
    validFrom: Date;
    validTo: Date;

    public fixDates() {
        this.answerDate = CalendarUtility.convertToDate(this.answerDate);
        this.created = CalendarUtility.convertToDate(this.created);
        this.validFrom = CalendarUtility.convertToDate(this.validFrom);
        this.validTo = CalendarUtility.convertToDate(this.validTo);
    }

    public setTypes() {
        if (this.recipients) {
            this.recipients = this.recipients.map(r => {
                let obj = new DataStorageRecipientDTO();
                angular.extend(obj, r);
                obj.fixDates();
                return obj;
            });
        } else {
            this.recipients = [];
        }

        if (this.records) {
            this.records = this.records.map(r => {
                let obj = new DataStorageRecordDTO();
                angular.extend(obj, r);
                return obj;
            });
        } else {
            this.records = [];
        }
    }

    public get isPdf(): boolean {
        return this.extension.endsWithCaseInsensitive('pdf');
    }

    public get isImage(): boolean {
        return this.extension.endsWithCaseInsensitive('jpg') || this.extension.endsWithCaseInsensitive('png');
    }
}

export class DataStorageDTO implements IDataStorageDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    createdOrModified: Date;
    data: number[];
    dataStorageRecipients: DataStorageRecipientDTO[];
    dataStorageRecords: DataStorageRecordDTO[];
    dataStorageId: number;
    description: string;
    downloadURL: string;
    employeeId: number;
    exportDate: Date;
    extension: string;
    fileName: string;
    fileSize: number;
    folder: string;
    information: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    needsConfirmation: boolean;
    originType: any;
    parentDataStorageId: number;
    seqNr: number;
    state: SoeEntityState;
    timePeriodId: number;
    type: SoeDataStorageRecordType;
    userId: number;
    validFrom: Date;
    validTo: Date;
    xml: string;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.createdOrModified = CalendarUtility.convertToDate(this.createdOrModified);
        this.exportDate = CalendarUtility.convertToDate(this.exportDate);
        this.modified = CalendarUtility.convertToDate(this.modified);
    }
}

export class DataStorageRecipientDTO implements IDataStorageRecipientDTO {
    confirmedDate: Date;
    dataStorageId: number;
    dataStorageRecipientId: number;
    employeeNrAndName: string;
    readDate: Date;
    state: SoeEntityState;
    userId: number;
    userName: string;

    public fixDates() {
        this.confirmedDate = CalendarUtility.convertToDate(this.confirmedDate);
        this.readDate = CalendarUtility.convertToDate(this.readDate);
    }
}

export class DataStorageRecordDTO implements IDataStorageRecordDTO {
    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    attestStatus: TermGroup_DataStorageRecordAttestStatus;
    currentAttestUsers: string;
    data: number[];
    dataStorageRecordId: number;
    entity: SoeEntityType;
    recordId: number;
    roleIds: number[];
    type: SoeDataStorageRecordType;

    // Extensions
    attestStatusText: string;
}

export class DocumentFolder {
    name: string;
    expanded: boolean;
    nbrOfUnread: number;

    constructor(name: string) {
        this.name = name;
        this.expanded = false;
    }
}