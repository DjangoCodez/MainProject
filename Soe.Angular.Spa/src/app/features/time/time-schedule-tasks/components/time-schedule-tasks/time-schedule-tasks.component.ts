import { Component } from '@angular/core';
import { TimeScheduleTasksGridComponent } from '../time-schedule-tasks-grid/time-schedule-tasks-grid.component';
import { TimeScheduleTasksEditComponent } from '../time-schedule-tasks-edit/time-schedule-tasks-edit.component';
import { TimeScheduleTasksForm } from '../../models/time-schedule-tasks-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeScheduleTasksComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeScheduleTasksGridComponent,
      editComponent: TimeScheduleTasksEditComponent,
      FormClass: TimeScheduleTasksForm,
      gridTabLabel: 'time.schedule.timescheduletask.tasks',
      editTabLabel: 'time.schedule.timescheduletask.task',
      createTabLabel: 'time.schedule.timescheduletask.new',
    },
  ];
}
