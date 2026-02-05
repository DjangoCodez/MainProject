import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { DirectDebitGridComponent } from '../direct-debit-grid/direct-debit-grid.component';
import { DirectDebitEditComponent } from '../direct-debit-edit/direct-debit-edit.component';
import { DirectDebitForm } from '../../models/direct-debit-form.model';

@Component({
  selector: 'soe-direct-debit',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class DirectDebitComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: DirectDebitGridComponent,
      editComponent: DirectDebitEditComponent,
      FormClass: DirectDebitForm,
      gridTabLabel: 'economy.export.paymentservice.paymentservices',
      editTabLabel: 'economy.export.paymentservice.paymentservices',
      createTabLabel: 'economy.export.paymentservice.new_paymentservice',
    },
  ];
}
