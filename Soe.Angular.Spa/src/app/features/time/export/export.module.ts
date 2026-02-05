import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ExportRoutingModule } from './export-routing.module';
import { ExportStandardDefinitionsEditComponent } from './export-standard-definitions/components/export-standard-definitions-edit/export-standard-definitions-edit.component';
import { ExportStandardDefinitionsLevelEditComponent } from './export-standard-definitions/components/export-standard-definitions-edit/export-standard-definitions-level-edit/export-standard-definitions-level-edit.component';
import { ExportStandardDefinitionsGridComponent } from './export-standard-definitions/components/export-standard-definitions-grid/export-standard-definitions-grid.component';
import { ExportStandardDefinitionsComponent } from './export-standard-definitions/components/export-standard-definitions/export-standard-definitions.component';

@NgModule({
  declarations: [
    ExportStandardDefinitionsComponent,
    ExportStandardDefinitionsEditComponent,
    ExportStandardDefinitionsGridComponent,
    ExportStandardDefinitionsLevelEditComponent,
  ],
  exports: [
    ExportStandardDefinitionsComponent,
    ExportStandardDefinitionsEditComponent,
    ExportStandardDefinitionsGridComponent,
    ExportStandardDefinitionsLevelEditComponent,
  ],
  imports: [
    SharedModule,
    ExportRoutingModule,
    MultiTabWrapperComponent,
    DialogComponent,
    EditFooterComponent,
    TextboxComponent,
    NumberboxComponent,
    ToolbarComponent,
    SelectComponent,
    GridWrapperComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    CheckboxComponent,
    ExpansionPanelComponent,
    CommonModule,
    ReactiveFormsModule,
  ],
})
export class ExportModule {}
