import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';

import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IInventoryWriteOffMethodGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { InventoryWriteOffMethodsService } from '../../services/inventory-write-off-methods.service';

@Component({
  selector: 'soe-inventory-write-off-methods-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoryWriteOffMethodsGridComponent extends GridBaseDirective<
  IInventoryWriteOffMethodGridDTO,
  InventoryWriteOffMethodsService
> {
  service = inject(InventoryWriteOffMethodsService);

  constructor(
    private translationService: TranslateService,
    public flowHandler: FlowHandlerService
  ) {
    super();
    this.startFlow(
      Feature.Economy_Inventory_WriteOffMethods,
      'economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IInventoryWriteOffMethodGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translationService
      .get([
        'common.name',
        'common.description',
        'economy.inventory.inventorywriteoffmethod.periodvalue',
        'economy.inventory.inventorywriteoffmethod.type',
        'economy.inventory.inventorywriteoffmethod.periodtype',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 20 });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 20,
        });
        this.grid.addColumnNumber(
          'periodValue',
          terms['economy.inventory.inventorywriteoffmethod.periodvalue'],
          { flex: 10, clearZero: true, alignLeft: true }
        );
        this.grid.addColumnText(
          'periodTypeName',
          terms['economy.inventory.inventorywriteoffmethod.periodtype'],
          { flex: 20 }
        );
        this.grid.addColumnText(
          'typeName',
          terms['economy.inventory.inventorywriteoffmethod.type'],
          { flex: 20 }
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
