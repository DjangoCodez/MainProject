import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HandleBillingComponent } from './components/handle-billing/handle-billing.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: HandleBillingComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HandleBillingRoutingModule {}
