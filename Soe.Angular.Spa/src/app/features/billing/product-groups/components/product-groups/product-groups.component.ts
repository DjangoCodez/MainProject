import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ProductGroupsForm } from '../../models/product-groups-form.model';
import { ProductGroupsGridComponent } from '../product-groups-grid/product-groups-grid.component';
import { ProductGroupsEditComponent } from '../product-groups-edit/product-groups-edit.component';

@Component({
  selector: 'soe-product-groups',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ProductGroupsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ProductGroupsGridComponent,
      editComponent: ProductGroupsEditComponent,
      FormClass: ProductGroupsForm,
      gridTabLabel: 'billing.invoices.productgroups.productgroups',
      editTabLabel: 'billing.invoices.productgroups.productgroup',
      createTabLabel: 'billing.invoices.productgroups.new_productgroup',
    },
  ];
}
