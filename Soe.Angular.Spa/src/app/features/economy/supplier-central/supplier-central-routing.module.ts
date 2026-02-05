import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SupplierCentralComponent } from './components/supplier-central/supplier-central.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SupplierCentralComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SupplierCentralRoutingModule {}
