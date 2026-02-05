import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ImportsInvoicesFinvoiceComponent } from './components/imports-invoices-finvoice/imports-invoices-finvoice.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ImportsInvoicesFinvoiceComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ImportsInvoicesFinvoiceRoutingModule {}
