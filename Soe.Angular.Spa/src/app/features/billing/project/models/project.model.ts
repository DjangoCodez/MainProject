import { IAccountingSettingDTO } from '@shared/models/generated-interfaces/AccountingSettingDTO';
import { ISaveInvoiceProjectModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  TermGroup_ProjectType,
  TermGroup_ProjectStatus,
  TermGroup_ProjectAllocationType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { IDecimalKeyValue } from '@shared/models/generated-interfaces/GenericType';
import {
  IProjectDTO,
  IProjectGridDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import { IProjectWeekTotal } from '@shared/models/generated-interfaces/ProjectWeekTotal';
import {
  IAccountingSettingsRowDTO,
  IAccountSmallDTO,
  IBudgetHeadDTO,
  ICompanyCategoryRecordDTO,
  IProjectUserDTO,
  ITimeProjectDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class TimeProjectDTO implements ITimeProjectDTO, IProjectDTO {
  projectId!: number;
  type!: TermGroup_ProjectType;
  actorCompanyId!: number;
  parentProjectId?: number | undefined;
  customerId?: number | undefined;
  status: TermGroup_ProjectStatus;
  allocationType!: TermGroup_ProjectAllocationType;
  invoiceId?: number | undefined;
  budgetId?: number | undefined;
  number!: string;
  name!: string;
  description!: string;
  startDate?: Date | undefined;
  stopDate?: Date | undefined;
  note!: string;
  useAccounting!: boolean;
  priceListTypeId?: number | undefined;
  created?: Date | undefined;
  createdBy!: string;
  modified?: Date | undefined;
  modifiedBy!: string;
  state!: SoeEntityState;
  workSiteKey!: string;
  workSiteNumber!: string;
  attestWorkFlowHeadId?: number | undefined;
  defaultDim1AccountId?: number | undefined;
  defaultDim2AccountId?: number | undefined;
  defaultDim3AccountId?: number | undefined;
  defaultDim4AccountId?: number | undefined;
  defaultDim5AccountId?: number | undefined;
  defaultDim6AccountId?: number | undefined;
  statusName!: string;
  creditAccounts!: Record<number, IAccountSmallDTO>;
  debitAccounts!: Record<number, IAccountSmallDTO>;
  salesNoVatAccounts!: Record<number, IAccountSmallDTO>;
  salesContractorAccounts!: Record<number, IAccountSmallDTO>;
  accountingSettings!: IAccountingSettingsRowDTO[];
  budgetHead!: IBudgetHeadDTO;
  payrollProductAccountingPrio!: string;
  invoiceProductAccountingPrio!: string;
  hasInvoices!: boolean;
  numberOfInvoices!: number;
  parentProjectNr!: string;
  parentProjectName!: string;
  orderTemplateId?: number | undefined;
  projectWeekTotals!: IProjectWeekTotal[];

  constructor(status: TermGroup_ProjectStatus) {
    this.status = status;
  }
}

// export class ProjectPriceListDTO implements IProductComparisonDTO, IProductSmallDTO {
//     productId!: number;
//     number!: string;
//     name!: string;
//     numberName!: string;
//     purchasePrice!: number;
//     comparisonPrice!: number;
//     price!: number;
//     startDate!: Date;
//     stopDate!: Date;

// }

export class SaveInvoiceProjectModel implements ISaveInvoiceProjectModel {
  invoiceProject!: TimeProjectDTO;
  priceLists!: IDecimalKeyValue[];
  categoryRecords!: ICompanyCategoryRecordDTO[];
  accountSettings!: IAccountingSettingDTO[];
  projectUsers!: IProjectUserDTO[];
  newPricelist!: boolean;
  pricelistName!: string;
}

export interface ProjectExtendedGridDTO extends IProjectGridDTO {
  categoriesArray: string[];
}

export interface IProjectUserExDTO extends IProjectUserDTO {
  isModified: boolean;
}
