import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
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
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { LeisureCodeSettingsEditComponent } from './components/leisure-code-settings-edit/leisure-code-settings-edit.component';
import { LeisureCodesEditGridComponent } from './components/leisure-code-settings-grid/leisure-codes-settings-grid.component';
import { LeisureCodesEditComponent } from './components/leisure-codes-edit/leisure-codes-edit.component';
import { LeisureCodesGridComponent } from './components/leisure-codes-grid/leisure-codes-grid.component';
import { LeisureCodesComponent } from './components/leisure-codes/leisure-codes.component';
import { LeisureCodesRoutingModule } from './leisure-codes-routing.module';

@NgModule({
  declarations: [
    LeisureCodesComponent,
    LeisureCodesGridComponent,
    LeisureCodesEditComponent,
    LeisureCodesEditGridComponent,
    LeisureCodeSettingsEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    LeisureCodesRoutingModule,
    ButtonComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    DatepickerComponent,
    ExpansionPanelComponent,
    DialogComponent,
    NumberboxComponent,
    TimeboxComponent,
  ],
})
export class LeisureCodesModule {}
