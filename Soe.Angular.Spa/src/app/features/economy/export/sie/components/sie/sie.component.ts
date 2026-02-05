import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SieEditComponent } from '../sie-edit/sie-edit.component';
@Component({
  selector: 'soe-sie',
  templateUrl: './sie.component.html',
  standalone: false,
})
export class SieComponent {
  config: MultiTabConfig[] = [
    {
      editComponent: SieEditComponent,
      editTabLabel: 'economy.export.sie',
    },
  ];
}
