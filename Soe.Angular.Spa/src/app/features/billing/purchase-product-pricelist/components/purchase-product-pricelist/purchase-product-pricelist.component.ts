import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupplierProductPriceListForm } from '../../models/purchase-product-pricelist-form.model';
import { PurchaseProductPricelistGridComponent } from '../purchase-product-pricelist-grid/purchase-product-pricelist-grid.component';
import { PurchaseProductPricelistEditComponent } from '../purchase-product-pricelist-edit/purchase-product-pricelist-edit.component';

@Component({
  selector: 'soe-purchase-product-pricelist',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PurchaseProductPricelistComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PurchaseProductPricelistGridComponent,
      editComponent: PurchaseProductPricelistEditComponent,
      FormClass: SupplierProductPriceListForm,
      gridTabLabel: 'billing.purchase.pricelists.pricelists',
      editTabLabel: 'billing.purchase.pricelists.pricelist',
      createTabLabel: 'billing.purchase.pricelists.new',
    },
  ];
}
