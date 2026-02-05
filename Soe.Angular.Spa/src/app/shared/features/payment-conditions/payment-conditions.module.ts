import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { PaymentConditionsEditComponent } from './components/payment-conditions-edit/payment-conditions-edit.component';
import { PaymentConditionsGridComponent } from './components/payment-conditions-grid/payment-conditions-grid.component';
import { PaymentConditionsComponent } from './components/payment-conditions/payment-conditions.component';
import { PaymentConditionsRoutingModule } from './payment-conditions-routing.module';

@NgModule({
  declarations: [
    PaymentConditionsComponent,
    PaymentConditionsEditComponent,
    PaymentConditionsGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    PaymentConditionsRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    CheckboxComponent,
  ],
})
export class PaymentConditionsModule {}
