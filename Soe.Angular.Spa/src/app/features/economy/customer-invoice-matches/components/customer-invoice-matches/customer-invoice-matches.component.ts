import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CustomerInvoiceMatchesGridComponent } from '../customer-invoice-matches-grid/customer-invoice-matches-grid.component';

@Component({
  selector: 'soe-customer-invoice-matches',
  standalone: false,
  templateUrl: './customer-invoice-matches.component.html'
})
export class CustomerInvoiceMatchesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CustomerInvoiceMatchesGridComponent,
      gridTabLabel: 'economy.customer.invoice.matches.matches',
      exportFilenameKey: 'economy.customer.invoice.matches.matches',
      editTabLabel: 'economy.customer.invoice.matches.match',
      createTabLabel: 'economy.customer.invoice.matches.new',
    },
  ];
}
