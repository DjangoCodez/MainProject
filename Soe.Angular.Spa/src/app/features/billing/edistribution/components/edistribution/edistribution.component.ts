import { Component } from '@angular/core';
import { EdistributionGridComponent } from '../edistribution-grid/edistribution-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
    selector: 'soe-edistribution',
    templateUrl: './edistribution.component.html',
    standalone: false
})
export class EdistributionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: EdistributionGridComponent,
      gridTabLabel: 'billing.distribution.edistribution.distribution',
    },
  ];
}
