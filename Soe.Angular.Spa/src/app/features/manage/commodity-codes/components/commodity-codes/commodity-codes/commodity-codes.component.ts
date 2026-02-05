import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CommodityCodesForm } from '../../../models/commodity-codes-form.model';
import { CommodityCodesGridComponent } from '../../commodity-codes-grid/commodity-codes-grid/commodity-codes-grid.component';

@Component({
  templateUrl: './commodity-codes.component.html',
  standalone: false,
})
export class CommodityCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CommodityCodesGridComponent,
      FormClass: CommodityCodesForm,
      gridTabLabel: 'manage.system.commoditycode.commoditycode',
    },
  ];
}
