import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseDeliveryComponent } from './components/purchase-delivery/purchase-delivery.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PurchaseDeliveryComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PurchaseDeliveryRoutingModule {}
