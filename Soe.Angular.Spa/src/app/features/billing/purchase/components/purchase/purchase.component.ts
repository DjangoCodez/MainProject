import { Component } from '@angular/core';
import { PurchaseForm } from '../../models/purchase-form.model';
import { PurchaseGridComponent } from '../purchase-grid/purchase-grid.component';
import { PurchaseEditComponent } from '../purchase-edit/purchase-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PurchaseComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PurchaseGridComponent,
      editComponent: PurchaseEditComponent,
      FormClass: PurchaseForm,
      recordConfig: {
        hideIfEmpty: true,
        hidePosition: false,
        showRecordName: false,
        hideDropdown: false,
        dropdownTextProperty: 'name',
        isDate: false,
        refetchDataOnRecordChange: true,
        hideRecordNavigator: true,
      },
      exportFilenameKey: 'billing.purchase.list.purchase',
      gridTabLabel: 'billing.purchase.list.purchases',
      editTabLabel: 'billing.purchase.list.purchase',
      createTabLabel: 'billing.purchase.list.new_purchase',
    },
  ];
}
