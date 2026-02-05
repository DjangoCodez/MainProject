import { Component } from '@angular/core';
import { IncomingDeliveriesGridComponent } from '../incoming-deliveries-grid/incoming-deliveries-grid.component';
import { IncomingDeliveriesEditComponent } from '../incoming-deliveries-edit/incoming-deliveries-edit.component';
import { IncomingDeliveriesForm } from '../../models/incoming-deliveries-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class IncomingDeliveriesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: IncomingDeliveriesGridComponent,
      editComponent: IncomingDeliveriesEditComponent,
      FormClass: IncomingDeliveriesForm,
      gridTabLabel: 'time.schedule.incomingdelivery.incomingdeliveries',
      editTabLabel: 'time.schedule.incomingdelivery.incomingdelivery',
      createTabLabel: 'time.schedule.incomingdelivery.new',
    },
  ];
}
