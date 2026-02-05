import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HandleBillingRoutingModule } from './handle-billing-routing.module';
import { HandleBillingComponent } from './components/handle-billing/handle-billing.component';
import { HandleBillingGridComponent } from './components/handle-billing-grid/handle-billing-grid.component';
import { HandleBillingGridHeaderComponent } from './components/handle-billing-grid-header/handle-billing-grid-header.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DaterangepickerComponent } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    HandleBillingComponent,
    HandleBillingGridComponent,
    HandleBillingGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    HandleBillingRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    LabelComponent,
    SelectComponent,
    MultiSelectComponent,
    EditFooterComponent,
    ButtonComponent,
    DaterangepickerComponent,
    DialogComponent,
    InstructionComponent,
    CheckboxComponent,
    ReactiveFormsModule,
  ],
})
export class HandleBillingModule {}
