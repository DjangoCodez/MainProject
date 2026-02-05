import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AccountingLiquidityPlanningComponent } from './components/accounting-liquidity-planning/accounting-liquidity-planning.component';

const routes: Routes = [
  {
    path: 'default.aspx',
    component: AccountingLiquidityPlanningComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountingLiquidityPlanningRoutingModule {}
