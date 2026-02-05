import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { StatisticalCommodityCodesGridComponent } from '../statistical-commodity-codes-grid/statistical-commodity-codes-grid.component';

@Component({
  templateUrl: './statistical-commodity-codes.component.html',
  standalone: false,
})
export class StatisticalCommodityCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StatisticalCommodityCodesGridComponent,
      gridTabLabel: 'common.commoditycodes.codes',
    },
  ];
}
