import { Component } from '@angular/core';
import { PurchaseStatisticsFilterForm as PurchaseStatisticsFilterForm } from '../../models/purchase-statistics-filter-form.model';
import { PurchaseStatisticsGridComponent } from '../purchase-statistics-grid/purchase-statistics-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-purchase-statistics',
  templateUrl: './purchase-statistics.component.html',
  standalone: false,
})
export class PurchaseStatisticsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PurchaseStatisticsGridComponent,
      FormClass: PurchaseStatisticsFilterForm,
      gridTabLabel: 'billing.purchase.statistics',
    },
  ];
}
