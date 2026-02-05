import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeDeviationCausesComponent } from './components/time-deviation-causes/time-deviation-causes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeDeviationCausesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TimeDeviationCausesRoutingModule { }
