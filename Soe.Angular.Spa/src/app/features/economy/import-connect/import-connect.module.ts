import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportConnectRoutingModule } from './import-connect-routing.module';
import { ImportConnectGridComponent } from './components/import-connect-grid/import-connect-grid.component';
import { ImportConnectEditComponent } from './components/import-connect-edit/import-connect-edit.component';
import { ImportConnectComponent } from './components/import-connect/import-connect.component';
import { ImportRowsComponent } from './components/import-rows/import-rows.component';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { FileUploadComponent } from '@ui/forms/file-upload/file-upload.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AccountDimsModule } from '@shared/components/account-dims/account-dims.module';
import { ByteFormatterPipe } from '@shared/pipes/byte-formatter.pipe';
import { TranslateModule } from '@ngx-translate/core';

@NgModule({
  declarations: [
    ImportConnectGridComponent,
    ImportConnectEditComponent,
    ImportConnectComponent,
    ImportRowsComponent,
  ],
  imports: [
    CommonModule,
    ImportConnectRoutingModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
    GridWrapperComponent,
    SelectComponent,
    TextboxComponent,
    CheckboxComponent,
    AccountDimsModule,
    ButtonComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    AutocompleteComponent,
    ByteFormatterPipe,
    TranslateModule.forChild(),
    FileUploadComponent,
  ],
})
export class ImportConnectModule {}
