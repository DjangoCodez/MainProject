import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StockPurchaseGridFilterComponent } from './components/stock-purchase-grid-filter/stock-purchase-grid-filter.component';
import { StockPurchaseRoutingModule } from './stock-purchase-routing.module';
import { StockPurchaseComponent } from '../stock-purchase/components/stock-purchase/stock-purchase.component';

import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { StockPurchaseGridComponent } from './components/stock-purchase-grid/stock-purchase-grid.component';
import { EditDeliveryAddressModule } from '@shared/components/billing/edit-delivery-address/edit-delivery-address.module';

@NgModule({
  declarations: [
    StockPurchaseComponent,
    StockPurchaseGridComponent,
    StockPurchaseGridFilterComponent,
  ],
  imports: [
    CommonModule,
    ExpansionPanelComponent,
    ButtonComponent,
    IconButtonComponent,
    GridWrapperComponent,
    EditFooterComponent,
    SelectComponent,
    MultiSelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    NumberboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    SharedModule,
    StockPurchaseRoutingModule,
    CheckboxComponent,
    DialogComponent,
    TextareaComponent,
    EditDeliveryAddressModule,
  ],
})
export class StockPurchaseModule {}
