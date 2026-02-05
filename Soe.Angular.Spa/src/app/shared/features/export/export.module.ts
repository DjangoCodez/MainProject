import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ExportComponent } from './components/export/export.component';
import { ExportEditComponent } from './components/export-edit/export-edit.component';
import { ExportGridComponent } from './components/export-grid/export-grid.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ExportRoutingModule } from './export-routing.module';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [ExportComponent, ExportEditComponent, ExportGridComponent],
  imports: [
    ExportRoutingModule,
    ButtonComponent,
    CommonModule,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    SharedModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class ExportModule {}
