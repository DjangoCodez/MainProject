import { Component } from '@angular/core';
import { MarkupGridComponent } from '../markup-grid/markup-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-markup',
  templateUrl: 'markup.component.html',
  standalone: false,
})
export class MarkupComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: MarkupGridComponent,
      gridTabLabel: 'billing.invoices.markup.markup',
    },
  ];
}
