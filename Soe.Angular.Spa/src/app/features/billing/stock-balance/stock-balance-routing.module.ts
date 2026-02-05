import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StockBalanceComponent } from './components/stock-balance/stock-balance.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: StockBalanceComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StockBalanceRoutingModule {}
