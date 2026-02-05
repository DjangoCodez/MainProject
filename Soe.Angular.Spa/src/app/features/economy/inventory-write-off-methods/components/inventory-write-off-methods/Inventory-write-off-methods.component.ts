import { Component } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { InventoryWriteOffMethodForm } from '../../models/inventory-write-off-method-form.model';
import { InventoryWriteOffMethodsEditComponent } from '../inventory-write-off-methods-edit/inventory-write-off-methods-edit.component';
import { InventoryWriteOffMethodsGridComponent } from '../inventory-write-off-methods-grid/inventory-write-off-methods-grid.component';

@Component({
    selector: 'soe-inventory-write-off-methods',
    templateUrl: '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
    providers: [ValidationHandler],
    standalone: false
})
export class InventoryWriteOffMethodsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: InventoryWriteOffMethodsGridComponent,
      editComponent: InventoryWriteOffMethodsEditComponent,
      FormClass: InventoryWriteOffMethodForm,
      gridTabLabel:
        'economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod',
      editTabLabel:
        'economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod',
      createTabLabel:
        'economy.inventory.inventorywriteoffmethods.new_inventorywriteoffmethod',
      exportFilenameKey:
        'economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod',
    },
  ];
}
