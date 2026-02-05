import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AnnualLeaveBalanceGridComponent } from '../annual-leave-balance-grid/annual-leave-balance-grid.component';
import { AnnualLeaveBalanceEditComponent } from '../annual-leave-balance-edit/annual-leave-balance-edit.component';
import { AnnualLeaveTransactionForm } from '../../models/annual-leave-balance-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AnnualLeaveBalanceComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AnnualLeaveBalanceGridComponent,
      editComponent: AnnualLeaveBalanceEditComponent,
      FormClass: AnnualLeaveTransactionForm,
      gridTabLabel: 'time.employee.annualleavebalance.transactions',
      editTabLabel: 'time.employee.annualleavebalance.transaction',
      createTabLabel: 'time.employee.annualleavebalance.transaction.new',
      passGridDataOnAdd: true,
    },
  ];
}
