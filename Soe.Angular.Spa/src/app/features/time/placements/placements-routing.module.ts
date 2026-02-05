import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PlacementsComponent } from './components/placements/placements.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PlacementsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PlacementsRoutingModule {}
