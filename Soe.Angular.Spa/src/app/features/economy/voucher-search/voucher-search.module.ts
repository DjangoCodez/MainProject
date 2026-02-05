import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { ReactiveFormsModule } from '@angular/forms';
import { DatePeriodModule } from '@shared/components/date-period/date-period.module';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { VoucherSearchDialogComponent } from './components/voucher-search-dialog/voucher-search-dialog.component';
import { VoucherSearchFilterComponent } from './components/voucher-search-grid/voucher-search-filter/voucher-search-filter.component';
import { VoucherSearchGridComponent } from './components/voucher-search-grid/voucher-search-grid.component';
import { VoucherSearchComponent } from './components/voucher-search/voucher-search.component';
import { VoucherSearchRoutingModule } from './voucher-search-routing.module';
import { VoucherModule } from '../voucher/voucher.module';

@NgModule({
  declarations: [
    VoucherSearchComponent,
    VoucherSearchGridComponent,
    VoucherSearchFilterComponent,
    VoucherSearchDialogComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    VoucherSearchRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    SelectComponent,
    MultiSelectComponent,
    DatepickerComponent,
    TextboxComponent,
    ButtonComponent,
    SaveButtonComponent,
    DatePeriodModule,
    NumberboxComponent,
    CheckboxComponent,
    ButtonComponent,
    AutocompleteComponent,
    DialogComponent,
    VoucherModule,
    MultiTabWrapperComponent,
  ],
})
export class VoucherSearchModule {}
