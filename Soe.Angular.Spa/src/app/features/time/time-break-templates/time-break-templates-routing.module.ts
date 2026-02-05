import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeBreakTemplatesComponent } from './components/time-break-templates/time-break-templates.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeBreakTemplatesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeBreakTemplatesRoutingModule {}
