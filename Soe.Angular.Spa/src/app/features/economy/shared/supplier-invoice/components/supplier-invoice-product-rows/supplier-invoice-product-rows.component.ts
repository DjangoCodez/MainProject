import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SupplierInvoiceProductRowsService } from '../../services/supplier-invoice-product-rows.service';
import { SupplierInvoiceProductRowDTO } from '../../models/supplier-invoice-product-rows.model';
import { TransferSupplierProductRowsModel } from '../../models/transfer-supplier-product-rows.model';
import { Observable } from 'rxjs';
import { TermCollection } from '@shared/localization/term-types';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  Feature,
  SoeOriginType,
  SupplierInvoiceRowType,
} from '@shared/models/generated-interfaces/Enumerations';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { CommonCustomerService } from '@features/billing/shared/services/common-customer.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { tap } from 'rxjs/operators';
import { BrowserUtil } from '@shared/util/browser-util';
import { CrudActionTypeEnum } from '@shared/enums';
import { Perform } from '@shared/util/perform.class';
import { SelectComponent } from '@ui/forms/select/select.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { SelectCustomerInvoiceDialogComponent } from '@shared/components/select-customer-invoice-dialog/component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import { CustomerInvoiceSearchDTO } from '@shared/components/select-customer-invoice-dialog/model/customer-invoice-search.model';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';

