import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ICustomerInvoiceRowDetailDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';
import {
  Feature,
  SoeInvoiceRowDiscountType,
  SoeInvoiceRowType,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { NumberUtil } from '@shared/util/number-util';
import { Perform } from '@shared/util/perform.class';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ColDef } from 'ag-grid-enterprise';
import { BehaviorSubject, take, tap } from 'rxjs';
import {
  CustomerInvoiceGridDTO,
  CustomerInvoiceRowDetailDTO,
} from '../../models/customer-central.model';

@Component({
  selector: 'soe-customer-central-contract-grid',
  templateUrl: './customer-central-contract-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerCentralContractGridComponent
  extends GridBaseDirective<CustomerInvoiceGridDTO>
  implements OnInit
{
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  @Input() rows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  @Input({ required: true }) productSalesPricePermission: boolean = false;
  @Output() reloadGridData = new EventEmitter<boolean>();
  @Output() showOnlyOpen = new EventEmitter<boolean>();

  isOnlyOpen: FormControl<boolean | null> = new FormControl<boolean>(true);
  performDetailGridLoad = new Perform<ICustomerInvoiceRowDetailDTO[]>(
    this.progressService
  );

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'contractGrid', { skipInitialLoad: true });
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideClearFilters: true,
    });
  }

  override refreshGrid(): void {
    this.reloadGridData.emit(true);
  }

  override onGridReadyToDefine(
    grid: GridComponent<CustomerInvoiceGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    const detailColumns: ColDef[] = [];
    this.translate
      .get([
        'common.customer.contracts.contractnumbershort',
        'common.customer.invoices.responsible',
        'common.customer.invoices.participant',
        'common.customer.invoices.internaltext',
        'common.categories',
        'economy.supplier.invoice.amountincvat',
        'common.customer.invoices.amountexvat',
        'common.startdate',
        'common.stopdate',
        'common.customer.contracts.nextperiod',
        'common.customer.contracts.contractgroup',
        'common.customer.invoices.row',
        'common.customer.invoices.edi',
        'common.customer.invoices.productnr',
        'common.customer.invoices.productname',
        'common.customer.invoices.quantity',
        'common.customer.invoices.unit',
        'common.customer.invoices.price',
        'common.customer.invoices.discount',
        'common.customer.invoices.type',
        'common.customer.invoices.sum',
        'common.customer.invoice.norows',
        'common.customer.invoices.multiplestatuses',
        'core.yes',
        'core.no',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        detailColumns.push(
          this.grid.addColumnText(
            'rowNr',
            terms['common.customer.invoices.row'],
            { enableHiding: true, pinned: 'left', width: 100, returnable: true }
          )
        );

        detailColumns.push(
          this.grid.addColumnIcon('rowTypeIcon', '', {
            enableHiding: true,
            pinned: 'left',
            returnable: true,
          })
        );

        detailColumns.push(
          this.grid.addColumnSingleValue({ returnable: true, forDetail: true })
        );

        detailColumns.push(
          this.grid.addColumnText(
            'ediTextValue',
            terms['common.customer.invoices.edi'],
            { flex: 1, enableHiding: true, returnable: true }
          )
        );
        detailColumns.push(
          this.grid.addColumnText(
            'productNr',
            terms['common.customer.invoices.productnr'],
            { flex: 1, enableHiding: true, returnable: true }
          )
        );
        detailColumns.push(
          this.grid.addColumnText(
            'text',
            terms['common.customer.invoices.productname'],
            { flex: 1, enableHiding: true, returnable: true }
          )
        );
        detailColumns.push(
          this.grid.addColumnNumber(
            'quantity',
            terms['common.customer.invoices.quantity'],
            { flex: 1, enableHiding: true, returnable: true }
          )
        );
        detailColumns.push(
          this.grid.addColumnText(
            'productUnitCode',
            terms['common.customer.invoices.unit'],
            { flex: 1, enableHiding: true, returnable: true }
          )
        );

        if (this.productSalesPricePermission) {
          detailColumns.push(
            this.grid.addColumnNumber(
              'amountCurrency',
              terms['common.customer.invoices.price'],
              { flex: 1, enableHiding: true, decimals: 2, returnable: true }
            )
          );
          detailColumns.push(
            this.grid.addColumnNumber(
              'discountValue',
              terms['common.customer.invoices.discount'],
              { flex: 1, enableHiding: true, decimals: 2, returnable: true }
            )
          );

          detailColumns.push(
            this.grid.addColumnText(
              'discountTypeText',
              terms['common.customer.invoices.type'],
              { flex: 1, enableHiding: true, returnable: true }
            )
          );
          detailColumns.push(
            this.grid.addColumnNumber(
              'sumAmountCurrency',
              terms['common.customer.invoices.sum'],
              { flex: 1, enableHiding: true, decimals: 2, returnable: true }
            )
          );
        }

        this.grid.enableMasterDetail(
          {
            floatingFiltersHeight: 0,
            columnDefs: detailColumns,
            detailRowHeight: 300,
          },
          {
            getDetailRowData: (params: ICustomerInvoiceRowDetailDTO) => {
              this.loadDetailRows(params, SoeOriginType.Contract);
            },
          },
          [
            {
              field: 'text',
              predicate: (data: CustomerInvoiceRowDetailDTO) =>
                data.type === SoeInvoiceRowType.TextRow,
            },
            {
              field: 'text',
              predicate: (data: CustomerInvoiceRowDetailDTO) =>
                data.type === SoeInvoiceRowType.PageBreakRow,
              editable: false,
              cellClass: 'bold',
            },
            {
              field: 'text',
              predicate: data => data.type === SoeInvoiceRowType.SubTotalRow,
              editable: true,
              cellClass: 'bold',
              cellRenderer: (data, value) => {
                const sum = data['sumAmountCurrency'] || '';
                return (
                  "<span class='pull-left' style='width:150px'>" +
                  value +
                  "</span><span class='pull-right' style='padding-left:5px;padding-right:2px;margin-right:-2px;background-color:#FFFFFF;'>" +
                  NumberUtil.formatDecimal(sum, 2) +
                  '</span>'
                );
              },
              spanTo: 'sumAmountCurrency',
            },
          ]
        );

        this.grid.addColumnText(
          'invoiceNr',
          terms['common.customer.contracts.contractnumbershort'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'mainUserName',
          terms['common.customer.invoices.responsible'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'users',
          terms['common.customer.invoices.participant'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'internalText',
          terms['common.customer.invoices.internaltext'],
          { flex: 1 }
        );
        this.grid.addColumnText('categories', terms['common.categories'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'totalAmount',
          terms['economy.supplier.invoice.amountincvat'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'totalAmountExVat',
          terms['common.customer.invoices.amountexvat'],
          { flex: 1 }
        );
        this.grid.addColumnText('invoiceDate', terms['common.startdate'], {
          flex: 1,
        });
        this.grid.addColumnText('dueDate', terms['common.stopdate'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'nextContractPeriod',
          terms['common.customer.contracts.nextperiod'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'contractGroupName',
          terms['common.customer.contracts.contractgroup'],
          { flex: 1 }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.openContract(row);
          },
        });

        this.grid.setNbrOfRowsToShow(10, 10);
        super.finalizeInitGrid();
      });
  }

  showOnlyOpenChange(showOpen: boolean) {
    this.showOnlyOpen.emit(showOpen);
  }

  onReloadGridData() {
    this.reloadGridData.emit(true);
  }

  loadDetailRows(params: any, originType: SoeOriginType) {
    params.successCallback([]);
    if (params.data.rowsLoaded) {
      params.successCallback(params.data.rows);
      return;
    }
    this.performDetailGridLoad.load(
      this.coreService
        .getCustomerInvoiceRowsSmall(params.data.customerInvoiceId)
        .pipe(
          tap(detailRow => {
            const dataRows: ICustomerInvoiceRowDetailDTO[] = [];
            detailRow
              .filter(
                row =>
                  row.type === SoeInvoiceRowType.ProductRow ||
                  row.type === SoeInvoiceRowType.TextRow ||
                  row.type === SoeInvoiceRowType.PageBreakRow ||
                  row.type === SoeInvoiceRowType.SubTotalRow
              )
              .forEach(row => {
                const dataRow = new CustomerInvoiceRowDetailDTO();
                Object.assign(dataRow, row);

                dataRow.ediTextValue = row.ediEntryId
                  ? this.terms['core.yes']
                  : this.terms['core.no'];
                dataRow.discountTypeText =
                  row.discountType === SoeInvoiceRowDiscountType.Percent
                    ? '%'
                    : params.data.currencyCode;

                if (row.isTimeProjectRow) {
                  dataRow.rowTypeIcon = 'clock';
                } else {
                  switch (row.type) {
                    case SoeInvoiceRowType.ProductRow:
                      dataRow.rowTypeIcon = 'box-alt';
                      break;
                    case SoeInvoiceRowType.TextRow:
                      dataRow.rowTypeIcon = 'text';
                      break;
                    case SoeInvoiceRowType.PageBreakRow:
                      dataRow.rowTypeIcon = 'cut';
                      break;
                    case SoeInvoiceRowType.SubTotalRow:
                      dataRow.rowTypeIcon = 'calculator-alt';
                      break;
                  }
                }

                dataRows.push(dataRow);
              });

            // need to be called after detail grid has been initialized for a specific row
            this.grid.updateSingleValueConfigurationForDetail(
              params.node.id,
              true
            );

            params.data.rows = dataRows;
            params.data.rowsLoaded = true;
            params.successCallback(dataRows);
          })
        )
    );
  }

  openContract(row: CustomerInvoiceGridDTO) {
    const url = `/soe/billing/contract/status/default.aspx?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.invoiceNr}`;

    BrowserUtil.openInNewTab(window, url);
  }
}
