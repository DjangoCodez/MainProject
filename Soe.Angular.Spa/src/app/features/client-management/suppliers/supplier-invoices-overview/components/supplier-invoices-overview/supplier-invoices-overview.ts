import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupplierInvoicesOverviewGrid } from '../supplier-invoices-overview-grid/supplier-invoices-overview-grid';

@Component({
  selector: 'soe-supplier-invoices-overview',
  templateUrl: './supplier-invoices-overview.html',
  standalone: false,
})
export class SupplierInvoicesOverview {
  config: MultiTabConfig[] = [
    {
      gridComponent: SupplierInvoicesOverviewGrid,
      gridTabLabel: 'clientmanagement.suppliers.invoices.overview',
    },
  ];
}
