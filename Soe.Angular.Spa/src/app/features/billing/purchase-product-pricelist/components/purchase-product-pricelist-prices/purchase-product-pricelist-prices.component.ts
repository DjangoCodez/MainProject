import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  SimpleChanges,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ISupplierProductGridDTO,
  ISupplierProductPriceSearchDTO,
} from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { StandardRowFunctions } from '@shared/util/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { SoeColumnType } from '@ui/grid/util/column-util';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellFocusedEvent,
  CellValueChangedEvent,
  ColDef,
  RowDataUpdatedEvent,
  TabToNextCellParams,
} from 'ag-grid-community';
import { BehaviorSubject, Subscription, of, take, tap } from 'rxjs';
import { PurchaseProductsEditComponent } from '../../../purchase-products/components/purchase-products-edit/purchase-products-edit.component';
import { PurchaseProductForm } from '../../../purchase-products/models/purchase-product-form.model';
import { SupplierProductGridHeaderDTO } from '../../../purchase-products/models/purchase-product.model';
import { PurchaseProductsService } from '../../../purchase-products/services/purchase-products.service';
import { SupplierProductPriceListForm } from '../../models/purchase-product-pricelist-form.model';
import { SupplierProductPriceComparisonDTO } from '../../models/purchase-product-pricelist.model';
import { PurchaseProductPricelistService } from '../../services/purchase-product-pricelist.service';
import { PurchaseProductPriceListPriceService } from './services/purchase-product-pricelist-price.service';

type ComparisonValueDTO = {
  key: string;
  supplierProductId: number;
  comparePrice: number;
  compareStartDate?: Date;
  compareEndDate?: Date;
  compareQuantity: number;
};

