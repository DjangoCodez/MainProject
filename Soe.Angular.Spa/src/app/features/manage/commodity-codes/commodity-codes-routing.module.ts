import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommodityCodesComponent } from './components/commodity-codes/commodity-codes/commodity-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CommodityCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CommodityCodesRoutingModule {}
