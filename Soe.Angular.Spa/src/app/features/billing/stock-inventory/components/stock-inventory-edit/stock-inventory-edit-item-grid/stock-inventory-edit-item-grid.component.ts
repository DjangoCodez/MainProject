import {
  Component,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IStockInventoryRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellEditingStoppedEvent,
  CellFocusedEvent,
  CellValueChangedEvent,
  Column,
} from 'ag-grid-community';
import { BehaviorSubject, Subject, Subscription, take, takeUntil } from 'rxjs';
import { StockInventoryHeadForm } from '../../../models/stock-inventory-head-form.model';
import { StockInventoryEditItemGridService } from './services/stock-inventory-edit-item-grid.service';

@Component({
  selector: 'soe-stock-inventory-edit-item-grid',
  templateUrl: './stock-inventory-edit-item-grid.component.html',
  providers: [
    FlowHandlerService,
    ToolbarService,
    StockInventoryEditItemGridService,
  ],
  standalone: false,
})
export class StockInventoryEditItemGridComponent
  extends GridBaseDirective<IStockInventoryRowDTO>
  implements OnInit, OnDestroy
{
  @Input() rows!: BehaviorSubject<IStockInventoryRowDTO[]>;
  @Input({ required: true }) form!: StockInventoryHeadForm;
  @Output() formChange: EventEmitter<StockInventoryHeadForm> =
    new EventEmitter<StockInventoryHeadForm>();
  @Output() enableItemUpdate: EventEmitter<boolean> =
    new EventEmitter<boolean>();

  @ViewChild(GridComponent)
  rowSubscription?: Subscription;
  private _destroy$ = new Subject<void>();
  private itemServiceInit = false;
  isRowSelected = signal(false);
  private _selectedRows: IStockInventoryRowDTO[] = [];
  public flowHandler = inject(FlowHandlerService);
  messageboxService = inject(MessageboxService);
  private editItemGridService = inject(StockInventoryEditItemGridService);
  menuList: MenuButtonItem[] = [];
  hasSelectedTransactionRows = false;
  hasTransactionDate = false;

  get isRowEditable() {
    return (
      this.form?.value.stockInventoryHeadId > 0 &&
      !this.form?.value.inventoryStop
    );
  }

  ngOnInit(): void {
    this.exportFilenameKey.set('billing.stock.stockinventory.stockinventory');
    this.startFlow(
      Feature.Billing_Stock_Inventory,
      'Billing.Stock.StockInventory.Rows',
      { skipInitialLoad: true }
    );

    this.form?.stockInventoryRows.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(() => {
        if (!this.itemServiceInit) {
          this.editItemGridService.init(this.form);
          this.itemServiceInit = true;
          this.rowData.next(this.editItemGridService.getRows());
        }
      });
    this.setDisableFunctionControl(false);
  }

  ngOnDestroy() {
    this.rowSubscription?.unsubscribe();
  }

  onGridReadyToDefine(grid: GridComponent<IStockInventoryRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onCellEditingStopped: this.onCellEditingStopped.bind(this),
      onCellFocused: this.onCellFocused.bind(this),
    });

    this.translate
      .get([
        'billing.stock.stockinventory.productnr',
        'common.name',
        'billing.stock.stockinventory.shelfname',
        'billing.stock.stockinventory.startingsaldo',
        'billing.stock.stockinventory.inventorysaldo',
        'billing.stock.stockinventory.difference',
        'common.modified',
        'billing.stock.stocksaldo.ordered',
        'billing.stock.stocksaldo.reserved',
        'billing.stock.stockinventory.stockinventory',
        'billing.stock.stockinventory.transactiondate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText(
          'productNumber',
          terms['billing.stock.stockinventory.productnr'],
          { flex: 1, enableHiding: false }
        );
        this.grid.addColumnText('productName', terms['common.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'shelfName',
          terms['billing.stock.stockinventory.shelfname'],
          { flex: 1, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'startingSaldo',
          terms['billing.stock.stockinventory.startingsaldo'],
          { decimals: 2, flex: 1, enableHiding: false }
        );
        this.grid.addColumnNumber(
          'orderedQuantity',
          terms['billing.stock.stocksaldo.ordered'],
          { decimals: 2, hide: true, flex: 1 }
        );
        this.grid.addColumnNumber(
          'reservedQuantity',
          terms['billing.stock.stocksaldo.reserved'],
          { decimals: 2, hide: true, flex: 1 }
        );
        this.grid.addColumnNumber(
          'inventorySaldo',
          terms['billing.stock.stockinventory.inventorysaldo'],
          {
            decimals: 2,
            editable: () => this.isRowEditable,
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'difference',
          terms['billing.stock.stockinventory.difference'],
          { decimals: 2, flex: 1, enableHiding: false }
        );
        this.grid.addColumnDate(
          'transactionDate',
          terms['billing.stock.stockinventory.transactiondate'],
          {
            flex: 1,
            enableHiding: false,
            editable: () => this.isRowEditable,
          }
        );
        this.grid.addColumnDate('modified', terms['common.modified'], {
          flex: 1,
          enableHiding: false,
        });

        this.grid.setNbrOfRowsToShow(12, 12);
        super.finalizeInitGrid();
      });

    this.rowSubscription = this.rows.subscribe(_rows => {
      this.rowData.next(_rows);
    });
  }

  selectionChanged(rows: IStockInventoryRowDTO[]) {
    this.editItemGridService.setData(this.rows.value);
    this._selectedRows = rows ?? [];
    this.isRowSelected.set(rows.length > 0);
    this.setDisableFunctionControl(rows.length > 0);
  }

  setDisableFunctionControl(hasSelectedTransactionRows: boolean) {
    this.hasSelectedTransactionRows = hasSelectedTransactionRows;
    this.hasTransactionDate = false;
    if (this.form?.value.inventoryStop) {
      this.hasTransactionDate = true;
    }

    this.enableItemUpdate.emit(
      this.isRowEditable &&
        !this.hasTransactionDate &&
        this.hasSelectedTransactionRows
    );
  }

  setTransactionDate() {
    const mb = this.messageboxService.show(
      this.translate.instant(
        'billing.stock.stocksaldo.updatetransactiondate.confirmationheader'
      ),
      this.translate.instant(
        'billing.stock.stocksaldo.updatetransactiondate.confirmationtext'
      ),
      {
        customIconName: 'calendar-days',
        showInputDate: true,
        inputDateLabel: 'billing.stock.stocksaldo.actioncreated',
        inputDateValue: new Date(),
        buttons: 'okCancel',
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) this.updateTransactionDate(response.dateValue);
    });
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    if (event.newValue !== event.oldValue) {
      if (
        event.column.getColId() === 'inventorySaldo' &&
        !isNaN(event.data.inventorySaldo)
      ) {
        const result = this.editItemGridService.updateInventoryRowDiff(
          event.data,
          event.data.inventorySaldo
        );
        if (result) {
          event.data.inventorySaldo = result.inventorySaldo;
          event.data.difference = result.difference;
        }
      }

      if (event.column.getColId() === 'transactionDate') {
        this.editItemGridService.updateTransactionDatebyValue(
          event.data.stockInventoryRowId,
          event.newValue as Date
        );
      }
      event.api.refreshCells();
    }
  }

  onCellEditingStopped(event: CellEditingStoppedEvent) {
    if (event.colDef.field === 'transactionDate' && event.rowIndex !== null) {
      const currentRow = event.api.getDisplayedRowAtIndex(event.rowIndex)?.data;
      const preRow =
        event.api.getDisplayedRowAtIndex(event.rowIndex - 1)?.data ?? undefined;
      const res = this.editItemGridService.updateTransactionDate(
        currentRow.stockInventoryRowId,
        preRow?.stockInventoryRowId ?? undefined
      );

      if (res && currentRow) {
        event.api.getDisplayedRowAtIndex(event.rowIndex)!.data.transactionDate =
          res.transactionDate;
        event.api.refreshCells();
      }
    }
  }

  onCellFocused(event: CellFocusedEvent): void {
    const col = event.column as Column;
    if (
      col.getColId() === 'transactionDate' &&
      event.rowIndex !== null &&
      event.rowIndex >= 0
    ) {
      const currentRow = event.api.getDisplayedRowAtIndex(event.rowIndex)?.data;
      const preRow =
        event.api.getDisplayedRowAtIndex(event.rowIndex - 1)?.data ?? undefined;

      const res = this.editItemGridService.updateTransactionDate(
        currentRow.stockInventoryRowId,
        preRow?.stockInventoryRowId ?? undefined
      );

      if (res && currentRow) {
        event.api.getDisplayedRowAtIndex(event.rowIndex)!.data.transactionDate =
          res.transactionDate;
        event.api.refreshCells();
      }
    }
  }

  resetFocus() {
    if (this.grid.options.context.focusedCell) {
      const event = this.grid.options.context.focusedCell as CellFocusedEvent;
      const row = event.rowIndex;
      const colkey = (event.column as Column).getColDef().field;
      if (row && colkey)
        this.grid.agGrid.api.startEditingCell({
          rowIndex: row,
          colKey: colkey,
        });
    }
  }

  updateQuantity(): void {
    const selectedIds =
      this._selectedRows.map(x => x.stockInventoryRowId) ?? [];

    this.editItemGridService.updateInventoryRowsQuantity(selectedIds);
    this.rowData.next(this.editItemGridService.getRows());
  }

  updateTransactionDate(dateValue?: Date) {
    const selectedIds =
      this._selectedRows.map(x => x.stockInventoryRowId) ?? [];
    if (dateValue && selectedIds.length > 0) {
      this.editItemGridService.updateInventoryRowsTransactionDate(
        selectedIds,
        dateValue
      );
      this.rowData.next(this.editItemGridService.getRows());
    }
  }
}
