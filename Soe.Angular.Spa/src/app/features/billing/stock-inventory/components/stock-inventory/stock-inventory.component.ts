import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { StockInventoryHeadForm } from '../../models/stock-inventory-head-form.model';
import { StockInventoryGridComponent } from '../stock-inventory-grid/stock-inventory-grid.component';
import { StockInventoryEditComponent } from '../stock-inventory-edit/stock-inventory-edit.component';

@Component({
  selector: 'soe-stock-inventory',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class StockInventoryComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StockInventoryGridComponent,
      editComponent: StockInventoryEditComponent,
      FormClass: StockInventoryHeadForm,
      gridTabLabel: 'billing.stock.stockinventory.stockinventory',
      editTabLabel: 'billing.stock.stockinventory.stockinventory',
      createTabLabel: 'billing.stock.stockinventory.new_stockinventory',
    },
  ];
}
