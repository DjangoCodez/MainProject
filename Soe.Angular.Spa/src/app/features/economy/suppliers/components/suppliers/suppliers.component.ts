import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { SupplierHeadForm } from '../../models/supplier-head-form.model';
import { SuppliersGridComponent } from '../suppliers-grid/suppliers-grid.component';
import { SuppliersEditComponent } from '../suppliers-edit/suppliers-edit.component';

@Component({
  selector: 'soe-suppliers',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  styleUrls: ['./suppliers.component.scss'],
  standalone: false,
})
export class SuppliersComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: SuppliersGridComponent,
      editComponent: SuppliersEditComponent,
      FormClass: SupplierHeadForm,
      gridTabLabel: 'economy.supplier.supplier.suppliers',
      editTabLabel: 'economy.supplier.supplier.supplier',
      createTabLabel: 'economy.supplier.supplier.new',
    },
  ];
}
