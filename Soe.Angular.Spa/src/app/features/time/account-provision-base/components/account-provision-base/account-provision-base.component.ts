import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AccountProvisionBaseForm } from '../../models/account-provision-base-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AccountProvisionBaseGridComponent } from '../account-provision-base-grid/account-provision-base-grid.component';

@Component({
  selector: 'soe-account-provision-base',
  standalone: false,
  templateUrl: './account-provision-base.component.html',
})
export class AccountProvisionBaseComponent {
  config: MultiTabConfig[] = [
    {
      FormClass: AccountProvisionBaseForm,
      gridComponent: AccountProvisionBaseGridComponent,
      gridTabLabel: 'time.payroll.accountprovision.accountprovisionbases',
      hideForCreateTabMenu: true,
      createTabLabel: 'time.payroll.accountprovision.accountprovisionbase',
    },
  ];

  constructor(protected translate: TranslateService) {}
}
