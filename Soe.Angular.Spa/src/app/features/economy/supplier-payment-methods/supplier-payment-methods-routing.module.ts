import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierPaymentMethodsComponent } from './components/supplier-payment-methods/supplier-payment-methods.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SupplierPaymentMethodsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SupplierPaymentMethodsRoutingModule {}
