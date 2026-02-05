import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountProvisionTransactionsComponent } from './components/account-provision-transactions/account-provision-transactions.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountProvisionTransactionsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountProvisionTransactionsRoutingModule {}
