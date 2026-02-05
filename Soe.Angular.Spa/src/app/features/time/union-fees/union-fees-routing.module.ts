import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UnionFeesComponent } from './components/union-fees/union-fees.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: UnionFeesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class UnionFeesRoutingModule {}
