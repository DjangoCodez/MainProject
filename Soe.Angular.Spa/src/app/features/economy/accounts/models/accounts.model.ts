import { ISaveAccountModel } from '@shared/models/generated-interfaces/EconomyModels';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import {
  IAccountEditDTO,
  IAccountGridDTO,
  IAccountInternalDTO,
  IAccountMappingDTO,
  ICategoryAccountDTO,
  ICompTermDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AccountsGridDTO implements IAccountGridDTO {
  accountId!: number;
  accountDimId!: number;
  accountNr!: string;
  name!: string;
  accountTypeSysTermId?: number;
  type!: string;
  sysVatAccountId?: number;
  vatType!: string;
  externalCode!: string;
  parentAccountName!: string;
  balance!: number;
  state: SoeEntityState = SoeEntityState.Active;
  isLinkedToShiftType!: boolean;
  categories!: string;
  isActive?: boolean | undefined;
}

export class AccountEditDTO implements IAccountEditDTO {
  accountId!: number;
  accountDimId!: number;
  accountNr!: string;
  name!: string;
  description!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = SoeEntityState.Active;
  attestWorkFlowHeadId?: number;
  parentAccountId?: number;
  useVatDeductionDim!: boolean;
  useVatDeduction!: boolean;
  vatDeduction!: number;
  externalCode!: string;
  accountHierachyPayrollExportExternalCode!: string;
  accountHierachyPayrollExportUnitExternalCode!: string;
  hierarchyOnly!: boolean;
  isAccrualAccount!: boolean;
  hierarchyNotOnSchedule!: boolean;
  sysAccountSruCode1Id?: number;
  sysAccountSruCode2Id?: number;
  sysVatAccountId?: number;
  accountTypeSysTermId!: number;
  amountStop!: number;
  unit!: string;
  unitStop!: boolean;
  sieKpTyp!: string;
  excludeVatVerification?: boolean | undefined;
  rowTextStop!: boolean;
  accountMappings!: IAccountMappingDTO[];
  active!: boolean;
  isStdAccount!: boolean;
  accountInternals!: IAccountInternalDTO[];
  translations: ICompTermDTO[];
  categoryIds: number[];
  sysVatRate?: string;

  constructor() {
    this.categoryIds = [];
    this.translations = [];
  }
}

export class CategoryAccountDTO implements ICategoryAccountDTO {
  categoryAccountId!: number;
  categoryId!: number;
  accountId!: number;
  actorCompanyId: number = 0;
  dateFrom?: Date;
  dateTo?: Date;
  state: SoeEntityState = SoeEntityState.Active;
}

export class SaveAccountModel implements ISaveAccountModel {
  account!: IAccountEditDTO;
  translations!: ICompTermDTO[];
  accountMappings!: IAccountMappingDTO[];
  categoryAccounts!: ICategoryAccountDTO[];
  extraFields!: IExtraFieldRecordDTO[];
  skipStateValidation?: boolean;
}
