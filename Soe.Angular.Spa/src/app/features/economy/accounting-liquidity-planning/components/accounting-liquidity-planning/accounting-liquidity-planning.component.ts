import { Component } from '@angular/core';
import { AccountingLiquidityPlanningGridComponent } from '../accounting-liquidity-planning-grid/accounting-liquidity-planning-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-accounting-liquidity-planning',
  templateUrl: './accounting-liquidity-planning.component.html',
  standalone: false,
})
export class AccountingLiquidityPlanningComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AccountingLiquidityPlanningGridComponent,
      gridTabLabel:
        'economy.supplier.invoice.liquidityplanning.liquidityplanning',
      exportFilenameKey:
        'economy.supplier.invoice.liquidityplanning.liquidityplanning',
    },
  ];
}
