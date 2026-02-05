import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupplierInvoiceMatchesGridComponent } from '../supplier-invoice-matches-grid/supplier-invoice-matches-grid.component';

@Component({
  selector: 'soe-supplier-invoice-matches',
  standalone: false,
  templateUrl: './supplier-invoice-matches.component.html'
})
export class SupplierInvoiceMatchesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SupplierInvoiceMatchesGridComponent,
      gridTabLabel: 'economy.supplier.invoice.matches.matches',
      exportFilenameKey: 'economy.supplier.invoice.matches.matches',
      editTabLabel: 'economy.supplier.invoice.matches.match',
      createTabLabel: 'economy.supplier.invoice.matches.new',
    },
  ];
}
