import { Injectable } from '@angular/core';
import {
  BudgetPeriodProjectDTO,
  BudgetRowProjectDTO,
} from '@features/billing/project-budget/models/project-budget.model';
import {
  ProjectCentralBudgetRowType,
  TermGroup_ProjectBudgetPeriodType,
} from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from '@shared/util/date-util';
import { AG_NODE } from '@ui/grid/grid.component';

@Injectable({
  providedIn: 'root',
})
export class ProjectBudgetRowService {
  private readonly budgetRowTypes: ProjectCentralBudgetRowType[] = [
    ProjectCentralBudgetRowType.IncomePersonellTotal,
    ProjectCentralBudgetRowType.IncomeMaterialTotal,
    ProjectCentralBudgetRowType.CostPersonell,
    ProjectCentralBudgetRowType.CostMaterial,
    ProjectCentralBudgetRowType.OverheadCostPerHour,
    ProjectCentralBudgetRowType.OverheadCost,
    ProjectCentralBudgetRowType.CostExpense,
  ];

  private periodRange: Date[] = [];

  setupDefaultRows(
    rows: BudgetRowProjectDTO[],
    overheadCostPerHour: boolean
  ): BudgetRowProjectDTO[] {
    let counter = 0;
    if (this.periodRange.length === 0) {
      this.periodRange =
        rows.find(r => r.periods.length > 0)?.periods.map(p => p.startDate) ||
        [];
    }

    return this.getBudgetRowTypes(overheadCostPerHour).map(t => {
      const exists = rows.find(r => r.type === t);
      if (exists) {
        exists.isDefault = true;
        return exists;
      }

      const row = BudgetRowProjectDTO.create(t, counter++);
      row.periods = this.periodRange.map(p =>
        BudgetPeriodProjectDTO.create(p, row.periods.length + 1)
      );
      row.isDefault = true;
      return row;
    });
  }

  getBudgetRowTypes(
    overheadCostPerHour: boolean
  ): ProjectCentralBudgetRowType[] {
    console.log('overheadCostPerHour', overheadCostPerHour);
    if (overheadCostPerHour)
      return this.budgetRowTypes.filter(
        t => t !== ProjectCentralBudgetRowType.OverheadCost
      );
    else
      return this.budgetRowTypes.filter(
        t => t !== ProjectCentralBudgetRowType.OverheadCostPerHour
      );
  }

  calculatePeriodRange(
    periodType: TermGroup_ProjectBudgetPeriodType,
    fromDate: Date,
    toDate: Date
  ): Date[] {
    if (!fromDate || !toDate || fromDate > toDate) return [];

    const result: Date[] = [];
    let current = new Date(fromDate.getFullYear(), fromDate.getMonth(), 1);
    const end = new Date(toDate.getFullYear(), toDate.getMonth(), 1);

    if (periodType === TermGroup_ProjectBudgetPeriodType.Monthly) {
      while (current <= end) {
        result.push(new Date(current));
        current.setMonth(current.getMonth() + 1);
      }
    } else if (periodType === TermGroup_ProjectBudgetPeriodType.Quarterly) {
      // Move to the start of the quarter
      const startMonth = Math.floor(current.getMonth() / 3) * 3;
      current = new Date(current.getFullYear(), startMonth, 1);

      while (current <= end) {
        result.push(new Date(current));
        current.setMonth(current.getMonth() + 3);
      }
    } else {
      return [];
    }
    this.periodRange = result;

    return result;
  }

  changePeriodRange(rows: BudgetRowProjectDTO[], range: Date[]) {
    const isSameMonth = (date1: Date, date2: Date) =>
      DateUtil.diffMonths(date1, date2) === 0;

    /**
     * Iterate over each row's periods and do the following:
     * 1. If there is no period that mathces the start date, create a new period.
     * 2. If there is a period that matches the start date, do nothing.
     * 3. If there is a period that does not match the start date, remove it.
     */
    rows.forEach(row => {
      row.periods = range.map(date => {
        const existing = row.periods.find(p => isSameMonth(p.startDate, date));
        if (existing) {
          return existing;
        } else {
          return BudgetPeriodProjectDTO.create(date, row.periods.length + 1);
        }
      });
      BudgetRowProjectDTO.calculateTotals(row);
    });

    return rows;
  }

  toSorted(rows: BudgetRowProjectDTO[]): BudgetRowProjectDTO[] {
    return rows.sort((a, b) => {
      if (a.type < b.type) return -1;
      if (a.type > b.type) return 1;
      return a.budgetRowId - b.budgetRowId;
    });
  }

  createRow(
    type: ProjectCentralBudgetRowType,
    rowNr: number
  ): BudgetRowProjectDTO {
    const newRow = BudgetRowProjectDTO.create(type, rowNr);
    if (this.periodRange.length > 0) {
      // Copy periods from the nextTo row to the new row
      let periodNr = 0;
      newRow.periods = this.periodRange.map(p =>
        BudgetPeriodProjectDTO.create(p, periodNr++)
      );
    }
    return newRow;
  }

  addRow(rows: BudgetRowProjectDTO[], nextTo: BudgetRowProjectDTO) {
    const index = rows.findIndex(r => r === nextTo);
    const newRow = this.createRow(nextTo.type, nextTo.budgetRowNr + 1);

    // Insert the row after the nextTo row
    rows.splice(index + 1, 0, newRow as AG_NODE<BudgetRowProjectDTO>);

    // Move the budgetRowNr of all rows after the new row
    for (let i = index + 1; i < rows.length; i++) {
      BudgetRowProjectDTO.incrementRowNr(rows[i]);
    }

    return newRow;
  }

  deleteRow(
    rows: AG_NODE<BudgetRowProjectDTO>[],
    row: AG_NODE<BudgetRowProjectDTO>
  ) {
    const index = rows.findIndex(r => r.AG_NODE_ID === row.AG_NODE_ID);
    if (index < 0) return;

    // Mark the row as deleted
    row.isDeleted = true;

    // Remove the row from the array
    rows.splice(index, 1);

    // Move the budgetRowNr of all rows after the deleted row
    for (let i = index; i < rows.length; i++) {
      BudgetRowProjectDTO.decrementRowNr(rows[i]);
    }
  }
}
