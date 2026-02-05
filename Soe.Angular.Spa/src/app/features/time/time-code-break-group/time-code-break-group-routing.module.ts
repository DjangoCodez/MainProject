import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeCodeBreakGroupComponent } from './components/time-code-break-group/time-code-break-group.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeCodeBreakGroupComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeCodeBreakGroupRoutingModule {}
