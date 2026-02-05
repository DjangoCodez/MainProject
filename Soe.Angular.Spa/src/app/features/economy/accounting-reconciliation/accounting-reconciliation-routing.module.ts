import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountingReconciliationComponent } from './compoents/accounting-reconciliation/accounting-reconciliation.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountingReconciliationComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountingReconciliationRoutingModule {}
