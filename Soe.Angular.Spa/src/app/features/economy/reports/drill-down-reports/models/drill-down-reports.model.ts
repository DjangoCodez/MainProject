import { ISearchVoucherRowsAngDTO } from '@shared/models/generated-interfaces/SearchVoucherRowDTO';
import {
  IDrilldownReportGridFlattenedDTO,
  IReportViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export interface IDrillDownReportDTO {
  reportId: number;
  sysReportTemplateTypeId: number;
  budgetId: number;
  accountYearFromId: number;
  accountYearToId: number;
  accountPeriodFromId: number;
  accountPeriodToId: number;
  accountPeriodFrom: Date;
  accountPeriodTo: Date;
}

export class DrillDownReportDTO implements IDrillDownReportDTO {
  reportId!: number;
  sysReportTemplateTypeId!: number;
  budgetId!: number;
  accountYearFromId!: number;
  accountYearToId!: number;
  accountPeriodFromId!: number;
  accountPeriodToId!: number;
  accountPeriodFrom!: Date;
  accountPeriodTo!: Date;
}

export class DrilldownReportGridFlattenedDTO
  implements IDrilldownReportGridFlattenedDTO
{
  reportGroupOrder!: number;
  reportGroupName!: string;
  reportHeaderOrder!: number;
  reportHeaderName!: string;
  accountNr!: string;
  accountNrCount!: string;
  accountName!: string;
  periodAmount!: number;
  yearAmount!: number;
  openingBalance!: number;
  prevPeriodAmount!: number;
  prevYearAmount!: number;
  budgetPeriodAmount!: number;
  budgetToPeriodEndAmount!: number;
  periodPrevPeriodDiff!: number;
  yearPrevYearDiff!: number;
  periodBudgetDiff!: number;
  yearBudgetDiff!: number;
}

export class ReportViewDTO implements IReportViewDTO {
  actorCompanyId!: number;
  reportId!: number;
  exportType!: number;
  reportName!: string;
  name!: string;
  reportNr!: number;
  reportSelectionId?: number;
  reportDescription!: string;
  sysReportTemplateTypeId!: number;
  showInAccountingReports!: boolean;
  sysReportTypeName!: string;
  isSystemReport!: boolean;
  reportNameDesc!: string;
}

export class SearchVoucherRowsAngDTO implements ISearchVoucherRowsAngDTO {
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
  voucherSeriesTypeIds!: number[];
}
