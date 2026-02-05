import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CustomerProductPriceListTypesComponent } from './components/customer-product-pricelisttypes/customer-product-pricelisttypes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CustomerProductPriceListTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CustomerProductPriceListRoutingModule {}
