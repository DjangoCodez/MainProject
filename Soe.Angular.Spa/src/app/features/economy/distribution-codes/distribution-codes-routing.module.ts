import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DistributionCodesComponent } from './components/distribution-codes/distribution-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DistributionCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DistributionCodesRoutingModule {}
