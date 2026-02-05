import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { StockWarehouseForm } from '../../models/stock-warehouse-form.model';
import { StockWarehouseGridComponent } from '../stock-warehouse-grid/stock-warehouse-grid.component';
import { StockWarehouseEditComponent } from '../stock-warehouse-edit/stock-warehouse-edit.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class StockWarehouseComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StockWarehouseGridComponent,
      editComponent: StockWarehouseEditComponent,
      FormClass: StockWarehouseForm,
      gridTabLabel: 'billing.stock.stocks.stocks',
      editTabLabel: 'billing.stock.stocks.stock',
      createTabLabel: 'billing.stock.stocks.new_stock',
    },
  ];
}
