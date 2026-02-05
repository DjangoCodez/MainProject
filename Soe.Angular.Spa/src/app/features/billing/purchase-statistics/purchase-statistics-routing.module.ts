import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseStatisticsComponent } from './components/purchase-statistics/purchase-statistics.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PurchaseStatisticsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PurchaseStatisticsRoutingModule {}
