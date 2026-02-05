import { Component } from '@angular/core';
import { InventoriesGridComponent } from '../inventories-grid/inventories-grid.component';
import { InventoriesEditComponent } from '../inventories-edit/inventories-edit.component';
import { InventoriesForm } from '../../models/inventories-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class InventoriesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: InventoriesGridComponent,
      editComponent: InventoriesEditComponent,
      FormClass: InventoriesForm,
      gridTabLabel: 'economy.inventory.inventories.inventory',
      editTabLabel: 'economy.inventory.inventories.inventory',
      createTabLabel: 'economy.inventory.inventories.new_inventory',
    },
  ];
}
