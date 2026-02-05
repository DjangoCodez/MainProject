import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StockWarehouseComponent } from './components/stock-warehouse/stock-warehouse.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: StockWarehouseComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class StockWarehouseRoutingModule {}
