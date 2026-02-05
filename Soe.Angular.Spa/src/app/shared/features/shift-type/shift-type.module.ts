import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ShiftTypeRoutingModule } from './shift-type-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { ColorpickerComponent } from '@ui/forms/colorpicker/colorpicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { InstructionComponent } from '@ui/instruction/instruction.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ShiftTypeComponent } from './components/shift-type/shift-type.component';
import { ShiftTypeGridComponent } from './components/shift-type-grid/shift-type-grid.component';
import { ShiftTypeEditComponent } from './components/shift-type-edit/shift-type-edit.component';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { AccountingSettingsModule } from '@shared/components/accounting-settings/accounting-settings.module';
import { EmployeeStatisticGridComponent } from './components/shift-type-edit/employee-statistic-grid/employee-statistic-grid.component';
import { SkillsModule } from '@shared/components/time/skills/skills.module';
import { HierarchyAccountsGridComponent } from './components/shift-type-edit/hierarchy-accounts-grid/hierarchy-accounts-grid/hierarchy-accounts-grid.component';

@NgModule({
  declarations: [
    ShiftTypeComponent,
    ShiftTypeGridComponent,
    ShiftTypeEditComponent,
    EmployeeStatisticGridComponent,
    HierarchyAccountsGridComponent,
  ],
  imports: [
    CommonModule,
    ShiftTypeRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    SharedModule,
    MultiTabWrapperComponent,
    TimeboxComponent,
    TextboxComponent,
    ToolbarComponent,
    InstructionComponent,
    ColorpickerComponent,
    CheckboxComponent,
    CategoriesModule,
    SkillsModule,
    AccountingSettingsModule,
    LabelComponent,
  ],
})
export class ShiftTypeModule {}
