import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { PurchaseRoutingModule } from './purchase-routing.module';
import { PurchaseComponent } from './components/purchase/purchase.component';
import { PurchaseGridComponent } from './components/purchase-grid/purchase-grid.component';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { CreatedModifiedComponent } from '@ui/created-modified/created-modified.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { PurchaseEditComponent } from './components/purchase-edit/purchase-edit.component';
import { SelectCustomerInvoiceDialogModule } from '@shared/components/select-customer-invoice-dialog/select-customer-invoice-dialog.module';
import { SelectProjectDialogModule } from '@shared/components/select-project-dialog/select-project-dialog.module';
import { PurchaseSetPurchaseDateDialogComponent } from './components/purchase-set-purchase-date-dialog/purchase-set-purchase-date-dialog.component';
import { PurchaseSetPurchaseDateComponent } from './components/purchase-set-purchase-date-dialog/purchase-set-purchase-date/purchase-set-purchase-date/purchase-set-purchase-date.component';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
import { PurchaseCustomerInvoiceRowsModule } from '@shared/components/billing/purchase-customer-invoice-rows/purchase-customer-invoice-rows.module';
import { DeliveryRowsModule } from '@shared/components/billing/delivery-rows/delivery-rows.module';
import { SelectReportDialogModule } from '@shared/components/select-report-dialog/select-report-dialog.module';
import { TextBlockDialogModule } from '@shared/components/text-block-dialog/text-block-dialog.module';
import { SelectEmailDialogModule } from '@shared/components/select-email-dialog/select-email-dialog.module';
import { PurchaseDeliveryAddressesDialogComponent } from './components/purchase-delivery-addresses-dialog/purchase-delivery-addresses-dialog.component';
import { PurchaseGridHeaderComponent } from './components/purchase-grid-header/purchase-grid-header.component';
import { PurchaseRowsComponent } from './components/purchase-rows/purchase-rows.component';
import { ChangeIntrastatCodeModule } from '@shared/components/billing/change-intrastat-code/change-intrastat-code.module';

@NgModule({
  declarations: [
    PurchaseComponent,
    PurchaseGridComponent,
    PurchaseGridHeaderComponent,
    PurchaseEditComponent,
    PurchaseSetPurchaseDateComponent,
    PurchaseSetPurchaseDateDialogComponent,
    PurchaseDeliveryAddressesDialogComponent,
    PurchaseRowsComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    PurchaseRoutingModule,
    CreatedModifiedComponent,
    ExpansionPanelComponent,
    ButtonComponent,
    DeleteButtonComponent,
    IconButtonComponent,
    MenuButtonComponent,
    CheckboxComponent,
    GridWrapperComponent,
    EditFooterComponent,
    SelectComponent,
    MultiSelectComponent,
    DatepickerComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    LabelComponent,
    AutocompleteComponent,
    NumberboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    SelectCustomerInvoiceDialogModule,
    TextBlockDialogModule,
    SelectProjectDialogModule,
    DialogComponent,
    InstructionComponent,
    TraceRowsModule,
    PurchaseCustomerInvoiceRowsModule,
    DeliveryRowsModule,
    SelectReportDialogModule,
    SelectEmailDialogModule,
    ChangeIntrastatCodeModule,
  ],
  exports: [
    PurchaseEditComponent,
    PurchaseSetPurchaseDateComponent,
    PurchaseSetPurchaseDateDialogComponent,
    PurchaseRowsComponent,
  ],
})
export class PurchaseModule {}
