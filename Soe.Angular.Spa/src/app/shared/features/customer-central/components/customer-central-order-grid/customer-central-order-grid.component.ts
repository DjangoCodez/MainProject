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
  CustomerCentralOrderDTO,
  CustomerInvoiceGridDTO,
  CustomerInvoiceRowDetailDTO,
} from '../../models/customer-central.model';
import { CustomerCentralOrderForm } from '../../models/customer-central-order-form.model';
import { ValidationHandler } from '@shared/handlers';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-customer-central-order-grid',
  templateUrl: './customer-central-order-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerCentralOrderGridComponent
  extends GridBaseDirective<CustomerInvoiceGridDTO>
  implements OnInit
{
  private coreService = inject(CoreService);
  private validationHandler = inject(ValidationHandler);

  @Input() rows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  @Input({ required: true }) currencyPermission: boolean = false;
  @Input({ required: true }) productSalesPricePermission: boolean = false;
  @Input({ required: true }) planningPermission: boolean = false;
  @Output() reloadGridData = new EventEmitter<boolean>();
  @Output() showOnlyOpen = new EventEmitter<boolean>();

  isOnlyOpen: FormControl<boolean | null> = new FormControl<boolean>(true);
  performDetailGridLoad = new Perform<ICustomerInvoiceRowDetailDTO[]>(
    this.progressService
  );

  attestStatesOrder: {}[] = [];
  attestStatesFullOrder: IAttestStateDTO[] = [];

  form: CustomerCentralOrderForm = new CustomerCentralOrderForm({
    validationHandler: this.validationHandler,
    element: new CustomerCentralOrderDTO(),
  });

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'orderGrid', {
      skipInitialLoad: true,
      lookups: [this.loadAttestStatesOrder()],
    });
    this.summarize();
  }

  summarize() {
    this.rows.subscribe(rows => {
      let _orderFilteredToBeInvoicedTotal = 0;
      let _orderFilteredTotal = 0;

      //filtered summary
      rows.forEach(y => {
        _orderFilteredTotal += y.totalAmount;
        _orderFilteredToBeInvoicedTotal += y.remainingAmount;
      });

      this.form.orderFilteredTotal.patchValue(_orderFilteredTotal);
      this.form.orderFilteredToBeInvoicedTotal.patchValue(
        _orderFilteredToBeInvoicedTotal
      );
    });
  }

  override selectionChanged(selectedRows: CustomerInvoiceGridDTO[]): void {
    //selected summary
    let _orderSelectedToBeInvoicedTotal = 0;
    let _orderSelectedTotal = 0;

    if (selectedRows?.length > 0) {
      selectedRows.forEach(y => {
        _orderSelectedTotal += y.totalAmount;
        _orderSelectedToBeInvoicedTotal += y.remainingAmount;
      });

      this.form.orderSelectedTotal.patchValue(_orderSelectedTotal);
      this.form.orderSelectedToBeInvoicedTotal.patchValue(
        _orderSelectedToBeInvoicedTotal
      );
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

  override onGridReadyToDefine(
    grid: GridComponent<CustomerInvoiceGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    const detailColumns: ColDef[] = [];

    this.translate
      .get([
        'common.customer.invoices.ordernr',
        'common.customer.invoices.projectnr',
        'common.customer.invoices.ordertype',
        'common.customer.invoices.rowstatus',
        'common.customer.invoices.status',
        'common.customer.invoices.responsible',
        'common.customer.invoices.participant',
        'common.customer.invoices.internaltext',
        'common.customer.invoices.payservice',
        'common.customer.invoices.deliveryaddress',
        'economy.supplier.invoice.amountincvat',
        'common.customer.invoices.amountexvat',
        'common.customer.invoices.foreignamount',
        'common.customer.invoices.currencycode',
        'common.customer.invoices.remainingamount',
        'common.customer.invoices.remainingamountexvat',
        'common.customer.invoices.orderdate',
        'common.customer.invoices.fixedpriceordertype',
        'common.customer.invoices.deliverydate',
        'common.customer.invoices.seqnr',
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
        detailColumns.push(ColumnUtil.createColumnRowSelection());
        detailColumns.push(
          ColumnUtil.createColumnText(
            'rowNr',
            terms['common.customer.invoices.row'],
            { enableHiding: true, pinned: 'left', width: 100, returnable: true }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnIcon('rowTypeIcon', '', {
            enableHiding: true,
            pinned: 'left',
            returnable: true,
          })
        );

        detailColumns.push(
          this.grid.addColumnSingleValue({ returnable: true, forDetail: true })
        );

        detailColumns.push(
          ColumnUtil.createColumnText(
            'ediTextValue',
            terms['common.customer.invoices.edi'],
            { flex: 1, enableHiding: true }
          )
        );
        detailColumns.push(
          ColumnUtil.createColumnText(
            'productNr',
            terms['common.customer.invoices.productnr'],
            { flex: 1, enableHiding: true }
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
        detailColumns.push(
          ColumnUtil.createColumnShape('attestStateColor', '', {
            width: 40,
            shape: 'circle',
            colorField: 'attestStateColor',
            tooltipField: 'attestStateName',
            pinned: 'right',
          })
        );

        this.grid.enableMasterDetail(
          {
            detailRowHeight: 120,
            floatingFiltersHeight: 0,

            columnDefs: detailColumns,
          },
          {
            autoHeight: false,
            getDetailRowData: (params: ICustomerInvoiceRowDetailDTO) => {
              this.loadDetailRows(params, SoeOriginType.Order);
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

        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'invoiceNr',
          terms['common.customer.invoices.ordernr'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'projectNr',
          terms['common.customer.invoices.projectnr'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'orderTypeName',
          terms['common.customer.invoices.ordertype'],
          { flex: 1, enableHiding: true }
        );

        this.grid.addColumnSingleValue();

        this.grid.addColumnText(
          'attestStateNames',
          terms['common.customer.invoices.rowstatus'],
          { flex: 1 }
        );
        // var colAttestStates = this.orderGridOptions.addColumnSelect("attestStateNames", this.terms["common.customer.invoices.rowstatus"], null, { displayField: "attestStateNames", selectOptions: this.attestStatesOrder, shapeValueField: "attestStateColor", shape: Constants.SHAPE_CIRCLE }); //, null, null, null, null, null, null, null, null, "attestStateColor", Constants.SHAPE_CIRCLE);
        // (<any>colAttestStates).filters = [
        //     {
        //         condition: (term, value, row, column) => {
        //             if (term === this.terms["common.customer.invoices.multiplestatuses"]) {
        //                 return (<string>value).indexOf(',') >= 0 ? true : false;
        //             }
        //             else {
        //                 return (<string>value).indexOf(term) >= 0 ? true : false;
        //             }
        //         }
        //     }
        // ];
        this.grid.addColumnText(
          'statusName',
          terms['common.customer.invoices.status'],
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
          'invoicePaymentServiceName',
          terms['common.customer.invoices.payservice'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'deliveryAddress',
          terms['common.customer.invoices.deliveryaddress'],
          { flex: 1, enableHiding: true }
        );

        if (this.productSalesPricePermission) {
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
          if (this.currencyPermission) {
            this.grid.addColumnNumber(
              'totalAmountCurrency',
              terms['common.customer.invoices.foreignamount'],
              { flex: 1, enableHiding: true, decimals: 2 }
            );
            this.grid.addColumnText(
              'currencyCode',
              terms['common.customer.invoices.currencycode'],
              { flex: 1, enableHiding: true }
            );
          }

          this.grid.addColumnNumber(
            'remainingAmount',
            terms['common.customer.invoices.remainingamount'],
            { flex: 1, enableHiding: true, decimals: 2 }
          );
          this.grid.addColumnNumber(
            'remainingAmountExVat',
            terms['common.customer.invoices.remainingamountexvat'],
            { flex: 1, enableHiding: true, decimals: 2 }
          );
        }

        this.grid.addColumnDate(
          'invoiceDate',
          terms['common.customer.invoices.orderdate'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'fixedPriceOrderName',
          terms['common.customer.invoices.fixedpriceordertype'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnDate(
          'deliveryDate',
          terms['common.customer.invoices.deliverydate'],
          { flex: 1 }
        );
        this.grid.addColumnIcon('', ' ', {
          iconName: 'pen',
          tooltipField: 'statusIconMessage',
          onClick: row => this.openOrder(row),
        });
        this.grid.addColumnIcon('statusIconValue', ' ', {
          showIcon: 'statusIconValue',
          iconClass: 'warningColor',
          tooltipField: 'statusIconMessage',
        });

        if (this.planningPermission) {
          this.grid.addColumnShape('shiftTypeColor', '', {
            flex: 1,
            alignCenter: true,
            enableHiding: true,
            shape: 'square',
            colorField: 'shiftTypeColor',
            tooltipField: 'shiftTypeName',
          });
        }
        this.grid.addColumnText(
          'seqNr',
          terms['common.customer.invoices.seqnr'],
          { alignLeft: true }
        );

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

  showOnlyOpenChange(showOpen: boolean) {
    this.showOnlyOpen.emit(showOpen);
  }

  openOrder(row: CustomerInvoiceGridDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/order/status/default.aspx?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }
}
