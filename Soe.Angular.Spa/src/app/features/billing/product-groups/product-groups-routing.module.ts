import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ProductGroupsComponent } from './components/product-groups/product-groups.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ProductGroupsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductGroupsRoutingModule {}
