import { Component, inject } from '@angular/core';
import { VoucherEditComponent } from '../voucher-edit/voucher-edit.component';
import { VoucherGridComponent } from '../voucher-grid/voucher-grid.component';
import { VoucherForm } from '../../models/voucher-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { VoucherParamsService } from '../../services/voucher-params.service';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class VoucherComponent {
  private readonly urlService = inject(VoucherParamsService);

  config: MultiTabConfig[] = [
    {
      gridComponent: VoucherGridComponent,
      editComponent: VoucherEditComponent,
      FormClass: VoucherForm,
      recordConfig: {
        hideIfEmpty: true,
        hidePosition: false,
        showRecordName: false,
        hideDropdown: true,
        dropdownTextProperty: 'name',
        isDate: false,
        refetchDataOnRecordChange: true,
        hideRecordNavigator: true,
      },
      exportFilenameKey: 'economy.accounting.voucher.voucher',
      gridTabLabel:
        'economy.accounting.voucher.' +
        (this.urlService.isTemplate() ? 'templates' : 'vouchers'),
      editTabLabel:
        'economy.accounting.voucher.' +
        (this.urlService.isTemplate() ? 'template' : 'voucher'),
      createTabLabel:
        'economy.accounting.voucher.' +
        (this.urlService.isTemplate() ? 'newtemplate' : 'new'),
    },
  ];
}
