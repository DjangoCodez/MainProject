import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CustomerDiscountGridComponent } from '../customer-discount-grid/customer-discount-grid.component';

@Component({
  // templateUrl: '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  templateUrl: './customer-discount.component.html',
  standalone: false,
})
export class CustomerDiscountComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CustomerDiscountGridComponent,
      gridTabLabel: 'billing.invoices.markup.customerdiscount',
      editTabLabel: 'billing.invoices.markup.customerdiscount',
      createTabLabel: 'billing.invoices.markup.customerdiscount',
      exportFilenameKey: '',
    },
  ];
}
