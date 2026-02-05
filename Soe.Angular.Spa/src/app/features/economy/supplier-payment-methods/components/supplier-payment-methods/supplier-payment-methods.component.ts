import { Component } from '@angular/core';
import { SupplierPaymentMethodsForm } from '../../models/supplier-payment-methods-form.model';
import { SupplierPaymentMethodsGridComponent } from '../supplier-payment-methods-grid/supplier-payment-methods-grid.component';
import { SupplierPaymentMethodsEditComponent } from '../supplier-payment-methods-edit/supplier-payment-methods-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class SupplierPaymentMethodsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SupplierPaymentMethodsGridComponent,
      editComponent: SupplierPaymentMethodsEditComponent,
      FormClass: SupplierPaymentMethodsForm,
      exportFilenameKey: 'economy.supplier.paymentmethod.paymentmethods',
      gridTabLabel: 'economy.supplier.paymentmethod.paymentmethods',
      editTabLabel: 'economy.supplier.paymentmethod.paymentmethod',
      createTabLabel: 'economy.supplier.paymentmethod.new_paymentmethod',
    },
  ];
}
