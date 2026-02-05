import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { IconModule } from '@ui/icon/icon.module';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { LabelComponent } from '@ui/label/label.component';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SieRoutingModule } from './sie-routing.module';
import { SieComponent } from './components/sie/sie.component';
import { SieEditComponent } from './components/sie-edit/sie-edit.component';
import { SieConflictsGridComponent } from './components/sie-edit/sie-conflicts-grid/sie-conflicts-grid.component';
import { SiePreviewAccountGridComponent } from './components/sie-edit/sie-preview-account-grid/sie-preview-account-grid.component';
import { SieImportHistoryGridComponent } from './components/sie-import-history-dialog/sie-import-history-grid/sie-import-history-grid.component';
import { SieImportHistoryDialogComponent } from './components/sie-import-history-dialog/sie-import-history-dialog.component';

@NgModule({
  declarations: [
    SieComponent,
    SieEditComponent,
    SieConflictsGridComponent,
    SieImportHistoryGridComponent,
    SieImportHistoryDialogComponent,
    SiePreviewAccountGridComponent,
  ],
  imports: [
    CommonModule,
    SieRoutingModule,
    MultiTabWrapperComponent,
    ReactiveFormsModule,
    SelectComponent,
    MultiSelectComponent,
    LabelComponent,
    ExpansionPanelComponent,
    CheckboxComponent,
    TextboxComponent,
    NumberboxComponent,
    ButtonComponent,
    SaveButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    FileUploadComponent,
    IconModule,
    InstructionComponent,
    ToolbarComponent,
    DialogComponent,
  ],
})
export class SieModule {}
