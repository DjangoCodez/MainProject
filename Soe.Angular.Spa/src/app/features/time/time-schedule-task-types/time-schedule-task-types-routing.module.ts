import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeScheduleTaskTypesComponent } from './components/time-schedule-task-types/time-schedule-task-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeScheduleTaskTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TaskTypesRoutingModule {}
