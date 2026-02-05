import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'clients',
    loadChildren: () =>
      import('./clients/clients.module').then(m => m.ClientsModule),
  },
  {
    path: 'suppliers/invoices/overview',
    loadChildren: () =>
      import(
        './suppliers/supplier-invoices-overview/supplier-invoices-overview-module'
      ).then(m => m.SupplierInvoicesOverviewModule),
  },
];

@NgModule({
  imports: [CommonModule, RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ClientManagementRoutingModule {}
