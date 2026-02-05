import { ISearchVoucherRowsAngDTO } from '@shared/models/generated-interfaces/SearchVoucherRowDTO';
export class SearchVoucherFilterDTO implements ISearchVoucherRowsAngDTO {
  actorCompanyId!: number;
  voucherDateFrom?: Date;
  voucherDateTo?: Date;
  voucherSeriesIdFrom!: number;
  voucherSeriesIdTo!: number;
  debitFrom!: number;
  debitTo!: number;
  creditFrom!: number;
  creditTo!: number;
  amountFrom!: number;
  amountTo!: number;
  voucherText!: string;
  createdFrom?: Date;
  createdTo?: Date;
  createdBy!: string;
  dim1AccountId!: number;
  dim1AccountFr!: string;
  dim1AccountTo!: string;
  dim2AccountId!: number;
  dim2AccountFr!: string;
  dim2AccountTo!: string;
  dim3AccountId!: number;
  dim3AccountFr!: string;
  dim3AccountTo!: string;
  dim4AccountId!: number;
  dim4AccountFr!: string;
  dim4AccountTo!: string;
  dim5AccountId!: number;
  dim5AccountFr!: string;
  dim5AccountTo!: string;
  dim6AccountId!: number;
  dim6AccountFr!: string;
  dim6AccountTo!: string;
  dim7AccountId!: number;
  dim7AccountFr!: string;
  dim7AccountTo!: string;

  voucherSeriesTypeIds: number[] = [];
  isCredit!: boolean;
  isDebit!: boolean;
}

export class VoucherSearchSummaryDTO {
  creditTotal: number = 0;
  debitTotal: number = 0;
  balance: number = 0;
  creditTotalSelected: number = 0;
  debitTotalSelected: number = 0;
  balanceSelected: number = 0;
}
