import { Component } from '@angular/core';
import { SieEditComponent } from '../sie-edit/sie-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl: './sie.component.html',
  standalone: false,
})
export class SieComponent {
  config: MultiTabConfig[] = [
    {
      editComponent: SieEditComponent,
      editTabLabel: 'economy.import.sie',
    },
  ];
}
