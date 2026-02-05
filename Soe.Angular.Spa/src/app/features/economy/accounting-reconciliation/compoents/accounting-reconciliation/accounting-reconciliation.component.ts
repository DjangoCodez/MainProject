import { Component } from '@angular/core';
import { AccountingReconciliationGridComponent } from '../accounting-reconciliation-grid/accounting-reconciliation-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AccountingReconciliationEditComponent } from '../accounting-reconciliation-edit/accounting-reconciliation-edit.component';
import { AccountingReconciliationForm } from '../../models/accounting-reconciliation-form.model';

@Component({
  selector: 'soe-accounting-reconciliation',
  templateUrl: './accounting-reconciliation.component.html',
  standalone: false,
})
export class AccountingReconciliationComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AccountingReconciliationGridComponent,
      editComponent: AccountingReconciliationEditComponent,
      FormClass: AccountingReconciliationForm,
      gridTabLabel: 'economy.accounting.reconciliation.reconciliation',
      exportFilenameKey: 'economy.accounting.reconciliation.reconciliation',
    },
  ];
}
