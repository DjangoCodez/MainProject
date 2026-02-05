import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MatchCodeComponent } from './components/match-codes/match-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: MatchCodeComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MatchCodeRoutingModule {}
