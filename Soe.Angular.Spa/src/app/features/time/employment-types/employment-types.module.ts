import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
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
import { EmploymentTypesEditComponent } from './components/employment-types-edit/employment-types-edit.component';
import { EmploymentTypesGridComponent } from './components/employment-types-grid/employment-types-grid.component';
import { EmploymentTypesComponent } from './components/employment-types/employment-types.component';
import { EmploymentTypesRoutingModule } from './employment-types-routing.module';

@NgModule({
  declarations: [
    EmploymentTypesComponent,
    EmploymentTypesGridComponent,
    EmploymentTypesEditComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    EmploymentTypesRoutingModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ButtonComponent,
    CheckboxComponent,
    TextboxComponent,
    EditFooterComponent,
    SelectComponent,
    ExpansionPanelComponent,
  ],
})
export class EmploymentTypesModule {}
