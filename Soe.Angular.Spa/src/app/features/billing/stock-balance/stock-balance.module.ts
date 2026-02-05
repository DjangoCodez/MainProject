import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StockBalanceRoutingModule } from './stock-balance-routing.module';
import { StockBalanceComponent } from './components/stock-balance/stock-balance.component';
import { StockBalanceGridComponent } from './components/stock-balance-grid/stock-balance-grid.component';
import { StockBalanceEditComponent } from './components/stock-balance-edit/stock-balance-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { StockBalanceGridFilterComponent } from './components/stock-balance-grid-filter/stock-balance-grid-filter.component';
import { StockBalanceEditStockTransactionComponent } from './components/stock-balance-edit-stock-transaction/stock-balance-edit-stock-transaction.component';
import { StockBalanceFileImportModule } from '@billing/shared/components/stock-balance-file-import/stock-balance-file-import.module';
import { VoucherModule } from '@features/economy/voucher/voucher.module';

@NgModule({
  declarations: [
    StockBalanceComponent,
    StockBalanceGridComponent,
    StockBalanceEditComponent,
    StockBalanceGridFilterComponent,
    StockBalanceEditStockTransactionComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    StockBalanceRoutingModule,
    EditFooterComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    GridWrapperComponent,
    ButtonComponent,
    SaveButtonComponent,
    SelectComponent,
    ExpansionPanelComponent,
    DatepickerComponent,
    DialogComponent,
    CheckboxComponent,
    VoucherModule,
    AutocompleteComponent,
    StockBalanceFileImportModule,
    VoucherModule,
  ],
})
export class StockBalanceModule {}
