import { Component, OnInit, inject } from '@angular/core';
import {
  EditDeliveryAddressComponent,
  IEditDeliveryAddressDialogData,
} from '@shared/components/billing/edit-delivery-address/edit-delivery-address.component';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseRowFromStockDTO } from '@shared/models/generated-interfaces/PurchaseDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellClassParams,
  CellEditingStartedEvent,
  CellValueChangedEvent,
} from 'ag-grid-community';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';
import { PurchaseProductsService } from '../../../purchase-products/services/purchase-products.service';
import { PurchaseService } from '../../../purchase/services/purchase.service';
import {
  StockPurchaseDTO,
  StockPurchaseFilterDTO,
} from '../../models/stock-purchase.model';
import { StockPurchaseService } from '../../services/stock-purchase.service';

@Component({
  selector: 'soe-stock-purchase-grid',
  templateUrl: './stock-purchase-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockPurchaseGridComponent
  extends GridBaseDirective<IPurchaseRowFromStockDTO, StockPurchaseService>
  implements OnInit
{
  service = inject(StockPurchaseService);
  private purchaseProductsService = inject(PurchaseProductsService);
  private purchaseService = inject(PurchaseService);
  public dialogService = inject(DialogService);
  public messageboxService = inject(MessageboxService);

  performGridLoad = new Perform<IPurchaseRowFromStockDTO[]>(
    this.progressService
  );
  performAction = new Perform<PurchaseService>(this.progressService);
  suppliers: Record<number, SmallGenericType> = [];
  suppliersDict: SmallGenericType[] = [];
  performSupplierFilter = new Perform<SmallGenericType[]>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Stock_Purchase,
      'Soe.Billing.Stock.Purchase',
      { skipInitialLoad: true }
    );
  }

  onGridReadyToDefine(grid: GridComponent<IPurchaseRowFromStockDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onCellEditingStarted: this.onCellClicked.bind(this),
    });

    this.translate
      .get([
        'common.code',
        'common.name',
        'common.quantity',
        'common.price',
        'common.sum',
        'common.currency',
        'common.report.selection.purchasenr',
        'common.customer.invoices.articlename',
        'common.productnr',
        'billing.stock.stocksaldo.productnumber',
        'billing.stock.stocksaldo.purchasetriggerquantity',
        'billing.stock.stocksaldo.purchasequantity',
        'billing.stock.stocksaldo.purchasedquantity',
        'billing.stock.stocksaldo.saldo',
        'billing.stock.stocksaldo.ordered',
        'billing.stock.stocksaldo.reserved',
        'billing.stock.stocks.stock',
        'billing.stock.purchase.availablequantity',
        'billing.stock.purchase.purchasequantity',
        'billing.stock.purchase.unitsupplier',
        'billing.purchase.supplier',
        'billing.purchase.deliveryaddress',
        'billing.purchaserows.purchaseunit',
        'billing.purchaserows.wanteddeliverydate',
        'billing.stock.stocksaldo.leadtime',
        'billing.stock.purchase.separatepurchase',
        'billing.productrows.dialogs.discountpercent',
        'billing.purchase.list.purchase',
        'billing.product.stock',
        'billing.products.product',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const productHeader = this.grid.addColumnHeader(
          'productGroup',
          terms['billing.products.product']
        );
        const stockHeader = this.grid.addColumnHeader(
          'stockGroup',
          terms['billing.product.stock']
        );
        const supplierHeader = this.grid.addColumnHeader(
          'supplierGroup',
          terms['billing.purchase.supplier']
        );
        const purchaseHeader = this.grid.addColumnHeader(
          'purchaseGroup',
          terms['billing.purchase.list.purchase']
        );

        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'productNr',
          terms['billing.stock.stocksaldo.productnumber'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: productHeader,
          }
        );
        this.grid.addColumnText(
          'productName',
          terms['common.customer.invoices.articlename'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: productHeader,
          }
        );
        this.grid.addColumnText(
          'unitCode',
          terms['billing.purchaserows.purchaseunit'],
          {
            flex: 1,
            enableHiding: true,
            headerColumnDef: productHeader,
          }
        );
        this.grid.addColumnText(
          'stockName',
          terms['billing.stock.stocks.stock'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: stockHeader,
          }
        );
        this.grid.addColumnNumber(
          'stockPurchaseTriggerQuantity',
          terms['billing.stock.stocksaldo.purchasetriggerquantity'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: stockHeader,
          }
        );
        this.grid.addColumnNumber(
          'stockPurchaseQuantity',
          terms['billing.stock.stocksaldo.purchasequantity'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: stockHeader,
          }
        );
        this.grid.addColumnNumber(
          'totalStockQuantity',
          terms['billing.stock.stocksaldo.saldo'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: stockHeader,
          }
        );
        this.grid.addColumnNumber(
          'availableStockQuantity',
          terms['billing.stock.purchase.availablequantity'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: stockHeader,
          }
        );
        this.grid.addColumnNumber(
          'purchasedQuantity',
          terms['billing.stock.stocksaldo.purchasedquantity'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: stockHeader,
          }
        );
        this.grid.addColumnAutocomplete<SmallGenericType>(
          'supplierId',
          terms['billing.purchase.supplier'],
          {
            flex: 1,
            editable: row => {
              return row.data?.multipleSupplierMatches || false;
            },
            source: _ => this.suppliersDict,
            optionIdField: 'id',
            optionNameField: 'name',
            optionDisplayNameField: 'supplierName',
            cellClassRules: {
              'information-background-color': (params: CellClassParams) =>
                this.validateRules(params) == 'Y',
              'error-background-color': (params: CellClassParams) =>
                this.validateRules(params) == 'R',
              '': (params: CellClassParams) => this.validateRules(params) == '',
            },
            headerColumnDef: supplierHeader,
          }
        );
        this.grid.addColumnText(
          'supplierUnitCode',
          terms['billing.stock.purchase.unitsupplier'],
          {
            flex: 1,
            hide: true,
            editable: true,
            enableHiding: true,
            headerColumnDef: supplierHeader,
          }
        );
        this.grid.addColumnNumber(
          'deliveryLeadTimeDays',
          terms['billing.stock.stocksaldo.leadtime'],
          {
            flex: 1,
            decimals: 0,
            maxDecimals: 0,
            editable: true,
            enableHiding: false,
            headerColumnDef: purchaseHeader,
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.stock.purchase.purchasequantity'],
          {
            flex: 1,
            editable: true,
            enableHiding: false,
            headerColumnDef: purchaseHeader,
          }
        );
        this.grid.addColumnNumber('price', terms['common.price'], {
          flex: 1,
          editable: true,
          enableHiding: false,
          headerColumnDef: purchaseHeader,
        });
        this.grid.addColumnNumber(
          'discountPercentage',
          terms['billing.productrows.dialogs.discountpercent'],
          {
            flex: 1,
            editable: true,
            maxDecimals: 2,
            clearZero: true,
            enableHiding: false,
            headerColumnDef: purchaseHeader,
          }
        );
        this.grid.addColumnNumber('sum', terms['common.sum'], {
          flex: 1,
          decimals: 2,
          enableHiding: false,
          headerColumnDef: purchaseHeader,
        });

        this.grid.addColumnText('currencyCode', terms['common.currency'], {
          flex: 1,
          enableHiding: true,
          headerColumnDef: purchaseHeader,
        });
        this.grid.addColumnDate(
          'requestedDeliveryDate',
          terms['billing.purchaserows.wanteddeliverydate'],
          {
            flex: 1,
            editable: true,
            enableHiding: false,
            headerColumnDef: purchaseHeader,
          }
        );
        this.grid.addColumnText(
          'deliveryAddress',
          terms['billing.purchase.deliveryaddress'],
          {
            width: 90,
            resizable: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.openEditAddress(row),
            },
            enableHiding: false,
            headerColumnDef: purchaseHeader,
          }
        );
        this.grid.addColumnBool(
          'exclusivePurchase',
          terms['billing.stock.purchase.separatepurchase'],
          {
            flex: 1,
            enableHiding: true,
            headerColumnDef: purchaseHeader,
          }
        );
        this.grid.addColumnText(
          'purchaseNr',
          terms['common.report.selection.purchasenr'],
          {
            flex: 1,
            enableHiding: false,
            headerColumnDef: purchaseHeader,
          }
        );
        this.exportFilenameKey.set('billing.stock.stocks.stocks');
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { searchDto: StockPurchaseFilterDTO }
  ): Observable<IPurchaseRowFromStockDTO[]> {
    return this.performGridLoad.load$(
      this.service.getGrid(undefined, additionalProps).pipe(
        tap(value => {
          value.forEach(e => {
            this.recalculateSum(e);
          });

          this.grid.setData(value);
        })
      )
    );
  }

  loadGridData(event: StockPurchaseFilterDTO) {
    this.loadData(undefined, { searchDto: event }).subscribe();
  }

  openEditAddress(row: IPurchaseRowFromStockDTO) {
    this.dialogService
      .open(EditDeliveryAddressComponent, {
        title: this.translate.instant('billing.order.deliveryaddress'),
        addressString: row.deliveryAddress,
        size: 'lg',
        disableClose: true,
      } as IEditDeliveryAddressDialogData)
      .afterClosed()
      .subscribe(value => {
        if (value !== false) {
          row.deliveryAddress = value.toString().trim();
          this.grid.refreshCells();
        }
      });
  }

  triggerCreate(): void {
    if (this.grid.getSelectedCount() == 0) {
      this.messageboxService.warning(
        'core.warning',
        'billing.stock.purchase.noselectedrows'
      );
    } else if (this.grid.getSelectedRows().some(r => !r.supplierId)) {
      this.messageboxService.warning(
        'core.warning',
        'billing.stock.purchase.missingsupplier'
      );
    } else {
      const mb = this.messageboxService.question(
        'core.info',
        this.translate
          .instant('billing.stock.purchase.selectedrows')
          .replace('{0}', this.grid.getSelectedCount().toString())
      );
      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) this.createPurchase();
      });
    }
  }

  createPurchase() {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.purchaseService
        .createPurchaseProposal(this.grid.getSelectedRows())
        .pipe(
          tap(backendResponse => {
            if (backendResponse) {
              this.suppliers = <Record<number, SmallGenericType>>(
                (<unknown>backendResponse)
              );
              this.grid.getSelectedRows().forEach(r => {
                if (this.suppliers[r.tempId]) {
                  r.purchaseId = this.suppliers[r.tempId].id;
                  r.purchaseNr = this.suppliers[r.tempId].name;
                  this.grid.refreshCells();
                }
                backendResponse.success = true;
              });
            }
          })
        )
    );
  }

  validateRules(params: CellClassParams) {
    if (params.data.supplierId && params.data.multipleSupplierMatches)
      return 'Y';
    if (!params.data.supplierId) return 'R';
    else return '';
  }

  private recalculateSum(entity: IPurchaseRowFromStockDTO) {
    const discount = (100 - entity.discountPercentage) / 100;
    entity.sum = entity.quantity * entity.price * discount;
    this.grid.refreshCells();
  }

  private onCellValueChanged(event: CellValueChangedEvent) {
    switch (event.colDef.field) {
      case 'quantity':
        this.getNewPrice(event.data);
        break;
      case 'deliveryLeadTimeDays':
        this.changeLeadTimeDays(event);
        break;
      case 'discountPercentage':
        this.recalculateSum(event.data);
        break;
      case 'price':
        this.recalculateSum(event.data);
        break;
    }
  }

  private onCellClicked(event: CellEditingStartedEvent) {
    switch (event.colDef.field) {
      case 'supplierId':
        this.filterSupplierOptions(event.data);
        break;
    }
  }

  filterSupplierOptions(data: StockPurchaseDTO) {
    return this.performSupplierFilter
      .load$(
        this.purchaseProductsService.getSupplierByProductId(data.productId)
      )
      .pipe(take(1))
      .subscribe(data => {
        this.suppliersDict = data;
      });
  }

  private getNewPrice(entity: IPurchaseRowFromStockDTO) {
    this.recalculateSum(entity);
  }

  private changeLeadTimeDays(entity: CellValueChangedEvent) {
    entity.data.requestedDeliveryDate = new Date().addDays(entity.newValue);
    this.grid.refreshCells();
  }

  isExcludeChange(event: boolean) {
    let value: IPurchaseRowFromStockDTO[] = [];
    if (event) {
      if (this.performGridLoad.data)
        value = this.performGridLoad.data.filter(v => v.quantity != 0);
    } else {
      if (this.performGridLoad.data) value = this.performGridLoad.data;
    }
    this.grid.setData(value);
  }
}
