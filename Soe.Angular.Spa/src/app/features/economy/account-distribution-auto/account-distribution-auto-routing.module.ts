import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountDistributionAutoComponent } from './components/account-distribution-auto.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountDistributionAutoComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountDistributionAutoRoutingModule {}
