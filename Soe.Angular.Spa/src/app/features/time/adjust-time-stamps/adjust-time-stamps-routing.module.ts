import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdjustTimeStampsComponent } from './components/adjust-time-stamps/adjust-time-stamps.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AdjustTimeStampsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdjustTimeStampsRoutingModule {}
