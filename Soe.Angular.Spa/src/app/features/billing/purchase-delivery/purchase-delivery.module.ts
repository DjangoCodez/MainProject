import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { PurchaseDeliveryRoutingModule } from './purchase-delivery-routing.module';
import { PurchaseDeliveryGridComponent } from './components/purchase-delivery-grid/purchase-delivery-grid.component';
import { PurchaseDeliveryEditComponent } from './components/purchase-delivery-edit/purchase-delivery-edit.component';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PurchaseDeliveryAwaitingDeliveryGridComponent } from './components/purchase-delivery-awaiting-delivery-grid/purchase-delivery-awaiting-delivery-grid.component';
import { PurchaseCustomerInvoiceRowsModule } from '@shared/components/billing/purchase-customer-invoice-rows/purchase-customer-invoice-rows.module';
import { DeliveryRowsModule } from '@shared/components/billing/delivery-rows/delivery-rows.module';
import { PurchaseDeliveryComponent } from './components/purchase-delivery/purchase-delivery.component';
import { PurchaseModule } from '../purchase/purchase.module';

@NgModule({
  declarations: [
    PurchaseDeliveryComponent,
    PurchaseDeliveryGridComponent,
    PurchaseDeliveryEditComponent,
    PurchaseDeliveryAwaitingDeliveryGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    PurchaseDeliveryRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    CheckboxComponent,
    DatepickerComponent,
    EditFooterComponent,
    GridWrapperComponent,
    AutocompleteComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    DeliveryRowsModule,
    PurchaseCustomerInvoiceRowsModule,
    PurchaseModule,
  ],
})
export class PurchaseDeliveryModule {}
