import { Component } from '@angular/core';
import { BudgetGridComponent } from '../budget-grid/budget-grid.component';
import { BudgetEditComponent } from '../budget-edit/budget-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { BudgetForm } from '../../models/budget-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class BudgetComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: BudgetGridComponent,
      editComponent: BudgetEditComponent,
      FormClass: BudgetForm,
      exportFilenameKey: 'economy.accounting.budget.budgets',
      gridTabLabel: 'economy.accounting.budget.budgets',
      editTabLabel: 'economy.accounting.budget.budget',
      createTabLabel: 'economy.accounting.budget.newbudget',
    },
  ];
}
