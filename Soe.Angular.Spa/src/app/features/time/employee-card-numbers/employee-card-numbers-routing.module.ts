import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeCardNumbersComponent } from './components/employee-card-numbers/employee-card-numbers.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: EmployeeCardNumbersComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeeCardNumbersRoutingModule {}
