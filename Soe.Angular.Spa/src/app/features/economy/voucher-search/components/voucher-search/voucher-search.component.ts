import { Component } from '@angular/core';
import { VoucherSearchGridComponent } from '../voucher-search-grid/voucher-search-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';

@Component({
  selector: 'soe-voucher-search',
  templateUrl: 'voucher-search.component.html',
  standalone: false,
})
export class VoucherSearchComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: VoucherSearchGridComponent,
      editComponent: VoucherEditComponent,
      FormClass: VoucherForm,
      gridTabLabel: 'economy.accounting.vouchersearch',
      exportFilenameKey: 'economy.accounting.vouchersearch',
      editTabLabel: 'economy.accounting.voucher.voucher',
      createTabLabel: 'economy.accounting.voucher.new',
    },
  ];
}
