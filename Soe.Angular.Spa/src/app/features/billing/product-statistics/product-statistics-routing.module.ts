import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductStatisticsComponent } from './components/product-statistics/product-statistics.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ProductStatisticsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductStatisticsRoutingModule {}
