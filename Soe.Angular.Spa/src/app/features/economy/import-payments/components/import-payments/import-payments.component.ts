import { Component, inject } from '@angular/core';
import { ImportPaymentType } from '@shared/models/generated-interfaces/Enumerations';
import { ImportPaymentsForm } from '../../models/import-payments-form.model';
import { ImportPaymentsGridComponent } from '../import-payments-grid/import-payments-grid.component';
import { ImportPaymentsEditComponent } from '../import-payments-edit/import-payments-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ImportPaymentsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ImportPaymentsGridComponent,
      editComponent: ImportPaymentsEditComponent,
      FormClass: ImportPaymentsForm,
      gridTabLabel: 'economy.import.payments.payments',
      editTabLabel: 'economy.import.payments.payment',
      createTabLabel: 'economy.import.payment.new_payment',
      exportFilenameKey: 'economy.import.payment.payments',
      addOptions: [
        {
          id: ImportPaymentType.CustomerPayment,
          label: 'economy.import.payment.customer',
        },
        {
          id: ImportPaymentType.SupplierPayment,
          label: 'economy.import.payment.supplier',
        },
      ],
    },
  ];
}
