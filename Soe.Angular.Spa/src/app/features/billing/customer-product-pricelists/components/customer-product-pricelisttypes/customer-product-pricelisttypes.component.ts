import { Component } from '@angular/core';
import { CustomerProductPriceListTypeForm } from '../../models/customer-product-pricelisttype-form.model';
import { CustomerProductPriceListTypesGridComponent } from '../customer-product-pricelisttypes-grid/customer-product-pricelisttypes-grid.component';
import { CustomerProductPriceListTypesEditComponent } from '../customer-product-pricelisttypes-edit/customer-product-pricelisttypes-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CustomerProductPriceListTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CustomerProductPriceListTypesGridComponent,
      editComponent: CustomerProductPriceListTypesEditComponent,
      FormClass: CustomerProductPriceListTypeForm,
      gridTabLabel: 'billing.product.pricelist.pricelists',
      editTabLabel: 'billing.product.pricelist.name',
      createTabLabel: 'billing.product.pricelist.new',
    },
  ];
}
