import { Component } from '@angular/core';
import { InventoryWriteOffTemplateForm } from '../../models/inventory-write-off-templates-form.model';
import { InventoryWriteOffTemplatesGridComponent } from '../inventory-write-off-templates-grid/inventory-write-off-templates-grid.component';
import { InventoryWriteOffTemplatesEditComponent } from '../inventory-write-off-templates-edit/inventory-write-off-templates-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class InventoryWriteOffTemplatesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: InventoryWriteOffTemplatesGridComponent,
      editComponent: InventoryWriteOffTemplatesEditComponent,
      FormClass: InventoryWriteOffTemplateForm,
      gridTabLabel:
        'economy.inventory.inventorywriteofftemplates.inventorywriteofftemplate',
      editTabLabel:
        'economy.inventory.inventorywriteofftemplates.inventorywriteofftemplate',
      createTabLabel:
        'economy.inventory.inventorywriteofftemplates.new_inventorywriteofftemplate',
    },
  ];
}
