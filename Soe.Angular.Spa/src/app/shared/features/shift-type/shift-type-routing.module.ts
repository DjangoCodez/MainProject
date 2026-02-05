import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ShiftTypeComponent } from './components/shift-type/shift-type.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ShiftTypeComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ShiftTypeRoutingModule {}
