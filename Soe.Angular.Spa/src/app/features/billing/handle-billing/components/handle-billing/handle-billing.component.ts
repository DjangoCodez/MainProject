import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { HandleBillingGridComponent } from '../handle-billing-grid/handle-billing-grid.component';

@Component({
  selector: 'soe-handle-billing',
  templateUrl: './handle-billing.component.html',
  standalone: false,
})
export class HandleBillingComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: HandleBillingGridComponent,
      gridTabLabel: 'billing.order.handlebilling.periodinvoicing',
    },
  ];
}
