export interface IAccountingPeriodSelection {
  selectionType: AccountingPeriodSelectionType;
  accountingYearFrom?: number;
  accountingYearTo?: number;
  monthFrom?: number;
  monthTo?: number;
  dateFrom?: Date;
  dateTo?: Date;
}

export enum AccountingPeriodSelectionType {
  ByFinancialYear = 1,
  ByDate = 2,
}
