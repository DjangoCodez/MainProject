import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DeliveryTypesComponent } from './components/delivery-types/delivery-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DeliveryTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DeliveryTypesRoutingModule {}
