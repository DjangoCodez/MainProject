import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmailTemplatesRoutingModule } from './email-templates-routing.module';
import { EmailTemplatesGridComponent } from './components/email-templates-grid/email-templates-grid.component';
import { EmailTemplateEditComponent } from './components/email-template-edit/email-template-edit.component';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TexteditorComponent } from '@ui/forms/texteditor/texteditor.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EmailTemplatesComponent } from './components/email-templates/email-templates.component';

@NgModule({
  declarations: [
    EmailTemplatesComponent,
    EmailTemplatesGridComponent,
    EmailTemplateEditComponent,
  ],
  imports: [
    CommonModule,
    EmailTemplatesRoutingModule,
    CommonModule,
    SharedModule,
    ExpansionPanelComponent,
    TextboxComponent,
    SelectComponent,
    TexteditorComponent,
    CheckboxComponent,
    ButtonComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
  ],
})
export class EmailTemplatesModule {}
