import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SalesStatisticsFilterForm } from '../../models/sales-statistics-filter-form.model';
import { SalesStatisticsGridComponent } from '../sales-statistics-grid/sales-statistics-grid.component';

@Component({
  selector: 'soe-sales-statistics',
  templateUrl: './sales-statistics.component.html',
  standalone: false,
})
export class SalesStatisticsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SalesStatisticsGridComponent,
      FormClass: SalesStatisticsFilterForm,
      gridTabLabel: 'billing.statistics.sales',
    },
  ];
}
