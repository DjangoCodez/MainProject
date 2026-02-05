import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DayTypesComponent } from './components/day-types/day-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: DayTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class DayTypesRoutingModule {}
