import { Component } from '@angular/core';
import { DeliveryTypesForm } from '../../models/delivery-types-form.model';
import { DeliveryTypesGridComponent } from '../delivery-types-grid/delivery-types-grid.component';
import { DeliveryTypesEditComponent } from '../delivery-types-edit/delivery-types-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-delivery-types',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class DeliveryTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DeliveryTypesGridComponent,
      editComponent: DeliveryTypesEditComponent,
      FormClass: DeliveryTypesForm,
      gridTabLabel: 'billing.invoices.deliverytype.deliverytype',
      editTabLabel: 'billing.invoices.deliverytype.deliverytype',
      createTabLabel: 'billing.invoices.deliverytype.new_deliverytype',
    },
  ];
}
