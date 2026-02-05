import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ScheduleCyclesComponent } from './components/schedule-cycles/schedule-cycles.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ScheduleCyclesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ScheduleCyclesRoutingModule {}
