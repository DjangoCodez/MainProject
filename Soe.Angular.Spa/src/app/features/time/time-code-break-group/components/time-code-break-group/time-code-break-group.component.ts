import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeCodeBreakGroupForm } from '../../models/time-code-break-group-form.model';
import { TimeCodeBreakGroupEditComponent } from '../time-code-break-group-edit/time-code-break-group-edit.component';
import { TimeCodeBreakGroupGridComponent } from '../time-code-break-group-grid/time-code-break-group-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeCodeBreakGroupComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeCodeBreakGroupGridComponent,
      editComponent: TimeCodeBreakGroupEditComponent,
      FormClass: TimeCodeBreakGroupForm,
      gridTabLabel: 'time.time.timecodebreakgroup.timecodebreakgroups',
      editTabLabel: 'time.time.timecodebreakgroup.timecodebreakgroup',
      createTabLabel: 'time.time.timecodebreakgroup.new',
    },
  ];
}
