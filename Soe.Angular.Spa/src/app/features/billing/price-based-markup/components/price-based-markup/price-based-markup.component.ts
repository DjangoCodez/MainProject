import { Component } from '@angular/core';
import { PriceBasedMarkupGridComponent } from '../price-based-markup-grid/price-based-markup-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl: 'price-based-markup.component.html',
  standalone: false,
})
export class PriceBasedMarkupComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PriceBasedMarkupGridComponent,
      gridTabLabel: 'billing.invoices.pricebasedmarkup.pricebasedmarkup',
    },
  ];
}
