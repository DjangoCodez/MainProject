import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { CustomerDiscountComponent } from './components/customer-discount/customer-discount.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CustomerDiscountComponent
  }
];

@NgModule({
  declarations: [],
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CustomerDiscountRoutingModule { }
