import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StockInventoryComponent } from './components/stock-inventory/stock-inventory.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: StockInventoryComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StockInventoryRoutingModule {}
