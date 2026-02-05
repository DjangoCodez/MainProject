import { IMessageDTO, IMessageRecipientDTO, IMessageAttachmentDTO, IMessageEditDTO, IMessageGroupDTO, IMessageGroupMemberDTO, IMessageGridDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_MessageType, XEMailAnswerType, TermGroup_EmployeeRequestType, TermGroup_EmployeeRequestTypeFlags, XEMailRecipientType, SoeEntityType, TermGroup_MessageDeliveryType, TermGroup_MessagePriority, TermGroup_MessageTextType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class MessageDTO implements IMessageDTO {
    actorCompanyId: number;
    answerDate: Date;
    created: Date;
    deletedDate: Date;
    expirationDate: Date;
    hasAttachment: boolean;
    hasBeenConfirmed: string;
    hasBeenRead: string;
    isExpired: boolean;
    isHandledByJob: boolean;
    isSelected: boolean;
    isUnRead: boolean;
    isVisible: boolean;
    messageId: number;
    messageTextId: number;
    messageType: TermGroup_MessageType;
    needsConfirmation: boolean;
    needsConfirmationAnswer: boolean;
    priority: number;
    readDate: Date;
    recieversName: string;
    recipientList: IMessageRecipientDTO[];
    senderName: string;
    sentDate: Date;
    subject: string;

    public fixDates() {
        this.answerDate = CalendarUtility.convertToDate(this.answerDate);
        this.created = CalendarUtility.convertToDate(this.created);
        this.deletedDate = CalendarUtility.convertToDate(this.deletedDate);
        this.expirationDate = CalendarUtility.convertToDate(this.expirationDate);
        this.readDate = CalendarUtility.convertToDate(this.readDate);
        this.sentDate = CalendarUtility.convertToDate(this.sentDate);
    }
}

export class MessageGridDTO implements IMessageGridDTO {
    answerDate: Date;
    created: Date;
    deletedDate: Date;
    firstTextRow: string;
    forwardDate: Date;
    hasAttachment: boolean;
    hasBeenConfirmed: string;
    hasBeenRead: string;
    messageId: number;
    messageType: TermGroup_MessageType;
    needsConfirmation: boolean;
    readDate: Date;
    recieversName: string;
    replyDate: Date;
    senderName: string;
    sentDate: Date;
    subject: string;

    // Extensions
    type: number;
    confirmationIconValue: string;
    confirmationIconMessage: string;

    public fixDates() {
        this.answerDate = CalendarUtility.convertToDate(this.answerDate);
        this.created = CalendarUtility.convertToDate(this.created);
        this.deletedDate = CalendarUtility.convertToDate(this.deletedDate);
        this.readDate = CalendarUtility.convertToDate(this.readDate);
        this.sentDate = CalendarUtility.convertToDate(this.sentDate);
        this.replyDate = CalendarUtility.convertToDate(this.replyDate);
        this.forwardDate = CalendarUtility.convertToDate(this.forwardDate);
    }

    public get answerDateFormatted(): string {
        return this.answerDate ? this.answerDate.toFormattedDateTime() : '';
    }

    public get createdFormatted(): string {
        return this.created ? this.created.toFormattedDateTime() : '';
    }

    public get deletedDateFormatted(): string {
        return this.deletedDate ? this.deletedDate.toFormattedDateTime() : '';
    }

    public get readDateFormatted(): string {
        return this.readDate ? this.readDate.toFormattedDateTime() : '';
    }

    public get sentDateFormatted(): string {
        return this.sentDate ? this.sentDate.toFormattedDateTime() : '';
    }

    public get replyDateFormatted(): string {
        return this.replyDate ? this.replyDate.toFormattedDateTime() : '';
    }

    public get forwardDateFormatted(): string {
        return this.forwardDate ? this.forwardDate.toFormattedDateTime() : '';
    }
}

export class MessageAttachmentDTO implements IMessageAttachmentDTO {
    data: number[];
    dataStorageId: number;
    filesize: number;
    isUploadedAsImage: boolean;
    messageAttachmentId: number;
    name: string;

    // Extensions
    icon: string;
    fileFormat: string;

    public get description(): string {
        return this.name;
    }
    public set description(value: string) {
        this.name = value;
    }

    public setFileFormat() {
        let parts: string[] = this.name ? this.name.split('.') : [];
        if (parts.length > 0)
            this.fileFormat = parts[parts.length - 1];
    }

    public setIcon() {
        if (this.isWordDocument())
            this.icon = "fal fa-file-word";
        else if (this.isExcelDocument())
            this.icon = "fal fa-file-excel";
        else if (this.isTextDocument())
            this.icon = "fal fa-file-alt";
        else if (this.isPdfDocument())
            this.icon = "fal fa-file-pdf";
        else if (this.isImage())
            this.icon = "fal fa-file-image";
        else
            this.icon = "fal fa-file";
    }

    public isWordDocument(): boolean {
        return this.isExtension("docx") || this.isExtension("doc");
    }

    public isExcelDocument(): boolean {
        return this.isExtension("xlsx") || this.isExtension("xls") || this.isExtension("csv");
    }

    public isTextDocument(): boolean {
        return this.isExtension("txt");
    }

    public isPdfDocument(): boolean {
        return this.isExtension("pdf");
    }

    public isImage(): boolean {
        return this.isExtension("jpg") || this.isExtension("jpeg") || this.isExtension("png") || this.isExtension("bmp") || this.isExtension("gif");
    }

    public isMiscDocument(): boolean {
        return !this.isWordDocument && !this.isExcelDocument && !this.isTextDocument && !this.isPdfDocument && !this.isImage;
    }

    private isExtension(extension: string): boolean {
        return this.name && this.name.endsWithCaseInsensitive(extension);
    }
}

export class MessageRecipientDTO implements IMessageRecipientDTO {
    answerDate: Date;
    answerType: XEMailAnswerType;
    createdById: number;
    deletedDate: Date;
    emailAddress: string;
    employeeRequestType: TermGroup_EmployeeRequestType;
    employeeRequestTypeFlags: TermGroup_EmployeeRequestTypeFlags;
    forwardDate: Date;
    externalId: number;
    isCC: boolean;
    isSelected: boolean;
    isVisible: boolean;
    name: string;
    readDate: Date;
    recipientId: number;
    replyDate: Date;
    sendCopyAsEmail: boolean;
    signeeKey: string;
    type: XEMailRecipientType;
    userId: number;
    userName: string;

    public fixDates() {
        this.answerDate = CalendarUtility.convertToDate(this.answerDate);
        this.deletedDate = CalendarUtility.convertToDate(this.deletedDate);
        this.readDate = CalendarUtility.convertToDate(this.readDate);
        this.replyDate = CalendarUtility.convertToDate(this.replyDate);
        this.forwardDate = CalendarUtility.convertToDate(this.forwardDate);
    }
}

export class MessageEditDTO implements IMessageEditDTO {
    absenceRequestEmployeeId: number;
    absenceRequestEmployeeUserId: number;
    actorCompanyId: number;
    answerType: XEMailAnswerType;
    attachments: MessageAttachmentDTO[];
    copyToSMS: boolean;
    created: Date;
    deletedDate: Date;
    entity: SoeEntityType;
    expirationDate: Date;
    forceSendToReceiver: boolean;
    forwardDate: Date;
    licenseId: number;
    markAsOutgoing: boolean;
    messageDeliveryType: TermGroup_MessageDeliveryType;
    messageId: number;
    messagePriority: TermGroup_MessagePriority;
    messageTextId: number;
    messageTextType: TermGroup_MessageTextType;
    messageType: TermGroup_MessageType;
    parentId: number;
    recievers: IMessageRecipientDTO[];
    recordId: number;
    replyDate: Date;
    roleId: number;
    senderEmail: string;
    senderName: string;
    senderUserId: number;
    sentDate: Date;
    shortText: string;
    subject: string;
    text: string;

    public fixDates() {
        this.created = CalendarUtility.convertToDate(this.created);
        this.deletedDate = CalendarUtility.convertToDate(this.deletedDate);
        this.expirationDate = CalendarUtility.convertToDate(this.expirationDate);
        this.sentDate = CalendarUtility.convertToDate(this.sentDate);
        this.replyDate = CalendarUtility.convertToDate(this.replyDate);
        this.forwardDate = CalendarUtility.convertToDate(this.forwardDate);
    }
}

export class MessageGroupDTO implements IMessageGroupDTO {
    actorCompanyId: number;
    description: string;
    groupMembers: MessageGroupMemberDTO[];
    isPublic: boolean;
    licenseId: number;
    messageGroupId: number;
    name: string;
    noUserValidation: boolean;
    userId: number;
}

export class MessageGroupMemberDTO implements IMessageGroupMemberDTO {
    entity: SoeEntityType;
    messageGroupId: number;
    name: string;
    recordId: number;
    username: string;

    // Extensions
    type: string;
}

