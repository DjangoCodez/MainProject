import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StockPurchaseComponent } from './components/stock-purchase/stock-purchase.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: StockPurchaseComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StockPurchaseRoutingModule {}
