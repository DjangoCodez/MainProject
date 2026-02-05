import { Component } from '@angular/core';
import { GrossProfitCodesForm } from '../../models/gross-profit-codes-form.model';
import { GrossProfitCodesEditComponent } from '../gross-profit-codes-edit/gross-profit-codes-edit.component';
import { GrossProfitCodesGridComponent } from '../gross-profit-codes-grid/gross-profit-codes-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class GrossProfitCodesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: GrossProfitCodesGridComponent,
      editComponent: GrossProfitCodesEditComponent,
      FormClass: GrossProfitCodesForm,
      exportFilenameKey: 'economy.accounting.grossprofitcode.grossprofitcodes',
      gridTabLabel: 'economy.accounting.grossprofitcode.grossprofitcodes',
      editTabLabel: 'economy.accounting.grossprofitcode.grossprofitcode',
      createTabLabel: 'economy.accounting.grossprofitcode.new_grossprofitcode',
    },
  ];
}
