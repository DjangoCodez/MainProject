import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AccountProvisionTransactionsForm } from '../../models/account-provision-transactions-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AccountProvisionTransactionsGridComponent } from '../account-provision-transactions-grid/account-provision-transactions-grid.component';

@Component({
  selector: 'soe-account-provision-base',
  standalone: false,
  templateUrl: './account-provision-transactions.component.html',
})
export class AccountProvisionTransactionsComponent {
  config: MultiTabConfig[] = [
    {
      FormClass: AccountProvisionTransactionsForm,
      gridComponent: AccountProvisionTransactionsGridComponent,
      gridTabLabel: 'time.payroll.accountprovision.accountprovisiontransaction',
      hideForCreateTabMenu: true,
      createTabLabel:
        'time.payroll.accountprovision.accountprovisiontransaction',
    },
  ];

  constructor(protected translate: TranslateService) {}
}
