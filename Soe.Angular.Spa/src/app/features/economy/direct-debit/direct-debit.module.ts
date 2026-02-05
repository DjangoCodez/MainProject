import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DirectDebitComponent } from './components/direct-debit/direct-debit.component';
import { DirectDebitGridComponent } from './components/direct-debit-grid/direct-debit-grid.component';
import { DirectDebitEditComponent } from './components/direct-debit-edit/direct-debit-edit.component';
import { SharedModule } from '@shared/shared.module';
import { DirectDebitRoutingModule } from './direct-debit-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AccountingSettingsModule } from '@shared/components/accounting-settings/accounting-settings.module';
import { DirectDebitEditGridComponent } from './components/direct-debit-edit-grid/direct-debit-edit-grid.component';

@NgModule({
  declarations: [
    DirectDebitComponent,
    DirectDebitGridComponent,
    DirectDebitEditComponent,
    DirectDebitEditGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    DirectDebitRoutingModule,
    MultiTabWrapperComponent,
    ExpansionPanelComponent,
    ToolbarComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    NumberboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ButtonComponent,
    SaveButtonComponent,
    SelectComponent,
    AccountingSettingsModule,
  ],
})
export class DirectDebitModule {}
