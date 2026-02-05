import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeCodeRankingComponent } from './components/time-code-ranking/time-code-ranking';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeCodeRankingComponent,
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeCodeRankingRoutingModule {}
