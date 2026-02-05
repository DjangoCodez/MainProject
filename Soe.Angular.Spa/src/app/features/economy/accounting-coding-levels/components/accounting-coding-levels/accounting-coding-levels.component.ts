import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AccountingCodingLevelsEditComponent } from '../accounting-coding-levels-edit/accounting-coding-levels-edit.component';
import { AccountingCodingLevelsGridComponent } from '../accounting-coding-levels-grid/accounting-coding-levels-grid.component';
import { AccountDimForm } from '../../models/accounting-coding-levels-form.model';

@Component({
  selector: 'soe-accounting-coding-levels',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class AccountingCodingLevelsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AccountingCodingLevelsGridComponent,
      editComponent: AccountingCodingLevelsEditComponent,
      FormClass: AccountDimForm,
      gridTabLabel: 'economy.accounting.accountdims',
      editTabLabel: 'economy.accounting.accountdim',
      createTabLabel: 'economy.accounting.newaccountdim',
      exportFilenameKey: 'economy.accounting.accountdims',
    },
  ];
}
