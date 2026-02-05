import { Component } from '@angular/core';
import { AccountDistributionEditComponent } from '../account-distribution-edit/account-distribution-edit.component';
import { AccountDistributionGridComponent } from '../account-distribution-grid/account-distribution-grid.component';
import { PeriodAccountDistributionForm } from '../../models/account-distribution-form.model';
import { InventoryWriteoffsGridComponent } from '../../../inventory-writeoffs/components/inventory-writeoffs-grid/inventory-writeoffs-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-account-distribution',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AccountDistributionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: InventoryWriteoffsGridComponent,
      gridTabLabel: 'economy.accounting.accountdistributionentry.entries',
      exportFilenameKey: 'economy.accounting.accountdistributionentry.entries',
      hideForCreateTabMenu: true,
    },
    {
      gridComponent: AccountDistributionGridComponent,
      editComponent: AccountDistributionEditComponent,
      FormClass: PeriodAccountDistributionForm,
      gridTabLabel:
        'economy.accounting.accountdistribution.accountdistributions',
      editTabLabel:
        'economy.accounting.accountdistribution.accountdistribution',
      createTabLabel: 'economy.accounting.accountdistribution.new',
    },
  ];
}
