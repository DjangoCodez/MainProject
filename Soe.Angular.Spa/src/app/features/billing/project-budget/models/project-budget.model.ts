import {
  ProjectCentralBudgetRowType,
  TermGroup_ProjectBudgetPeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IBudgetHeadProjectDTO,
  IBudgetPeriodProjectDTO,
  IBudgetRowProjectChangeLogDTO,
  IBudgetRowProjectDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class BudgetHeadProjectDTO implements IBudgetHeadProjectDTO {
  budgetHeadId!: number;
  actorCompanyId!: number;
  type!: number;
  noOfPeriods!: number;
  status!: number;
  projectId?: number;
  projectNr!: string;
  projectName!: string;
  projectFromDate?: Date;
  projectToDate?: Date;
  statusName!: string;
  name!: string;
  fromDate?: Date;
  toDate?: Date;
  periodType!: TermGroup_ProjectBudgetPeriodType;
  createdDate!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  parentBudgetHeadId?: number;
  parentBudgetHeadName!: string;
  rows: IBudgetRowProjectDTO[] = [];
  resultModified!: string;
}

export class BudgetRowProjectDTO implements IBudgetRowProjectDTO {
  public static create(
    type: ProjectCentralBudgetRowType,
    rowNr: number
  ): BudgetRowProjectDTO {
    const row = new BudgetRowProjectDTO();
    row.type = type;
    row.budgetRowNr = rowNr;
    row.isModified = true;
    row.isAdded = true;
    return row;
  }

  budgetRowId!: number;
  budgetHeadId!: number;
  timeCodeId!: number;
  typeCodeName!: string;
  name!: string;
  budgetRowNr!: number;
  type!: ProjectCentralBudgetRowType;
  typeName = '';
  isTotalsRow = false;
  isAdded = false;
  isModified = false;
  isDeleted = false;
  isLocked = false;
  isDefault = false;
  hasLogPosts = false;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  totalAmount = 0;
  totalQuantity = 0;
  totalAmountResult = 0;
  totalQuantityResult = 0;
  totalDiffResult = 0;
  totalAmountCompBudget = 0;
  totalQuantityCompBudget = 0;
  totalDiffCompBudget = 0;
  comment = '';
  changeLogItems: BudgetRowProjectChangeLogDTO[] = [];
  periods: BudgetPeriodProjectDTO[] = [];
  usedRatio: number = 0;

  constructor(budgetRowId: number = 0) {
    this.budgetRowId = budgetRowId;
  }

  static calculateTotals(row: BudgetRowProjectDTO): void {
    if (!row.periods || row.periods.length === 0) return;
    row.totalAmount = 0;
    row.totalQuantity = 0;
    row.periods.forEach(period => {
      row.totalAmount += period.amount;
      row.totalQuantity += period.quantity;
    });
  }

  static incrementRowNr(row: BudgetRowProjectDTO): void {
    if (!row.isTotalsRow) {
      row.budgetRowNr++;
    }
  }

  static decrementRowNr(row: BudgetRowProjectDTO): void {
    if (!row.isTotalsRow) {
      row.budgetRowNr--;
    }
  }

  static calculateRatio(row: BudgetRowProjectDTO): void {
    row.usedRatio = (row.totalAmountResult / row.totalAmount) * 100;
  }
}

export class BudgetPeriodProjectDTO implements IBudgetPeriodProjectDTO {
  public static create(date: Date, periodNr: number): BudgetPeriodProjectDTO {
    const period = new BudgetPeriodProjectDTO();
    period.startDate = date;
    period.periodNr = periodNr;
    period.isModified = true;
    return period;
  }
  budgetRowPeriodId!: number;
  budgetRowId!: number;
  periodNr!: number;
  type?: number;
  startDate!: Date;
  isModified = false;
  amount = 0;
  quantity = 0;
}

export class BudgetRowProjectChangeLogDTO
  implements IBudgetRowProjectChangeLogDTO
{
  budgetRowChangeLogId!: number;
  budgetRowId!: number;
  created?: Date;
  createdBy!: string;
  fromTotalAmount!: number;
  fromTotalQuantity!: number;
  toTotalAmount!: number;
  toTotalQuantity!: number;
  totalAmountDiff!: number;
  totalQuantityDiff!: number;
  comment!: string;

  //Extended properties
  typeName!: string;
  description!: string;
  timeCodeName!: string;
}
