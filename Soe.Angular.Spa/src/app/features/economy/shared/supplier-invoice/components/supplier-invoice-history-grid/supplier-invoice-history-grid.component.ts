import {
  Component,
  effect,
  inject,
  input,
  OnInit,
  output,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SupplierInvoiceHistoryService } from '../../services/supplier-invoice-history.service';
import { SupplierInvoiceHistoryGridDTO } from '../../models/supplier-invoice-history.model';
import { Observable } from 'rxjs';
import { TermCollection } from '@shared/localization/term-types';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { RowDoubleClickedEvent } from 'ag-grid-enterprise';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { EditComponentDialogData } from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';
import { SupplierInvoiceForm } from '../../models/supplier-invoice-form.model';
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import { SupplierInvoiceDTO } from '../../models/supplier-invoice.model';
import { ValidationHandler } from '@shared/handlers';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { SupplierInvoiceHistoryDetailsComponent } from '../supplier-invoice-history-details/supplier-invoice-history-details.component';

@Component({
  selector: 'soe-supplier-invoice-history-grid',
  templateUrl: './supplier-invoice-history-grid.component.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class SupplierInvoiceHistoryGridComponent
  extends GridBaseDirective<
    SupplierInvoiceHistoryGridDTO,
    SupplierInvoiceHistoryService
  >
  implements OnInit
{
  service = inject(SupplierInvoiceHistoryService);
  dialogService = inject(DialogService);
  validationHandler = inject(ValidationHandler);

  supplierId = input.required<number>();
  accountingRowsChosen = output<AccountingRowDTO[]>();

  constructor() {
    super();
    effect(() => {
      this.refreshGrid();
    });
  }

  ngOnInit() {
    this.startFlow(
      Feature.Economy_Customer_Invoice_Invoices,
      'economy.supplier.invoice.history',
      {
        skipInitialLoad: true,
      }
    );
  }

  override loadData(): Observable<SupplierInvoiceHistoryGridDTO[]> {
    return this.performLoadData.load$(this.service.getGrid(this.supplierId()));
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.supplier.invoice.invoicenr',
      'economy.supplier.invoice.amountincvat',
      'economy.supplier.invoice.invoicedate',
      'economy.supplier.invoice.duedate',
      'economy.supplier.invoice.attestgroup',
      'economy.supplier.invoice.history',
      'economy.supplier.payment.paymentdate',
    ]);
  }

  override onGridReadyToDefine(
    grid: GridComponent<SupplierInvoiceHistoryGridDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.loadTerms().subscribe(() => {
      this.grid.addColumnText(
        'invoiceNr',
        this.terms['economy.supplier.invoice.invoicenr'],
        {
          enableHiding: false,
          hide: false,
          flex: 1,
        }
      );
      this.grid.addColumnText(
        'approvalGroup',
        this.terms['economy.supplier.invoice.attestgroup'],
        {
          enableHiding: true,
          hide: false,
          flex: 1,
        }
      );
      this.grid.addColumnDate(
        'invoiceDate',
        this.terms['economy.supplier.invoice.invoicedate'],
        {
          enableHiding: false,
          hide: false,
          flex: 1,
        }
      );
      this.grid.addColumnDate(
        'paymentDate',
        this.terms['economy.supplier.payment.paymentdate'],
        {
          enableHiding: true,
          hide: false,
          flex: 1,
        }
      );
      this.grid.addColumnNumber(
        'totalAmount',
        this.terms['economy.supplier.invoice.amountincvat'],
        {
          enableHiding: false,
          hide: false,
          flex: 1,
        }
      );

      super.finalizeInitGrid();
    });

    this.grid.onRowDoubleClicked = this.doubleClickInvoice.bind(this);
  }

  private doubleClickInvoice(
    event: RowDoubleClickedEvent<SupplierInvoiceHistoryGridDTO, any>
  ) {
    const row = { ...(event.data as SupplierInvoiceHistoryGridDTO) };
    this.viewInvoice(row);
  }

  viewInvoice(row: SupplierInvoiceHistoryGridDTO) {
    if (!row.invoiceId) return;

    const dialogData: EditComponentDialogData<
      SupplierInvoiceDTO,
      SupplierInvoiceService,
      SupplierInvoiceForm,
      number[]
    > = {
      title: this.translate.instant('economy.supplier.invoice.history'),
      size: 'xl',
      maxHeight: '90vh',
      hasBackdrop: false,
      hideFooter: true,
      parameters: this.rowData.value.map(r => r.invoiceId),
      form: new SupplierInvoiceForm(
        {
          validationHandler: this.validationHandler,
          element: row as any as SupplierInvoiceDTO,
        },
        true // isReadOnly
      ),
      editComponent: SupplierInvoiceHistoryDetailsComponent,
    };
    this.dialogService
      .openEditComponent(dialogData)
      .afterClosed()
      .subscribe((value: { response: AccountingRowDTO[] }) => {
        if (!value.response?.length) return;

        this.accountingRowsChosen.emit(value.response);
      });
  }
}
