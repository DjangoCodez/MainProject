import { DragDropModule } from '@angular/cdk/drag-drop';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { TranslateModule } from '@ngx-translate/core';
import { DynamicGridModule } from '@shared/components/dynamic-grid/dynamic-grid.module';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { IconModule } from '@ui/icon/icon.module'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { RecordNavigatorComponent } from '@ui/record-navigator/record-navigator.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ColumnMapperComponent } from './import-dynamic/column-mapper/column-mapper.component';
import { FieldOptionsComponent } from './import-dynamic/field-options/field-options.component';
import { ImportDynamicComponent } from './import-dynamic/import-dynamic.component';

@NgModule({
  declarations: [
    ImportDynamicComponent,
    ColumnMapperComponent,
    FieldOptionsComponent,
  ],
  imports: [
    CommonModule,
    ExpansionPanelComponent,
    DialogComponent,
    ReactiveFormsModule,
    ButtonComponent,
    IconButtonComponent,
    LabelComponent,
    MatStepperModule,
    MatIconModule,
    IconModule,
    TranslateModule,
    SelectComponent,
    TextboxComponent,
    NumberboxComponent,
    CheckboxComponent,
    RecordNavigatorComponent,
    InstructionComponent,
    GridWrapperComponent,
    DatepickerComponent,
    DragDropModule,
    SharedModule,
    DynamicGridModule,
  ],
  exports: [ImportDynamicComponent],
})
export class ImportDynamicModule {}
