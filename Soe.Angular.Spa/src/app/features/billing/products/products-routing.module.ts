import { Routes, RouterModule } from '@angular/router';
import { ProductsComponent } from './components/products/products.component';
import { NgModule } from '@angular/core';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ProductsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductsRoutingModule {}
