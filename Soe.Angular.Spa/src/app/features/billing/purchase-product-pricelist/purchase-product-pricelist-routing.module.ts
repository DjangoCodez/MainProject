import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PurchaseProductPricelistComponent } from './components/purchase-product-pricelist/purchase-product-pricelist.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PurchaseProductPricelistComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PurchaseProductPricelistRoutingModule {}
