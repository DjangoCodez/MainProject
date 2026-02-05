import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { RadioComponent } from '@ui/forms/radio/radio.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { SharedModule } from '@shared/shared.module';
import { SelectEmailDialogComponent } from './components/select-email-dialog/select-email-dialog.component';
import { ReactiveFormsModule } from '@angular/forms';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';

@NgModule({
  declarations: [SelectEmailDialogComponent],
  exports: [SelectEmailDialogComponent],
  imports: [
    CommonModule,
    SharedModule,
    DialogComponent,
    ButtonComponent,
    SelectComponent,
    CheckboxComponent,
    LabelComponent,
    RadioComponent,
    ReactiveFormsModule,
    InstructionComponent,
    TextboxComponent,
  ],
})
export class SelectEmailDialogModule {}
