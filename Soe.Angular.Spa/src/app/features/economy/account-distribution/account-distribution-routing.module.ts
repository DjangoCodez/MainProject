import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountDistributionComponent } from './components/account-distribution/account-distribution.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountDistributionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountDistributionRoutingModule {}
