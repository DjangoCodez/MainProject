import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IInventoryWriteOffTemplateGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { InventoryWriteOffTemplatesService } from '../../services/inventory-write-off-templates.service';

@Component({
  selector: 'soe-inventory-write-off-templates-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoryWriteOffTemplatesGridComponent
  extends GridBaseDirective<IInventoryWriteOffTemplateGridDTO>
  implements OnInit
{
  service = inject(InventoryWriteOffTemplatesService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Inventory_WriteOffTemplates,
      'Economy.Inventory.InventoryWriteOffTemplate'
    );
  }

  onGridReadyToDefine(grid: GridComponent<IInventoryWriteOffTemplateGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'economy.inventory.inventorywriteofftemplate.writeoffmethod',
        'economy.inventory.inventorywriteofftemplate.voucherserie',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'inventoryWriteOffName',
          terms['economy.inventory.inventorywriteofftemplate.writeoffmethod'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'voucherSeriesName',
          terms['economy.inventory.inventorywriteofftemplate.voucherserie'],
          {
            flex: 1,
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
