import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { DiscountLettersGridComponent } from '../discount-letters-grid/discount-letters-grid.component';
import { NetPricesGridComponent } from '../net-prices-grid/net-prices-grid.component';

@Component({
  selector: 'soe-discount-letters',
  templateUrl: './discount-letters.component.html',
  standalone: false,
})
export class DiscountLettersComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DiscountLettersGridComponent,
      gridTabLabel: 'billing.invoices.supplieragreement.agreement',
    },
    {
      gridComponent: NetPricesGridComponent,
      gridTabLabel: 'billing.invoices.supplieragreement.netprices',
    },
  ];
}
