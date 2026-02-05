import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PayrollLevelsComponent } from './components/payroll-levels/payroll-levels.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: PayrollLevelsComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PayrollLevelsRoutingModule { }
