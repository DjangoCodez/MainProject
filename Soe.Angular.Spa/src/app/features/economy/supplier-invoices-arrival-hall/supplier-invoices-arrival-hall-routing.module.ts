import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierInvoicesArrivalHallComponent } from './components/supplier-invoices-arrival-hall/supplier-invoices-arrival-hall.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SupplierInvoicesArrivalHallComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SupplierInvoicesArrivalHallRoutingModule { }
