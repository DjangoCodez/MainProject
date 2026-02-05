import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeGroupsComponent } from './components/employee-groups/employee-groups.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmployeeGroupsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeeGroupsRoutingModule {}
