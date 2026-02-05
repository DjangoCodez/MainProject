import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TimeScheduleTypeEditFactorsGridComponent } from './components/time-schedule-type-edit/time-schedule-type-edit-factors-grid/time-schedule-type-edit-factors-grid.component';
import { TimeScheduleTypeEditComponent } from './components/time-schedule-type-edit/time-schedule-type-edit.component';
import { TimeScheduleTypeGridComponent } from './components/time-schedule-type-grid/time-schedule-type-grid.component';
import { TimeScheduleTypeComponent } from './components/time-schedule-type/time-schedule-type.component';
import { TimeScheduleTypeRoutingModule } from './time-schedule-type-routing.module';

@NgModule({
  declarations: [
    TimeScheduleTypeComponent,
    TimeScheduleTypeGridComponent,
    TimeScheduleTypeEditComponent,
    TimeScheduleTypeEditFactorsGridComponent,
  ],
  imports: [
    CommonModule,
    TimeScheduleTypeRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    TextboxComponent,
    ButtonComponent,
    EditFooterComponent,
    ExpansionPanelComponent,
    CheckboxComponent,
    SelectComponent,
    DialogComponent,
  ],
})
export class TimeScheduleTypeModule {}
