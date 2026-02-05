import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MaterialCodesComponent } from './components/material-codes/material-codes.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: MaterialCodesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MaterialCodesRoutingModule {}
