import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DistributionSalesEuComponent } from './components/distribution-sales-eu/distribution-sales-eu.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DistributionSalesEuComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DistributionSalesEuRoutingModule {}
