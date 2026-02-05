import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SkillsModule } from '@shared/components/time/skills/skills.module';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EmployeePositionEditComponent } from './components/employee-position-edit/employee-position-edit.component';
import { EmployeePositionGridComponent } from './components/employee-position-grid/employee-position-grid.component';
import { EmployeePositionComponent } from './components/employee-position/employee-position.component';
import { EmployeeSystemPositionGridComponent } from './components/employee-system-position-grid/employee-system-position-grid.component';
import { EmployeePositionRoutingModule } from './employee-position-routing.module';

@NgModule({
  declarations: [
    EmployeePositionGridComponent,
    EmployeePositionEditComponent,
    EmployeePositionComponent,
    EmployeeSystemPositionGridComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    EmployeePositionRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    ExpansionPanelComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    SkillsModule,
  ],
})
export class EmployeePositionModule {}
