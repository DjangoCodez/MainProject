import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierInvoicesOverview } from './components/supplier-invoices-overview/supplier-invoices-overview';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SupplierInvoicesOverview,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SupplierInvoicesOverviewRoutingModule {}
