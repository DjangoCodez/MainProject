import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { InboundEmailGridComponent } from '../inbound-email-grid/inbound-email-grid.component';

@Component({
  selector: 'soe-inbound-email',
  standalone: false,
  templateUrl: './inbound-email.component.html',
})
export class InboundEmailComponent {
  config: MultiTabConfig[] = [
    {
      gridTabLabel: 'Inbound Emails',
      gridComponent: InboundEmailGridComponent,
    },
  ];
}
