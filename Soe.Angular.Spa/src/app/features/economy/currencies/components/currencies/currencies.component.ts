import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CurrenciesGridComponent } from '../currencies-grid/currencies-grid.component';
import { CurrenciesEditComponent } from '../currencies-edit/currencies-edit.component';
import { CurrenciesForm } from '../../models/currencies-form.model';

@Component({
  selector: 'soe-match-settings',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CurrenciesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CurrenciesGridComponent,
      editComponent: CurrenciesEditComponent,
      FormClass: CurrenciesForm,
      gridTabLabel: 'economy.accounting.currency.currencies',
      editTabLabel: 'economy.accounting.currency.currency',
      createTabLabel: 'economy.accounting.currency.newcurrency',
    },
  ];
}
