import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
  TermGroup,
  TermGroup_StockTransactionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IStockTransactionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { AggregationType } from '@ui/grid/interfaces';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';
import {
  IAutocompleteIStockProductDTO,
  StockTransactionDTO,
} from '../../models/stock-balance.model';

@Component({
  selector: 'soe-stock-balance-edit-stock-transaction',
  templateUrl: './stock-balance-edit-stock-transaction.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockBalanceEditStockTransactionComponent
  extends GridBaseDirective<StockTransactionDTO>
  implements OnInit, OnChanges
{
  @Input() transactions: StockTransactionDTO[] = [];
  @Input({ required: true }) showVoucherColumn!: boolean;
  @Input({ required: false }) editMode!: boolean;

  transactionRows = new BehaviorSubject<StockTransactionDTO[]>([]);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);

  actionTypes: ISmallGenericType[] = [];

  performLoadActionTypes = new Perform<SmallGenericType[]>(
    this.progressService
  );
  productStocksData: IAutocompleteIStockProductDTO[] = [];

  hasPriceChangePermission = signal(true);
  private availableScreenHeight = signal(0);
  private toolbarHeight = 235;

  gridHeight = computed(() => {
    return this.availableScreenHeight() - 374;
  });

  ngOnInit(): void {
    super.ngOnInit();

    this.availableScreenHeight.set(window.innerHeight - this.toolbarHeight);
    this.startFlow(
      Feature.Billing_Stock,
      'Billing.Stock.StockSaldo.StockTransactions',
      {
        skipInitialLoad: true,
        lookups: [this.loadActionTypes()],
      }
    );
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.hasPriceChangePermission.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Stock_Change_AvgPrice)
    );
  }

  override onGridReadyToDefine(grid: GridComponent<StockTransactionDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.stock.stocksaldo.actiontype',
        'billing.stock.stocksaldo.actionquantity',
        'billing.stock.stocksaldo.actionprice',
        'billing.stock.stocksaldo.actionnote',
        'billing.stock.stocksaldo.actioncreated',
        'billing.stock.stocksaldo.actioncreatedby',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'billing.stock.stocks.stock',
        'billing.stock.stocksaldo.productnumber',
        'billing.stock.stocksaldo.stocktransactions',
        'common.created',
        'billing.stock.stocksaldo.vouchernr',
        'core.source',
        'billing.stock.stocksaldo.stockplaceto',
        'core.total',
        'billing.stock.stocksaldo.avgprice',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (this.editMode) {
          this.grid.addColumnModified('isModified');
          this.grid.addColumnText(
            'productNr',
            terms['billing.stock.stocksaldo.productnumber'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
          this.grid.addColumnText(
            'stockName',
            terms['billing.stock.stocks.stock'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
        }
        this.grid.addColumnText(
          'actionTypeName',
          terms['billing.stock.stocksaldo.actiontype'],
          {
            flex: 1,
            enableHiding: false,
            showSetFilter: true,
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.stock.stocksaldo.actionquantity'],
          {
            decimals: 2,
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'price',
          terms['billing.stock.stocksaldo.actionprice'],
          {
            decimals: 2,
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'avgPrice',
          terms['billing.stock.stocksaldo.avgprice'],
          {
            decimals: 2,
            flex: 1,
            enableHiding: true,
            clearZero: true,
          }
        );
        this.grid.addColumnText(
          'note',
          terms['billing.stock.stocksaldo.actionnote'],
          {
            flex: 5,
            enableHiding: false,
          }
        );
        this.grid.addColumnText('sourceLabel', terms['core.source'], {
          flex: 5,
          enableHiding: true,
          showSetFilter: true,
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconName: 'pencil',
            onClick: row => this.openSource(row),
            show: row =>
              Boolean(
                row.stockInventoryHeadId ??
                  row.purchaseId ??
                  row.invoiceId ??
                  false
              ),
          },
        });
        this.grid.addColumnText(
          'childStockTransaction',
          terms['billing.stock.stocksaldo.stockplaceto'],
          {
            flex: 1,
            hide: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber('total', terms['core.total'], {
          flex: 1,
          enableHiding: true,
          hide: true,
          decimals: 2,
        });
        this.grid.addColumnDate(
          'transactionDate',
          terms['billing.stock.stocksaldo.actioncreated'],
          {
            flex: 2,
            enableHiding: false,
          }
        );

        if (!this.editMode) {
          this.grid.addColumnDate('created', terms['common.created'], {
            flex: 1,
          });
          this.grid.addColumnText(
            'createdBy',
            terms['billing.stock.stocksaldo.actioncreatedby'],
            {
              flex: 1,
            }
          );

          if (this.showVoucherColumn) {
            this.grid.addColumnText(
              'voucherNr',
              terms['billing.stock.stocksaldo.vouchernr'],
              {
                flex: 1,
                enableHiding: false,
                showSetFilter: true,
                buttonConfiguration: {
                  iconPrefix: 'fal',
                  iconName: 'pen',
                  onClick: (row: IStockTransactionDTO) => {
                    this.openEditInNewTab.emit({
                      id: row.voucherId || 0,
                      additionalProps: {
                        editComponent: VoucherEditComponent,
                        FormClass: VoucherForm,
                        editTabLabel: 'economy.accounting.voucher.voucher',
                      },
                    });
                  },
                  show: (row: IStockTransactionDTO) =>
                    row.voucherId != undefined,
                },
              }
            );
          }
        }

        if (this.editMode) {
          this.grid.addColumnIconDelete({
            onClick: r => this.grid.deleteRow(r),
          });
        }

        this.grid.addAggregationsRow({
          total: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
  }

  openSource(row: StockTransactionDTO): void {
    let url = '';

    if (row.stockInventoryHeadId) {
      url = `/soe/billing/stock/inventory/default.aspx?stockInventoryHeadId=${row.stockInventoryHeadId}`;
    } else if (row.purchaseId) {
      url = `/soe/billing/purchase/list/default.aspx?&purchaseId=${row.purchaseId}&purchaseNr=${row.sourceNr}`;
    } else if (row.originType === SoeOriginType.Order) {
      url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${row.invoiceId}&invoiceNr=${row.sourceNr}`;
    } else if (row.originType === SoeOriginType.CustomerInvoice) {
      url = `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.sourceNr}`;
    }

    if (url) {
      BrowserUtil.openInNewTab(window, url);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.transactions) {
      this.transactionRows.next(this.transactions);
    }
  }

  loadActionTypes(): Observable<SmallGenericType[]> {
    return this.performLoadActionTypes.load$(
      this.coreService
        .getTermGroupContent(TermGroup.StockTransactionType, false, false)
        .pipe(
          tap(data => {
            this.actionTypes = data.filter(x => x.id < 3 || x.id > 4);
            if (!this.hasPriceChangePermission()) {
              this.actionTypes = this.actionTypes.filter(
                x => x.id != TermGroup_StockTransactionType.AveragePriceChange
              );
            }
          })
        )
    );
  }
}
