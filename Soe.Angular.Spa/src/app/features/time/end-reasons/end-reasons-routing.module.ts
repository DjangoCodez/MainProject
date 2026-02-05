import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EndReasonsComponent } from './components/end-reasons/end-reasons.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EndReasonsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EndReasonsRoutingModule {}
