import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ScheduleCycleRuleTypesComponent } from './components/schedule-cycle-rule-types/schedule-cycle-rule-types.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: ScheduleCycleRuleTypesComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ScheduleCycleRuleTypesRoutingModule {}
