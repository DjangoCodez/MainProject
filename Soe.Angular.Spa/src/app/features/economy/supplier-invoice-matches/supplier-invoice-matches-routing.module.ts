import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierInvoiceMatchesComponent } from './components/supplier-invoice-matches/supplier-invoice-matches.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SupplierInvoiceMatchesComponent,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SupplierInvoiceMatchesRoutingModule { }
