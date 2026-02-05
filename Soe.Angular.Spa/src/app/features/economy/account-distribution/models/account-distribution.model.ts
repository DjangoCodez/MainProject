import {
  TermGroup_AccountDistributionTriggerType,
  TermGroup_AccountDistributionCalculationType,
  TermGroup_AccountDistributionPeriodType,
  SoeEntityState,
  WildCard,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDistributionHeadDTO,
  IAccountDistributionHeadSmallDTO,
  IAccountDistributionRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AccountDistributionGridFilterDTO {
  showOpen!: boolean;
  showClosed!: boolean;
}

export class AccountDistributionHeadDTO implements IAccountDistributionHeadDTO {
  accountDistributionHeadId: number;
  actorCompanyId: number;
  voucherSeriesTypeId?: number;
  type: number;
  name: string;
  description: string;
  triggerType: TermGroup_AccountDistributionTriggerType;
  calculationType: TermGroup_AccountDistributionCalculationType;
  calculate: number;
  periodType: TermGroup_AccountDistributionPeriodType;
  periodValue: number;
  sort: number;
  startDate?: Date;
  endDate?: Date;
  dayNumber: number;
  amount: number;
  amountOperator: number;
  keepRow: boolean;
  useInVoucher: boolean;
  useInSupplierInvoice: boolean;
  useInCustomerInvoice: boolean;
  useInImport: boolean;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState;
  dim1Id: number;
  dim1Expression: string;
  dim2Id: number;
  dim2Expression: string;
  dim3Id: number;
  dim3Expression: string;
  dim4Id: number;
  dim4Expression: string;
  dim5Id: number;
  dim5Expression: string;
  dim6Id: number;
  dim6Expression: string;
  rows: AccountDistributionRowDTO[];
  useInPayrollVacationVoucher: boolean;
  useInPayrollVoucher: boolean;

  // GridFilter
  showOpen!: boolean;
  showClosed!: boolean;

  constructor() {
    this.accountDistributionHeadId = 0;
    this.actorCompanyId = 0;
    this.type = 0;
    this.name = '';
    this.description = '';
    this.triggerType = TermGroup_AccountDistributionTriggerType.Registration;
    this.calculationType = TermGroup_AccountDistributionCalculationType.Percent;
    this.calculate = 0;
    this.periodType = TermGroup_AccountDistributionPeriodType.Unknown;
    this.periodValue = 0;
    this.sort = 0;
    this.dayNumber = 0;
    this.amount = 0;
    this.amountOperator = 0;
    this.keepRow = false;
    this.useInVoucher = false;
    this.useInSupplierInvoice = false;
    this.useInCustomerInvoice = false;
    this.useInImport = false;
    this.state = SoeEntityState.Active;
    this.dim1Id = 0;
    this.dim1Expression = '';
    this.dim2Id = 0;
    this.dim2Expression = '';
    this.dim3Id = 0;
    this.dim3Expression = '';
    this.dim4Id = 0;
    this.dim4Expression = '';
    this.dim5Id = 0;
    this.dim5Expression = '';
    this.dim6Id = 0;
    this.dim6Expression = '';
    this.rows = [];
    this.useInPayrollVacationVoucher = false;
    this.useInPayrollVoucher = false;
  }
}

export class AccountDistributionHeadSmallDTO
  implements IAccountDistributionHeadSmallDTO
{
  accountDistributionHeadId: number;
  type: number;
  name: string;
  description: string;
  calculationType: TermGroup_AccountDistributionCalculationType;
  triggerType: TermGroup_AccountDistributionTriggerType;
  periodValue: number;
  startDate?: Date;
  endDate?: Date;
  dayNumber: number;
  amount: number;
  amountOperator: number;
  keepRow: boolean;
  sort: number;
  dim1Expression: string;
  dim2Expression: string;
  dim3Expression: string;
  dim4Expression: string;
  dim5Expression: string;
  dim6Expression: string;
  entryTotalCount!: number;
  entryTransferredCount!: number;
  entryLatestTransferDate?: Date;
  entryTotalAmount!: number;
  entryTransferredAmount!: number;
  entryPeriodAmount?: number;
  useInVoucher: boolean;
  useInImport: boolean;

  entryRemainingCount!: number;
  entryRemainingAmount!: number;

  constructor() {
    this.accountDistributionHeadId = 0;
    this.type = 0;
    this.name = '';
    this.description = '';
    this.triggerType = TermGroup_AccountDistributionTriggerType.Registration;
    this.calculationType = TermGroup_AccountDistributionCalculationType.Percent;
    this.periodValue = 0;
    this.sort = 0;
    this.dayNumber = 0;
    this.amount = 0;
    this.amountOperator = WildCard.Equals;
    this.keepRow = false;
    this.useInVoucher = false;
    this.useInImport = false;
    this.dim1Expression = '';
    this.dim2Expression = '';
    this.dim3Expression = '';
    this.dim4Expression = '';
    this.dim5Expression = '';
    this.dim6Expression = '';
  }
}

export class AccountDistributionRowDTO implements IAccountDistributionRowDTO {
  accountDistributionRowId: number;
  accountDistributionHeadId: number;
  rowNbr: number;
  calculateRowNbr: number;
  sameBalance: number;
  oppositeBalance: number;
  description: string;
  state: SoeEntityState;
  dim1Id?: number;
  dim1Nr: string;
  dim1Name: string;
  dim1Disabled: boolean;
  dim1Mandatory: boolean;
  previousRowNbr: number;
  dim2Id: number;
  dim2Nr: string;
  dim2Name: string;
  dim2Disabled: boolean;
  dim2Mandatory: boolean;
  dim2KeepSourceRowAccount: boolean;
  dim3Id: number;
  dim3Nr: string;
  dim3Name: string;
  dim3Disabled: boolean;
  dim3Mandatory: boolean;
  dim3KeepSourceRowAccount: boolean;
  dim4Id: number;
  dim4Nr: string;
  dim4Name: string;
  dim4Disabled: boolean;
  dim4Mandatory: boolean;
  dim4KeepSourceRowAccount: boolean;
  dim5Id: number;
  dim5Nr: string;
  dim5Name: string;
  dim5Disabled: boolean;
  dim5Mandatory: boolean;
  dim5KeepSourceRowAccount: boolean;
  dim6Id: number;
  dim6Nr: string;
  dim6Name: string;
  dim6Disabled: boolean;
  dim6Mandatory: boolean;
  dim6KeepSourceRowAccount: boolean;

  numberName!: string;
  selectOptions!: any[];

  constructor() {
    this.accountDistributionRowId = 0;
    this.accountDistributionHeadId = 0;
    this.rowNbr = 0;
    this.calculateRowNbr = 0;
    this.sameBalance = 0;
    this.oppositeBalance = 0;
    this.description = '';
    this.state = SoeEntityState.Active;
    this.dim1Nr = '';
    this.dim1Name = '';
    this.dim1Disabled = false;
    this.dim1Mandatory = false;
    this.previousRowNbr = 0;
    this.dim2Id = 0;
    this.dim2Nr = '';
    this.dim2Name = '';
    this.dim2Disabled = false;
    this.dim2Mandatory = false;
    this.dim2KeepSourceRowAccount = false;
    this.dim3Id = 0;
    this.dim3Nr = '';
    this.dim3Name = '';
    this.dim3Disabled = false;
    this.dim3Mandatory = false;
    this.dim3KeepSourceRowAccount = false;
    this.dim4Id = 0;
    this.dim4Nr = '';
    this.dim4Name = '';
    this.dim4Disabled = false;
    this.dim4Mandatory = false;
    this.dim4KeepSourceRowAccount = false;
    this.dim5Id = 0;
    this.dim5Nr = '';
    this.dim5Name = '';
    this.dim5Disabled = false;
    this.dim5Mandatory = false;
    this.dim5KeepSourceRowAccount = false;
    this.dim6Id = 0;
    this.dim6Nr = '';
    this.dim6Name = '';
    this.dim6Disabled = false;
    this.dim6Mandatory = false;
    this.dim6KeepSourceRowAccount = false;
  }
}
