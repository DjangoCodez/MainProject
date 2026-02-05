import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SalesStatisticsComponent } from './components/sales-statistics/sales-statistics.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SalesStatisticsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SalesStatisticsRoutingModule {}
