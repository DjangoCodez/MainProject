import { AccountDTO } from '@shared/models/account.model';
import {
  ISaveAccountYearBalanceModel,
  ISaveAccountYearModel,
} from '@shared/models/generated-interfaces/EconomyModels';
import { TermGroup_AccountStatus } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountPeriodDTO,
  IAccountYearBalanceFlatDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IVoucherSeriesDTO } from '@shared/models/generated-interfaces/VoucherSeriesDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class AccountYearDTO implements IAccountYearDTO {
  accountYearId: number;
  actorCompanyId: number;
  from: Date;
  to: Date;
  status: TermGroup_AccountStatus;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  periods: AccountPeriodDTO[];
  noOfPeriods: number;
  statusText: string;
  yearFromTo: string;

  statusIcon!: string;
  keepNumberSeries!: boolean;

  constructor() {
    this.accountYearId = 0;
    this.actorCompanyId = 0;
    this.from = new Date();
    this.to = new Date();
    this.status = TermGroup_AccountStatus.New;
    this.periods = [];
    this.noOfPeriods = 0;
    this.statusText = '';
    this.yearFromTo = '';
  }
}

export class AccountPeriodDTO implements IAccountPeriodDTO {
  accountPeriodId: number;
  accountYearId: number;
  periodNr: number;
  from: Date;
  to: Date;
  status: TermGroup_AccountStatus;
  startValue: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  isDeleted: boolean;
  hasExistingVouchers: boolean;

  // External
  statusName!: string;
  monthName!: string;
  statusIcon!: string;
  periodName!: string;
  isModified!: boolean;

  constructor() {
    this.accountPeriodId = 0;
    this.accountYearId = 0;
    this.periodNr = 0;
    this.from = new Date();
    this.to = new Date();
    this.status = TermGroup_AccountStatus.New;
    this.isDeleted = false;
    this.startValue = '';
    this.hasExistingVouchers = false;
  }
}

export class AccountPeriodGridDTO {
  periodStatus: TermGroup_AccountStatus;

  constructor() {
    this.periodStatus = TermGroup_AccountStatus.New;
  }
}

export class VoucherSeriesDTO implements IVoucherSeriesDTO {
  voucherSeriesId: number;
  voucherSeriesTypeId: number;
  accountYearId: number;
  voucherNrLatest?: number;
  voucherDateLatest?: Date;
  status?: TermGroup_AccountStatus | undefined;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  voucherSeriesTypeName: string;
  voucherSeriesTypeNumberName: string;
  voucherSeriesTypeIsTemplate: boolean;
  isModified: boolean;
  isDeleted: boolean;
  voucherSeriesTypeNr: number;

  // External
  startNr!: number;

  constructor() {
    this.voucherSeriesId = 0;
    this.voucherSeriesTypeId = 0;
    this.accountYearId = 0;
    this.voucherSeriesTypeName = '';
    this.voucherSeriesTypeNumberName = '';
    this.voucherSeriesTypeIsTemplate = false;
    this.isModified = false;
    this.isDeleted = false;
    this.voucherSeriesTypeNr = 0;
  }
}

export class AccountYearBalanceFlatDTO implements IAccountYearBalanceFlatDTO {
  accountYearBalanceHeadId: number;
  accountYearId: number;
  balance: number;
  balanceEntCurrency: number;
  quantity?: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  dim1Id: number;
  dim1Nr: string;
  dim1Name: string;
  dim1TypeName: string;
  dim2Id: number;
  dim2Nr: string;
  dim2Name: string;
  dim3Id: number;
  dim3Nr: string;
  dim3Name: string;
  dim4Id: number;
  dim4Nr: string;
  dim4Name: string;
  dim5Id: number;
  dim5Nr: string;
  dim5Name: string;
  dim6Id: number;
  dim6Nr: string;
  dim6Name: string;
  debitAmount: number;
  creditAmount: number;
  rowNr: number;
  isModified: boolean;
  isDeleted: boolean;
  isDiffRow: boolean;

  numberName!: string;

  constructor() {
    this.accountYearBalanceHeadId = 0;
    this.accountYearId = 0;
    this.balance = 0;
    this.balanceEntCurrency = 0;
    this.dim1Id = 0;
    this.dim1Nr = '';
    this.dim1Name = '';
    this.dim1TypeName = '';
    this.dim2Id = 0;
    this.dim2Nr = '';
    this.dim2Name = '';
    this.dim3Id = 0;
    this.dim3Nr = '';
    this.dim3Name = '';
    this.dim4Id = 0;
    this.dim4Nr = '';
    this.dim4Name = '';
    this.dim5Id = 0;
    this.dim5Nr = '';
    this.dim5Name = '';
    this.dim6Id = 0;
    this.dim6Nr = '';
    this.dim6Name = '';
    this.debitAmount = 0;
    this.creditAmount = 0;
    this.rowNr = 0;
    this.isModified = false;
    this.isDeleted = false;
    this.isDiffRow = false;
  }
}

export class SaveAccountYearBalanceModel
  implements ISaveAccountYearBalanceModel
{
  accountYearId!: number;
  items!: IAccountYearBalanceFlatDTO[];
}

export class SaveAccountYearModel implements ISaveAccountYearModel {
  accountYear!: AccountYearDTO;
  voucherSeries!: VoucherSeriesDTO[];
  keepNumbers!: boolean;
}

export class SetAccountDialogData implements DialogData {
  size?: DialogSize | undefined;
  title!: string;
  content?: string | undefined;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  accounts!: AccountDTO[];
  amount!: number;
}
