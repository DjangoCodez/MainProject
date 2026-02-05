import {
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import {
  StockDTO,
  StockShelfDTO,
} from '@features/billing/products/models/stock.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, map, take, tap } from 'rxjs';
import { StockWarehouseService } from '../../../../stock-warehouse/services/stock-warehouse.service';
import {
  IStockDTO,
  IStockShelfDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { InvoiceProductForm } from '@features/billing/products/models/invoice-product-form.model';
import { CellValueChangedEvent } from 'ag-grid-community';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { addEmptyOption } from '@shared/util/array-util';

type StockGridDTO = AG_NODE<StockDTO>;

@Component({
  selector: 'soe-product-stocks',
  templateUrl: './product-stocks.component.html',
  styleUrls: ['./product-stocks.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductStocksComponent
  extends GridBaseDirective<StockDTO>
  implements OnInit, OnDestroy
{
  stocksForProduct = model.required<StockDTO[]>();
  defaultStockId = input.required<number>();
  form = input.required<InvoiceProductForm>();

  private readonly stockService = inject(StockWarehouseService);
  private purchasePermission: boolean = false;
  private readOnly = signal(false);
  private stocksForCompany: IStockDTO[] = [];
  private allStockShelves: StockShelfDTO[] = [];

  private setRowDataEff = effect((): void => {
    const rows = this.stocksForProduct();
    setTimeout(() => {
      this.rowData.next(rows);
    });
  });

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Stock_Place,
      'Billing.Products.Products.Views.Stocks',
      {
        additionalModifyPermissions: [Feature.Billing_Purchase],
        skipInitialLoad: true,
        lookups: [this.loadStocksForCompany(), this.loadShelvesForCompany()],
      }
    );
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.purchasePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Purchase
    );
    this.readOnly.set(!this.flowHandler.modifyPermission());
  }

  private loadStocksForCompany(): Observable<IStockDTO[]> {
    return this.stockService
      .getStocks(true)
      .pipe(tap(x => (this.stocksForCompany = x)));
  }

  private loadShelvesForCompany(): Observable<StockShelfDTO[]> {
    return this.stockService.getStockShelves(false, 0).pipe(
      map((shelves: IStockShelfDTO[]): StockShelfDTO[] => {
        shelves
          .map(x => x as StockShelfDTO)
          .forEach(s => {
            s.shelfName = s.name;
          });
        return shelves as StockShelfDTO[];
      }),
      tap(x => {
        addEmptyOption(x);
        this.allStockShelves = x;
      })
    );
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('plus', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          hidden: this.readOnly,
          onAction: this.addRow.bind(this),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<StockDTO>): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.cellValueChanged.bind(this),
    });

    this.translate
      .get([
        'billing.product.stocks.stock.name',
        'billing.product.stocks.stock.stockshelfname',
        'common.quantity',
        'billing.product.stocks.stock.avgprice',
        'billing.stock.stocksaldo.leadtime',
        'billing.stock.stocksaldo.purchasequantity',
        'billing.stock.stocksaldo.purchasetriggerquantity',
      ])
      .pipe(take(1))
      .subscribe((terms: TermCollection): void => {
        const editableFunc = (data?: StockDTO) =>
          !data?.stockProductId || false;

        this.grid.addColumnSelect(
          'stockId',
          terms['billing.product.stocks.stock.name'],
          this.stocksForCompany,
          undefined,
          {
            dropDownIdLabel: 'stockId',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: row => editableFunc(row.data),
          }
        );
        this.grid.addColumnSelect(
          'stockShelfId',
          terms['billing.product.stocks.stock.stockshelfname'],
          [],
          undefined,
          {
            dropDownIdLabel: 'stockShelfId',
            dropDownValueLabel: 'shelfName',
            dynamicSelectOptions: row => this.getDynamicStockShelves(row),
            flex: 1,
            editable: true,
          }
        );
        this.grid.addColumnNumber('saldo', terms['common.quantity'], {
          flex: 1,
          editable: false,
          decimals: 2,
          clearZero: false,
        });
        this.grid.addColumnNumber(
          'avgPrice',
          terms['billing.product.stocks.stock.avgprice'],
          {
            flex: 1,
            editable: row => editableFunc(row.data),
            decimals: 2,
            clearZero: false,
          }
        );

        if (this.purchasePermission) {
          this.grid.addColumnNumber(
            'purchaseQuantity',
            terms['billing.stock.stocksaldo.purchasequantity'],
            {
              flex: 1,
              editable: true,
              decimals: 2,
              clearZero: false,
            }
          );
          this.grid.addColumnNumber(
            'purchaseTriggerQuantity',
            terms['billing.stock.stocksaldo.purchasetriggerquantity'],
            {
              flex: 1,
              editable: true,
              decimals: 2,
              clearZero: false,
            }
          );
          this.grid.addColumnNumber(
            'deliveryLeadTimeDays',
            terms['billing.stock.stocksaldo.leadtime'],
            {
              flex: 1,
              editable: true,
              decimals: 0,
              clearZero: false,
            }
          );
        }

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => this.deleteRow(row as StockGridDTO),
          showIcon: row => editableFunc(row),
        });

        this.grid.context.suppressGridMenu = true;
        this.grid.setNbrOfRowsToShow(5, 5);
        this.exportFilenameKey.set('billing.product.stocks.stock');

        super.finalizeInitGrid({ hidden: true });
        this.grid.updateGridHeightBasedOnNbrOfRows();
      });
  }

  private cellValueChanged({
    newValue,
    oldValue,
  }: CellValueChangedEvent): void {
    if (newValue === oldValue) return;

    this.form().markAsDirty();
  }

  private addRow(): void {
    const stockPlace = this.stocksForCompany.find(
      s => s.stockId === this.defaultStockId()
    );
    const row = new StockDTO();
    row.stockId = this.defaultStockId();
    row.name = stockPlace?.name ?? '';
    row.code = stockPlace?.code ?? '';
    this.stocksForProduct.update((rows: StockDTO[]): StockDTO[] => {
      return [...rows, row];
    });
    this.focusFirstCell();
  }

  private deleteRow(row: StockGridDTO): void {
    if (row) {
      this.stocksForProduct.update((rows: StockDTO[]) => {
        return rows.filter(
          r => (r as StockGridDTO).AG_NODE_ID !== row.AG_NODE_ID
        );
      });
      this.form().markAsDirty();
    }
  }

  private focusFirstCell(): void {
    setTimeout((): void => {
      const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
      this.grid?.api.setFocusedCell(lastRowIdx, 'stockId');
      this.grid?.api.startEditingCell({
        rowIndex: lastRowIdx,
        colKey: 'stockId',
      });
    }, 100);
  }

  private getDynamicStockShelves(row: any): StockShelfDTO[] {
    return this.allStockShelves.filter(
      s => s.stockId == row.data.stockId || s.stockId === 0
    );
  }

  ngOnDestroy(): void {
    this.setRowDataEff?.destroy();
  }
}
