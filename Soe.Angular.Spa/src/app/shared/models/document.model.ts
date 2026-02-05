import {
  XEMailAnswerType,
  TermGroup_DataStorageRecordAttestStatus,
  SoeEntityState,
} from './generated-interfaces/Enumerations';
import {
  IDataStorageRecipientDTO,
  IDataStorageRecordDTO,
  IDocumentDTO,
} from './generated-interfaces/SOECompModelDTOs';

export class DocumentDTO implements IDocumentDTO {
  dataStorageId: number;
  messageId?: number;
  userId?: number;
  name: string;
  description: string;
  fileName: string;
  extension: string;
  fileSize?: number;
  folder: string;
  validFrom?: Date;
  validTo?: Date;
  readDate?: Date;
  confirmedDate?: Date;
  needsConfirmation: boolean;
  answerType: XEMailAnswerType;
  answerDate?: Date;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  records: IDataStorageRecordDTO[];
  recipients: DataStorageRecipientDTO[];
  messageGroupIds: number[];
  attestStatus: TermGroup_DataStorageRecordAttestStatus;
  currentAttestUsers: string;
  attestStateId?: number;
  displayName: string;

  constructor() {
    this.dataStorageId = 0;
    this.name = '';
    this.description = '';
    this.fileName = '';
    this.extension = '';
    this.folder = '';
    this.needsConfirmation = false;
    this.answerType = XEMailAnswerType.None;
    this.createdBy = '';
    this.modifiedBy = '';
    this.records = [];
    this.recipients = [];
    this.messageGroupIds = [];
    this.attestStatus = TermGroup_DataStorageRecordAttestStatus.None;
    this.currentAttestUsers = '';
    this.displayName = '';
  }

  get isPdf(): boolean {
    return this.extension.endsWithCaseInsensitive('pdf');
  }

  get isImage(): boolean {
    return (
      this.extension.endsWithCaseInsensitive('jpg') ||
      this.extension.endsWithCaseInsensitive('png')
    );
  }

  get canViewDocument(): boolean {
    return this.isPdf || this.isImage;
  }
}

export class SaveDocumentModel {
  document: DocumentDTO;
  fileData: string;

  constructor(document: DocumentDTO, fileData: string) {
    this.document = document;
    this.fileData = fileData;
  }
}

export class DocumentFolder {
  name: string;
  expanded: boolean;
  nbrOfUnread: number;

  constructor(name: string) {
    this.name = name;
    this.expanded = false;
    this.nbrOfUnread = 0;
  }
}

export class DataStorageRecipientDTO implements IDataStorageRecipientDTO {
  dataStorageRecipientId: number;
  dataStorageId: number;
  userId: number;
  readDate?: Date;
  confirmedDate?: Date;
  state: SoeEntityState;
  userName: string;
  employeeNrAndName: string;

  constructor() {
    this.dataStorageRecipientId = 0;
    this.dataStorageId = 0;
    this.userId = 0;
    this.state = SoeEntityState.Active;
    this.userName = '';
    this.employeeNrAndName = '';
  }
}
