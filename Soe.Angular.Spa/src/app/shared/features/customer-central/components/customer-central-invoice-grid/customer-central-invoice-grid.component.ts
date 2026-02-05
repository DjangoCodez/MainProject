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
  SoeModule,
  SoeOriginType,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { IAttestStateDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { NumberUtil } from '@shared/util/number-util';
import { ColumnUtil } from '@ui/grid/util/column-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ColDef } from 'ag-grid-enterprise';
import { BehaviorSubject, take, tap } from 'rxjs';
import {
  CustomerCentralInvoiceDTO,
  CustomerInvoiceGridDTO,
  CustomerInvoiceRowDetailDTO,
} from '../../models/customer-central.model';
import { CustomerCentralInvoiceForm } from '../../models/customer-central-invoice-form.model';
import { ValidationHandler } from '@shared/handlers/validation.handler';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-customer-central-invoice-grid',
  templateUrl: './customer-central-invoice-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerCentralInvoiceGridComponent
  extends GridBaseDirective<CustomerInvoiceGridDTO>
  implements OnInit
{
  private coreService = inject(CoreService);
  private validationHandler = inject(ValidationHandler);

  @Input() rows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  @Input({ required: true }) currencyPermission: boolean = false;
  @Input({ required: true }) exportPermission: boolean = false;
  @Input({ required: true }) productSalesPricePermission: boolean = false;
  @Output() reloadGridData = new EventEmitter<boolean>();
  @Output() showOnlyOpen = new EventEmitter<boolean>();
  isOnlyOpen: FormControl<boolean | null> = new FormControl<boolean>(true);
  performDetailGridLoad = new Perform<ICustomerInvoiceRowDetailDTO[]>(
    this.progressService
  );

  attestStatesOrder: {}[] = [];
  attestStatesFullOrder: IAttestStateDTO[] = [];

  form: CustomerCentralInvoiceForm = new CustomerCentralInvoiceForm({
    validationHandler: this.validationHandler,
    element: new CustomerCentralInvoiceDTO(),
  });

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'customerInvoiceGrid', {
      skipInitialLoad: true,
      lookups: [this.loadAttestStatesOrder()],
    });
    this.summarize();
  }

  summarize() {
    this.rows.subscribe(rows => {
      let _invoiceFilteredToPay = 0;
      let _invoiceFilteredTotal = 0;

      //filtered summary
      rows.forEach(y => {
        _invoiceFilteredTotal += y.totalAmount;
        _invoiceFilteredToPay += y.payAmount;
      });

      this.form.invoiceFilteredTotal.patchValue(_invoiceFilteredTotal);
      this.form.invoiceFilteredToPay.patchValue(_invoiceFilteredToPay);
    });
  }

  override selectionChanged(selectedRows: CustomerInvoiceGridDTO[]): void {
    //selected summary
    let _invoiceSelectedToPay = 0;
    let _invoiceSelectedTotal = 0;

    if (selectedRows?.length > 0) {
      selectedRows.forEach(y => {
        _invoiceSelectedTotal += y.totalAmount;
        _invoiceSelectedToPay += y.payAmount;
      });

      this.form.invoiceSelectedTotal.patchValue(_invoiceSelectedTotal);
      this.form.invoiceSelectedToPay.patchValue(_invoiceSelectedToPay);
    }
  }

  override createGridToolbar(): void {
    this.toolbarService.clearItemGroups();
    super.createGridToolbar({
      reloadOption: {
        onAction: () => this.reloadGridData.emit(true),
      },
    });
  }

  showOnlyOpenChange(showOpen: boolean) {
    this.showOnlyOpen.emit(showOpen);
  }

  setupGrid(grid: GridComponent<CustomerInvoiceGridDTO>): void {
    super.setupGrid(grid, '');

    const detailColumns: ColDef[] = [];

    this.translate
      .get([
        'common.customer.invoices.seqnr',
        'common.customer.invoices.invoicenr',
        'common.customer.customer.invoicedeliverytypeshort',
        'common.customer.invoices.ordernr',
        'common.customer.invoices.type',
        'common.customer.invoices.status',
        'common.customer.invoices.export',
        'common.customer.invoices.responsible',
        'common.customer.invoices.participant',
        'common.customer.invoices.internaltext',
        'common.customer.invoices.deliveryaddress',
        'common.customer.invoices.payservice',
        'economy.supplier.invoice.amountincvat',
        'common.customer.invoices.amountexvat',
        'common.customer.invoices.amount',
        'common.customer.invoices.foreignamount',
        'common.customer.invoices.currencycode',
        'economy.import.payment.invoicetotalamount',
        'common.customer.invoices.payamountcurrency',
        'common.customer.invoices.projectnr',
        'common.customer.invoices.invoicedate',
        'common.customer.invoices.duedate',
        'common.customer.invoices.paydate',
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
        this.terms = terms;
        detailColumns.push(ColumnUtil.createColumnRowSelection());
        detailColumns.push(
          ColumnUtil.createColumnText(
            'rowNr',
            terms['common.customer.invoices.row'],
            { width: 100, enableHiding: true, pinned: 'left' }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnIcon('rowTypeIcon', '', {
            editable: false,
            enableHiding: true,
            pinned: 'left',
          })
        );

        detailColumns.push(
          this.grid.addColumnSingleValue({ returnable: true, forDetail: true })
        );

        detailColumns.push(
          ColumnUtil.createColumnText(
            'ediTextValue',
            terms['common.customer.invoices.edi'],
            { flex: 1, enableHiding: true, editable: false }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnText(
            'productNr',
            terms['common.customer.invoices.productnr'],
            { enableHiding: true, flex: 1 }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnText(
            'text',
            terms['common.customer.invoices.productname'],
            { flex: 1, enableHiding: true }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnNumber(
            'quantity',
            terms['common.customer.invoices.quantity'],
            { flex: 1, enableHiding: true }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnText(
            'productUnitCode',
            terms['common.customer.invoices.unit'],
            { flex: 1, enableHiding: true }
          )
        );
        if (this.productSalesPricePermission) {
          detailColumns.push(
            ColumnUtil.createColumnNumber(
              'amountCurrency',
              terms['common.customer.invoices.price'],
              { flex: 1, enableHiding: true, decimals: 2 }
            )
          );
          detailColumns.push(
            ColumnUtil.createColumnNumber(
              'discountValue',
              terms['common.customer.invoices.discount'],
              { flex: 1, enableHiding: true, decimals: 2 }
            )
          );
          detailColumns.push(
            ColumnUtil.createColumnText(
              'discountTypeText',
              terms['common.customer.invoices.type'],
              { flex: 1, enableHiding: true }
            )
          );
          detailColumns.push(
            ColumnUtil.createColumnNumber(
              'sumAmountCurrency',
              terms['common.customer.invoices.sum'],
              { flex: 1, enableHiding: true, decimals: 2 }
            )
          );
        }

        this.grid.enableMasterDetail(
          {
            detailRowHeight: 120,
            floatingFiltersHeight: 0,

            columnDefs: detailColumns,
          },
          {
            autoHeight: false,
            getDetailRowData: (params: ICustomerInvoiceRowDetailDTO) => {
              this.loadDetailRows(params, SoeOriginType.CustomerInvoice);
            },
          }
        );

        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'seqNr',
          terms['common.customer.invoices.seqnr'],
          { flex: 1, alignLeft: true }
        );
        this.grid.addColumnText(
          'invoiceNr',
          terms['common.customer.invoices.invoicenr'],
          { flex: 1 }
        );

        this.grid.addColumnSingleValue();

        this.grid.addColumnText(
          'deliveryTypeName',
          terms['common.customer.customer.invoicedeliverytypeshort'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'orderNumbers',
          terms['common.customer.invoices.ordernr'],
          { flex: 1, enableHiding: true, hide: true }
        );
        this.grid.addColumnText(
          'billingTypeName',
          terms['common.customer.invoices.type'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'statusName',
          terms['common.customer.invoices.status'],
          { flex: 1 }
        );
        if (this.exportPermission)
          this.grid.addColumnText(
            'exportStatusName',
            terms['common.customer.invoices.export'],
            { flex: 1, enableHiding: true }
          );

        this.grid.addColumnText(
          'mainUserName',
          terms['common.customer.invoices.responsible'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'users',
          terms['common.customer.invoices.participant'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'internalText',
          terms['common.customer.invoices.internaltext'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'deliveryAddress',
          terms['common.customer.invoices.deliveryaddress'],
          { flex: 1, enableHiding: true }
        );

        this.grid.addColumnText(
          'invoicePaymentServiceName',
          terms['common.customer.invoices.payservice'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnNumber(
          'totalAmount',
          terms['economy.supplier.invoice.amountincvat'],
          { flex: 1, enableHiding: true, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'totalAmountExVat',
          terms['common.customer.invoices.amountexvat'],
          { flex: 1, enableHiding: true, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'totalAmount',
          terms['common.customer.invoices.amount'],
          { flex: 1, enableHiding: true, decimals: 2 }
        );

        if (this.currencyPermission) {
          this.grid.addColumnNumber(
            'totalAmountCurrency',
            terms['common.customer.invoices.foreignamoun'],
            { flex: 1, enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnText(
            'currencyCode',
            terms['common.customer.invoices.currencycode'],
            { flex: 1, enableHiding: false }
          );
        }
        this.grid.addColumnNumber(
          'payAmount',
          terms['economy.import.payment.invoicetotalamount'],
          { flex: 1, enableHiding: true, decimals: 2 }
        );

        if (this.currencyPermission)
          this.grid.addColumnNumber(
            'payAmountCurrency',
            terms['common.customer.invoices.payamountcurrency'],
            { flex: 1, enableHiding: true, decimals: 2 }
          );

        this.grid.addColumnText(
          'projectNr',
          terms['common.customer.invoices.projectnr'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnDate(
          'invoiceDate',
          terms['common.customer.invoices.invoicedate'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'dueDate',
          terms['common.customer.invoices.duedate'],
          { flex: 1 }
        );
        this.grid.addColumnDate(
          'payDate',
          terms['common.customer.invoices.paydate'],
          { flex: 1 }
        );
        this.grid.addColumnIcon('billingIconValue', ' ', {
          showIcon: 'billingIconValue',
          tooltipField: 'billingIconMessage',
        });
        this.grid.addColumnIcon('', ' ', {
          iconName: 'pen',
          tooltipField: 'statusIconMessage',
          onClick: row => this.openInvoice(row),
        });
        this.grid.addColumnIcon('statusIconValue', ' ', {
          showIcon: 'statusIconValue',
          tooltipField: 'statusIconMessage',
        });
        this.grid.addColumnShape('paidStatusColor', '', {
          flex: 1,
          alignCenter: true,
          enableHiding: true,
          shape: 'circle',
          colorField: 'paidStatusColor',
          tooltipField: 'paidInfo',
        });
        this.grid.addColumnText(
          'projectNr',
          terms['common.customer.invoices.projectnr'],
          { flex: 1, enableHiding: true }
        );

        this.grid.setSingelValueConfigurationForDetail([
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
        ]);

        this.grid.setNbrOfRowsToShow(10, 10);
        super.finalizeInitGrid();
      });
  }

  loadAttestStatesOrder() {
    return this.coreService
      .getAttestStates(TermGroup_AttestEntity.Order, SoeModule.Billing, false)
      .pipe(
        tap(x => {
          this.attestStatesFullOrder = x;
          this.attestStatesOrder.push({
            value: this.terms['common.customer.invoice.norows'],
            label: this.terms['common.customer.invoice.norows'],
          });
          x.forEach(a => {
            this.attestStatesOrder.push({ value: a.name, label: a.name });
          });
          this.attestStatesOrder.push({
            value: this.terms['common.customer.invoices.multiplestatuses'],
            label: this.terms['common.customer.invoices.multiplestatuses'],
          });
        })
      );
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
                if (row.attestStateId) {
                  if (originType === SoeOriginType.Order) {
                    const attestStateOrder = this.attestStatesFullOrder.find(
                      a => a.attestStateId === row.attestStateId
                    );
                    if (attestStateOrder) {
                      dataRow.attestStateName = attestStateOrder.name;
                      dataRow.attestStateColor = attestStateOrder.color;
                    }
                  }
                }
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

  openInvoice(row: CustomerInvoiceGridDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/invoice/status/?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }
}
