import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AnnualLeaveBalanceComponent } from './components/annual-leave-balance/annual-leave-balance.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AnnualLeaveBalanceComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AnnualLeaveBalanceRoutingModule {}
