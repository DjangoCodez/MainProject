import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PlanningPeriodsComponent } from './components/planning-periods/planning-period.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PlanningPeriodsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PlanningPeriodsRoutingModule {}
