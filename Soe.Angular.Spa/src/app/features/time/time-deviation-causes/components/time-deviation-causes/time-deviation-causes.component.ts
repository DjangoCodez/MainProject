import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { TimeDeviationCausesGridComponent } from '../time-deviation-causes-grid/time-deviation-causes-grid.component';
import { TimeDeviationCausesEditComponent } from '../time-deviation-causes-edit/time-deviation-causes-edit.component';
import { TimeDeviationCausesForm } from '../../models/time-deviation-causes-form.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TimeDeviationCausesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TimeDeviationCausesGridComponent,
      editComponent: TimeDeviationCausesEditComponent,
      FormClass: TimeDeviationCausesForm,
      gridTabLabel: 'time.time.timedeviationcause.timedeviationcauses',
      editTabLabel: 'time.time.timedeviationcause.timedeviationcause',
      createTabLabel: 'time.time.timedeviationcause.new',
    },
  ];
}
