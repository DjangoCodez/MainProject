import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupplierInvoicesArrivalHallGridComponent } from '../supplier-invoices-arrival-hall-grid/supplier-invoices-arrival-hall-grid.component';
import { SupplierInvoiceEditComponent } from '@features/economy/shared/supplier-invoice/components/supplier-invoice-edit/supplier-invoice-edit.component';
import { SupplierInvoiceForm } from '@features/economy/shared/supplier-invoice/models/supplier-invoice-form.model';

@Component({
  selector: 'soe-supplier-invoice-arrival-hall',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SupplierInvoicesArrivalHallComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SupplierInvoicesArrivalHallGridComponent,
      editComponent: SupplierInvoiceEditComponent,
      FormClass: SupplierInvoiceForm,
      gridTabLabel: 'economy.supplier.invoice.incoming',
      editTabLabel: 'economy.supplier.invoice.invoice',
      createTabLabel: 'economy.supplier.invoice.new',
    },
  ];
}
