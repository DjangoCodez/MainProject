import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { StockInventoryRoutingModule } from './stock-inventory-routing.module';
import { StockInventoryComponent } from './components/stock-inventory/stock-inventory.component';
import { StockInventoryEditComponent } from './components/stock-inventory-edit/stock-inventory-edit.component';
import { StockInventoryGridComponent } from './components/stock-inventory-grid/stock-inventory-grid.component';
import { ReactiveFormsModule } from '@angular/forms';
import { StockInventoryEditItemGridComponent } from './components/stock-inventory-edit/stock-inventory-edit-item-grid/stock-inventory-edit-item-grid.component';
import { StockBalanceFileImportModule } from '@billing/shared/components/stock-balance-file-import/stock-balance-file-import.module';

@NgModule({
  declarations: [
    StockInventoryComponent,
    StockInventoryEditComponent,
    StockInventoryGridComponent,
    StockInventoryEditItemGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    StockInventoryRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    MenuButtonComponent,
    CheckboxComponent,
    SelectComponent,
    MultiSelectComponent,
    DialogComponent,
    StockBalanceFileImportModule,
    AutocompleteComponent,
  ],
})
export class StockInventoryModule {}
