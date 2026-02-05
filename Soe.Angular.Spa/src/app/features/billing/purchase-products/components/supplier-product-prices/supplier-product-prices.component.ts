import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISupplierProductPriceDTO } from '@shared/models/generated-interfaces/SupplierProductDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import {
  BehaviorSubject,
  Observable,
  Subject,
  take,
  takeUntil,
  tap,
} from 'rxjs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Perform } from '@shared/util/perform.class';
import { BillingService } from '../../../services/services/billing.service';
import { SupplierProductPriceDTO } from '../../models/purchase-product.model';
import { ICompCurrencySmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  CellValueChangedEvent,
  GridApi,
  RowDataUpdatedEvent,
  TabToNextCellParams,
} from 'ag-grid-community';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { PurchaseProductForm } from '../../models/purchase-product-form.model';

@Component({
  selector: 'soe-supplier-product-prices',
  templateUrl: './supplier-product-prices.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupplierProductPricesComponent
  extends GridBaseDirective<SupplierProductPriceDTO>
  implements OnInit, OnDestroy
{
  @Input() rows!: BehaviorSubject<SupplierProductPriceDTO[]>;
  @Input({ required: true }) form!: PurchaseProductForm;
  @Output() rowsStatusChanged = new EventEmitter<ISupplierProductPriceDTO[]>();

  public flowHandler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  billingService = inject(BillingService);

  private _destroy$ = new Subject<void>();
  private _modifiedRows: SupplierProductPriceDTO[] = [];
  private _canUseCurrency = false;
  performLoadCurrency = new Perform<ICompCurrencySmallDTO[]>(
    this.progressService
  );
  currency: SmallGenericType[] = [];
  supplierProductId = 0;

  ngOnInit(): void {
    super.ngOnInit();
    this.flowHandler.execute({
      permission: Feature.Billing_Purchase_Purchase_Edit,
      additionalModifyPermissions: [Feature.Economy_Preferences_Currency],
      onPermissionsLoaded: this.onPermissionsLoaded.bind(this),
      lookups: [this.loadCurrency()],
      setupGrid: this.setupGrid.bind(this),
      setupDefaultToolbar: this.setupToolbar.bind(this),
      parentGuid: this.guid(),
    });
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }

  onPermissionsLoaded(): void {
    this._canUseCurrency = this.flowHandler.hasModifyAccess(
      Feature.Economy_Preferences_Currency
    );
  }

  setupGrid(grid: GridComponent<SupplierProductPriceDTO>) {
    super.setupGrid(grid, 'billing.purchase.rows', false);
    this.grid.api.updateGridOptions({
      onRowDataUpdated: this.onDataUpdated.bind(this),
      tabToNextCell: this.onTabToNextCell.bind(this),
      onCellValueChanged: this.onAfterCellEdit.bind(this),
    });

    this.rows
      .asObservable()
      .pipe(takeUntil(this._destroy$))
      .subscribe(r => {
        if (r.filter(x => x.isModified).length === 0) {
          this._modifiedRows = [];
        }
      });

    this.translate
      .get([
        'billing.product.purchaseprice',
        'billing.purchase.product.pricestartdate',
        'billing.purchase.product.priceqty',
        'billing.purchase.product.priceenddate',
        'common.newrow',
        'core.deleterow',
        'common.currency',
        'billing.purchase.product.linkedtopricelist',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.purchase.product.priceqty'],
          { flex: 1, enableHiding: false, editable: true, decimals: 2 }
        );
        this.grid.addColumnNumber(
          'price',
          terms['billing.product.purchaseprice'],
          { flex: 1, enableHiding: false, editable: true, decimals: 2 }
        );

        if (this._canUseCurrency) {
          this.grid.addColumnSelect(
            'currencyId',
            terms['common.currency'],
            this.currency,
            undefined,
            {
              dropDownIdLabel: 'id',
              dropDownValueLabel: 'name',
              flex: 1,
              editable: row => {
                return !row.data?.supplierProductPriceListId;
              },
              enableHiding: false,
            }
          );
        }

        this.grid.addColumnDate(
          'startDate',
          terms['billing.purchase.product.pricestartdate'],
          {
            flex: 1,
            editable: row => {
              return !row.data?.supplierProductPriceListId;
            },
            enableHiding: false,
          }
        );
        this.grid.addColumnDate(
          'endDate',
          terms['billing.purchase.product.priceenddate'],
          {
            flex: 1,
            editable: row => {
              return !row.data?.supplierProductPriceListId;
            },
            enableHiding: false,
          }
        );
        this.grid.addColumnIcon('priceIcon', '', {
          showIcon: row => !!row.supplierProductPriceListId,
          iconName: 'file-spreadsheet',
          width: 20,
          tooltip: terms['billing.purchase.product.linkedtopricelist'],
          enableHiding: false,
        });
        this.grid.addColumnIconDelete({ onClick: r => this.deleteRow(r) });

        this.grid.setNbrOfRowsToShow(4, 6);

        this.exportFilenameKey.set('billing.purchase.rows');
        super.finalizeInitGrid();
      });
  }

  private setupToolbar(): void {
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: ['fal', 'plus'],
          title: 'common.newrow',
          label: 'common.newrow',
          onClick: () => this.addRow(),
        }),
      ],
    });
  }

  loadCurrency(): Observable<ICompCurrencySmallDTO[]> {
    return this.performLoadCurrency.load$(
      this.billingService.getCompCurrenciesDictSmall().pipe(
        tap(data => {
          this.currency = data.map(
            x => <SmallGenericType>{ id: x.currencyId, name: x.code }
          );
        })
      )
    );
  }

  //#region Grid Events

  onAfterCellEdit(event: CellValueChangedEvent) {
    this.updateRow(event);
    this.setRowAsModified(event.api);
  }

  onDataUpdated(event: RowDataUpdatedEvent) {
    if (event.context.newRow && !event.api.isAnyFilterPresent()) {
      const index = event.api.getLastDisplayedRowIndex();
      event.api.setFocusedCell(index, 'quantity');
      event.api.startEditingCell({
        rowIndex: index,
        colKey: 'quantity',
      });
      event.context.newRow = false;
    }
  }

  onTabToNextCell(event: TabToNextCellParams) {
    console.log(event.nextCellPosition?.column.getColId());
    if (
      !event.backwards &&
      event.nextCellPosition?.column.getColId() === 'priceIcon' &&
      event.previousCellPosition.rowIndex ===
        event.api.getLastDisplayedRowIndex()
    ) {
      this.addRow();
      return false; //Don't perform tab
    }
    return event.nextCellPosition || false;
  }

  //#endregion

  //#region Helper methods
  addRow() {
    const row = new SupplierProductPriceDTO();
    row.quantity = 0;
    row.state = SoeEntityState.Active;
    row.isModified = true;

    if (this.rows.value.length > 0) {
      const lastRow = this.rows.value.slice(-1)[0];
      row.currencyId = lastRow.currencyId;
      row.currencyCode = lastRow.currencyCode;
      this.supplierProductId = lastRow.supplierProductId;
    } else if (this.currency.length > 0) {
      row.currencyId = this.currency[0].id;
      row.currencyCode = this.currency[0].name;
    }
    row.supplierProductId = this.supplierProductId;

    const currentRows = this.rows.getValue();
    const updatedRows = [...currentRows, row];
    this.rows.next(updatedRows);
    this.grid.options.context.newRow = true;
    this.setRowAsModified(this.grid.agGrid.api);
  }

  deleteRow(row: SupplierProductPriceDTO) {
    //Delete row
    const rows = this.rows.value;
    if (rows) {
      const index: number = rows.indexOf(row);
      rows.splice(index, 1);
      this.grid.resetRows();
    }
    row.state = SoeEntityState.Deleted;
    row.isModified = true;

    if (row.supplierProductPriceId != 0) {
      //Update modified rows
      const idx = this._modifiedRows.indexOf(row);
      if (idx < 0) {
        this._modifiedRows.push(row);
      } else {
        this._modifiedRows[idx] = row;
      }
    }

    this.setRowAsModified(this.grid.agGrid.api);
  }

  updateRow(row: CellValueChangedEvent) {
    const isDateColumn = row.colDef.cellDataType === 'date';
    const isDateEquals = (oldDate: Date, newDate: Date): boolean => {
      return oldDate.toDateString() === newDate.toDateString();
    };

    if (
      row.data &&
      ((isDateColumn &&
        !isDateEquals(new Date(row.oldValue), new Date(row.newValue))) ||
        (!isDateColumn && row.newValue !== row.oldValue))
    ) {
      row.data.isModified = true;
      const idx = this._modifiedRows.indexOf(row.data);
      if (idx < 0) {
        this._modifiedRows.push(row.data);
      } else {
        this._modifiedRows[idx] = row.data;
      }
    }
  }

  private setRowAsModified(gridApi: GridApi) {
    gridApi.refreshCells();
    this.form.markAsDirty();
    this.rowsStatusChanged.emit(<ISupplierProductPriceDTO[]>this._modifiedRows);
  }

  //#endregion
}
