import { Component } from '@angular/core';
import { BankintegrationDownloadRequestGridComponent } from '../bankintegration-downloadrequest-grid/bankintegration-downloadrequest-grid.component';
import { BankintegrationOnboardingGridComponent } from '../bankintegration-onboarding-grid/bankintegration-onboarding-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl: './bankintegration.component.html',
  standalone: false,
})
export class BankintegrationComponent {
  config: MultiTabConfig[] = [
    {
      gridTabLabel: 'manage.system.bankintegration.downloadrequest',
      gridComponent: BankintegrationDownloadRequestGridComponent,
    },
    {
      gridTabLabel: 'manage.system.bankintegration.onboarding',
      gridComponent: BankintegrationOnboardingGridComponent,
    },
  ];
}
