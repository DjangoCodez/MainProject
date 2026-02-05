import { Component } from '@angular/core';
import { PurchaseDeliveryForm } from '../../models/purchase-delivery-form.model';
import { PurchaseDeliveryGridComponent } from '../purchase-delivery-grid/purchase-delivery-grid.component';
import { PurchaseDeliveryEditComponent } from '../purchase-delivery-edit/purchase-delivery-edit.component';
import { PurchaseDeliveryAwaitingDeliveryGridComponent } from '../purchase-delivery-awaiting-delivery-grid/purchase-delivery-awaiting-delivery-grid.component';
import { PurchaseEditComponent } from '../../../purchase/components/purchase-edit/purchase-edit.component';
import { PurchaseForm } from '../../../purchase/models/purchase-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PurchaseDeliveryComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PurchaseDeliveryAwaitingDeliveryGridComponent,
      editComponent: PurchaseEditComponent,
      FormClass: PurchaseForm,
      hideForCreateTabMenu: true,
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
      gridTabLabel: 'billing.purchase.delivery.awaitingdelivery',
      editTabLabel: 'billing.purchase.list.purchase',
      createTabLabel: '',
    },
    {
      gridComponent: PurchaseDeliveryGridComponent,
      editComponent: PurchaseDeliveryEditComponent,
      FormClass: PurchaseDeliveryForm,
      gridTabLabel: 'billing.purchase.delivery.deliveries',
      editTabLabel: 'billing.purchase.delivery.delivery',
      createTabLabel: 'billing.purchase.delivery.new_delivery',
    },
  ];
}
