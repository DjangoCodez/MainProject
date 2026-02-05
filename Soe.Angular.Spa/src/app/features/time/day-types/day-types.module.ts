import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { DayTypesEditComponent } from './components/day-types-edit/day-types-edit.component';
import { DayTypesGridComponent } from './components/day-types-grid/day-types-grid.component';
import { DayTypesComponent } from './components/day-types/day-types.component';
import { DayTypesRoutingModule } from './day-types-routing.module';

@NgModule({
  declarations: [
    DayTypesComponent,
    DayTypesGridComponent,
    DayTypesEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    DayTypesRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class DayTypesModule {}
