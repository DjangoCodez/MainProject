import { Component } from '@angular/core';
import { ProductUnitsGridComponent } from '../product-units-grid/product-units-grid.component';
import { ProductUnitsEditComponent } from '../product-units-edit/product-units-edit.component';
import { ProductUnitsForm } from '../../models/product-units-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ProductUnitsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ProductUnitsGridComponent,
      editComponent: ProductUnitsEditComponent,
      FormClass: ProductUnitsForm,
      gridTabLabel: 'billing.product.productunit.productunits',
      editTabLabel: 'billing.product.productunit.productunit',
      createTabLabel: 'billing.product.productunit.new',
      exportFilenameKey: 'billing.product.productunit.productunits',
    },
  ];
}
