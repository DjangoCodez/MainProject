import { Component } from '@angular/core';
import { DeliveryConditionEditComponent } from '../delivery-condition-edit/delivery-condition-edit.component';
import { DeliveryConditionGridComponent } from '../delivery-condition-grid/delivery-condition-grid.component';
import { DeliveryConditionForm } from '../../models/delivery-condition-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class DeliveryConditionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DeliveryConditionGridComponent,
      editComponent: DeliveryConditionEditComponent,
      FormClass: DeliveryConditionForm,
      gridTabLabel: 'billing.invoices.deliverycondition.deliveryconditions',
      editTabLabel: 'billing.invoices.deliverycondition.deliverycondition',
      createTabLabel:
        'billing.invoices.deliverycondition.new_deliverycondition',
    },
  ];
}
