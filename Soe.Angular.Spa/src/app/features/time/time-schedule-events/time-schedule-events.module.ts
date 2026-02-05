import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component';
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component';
import { LabelComponent } from '@shared/ui-components/label/label.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { MultiSelectGridModule } from '@shared/components/multi-select-grid/multi-select-grid.module';
import { TimeScheduleEventsRoutingModule } from './time-schedule-events-routing.module';
import { TimeScheduleEventsComponent } from './components/time-schedule-events/time-schedule-events.component';
import { TimeScheduleEventsGridComponent } from './components/time-schedule-events-grid/time-schedule-events-grid.component';
import { TimeScheduleEventsEditComponent } from './components/time-schedule-events-edit/time-schedule-events-edit.component';

@NgModule({
  declarations: [
    TimeScheduleEventsComponent,
    TimeScheduleEventsGridComponent,
    TimeScheduleEventsEditComponent,
  ],
  imports: [
    CommonModule,
    TimeScheduleEventsRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    DatepickerComponent,
    ToolbarComponent,
    LabelComponent,
    MultiSelectGridModule,
  ],
})
export class TimeScheduleEventsModule {}
