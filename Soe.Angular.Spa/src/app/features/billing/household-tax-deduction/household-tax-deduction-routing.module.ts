import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HouseholdTaxDeductionComponent } from './components/household-tax-deduction/household-tax-deduction.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: HouseholdTaxDeductionComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class HouseholdTaxDeductionRoutingModule {}
