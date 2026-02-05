import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IncomingDeliveryTypesComponent } from './components/incoming-delivery-types/incoming-delivery-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: IncomingDeliveryTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class IncomingDeliveryTypesRoutingModule {}
