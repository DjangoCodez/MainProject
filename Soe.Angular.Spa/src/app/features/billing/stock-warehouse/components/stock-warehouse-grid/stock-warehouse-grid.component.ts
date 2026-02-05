import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IStockGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { StockWarehouseService } from '../../services/stock-warehouse.service';

@Component({
  selector: 'soe-warehouse-code-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockWarehouseGridComponent
  extends GridBaseDirective<IStockGridDTO>
  implements OnInit
{
  service = inject(StockWarehouseService);

  ngOnInit(): void {
    this.startFlow(Feature.Billing_Stock, 'Soe.Billing.Stock.Stocks');
  }

  override onGridReadyToDefine(grid: GridComponent<IStockGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.code',
        'billing.stock.stocks.isexternal',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 3,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 3,
        });
        this.grid.addColumnBool(
          'isExternal',
          terms['billing.stock.stocks.isexternal'],
          {
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
