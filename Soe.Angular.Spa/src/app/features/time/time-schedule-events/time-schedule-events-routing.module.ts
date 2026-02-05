import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeScheduleEventsComponent } from './components/time-schedule-events/time-schedule-events.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeScheduleEventsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeScheduleEventsRoutingModule {}
