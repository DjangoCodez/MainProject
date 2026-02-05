import { ICompanyGroupTransferModel } from '@shared/models/generated-interfaces/EconomyModels';
import {
  CompanyGroupTransferType,
  CompanyGroupTransferStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ICompanyGroupTransferHeadDTO,
  ICompanyGroupTransferRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CompanyGroupTransfer {}

export class CompanyGroupTransferHeadDTO
  implements ICompanyGroupTransferHeadDTO
{
  companyGroupTransferHeadId?: number;
  actorCompanyId!: number;
  accountYearId?: number;
  accountYearText: string;
  fromAccountPeriodId?: number;
  fromAccountPeriodText: string;
  toAccountPeriodId?: number;
  toAccountPeriodText: string;
  transferType: CompanyGroupTransferType;
  transferTypeName: string;
  transferStatus: CompanyGroupTransferStatus;
  transferStatusName: string;
  transferDate?: Date;
  isOnlyVoucher: boolean;
  companyGroupTransferRows: ICompanyGroupTransferRowDTO[];

  //Extends
  voucherSeriesId: number | undefined;
  childCompanyId: number | undefined;
  childBudgetId: number | undefined;
  masterBudgetId: number | undefined;

  constructor() {
    this.accountYearText = '';
    this.fromAccountPeriodText = '';
    this.toAccountPeriodText = '';
    this.transferType = CompanyGroupTransferType.None;
    this.transferTypeName = '';
    this.transferStatus = CompanyGroupTransferStatus.None;
    this.transferStatusName = '';
    this.isOnlyVoucher = false;
    this.companyGroupTransferRows = [];
  }
}

export class SaveTransferModel implements ICompanyGroupTransferModel {
  accountYearId!: number;
  budgetChild!: number;
  budgetCompanyGroup: number | undefined;
  includeIB: boolean = false;
  periodFrom!: number;
  periodTo!: number;
  transferType!: CompanyGroupTransferType;
  voucherSeriesId!: number;
}
