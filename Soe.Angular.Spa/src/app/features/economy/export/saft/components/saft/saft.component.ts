import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SaftGridComponent } from '../saft-grid/saft-grid.component';

@Component({
  templateUrl: './saft.component.html',
  standalone: false,
})
export class SaftComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SaftGridComponent,
      FormClass: undefined,
      gridTabLabel: 'economy.export.saft',
      exportFilenameKey: 'economy.export.saft',
    },
  ];
}
