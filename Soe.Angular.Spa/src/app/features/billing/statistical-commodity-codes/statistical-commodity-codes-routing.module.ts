import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { StatisticalCommodityCodesComponent } from './components/statistical-commodity-codes/statistical-commodity-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: StatisticalCommodityCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StatisticalCommodityCodesRoutingModule { }
