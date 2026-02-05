import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeNumberFormControl } from '@shared/extensions';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IInvoiceExportIODTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, take } from 'rxjs';
import { InvoiceExportIODTO } from '../../models/direct-debit.model';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-direct-debit-edit-grid',
  templateUrl: './direct-debit-edit-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DirectDebitEditGridComponent
  extends GridBaseDirective<InvoiceExportIODTO>
  implements OnInit
{
  @Output() selectedInvoices = new EventEmitter<InvoiceExportIODTO[]>();

  gridUpdated = false;
  flowHandler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  messageService = inject(MessageboxService);
  exportPaymentRows: IInvoiceExportIODTO[] = [];
  @Input() rows = new BehaviorSubject<InvoiceExportIODTO[]>([]);
  selectedTotal = new SoeNumberFormControl(0, { decimals: 2, disabled: true });
  performValidateShelf = new Perform<BackendResponse>(this.progressService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Export_Payments, 'InvoicesGrid', {
      skipInitialLoad: true,
    });
    this.selectedTotal.disable();
  }

  onGridReadyToDefine(grid: GridComponent<InvoiceExportIODTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.options.context.newRow = false;

    this.translate
      .get([
        'common.type',
        'common.report.selection.invoicenr',
        'common.customer',
        'economy.export.payments.invoiceamount',
        'economy.export.payments.invoicedate',
        'economy.export.payments.paydate',
        'economy.export.payments.bankaccount',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText('invoiceTypeName', terms['common.type'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'invoiceNr',
          terms['common.report.selection.invoicenr'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText('customerName', terms['common.customer'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'invoiceAmount',
          terms['economy.export.payments.invoiceamount']
        );
        this.grid.addColumnDate(
          'invoiceDate',
          terms['economy.export.payments.invoicedate'],
          {
            sort: 'asc',
          }
        );
        this.grid.addColumnDate(
          'dueDate',
          terms['economy.export.payments.paydate']
        );
        this.grid.addColumnText(
          'bankAccount',
          terms['economy.export.payments.bankaccount']
        );

        super.finalizeInitGrid();
      });
  }

  selectionChanged(data: InvoiceExportIODTO[]) {
    let totInvoiceAmount = 0;
    if (data.length > 0) {
      data.forEach(value => {
        if (value?.invoiceAmount !== undefined) {
          totInvoiceAmount += value.invoiceAmount;
        }
      });
    }
    this.selectedInvoices.emit(data);
    this.selectedTotal.patchValue(totInvoiceAmount);
  }
}
