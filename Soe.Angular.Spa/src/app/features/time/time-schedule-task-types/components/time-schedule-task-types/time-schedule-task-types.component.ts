import { Component } from '@angular/core';
import { TimeScheduleTaskTypeForm } from '../../models/time-schedule-task-types-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeScheduleTaskTypesEditComponent } from '../time-schedule-task-types-edit/time-schedule-task-types-edit.component';
import { TimeScheduleTaskTypesGridComponent } from '../time-schedule-task-types-grid/time-schedule-task-types-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeScheduleTaskTypesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeScheduleTaskTypesGridComponent,
      editComponent: TimeScheduleTaskTypesEditComponent,
      FormClass: TimeScheduleTaskTypeForm,
      gridTabLabel: 'time.schedule.timescheduletasktype.types',
      editTabLabel: 'time.schedule.timescheduletasktype.type',
      createTabLabel: 'time.schedule.timescheduletasktype.new_type',
    },
  ];
}
