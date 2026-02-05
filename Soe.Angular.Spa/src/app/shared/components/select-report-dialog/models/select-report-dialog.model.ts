import {
  SoeModule,
  SoeReportTemplateType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IGetPurchasePrintUrlModel,
  IGetReportsForTypesModel,
} from '@shared/models/generated-interfaces/ReportModels';
import { IReportViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SelectReportDialogFormDTO {
  languageId!: number;
  isReportCopy!: boolean;
  isReminder!: boolean;
  savePrintout!: boolean;
}

export class GetReportsForTypesModel implements IGetReportsForTypesModel {
  reportTemplateTypeIds!: number[];
  onlyOriginal!: boolean;
  onlyStandard!: boolean;
  module?: SoeModule;
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

export class SelectReportDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;

  langId?: number;
  reports!: ReportViewDTO[];
  module?: SoeModule;
  reportTypes!: SoeReportTemplateType[];
  defaultReportId!: number;
  showCopy!: boolean;
  showEmail!: boolean;
  copyValue!: boolean;

  showReminder!: boolean;
  showLangSelection = true;
  showSavePrintout!: boolean;
  savePrintout!: boolean;
}

export class SelectReportDialogCloseData {
  reportId!: number;
  reportType!: number;
  languageId!: number;
  createCopy!: boolean;
  email!: boolean;
  reminder!: boolean;
  savePrintout!: boolean;
  employeeTemplateId!: number;
  constructor(
    reportId: number,
    reportType: number,
    languageId: number,
    createCopy: boolean,
    email: boolean,
    reminder: boolean,
    savePrintout: boolean,
    employeeTemplateId: number
  ) {
    this.reportId = reportId;
    this.reportType = reportType;
    this.languageId = languageId;
    this.createCopy = createCopy;
    this.email = email;
    this.reminder = reminder;
    this.savePrintout = savePrintout;
    this.employeeTemplateId = employeeTemplateId;
  }

}
