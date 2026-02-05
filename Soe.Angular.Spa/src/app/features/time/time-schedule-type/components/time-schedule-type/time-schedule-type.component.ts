import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeScheduleTypeGridComponent } from '../time-schedule-type-grid/time-schedule-type-grid.component';
import { TimeScheduleTypeEditComponent } from '../time-schedule-type-edit/time-schedule-type-edit.component';
import { TimeScheduleTypeForm } from '../../models/time-schedule-type-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeScheduleTypeComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeScheduleTypeGridComponent,
      editComponent: TimeScheduleTypeEditComponent,
      FormClass: TimeScheduleTypeForm,
      gridTabLabel: 'time.schedule.scheduletype.scheduletypes',
      editTabLabel: 'time.schedule.scheduletype.scheduletype',
      createTabLabel: 'time.schedule.scheduletype.new',
    },
  ];
}
