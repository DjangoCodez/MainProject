import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ScheduleCyclesGridComponent } from '../schedule-cycles-grid/schedule-cycles-grid.component';
import { ScheduleCyclesEditComponent } from '../schedule-cycles-edit/schedule-cycles-edit.component';
import { ScheduleCyclesForm } from '../../models/schedule-cycles-form.model';

@Component({
  selector: 'soe-schedule-cycles',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ScheduleCyclesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ScheduleCyclesGridComponent,
      editComponent: ScheduleCyclesEditComponent,
      FormClass: ScheduleCyclesForm,
      gridTabLabel: 'time.schedule.schedulecycle.schedulecycles',
      editTabLabel: 'time.schedule.schedulecycle.schedulecycle',
      createTabLabel: 'time.schedule.schedulecycle.new',
    },
  ];
}
