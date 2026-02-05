import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { CustomerCentralComponent } from './components/customer-central/customer-central.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: CustomerCentralComponent
  }
]

@NgModule({
  declarations: [],
  imports: [ RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CustomerCentralRoutingModule { }
