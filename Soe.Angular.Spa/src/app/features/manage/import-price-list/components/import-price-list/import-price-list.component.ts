import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ImportPriceListGridComponent } from '../import-price-list-grid/import-price-list-grid.component';
import { ImportPriceListForm } from '../../models/import-price-list-form.model';

@Component({
  standalone: false,
  templateUrl: './import-price-list.component.html',
})
export class ImportPriceListComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ImportPriceListGridComponent,
      FormClass: ImportPriceListForm,
      gridTabLabel: 'manage.system.import.pricelist',
    },
  ];
}
