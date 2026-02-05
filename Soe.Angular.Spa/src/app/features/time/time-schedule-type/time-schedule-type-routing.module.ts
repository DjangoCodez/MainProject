import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeScheduleTypeComponent } from './components/time-schedule-type/time-schedule-type.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeScheduleTypeComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeScheduleTypeRoutingModule {}
