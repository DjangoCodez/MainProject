import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductUnitsComponent } from './components/product-units/product-units.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ProductUnitsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductUnitsRoutingModule {}
