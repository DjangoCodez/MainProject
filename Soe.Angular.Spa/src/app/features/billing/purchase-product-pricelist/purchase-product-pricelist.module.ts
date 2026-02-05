import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PurchaseProductPricelistRoutingModule } from './purchase-product-pricelist-routing.module';
import { PurchaseProductPricelistComponent } from './components/purchase-product-pricelist/purchase-product-pricelist.component';
import { PurchaseProductPricelistEditComponent } from './components/purchase-product-pricelist-edit/purchase-product-pricelist-edit.component';
import { PurchaseProductPricelistGridComponent } from './components/purchase-product-pricelist-grid/purchase-product-pricelist-grid.component';
import { PurchaseProductPricelistGridHeaderComponent } from './components/purchase-product-pricelist-grid-header/purchase-product-pricelist-grid-header.component';
import { PurchaseProductPricelistPricesComponent } from './components/purchase-product-pricelist-prices/purchase-product-pricelist-prices.component';
import { ImportDynamicModule } from '@shared/components/billing/import-dynamic/import-dynamic.module';

@NgModule({
  declarations: [
    PurchaseProductPricelistComponent,
    PurchaseProductPricelistGridComponent,
    PurchaseProductPricelistEditComponent,
    PurchaseProductPricelistGridHeaderComponent,
    PurchaseProductPricelistPricesComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    PurchaseProductPricelistRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    MenuButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    AutocompleteComponent,
    LabelComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    DatepickerComponent,
    ImportDynamicModule,
  ],
})
export class PurchaseProductPricelistModule {}
