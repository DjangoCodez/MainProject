import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EdistributionComponent } from './components/edistribution/edistribution.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EdistributionComponent,
  },
]


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EdistributionRoutingModule { }
