import { Component } from '@angular/core';
import { CustomerPaymentMethodsForm } from '../../../models/customer-payment-methods-form.model';
import { CustomerPaymentMethodsGridComponent } from '../../customer-payment-methods-grid/customer-payment-methods-grid/customer-payment-methods-grid.component';
import { CustomerPaymentMethodsEditComponent } from '../../customer-payment-methods-edit/customer-payment-methods-edit/customer-payment-methods-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CustomerPaymentMethodsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CustomerPaymentMethodsGridComponent,
      editComponent: CustomerPaymentMethodsEditComponent,
      FormClass: CustomerPaymentMethodsForm,
      exportFilenameKey: 'economy.customer.paymentmethod.paymentmethods',
      gridTabLabel: 'economy.customer.paymentmethod.paymentmethods',
      editTabLabel: 'economy.customer.paymentmethod.paymentmethod',
      createTabLabel: 'economy.customer.paymentmethod.new_paymentmethod',
    },
  ];
}
