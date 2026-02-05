import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SchedulePlanningComponent } from './components/schedule-planning/schedule-planning.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: SchedulePlanningComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SchedulePlanningRoutingModule {}
