import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImportPaymentsRoutingModule } from './import-payments-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CreatedModifiedComponent } from '@ui/created-modified/created-modified.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ImportPaymentsComponent } from './components/import-payments/import-payments.component';
import { ImportPaymentsGridComponent } from './components/import-payments-grid/import-payments-grid.component';
import { ImportPaymentsEditComponent } from './components/import-payments-edit/import-payments-edit.component';
import { ImportPaymentsEditDetailGridComponent } from './components/import-payments-edit-detail-grid/import-payments-edit-detail-grid.component';
import { ImportPaymentsGridFilterComponent } from './components/import-payments-grid-filter/import-payments-grid-filter.component';
import { TraceRowsModule } from '@shared/components/trace-rows/trace-rows.module';
@NgModule({
  declarations: [
    ImportPaymentsComponent,
    ImportPaymentsGridComponent,
    ImportPaymentsEditComponent,
    ImportPaymentsEditDetailGridComponent,
    ImportPaymentsGridFilterComponent,
  ],
  imports: [
    CommonModule,
    ImportPaymentsRoutingModule,
    CreatedModifiedComponent,
    EditFooterComponent,
    MultiTabWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    GridWrapperComponent,
    ButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    ExpansionPanelComponent,
    DatepickerComponent,
    DialogComponent,
    TraceRowsModule,
    LabelComponent,
    SelectComponent,
  ],
})
export class ImportPaymentsModule {}
