import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PricelistTypeDialogComponent } from './component/pricelist-type-dialog/pricelist-type-dialog.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { LabelComponent } from '@ui/label/label.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [PricelistTypeDialogComponent],
  imports: [
    CommonModule,
    SharedModule,
    DialogComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    FormsModule,
    SaveButtonComponent,
    DeleteButtonComponent,
    TextboxComponent,
    SelectComponent,
    LabelComponent,
    CheckboxComponent,
  ],
  exports: [PricelistTypeDialogComponent],
})
export class PricelistTypeDialogModule {}
