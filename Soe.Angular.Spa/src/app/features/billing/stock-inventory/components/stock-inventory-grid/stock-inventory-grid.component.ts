import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IStockInventoryGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, take } from 'rxjs';
import { StockInventoryService } from '../../services/stock-inventory.service';

@Component({
  selector: 'soe-stock-inventory-grid',
  templateUrl: './stock-inventory-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockInventoryGridComponent
  extends GridBaseDirective<IStockInventoryGridDTO, StockInventoryService>
  implements OnInit
{
  completedLoaded = false;
  includeCompleted = false;
  loadedInventories: IStockInventoryGridDTO[] = [];
  service = inject(StockInventoryService);
  progressService = inject(ProgressService);
  performLoad = new Perform<any>(this.progressService);

  ngOnInit(): void {
    this.startFlow(
      Feature.Billing_Stock_Inventory,
      'Billing.Stock.StockInventory'
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      reloadOption: {
        onAction: () => this.reloadGrid(),
      },
    });
  }

  onGridReadyToDefine(grid: GridComponent<IStockInventoryGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'billing.stock.stock',
        'billing.stock.stocks.stock',
        'billing.stock.stockinventory.inventorystart',
        'billing.stock.stockinventory.inventorystop',
        'common.createdby',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('headerText', terms['common.name'], {});
        this.grid.addColumnText(
          'stockName',
          terms['billing.stock.stocks.stock'],
          {
            flex: 1,
            showSetFilter: true,
          }
        );
        this.grid.addColumnDate(
          'inventoryStart',
          terms['billing.stock.stockinventory.inventorystart'],
          {
            flex: 1,
            sortable: true,
            sort: 'desc',
          }
        );
        this.grid.addColumnDate(
          'inventoryStop',
          terms['billing.stock.stockinventory.inventorystop'],
          { flex: 1 }
        );
        this.grid.addColumnText('createdBy', terms['common.createdby'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        super.finalizeInitGrid();
      });
  }

  reloadGrid() {
    this.completedLoaded = this.includeCompleted;
    this.refreshGrid();
  }

  showCompleted(event: boolean) {
    this.includeCompleted = event;
    if (this.completedLoaded)
      this.grid.setData(
        this.loadedInventories.filter(i =>
          this.includeCompleted ? true : !i.inventoryStop
        )
      );
    else this.refreshGrid();
  }

  override loadData(
    id?: number | undefined
  ): Observable<IStockInventoryGridDTO[]> {
    return this.performLoad.load$(
      this.service
        .getGrid(undefined, { includeCompleted: this.includeCompleted })
        .pipe(
          map(data => {
            this.loadedInventories = data;
            if (!this.completedLoaded && this.includeCompleted)
              this.completedLoaded = true;
            return data;
          })
        )
    );
  }
}
