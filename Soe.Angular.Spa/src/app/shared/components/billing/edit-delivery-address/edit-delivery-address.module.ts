import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EditDeliveryAddressComponent } from './edit-delivery-address.component';

@NgModule({
  declarations: [EditDeliveryAddressComponent],
  exports: [EditDeliveryAddressComponent],
  imports: [
    CommonModule,
    TranslateModule,
    ButtonComponent,
    SaveButtonComponent,
    DialogComponent,
    TextareaComponent,
    CheckboxComponent,
    TextboxComponent,
    ReactiveFormsModule,
    SharedModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    InstructionComponent,
  ],
})
export class EditDeliveryAddressModule {}
