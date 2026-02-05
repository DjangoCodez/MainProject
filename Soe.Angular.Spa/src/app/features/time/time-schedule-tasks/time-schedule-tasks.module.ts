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
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextareaComponent } from '@ui/forms/textarea/textarea.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { TimeScheduleTasksEditComponent } from './components/time-schedule-tasks-edit/time-schedule-tasks-edit.component';
import { TimeScheduleTasksGridComponent } from './components/time-schedule-tasks-grid/time-schedule-tasks-grid.component';
import { TimeScheduleTasksComponent } from './components/time-schedule-tasks/time-schedule-tasks.component';
import { TimeScheduleTasksRoutingModule } from './time-schedule-tasks-routing.module';

@NgModule({
  declarations: [
    TimeScheduleTasksComponent,
    TimeScheduleTasksGridComponent,
    TimeScheduleTasksEditComponent,
  ],
  imports: [
    SharedModule,
    CommonModule,
    ReactiveFormsModule,
    TimeScheduleTasksRoutingModule,
    ButtonComponent,
    CheckboxComponent,
    ExpansionPanelComponent,
    EditFooterComponent,
    GridWrapperComponent,
    InstructionComponent,
    NumberboxComponent,
    SelectComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    TextareaComponent,
    TimeboxComponent,
    ToolbarComponent,
  ],
})
export class TimeScheduleTasksModule {}
