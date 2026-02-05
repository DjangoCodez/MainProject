import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { InventoryWriteoffsGridComponent } from '../inventory-writeoffs-grid/inventory-writeoffs-grid.component';

@Component({
  templateUrl: './inventory-writeoffs.component.html',
  standalone: false,
})
export class InventoryWriteoffsComponent {
  config: MultiTabConfig[] = [
    {
      gridTabLabel: 'economy.inventory.accountdistributionentry.entries',
      gridComponent: InventoryWriteoffsGridComponent,
      FormClass: undefined,
      exportFilenameKey: 'economy.inventory.inventories.writeoff',
    },
  ];
}
