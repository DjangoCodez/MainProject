import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeWorkReductionGridComponent } from '../time-work-reduction-grid/time-work-reduction-grid.component';
import { TimeWorkReductionEditComponent } from '../time-work-reduction-edit/time-work-reduction-edit.component';
import { TimeWorkReductionForm } from '../../models/time-work-reduction-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeWorkReductionComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeWorkReductionGridComponent,
      editComponent: TimeWorkReductionEditComponent,
      FormClass: TimeWorkReductionForm,
      gridTabLabel: 'time.time.timeworkreduction.timeworkreduction',
      editTabLabel: 'time.time.timeworkreduction.timeworkreduction',
      createTabLabel: 'time.time.timeworkreduction.new',
    },
  ];
}
