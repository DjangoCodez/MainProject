import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OpeningHoursComponent } from './components/opening-hours/opening-hours.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: OpeningHoursComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OpeningHoursRoutingModule {}
