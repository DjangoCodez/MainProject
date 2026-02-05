import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeFollowupTypesComponent } from './components/employee-followup-types/employee-followup-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmployeeFollowupTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeeFollowupTypesRoutingModule {}
