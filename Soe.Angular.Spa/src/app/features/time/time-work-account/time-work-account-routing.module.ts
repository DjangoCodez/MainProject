import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeWorkAccountComponent } from './components/time-work-account/time-work-account.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeWorkAccountComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeWorkAccountRoutingModule {}
