import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PaymentConditionsComponent } from './components/payment-conditions/payment-conditions.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PaymentConditionsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PaymentConditionsRoutingModule {}
