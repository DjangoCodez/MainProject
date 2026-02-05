import { Component, inject, input, OnInit } from '@angular/core';
import { StockProductDTO } from '@features/billing/stock-balance/models/stock-balance.model';
import { StockWarehouseForm } from '@features/billing/stock-warehouse/models/stock-warehouse-form.model';
import { StockWarehouseService } from '@features/billing/stock-warehouse/services/stock-warehouse.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IStockShelfDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarGridConfig } from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellKeyDownEvent, CellValueChangedEvent } from 'ag-grid-community';
import { Observable, of, tap } from 'rxjs';

@Component({
  selector: 'soe-stock-warehouse-edit-products-grid',
  standalone: false,
  templateUrl: './stock-warehouse-edit-products-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class StockWarehouseEditProductsGridComponent
  extends GridBaseDirective<StockProductDTO>
  implements OnInit
{
  form = input.required<StockWarehouseForm>();
  stockWarehouseService = inject(StockWarehouseService);
  stockShelf: IStockShelfDTO[] = [];

  ngOnInit(): void {
    this.startFlow(Feature.Billing_Stock, 'Soe.Billing.Stock.Products', {
      lookups: [this.loadStockShelves(this.form()?.getIdControl()?.value)],
    });

    this.form()
      .getIdControl()
      ?.valueChanges.subscribe(id => {
        if (id) {
          this.loadData().subscribe();
          this.loadStockShelves(id).subscribe();
        }
      });
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<StockProductDTO[]> {
    id = this.form()?.getIdControl()?.value;
    if (id) {
      return this.stockWarehouseService.getStockProducts(id).pipe(
        tap(products => {
          this.form().customWarehouseProductsPatchValue(products);
          this.form().stockProducts.updateValueAndValidity();
          this.rowData.next(products);
        })
      );
    }
    return of([]);
  }

  override createGridToolbar(config?: Partial<ToolbarGridConfig>): void {
    super.createGridToolbar();
  }

  onGridReadyToDefine(grid: GridComponent<StockProductDTO>): void {
    super.onGridReadyToDefine(grid);
    this.grid.options.context.newrow = false;
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onCellKeyDown: this.onCellKeyDown.bind(this),
    });
    this.translate
      .get([
        'common.number',
        'common.name',
        'billing.stock.stocksaldo.saldo',
        'billing.stock.products.quantity',
        'billing.product.productgroup',
        'billing.stock.stockplaces.stockplace',
        'billing.product.stocks.stock.avgprice',
        'billing.stock.stocksaldo.purchasequantity',
        'billing.stock.stocksaldo.purchasetriggerquantity',
        'billing.stock.stocksaldo.leadtime',
      ])
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');

        this.grid.addColumnText('productNumber', terms['common.number'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText('productName', terms['common.name'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'productGroupName',
          terms['billing.product.productgroup'],
          {
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnAutocomplete(
          'stockShelfId',
          terms['billing.stock.stockplaces.stockplace'],
          {
            editable: true,
            limit: 7,
            flex: 1,
            source: () => this.stockShelf,
            optionIdField: 'stockShelfId',
            optionDisplayNameField: 'stockShelfName',
            optionNameField: 'name',
          }
        );

        this.grid.addColumnNumber(
          'quantity',
          terms['billing.stock.stocksaldo.saldo'],
          {
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'avgPrice',
          terms['billing.product.stocks.stock.avgprice'],
          {
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'purchaseQuantity',
          terms['billing.stock.stocksaldo.purchasequantity'],
          {
            editable: true,
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'purchaseTriggerQuantity',
          terms['billing.stock.stocksaldo.purchasetriggerquantity'],
          {
            editable: true,
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'deliveryLeadTimeDays',
          terms['billing.stock.stocksaldo.leadtime'],
          {
            editable: true,
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.setNbrOfRowsToShow(5, 10);
        super.finalizeInitGrid();
      });
  }

  private loadStockShelves(id: number = 0): Observable<IStockShelfDTO[]> {
    return this.stockWarehouseService.getStockShelves(false, id).pipe(
      tap(x => {
        this.stockShelf = x;
      })
    );
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    const { colDef, data, newValue, oldValue } = event;
    if (newValue !== oldValue && newValue) {
      this.rowChanged(data);
    }

    this.form()?.setDirtyOnProductStockChange(data);
  }

  rowChanged(row: any) {
    row.isModified = true;
    this.grid.refreshCells();
  }

  onCellKeyDown(event: CellKeyDownEvent) {
    if (!event) return;
    const rowIndex = event.rowIndex || 0;
    const colId = event.colDef.field || 'stockShelfId';
    const keyboardEvent = event.event as unknown as KeyboardEvent;
    if (keyboardEvent.key === 'Enter') {
      console.log('event data', event);
      this.grid.agGrid.api.setFocusedCell(rowIndex + 1, colId);
      this.grid.agGrid.api.startEditingCell({
        rowIndex: rowIndex + 1,
        colKey: colId,
      });
    }
  }
}
