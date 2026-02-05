import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeScheduleTasksComponent } from './components/time-schedule-tasks/time-schedule-tasks.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeScheduleTasksComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeScheduleTasksRoutingModule {}
