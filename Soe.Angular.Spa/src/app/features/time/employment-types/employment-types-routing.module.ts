import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmploymentTypesComponent } from './components/employment-types/employment-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmploymentTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EmploymentTypesRoutingModule { }
