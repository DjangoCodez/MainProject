import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TimeCodeAdditionDeductionComponent } from './components/time-code-addition-deduction/time-code-addition-deduction.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: TimeCodeAdditionDeductionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TimeCodeAdditionDeductionRoutingModule {}
