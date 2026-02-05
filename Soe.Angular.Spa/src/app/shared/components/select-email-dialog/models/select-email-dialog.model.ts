import {
  ImageFormatType,
  InvoiceAttachmentSourceType,
  SoeDataStorageRecordType,
  SoeEntityImageType,
  SoeEntityState,
  SoeModule,
  TermGroup_DataStorageRecordAttestStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IGetPurchasePrintUrlModel,
  IGetReportsForTypesModel,
} from '@shared/models/generated-interfaces/ReportModels';
import {
  IChecklistExtendedRowDTO,
  IChecklistHeadRecordCompactDTO,
  IEmailTemplateDTO,
  IImagesDTO,
  IReportViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SelectEmailDialogFormDTO {
  selectedLanguageId!: number;
  selectedReportId!: number;
  selectedTemplateId!: number;
  mergePdfs!: boolean;
  emailAddresses!: string;
}

export class EmailTemplateDTO implements IEmailTemplateDTO {
  emailTemplateId!: number;
  actorCompanyId!: number;
  type!: number;
  name!: string;
  subject!: string;
  body!: string;
  typename!: string;
  bodyIsHTML!: boolean;
  emailAddresses!: string[];
}

export class SelectEmailRecipientsDTO implements ISmallGenericType {
  id!: number;
  name!: string;
  isSelected!: boolean;

  constructor(id: number, name: string, isSelected: boolean) {
    this.id = id;
    this.name = name;
    this.isSelected = isSelected;
  }
}

export class SelectEmailAttachmentsDTO implements IImagesDTO {
  imageId!: number;
  invoiceAttachmentId?: number;
  formatType!: ImageFormatType;
  image!: number[];
  description!: string;
  fileName!: string;
  connectedTypeName!: string;
  created?: Date;
  type!: SoeEntityImageType;
  needsConfirmation!: boolean;
  includeWhenDistributed?: boolean;
  includeWhenTransfered?: boolean;
  dataStorageRecordType?: SoeDataStorageRecordType;
  sourceType!: InvoiceAttachmentSourceType;
  lastSentDate?: Date;
  confirmed!: boolean;
  confirmedDate?: Date;
  canDelete!: boolean;
  attestStateId?: number;
  attestStateName!: string;
  attestStateColor!: string;
  currentAttestUsers!: string;
  attestStatus!: TermGroup_DataStorageRecordAttestStatus;
  isSelected!: boolean;

  //extended properties
  fileRecordId!: number;
}
export class SelectEmailCheckListsDTO
  implements IChecklistHeadRecordCompactDTO
{
  tempHeadId!: string;
  checklistHeadRecordId!: number;
  checklistHeadId!: number;
  recordId!: number;
  checklistHeadName!: string;
  state!: SoeEntityState;
  created?: Date;
  rowNr!: number;
  addAttachementsToEInvoice!: boolean;
  checklistRowRecords!: IChecklistExtendedRowDTO[];
  isSelected!: boolean;
  signatures!: IImagesDTO[];
}

export class GetReportsForTypesModel implements IGetReportsForTypesModel {
  reportTemplateTypeIds!: number[];
  onlyOriginal!: boolean;
  onlyStandard!: boolean;
  module?: SoeModule = undefined;
  constructor(
    reportTemplateTypeIds: number[],
    onlyOriginal: boolean,
    onlyStandard: boolean,
    module?: SoeModule
  ) {
    this.reportTemplateTypeIds = reportTemplateTypeIds;
    this.onlyOriginal = onlyOriginal;
    this.onlyStandard = onlyStandard;
    this.module = module;
  }
}

export class GetPurchasePrintUrlModel implements IGetPurchasePrintUrlModel {
  purchaseIds!: number[];
  emailRecipients!: number[];
  reportId!: number;
  languageId!: number;
  constructor(
    purchaseIds: number[],
    emailRecipients: number[],
    reportId: number,
    languageId: number
  ) {
    this.purchaseIds = purchaseIds;
    this.emailRecipients = emailRecipients;
    this.reportId = reportId;
    this.languageId = languageId;
  }
}
export class ReportViewDTO implements IReportViewDTO {
  isSystemReport!: boolean;
  actorCompanyId!: number;
  reportId!: number;
  exportType!: number;
  reportName!: string;
  reportNr!: number;
  reportSelectionId?: number;
  reportDescription!: string;
  sysReportTemplateTypeId!: number;
  showInAccountingReports!: boolean;
  sysReportTypeName!: string;
  reportNameDesc!: string;
  default!: boolean;
  employeeTemplateId!: number;
}

export class SelectEmailDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;

  langId?: number;
  reports!: ISmallGenericType[];
  defaultEmailTemplateId!: number;
  defaultReportTemplateId?: number;
  defaultEmail?: any;
  type!: number;
  types!: any;
  showReportSelection!: boolean;
  recipients?: any[];
  attachments?: any[];
  attachmentsSelected?: boolean;
  checklists?: any[];
  grid!: boolean;
  hideTemplate?: boolean;
  isSendEmailDocuments?: boolean;
  showAddRecipient?: boolean;
}

export class SelectEmailDialogCloseData {
  emailTemplateId!: number;
  reportId!: number;
  languageId!: number;
  mergePdfs!: boolean;
  recipients!: ISmallGenericType[];
  attachments!: any[];
  checklists!: any[];
  emailAddresses?: string;
  constructor(
    emailTemplateId: number,
    reportId: number,
    languageId: number,
    mergePdfs: boolean,
    recipients: any[],
    attachments: any[],
    checklists: any[]
  ) {
    this.emailTemplateId = emailTemplateId;
    this.reportId = reportId;
    this.languageId = languageId;
    this.mergePdfs = mergePdfs;
    this.recipients = recipients;
    this.attachments = attachments;
    this.checklists = checklists;
  }
}
