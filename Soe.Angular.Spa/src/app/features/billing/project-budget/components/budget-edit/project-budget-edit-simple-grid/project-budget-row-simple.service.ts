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
    ProjectCentralBudgetRowType.CostExpense,
    ProjectCentralBudgetRowType.OverheadCost,
    ProjectCentralBudgetRowType.OverheadCostPerHour,
  ];

  private periodRange: Date[] = [];

  setupDefaultRows(
    rows: BudgetRowProjectDTO[],
    overheadCostPerHour: boolean
  ): BudgetRowProjectDTO[] {
    let counter = 0;
    return this.budgetRowTypes.map(t => {
      const exists = rows.find(r => r.type === t);
      if (exists) {
        exists.isDefault = true;
        return exists;
      }

      const row = BudgetRowProjectDTO.create(t, counter++);
      row.isDefault = true;
      row.isLocked = true;
      return row;
    });
  }

  toSorted(rows: BudgetRowProjectDTO[]): BudgetRowProjectDTO[] {
    return rows.sort((a, b) => {
      if (!a.budgetRowId || a.budgetRowId === 0) return 0;
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
