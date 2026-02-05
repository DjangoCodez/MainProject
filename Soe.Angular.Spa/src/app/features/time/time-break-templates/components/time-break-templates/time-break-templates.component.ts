import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeBreakTemplatesGridComponent } from '../time-break-templates-grid/time-break-templates-grid.component';
import { TimeBreakTemplatesEditComponent } from '../time-break-templates-edit/time-break-templates-edit.component';
import { TimeBreakTemplatesForm } from '../../models/time-break-templates-form.model';

@Component({
  selector: 'soe-time-break-templates',
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
})
export class TimeBreakTemplatesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeBreakTemplatesGridComponent,
      editComponent: TimeBreakTemplatesEditComponent,
      FormClass: TimeBreakTemplatesForm,
      gridTabLabel: 'time.schedule.timebreaktemplate.timebreaktemplates',
      editTabLabel: 'time.schedule.timebreaktemplate.timebreaktemplate',
      createTabLabel: 'time.schedule.timebreaktemplate.new',
    },
  ];
}
