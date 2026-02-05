import { ISaveAccountSmallModel } from '@shared/models/generated-interfaces/EconomyModels';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountEditDTO,
  IAccountInternalDTO,
  IAccountMappingDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISysAccountStdDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class AddAccountDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;

  accountNr: string = '';
}

export enum AddAccountDialogResultType {
  Copy = 'copy',
  New = 'new',
}
export class AddAccountDialogResultData {
  type: AddAccountDialogResultType = AddAccountDialogResultType.New;
  data!: ISysAccountStdDTO | AccountEditDTO;

  constructor(
    type: AddAccountDialogResultType,
    data: ISysAccountStdDTO | AccountEditDTO
  ) {
    this.type = type;
    this.data = data;
  }
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
  state!: SoeEntityState;
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
  excludeVatVerification?: boolean;
  rowTextStop!: boolean;
  accountMappings!: IAccountMappingDTO[];
  active!: boolean;
  isStdAccount!: boolean;
  accountInternals!: IAccountInternalDTO[];
}

export class SaveAccountSmallModel implements ISaveAccountSmallModel {
  accountNr!: string;
  name!: string;
  accountTypeId!: number;
  vatAccountId!: number;
  sruCode1Id!: number;
}
