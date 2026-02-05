import { Component } from '@angular/core';
import { IntrastatExportGridComponent } from '../intrastat-export-grid/intrastat-export-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl: './intrastat-export.component.html',
  standalone: false,
})
export class IntrastatExportComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: IntrastatExportGridComponent,
      FormClass: undefined,
      gridTabLabel: 'common.intrastat.reportingandexport',
      editTabLabel: 'common.intrastat.reportingandexport',
    },
  ];
}
