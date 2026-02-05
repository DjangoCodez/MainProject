import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SupplierPaymentMethodsRoutingModule } from './supplier-payment-methods-routing.module';
import { SupplierPaymentMethodsComponent } from './components/supplier-payment-methods/supplier-payment-methods.component';
import { SupplierPaymentMethodsGridComponent } from './components/supplier-payment-methods-grid/supplier-payment-methods-grid.component';
import { SupplierPaymentMethodsEditComponent } from './components/supplier-payment-methods-edit/supplier-payment-methods-edit.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [
    SupplierPaymentMethodsComponent,
    SupplierPaymentMethodsGridComponent,
    SupplierPaymentMethodsEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    SupplierPaymentMethodsRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    GridWrapperComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ReactiveFormsModule,
    ToolbarComponent,
    EditFooterComponent,
  ],
})
export class SupplierPaymentMethodsModule {}
