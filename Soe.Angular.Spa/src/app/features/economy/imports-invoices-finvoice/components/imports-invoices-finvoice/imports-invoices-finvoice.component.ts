import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ImportsInvoicesFinvoiceGridComponent } from '../imports-invoices-finvoice-grid/imports-invoices-finvoice-grid.component';

@Component({
  templateUrl: 'imports-invoices-finvoice.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class ImportsInvoicesFinvoiceComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ImportsInvoicesFinvoiceGridComponent,
      gridTabLabel: 'economy.import.finvoice.importinvoices',
      editTabLabel: 'economy.import.finvoice',
      createTabLabel: 'economy.import.finvoice.new_importinvoice',
      exportFilenameKey: 'economy.import.finvoice',
    },
  ];
}
