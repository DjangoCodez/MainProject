import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomerPaymentMethodsRoutingModule } from './customer-payment-methods-routing.module';
import { CustomerPaymentMethodsComponent } from './components/customer-payment-methods/customer-payment-methods/customer-payment-methods.component';
import { CustomerPaymentMethodsGridComponent } from './components/customer-payment-methods-grid/customer-payment-methods-grid/customer-payment-methods-grid.component';
import { CustomerPaymentMethodsEditComponent } from './components/customer-payment-methods-edit/customer-payment-methods-edit/customer-payment-methods-edit.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
@NgModule({
  declarations: [
    CustomerPaymentMethodsComponent,
    CustomerPaymentMethodsGridComponent,
    CustomerPaymentMethodsEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    CustomerPaymentMethodsRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    NumberboxComponent,
    ToolbarComponent,
    EditFooterComponent,
  ],
})
export class CustomerPaymentMethodsModule {}
