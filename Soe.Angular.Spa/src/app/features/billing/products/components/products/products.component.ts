import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ProductsGridComponent } from '../products-grid/products-grid.component';
import { InvoiceProductForm } from '../../models/invoice-product-form.model';
import { ProductsEditComponent } from '../products-edit/products-edit.component';

@Component({
  selector: 'soe-products',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ProductsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ProductsGridComponent,
      editComponent: ProductsEditComponent,
      FormClass: InvoiceProductForm,
      gridTabLabel: 'billing.products.products',
      editTabLabel: 'billing.products.product',
      createTabLabel: 'billing.products.newproduct',
      exportFilenameKey: 'billing.products.products',
    },
  ];
}
