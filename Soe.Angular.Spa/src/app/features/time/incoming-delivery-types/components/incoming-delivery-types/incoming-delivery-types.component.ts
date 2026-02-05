import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { IncomingDeliveryTypeForm } from '../../models/incoming-delivery-types-form.model';
import { IncomingDeliveryTypesEditComponent } from '../incoming-delivery-types-edit/incoming-delivery-types-edit.component';
import { IncomingDeliveryTypesGridComponent } from '../incoming-delivery-types-grid/incoming-delivery-types-grid.component';

@Component({
  selector: 'soe-incoming-delivery-types',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class IncomingDeliveryTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: IncomingDeliveryTypesGridComponent,
      editComponent: IncomingDeliveryTypesEditComponent,
      FormClass: IncomingDeliveryTypeForm,
      gridTabLabel: 'time.schedule.incomingdeliverytype.incomingdeliverytype',
      editTabLabel: 'time.schedule.incomingdeliverytype.incomingdeliverytype',
      createTabLabel:
        'time.schedule.incomingdeliverytype.new_incomingdeliverytype',
    },
  ];
}
