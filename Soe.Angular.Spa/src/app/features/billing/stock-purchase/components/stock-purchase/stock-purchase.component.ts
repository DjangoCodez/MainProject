import { Component } from '@angular/core';
import { StockPurchaseGridComponent } from '../stock-purchase-grid/stock-purchase-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-stock-purchase',
  templateUrl: './stock-purchase.component.html',
  standalone: false,
})
export class StockPurchaseComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StockPurchaseGridComponent,
      gridTabLabel: 'billing.stock.purchase.suggestion',
      hideForCreateTabMenu: true,
    },
  ];
}
