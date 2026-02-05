import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridComponent } from '@ui/grid/grid.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { SelectCustomerDialogComponent } from './components/select-customer-dialog/select-customer-dialog.component';
import { SelectCustomerGridComponent } from './components/select-customer-grid/select-customer-grid.component';

@NgModule({
  declarations: [SelectCustomerGridComponent, SelectCustomerDialogComponent],
  imports: [
    CommonModule,
    DialogComponent,
    GridComponent,
    ButtonComponent,
    TextboxComponent,
    ReactiveFormsModule,
    LabelComponent,
    InstructionComponent,
  ],
  exports: [SelectCustomerGridComponent, SelectCustomerDialogComponent],
})
export class SelectCustomerDialogModule {}
