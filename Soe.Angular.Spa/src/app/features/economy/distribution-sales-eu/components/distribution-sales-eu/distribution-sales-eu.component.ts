import { Component } from '@angular/core';
import { DistributionSalesEuGridComponent } from '../distribution-sales-eu-grid/distribution-sales-eu-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-distribution-sales-eu',
  templateUrl: './distribution-sales-eu.component.html',
  standalone: false,
})
export class DistributionSalesEuComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DistributionSalesEuGridComponent,
      gridTabLabel: 'economy.reports.saleseu',
      exportFilenameKey: 'economy.reports.saleseu',
    },
  ];
}
