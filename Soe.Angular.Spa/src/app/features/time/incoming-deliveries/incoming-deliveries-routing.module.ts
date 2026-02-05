import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IncomingDeliveriesComponent } from './components/incoming-deliveries/incoming-deliveries.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: IncomingDeliveriesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class IncomingDeliveriesRoutingModule {}
