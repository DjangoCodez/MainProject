import { Component, Input, OnDestroy, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Subject, take, takeUntil } from 'rxjs';
import { IStockShelfDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { StockWarehouseForm } from '../../../models/stock-warehouse-form.model';
import {
  CellValueChangedEvent,
  RowDataUpdatedEvent,
  TabToNextCellParams,
} from 'ag-grid-community';
import { StockShelfDTO } from '../../../models/stock-warehouse.model';
import { Perform } from '@shared/util/perform.class';
import { StockWarehouseService } from '@features/billing/stock-warehouse/services/stock-warehouse.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-stock-warehouse-edit-grid',
  templateUrl: './stock-warehouse-edit-grid.component.html',
  styleUrls: ['./stock-warehouse-edit-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockWarehouseEditGridComponent
  extends GridBaseDirective<StockShelfDTO>
  implements OnInit, OnDestroy
{
  @Input({ required: true }) form!: StockWarehouseForm;

  private _destroy$ = new Subject<void>();

  gridUpdated = false;
  stockWarehouseService = inject(StockWarehouseService);
  progressService = inject(ProgressService);
  messageService = inject(MessageboxService);
  shelfRows: StockShelfDTO[] = [];
  rows = new BehaviorSubject<StockShelfDTO[]>([]);
  performValidateShelf = new Perform<BackendResponse>(this.progressService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Stock, 'Soe.Billing.Stock.Stocks.Shelves', {
      skipInitialLoad: true,
      useLegacyToolbar: true,
    });

    this.form?.stockShelves.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(rows => {
        if (!this.form?.stockShelves.dirty) {
          this.shelfRows = <IStockShelfDTO[]>(
            rows.filter((x: IStockShelfDTO) => x.stockShelfId !== null)
          );
          this.setStockShelfGridData();
        }
      });

    this.shelfRows = <IStockShelfDTO[]>this.form?.stockShelves.value;
    this.setStockShelfGridData();
  }

  onGridReadyToDefine(grid: GridComponent<IStockShelfDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.options.context.newRow = false;
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onRowDataUpdated: this.onRowDataUpdated.bind(this),
      tabToNextCell: this.onTabToNextCell.bind(this),
    });

    this.translate
      .get(['common.code', 'common.name', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 1,
          editable: true,
          enableHiding: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          editable: true,
          enableHiding: false,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.triggerDelete(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override createLegacyGridToolbar(): void {
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: ['fal', 'sync'],
          title: 'core.reload_data',
          onClick: () => this.refreshShelves(),
        }),
      ],
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: ['fal', 'plus'],
          label: 'core.add',
          title: 'core.add',
          onClick: () => this.addShelf(),
        }),
      ],
    });
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    this.gridUpdated = true;
    this.form?.setDirtyOnstockShelvesChange(event.data);
  }

  onRowDataUpdated(event: RowDataUpdatedEvent) {
    if (event.context.newRow) {
      const index = event.api.getLastDisplayedRowIndex();
      this.grid.agGrid.api.setFocusedCell(index, 'code');
      this.grid.agGrid.api.startEditingCell({
        rowIndex: index,
        colKey: 'code',
      });
      event.context.newRow = false;
    }
  }

  onTabToNextCell(event: TabToNextCellParams) {
    if (
      !event.backwards &&
      event.nextCellPosition?.column.isPinned() &&
      event.previousCellPosition.rowIndex ===
        event.api.getLastDisplayedRowIndex()
    ) {
      this.addShelf();
      return false;
    }
    return event.nextCellPosition || false;
  }

  protected triggerDelete(row: StockShelfDTO): void {
    if (row.stockShelfId > 0) {
      this.performValidateShelf
        .load$(
          this.stockWarehouseService.validateShelfBeforeDelete(row.stockShelfId)
        )
        .subscribe(result => {
          if (result.success) {
            this.deleteShelf(row);
          } else {
            this.messageService.error(
              'core.error',
              ResponseUtil.getErrorMessage(result) ?? 'core.error'
            );
          }
        });
    } else this.deleteShelf(row);
  }

  private deleteShelf(row: StockShelfDTO): void {
    this.form?.deleteShelf(row);
    this.shelfRows.find(x => x.stockShelfId === row.stockShelfId)!.isDelete =
      true;
    this.setStockShelfGridData();
    this.grid?.deleteRow(row);
    this.grid.agGrid.api.refreshCells();
  }

  public addShelf() {
    this.shelfRows.push(<StockShelfDTO>this.form?.addShelf());
    this.grid.options.context.newRow = true;
    this.setStockShelfGridData();
  }

  rowChanged(row: any) {
    this.grid.refreshCells();
  }

  private setStockShelfGridData() {
    this.shelfRows = this.shelfRows.filter(
      x => !((!x.code || !x.name) && x.isDelete)
    );
    const rows = this.shelfRows
      ? this.shelfRows.filter((r: StockShelfDTO) => !r.isDelete)
      : [];
    this.rows.next(<StockShelfDTO[]>rows);
  }

  refreshShelves() {
    this.shelfRows = this.form?.resetShelves();
    this.setStockShelfGridData();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
