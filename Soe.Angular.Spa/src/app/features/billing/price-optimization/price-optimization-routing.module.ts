import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PriceOptimizationComponent } from './components/price-optimization/price-optimization.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PriceOptimizationComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PriceOptimizationRoutingModule {}
