import { Component } from '@angular/core';
import { VoucherSeriesEditComponent } from '../voucher-series/voucher-series-edit/voucher-series-edit.component';
import { VoucherSeriesGridComponent } from '../voucher-series/voucher-series-grid/voucher-series-grid.component';
import { VoucherSeriesForm } from '../../models/voucher-series-form.model';
import { AccountYearGridComponent } from '../account-year/account-year-grid/account-year-grid.component';
import { AccountYearEditComponent } from '../account-year/account-year-edit/account-year-edit.component';
import { AccountYearForm } from '../../models/account-year-form.model';
import { OpeningBalancesGridComponent } from '../opening-balances/opening-balances-grid/opening-balances-grid.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class YearsAndPeriodsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: AccountYearGridComponent,
      editComponent: AccountYearEditComponent,
      FormClass: AccountYearForm,
      gridTabLabel: 'economy.accounting.accountyear.accountyears',
      editTabLabel: 'economy.accounting.accountyear.accountyear',
      createTabLabel: 'economy.accounting.accountyear.newaccountyear',
    },
    {
      gridComponent: OpeningBalancesGridComponent,
      FormClass: undefined,
      gridTabLabel: 'economy.accounting.balance.balance',
      hideForCreateTabMenu: true,
    },
    {
      gridComponent: VoucherSeriesGridComponent,
      editComponent: VoucherSeriesEditComponent,
      FormClass: VoucherSeriesForm,
      gridTabLabel: 'economy.accounting.voucherseriestypes',
      editTabLabel: 'economy.accounting.voucherseriestype',
      createTabLabel: 'economy.accounting.newvoucherseriestype',
    },
  ];
}
