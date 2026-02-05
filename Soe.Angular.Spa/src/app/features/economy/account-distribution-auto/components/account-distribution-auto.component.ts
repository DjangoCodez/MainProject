import { Component } from '@angular/core';
import { AccountDistributionGridComponent } from '@features/economy/account-distribution/components/account-distribution-grid/account-distribution-grid.component';
import { AccountDistributionEditComponent } from '@features/economy/account-distribution/components/account-distribution-edit/account-distribution-edit.component';
import { AutoAccountDistributionForm } from '@features/economy/account-distribution/models/account-distribution-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-account-distribution-auto',
  templateUrl:
    '../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AccountDistributionAutoComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AccountDistributionGridComponent,
      editComponent: AccountDistributionEditComponent,
      FormClass: AutoAccountDistributionForm,
      gridTabLabel:
        'economy.accounting.accountdistribution.accountdistributionsauto',
      editTabLabel:
        'economy.accounting.accountdistribution.accountdistributionauto',
      createTabLabel: 'economy.accounting.accountdistribution.newauto',
    },
  ];
}
