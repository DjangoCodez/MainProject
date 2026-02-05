import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ProductStatisticsGridComponent } from '../product-statistics-grid/product-statistics-grid.component';

@Component({
  selector: 'soe-product-statistics',
  templateUrl: './product-statistics.component.html',
  styleUrls: ['./product-statistics.component.scss'],
  standalone: false,
})
export class ProductStatisticsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ProductStatisticsGridComponent,
      gridTabLabel: 'billing.product.statistics',
    },
  ];
}