@Component({
  selector: 'soe-purchase-product-pricelist-prices',
  templateUrl: './purchase-product-pricelist-prices.component.html',
  providers: [
    FlowHandlerService,
    ToolbarService,
    PurchaseProductPriceListPriceService,
  ],
  standalone: false,
})
export class PurchaseProductPricelistPricesComponent
  extends GridBaseDirective<SupplierProductPriceComparisonDTO>
  implements OnChanges, OnInit, OnDestroy
{
  @Input({ required: true }) form!: SupplierProductPriceListForm;
  @Input() supplierId!: number;
  @Input() startDate!: Date;
  @Input() currencyId!: number;
  @Input() pricelistId!: number;
  @Input() endDate!: Date;
  @Input() rows = new BehaviorSubject<SupplierProductPriceComparisonDTO[]>([]);
  @Input() rowCount = signal<number>(0);
  @Input() isInActiveSupplier = signal(false);

  purchaseProductPricelistService = inject(PurchaseProductPricelistService);
  readonly flowHandler = inject(FlowHandlerService);
  readonly progressService = inject(ProgressService);
  readonly purchaseProductService = inject(PurchaseProductsService);
  readonly messageBoxService = inject(MessageboxService);
  readonly priceListPriceService = inject(PurchaseProductPriceListPriceService);
  performLoad = new Perform<
    SupplierProductPriceComparisonDTO[] | ISupplierProductGridDTO[]
  >(this.progressService);

  priceRowsSubscription?: Subscription;
  loadActualPrices = new FormControl(false);
  purchaseProducts: ISupplierProductGridDTO[] = [];
  comparisonPrices: ComparisonValueDTO[] = [];
  rowFunctions: MenuButtonItem[] = [];

  hasSupplierId = signal(false);
  isAddRow = signal(false);
  getAllGetAllProductsClicked = signal(false);
  disableGetAllProducts = computed(() => {
    return (
      this.isInActiveSupplier() ||
      !this.hasSupplierId() ||
      this.rowCount() > 0 ||
      this.getAllGetAllProductsClicked()
    );
  });
  disableAddRow = computed((): boolean => {
    return this.isInActiveSupplier() || !this.hasSupplierId();
  });
  hideLoadCurrentPrices = signal(false);
  actualPriceLoaded = signal(false);
  disableRowDelete = signal(true);

  private _effectRef = effect((): void => {
    const inActSup = this.isInActiveSupplier();
    if (inActSup) {
      this.loadActualPrices.disable();

      const columns: Array<ColDef> = [];
      this.grid?.api.getColumnDefs()?.forEach((col: ColDef): void => {
        const sCol = col;
        if (sCol.context.soeColumnType !== SoeColumnType.Icon)
          columns.push(col);
      });
      this.grid?.api.updateGridOptions({ columnDefs: columns });
    } else this.loadActualPrices.enable();
  });

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Purchase_Purchase_Edit,
      'billing.purchase.pricelist.prices',
      {
        skipInitialLoad: true,
        lookups: [this.loadPurchaseProduct()],
      }
    );
    this.setupRowFunctions();

    this.priceRowsSubscription = this.priceListPriceService.rows$.subscribe(
      priceRows => {
        if (!this.grid?.options?.context?.isSameRow) {
          this.rowData.next(priceRows);
          this.rowCount.set(priceRows.length);
        }
        if (this.grid) this.grid.options.context.isSameRow = false;
      }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    const { supplierId, form, rows } = changes;
    if (supplierId) {
      if (supplierId.currentValue > 0) {
        this.loadPurchaseProduct().subscribe();
        this.hasSupplierId.set(true);
        this.getAllGetAllProductsClicked.set(false);
      } else {
        this.purchaseProducts = [];
        this.rowData.next([]);
        this.hasSupplierId.set(false);
      }
    }

    if (form) {
      this.hideLoadCurrentPrices.set(this.form?.isNew);
    }

    if (rows) {
      this.rowCount.set(rows.currentValue.length);
    }

    this.rows.subscribe(() => {
      if (this.form && !this.form.dirty && this.rows.value) {
        this.priceListPriceService.init(this.form, this.rows.value);
      }
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<SupplierProductPriceComparisonDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellFocused: this.onFocusCell.bind(this),
      onRowDataUpdated: this.onRowDataUpdated.bind(this),
      onCellValueChanged: (event: CellValueChangedEvent): void => {
        this.priceListPriceService.updateRow(
          event.data as SupplierProductPriceComparisonDTO,
          true
        );
        this.rows.next(this.rowData.value);
        this.form.markAsDirty();
      },
      tabToNextCell: (event: TabToNextCellParams) => {
        if (
          !event.backwards &&
          event.nextCellPosition?.rowIndex ===
            event.previousCellPosition.rowIndex &&
          event.nextCellPosition?.column.getColId() !== 'soe-grid-menu-column'
        ) {
          this.grid.options.context.isSameRow = true;
          return event.nextCellPosition;
        }
        return false;
      },
    });

    this.translate
      .get([
        'billing.purchase.product.price',
        'billing.purchase.product.pricestartdate',
        'billing.purchase.product.priceqty',
        'billing.purchase.product.supplieritemno',
        'billing.purchase.product.supplieritemname',
        'billing.purchase.product.purchaseprice',
        'billing.product.number',
        'billing.purchaserows.productnr',
        'billing.purchase.product.actualpurchaseprices',
        'billing.purchase.product.newpurchaseprices',
        'billing.purchase.product.product',
        'billing.purchase.product.priceenddate',
        'common.newrow',
        'core.deleterow',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const productHeader = this.grid.addColumnHeader(
          'productGroup',
          terms['billing.purchase.product.product']
        );
        const comparisonHeader = this.grid.addColumnHeader(
          'comparisonGroup',
          terms['billing.purchase.product.actualpurchaseprices']
        );
        const currentHeader = this.grid.addColumnHeader(
          'currentGroup',
          terms['billing.purchase.product.newpurchaseprices']
        );

        this.grid.enableRowSelection();
        this.grid.addColumnModified('isModified');
        this.grid.addColumnAutocomplete<ISupplierProductGridDTO>(
          'supplierProductId',
          terms['billing.purchase.product.supplieritemno'],
          {
            flex: 1,
            enableHiding: false,
            editable: () => {
              return !this.isInActiveSupplier() && !this.actualPriceLoaded();
            },
            source: () => this.purchaseProducts,
            updater: (row, product): void => {
              row.productName = product?.supplierProductName ?? '';
              row.supplierProductId = product?.supplierProductId ?? 0;
              row.productNr = product?.supplierProductNr ?? '';
              row.ourProductName = product?.productName ?? '';
              this.grid.refreshCells();
              this.form?.markAsDirty();
            },
            limit: 10,
            optionIdField: 'supplierProductId',
            optionNameField: 'supplierProductNr',
            optionDisplayNameField: 'productNr',
            headerColumnDef: productHeader,
          }
        );
        this.grid.addColumnIcon('', '', {
          pinned: undefined,
          width: 35,
          iconName: 'pen',
          iconClass: 'pen',
          onClick: (row: SupplierProductPriceComparisonDTO) => {
            this.openEditInNewTab.emit({
              id: row.supplierProductId,
              additionalProps: {
                editComponent: PurchaseProductsEditComponent,
                FormClass: PurchaseProductForm,
                editTabLabel: 'billing.purchase.product.product',
              },
            });
          },
          enableHiding: false,
          headerColumnDef: productHeader,
        });
        this.grid.addColumnText(
          'productName',
          terms['billing.purchase.product.supplieritemname'],
          {
            flex: 1,
            headerColumnDef: productHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'ourProductName',
          terms['billing.product.number'],
          {
            flex: 1,
            headerColumnDef: productHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'compareQuantity',
          terms['billing.purchase.product.priceqty'],
          {
            flex: 1,
            decimals: 2,
            headerColumnDef: comparisonHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'comparePrice',
          terms['billing.purchase.product.purchaseprice'],
          {
            flex: 1,
            decimals: 2,
            headerColumnDef: comparisonHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'compareStartDate',
          terms['billing.purchase.product.pricestartdate'],
          {
            flex: 1,
            headerColumnDef: comparisonHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'compareEndDate',
          terms['billing.purchase.product.priceenddate'],
          {
            flex: 1,
            headerColumnDef: comparisonHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.purchase.product.priceqty'],
          {
            flex: 1,
            decimals: 2,
            editable: () => {
              return !this.isInActiveSupplier() && !this.actualPriceLoaded();
            },
            headerColumnDef: currentHeader,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'price',
          terms['billing.purchase.product.purchaseprice'],
          {
            flex: 1,
            decimals: 2,
            editable: () => {
              return !this.isInActiveSupplier();
            },
            enableHiding: false,
            headerColumnDef: currentHeader,
          }
        );

        this.grid.setNbrOfRowsToShow(13, 15);
        this.exportFilenameKey.set('billing.purchase.pricelist.prices');
        super.finalizeInitGrid();
      });
  }

  onFocusCell(event: CellFocusedEvent) {
    const index = event.api.getLastDisplayedRowIndex();
    if (this.isAddRow())
      event.api.startEditingCell({
        rowIndex: index,
        colKey: 'supplierProductId',
      });
    this.isAddRow.set(false);
  }

  focusCell() {
    this.isAddRow.set(true);
    const index = this.grid.api.getLastDisplayedRowIndex();
    this.grid.api.setFocusedCell(index, 'supplierProductId');
  }

  onRowDataUpdated(event: RowDataUpdatedEvent): void {
    if (event.context.newRow && !event.api.isAnyFilterPresent()) {
      const index = event.api.getLastDisplayedRowIndex();
      event.api.setFocusedCell(index, 'supplierProductId');
      event.api.startEditingCell({
        rowIndex: index,
        colKey: 'supplierProductId',
      });
      event.context.newRow = false;
    } else if (
      event.context.allProductsLoaded &&
      !event.api.isAnyFilterPresent()
    ) {
      const index = event.api.getLastDisplayedRowIndex();
      if (index >= 0) {
        event.api.setFocusedCell(index, 'supplierProductId');
        event.api.startEditingCell({
          rowIndex: 0,
          colKey: 'price',
        });
      }
      event.context.allProductsLoaded = false;
    }
  }

  rowSelectionChanged(rows: SupplierProductPriceComparisonDTO[]): void {
    this.disableRowDelete.set(rows.length === 0);
  }

  getAllProducts() {
    this.getAllGetAllProductsClicked.set(true);
    const model: ISupplierProductPriceSearchDTO = {
      compareDate: this.startDate,
      supplierId: this.supplierId,
      currencyId: this.currencyId,
      includePricelessProducts: true,
    };

    this.performLoad.load(
      this.purchaseProductPricelistService.getSupplierPriceCompare(model).pipe(
        tap((value: SupplierProductPriceComparisonDTO[]): void => {
          this.purchaseProductPricelistService.fixDates(value);
          this.priceListPriceService.addRows(value);
          this.rows.next(this.rowData.value);
          this.form.markAsDirty();
          this.grid.options.context.allProductsLoaded = true;
        })
      )
    );
  }

  getComparisonValues(): void {
    const searchModel: ISupplierProductPriceSearchDTO = {
      compareDate: this.startDate,
      supplierId: this.supplierId,
      currencyId: this.currencyId,
      includePricelessProducts: false,
    };

    this.performLoad.load(
      this.purchaseProductPricelistService
        .getSupplierPriceCompare(searchModel)
        .pipe(
          tap((value: SupplierProductPriceComparisonDTO[]): void => {
            this.createPriceComparisonDict(value);

            const _rows = this.rowData.value;
            _rows.forEach(r => this.setComparisonValues(r));
          })
        )
    );
  }

  private createPriceComparisonDict(
    value: SupplierProductPriceComparisonDTO[]
  ): void {
    this.comparisonPrices = value.map(
      p =>
        <ComparisonValueDTO>{
          key: `${p.supplierProductId},${p.compareQuantity}`,
          supplierProductId: p.supplierProductPriceId,
          comparePrice: p.comparePrice,
          compareStartDate: p.compareStartDate,
          compareEndDate: p.compareEndDate,
          compareQuantity: p.compareQuantity,
        }
    );
  }

  private setComparisonValues(row: SupplierProductPriceComparisonDTO): void {
    const compVals = this.comparisonPrices.find(
      x => x.key === `${row.supplierProductId},${row.quantity}`
    );
    row = this.purchaseProductPricelistService.fixDates([row])[0];

    if (compVals) {
      row.comparePrice = compVals.comparePrice;
      row.compareQuantity = compVals.compareQuantity;
      row.compareStartDate = compVals.compareStartDate;
      row.compareEndDate = compVals.compareEndDate;
    } else {
      row.comparePrice = 0;
      row.compareQuantity = 0;
      row.compareStartDate = undefined;
    }
    this.priceListPriceService.updateRow(row, false);
  }

  addRow(): void {
    this.priceListPriceService.addRow();
    this.grid.options.context.newRow = true;
    this.focusCell();
    this.rows.next(this.rowData.value);
  }

  deleteRows(): void {
    const mb = this.messageBoxService.warning(
      'core.warning',
      'core.deleterowwarning'
    );
    mb.afterClosed().subscribe((res: any) => {
      if (res?.result === true) {
        this.disableRowDelete.set(this.grid.getSelectedCount() > 0);

        setTimeout(() => {
          this.deleteSelectedRows();
        });
      }
    });
  }

  deleteSelectedRows(): void {
    const rowIds = this.grid.getSelectedIds('supplierProductPriceId');
    const rows = this.grid.getSelectedRows();

    if (this.grid.getSelectedCount() != this.grid.totalRowsCount()) {
      this.priceRowsSubscription = this.priceListPriceService.rows$.subscribe(
        priceRowsForm => {
          rows.forEach(row => {
            const matchingRow = priceRowsForm.find(
              element =>
                element.supplierProductPriceId === row.supplierProductPriceId
            );
            if (matchingRow) {
              matchingRow.state = SoeEntityState.Deleted;
              matchingRow.entityState = SoeEntityState.Deleted;
            }
          });
        }
      );
    }
    if (rowIds.length > 0) {
      this.priceListPriceService.deleteRows(rowIds);
    }
  }

  loadPurchaseProduct() {
    if (this.supplierId) {
      const _searchDto: SupplierProductGridHeaderDTO = {
        supplierIds: [this.supplierId],
        supplierProduct: '',
        supplierProductName: '',
        product: '',
        productName: '',
        invoiceProductId: 0,
      };
      return this.performLoad.load$(
        this.purchaseProductService
          .getGrid(undefined, { searchDto: _searchDto })
          .pipe(
            tap((x: ISupplierProductGridDTO[]): void => {
              this.purchaseProducts = x;
              this.grid?.refreshCells();
            })
          )
      );
    }
    return of([]);
  }

  setupRowFunctions(): void {
    this.translate
      .get(['common.newrow', 'core.deleterow'])
      .pipe(take(1))
      .subscribe(terms => {
        this.rowFunctions.push({
          id: StandardRowFunctions.Add,
          label: terms['common.newrow'],
          icon: 'plus',
        });
        this.rowFunctions.push({
          id: StandardRowFunctions.Delete,
          label: terms['core.deleterow'],
          icon: 'times',
          disabled: this.disableRowDelete,
        });
      });
  }

  performRowFunctions(selected: MenuButtonItem): void {
    switch (selected.id) {
      case StandardRowFunctions.Add:
        this.addRow();
        break;
      case StandardRowFunctions.Delete:
        this.deleteRows();
        break;
    }
  }

  loadActualPricesClicked(value: boolean): void {
    if (value) {
      this.loadActualPrices.disable();
      this.getComparisonValues();
      this.actualPriceLoaded.set(true);
    } else {
      this.loadActualPrices.enable();
    }
  }

  ngOnDestroy(): void {
    this._effectRef.destroy();
    this.priceRowsSubscription?.unsubscribe();
  }
}
