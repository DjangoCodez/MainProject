import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SysCompanyBankAccountsGridComponent } from './components/sys-company-edit/sys-company-bank-accounts-grid/sys-company-bank-accounts-grid.component';
import { SysCompanyEditComponent } from './components/sys-company-edit/sys-company-edit.component';
import { SysCompanySettingsGridComponent } from './components/sys-company-edit/sys-company-settings-grid/sys-company-settings-grid.component';
import { SysCompanyUniqueValuesGridComponent } from './components/sys-company-edit/sys-company-unique-values-grid/sys-company-unique-values-grid.component';
import { SysCompanyGridComponent } from './components/sys-company-grid/sys-company-grid.component';
import { SysCompanyComponent } from './components/sys-company/sys-company.component';
import { SysCompanyRoutingModule } from './sys-company-routing.module';

@NgModule({
  declarations: [
    SysCompanyComponent,
    SysCompanyEditComponent,
    SysCompanyGridComponent,
    SysCompanySettingsGridComponent,
    SysCompanyBankAccountsGridComponent,
    SysCompanyUniqueValuesGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ExpansionPanelComponent,
    ButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    InstructionComponent,
    SysCompanyRoutingModule,
  ],
})
export class SysCompanyModule {}
