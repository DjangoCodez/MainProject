import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { AddAccountDialogComponent } from './components/add-account-dialog/add-account-dialog.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';

@NgModule({
  declarations: [AddAccountDialogComponent],
  imports: [
    CommonModule,
    ButtonComponent,
    SelectComponent,
    DialogComponent,
    TextboxComponent,
    InstructionComponent,
    ReactiveFormsModule,
    SaveButtonComponent,
  ],
  exports: [AddAccountDialogComponent],
})
export class AddAccountDialogModule {}
