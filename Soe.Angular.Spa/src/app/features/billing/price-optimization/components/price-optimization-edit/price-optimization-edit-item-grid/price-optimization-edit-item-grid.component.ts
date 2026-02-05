import {
  Component,
  EventEmitter,
  inject,
  input,
  Input,
  OnInit,
  Output,
  signal,
} from '@angular/core';
import { PriceOptimizationForm } from '@features/billing/price-optimization/models/price-optimization-form.model';
import { PurchaseCartRowDTO } from '@features/billing/price-optimization/models/price-optimization.model';
import { PriceOptimizationService } from '@features/billing/price-optimization/services/price-optimization.service';
import { SearchInvoiceProductDialogData } from '@shared/components/search-invoice-product-dialog/models/search-invoice-product-dialog.models';
import { SearchInvoiceProductDialogComponent } from '@shared/components/search-invoice-product-dialog/search-invoice-product-dialog.component';
import { ProductImageCellRendererComponent } from '@shared/components/search-invoice-product-dialog/search-invoice-product-grid/product-image/product-image-cell-renderer';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
  SoeEntityState,
  SoeSysPriceListProviderType,
  TermGroup_PurchaseCartPriceStrategy,
  TermGroup_PurchaseCartStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { TwoValueCellRenderer } from '@ui/grid/cell-renderers/two-value-cell-renderer/two-value-cell-renderer.component';
import { GridComponent } from '@ui/grid/grid.component';
import { ISoeAggregationResult } from '@ui/grid/interfaces';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClickedEvent, CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';

