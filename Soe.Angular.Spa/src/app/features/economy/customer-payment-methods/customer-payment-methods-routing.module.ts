import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CustomerPaymentMethodsComponent } from './components/customer-payment-methods/customer-payment-methods/customer-payment-methods.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CustomerPaymentMethodsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CustomerPaymentMethodsRoutingModule {}
