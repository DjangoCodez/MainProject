import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CustomerGridComponent } from '../customer-grid/customer-grid.component';
import { CustomerEditComponent } from '../customer-edit/customer-edit.component';
import { CustomerForm } from '../../models/customer-form.model';

@Component({
  selector: 'soe-customer',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CustomerComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CustomerGridComponent,
      editComponent: CustomerEditComponent,
      FormClass: CustomerForm,
      gridTabLabel: 'common.customer.customer.customers',
      editTabLabel: 'common.customer.customer.customer',
      createTabLabel: 'common.customer.customer.new',
      exportFilenameKey: 'common.customer.customer.customers',
    },
  ];
}