@Component({
  selector: 'soe-price-optimization-edit-item-grid',
  templateUrl: './price-optimization-edit-item-grid.component.html',
  styleUrl: './price-optimization-edit-item-grid.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PriceOptimizationEditItemGridComponent
  extends GridBaseDirective<PurchaseCartRowDTO>
  implements OnInit
{
  @Input() purchaseCartId: number = 0;
  @Input() isNotOpen = signal(false);
  @Input() wholesalerList: BehaviorSubject<SmallGenericType[]> =
    new BehaviorSubject<SmallGenericType[]>([]);
  @Input() purchaseRows: BehaviorSubject<PurchaseCartRowDTO[]> =
    new BehaviorSubject<PurchaseCartRowDTO[]>([]);
  @Output() getPrices: EventEmitter<boolean> = new EventEmitter<boolean>();

  form = input.required<PriceOptimizationForm>();

  cartService = inject(PriceOptimizationService);
  dialogService = inject(DialogService);
  coreService = inject(CoreService);
  toaster = inject(ToasterService);
  messageboxService = inject(MessageboxService);

  protected wholesalerSelection: SmallGenericType[] = [];
  protected currencyId?: number;
  wholesalersColumn: any;
  selectedColumn: string = '';
  hasEmptyCheapestPrice = signal(false);
  showAllRowsValue = signal(false);
  selectedRowId: number | null = null;
  infoText = '';
  showColumnNames: string[] = [];
  hideColumnNames: string[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    //wait till the wholesaler list is loaded
    this.wholesalerList.asObservable().subscribe(list => {
      if (list?.length) {
        this.startFlow(
          Feature.Billing_Price_Optimization,
          'billing.purchase.priceoptimization.productpricerows',
          {
            skipInitialLoad: true,
          }
        );
      }
    });

    this.form().selectedWholesellerIds.valueChanges.subscribe(() => {
      this.onWholesalerSelectionChanged();
    });

    this.setInit();
  }

  setInit() {
    this.purchaseRows.asObservable().subscribe(x => {
      this.rowData.next(x);

      setTimeout(() => {
        this.onWholesalerSelectionChanged();
      }, 300);

      this.onUpdatePriceStrategy(
        this.form().getRawValue().priceStrategy,
        false
      );
      this.showHideByStatus();
    });
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [CompanySettingType.CoreBaseCurrency];
    return this.performLoadData.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap((settings: UserCompanySettingCollection) => {
          this.currencyId = SettingsUtil.getIntCompanySetting(
            settings,
            CompanySettingType.CoreBaseCurrency
          );
        })
      )
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      clearFiltersOption: { hidden: signal(true) },
      reloadOption: { hidden: signal(true) },
    });
  }

  cellValueChanged(event: CellValueChangedEvent<PurchaseCartRowDTO>) {
    if (event.colDef.field === 'quantity' && !this.isNotOpen()) {
      const row = this.grid.api.getRowNode(event.rowIndex?.toString() || '');
      if (row) {
        row.data.isModified = true;
      }
      this.updateTotalRows();
      this.form().markAsDirty();
    }
  }

  cellClick(event: CellClickedEvent) {
    if (event.colDef.field?.includes('wholesalerPrice') && !this.isNotOpen()) {
      const row = this.grid.api.getRowNode(event.rowIndex?.toString() || '');
      this.clearPreviousSelection(event.rowIndex, row);

      //set the new selection
      this.selectedColumn = event.colDef.field || '';
      this.selectedRowId = event.rowIndex;
      if (row) {
        row.data.sysWholesellerId = this.wholesalerList.value.find(
          w => w.name == event.colDef.headerName?.trim()
        )?.id;
        row.data.purchasePrice = row.data[this.selectedColumn];
        row.data.selectedPrice = row.data.purchasePrice;
        row.data.isModified = true;

        this.updateTotalRows();
        this.form().markAsDirty();
      }

      //change the price strategy
      if (this.selectedColumn.includes('wholesalerPrice')) {
        this.form().priceStrategy.patchValue(
          TermGroup_PurchaseCartPriceStrategy.CustomerPriceList
        );
      }
    }
    this.grid?.refreshCells();
  }

  clearPreviousSelection(rowIndex: number | null, row: any) {
    if (rowIndex && this.selectedRowId == rowIndex) {
      this.selectedColumn = '';
      this.selectedRowId = null;
    }
    if (row) {
      row.data.sysWholesellerId = null;
    }
  }

  override onGridReadyToDefine(grid: GridComponent<PurchaseCartRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellClicked: this.cellClick.bind(this),
      onCellValueChanged: this.cellValueChanged.bind(this),
    });

    this.translate
      .get([
        'billing.purchase.priceoptimization.productinfo',
        'billing.purchase.priceoptimization.quantity',
        'billing.purchase.priceoptimization.purchasepriceheader',
        'billing.purchase.priceoptimization.productnamenumber',
        'billing.purchase.priceoptimization.selectedprice',
        'billing.purchase.priceoptimization.selectedtotal',
        'billing.purchase.priceoptimization.total',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;
        this.grid.addColumnModified('isModified');

        this.grid.addColumnIcon('imageUrl', '', {
          columnSeparator: true,
          suppressFilter: true,
          width: 60,
          minWidth: 60,
          maxWidth: 60,
          enableHiding: false,
          editable: false,
          cellRenderer: ProductImageCellRendererComponent,
        });
        this.grid.addColumnText(
          'productNr',
          terms['billing.purchase.priceoptimization.productnamenumber'],
          {
            flex: 2,
            enableHiding: false,
            editable: false,
            cellRenderer: TwoValueCellRenderer,
            cellRendererParams: {
              primaryValueKey: `productNr`,
              secondaryValueKey: `productName`,
            },
          }
        );
        this.grid.addColumnText(
          'productInfo',
          terms['billing.purchase.priceoptimization.productinfo'],
          {
            flex: 2,
            enableHiding: false,
            editable: false,
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.purchase.priceoptimization.quantity'],
          {
            flex: 1,
            enableHiding: false,
            editable: !this.isNotOpen(),
          }
        );

        // selected prices
        this.grid.addColumnNumber(
          'selectedPrice',
          terms['billing.purchase.priceoptimization.selectedprice'],
          {
            flex: 1,
            enableHiding: false,
            editable: false,
            decimals: 2,
            clearZero: true,
            cellClassRules: {
              'selected-price-cell': 'true',
            },
          }
        );

        const colHeader = this.grid.addColumnHeader(
          '',
          terms['billing.purchase.priceoptimization.purchasepriceheader']
        );

        this.wholesalersColumn = this.wholesalerList.value.forEach((w, i) => {
          const index = i + 1;
          const idField = `wholesalerPrice${index}`;

          // wholesaler prices
          this.grid.addColumnNumber(idField, w.name, {
            flex: 1,
            enableHiding: false,
            editable: false,
            decimals: 2,
            clearZero: true,
            headerColumnDef: colHeader,
            cellClassRules: {
              'selected-cell': (params: any) => {
                const isSelected =
                  this.selectedColumn === params.colDef.field &&
                  this.selectedRowId === parseInt(params.data.AG_NODE_ID);
                const isWholesaler = w.id === params.data.sysWholesellerId;

                return isSelected || isWholesaler;
              },
            },
          });
        });

        if (
          this.form().getRawValue().status === TermGroup_PurchaseCartStatus.Open
        ) {
          this.grid.addColumnIconDelete({
            tooltip: terms['core.delete'],
            onClick: row => {
              this.deleteRow(row);
            },
          });
        }
        this.grid.setTotalGridRowClassCallback(params => {
          if (params?.rowIndex == 1) return 'aggregations-error-row';
          return '';
        });

        this.grid?.addAggregationsRow({});
        this.updateTotalRows();

        super.finalizeInitGrid();
      });
  }

  private showHideByStatus() {
    if (this.isNotOpen()) {
      this.form().selectedWholesellerIds.disable();
      this.form().priceStrategy.disable();
    } else {
      this.form().selectedWholesellerIds.enable();
      this.form().priceStrategy.enable();
    }
  }

  getTotals(): ISoeAggregationResult<PurchaseCartRowDTO> {
    const rows = this.rowData.value.filter(
      r => r.state !== SoeEntityState.Deleted
    );
    const wholesalerTotals: Record<string, number> = {};

    //quantity
    const totalQuantity = rows.reduce(
      (sum, row) => sum + (Number(row.quantity) || 0),
      0
    );

    // wholesaler
    if (this.wholesalerList?.value?.length) {
      this.wholesalerList.subscribe(wholesalers => {
        wholesalers.forEach((_, i) => {
          const key = `wholesalerPrice${i + 1}`;

          wholesalerTotals[key] = rows.reduce((sum, row) => {
            const price = Number((row as any)[key] || 0);
            const qty = Number(row.quantity || 0);

            return sum + (price * qty || 0);
          }, 0);
        });
      });
    }

    return {
      quantity: totalQuantity,
      productNr: this.terms['billing.purchase.priceoptimization.total'],
      ...wholesalerTotals,
    } as ISoeAggregationResult<PurchaseCartRowDTO>;
  }

  getSelectedTotals(): ISoeAggregationResult<PurchaseCartRowDTO> {
    const rows = this.rowData.value.filter(
      r => r.state !== SoeEntityState.Deleted
    );
    const wholesalerTotals: Record<string, number> = {};

    //quantity
    const totalQuantity = rows.reduce(
      (sum, row) => sum + (Number(row.quantity) || 0),
      0
    );

    //selected
    const selectedTotal = rows.reduce(
      (sum, row) => sum + (Number(row.selectedPrice) || 0),
      0
    );

    // wholesaler
    if (this.wholesalerList?.value?.length) {
      this.wholesalerList.value.forEach((wholesaler, i) => {
        const key = `wholesalerPrice${i + 1}`;

        wholesalerTotals[key] = rows.reduce((sum, row) => {
          const price = Number((row as any)[key] || 0);
          const qty = Number(row.quantity || 0);

          const isHighlighted =
            row.sysWholesellerId === wholesaler.id ||
            (this.selectedColumn === key &&
              this.selectedRowId === rows.indexOf(row));

          return sum + (isHighlighted && price > 0 ? price * qty : 0);
        }, 0);
      });
    }

    return {
      quantity: totalQuantity,
      selectedPrice: selectedTotal,
      productNr: this.terms['billing.purchase.priceoptimization.selectedtotal'],
      ...wholesalerTotals,
    } as ISoeAggregationResult<PurchaseCartRowDTO>;
  }

  deleteRow(row: PurchaseCartRowDTO) {
    const rows = this.rowData.value;
    if (rows) {
      const index: number = rows.indexOf(row);
      rows.splice(index, 1);
      this.grid.resetRows();
    }

    row.state = SoeEntityState.Deleted;
    row.isModified = true;

    this.form().patchValue({ purchaseCartRows: rows });
    this.form().markAsDirty();
    this.updateTotalRows();

    this.showAllRows(this.showAllRowsValue());
  }

  addProducts() {
    const dialogOpts = <Partial<SearchInvoiceProductDialogData>>{
      size: 'xl',
      disableClose: true,
      hideProducts: false,
      priceListTypeId: 0,
      customerId: 0,
      currencyId: this.currencyId,
      sysWholesellerId: undefined,
      hidePrices: true,
      number: '',
      name: '',
      quantity: 1,
    };

    const dialogRef = this.dialogService.open(
      SearchInvoiceProductDialogComponent,
      dialogOpts
    );

    dialogRef.componentInstance.productAdded.subscribe(result => {
      const productList = {} as PurchaseCartRowDTO;

      if (result) {
        productList.sysProductId = result.productId ?? 0;
        productList.quantity = result.quantity ?? 1;
        productList.imageUrl = result.imageUrl ?? '';
        productList.type = result.type ?? SoeSysPriceListProviderType.Unknown;
        productList.externalId = result.externalId ?? 0;
        productList.productNr = result.productNr ?? '';
        productList.productName = result.productName ?? '';
        productList.productInfo = result.productInfo ?? '';
        productList.purchaseCartRowId = 0;
        productList.purchaseCartId = this.purchaseCartId;
        productList.isModified = true;

        this.purchaseRows.value.push(productList);
        this.rowData.next(this.purchaseRows.value);
        this.form().markAsDirty();

        this.showAllRows(this.showAllRowsValue());

        this.toaster.success(
          this.translate.instant(
            'billing.purchase.priceoptimization.itemadded'
          ),
          productList.productName,
          { newestOnTop: true }
        );
      }
    });

    dialogRef.afterClosed().subscribe(c => {
      this.getPrices.emit(this.form().dirty);
    });
  }

  protected onPriceStrategyChange(selectedOption: number | undefined) {
    this.onUpdatePriceStrategy(selectedOption);
    this.openPopup(selectedOption);
  }

  private onUpdatePriceStrategy(
    selectedOption: number | undefined,
    isFormDirty: boolean = true
  ) {
    if (selectedOption) {
      this.changePriceStrategyInfo(selectedOption);
      this.selectBestPrice(selectedOption, isFormDirty);
      this.updateTotalRows();
    }
  }

  updateTotalRows() {
    if (this.grid) {
      this.grid.setAggregationsErrorRow([
        this.getSelectedTotals(),
        this.getTotals(),
      ]);
      this.grid.api.refreshClientSideRowModel('aggregate');
      this.grid.api.refreshCells();
    }
  }

  showAllRowsValueChanged(status: boolean) {
    this.showAllRowsValue.set(status);
    this.showAllRows(status);
  }

  showAllRows(status: boolean) {
    if (status) {
      this.grid.setNbrOfRowsToShow(this.grid.getAllRows().length + 1);
    } else this.grid.setNbrOfRowsToShow(8, 8);

    this.grid.updateGridHeightBasedOnNbrOfRows();
  }

  protected changePriceStrategyInfo(selectedOption: number): void {
    if (
      selectedOption === TermGroup_PurchaseCartPriceStrategy.CustomerPriceList
    ) {
      this.infoText = this.translate.instant(
        'billing.purchase.priceoptimization.manualpricestrategyinfo'
      );
    } else if (
      selectedOption === TermGroup_PurchaseCartPriceStrategy.WholesalerPriceList
    ) {
      this.infoText = this.translate.instant(
        'billing.purchase.priceoptimization.wholesalerpricestrategyinfo'
      );
    } else if (
      selectedOption === TermGroup_PurchaseCartPriceStrategy.CheapestPriceList
    ) {
      this.infoText = this.translate.instant(
        'billing.purchase.priceoptimization.lowestpricestrategyinfo'
      );
    }
  }

  private selectBestPrice(
    selectedOption: number,
    isFormDirty: boolean = true
  ): void {
    if (isFormDirty) this.clearSelection();

    const rows = this.getRowsWithVisibleColumns();

    const getPrice = (row: any, index: number): number => {
      const colKey = `wholesalerPrice${index + 1}`;
      const value = row[colKey] ?? row.wholesalerPrices?.[colKey] ?? 0;
      return Number(value) || 0;
    };

    this.wholesalerList.subscribe(wholesalers => {
      if (
        selectedOption === TermGroup_PurchaseCartPriceStrategy.CustomerPriceList
      ) {
        rows.forEach(row => {
          row.selectedPrice = row.purchasePrice;
        });
      } else if (
        selectedOption ===
        TermGroup_PurchaseCartPriceStrategy.WholesalerPriceList
      ) {
        // get total
        const totals: number[] = wholesalers.map((_, i) =>
          rows.reduce((sum, row) => {
            const price = getPrice(row, i);
            return sum + (price > 0 ? price * (row.quantity || 1) : 0);
          }, 0)
        );

        //get the lowest total
        const bestTotal = Math.min(...totals.filter(t => t > 0));
        const bestWholesalerIndex = totals.findIndex(t => t === bestTotal);
        if (bestWholesalerIndex === -1) return;
        const bestWholesaler = wholesalers[bestWholesalerIndex];

        //select that column
        rows.forEach(row => {
          if (row.state === SoeEntityState.Deleted) return;

          const price = getPrice(row, bestWholesalerIndex);
          row.sysWholesellerId = bestWholesaler.id;
          row.purchasePrice = price > 0 ? price : 0;
          row.selectedPrice = row.purchasePrice;
          row.isModified = isFormDirty; // isFormDirty && this.form().isCopy || isFormDirty;

          if (row.selectedPrice === 0) {
            this.hasEmptyCheapestPrice.set(true);
          }
        });
        this.selectedColumn = `wholesalerPrice${bestWholesalerIndex + 1}`;
      } else if (
        selectedOption === TermGroup_PurchaseCartPriceStrategy.CheapestPriceList
      ) {
        rows.forEach(row => {
          const priceList = wholesalers
            .map((_, i) => getPrice(row, i))
            .map((p, i) => ({ price: p, index: i }))
            .filter(p => p.price > 0);

          if (!priceList.length) return;

          const minPrice = Math.min(...priceList.map(p => p.price));
          const best = priceList.find(p => p.price === minPrice);
          if (!best) return;

          const bestWholesaler = wholesalers[best.index];
          row.sysWholesellerId = bestWholesaler.id;
          row.purchasePrice = minPrice;
          row.selectedPrice = row.purchasePrice;
          row.isModified = isFormDirty; // isFormDirty && this.form().isCopy || isFormDirty;
        });
      }
    });
  }

  private openPopup(selectedOption: number | undefined): void {
    if (
      this.hasEmptyCheapestPrice() &&
      selectedOption === TermGroup_PurchaseCartPriceStrategy.WholesalerPriceList
    ) {
      this.messageboxService.warning(
        this.translate.instant(
          'billing.purchase.priceoptimization.missingpricetitle'
        ),
        this.translate.instant(
          'billing.purchase.priceoptimization.missingpricetext'
        )
      );
    }
  }

  protected loadPrices(): void {
    this.getPrices.emit();
  }

  private onWholesalerSelectionChanged(): void {
    this.showColumnNames = [];
    this.hideColumnNames = [];

    this.wholesalerList.value.forEach((wh, i) => {
      const colKey = `wholesalerPrice${i + 1}`;
      const shouldShow = this.form().selectedWholesellerIds.value.includes(
        wh.id
      );

      if (shouldShow) {
        this.showColumnNames.push(colKey);
      } else {
        this.hideColumnNames.push(colKey);
      }
    });

    this.grid?.showColumns(this.showColumnNames);
    this.grid?.hideColumns(this.hideColumnNames);
  }

  private getRowsWithVisibleColumns(): PurchaseCartRowDTO[] {
    const rows = this.rowData.value;

    rows.forEach(row => {
      this.hideColumnNames.forEach(col => {
        if (Object.prototype.hasOwnProperty.call(row, col)) {
          delete (row as any)[col];
        }
      });
    });

    return rows;
  }

  private clearSelection(): void {
    this.rowData.value.forEach(row => {
      row.sysWholesellerId = 0;
      row.purchasePrice = 0;
    });

    this.hasEmptyCheapestPrice.set(false);
    this.clearSelectedPriceColumn();
  }

  private clearSelectedPriceColumn() {
    this.selectedColumn = '';
    this.selectedRowId = null;
  }
}
