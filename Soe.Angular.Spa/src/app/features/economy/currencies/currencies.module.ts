import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
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
import { SharedModule } from '@shared/shared.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CurrenciesComponent } from './components/currencies/currencies.component';
import { CurrenciesEditComponent } from './components/currencies-edit/currencies-edit.component';
import { CurrenciesGridComponent } from './components/currencies-grid/currencies-grid.component';
import { CurrenciesRoutingModule } from './currencies-routing.module';
import { CurrencyRatesGridComponent } from './components/currencies-edit/currency-rates-grid/currency-rates-grid.component';
import { CurrencyRatesEditModal } from './components/currencies-edit/currency-rates-edit-modal/currency-rates-edit-modal.component';

@NgModule({
  declarations: [
    CurrenciesComponent,
    CurrenciesEditComponent,
    CurrenciesGridComponent,
    CurrencyRatesGridComponent,
    CurrencyRatesEditModal,
  ],
  imports: [
    SharedModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    SelectComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    EditFooterComponent,
    ButtonComponent,
    SaveButtonComponent,
    CommonModule,
    CurrenciesRoutingModule,
    DialogComponent,
    DatepickerComponent,
  ],
})
export class CurrenciesModule {}
