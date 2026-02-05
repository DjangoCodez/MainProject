import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PositionsComponent } from './components/positions/positions.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PositionsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PositionsRoutingModule {}
