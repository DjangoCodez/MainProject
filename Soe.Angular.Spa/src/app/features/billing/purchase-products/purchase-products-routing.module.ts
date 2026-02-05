import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseProductsComponent } from './components/purchase-products/purchase-products.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PurchaseProductsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PurchaseProductsRoutingModule {}
