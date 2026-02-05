import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountProvisionBaseComponent } from './components/account-provision-base/account-provision-base.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountProvisionBaseComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountProvisionBaseRoutingModule { }
