import { Component } from '@angular/core';
import { VatCodeForm } from '../../models/vat-codes-form.model';
import { VatCodesGridComponent } from '../vat-codes-grid/vat-codes-grid.component';
import { VatCodesEditComponent } from '../vat-codes-edit/vat-codes-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class VatCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: VatCodesGridComponent,
      editComponent: VatCodesEditComponent,
      FormClass: VatCodeForm,
      gridTabLabel: 'economy.accounting.vatcode.vatcodes',
      editTabLabel: 'economy.accounting.vatcode.vatcode',
      createTabLabel: 'economy.accounting.vatcode.new_vatcode',
    },
  ];
}
