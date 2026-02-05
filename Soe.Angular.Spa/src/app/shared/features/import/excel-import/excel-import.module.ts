import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { LanguageTranslationsModule } from '@shared/features/language-translations/language-translations.module';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ExcelImportConflictsGridComponent } from './components/excel-import-conflicts-grid/excel-import-conflicts-grid.component';
import { ExcelImportGridComponent } from './components/excel-import-grid/excel-import-grid.component';
import { ExcelImportComponent } from './components/excel-import/excel-import.component';
import { ExcelImportRoutingModule } from './excel-import-routing.module';

@NgModule({
  declarations: [
    ExcelImportComponent,
    ExcelImportGridComponent,
    ExcelImportConflictsGridComponent,
  ],
  imports: [
    CommonModule,
    ExcelImportRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    SaveButtonComponent,
    CommonModule,
    ReactiveFormsModule,
    LanguageTranslationsModule,
    EditFooterComponent,
    GridWrapperComponent,
    SharedModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    CheckboxComponent,
    FileUploadComponent,
  ],
})
export class ExcelImportModule {}
