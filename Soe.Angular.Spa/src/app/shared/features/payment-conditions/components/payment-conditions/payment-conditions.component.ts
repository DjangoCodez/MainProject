import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { PaymentConditionForm } from '../../models/payment-condition-form.model';
import { PaymentConditionsGridComponent } from '../payment-conditions-grid/payment-conditions-grid.component';
import { PaymentConditionsEditComponent } from '../payment-conditions-edit/payment-conditions-edit.component';

@Component({
    selector: 'soe-payment-conditions',
    templateUrl: '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
    standalone: false
})
export class PaymentConditionsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PaymentConditionsGridComponent,
      editComponent: PaymentConditionsEditComponent,
      FormClass: PaymentConditionForm,
      gridTabLabel: 'common.paymentcondition.paymentconditions',
      editTabLabel: 'common.paymentcondition.paymentcondition',
      createTabLabel: 'common.paymentcondition.new_paymentcondition',
      exportFilenameKey: 'common.paymentcondition.paymentconditions',
    },
  ];
}
