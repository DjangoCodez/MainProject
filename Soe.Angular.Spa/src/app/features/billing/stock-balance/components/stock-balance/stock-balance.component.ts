import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { StockBalanceForm } from '../../models/stock-balance-form.model';
import { StockBalanceGridComponent } from '../stock-balance-grid/stock-balance-grid.component';
import { StockBalanceEditComponent } from '../stock-balance-edit/stock-balance-edit.component';

@Component({
  templateUrl: './stock-balance.component.html',
  standalone: false,
})
export class StockBalanceComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: StockBalanceGridComponent,
      editComponent: StockBalanceEditComponent,
      FormClass: StockBalanceForm,
      gridTabLabel: 'billing.stock.stocksaldo.stocksaldo',
      editTabLabel: 'billing.stock.stocksaldo.stocksaldo',
      createTabLabel: 'billing.stock.stocksaldo.stocksaldo',
    },
  ];
}
