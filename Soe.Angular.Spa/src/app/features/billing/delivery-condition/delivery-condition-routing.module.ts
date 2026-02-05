import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DeliveryConditionComponent } from './components/delivery-condition/delivery-condition.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DeliveryConditionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DeliveryConditionRoutingModule {}
