import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CustomerInvoiceMatchesComponent } from './components/customer-invoice-matches/customer-invoice-matches.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CustomerInvoiceMatchesComponent,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CustomerInvoiceMatchesRoutingModule { }
