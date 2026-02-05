import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ExcelImportGridComponent } from '../excel-import-grid/excel-import-grid.component';

@Component({
  selector: 'soe-excel-import',
  templateUrl: './excel-import.component.html',
  standalone: false,
})
export class ExcelImportComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ExcelImportGridComponent,
      gridTabLabel: 'common.excelimport',
      editTabLabel: 'common.excelimport',
      createTabLabel: 'common.excelimport',
    },
  ];
}
