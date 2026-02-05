import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BatchUpdateComponent } from './components/batch-update/batch-update.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { IconModule } from '@ui/icon/icon.module'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';

@NgModule({
  declarations: [BatchUpdateComponent],
  exports: [BatchUpdateComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    ExpansionPanelComponent,
    DialogComponent,
    LabelComponent,
    TextboxComponent,
    ToolbarComponent,
    SelectComponent,
    MultiSelectComponent,
    InstructionComponent,
    ButtonComponent,
    IconButtonComponent,
    SaveButtonComponent,
    IconModule,
    DatepickerComponent,
    NumberboxComponent,
    CheckboxComponent,
    TimeboxComponent,
  ],
})
export class BatchUpdateModule {}
