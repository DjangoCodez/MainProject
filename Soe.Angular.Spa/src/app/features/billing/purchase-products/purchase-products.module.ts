import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PurchaseProductsRoutingModule } from './purchase-products-routing.module';
import { PurchaseProductsComponent } from './components/purchase-products/purchase-products.component';
import { PurchaseProductsGridComponent } from './components/purchase-products-grid/purchase-products-grid.component';
import { PurchaseProductsEditComponent } from './components/purchase-products-edit/purchase-products-edit.component';
import { ImportProductsDialogComponent } from './components/import-products-dialog/import-products-dialog.component';
import { PriceUpdateComponent } from './components/purchase-products-grid/price-update-modal/price-update-modal.component';
import { ImportDynamicModule } from '@shared/components/billing/import-dynamic/import-dynamic.module';
import { SupplierProductPricesComponent } from './components/supplier-product-prices/supplier-product-prices.component';
import { PurchaseProductsGridHeaderComponent } from './components/purchase-products-grid-header/purchase-products-grid-header.component';

@NgModule({
  declarations: [
    PurchaseProductsComponent,
    PurchaseProductsGridComponent,
    PurchaseProductsEditComponent,
    PurchaseProductsGridHeaderComponent,
    ImportProductsDialogComponent,
    PriceUpdateComponent,
    SupplierProductPricesComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ImportDynamicModule,
    PurchaseProductsRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    SaveButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiSelectComponent,
    AutocompleteComponent,
    NumberboxComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    LabelComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    DialogComponent,
    InstructionComponent,
    DatepickerComponent,
  ],
})
export class PurchaseProductsModule {}