@Component({
  selector: 'soe-supplier-invoice-product-rows',
  templateUrl: './supplier-invoice-product-rows.component.html',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    GridWrapperComponent,
    SelectComponent,
    ButtonComponent,
  ],
  providers: [
    FlowHandlerService,
    ToolbarService,
    SupplierInvoiceProductRowsService,
  ],
})
export class SupplierInvoiceProductRowsComponent
  extends GridBaseDirective<
    SupplierInvoiceProductRowDTO,
    SupplierInvoiceProductRowsService
  >
  implements OnInit
{
  service = inject(SupplierInvoiceProductRowsService);
  private readonly commonCustomerService = inject(CommonCustomerService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly dialogService = inject(DialogService);

  supplierInvoiceId = input.required<number>();

  wholesellerId = signal<number>(0);
  wholesellers: SmallGenericType[] = [];
  selectedRowCount = signal<number>(0);

  transferDisabled = computed(
    () => this.selectedRowCount() === 0 || this.wholesellerId() === 0
  );

  // Form for the wholeseller dropdown
  transferForm = new FormGroup({
    wholesellerId: new FormControl<number>(0),
  });

  performAction = new Perform<SupplierInvoiceProductRowsService>(
    this.progressService
  );

  ngOnInit() {
    super.ngOnInit();

    // Sync form control value changes to signal
    this.transferForm.get('wholesellerId')?.valueChanges.subscribe(value => {
      if (value !== null && value !== undefined) {
        this.wholesellerId.set(value);
      }
    });

    this.startFlow(
      Feature.Economy_Supplier_Invoice_ProductRows,
      'economy.supplier.invoice.productrows',
      {
        lookups: [this.loadWholesellers()],
      }
    );
  }

  override loadData(): Observable<SupplierInvoiceProductRowDTO[]> {
    return super
      .loadData(this.supplierInvoiceId())
      .pipe(tap(rows => this.addRowIcons(rows)));
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.sum',
      'common.productnr',
      'common.quantity',
      'common.unit',
      'common.purchaseprice',
      'common.amount',
      'common.text',
      'common.customer.invoices.ordernr',
      'core.continue',
      'economy.supplier.invoice.productrows.verifytransfer',
      'common.customer.invoices.selectorder',
      'core.error',
      'common.order',
    ]);
  }

  override onGridReadyToDefine(
    grid: GridComponent<SupplierInvoiceProductRowDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.grid.enableRowSelection();

    this.grid.addColumnIcon('rowTypeIcon', '', {
      enableHiding: false,
      pinned: 'left',
      width: 50,
    });

    this.grid.addColumnText(
      'sellerProductNumber',
      this.terms['common.productnr'],
      {
        enableHiding: true,
        flex: 1,
      }
    );

    // Add single value column before the columns that should span
    this.grid.addColumnSingleValue();

    this.grid.addColumnText('text', this.terms['common.text'], {
      enableHiding: false,
      flex: 2,
    });

    this.grid.addColumnNumber('quantity', this.terms['common.quantity'], {
      enableHiding: false,
      flex: 1,
      decimals: 2,
      maxDecimals: 4,
      editable: false,
    });

    this.grid.addColumnText('unitCode', this.terms['common.unit'], {
      enableHiding: true,
      flex: 1,
    });

    this.grid.addColumnNumber(
      'priceCurrency',
      this.terms['common.purchaseprice'],
      {
        enableHiding: false,
        flex: 1,
        decimals: 2,
      }
    );

    this.grid.addColumnNumber('amountCurrency', this.terms['common.amount'], {
      enableHiding: false,
      flex: 1,
      decimals: 2,
    });

    this.grid.addColumnText(
      'customerInvoiceNumber',
      this.terms['common.customer.invoices.ordernr'],
      {
        enableHiding: true,
        flex: 1,
        buttonConfiguration: {
          iconPrefix: 'fal',
          iconName: 'pencil',
          iconClass: 'iconEdit',
          onClick: row => this.openOrder(row),
          show: row => !!(row && row.customerInvoiceId),
        },
      }
    );

    // Set span configuration for text rows - MUST be called after all columns are defined
    this.grid.setSingelValueConfiguration([
      {
        field: 'text',
        predicate: (data: SupplierInvoiceProductRowDTO) =>
          data.rowType === SupplierInvoiceRowType.TextRow,
        editable: false,
        spanTo: 'amountCurrency',
      },
    ]);

    super.finalizeInitGrid();
  }

  private addRowIcons(rows: SupplierInvoiceProductRowDTO[]): void {
    rows.forEach(row => {
      row.rowTypeIcon =
        row.rowType === SupplierInvoiceRowType.ProductRow ? 'box-alt' : 'text';
    });
  }

  selectionChanged(selectedRows: SupplierInvoiceProductRowDTO[]): void {
    this.selectedRowCount.set(selectedRows.length);
  }

  private loadWholesellers(): Observable<SmallGenericType[]> {
    return this.commonCustomerService.getSysWholesellersDict(true).pipe(
      tap((data: SmallGenericType[]) => {
        this.wholesellers = data;
        if (data.length > 0) {
          const firstId = data[0].id;
          this.wholesellerId.set(firstId);
          this.transferForm.patchValue({ wholesellerId: firstId });
        }
      })
    );
  }

  private openOrder(row: SupplierInvoiceProductRowDTO) {
    if (!row.customerInvoiceId) return;

    BrowserUtil.openInNewWindow(
      window,
      `/soe/billing/order/status/default.aspx?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.customerInvoiceNumber}`
    );
  }

  showTransferDialog() {
    if (!this.flowHandler.modifyPermission()) return;

    const selectedRows = this.grid.getSelectedRows();

    if (!selectedRows || selectedRows.length === 0) {
      return;
    }

    const supplierInvoiceProductRowIds = selectedRows.map(
      r => r.supplierInvoiceProductRowId
    );

    // Open customer invoice selection dialog
    const dialogData: CustomerInvoiceSearchDTO = {
      title: this.terms['common.customer.invoices.selectorder'],
      originType: SoeOriginType.Order,
      isNew: false,
      ignoreChildren: false,
      invoiceValue: undefined,
      size: 'lg',
    };

    this.dialogService
      .open(SelectCustomerInvoiceDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result && result.customerInvoiceId) {
          this.startTransfer(
            result.customerInvoiceId,
            supplierInvoiceProductRowIds
          );
        }
      });
  }

  private startTransfer(
    customerInvoiceId: number,
    supplierInvoiceProductRowIds: number[]
  ) {
    const message = this.terms[
      'economy.supplier.invoice.productrows.verifytransfer'
    ].replace('{0}', String(supplierInvoiceProductRowIds.length));

    this.messageboxService
      .question('core.continue', message)
      .afterClosed()
      .subscribe((response: IMessageboxComponentResponse) => {
        if (response.result) {
          this.performTransfer(customerInvoiceId, supplierInvoiceProductRowIds);
        }
      });
  }

  private performTransfer(
    customerInvoiceId: number,
    supplierInvoiceProductRowIds: number[]
  ) {
    const model: TransferSupplierProductRowsModel = {
      customerInvoiceId,
      supplierInvoiceId: this.supplierInvoiceId(),
      wholesellerId: this.wholesellerId(),
      supplierInvoiceProductRowIds,
    };

    this.performAction.crud(
      CrudActionTypeEnum.Work,
      this.service.transferToOrder(model),
      this.refreshGrid.bind(this)
    );
  }
}
