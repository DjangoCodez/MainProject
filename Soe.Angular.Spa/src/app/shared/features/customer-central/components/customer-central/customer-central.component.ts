import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CustomerCentralEditComponent } from '../customer-central-edit/customer-central-edit.component';
import { CustomerCentralForm } from '../../models/customer-central-form.model';

@Component({
  selector: 'soe-customer-central',
  templateUrl: './customer-central.component.html',
  standalone: false,
})
export class CustomerCentralComponent {
  config: MultiTabConfig[] = [
    {
      editComponent: CustomerCentralEditComponent,
      editTabLabel: 'economy.customer.customercentral',
      FormClass: CustomerCentralForm,
    },
  ];
}
