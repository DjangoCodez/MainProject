import { Component } from '@angular/core';
import { TimeScheduleEventsGridComponent } from '../../components/time-schedule-events-grid/time-schedule-events-grid.component';
import { TimeScheduleEventsEditComponent } from '../../components/time-schedule-events-edit/time-schedule-events-edit.component';
import { TimeScheduleEventForm } from '../../models/time-schedule-event-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeScheduleEventsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeScheduleEventsGridComponent,
      editComponent: TimeScheduleEventsEditComponent,
      FormClass: TimeScheduleEventForm,
      gridTabLabel: 'time.schedule.timescheduleevent.events',
      editTabLabel: 'time.schedule.timescheduleevent.event',
      createTabLabel: 'time.schedule.timescheduleevent.new_event',
    },
  ];
}
