import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { YearsAndPeriodsComponent } from './components/account-years-and-periods/account-years-and-periods.component';
import { VoucherSeriesEditComponent } from './components/voucher-series/voucher-series-edit/voucher-series-edit.component';
import { VoucherSeriesGridComponent } from './components/voucher-series/voucher-series-grid/voucher-series-grid.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { YearsAndPeriodsRoutingModule } from './account-years-and-periods-routing.module';
import { AccountYearGridComponent } from './components/account-year/account-year-grid/account-year-grid.component';
import { AccountYearEditComponent } from './components/account-year/account-year-edit/account-year-edit.component';
import { AccountYearPeriodGridComponent } from './components/account-year/account-year-edit/account-year-period-grid/account-year-period-grid.component';
import { VoucherSeriesTypeGridComponent } from './components/account-year/account-year-edit/voucher-series-type-grid/voucher-series-type-grid.component';
import { VoucherTemplateGridComponent } from './components/account-year/account-year-edit/voucher-template-grid/voucher-template-grid.component';
import { GrossProfitCodesGridComponent } from './components/account-year/account-year-edit/gross-profit-codes-grid/gross-profit-codes-grid.component';
import { OpeningBalancesGridComponent } from './components/opening-balances/opening-balances-grid/opening-balances-grid.component';
import { SetAccountDialogComponent } from './components/opening-balances/set-account-dialog/set-account-dialog.component';

@NgModule({
  declarations: [
    YearsAndPeriodsComponent,
    VoucherSeriesEditComponent,
    VoucherSeriesGridComponent,
    VoucherSeriesTypeGridComponent,
    VoucherTemplateGridComponent,
    GrossProfitCodesGridComponent,
    AccountYearGridComponent,
    AccountYearEditComponent,
    AccountYearPeriodGridComponent,
    OpeningBalancesGridComponent,
    SetAccountDialogComponent,
  ],
  imports: [
    CommonModule,
    MultiTabWrapperComponent,
    SharedModule,
    YearsAndPeriodsRoutingModule,
    ButtonComponent,
    IconButtonComponent,
    SaveButtonComponent,
    EditFooterComponent,
    DatepickerComponent,
    GridWrapperComponent,
    DialogComponent,
    SelectComponent,
    NumberboxComponent,
    ReactiveFormsModule,
    TextboxComponent,
    ToolbarComponent,
    LabelComponent,
    AutocompleteComponent,
    CheckboxComponent,
    InstructionComponent,
    ExpansionPanelComponent,
  ],
})
export class YearsAndPeriodsModule {}
