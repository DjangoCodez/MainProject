import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeWorkReductionComponent } from './components/time-work-reduction/time-work-reduction.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeWorkReductionComponent,
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TimeWorkReductionRoutingModule { }
