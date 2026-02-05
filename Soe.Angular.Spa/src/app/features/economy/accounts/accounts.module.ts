import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountsComponent } from './components/accounts/accounts.component';
import { AccountsEditComponent } from './components/accounts-edit/accounts-edit.component';
import { AccountsGridComponent } from './components/accounts-grid/accounts-grid.component';
import { AccountsRoutingModule } from './accounts-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SharedModule } from '@shared/shared.module';
import { ExtraFieldsModule } from '@shared/features/extra-fields/extra-fields.module';
import { LanguageTranslationsModule } from '@shared/features/language-translations/language-translations.module';
import { AccountingCodingLevelsModule } from '../accounting-coding-levels/accounting-coding-levels.module';
import { UpdateAccountDimStdComponent } from './components/update-account-dim-std/update-account-dim-std.component';

@NgModule({
  declarations: [
    AccountsComponent,
    AccountsEditComponent,
    AccountsGridComponent,
    UpdateAccountDimStdComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    AccountsRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    ExpansionPanelComponent,
    NumberboxComponent,
    SelectComponent,
    MultiSelectComponent,
    LabelComponent,
    TextboxComponent,
    CheckboxComponent,
    EditFooterComponent,
    ButtonComponent,
    SaveButtonComponent,
    ExtraFieldsModule,
    LanguageTranslationsModule,
    InstructionComponent,
    AccountingCodingLevelsModule,
    DialogComponent,
    LabelComponent,
  ],
})
export class AccountsModule {}
