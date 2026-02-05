import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridComponent } from '@ui/grid/grid.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { FieldSettingsEditGridComponent } from './components/field-settings-edit/field-settings-edit-grid/field-settings-edit-grid.component';
import { FieldSettingsEditComponent } from './components/field-settings-edit/field-settings-edit.component';
import { FieldSettingsGridComponent } from './components/field-settings-grid/field-settings-grid.component';
import { FieldSettingsComponent } from './components/field-settings/field-settings.component';
import { FieldSettingsRoutingModule } from './field-settings-routing.module';

@NgModule({
  declarations: [
    FieldSettingsComponent,
    FieldSettingsGridComponent,
    FieldSettingsEditComponent,
    FieldSettingsEditGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FieldSettingsRoutingModule,
    MultiTabWrapperComponent,
    GridComponent,
    GridWrapperComponent,
    EditFooterComponent,
    ToolbarComponent,
    TextboxComponent,
    ExpansionPanelComponent,
    SelectComponent,
  ],
})
export class FieldSettingsModule {}
