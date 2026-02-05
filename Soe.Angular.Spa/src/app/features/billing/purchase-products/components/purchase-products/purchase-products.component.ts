import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { PurchaseProductForm } from '../../models/purchase-product-form.model';
import { PurchaseProductsGridComponent } from '../purchase-products-grid/purchase-products-grid.component';
import { PurchaseProductsEditComponent } from '../purchase-products-edit/purchase-products-edit.component';

@Component({
  selector: 'soe-purchase-products',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class PurchaseProductsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: PurchaseProductsGridComponent,
      editComponent: PurchaseProductsEditComponent,
      FormClass: PurchaseProductForm,
      gridTabLabel: 'billing.purchase.product.products',
      editTabLabel: 'billing.purchase.product.product',
      createTabLabel: 'billing.purchase.product.new_product',
    },
  ];
}
