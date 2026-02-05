import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DocumentMenuComponent } from './document-menu.component';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component'
import { IconModule } from '@ui/icon/icon.module'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { PdfViewerComponent } from '@ui/pdf-viewer/pdf-viewer.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CdkContextMenuTrigger, CdkMenuItem, CdkMenu } from '@angular/cdk/menu';
import { ContextMenuComponent } from './context-menu/context-menu.component';
import { DocumentEditComponent } from './document-edit/document-edit.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    DocumentMenuComponent,
    ContextMenuComponent,
    DocumentEditComponent,
  ],
  exports: [DocumentMenuComponent],
  imports: [
    CommonModule,
    SharedModule,
    IconModule,
    ButtonComponent,
    IconButtonComponent,
    InstructionComponent,
    PdfViewerComponent,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    CdkContextMenuTrigger,
    CdkMenu,
    CdkMenuItem,
    ReactiveFormsModule,
    CheckboxComponent,
    DatepickerComponent,
    ExpansionPanelComponent,
    SelectComponent,
    MultiSelectComponent,
    TextboxComponent,
    ToolbarComponent,
    EditFooterComponent,
  ],
})
export class DocumentMenuModule {}
