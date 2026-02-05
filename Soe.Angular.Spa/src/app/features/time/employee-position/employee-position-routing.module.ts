import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeePositionComponent } from './components/employee-position/employee-position.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmployeePositionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeePositionRoutingModule {}
