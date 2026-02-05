import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { AccountingSettingsModule } from '@shared/components/accounting-settings/accounting-settings.module';
import { StockWarehouseEditProductsGridComponent } from './components/stock-warehouse-edit/stock-warehouse-edit-products-grid/stock-warehouse-edit-products-grid.component';
import { StockWarehouseComponent } from './components/stock-warehouse/stock-warehouse.component';
import { StockWarehouseGridComponent } from './components/stock-warehouse-grid/stock-warehouse-grid.component';
import { StockWarehouseEditComponent } from './components/stock-warehouse-edit/stock-warehouse-edit.component';
import { StockWarehouseEditGridComponent } from './components/stock-warehouse-edit/stock-warehouse-edit-grid/stock-warehouse-edit-grid.component';
import { StockWarehouseRoutingModule } from './stock-warehouse-routing.module';

@NgModule({
  declarations: [
    StockWarehouseComponent,
    StockWarehouseGridComponent,
    StockWarehouseEditComponent,
    StockWarehouseEditGridComponent,
    StockWarehouseEditProductsGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    StockWarehouseRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    CheckboxComponent,
    SelectComponent,
    AccountingSettingsModule,
  ],
})
export class StockWarehouseModule {}
